using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Leave approvals (managers and HR), a queue: a <see cref="Grid{TItem}" /> of the requests waiting for the signed-in
/// person (<see cref="HrState.TeamLeave" />), oldest first, with the employee, type, dates, working days, when it was
/// requested, the balance after approval in words (flagged when it no longer fits), and the employee's note; paging,
/// sorting, the column chooser, CSV export, and a saved view that can be reset. Rows can be selected with the grid's
/// checkboxes and approved or rejected together from a bulk bar, with one note for every employee (required to reject;
/// <see cref="HrState.DecideLeaveMany" />). A row opens a decision dialog with the full facts (the balance after approval,
/// teammates away on the same days, the note) to approve with an optional note or reject with a required one
/// (<see cref="HrState.DecideLeave" />). Below, the decided history as a second grid with its own saved view: status, who
/// decided and when, and the note; a row opens the employee's profile. Every result is announced in an always-rendered
/// status region, which takes focus when the bar or the dialog closes.
/// </summary>
public partial class Approvals : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private string bulkNote = string.Empty;

    private string? bulkNoteError;

    private int bulkNoteErrors;

    private ElementReference? bulkNoteInput;

    private Grid<DecidedRow>? decidedGrid;

    private int decidedPageSize = DefaultPageSize;

    /// <summary>Changes when the decided view is reset, so its grid is built again with its defaults.</summary>
    private int decidedVersion;

    private ShowcaseGridView decidedView = default!;

    /// <summary>The request the decision dialog is about.</summary>
    private int decisionId;

    private Modal decisionModal = default!;

    /// <summary>Whether the dialog decided its request; focus then goes to the status, which says how it went.</summary>
    private bool decisionMade;

    private string dialogNote = string.Empty;

    private string? dialogNoteError;

    private int dialogNoteErrors;

    private ElementReference? dialogNoteInput;

    /// <summary>Whether the note field (in the bar or the dialog) takes focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The buttons that open the decision dialog, by request, so focus can return to the one that opened it.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    /// <summary>The waiting requests selected in the grid (on its current page).</summary>
    private List<WaitingRow> selected = new();

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private bool viewsLoaded;

    private Grid<WaitingRow>? waitingGrid;

    private int waitingPageSize = DefaultPageSize;

    /// <summary>Changes when the waiting view is reset, so its grid is built again with its defaults.</summary>
    private int waitingVersion;

    private ShowcaseGridView waitingView = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The saved views live in the tab's session storage, so they load once the page is interactive; then the grids show.
        if (firstRender)
        {
            await waitingView.LoadAsync();
            await decidedView.LoadAsync();
            waitingPageSize = (await waitingView.GetSettingsAsync()).PageSize;
            decidedPageSize = (await decidedView.GetSettingsAsync()).PageSize;
            viewsLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HrDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        waitingView = GridViews.Get(State.SiteSlug, "approvals-waiting", DefaultPageSize);
        decidedView = GridViews.Get(State.SiteSlug, "approvals-decided", DefaultPageSize);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    private static string StatusWords(HrLeaveStatus status) => status switch
    {
        HrLeaveStatus.Pending => "waiting for a decision",
        _ => status.ToString().ToLowerInvariant(),
    };

    /// <summary>
    /// The employee's balance for the request's year if it's approved, in words, and whether it no longer fits (approving
    /// would be refused).
    /// </summary>
    private BalanceWords BalanceAfter(HrLeaveRequest request, HrEmployee? employee, HrLeavePolicy? policy)
    {
        if (employee is null || policy is null)
            return new BalanceWords(false, "Not available: the employee or the leave type is no longer on file.", "Not available");

        var year = request.From.Year;
        var first = LeaveText.FirstName(employee);
        var requests = State.LeaveOf(employee.Id);

        if (policy.IsUnlimited)
        {
            var used = requests.Where(item => item.PolicyId == policy.Id && item.From.Year == year && item.Status == HrLeaveStatus.Approved).Sum(item => item.Days);

            return new BalanceWords(false, $"{policy.Name} has no yearly limit. {first} has taken {HrDisplay.Days(used)} of it in {year}; this would make {HrDisplay.Days(used + request.Days)}.",
                "No yearly limit");
        }

        var balance = LeaveText.BalanceFor(requests, policy, year, request.Id);
        var usedAfter = balance.Used + request.Days;

        if (usedAfter > balance.Allowance)
        {
            var left = Math.Max(0, balance.Allowance - balance.Used);

            return new BalanceWords(true,
                $"Doesn't fit: {first} has {HrDisplay.Days(left)} of {policy.Name} left for {year} after approved leave, and this takes {HrDisplay.Days(request.Days)}. Reject it with a note, or ask HR to raise the allowance.",
                $"Doesn't fit: {HrDisplay.Days(left)} left");
        }

        var leftAfter = balance.Allowance - usedAfter - balance.Pending;
        var text = $"{first} would have {HrDisplay.Days(Math.Max(0, leftAfter))} of {policy.Name} left for {year} ({usedAfter} of {balance.Allowance} used)";

        if (balance.Pending > 0)
            text += $", with {HrDisplay.Days(balance.Pending)} more waiting{(leftAfter < 0 ? " that wouldn't all fit" : null)}";

        return new BalanceWords(false, text + ".", $"{HrDisplay.Days(Math.Max(0, leftAfter))} left for {year}");
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (waitingGrid is not null)
            await waitingGrid.UnSelectAllItemsAsync();

        selected = new List<WaitingRow>();
        bulkNoteError = null;
        SetStatus("Selection cleared.");
        focusTarget = () => statusRegion;
    }

    /// <summary>
    /// Decides the open dialog's request with its note. A missing or too long note is shown under the field, which takes
    /// focus; any other answer closes the dialog and is announced.
    /// </summary>
    private async Task DecideInDialogAsync(bool approve)
    {
        if (DecisionRequest is not { } request)
            return;

        var first = LeaveText.FirstName(State.FindEmployee(request.EmployeeId));
        var note = dialogNote.Trim();

        if (NoteProblem(approve, note, $"Write a note telling {first} why the request is rejected.") is { } problem)
        {
            dialogNoteErrors++;
            dialogNoteError = dialogNoteErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({dialogNoteErrors})");
            focusTarget = () => dialogNoteInput;

            return;
        }

        var label = $"{EmployeeName(request)}'s {LeaveText.RequestLabel(request, State.FindPolicy(request.PolicyId))}";
        var result = State.DecideLeave(request.Id, approve, note);
        SetStatus(Outcome(result, approve, label, first, request));
        decisionMade = true;

        await decisionModal.HideAsync();
    }

    /// <summary>
    /// Approves or rejects the selected requests together with the bar's note (<see cref="HrState.DecideLeaveMany" />), says
    /// how many were decided and why any weren't, then refreshes the grid and clears the selection; the bar closes, so focus
    /// moves to the announcement.
    /// </summary>
    private async Task DecideSelectedAsync(bool approve)
    {
        var note = bulkNote.Trim();

        if (NoteProblem(approve, note, "Write a note telling the employees why their requests are rejected.") is { } problem)
        {
            bulkNoteErrors++;
            bulkNoteError = bulkNoteErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkNoteErrors})");
            focusTarget = () => bulkNoteInput;

            return;
        }

        bulkNoteError = null;
        bulkNoteErrors = 0;

        var results = State.DecideLeaveMany(selected.Select(row => row.Id), approve, note);
        var decided = results.Count(item => item.Result == HrResult.Ok);
        var verb = approve ? "Approved" : "Rejected";
        var parts = new List<string>
        {
            decided == 0 ? $"Nothing was {verb.ToLowerInvariant()}." : $"{verb} {HrDisplay.Plural(decided, "request")}; each employee sees it under My leave.",
        };

        var shortBalance = results.Where(item => item.Result == HrResult.InsufficientBalance).Select(item => selected.FirstOrDefault(row => row.Id == item.RequestId)?.Employee).OfType<string>().ToList();

        if (shortBalance.Count > 0)
            parts.Add($"Not enough leave left for {LeaveText.JoinAnd(shortBalance)}, so {(shortBalance.Count == 1 ? "that request stays" : "those requests stay")} waiting.");

        var gone = results.Count(item => item.Result is HrResult.WrongState or HrResult.NotFound);

        if (gone > 0)
            parts.Add($"{HrDisplay.Plural(gone, "request was", "requests were")} already decided or cancelled.");

        var refused = results.Count(item => item.Result == HrResult.NotAllowed);

        if (refused > 0)
            parts.Add($"You can't decide {HrDisplay.Plural(refused, "request")}: your own, or someone who doesn't report to you.");

        if (results.Any(item => item.Result == HrResult.Invalid))
            parts.Add("This demo tab's storage is full, so nothing was saved.");

        SetStatus(string.Join(" ", parts));
        bulkNote = string.Empty;

        if (waitingGrid is not null)
        {
            await waitingGrid.RefreshDataAsync();
            await waitingGrid.UnSelectAllItemsAsync();
        }

        selected = new List<WaitingRow>();
        focusTarget = () => statusRegion;
    }

    /// <summary>The employee's name, or "A former employee".</summary>
    private string EmployeeName(HrLeaveRequest request) => State.FindEmployee(request.EmployeeId)?.Name ?? "A former employee";

    /// <summary>What's wrong with a decision note, or <see langword="null" />: needed to reject, and at most 300 characters.</summary>
    private static string? NoteProblem(bool approve, string note, string missing) =>
        !approve && note.Length == 0 ? missing
        : note.Length > HrState.MaxNoteLength ? FormattableString.Invariant($"Shorten the note to {HrState.MaxNoteLength} characters or fewer (now {note.Length}).")
        : null;

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a decision the row is gone, so focus goes to the status.</summary>
    private void OnDecisionHidden()
    {
        var id = decisionId;

        if (decisionMade || !openers.TryGetValue(id, out var opener) || State.FindLeave(id) is not { Status: HrLeaveStatus.Pending })
            focusTarget = () => statusRegion;
        else
            focusTarget = () => opener;

        decisionMade = false;
    }

    private Task OnDecisionShownAsync() => HrDisplay.FocusAsync(dialogNoteInput);

    private void OnSelectionChanged(HashSet<WaitingRow> items)
    {
        selected = items.ToList();

        if (selected.Count == 0)
            bulkNoteError = null;
    }

    /// <summary>Another screen or a new request changed the state: both grids read the requests again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridsAsync();
        StateHasChanged();
    });

    private async Task OpenDecisionAsync(int requestId)
    {
        decisionId = requestId;
        decisionMade = false;
        dialogNote = string.Empty;
        dialogNoteError = null;
        dialogNoteErrors = 0;

        await decisionModal.ShowAsync();
    }

    /// <summary>A decision's result in words, for the status region.</summary>
    private string Outcome(HrResult result, bool approve, string label, string first, HrLeaveRequest request)
    {
        var verb = approve ? "approved" : "rejected";

        switch (result)
        {
            case HrResult.Ok:
                return approve
                    ? $"Approved {label} ({HrDisplay.Days(request.Days)}). {first} sees it under My leave, and the days now count as used."
                    : $"Rejected {label}. {first} sees your note under My leave, and the days no longer count against the balance.";

            case HrResult.InsufficientBalance when State.FindEmployee(request.EmployeeId) is { } employee && State.FindPolicy(request.PolicyId) is { } policy:
                var balance = LeaveText.BalanceFor(State.LeaveOf(employee.Id), policy, request.From.Year, request.Id);

                return $"{employee.Name} doesn't have enough {policy.Name} left for {request.From.Year}: {HrDisplay.Days(Math.Max(0, balance.Allowance - balance.Used))} after approved leave, and this request takes {HrDisplay.Days(request.Days)}. Reject it with a note, or ask HR to raise the allowance. Nothing changed.";

            case HrResult.InsufficientBalance:
                return "There isn't enough leave left for this request. Reject it with a note, or ask HR to raise the allowance. Nothing changed.";

            case HrResult.NotAllowed when request.EmployeeId == State.CurrentEmployee?.Id:
                return "You can't decide your own request; your manager does. Nothing changed.";

            case HrResult.NotAllowed:
                return $"{Capitalize(label)} isn't yours to decide: they don't report to you. Nothing changed.";

            case HrResult.WrongState:
                return $"{Capitalize(label)} is already {StatusWords(State.FindLeave(request.Id)?.Status ?? HrLeaveStatus.Cancelled)}. Nothing changed.";

            case HrResult.Invalid:
                return $"This demo tab's storage is full, so the request wasn't {verb}. Nothing changed.";

            case HrResult.NotFound:
                return "That request, or the employee, no longer exists. Nothing changed.";

            default:
                return $"That didn't work, and the request wasn't {verb}. Try again.";
        }
    }

    /// <summary>
    /// Teammates' (and their manager's) approved leave on any of the request's days, in words.
    /// </summary>
    private string OverlapText(HrLeaveRequest request, HrEmployee? employee)
    {
        if (employee?.ManagerId is not { } managerId)
            return "No team to compare with.";

        var me = State.CurrentEmployee;
        var people = State.DirectReports(managerId).Where(person => person.Id != employee.Id).ToList();

        if (State.FindEmployee(managerId) is { } manager)
            people.Add(manager);

        var parts = people.SelectMany(person => State.LeaveOf(person.Id)
                .Where(item => item.Status == HrLeaveStatus.Approved && LeaveText.Overlaps(item, request.From, request.To))
                .Select(item => (person, item)))
            .OrderBy(pair => pair.item.From)
            .Select(pair => $"{(pair.person.Id == me?.Id ? "you" : pair.person.Name)} ({LeaveText.RequestLabel(pair.item, State.FindPolicy(pair.item.PolicyId))})")
            .ToList();

        return parts.Count == 0 ? "No one else on the team has approved leave on those days." : $"{Capitalize(LeaveText.JoinAnd(parts))}.";
    }

    /// <summary>The page of decided requests the grid asked for: the most recently decided first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<DecidedRow>> ProvideDecidedAsync(GridDataProviderRequest<DecidedRow> request)
    {
        var me = State.CurrentEmployee;
        var rows = State.TeamLeave.Where(item => item.Status != HrLeaveStatus.Pending).Select(item =>
        {
            var employee = State.FindEmployee(item.EmployeeId);
            var decider = item.Status == HrLeaveStatus.Cancelled ? $"{employee?.Name ?? "The employee"} (withdrawn)"
                : item.DecidedById is not { } deciderId ? "Not recorded"
                : deciderId == me?.Id ? "You"
                : State.FindEmployee(deciderId)?.Name ?? "A former colleague";
            var decidedAt = item.DecidedAt ?? item.CreatedAt;

            return new DecidedRow(item.Id, employee?.Name ?? "A former employee", employee?.Id, State.FindPolicy(item.PolicyId)?.Name ?? "Leave", HrDisplay.Range(item.From, item.To),
                item.From, item.Days, HrDisplay.LeaveLabel(item.Status), item.Status, decider, item.DecidedAt is { } at ? HrDisplay.Date(at) : "Not decided", decidedAt, item.DecisionNote);
        }).ToList();

        return decidedView.ApplyAsync(request, rows);
    }

    /// <summary>The page of waiting requests the grid asked for: oldest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<WaitingRow>> ProvideWaitingAsync(GridDataProviderRequest<WaitingRow> request)
    {
        var rows = State.TeamLeave.Where(item => item.Status == HrLeaveStatus.Pending).Select(item =>
        {
            var employee = State.FindEmployee(item.EmployeeId);
            var policy = State.FindPolicy(item.PolicyId);
            var balance = BalanceAfter(item, employee, policy);

            return new WaitingRow(item.Id, employee?.Name ?? "A former employee", policy?.Name ?? "Leave", HrDisplay.Range(item.From, item.To), item.From, item.Days,
                HrDisplay.Date(item.CreatedAt), item.CreatedAt, balance.ShortText, balance.Short, item.Note);
        }).ToList();

        return waitingView.ApplyAsync(request, rows);
    }

    private async Task RefreshGridsAsync()
    {
        if (waitingGrid is not null)
            await waitingGrid.RefreshDataAsync();

        if (decidedGrid is not null)
            await decidedGrid.RefreshDataAsync();
    }

    /// <summary>Forgets the decided list's saved view and builds its grid again with its defaults; focus moves to the status.</summary>
    private async Task ResetDecidedViewAsync()
    {
        await decidedView.ResetAsync();
        decidedPageSize = DefaultPageSize;
        decidedVersion++;
        SetStatus("The decided list's view is reset: most recent first, 10 a page, and every column shown.");
        focusTarget = () => statusRegion;
    }

    /// <summary>Forgets the waiting list's saved view and builds its grid again with its defaults; focus moves to the status.</summary>
    private async Task ResetWaitingViewAsync()
    {
        await waitingView.ResetAsync();
        waitingPageSize = DefaultPageSize;
        selected = new List<WaitingRow>();
        waitingVersion++;
        SetStatus("The waiting list's view is reset: oldest first, 10 a page, and every column shown.");
        focusTarget = () => statusRegion;
    }

    /// <summary>Keeps the decided grid's settings in its saved view, and follows the page size the selector chose.</summary>
    private async Task SaveDecidedSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            decidedPageSize = settings.PageSize;

        await decidedView.SaveSettingsAsync(settings);
    }

    /// <summary>Keeps the waiting grid's settings in its saved view, and follows the page size the selector chose.</summary>
    private async Task SaveWaitingSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            waitingPageSize = settings.PageSize;

        await waitingView.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private string WaitingRowClass(WaitingRow row) => selected.Any(item => item.Id == row.Id) ? "hr-row-selected" : string.Empty;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the request the decision dialog is about, or <see langword="null" /> when it's gone.</summary>
    private HrLeaveRequest? DecisionRequest => decisionId == 0 ? null : State.FindLeave(decisionId);

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A balance after approval in words.</summary>
    /// <param name="Short">Whether the request no longer fits (approving would be refused).</param>
    /// <param name="Text">The full sentence, for the decision dialog.</param>
    /// <param name="ShortText">A few words, for the grid.</param>
    private sealed record BalanceWords(bool Short, string Text, string ShortText);

    /// <summary>A decided request as a grid row: the words each column shows and exports, and the values it sorts by.</summary>
    private sealed record DecidedRow(int Id, string Employee, int? ProfileId, string Leave, string Dates, DateTime From, int Days, string Status, HrLeaveStatus StatusKind,
        string DecidedBy, string DecidedOn, DateTime DecidedAt, string Note);

    /// <summary>A waiting request as a grid row: the words each column shows and exports, and the values it sorts by.</summary>
    private sealed record WaitingRow(int Id, string Employee, string Leave, string Dates, DateTime From, int Days, string Requested, DateTime CreatedAt, string Balance, bool Short,
        string Note);

    #endregion
}
