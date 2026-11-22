using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The task form in a <see cref="Modal" />, an <see cref="EditForm" /> (rule WR-002): title (required, up to
/// <see cref="PbState.MaxTaskTitleLength" /> characters), description, column (new tasks), assignee (the project's people
/// only, or Unassigned; someone no longer on the project is refused), priority, due date (optional, within the project's
/// start and due dates, a cross-field rule), estimate (whole hours in range), and milestone (one of the project's).
/// Messages show after a field is touched or on submit, and a failed submit shows a summary that links to each field. It
/// saves with <see cref="PbState.CreateTask" /> or <see cref="PbState.UpdateTask" />, maps each refusal to its field or the
/// dialog's alert, and reports the outcome in <see cref="OnClosed" /> once the dialog is hidden, so the page announces it
/// and moves focus back (the Modal doesn't).
/// </summary>
public partial class TaskEditor : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private int formErrorCount;

    private string? formError;

    private string? formErrorText;

    private TaskForm form = new();

    private Modal modal = default!;

    /// <summary>Whether the dialog is open (or opening), so the form renders.</summary>
    private bool open;

    /// <summary>The outcome to report once the dialog is hidden; <see langword="null" /> means it was cancelled.</summary>
    private TaskEditorResult? pendingResult;

    private int projectId;

    private bool saving;

    /// <summary>The state's answers that belong to a field (an assignee no longer on the project, a deleted milestone), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    /// <summary>The task being edited, or 0 for a new one.</summary>
    private int taskId;

    private InputText? titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        ResetForm(new TaskForm());
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    /// <summary>Opens the form for a new task in a project, in a column (the Column select can change it).</summary>
    public async Task OpenCreateAsync(int forProjectId, PbTaskStatus inStatus)
    {
        Reset();
        projectId = forProjectId;
        ResetForm(new TaskForm { Status = inStatus });
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
            ResetForm(new TaskForm
            {
                Status = task.Status, Title = task.Title, Description = task.Description, AssigneeId = task.AssigneeId ?? string.Empty, Priority = task.Priority, DueDate = task.DueDate,
                EstimateHours = task.EstimateHours == 0 ? null : task.EstimateHours, MilestoneId = task.MilestoneId,
            });
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

    private static IReadOnlyList<PbPerson> MembersOf(PbState state, PbProject? project) =>
        project is null ? Array.Empty<PbPerson>() : state.People.Where(person => project.MemberIds.Contains(person.Id)).ToList();

    private async Task CancelAsync()
    {
        pendingResult = null;

        await modal.HideAsync();
    }

    private IReadOnlyList<PbPerson> Members(PbProject? project) => MembersOf(State, project);

    /// <summary>Any change clears the dialog's alert and the state's message on that field.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        formError = formErrorText = null;
        formErrorCount = 0;
    }

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
            await PbDisplay.FocusAsync(titleInput?.Element);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void Reset()
    {
        taskId = 0;
        formError = formErrorText = null;
        formErrorCount = 0;
        pendingResult = null;
        saving = false;
    }

    private void ResetForm(TaskForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>
    /// Saves once the form is valid (every rule runs, and a failed submit shows the summary) and closes the dialog on
    /// success. The state checks again; its refusal maps to a field or the alert, and the dialog stays open.
    /// </summary>
    private async Task SaveAsync()
    {
        if (saving)
            return;

        formError = formErrorText = null;

        if (State.FindProject(projectId) is not { } project)
        {
            ShowError(taskId == 0 ? "This project was deleted, so the task can't be created." : "This task's project was deleted, so the changes can't be saved.");

            return;
        }

        // The cross-field rules need the project as it is now.
        form.MemberIds = project.MemberIds.ToList();
        form.MilestoneIds = project.Milestones.Select(milestone => milestone.Id).ToList();
        form.ProjectKey = project.Key;
        form.ProjectStart = project.StartDate;
        form.ProjectDue = project.DueDate;
        form.AssigneeName = State.FindPerson(string.IsNullOrEmpty(form.AssigneeId) ? null : form.AssigneeId)?.Name;
        serverErrors?.Clear();

        if (!await editContext.ValidateAsync())
            return;

        var input = new PbTaskInput(form.Title.Trim(), (form.Description ?? string.Empty).Trim(), string.IsNullOrEmpty(form.AssigneeId) ? null : form.AssigneeId, form.Priority, form.DueDate?.Date,
            form.EstimateHours ?? 0, form.MilestoneId);
        saving = true;

        if (taskId == 0)
        {
            var result = State.CreateTask(project.Id, form.Status, input, out var created);

            switch (result)
            {
                case PbResult.Ok when created is not null:
                    var who = State.FindPerson(created.AssigneeId)?.Name;
                    pendingResult = new TaskEditorResult(true, true, created,
                        $"Created {State.TaskKey(created)} “{created.Title}” in {PbDisplay.StatusLabel(created.Status)}{(who is null ? ", unassigned" : $", assigned to {who}")}.");
                    await modal.HideAsync();

                    return;

                case PbResult.WrongState:
                    ShowError($"{project.Name} is completed, so it takes no new tasks. Nothing was created.");
                    break;

                case PbResult.NotFound:
                    ShowError("This project was deleted, so the task can't be created.");
                    break;

                case PbResult.NotAllowed:
                    ShowError("Your sign-in ended. Sign in again to add tasks. Nothing was created.");
                    break;

                case PbResult.Invalid:
                    Refused(project, input);
                    break;

                default:
                    ShowError("That didn't work, and nothing was created. Try again.");
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
                    ShowError("This task was deleted, so the changes can't be saved.");
                    break;

                case PbResult.WrongState:
                    ShowError($"{project.Name} is completed, so its tasks are read-only. Nothing was saved.");
                    break;

                case PbResult.NotAllowed:
                    ShowError("Your sign-in ended. Sign in again to edit tasks. Nothing was saved.");
                    break;

                case PbResult.Invalid:
                    Refused(project, input);
                    break;

                default:
                    ShowError("That didn't work, and nothing was saved. Try again.");
                    break;
            }
        }

        saving = false;
    }

    /// <summary>Why the state refused the fields when the form's own checks passed (the project changed meanwhile): on the field, or in the alert.</summary>
    private void Refused(PbProject project, PbTaskInput input)
    {
        if (input.AssigneeId is { } id && !project.MemberIds.Contains(id))
            ServerError(() => form.AssigneeId, $"{State.FindPerson(id)?.Name ?? "That person"} isn't on {project.Key} any more. Choose someone on the project, or Unassigned.");
        else if (input.MilestoneId is { } milestoneId && project.Milestones.All(milestone => milestone.Id != milestoneId))
            ServerError(() => form.MilestoneId, "That milestone was deleted. Choose another one, or None.");
        else if (input.DueDate is { } due && !PbState.IsDueInProject(project, due))
            ServerError(() => form.DueDate, $"{project.Key}'s dates changed: choose a due date from {PbDisplay.Date(project.StartDate)} to {PbDisplay.Date(project.DueDate)}.");
        else if (taskId == 0 && State.AllTasks.Count >= PbState.MaxTasks)
            ShowError($"This demo holds up to {PbState.MaxTasks.ToString("#,0", CultureInfo.InvariantCulture)} tasks, and it's full. A project manager can delete tasks first. Nothing was created.");
        else
            ShowError("The storage for this demo tab is full, so nothing was saved. Remove some comments or tasks, then try again.");
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    /// <summary>Shows the dialog's alert; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void ShowError(string message)
    {
        formErrorCount = message == formErrorText ? formErrorCount + 1 : 1;
        formErrorText = message;
        formError = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
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

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// A task's fields, with the limits of <see cref="PbState.CreateTask" /> and <see cref="PbState.UpdateTask" />: a required
    /// title, bounded text, an estimate in range, and cross-field rules against the project (set by the page before it
    /// validates): the assignee is on it, the milestone is one of its own, and the due date falls within its dates.
    /// </summary>
    private sealed class TaskForm : IValidatableObject
    {
        /// <summary>The chosen person's ID, or empty for Unassigned.</summary>
        public string AssigneeId { get; set; } = string.Empty;

        /// <summary>The chosen person's name, for the message; set by the page.</summary>
        public string? AssigneeName { get; set; }

        [StringLength(PbState.MaxDescriptionLength, ErrorMessage = "Use 2,000 characters or fewer for the description.")]
        public string? Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Range(0, PbState.MaxEstimateHours, ErrorMessage = "Enter whole hours from 0 to 80, or leave the estimate empty.")]
        public int? EstimateHours { get; set; }

        /// <summary>The project's people, set by the page.</summary>
        public IReadOnlyList<string> MemberIds { get; set; } = Array.Empty<string>();

        public int? MilestoneId { get; set; }

        /// <summary>The project's milestones, set by the page.</summary>
        public IReadOnlyList<int> MilestoneIds { get; set; } = Array.Empty<int>();

        public PbTaskPriority Priority { get; set; } = PbTaskPriority.Medium;

        /// <summary>The project's due date, set by the page.</summary>
        public DateTime ProjectDue { get; set; } = DateTime.MaxValue;

        /// <summary>The project's key, for the messages; set by the page.</summary>
        public string ProjectKey { get; set; } = string.Empty;

        /// <summary>The project's start date, set by the page.</summary>
        public DateTime ProjectStart { get; set; } = DateTime.MinValue;

        public PbTaskStatus Status { get; set; } = PbTaskStatus.Todo;

        [Required(ErrorMessage = "Enter a title for the task.")]
        [StringLength(PbState.MaxTaskTitleLength, ErrorMessage = "Use 120 characters or fewer for the title.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the assignee, the milestone, and the due date against the project.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(AssigneeId) && !MemberIds.Contains(AssigneeId))
                yield return new ValidationResult($"{AssigneeName ?? "That person"} isn't on {ProjectKey}. Choose someone on the project, or Unassigned.", new[] { nameof(AssigneeId) });

            if (MilestoneId is { } milestoneId && !MilestoneIds.Contains(milestoneId))
                yield return new ValidationResult("That milestone was deleted. Choose another one, or None.", new[] { nameof(MilestoneId) });

            if (DueDate is { } due && due.Date < ProjectStart.Date)
                yield return new ValidationResult($"Choose a due date on or after {ProjectKey}'s start, {PbDisplay.Date(ProjectStart)}.", new[] { nameof(DueDate) });
            else if (DueDate is { } late && late.Date > ProjectDue.Date)
                yield return new ValidationResult($"Choose a due date on or before {ProjectKey}'s due date, {PbDisplay.Date(ProjectDue)}.", new[] { nameof(DueDate) });
        }
    }

    #endregion
}

/// <summary>What happened in a <see cref="TaskEditor" />, reported once its dialog is hidden.</summary>
/// <param name="Saved">Whether the task was created or changed.</param>
/// <param name="Created">Whether it was the form for a new task.</param>
/// <param name="Task">The task, when it exists.</param>
/// <param name="Message">What to announce, or <see langword="null" /> when the form was cancelled.</param>
public sealed record TaskEditorResult(bool Saved, bool Created, PbTask? Task, string? Message);
