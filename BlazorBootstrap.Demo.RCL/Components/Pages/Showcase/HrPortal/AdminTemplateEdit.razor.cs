using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// One onboarding template (HR admin): its tasks by due day ("3 days before day one", "Day one", "Day 14") with who does
/// each in words, Edit and Delete in each row, and an inline form to add or edit a task: what to do, who does it (new hire,
/// manager, or HR), and the due day as a native number from -<see cref="HrState.MaxTaskOffsetDays" /> to
/// <see cref="HrState.MaxTaskOffsetDays" /> with a live "N days before or after day one" hint
/// (<see cref="HrState.SaveTemplateTask" />). A side form renames it and edits its description
/// (<see cref="HrState.SaveTemplate" />; a taken name is shown under the field). Deleting a task or the template asks first
/// (plans already started keep their tasks) and runs once the dialog is hidden; deleting the template returns to the list,
/// which says so. A new template arrives with <c>?created=1</c>, announced after the page renders. An unknown ID shows
/// <see cref="HrNotFound" />.
/// </summary>
public partial class AdminTemplateEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Edit and Delete buttons of the tasks, by "edit-{id}" or "delete-{id}".</summary>
    private readonly Dictionary<string, ElementReference> rowButtons = new();

    private Modal deleteModal = default!;

    private ElementReference? deleteTemplateButton;

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference? descriptionInput;

    /// <summary>The task the delete dialog is about, or 0 when it is about the whole template.</summary>
    private int dialogTaskId;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The template the forms were loaded for, or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    /// <summary>Whether Delete was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    /// <summary>Whether the "template created" message waits for the status region to be on the page.</summary>
    private bool pendingCreatedNotice;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? taskDueError;

    private ElementReference? taskDueInput;

    private string taskDueText = "0";

    /// <summary>The task being edited in the form, or 0 when the form adds one.</summary>
    private int taskEditId;

    private HrTaskOwner taskOwner = HrTaskOwner.NewHire;

    private string taskTitle = string.Empty;

    private string? taskTitleError;

    private ElementReference? taskTitleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The status region renders once the role guard shows the page; fill it in the render after, so it is announced.
        if (pendingCreatedNotice && statusRegion is not null)
        {
            pendingCreatedNotice = false;

            if (Template is { } template)
            {
                Announce(true, $"Created the template “{template.Name}”. Add its tasks below; onboarding can start from it once it has one.");
                focusTarget = () => taskTitleInput;
                StateHasChanged();
            }
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

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = null;
        statusIsError = false;
        pendingCreatedNotice = Created == 1;
        LoadDetails();
        ResetTaskForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Says what a typed due day means, like "3 days before day one", or what is wrong with it.</summary>
    private static string DueHint(string text) =>
        AdminHelpers.TryNumber(text) is { } offset
            ? offset is < -HrState.MaxTaskOffsetDays or > HrState.MaxTaskOffsetDays
                ? string.Create(CultureInfo.InvariantCulture, $"Use -{HrState.MaxTaskOffsetDays} to {HrState.MaxTaskOffsetDays}: negative for before day one, 0 for day one.")
                : $"{AdminHelpers.OffsetPhrase(offset)}. Negative numbers are before the start date."
            : string.Create(CultureInfo.InvariantCulture, $"A whole number from -{HrState.MaxTaskOffsetDays} to {HrState.MaxTaskOffsetDays}: negative for before day one, 0 for day one.");

    private static string OwnerHelp(HrTaskOwner owner) => owner switch
    {
        HrTaskOwner.NewHire => "The new hire ticks it off in My onboarding.",
        HrTaskOwner.Manager => "Their manager ticks it off on New hires.",
        _ => "HR ticks it off on New hires.",
    };

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private void CancelEditTask()
    {
        var id = taskEditId;
        ResetTaskForm();

        if (rowButtons.TryGetValue("edit-" + id.ToString(CultureInfo.InvariantCulture), out var button))
            focusTarget = () => button;
    }

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await deleteModal.HideAsync();
    }

    private void LoadDetails()
    {
        name = Template?.Name ?? string.Empty;
        description = Template?.Description ?? string.Empty;
        nameError = descriptionError = null;
    }

    private void OnDeleteHidden()
    {
        var taskId = dialogTaskId;

        if (!pendingDelete)
        {
            if (taskId == 0)
                focusTarget = () => deleteTemplateButton;
            else if (rowButtons.TryGetValue("delete-" + taskId.ToString(CultureInfo.InvariantCulture), out var button) && DialogTask is not null)
                focusTarget = () => button;

            return;
        }

        pendingDelete = false;
        focusStatus = true;

        if (Template is not { } template)
            return;

        if (taskId == 0)
        {
            DeleteTemplate(template);

            return;
        }

        if (template.Tasks.FirstOrDefault(item => item.Id == taskId) is not { } task)
        {
            Announce(false, "That task was already deleted, so nothing changed.");

            return;
        }

        var result = State.DeleteTemplateTask(template.Id, taskId);

        switch (result)
        {
            case HrResult.Ok:
                if (taskEditId == taskId)
                    ResetTaskForm();

                Announce(true, $"Deleted the task “{task.Title}”. Plans already started keep it.");

                break;

            case HrResult.NotFound:
                Announce(false, "That task was already deleted, so nothing changed.");

                break;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can change templates. Nothing was deleted.");

                break;

            default:
                Announce(false, $"“{task.Title}” couldn't be deleted. Try again.");

                break;
        }
    }

    private void DeleteTemplate(HrOnboardingTemplate template)
    {
        var result = State.DeleteTemplate(template.Id);

        switch (result)
        {
            case HrResult.Ok:
            case HrResult.NotFound:
                Navigation.NavigateTo(AdminHelpers.TemplatesUrl + "?deleted=1");

                break;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can delete templates. Nothing was deleted.");

                break;

            default:
                Announce(false, $"“{template.Name}” couldn't be deleted. Try again.");

                break;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteTaskAsync(int taskId)
    {
        dialogTaskId = taskId;
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    private async Task OpenDeleteTemplateAsync()
    {
        dialogTaskId = 0;
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    private void ResetTaskForm()
    {
        taskEditId = 0;
        taskTitle = string.Empty;
        taskOwner = HrTaskOwner.NewHire;
        taskDueText = "0";
        taskTitleError = taskDueError = null;
    }

    /// <summary>Checks the name and description and saves them with <see cref="HrState.SaveTemplate" />.</summary>
    private void SaveDetails()
    {
        if (Template is not { } template)
            return;

        var trimmedName = name.Trim();

        nameError = AdminHelpers.TextProblem(name, HrState.MaxNameLength, "Give the template a name.", "name")
            ?? (State.Templates.FirstOrDefault(item => item.Id != template.Id && string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)) is { } same
                ? $"That name is taken by “{same.Name}”. Choose another."
                : null);
        descriptionError = description.Trim().Length > HrState.MaxDescriptionLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the description to {HrState.MaxDescriptionLength} characters or fewer (now {description.Trim().Length}).")
            : null;

        focusTarget = nameError is not null ? () => nameInput : descriptionError is not null ? () => descriptionInput : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save the name and description again.");

            return;
        }

        var previousName = template.Name;
        var result = State.SaveTemplate(template.Id, trimmedName, description, out var saved);

        switch (result)
        {
            case HrResult.Ok when saved is not null:
                LoadDetails();
                Announce(true, previousName == saved.Name
                    ? $"Saved the description of “{saved.Name}”."
                    : $"Renamed “{previousName}” to “{saved.Name}”. Plans already started keep the old name.");

                return;

            case HrResult.Duplicate:
                nameError = "That name is taken by another template. Choose another.";
                focusTarget = () => nameInput;
                Announce(false, "That name is taken. Nothing was saved.");

                return;

            case HrResult.NoChange:
                Announce(true, "Nothing changed: the name and description are already saved like this.");

                return;

            case HrResult.NotFound:
                Announce(false, "This template was deleted, so the changes couldn't be saved.");

                return;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can change templates. Nothing was saved.");
                focusStatus = true;

                return;

            case HrResult.Invalid:
                Announce(false, "The template couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    /// <summary>Checks the task form and saves it with <see cref="HrState.SaveTemplateTask" />.</summary>
    private void SaveTask()
    {
        if (Template is not { } template)
            return;

        var editing = taskEditId == 0 ? null : template.Tasks.FirstOrDefault(item => item.Id == taskEditId);

        if (taskEditId != 0 && editing is null)
        {
            ResetTaskForm();
            Announce(false, "That task was deleted, so the changes couldn't be saved.");
            focusStatus = true;

            return;
        }

        var title = taskTitle.Trim();
        var offset = AdminHelpers.TryNumber(taskDueText);

        taskTitleError = AdminHelpers.TextProblem(taskTitle, HrState.MaxNameLength, "Say what to do, like “Pick an onboarding buddy”.", "task");
        taskDueError = offset is not { } value || value is < -HrState.MaxTaskOffsetDays or > HrState.MaxTaskOffsetDays
            ? string.Create(CultureInfo.InvariantCulture, $"Enter a whole number of days from -{HrState.MaxTaskOffsetDays} to {HrState.MaxTaskOffsetDays}: negative for before day one, 0 for day one.")
            : null;

        focusTarget = taskTitleError is not null ? () => taskTitleInput : taskDueError is not null ? () => taskDueInput : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save the task again.");

            return;
        }

        var result = State.SaveTemplateTask(template.Id, taskEditId, title, taskOwner, offset!.Value);
        var when = AdminHelpers.OffsetPhrase(offset.Value).ToLowerInvariant();

        switch (result)
        {
            case HrResult.Ok:
                Announce(true, editing is null
                    ? $"Added “{title}” for {OwnerWords(taskOwner)}, due {when}."
                    : $"Saved “{title}” for {OwnerWords(taskOwner)}, due {when}.");
                ResetTaskForm();
                focusTarget = () => taskTitleInput;

                return;

            case HrResult.NoChange:
                Announce(true, "Nothing changed: the task is already saved like this.");

                return;

            case HrResult.NotFound:
                ResetTaskForm();
                Announce(false, "The template or the task was deleted, so nothing was saved.");
                focusStatus = true;

                return;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can change templates. Nothing was saved.");
                focusStatus = true;

                return;

            case HrResult.Invalid:
                Announce(false, editing is null && template.Tasks.Count >= HrState.MaxTemplateTasks
                    ? $"A template can have up to {HrState.MaxTemplateTasks} tasks. Delete one to add another. Nothing was saved."
                    : "The task couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }

        static string OwnerWords(HrTaskOwner owner) => owner switch
        {
            HrTaskOwner.NewHire => "the new hire",
            HrTaskOwner.Manager => "the manager",
            _ => "HR",
        };
    }

    /// <summary>Fills the task form with a task, to change it.</summary>
    private void StartEditTask(int taskId)
    {
        if (Template?.Tasks.FirstOrDefault(item => item.Id == taskId) is not { } task)
            return;

        taskEditId = task.Id;
        taskTitle = task.Title;
        taskOwner = task.Owner;
        taskDueText = task.DueOffsetDays.ToString(CultureInfo.InvariantCulture);
        taskTitleError = taskDueError = null;
        focusTarget = () => taskTitleInput;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets 1 when the template was just created on the list page.</summary>
    [SupplyParameterFromQuery(Name = "created")] public int? Created { get; set; }

    /// <summary>Gets or sets the template ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private HrTemplateTask? DialogTask => dialogTaskId == 0 ? null : Template?.Tasks.FirstOrDefault(item => item.Id == dialogTaskId);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HrState State { get; set; } = default!;

    private HrOnboardingTemplate? Template => State.FindTemplate(Id);

    #endregion
}
