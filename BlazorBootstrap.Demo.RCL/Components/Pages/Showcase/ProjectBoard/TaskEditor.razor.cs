using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The task form in a <see cref="Modal" />: title, description, column (new tasks), assignee (the project's people only, or
/// Unassigned), priority, due date (native <c>date</c> input), estimate (native <c>number</c> input), and milestone. Limits
/// come from <see cref="PbState" /> (<c>maxlength</c>, <c>min</c>, <c>max</c>, and help text). It checks the fields itself,
/// ties each error to its field (<c>aria-describedby</c>, <c>aria-invalid</c>), and focuses the first invalid field after
/// the render that shows it. It saves with <see cref="PbState.CreateTask" /> or <see cref="PbState.UpdateTask" />, maps
/// every result to a message, and reports the outcome in <see cref="OnClosed" /> once the dialog is hidden, so the page
/// announces it and moves focus back (the Modal doesn't).
/// </summary>
public partial class TaskEditor : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string assigneeId = string.Empty;

    private string? assigneeError;

    private ElementReference? assigneeInput;

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference? descriptionInput;

    private string? dueError;

    private ElementReference? dueInput;

    private string dueText = string.Empty;

    private string? estimateError;

    private ElementReference? estimateInput;

    private string estimateText = string.Empty;

    private string? formError;

    private ElementReference? formStatus;

    /// <summary>Returns the field to focus after the next render (the first invalid one).</summary>
    private Func<ElementReference?>? focusTarget;

    private string? milestoneError;

    private ElementReference? milestoneInput;

    private string milestoneText = string.Empty;

    private Modal modal = default!;

    /// <summary>Whether the dialog is open (or opening), so the form renders.</summary>
    private bool open;

    /// <summary>The outcome to report once the dialog is hidden; <see langword="null" /> means it was cancelled.</summary>
    private TaskEditorResult? pendingResult;

    private PbTaskPriority priority = PbTaskPriority.Medium;

    private int projectId;

    private bool saving;

    private PbTaskStatus status = PbTaskStatus.Todo;

    /// <summary>The task being edited, or 0 for a new one.</summary>
    private int taskId;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference? titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await PbDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Opens the form for a new task in a project, in a column (the Column select can change it).</summary>
    public async Task OpenCreateAsync(int forProjectId, PbTaskStatus inStatus)
    {
        Reset();
        projectId = forProjectId;
        status = inStatus;
        open = true;

        // Called by the page, not by an event of this component: render the form before the dialog opens.
        StateHasChanged();
        await modal.ShowAsync();
    }

    /// <summary>Opens the form with a task's fields.</summary>
    public async Task OpenEditAsync(int editTaskId)
    {
        Reset();

        if (State.FindTask(editTaskId) is { } task)
        {
            taskId = task.Id;
            projectId = task.ProjectId;
            status = task.Status;
            title = task.Title;
            description = task.Description;
            assigneeId = task.AssigneeId ?? string.Empty;
            priority = task.Priority;
            dueText = task.DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
            estimateText = task.EstimateHours == 0 ? string.Empty : task.EstimateHours.ToString(CultureInfo.InvariantCulture);
            milestoneText = task.MilestoneId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }
        else
        {
            taskId = editTaskId;
            projectId = 0;
        }

        open = true;

        StateHasChanged();
        await modal.ShowAsync();
    }

    #endregion

    #region Private Methods

    private static string InputClass(string baseClass, string? error) => error is null ? baseClass : baseClass + " is-invalid";

    private static IReadOnlyList<PbPerson> MembersOf(PbState state, PbProject? project) =>
        project is null ? Array.Empty<PbPerson>() : state.People.Where(person => project.MemberIds.Contains(person.Id)).ToList();

    private async Task CancelAsync()
    {
        pendingResult = null;

        await modal.HideAsync();
    }

    /// <summary>Why the state refused the fields when the form's own checks passed.</summary>
    private string InvalidText(PbProject project, PbTaskInput input)
    {
        if (input.AssigneeId is { } id && !project.MemberIds.Contains(id))
            return $"{State.FindPerson(id)?.Name ?? "That person"} isn't on {project.Key} any more. Choose someone on the project, or Unassigned. Nothing was saved.";

        if (input.MilestoneId is { } milestoneId && project.Milestones.All(milestone => milestone.Id != milestoneId))
            return "That milestone was deleted. Choose another one, or None. Nothing was saved.";

        if (taskId == 0 && State.AllTasks.Count >= PbState.MaxTasks)
            return $"This demo holds up to {PbState.MaxTasks} tasks, and it's full. A project manager can delete tasks first. Nothing was created.";

        return "The storage for this demo tab is full, so nothing was saved. Remove some comments or tasks, then try again.";
    }

    private IReadOnlyList<PbPerson> Members(PbProject? project) => MembersOf(State, project);

    /// <summary>Reports the outcome (or a cancel) now that the dialog is hidden.</summary>
    private async Task OnHiddenAsync()
    {
        var result = pendingResult ?? new TaskEditorResult(false, taskId == 0, null, null);
        pendingResult = null;
        open = false;
        saving = false;

        await OnClosed.InvokeAsync(result);
    }

    private async Task OnShownAsync()
    {
        if (open)
            await PbDisplay.FocusAsync(titleInput);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void Reset()
    {
        taskId = 0;
        title = description = assigneeId = dueText = estimateText = milestoneText = string.Empty;
        priority = PbTaskPriority.Medium;
        status = PbTaskStatus.Todo;
        titleError = descriptionError = assigneeError = dueError = estimateError = milestoneError = formError = null;
        pendingResult = null;
        saving = false;
    }

    /// <summary>Checks the fields, saves, and closes the dialog on success; errors stay in the form.</summary>
    private async Task SaveAsync()
    {
        if (saving)
            return;

        formError = null;

        if (State.FindProject(projectId) is not { } project)
        {
            formError = taskId == 0 ? "This project was deleted, so the task can't be created." : "This task's project was deleted, so the changes can't be saved.";
            focusTarget = () => formStatus;

            return;
        }

        var trimmedTitle = title.Trim();
        var trimmedDescription = description.Trim();
        string? assignee = assigneeId.Length == 0 ? null : assigneeId;
        DateTime? due = null;
        var estimate = 0;
        int? milestoneId = null;

        titleError = trimmedTitle.Length == 0 ? "Enter a title for the task."
            : trimmedTitle.Length > PbState.MaxTaskTitleLength ? $"Shorten the title to {PbState.MaxTaskTitleLength} characters or fewer (now {trimmedTitle.Length})."
            : null;
        descriptionError = trimmedDescription.Length > PbState.MaxDescriptionLength
            ? $"Shorten the description to {PbState.MaxDescriptionLength} characters or fewer (now {trimmedDescription.Length})."
            : null;
        assigneeError = assignee is not null && !project.MemberIds.Contains(assignee)
            ? $"{State.FindPerson(assignee)?.Name ?? "That person"} isn't on {project.Key}. Choose someone on the project, or Unassigned."
            : null;

        if (dueText.Trim().Length == 0)
            dueError = null;
        else if (!DateTime.TryParseExact(dueText.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            dueError = "Enter the due date as a full date, like 2026-12-03, or leave it empty.";
        else if (parsed < MinDate || parsed > MaxDate)
            dueError = $"Choose a due date from {PbDisplay.Date(MinDate)} to {PbDisplay.Date(MaxDate)}.";
        else
        {
            dueError = null;
            due = parsed;
        }

        if (estimateText.Trim().Length == 0)
            estimateError = null;
        else if (!int.TryParse(estimateText.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out estimate) || estimate > PbState.MaxEstimateHours)
            estimateError = $"Enter whole hours from 0 to {PbState.MaxEstimateHours}, or leave it empty.";
        else
            estimateError = null;

        if (milestoneText.Length == 0)
            milestoneError = null;
        else if (int.TryParse(milestoneText, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && project.Milestones.Any(milestone => milestone.Id == id))
        {
            milestoneError = null;
            milestoneId = id;
        }
        else
            milestoneError = "That milestone was deleted. Choose another one, or None.";

        focusTarget = titleError is not null ? () => titleInput
            : descriptionError is not null ? () => descriptionInput
            : assigneeError is not null ? () => assigneeInput
            : dueError is not null ? () => dueInput
            : estimateError is not null ? () => estimateInput
            : milestoneError is not null ? () => milestoneInput
            : null;

        if (focusTarget is not null)
        {
            formError = "Fix the highlighted fields, then save again.";

            return;
        }

        var input = new PbTaskInput(trimmedTitle, trimmedDescription, assignee, priority, due, estimateError is null ? estimate : 0, milestoneId);
        saving = true;

        if (taskId == 0)
        {
            var result = State.CreateTask(project.Id, status, input, out var created);

            switch (result)
            {
                case PbResult.Ok when created is not null:
                    var who = State.FindPerson(created.AssigneeId)?.Name;
                    pendingResult = new TaskEditorResult(true, true, created,
                        $"Created {State.TaskKey(created)} “{created.Title}” in {PbDisplay.StatusLabel(created.Status)}{(who is null ? ", unassigned" : $", assigned to {who}")}.");
                    await modal.HideAsync();

                    return;

                case PbResult.WrongState:
                    formError = $"{project.Name} is completed, so it takes no new tasks. Nothing was created.";
                    break;

                case PbResult.NotFound:
                    formError = "This project was deleted, so the task can't be created.";
                    break;

                case PbResult.NotAllowed:
                    formError = "Your sign-in ended. Sign in again to add tasks. Nothing was created.";
                    break;

                case PbResult.Invalid:
                    formError = InvalidText(project, input);
                    break;

                default:
                    formError = "That didn't work, and nothing was created. Try again.";
                    break;
            }
        }
        else
        {
            var result = State.UpdateTask(taskId, input);
            var key = State.FindTask(taskId) is { } task ? State.TaskKey(task) : "The task";

            switch (result)
            {
                case PbResult.Ok:
                    pendingResult = new TaskEditorResult(true, false, State.FindTask(taskId), $"Saved the changes to {key}.");
                    await modal.HideAsync();

                    return;

                case PbResult.NoChange:
                    pendingResult = new TaskEditorResult(false, false, State.FindTask(taskId), $"{key} had no changes to save, so nothing changed.");
                    await modal.HideAsync();

                    return;

                case PbResult.NotFound:
                    formError = "This task was deleted, so the changes can't be saved.";
                    break;

                case PbResult.NotAllowed:
                    formError = "Your sign-in ended. Sign in again to edit tasks. Nothing was saved.";
                    break;

                case PbResult.Invalid:
                    formError = InvalidText(project, input);
                    break;

                default:
                    formError = "That didn't work, and nothing was saved. Try again.";
                    break;
            }
        }

        saving = false;
        focusTarget = () => formStatus;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the callback with the outcome once the dialog is hidden (saved, no change, or cancelled).</summary>
    [Parameter] public EventCallback<TaskEditorResult> OnClosed { get; set; }

    private string DialogTitle
    {
        get
        {
            if (taskId != 0)
                return State.FindTask(taskId) is { } task ? $"Edit {State.TaskKey(task)}" : "Task not found";

            return State.FindProject(projectId) is { } project ? $"New task in {project.Key}" : "Project not found";
        }
    }

    /// <summary>The latest due date the state accepts (three years after the seed time).</summary>
    private static DateTime MaxDate => PbSeedData.Now.AddYears(3).Date;

    /// <summary>The earliest due date the state accepts (three years before the seed time, rounded up to a whole day).</summary>
    private static DateTime MinDate => PbSeedData.Now.AddYears(-3).Date.AddDays(1);

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}

/// <summary>What happened in a <see cref="TaskEditor" />, reported once its dialog is hidden.</summary>
/// <param name="Saved">Whether the task was created or changed.</param>
/// <param name="Created">Whether it was the form for a new task.</param>
/// <param name="Task">The task, when it exists.</param>
/// <param name="Message">What to announce, or <see langword="null" /> when the form was cancelled.</param>
public sealed record TaskEditorResult(bool Saved, bool Created, PbTask? Task, string? Message);
