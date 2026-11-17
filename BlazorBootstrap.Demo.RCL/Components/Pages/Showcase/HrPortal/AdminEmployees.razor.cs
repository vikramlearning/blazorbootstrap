using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Employees (HR admin): a table of everyone, including people joining and those who left (name with email, title,
/// department, manager, location, start date, status in words like "Joining Dec 1" or "Left Aug 28", and onboarding
/// progress as "x of y tasks done" with overdue tasks in words), filtered by status (working and joining, joining, left,
/// or everyone) with counts, by department, and by a name search. "Add employee" opens the editor. Each row has Edit (View
/// for someone who left), Start onboarding (a <see cref="Modal" /> with a template select showing each template's task
/// count; <see cref="HrState.StartOnboarding" />), and Mark as left (a <see cref="Modal" /> with the last day as a native
/// date and what happens, including the pending leave that is cancelled; <see cref="HrState.MarkLeft" />). Someone others
/// still report to, or yourself, can't be marked as left: a panel lists their reports with a link to change each one's
/// manager. Actions run after the dialog is hidden, outcomes are announced in an always-rendered status region, and focus
/// returns to the button that opened the dialog, or to the status message when the button is gone.
/// </summary>
public partial class AdminEmployees : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The row buttons that open a dialog, by "onb-{id}" or "left-{id}", for focus after the dialog.</summary>
    private readonly Dictionary<string, ElementReference> actionButtons = new();

    /// <summary>The person whose "can't mark as left yet" panel is shown, or <see langword="null" />.</summary>
    private int? blockedId;

    /// <summary>The department filter, or empty for every department.</summary>
    private string departmentId = string.Empty;

    /// <summary>The person the open (or last) dialog is about.</summary>
    private int dialogPersonId;

    /// <summary>The key of the button that takes focus after the next render.</summary>
    private string? focusButtonKey;

    private bool focusLastDay;

    private bool focusStatus;

    private string? lastDayError;

    private ElementReference? lastDayInput;

    private string lastDayText = string.Empty;

    private Modal leftModal = default!;

    private Modal onboardingModal = default!;

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

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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

    protected override void OnInitialized() => State.Changed += OnStateChanged;

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

    /// <summary>Describes the table, like "23 people working or joining in Engineering matching “an”".</summary>
    private string CaptionText(int count)
    {
        var what = statusFilter switch
        {
            EmployeeFilter.Current => "working here or joining",
            EmployeeFilter.Joining => "joining",
            EmployeeFilter.Left => "who left",
            _ => "in all (working, joining, and left)",
        };
        var department = State.FindDepartment(departmentId)?.Name;

        return $"{HrDisplay.Plural(count, "person", "people")} {what}"
               + (department is null ? null : $" in {department}")
               + (search.Trim().Length == 0 ? null : $" matching “{search.Trim()}”");
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

    /// <summary>Applies the department filter and the name search (not the status).</summary>
    private IReadOnlyList<HrEmployee> Narrowed(IReadOnlyList<HrEmployee> employees)
    {
        var term = search.Trim();

        return employees.Where(employee => (departmentId.Length == 0 || employee.DepartmentId == departmentId)
                                           && (term.Length == 0 || employee.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();
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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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

    private static string ShownText(int shown, int total) =>
        shown == total ? $"Showing all {HrDisplay.Plural(total, "person", "people")}." : $"Showing {shown} of {HrDisplay.Plural(total, "person", "people")}.";

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

    private HrOnboardingTemplate? SelectedTemplate =>
        int.TryParse(templateValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? State.FindTemplate(id) : null;

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The status filter of the table.</summary>
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

    /// <summary>A status filter button.</summary>
    private sealed record FilterOption(string Text, EmployeeFilter Filter);

    #endregion
}
