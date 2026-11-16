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

/// <summary>A client company of the agency.</summary>
public sealed class PbClient
{
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

    /// <summary>Gets or sets the phone number (free text).</summary>
    public string Phone { get; set; } = string.Empty;

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
public sealed record PbProjectInput(string Name, string Key, int? ClientId, string Description, PbProjectStatus Status, DateTime StartDate, DateTime DueDate, IReadOnlyList<string> MemberIds);

/// <summary>The fields of a deal the account manager saves at once.</summary>
public sealed record PbDealInput(int ClientId, string Title, int Value, DateTime CloseDate);
