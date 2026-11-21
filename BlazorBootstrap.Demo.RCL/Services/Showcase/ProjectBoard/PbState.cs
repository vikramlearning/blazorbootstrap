using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

/// <summary>A person's open work, split into this week (due by Sunday, or overdue) and later (or no due date).</summary>
public sealed record PbWorkload(PbPerson Person, IReadOnlyList<PbTask> Open, int HoursThisWeek, int HoursLater);

/// <summary>How many tasks (and estimated hours) were finished in a week starting on Monday.</summary>
public sealed record PbWeekThroughput(DateTime WeekStart, int Tasks, int Hours);

/// <summary>
/// State of "Tandem", the project and client board of Brightline Studio, for the current browser tab: the team, projects
/// with milestones and tasks, clients and contacts, the deal pipeline, client activity, the audit log, notifications, and
/// the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation (only what differs from
/// the deterministic seed); nothing is sent anywhere. The enterprise baseline (audit log, notifications, global search,
/// uniqueness checks, and the lists' bulk changes) is in <c>PbState.Enterprise.cs</c>.
/// </summary>
/// <remarks>
/// Rights come from the identity role: every signed-in person works tasks on the project boards (create, edit, move,
/// comment, checklists); a project manager also creates and edits projects and milestones, deletes tasks, and sees the
/// workload, roadmap, reports, and audit log; an account manager also manages clients, contacts, deals, and activity, and
/// sees the audit log. Moving a deal to Won creates a planned project for the project managers. Tasks of a completed
/// project are read-only: moving, editing, reassigning, and checklist changes return <see cref="PbResult.WrongState" />;
/// comments are still allowed.
/// </remarks>
public sealed partial class PbState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>The longest activity text.</summary>
    public const int MaxActivityLength = 1000;

    /// <summary>The most activity entries.</summary>
    public const int MaxActivities = 2000;

    /// <summary>The highest project budget, in the client's currency.</summary>
    public const int MaxBudgetAmount = 5_000_000;

    /// <summary>The highest project budget in hours.</summary>
    public const int MaxBudgetHours = 10_000;

    /// <summary>The most checklist items on a task.</summary>
    public const int MaxChecklistItems = 20;

    /// <summary>The longest checklist item.</summary>
    public const int MaxChecklistTextLength = 120;

    /// <summary>The most clients.</summary>
    public const int MaxClients = 300;

    /// <summary>The longest client name.</summary>
    public const int MaxClientNameLength = 80;

    /// <summary>The longest comment.</summary>
    public const int MaxCommentLength = 2000;

    /// <summary>The most comments on a task.</summary>
    public const int MaxComments = 50;

    /// <summary>The most contacts.</summary>
    public const int MaxContacts = 2000;

    /// <summary>The longest contact name, job title, or industry.</summary>
    public const int MaxContactFieldLength = 80;

    /// <summary>The highest deal value, in dollars.</summary>
    public const int MaxDealValue = 5_000_000;

    /// <summary>The most deals.</summary>
    public const int MaxDeals = 150;

    /// <summary>The longest deal title.</summary>
    public const int MaxDealTitleLength = 100;

    /// <summary>The longest task or project description.</summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>The highest task estimate, in hours.</summary>
    public const int MaxEstimateHours = 80;

    /// <summary>The highest weekly capacity a person can set, in hours.</summary>
    public const int MaxCapacityHours = 60;

    /// <summary>The most milestones on a project.</summary>
    public const int MaxMilestones = 10;

    /// <summary>The longest milestone title.</summary>
    public const int MaxMilestoneTitleLength = 60;

    /// <summary>The most people on the team: the seeded team plus every account the demo sign-in allows.</summary>
    public const int MaxPeople = 40 + ShowcaseIdentity.MaxAccounts;

    /// <summary>The longest phone number.</summary>
    public const int MaxPhoneLength = 30;

    /// <summary>The longest project name.</summary>
    public const int MaxProjectNameLength = 80;

    /// <summary>The most projects.</summary>
    public const int MaxProjects = 600;

    /// <summary>The longest task title.</summary>
    public const int MaxTaskTitleLength = 120;

    /// <summary>The most tasks.</summary>
    public const int MaxTasks = 5000;

    /// <summary>The longest client website.</summary>
    public const int MaxWebsiteLength = 80;

    private const int MaxAudit = 3000;

    /// <summary>The longest audit text (action, record, details).</summary>
    private const int MaxAuditTextLength = 300;

    private const int MaxNotifications = 1000;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int NextIdLimit = 1_000_000;

    /// <summary>The fresh tab's data, built once and never changed: a snapshot keeps only what differs from it.</summary>
    private static readonly Lazy<PbSeedSet> seed = new(PbSeedData.Build);

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<PbActivity> activities;

    private readonly List<ShowcaseAuditEntry> audit;

    private readonly List<PbClient> clients;

    private readonly List<PbContact> contacts;

    private readonly List<PbDeal> deals;

    private readonly List<ShowcaseNotification> notifications;

    private readonly List<PbPerson> people;

    private readonly List<PbProject> projects;

    private readonly List<PbTask> tasks;

    /// <summary>How many bulk changes are running; <see cref="Changed" /> is raised once when the outermost one ends.</summary>
    private int batchDepth;

    private bool changedInBatch;

    private bool isDarkTheme;

    private int nextActivityId;

    private int nextAuditId;

    private int nextClientId;

    private int nextContactId;

    private int nextDealId;

    private int nextNotificationId;

    private int nextProjectId;

    private int nextTaskId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public PbState()
    {
        var data = PbSeedData.Build();
        activities = data.Activities;
        audit = data.Audit;
        clients = data.Clients;
        contacts = data.Contacts;
        deals = data.Deals;
        notifications = data.Notifications;
        people = data.People;
        projects = data.Projects;
        tasks = data.Tasks;
        timeEntries = data.TimeEntries;
        timesheets = data.Timesheets;
        invoices = data.Invoices;
        rates = data.Rates;
        ResetCounters(data);

        Identity = new ShowcaseIdentity(PbSeedData.Roles(), PbSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns a client's activity, newest first.</summary>
    public IReadOnlyList<PbActivity> ActivitiesOf(int clientId) => activities.Where(item => item.ClientId == clientId).OrderByDescending(item => item.At).ThenByDescending(item => item.Id).ToList();

    /// <summary>Adds a checklist item to a task (any signed-in person); items of a task are different (Duplicate otherwise).</summary>
    public PbResult AddChecklistItem(int taskId, string text)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        if (!IsOpen(task))
            return PbResult.WrongState;

        text = text?.Trim() ?? string.Empty;

        if (!IsText(text, MaxChecklistTextLength) || task.Checklist.Count >= MaxChecklistItems || task.NextItemId >= NextIdLimit)
            return PbResult.Invalid;

        if (task.Checklist.Any(other => string.Equals(other.Text.Trim(), text, StringComparison.OrdinalIgnoreCase)))
            return PbResult.Duplicate;

        var item = new PbChecklistItem { Id = task.NextItemId++, Text = text };
        task.Checklist.Add(item);

        return Commit(() =>
        {
            task.Checklist.Remove(item);
            task.NextItemId--;
        }, () => Record("Checklist item added", TaskTarget(task), TaskPath(task.Id), text));
    }

    /// <summary>
    /// Adds a comment to a task as the signed-in person (plain text). The task's assignee is told, and so is everyone
    /// mentioned with @ and their first or full name (people who can sign in, not the writer).
    /// </summary>
    public PbResult AddComment(int taskId, string text)
    {
        if (Staff() is not { } person)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        text = text?.Trim() ?? string.Empty;

        if (!IsText(text, MaxCommentLength) || task.Comments.Count >= MaxComments || task.NextItemId >= NextIdLimit)
            return PbResult.Invalid;

        var comment = new PbComment { Id = task.NextItemId++, AuthorId = person.Id, At = Clock.Now, Text = text };
        task.Comments.Add(comment);

        return Commit(() =>
        {
            task.Comments.Remove(comment);
            task.NextItemId--;
        }, () =>
        {
            Record("Comment added", TaskTarget(task), TaskPath(task.Id), Clip(text, 120));
            var key = TaskKey(task);
            var mentioned = Mentioned(text);

            foreach (var other in mentioned)
                NotifyPerson(other.Id, $"{person.Name} mentioned you on {key}", Clip(text, 160), TaskPath(task.Id));

            if (task.AssigneeId is { } assigneeId && mentioned.All(other => other.Id != assigneeId))
                NotifyPerson(assigneeId, $"{person.Name} commented on {key}", Clip(text, 160), TaskPath(task.Id));
        });
    }

    /// <summary>
    /// Returns the snapshot: the ID counters and, of every list, only what differs from the fresh tab's seed (records changed
    /// or added, and the IDs of seeded records removed). The seed is the same in every tab, so the restore rebuilds the rest
    /// from it; a fresh tab's snapshot is a few hundred bytes although it holds thousands of records (devspec finding F-4).
    /// </summary>
    public object CaptureData()
    {
        var data = seed.Value;
        var (changedActivities, removedActivities) = Diff(activities, data.Activities, item => item.Id, SameJson);
        var (changedAudit, removedAudit) = Diff(audit, data.Audit, entry => entry.Id, SameAudit);
        var (changedClients, removedClients) = Diff(clients, data.Clients, client => client.Id, SameClient);
        var (changedContacts, removedContacts) = Diff(contacts, data.Contacts, contact => contact.Id, SameContact);
        var (changedDeals, removedDeals) = Diff(deals, data.Deals, deal => deal.Id, SameJson);
        var (changedNotifications, removedNotifications) = Diff(notifications, data.Notifications, notice => notice.Id, SameNotification);
        var (changedPeople, removedPeople) = Diff(people, data.People, person => person.Id, SameJson);
        var (changedProjects, removedProjects) = Diff(projects, data.Projects, project => project.Id, SameProject);
        var (changedTasks, removedTasks) = Diff(tasks, data.Tasks, task => task.Id, SameTask);
        var (changedEntries, removedEntries) = Diff(timeEntries, data.TimeEntries, entry => entry.Id, SameEntry);
        var (changedSheets, removedSheets) = Diff(timesheets, data.Timesheets, sheet => sheet.Id, SameJson);
        var (changedInvoices, removedInvoices) = Diff(invoices, data.Invoices, invoice => invoice.Id, SameJson);
        var ratesChanged = rates.Any(rate => data.Rates.First(seeded => seeded.Role == rate.Role).HourlyRate != rate.HourlyRate);

        return new PbSnapshot
        {
            Activities = changedActivities,
            RemovedActivityIds = removedActivities,
            Audit = changedAudit,
            RemovedAuditIds = removedAudit,
            Clients = changedClients,
            RemovedClientIds = removedClients,
            Contacts = changedContacts,
            RemovedContactIds = removedContacts,
            Deals = changedDeals,
            RemovedDealIds = removedDeals,
            Notifications = changedNotifications,
            RemovedNotificationIds = removedNotifications,
            People = changedPeople,
            RemovedPersonIds = removedPeople,
            Projects = changedProjects,
            RemovedProjectIds = removedProjects,
            Tasks = changedTasks,
            RemovedTaskIds = removedTasks,
            TimeEntries = changedEntries,
            RemovedTimeEntryIds = removedEntries,
            Timesheets = changedSheets,
            RemovedTimesheetIds = removedSheets,
            Invoices = changedInvoices,
            RemovedInvoiceIds = removedInvoices,
            Rates = ratesChanged ? rates.ToList() : null,
            NextInvoiceId = nextInvoiceId,
            NextInvoiceLineId = nextInvoiceLineId,
            NextTimeEntryId = nextTimeEntryId,
            NextTimesheetId = nextTimesheetId,
            NextActivityId = nextActivityId,
            NextAuditId = nextAuditId,
            NextClientId = nextClientId,
            NextContactId = nextContactId,
            NextDealId = nextDealId,
            NextNotificationId = nextNotificationId,
            NextProjectId = nextProjectId,
            NextTaskId = nextTaskId,
        };
    }

    /// <summary>Returns the tasks finished in each of the last <paramref name="weeks" /> weeks (Monday to Sunday), oldest first, this week last.</summary>
    public IReadOnlyList<PbWeekThroughput> CompletedPerWeek(int weeks)
    {
        var thisWeek = WeekStart(Clock.Today);

        return Enumerable.Range(0, Math.Clamp(weeks, 1, 26)).Reverse().Select(back =>
        {
            var start = thisWeek.AddDays(-7 * back);
            var done = tasks.Where(task => task.CompletedAt is { } at && at >= start && at < start.AddDays(7)).ToList();

            return new PbWeekThroughput(start, done.Count, done.Sum(task => task.EstimateHours));
        }).ToList();
    }

    /// <summary>Returns a client's contacts, the primary contact first.</summary>
    public IReadOnlyList<PbContact> ContactsOf(int clientId) =>
        contacts.Where(item => item.ClientId == clientId).OrderByDescending(item => item.IsPrimary).ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>
    /// Creates a task at the end of a column (any signed-in person; not on a completed project). Its due date, when set,
    /// falls within the project's start and due dates. Someone assigned other than the creator is told.
    /// </summary>
    public PbResult CreateTask(int projectId, PbTaskStatus status, PbTaskInput input, out PbTask? created)
    {
        created = null;

        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindProject(projectId) is not { } project)
            return PbResult.NotFound;

        if (project.Status == PbProjectStatus.Completed)
            return PbResult.WrongState;

        if (!Enum.IsDefined(status) || !ValidTaskInput(project, input) || tasks.Count >= MaxTasks || nextTaskId >= NextIdLimit || project.NextTaskNumber >= NextIdLimit)
            return PbResult.Invalid;

        var task = new PbTask
        {
            Id = nextTaskId++, ProjectId = project.Id, Number = project.NextTaskNumber++,
            Status = status, Position = tasks.Count(item => item.ProjectId == project.Id && item.Status == status), CreatedAt = Clock.Now,
            CompletedAt = status == PbTaskStatus.Done ? Clock.Now : null,
        };

        Apply(task, input);
        tasks.Add(task);
        var result = Commit(() =>
        {
            tasks.Remove(task);
            nextTaskId--;
            project.NextTaskNumber--;
        }, () =>
        {
            Record("Task created", TaskTarget(task), TaskPath(task.Id), $"{task.Title}, in {StatusWord(task.Status)}{(task.AssigneeId is null ? ", unassigned" : $", assigned to {FindPerson(task.AssigneeId)?.Name}")}");
            NotifyAssigned(task);
        });

        if (result == PbResult.Ok)
            created = task;

        return result;
    }

    /// <summary>Returns the deals of a client, open ones first.</summary>
    public IReadOnlyList<PbDeal> DealsOf(int clientId) =>
        deals.Where(item => item.ClientId == clientId).OrderBy(item => item.ClosedAt is not null).ThenBy(item => item.CloseDate).ToList();

    /// <summary>Returns the deals in a pipeline column, in their order.</summary>
    public IReadOnlyList<PbDeal> DealsIn(PbDealStage stage) => deals.Where(item => item.Stage == stage).OrderBy(item => item.Position).ThenBy(item => item.Id).ToList();

    /// <summary>Deletes an activity the signed-in account manager logged (not a system entry).</summary>
    public PbResult DeleteActivity(int activityId)
    {
        if (AccountManager() is not { } person)
            return PbResult.NotAllowed;

        if (activities.FirstOrDefault(item => item.Id == activityId) is not { } activity)
            return PbResult.NotFound;

        if (activity.Kind is PbActivityKind.Deal or PbActivityKind.Project || activity.AuthorId != person.Id)
            return PbResult.WrongState;

        activities.Remove(activity);
        Record("Activity deleted", ClientTarget(activity.ClientId), ClientPath(activity.ClientId), $"{activity.Kind}: {Clip(activity.Text, 120)}");
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Removes a checklist item (any signed-in person).</summary>
    public PbResult DeleteChecklistItem(int taskId, int itemId)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task || task.Checklist.FirstOrDefault(item => item.Id == itemId) is not { } item)
            return PbResult.NotFound;

        if (!IsOpen(task))
            return PbResult.WrongState;

        task.Checklist.Remove(item);
        Record("Checklist item removed", TaskTarget(task), TaskPath(task.Id), item.Text);
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes a client (account managers): only one with no projects and no deals; its contacts and activity go with it.</summary>
    public PbResult DeleteClient(int clientId)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (FindClient(clientId) is not { } client)
            return PbResult.NotFound;

        if (projects.Any(item => item.ClientId == clientId) || deals.Any(item => item.ClientId == clientId) || invoices.Any(item => item.ClientId == clientId))
            return PbResult.WrongState;

        var contactCount = contacts.Count(item => item.ClientId == clientId);
        clients.Remove(client);
        contacts.RemoveAll(item => item.ClientId == clientId);
        activities.RemoveAll(item => item.ClientId == clientId);
        Record("Client deleted", $"Client {client.Name}", null, Invariant($"With {contactCount} contact(s) and its activity"));
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes the signed-in person's own comment.</summary>
    public PbResult DeleteComment(int taskId, int commentId)
    {
        if (Staff() is not { } person)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task || task.Comments.FirstOrDefault(item => item.Id == commentId) is not { } comment)
            return PbResult.NotFound;

        if (comment.AuthorId != person.Id)
            return PbResult.WrongState;

        task.Comments.Remove(comment);
        Record("Comment deleted", TaskTarget(task), TaskPath(task.Id), Clip(comment.Text, 120));
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes a contact (account managers).</summary>
    public PbResult DeleteContact(int contactId)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (contacts.FirstOrDefault(item => item.Id == contactId) is not { } contact)
            return PbResult.NotFound;

        contacts.Remove(contact);
        Record("Contact deleted", ContactTarget(contact), ClientPath(contact.ClientId), contact.Email);
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes a deal (account managers); not a won deal whose project still exists.</summary>
    public PbResult DeleteDeal(int dealId)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (FindDeal(dealId) is not { } deal)
            return PbResult.NotFound;

        if (deal.ProjectId is { } projectId && FindProject(projectId) is not null)
            return PbResult.WrongState;

        deals.Remove(deal);
        Renumber(DealsIn(deal.Stage));
        Record("Deal deleted", DealTarget(deal), null, $"{deal.Stage}, {Dollars(deal.Value)}");
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes a milestone (project managers); its tasks no longer count toward one.</summary>
    public PbResult DeleteMilestone(int projectId, int milestoneId)
    {
        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        if (FindProject(projectId) is not { } project || project.Milestones.FirstOrDefault(item => item.Id == milestoneId) is not { } milestone)
            return PbResult.NotFound;

        project.Milestones.Remove(milestone);

        foreach (var task in tasks.Where(item => item.ProjectId == projectId && item.MilestoneId == milestoneId))
            task.MilestoneId = null;

        Record("Milestone deleted", ProjectTarget(project), ProjectPath(project.Id), milestone.Title);
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes a project and its tasks (project managers); a deal it came from stays won. A project with logged hours keeps its history (WrongState).</summary>
    public PbResult DeleteProject(int projectId)
    {
        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        if (FindProject(projectId) is not { } project)
            return PbResult.NotFound;

        if (timeEntries.Any(entry => entry.ProjectId == projectId) || invoices.Any(invoice => invoice.Lines.Any(line => line.ProjectId == projectId)))
            return PbResult.WrongState;

        var taskCount = tasks.Count(item => item.ProjectId == projectId);
        projects.Remove(project);
        tasks.RemoveAll(item => item.ProjectId == projectId);

        foreach (var deal in deals.Where(item => item.ProjectId == projectId))
            deal.ProjectId = null;

        Record("Project deleted", ProjectTarget(project), null, Invariant($"With {taskCount} task(s)"));
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes a task (project managers).</summary>
    public PbResult DeleteTask(int taskId)
    {
        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        var target = TaskTarget(task);
        tasks.Remove(task);

        // Hours logged on it stay on the project.
        foreach (var entry in timeEntries.Where(entry => entry.TaskId == taskId))
            entry.TaskId = null;

        Renumber(TasksIn(task.ProjectId, task.Status));
        Record("Task deleted", target, null, task.Title);
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Finds a client, or returns <see langword="null" />.</summary>
    public PbClient? FindClient(int? clientId) => clients.FirstOrDefault(item => item.Id == clientId);

    /// <summary>Finds a contact, or returns <see langword="null" />.</summary>
    public PbContact? FindContact(int contactId) => contacts.FirstOrDefault(item => item.Id == contactId);

    /// <summary>Finds a deal, or returns <see langword="null" />.</summary>
    public PbDeal? FindDeal(int? dealId) => deals.FirstOrDefault(item => item.Id == dealId);

    /// <summary>Finds a person, or returns <see langword="null" />.</summary>
    public PbPerson? FindPerson(string? personId) => people.FirstOrDefault(item => item.Id == personId);

    /// <summary>Finds a project, or returns <see langword="null" />.</summary>
    public PbProject? FindProject(int? projectId) => projects.FirstOrDefault(item => item.Id == projectId);

    /// <summary>Finds a task, or returns <see langword="null" />.</summary>
    public PbTask? FindTask(int taskId) => tasks.FirstOrDefault(item => item.Id == taskId);

    /// <summary>
    /// Logs a note, call, email, or meeting on a client's timeline as the signed-in account manager, optionally about one of
    /// the client's deals, on the day it happened: today (at the current time) or an earlier day (at noon), never in the
    /// future and not before the client was added.
    /// </summary>
    public PbResult LogActivity(int clientId, PbActivityKind kind, string text, int? dealId, DateTime date, out PbActivity? logged)
    {
        logged = null;

        if (AccountManager() is not { } person)
            return PbResult.NotAllowed;

        if (FindClient(clientId) is not { } client)
            return PbResult.NotFound;

        text = text?.Trim() ?? string.Empty;

        if (kind is not (PbActivityKind.Note or PbActivityKind.Call or PbActivityKind.Email or PbActivityKind.Meeting) || !IsText(text, MaxActivityLength)
            || (dealId is { } id && FindDeal(id)?.ClientId != clientId) || activities.Count >= MaxActivities || nextActivityId >= NextIdLimit
            || date.Date > Clock.Today || date.Date < client.CreatedAt.Date || !IsPlausible(date))
            return PbResult.Invalid;

        var at = date.Date == Clock.Today ? Clock.Now : date.Date.AddHours(12);
        var activity = new PbActivity { Id = nextActivityId++, ClientId = clientId, Kind = kind, Text = text, DealId = dealId, AuthorId = person.Id, At = at };
        activities.Add(activity);
        var result = Commit(() =>
        {
            activities.Remove(activity);
            nextActivityId--;
        }, () => Record("Activity logged", ClientTarget(clientId), ClientPath(clientId), $"{kind}: {Clip(text, 120)}"));

        if (result == PbResult.Ok)
            logged = activity;

        return result;
    }

    /// <summary>Returns how many of a milestone's tasks are done, and how many it has.</summary>
    public (int Done, int Total) MilestoneProgress(int projectId, int milestoneId)
    {
        var list = tasks.Where(item => item.ProjectId == projectId && item.MilestoneId == milestoneId).ToList();

        return (list.Count(item => item.Status == PbTaskStatus.Done), list.Count);
    }

    /// <summary>
    /// Moves a deal to a stage and position (account managers). Won creates a planned project for the client (returned in
    /// <paramref name="createdProject" />), logs it, and tells its project manager; Lost closes the deal. A won deal whose
    /// project exists can't leave Won.
    /// </summary>
    public PbResult MoveDeal(int dealId, PbDealStage stage, int index, out PbProject? createdProject)
    {
        createdProject = null;

        if (AccountManager() is not { } person)
            return PbResult.NotAllowed;

        if (FindDeal(dealId) is not { } deal)
            return PbResult.NotFound;

        if (!Enum.IsDefined(stage))
            return PbResult.Invalid;

        var from = deal.Stage;

        if (from == stage)
        {
            var column = DealsIn(stage).ToList();
            var oldIndex = column.IndexOf(deal);
            index = Math.Clamp(index, 0, column.Count - 1);

            if (oldIndex == index)
                return PbResult.NoChange;

            column.RemoveAt(oldIndex);
            column.Insert(index, deal);
            Renumber(column);
            RaiseChanged();

            return PbResult.Ok;
        }

        if (deal.ProjectId is { } projectId && FindProject(projectId) is not null)
            return PbResult.WrongState;

        // A won or lost deal adds a timeline entry: only when the snapshot has room for it.
        if (stage is PbDealStage.Won or PbDealStage.Lost && !FitsSnapshot(1024))
            return PbResult.Invalid;

        PbProject? project = null;

        if (stage == PbDealStage.Won && deal.ProjectId is null)
        {
            if (projects.Count >= MaxProjects || nextProjectId >= NextIdLimit || activities.Count >= MaxActivities)
                return PbResult.Invalid;

            if (UniqueKey(FindClient(deal.ClientId)?.Name ?? deal.Title) is not { } key)
                return PbResult.Invalid;

            var start = WeekStart(Clock.Today).AddDays(7);
            project = new PbProject
            {
                Id = nextProjectId++, Key = key, Name = FreeProjectName(Clip(deal.Title, MaxProjectNameLength), deal.ClientId), ClientId = deal.ClientId,
                Status = PbProjectStatus.Planned, StartDate = start, DueDate = start.AddDays(7 * 8), OwnerId = DefaultProjectManagerId(), FromDealId = deal.Id, CreatedAt = Clock.Now,
                Description = $"Created from the won deal “{deal.Title}”. Plan the scope, team, and milestones.", BudgetAmount = Math.Min(deal.Value, MaxBudgetAmount),
                BudgetHours = Math.Min(deal.Value / 150, MaxBudgetHours),
            };
            projects.Add(project);

            // Room for the project and its timeline entry; otherwise nothing changes.
            if (!FitsSnapshot(2048))
            {
                projects.Remove(project);
                nextProjectId--;

                return PbResult.Invalid;
            }

            deal.ProjectId = project.Id;
        }

        var oldColumn = DealsIn(from).Where(item => item != deal).ToList();
        var newColumn = DealsIn(stage).ToList();
        newColumn.Insert(Math.Clamp(index, 0, newColumn.Count), deal);
        deal.Stage = stage;
        deal.ClosedAt = stage is PbDealStage.Won or PbDealStage.Lost ? Clock.Now : null;
        Renumber(oldColumn);
        Renumber(newColumn);

        if (stage is PbDealStage.Won or PbDealStage.Lost && activities.Count < MaxActivities && nextActivityId < NextIdLimit)
        {
            var text = stage == PbDealStage.Lost
                ? $"Lost “{deal.Title}” ({Dollars(deal.Value)})."
                : project is not null ? $"Won “{deal.Title}” ({Dollars(deal.Value)}). Project {project.Key} created for the project managers." : $"Won “{deal.Title}” ({Dollars(deal.Value)}).";
            activities.Add(new PbActivity { Id = nextActivityId++, ClientId = deal.ClientId, DealId = deal.Id, Kind = PbActivityKind.Deal, AuthorId = person.Id, At = Clock.Now, Text = text });
        }

        Record(stage switch { PbDealStage.Won => "Deal won", PbDealStage.Lost => "Deal lost", _ => "Deal moved" }, DealTarget(deal), DealPath(),
            $"{from} → {stage}, {Dollars(deal.Value)}");

        if (project is not null)
        {
            Record("Project created", ProjectTarget(project), ProjectPath(project.Id), $"From the won deal “{deal.Title}”, planned for {Day(project.StartDate)}");
            NotifyPerson(project.OwnerId, $"{project.Key} was created from a won deal", $"{person.Name} won “{deal.Title}”. Plan its team and milestones.", ProjectEditPath(project.Id));
        }

        createdProject = project;
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>
    /// Moves a task to a column and position (any signed-in person): drag and drop on the board, or the "Move to" menu.
    /// Moving to Done records when it was finished; moving out of Done clears it. Moving to In review tells the project
    /// manager.
    /// </summary>
    public PbResult MoveTask(int taskId, PbTaskStatus status, int index)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        if (!IsOpen(task))
            return PbResult.WrongState;

        if (!Enum.IsDefined(status))
            return PbResult.Invalid;

        if (task.Status == status)
        {
            var column = TasksIn(task.ProjectId, status).ToList();
            var oldIndex = column.IndexOf(task);
            index = Math.Clamp(index, 0, column.Count - 1);

            if (oldIndex == index)
                return PbResult.NoChange;

            column.RemoveAt(oldIndex);
            column.Insert(index, task);
            Renumber(column);
            RaiseChanged();

            return PbResult.Ok;
        }

        var from = task.Status;
        var oldColumn = TasksIn(task.ProjectId, task.Status).Where(item => item != task).ToList();
        var newColumn = TasksIn(task.ProjectId, status).ToList();
        newColumn.Insert(Math.Clamp(index, 0, newColumn.Count), task);
        task.Status = status;
        task.CompletedAt = status == PbTaskStatus.Done ? Clock.Now : null;
        Renumber(oldColumn);
        Renumber(newColumn);
        Record(status == PbTaskStatus.Done ? "Task completed" : from == PbTaskStatus.Done ? "Task reopened" : "Task moved", TaskTarget(task), TaskPath(task.Id),
            $"{StatusWord(from)} → {StatusWord(status)}");

        if (status == PbTaskStatus.Review && FindProject(task.ProjectId) is { } project)
        {
            NotifyPerson(project.OwnerId, $"{TaskKey(task)} is ready for review",
                $"{Identity.Current?.Name ?? "Someone"} moved “{task.Title}” to In review.", TaskPath(task.Id));
        }

        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Returns the chance a deal in a stage closes, in percent.</summary>
    public static int Probability(PbDealStage stage) => stage switch
    {
        PbDealStage.Lead => 10,
        PbDealStage.Qualified => 25,
        PbDealStage.Proposal => 50,
        PbDealStage.Negotiation => 75,
        PbDealStage.Won => 100,
        _ => 0,
    };

    /// <summary>Returns how many of a project's tasks are done, and how many it has.</summary>
    public (int Done, int Total) ProjectProgress(int projectId)
    {
        var list = tasks.Where(item => item.ProjectId == projectId).ToList();

        return (list.Count(item => item.Status == PbTaskStatus.Done), list.Count);
    }

    /// <summary>Returns every project's tasks done and total, by project ID, in one pass (for lists).</summary>
    public IReadOnlyDictionary<int, (int Done, int Total)> ProgressByProject() =>
        tasks.GroupBy(task => task.ProjectId).ToDictionary(group => group.Key, group => (group.Count(task => task.Status == PbTaskStatus.Done), group.Count()));

    /// <summary>Assigns a task to one of its project's people, or unassigns it (any signed-in person); the new assignee is told.</summary>
    public PbResult Reassign(int taskId, string? personId)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        if (!IsOpen(task))
            return PbResult.WrongState;

        if (personId is not null && FindProject(task.ProjectId)?.MemberIds.Contains(personId) != true)
            return PbResult.WrongState;

        if (task.AssigneeId == personId)
            return PbResult.NoChange;

        var before = task.AssigneeId;
        task.AssigneeId = personId;
        RecordAssignment(task, before);
        NotifyAssigned(task);
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>
    /// Creates (<paramref name="clientId" /> 0) or edits a client (account managers). Names are unique; the website, billing
    /// email, and phone have their formats; the currency is one of <see cref="PbSeedData.Currencies" />; a contract end needs a
    /// start and falls on or after it. A new owner other than the signed-in person is told.
    /// </summary>
    public PbResult SaveClient(int clientId, PbClientInput input, out PbClient? saved)
    {
        saved = null;

        if (AccountManager() is null)
            return PbResult.NotAllowed;

        var existing = clientId == 0 ? null : FindClient(clientId);

        if (clientId != 0 && existing is null)
            return PbResult.NotFound;

        var name = input?.Name?.Trim() ?? string.Empty;
        var industry = input?.Industry?.Trim() ?? string.Empty;
        var website = input?.Website?.Trim() ?? string.Empty;
        var billingEmail = (input?.BillingEmail ?? string.Empty).Trim().ToLowerInvariant();
        var phone = input?.Phone?.Trim() ?? string.Empty;
        var ownerId = input?.OwnerId ?? string.Empty;

        if (input is null || !IsText(name, MaxClientNameLength) || !IsText(industry, MaxContactFieldLength) || !(website.Length == 0 || IsWebsite(website))
            || !(billingEmail.Length == 0 || IsEmail(billingEmail)) || !(phone.Length == 0 || IsPhone(phone)) || !PbSeedData.Currencies.Contains(input.BillingCurrency)
            || !ValidContract(input.ContractStart, input.ContractEnd) || AccountManagers.All(person => person.Id != ownerId))
            return PbResult.Invalid;

        if (IsClientNameTaken(name, existing?.Id ?? 0))
            return PbResult.Duplicate;

        var start = input.ContractStart?.Date;
        var end = input.ContractEnd?.Date;

        if (existing is null)
        {
            if (clients.Count >= MaxClients || nextClientId >= NextIdLimit)
                return PbResult.Invalid;

            var client = new PbClient
            {
                Id = nextClientId++, Name = name, Industry = industry, Website = website, OwnerId = ownerId, CreatedAt = Clock.Now, BillingCurrency = input.BillingCurrency,
                BillingEmail = billingEmail, Phone = phone, ContractStart = start, ContractEnd = end,
            };
            clients.Add(client);
            var result = Commit(() =>
            {
                clients.Remove(client);
                nextClientId--;
            }, () =>
            {
                Record("Client added", ClientTarget(client.Id), ClientPath(client.Id), $"{client.Industry}, billed in {client.BillingCurrency}, owner {FindPerson(ownerId)?.Name}");
                NotifyOwner(client);
            });

            if (result == PbResult.Ok)
                saved = client;

            return result;
        }

        var before = (existing.Name, existing.Industry, existing.Website, existing.OwnerId, existing.BillingCurrency, existing.BillingEmail, existing.Phone, existing.ContractStart, existing.ContractEnd);
        var after = (name, industry, website, ownerId, input.BillingCurrency, billingEmail, phone, start, end);

        if (before == after)
            return PbResult.NoChange;

        (existing.Name, existing.Industry, existing.Website, existing.OwnerId, existing.BillingCurrency, existing.BillingEmail, existing.Phone, existing.ContractStart, existing.ContractEnd) = after;
        saved = existing;

        return Commit(
            () => (existing.Name, existing.Industry, existing.Website, existing.OwnerId, existing.BillingCurrency, existing.BillingEmail, existing.Phone, existing.ContractStart, existing.ContractEnd) = before,
            () =>
            {
                var changes = new List<string>();

                if (before.Name != name)
                    changes.Add($"name “{before.Name}” → “{name}”");

                if (before.Industry != industry)
                    changes.Add("industry");

                if (before.Website != website)
                    changes.Add("website");

                if (before.OwnerId != ownerId)
                    changes.Add($"owner {FindPerson(before.OwnerId)?.Name ?? "nobody"} → {FindPerson(ownerId)?.Name}");

                if (before.BillingCurrency != input.BillingCurrency)
                    changes.Add($"currency {before.BillingCurrency} → {input.BillingCurrency}");

                if (before.BillingEmail != billingEmail || before.Phone != phone)
                    changes.Add("billing contact details");

                if (before.ContractStart != start || before.ContractEnd != end)
                    changes.Add("contract dates");

                Record("Client changed", ClientTarget(existing.Id), ClientPath(existing.Id), UpperFirst(string.Join(", ", changes)));

                if (before.OwnerId != ownerId)
                    NotifyOwner(existing);
            });
    }

    /// <summary>
    /// Creates (<paramref name="contactId" /> 0) or edits a contact (account managers): an email unique at the client, a role,
    /// and at most one primary contact per client. A contact added by someone other than the client's owner is told to the
    /// owner.
    /// </summary>
    public PbResult SaveContact(int contactId, int clientId, PbContactInput input, out PbContact? saved)
    {
        saved = null;

        if (AccountManager() is null)
            return PbResult.NotAllowed;

        var existing = contactId == 0 ? null : FindContact(contactId);

        if ((contactId != 0 && existing is null) || FindClient(clientId) is not { } client)
            return PbResult.NotFound;

        var name = input?.Name?.Trim() ?? string.Empty;
        var title = input?.Title?.Trim() ?? string.Empty;
        var email = (input?.Email ?? string.Empty).Trim().ToLowerInvariant();
        var phone = input?.Phone?.Trim() ?? string.Empty;
        var isPrimary = input?.IsPrimary ?? false;

        if (input is null || !IsText(name, MaxContactFieldLength) || title.Length > MaxContactFieldLength || !IsEmail(email) || !(phone.Length == 0 || IsPhone(phone))
            || !Enum.IsDefined(input.Role) || (existing is not null && existing.ClientId != clientId))
            return PbResult.Invalid;

        if (IsContactEmailTaken(clientId, email, existing?.Id ?? 0))
            return PbResult.Duplicate;

        var contact = existing ?? new PbContact { Id = nextContactId, ClientId = clientId };

        if (existing is null && (contacts.Count >= MaxContacts || nextContactId >= NextIdLimit))
            return PbResult.Invalid;

        if (existing is not null && existing.Name == name && existing.Title == title && existing.Email == email && existing.Phone == phone && existing.Role == input.Role
            && existing.IsPrimary == isPrimary)
            return PbResult.NoChange;

        var before = contacts.Where(item => item.ClientId == clientId).ToDictionary(item => item.Id, item => item.IsPrimary);
        var previous = (contact.Name, contact.Title, contact.Email, contact.Phone, contact.Role, contact.IsPrimary);

        (contact.Name, contact.Title, contact.Email, contact.Phone, contact.Role, contact.IsPrimary) = (name, title, email, phone, input.Role, isPrimary);

        if (isPrimary)
        {
            foreach (var other in contacts.Where(item => item.ClientId == clientId && item != contact))
                other.IsPrimary = false;
        }

        if (existing is null)
        {
            contacts.Add(contact);
            nextContactId++;
        }

        var result = Commit(() =>
        {
            if (existing is null)
            {
                contacts.Remove(contact);
                nextContactId--;
            }
            else
            {
                (contact.Name, contact.Title, contact.Email, contact.Phone, contact.Role, contact.IsPrimary) = previous;
            }

            foreach (var other in contacts.Where(item => before.ContainsKey(item.Id)))
                other.IsPrimary = before[other.Id];
        }, () =>
        {
            Record(existing is null ? "Contact added" : "Contact changed", ContactTarget(contact), ClientPath(clientId),
                $"{RoleWord(contact.Role)}{(contact.IsPrimary ? ", primary" : null)}, {contact.Email}");

            if (existing is null)
                NotifyPerson(client.OwnerId, $"A contact was added at {client.Name}", $"{Identity.Current?.Name} added {contact.Name} ({RoleWord(contact.Role).ToLowerInvariant()}).", ClientPath(clientId));
        });

        if (result == PbResult.Ok)
            saved = contact;

        return result;
    }

    /// <summary>
    /// Creates (<paramref name="dealId" /> 0, in an open stage, owned by the signed-in account manager) or edits an open deal
    /// (its stage changes on the pipeline). An open deal's expected close date is today or later.
    /// </summary>
    public PbResult SaveDeal(int dealId, PbDealInput input, out PbDeal? saved)
    {
        saved = null;

        if (AccountManager() is not { } person)
            return PbResult.NotAllowed;

        var existing = dealId == 0 ? null : FindDeal(dealId);

        if (dealId != 0 && existing is null)
            return PbResult.NotFound;

        if (existing?.ClosedAt is not null)
            return PbResult.WrongState;

        var title = input?.Title?.Trim() ?? string.Empty;

        if (input is null || FindClient(input.ClientId) is null || !IsText(title, MaxDealTitleLength) || input.Value is < 1 or > MaxDealValue || !IsPlausible(input.CloseDate)
            || input.CloseDate.Date < Clock.Today || (existing is null && input.Stage is not (PbDealStage.Lead or PbDealStage.Qualified or PbDealStage.Proposal or PbDealStage.Negotiation)))
            return PbResult.Invalid;

        if (existing is null)
        {
            if (deals.Count >= MaxDeals || nextDealId >= NextIdLimit)
                return PbResult.Invalid;

            var deal = new PbDeal
            {
                Id = nextDealId++, ClientId = input.ClientId, Title = title, Value = input.Value, CloseDate = input.CloseDate.Date, Stage = input.Stage,
                Position = DealsIn(input.Stage).Count, OwnerId = person.Id, CreatedAt = Clock.Now,
            };
            deals.Add(deal);
            var result = Commit(() =>
            {
                deals.Remove(deal);
                nextDealId--;
            }, () => Record("Deal added", DealTarget(deal), DealPath(), $"{ClientName(deal.ClientId)}, {deal.Stage}, {Dollars(deal.Value)}, closes {Day(deal.CloseDate)}"));

            if (result == PbResult.Ok)
                saved = deal;

            return result;
        }

        if (existing.ClientId == input.ClientId && existing.Title == title && existing.Value == input.Value && existing.CloseDate == input.CloseDate.Date)
            return PbResult.NoChange;

        var previous = (existing.ClientId, existing.Title, existing.Value, existing.CloseDate);
        (existing.ClientId, existing.Title, existing.Value, existing.CloseDate) = (input.ClientId, title, input.Value, input.CloseDate.Date);
        saved = existing;

        return Commit(() => (existing.ClientId, existing.Title, existing.Value, existing.CloseDate) = previous,
            () => Record("Deal changed", DealTarget(existing), DealPath(), $"{Dollars(existing.Value)}, closes {Day(existing.CloseDate)}"));
    }

    /// <summary>Creates (<paramref name="milestoneId" /> 0) or edits a project milestone (project managers).</summary>
    public PbResult SaveMilestone(int projectId, int milestoneId, string title, DateTime due)
    {
        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        if (FindProject(projectId) is not { } project)
            return PbResult.NotFound;

        var existing = milestoneId == 0 ? null : project.Milestones.FirstOrDefault(item => item.Id == milestoneId);

        if (milestoneId != 0 && existing is null)
            return PbResult.NotFound;

        title = title?.Trim() ?? string.Empty;

        if (!IsText(title, MaxMilestoneTitleLength) || !IsPlausible(due) || (existing is null && (project.Milestones.Count >= MaxMilestones || project.NextMilestoneId >= NextIdLimit)))
            return PbResult.Invalid;

        if (project.Milestones.Any(item => item != existing && string.Equals(item.Title, title, StringComparison.OrdinalIgnoreCase)))
            return PbResult.Duplicate;

        if (existing is not null && existing.Title == title && existing.Due == due.Date)
            return PbResult.NoChange;

        var milestone = existing ?? new PbMilestone { Id = project.NextMilestoneId++ };
        var previous = (milestone.Title, milestone.Due);
        (milestone.Title, milestone.Due) = (title, due.Date);

        if (existing is null)
            project.Milestones.Add(milestone);

        project.Milestones.Sort((a, b) => a.Due.CompareTo(b.Due));

        return Commit(() =>
        {
            if (existing is null)
            {
                project.Milestones.Remove(milestone);
                project.NextMilestoneId--;
            }
            else
            {
                (milestone.Title, milestone.Due) = previous;
                project.Milestones.Sort((a, b) => a.Due.CompareTo(b.Due));
            }
        }, () => Record(existing is null ? "Milestone added" : "Milestone changed", ProjectTarget(project), ProjectPath(project.Id), $"{title}, due {Day(due)}"));
    }

    /// <summary>
    /// Creates (<paramref name="projectId" /> 0) or edits a project (project managers). Keys are 2 to 5 capital letters and
    /// unique, and names are unique per client; the project manager is someone who runs projects; the due date is on or
    /// after the start; budgets are within their ranges. Tasks assigned to someone removed from the project become
    /// unassigned. A new project manager and, on a status change, the project's manager are told when someone else did it.
    /// </summary>
    public PbResult SaveProject(int projectId, PbProjectInput input, out PbProject? saved)
    {
        saved = null;

        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        var existing = projectId == 0 ? null : FindProject(projectId);

        if (projectId != 0 && existing is null)
            return PbResult.NotFound;

        var name = input?.Name?.Trim() ?? string.Empty;
        var key = input?.Key?.Trim().ToUpperInvariant() ?? string.Empty;
        var description = input?.Description?.Trim() ?? string.Empty;
        var memberIds = (input?.MemberIds ?? Array.Empty<string>()).Distinct().ToList();

        if (input is null || !IsText(name, MaxProjectNameLength) || !KeyPattern().IsMatch(key) || description.Length > MaxDescriptionLength || !Enum.IsDefined(input.Status)
            || (input.ClientId is { } clientId && FindClient(clientId) is null) || !IsPlausible(input.StartDate) || !IsPlausible(input.DueDate) || input.DueDate.Date < input.StartDate.Date
            || memberIds.Any(id => FindPerson(id) is null) || ProjectManagers.All(person => person.Id != input.OwnerId) || input.BudgetHours is < 0 or > MaxBudgetHours
            || input.BudgetAmount is < 0 or > MaxBudgetAmount)
            return PbResult.Invalid;

        if (IsProjectKeyTaken(key, existing?.Id ?? 0) || IsProjectNameTaken(name, input.ClientId, existing?.Id ?? 0))
            return PbResult.Duplicate;

        // Logged hours and invoices belong to the project's client: a project with hours keeps it.
        if (existing is not null && existing.ClientId != input.ClientId && timeEntries.Any(entry => entry.ProjectId == existing.Id))
            return PbResult.WrongState;

        if (existing is null)
        {
            if (projects.Count >= MaxProjects || nextProjectId >= NextIdLimit)
                return PbResult.Invalid;

            var project = new PbProject { Id = nextProjectId++, CreatedAt = Clock.Now };
            ApplyProject(project, name, key, description, input, memberIds);
            projects.Add(project);
            var result = Commit(() =>
            {
                projects.Remove(project);
                nextProjectId--;
            }, () =>
            {
                Record("Project created", ProjectTarget(project), ProjectPath(project.Id),
                    $"{ProjectStatusWord(project.Status)}, {Day(project.StartDate)} to {Day(project.DueDate)}, managed by {FindPerson(project.OwnerId)?.Name}");
                NotifyPerson(project.OwnerId, $"You're the project manager of {project.Key}", $"{Identity.Current?.Name} created {project.Name}.", ProjectEditPath(project.Id));
            });

            if (result == PbResult.Ok)
                saved = project;

            return result;
        }

        if (existing.Name == name && existing.Key == key && existing.ClientId == input.ClientId && existing.Description == description && existing.Status == input.Status
            && existing.StartDate == input.StartDate.Date && existing.DueDate == input.DueDate.Date && existing.MemberIds.Order().SequenceEqual(memberIds.Order())
            && existing.OwnerId == input.OwnerId && existing.BudgetHours == input.BudgetHours && existing.BudgetAmount == input.BudgetAmount)
            return PbResult.NoChange;

        var before = new PbProjectInput(existing.Name, existing.Key, existing.ClientId, existing.Description, existing.Status, existing.StartDate, existing.DueDate, existing.MemberIds.ToList(),
            existing.OwnerId, existing.BudgetHours, existing.BudgetAmount);
        var unassigned = tasks.Where(task => task.ProjectId == existing.Id && task.AssigneeId is not null && !memberIds.Contains(task.AssigneeId)).ToList();
        var previousAssignees = unassigned.ToDictionary(task => task.Id, task => task.AssigneeId);

        ApplyProject(existing, name, key, description, input, memberIds);

        foreach (var task in unassigned)
            task.AssigneeId = null;

        saved = existing;

        return Commit(() =>
        {
            ApplyProject(existing, before.Name, before.Key, before.Description, before, before.MemberIds.ToList());

            foreach (var task in unassigned)
                task.AssigneeId = previousAssignees[task.Id];
        }, () =>
        {
            var changes = new List<string>();

            if (before.Name != name || before.Key != key)
                changes.Add($"{before.Key} · {before.Name} → {key} · {name}");

            if (before.ClientId != input.ClientId)
                changes.Add($"client {ClientName(before.ClientId)} → {ClientName(input.ClientId)}");

            if (before.StartDate != input.StartDate.Date || before.DueDate != input.DueDate.Date)
                changes.Add($"dates {Day(input.StartDate)} to {Day(input.DueDate)}");

            if (before.BudgetHours != input.BudgetHours || before.BudgetAmount != input.BudgetAmount)
                changes.Add(Invariant($"budget {input.BudgetHours} h, {input.BudgetAmount:#,0}"));

            if (!before.MemberIds.Order().SequenceEqual(memberIds.Order()))
                changes.Add(Invariant($"team of {memberIds.Count}") + (unassigned.Count > 0 ? Invariant($" ({unassigned.Count} task(s) unassigned)") : null));

            if (before.Description != description)
                changes.Add("description");

            if (changes.Count > 0)
                Record("Project changed", ProjectTarget(existing), ProjectPath(existing.Id), UpperFirst(string.Join(", ", changes)));

            TellAboutProjectChange(existing, before.Status, before.OwnerId);
        });
    }

    /// <summary>Ticks or unticks a checklist item (any signed-in person).</summary>
    public PbResult SetChecklistItem(int taskId, int itemId, bool done)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task || task.Checklist.FirstOrDefault(item => item.Id == itemId) is not { } item)
            return PbResult.NotFound;

        if (!IsOpen(task))
            return PbResult.WrongState;

        if (item.Done == done)
            return PbResult.NoChange;

        item.Done = done;
        Record(done ? "Checklist item ticked" : "Checklist item unticked", TaskTarget(task), TaskPath(task.Id), item.Text);
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Records the color mode the theme switcher applied, so charts can redraw in matching colors.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDarkTheme == isDark)
            return;

        isDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Returns a task's key, like "WEB-12".</summary>
    public string TaskKey(PbTask task) => FormattableString.Invariant($"{FindProject(task.ProjectId)?.Key ?? "?"}-{task.Number}");

    /// <summary>Returns a project's tasks, by column and order.</summary>
    public IReadOnlyList<PbTask> TasksOf(int projectId) =>
        tasks.Where(item => item.ProjectId == projectId).OrderBy(item => item.Status).ThenBy(item => item.Position).ThenBy(item => item.Id).ToList();

    /// <summary>Returns the tasks in a project's column, in their order.</summary>
    public IReadOnlyList<PbTask> TasksIn(int projectId, PbTaskStatus status) =>
        tasks.Where(item => item.ProjectId == projectId && item.Status == status).OrderBy(item => item.Position).ThenBy(item => item.Id).ToList();

    /// <summary>
    /// Rebuilds the board from the seed and a stored snapshot's changes, validates every part against itself, and applies it
    /// only when everything is valid: known IDs and references, defined enum values, numbers and dates in range, bounded
    /// text, and links that stay on this site.
    /// </summary>
    public bool TryRestoreData(JsonElement data)
    {
        PbSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<PbSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return false;
        }

        if (snapshot is null
            || new[]
                {
                    snapshot.NextActivityId, snapshot.NextAuditId, snapshot.NextClientId, snapshot.NextContactId, snapshot.NextDealId, snapshot.NextNotificationId, snapshot.NextProjectId, snapshot.NextTaskId,
                    snapshot.NextTimeEntryId, snapshot.NextTimesheetId, snapshot.NextInvoiceId, snapshot.NextInvoiceLineId,
                }
                .Any(id => id is < 1 or > NextIdLimit))
            return false;

        // A fresh copy of the seed: the stored changes replace, remove, and add records on it; an inconsistent change list discards the snapshot.
        var fresh = PbSeedData.Build();

        if (Overlay(fresh.Activities, snapshot.Activities, snapshot.RemovedActivityIds, item => item.Id) is not { Count: <= MaxActivities } storedActivities
            || Overlay(fresh.Audit, snapshot.Audit, snapshot.RemovedAuditIds, entry => entry.Id) is not { Count: <= MaxAudit } storedAudit
            || Overlay(fresh.Clients, snapshot.Clients, snapshot.RemovedClientIds, client => client.Id) is not { Count: <= MaxClients } storedClients
            || Overlay(fresh.Contacts, snapshot.Contacts, snapshot.RemovedContactIds, contact => contact.Id) is not { Count: <= MaxContacts } storedContacts
            || Overlay(fresh.Deals, snapshot.Deals, snapshot.RemovedDealIds, deal => deal.Id) is not { Count: <= MaxDeals } storedDeals
            || Overlay(fresh.Notifications, snapshot.Notifications, snapshot.RemovedNotificationIds, notice => notice.Id) is not { Count: <= MaxNotifications } storedNotifications
            || Overlay(fresh.People, snapshot.People, snapshot.RemovedPersonIds, person => person.Id) is not { Count: > 0 and <= MaxPeople } storedPeople
            || Overlay(fresh.Projects, snapshot.Projects, snapshot.RemovedProjectIds, project => project.Id) is not { Count: <= MaxProjects } storedProjects
            || Overlay(fresh.Tasks, snapshot.Tasks, snapshot.RemovedTaskIds, task => task.Id) is not { Count: <= MaxTasks } storedTasks
            || Overlay(fresh.TimeEntries, snapshot.TimeEntries, snapshot.RemovedTimeEntryIds, entry => entry.Id) is not { Count: <= MaxTimeEntries } storedEntries
            || Overlay(fresh.Timesheets, snapshot.Timesheets, snapshot.RemovedTimesheetIds, sheet => sheet.Id) is not { Count: <= MaxTimesheets } storedSheets
            || Overlay(fresh.Invoices, snapshot.Invoices, snapshot.RemovedInvoiceIds, invoice => invoice.Id) is not { Count: <= MaxInvoices } storedInvoices)
            return false;

        var storedRates = snapshot.Rates ?? fresh.Rates;

        var seededPeople = fresh.People.Select(person => person.Id).ToHashSet();

        // A person with a sign-in has the account's ID; a demo account's or a registered account's person always has its account.
        if (storedPeople.Any(person => person is null || !(seededPeople.Contains(person.Id) || IsAccountId(person.Id)) || (person.AccountId is not null && (person.AccountId != person.Id || !IsAccountId(person.AccountId)))
                || (IsAccountId(person.Id) && person.AccountId != person.Id)
                || !IsText(person.Name, ShowcaseIdentity.MaxNameLength) || !IsText(person.Initials, 3) || !IsText(person.Title, MaxContactFieldLength) || person.CapacityHours is < 0 or > MaxCapacityHours)
            || !Unique(storedPeople.Select(person => person.Id)))
            return false;

        var personIds = storedPeople.Select(person => person.Id).ToHashSet();

        if (storedClients.Any(client => client is null || client.Id < 1 || client.Id >= snapshot.NextClientId || !IsText(client.Name, MaxClientNameLength) || !IsText(client.Industry, MaxContactFieldLength)
                || client.Website is not { } website || !(website.Length == 0 || IsWebsite(website)) || !personIds.Contains(client.OwnerId) || !IsPlausible(client.CreatedAt)
                || client.BillingCurrency is null || !PbSeedData.Currencies.Contains(client.BillingCurrency) || client.BillingEmail is not { } billing || !(billing.Length == 0 || IsEmail(billing))
                || client.Phone is not { } phone || !(phone.Length == 0 || IsPhone(phone)) || !ValidContract(client.ContractStart, client.ContractEnd))
            || !Unique(storedClients.Select(client => client.Id)) || !Unique(storedClients.Select(client => client.Name.Trim().ToLowerInvariant())))
            return false;

        var clientIds = storedClients.Select(client => client.Id).ToHashSet();

        if (storedContacts.Any(contact => contact is null || contact.Id < 1 || contact.Id >= snapshot.NextContactId || !clientIds.Contains(contact.ClientId) || !IsText(contact.Name, MaxContactFieldLength)
                || contact.Title is not { Length: <= MaxContactFieldLength } || !IsEmail(contact.Email) || contact.Phone is not { } phone || !(phone.Length == 0 || IsPhone(phone))
                || !Enum.IsDefined(contact.Role))
            || !Unique(storedContacts.Select(contact => contact.Id)) || !Unique(storedContacts.Select(contact => (contact.ClientId, contact.Email)))
            || storedContacts.GroupBy(contact => contact.ClientId).Any(group => group.Count(contact => contact.IsPrimary) > 1))
            return false;

        if (storedProjects.Any(project => !ValidProject(project, snapshot, clientIds, personIds)) || !Unique(storedProjects.Select(project => project.Id)) || !Unique(storedProjects.Select(project => project.Key))
            || !Unique(storedProjects.Select(project => (project.ClientId, project.Name.Trim().ToLowerInvariant()))))
            return false;

        var projectsById = storedProjects.ToDictionary(project => project.Id);

        if (storedTasks.Any(task => !ValidTask(task, snapshot, projectsById, personIds)) || !Unique(storedTasks.Select(task => task.Id))
            || !Unique(storedTasks.Select(task => (task.ProjectId, task.Number))))
            return false;

        if (storedDeals.Any(deal => deal is null || deal.Id < 1 || deal.Id >= snapshot.NextDealId || !clientIds.Contains(deal.ClientId) || !IsText(deal.Title, MaxDealTitleLength)
                || deal.Value is < 1 or > MaxDealValue || !Enum.IsDefined(deal.Stage) || !IsPlausible(deal.CloseDate) || !IsPlausible(deal.CreatedAt) || !personIds.Contains(deal.OwnerId)
                || deal.Position is < 0 or > MaxDeals || (deal.ClosedAt is null) == (deal.Stage is PbDealStage.Won or PbDealStage.Lost) || (deal.ClosedAt is { } closed && !IsPlausible(closed))
                || (deal.ProjectId is { } projectId && (deal.Stage != PbDealStage.Won || !projectsById.ContainsKey(projectId))))
            || !Unique(storedDeals.Select(deal => deal.Id)) || !Unique(storedDeals.Where(deal => deal.ProjectId is not null).Select(deal => deal.ProjectId)))
            return false;

        if (storedActivities.Any(activity => activity is null || activity.Id < 1 || activity.Id >= snapshot.NextActivityId || !clientIds.Contains(activity.ClientId) || !Enum.IsDefined(activity.Kind)
                || !IsText(activity.Text, MaxActivityLength) || !personIds.Contains(activity.AuthorId) || !IsPlausible(activity.At))
            || !Unique(storedActivities.Select(activity => activity.Id)))
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausible(entry.At) || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength)
                || !IsText(entry.Action, MaxAuditTextLength) || !IsText(entry.Target, MaxAuditTextLength) || (entry.Details is not null && !IsText(entry.Details, MaxAuditTextLength))
                || !IsSiteLink(entry.TargetUrl))
            || !Unique(storedAudit.Select(entry => entry.Id)))
            return false;

        if (storedNotifications.Any(notice => notice is null || notice.Id < 1 || notice.Id >= snapshot.NextNotificationId || !IsAccountId(notice.AccountId) || !IsPlausible(notice.At)
                || !IsText(notice.Title, MaxNoticeTitleLength) || (notice.Text is not null && !IsText(notice.Text, MaxNoticeTextLength)) || !IsSiteLink(notice.Url))
            || !Unique(storedNotifications.Select(notice => notice.Id)))
            return false;

        if (!ValidTimesheetData(storedEntries, storedSheets, storedInvoices, storedRates, snapshot, projectsById, personIds, storedClients.ToDictionary(client => client.Id),
                storedTasks.Select(task => task.Id).ToHashSet(), storedTasks.ToDictionary(task => task.Id, task => task.ProjectId)))
            return false;

        Replace(activities, storedActivities);
        Replace(audit, storedAudit.OrderBy(entry => entry.Id).ToList());
        Replace(timeEntries, storedEntries);
        Replace(timesheets, storedSheets);
        Replace(invoices, storedInvoices);
        Replace(rates, storedRates);
        nextTimeEntryId = snapshot.NextTimeEntryId;
        nextTimesheetId = snapshot.NextTimesheetId;
        nextInvoiceId = snapshot.NextInvoiceId;
        nextInvoiceLineId = snapshot.NextInvoiceLineId;
        Replace(clients, storedClients);
        Replace(contacts, storedContacts);
        Replace(deals, storedDeals);
        Replace(notifications, storedNotifications.OrderByDescending(notice => notice.At).ThenByDescending(notice => notice.Id).ToList());
        Replace(people, storedPeople);
        Replace(projects, storedProjects);
        Replace(tasks, storedTasks);
        nextActivityId = snapshot.NextActivityId;
        nextAuditId = snapshot.NextAuditId;
        nextClientId = snapshot.NextClientId;
        nextContactId = snapshot.NextContactId;
        nextDealId = snapshot.NextDealId;
        nextNotificationId = snapshot.NextNotificationId;
        nextProjectId = snapshot.NextProjectId;
        nextTaskId = snapshot.NextTaskId;

        return true;
    }

    /// <summary>
    /// Saves the signed-in person's work details from their profile: job title (required, up to
    /// <see cref="MaxContactFieldLength" /> characters) and weekly capacity (0 to <see cref="MaxCapacityHours" /> hours).
    /// </summary>
    public PbResult UpdateMyWork(string title, int capacityHours)
    {
        if (Staff() is not { } person)
            return PbResult.NotAllowed;

        title = title?.Trim() ?? string.Empty;

        if (!IsText(title, MaxContactFieldLength) || capacityHours is < 0 or > MaxCapacityHours)
            return PbResult.Invalid;

        if (person.Title == title && person.CapacityHours == capacityHours)
            return PbResult.NoChange;

        var previous = (person.Title, person.CapacityHours);
        (person.Title, person.CapacityHours) = (title, capacityHours);

        return Commit(() => (person.Title, person.CapacityHours) = previous,
            () => Record("Profile updated", person.Name, null, Invariant($"{title}, {capacityHours} h a week")));
    }

    /// <summary>
    /// Saves a task's fields (any signed-in person): the assignee must be on the project, and a due date falls within the
    /// project's dates. A new assignee other than the signed-in person is told.
    /// </summary>
    public PbResult UpdateTask(int taskId, PbTaskInput input)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task || FindProject(task.ProjectId) is not { } project)
            return PbResult.NotFound;

        if (!IsOpen(task))
            return PbResult.WrongState;

        if (!ValidTaskInput(project, input))
            return PbResult.Invalid;

        var current = new PbTaskInput(task.Title, task.Description, task.AssigneeId, task.Priority, task.DueDate, task.EstimateHours, task.MilestoneId);
        var next = input with { Title = input.Title.Trim(), Description = input.Description?.Trim() ?? string.Empty, DueDate = input.DueDate?.Date };

        if (current == next)
            return PbResult.NoChange;

        Apply(task, next);

        return Commit(() => Apply(task, current), () =>
        {
            var changes = new List<string>();

            if (current.Title != next.Title)
                changes.Add("title");

            if (current.Description != next.Description)
                changes.Add("description");

            if (current.Priority != next.Priority)
                changes.Add($"priority {current.Priority} → {next.Priority}");

            if (current.DueDate != next.DueDate)
                changes.Add($"due {(next.DueDate is { } due ? Day(due) : "none")}");

            if (current.EstimateHours != next.EstimateHours)
                changes.Add(Invariant($"estimate {next.EstimateHours} h"));

            if (current.MilestoneId != next.MilestoneId)
                changes.Add("milestone");

            if (changes.Count > 0)
                Record("Task changed", TaskTarget(task), TaskPath(task.Id), UpperFirst(string.Join(", ", changes)));

            if (current.AssigneeId != next.AssigneeId)
            {
                RecordAssignment(task, current.AssigneeId);
                NotifyAssigned(task);
            }
        });
    }

    /// <summary>Returns everyone's open work (not on completed projects), the busiest (this week, against capacity) first.</summary>
    public IReadOnlyList<PbWorkload> Workload()
    {
        var weekEnd = WeekStart(Clock.Today).AddDays(7);
        var openProjects = projects.Where(project => project.Status != PbProjectStatus.Completed).Select(project => project.Id).ToHashSet();
        var openByPerson = tasks.Where(task => task.AssigneeId is not null && task.Status != PbTaskStatus.Done && openProjects.Contains(task.ProjectId)).ToLookup(task => task.AssigneeId!);

        return people.Select(person =>
            {
                var open = openByPerson[person.Id].OrderBy(task => task.DueDate ?? DateTime.MaxValue).ToList();
                var thisWeek = open.Where(task => task.DueDate is { } due && due < weekEnd).Sum(task => task.EstimateHours);

                return new PbWorkload(person, open, thisWeek, open.Sum(task => task.EstimateHours) - thisWeek);
            })
            .OrderByDescending(item => item.Person.CapacityHours == 0 ? item.HoursThisWeek : item.HoursThisWeek / (double)item.Person.CapacityHours)
            .ThenBy(item => item.Person.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Returns a deal's value weighted by the chance it closes.</summary>
    public static int Weighted(PbDeal deal) => (int)((long)deal.Value * Probability(deal.Stage) / 100);

    /// <summary>Returns the Monday of the week a date is in.</summary>
    public static DateTime WeekStart(DateTime date) => date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    #endregion

    #region Private Methods

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength - 1), "…");

    private static string Day(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns the records that differ from the seed (changed or added, in list order) and the keys of seeded records that
    /// were removed; <see langword="null" /> for an empty list, so it is left out of the snapshot.
    /// </summary>
    private static (List<T>? Changed, List<TKey>? Removed) Diff<T, TKey>(IReadOnlyList<T> current, IReadOnlyList<T> seeded, Func<T, TKey> key, Func<T, T, bool> equals)
        where TKey : notnull
    {
        var seededByKey = seeded.ToDictionary(key);
        var currentKeys = new HashSet<TKey>();
        var changed = new List<T>();

        foreach (var item in current)
        {
            var itemKey = key(item);
            currentKeys.Add(itemKey);

            if (!seededByKey.TryGetValue(itemKey, out var original) || !equals(original, item))
                changed.Add(item);
        }

        var removed = seeded.Where(item => !currentKeys.Contains(key(item))).Select(key).ToList();

        return (changed.Count == 0 ? null : changed, removed.Count == 0 ? null : removed);
    }

    private static string Dollars(int value) => value.ToString("$#,0", CultureInfo.InvariantCulture);

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

    private static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return (parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1],
            _ => string.Concat(parts[0][..1], parts[^1][..1]),
        }).ToUpperInvariant();
    }

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (PbSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsPlausible(DateTime value) => value >= PbSeedData.Now.AddYears(-3) && value <= PbSeedData.Now.AddYears(3);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    [GeneratedRegex("^[A-Z]{2,5}$")]
    private static partial Regex KeyPattern();

    /// <summary>
    /// Rebuilds a list from the seed and a snapshot's changes: seeded records in their order (replaced by a stored record
    /// with the same key, left out when removed), then the added records in their stored order. Returns
    /// <see langword="null" /> for an inconsistent change list (a missing record, a repeated key, or a removed key that isn't
    /// seeded), which discards the snapshot.
    /// </summary>
    private static List<T>? Overlay<T, TKey>(IReadOnlyList<T> seeded, List<T>? changed, List<TKey>? removed, Func<T, TKey> key)
        where TKey : notnull
    {
        changed ??= new List<T>();
        removed ??= new List<TKey>();

        if (changed.Count > 20_000 || removed.Count > 20_000 || changed.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var replacements = new Dictionary<TKey, T>();

        foreach (var item in changed)
        {
            if (key(item) is not { } itemKey || !replacements.TryAdd(itemKey, item))
                return null;
        }

        var seededKeys = seeded.Select(key).ToHashSet();
        var removedKeys = removed.ToHashSet();

        if (removedKeys.Count != removed.Count || removedKeys.Any(item => !seededKeys.Contains(item) || replacements.ContainsKey(item)))
            return null;

        var result = new List<T>(seeded.Count + changed.Count);

        foreach (var item in seeded)
        {
            var itemKey = key(item);

            if (replacements.TryGetValue(itemKey, out var replacement))
                result.Add(replacement);
            else if (!removedKeys.Contains(itemKey))
                result.Add(item);
        }

        result.AddRange(changed.Where(item => !seededKeys.Contains(key(item))));

        return result;
    }

    private static void Renumber(IEnumerable<PbTask> column)
    {
        var position = 0;

        foreach (var task in column)
            task.Position = position++;
    }

    private static void Renumber(IEnumerable<PbDeal> column)
    {
        var position = 0;

        foreach (var deal in column)
            deal.Position = position++;
    }

    private static void Replace<T>(List<T> target, List<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private static bool SameAudit(ShowcaseAuditEntry left, ShowcaseAuditEntry right) =>
        left.Id == right.Id && left.At == right.At && left.Actor == right.Actor && left.Action == right.Action && left.Target == right.Target
        && left.TargetUrl == right.TargetUrl && left.Details == right.Details;

    private static bool SameClient(PbClient left, PbClient right) =>
        left.Id == right.Id && left.Name == right.Name && left.Industry == right.Industry && left.Website == right.Website && left.OwnerId == right.OwnerId && left.CreatedAt == right.CreatedAt
        && left.BillingCurrency == right.BillingCurrency && left.BillingEmail == right.BillingEmail && left.Phone == right.Phone && left.ContractStart == right.ContractStart
        && left.ContractEnd == right.ContractEnd;

    private static bool SameContact(PbContact left, PbContact right) =>
        left.Id == right.Id && left.ClientId == right.ClientId && left.Name == right.Name && left.Title == right.Title && left.Email == right.Email && left.Phone == right.Phone
        && left.Role == right.Role && left.IsPrimary == right.IsPrimary;

    private static bool SameEntry(PbTimeEntry left, PbTimeEntry right) =>
        left.Id == right.Id && left.PersonId == right.PersonId && left.ProjectId == right.ProjectId && left.TaskId == right.TaskId && left.Date == right.Date && left.Hours == right.Hours
        && left.IsBillable == right.IsBillable && left.Note == right.Note && left.CreatedAt == right.CreatedAt && left.InvoiceId == right.InvoiceId;

    private static bool SameJson<T>(T left, T right) => JsonSerializer.Serialize(left, snapshotOptions) == JsonSerializer.Serialize(right, snapshotOptions);

    private static bool SameNotification(ShowcaseNotification left, ShowcaseNotification right) =>
        left.Id == right.Id && left.AccountId == right.AccountId && left.At == right.At && left.IsRead == right.IsRead && left.Title == right.Title && left.Text == right.Text
        && left.Url == right.Url;

    private static bool SameProject(PbProject left, PbProject right) =>
        left.Id == right.Id && left.Key == right.Key && left.Name == right.Name && left.ClientId == right.ClientId && left.Description == right.Description && left.Status == right.Status
        && left.StartDate == right.StartDate && left.DueDate == right.DueDate && left.OwnerId == right.OwnerId && left.FromDealId == right.FromDealId && left.CreatedAt == right.CreatedAt
        && left.BudgetHours == right.BudgetHours && left.BudgetAmount == right.BudgetAmount && left.NextMilestoneId == right.NextMilestoneId && left.NextTaskNumber == right.NextTaskNumber
        && left.MemberIds.SequenceEqual(right.MemberIds)
        && left.Milestones.Count == right.Milestones.Count
        && left.Milestones.Zip(right.Milestones).All(pair => pair.First.Id == pair.Second.Id && pair.First.Title == pair.Second.Title && pair.First.Due == pair.Second.Due);

    private static bool SameTask(PbTask left, PbTask right) =>
        left.Id == right.Id && left.ProjectId == right.ProjectId && left.Number == right.Number && left.Title == right.Title && left.Description == right.Description
        && left.Status == right.Status && left.Position == right.Position && left.AssigneeId == right.AssigneeId && left.Priority == right.Priority && left.DueDate == right.DueDate
        && left.EstimateHours == right.EstimateHours && left.MilestoneId == right.MilestoneId && left.CreatedAt == right.CreatedAt && left.CompletedAt == right.CompletedAt
        && left.NextItemId == right.NextItemId
        && left.Checklist.Count == right.Checklist.Count
        && left.Checklist.Zip(right.Checklist).All(pair => pair.First.Id == pair.Second.Id && pair.First.Text == pair.Second.Text && pair.First.Done == pair.Second.Done)
        && left.Comments.Count == right.Comments.Count
        && left.Comments.Zip(right.Comments).All(pair => pair.First.Id == pair.Second.Id && pair.First.Text == pair.Second.Text && pair.First.AuthorId == pair.Second.AuthorId
            && pair.First.At == pair.Second.At);

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    /// <summary>Upper-cases the first letter only, so names and dates inside stay as they are.</summary>
    private static string UpperFirst(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>A contract has no dates, a start only, or a start and an end on or after it, all plausible.</summary>
    private static bool ValidContract(DateTime? start, DateTime? end) =>
        (start is null && end is null) || (start is { } from && IsPlausible(from) && (end is null || (end.Value.Date >= from.Date && IsPlausible(end.Value))));

    private static void ApplyProject(PbProject project, string name, string key, string description, PbProjectInput input, List<string> memberIds)
    {
        project.Name = name;
        project.Key = key;
        project.ClientId = input.ClientId;
        project.Description = description;
        project.Status = input.Status;
        project.StartDate = input.StartDate.Date;
        project.DueDate = input.DueDate.Date;
        project.MemberIds = memberIds.ToList();
        project.OwnerId = input.OwnerId;
        project.BudgetHours = input.BudgetHours;
        project.BudgetAmount = input.BudgetAmount;
    }

    private static void Apply(PbTask task, PbTaskInput input)
    {
        task.Title = input.Title.Trim();
        task.Description = input.Description?.Trim() ?? string.Empty;
        task.AssigneeId = input.AssigneeId;
        task.Priority = input.Priority;
        task.DueDate = input.DueDate?.Date;
        task.EstimateHours = input.EstimateHours;
        task.MilestoneId = input.MilestoneId;
    }

    /// <summary>The signed-in person when they are an account manager, or <see langword="null" />.</summary>
    private PbPerson? AccountManager() => Identity.Current?.RoleId == PbSeedData.AccountManagerRole ? Staff() : null;

    /// <summary>
    /// Raises <see cref="Changed" /> when the snapshot still fits, after <paramref name="committed" /> (the audit entries
    /// and notifications of the change); otherwise undoes the change and reports Invalid.
    /// </summary>
    private PbResult Commit(Action undo, Action? committed = null)
    {
        if (!FitsSnapshot())
        {
            undo();

            return PbResult.Invalid;
        }

        committed?.Invoke();
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>The project manager new projects from won deals go to: a project manager with an account, else the demo one.</summary>
    private string DefaultProjectManagerId()
    {
        return people.FirstOrDefault(person => Identity.FindAccount(person.AccountId)?.RoleId == PbSeedData.ProjectManagerRole)?.Id
            ?? people.FirstOrDefault(person => person.Id == PbSeedData.SeededProjectManagerId)?.Id
            ?? Staff()!.Id;
    }

    /// <summary>Whether the snapshot, as it would be saved now (plus <paramref name="reserve" /> characters), still fits under <see cref="MaxSnapshotLength" />.</summary>
    private bool FitsSnapshot(int reserve = 0)
    {
        try
        {
            return JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length + reserve <= MaxSnapshotLength;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>A project name that is free at the client: the name itself, or with " (2)", " (3)", and so on.</summary>
    private string FreeProjectName(string name, int? clientId)
    {
        if (!IsProjectNameTaken(name, clientId))
            return name;

        for (var number = 2; ; number++)
        {
            var tail = Invariant($" ({number})");
            var candidate = name[..Math.Min(name.Length, MaxProjectNameLength - tail.Length)] + tail;

            if (!IsProjectNameTaken(candidate, clientId))
                return candidate;
        }
    }

    /// <summary>A new account joins the team as a person; a person's name follows their profile. The person hears about tasks due soon.</summary>
    private void OnIdentityChanged()
    {
        if (Identity.Current is { } account)
        {
            var name = Clip(account.Name.Trim(), ShowcaseIdentity.MaxNameLength);

            if (people.FirstOrDefault(person => person.AccountId == account.Id) is { } person)
            {
                person.Name = name;
                person.Initials = Initials(name);
            }
            else if (people.Count < MaxPeople && people.All(item => item.Id != account.Id))
            {
                people.Add(new PbPerson { Id = account.Id, AccountId = account.Id, Name = name, Initials = Initials(name), Title = "Team member", CapacityHours = 32 });
            }

            RemindDueTasks();
            RemindOverdueInvoices();
        }

        Changed?.Invoke();
    }

    /// <summary>Whether a task's project still takes changes (it isn't completed).</summary>
    private bool IsOpen(PbTask task) => FindProject(task.ProjectId)?.Status != PbProjectStatus.Completed;

    /// <summary>The signed-in project manager, or <see langword="null" />.</summary>
    private PbPerson? ProjectManager() => Identity.Current?.RoleId == PbSeedData.ProjectManagerRole ? Staff() : null;

    /// <summary>Raises <see cref="Changed" />, or once at the end of a bulk change.</summary>
    private void RaiseChanged()
    {
        if (batchDepth > 0)
        {
            changedInBatch = true;

            return;
        }

        Changed?.Invoke();
    }

    /// <summary>Sets the ID counters past the highest IDs of a fresh tab's data.</summary>
    private void ResetCounters(PbSeedSet data)
    {
        nextActivityId = data.Activities.Max(item => item.Id) + 1;
        nextAuditId = data.Audit.Count == 0 ? 1 : data.Audit.Max(item => item.Id) + 1;
        nextClientId = data.Clients.Max(item => item.Id) + 1;
        nextContactId = data.Contacts.Max(item => item.Id) + 1;
        nextDealId = data.Deals.Max(item => item.Id) + 1;
        nextNotificationId = data.Notifications.Count == 0 ? 1 : data.Notifications.Max(item => item.Id) + 1;
        nextProjectId = data.Projects.Max(item => item.Id) + 1;
        nextTaskId = data.Tasks.Max(item => item.Id) + 1;
        nextTimeEntryId = data.TimeEntries.Count == 0 ? 1 : data.TimeEntries.Max(item => item.Id) + 1;
        nextTimesheetId = data.Timesheets.Count == 0 ? 1 : data.Timesheets.Max(item => item.Id) + 1;
        nextInvoiceId = data.Invoices.Count == 0 ? 1 : data.Invoices.Max(item => item.Id) + 1;
        nextInvoiceLineId = data.Invoices.SelectMany(item => item.Lines).Select(line => line.Id).DefaultIfEmpty(0).Max() + 1;
    }

    /// <summary>The signed-in person (any role), or <see langword="null" />.</summary>
    private PbPerson? Staff() => Identity.Current is { } account ? people.FirstOrDefault(person => person.AccountId == account.Id) : null;

    /// <summary>A project key from a name: its initials, made unique with a letter; <see langword="null" /> when none is free.</summary>
    private string? UniqueKey(string name)
    {
        var letters = new string(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word => word.FirstOrDefault(char.IsLetter)).Where(char.IsLetter)
            .Select(char.ToUpperInvariant).Where(letter => letter is >= 'A' and <= 'Z').Take(4).ToArray());
        var key = letters.Length >= 2 ? letters : "PRJ";

        if (projects.All(project => project.Key != key))
            return key;

        var stem = key.Length >= 5 ? key[..4] : key;

        for (var letter = 'A'; letter <= 'Z'; letter++)
        {
            var candidate = stem + letter;

            if (projects.All(project => project.Key != candidate))
                return candidate;
        }

        return null;
    }

    private bool ValidProject(PbProject? project, PbSnapshot snapshot, HashSet<int> clientIds, HashSet<string> personIds) =>
        project is not null && project.Id >= 1 && project.Id < snapshot.NextProjectId && IsText(project.Name, MaxProjectNameLength) && project.Key is not null && KeyPattern().IsMatch(project.Key)
        && (project.ClientId is null || clientIds.Contains(project.ClientId.Value)) && project.Description is { Length: <= MaxDescriptionLength } && Enum.IsDefined(project.Status)
        && IsPlausible(project.StartDate) && IsPlausible(project.DueDate) && project.DueDate >= project.StartDate && IsPlausible(project.CreatedAt) && personIds.Contains(project.OwnerId)
        && project.BudgetHours is >= 0 and <= MaxBudgetHours && project.BudgetAmount is >= 0 and <= MaxBudgetAmount
        && project.MemberIds is { Count: <= MaxPeople } && project.MemberIds.All(id => id is not null && personIds.Contains(id)) && Unique(project.MemberIds)
        && project.Milestones is { Count: <= MaxMilestones } && project.NextMilestoneId is >= 1 and < NextIdLimit && project.NextTaskNumber is >= 1 and <= NextIdLimit
        && project.Milestones.All(milestone => milestone is not null && milestone.Id >= 1 && milestone.Id < project.NextMilestoneId && IsText(milestone.Title, MaxMilestoneTitleLength) && IsPlausible(milestone.Due))
        && Unique(project.Milestones.Select(milestone => milestone.Id));

    private static bool ValidTask(PbTask? task, PbSnapshot snapshot, Dictionary<int, PbProject> projectsById, HashSet<string> personIds)
    {
        if (task is null || task.Id < 1 || task.Id >= snapshot.NextTaskId || !projectsById.TryGetValue(task.ProjectId, out var project) || task.Number < 1 || task.Number >= project.NextTaskNumber
            || !IsText(task.Title, MaxTaskTitleLength) || task.Description is not { Length: <= MaxDescriptionLength } || !Enum.IsDefined(task.Status) || !Enum.IsDefined(task.Priority)
            || task.Position is < 0 or > MaxTasks || task.EstimateHours is < 0 or > MaxEstimateHours || (task.AssigneeId is not null && !personIds.Contains(task.AssigneeId))
            || (task.MilestoneId is { } milestoneId && project.Milestones.All(milestone => milestone.Id != milestoneId)) || (task.DueDate is { } due && !IsPlausible(due))
            || !IsPlausible(task.CreatedAt) || (task.CompletedAt is null) == (task.Status == PbTaskStatus.Done) || (task.CompletedAt is { } completed && !IsPlausible(completed))
            || task.NextItemId is < 1 or >= NextIdLimit || task.Checklist is not { Count: <= MaxChecklistItems } || task.Comments is not { Count: <= MaxComments })
            return false;

        return task.Checklist.All(item => item is not null && item.Id >= 1 && item.Id < task.NextItemId && IsText(item.Text, MaxChecklistTextLength))
            && task.Comments.All(comment => comment is not null && comment.Id >= 1 && comment.Id < task.NextItemId && IsText(comment.Text, MaxCommentLength) && personIds.Contains(comment.AuthorId) && IsPlausible(comment.At))
            && Unique(task.Checklist.Select(item => item.Id).Concat(task.Comments.Select(comment => comment.Id)));
    }

    /// <summary>
    /// The state's rules for a task's fields: a title, bounded text, a defined priority, an assignee on the project, an
    /// estimate in range, a milestone of the project, and a due date (when set) within the project's start and due dates.
    /// </summary>
    private bool ValidTaskInput(PbProject project, PbTaskInput? input) =>
        input is not null && IsText(input.Title?.Trim(), MaxTaskTitleLength) && (input.Description?.Trim().Length ?? 0) <= MaxDescriptionLength && Enum.IsDefined(input.Priority)
        && (input.AssigneeId is null || project.MemberIds.Contains(input.AssigneeId)) && input.EstimateHours is >= 0 and <= MaxEstimateHours
        && (input.MilestoneId is null || project.Milestones.Any(milestone => milestone.Id == input.MilestoneId))
        && (input.DueDate is null || (IsPlausible(input.DueDate.Value) && IsDueInProject(project, input.DueDate.Value)));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the people who can own clients and deals: account managers.</summary>
    public IReadOnlyList<PbPerson> AccountManagers
    {
        get => people.Where(person => person.AccountId is not null
                ? Identity.FindAccount(person.AccountId)?.RoleId == PbSeedData.AccountManagerRole
                : person.Title == PbSeedData.AccountManagerTitle)
            .OrderBy(person => person.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Gets every client activity entry, newest first.</summary>
    public IReadOnlyList<PbActivity> Activities => activities.OrderByDescending(item => item.At).ThenByDescending(item => item.Id).ToList();

    /// <summary>Gets every task (for reports and workload).</summary>
    public IReadOnlyList<PbTask> AllTasks => tasks.ToList();

    /// <summary>Gets the contacts of every client (for lists that need them all at once).</summary>
    public IReadOnlyList<PbContact> AllContacts => contacts.ToList();

    /// <summary>Gets the clients, by name.</summary>
    public IReadOnlyList<PbClient> Clients => clients.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(PbSeedData.Now);

    /// <summary>Gets the signed-in person, or <see langword="null" />.</summary>
    public PbPerson? CurrentPerson => Staff();

    /// <summary>Gets every deal, by stage and order.</summary>
    public IReadOnlyList<PbDeal> Deals => deals.OrderBy(item => item.Stage).ThenBy(item => item.Position).ToList();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the signed-in person is an account manager.</summary>
    public bool IsAccountManager => AccountManager() is not null;

    /// <summary>Gets whether charts should use dark-theme colors.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets whether the signed-in person is a project manager.</summary>
    public bool IsProjectManager => ProjectManager() is not null;

    /// <summary>
    /// Gets the signed-in person's tasks: open ones (not on completed projects) first by due date (no date last), then the
    /// ones done in the last 14 days, most recent first.
    /// </summary>
    public IReadOnlyList<PbTask> MyTasks
    {
        get
        {
            if (Staff() is not { } person)
                return Array.Empty<PbTask>();

            // A completed project's open tasks are read-only, so they leave the person's list.
            var mine = tasks.Where(task => task.AssigneeId == person.Id && (task.Status == PbTaskStatus.Done || IsOpen(task))).ToList();

            return mine.Where(task => task.Status != PbTaskStatus.Done).OrderBy(task => task.DueDate ?? DateTime.MaxValue).ThenByDescending(task => task.Priority)
                .Concat(mine.Where(task => task.CompletedAt >= Clock.Today.AddDays(-14)).OrderByDescending(task => task.CompletedAt))
                .ToList();
        }
    }

    /// <summary>Gets the team.</summary>
    public IReadOnlyList<PbPerson> People => people.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the people who can run projects: project managers with a sign-in, and teammates with that job title.</summary>
    public IReadOnlyList<PbPerson> ProjectManagers
    {
        get => people.Where(person => person.AccountId is not null
                ? Identity.FindAccount(person.AccountId)?.RoleId == PbSeedData.ProjectManagerRole
                : person.Title == PbSeedData.ProjectManagerTitle)
            .OrderBy(person => person.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Gets the projects: active, then planned, on hold, and completed; by due date within each.</summary>
    public IReadOnlyList<PbProject> Projects =>
        projects.OrderBy(item => item.Status switch { PbProjectStatus.Active => 0, PbProjectStatus.Planned => 1, PbProjectStatus.OnHold => 2, _ => 3 }).ThenBy(item => item.DueDate).ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "project-board";

    /// <summary>
    /// Gets the snapshot format version: 2 stores only what differs from the seed, and adds the audit log, notifications,
    /// client billing fields, contact roles, and project budgets; 3 adds time entries, timesheets, invoices, and the rate card.
    /// </summary>
    public int SnapshotVersion => 3;

    #endregion

    #region Events

    /// <summary>Raised after any change, so open pages re-render and the session store saves.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes, so charts can redraw.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Other

    /// <summary>
    /// The stored form of the state: the ID counters and, of each list, the records that differ from the seed (changed or
    /// added) and the IDs of seeded records that were removed; it is rebuilt over the seed on restore. Empty change lists are
    /// left out of the JSON.
    /// </summary>
    private sealed class PbSnapshot
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbActivity>? Activities { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbClient>? Clients { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbContact>? Contacts { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbDeal>? Deals { get; set; }

        public int NextActivityId { get; set; }

        public int NextAuditId { get; set; }

        public int NextClientId { get; set; }

        public int NextContactId { get; set; }

        public int NextDealId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextProjectId { get; set; }

        public int NextTaskId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseNotification>? Notifications { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbPerson>? People { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbProject>? Projects { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedActivityIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAuditIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedClientIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedContactIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedDealIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedNotificationIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedPersonIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedProjectIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedTaskIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbTask>? Tasks { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbInvoice>? Invoices { get; set; }

        public int NextInvoiceId { get; set; }

        public int NextInvoiceLineId { get; set; }

        public int NextTimeEntryId { get; set; }

        public int NextTimesheetId { get; set; }

        /// <summary>The rate card, only when it differs from the seed's.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbRate>? Rates { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedInvoiceIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedTimeEntryIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedTimesheetIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbTimeEntry>? TimeEntries { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PbTimesheet>? Timesheets { get; set; }
    }

    #endregion
}
