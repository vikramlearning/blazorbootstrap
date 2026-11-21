namespace BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;

/// <summary>The kind of an entry on a client's activity timeline.</summary>
public enum PbActivityKind
{
    /// <summary>A note.</summary>
    Note,

    /// <summary>A phone or video call.</summary>
    Call,

    /// <summary>An email.</summary>
    Email,

    /// <summary>A meeting.</summary>
    Meeting,

    /// <summary>A deal changed (recorded by the system).</summary>
    Deal,

    /// <summary>A project started or finished for the client (recorded by the system).</summary>
    Project,
}

/// <summary>What a contact does for the agency's work with their company.</summary>
public enum PbContactRole
{
    /// <summary>Signs off scope and budget.</summary>
    DecisionMaker,

    /// <summary>Works with the team day to day.</summary>
    DayToDay,

    /// <summary>Receives invoices and handles payment.</summary>
    Billing,

    /// <summary>Owns the client's systems and integrations.</summary>
    Technical,

    /// <summary>Anything else.</summary>
    Other,
}

/// <summary>The stage of a deal on the pipeline board.</summary>
public enum PbDealStage
{
    /// <summary>A new lead, not yet qualified.</summary>
    Lead,

    /// <summary>Qualified: there is a need and a budget.</summary>
    Qualified,

    /// <summary>A proposal was sent.</summary>
    Proposal,

    /// <summary>Terms are being negotiated.</summary>
    Negotiation,

    /// <summary>Won: it becomes a project.</summary>
    Won,

    /// <summary>Lost.</summary>
    Lost,
}

/// <summary>The status of a project.</summary>
public enum PbProjectStatus
{
    /// <summary>Planned, not started.</summary>
    Planned,

    /// <summary>In progress.</summary>
    Active,

    /// <summary>Paused.</summary>
    OnHold,

    /// <summary>Finished.</summary>
    Completed,
}

/// <summary>The outcome of an action on the board's state.</summary>
public enum PbResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>The signed-in account's role can't do this.</summary>
    NotAllowed,

    /// <summary>The item doesn't exist (or was deleted).</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range.</summary>
    Invalid,

    /// <summary>The item isn't in a state that allows this.</summary>
    WrongState,

    /// <summary>Nothing changed.</summary>
    NoChange,

    /// <summary>Something with that name or key already exists.</summary>
    Duplicate,
}

/// <summary>The priority of a task.</summary>
public enum PbTaskPriority
{
    /// <summary>Low.</summary>
    Low,

    /// <summary>Medium.</summary>
    Medium,

    /// <summary>High.</summary>
    High,

    /// <summary>Urgent.</summary>
    Urgent,
}

/// <summary>The column of a task on a project board.</summary>
public enum PbTaskStatus
{
    /// <summary>To do.</summary>
    Todo,

    /// <summary>In progress.</summary>
    InProgress,

    /// <summary>In review.</summary>
    Review,

    /// <summary>Done.</summary>
    Done,
}

/// <summary>An entry on a client's activity timeline.</summary>
public sealed class PbActivity
{
    /// <summary>Gets or sets when it happened.</summary>
    public DateTime At { get; init; }

    /// <summary>Gets or sets who logged it (a person ID).</summary>
    public string AuthorId { get; init; } = string.Empty;

    /// <summary>Gets or sets the client.</summary>
    public int ClientId { get; init; }

    /// <summary>Gets or sets the deal it is about, if any.</summary>
    public int? DealId { get; init; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the kind.</summary>
    public PbActivityKind Kind { get; init; }

    /// <summary>Gets or sets the text (plain).</summary>
    public string Text { get; init; } = string.Empty;
}

/// <summary>One item of a task's checklist.</summary>
public sealed class PbChecklistItem
{
    /// <summary>Gets or sets whether it is done.</summary>
    public bool Done { get; set; }

    /// <summary>Gets or sets the ID (unique within the task).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the text (plain).</summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// A client company of the agency. The billing fields (currency, billing email, contract dates) are where the client's
/// invoices will come from once hours are billed.
/// </summary>
public sealed class PbClient
{
    /// <summary>Gets or sets the currency the client is billed in, an ISO 4217 code like "USD".</summary>
    public string BillingCurrency { get; set; } = "USD";

    /// <summary>Gets or sets where invoices go, or empty for none yet (shown as text, never linked).</summary>
    public string BillingEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the last day of the current contract, if any (on or after <see cref="ContractStart" />).</summary>
    public DateTime? ContractEnd { get; set; }

    /// <summary>Gets or sets the first day of the current contract, if any.</summary>
    public DateTime? ContractStart { get; set; }

