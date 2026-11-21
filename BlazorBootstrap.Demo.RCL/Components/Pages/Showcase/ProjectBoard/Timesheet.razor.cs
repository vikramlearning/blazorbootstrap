using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The signed-in person's timesheet (everyone, AC-7f): one week at a time (<c>?week=2026-11-16</c>; this week by default),
/// with Previous week, This week, and Next week. The week's status (Draft, Submitted, Returned with the reason, Approved) and
/// each project's approval, the hours by project and day with totals (a plain table: an aggregate), and the entries in a
/// <see cref="Grid{TItem}" /> (day, project, task, hours, billable, note, invoice) with a saved view, the column chooser,
/// and CSV export. While the week is a draft or returned, "Log time" and the row (a stretched button) open a dialog with an
/// <see cref="EditForm" /> (rule WR-002): project (required, one the person is on that has started), task (one of the
/// project's), day (required, in the week, not in the future, not before the project's start), hours (0.25 to 12 in quarter
/// hours, at most 24 that day), billable (never on an internal project), and a note; and "Submit week" sends it to the
/// projects' managers (<see cref="PbState.SubmitWeek" />). "Your weeks" lists every week in a second Grid; a row opens it.
/// </summary>
public partial class Timesheet : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> editButtons = new();

    private int deleteId;

    private Modal deleteModal = default!;

    private EditContext editContext = default!;

    /// <summary>The entry the form edits, or 0 for a new one.</summary>
    private int editId;

    private Modal editorModal = default!;

    private Grid<EntryRow>? entryGrid;

    private int entryPageSize = 25;

    private ShowcaseGridView entryView = default!;

    private Func<ElementReference?>? focusTarget;

    private EntryForm form = new();

    private int formErrorCount;

    private string? formMessage;

    private string? formText;

    /// <summary>Changes when the views are reset, so the grids are built again with their defaults.</summary>
    private int gridVersion;

    private ElementReference? logButton;

    private bool pendingDelete;

    private InputSelect<int>? projectInput;

    private string? savedMessage;

    private ValidationMessageStore? serverErrors;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private bool viewLoaded;

    private Grid<WeekRow>? weekGrid;

    private int weekPageSize = 10;

    /// <summary>The Monday of the week shown.</summary>
    private DateTime weekStart;

    private ShowcaseGridView weekView = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await entryView.LoadAsync();
            await weekView.LoadAsync();
            entryPageSize = (await entryView.GetSettingsAsync()).PageSize;
            weekPageSize = (await weekView.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await PbDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        entryView = GridViews.Get(State.SiteSlug, "timesheet-entries", 25, "day", SortDirection.Ascending);
        weekView = GridViews.Get(State.SiteSlug, "timesheet-weeks", 10, "week", SortDirection.Descending);
        ResetForm(new EntryForm());
        State.Changed += OnStateChanged;
    }

    /// <summary>The week follows <c>?week=</c> (any day of it); this week when it's missing or not a date.</summary>
    protected override async Task OnParametersSetAsync()
    {
        var requested = DealDisplay.TryParseDateInput(WeekQuery, out var date) && date.Year is > 2000 and < 2100 ? PbState.WeekStart(date) : PbState.WeekStart(State.Clock.Today);

        if (requested == weekStart)
            return;

        weekStart = requested;
        statusMessage = statusText = null;
        editButtons.Clear();
        await RefreshGridsAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    /// <summary>Formats hours like "7.5 h", or "7.5" without the unit.</summary>
    private static string Hours(decimal hours, bool unit = true) => hours.ToString("0.##", CultureInfo.InvariantCulture) + (unit ? " h" : null);

    private static string StatusColor(PbTimesheetStatus? status) => status switch
    {
        PbTimesheetStatus.Submitted => "info",
        PbTimesheetStatus.Returned => "warning",
        PbTimesheetStatus.Approved => "success",
        _ => "secondary",
    };

    private static string StatusText(PbTimesheetStatus? status) => status switch
    {
        PbTimesheetStatus.Submitted => "Submitted",
        PbTimesheetStatus.Returned => "Returned",
        PbTimesheetStatus.Approved => "Approved",
        PbTimesheetStatus.Draft => "Draft",
        _ => "Not started",
    };

    private static string WeekUrl(DateTime week) => $"{PbLayout.Base}/timesheet?week={DealDisplay.ToDateInput(week)}";

    private string ApproverText(PbProject? project)
    {
        if (project is null)
            return "its project manager";

        var owner = State.FindPerson(project.OwnerId);

        return owner?.AccountId is not null && State.Identity.FindAccount(owner.AccountId) is not null ? owner.Name : $"a project manager ({owner?.Name ?? "its manager"} can't sign in)";
    }

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await deleteModal.HideAsync();
    }

    private IReadOnlyList<EntryRow> EntryRows()
    {
        if (State.CurrentPerson is not { } person)
            return Array.Empty<EntryRow>();

        return State.EntriesOf(person.Id, weekStart).Select(entry =>
        {
            var project = State.FindProject(entry.ProjectId);
            var task = entry.TaskId is { } taskId ? State.FindTask(taskId) : null;
            var invoice = entry.InvoiceId is { } invoiceId ? State.FindInvoice(invoiceId) : null;

            return new EntryRow(entry.Id, entry.Date, entry.Date.ToString("ddd, MMM d", CultureInfo.InvariantCulture), project is null ? "Deleted project" : $"{project.Key} · {project.Name}",
                task is null ? string.Empty : $"{State.TaskKey(task)} {task.Title}", entry.Hours, entry.IsBillable ? "Billable" : "Not billable", entry.Note,
                entry.InvoiceId is null ? string.Empty : invoice?.Number ?? "Draft invoice");
        }).ToList();
    }

    private void OnDeleteHidden()
    {
        if (!pendingDelete)
        {
            var id = deleteId;
            focusTarget = () => editButtons.TryGetValue(id, out var button) ? button : statusRegion;
            StateHasChanged();

            return;
        }

        pendingDelete = false;
        var result = State.DeleteTimeEntry(deleteId);
        SetStatus(result switch
        {
            PbResult.Ok => "The entry is deleted.",
            PbResult.NotFound => "That entry was already deleted.",
            PbResult.WrongState => "This week is submitted or approved, or the hours are invoiced, so they can't change. Nothing was deleted.",
            PbResult.NotAllowed => "Your sign-in ended. Sign in again to change your timesheet. Nothing was deleted.",
            _ => "That didn't work, and nothing was deleted. Try again.",
        }, result != PbResult.Ok);
        editButtons.Remove(deleteId);
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    private void OnEditorHidden()
    {
        if (savedMessage is { } message)
        {
            savedMessage = null;
            SetStatus(message, false);
            focusTarget = () => statusRegion;
        }
        else
        {
            var id = editId;
            focusTarget = () => id != 0 && editButtons.TryGetValue(id, out var button) ? button : logButton ?? statusRegion;
        }

        StateHasChanged();
    }

    private async Task OnEditorShownAsync() => await PbDisplay.FocusAsync(projectInput?.Element);

    /// <summary>
    /// A new project sets billable to what the project does by default and drops a task of another project; any change clears
    /// the dialog's alert and the state's message on that field.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        formMessage = formText = null;
        formErrorCount = 0;

        if (args.FieldIdentifier.FieldName != nameof(EntryForm.ProjectId))
            return;

        var project = State.FindProject(form.ProjectId);
        form.IsBillable = project?.ClientId is not null;

        if (form.TaskId is { } taskId && State.FindTask(taskId)?.ProjectId != form.ProjectId)
            form.TaskId = null;
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridsAsync();
        StateHasChanged();
    });

    private async Task OpenDeleteAsync(int entryId)
    {
        deleteId = entryId;
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    /// <summary>Opens the dialog for a new entry (today when it's in this week, otherwise the week's Monday) or for an entry's fields.</summary>
    private async Task OpenEditorAsync(int entryId)
    {
        var entry = entryId == 0 ? null : State.FindTimeEntry(entryId);
        editId = entry?.Id ?? 0;
        var today = State.Clock.Today;
        var defaultDay = today >= weekStart && today < weekStart.AddDays(7) ? today : weekStart;
        var lastProject = State.CurrentPerson is { } me ? State.EntriesOf(me.Id, weekStart).LastOrDefault()?.ProjectId ?? 0 : 0;
        var projectId = entry?.ProjectId ?? lastProject;

        ResetForm(new EntryForm
        {
            ProjectId = projectId, TaskId = entry?.TaskId, Date = entry?.Date ?? defaultDay, Hours = entry?.Hours, IsBillable = entry?.IsBillable ?? State.FindProject(projectId)?.ClientId is not null,
            Note = entry?.Note ?? string.Empty,
        });
        formMessage = formText = savedMessage = null;
        formErrorCount = 0;
        await editorModal.ShowAsync();
    }

    private Task<GridDataProviderResult<EntryRow>> ProvideEntriesAsync(GridDataProviderRequest<EntryRow> request) => entryView.ApplyAsync(request, EntryRows());

    private Task<GridDataProviderResult<WeekRow>> ProvideWeeksAsync(GridDataProviderRequest<WeekRow> request)
    {
        var rows = new List<WeekRow>();

        if (State.CurrentPerson is { } person)
        {
            foreach (var sheet in State.MyTimesheets)
            {
                var entries = State.EntriesOf(person.Id, sheet.WeekStart);
                rows.Add(new WeekRow(sheet.WeekStart, $"{PbDisplay.Date(sheet.WeekStart)} to {sheet.WeekStart.AddDays(6).ToString("MMM d", CultureInfo.InvariantCulture)}", StatusText(sheet.Status), sheet.Status,
                    entries.Sum(entry => entry.Hours), entries.Where(entry => entry.IsBillable).Sum(entry => entry.Hours),
                    string.Join(", ", entries.Select(entry => State.FindProject(entry.ProjectId)?.Key).Distinct())));
            }
        }

        return weekView.ApplyAsync(request, rows);
    }

    private async Task RefreshGridsAsync()
    {
        if (entryGrid is not null)
            await entryGrid.RefreshDataAsync();

        if (weekGrid is not null)
            await weekGrid.RefreshDataAsync();
    }

    private void ResetForm(EntryForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>Forgets both saved views and builds the grids again with their defaults.</summary>
    private async Task ResetViewsAsync()
    {
        await entryView.ResetAsync();
        await weekView.ResetAsync();
        entryPageSize = 25;
        weekPageSize = 10;
        gridVersion++;
        SetStatus("The views are reset: entries by day, 25 a page; weeks newest first, 10 a page; every column shown.", false);
        focusTarget = () => statusRegion;
    }

    /// <summary>
    /// Saves the entry once the form is valid: the page gives the form what the cross-field rules need (the week, today, the
    /// project's dates, the other hours that day), every rule runs, and a failed submit shows the summary (rule WR-002). The
    /// state checks again; its refusal maps to a field or the dialog's alert.
    /// </summary>
    private async Task SaveAsync()
    {
        if (State.CurrentPerson is not { } person)
            return;

        var project = State.FindProject(form.ProjectId);
        form.WeekStart = weekStart;
        form.Today = State.Clock.Today;
        form.ProjectStart = project?.StartDate;
        form.ProjectEnd = project?.Status == PbProjectStatus.Completed ? project.DueDate : null;
        form.IsInternal = project is not null && project.ClientId is null;
        form.CanLog = project is not null && project.Status != PbProjectStatus.Planned && (project.MemberIds.Contains(person.Id) || project.OwnerId == person.Id);
        form.TaskOk = form.TaskId is not { } taskId || State.FindTask(taskId)?.ProjectId == form.ProjectId;
        form.OtherHours = form.Date is { } day
            ? State.EntriesOf(person.Id, day).Where(entry => entry.Date == day.Date && entry.Id != editId).Sum(entry => entry.Hours)
            : 0m;
        serverErrors?.Clear();

        if (!editContext.Validate())
            return;

        var input = new PbTimeEntryInput(form.ProjectId, form.TaskId, form.Date!.Value, form.Hours!.Value, form.IsBillable && !form.IsInternal, form.Note ?? string.Empty);
        var result = State.SaveTimeEntry(editId, input, out var saved);

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                savedMessage = $"{(editId == 0 ? "Logged" : "Saved")} {Hours(saved.Hours)} on {State.FindProject(saved.ProjectId)?.Key}, {PbDisplay.Date(saved.Date)}.";
                await editorModal.HideAsync();

                return;

            case PbResult.NoChange:
                savedMessage = "Nothing changed in that entry.";
                await editorModal.HideAsync();

                return;

            case PbResult.WrongState:
                ShowFormError("This week was submitted meanwhile, or the hours are invoiced, so they can't change. Nothing was saved.");

                return;

            case PbResult.NotFound:
                ShowFormError("That project or entry was deleted meanwhile. Nothing was saved.");

                return;

            case PbResult.NotAllowed:
                ShowFormError("Your sign-in ended. Sign in again to change your timesheet. Nothing was saved.");

                return;

            default:
                ShowFormError("Those hours couldn't be saved: the project may have changed, or the storage for this demo tab is full. Nothing was saved.");

                return;
        }
    }

    private async Task SaveEntrySettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            entryPageSize = settings.PageSize;

        await entryView.SaveSettingsAsync(settings);
    }

    private async Task SaveWeekSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            weekPageSize = settings.PageSize;

        await weekView.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private void ShowFormError(string message)
    {
        formErrorCount = message == formText ? formErrorCount + 1 : 1;
        formText = message;
        formMessage = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
    }

    private string StatusHelp(PbTimesheet? sheet, int entries) => sheet?.Status switch
    {
        PbTimesheetStatus.Submitted => $"Submitted {PbDisplay.Date(sheet.SubmittedAt!.Value)}. Each project's manager approves its hours; until then the week can't change.",
        PbTimesheetStatus.Approved => $"Approved {PbDisplay.Date(sheet.DecidedAt!.Value)}. Billable hours go on the client's next invoice.",
        PbTimesheetStatus.Returned => "Change what the project manager asked for, then submit the week again.",
        PbTimesheetStatus.Draft => entries == 0 ? "No hours yet." : "Not submitted yet. Submit it once the week's hours are in.",
        _ => "Log time to start this week's timesheet.",
    };

    /// <summary>Submits the week; the outcome is announced (the buttons change, so focus moves to the message).</summary>
    private void Submit()
    {
        var result = State.SubmitWeek(weekStart);
        SetStatus(result switch
        {
            PbResult.Ok => $"Submitted the week of {PbDisplay.Date(weekStart)}. The project managers are told; you'll hear back when it's approved or returned.",
            PbResult.NotFound => "Log time first: a week needs at least one entry to submit.",
            PbResult.WrongState => "This week is already submitted or approved.",
            PbResult.Invalid => "This week hasn't started yet, so it can't be submitted.",
            PbResult.NotAllowed => "Your sign-in ended. Sign in again to submit your week.",
            _ => "That didn't work, and nothing was submitted. Try again.",
        }, result != PbResult.Ok);
        focusTarget = () => statusRegion;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private PbState State { get; set; } = default!;

    /// <summary>Gets or sets the week to show, as any day of it (yyyy-MM-dd), like <c>?week=2026-11-16</c>.</summary>
    [SupplyParameterFromQuery(Name = "week")] public string? WeekQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>
    /// A time entry's fields, with the limits of <see cref="PbState.SaveTimeEntry" />: a project and a day (required), hours
    /// in quarter hours from 0.25 to 12, a note, and cross-field rules against values the page sets before it validates: the
    /// day is in the week, not in the future, and within the project's dates; the day's hours stay at 24 or fewer; an internal
    /// project isn't billable; the task is the project's.
    /// </summary>
    private sealed class EntryForm : IValidatableObject
    {
        /// <summary>Whether the person can log hours on the project, set by the page.</summary>
        public bool CanLog { get; set; } = true;

        [Required(ErrorMessage = "Pick the day you worked.")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Enter the hours, like 1.5.")]
        [Range(typeof(decimal), "0.25", "12", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "Enter from 0.25 to 12 hours for one entry.")]
        public decimal? Hours { get; set; }

        public bool IsBillable { get; set; } = true;

        /// <summary>Whether the project is internal (no client), set by the page.</summary>
        public bool IsInternal { get; set; }

        [StringLength(PbState.MaxEntryNoteLength, ErrorMessage = "Use 200 characters or fewer for the note.")]
        public string? Note { get; set; } = string.Empty;

        /// <summary>The person's other hours that day, set by the page.</summary>
        public decimal OtherHours { get; set; }

        /// <summary>A completed project's due date (no hours after it), or <see langword="null" />; set by the page.</summary>
        public DateTime? ProjectEnd { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Choose the project you worked on.")]
        public int ProjectId { get; set; }

        /// <summary>The project's start date, set by the page.</summary>
        public DateTime? ProjectStart { get; set; }

        public int? TaskId { get; set; }

        /// <summary>Whether the task is the project's, set by the page.</summary>
        public bool TaskOk { get; set; } = true;

        /// <summary>The board's today, set by the page.</summary>
        public DateTime Today { get; set; } = DateTime.MaxValue;

        /// <summary>The Monday of the week, set by the page.</summary>
        public DateTime WeekStart { get; set; } = DateTime.MinValue;

        /// <summary>Runs once the fields are filled in: the cross-field rules.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProjectId != 0 && !CanLog)
                yield return new ValidationResult("You can't log hours on that project: you aren't on its team, or it hasn't started. Choose another.", new[] { nameof(ProjectId) });

            if (!TaskOk)
                yield return new ValidationResult("That task isn't on the chosen project. Choose one of its tasks, or No task.", new[] { nameof(TaskId) });

            if (Date is { } day)
            {
                if (day.Date < WeekStart.Date || day.Date > WeekStart.Date.AddDays(6))
                    yield return new ValidationResult($"Pick a day in this week, {PbDisplay.Date(WeekStart)} to {PbDisplay.Date(WeekStart.AddDays(6))}.", new[] { nameof(Date) });
                else if (day.Date > Today.Date)
                    yield return new ValidationResult("Pick today or an earlier day: hours are logged once they're worked.", new[] { nameof(Date) });
                else if (ProjectStart is { } start && day.Date < start.Date)
                    yield return new ValidationResult($"The project starts on {PbDisplay.Date(start)}. Pick that day or later.", new[] { nameof(Date) });
                else if (ProjectEnd is { } end && day.Date > end.Date)
                    yield return new ValidationResult($"The project was completed on {PbDisplay.Date(end)}. Pick that day or earlier.", new[] { nameof(Date) });
            }

            if (Hours is { } hours && hours is >= PbState.MinEntryHours and <= PbState.MaxEntryHours)
            {
                if (!PbState.IsValidHours(hours))
                    yield return new ValidationResult("Use quarter hours, like 1.25, 1.5, or 1.75.", new[] { nameof(Hours) });
                else if (OtherHours + hours > PbState.MaxHoursPerDay)
                    yield return new ValidationResult(string.Create(CultureInfo.InvariantCulture, $"That day already has {OtherHours:0.##} hours; a day can have at most 24. Enter {PbState.MaxHoursPerDay - OtherHours:0.##} or fewer."), new[] { nameof(Hours) });
            }

            if (IsBillable && IsInternal)
                yield return new ValidationResult("An internal project is never billed. Untick Billable.", new[] { nameof(IsBillable) });
        }
    }

    /// <summary>One entry as the grid shows and exports it.</summary>
    private sealed record EntryRow(int Id, DateTime Date, string DayText, string Project, string Task, decimal Hours, string BillableText, string Note, string InvoiceText);

    /// <summary>One week as the grid shows and exports it.</summary>
    private sealed record WeekRow(DateTime WeekStart, string WeekText, string Status, PbTimesheetStatus StatusValue, decimal Hours, decimal Billable, string Projects);

    #endregion
}
