using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Leave approvals (managers and HR): the requests waiting for the signed-in person (<see cref="HrState.TeamLeave" />),
/// oldest first, each with the employee, type and dates, working days, when it was requested, their balance after approval
/// in words (or that it no longer fits), teammates' approved leave on the same days, and the employee's note. Approve takes
/// an optional note and Reject a required one (<see cref="HrState.DecideLeave" />); every result is announced in an
/// always-rendered status region, and focus moves to the next waiting request (or the status region when none is left).
/// Below, the decided history: status, who decided and when, and the note.
/// </summary>
public partial class Approvals : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The waiting requests' headings, by request, so focus can move to the next one.</summary>
    private readonly Dictionary<int, ElementReference> headings = new();

    private readonly Dictionary<int, string?> noteErrors = new();

    private readonly Dictionary<int, ElementReference> noteInputs = new();

    private readonly Dictionary<int, string> notes = new();

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

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
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

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

    /// <summary>
    /// The employee's balance for the request's year if it's approved, in words, and whether it no longer fits (approving
    /// would be refused).
    /// </summary>
    private BalanceWords BalanceAfter(HrLeaveRequest request, HrEmployee? employee, HrLeavePolicy? policy)
    {
        if (employee is null || policy is null)
            return new BalanceWords(false, "Not available: the employee or the leave type is no longer on file.");

        var year = request.From.Year;
        var first = LeaveText.FirstName(employee);
        var requests = State.LeaveOf(employee.Id);

        if (policy.IsUnlimited)
        {
            var used = requests.Where(item => item.PolicyId == policy.Id && item.From.Year == year && item.Status == HrLeaveStatus.Approved).Sum(item => item.Days);

            return new BalanceWords(false, $"{policy.Name} has no yearly limit. {first} has taken {HrDisplay.Days(used)} of it in {year}; this would make {HrDisplay.Days(used + request.Days)}.");
        }

        var balance = LeaveText.BalanceFor(requests, policy, year, request.Id);
        var usedAfter = balance.Used + request.Days;

        if (usedAfter > balance.Allowance)
        {
            return new BalanceWords(true,
                $"Doesn't fit: {first} has {HrDisplay.Days(Math.Max(0, balance.Allowance - balance.Used))} of {policy.Name} left for {year} after approved leave, and this takes {HrDisplay.Days(request.Days)}. Reject it with a note, or ask HR to raise the allowance.");
        }

        var leftAfter = balance.Allowance - usedAfter - balance.Pending;
        var text = $"{first} would have {HrDisplay.Days(Math.Max(0, leftAfter))} of {policy.Name} left for {year} ({usedAfter} of {balance.Allowance} used)";

        if (balance.Pending > 0)
            text += $", with {HrDisplay.Days(balance.Pending)} more waiting{(leftAfter < 0 ? " that wouldn't all fit" : null)}";

        return new BalanceWords(false, text + ".");
    }

    private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>"You", the decider's name, or "A former colleague".</summary>
    private string DeciderName(int? deciderId, HrEmployee me) =>
        deciderId is not { } id ? "Not recorded" : id == me.Id ? "You" : State.FindEmployee(id)?.Name ?? "A former colleague";

    /// <summary>Approves or rejects a waiting request with the note, announces the result, and moves focus on.</summary>
    private void Decide(int requestId, bool approve)
    {
        var waitingBefore = State.TeamLeave.Where(item => item.Status == HrLeaveStatus.Pending).ToList();

        if (waitingBefore.FirstOrDefault(item => item.Id == requestId) is not { } request)
        {
            Announce(false, State.FindLeave(requestId) is { } gone ? $"That request is already {StatusWords(gone.Status)}. Nothing changed." : "That request no longer exists. Nothing changed.");
            focusTarget = () => statusRegion;

            return;
        }

        var employee = State.FindEmployee(request.EmployeeId);
        var name = employee?.Name ?? "A former employee";
        var first = LeaveText.FirstName(employee);
        var policy = State.FindPolicy(request.PolicyId);
        var label = LeaveText.RequestLabel(request, policy);
        var verb = approve ? "approved" : "rejected";
        var note = (notes.GetValueOrDefault(requestId) ?? string.Empty).Trim();

        noteErrors[requestId] = !approve && note.Length == 0 ? $"Write a note telling {first} why the request is rejected."
            : note.Length > HrState.MaxNoteLength ? FormattableString.Invariant($"Shorten the note to {HrState.MaxNoteLength} characters or fewer (now {note.Length}).")
            : null;

        if (noteErrors[requestId] is not null)
        {
            Announce(false, $"The request wasn't {verb}. Fix the note, then try again.");
            FocusNote(requestId);

            return;
        }

        var index = waitingBefore.IndexOf(request);
        var result = State.DecideLeave(requestId, approve, note);

        switch (result)
        {
            case HrResult.Ok:
                notes.Remove(requestId);
                noteErrors.Remove(requestId);
                noteInputs.Remove(requestId);
                headings.Remove(requestId);
                Announce(true, approve
                    ? $"Approved {name}'s {label} ({HrDisplay.Days(request.Days)}). {first} sees it under My leave, and the days now count as used."
                    : $"Rejected {name}'s {label}. {first} sees your note under My leave, and the days no longer count against the balance.");

                var waitingAfter = State.TeamLeave.Where(item => item.Status == HrLeaveStatus.Pending).ToList();
                var nextId = (waitingAfter.ElementAtOrDefault(index) ?? waitingAfter.LastOrDefault())?.Id;
                focusTarget = () => nextId is { } id && headings.TryGetValue(id, out var heading) ? heading : statusRegion;

                return;

            case HrResult.InsufficientBalance when employee is not null && policy is not null:
                var balance = LeaveText.BalanceFor(State.LeaveOf(employee.Id), policy, request.From.Year, request.Id);
                Announce(false,
                    $"{name} doesn't have enough {policy.Name} left for {request.From.Year}: {HrDisplay.Days(Math.Max(0, balance.Allowance - balance.Used))} after approved leave, and this request takes {HrDisplay.Days(request.Days)}. Reject it with a note, or ask HR to raise the allowance. Nothing changed.");
                break;

            case HrResult.InsufficientBalance:
                Announce(false, $"{name} doesn't have enough leave left for this request. Reject it with a note, or ask HR to raise the allowance. Nothing changed.");
                break;

            case HrResult.NotAllowed when request.EmployeeId == State.CurrentEmployee?.Id:
                Announce(false, "You can't decide your own request; your manager does. Nothing changed.");
                break;

            case HrResult.NotAllowed:
                Announce(false, $"{name} doesn't report to you, so you can't decide this request. Nothing changed.");
                break;

            case HrResult.WrongState:
                Announce(false, $"{name}'s request for {label} is already {StatusWords(State.FindLeave(requestId)?.Status ?? HrLeaveStatus.Cancelled)}. Nothing changed.");
                break;

            case HrResult.Invalid when !approve && note.Length == 0:
                noteErrors[requestId] = $"Write a note telling {first} why the request is rejected.";
                Announce(false, "The request wasn't rejected. Fix the note, then try again.");
                FocusNote(requestId);

                return;

            case HrResult.Invalid when note.Length > HrState.MaxNoteLength:
                noteErrors[requestId] = FormattableString.Invariant($"Shorten the note to {HrState.MaxNoteLength} characters or fewer (now {note.Length}).");
                Announce(false, $"The request wasn't {verb}. Fix the note, then try again.");
                FocusNote(requestId);

                return;

            case HrResult.Invalid:
                Announce(false, $"This demo tab's storage is full, so the request wasn't {verb}. Nothing changed.");
                break;

            case HrResult.NotFound:
                Announce(false, "That request, or the employee, no longer exists. Nothing changed.");
                break;

            default:
                Announce(false, $"That didn't work, and the request wasn't {verb}. Try again.");
                break;
        }

        focusTarget = () => statusRegion;
    }

    private void FocusNote(int requestId) => focusTarget = () => noteInputs.TryGetValue(requestId, out var input) ? input : statusRegion;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Teammates' (and their manager's) approved leave on any of the request's days, in words.</summary>
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

    private static string StatusWords(HrLeaveStatus status) => status switch
    {
        HrLeaveStatus.Pending => "waiting for a decision",
        _ => status.ToString().ToLowerInvariant(),
    };

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A balance after approval in words.</summary>
    /// <param name="Short">Whether the request no longer fits (approving would be refused).</param>
    /// <param name="Text">The words.</param>
    private sealed record BalanceWords(bool Short, string Text);

    #endregion
}
