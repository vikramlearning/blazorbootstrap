using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Employees (HR admin): a <see cref="Grid{TItem}" /> of everyone, including people joining and those who left (name, work
/// email, title, department, manager, location, start date, status in words like "Joining Dec 1" or "Left Aug 28", and
/// onboarding progress as "x of y tasks done" with overdue tasks in words), with paging, sorting, the column chooser, and
/// CSV export, filtered by status (working and joining, joining, left, or everyone) with counts, by department, and by a
/// search of names, emails, and titles. The grid's view (page size, sort, columns) is saved for the visit and can be reset;
/// a row opens the person's record in the editor through its stretched name link. "Add employee" opens the editor. Each row
/// has Start onboarding (a <see cref="Modal" /> with a template select showing each template's task count;
/// <see cref="HrState.StartOnboarding" />) and Mark as left (a <see cref="Modal" /> with the last day as a native date and
/// what happens, including the pending leave that is cancelled; <see cref="HrState.MarkLeft" />). Someone others still
/// report to, or yourself, can't be marked as left: a panel lists their reports with a link to change each one's manager.
/// Actions run after the dialog is hidden, outcomes are announced in an always-rendered status region, and focus returns
/// to the button that opened the dialog, or to the status message when the button is gone.
/// </summary>
public partial class AdminEmployees : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    /// <summary>The row buttons that open a dialog, by "onb-{id}" or "left-{id}", for focus after the dialog.</summary>
    private readonly Dictionary<string, ElementReference> actionButtons = new();

    /// <summary>The person whose "can't mark as left yet" panel is shown, or <see langword="null" />.</summary>
    private int? blockedId;

    private ElementReference countRegion;

    /// <summary>The department filter, or empty for every department.</summary>
    private string departmentId = string.Empty;

    /// <summary>The person the open (or last) dialog is about.</summary>
    private int dialogPersonId;

    /// <summary>The key of the button that takes focus after the next render.</summary>
    private string? focusButtonKey;

    private bool focusLastDay;

    private bool focusStatus;

    private Grid<EmployeeRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastDayError;

    private ElementReference? lastDayInput;

    private string lastDayText = string.Empty;

    private Modal leftModal = default!;

    private Modal onboardingModal = default!;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>Whether Mark as left was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingLeft;

    /// <summary>Whether Start onboarding was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingOnboarding;

    private string search = string.Empty;

    private EmployeeFilter statusFilter = EmployeeFilter.Current;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ElementReference? templateSelect;

    private string templateValue = string.Empty;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The saved view lives in the tab's session storage, so it loads once the page is interactive; then the grid shows.
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusButtonKey is { } key)
        {
            focusButtonKey = null;

            if (actionButtons.TryGetValue(key, out var button) && ButtonStillShown(key))
                await HrDisplay.FocusAsync(button);
            else
                focusStatus = true;
        }

        if (focusLastDay)
        {
            focusLastDay = false;
            await HrDisplay.FocusAsync(lastDayInput);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await HrDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "employees", DefaultPageSize);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static BadgeColor StatusBadge(HrEmployeeStatus status) => status switch
    {
        HrEmployeeStatus.Active => BadgeColor.Success,
        HrEmployeeStatus.Joining => BadgeColor.Info,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Whether someone is in a status filter today.</summary>
    private static bool Matches(HrEmployee employee, EmployeeFilter filter, DateTime today)
    {
        var status = AdminHelpers.EffectiveStatus(employee, today);

        return filter switch
        {
            EmployeeFilter.Current => status != HrEmployeeStatus.Left,
            EmployeeFilter.Joining => status == HrEmployeeStatus.Joining,
            EmployeeFilter.Left => status == HrEmployeeStatus.Left,
            _ => true,
        };
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Whether a row button is still on the page (the person is shown and the action still applies).</summary>
    private bool ButtonStillShown(string key)
    {
        if (State.FindEmployee(dialogPersonId) is not { Status: not HrEmployeeStatus.Left } person)
            return false;

        var today = State.Clock.Today;

        if (!Narrowed(new[] { person }).Any() || !Matches(person, statusFilter, today))
            return false;

        return !key.StartsWith("onb-", StringComparison.Ordinal) || State.OnboardingOf(person.Id) is null;
    }

    private async Task ConfirmLeftAsync()
    {
        if (DialogPerson is not { } person)
            return;

        lastDayError = AdminHelpers.DateProblem(lastDayText, "last day", out var lastDay);

        if (lastDayError is null && lastDay < person.StartDate.Date)
            lastDayError = $"The last day can't be before their start date, {HrDisplay.Date(person.StartDate)}.";
        else if (lastDayError is null && lastDay > State.Clock.Today)
            lastDayError = "The last day can't be in the future: mark someone as left on or after their last day.";

        if (lastDayError is not null)
        {
            focusLastDay = true;

            return;
        }

        pendingLeft = true;
        await leftModal.HideAsync();
    }

    private async Task ConfirmOnboardingAsync()
    {
        pendingOnboarding = true;
        await onboardingModal.HideAsync();
    }

    private void DismissBlocked()
    {
        blockedId = null;
        focusStatus = true;
    }

    /// <summary>A filter changed: back to the first page of the filtered list.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Applies the department filter and the search of names, emails, and titles (not the status).</summary>
    private IReadOnlyList<HrEmployee> Narrowed(IReadOnlyList<HrEmployee> employees)
    {
        var term = search.Trim();

        return employees.Where(employee => (departmentId.Length == 0 || employee.DepartmentId == departmentId)
                                           && (term.Length == 0 || employee.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                                               || employee.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                                               || employee.Title.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private async Task OnDepartmentChangedAsync(ChangeEventArgs args)
    {
        departmentId = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    private void OnLeftHidden()
    {
        var id = dialogPersonId;

        if (!pendingLeft)
        {
            focusButtonKey = $"left-{id}";

            return;
        }

        pendingLeft = false;
        focusStatus = true;

        if (State.FindEmployee(id) is not { } person)
        {
            Announce(false, "That person's record was removed, so nothing changed.");

            return;
        }

        var lastDay = AdminHelpers.TryDate(lastDayText) ?? State.Clock.Today;
        var pending = State.LeaveOf(id).Count(request => request.Status == HrLeaveStatus.Pending);
        var result = State.MarkLeft(id, lastDay);

        switch (result)
        {
            case HrResult.Ok:
                blockedId = null;
                Announce(true, $"Marked {person.Name} as left, last day {HrDisplay.Date(lastDay)}. They're no longer in the directory or the org chart."
                               + (pending == 0 ? null : $" Their {HrDisplay.Plural(pending, "pending leave request was", "pending leave requests were")} cancelled."));

                break;

            case HrResult.WrongState when person.Status == HrEmployeeStatus.Left:
                Announce(false, $"{person.Name} was already marked as left, so nothing changed.");

                break;

            case HrResult.WrongState:
                blockedId = id;
                Announce(false, State.CurrentEmployee?.Id == id
                    ? "You can't mark yourself as left. Nothing changed."
                    : $"{person.Name} can't be marked as left while people still report to them. Nothing changed; give them a new manager first.");

                break;

            case HrResult.Invalid:
                Announce(false, $"The last day must be today or earlier, and on or after {person.Name}'s start date ({HrDisplay.Date(person.StartDate)}). Mark someone as left on or after their last day. Nothing changed.");
                focusButtonKey = $"left-{id}";
                focusStatus = false;

                break;

            case HrResult.NotFound:
                Announce(false, "That person's record was removed, so nothing changed.");

                break;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can mark someone as left. Nothing changed.");

                break;

            default:
                Announce(false, "That didn't work, and nothing changed. Try again.");

                break;
        }
    }

    private async Task OnLeftShown() => await HrDisplay.FocusAsync(lastDayInput);

    private void OnOnboardingHidden()
    {
        var id = dialogPersonId;

        if (!pendingOnboarding)
        {
            focusButtonKey = $"onb-{id}";

            return;
        }

        pendingOnboarding = false;
        focusButtonKey = $"onb-{id}";

        if (State.FindEmployee(id) is not { } person)
        {
            Announce(false, "That person's record was removed, so onboarding didn't start.");
            focusStatus = true;

            return;
        }

        var template = SelectedTemplate;
        var result = State.StartOnboarding(id, template?.Id ?? 0, out var plan);
        var (ok, message) = AdminHelpers.StartOnboardingOutcome(State, result, person, template, plan);
        Announce(ok, message);
    }

    private async Task OnOnboardingShown() => await HrDisplay.FocusAsync(templateSelect);

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    /// <summary>Someone was added, changed, or left (here or elsewhere): the grid reads the list again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>
    /// Opens Mark as left, or, when someone still reports to the person (or it's you), shows who has to move first.
    /// </summary>
    private async Task OpenLeftAsync(int employeeId)
    {
        if (State.FindEmployee(employeeId) is not { } person)
            return;

        dialogPersonId = employeeId;

        if (State.CurrentEmployee?.Id == employeeId || State.DirectReports(employeeId).Count > 0)
        {
            blockedId = employeeId;
            Announce(false, State.CurrentEmployee?.Id == employeeId
                ? "You can't mark yourself as left."
                : $"{person.Name} can't be marked as left yet: {HrDisplay.Plural(State.DirectReports(employeeId).Count, "person still reports", "people still report")} to them.");
            focusStatus = true;

            return;
        }

        blockedId = null;
        pendingLeft = false;
        lastDayError = null;
        lastDayText = AdminHelpers.DateValue(State.Clock.Today < person.StartDate.Date ? person.StartDate.Date : State.Clock.Today);
        await leftModal.ShowAsync();
    }

    private async Task OpenOnboardingAsync(int employeeId)
    {
        if (State.FindEmployee(employeeId) is not { } person)
            return;

        dialogPersonId = employeeId;
        pendingOnboarding = false;
        templateValue = AdminHelpers.DefaultTemplate(State, person)?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        await onboardingModal.ShowAsync();
    }

    /// <summary>The page of people the grid asked for, from the filtered list by name (or the column sort the grid asks for).</summary>
    private Task<GridDataProviderResult<EmployeeRow>> ProvideEmployeesAsync(GridDataProviderRequest<EmployeeRow> request)
    {
        var today = State.Clock.Today;
        var me = State.CurrentEmployee;
        var rows = Narrowed(State.AllEmployees).Where(employee => Matches(employee, statusFilter, today)).Select(employee => ToRow(employee, today, me)).ToList();

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        Announce(true, "The view is reset: people by name, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HrDisplay.FocusAsync(countRegion);
    }

    private string RowClass(EmployeeRow row) => row.StatusKind == HrEmployeeStatus.Left ? "hr-ae-row-left" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private async Task SetStatusFilterAsync(EmployeeFilter filter)
    {
        if (statusFilter == filter)
            return;

        statusFilter = filter;
        await FiltersChangedAsync();
    }

    private static string ShownText(int shown, int total) =>
        shown == total ? $"Showing all {HrDisplay.Plural(total, "person", "people")}." : $"Showing {shown} of {HrDisplay.Plural(total, "person", "people")}.";

    /// <summary>One person as a grid row, with the words the columns show and export.</summary>
    private EmployeeRow ToRow(HrEmployee employee, DateTime today, HrEmployee? me)
    {
        var manager = employee.ManagerId is { } managerId ? State.FindEmployee(managerId) : null;
        var plan = State.OnboardingOf(employee.Id);
        var progress = plan is null ? default : HrState.Progress(plan);
        var status = AdminHelpers.EffectiveStatus(employee, today);
        var overdue = plan is null || status == HrEmployeeStatus.Left ? 0 : plan.Tasks.Count(task => task.DoneAt is null && task.Due < today);

        return new EmployeeRow(employee.Id, employee.Name, employee.Initials, employee.Email, employee.Title, State.FindDepartment(employee.DepartmentId)?.Name ?? "No department",
            employee.ManagerId is null ? "None" : manager?.Name ?? "A former colleague", employee.ManagerId, employee.Location, employee.StartDate, HrDisplay.Date(employee.StartDate),
            AdminHelpers.StatusText(employee, today), status, plan is null ? "None" : AdminHelpers.ProgressText(progress.Done, progress.Total), progress.Done, progress.Total, overdue,
            me?.Id == employee.Id);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the person of the "can't mark as left yet" panel.</summary>
    private HrEmployee? BlockedPerson => blockedId is { } id ? State.FindEmployee(id) : null;

    private HrEmployee? DialogPerson => State.FindEmployee(dialogPersonId);

    private static IReadOnlyList<FilterOption> FilterOptions { get; } = new[]
    {
        new FilterOption("Working and joining", EmployeeFilter.Current),
        new FilterOption("Joining", EmployeeFilter.Joining),
        new FilterOption("Left", EmployeeFilter.Left),
        new FilterOption("Everyone", EmployeeFilter.All),
    };

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private HrOnboardingTemplate? SelectedTemplate =>
        int.TryParse(templateValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? State.FindTemplate(id) : null;

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The status filter of the list.</summary>
    private enum EmployeeFilter
    {
        /// <summary>Working here or joining.</summary>
        Current,

        /// <summary>Joining (starting after today).</summary>
        Joining,

        /// <summary>Left.</summary>
        Left,

        /// <summary>Everyone.</summary>
        All,
    }

    /// <summary>
    /// One person as a grid row: the words each column shows (and CSV export writes), and the values it sorts by.
    /// </summary>
    private sealed record EmployeeRow(int Id, string Name, string Initials, string Email, string Title, string Department, string Manager, int? ManagerId, string Location,
        DateTime StartDate, string Start, string Status, HrEmployeeStatus StatusKind, string Onboarding, int OnboardingDone, int OnboardingTotal, int Overdue, bool IsMe)
    {
        /// <summary>Gets the share of onboarding tasks done, for sorting; -1 without a plan, so those come first.</summary>
        public double OnboardingShare => OnboardingTotal == 0 ? -1 : (double)OnboardingDone / OnboardingTotal;
    }

    /// <summary>A status filter button.</summary>
    private sealed record FilterOption(string Text, EmployeeFilter Filter);

    #endregion
}
