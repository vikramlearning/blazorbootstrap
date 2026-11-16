using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Adds (<c>/new</c>) or edits an employee (HR admin): name, work email (the person registers with it to sign in; a taken
/// email names who has it), job title, department, location, manager (people who work here or are joining, by name with
/// title, or "No manager" for the top of the org chart), start date (a native date; a date after today makes them
/// "Joining"), and phone. Each problem is shown under its field and tied to it, and the first invalid field takes focus
/// after the render that shows it; every <see cref="HrState.SaveEmployee" /> result is mapped to a field or the status
/// message (a manager in their own reporting line names the manager). Adding someone moves to their edit route, keeps the
/// message, and offers "Start onboarding" right there with a template select (<see cref="HrState.StartOnboarding" />), so
/// a new hire is added and onboarded in one flow; once started, the panel shows the plan's progress. The sign-in panel says
/// whether an account is linked yet. Someone who left is shown read-only with their last day; an unknown ID shows
/// <see cref="HrNotFound" />.
/// </summary>
public partial class AdminEmployeeEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after adding someone: the form and its status message stay.</summary>
    private int? createdId;

    private string? departmentError;

    private string departmentId = string.Empty;

    private ElementReference? departmentSelect;

    private string email = string.Empty;

    private string? emailError;

    private ElementReference? emailInput;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (the first field with an error).</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>Whether the person was just added in this page, so the onboarding panel is the next step.</summary>
    private bool justCreated;

    /// <summary>The route ID the form was loaded for (0 for a new employee), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private string location = string.Empty;

    private string? locationError;

    private ElementReference? locationSelect;

    private string? managerError;

    private ElementReference? managerSelect;

    private string managerValue = string.Empty;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    private string phone = string.Empty;

    private string? phoneError;

    private ElementReference? phoneInput;

    private string? startError;

    private ElementReference? startInput;

    private string startText = string.Empty;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ElementReference? templateSelect;

    private string templateValue = string.Empty;

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
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;

        // The first save of a new employee moves to their edit route: the form and the status message stay as they are.
        if (createdId is not null && createdId == routeId)
        {
            createdId = null;

            return;
        }

        statusMessage = null;
        statusIsError = false;
        justCreated = false;
        Load(Employee);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-employee route has no ID, and the router reuses this page when moving from
    /// someone's record to "Add employee", so the previous ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    /// <summary>Returns the first Monday at least a week after a day (a new hire's usual start).</summary>
    private static DateTime NextStart(DateTime today)
    {
        var day = today.AddDays(7);

        while (day.DayOfWeek != DayOfWeek.Monday)
            day = day.AddDays(1);

        return day;
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Whether <paramref name="managerId" /> is <paramref name="employeeId" /> or reports to them, directly or not.</summary>
    private bool InOwnLine(int managerId, int employeeId)
    {
        var seen = new HashSet<int>();

        for (var current = State.FindEmployee(managerId); current is not null && seen.Add(current.Id); current = current.ManagerId is { } next ? State.FindEmployee(next) : null)
        {
            if (current.Id == employeeId)
                return true;
        }

        return false;
    }

    /// <summary>Fills the form from someone's record, or with a new hire's starting values (the first Monday a week from now).</summary>
    private void Load(HrEmployee? employee)
    {
        name = employee?.Name ?? string.Empty;
        email = employee?.Email ?? string.Empty;
        title = employee?.Title ?? string.Empty;
        departmentId = employee?.DepartmentId ?? string.Empty;
        location = employee?.Location ?? State.Locations.FirstOrDefault() ?? string.Empty;
        managerValue = employee?.ManagerId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        startText = AdminHelpers.DateValue(employee?.StartDate ?? NextStart(State.Clock.Today));
        phone = employee?.Phone ?? string.Empty;
        templateValue = AdminHelpers.DefaultTemplate(State, employee)?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        nameError = emailError = titleError = departmentError = locationError = managerError = startError = phoneError = null;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Checks the form, saves the person, and maps the state's answer to a field or the status message.</summary>
    private void Save()
    {
        var existing = Employee;

        if (Id is not null && existing is null)
            return;

        var today = State.Clock.Today;
        var trimmedEmail = email.Trim().ToLowerInvariant();
        int? managerId = int.TryParse(managerValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        var manager = managerId is { } id ? State.FindEmployee(id) : null;

        nameError = AdminHelpers.TextProblem(name, HrState.MaxNameLength, "Enter their name.", "name");
        emailError = trimmedEmail.Length == 0 ? "Enter their work email, like name@example.com."
            : !AdminHelpers.IsEmail(trimmedEmail) ? "Enter a work email like name@example.com."
            : State.AllEmployees.FirstOrDefault(item => item.Id != existing?.Id && item.Email == trimmedEmail) is { } taken ? $"That email is taken by {taken.Name}{(taken.Status == HrEmployeeStatus.Left ? " (who left)" : null)}. Use another."
            : null;
        titleError = AdminHelpers.TextProblem(title, HrState.MaxNameLength, "Enter their job title.", "job title");
        departmentError = State.FindDepartment(departmentId) is null ? "Choose a department." : null;
        locationError = State.Locations.Contains(location) ? null : "Choose where they work.";
        managerError = managerId is not null && manager is not { Status: not HrEmployeeStatus.Left } ? "That manager has left. Choose someone who works here."
            : managerId is { } chosen && existing is not null && InOwnLine(chosen, existing.Id) ? $"That would put them in their own reporting line: {manager!.Name} reports to {existing.Name}."
            : null;
        startError = AdminHelpers.DateProblem(startText, "start date", out var start);
        phoneError = phone.Trim().Length > HrState.MaxPhoneLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the phone number to {HrState.MaxPhoneLength} characters or fewer (now {phone.Trim().Length}).")
            : null;

        focusTarget = nameError is not null ? () => nameInput
            : emailError is not null ? () => emailInput
            : titleError is not null ? () => titleInput
            : departmentError is not null ? () => departmentSelect
            : locationError is not null ? () => locationSelect
            : managerError is not null ? () => managerSelect
            : startError is not null ? () => startInput
            : phoneError is not null ? () => phoneInput
            : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        if (existing is null && State.AllEmployees.Count >= HrState.MaxEmployees)
        {
            Announce(false, $"This demo tab already holds {HrState.MaxEmployees} people, the most it can. Nothing was saved.");
            focusStatus = true;

            return;
        }

        var startChanged = existing is not null && existing.StartDate.Date != start.Date;
        var input = new HrEmployeeInput(name.Trim(), trimmedEmail, title.Trim(), departmentId, managerId, location, start, phone.Trim());
        var result = State.SaveEmployee(existing?.Id ?? 0, input, out var saved);

        switch (result)
        {
            case HrResult.Ok when saved is not null:
                var joining = saved.StartDate.Date > today ? $", joining {HrDisplay.Date(saved.StartDate)}" : null;
                var team = manager is null ? null : $" and in {manager.Name}'s team";

                if (existing is null)
                {
                    Load(saved);
                    justCreated = true;
                    Announce(true, $"Added {saved.Name}{joining}. They're in the directory and the org chart{team}. Next, start their onboarding: they see it when they register with {saved.Email}.");

                    // The edit route renders the same page, so the form and the status message stay.
                    createdId = saved.Id;
                    Navigation.NavigateTo(AdminHelpers.EditEmployeeUrl(saved.Id), replace: true);

                    return;
                }

                Load(saved);
                Announce(true, $"Saved {saved.Name}{joining}."
                               + (startChanged && State.OnboardingOf(saved.Id) is not null ? " Their onboarding plan keeps the due dates it was started with." : null));

                return;

            case HrResult.Duplicate:
                emailError = State.AllEmployees.FirstOrDefault(item => item.Id != existing?.Id && item.Email == trimmedEmail) is { } holder
                    ? $"That email is taken by {holder.Name}. Use another."
                    : "That email is taken by someone else. Use another.";
                focusTarget = () => emailInput;
                Announce(false, "That work email is taken. Nothing was saved.");

                return;

            case HrResult.NoChange:
                Announce(true, "Nothing changed: their record is already saved like this.");

                return;

            case HrResult.Invalid when managerId is { } loopId && existing is not null && InOwnLine(loopId, existing.Id):
                managerError = $"That would put them in their own reporting line: {manager?.Name ?? "that person"} reports to {existing.Name}.";
                focusTarget = () => managerSelect;
                Announce(false, "That manager would create a loop in the org chart. Nothing was saved.");

                return;

            case HrResult.Invalid when managerId is not null && State.FindEmployee(managerId.Value) is not { Status: not HrEmployeeStatus.Left }:
                managerError = "That manager has left. Choose someone who works here.";
                focusTarget = () => managerSelect;
                Announce(false, "That manager has left. Nothing was saved.");

                return;

            case HrResult.Invalid:
                Announce(false, existing is null && State.AllEmployees.Count >= HrState.MaxEmployees
                    ? $"This demo tab already holds {HrState.MaxEmployees} people, the most it can. Nothing was saved."
                    : "Their record couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");
                focusStatus = true;

                return;

            case HrResult.WrongState:
                Announce(false, "They were marked as left, so their record is read-only now. Nothing was saved.");
                focusStatus = true;

                return;

            case HrResult.NotFound:
                Announce(false, "This person's record was removed, so the changes couldn't be saved.");
                focusStatus = true;

                return;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can add and edit employees. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    /// <summary>Says what the start date means: joining (after today) or working here, and what it does to onboarding.</summary>
    private string StartHelp(DateTime today)
    {
        if (AdminHelpers.TryDate(startText) is not { } start)
            return "Their first day. A date after today makes them “Joining” until then.";

        var text = start > today
            ? $"After today: they show as “Joining {start.ToString("MMM d", CultureInfo.InvariantCulture)}” in the directory and the org chart until then."
            : "On or before today: they show as working here.";

        return Employee is { } employee && State.OnboardingOf(employee.Id) is not null && employee.StartDate.Date != start.Date
            ? text + " Their onboarding plan keeps the due dates it was started with."
            : text + " Onboarding due dates count from it.";
    }

    /// <summary>Starts onboarding for the person shown, from the chosen template.</summary>
    private void StartOnboarding()
    {
        if (Employee is not { } employee)
            return;

        if (SelectedTemplate is not { } template)
        {
            Announce(false, "Choose a template first.");
            focusTarget = () => templateSelect;

            return;
        }

        var result = State.StartOnboarding(employee.Id, template.Id, out var plan);
        var (ok, message) = AdminHelpers.StartOnboardingOutcome(State, result, employee, template, plan);
        Announce(ok, message);

        if (ok)
            justCreated = false;

        focusStatus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the employee ID from the route, or <see langword="null" /> on the new-employee route.</summary>
    [Parameter] public int? Id { get; set; }

    /// <summary>Gets the person of the route (the HR admin sees those who left too).</summary>
    private HrEmployee? Employee => Id is { } id ? State.FindEmployee(id) : null;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private HrOnboardingTemplate? SelectedTemplate =>
        int.TryParse(templateValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? State.FindTemplate(id) : null;

    [Inject] private HrState State { get; set; } = default!;

    #endregion
}