    /// <summary>Gets or sets when it was added.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the industry.</summary>
    public string Industry { get; set; } = string.Empty;

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the account manager who owns the relationship (a person ID).</summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>Gets or sets the main phone number, or empty (shown as text, never linked).</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets the website (shown as text, never linked).</summary>
    public string Website { get; set; } = string.Empty;
}

/// <summary>A comment on a task.</summary>
public sealed class PbComment
{
    /// <summary>Gets or sets when it was written.</summary>
    public DateTime At { get; init; }

    /// <summary>Gets or sets the author (a person ID).</summary>
    public string AuthorId { get; init; } = string.Empty;

    /// <summary>Gets or sets the ID (unique within the task).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the text (plain).</summary>
    public string Text { get; init; } = string.Empty;
}

/// <summary>A person at a client company.</summary>
public sealed class PbContact
{
    /// <summary>Gets or sets the client.</summary>
    public int ClientId { get; init; }

    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether this is the client's main contact.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone number (digits, spaces, and + - ( ) .).</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets what they do for the work with the agency.</summary>
    public PbContactRole Role { get; set; } = PbContactRole.DayToDay;

    /// <summary>Gets or sets the job title.</summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>A sales opportunity with a client.</summary>
public sealed class PbDeal
{
    /// <summary>Gets or sets the client.</summary>
    public int ClientId { get; set; }

    /// <summary>Gets or sets when it closed (won or lost).</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>Gets or sets the expected close date.</summary>
    public DateTime CloseDate { get; set; }

    /// <summary>Gets or sets when it was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the owner (an account manager's person ID).</summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>Gets or sets the order within its stage column.</summary>
    public int Position { get; set; }

    /// <summary>Gets or sets the project the won deal became, if any.</summary>
    public int? ProjectId { get; set; }

    /// <summary>Gets or sets the stage.</summary>
    public PbDealStage Stage { get; set; }

    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the value in dollars (whole).</summary>
    public int Value { get; set; }
}

/// <summary>A dated goal of a project, shown on the roadmap.</summary>
public sealed class PbMilestone
{
    /// <summary>Gets or sets the due date.</summary>
    public DateTime Due { get; set; }

    /// <summary>Gets or sets the ID (unique within the project).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>A person on the agency's team (with or without a demo account).</summary>
public sealed class PbPerson
{
    /// <summary>Gets or sets the demo account, or <see langword="null" /> for a teammate who can't sign in.</summary>
    public string? AccountId { get; set; }

    /// <summary>Gets or sets how many hours a week they can take on.</summary>
    public int CapacityHours { get; set; } = 32;

    /// <summary>Gets or sets the ID.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets or sets the initials for the avatar.</summary>
    public string Initials { get; set; } = string.Empty;

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the job title.</summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>A client project the team delivers.</summary>
public sealed class PbProject
{
    /// <summary>Gets or sets the agreed budget in the client's billing currency (whole units), or 0 for none; billed hours will count against it.</summary>
    public int BudgetAmount { get; set; }

    /// <summary>Gets or sets the agreed budget in hours, or 0 for none; logged hours will count against it.</summary>
    public int BudgetHours { get; set; }

    /// <summary>Gets or sets the client, or <see langword="null" /> for an internal project.</summary>
    public int? ClientId { get; set; }

    /// <summary>Gets or sets when it was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets the description (plain).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the due date.</summary>
    public DateTime DueDate { get; set; }

    /// <summary>Gets or sets the won deal it came from, if any.</summary>
    public int? FromDealId { get; init; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the short key used in task numbers, like "WEB" (2 to 5 capital letters).</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the people on the project (person IDs).</summary>
    public List<string> MemberIds { get; set; } = new();

    /// <summary>Gets or sets the milestones, by due date.</summary>
    public List<PbMilestone> Milestones { get; set; } = new();

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the next milestone ID.</summary>
    public int NextMilestoneId { get; set; } = 1;

    /// <summary>Gets or sets the next task number (numbers are never reused, so a key like WEB-10 always means one task).</summary>
    public int NextTaskNumber { get; set; } = 1;

    /// <summary>Gets or sets the project manager (a person ID).</summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the status.</summary>
    public PbProjectStatus Status { get; set; }
}

/// <summary>A task on a project board.</summary>
public sealed class PbTask
{
    /// <summary>Gets or sets the assignee (a person ID), or <see langword="null" /> when unassigned.</summary>
    public string? AssigneeId { get; set; }

    /// <summary>Gets or sets the checklist.</summary>
    public List<PbChecklistItem> Checklist { get; set; } = new();

    /// <summary>Gets or sets the comments, oldest first.</summary>
    public List<PbComment> Comments { get; set; } = new();

