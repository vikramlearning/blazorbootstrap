using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The admin's SLA policies: a <see cref="Grid{TItem}" /> with one row per priority (Urgent first), its first reply and
/// resolution targets, and the open tickets of that priority with how many are breached, at risk, or overdue for a first
/// reply now, with a saved view, the column chooser, and CSV export. Each row opens a validated dialog that saves through
/// <see cref="HelpDeskState.UpdateSlaPolicy" />. The counts come from the saved policy, so they move as soon as a change is saved.
/// Below it, the escalation rules (AC-7a): a second grid with its own saved view, a validated dialog that shows how many
/// tickets saving would escalate, and a delete confirmation. Saving a rule runs the rules straight away.
/// </summary>
public partial class AdminSla : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button addRuleButton = default!;

    private Modal deleteRuleModal = default!;

    private HelpDeskEscalationRule? deletingRule;

    private readonly Dictionary<HelpDeskPriority, Button> editButtons = new();

    /// <summary>The rule the dialog edits; <see langword="null" /> while adding one.</summary>
    private int? editingRuleId;

    private string? editingRuleName;

    /// <summary>Set when the rule dialog or the delete dialog closes after a change, so focus goes to the announcement.</summary>
    private bool ruleChanged;

    private EditContext ruleContext = new(new RuleForm());

    private readonly Dictionary<int, Button> ruleEditButtons = new();

    private readonly LiveText ruleError = new();

    private RuleForm ruleForm = new();

    private readonly LiveText ruleFormError = new();

    private Modal ruleModal = default!;

    private ShowcaseUniqueCheck? ruleNameCheck;

    private InputText? ruleNameInput;

    private Grid<HelpDeskEscalationRule>? rulesGrid;

    /// <summary>Changes when the rules view is reset, so its grid is built again with its defaults.</summary>
    private int rulesGridVersion;

    private ShowcaseGridView rulesView = default!;

    private readonly LiveText ruleStatus = new();

    private ElementReference ruleStatusRegion;

    private EditContext editContext = new(new SlaForm());

    /// <summary>The priority whose targets the dialog edits; <see langword="null" /> while it is closed.</summary>
    private HelpDeskPriority? editing;

    private Modal editorModal = default!;

    private InputNumber<int?>? firstInput;

    private SlaForm form = new();

    private int formErrorCount;

    private string? formError;

    private Grid<HelpDeskSlaPolicy>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? pageError;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await view.LoadAsync();
        await rulesView.LoadAsync();
        viewLoaded = true;
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "sla-policies");
        rulesView = GridViews.Get(State.SiteSlug, "escalation-rules");
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Hours(int hours) => hours == 1 ? "1 hour" : string.Create(CultureInfo.InvariantCulture, $"{hours} hours");

    /// <summary>A rule's condition in words, like "Not resolved within 1 hour".</summary>
    private static string ConditionText(HelpDeskEscalationRule rule) =>
        rule.Trigger == HelpDeskEscalationTrigger.NoFirstReply ? $"No first reply within {Hours(rule.Hours)}" : $"Not resolved within {Hours(rule.Hours)}";

    /// <summary>How many tickets a save escalated, in words.</summary>
    private static string EscalatedText(int escalated, bool isActive) => !isActive
        ? "It's off, so it escalates nothing until it's switched on."
        : escalated switch
        {
            0 => "No ticket meets it right now; it escalates tickets as they reach it.",
            1 => "1 ticket was escalated now.",
            _ => string.Create(CultureInfo.InvariantCulture, $"{escalated} tickets were escalated now."),
        };

    private Task AddRuleAsync() => OpenRuleAsync(null);

    /// <summary>Asks before deleting a rule.</summary>
    private async Task AskDeleteRuleAsync(int ruleId)
    {
        if (State.FindEscalationRule(ruleId) is not { } rule)
            return;

        deletingRule = rule;
        ruleChanged = false;
        ruleError.Clear();

        await deleteRuleModal.ShowAsync();
    }

    /// <summary>Deletes the rule; the dialog closes and focus goes to the announcement.</summary>
    private async Task ConfirmDeleteRuleAsync()
    {
        if (deletingRule is not { } rule)
            return;

        switch (State.DeleteEscalationRule(rule.Id))
        {
            case HelpDeskAdminResult.Ok:
                ruleStatus.Set($"The rule \"{rule.Name}\" is deleted. The tickets it escalated stay at their tier.");
                ruleChanged = true;
                break;
            case HelpDeskAdminResult.NotFound:
                ruleError.Set("That rule no longer exists.");
                break;
            default:
                ruleError.Set("Only admins can change escalation rules. Sign in as an admin to make this change.");
                break;
        }

        await deleteRuleModal.HideAsync();
    }

    private Task EditRuleAsync(int ruleId) => State.FindEscalationRule(ruleId) is { } rule ? OpenRuleAsync(rule) : Task.CompletedTask;

    /// <summary>Whether another rule already has this name (the rule being edited doesn't count).</summary>
    private Task<bool> IsRuleNameTakenAsync(string name) =>
        Task.FromResult(State.EscalationRules.Any(rule => rule.Id != editingRuleId && string.Equals(rule.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

    /// <summary>After a delete, focus goes to the announcement; after Keep rule, back to the rule's Edit button.</summary>
    private async Task OnDeleteRuleHiddenAsync()
    {
        var ruleId = deletingRule?.Id;
        deletingRule = null;

        if (!ruleChanged && ruleId is { } id && ruleEditButtons.TryGetValue(id, out var button))
            await HelpDeskDisplay.FocusAsync(button.Element);
        else
            await HelpDeskDisplay.FocusAsync(ruleStatusRegion);
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: it goes to the announcement after a save, or back to the button that opened it.</summary>
    private async Task OnRuleHiddenAsync()
    {
        var ruleId = editingRuleId;
        editingRuleId = null;

        if (ruleChanged)
            await HelpDeskDisplay.FocusAsync(ruleStatusRegion);
        else if (ruleId is { } id && ruleEditButtons.TryGetValue(id, out var button))
            await HelpDeskDisplay.FocusAsync(button.Element);
        else
            await HelpDeskDisplay.FocusAsync(addRuleButton.Element);
    }

    private Task OnRuleShownAsync() => HelpDeskDisplay.FocusAsync(ruleNameInput?.Element);

    /// <summary>Opens the rule dialog, empty for a new rule (Urgent, not resolved, on) or with a rule's saved values.</summary>
    private async Task OpenRuleAsync(HelpDeskEscalationRule? rule)
    {
        editingRuleId = rule?.Id;
        editingRuleName = rule?.Name;
        ruleChanged = false;
        ruleForm = rule is null
            ? new RuleForm { Priority = nameof(HelpDeskPriority.Urgent), Trigger = HelpDeskEscalationTrigger.NotResolved, IsActive = true }
            : new RuleForm { Name = rule.Name, Priority = rule.Priority?.ToString() ?? string.Empty, Trigger = rule.Trigger, Hours = rule.Hours, IsActive = rule.IsActive };
        ruleContext = new EditContext(ruleForm);
        ruleFormError.Clear();
        ruleError.Clear();

        await ruleModal.ShowAsync();
    }

    /// <summary>The rules the grid asked for, in the order they were added unless a column is sorted.</summary>
    private Task<GridDataProviderResult<HelpDeskEscalationRule>> ProvideRulesAsync(GridDataProviderRequest<HelpDeskEscalationRule> request) =>
        rulesView.ApplyAsync(request, State.EscalationRules.OrderBy(rule => rule.Id));

    private async Task RefreshRulesGridAsync()
    {
        if (rulesGrid is not null)
            await rulesGrid.RefreshDataAsync();
    }

    /// <summary>Forgets the rules view and builds its grid again with the defaults; focus moves to the rules' announcement.</summary>
    private async Task ResetRulesViewAsync()
    {
        await rulesView.ResetAsync();
        rulesGridVersion++;
        ruleStatus.Set("The rules view is reset: the rules in the order they were added, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(ruleStatusRegion);
    }

    /// <summary>
    /// Saves the rule once the form is valid (rule WR-002: the summary shows on a failed submit) and the name is free; the
    /// state runs the rules straight away, and the announcement says how many tickets moved.
    /// </summary>
    private async Task SaveRuleAsync()
    {
        ruleFormError.Clear();

        var nameFree = ruleNameCheck is null || await ruleNameCheck.CheckNowAsync();

        if (!await ruleContext.ValidateAsync() || !nameFree)
            return;

        HelpDeskPriority? priority = Enum.TryParse<HelpDeskPriority>(ruleForm.Priority, out var parsed) && Enum.IsDefined(parsed) ? parsed : null;
        var adding = editingRuleId is null;

        switch (State.SaveEscalationRule(editingRuleId, ruleForm.Name, priority, ruleForm.Trigger, ruleForm.Hours!.Value, ruleForm.IsActive, out var saved, out var escalated))
        {
            case HelpDeskAdminResult.Ok when saved is not null:
                ruleStatus.Set($"The rule \"{saved.Name}\" is {(adding ? "added" : "saved")}. {EscalatedText(escalated, saved.IsActive)}");
                ruleChanged = true;
                break;
            case HelpDeskAdminResult.NoChange:
                ruleStatus.Set($"Nothing changed for the rule \"{editingRuleName}\".");
                break;
            case HelpDeskAdminResult.Duplicate:
                ruleFormError.Set("Another rule already has that name. Use a different one.");

                return;
            case HelpDeskAdminResult.LimitReached:
                ruleFormError.Set(string.Create(CultureInfo.InvariantCulture, $"There can be up to {HelpDeskState.MaxEscalationRules} escalation rules. Delete one first."));

                return;
            case HelpDeskAdminResult.Invalid:
                ruleFormError.Set("Check the name and hours: one of them isn't valid.");

                return;
            case HelpDeskAdminResult.NotFound:
                ruleFormError.Set("This rule no longer exists. Cancel to see the current rules.");

                return;
            default:
                ruleError.Set("Only admins can change escalation rules. Sign in as an admin to make this change.");
                break;
        }

        await ruleModal.HideAsync();
    }

    /// <summary>The open tickets of a priority now, and how many are breached, at risk, or still waiting past their first reply target.</summary>
    private (int Open, int Breached, int AtRisk, int FirstReplyOverdue) Counts(HelpDeskPriority priority)
    {
        var open = State.Tickets.Where(ticket => ticket.Priority == priority && ticket.Status != HelpDeskStatus.Resolved).ToList();
        var now = State.Now;

        return (open.Count,
            open.Count(ticket => State.Sla(ticket) == HelpDeskSlaState.Breached),
            open.Count(ticket => State.Sla(ticket) == HelpDeskSlaState.AtRisk),
            open.Count(ticket => ticket.FirstReplyAt is null && State.FirstReplyDueAt(ticket) < now));
    }

    /// <summary>Opens the dialog with a priority's saved targets.</summary>
    private async Task EditAsync(HelpDeskPriority priority)
    {
        var policy = State.Policy(priority);

        editing = priority;
        form = new SlaForm { FirstReplyHours = policy.FirstReplyHours, ResolveHours = policy.ResolveHours };
        editContext = new EditContext(form);
        formError = null;
        formErrorCount = 0;
        pageError = null;

        await editorModal.ShowAsync();
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: it goes back to the row's Edit button.</summary>
    private async Task OnEditorHiddenAsync()
    {
        var priority = editing;
        editing = null;

        if (priority is { } key && editButtons.TryGetValue(key, out var button))
            await HelpDeskDisplay.FocusAsync(button.Element);
        else
            await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    private Task OnEditorShownAsync() => HelpDeskDisplay.FocusAsync(firstInput?.Element);

    /// <summary>A policy or a ticket changed (here or elsewhere): the grid reads the policies again, so the counts follow.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        await RefreshRulesGridAsync();
        StateHasChanged();
    });

    /// <summary>The policies the grid asked for, Urgent first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<HelpDeskSlaPolicy>> ProvidePoliciesAsync(GridDataProviderRequest<HelpDeskSlaPolicy> request) =>
        view.ApplyAsync(request, State.SlaPolicies.OrderByDescending(policy => policy.Priority));

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
        SetStatus("The view is reset: Urgent first, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    /// <summary>
    /// Saves the targets once the form is valid (both in range, the resolution longer than the first reply); a failed save
    /// shows the summary (rule WR-002). The state checks again and its answer maps to a message, as before.
    /// </summary>
    private async Task SaveAsync()
    {
        formError = null;

        if (editing is not { } priority || !await editContext.ValidateAsync())
            return;

        var first = form.FirstReplyHours!.Value;
        var resolve = form.ResolveHours!.Value;

        switch (State.UpdateSlaPolicy(priority, first, resolve))
        {
            case HelpDeskAdminResult.Ok:
                var counts = Counts(priority);
                var name = priority.ToString().ToLowerInvariant();
                var targets = $"{priority} tickets now need a first reply within {Hours(first)} and a resolution within {Hours(resolve)}.";
                SetStatus(counts.Open == 0
                    ? $"{targets} There are no open {name} tickets right now; new ones follow the new targets."
                    : string.Create(CultureInfo.InvariantCulture,
                        $"{targets} The {counts.Open} open {name} {(counts.Open == 1 ? "ticket follows" : "tickets follow")} them now: {counts.Breached} breached, {counts.AtRisk} at risk."));
                break;
            case HelpDeskAdminResult.NoChange:
                SetStatus($"The {priority} targets are unchanged.");
                break;
            case HelpDeskAdminResult.Invalid:
                SetFormError($"The {priority} targets weren't saved: check that both are in range and the first reply isn't longer than the resolution.");

                return;
            default:
                pageError = "Only admins can change SLA policies. Sign in as an admin to make this change.";
                break;
        }

        await editorModal.HideAsync();
    }

    /// <summary>Shows a dialog error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void SetFormError(string message)
    {
        formErrorCount++;
        formError = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private HelpDeskState State { get; set; } = default!;

    /// <summary>The most hours the rule dialog's trigger allows.</summary>
    private int RuleMaxHours => ruleForm.Trigger == HelpDeskEscalationTrigger.NoFirstReply ? HelpDeskState.MaxFirstReplyHours : HelpDeskState.MaxResolveHours;

    /// <summary>What saving the rule dialog would do now, from its current values; empty until the hours are valid.</summary>
    private string? RulePreview
    {
        get
        {
            if (ruleForm.Hours is not { } hours || !HelpDeskState.ValidRuleHours(ruleForm.Trigger, hours))
                return null;

            if (!ruleForm.IsActive)
                return "The rule is off: saving it escalates nothing until it's switched on.";

            HelpDeskPriority? priority = Enum.TryParse<HelpDeskPriority>(ruleForm.Priority, out var parsed) && Enum.IsDefined(parsed) ? parsed : null;
            var matches = State.CountRuleMatches(priority, ruleForm.Trigger, hours, editingRuleId);

            return matches switch
            {
                0 => "No unresolved ticket meets this rule now. It escalates tickets as they reach it.",
                1 => "1 unresolved ticket meets this rule now and is escalated when you save.",
                _ => string.Create(CultureInfo.InvariantCulture, $"{matches} unresolved tickets meet this rule now and are escalated when you save."),
            };
        }
    }

    #endregion

    #region Nested Types

    /// <summary>A message in a live region; the same message again gets a counter, so a screen reader announces it again.</summary>
    private sealed class LiveText
    {
        private int count;

        private string? last;

        /// <summary>Gets the text to show.</summary>
        public string? Text { get; private set; }

        /// <summary>Hides the message; the counter carries on, so the same message later is still announced.</summary>
        public void Clear() => Text = null;

        /// <summary>Shows a message.</summary>
        public void Set(string message)
        {
            count = message == last ? count + 1 : 1;
            last = message;
            Text = count == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({count})");
        }
    }

    /// <summary>
    /// The rule dialog's fields, with the limits of <see cref="HelpDeskState.SaveEscalationRule" />: a name of 1 to 60
    /// characters, and hours of 1 to 720, or 1 to 168 for a first reply rule (a cross-field rule).
    /// </summary>
    private sealed class RuleForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the number of hours.")]
        [Range(1, HelpDeskState.MaxResolveHours, ErrorMessage = "Use 1 to 720 hours.")]
        public int? Hours { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Enter the rule name.")]
        [StringLength(HelpDeskState.MaxRuleNameLength, ErrorMessage = "Use 60 characters or fewer.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>The priority name; empty for any priority.</summary>
        public string Priority { get; set; } = string.Empty;

        public HelpDeskEscalationTrigger Trigger { get; set; } = HelpDeskEscalationTrigger.NotResolved;

        /// <summary>Runs once the hours are in range: a first reply rule waits a week at most, like the first reply target.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Trigger == HelpDeskEscalationTrigger.NoFirstReply && Hours is > HelpDeskState.MaxFirstReplyHours)
                yield return new ValidationResult("A first reply rule can wait up to 168 hours (one week).", new[] { nameof(Hours) });
        }
    }

    /// <summary>
    /// The SLA dialog's fields, with the limits of <see cref="HelpDeskState.UpdateSlaPolicy" />: first reply 1 to 168 hours,
    /// resolution 1 to 720 hours, and the resolution longer than the first reply (a cross-field rule).
    /// </summary>
    private sealed class SlaForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the first reply target in hours.")]
        [Range(1, HelpDeskState.MaxFirstReplyHours, ErrorMessage = "Use 1 to 168 hours.")]
        public int? FirstReplyHours { get; set; }

        [Required(ErrorMessage = "Enter the resolution target in hours.")]
        [Range(1, HelpDeskState.MaxResolveHours, ErrorMessage = "Use 1 to 720 hours.")]
        public int? ResolveHours { get; set; }

        /// <summary>Runs once both targets are in range: the first reply must come before the resolution.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FirstReplyHours is { } first && ResolveHours is { } resolve && first >= resolve)
                yield return new ValidationResult(
                    string.Create(CultureInfo.InvariantCulture, $"The first reply target must be shorter than the resolution target ({Hours(resolve)})."),
                    new[] { nameof(FirstReplyHours) });
        }
    }

    #endregion
}
