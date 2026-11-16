using System.Text.Json;
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
/// with milestones and tasks, clients and contacts, the deal pipeline, client activity, and the demo sign-in. It is kept in
/// the tab's <c>sessionStorage</c> through the showcase foundation; nothing is sent anywhere.
/// </summary>
/// <remarks>
/// Rights come from the identity role: every signed-in person works tasks on the project boards (create, edit, move,
/// comment, checklists); a project manager also creates and edits projects and milestones, deletes tasks, and sees the
/// workload, roadmap, and reports; an account manager also manages clients, contacts, deals, and activity. Moving a deal to
/// Won creates a planned project for the project managers. Tasks of a completed project are read-only: moving, editing,
/// reassigning, and checklist changes return <see cref="PbResult.WrongState" />; comments are still allowed.
/// </remarks>
public sealed partial class PbState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The longest activity text.</summary>
    public const int MaxActivityLength = 1000;

    /// <summary>The most activity entries.</summary>
    public const int MaxActivities = 400;

    /// <summary>The most checklist items on a task.</summary>
    public const int MaxChecklistItems = 20;

    /// <summary>The longest checklist item.</summary>
    public const int MaxChecklistTextLength = 120;

    /// <summary>The most clients.</summary>
    public const int MaxClients = 60;

    /// <summary>The longest client name.</summary>
    public const int MaxClientNameLength = 80;

    /// <summary>The longest comment.</summary>
    public const int MaxCommentLength = 1000;

    /// <summary>The most comments on a task.</summary>
    public const int MaxComments = 50;

    /// <summary>The most contacts.</summary>
    public const int MaxContacts = 200;

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

    /// <summary>The most milestones on a project.</summary>
    public const int MaxMilestones = 10;

    /// <summary>The longest milestone title.</summary>
    public const int MaxMilestoneTitleLength = 60;

    /// <summary>The most people on the team: the seeded team plus every account the demo sign-in allows.</summary>
    public const int MaxPeople = 10 + ShowcaseIdentity.MaxAccounts;

    /// <summary>The longest phone number.</summary>
    public const int MaxPhoneLength = 30;

    /// <summary>The longest project name.</summary>
    public const int MaxProjectNameLength = 80;

    /// <summary>The most projects.</summary>
    public const int MaxProjects = 30;

    /// <summary>The longest task title.</summary>
    public const int MaxTaskTitleLength = 120;

    /// <summary>The most tasks.</summary>
    public const int MaxTasks = 400;

    /// <summary>The longest client website.</summary>
    public const int MaxWebsiteLength = 80;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int NextIdLimit = 1_000_000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<PbActivity> activities = PbSeedData.Activities();

    private readonly List<PbClient> clients = PbSeedData.Clients();

    private readonly List<PbContact> contacts = PbSeedData.Contacts();

    private readonly List<PbDeal> deals = PbSeedData.Deals();

    private readonly List<PbPerson> people = PbSeedData.People();

    private readonly List<PbProject> projects = PbSeedData.Projects();

    private readonly List<PbTask> tasks = PbSeedData.Tasks();

    private bool isDarkTheme;

    private int nextActivityId;

    private int nextClientId;

    private int nextContactId;

    private int nextDealId;

    private int nextProjectId;

    private int nextTaskId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public PbState()
    {
        nextActivityId = activities.Max(item => item.Id) + 1;
        nextClientId = clients.Max(item => item.Id) + 1;
        nextContactId = contacts.Max(item => item.Id) + 1;
        nextDealId = deals.Max(item => item.Id) + 1;
        nextProjectId = projects.Max(item => item.Id) + 1;
        nextTaskId = tasks.Max(item => item.Id) + 1;

        foreach (var project in projects)
            project.NextTaskNumber = tasks.Where(task => task.ProjectId == project.Id).Select(task => task.Number).DefaultIfEmpty(0).Max() + 1;

        Identity = new ShowcaseIdentity(PbSeedData.Roles(), PbSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns a client's activity, newest first.</summary>
    public IReadOnlyList<PbActivity> ActivitiesOf(int clientId) => activities.Where(item => item.ClientId == clientId).OrderByDescending(item => item.At).ThenByDescending(item => item.Id).ToList();

    /// <summary>Adds a checklist item to a task (any signed-in person).</summary>
    public PbResult AddChecklistItem(int taskId, string text)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        if (!IsOpen(FindTask(taskId)!))
            return PbResult.WrongState;

        text = text?.Trim() ?? string.Empty;

        if (!IsText(text, MaxChecklistTextLength) || task.Checklist.Count >= MaxChecklistItems || task.NextItemId >= NextIdLimit)
            return PbResult.Invalid;

        var item = new PbChecklistItem { Id = task.NextItemId++, Text = text };
        task.Checklist.Add(item);

        return Commit(() =>
        {
            task.Checklist.Remove(item);
            task.NextItemId--;
        });
    }

    /// <summary>Adds a comment to a task as the signed-in person (plain text).</summary>
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
        });
    }

    /// <summary>Returns the snapshot: every collection and the ID counters.</summary>
    public object CaptureData() => new PbSnapshot
    {
        Activities = activities.ToList(),
        Clients = clients.ToList(),
        Contacts = contacts.ToList(),
        Deals = deals.ToList(),
        NextActivityId = nextActivityId,
        NextClientId = nextClientId,
        NextContactId = nextContactId,
        NextDealId = nextDealId,
        NextProjectId = nextProjectId,
        NextTaskId = nextTaskId,
        People = people.ToList(),
        Projects = projects.ToList(),
        Tasks = tasks.ToList(),
    };

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

    /// <summary>Creates a task at the end of a column (any signed-in person; not on a completed project).</summary>
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
        Changed?.Invoke();

        return PbResult.Ok;
    }

    /// <summary>Removes a checklist item (any signed-in person).</summary>
    public PbResult DeleteChecklistItem(int taskId, int itemId)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId)?.Checklist.FirstOrDefault(item => item.Id == itemId) is not { } item)
            return PbResult.NotFound;

        if (!IsOpen(FindTask(taskId)!))
            return PbResult.WrongState;

        FindTask(taskId)!.Checklist.Remove(item);
        Changed?.Invoke();

        return PbResult.Ok;
    }

    /// <summary>Deletes a client (account managers): only one with no projects and no deals; its contacts and activity go with it.</summary>
    public PbResult DeleteClient(int clientId)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (FindClient(clientId) is not { } client)
            return PbResult.NotFound;

        if (projects.Any(item => item.ClientId == clientId) || deals.Any(item => item.ClientId == clientId))
            return PbResult.WrongState;

        clients.Remove(client);
        contacts.RemoveAll(item => item.ClientId == clientId);
        activities.RemoveAll(item => item.ClientId == clientId);
        Changed?.Invoke();

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
        Changed?.Invoke();

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
        Changed?.Invoke();

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
        Changed?.Invoke();

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

        Changed?.Invoke();

        return PbResult.Ok;
    }

    /// <summary>Deletes a project and its tasks (project managers); a deal it came from stays won.</summary>
    public PbResult DeleteProject(int projectId)
    {
        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        if (FindProject(projectId) is not { } project)
            return PbResult.NotFound;

        projects.Remove(project);
        tasks.RemoveAll(item => item.ProjectId == projectId);

        foreach (var deal in deals.Where(item => item.ProjectId == projectId))
            deal.ProjectId = null;

        Changed?.Invoke();

        return PbResult.Ok;
    }

    /// <summary>Deletes a task (project managers).</summary>
    public PbResult DeleteTask(int taskId)
    {
        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        tasks.Remove(task);
        Renumber(TasksIn(task.ProjectId, task.Status));
        Changed?.Invoke();

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
    /// the client's deals.
    /// </summary>
    public PbResult LogActivity(int clientId, PbActivityKind kind, string text, int? dealId, out PbActivity? logged)
    {
        logged = null;

        if (AccountManager() is not { } person)
            return PbResult.NotAllowed;

        if (FindClient(clientId) is null)
            return PbResult.NotFound;

        text = text?.Trim() ?? string.Empty;

        if (kind is not (PbActivityKind.Note or PbActivityKind.Call or PbActivityKind.Email or PbActivityKind.Meeting) || !IsText(text, MaxActivityLength)
            || (dealId is { } id && FindDeal(id)?.ClientId != clientId) || activities.Count >= MaxActivities || nextActivityId >= NextIdLimit)
            return PbResult.Invalid;

        var activity = new PbActivity { Id = nextActivityId++, ClientId = clientId, Kind = kind, Text = text, DealId = dealId, AuthorId = person.Id, At = Clock.Now };
        activities.Add(activity);
        var result = Commit(() =>
        {
            activities.Remove(activity);
            nextActivityId--;
        });

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
    /// <paramref name="createdProject" />) and logs it; Lost closes the deal. A won deal whose project exists can't leave Won.
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
            Changed?.Invoke();

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
                Id = nextProjectId++, Key = key, Name = Clip(deal.Title, MaxProjectNameLength), ClientId = deal.ClientId,
                Status = PbProjectStatus.Planned, StartDate = start, DueDate = start.AddDays(7 * 8), OwnerId = DefaultProjectManagerId(), FromDealId = deal.Id, CreatedAt = Clock.Now,
                Description = $"Created from the won deal “{deal.Title}”. Plan the scope, team, and milestones.",
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
                ? $"Lost “{deal.Title}” (${deal.Value:#,0})."
                : project is not null ? $"Won “{deal.Title}” (${deal.Value:#,0}). Project {project.Key} created for the project managers." : $"Won “{deal.Title}” (${deal.Value:#,0}).";
            activities.Add(new PbActivity { Id = nextActivityId++, ClientId = deal.ClientId, DealId = deal.Id, Kind = PbActivityKind.Deal, AuthorId = person.Id, At = Clock.Now, Text = text });
        }

        createdProject = project;
        Changed?.Invoke();

        return PbResult.Ok;
    }

    /// <summary>
    /// Moves a task to a column and position (any signed-in person): drag and drop on the board, or the "Move to" menu.
    /// Moving to Done records when it was finished; moving out of Done clears it.
    /// </summary>
    public PbResult MoveTask(int taskId, PbTaskStatus status, int index)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        if (!IsOpen(FindTask(taskId)!))
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
            Changed?.Invoke();

            return PbResult.Ok;
        }

        var oldColumn = TasksIn(task.ProjectId, task.Status).Where(item => item != task).ToList();
        var newColumn = TasksIn(task.ProjectId, status).ToList();
        newColumn.Insert(Math.Clamp(index, 0, newColumn.Count), task);
        task.Status = status;
        task.CompletedAt = status == PbTaskStatus.Done ? Clock.Now : null;
        Renumber(oldColumn);
        Renumber(newColumn);
        Changed?.Invoke();

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

    /// <summary>Assigns a task to one of its project's people, or unassigns it (any signed-in person).</summary>
    public PbResult Reassign(int taskId, string? personId)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task)
            return PbResult.NotFound;

        if (!IsOpen(FindTask(taskId)!))
            return PbResult.WrongState;

        if (personId is not null && FindProject(task.ProjectId)?.MemberIds.Contains(personId) != true)
            return PbResult.WrongState;

        if (task.AssigneeId == personId)
            return PbResult.NoChange;

        task.AssigneeId = personId;
        Changed?.Invoke();

        return PbResult.Ok;
    }

    /// <summary>Creates (<paramref name="clientId" /> 0) or edits a client (account managers); names are unique.</summary>
    public PbResult SaveClient(int clientId, string name, string industry, string website, string ownerId, out PbClient? saved)
    {
        saved = null;

        if (AccountManager() is null)
            return PbResult.NotAllowed;

        var existing = clientId == 0 ? null : FindClient(clientId);

        if (clientId != 0 && existing is null)
            return PbResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        industry = industry?.Trim() ?? string.Empty;
        website = website?.Trim() ?? string.Empty;

        if (!IsText(name, MaxClientNameLength) || !IsText(industry, MaxContactFieldLength) || website.Length > MaxWebsiteLength || AccountManagers.All(person => person.Id != ownerId))
            return PbResult.Invalid;

        if (clients.Any(item => item != existing && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
            return PbResult.Duplicate;

        if (existing is null)
        {
            if (clients.Count >= MaxClients || nextClientId >= NextIdLimit)
                return PbResult.Invalid;

            var client = new PbClient { Id = nextClientId++, Name = name, Industry = industry, Website = website, OwnerId = ownerId, CreatedAt = Clock.Now };
            clients.Add(client);
            var result = Commit(() =>
            {
                clients.Remove(client);
                nextClientId--;
            });

            if (result == PbResult.Ok)
                saved = client;

            return result;
        }

        if (existing.Name == name && existing.Industry == industry && existing.Website == website && existing.OwnerId == ownerId)
            return PbResult.NoChange;

        var previous = (existing.Name, existing.Industry, existing.Website, existing.OwnerId);
        (existing.Name, existing.Industry, existing.Website, existing.OwnerId) = (name, industry, website, ownerId);
        saved = existing;

        return Commit(() => (existing.Name, existing.Industry, existing.Website, existing.OwnerId) = previous);
    }

    /// <summary>Creates (<paramref name="contactId" /> 0) or edits a contact (account managers); a client has at most one primary contact.</summary>
    public PbResult SaveContact(int contactId, int clientId, string name, string title, string email, string phone, bool isPrimary, out PbContact? saved)
    {
        saved = null;

        if (AccountManager() is null)
            return PbResult.NotAllowed;

        var existing = contactId == 0 ? null : FindContact(contactId);

        if ((contactId != 0 && existing is null) || FindClient(clientId) is null)
            return PbResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        title = title?.Trim() ?? string.Empty;
        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        phone = phone?.Trim() ?? string.Empty;

        if (!IsText(name, MaxContactFieldLength) || title.Length > MaxContactFieldLength || !IsEmail(email) || phone.Length > MaxPhoneLength || (existing is not null && existing.ClientId != clientId))
            return PbResult.Invalid;

        if (contacts.Any(item => item != existing && item.ClientId == clientId && item.Email == email))
            return PbResult.Duplicate;

        var contact = existing ?? new PbContact { Id = nextContactId, ClientId = clientId };

        if (existing is null && (contacts.Count >= MaxContacts || nextContactId >= NextIdLimit))
            return PbResult.Invalid;

        if (existing is not null && existing.Name == name && existing.Title == title && existing.Email == email && existing.Phone == phone && existing.IsPrimary == isPrimary)
            return PbResult.NoChange;

        var before = contacts.Where(item => item.ClientId == clientId).ToDictionary(item => item.Id, item => item.IsPrimary);
        var previous = (contact.Name, contact.Title, contact.Email, contact.Phone, contact.IsPrimary);

        (contact.Name, contact.Title, contact.Email, contact.Phone, contact.IsPrimary) = (name, title, email, phone, isPrimary);

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
                (contact.Name, contact.Title, contact.Email, contact.Phone, contact.IsPrimary) = previous;
            }

            foreach (var other in contacts.Where(item => before.ContainsKey(item.Id)))
                other.IsPrimary = before[other.Id];
        });

        if (result == PbResult.Ok)
            saved = contact;

        return result;
    }

    /// <summary>Creates (<paramref name="dealId" /> 0, as a Lead owned by the signed-in account manager) or edits an open deal.</summary>
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

        if (input is null || FindClient(input.ClientId) is null || !IsText(title, MaxDealTitleLength) || input.Value is < 1 or > MaxDealValue || !IsPlausible(input.CloseDate))
            return PbResult.Invalid;

        if (existing is null)
        {
            if (deals.Count >= MaxDeals || nextDealId >= NextIdLimit)
                return PbResult.Invalid;

            var deal = new PbDeal
            {
                Id = nextDealId++, ClientId = input.ClientId, Title = title, Value = input.Value, CloseDate = input.CloseDate.Date, Stage = PbDealStage.Lead,
                Position = DealsIn(PbDealStage.Lead).Count, OwnerId = person.Id, CreatedAt = Clock.Now,
            };
            deals.Add(deal);
            var result = Commit(() =>
            {
                deals.Remove(deal);
                nextDealId--;
            });

            if (result == PbResult.Ok)
                saved = deal;

            return result;
        }

        if (existing.ClientId == input.ClientId && existing.Title == title && existing.Value == input.Value && existing.CloseDate == input.CloseDate.Date)
            return PbResult.NoChange;

        var previous = (existing.ClientId, existing.Title, existing.Value, existing.CloseDate);
        (existing.ClientId, existing.Title, existing.Value, existing.CloseDate) = (input.ClientId, title, input.Value, input.CloseDate.Date);
        saved = existing;

        return Commit(() => (existing.ClientId, existing.Title, existing.Value, existing.CloseDate) = previous);
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
        });
    }

    /// <summary>
    /// Creates (<paramref name="projectId" /> 0, owned by the signed-in project manager) or edits a project (project
    /// managers). Keys are 2 to 5 capital letters and unique. Tasks assigned to someone removed from the project become
    /// unassigned.
    /// </summary>
    public PbResult SaveProject(int projectId, PbProjectInput input, out PbProject? saved)
    {
        saved = null;

        if (ProjectManager() is not { } person)
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
            || memberIds.Any(id => FindPerson(id) is null))
            return PbResult.Invalid;

        if (projects.Any(item => item != existing && item.Key == key))
            return PbResult.Duplicate;

        if (existing is null)
        {
            if (projects.Count >= MaxProjects || nextProjectId >= NextIdLimit)
                return PbResult.Invalid;

            var project = new PbProject { Id = nextProjectId++, OwnerId = person.Id, CreatedAt = Clock.Now };
            ApplyProject(project, name, key, description, input, memberIds);
            projects.Add(project);
            var result = Commit(() =>
            {
                projects.Remove(project);
                nextProjectId--;
            });

            if (result == PbResult.Ok)
                saved = project;

            return result;
        }

        if (existing.Name == name && existing.Key == key && existing.ClientId == input.ClientId && existing.Description == description && existing.Status == input.Status
            && existing.StartDate == input.StartDate.Date && existing.DueDate == input.DueDate.Date && existing.MemberIds.Order().SequenceEqual(memberIds.Order()))
            return PbResult.NoChange;

        var before = new PbProject
        {
            Id = existing.Id, Name = existing.Name, Key = existing.Key, ClientId = existing.ClientId, Description = existing.Description, Status = existing.Status,
            StartDate = existing.StartDate, DueDate = existing.DueDate, MemberIds = existing.MemberIds.ToList(),
        };
        var unassigned = tasks.Where(task => task.ProjectId == existing.Id && task.AssigneeId is not null && !memberIds.Contains(task.AssigneeId)).ToList();
        var previousAssignees = unassigned.ToDictionary(task => task.Id, task => task.AssigneeId);

        ApplyProject(existing, name, key, description, input, memberIds);

        foreach (var task in unassigned)
            task.AssigneeId = null;

        saved = existing;

        return Commit(() =>
        {
            ApplyProject(existing, before.Name, before.Key, before.Description,
                new PbProjectInput(before.Name, before.Key, before.ClientId, before.Description, before.Status, before.StartDate, before.DueDate, before.MemberIds), before.MemberIds);

            foreach (var task in unassigned)
                task.AssigneeId = previousAssignees[task.Id];
        });
    }

    /// <summary>Ticks or unticks a checklist item (any signed-in person).</summary>
    public PbResult SetChecklistItem(int taskId, int itemId, bool done)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId)?.Checklist.FirstOrDefault(item => item.Id == itemId) is not { } item)
            return PbResult.NotFound;

        if (!IsOpen(FindTask(taskId)!))
            return PbResult.WrongState;

        if (item.Done == done)
            return PbResult.NoChange;

        item.Done = done;
        Changed?.Invoke();

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

    /// <summary>Validates a stored snapshot against its own data and applies it only when every part is valid.</summary>
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

        if (snapshot is not
            {
                Activities: { Count: <= MaxActivities } storedActivities,
                Clients: { Count: <= MaxClients } storedClients,
                Contacts: { Count: <= MaxContacts } storedContacts,
                Deals: { Count: <= MaxDeals } storedDeals,
                People: { Count: > 0 and <= MaxPeople } storedPeople,
                Projects: { Count: <= MaxProjects } storedProjects,
                Tasks: { Count: <= MaxTasks } storedTasks,
            }
            || new[] { snapshot.NextActivityId, snapshot.NextClientId, snapshot.NextContactId, snapshot.NextDealId, snapshot.NextProjectId, snapshot.NextTaskId }.Any(id => id is < 1 or > NextIdLimit))
            return false;

        var seededPeople = PbSeedData.People().Select(person => person.Id).ToHashSet();

        // A person with a sign-in has the account's ID; a demo account's or a registered account's person always has its account.
        if (storedPeople.Any(person => person is null || !(seededPeople.Contains(person.Id) || IsAccountId(person.Id)) || (person.AccountId is not null && (person.AccountId != person.Id || !IsAccountId(person.AccountId)))
                || (IsAccountId(person.Id) && person.AccountId != person.Id)
                || !IsText(person.Name, ShowcaseIdentity.MaxNameLength) || !IsText(person.Initials, 3) || !IsText(person.Title, MaxContactFieldLength) || person.CapacityHours is < 0 or > 60)
            || !Unique(storedPeople.Select(person => person.Id)))
            return false;

        var personIds = storedPeople.Select(person => person.Id).ToHashSet();

        if (storedClients.Any(client => client is null || client.Id < 1 || client.Id >= snapshot.NextClientId || !IsText(client.Name, MaxClientNameLength) || !IsText(client.Industry, MaxContactFieldLength)
                || client.Website is not { Length: <= MaxWebsiteLength } || !personIds.Contains(client.OwnerId) || !IsPlausible(client.CreatedAt))
            || !Unique(storedClients.Select(client => client.Id)) || !Unique(storedClients.Select(client => client.Name.ToLowerInvariant())))
            return false;

        var clientIds = storedClients.Select(client => client.Id).ToHashSet();

        if (storedContacts.Any(contact => contact is null || contact.Id < 1 || contact.Id >= snapshot.NextContactId || !clientIds.Contains(contact.ClientId) || !IsText(contact.Name, MaxContactFieldLength)
                || contact.Title is not { Length: <= MaxContactFieldLength } || !IsEmail(contact.Email) || contact.Phone is not { Length: <= MaxPhoneLength })
            || !Unique(storedContacts.Select(contact => contact.Id)) || storedContacts.GroupBy(contact => contact.ClientId).Any(group => group.Count(contact => contact.IsPrimary) > 1))
            return false;

        if (storedProjects.Any(project => !ValidProject(project, snapshot, clientIds, personIds)) || !Unique(storedProjects.Select(project => project.Id)) || !Unique(storedProjects.Select(project => project.Key)))
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

        Replace(activities, storedActivities);
        Replace(clients, storedClients);
        Replace(contacts, storedContacts);
        Replace(deals, storedDeals);
        Replace(people, storedPeople);
        Replace(projects, storedProjects);
        Replace(tasks, storedTasks);
        nextActivityId = snapshot.NextActivityId;
        nextClientId = snapshot.NextClientId;
        nextContactId = snapshot.NextContactId;
        nextDealId = snapshot.NextDealId;
        nextProjectId = snapshot.NextProjectId;
        nextTaskId = snapshot.NextTaskId;

        return true;
    }

    /// <summary>Saves a task's fields (any signed-in person); the assignee must be on the project.</summary>
    public PbResult UpdateTask(int taskId, PbTaskInput input)
    {
        if (Staff() is null)
            return PbResult.NotAllowed;

        if (FindTask(taskId) is not { } task || FindProject(task.ProjectId) is not { } project)
            return PbResult.NotFound;

        if (!IsOpen(FindTask(taskId)!))
            return PbResult.WrongState;

        if (!ValidTaskInput(project, input))
            return PbResult.Invalid;

        var current = new PbTaskInput(task.Title, task.Description, task.AssigneeId, task.Priority, task.DueDate, task.EstimateHours, task.MilestoneId);
        var next = input with { Title = input.Title.Trim(), Description = input.Description?.Trim() ?? string.Empty, DueDate = input.DueDate?.Date };

        if (current == next)
            return PbResult.NoChange;

        Apply(task, next);

        return Commit(() => Apply(task, current));
    }

    /// <summary>Returns everyone's open work (not on completed projects), the busiest (this week, against capacity) first.</summary>
    public IReadOnlyList<PbWorkload> Workload()
    {
        var weekEnd = WeekStart(Clock.Today).AddDays(7);

        return people.Select(person =>
            {
                var open = tasks.Where(task => task.AssigneeId == person.Id && task.Status != PbTaskStatus.Done && IsOpen(task)).OrderBy(task => task.DueDate ?? DateTime.MaxValue).ToList();
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

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

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

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (PbSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsEmail(string? value) => value is not null && value.Length <= ShowcaseIdentity.MaxEmailLength && EmailPattern().IsMatch(value);

    private static bool IsPlausible(DateTime value) => value >= PbSeedData.Now.AddYears(-3) && value <= PbSeedData.Now.AddYears(3);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    [GeneratedRegex("^[A-Z]{2,5}$")]
    private static partial Regex KeyPattern();

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

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

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

    /// <summary>Raises <see cref="Changed" /> when the snapshot still fits; otherwise undoes the change and reports Invalid.</summary>
    private PbResult Commit(Action undo)
    {
        if (!FitsSnapshot())
        {
            undo();

            return PbResult.Invalid;
        }

        Changed?.Invoke();

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

    /// <summary>A new account joins the team as a person; a person's name follows their profile.</summary>
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
        }

        Changed?.Invoke();
    }

    /// <summary>Whether a task's project still takes changes (it isn't completed).</summary>
    private bool IsOpen(PbTask task) => FindProject(task.ProjectId)?.Status != PbProjectStatus.Completed;

    /// <summary>The signed-in project manager, or <see langword="null" />.</summary>
    private PbPerson? ProjectManager() => Identity.Current?.RoleId == PbSeedData.ProjectManagerRole ? Staff() : null;

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

    private bool ValidTaskInput(PbProject project, PbTaskInput? input) =>
        input is not null && IsText(input.Title?.Trim(), MaxTaskTitleLength) && (input.Description?.Trim().Length ?? 0) <= MaxDescriptionLength && Enum.IsDefined(input.Priority)
        && (input.AssigneeId is null || project.MemberIds.Contains(input.AssigneeId)) && input.EstimateHours is >= 0 and <= MaxEstimateHours
        && (input.MilestoneId is null || project.Milestones.Any(milestone => milestone.Id == input.MilestoneId)) && (input.DueDate is null || IsPlausible(input.DueDate.Value));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the people who can own clients and deals: account managers.</summary>
    public IReadOnlyList<PbPerson> AccountManagers
    {
        get => people.Where(person => person.AccountId is not null
                ? Identity.FindAccount(person.AccountId)?.RoleId == PbSeedData.AccountManagerRole
                : person.Title == "Account manager")
            .ToList();
    }

    /// <summary>Gets every client activity entry, newest first.</summary>
    public IReadOnlyList<PbActivity> Activities => activities.OrderByDescending(item => item.At).ThenByDescending(item => item.Id).ToList();

    /// <summary>Gets every task (for reports and workload).</summary>
    public IReadOnlyList<PbTask> AllTasks => tasks.ToList();

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

    /// <summary>Gets the projects: active, then planned, on hold, and completed; by due date within each.</summary>
    public IReadOnlyList<PbProject> Projects =>
        projects.OrderBy(item => item.Status switch { PbProjectStatus.Active => 0, PbProjectStatus.Planned => 1, PbProjectStatus.OnHold => 2, _ => 3 }).ThenBy(item => item.DueDate).ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "project-board";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

    #endregion

    #region Events

    /// <summary>Raised after any change, so open pages re-render and the session store saves.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes, so charts can redraw.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Other

    /// <summary>The stored form of the state.</summary>
    private sealed class PbSnapshot
    {
        public List<PbActivity>? Activities { get; set; }

        public List<PbClient>? Clients { get; set; }

        public List<PbContact>? Contacts { get; set; }

        public List<PbDeal>? Deals { get; set; }

        public int NextActivityId { get; set; }

        public int NextClientId { get; set; }

        public int NextContactId { get; set; }

        public int NextDealId { get; set; }

        public int NextProjectId { get; set; }

        public int NextTaskId { get; set; }

        public List<PbPerson>? People { get; set; }

        public List<PbProject>? Projects { get; set; }

        public List<PbTask>? Tasks { get; set; }
    }

    #endregion
}
