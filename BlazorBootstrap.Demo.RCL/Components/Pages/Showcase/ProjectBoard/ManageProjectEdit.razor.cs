using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Creates (<c>/new</c>) or edits a project (project managers): name (with a counter), key (2 to 5 capital letters,
/// upper-cased as typed; a taken key names the project that has it), client (or internal), description (with a counter),
/// status, start and due dates (native date inputs; due on or after start), and the team as checkboxes, with a warning in
/// words when unticking someone who has tasks in the project (they become unassigned). Each problem is shown under its
/// field and tied to it, and the first invalid field takes focus after the render that shows it; every
/// <see cref="PbState.SaveProject" /> result is mapped to a field or the status message. Creating a project moves to its
/// edit route and keeps the message. An existing project also has its milestones (title, due date, tasks done) with an
/// inline add and edit form (<see cref="PbState.SaveMilestone" />) and Delete with a confirmation that its tasks stay but no
/// longer count toward it; a project from a won deal names the deal. An unknown ID shows <see cref="PbNotFound" />.
/// </summary>
public partial class ManageProjectEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after creating a project: the form and its status message stay.</summary>
    private int? createdId;

    private string clientValue = string.Empty;

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference? descriptionInput;

    private string? dueError;

    private ElementReference? dueInput;

    private string dueText = string.Empty;

    private bool focusMilestoneStatus;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (the first field with an error, or a button).</summary>
    private Func<ElementReference?>? focusTarget;

    private string key = string.Empty;

    private string? keyError;

    private ElementReference? keyInput;

    /// <summary>The route ID the form was loaded for (0 for a new project), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    /// <summary>The people ticked in the team list.</summary>
    private HashSet<string> memberIds = new();

    /// <summary>The Delete buttons of the milestones, by milestone ID.</summary>
    private readonly Dictionary<int, ElementReference> msDeleteButtons = new();

    /// <summary>The milestone the delete dialog is about.</summary>
    private int msDialogId;

    private Modal msDialogModal = default!;

    private string? msDueError;

    private ElementReference? msDueInput;

    private string msDueText = string.Empty;

    /// <summary>The Edit buttons of the milestones, by milestone ID.</summary>
    private readonly Dictionary<int, ElementReference> msEditButtons = new();

    /// <summary>The milestone being edited in the form, or 0 when the form adds one.</summary>
    private int msEditId;

    private bool msStatusIsError;

    private string? msStatusMessage;

    private ElementReference? msStatusRegion;

    private string msTitle = string.Empty;

    private string? msTitleError;

    private ElementReference? msTitleInput;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    /// <summary>Whether Delete was confirmed in the milestone dialog; it runs once the dialog is hidden.</summary>
    private bool pendingMilestoneDelete;

    private string? startError;

    private ElementReference? startInput;

    private string startText = string.Empty;

    private PbProjectStatus status = PbProjectStatus.Planned;

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
        Load(Project);
        ResetMilestoneForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

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

    private static string Counter(string value, int max)
    {
        var length = value.Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    private static string DateValue(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>Checks a date field: empty, not a date, or outside the range the demo keeps.</summary>
    private static string? DateProblem(string text, string what, out DateTime value)
    {
        if (TryDate(text) is not { } date)
        {
            value = default;

            return string.IsNullOrWhiteSpace(text) ? $"Pick the {what}." : $"Enter the {what} as a date, like {PbDisplay.Date(PbSeedData.Now)}.";
        }

        value = date;

        return date < MinDate || date > MaxDate ? $"Pick a {what} from {PbDisplay.Date(MinDate)} to {PbDisplay.Date(MaxDate)}." : null;
    }

    private static string StatusHelp(PbProjectStatus value) => value switch
    {
        PbProjectStatus.Planned => "Planned: not started yet. It shows on the roadmap.",
        PbProjectStatus.Active => "Active: the team is working on it.",
        PbProjectStatus.OnHold => "On hold: paused. It stays on the roadmap.",
        _ => "Completed: it leaves the roadmap, and its tasks become read-only (they leave My tasks and the workload); comments still work.",
    };

    private static string? TextProblem(string value, int max, string missing, string what)
    {
        var length = value.Trim().Length;

        return length == 0 ? missing
            : length > max ? string.Create(CultureInfo.InvariantCulture, $"Shorten the {what} to {max} characters or fewer (now {length}).")
            : null;
    }

    private static DateTime? TryDate(string text) =>
        DateTime.TryParseExact(text?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

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

    private async Task ConfirmDeleteMilestoneAsync()
    {
        pendingMilestoneDelete = true;
        await msDialogModal.HideAsync();
    }

    /// <summary>Fills the form from a project, or with a new project's starting values (planned, from next Monday for 8 weeks).</summary>
    private void Load(PbProject? project)
    {
        var start = PbState.WeekStart(State.Clock.Today).AddDays(7);

        name = project?.Name ?? string.Empty;
        key = project?.Key ?? string.Empty;
        clientValue = project?.ClientId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        description = project?.Description ?? string.Empty;
        status = project?.Status ?? PbProjectStatus.Planned;
        startText = DateValue(project?.StartDate ?? start);
        dueText = DateValue(project?.DueDate ?? start.AddDays(7 * 8));
        memberIds = project?.MemberIds.ToHashSet() ?? new HashSet<string>();
        nameError = keyError = descriptionError = startError = dueError = null;
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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteMilestoneAsync(int milestoneId)
    {
        msDialogId = milestoneId;
        pendingMilestoneDelete = false;
        await msDialogModal.ShowAsync();
    }

    private void ResetMilestoneForm()
    {
        msEditId = 0;
        msTitle = string.Empty;
        msDueText = DateValue(State.Clock.Today.AddDays(14));
        msTitleError = msDueError = null;
    }

    /// <summary>Checks the form, saves the project, and maps the state's answer to a field or the status message.</summary>
    private void Save()
    {
        var existing = Project;

        if (Id is not null && existing is null)
            return;

        var trimmedName = name.Trim();
        var trimmedKey = key.Trim().ToUpperInvariant();

        nameError = TextProblem(name, PbState.MaxProjectNameLength, "Give the project a name.", "name");
        keyError = trimmedKey.Length == 0 ? "Enter a key of 2 to 5 capital letters, like WEB."
            : trimmedKey.Length is < 2 or > 5 || trimmedKey.Any(letter => letter is < 'A' or > 'Z') ? "Use 2 to 5 capital letters A to Z only, like WEB."
            : State.Projects.FirstOrDefault(item => item.Id != existing?.Id && item.Key == trimmedKey) is { } taken ? $"That key is taken by {taken.Name}. Choose another."
            : null;
        descriptionError = description.Trim().Length > PbState.MaxDescriptionLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the description to {PbState.MaxDescriptionLength} characters or fewer (now {description.Trim().Length}).")
            : null;
        startError = DateProblem(startText, "start date", out var start);
        dueError = DateProblem(dueText, "due date", out var due);

        if (startError is null && dueError is null && due < start)
            dueError = $"The due date can't be before the start date ({PbDisplay.Date(start)}).";

        focusTarget = nameError is not null ? () => nameInput
            : keyError is not null ? () => keyInput
            : descriptionError is not null ? () => descriptionInput
            : startError is not null ? () => startInput
            : dueError is not null ? () => dueInput
            : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        if (existing is null && State.Projects.Count >= PbState.MaxProjects)
        {
            Announce(false, $"This tab already holds {PbState.MaxProjects} projects, the most it can. Delete one before creating another. Nothing was saved.");
            focusStatus = true;

            return;
        }

        int? clientId = int.TryParse(clientValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        var team = State.People.Where(person => memberIds.Contains(person.Id)).Select(person => person.Id).ToList();
        var unassigned = existing is null ? 0 : State.TasksOf(existing.Id).Count(task => task.AssigneeId is not null && !team.Contains(task.AssigneeId));
        var input = new PbProjectInput(trimmedName, trimmedKey, clientId, description, status, start, due, team);
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
                    Navigation.NavigateTo(ManageProjects.EditUrl(saved.Id), replace: true);

                    return;
                }

                Announce(true, $"Saved {saved.Key} · {saved.Name}." + (unassigned == 0 ? null : $" {ManageProjects.Plural(unassigned, "task")} became unassigned."));

                return;

            case PbResult.Duplicate:
                keyError = State.Projects.FirstOrDefault(item => item.Id != existing?.Id && item.Key == trimmedKey) is { } holder
                    ? $"That key is taken by {holder.Name}. Choose another."
                    : "That key is taken by another project. Choose another.";
                focusTarget = () => keyInput;
                Announce(false, "That key is taken. Nothing was saved.");

                return;

            case PbResult.NoChange:
                Announce(true, "Nothing changed: the project is already saved like this.");

                return;

            case PbResult.NotFound:
                Announce(false, "This project was deleted, so the changes couldn't be saved.");
                focusStatus = true;

                return;

            case PbResult.NotAllowed:
                Announce(false, "Only project managers can create and edit projects. Nothing was saved.");
                focusStatus = true;

                return;

            case PbResult.Invalid:
                Announce(false, existing is null && State.Projects.Count >= PbState.MaxProjects
                    ? $"This tab already holds {PbState.MaxProjects} projects, the most it can. Nothing was saved."
                    : "The project couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    /// <summary>Checks the milestone form and saves it with <see cref="PbState.SaveMilestone" />.</summary>
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

        var title = msTitle.Trim();
        msTitleError = TextProblem(msTitle, PbState.MaxMilestoneTitleLength, "Give the milestone a title.", "title")
            ?? (project.Milestones.FirstOrDefault(item => item != editing && string.Equals(item.Title, title, StringComparison.OrdinalIgnoreCase)) is { } same
                ? $"This project already has a milestone called “{same.Title}”. Choose another title."
                : null);
        msDueError = DateProblem(msDueText, "due date", out var due);

        focusTarget = msTitleError is not null ? () => msTitleInput : msDueError is not null ? () => msDueInput : null;

        if (focusTarget is not null)
        {
            AnnounceMilestone(false, "Fix the highlighted fields, then save the milestone again.");

            return;
        }

        var result = State.SaveMilestone(project.Id, msEditId, title, due);

        switch (result)
        {
            case PbResult.Ok:
                AnnounceMilestone(true, editing is null ? $"Added the milestone “{title}”, due {PbDisplay.Date(due)}." : $"Saved the milestone “{title}”, due {PbDisplay.Date(due)}.");
                ResetMilestoneForm();
                focusTarget = () => msTitleInput;

                return;

            case PbResult.Duplicate:
                msTitleError = "That title is taken by another milestone of this project. Choose another.";
                focusTarget = () => msTitleInput;
                AnnounceMilestone(false, "That title is taken. Nothing was saved.");

                return;

            case PbResult.NoChange:
                AnnounceMilestone(true, "Nothing changed: the milestone is already saved like this.");

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

    /// <summary>Upper-cases the key as it is typed and keeps letters only.</summary>
    private void SetKey(string? value) => key = new string((value ?? string.Empty).ToUpperInvariant().Where(char.IsAsciiLetterUpper).Take(5).ToArray());

    private void StartEditMilestone(int milestoneId)
    {
        if (Project?.Milestones.FirstOrDefault(item => item.Id == milestoneId) is not { } milestone)
            return;

        msEditId = milestone.Id;
        msTitle = milestone.Title;
        msDueText = DateValue(milestone.Due);
        msTitleError = msDueError = null;
        focusTarget = () => msTitleInput;
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

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the project ID from the route, or <see langword="null" /> on the new-project route.</summary>
    [Parameter] public int? Id { get; set; }

    private PbMilestone? DialogMilestone => Project?.Milestones.FirstOrDefault(item => item.Id == msDialogId);

    private string KeyPreview => key.Length == 0 ? "KEY" : key;

    /// <summary>The latest date the demo keeps (three years after its start).</summary>
    private static DateTime MaxDate => PbSeedData.Now.AddYears(3).Date;

    /// <summary>The earliest date the demo keeps (three years before its start).</summary>
    private static DateTime MinDate => PbSeedData.Now.AddYears(-3).Date.AddDays(1);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private PbProject? Project => Id is { } id ? State.FindProject(id) : null;

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
