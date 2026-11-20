using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Adds (<c>/new</c>) or edits an employee (HR admin) in a validated form (rule WR-002): name (required, up to 80
/// characters), work email (required, an address like name@example.com, and unique: checked as you type, naming who has
/// it; the person registers with it to sign in), job title (required), department and location (required), manager
/// (people who work here or are joining, by name with title, or "No manager" for the top of the org chart; not someone in
/// their own reporting line), start date (required; a date after today makes them "Joining"), probation end date
/// (optional; after the start date and at most <see cref="HrState.MaxProbationMonths" /> months later: a cross-field
/// rule), and phone. A failed submit shows a summary linking to each field, and every <see cref="HrState.SaveEmployee" />
/// result is mapped to a field or the status message. Adding someone moves to their edit route, keeps the message, and
/// offers "Start onboarding" right there with a template select (<see cref="HrState.StartOnboarding" />), so a new hire is
/// added and onboarded in one flow; once started, the panel shows the plan's progress. The sign-in panel says whether an
/// account is linked yet. Someone who left is shown read-only with their last day; an unknown ID shows
/// <see cref="HrNotFound" />.
/// </summary>
public partial class AdminEmployeeEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after adding someone: the form and its status message stay.</summary>
    private int? createdId;

    private EditContext editContext = new(new EmployeeForm());

    private ShowcaseUniqueCheck? emailCheck;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private EmployeeForm form = new();

    /// <summary>Whether the person was just added in this page, so the onboarding panel is the next step.</summary>
    private bool justCreated;

    /// <summary>The route ID the form was loaded for (0 for a new employee), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private InputText? nameInput;

    /// <summary>The answers of the state that belong to a field (a manager in their own reporting line, a taken email), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

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

    /// <summary>Names who already has a work email, for the uniqueness check's message.</summary>
    private string EmailTakenMessage(string value) =>
        State.AllEmployees.FirstOrDefault(item => item.Id != Employee?.Id && item.Email == value.Trim().ToLowerInvariant()) is { } holder
            ? $"That email is taken by {holder.Name}{(holder.Status == HrEmployeeStatus.Left ? " (who left)" : null)}. Use another."
            : "That email is taken by someone else. Use another.";

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

    /// <summary>Whether another person (including someone who left) already has this work email.</summary>
    private Task<bool> IsEmailTakenAsync(string value)
    {
        var email = value.Trim().ToLowerInvariant();

        return Task.FromResult(State.AllEmployees.Any(item => item.Id != Employee?.Id && item.Email == email));
    }

    /// <summary>Fills the form from someone's record, or with a new hire's starting values (the first Monday a week from now).</summary>
    private void Load(HrEmployee? employee)
    {
        form = new EmployeeForm
        {
            Name = employee?.Name ?? string.Empty,
            Email = employee?.Email ?? string.Empty,
            Title = employee?.Title ?? string.Empty,
            DepartmentId = employee?.DepartmentId ?? string.Empty,
            Location = employee?.Location ?? State.Locations.FirstOrDefault() ?? string.Empty,
            ManagerValue = employee?.ManagerId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            StartDate = employee?.StartDate ?? NextStart(State.Clock.Today),
            ProbationEndDate = employee is null ? NextStart(State.Clock.Today).AddMonths(6) : employee.ProbationEndDate,
            Phone = employee?.Phone ?? string.Empty,
        };
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) => serverErrors?.Clear(args.FieldIdentifier);
        templateValue = AdminHelpers.DefaultTemplate(State, employee)?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(System.Linq.Expressions.Expression<Func<object>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(field, message);
        editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Saves the person once the form is valid: the email check finishes first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The manager's reporting line is checked next, and the state checks again; its answer
    /// maps to a field or the status message, as before.
    /// </summary>
    private async Task SaveAsync()
    {
        var existing = Employee;

        if (Id is not null && existing is null)
            return;

        serverErrors?.Clear();

        var emailFree = emailCheck is null || await emailCheck.CheckNowAsync();

        if (!editContext.Validate() || !emailFree)
            return;

        var today = State.Clock.Today;
        var trimmedEmail = form.Email.Trim().ToLowerInvariant();
        int? managerId = int.TryParse(form.ManagerValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        var manager = managerId is { } id ? State.FindEmployee(id) : null;

        if (managerId is not null && manager is not { Status: not HrEmployeeStatus.Left })
        {
            ServerError(() => form.ManagerValue, "That manager has left. Choose someone who works here.");

            return;
        }

        if (managerId is { } chosen && existing is not null && InOwnLine(chosen, existing.Id))
        {
            ServerError(() => form.ManagerValue, $"That would put them in their own reporting line: {manager!.Name} reports to {existing.Name}.");

            return;
        }

        if (existing is null && State.AllEmployees.Count >= HrState.MaxEmployees)
        {
            Announce(false, $"This demo tab already holds {HrState.MaxEmployees} people, the most it can. Nothing was saved.");
            focusStatus = true;

            return;
        }

        var start = form.StartDate!.Value.Date;
        var startChanged = existing is not null && existing.StartDate.Date != start;
        var input = new HrEmployeeInput(form.Name.Trim(), trimmedEmail, form.Title.Trim(), form.DepartmentId, managerId, form.Location, start, form.Phone.Trim(), form.ProbationEndDate?.Date);
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
                    focusStatus = true;

                    // The edit route renders the same page, so the form and the status message stay.
                    createdId = saved.Id;
                    Navigation.NavigateTo(AdminHelpers.EditEmployeeUrl(saved.Id), replace: true);

                    return;
                }

                Load(saved);
                Announce(true, $"Saved {saved.Name}{joining}."
                               + (startChanged && State.OnboardingOf(saved.Id) is not null ? " Their onboarding plan keeps the due dates it was started with." : null));
                focusStatus = true;

                return;

            case HrResult.Duplicate:
                ServerError(() => form.Email, EmailTakenMessage(trimmedEmail));
                Announce(false, "That work email is taken. Nothing was saved.");

                return;

            case HrResult.NoChange:
                Announce(true, "Nothing changed: their record is already saved like this.");
                focusStatus = true;

                return;

            case HrResult.Invalid when managerId is { } loopId && existing is not null && InOwnLine(loopId, existing.Id):
                ServerError(() => form.ManagerValue, $"That would put them in their own reporting line: {manager?.Name ?? "that person"} reports to {existing.Name}.");
                Announce(false, "That manager would create a loop in the org chart. Nothing was saved.");

                return;

            case HrResult.Invalid when managerId is not null && State.FindEmployee(managerId.Value) is not { Status: not HrEmployeeStatus.Left }:
                ServerError(() => form.ManagerValue, "That manager has left. Choose someone who works here.");
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
        if (form.StartDate is not { } start)
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

    #region Nested Types

    /// <summary>
    /// The job details form, with the limits of <see cref="HrState.SaveEmployee" />: name, email, and title required (up to
    /// 80 characters, the email up to 120 and like name@example.com), a department and a location, a start date the portal
    /// keeps (2015 to three years ahead), an optional probation end date after the start date and at most 12 months later
    /// (cross-field), and a phone of up to 30 characters (digits, spaces, and + ( ) - .).
    /// </summary>
    private sealed class EmployeeForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose a department.")]
        public string DepartmentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter their work email, like name@example.com.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer for the email.")]
        [RegularExpression(@"^\s*[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}\s*$", ErrorMessage = "Enter a work email like name@example.com.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose where they work.")]
        public string Location { get; set; } = string.Empty;

        /// <summary>Gets or sets the manager's ID as text; empty for no manager.</summary>
        public string ManagerValue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter their name.")]
        [StringLength(HrState.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(HrState.MaxPhoneLength, ErrorMessage = "Use 30 characters or fewer for the phone number.")]
        [RegularExpression(@"^[0-9+()\-.\s]*$", ErrorMessage = "Use digits, spaces, and + ( ) - . only for the phone number.")]
        public string Phone { get; set; } = string.Empty;

        public DateTime? ProbationEndDate { get; set; }

        [Required(ErrorMessage = "Pick their start date.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Enter their job title.")]
        [StringLength(HrState.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the job title.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the start date is one the portal keeps, and probation ends after it, within a year.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate is not { } start)
                yield break;

            if (start.Date < AdminHelpers.MinDate || start.Date > AdminHelpers.MaxDate)
                yield return new ValidationResult($"Pick a start date from {HrDisplay.Date(AdminHelpers.MinDate)} to {HrDisplay.Date(AdminHelpers.MaxDate)}.", new[] { nameof(StartDate) });

            if (ProbationEndDate is { } end && end.Date <= start.Date)
                yield return new ValidationResult($"The probation must end after the start date, {HrDisplay.Date(start)}.", new[] { nameof(ProbationEndDate) });
            else if (ProbationEndDate is { } late && late.Date > start.Date.AddMonths(HrState.MaxProbationMonths))
                yield return new ValidationResult(
                    string.Create(CultureInfo.InvariantCulture, $"The probation can last up to {HrState.MaxProbationMonths} months: end it by {HrDisplay.Date(start.Date.AddMonths(HrState.MaxProbationMonths))}."),
                    new[] { nameof(ProbationEndDate) });
        }
    }

    #endregion
}
