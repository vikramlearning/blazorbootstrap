using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The admin's SLA policies: one row per priority (Urgent first) with its first reply and resolution targets as number
/// inputs, saved row by row through <see cref="HelpDeskState.UpdateSlaPolicy" />, next to the open tickets of that priority
/// and how many are breached, at risk, or overdue for a first reply now. The counts come from the saved policy, so they move
/// as soon as a change is saved.
/// </summary>
public partial class AdminSla : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The priorities, most urgent first.</summary>
    private static readonly HelpDeskPriority[] priorities = Enum.GetValues<HelpDeskPriority>().OrderByDescending(priority => priority).ToArray();

    /// <summary>What the admin typed for the first reply target, per priority, until it is saved.</summary>
    private readonly Dictionary<HelpDeskPriority, string> firstDrafts = new();

    private readonly Dictionary<HelpDeskPriority, string?> firstErrors = new();

    private readonly Dictionary<HelpDeskPriority, ElementReference> firstInputs = new();

    private string? pageError;

    /// <summary>What the admin typed for the resolution target, per priority, until it is saved.</summary>
    private readonly Dictionary<HelpDeskPriority, string> resolveDrafts = new();

    private readonly Dictionary<HelpDeskPriority, string?> resolveErrors = new();

    private readonly Dictionary<HelpDeskPriority, ElementReference> resolveInputs = new();

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string FormId(HelpDeskPriority priority) => $"hd-sla-form-{Key(priority)}";

    private static string Hours(int hours) => hours == 1 ? "1 hour" : string.Create(CultureInfo.InvariantCulture, $"{hours} hours");

    private static string Key(HelpDeskPriority priority) => priority.ToString().ToLowerInvariant();

    /// <summary>Reads a target in whole hours, or says what is wrong with it.</summary>
    private static (int Hours, string? Error) ParseHours(string? text, int max, string what)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (0, $"Enter the {what} target in hours.");

        if (!int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var hours))
            return (0, "Use whole hours, like 4.");

        return hours < 1 || hours > max
            ? (hours, string.Create(CultureInfo.InvariantCulture, $"Use 1 to {max} hours."))
            : (hours, null);
    }

    private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);

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

    private string FirstValue(HelpDeskPriority priority) =>
        firstDrafts.TryGetValue(priority, out var draft) ? draft : Text(State.Policy(priority).FirstReplyHours);

    /// <summary>Whether a row has typed values that differ from its saved policy.</summary>
    private bool IsDirty(HelpDeskPriority priority) =>
        FirstValue(priority).Trim() != Text(State.Policy(priority).FirstReplyHours) || ResolveValue(priority).Trim() != Text(State.Policy(priority).ResolveHours);

    private void OnFirstInput(HelpDeskPriority priority, ChangeEventArgs args)
    {
        firstDrafts[priority] = args.Value?.ToString() ?? string.Empty;

        // Inline validation: once a field shows an error, it updates as the admin types, and clears when fixed.
        if (firstErrors.GetValueOrDefault(priority) is not null)
            Validate(priority);
    }

    private void OnResolveInput(HelpDeskPriority priority, ChangeEventArgs args)
    {
        resolveDrafts[priority] = args.Value?.ToString() ?? string.Empty;

        if (resolveErrors.GetValueOrDefault(priority) is not null || firstErrors.GetValueOrDefault(priority) is not null)
            Validate(priority);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private string ResolveValue(HelpDeskPriority priority) =>
        resolveDrafts.TryGetValue(priority, out var draft) ? draft : Text(State.Policy(priority).ResolveHours);

    private async Task SaveAsync(HelpDeskPriority priority)
    {
        pageError = null;
        var (first, resolve) = Validate(priority);

        if (firstErrors.GetValueOrDefault(priority) is not null)
        {
            await HelpDeskDisplay.FocusAsync(firstInputs.GetValueOrDefault(priority));

            return;
        }

        if (resolveErrors.GetValueOrDefault(priority) is not null)
        {
            await HelpDeskDisplay.FocusAsync(resolveInputs.GetValueOrDefault(priority));

            return;
        }

        switch (State.UpdateSlaPolicy(priority, first, resolve))
        {
            case HelpDeskAdminResult.Ok:
                firstDrafts.Remove(priority);
                resolveDrafts.Remove(priority);
                var counts = Counts(priority);
                var name = priority.ToString().ToLowerInvariant();
                var targets = $"{priority} tickets now need a first reply within {Hours(first)} and a resolution within {Hours(resolve)}.";
                SetStatus(counts.Open == 0
                    ? $"{targets} There are no open {name} tickets right now; new ones follow the new targets."
                    : string.Create(CultureInfo.InvariantCulture,
                        $"{targets} The {counts.Open} open {name} {(counts.Open == 1 ? "ticket follows" : "tickets follow")} them now: {counts.Breached} breached, {counts.AtRisk} at risk."));
                break;
            case HelpDeskAdminResult.NoChange:
                firstDrafts.Remove(priority);
                resolveDrafts.Remove(priority);
                SetStatus($"The {priority} targets are unchanged.");
                break;
            case HelpDeskAdminResult.Invalid:
                pageError = $"The {priority} targets weren't saved: check that both are in range and the first reply isn't longer than the resolution.";
                break;
            default:
                pageError = "Only admins can change SLA policies. Sign in as an admin to make this change.";
                break;
        }
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Checks a row: both targets in range, and the first reply not longer than the resolution.</summary>
    private (int First, int Resolve) Validate(HelpDeskPriority priority)
    {
        var (first, firstError) = ParseHours(FirstValue(priority), HelpDeskState.MaxFirstReplyHours, "first reply");
        var (resolve, resolveError) = ParseHours(ResolveValue(priority), HelpDeskState.MaxResolveHours, "resolution");

        if (firstError is null && resolveError is null && first > resolve)
            firstError = $"The first reply target can't be longer than the resolution target ({Hours(resolve)}).";

        firstErrors[priority] = firstError;
        resolveErrors[priority] = resolveError;

        return (first, resolve);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
