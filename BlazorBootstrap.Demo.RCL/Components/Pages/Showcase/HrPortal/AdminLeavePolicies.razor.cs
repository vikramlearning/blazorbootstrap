using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Leave policies (HR admin): a table of every leave type (name with its description, days a year or "Unlimited", minimum
/// notice, offered or turned off in words, this year's requests that weren't cancelled, and the days approved this year
/// across the company), with Edit and Delete in each row, and an inline form to add or edit one: name, description (with a
/// counter), "No yearly limit", days a year (1 to <see cref="HrState.MaxAnnualDays" />, disabled when unlimited, saying how
/// everyone's balance changes), minimum notice (0 to <see cref="HrState.MaxNoticeDays" />), and "Offered for new requests"
/// (<see cref="HrState.SavePolicy" />; a taken name is shown under the field). Delete opens a <see cref="Modal" />: a type
/// no request uses is deleted once the dialog is hidden; one in use names how many requests use it and offers "Turn it off"
/// instead, and a deletion refused later (WrongState) shows a one-click "Turn off" beside the status message. Outcomes are
/// announced in an always-rendered status region, and focus returns to the row button or the status message.
/// </summary>
public partial class AdminLeavePolicies : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Edit and Delete buttons, by "edit-{id}" or "delete-{id}".</summary>
    private readonly Dictionary<string, ElementReference> rowButtons = new();

    private string daysText = "10";

    private string? daysError;

    private ElementReference? daysInput;

    private Modal deleteModal = default!;

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference? descriptionInput;

    /// <summary>The policy the delete dialog is about.</summary>
    private int dialogPolicyId;

    /// <summary>The policy being edited in the form, or 0 when the form adds one.</summary>
    private int editId;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private bool isActive = true;

    private bool isUnlimited;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    private string? noticeError;

    private ElementReference? noticeInput;

    private string noticeText = "0";

    /// <summary>What the delete dialog confirmed; it runs once the dialog is hidden.</summary>
    private PendingAction pending;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    /// <summary>The policy a refused deletion offers to turn off beside the status message, or 0.</summary>
    private int turnOffOfferId;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HrDisplay.FocusAsync(target());
        }

        if (focusStatus)
        {
            focusStatus = false;
            await HrDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns everyone's leave requests (the HR admin sees all of them).</summary>
    private List<HrLeaveRequest> AllRequests() => State.AllEmployees.SelectMany(employee => State.LeaveOf(employee.Id)).ToList();

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Describes a saved policy, like "25 days a year, 7 days' notice, offered".</summary>
    private static string Summary(HrLeavePolicy policy) =>
        (policy.IsUnlimited ? "no yearly limit" : $"{HrDisplay.Days(policy.AnnualDays)} a year")
        + (policy.MinNoticeDays == 0 ? ", no notice needed" : $", {HrDisplay.Days(policy.MinNoticeDays)}' notice")
        + (policy.IsActive ? ", offered" : ", turned off");

    private void CancelEdit()
    {
        var id = editId;
        ResetForm();
        FocusRowButton("edit-" + id.ToString(CultureInfo.InvariantCulture), id);
    }

    private async Task ConfirmAsync(PendingAction action)
    {
        pending = action;
        await deleteModal.HideAsync();
    }

    /// <summary>Returns focus to a row's button, or to the status message when the row is gone.</summary>
    private void FocusRowButton(string key, int policyId)
    {
        if (rowButtons.TryGetValue(key, out var button) && State.FindPolicy(policyId) is not null)
            focusTarget = () => button;
        else
            focusStatus = true;
    }

    private void OnDeleteHidden()
    {
        var id = dialogPolicyId;
        var action = pending;
        pending = PendingAction.None;

        if (action == PendingAction.None)
        {
            FocusRowButton("delete-" + id.ToString(CultureInfo.InvariantCulture), id);

            return;
        }

        if (action == PendingAction.TurnOff)
        {
            TurnOff(id);

            return;
        }

        focusStatus = true;

        if (State.FindPolicy(id) is not { } policy)
        {
            Announce(false, "That leave type was already deleted, so nothing changed.");

            return;
        }

        var result = State.DeletePolicy(id);

        switch (result)
        {
            case HrResult.Ok:
                turnOffOfferId = 0;

                if (editId == id)
                    ResetForm();

                Announce(true, $"Deleted the leave type “{policy.Name}”.");

                break;

            case HrResult.WrongState:
                var uses = UsesOf(id);
                turnOffOfferId = policy.IsActive ? id : 0;
                Announce(false, $"{HrDisplay.Plural(uses, "request uses", "requests use")} “{policy.Name}”, so it wasn't deleted. "
                                + (policy.IsActive ? "Turn it off instead." : "It's already turned off, so nobody can request it."));

                break;

            case HrResult.NotFound:
                Announce(false, "That leave type was already deleted, so nothing changed.");

                break;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can delete leave types. Nothing was deleted.");

                break;

            default:
                Announce(false, $"“{policy.Name}” couldn't be deleted. Try again.");

                break;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteAsync(int policyId)
    {
        dialogPolicyId = policyId;
        pending = PendingAction.None;
        await deleteModal.ShowAsync();
    }

    private void ResetForm()
    {
        editId = 0;
        name = description = string.Empty;
        daysText = "10";
        noticeText = "0";
        isUnlimited = false;
        isActive = true;
        nameError = descriptionError = daysError = noticeError = null;
    }

    /// <summary>Checks the form and saves the policy, mapping every result to a field or the status message.</summary>
    private void Save()
    {
        var existing = editId == 0 ? null : State.FindPolicy(editId);

        if (editId != 0 && existing is null)
        {
            ResetForm();
            Announce(false, "That leave type was deleted, so the changes couldn't be saved.");
            focusStatus = true;

            return;
        }

        var trimmedName = name.Trim();
        var days = AdminHelpers.TryNumber(daysText);
        var notice = AdminHelpers.TryNumber(noticeText);

        nameError = AdminHelpers.TextProblem(name, HrState.MaxNameLength, "Give the leave type a name.", "name")
            ?? (State.AllPolicies.FirstOrDefault(item => item.Id != existing?.Id && string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)) is { } same
                ? $"That name is taken by “{same.Name}”. Choose another."
                : null);
        descriptionError = description.Trim().Length > HrState.MaxDescriptionLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the description to {HrState.MaxDescriptionLength} characters or fewer (now {description.Trim().Length}).")
            : null;
        daysError = isUnlimited ? null
            : days is not { } dayCount || dayCount is < 1 or > HrState.MaxAnnualDays ? string.Create(CultureInfo.InvariantCulture, $"Enter a whole number of days from 1 to {HrState.MaxAnnualDays}, or tick “No yearly limit”.")
            : null;
        noticeError = notice is not { } noticeCount || noticeCount is < 0 or > HrState.MaxNoticeDays
            ? string.Create(CultureInfo.InvariantCulture, $"Enter a whole number of days from 0 to {HrState.MaxNoticeDays}.")
            : null;

        focusTarget = nameError is not null ? () => nameInput
            : descriptionError is not null ? () => descriptionInput
            : daysError is not null ? () => daysInput
            : noticeError is not null ? () => noticeInput
            : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        var result = State.SavePolicy(existing?.Id ?? 0, trimmedName, description, isUnlimited ? 0 : days!.Value, isUnlimited, notice!.Value, isActive, out var saved);

        switch (result)
        {
            case HrResult.Ok when saved is not null:
                Announce(true, (existing is null ? $"Added “{saved.Name}”: " : $"Saved “{saved.Name}”: ") + Summary(saved) + ". Balances updated at once for everyone.");
                ResetForm();
                focusStatus = true;

                return;

            case HrResult.Duplicate:
                nameError = State.AllPolicies.FirstOrDefault(item => item.Id != existing?.Id && string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)) is { } holder
                    ? $"That name is taken by “{holder.Name}”. Choose another."
                    : "That name is taken by another leave type. Choose another.";
                focusTarget = () => nameInput;
                Announce(false, "That name is taken. Nothing was saved.");

                return;

            case HrResult.NoChange:
                Announce(true, $"Nothing changed: “{existing?.Name}” is already saved like this.");

                return;

            case HrResult.NotFound:
                ResetForm();
                Announce(false, "That leave type was deleted, so the changes couldn't be saved.");
                focusStatus = true;

                return;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can change leave policies. Nothing was saved.");
                focusStatus = true;

                return;

            case HrResult.Invalid:
                Announce(false, existing is null && State.AllPolicies.Count >= HrState.MaxPolicies
                    ? $"There are {HrState.MaxPolicies} leave types, the most the portal keeps. Nothing was saved."
                    : "The leave type couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    /// <summary>Fills the form with a policy, to change it.</summary>
    private void StartEdit(int policyId)
    {
        if (State.FindPolicy(policyId) is not { } policy)
            return;

        editId = policy.Id;
        name = policy.Name;
        description = policy.Description;
        isUnlimited = policy.IsUnlimited;
        daysText = (policy.IsUnlimited ? 10 : policy.AnnualDays).ToString(CultureInfo.InvariantCulture);
        noticeText = policy.MinNoticeDays.ToString(CultureInfo.InvariantCulture);
        isActive = policy.IsActive;
        nameError = descriptionError = daysError = noticeError = null;
        focusTarget = () => nameInput;
    }

    /// <summary>Turns a policy off (keeping everything else), so nobody can request it any more.</summary>
    private void TurnOff(int policyId)
    {
        focusStatus = true;

        if (State.FindPolicy(policyId) is not { } policy)
        {
            turnOffOfferId = 0;
            Announce(false, "That leave type was deleted, so nothing changed.");

            return;
        }

        var result = State.SavePolicy(policy.Id, policy.Name, policy.Description, policy.AnnualDays, policy.IsUnlimited, policy.MinNoticeDays, false, out _);

        switch (result)
        {
            case HrResult.Ok:
            case HrResult.NoChange:
                turnOffOfferId = 0;

                if (editId == policy.Id)
                    isActive = false;

                Announce(true, $"Turned “{policy.Name}” off: nobody can request it any more. Existing requests and this year's balances keep it.");

                break;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can change leave policies. Nothing changed.");

                break;

            case HrResult.NotFound:
                turnOffOfferId = 0;
                Announce(false, "That leave type was deleted, so nothing changed.");

                break;

            default:
                Announce(false, $"“{policy.Name}” couldn't be turned off: the storage for this demo tab may be full. Try again.");

                break;
        }
    }

    /// <summary>Returns how many requests (any year, any status) use a policy: the ones that stop it being deleted.</summary>
    private int UsesOf(int policyId) => AllRequests().Count(request => request.PolicyId == policyId);

    #endregion

    #endregion

    #region Properties, Indexers

    private HrLeavePolicy? DialogPolicy => State.FindPolicy(dialogPolicyId);

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What the delete dialog confirmed.</summary>
    private enum PendingAction
    {
        /// <summary>Nothing (Cancel).</summary>
        None,

        /// <summary>Delete the policy.</summary>
        Delete,

        /// <summary>Turn the policy off instead.</summary>
        TurnOff,
    }

    #endregion
}
