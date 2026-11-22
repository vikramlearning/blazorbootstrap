using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Leave policies (HR admin): a <see cref="Grid{TItem}" /> of every leave type (name with its description, days a year or
/// "Unlimited", minimum notice, offered or turned off in words, this year's requests that weren't cancelled, and the days
/// approved this year across the company), with sorting, the column chooser, CSV export, and a saved view that can be
/// reset; Edit and Delete in each row, and the name (its stretched area, the whole row) opens the editor. The editor is a
/// validated dialog (rule WR-002): a unique name, checked as you type; a description of up to 200 characters with a
/// counter; "No yearly limit"; days a year (1 to <see cref="HrState.MaxAnnualDays" />, needed unless there is no yearly
/// limit: a cross-field rule); minimum notice (0 to <see cref="HrState.MaxNoticeDays" />); and "Offered for new requests"
/// (<see cref="HrState.SavePolicy" />, whose answer still maps to a message). Delete opens a <see cref="Modal" />: a type no
/// request uses is deleted once the dialog is hidden; one in use names how many requests use it and offers "Turn it off"
/// instead, and a deletion refused later (WrongState) shows a one-click "Turn off" beside the status message. Outcomes are
/// announced in an always-rendered status region, and focus returns to the row button or the status message.
/// </summary>
public partial class AdminLeavePolicies : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Edit and Delete buttons, by "edit-{id}" or "delete-{id}".</summary>
    private readonly Dictionary<string, ElementReference> rowButtons = new();

    private ElementReference? addButton;

    private Modal deleteModal = default!;

    /// <summary>The description as typed, for the live counter (the bound value changes only on change).</summary>
    private string descriptionText = string.Empty;

    /// <summary>The policy the delete dialog is about.</summary>
    private int dialogPolicyId;

    private EditContext editContext = new(new PolicyForm());

    /// <summary>The policy being edited in the dialog, or 0 when it adds one.</summary>
    private int editId;

    private Modal editorModal = default!;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private PolicyForm form = new();

    private string? formError;

    private int formErrorCount;

    private Grid<PolicyRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private ShowcaseUniqueCheck? nameCheck;

    private InputText? nameInput;

    /// <summary>What the delete dialog confirmed; it runs once the dialog is hidden.</summary>
    private PendingAction pending;

    /// <summary>The policy the editor saved (0 for none), so focus returns to its Edit button or the status.</summary>
    private int savedId;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    /// <summary>The policy a refused deletion offers to turn off beside the status message, or 0.</summary>
    private int turnOffOfferId;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The saved view lives in the tab's session storage, so it loads once the page is interactive; then the grid shows.
        if (firstRender)
        {
            await view.LoadAsync();
            viewLoaded = true;
            StateHasChanged();
        }

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

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "leave-policies", HrState.MaxPolicies);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Describes a saved policy, like "25 days a year, 7 days' notice, offered".</summary>
    private static string Summary(HrLeavePolicy policy) =>
        (policy.IsUnlimited ? "no yearly limit" : $"{HrDisplay.Days(policy.AnnualDays)} a year")
        + (policy.MinNoticeDays == 0 ? ", no notice needed" : $", {HrDisplay.Days(policy.MinNoticeDays)}' notice")
        + (policy.IsActive ? ", offered" : ", turned off");

    private Task AddAsync() => OpenEditorAsync(0);

    /// <summary>Returns everyone's leave requests (the HR admin sees all of them).</summary>
    private List<HrLeaveRequest> AllRequests() => State.AllEmployees.SelectMany(employee => State.LeaveOf(employee.Id)).ToList();

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private async Task ConfirmAsync(PendingAction action)
    {
        pending = action;
        await deleteModal.HideAsync();
    }

    private Task EditAsync(int policyId) => OpenEditorAsync(policyId);

    /// <summary>Returns focus to a row's button, or to the status message when the row is gone.</summary>
    private void FocusRowButton(string key, int policyId)
    {
        if (rowButtons.TryGetValue(key, out var button) && State.FindPolicy(policyId) is not null)
            focusTarget = () => button;
        else
            focusStatus = true;
    }

    /// <summary>Whether another leave type already has this name (the one being edited doesn't count).</summary>
    private Task<bool> IsNameTakenAsync(string value) =>
        Task.FromResult(State.AllPolicies.Any(item => item.Id != editId && string.Equals(item.Name, value, StringComparison.OrdinalIgnoreCase)));

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

    private void OnDescriptionInput(ChangeEventArgs args) => descriptionText = args.Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns focus after the editor closes: to the policy's Edit button after an edit or a cancelled edit, to the status
    /// after adding one, or to "Add leave type" when adding was cancelled.
    /// </summary>
    private void OnEditorHidden()
    {
        var id = savedId != 0 ? savedId : editId;
        var wasNew = editId == 0;
        var wasSaved = savedId != 0;
        editId = 0;
        savedId = 0;

        if (wasSaved && wasNew)
            focusStatus = true;
        else if (id != 0)
            FocusRowButton("edit-" + id.ToString(CultureInfo.InvariantCulture), id);
        else
            focusTarget = () => addButton;
    }

    private Task OnEditorShownAsync() => HrDisplay.FocusAsync(nameInput?.Element);

    /// <summary>A leave type or a request changed (here or elsewhere): the grid reads the policies again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenDeleteAsync(int policyId)
    {
        dialogPolicyId = policyId;
        pending = PendingAction.None;
        await deleteModal.ShowAsync();
    }

    /// <summary>Fills the editor with a policy (or a new type's defaults) and opens it.</summary>
    private async Task OpenEditorAsync(int policyId)
    {
        var policy = policyId == 0 ? null : State.FindPolicy(policyId);

        if (policyId != 0 && policy is null)
            return;

        editId = policy?.Id ?? 0;
        savedId = 0;
        form = new PolicyForm
        {
            Name = policy?.Name ?? string.Empty,
            Description = policy?.Description ?? string.Empty,
            IsUnlimited = policy?.IsUnlimited ?? false,
            AnnualDays = policy is null || policy.IsUnlimited ? 10 : policy.AnnualDays,
            MinNoticeDays = policy?.MinNoticeDays ?? 0,
            IsActive = policy?.IsActive ?? true,
        };
        editContext = new EditContext(form);
        descriptionText = form.Description;
        formError = null;
        formErrorCount = 0;

        await editorModal.ShowAsync();
    }

    /// <summary>The leave types for the grid, in the order they were added unless a column is sorted.</summary>
    private Task<GridDataProviderResult<PolicyRow>> ProvidePoliciesAsync(GridDataProviderRequest<PolicyRow> request)
    {
        var year = State.Clock.Today.Year;
        var requests = AllRequests();
        var rows = State.AllPolicies.Select(policy =>
        {
            var thisYear = requests.Where(item => item.PolicyId == policy.Id && item.From.Year == year).ToList();

            return new PolicyRow(policy.Id, policy.Name, policy.Description, policy.IsUnlimited ? "Unlimited" : HrDisplay.Days(policy.AnnualDays),
                policy.IsUnlimited ? int.MaxValue : policy.AnnualDays, policy.MinNoticeDays == 0 ? "None" : HrDisplay.Days(policy.MinNoticeDays), policy.MinNoticeDays,
                policy.IsActive ? "Offered" : "Turned off", policy.IsActive, thisYear.Count(item => item.Status != HrLeaveStatus.Cancelled),
                thisYear.Where(item => item.Status == HrLeaveStatus.Approved).Sum(item => item.Days));
        }).ToList();

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the status.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        gridVersion++;
        Announce(true, "The view is reset: leave types in the order they were added, and every column shown.");
        focusStatus = true;
    }

    /// <summary>
    /// Saves the leave type once the form is valid: the uniqueness check finishes first, then every rule runs, and a failed
    /// save shows the summary (rule WR-002). The state checks again and its answer maps to a message, as before.
    /// </summary>
    private async Task SaveAsync()
    {
        formError = null;

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !nameFree)
            return;

        var existing = editId == 0 ? null : State.FindPolicy(editId);

        if (editId != 0 && existing is null)
        {
            SetFormError("That leave type was deleted, so the changes couldn't be saved. Cancel to see the current list.");

            return;
        }

        var result = State.SavePolicy(existing?.Id ?? 0, form.Name.Trim(), form.Description, form.IsUnlimited ? 0 : form.AnnualDays!.Value, form.IsUnlimited,
            form.MinNoticeDays!.Value, form.IsActive, out var saved);

        switch (result)
        {
            case HrResult.Ok when saved is not null:
                Announce(true, (existing is null ? $"Added “{saved.Name}”: " : $"Saved “{saved.Name}”: ") + Summary(saved) + ". Balances updated at once for everyone.");
                savedId = saved.Id;

                if (turnOffOfferId == saved.Id && !saved.IsActive)
                    turnOffOfferId = 0;

                break;

            case HrResult.NoChange:
                Announce(true, $"Nothing changed: “{existing?.Name}” is already saved like this.");
                savedId = existing?.Id ?? 0;

                break;

            case HrResult.Duplicate:
                SetFormError("Another leave type already has that name. Choose another.");

                return;

            case HrResult.NotFound:
                SetFormError("That leave type was deleted, so the changes couldn't be saved. Cancel to see the current list.");

                return;

            case HrResult.NotAllowed:
                SetFormError("Only the HR admin can change leave policies. Nothing was saved.");

                return;

            case HrResult.Invalid:
                SetFormError(existing is null && State.AllPolicies.Count >= HrState.MaxPolicies
                    ? $"There are {HrState.MaxPolicies} leave types, the most the portal keeps. Nothing was saved."
                    : "The leave type couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");

                return;

            default:
                SetFormError("That didn't work, and nothing was saved. Try again.");

                return;
        }

        await editorModal.HideAsync();
    }

    /// <summary>Shows a dialog error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void SetFormError(string message)
    {
        formErrorCount++;
        formError = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
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

    /// <summary>Gets the policy the editor is changing, or <see langword="null" /> while it adds one.</summary>
    private HrLeavePolicy? Editing => editId == 0 ? null : State.FindPolicy(editId);

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

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

    /// <summary>
    /// The editor's fields, with the limits of <see cref="HrState.SavePolicy" />: a name of 1 to 80 characters, a description
    /// of up to 200, days a year from 1 to 60 unless there is no yearly limit (a cross-field rule), and a notice of 0 to 60 days.
    /// </summary>
    private sealed class PolicyForm : IValidatableObject
    {
        /// <summary>Gets or sets the days a year; checked in <see cref="Validate" />, because it's only needed with a yearly limit.</summary>
        public int? AnnualDays { get; set; }

        [StringLength(HrState.MaxDescriptionLength, ErrorMessage = "Shorten the description to 200 characters or fewer.")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsUnlimited { get; set; }

        [Required(ErrorMessage = "Enter the minimum notice in days; 0 for none.")]
        [Range(0, HrState.MaxNoticeDays, ErrorMessage = "Use 0 to 60 days of notice.")]
        public int? MinNoticeDays { get; set; }

        [Required(ErrorMessage = "Give the leave type a name.")]
        [StringLength(HrState.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>A type with a yearly limit needs its days a year, from 1 to 60; one without a limit doesn't use them.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUnlimited)
                yield break;

            if (AnnualDays is null)
                yield return new ValidationResult("Enter the days a year, or tick “No yearly limit”.", new[] { nameof(AnnualDays) });
            else if (AnnualDays is < 1 or > HrState.MaxAnnualDays)
                yield return new ValidationResult(string.Create(CultureInfo.InvariantCulture, $"Use 1 to {HrState.MaxAnnualDays} days a year, or tick “No yearly limit”."), new[] { nameof(AnnualDays) });
        }
    }

    /// <summary>One leave type as a grid row: the words each column shows and exports, and the values it sorts by.</summary>
    private sealed record PolicyRow(int Id, string Name, string Description, string DaysAYear, int DaysSort, string Notice, int NoticeDays, string Offered, bool IsActive,
        int Requests, int DaysUsed);

    #endregion
}