    /// <summary>Gets or sets when it was moved to Done (cleared when it leaves Done).</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets when it was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets the description (plain).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the due date, if any.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Gets or sets the estimate in hours (0 to 80).</summary>
    public int EstimateHours { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the milestone it counts toward, if any.</summary>
    public int? MilestoneId { get; set; }

    /// <summary>Gets or sets the next checklist or comment ID.</summary>
    public int NextItemId { get; set; } = 1;

    /// <summary>Gets or sets the number within its project, shown with the key, like "WEB-12".</summary>
    public int Number { get; init; }

    /// <summary>Gets or sets the order within its column.</summary>
    public int Position { get; set; }

    /// <summary>Gets or sets the priority.</summary>
    public PbTaskPriority Priority { get; set; } = PbTaskPriority.Medium;

    /// <summary>Gets or sets the project.</summary>
    public int ProjectId { get; init; }

    /// <summary>Gets or sets the column.</summary>
    public PbTaskStatus Status { get; set; }

    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>The fields of a task an editor saves at once.</summary>
public sealed record PbTaskInput(string Title, string Description, string? AssigneeId, PbTaskPriority Priority, DateTime? DueDate, int EstimateHours, int? MilestoneId);

/// <summary>The fields of a project the project manager saves at once.</summary>
public sealed record PbProjectInput(
    string Name,
    string Key,
    int? ClientId,
    string Description,
    PbProjectStatus Status,
    DateTime StartDate,
    DateTime DueDate,
    IReadOnlyList<string> MemberIds,
    string OwnerId,
    int BudgetHours,
    int BudgetAmount);

/// <summary>The fields of a client the account manager saves at once.</summary>
public sealed record PbClientInput(
    string Name,
    string Industry,
    string Website,
    string OwnerId,
    string BillingCurrency,
    string BillingEmail,
    string Phone,
    DateTime? ContractStart,
    DateTime? ContractEnd);

/// <summary>The fields of a client contact the account manager saves at once.</summary>
public sealed record PbContactInput(string Name, string Title, string Email, string Phone, PbContactRole Role, bool IsPrimary);

/// <summary>The fields of a deal the account manager saves at once; the stage applies to a new deal only (an open one's moves on the pipeline).</summary>
public sealed record PbDealInput(int ClientId, string Title, int Value, DateTime CloseDate, PbDealStage Stage = PbDealStage.Lead);

/// <summary>The outcome of a bulk change from a list: the result, and how many records changed and how many were left as they were.</summary>
public sealed record PbBulkOutcome(PbResult Result, int Changed, int Unchanged);

/// <summary>Where an invoice is: a draft the account manager prepares, issued to the client, or paid. An issued invoice past its due date is overdue.</summary>
public enum PbInvoiceStatus
{
    /// <summary>Prepared, not sent: lines can still be removed, and it can be deleted.</summary>
    Draft,

    /// <summary>Sent to the client with a number and a due date; its lines are locked.</summary>
    Issued,

    /// <summary>Paid by the client.</summary>
    Paid,
}

/// <summary>The rate card's roles: each person bills at the rate of the role their job title falls under.</summary>
public enum PbRateRole
{
    /// <summary>Designers and researchers.</summary>
    Design,

    /// <summary>Developers and engineers (and anyone not in another role).</summary>
    Engineering,

    /// <summary>QA engineers.</summary>
    Quality,

    /// <summary>Content strategists and copywriters.</summary>
    Content,

    /// <summary>Project and account managers.</summary>
    Management,
}

/// <summary>Where a person's week of hours is: being filled in, waiting for the project managers, returned for changes, or approved.</summary>
public enum PbTimesheetStatus
{
    /// <summary>Being filled in; not submitted yet.</summary>
    Draft,

    /// <summary>Submitted: waiting for each project's manager to approve their project's hours.</summary>
    Submitted,

    /// <summary>Returned by a project manager with a reason: the person changes it and submits again.</summary>
    Returned,

    /// <summary>Every project's hours are approved; billable hours can be invoiced.</summary>
    Approved,
}

/// <summary>One line of an invoice: a person's approved billable hours on a project at their role's rate.</summary>
public sealed class PbInvoiceLine
{
    /// <summary>Gets or sets the amount: hours times rate, rounded to cents.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the hours.</summary>
    public decimal Hours { get; set; }

    /// <summary>Gets or sets the ID (unique on the board).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the person whose hours these are, or <see langword="null" /> for an invoice from before time tracking.</summary>
    public string? PersonId { get; init; }

    /// <summary>Gets or sets the project.</summary>
    public int ProjectId { get; init; }

    /// <summary>Gets or sets the hourly rate when the draft was made, in the invoice's currency.</summary>
    public int Rate { get; set; }

    /// <summary>Gets or sets the rate card role the rate came from.</summary>
    public PbRateRole Role { get; set; }
}

/// <summary>An invoice to a client for approved billable hours.</summary>
public sealed class PbInvoice
{
    /// <summary>Gets or sets the client.</summary>
    public int ClientId { get; init; }

    /// <summary>Gets or sets when the draft was made.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets who made the draft (a person ID).</summary>
    public string CreatedById { get; init; } = string.Empty;

    /// <summary>Gets or sets the currency, the client's billing currency when the draft was made.</summary>
    public string Currency { get; init; } = "USD";

    /// <summary>Gets or sets when payment is due (issue date plus the payment terms), once issued.</summary>
    public DateTime? DueOn { get; set; }

    /// <summary>Gets or sets the last day of the hours it covers.</summary>
    public DateTime HoursUpTo { get; init; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether it comes from before time tracking: its lines carry hours and amounts without time entries.</summary>
    public bool IsImported { get; init; }

    /// <summary>Gets or sets when it was issued.</summary>
    public DateTime? IssuedOn { get; set; }

    /// <summary>Gets or sets the lines, by project and person.</summary>
    public List<PbInvoiceLine> Lines { get; set; } = new();

    /// <summary>Gets or sets the invoice number given when it is issued, like "INV-2026-0042"; <see langword="null" /> for a draft.</summary>
    public string? Number { get; set; }

    /// <summary>Gets or sets when it was paid.</summary>
    public DateTime? PaidOn { get; set; }

    /// <summary>Gets or sets the status.</summary>
    public PbInvoiceStatus Status { get; set; }

    /// <summary>Gets or sets the payment terms in days (Net 14, 30, 45, or 60), once issued.</summary>
    public int TermsDays { get; set; }
}

/// <summary>A project manager's approval of one project's hours in a submitted week.</summary>
public sealed class PbProjectApproval
{
    /// <summary>Gets or sets when it was approved.</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Gets or sets who approved it (a person ID), or <see langword="null" /> while it waits.</summary>
    public string? ApprovedById { get; set; }

    /// <summary>Gets or sets the project.</summary>
    public int ProjectId { get; init; }
}

/// <summary>One rate card entry: a role's hourly rate, the same number in every billing currency.</summary>
public sealed class PbRate
{
    /// <summary>Gets or sets the hourly rate, in whole units of the client's currency.</summary>
    public int HourlyRate { get; set; }

    /// <summary>Gets or sets the role.</summary>
    public PbRateRole Role { get; init; }
}

/// <summary>Hours a person spent on a project on one day, logged on their timesheet.</summary>
public sealed class PbTimeEntry
{
    /// <summary>Gets or sets when it was logged.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets the day.</summary>
    public DateTime Date { get; set; }

    /// <summary>Gets or sets the hours, in quarter hours.</summary>
    public decimal Hours { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the draft or issued invoice it is on, or <see langword="null" /> while it isn't invoiced.</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Gets or sets whether the client is billed for it (never for an internal project).</summary>
    public bool IsBillable { get; set; }

    /// <summary>Gets or sets a short note (plain), or empty.</summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>Gets or sets whose hours they are (a person ID).</summary>
    public string PersonId { get; init; } = string.Empty;

    /// <summary>Gets or sets the project.</summary>
    public int ProjectId { get; set; }

    /// <summary>Gets or sets the task worked on, if any (one of the project's).</summary>
    public int? TaskId { get; set; }
}

/// <summary>A person's week of hours (Monday to Sunday) and where it is in the approval.</summary>
public sealed class PbTimesheet
{
    /// <summary>Gets or sets each project's approval while the week is submitted or approved.</summary>
    public List<PbProjectApproval> Approvals { get; set; } = new();

    /// <summary>Gets or sets when it was approved or returned.</summary>
    public DateTime? DecidedAt { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whose week it is (a person ID).</summary>
    public string PersonId { get; init; } = string.Empty;

    /// <summary>Gets or sets who returned it (a person ID).</summary>
    public string? ReturnedById { get; set; }

    /// <summary>Gets or sets why it was returned (plain), while it is returned.</summary>
    public string? ReturnReason { get; set; }

    /// <summary>Gets or sets the status.</summary>
    public PbTimesheetStatus Status { get; set; }

    /// <summary>Gets or sets when it was last submitted.</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Gets or sets the Monday the week starts on.</summary>
    public DateTime WeekStart { get; init; }
}

/// <summary>The fields of a time entry a person saves at once.</summary>
public sealed record PbTimeEntryInput(int ProjectId, int? TaskId, DateTime Date, decimal Hours, bool IsBillable, string Note);
