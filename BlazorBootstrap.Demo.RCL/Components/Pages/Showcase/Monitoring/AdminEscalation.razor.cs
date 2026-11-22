using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The admins' escalation policies (AC-7c): a <see cref="Grid{TItem}" /> of policies (what each covers, its steps, the most
/// pages it sends, on or off, and how many incidents page under it now) with sorting, the column chooser, CSV export, and a
/// saved view; the whole row opens the policy form in a <see cref="Modal" />. The form is an <see cref="EditForm" />
/// (rule WR-002): a unique name checked as you type, the services it covers or the default, the minutes before the
/// secondary and an optional step 3 (an engineer and minutes), repeats, and on or off; an active policy may not cover a
/// service another active policy names, nor be a second active default. A live preview says in words who would be paged
/// and when with this week's rotation. Turn on or off toggles a policy, and Delete is confirmed with what happens to its
/// services. Focus returns to the button that opened a dialog, or to the status message when that button is gone.
/// </summary>
public partial class AdminEscalation : ComponentBase, IDisposable
{
    #region Fields and Constants

    private ElementReference? countRegion;

    private readonly Dictionary<int, Button> deleteButtons = new();

    private Modal deleteModal = default!;

    /// <summary>Whether the confirmed delete removed the policy, so focus goes to the status message.</summary>
    private bool deleted;

    /// <summary>The policy the delete confirmation is about.</summary>
    private MonitoringEscalationPolicy? deleting;

    private readonly Dictionary<int, Button> editButtons = new();

    /// <summary>The policy the form edits; 0 for a new policy.</summary>
    private int editingId;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private PolicyForm form = new();

    private EditContext formContext = new(new PolicyForm());

    private string? formError;

    private Grid<PolicyRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private ShowcaseUniqueCheck? nameCheck;

    private InputText? nameInput;

    private Button? newPolicyButton;

    /// <summary>The button that opened the form.</summary>
    private Button? opener;

    private Modal policyModal = default!;

