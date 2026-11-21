using System.Text.Json.Serialization;

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

/// <summary>Where a performance review cycle is: each stage follows the one before, and HR moves it on.</summary>
public enum HrCycleStatus
{
    /// <summary>Employees write self reviews and managers write reviews with a rating.</summary>
    Open,

    /// <summary>Manager reviews are in; HR calibrates the ratings across the group.</summary>
    Calibration,

    /// <summary>Released: employees and managers read the outcome and sign off.</summary>
    SignOff,

    /// <summary>Done; kept for the record.</summary>
    Closed,
}

/// <summary>Where one person's review is, worked out from its fields and its cycle's status.</summary>
public enum HrReviewStage
{
    /// <summary>Waiting for the employee's self review.</summary>
    SelfReview,

    /// <summary>Waiting for the manager's review and rating.</summary>
    ManagerReview,

    /// <summary>The manager's review is in; waiting for HR to calibrate and release.</summary>
    Calibration,

    /// <summary>Released; waiting for the employee's or the manager's sign-off.</summary>
    SignOff,

    /// <summary>Signed off by both (or the cycle closed).</summary>
    Done,

    /// <summary>Left out when the cycle moved to calibration without a manager review.</summary>
    LeftOut,
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

/// <summary>
/// Someone at the company (with or without a demo account). Fields that are usually empty are left out of the tab's
/// snapshot while they hold their default, so a few hundred people fit in it.
/// </summary>
public sealed class HrEmployee
{
    /// <summary>Gets or sets the demo account, or <see langword="null" /> for someone who can't sign in.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountId { get; set; }

    /// <summary>Gets or sets a short introduction (plain text).</summary>
    public string Bio { get; set; } = string.Empty;

    /// <summary>Gets or sets the department.</summary>
    public string DepartmentId { get; set; } = string.Empty;

    /// <summary>Gets or sets the work email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the last day, once they left.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the initials for the avatar.</summary>
    public string Initials { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether HR invited this person to sign in: the first account that registers with the work email takes the
    /// record up (and this turns off). Records without an invitation can never be taken up by email.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool InvitePending { get; set; }

    /// <summary>Gets or sets where they work.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets their manager, or <see langword="null" /> for the top of the org chart.</summary>
    public int? ManagerId { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone number.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets the last day of their probation period, or <see langword="null" /> when they have none; always after the start date.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? ProbationEndDate { get; set; }

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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DecidedAt { get; set; }

    /// <summary>Gets or sets who decided (an employee ID).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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

/// <summary>The fields of an employee the HR admin saves at once; the probation end date is optional and after the start date.</summary>
public sealed record HrEmployeeInput(string Name, string Email, string Title, string DepartmentId, int? ManagerId, string Location, DateTime StartDate, string Phone, DateTime? ProbationEndDate = null);

/// <summary>A leave balance for one type in the current year.</summary>
public sealed record HrBalance(HrLeavePolicy Policy, int Allowance, int Used, int Pending)
{
    /// <summary>Gets the days left after approved and pending requests (unlimited types have no limit).</summary>
    public int Available => Policy.IsUnlimited ? int.MaxValue : Math.Max(0, Allowance - Used - Pending);
}

/// <summary>
/// A performance review cycle the HR admin opens for the whole company or some departments: everyone in scope who works
/// here and has a manager gets a review, with due dates for the self review and the manager's review.
/// </summary>
public sealed class HrReviewCycle
{
    /// <summary>Gets or sets when it moved to calibration.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CalibrationAt { get; set; }

    /// <summary>Gets or sets when it was closed.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? ClosedAt { get; set; }

    /// <summary>Gets or sets when it was opened.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets who opened it (an employee ID).</summary>
    public int CreatedById { get; init; }

    /// <summary>Gets or sets the departments in scope; empty for the whole company.</summary>
    public List<string> DepartmentIds { get; set; } = new();

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the day manager reviews are due.</summary>
    public DateTime ManagerReviewDue { get; init; }

    /// <summary>Gets or sets the name, like "2026 year-end reviews".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the day self reviews are due.</summary>
    public DateTime SelfReviewDue { get; init; }

    /// <summary>Gets or sets when it was released for sign-off.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? SignOffAt { get; set; }

    /// <summary>Gets or sets where it is.</summary>
    public HrCycleStatus Status { get; set; }
}

/// <summary>
/// One person's review in a cycle: their self review, their manager's review and rating (1 to 5), the rating HR
/// calibrated, and both sign-offs. Texts are plain text. Empty fields are left out of the tab's snapshot.
/// </summary>
public sealed class HrReview
{
    /// <summary>Gets or sets when HR calibrated the rating.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CalibratedAt { get; set; }

    /// <summary>Gets or sets who calibrated it (an employee ID).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CalibratedById { get; set; }

    /// <summary>Gets or sets the rating after calibration (1 to 5), or <see langword="null" /> until calibrated.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CalibratedRating { get; set; }

    /// <summary>Gets or sets why HR changed the manager's rating (required when it differs).</summary>
    public string CalibrationNote { get; set; } = string.Empty;

    /// <summary>Gets or sets the cycle.</summary>
    public int CycleId { get; init; }

    /// <summary>Gets or sets the employee's sign-off comment (optional).</summary>
    public string EmployeeComment { get; set; } = string.Empty;

    /// <summary>Gets or sets the employee.</summary>
    public int EmployeeId { get; init; }

    /// <summary>Gets or sets when the employee signed off.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? EmployeeSignedAt { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether it was left out of the cycle (no manager review when the cycle moved to calibration).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsLeftOut { get; set; }

    /// <summary>Gets or sets the manager when the cycle opened (an employee ID); they write the review and sign off.</summary>
    public int ManagerId { get; init; }

    /// <summary>Gets or sets the manager's rating (1 to 5).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ManagerRating { get; set; }

    /// <summary>Gets or sets when the manager signed off.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? ManagerSignedAt { get; set; }

    /// <summary>Gets or sets who signed off for the manager (the manager, or HR for a manager who can't sign in).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ManagerSignedById { get; set; }

    /// <summary>Gets or sets when the manager's review was submitted.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? ManagerSubmittedAt { get; set; }

    /// <summary>Gets or sets who submitted it (the manager, or HR for a manager who can't sign in).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ManagerSubmittedById { get; set; }

    /// <summary>Gets or sets the manager's review.</summary>
    public string ManagerText { get; set; } = string.Empty;

    /// <summary>Gets or sets the employee's own rating (optional, 1 to 5).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SelfRating { get; set; }

    /// <summary>Gets or sets when the self review was submitted.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? SelfSubmittedAt { get; set; }

    /// <summary>Gets or sets the employee's self review.</summary>
    public string SelfText { get; set; } = string.Empty;
}
