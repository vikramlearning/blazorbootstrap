namespace BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;

/// <summary>Whether someone works at the company.</summary>
public enum HrEmployeeStatus
{
    /// <summary>Hired, not started yet (already shown in the directory, as "Joining").</summary>
    Joining,

    /// <summary>Working.</summary>
    Active,

    /// <summary>Left the company (kept for analytics, hidden from the directory).</summary>
    Left,
}

/// <summary>Where a leave request is.</summary>
public enum HrLeaveStatus
{
    /// <summary>Waiting for the manager.</summary>
    Pending,

    /// <summary>Approved.</summary>
    Approved,

    /// <summary>Rejected by the manager (with a reason).</summary>
    Rejected,

    /// <summary>Withdrawn by the employee.</summary>
    Cancelled,
}

/// <summary>Who does an onboarding task.</summary>
public enum HrTaskOwner
{
    /// <summary>The new hire.</summary>
    NewHire,

    /// <summary>The new hire's manager.</summary>
    Manager,

    /// <summary>The HR team.</summary>
    Hr,
}

/// <summary>The outcome of an action on the portal's state.</summary>
public enum HrResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>The signed-in account's role (or relationship) doesn't allow this.</summary>
    NotAllowed,

    /// <summary>The item doesn't exist.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range.</summary>
    Invalid,

    /// <summary>The item isn't in a state that allows this.</summary>
    WrongState,

    /// <summary>Nothing changed.</summary>
    NoChange,

    /// <summary>Something with that name or email already exists.</summary>
    Duplicate,

    /// <summary>Not enough leave left in the balance.</summary>
    InsufficientBalance,

    /// <summary>It overlaps another request.</summary>
    Overlap,
}

/// <summary>A department.</summary>
public sealed record HrDepartment(string Id, string Name);

/// <summary>A company holiday (not counted as a leave day).</summary>
public sealed record HrHoliday(DateTime Date, string Name);

/// <summary>Someone at the company (with or without a demo account).</summary>
public sealed class HrEmployee
{
    /// <summary>Gets or sets the demo account, or <see langword="null" /> for someone who can't sign in.</summary>
    public string? AccountId { get; set; }

    /// <summary>Gets or sets a short introduction (plain text).</summary>
    public string Bio { get; set; } = string.Empty;

    /// <summary>Gets or sets the department.</summary>
    public string DepartmentId { get; set; } = string.Empty;

    /// <summary>Gets or sets the work email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the last day, once they left.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the initials for the avatar.</summary>
    public string Initials { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether HR invited this person to sign in: the first account that registers with the work email takes the
    /// record up (and this turns off). Records without an invitation can never be taken up by email.
    /// </summary>
    public bool InvitePending { get; set; }

    /// <summary>Gets or sets where they work.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets their manager, or <see langword="null" /> for the top of the org chart.</summary>
    public int? ManagerId { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone number.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets the pronouns they want shown (optional).</summary>
    public string Pronouns { get; set; } = string.Empty;

    /// <summary>Gets or sets the first day.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets whether they work here.</summary>
    public HrEmployeeStatus Status { get; set; }

    /// <summary>Gets or sets the job title.</summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>A type of leave and how many days a year it gives.</summary>
public sealed class HrLeavePolicy
{
    /// <summary>Gets or sets the days a year (0 for unlimited types like sick leave, which only track use).</summary>
    public int AnnualDays { get; set; }

    /// <summary>Gets or sets a short description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether it is offered for new requests.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets whether there is no yearly limit (only use is tracked).</summary>
    public bool IsUnlimited { get; set; }

    /// <summary>Gets or sets the minimum notice in days (0 for none).</summary>
    public int MinNoticeDays { get; set; }

    /// <summary>Gets or sets the name, like "Vacation".</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>A request for time off.</summary>
public sealed class HrLeaveRequest
{
    /// <summary>Gets or sets when it was made.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets when the manager decided.</summary>
    public DateTime? DecidedAt { get; set; }

    /// <summary>Gets or sets who decided (an employee ID).</summary>
    public int? DecidedById { get; set; }

    /// <summary>Gets or sets the manager's note (required to reject; plain text).</summary>
    public string DecisionNote { get; set; } = string.Empty;

    /// <summary>Gets or sets the working days it takes (weekdays that aren't company holidays).</summary>
    public int Days { get; set; }

    /// <summary>Gets or sets the employee.</summary>
    public int EmployeeId { get; init; }

    /// <summary>Gets or sets the first day.</summary>
    public DateTime From { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the employee's note (plain text).</summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>Gets or sets the leave type.</summary>
    public int PolicyId { get; set; }

    /// <summary>Gets or sets where it is.</summary>
    public HrLeaveStatus Status { get; set; }

    /// <summary>Gets or sets the last day.</summary>
    public DateTime To { get; set; }
}

/// <summary>A task of an onboarding template.</summary>
public sealed class HrTemplateTask
{
    /// <summary>Gets or sets the day it is due, counted from the start date (negative for before day one).</summary>
    public int DueOffsetDays { get; set; }

    /// <summary>Gets or sets the ID (unique within the template).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets who does it.</summary>
    public HrTaskOwner Owner { get; set; }

    /// <summary>Gets or sets what to do (plain text).</summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>A reusable onboarding checklist the HR admin keeps.</summary>
public sealed class HrOnboardingTemplate
{
    /// <summary>Gets or sets a short description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the next task ID.</summary>
    public int NextTaskId { get; set; } = 1;

    /// <summary>Gets or sets the tasks, by due day.</summary>
    public List<HrTemplateTask> Tasks { get; set; } = new();
}

/// <summary>One task of a new hire's onboarding.</summary>
public sealed class HrOnboardingTask
{
    /// <summary>Gets or sets when it was ticked off.</summary>
    public DateTime? DoneAt { get; set; }

    /// <summary>Gets or sets who ticked it off (an employee ID).</summary>
    public int? DoneById { get; set; }

    /// <summary>Gets or sets when it is due.</summary>
    public DateTime Due { get; init; }

    /// <summary>Gets or sets the ID (unique within the plan).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets who does it.</summary>
    public HrTaskOwner Owner { get; init; }

    /// <summary>Gets or sets what to do.</summary>
    public string Title { get; init; } = string.Empty;
}

/// <summary>A new hire's onboarding: the template's tasks with dates from the start date.</summary>
public sealed class HrOnboarding
{
    /// <summary>Gets or sets when it was started.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets the employee.</summary>
    public int EmployeeId { get; init; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the tasks, by due date.</summary>
    public List<HrOnboardingTask> Tasks { get; set; } = new();

    /// <summary>Gets or sets the template's name when it was started.</summary>
    public string TemplateName { get; init; } = string.Empty;
}

/// <summary>The fields of an employee the HR admin saves at once.</summary>
public sealed record HrEmployeeInput(string Name, string Email, string Title, string DepartmentId, int? ManagerId, string Location, DateTime StartDate, string Phone);

/// <summary>A leave balance for one type in the current year.</summary>
public sealed record HrBalance(HrLeavePolicy Policy, int Allowance, int Used, int Pending)
{
    /// <summary>Gets the days left after approved and pending requests (unlimited types have no limit).</summary>
    public int Available => Policy.IsUnlimited ? int.MaxValue : Math.Max(0, Allowance - Used - Pending);
}
