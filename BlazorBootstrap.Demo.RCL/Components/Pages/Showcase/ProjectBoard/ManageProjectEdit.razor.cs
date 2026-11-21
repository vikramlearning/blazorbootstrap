using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Creates (<c>/new</c>) or edits a project (project managers) in an <see cref="EditForm" /> (rule WR-002): name (required,
/// with a counter, unique among the client's projects, checked as you type and again when the client changes), key (2 to
/// 5 capital letters, upper-cased as typed, unique, checked as you type), client (required: a client, or Internal),
/// description (with a counter), status, start and due dates (required; due on or after start, a cross-field rule), project
/// manager (required), budget in hours and in the client's currency (whole numbers in range), and the team as checkboxes,
/// with a warning in words when unticking someone who has tasks in the project (they become unassigned). Messages show
/// after a field is touched or on submit, and a failed submit shows a summary that links to each field; every
/// <see cref="PbState.SaveProject" /> result is mapped to a field, the status message, or the alert. Creating a project
/// moves to its edit route and keeps the message. An existing project also has its milestones (title, due date, tasks
/// done) with an add and edit form, also an <see cref="EditForm" /> (title required and unique in the project, due date
/// within the project's dates; <see cref="PbState.SaveMilestone" />), and Delete with a confirmation that its tasks stay
/// but no longer count toward it; a project from a won deal names the deal. An unknown ID shows <see cref="PbNotFound" />.
/// </summary>
public partial class ManageProjectEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The client select's value for an internal project.</summary>
    private const string InternalValue = "internal";

    /// <summary>The Delete buttons of the milestones, by milestone ID.</summary>
    private readonly Dictionary<int, ElementReference> msDeleteButtons = new();

    /// <summary>The Edit buttons of the milestones, by milestone ID.</summary>
    private readonly Dictionary<int, ElementReference> msEditButtons = new();

    /// <summary>The ID the router moves to after creating a project: the form and its status message stay.</summary>
    private int? createdId;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private bool focusMilestoneStatus;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (a button, or the milestone title).</summary>
    private Func<ElementReference?>? focusTarget;

    private ProjectForm form = new();

    private ShowcaseUniqueCheck? keyCheck;

    /// <summary>The route ID the form was loaded for (0 for a new project), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    /// <summary>The people ticked in the team list.</summary>
    private HashSet<string> memberIds = new();

    private EditContext msContext = default!;

    /// <summary>The milestone the delete dialog is about.</summary>
    private int msDialogId;

    private Modal msDialogModal = default!;

    /// <summary>The milestone being edited in the form, or 0 when the form adds one.</summary>
    private int msEditId;

    private MilestoneForm msForm = new();

    private ValidationMessageStore? msServerErrors;

    private bool msStatusIsError;

    private string? msStatusMessage;

    private ElementReference? msStatusRegion;

    private InputText? msTitleInput;

    private ShowcaseUniqueCheck? nameCheck;

    /// <summary>Whether Delete was confirmed in the milestone dialog; it runs once the dialog is hidden.</summary>
    private bool pendingMilestoneDelete;

    /// <summary>The state's answers that belong to a field (a taken key or name), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

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
            await PbDisplay.FocusAsync(target());
        }

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }

        if (focusMilestoneStatus)
        {
            focusMilestoneStatus = false;
            await PbDisplay.FocusAsync(msStatusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;

        // The first save of a new project moves to its edit route: the form and the status message stay as they are.
        if (createdId is not null && createdId == routeId)
        {
            createdId = null;

            return;
        }

        statusMessage = msStatusMessage = null;
        statusIsError = msStatusIsError = false;
        ClearError();
        Load(Project);
        ResetMilestoneForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
        msContext.OnFieldChanged -= OnMilestoneFieldChanged;
    }

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-project route has no ID, and the router reuses this page when moving from a
    /// project to "New project", so the previous project's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    private static string Counter(string? value, int max)
    {
        var length = (value ?? string.Empty).Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    private static string DateValue(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string StatusHelp(PbProjectStatus value) => value switch
    {
        PbProjectStatus.Planned => "Planned: not started yet. It shows on the roadmap.",
        PbProjectStatus.Active => "Active: the team is working on it.",
        PbProjectStatus.OnHold => "On hold: paused. It stays on the roadmap.",
        _ => "Completed: it leaves the roadmap, and its tasks become read-only (they leave My tasks and the workload); comments still work.",
    };

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private void AnnounceMilestone(bool ok, string message)
    {
        msStatusIsError = !ok;
        msStatusMessage = message;
    }

    private void CancelEditMilestone()
    {
        var id = msEditId;
        ResetMilestoneForm();

        if (msEditButtons.TryGetValue(id, out var button))
            focusTarget = () => button;
    }

    private void ClearError()
    {
        errorMessage = errorText = null;
        errorCount = 0;
    }

    private async Task ConfirmDeleteMilestoneAsync()
    {
        pendingMilestoneDelete = true;
        await msDialogModal.HideAsync();
    }

    private Task<bool> IsKeyTakenAsync(string key) => Task.FromResult(State.IsProjectKeyTaken(key, Project?.Id ?? 0));

    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(form.ClientValue.Length > 0 && State.IsProjectNameTaken(name, SelectedClientId, Project?.Id ?? 0));

    private string KeyTakenMessage(string key) =>
        State.Projects.FirstOrDefault(item => item.Id != (Project?.Id ?? 0) && string.Equals(item.Key, key.Trim(), StringComparison.OrdinalIgnoreCase)) is { } holder
            ? $"{key.Trim().ToUpperInvariant()} is taken by {holder.Name}. Choose another key."
            : $"{key.Trim().ToUpperInvariant()} is taken by another project. Choose another key.";

    /// <summary>Fills the form from a project, or with a new project's starting values (planned, from next Monday for 8 weeks, run by the signed-in project manager).</summary>
    private void Load(PbProject? project)
    {
        var start = PbState.WeekStart(State.Clock.Today).AddDays(7);

        ResetForm(new ProjectForm
        {
            Name = project?.Name ?? string.Empty,
            Key = project?.Key ?? string.Empty,
            ClientValue = project is null ? string.Empty : project.ClientId?.ToString(CultureInfo.InvariantCulture) ?? InternalValue,
            Description = project?.Description ?? string.Empty,
            Status = project?.Status ?? PbProjectStatus.Planned,
            StartDate = project?.StartDate ?? start,
            DueDate = project?.DueDate ?? start.AddDays(7 * 8),
            OwnerId = project?.OwnerId ?? State.CurrentPerson?.Id ?? string.Empty,
            BudgetHours = project?.BudgetHours ?? 0,
            BudgetAmount = project?.BudgetAmount ?? 0,
        });
        memberIds = project?.MemberIds.ToHashSet() ?? new HashSet<string>();
    }

    private string NameTakenMessage(string name) =>
        $"{State.FindClient(SelectedClientId)?.Name ?? "Internal work"} already has a project called “{name.Trim()}”. Choose another name, or open that project from Manage projects.";

    /// <summary>
    /// The key is upper-cased as it is typed and keeps letters only; a new client checks the name again (names are unique
    /// per client). Any change clears the last error and the state's message on that field.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);

        if (args.FieldIdentifier.FieldName == nameof(ProjectForm.Key))
        {
            var normalized = new string((form.Key ?? string.Empty).ToUpperInvariant().Where(char.IsAsciiLetterUpper).Take(5).ToArray());

            if (normalized != form.Key)
                form.Key = normalized;
        }

        if (args.FieldIdentifier.FieldName == nameof(ProjectForm.ClientValue) && !string.IsNullOrWhiteSpace(form.Name))
            editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.Name));

        if (args.FieldIdentifier.FieldName == nameof(ProjectForm.StartDate))
            editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.DueDate));

        ClearError();
    }

    private void OnMilestoneDialogHidden()
    {
        var id = msDialogId;

        if (!pendingMilestoneDelete)
        {
            if (msDeleteButtons.TryGetValue(id, out var button) && DialogMilestone is not null)
                focusTarget = () => button;

            return;
        }

        pendingMilestoneDelete = false;
        focusMilestoneStatus = true;

        if (Project is not { } project || project.Milestones.FirstOrDefault(item => item.Id == id) is not { } milestone)
        {
            AnnounceMilestone(false, "That milestone was already deleted, so nothing changed.");

            return;
        }

        var count = State.MilestoneProgress(project.Id, id).Total;
        var result = State.DeleteMilestone(project.Id, id);

        switch (result)
        {
            case PbResult.Ok:
                if (msEditId == id)
                    ResetMilestoneForm();

                AnnounceMilestone(true, $"Deleted the milestone “{milestone.Title}”." + (count == 0 ? null : $" Its {ManageProjects.Plural(count, "task")} stay on the board."));

                break;

            case PbResult.NotFound:
                AnnounceMilestone(false, "That milestone was already deleted, so nothing changed.");

                break;

            case PbResult.NotAllowed:
                AnnounceMilestone(false, "Only project managers can delete milestones. Nothing was deleted.");

                break;

            default:
                AnnounceMilestone(false, $"“{milestone.Title}” couldn't be deleted. Try again.");

                break;
        }
    }

    private void OnMilestoneFieldChanged(object? sender, FieldChangedEventArgs args) => msServerErrors?.Clear(args.FieldIdentifier);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteMilestoneAsync(int milestoneId)
    {
        msDialogId = milestoneId;
        pendingMilestoneDelete = false;
        await msDialogModal.ShowAsync();
    }

    private void ResetForm(ProjectForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>Empties the milestone form for a new milestone, due in two weeks (within the project's dates).</summary>
    private void ResetMilestoneForm()
    {
        msEditId = 0;
        SetMilestoneForm(string.Empty, Project is { } project ? Clamp(State.Clock.Today.AddDays(14), project.StartDate, project.DueDate) : State.Clock.Today.AddDays(14));
    }

    /// <summary>
    /// Saves the project once the form is valid: the uniqueness checks finish first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The state checks again; its answer maps to a field, the status message, or the alert.
    /// </summary>
    private async Task SaveAsync()
    {
        var existing = Project;

        if (Id is not null && existing is null)
            return;

        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var keyFree = keyCheck is null || await keyCheck.CheckNowAsync();
        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!editContext.Validate() || !keyFree || !nameFree)
            return;

        if (existing is null && State.Projects.Count >= PbState.MaxProjects)
        {
            ShowError($"This tab already holds {PbState.MaxProjects} projects, the most it can. Delete one before creating another. Nothing was saved.");

            return;
        }

        var key = form.Key.Trim().ToUpperInvariant();
        var team = State.People.Where(person => memberIds.Contains(person.Id)).Select(person => person.Id).ToList();
        var unassigned = existing is null ? 0 : State.TasksOf(existing.Id).Count(task => task.AssigneeId is not null && !team.Contains(task.AssigneeId));
        var input = new PbProjectInput(form.Name, key, SelectedClientId, form.Description ?? string.Empty, form.Status, form.StartDate!.Value, form.DueDate!.Value, team, form.OwnerId,
            form.BudgetHours ?? 0, form.BudgetAmount ?? 0);
        var result = State.SaveProject(existing?.Id ?? 0, input, out var saved);

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                Load(saved);

                if (existing is null)
                {
                    Announce(true, $"Created {saved.Key} · {saved.Name}. Add its milestones here, then open its board to add tasks.");

                    // The edit route renders the same page, so the form and the status message stay.
                    createdId = saved.Id;
                    ResetMilestoneForm();
                    Navigation.NavigateTo(ManageProjects.EditUrl(saved.Id), replace: true);
                    focusStatus = true;

                    return;
                }

                Announce(true, $"Saved {saved.Key} · {saved.Name}." + (unassigned == 0 ? null : $" {ManageProjects.Plural(unassigned, "task")} became unassigned."));
                focusStatus = true;

                return;

            case PbResult.Duplicate when State.IsProjectKeyTaken(key, existing?.Id ?? 0):
                ServerError(() => form.Key, KeyTakenMessage(key));

                return;

            case PbResult.Duplicate:
                ServerError(() => form.Name, NameTakenMessage(form.Name));

                return;

            case PbResult.WrongState:
                ServerError(() => form.ClientValue, $"{existing?.Key} has logged hours, which belong to its client, so its client can't change. Keep {ManageProjects.ClientName(State, existing?.ClientId)}.");

                return;

            case PbResult.NoChange:
                Announce(true, "Nothing changed: the project is already saved like this.");
                focusStatus = true;

                return;

            case PbResult.NotFound:
                ShowError("This project was deleted, so the changes couldn't be saved.");

                return;

            case PbResult.NotAllowed:
                ShowError("Only project managers can create and edit projects. Nothing was saved.");

                return;

            case PbResult.Invalid when State.ProjectManagers.All(person => person.Id != form.OwnerId):
                ServerError(() => form.OwnerId, "That person no longer runs projects. Choose another project manager.");

                return;

            case PbResult.Invalid when SelectedClientId is { } clientId && State.FindClient(clientId) is null:
                ServerError(() => form.ClientValue, "That client was deleted meanwhile. Choose another client, or Internal.");

                return;

            case PbResult.Invalid:
                ShowError(existing is null && State.Projects.Count >= PbState.MaxProjects
                    ? $"This tab already holds {PbState.MaxProjects} projects, the most it can. Nothing was saved."
                    : "The project couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");

                return;

            default:
                ShowError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    /// <summary>Saves the milestone form with <see cref="PbState.SaveMilestone" /> once it is valid; the state's answer maps to a field or the milestone status.</summary>
    private void SaveMilestone()
    {
        if (Project is not { } project)
            return;

        var editing = msEditId == 0 ? null : project.Milestones.FirstOrDefault(item => item.Id == msEditId);

        if (msEditId != 0 && editing is null)
        {
            ResetMilestoneForm();
            AnnounceMilestone(false, "That milestone was deleted, so the changes couldn't be saved.");
            focusMilestoneStatus = true;

            return;
        }

        msForm.OtherTitles = project.Milestones.Where(item => item != editing).Select(item => item.Title).ToList();
        msForm.ProjectStart = project.StartDate;
        msForm.ProjectDue = project.DueDate;
        msServerErrors?.Clear();

        if (!msContext.Validate())
            return;

        var title = msForm.Title.Trim();
        var due = msForm.Due!.Value;
        var result = State.SaveMilestone(project.Id, msEditId, title, due);

        switch (result)
        {
            case PbResult.Ok:
                AnnounceMilestone(true, editing is null ? $"Added the milestone “{title}”, due {PbDisplay.Date(due)}." : $"Saved the milestone “{title}”, due {PbDisplay.Date(due)}.");
                ResetMilestoneForm();
                focusTarget = () => msTitleInput?.Element;

                return;

            case PbResult.Duplicate:
                msServerErrors?.Add(FieldIdentifier.Create(() => msForm.Title), "That title is taken by another milestone of this project. Choose another.");
                msContext.NotifyValidationStateChanged();

                return;

            case PbResult.NoChange:
                AnnounceMilestone(true, "Nothing changed: the milestone is already saved like this.");
                focusMilestoneStatus = true;

                return;

            case PbResult.NotFound:
                ResetMilestoneForm();
                AnnounceMilestone(false, "The project or the milestone was deleted, so nothing was saved.");
                focusMilestoneStatus = true;

                return;

            case PbResult.NotAllowed:
                AnnounceMilestone(false, "Only project managers can add and edit milestones. Nothing was saved.");
                focusMilestoneStatus = true;

                return;

            case PbResult.Invalid:
                AnnounceMilestone(false, editing is null && project.Milestones.Count >= PbState.MaxMilestones
                    ? $"A project can have up to {PbState.MaxMilestones} milestones. Delete one to add another. Nothing was saved."
                    : "The milestone couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");
                focusMilestoneStatus = true;

                return;

            default:
                AnnounceMilestone(false, "That didn't work, and nothing was saved. Try again.");
                focusMilestoneStatus = true;

                return;
        }
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void SetMilestoneForm(string title, DateTime due)
    {
        if (msContext is not null)
            msContext.OnFieldChanged -= OnMilestoneFieldChanged;

        msForm = new MilestoneForm { Title = title, Due = due };
        msContext = new EditContext(msForm);
        msServerErrors = new ValidationMessageStore(msContext);
        msContext.OnFieldChanged += OnMilestoneFieldChanged;
    }

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    private void StartEditMilestone(int milestoneId)
    {
        if (Project?.Milestones.FirstOrDefault(item => item.Id == milestoneId) is not { } milestone)
            return;

        msEditId = milestone.Id;
        SetMilestoneForm(milestone.Title, milestone.Due);
        focusTarget = () => msTitleInput?.Element;
    }

    /// <summary>Returns, by person ID, a warning for each saved member who is unticked and has tasks in the project.</summary>
    private Dictionary<string, string> TeamWarnings(PbProject? project)
    {
        var warnings = new Dictionary<string, string>();

        if (project is null)
            return warnings;

        var tasks = State.TasksOf(project.Id);

        foreach (var personId in project.MemberIds.Where(id => !memberIds.Contains(id)))
        {
            var count = tasks.Count(task => task.AssigneeId == personId);

            if (count > 0 && State.FindPerson(personId) is { } person)
                warnings[personId] = $"{person.Name} has {ManageProjects.Plural(count, "task")} here; {(count == 1 ? "it" : "they")}'ll become unassigned when you save.";
        }

        return warnings;
    }

    private void ToggleMember(string personId, bool on)
    {
        if (on)
            memberIds.Add(personId);
        else
            memberIds.Remove(personId);
    }

    private static DateTime Clamp(DateTime value, DateTime min, DateTime max) => value < min ? min : value > max ? max : value;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the project ID from the route, or <see langword="null" /> on the new-project route.</summary>
    [Parameter] public int? Id { get; set; }

    private PbMilestone? DialogMilestone => Project?.Milestones.FirstOrDefault(item => item.Id == msDialogId);

    private string KeyPreview => string.IsNullOrEmpty(form.Key) ? "KEY" : form.Key;

    /// <summary>The latest date the demo keeps (three years after its start).</summary>
    private static DateTime MaxDate => PbSeedData.Now.AddYears(3).Date;

    /// <summary>The earliest date the demo keeps (three years before its start).</summary>
    private static DateTime MinDate => PbSeedData.Now.AddYears(-3).Date.AddDays(1);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private PbProject? Project => Id is { } id ? State.FindProject(id) : null;

    /// <summary>The chosen client's ID, or <see langword="null" /> for internal work (or nothing chosen yet).</summary>
    private int? SelectedClientId => int.TryParse(form.ClientValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : null;

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A milestone's fields: a title (required, unique in the project) and a due date within the project's dates.</summary>
    private sealed class MilestoneForm : IValidatableObject
    {
        [Required(ErrorMessage = "Pick the milestone's due date.")]
        public DateTime? Due { get; set; }

        /// <summary>The project's other milestone titles, set by the page before it validates.</summary>
        public IReadOnlyList<string> OtherTitles { get; set; } = Array.Empty<string>();

        /// <summary>The project's due date, set by the page.</summary>
        public DateTime ProjectDue { get; set; } = DateTime.MaxValue;

        /// <summary>The project's start date, set by the page.</summary>
        public DateTime ProjectStart { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "Give the milestone a title.")]
        [StringLength(PbState.MaxMilestoneTitleLength, ErrorMessage = "Use 60 characters or fewer for the milestone's title.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: a title no other milestone of the project has, and a due date within the project's dates.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OtherTitles.FirstOrDefault(other => string.Equals(other, Title.Trim(), StringComparison.OrdinalIgnoreCase)) is { } same)
                yield return new ValidationResult($"This project already has a milestone called “{same}”. Choose another title.", new[] { nameof(Title) });

            if (Due is { } due && (due.Date < ProjectStart.Date || due.Date > ProjectDue.Date))
            {
                yield return new ValidationResult($"Choose a due date within the project's dates, {PbDisplay.Date(ProjectStart)} to {PbDisplay.Date(ProjectDue)}.", new[] { nameof(Due) });
            }
        }
    }

    /// <summary>
    /// The project's fields, with the limits of <see cref="PbState.SaveProject" />: required text with lengths, a key's form,
    /// a client choice, required dates in order (a cross-field rule), a project manager, and budgets in range.
    /// </summary>
    private sealed class ProjectForm : IValidatableObject
    {
        [Range(0, PbState.MaxBudgetAmount, ErrorMessage = "Use a budget from 0 to 5,000,000, in whole units of the client's currency.")]
        public int? BudgetAmount { get; set; }

        [Range(0, PbState.MaxBudgetHours, ErrorMessage = "Use a budget from 0 to 10,000 hours.")]
        public int? BudgetHours { get; set; }

        /// <summary>The client's ID, <see cref="InternalValue" /> for internal work, or empty while nothing is chosen.</summary>
        [Required(ErrorMessage = "Choose the client the project is for, or Internal for the agency's own work.")]
        public string ClientValue { get; set; } = string.Empty;

        [StringLength(PbState.MaxDescriptionLength, ErrorMessage = "Use 2,000 characters or fewer for the description.")]
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pick the due date.")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Enter a key of 2 to 5 capital letters, like WEB.")]
        [RegularExpression("^[A-Z]{2,5}$", ErrorMessage = "Use 2 to 5 capital letters A to Z only, like WEB.")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Give the project a name.")]
        [StringLength(PbState.MaxProjectNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose the project manager who runs it.")]
        public string OwnerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pick the start date.")]
        public DateTime? StartDate { get; set; }

        public PbProjectStatus Status { get; set; } = PbProjectStatus.Planned;

        /// <summary>Runs once the fields are filled in: the dates are in the range the demo keeps, and the due date is on or after the start.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate is { } start && (start < MinDate || start > MaxDate))
                yield return new ValidationResult($"Pick a start date from {PbDisplay.Date(MinDate)} to {PbDisplay.Date(MaxDate)}.", new[] { nameof(StartDate) });

            if (DueDate is { } due && (due < MinDate || due > MaxDate))
                yield return new ValidationResult($"Pick a due date from {PbDisplay.Date(MinDate)} to {PbDisplay.Date(MaxDate)}.", new[] { nameof(DueDate) });
            else if (StartDate is { } from && DueDate is { } to && to.Date < from.Date)
                yield return new ValidationResult($"The due date can't be before the start date ({PbDisplay.Date(from)}).", new[] { nameof(DueDate) });

            if (BudgetAmount is > 0 && BudgetHours is null or 0)
                yield return new ValidationResult("A budget in money needs a budget in hours too, so the team can see how much time it buys.", new[] { nameof(BudgetHours) });
        }
    }

    #endregion
}
