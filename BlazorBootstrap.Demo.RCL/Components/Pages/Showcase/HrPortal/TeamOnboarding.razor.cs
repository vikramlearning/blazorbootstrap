using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// New hires (managers and HR): the onboarding plans the signed-in person follows (<see cref="HrState.Onboardings" />: a
/// manager's direct reports', or everyone's for HR), people starting within 14 days first and highlighted, then the rest
/// by start date, newest first. Each shows the first day, template, progress in a <see cref="Progress" />, overdue tasks
/// in words, and the tasks (<see cref="OnboardingTaskList" />), where the manager ticks the manager's tasks and HR any task
/// (<see cref="HrState.SetTaskDone" />; WrongState says who can tick it). Each tick is announced in an always-rendered
/// status region; focus stays on the checkbox.
/// </summary>
public partial class TeamOnboarding : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How soon a start counts as "starting soon", in days.</summary>
    private const int SoonDays = 14;

    private bool statusIsError;

    private string? statusMessage;

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

    private void Announce(bool isError, string message)
    {
        statusIsError = isError;
        statusMessage = message;
    }

    /// <summary>Whether the signed-in person may tick a task: HR any; a manager the manager's tasks of their reports.</summary>
    private bool CanTick(HrOnboardingTask task, HrEmployee person, HrEmployee me) =>
        State.IsHrAdmin || (task.Owner == HrTaskOwner.Manager && person.ManagerId == me.Id);

    /// <summary>The plans with their people (those still in the directory), starting soon first.</summary>
    private IReadOnlyList<(HrEmployee Person, HrOnboarding Plan)> Hires(DateTime today) =>
        State.Onboardings
            .Select(plan => (Person: State.FindEmployee(plan.EmployeeId), Plan: plan))
            .Where(pair => pair.Person is not null)
            .Select(pair => (Person: pair.Person!, pair.Plan))
            .OrderBy(pair => IsStartingSoon(pair.Person, today) ? 0 : 1)
            .ThenBy(pair => IsStartingSoon(pair.Person, today) ? pair.Person.StartDate.Ticks : -pair.Person.StartDate.Ticks)
            .ToList();

    private static bool IsStartingSoon(HrEmployee person, DateTime today) => person.StartDate.Date >= today && person.StartDate.Date <= today.AddDays(SoonDays);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>"Complete the security training (Theo's, due Nov 5) and Two-week check-in (yours, due Nov 16)".</summary>
    private static string OverdueText(IReadOnlyList<HrOnboardingTask> overdue, HrEmployee person, HrEmployee? manager, HrEmployee me) =>
        LeaveText.JoinAnd(overdue.Select(task => $"{task.Title} ({OwnerPossessive(task.Owner, person, manager, me)}, due {HrDisplay.Date(task.Due)})").ToList());

    private static string OwnerPossessive(HrTaskOwner owner, HrEmployee person, HrEmployee? manager, HrEmployee me) => owner switch
    {
        HrTaskOwner.NewHire => $"{LeaveText.FirstName(person)}'s",
        HrTaskOwner.Manager when manager?.Id == me.Id => "yours",
        HrTaskOwner.Manager => manager is null ? "the manager's" : $"{manager.Name}'s",
        _ => "HR's",
    };

    /// <summary>"in 7 days", "today", or "3 weeks ago".</summary>
    private static string StartText(DateTime start, DateTime today)
    {
        var days = (int)(start.Date - today).TotalDays;

        return days switch
        {
            0 => "starts today",
            1 => "starts tomorrow",
            > 1 => $"starts in {HrDisplay.Days(days)}",
            _ => $"started {HrDisplay.Tenure(start, today).ToLowerInvariant()} ago",
        };
    }

    /// <summary>Ticks or unticks a task on someone's plan and announces the outcome with their new progress.</summary>
    private HrResult Toggle(HrEmployee person, HrOnboardingTask task, bool done)
    {
        if (State.CurrentEmployee is not { } me)
        {
            Announce(true, "Your sign-in ended. Sign in again to update checklists; nothing changed.");

            return HrResult.NotAllowed;
        }

        var result = State.SetTaskDone(person.Id, task.Id, done);
        var plan = State.OnboardingOf(person.Id);
        var progress = plan is null ? string.Empty : $" {person.Name} is at {HrState.Progress(plan).Done} of {HrState.Progress(plan).Total} done.";
        var manager = person.ManagerId is { } managerId ? State.FindEmployee(managerId) : null;

        switch (result)
        {
            case HrResult.Ok:
                Announce(false, done
                    ? $"Marked “{task.Title}” done for {person.Name}.{progress} {LeaveText.FirstName(person)} sees it on their checklist."
                    : $"Marked “{task.Title}” as not done for {person.Name}.{progress}");
                break;

            case HrResult.NoChange:
                Announce(false, $"“{task.Title}” was already {(done ? "done" : "not done")} for {person.Name}, so nothing changed.");
                break;

            case HrResult.WrongState:
                Announce(true, $"Only {OnboardingTaskList.OwnerWho(task.Owner, person, manager, me.Id)} can tick “{task.Title}”. Nothing changed.");
                break;

            case HrResult.NotFound:
                Announce(true, $"“{task.Title}” is no longer on {person.Name}'s checklist. Nothing changed.");
                break;

            case HrResult.NotAllowed:
                Announce(true, "Your sign-in ended. Sign in again to update checklists; nothing changed.");
                break;

            default:
                Announce(true, "That didn't work, and nothing changed. Try again.");
                break;
        }

        return result;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HrState State { get; set; } = default!;

    #endregion
}