    /// <summary>The state's answers that belong to a field (a taken name, a coverage overlap), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

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
            await MonitoringDisplay.FocusAsync(target());
        }

        if (focusStatus)
        {
            focusStatus = false;
            await MonitoringDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        // With no column sorted, the policies keep their number order.
        view = GridViews.Get(State.SiteSlug, "escalation-policies", 25);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deleting is { } policy)
        {
            var consequence = DeleteConsequence(policy);
            var result = State.DeletePolicy(policy.Id);
            deleted = result == MonitoringResult.Ok;
            Announce(deleted, result switch
            {
                MonitoringResult.Ok => $"Deleted “{policy.Name}”. {consequence}",
                MonitoringResult.NotFound => $"“{policy.Name}” was already deleted, so nothing changed.",
                _ => "Sign in as an admin to change escalation policies. Nothing changed.",
            });
        }

        await deleteModal.HideAsync();
    }

    /// <summary>Says what new incidents on a policy's services do once it is gone (or off).</summary>
    private string DeleteConsequence(MonitoringEscalationPolicy policy)
    {
        var fallback = State.Policies.FirstOrDefault(other => other.Id != policy.Id && other.IsActive && other.IsDefault);

        if (policy.IsDefault)
            return "Services without their own policy are no longer paged: an alert on them notifies the on-call engineers, but nobody is escalated.";

        return fallback is null
            ? $"New incidents on {State.CoverageText(policy)} aren't paged any more: there's no active default policy, so the on-call engineers are only notified."
            : $"New incidents on {State.CoverageText(policy)} use the default policy, “{fallback.Name}”.";
    }

    private string EngineerName(string? engineerId) => State.FindEngineer(engineerId)?.Name ?? engineerId ?? "nobody";

    /// <summary>Whether another policy already has this name (the policy being edited doesn't count).</summary>
    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(State.IsPolicyNameTaken(name, editingId));

    /// <summary>The policy form's own messages: what the page knows that the field rules don't.</summary>
    private void NewForm(PolicyForm value)
    {
        value.Conflict = (isDefault, ids) => State.CoverageConflict(isDefault, ids, editingId);
        value.Names = ids => string.Join(", ", ids.Select(id => State.FindService(id)?.Name ?? id));
        form = value;
        formContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(formContext);
        formContext.OnFieldChanged += (_, args) => serverErrors?.Clear();
        formError = null;
    }

    private void OnDeleteHidden()
    {
        var policy = deleting;
        deleting = null;

        if (deleted || policy is null || !deleteButtons.TryGetValue(policy.Id, out var button))
        {
            deleted = false;
            focusStatus = true;
        }
        else
        {
            focusTarget = () => button.Element;
        }
    }

    private Task OnFormShownAsync() => MonitoringDisplay.FocusAsync(nameInput?.Element);

    /// <summary>The policy was saved before the dialog closed (or it was cancelled): focus goes back to the button that opened it.</summary>
    private void OnFormHidden()
    {
        var button = opener;
        focusTarget = () => button?.Element;
    }

    /// <summary>A policy changed, or paging moved on: the grid reads the policies again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenDeleteAsync(MonitoringEscalationPolicy policy)
    {
        deleting = policy;
        deleted = false;
        await deleteModal.ShowAsync();
    }

    private async Task OpenEditAsync(MonitoringEscalationPolicy policy)
    {
        editingId = policy.Id;
        opener = editButtons.GetValueOrDefault(policy.Id);
        NewForm(new PolicyForm
        {
            Name = policy.Name, IsDefault = policy.IsDefault, ServiceIds = policy.ServiceIds.ToList(), SecondaryAfterMinutes = policy.SecondaryAfterMinutes,
            HasThird = policy.ThirdEngineerId is not null, ThirdEngineerId = policy.ThirdEngineerId ?? string.Empty, ThirdAfterMinutes = policy.ThirdAfterMinutes,
            Repeats = policy.Repeats, IsActive = policy.IsActive,
        });

        await policyModal.ShowAsync();
    }

    private async Task OpenNewAsync()
    {
        editingId = 0;
        opener = newPolicyButton;
        NewForm(new PolicyForm());

        await policyModal.ShowAsync();
    }

    /// <summary>The active policy, other than the one being edited, that names a service now.</summary>
    private MonitoringEscalationPolicy? OtherPolicyFor(string serviceId) =>
        State.Policies.FirstOrDefault(policy => policy.Id != editingId && policy.IsActive && !policy.IsDefault && policy.ServiceIds.Contains(serviceId));

    /// <summary>The rows the grid asked for, in number order unless a column is sorted.</summary>
    private Task<GridDataProviderResult<PolicyRow>> ProvideRowsAsync(GridDataProviderRequest<PolicyRow> request) =>
        view.ApplyAsync(request, State.Policies.Select(policy => new PolicyRow(policy, State.CoverageText(policy), State.StepsText(policy),
            MonitoringState.PagingSchedule(policy.SecondaryAfterMinutes, policy.ThirdEngineerId is null ? null : policy.ThirdAfterMinutes, policy.Repeats).Count, State.PagingCount(policy.Id))).ToList());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        gridVersion++;
        Announce(true, "The view is reset: policies in number order, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await MonitoringDisplay.FocusAsync(countRegion);
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(FieldIdentifier field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(field, message);
        formContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Saves the policy once the form is valid: the name check finishes first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The state checks again; its answer maps to a field or the form's message, and the
    /// dialog closes once the policy is saved.
    /// </summary>
    private async Task SubmitAsync()
    {
        formError = null;
        serverErrors?.Clear();
        form.ServiceIds = State.Services.Where(service => form.ServiceIds.Contains(service.Id)).Select(service => service.Id).ToList();
        formContext.NotifyValidationStateChanged();

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!await formContext.ValidateAsync() || !nameFree)
            return;

        if (editingId == 0 && State.Policies.Count >= MonitoringState.MaxPolicies)
        {
            formError = string.Create(CultureInfo.InvariantCulture, $"This tab already has {MonitoringState.MaxPolicies} policies, the most it keeps. Delete one first.");
            return;
        }

        var name = form.Name.Trim();
        var ids = form.IsDefault ? new List<string>() : form.ServiceIds;
        var third = form.HasThird ? form.ThirdEngineerId : null;
        var thirdMinutes = form.HasThird ? form.ThirdAfterMinutes : null;
        var result = State.SavePolicy(editingId, name, form.IsDefault, ids, form.SecondaryAfterMinutes!.Value, third, thirdMinutes, form.Repeats, form.IsActive, out var saved);

        switch (result)
        {
            case MonitoringResult.Ok or MonitoringResult.NoChange:
                Announce(true, result == MonitoringResult.NoChange
                    ? $"Nothing changed: “{name}” already has these settings."
                    : (editingId == 0 ? $"Created “{saved!.Name}”. " : $"Saved “{saved!.Name}”. ")
                      + (saved.IsActive ? $"New incidents on {State.CoverageText(saved).ToLowerInvariant()} use it; incidents already paging keep the policy they started with." : "It's off, so no new incident uses it."));
                await policyModal.HideAsync();

                return;

            case MonitoringResult.Duplicate:
                ServerError(FieldIdentifier.Create(() => form.Name), $"Another policy is already called \"{name}\". Choose a different name.");

                return;

            case MonitoringResult.WrongState:
                if (State.CoverageConflict(form.IsDefault, ids, editingId) is { } conflict && form.IsDefault)
                    ServerError(FieldIdentifier.Create(() => form.IsDefault), $"“{conflict.Name}” is already the active default. Turn it off first, or save this one off.");
                else
                    ServerError(FieldIdentifier.Create(() => form.ServiceIds), PolicyForm.ConflictMessage(form.ServiceIds, State.CoverageConflict(false, ids, editingId), form.Names!));

                return;

            case MonitoringResult.NotFound:
                formError = editingId != 0 && State.FindPolicy(editingId) is null
                    ? "This policy was deleted meanwhile. Cancel and create a new one."
                    : "One of the chosen services was deleted meanwhile. Check the services.";

                return;

            case MonitoringResult.Invalid:
                formError = "The policy couldn't be saved: "
                    + (MonitoringState.PolicyProblem(name, form.IsDefault, ids, form.SecondaryAfterMinutes.Value, third, thirdMinutes, form.Repeats) ?? "this tab's storage is full.");

                return;

            default:
                formError = "Sign in as an admin to change escalation policies. Nothing was saved.";

                return;
        }
    }

    /// <summary>Turn on or off stays (its text changes), so focus stays on it.</summary>
    private void ToggleActive(MonitoringEscalationPolicy policy)
    {
        var turnOn = !policy.IsActive;
        var result = State.SetPolicyActive(policy.Id, turnOn);
        var conflict = turnOn ? State.CoverageConflict(policy.IsDefault, policy.ServiceIds, policy.Id) : null;

        Announce(result is MonitoringResult.Ok or MonitoringResult.NoChange, result switch
        {
            MonitoringResult.Ok when turnOn => $"“{policy.Name}” is on: new incidents on {State.CoverageText(policy).ToLowerInvariant()} use it.",
            MonitoringResult.Ok => $"“{policy.Name}” is off. {DeleteConsequence(policy)}",
            MonitoringResult.NoChange => $"“{policy.Name}” is already {(turnOn ? "on" : "off")}, so nothing changed.",
            MonitoringResult.WrongState => $"“{policy.Name}” can't be turned on: “{conflict?.Name}” already covers {(policy.IsDefault ? "the services without their own policy" : "some of its services")}. Turn that one off first. Nothing changed.",
            MonitoringResult.NotFound => "That policy was deleted meanwhile, so nothing changed.",
            _ => "Sign in as an admin to change escalation policies. Nothing changed.",
        });
    }

    private void ToggleService(string serviceId, ChangeEventArgs e)
    {
        var ids = form.ServiceIds.ToList();

        if (e.Value is true || (e.Value is string text && bool.TryParse(text, out var isOn) && isOn))
        {
            if (!ids.Contains(serviceId))
                ids.Add(serviceId);
        }
        else
        {
            ids.Remove(serviceId);
        }

        form.ServiceIds = ids;
        formContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.ServiceIds));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>The live preview of the form's policy, in words, with this week's rotation.</summary>
    private string Preview =>
        form.SecondaryAfterMinutes is { } minutes
            ? State.DescribePolicy(minutes, form.HasThird && form.ThirdEngineerId.Length > 0 ? form.ThirdEngineerId : null, form.HasThird ? form.ThirdAfterMinutes : null, form.Repeats)
            : "Enter the minutes before the secondary is paged to see who is paged and when.";

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The policy form's fields, with the limits of <see cref="MonitoringState.SavePolicy" />: a name, minutes from 1 to 60,
    /// and the cross-field rules: chosen services or the default, step 3 with both an engineer and minutes, and (when on)
    /// no service another active policy names, and one active default at most.
    /// </summary>
    private sealed class PolicyForm : IValidatableObject
    {
        /// <summary>Finds an active policy this one would overlap; set by the page.</summary>
        public Func<bool, IReadOnlyList<string>, MonitoringEscalationPolicy?>? Conflict { get; set; }

        public bool HasThird { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; }

        [Required(ErrorMessage = "Give the policy a name.")]
        [StringLength(MonitoringState.MaxPolicyNameLength, ErrorMessage = "Use 60 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Names services for a message; set by the page.</summary>
        public Func<IEnumerable<string>, string>? Names { get; set; }

        public int Repeats { get; set; }

        [Required(ErrorMessage = "Enter the minutes to wait before paging the secondary.")]
        [Range(MonitoringState.MinEscalationMinutes, MonitoringState.MaxEscalationMinutes, ErrorMessage = "Wait 1 to 60 minutes before paging the secondary.")]
        public int? SecondaryAfterMinutes { get; set; } = 5;

        public List<string> ServiceIds { get; set; } = new();

        [Range(MonitoringState.MinEscalationMinutes, MonitoringState.MaxEscalationMinutes, ErrorMessage = "Wait 1 to 60 minutes before step 3.")]
        public int? ThirdAfterMinutes { get; set; } = 10;

        public string ThirdEngineerId { get; set; } = string.Empty;

        /// <summary>Says which chosen services another active policy already names.</summary>
        public static string ConflictMessage(IReadOnlyList<string> serviceIds, MonitoringEscalationPolicy? other, Func<IEnumerable<string>, string> names) =>
            other is null
                ? "Another active policy already names one of these services. Each service has one active policy."
                : $"{names(serviceIds.Where(other.ServiceIds.Contains))} already {(serviceIds.Count(other.ServiceIds.Contains) == 1 ? "uses" : "use")} “{other.Name}”. Each service has one active policy: leave {(serviceIds.Count(other.ServiceIds.Contains) == 1 ? "it" : "them")} out, or change that policy first.";

        /// <summary>Runs once the fields are filled in: coverage, step 3, and the overlap with other active policies.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsDefault && ServiceIds.Count == 0)
                yield return new ValidationResult("Choose at least one service, or make it the default policy.", new[] { nameof(ServiceIds) });

            if (HasThird && ThirdEngineerId.Length == 0)
                yield return new ValidationResult("Choose the engineer step 3 pages, or turn step 3 off.", new[] { nameof(ThirdEngineerId) });

            if (HasThird && ThirdAfterMinutes is null)
                yield return new ValidationResult("Enter the minutes to wait before step 3.", new[] { nameof(ThirdAfterMinutes) });

            if (!IsActive || (!IsDefault && ServiceIds.Count == 0))
                yield break;

            if (Conflict?.Invoke(IsDefault, IsDefault ? Array.Empty<string>() : ServiceIds) is { } other)
                yield return IsDefault
                    ? new ValidationResult($"“{other.Name}” is already the active default. Turn it off first, or save this one off.", new[] { nameof(IsDefault) })
                    : new ValidationResult(ConflictMessage(ServiceIds, other, Names ?? (ids => string.Join(", ", ids))), new[] { nameof(ServiceIds) });
        }
    }

    /// <summary>One row of the list: a policy and its texts, with properties the CSV export can read.</summary>
    private sealed record PolicyRow(MonitoringEscalationPolicy Policy, string Covers, string Steps, int MaxPages, int PagingNow)
    {
        public string Name => Policy.Name;

        public string State => Policy.IsActive ? "On" : "Off";
    }

    #endregion
}
