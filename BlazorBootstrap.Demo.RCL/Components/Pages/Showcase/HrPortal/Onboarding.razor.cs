using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// My onboarding: the signed-in employee's plan (<see cref="HrState.OnboardingOf" />) with progress ("6 of 10 done") in a
/// <see cref="Progress" />, the template, first day, tasks left, and overdue tasks in words; then the tasks grouped
/// "Before day one", "First week", and "Later" (<see cref="OnboardingTaskList" />). The new hire ticks their own tasks
/// (<see cref="HrState.SetTaskDone" />); the manager's and HR's tasks are read-only and say who does them. Without a plan,
/// the page explains that HR starts one. Each tick is announced in an always-rendered status region; focus stays on the
/// checkbox.
/// </summary>
public partial class Onboarding : ComponentBase, IDisposable
{
    #region Fields and Constants

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>"Complete the security training (yours, due Nov 5) and Two-week check-in (Rafael Costa's, due Nov 16)".</summary>
    private static string OverdueText(IReadOnlyList<HrOnboardingTask> overdue, HrEmployee? manager) =>
        LeaveText.JoinAnd(overdue.Select(task => $"{task.Title} ({OwnerPossessive(task.Owner, manager)}, due {HrDisplay.Date(task.Due)})").ToList());

    private static string OwnerPossessive(HrTaskOwner owner, HrEmployee? manager) => owner switch
    {
        HrTaskOwner.NewHire => "yours",
        HrTaskOwner.Manager => manager is null ? "your manager's" : $"{manager.Name}'s",
        _ => "HR's",
    };

    /// <summary>"3 weeks ago", "today", or "in 7 days".</summary>
    private static string StartText(DateTime start, DateTime today)
    {
        var days = (int)(start.Date - today).TotalDays;

        return days switch
        {
            0 => "today",
            1 => "tomorrow",
            > 1 => $"in {HrDisplay.Days(days)}",
            _ => $"{HrDisplay.Tenure(start, today).ToLowerInvariant()} ago",
        };
    }

    /// <summary>Ticks or unticks one of the new hire's own tasks and announces the outcome with the new progress.</summary>
    private HrResult Toggle(HrOnboardingTask task, bool done)
    {
        if (State.CurrentEmployee is not { } me)
        {
            Announce(true, "Your sign-in ended. Sign in again to update your checklist; nothing changed.");

            return HrResult.NotAllowed;
        }

        var result = State.SetTaskDone(me.Id, task.Id, done);
        var plan = State.OnboardingOf(me.Id);
        var progress = plan is null ? "" : $" {HrState.Progress(plan).Done} of {HrState.Progress(plan).Total} done.";
        var manager = me.ManagerId is { } managerId ? State.FindEmployee(managerId) : null;

        switch (result)
        {
            case HrResult.Ok:
                Announce(false, done ? $"Marked “{task.Title}” done.{progress} Your manager sees it on their new hires page." : $"Marked “{task.Title}” as not done.{progress}");
                break;

            case HrResult.NoChange:
                Announce(false, $"“{task.Title}” was already {(done ? "done" : "not done")}, so nothing changed.");
                break;

            case HrResult.WrongState:
                Announce(true, $"Only {OnboardingTaskList.OwnerWho(task.Owner, me, manager, me.Id)} can tick “{task.Title}”. Nothing changed.");
                break;

            case HrResult.NotFound:
                Announce(true, $"“{task.Title}” is no longer on your checklist. Nothing changed.");
                break;

            case HrResult.NotAllowed:
                Announce(true, "Your sign-in ended. Sign in again to update your checklist; nothing changed.");
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
