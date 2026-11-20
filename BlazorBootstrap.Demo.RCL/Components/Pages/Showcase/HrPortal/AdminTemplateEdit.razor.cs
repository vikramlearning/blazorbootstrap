using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// One onboarding template (HR admin): its tasks by due day ("3 days before day one", "Day one", "Day 14") with who does
/// each in words, Edit and Delete in each row (line items, so a plain table), and a validated form to add or edit a task
/// (rule WR-002): what to do (required, up to 80 characters), who does it (new hire, manager, or HR), and the due day as a
/// number from -<see cref="HrState.MaxTaskOffsetDays" /> to <see cref="HrState.MaxTaskOffsetDays" /> with a live "N days
/// before or after day one" hint; a new hire's task can't be due before day one, when they can't sign in yet (a cross-field
/// rule; <see cref="HrState.SaveTemplateTask" />). A side form renames it and edits its description, with the name's
/// uniqueness checked as you type (<see cref="HrState.SaveTemplate" />). Both forms show a linked error summary after a
/// failed submit. Deleting a task or the template asks first (plans already started keep their tasks) and runs once the
/// dialog is hidden; deleting the template returns to the list, which says so. A new template arrives with
/// <c>?created=1</c>, announced after the page renders. An unknown ID shows <see cref="HrNotFound" />.
/// </summary>
public partial class AdminTemplateEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Edit and Delete buttons of the tasks, by "edit-{id}" or "delete-{id}".</summary>
    private readonly Dictionary<string, ElementReference> rowButtons = new();

    private Modal deleteModal = default!;

    private ElementReference? deleteTemplateButton;

    /// <summary>The description as typed, for the live counter (the bound value changes only on change).</summary>
    private string descriptionText = string.Empty;

    private EditContext detailsContext = new(new DetailsForm());

    private DetailsForm detailsForm = new();

    /// <summary>The task the delete dialog is about, or 0 when it is about the whole template.</summary>
    private int dialogTaskId;

    /// <summary>The due day as typed, for the live hint (the bound value changes only on change).</summary>
    private string dueText = "0";

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The template the forms were loaded for, or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private ShowcaseUniqueCheck? nameCheck;

    /// <summary>Whether Delete was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    /// <summary>Whether the "template created" message waits for the status region to be on the page.</summary>
    private bool pendingCreatedNotice;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private EditContext taskContext = new(new TaskForm());

    /// <summary>The task being edited in the form, or 0 when the form adds one.</summary>
    private int taskEditId;

    private TaskForm taskForm = new();

    private InputText? taskTitleInput;

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
                focusTarget = () => taskTitleInput?.Element;
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
        HrTaskOwner.NewHire => "The new hire ticks it off in My onboarding, from day one.",
        HrTaskOwner.Manager => "Their manager ticks it off on New hires.",
        _ => "HR ticks it off on New hires.",
    };

    private static string OwnerWords(HrTaskOwner owner) => owner switch
    {
        HrTaskOwner.NewHire => "the new hire",
        HrTaskOwner.Manager => "the manager",
        _ => "HR",
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

    /// <summary>Whether another template already has this name (this one doesn't count).</summary>
    private Task<bool> IsNameTakenAsync(string value) =>
        Task.FromResult(State.Templates.Any(item => item.Id != Id && string.Equals(item.Name, value, StringComparison.OrdinalIgnoreCase)));

    private void LoadDetails()
    {
        detailsForm = new DetailsForm { Name = Template?.Name ?? string.Empty, Description = Template?.Description ?? string.Empty };
        detailsContext = new EditContext(detailsForm);
        descriptionText = detailsForm.Description;
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

    private void OnDescriptionInput(ChangeEventArgs args) => descriptionText = args.Value?.ToString() ?? string.Empty;

    private void OnDueInput(ChangeEventArgs args) => dueText = args.Value?.ToString() ?? string.Empty;

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
        taskForm = new TaskForm();
        taskContext = new EditContext(taskForm);
        dueText = "0";
    }

    /// <summary>
    /// Saves the name and description once the form is valid (the uniqueness check finishes first, then every rule runs,
    /// and a failed submit shows the summary) with <see cref="HrState.SaveTemplate" />, whose answer maps to a message.
    /// </summary>
    private async Task SaveDetailsAsync()
    {
        if (Template is not { } template)
            return;

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!detailsContext.Validate() || !nameFree)
            return;

        var previousName = template.Name;
        var result = State.SaveTemplate(template.Id, detailsForm.Name.Trim(), detailsForm.Description, out var saved);
        focusStatus = true;

        switch (result)
        {
            case HrResult.Ok when saved is not null:
                LoadDetails();
                Announce(true, previousName == saved.Name
                    ? $"Saved the description of “{saved.Name}”."
                    : $"Renamed “{previousName}” to “{saved.Name}”. Plans already started keep the old name.");

                return;

            case HrResult.Duplicate:
                Announce(false, "Another template already has that name. Choose another; nothing was saved.");

                return;

            case HrResult.NoChange:
                Announce(true, "Nothing changed: the name and description are already saved like this.");

                return;

            case HrResult.NotFound:
                Announce(false, "This template was deleted, so the changes couldn't be saved.");

                return;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can change templates. Nothing was saved.");

                return;

            case HrResult.Invalid:
                Announce(false, "The template couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    /// <summary>
    /// Saves the task once the form is valid (a failed submit shows the summary) with <see cref="HrState.SaveTemplateTask" />,
    /// whose answer maps to a message.
    /// </summary>
    private Task SaveTaskAsync()
    {
        if (Template is not { } template || !taskContext.Validate())
            return Task.CompletedTask;

        var editing = taskEditId == 0 ? null : template.Tasks.FirstOrDefault(item => item.Id == taskEditId);

        if (taskEditId != 0 && editing is null)
        {
            ResetTaskForm();
            Announce(false, "That task was deleted, so the changes couldn't be saved.");
            focusStatus = true;

            return Task.CompletedTask;
        }

        var title = taskForm.Title.Trim();
        var owner = taskForm.Owner;
        var offset = taskForm.DueOffsetDays!.Value;
        var result = State.SaveTemplateTask(template.Id, taskEditId, title, owner, offset);
        var when = AdminHelpers.OffsetPhrase(offset).ToLowerInvariant();

        switch (result)
        {
            case HrResult.Ok:
                Announce(true, editing is null
                    ? $"Added “{title}” for {OwnerWords(owner)}, due {when}."
                    : $"Saved “{title}” for {OwnerWords(owner)}, due {when}.");
                ResetTaskForm();
                focusTarget = () => taskTitleInput?.Element;

                break;

            case HrResult.NoChange:
                Announce(true, "Nothing changed: the task is already saved like this.");
                focusStatus = true;

                break;

            case HrResult.NotFound:
                ResetTaskForm();
                Announce(false, "The template or the task was deleted, so nothing was saved.");
                focusStatus = true;

                break;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can change templates. Nothing was saved.");
                focusStatus = true;

                break;

            case HrResult.Invalid:
                Announce(false, editing is null && template.Tasks.Count >= HrState.MaxTemplateTasks
                    ? $"A template can have up to {HrState.MaxTemplateTasks} tasks. Delete one to add another. Nothing was saved."
                    : "The task couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");
                focusStatus = true;

                break;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                break;
        }

        return Task.CompletedTask;
    }

    /// <summary>Fills the task form with a task, to change it.</summary>
    private void StartEditTask(int taskId)
    {
        if (Template?.Tasks.FirstOrDefault(item => item.Id == taskId) is not { } task)
            return;

        taskEditId = task.Id;
        taskForm = new TaskForm { Title = task.Title, Owner = task.Owner, DueOffsetDays = task.DueOffsetDays };
        taskContext = new EditContext(taskForm);
        dueText = task.DueOffsetDays.ToString(CultureInfo.InvariantCulture);
        focusTarget = () => taskTitleInput?.Element;
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

    #region Nested Types

    /// <summary>The name-and-description form's fields, with the limits of <see cref="HrState.SaveTemplate" />.</summary>
    private sealed class DetailsForm
    {
        [StringLength(HrState.MaxDescriptionLength, ErrorMessage = "Shorten the description to 200 characters or fewer.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Give the template a name.")]
        [StringLength(HrState.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// The task form's fields, with the limits of <see cref="HrState.SaveTemplateTask" />: what to do in 1 to 80 characters,
    /// and a due day from -90 to 90; a new hire's own task is due on day one or later (a cross-field rule).
    /// </summary>
    private sealed class TaskForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the due day: a whole number, negative for before day one, 0 for day one.")]
        [Range(-HrState.MaxTaskOffsetDays, HrState.MaxTaskOffsetDays, ErrorMessage = "Use -90 to 90 days: negative for before day one, 0 for day one.")]
        public int? DueOffsetDays { get; set; } = 0;

        public HrTaskOwner Owner { get; set; } = HrTaskOwner.NewHire;

        [Required(ErrorMessage = "Say what to do, like “Pick an onboarding buddy”.")]
        [StringLength(HrState.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the task.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Runs once the fields are in range: the new hire can't sign in before day one, so their tasks start then.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Owner == HrTaskOwner.NewHire && DueOffsetDays is < 0)
                yield return new ValidationResult("A new hire's task can't be due before day one: they can't sign in yet. Use 0 or later, or give it to the manager or HR.",
                    new[] { nameof(DueOffsetDays) });
        }
    }

    #endregion
}
