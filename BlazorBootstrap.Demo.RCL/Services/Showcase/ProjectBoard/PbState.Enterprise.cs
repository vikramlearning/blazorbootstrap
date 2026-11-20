using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

/// <summary>
/// The enterprise baseline of <see cref="PbState" /> (rule WR-001): the audit log, the header's notifications (raised where
/// someone must act: a task assigned to them, a comment or an @mention on it, a task due soon or overdue, a task ready for
/// review, a project they now manage or whose status changed, a client they now own or a contact added to it), the
/// header's global search, the uniqueness checks the forms ask while someone types, and the lists' bulk changes (projects,
/// client owners, and the person's own tasks). Audit entries and notifications link only to pages on this site.
/// </summary>
public sealed partial class PbState
{
    #region Fields and Constants

    /// <summary>The longest notification text.</summary>
    private const int MaxNoticeTextLength = 200;

    /// <summary>The longest notification title.</summary>
    private const int MaxNoticeTitleLength = 80;

    /// <summary>The most reminders one sign-in raises about tasks due soon.</summary>
    private const int MaxRemindersPerSignIn = 5;

    /// <summary>The site's base route (the same as <c>PbLayout.Base</c>).</summary>
    private const string SiteBase = PbSeedData.SiteBase;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Changes the owner of several clients at once from the client list (account managers). Clients the account manager
    /// already owns are left as they are; each change is audited, and an owner other than the signed-in person is told.
    /// </summary>
    public PbBulkOutcome BulkChangeClientOwner(IReadOnlyCollection<int> clientIds, string ownerId)
    {
        if (AccountManager() is null)
            return new PbBulkOutcome(PbResult.NotAllowed, 0, 0);

        if (AccountManagers.All(person => person.Id != ownerId))
            return new PbBulkOutcome(PbResult.NotFound, 0, 0);

        return Bulk(clientIds, id =>
        {
            if (FindClient(id) is not { } client || client.OwnerId == ownerId)
                return false;

            var before = client.OwnerId;
            client.OwnerId = ownerId;
            Record("Owner changed", ClientTarget(client.Id), ClientPath(client.Id), $"{FindPerson(before)?.Name ?? "Nobody"} → {FindPerson(ownerId)?.Name}, from the client list");
            NotifyOwner(client);

            return true;
        });
    }

    /// <summary>
    /// Moves several of the signed-in person's tasks to Done at once (My tasks), each at the end of its project's Done column.
    /// Tasks already done, on a completed project, or no longer there are left as they are.
    /// </summary>
    public PbBulkOutcome BulkCompleteTasks(IReadOnlyCollection<int> taskIds)
    {
        if (Staff() is null)
            return new PbBulkOutcome(PbResult.NotAllowed, 0, 0);

        return Bulk(taskIds, id => FindTask(id) is { Status: not PbTaskStatus.Done } task && MoveTask(task.Id, PbTaskStatus.Done, TasksIn(task.ProjectId, PbTaskStatus.Done).Count) == PbResult.Ok);
    }

    /// <summary>
    /// Assigns several tasks at once (My tasks) to someone, or unassigns them (<see langword="null" />). A task whose project
    /// doesn't include the person, a done or read-only task, or one already assigned to them is left as it is.
    /// </summary>
    public PbBulkOutcome BulkReassignTasks(IReadOnlyCollection<int> taskIds, string? personId)
    {
        if (Staff() is null)
            return new PbBulkOutcome(PbResult.NotAllowed, 0, 0);

        if (personId is not null && FindPerson(personId) is null)
            return new PbBulkOutcome(PbResult.NotFound, 0, 0);

        return Bulk(taskIds, id => FindTask(id) is { Status: not PbTaskStatus.Done } task && Reassign(task.Id, personId) == PbResult.Ok);
    }

    /// <summary>
    /// Sets the due date of several open tasks at once (My tasks), or clears it (<see langword="null" />). A task whose project
    /// dates don't include the date, a done or read-only task, or one already due then is left as it is.
    /// </summary>
    public PbBulkOutcome BulkSetDueDate(IReadOnlyCollection<int> taskIds, DateTime? due)
    {
        if (Staff() is null)
            return new PbBulkOutcome(PbResult.NotAllowed, 0, 0);

        if (due is { } date && !IsPlausible(date))
            return new PbBulkOutcome(PbResult.Invalid, 0, 0);

        return Bulk(taskIds, id =>
        {
            if (FindTask(id) is not { Status: not PbTaskStatus.Done } task)
                return false;

            var input = new PbTaskInput(task.Title, task.Description, task.AssigneeId, task.Priority, due?.Date, task.EstimateHours, task.MilestoneId);

            return UpdateTask(task.Id, input) == PbResult.Ok;
        });
    }

    /// <summary>
    /// Changes several projects at once from Manage projects (project managers): sets their status, assigns a project
    /// manager, or both (<see langword="null" /> leaves it). Each change is audited, and the project managers concerned are
    /// told like a single edit.
    /// </summary>
    public PbBulkOutcome BulkUpdateProjects(IReadOnlyCollection<int> projectIds, PbProjectStatus? status, string? ownerId)
    {
        if (ProjectManager() is null)
            return new PbBulkOutcome(PbResult.NotAllowed, 0, 0);

        if ((status is { } value && !Enum.IsDefined(value)) || (status is null && ownerId is null))
            return new PbBulkOutcome(PbResult.Invalid, 0, 0);

        if (ownerId is not null && ProjectManagers.All(person => person.Id != ownerId))
            return new PbBulkOutcome(PbResult.NotFound, 0, 0);

        return Bulk(projectIds, id =>
        {
            if (FindProject(id) is not { } project)
                return false;

            var targetStatus = status ?? project.Status;
            var targetOwner = ownerId ?? project.OwnerId;

            if (project.Status == targetStatus && project.OwnerId == targetOwner)
                return false;

            var (beforeStatus, beforeOwner) = (project.Status, project.OwnerId);
            (project.Status, project.OwnerId) = (targetStatus, targetOwner);
            TellAboutProjectChange(project, beforeStatus, beforeOwner, " from Manage projects");

            return true;
        });
    }

    /// <summary>Returns the signed-in account's notifications for the header's menu, newest first; empty when nobody is signed in.</summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Identity.Current is not { } account
            ? Array.Empty<ShowcaseNotification>()
            : notifications.Where(notice => notice.AccountId == account.Id).OrderByDescending(notice => notice.At).ThenByDescending(notice => notice.Id).ToList();

    /// <summary>Returns whether another client has this name, ignoring case and surrounding spaces; the client being edited doesn't count.</summary>
    public bool IsClientNameTaken(string? name, int exceptClientId = 0) =>
        !string.IsNullOrWhiteSpace(name) && clients.Any(client => client.Id != exceptClientId && string.Equals(client.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether another contact at the client has this email, ignoring case; the contact being edited doesn't count.</summary>
    public bool IsContactEmailTaken(int clientId, string? email, int exceptContactId = 0) =>
        !string.IsNullOrWhiteSpace(email)
        && contacts.Any(contact => contact.Id != exceptContactId && contact.ClientId == clientId && string.Equals(contact.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether a due date falls within a project's start and due dates.</summary>
    public static bool IsDueInProject(PbProject project, DateTime due) => due.Date >= project.StartDate.Date && due.Date <= project.DueDate.Date;

    /// <summary>Returns whether a phone number has the right form: 7 to 30 characters of digits, spaces, and + - ( ) ., with at least 7 digits.</summary>
    public static bool IsPhone(string? value) => value is { Length: >= 7 and <= MaxPhoneLength } && PhonePattern().IsMatch(value) && value.Count(char.IsAsciiDigit) >= 7;

    /// <summary>Returns whether another project has this key; the project being edited doesn't count.</summary>
    public bool IsProjectKeyTaken(string? key, int exceptProjectId = 0) =>
        !string.IsNullOrWhiteSpace(key) && projects.Any(project => project.Id != exceptProjectId && string.Equals(project.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether another project of the same client (or another internal project) has this name, ignoring case; the project being edited doesn't count.</summary>
    public bool IsProjectNameTaken(string? name, int? clientId, int exceptProjectId = 0) =>
        !string.IsNullOrWhiteSpace(name)
        && projects.Any(project => project.Id != exceptProjectId && project.ClientId == clientId && string.Equals(project.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether a website has the right form: a domain like example.com, optionally with http(s):// and a path.</summary>
    public static bool IsWebsite(string? value) => value is { Length: > 0 and <= MaxWebsiteLength } && WebsitePattern().IsMatch(value);

    /// <summary>Marks every notification of the signed-in account as read (the header's "Mark all as read").</summary>
    public void MarkAllNotificationsRead()
    {
        if (Identity.Current is not { } account || notifications.All(notice => notice.AccountId != account.Id || notice.IsRead))
            return;

        foreach (var notice in notifications.Where(notice => notice.AccountId == account.Id))
            notice.IsRead = true;

        RaiseChanged();
    }

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (Identity.Current is { } account && notifications.FirstOrDefault(notice => notice.Id == id && notice.AccountId == account.Id) is { IsRead: false } notice)
        {
            notice.IsRead = true;
            RaiseChanged();
        }
    }

    /// <summary>Returns a contact role in words, like "Decision maker".</summary>
    public static string RoleWord(PbContactRole role) => role switch
    {
        PbContactRole.DecisionMaker => "Decision maker",
        PbContactRole.DayToDay => "Day-to-day contact",
        PbContactRole.Billing => "Billing contact",
        PbContactRole.Technical => "Technical contact",
        _ => "Other",
    };

    /// <summary>
    /// The header's global search, best matches first (exactly the term, then starting with it, then containing it), and
    /// among equal matches by kind. Everyone finds projects (by key, name, or client), each opening its board, and tasks (by
    /// key like "WEB-12", or title), each opening the task, open ones first; account managers also find clients (by name or
    /// industry) and client contacts (by name or email), each opening the client's page. Returns nothing for a blank term or
    /// when nobody is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || Staff() is null)
            return Array.Empty<ShowcaseSearchResult>();

        // 0: exactly the term, 1: starts with it, 2: contains it; null: no match.
        int? Rank(params string?[] values) => values
            .Select(value => value is null ? (int?)null
                : string.Equals(value, term, StringComparison.OrdinalIgnoreCase) ? 0
                : value.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 1
                : value.Contains(term, StringComparison.OrdinalIgnoreCase) ? 2
                : null)
            .Where(rank => rank is not null)
            .DefaultIfEmpty(null)
            .Min();

        var clientNames = clients.ToDictionary(client => client.Id, client => client.Name);
        var projectsById = projects.ToDictionary(project => project.Id);
        var names = people.ToDictionary(person => person.Id, person => person.Name);
        var matches = new List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)>();

        // Kind: 0 a project, 1 a task, 2 a client, 3 a contact.
        foreach (var project in projects)
        {
            var client = project.ClientId is { } clientId ? clientNames.GetValueOrDefault(clientId) : null;

            if (Rank(project.Key, project.Name, client) is { } found)
            {
                matches.Add((found, 0, project.Status == PbProjectStatus.Completed ? 1 : 0, new ShowcaseSearchResult("Project", $"{project.Key} · {project.Name}",
                    $"{ProjectStatusWord(project.Status)} · {client ?? "Internal"} · due {Day(project.DueDate)}", ProjectPath(project.Id))));
            }
        }

        if (term.Length >= 2)
        {
            foreach (var task in tasks)
            {
                var project = projectsById.GetValueOrDefault(task.ProjectId);
                var key = Invariant($"{project?.Key ?? "?"}-{task.Number}");

                if (Rank(key, task.Title) is { } found)
                {
                    matches.Add((found, 1, task.Status == PbTaskStatus.Done ? 1 : 0, new ShowcaseSearchResult("Task", $"{key} {task.Title}",
                        $"{StatusWord(task.Status)} · {(task.AssigneeId is { } assignee ? names.GetValueOrDefault(assignee) : "Unassigned")} · {project?.Name}", TaskPath(task.Id))));
                }
            }
        }

        if (IsAccountManager)
        {
            foreach (var client in clients)
            {
                if (Rank(client.Name, client.Industry) is { } found)
                {
                    matches.Add((found, 2, 0, new ShowcaseSearchResult("Client", client.Name,
                        $"{client.Industry} · owner {names.GetValueOrDefault(client.OwnerId) ?? "nobody"} · {client.BillingCurrency}", ClientPath(client.Id))));
                }
            }

            foreach (var contact in contacts)
            {
                if (Rank(contact.Name, contact.Email) is { } found)
                {
                    matches.Add((found, 3, contact.IsPrimary ? 0 : 1, new ShowcaseSearchResult("Contact", contact.Name,
                        $"{RoleWord(contact.Role)} at {clientNames.GetValueOrDefault(contact.ClientId)} · {contact.Email}", ClientPath(contact.ClientId))));
                }
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Order)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    /// <summary>Returns a project status in words, like "On hold".</summary>
    public static string ProjectStatusWord(PbProjectStatus status) => status == PbProjectStatus.OnHold ? "On hold" : status.ToString();

    /// <summary>Returns a task column in words, like "In review".</summary>
    public static string StatusWord(PbTaskStatus status) => status switch
    {
        PbTaskStatus.Todo => "To do",
        PbTaskStatus.InProgress => "In progress",
        PbTaskStatus.Review => "In review",
        _ => "Done",
    };

    #endregion

    #region Private Methods

    private static string ClientPath(int clientId) => Invariant($"{SiteBase}/clients/{clientId}");

    private static string DealPath() => SiteBase + "/pipeline";

    private static string DealTarget(PbDeal deal) => $"Deal “{deal.Title}”";

    /// <summary>Checks a stored audit or notification link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) =>
        url is null || (url.Length <= 200 && (url == SiteBase || url.StartsWith(SiteBase + "/", StringComparison.Ordinal)) && !url.Contains("//", StringComparison.Ordinal)
            && url.All(character => !char.IsWhiteSpace(character) && !char.IsControl(character) && character is not ('"' or '\'' or '<' or '>' or '\\')));

    private static bool IsEmail(string? value) => value is not null && value.Length <= ShowcaseIdentity.MaxEmailLength && EmailPattern().IsMatch(value);

    [GeneratedRegex(@"^\+?[0-9 ().\-]+$")]
    private static partial Regex PhonePattern();

    private static string ProjectEditPath(int projectId) => Invariant($"{SiteBase}/manage/projects/{projectId}/edit");

    private static string ProjectPath(int projectId) => Invariant($"{SiteBase}/projects/{projectId}");

    /// <summary>A project as the audit log names it: its key and name, like "Project WEB · Harbor &amp; Pine storefront redesign".</summary>
    private static string ProjectTarget(PbProject project) => $"Project {project.Key} · {project.Name}";

    private static string TaskPath(int taskId) => Invariant($"{SiteBase}/tasks/{taskId}");

    [GeneratedRegex(@"^(https?://)?([a-z0-9]([a-z0-9-]*[a-z0-9])?\.)+[a-z]{2,}(/[^\s]*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex WebsitePattern();

    /// <summary>Runs one change on several records, raising <see cref="Changed" /> once; Invalid for an empty list.</summary>
    private PbBulkOutcome Bulk(IReadOnlyCollection<int> ids, Func<int, bool> change)
    {
        var list = (ids ?? Array.Empty<int>()).Distinct().ToList();

        if (list.Count == 0)
            return new PbBulkOutcome(PbResult.Invalid, 0, 0);

        int changed = 0, unchanged = 0;
        batchDepth++;

        try
        {
            foreach (var id in list)
            {
                if (change(id))
                    changed++;
                else
                    unchanged++;
            }
        }
        finally
        {
            batchDepth--;
        }

        if (batchDepth == 0 && (changedInBatch || changed > 0))
        {
            changedInBatch = false;
            Changed?.Invoke();
        }

        return new PbBulkOutcome(changed == 0 ? PbResult.NoChange : PbResult.Ok, changed, unchanged);
    }

    private string ClientName(int? clientId) => clientId is null ? "internal" : FindClient(clientId)?.Name ?? "a deleted client";

    /// <summary>A client as the audit log names it.</summary>
    private string ClientTarget(int clientId) => $"Client {FindClient(clientId)?.Name ?? "(deleted)"}";

    /// <summary>A contact as the audit log names it: the person and their company.</summary>
    private string ContactTarget(PbContact contact) => $"Contact {contact.Name} ({FindClient(contact.ClientId)?.Name ?? "a deleted client"})";

    /// <summary>The people with a sign-in mentioned in a comment with @ and their first or full name, like "@Jordan" or "@Jordan Lee".</summary>
    private List<PbPerson> Mentioned(string text)
    {
        if (!text.Contains('@'))
            return new List<PbPerson>();

        return people.Where(person => person.AccountId is not null && person.Name.Length > 0
                && (text.Contains("@" + person.Name, StringComparison.OrdinalIgnoreCase)
                    || Regex.IsMatch(text, @"(^|\s)@" + Regex.Escape(person.Name.Split(' ')[0]) + @"\b", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100))))
            .ToList();
    }

    /// <summary>
    /// Raises a notification for an account that can sign in (rule WR-001); nothing for a teammate without a sign-in, and
    /// nothing for the person who caused it, unless the system did (<paramref name="fromSystem" />). The site keeps its
    /// newest notifications within the limit a restore accepts.
    /// </summary>
    private void Notify(string? accountId, string title, string text, string? link, bool fromSystem = false)
    {
        if (accountId is null || Identity.FindAccount(accountId) is null || (!fromSystem && accountId == Identity.Current?.Id) || nextNotificationId >= NextIdLimit)
            return;

        notifications.Insert(0, new ShowcaseNotification
        {
            Id = nextNotificationId++,
            AccountId = accountId,
            At = Clock.Now,
            Title = Clip(title, MaxNoticeTitleLength),
            Text = string.IsNullOrWhiteSpace(text) ? null : Clip(text, MaxNoticeTextLength),
            Url = IsSiteLink(link) ? link : null,
        });

        while (notifications.Count > MaxNotifications)
            notifications.RemoveAt(notifications.Count - 1);
    }

    /// <summary>Tells a task's assignee that it is theirs now, unless they assigned it themselves.</summary>
    private void NotifyAssigned(PbTask task)
    {
        if (task.AssigneeId is not { } assigneeId || FindProject(task.ProjectId) is not { } project)
            return;

        var due = task.DueDate is { } date ? $", due {date.ToString("MMM d", CultureInfo.InvariantCulture)}" : null;
        NotifyPerson(assigneeId, $"{TaskKey(task)} is assigned to you", $"{Identity.Current?.Name ?? "Someone"} assigned “{task.Title}” to you on {project.Name}{due}.", TaskPath(task.Id));
    }

    /// <summary>Tells a client's owner that they own it now, unless they made the change.</summary>
    private void NotifyOwner(PbClient client) =>
        NotifyPerson(client.OwnerId, $"You own {client.Name}", $"{Identity.Current?.Name ?? "Someone"} made you the account manager for {client.Name} ({client.Industry}).", ClientPath(client.Id));

    /// <summary>Raises a notification for a person of the team, when they can sign in.</summary>
    private void NotifyPerson(string? personId, string title, string text, string? link, bool fromSystem = false) =>
        Notify(FindPerson(personId)?.AccountId, title, text, link, fromSystem);

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock; Tandem itself when nobody is signed in. The log keeps its newest
    /// entries.
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details = null)
    {
        if (nextAuditId >= NextIdLimit)
            return;

        var actor = Identity.Current?.Name;

        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = Clock.Now,
            Actor = Clip(string.IsNullOrWhiteSpace(actor) ? PbSeedData.SystemActorName : actor.Trim(), ShowcaseIdentity.MaxNameLength),
            Action = Clip(action, MaxAuditTextLength),
            Target = Clip(target, MaxAuditTextLength),
            TargetUrl = IsSiteLink(targetUrl) ? targetUrl : null,
            Details = string.IsNullOrWhiteSpace(details) ? null : Clip(details, MaxAuditTextLength),
        });

        while (audit.Count > MaxAudit)
            audit.RemoveAt(0);
    }

    /// <summary>Audits a task's new assignee (or that it is unassigned now).</summary>
    private void RecordAssignment(PbTask task, string? before) =>
        Record(task.AssigneeId is null ? "Task unassigned" : "Task assigned", TaskTarget(task), TaskPath(task.Id),
            $"{FindPerson(before)?.Name ?? "Unassigned"} → {FindPerson(task.AssigneeId)?.Name ?? "Unassigned"}");

    /// <summary>
    /// Reminds the signed-in person of their open tasks due today or tomorrow, or overdue (a few at a time, once per task and
    /// kind), so the bell says what needs doing when they sign in.
    /// </summary>
    private void RemindDueTasks()
    {
        if (Staff() is not { } person || person.AccountId is not { } accountId)
            return;

        var today = Clock.Today;
        var reminded = 0;

        foreach (var task in tasks.Where(task => task.AssigneeId == person.Id && task.Status != PbTaskStatus.Done && task.DueDate is { } due && due.Date <= today.AddDays(1) && IsOpen(task))
                     .OrderBy(task => task.DueDate))
        {
            var days = (task.DueDate!.Value.Date - today).Days;
            var key = TaskKey(task);
            var title = days switch { < 0 => $"{key} is overdue", 0 => $"{key} is due today", _ => $"{key} is due tomorrow" };
            var url = TaskPath(task.Id);

            if (notifications.Any(notice => notice.AccountId == accountId && notice.Url == url && notice.Title == title))
                continue;

            Notify(accountId, title, $"“{task.Title}” on {FindProject(task.ProjectId)?.Name}.", url, fromSystem: true);

            if (++reminded >= MaxRemindersPerSignIn)
                break;
        }
    }

    /// <summary>A task as the audit log names it: its key, like "Task WEB-12".</summary>
    private string TaskTarget(PbTask task) => $"Task {TaskKey(task)}";

    /// <summary>
    /// Audits a project's status and project manager changes (the other fields are audited by the edit), and tells the
    /// project's manager about a status change and a new project manager that they manage it now, when someone else did it.
    /// </summary>
    private void TellAboutProjectChange(PbProject project, PbProjectStatus beforeStatus, string beforeOwner, string? where = null)
    {
        if (project.Status != beforeStatus)
        {
            Record("Status changed", ProjectTarget(project), ProjectPath(project.Id), $"{ProjectStatusWord(beforeStatus)} → {ProjectStatusWord(project.Status)}{where}");
            NotifyPerson(project.OwnerId, $"{project.Key} is now {ProjectStatusWord(project.Status).ToLowerInvariant()}",
                $"{Identity.Current?.Name ?? "Someone"} changed {project.Name} from {ProjectStatusWord(beforeStatus).ToLowerInvariant()}.", ProjectPath(project.Id));
        }

        if (project.OwnerId != beforeOwner)
        {
            Record("Project manager changed", ProjectTarget(project), ProjectPath(project.Id), $"{FindPerson(beforeOwner)?.Name ?? "Nobody"} → {FindPerson(project.OwnerId)?.Name}{where}");
            NotifyPerson(project.OwnerId, $"You're the project manager of {project.Key}", $"{Identity.Current?.Name ?? "Someone"} handed {project.Name} to you.", ProjectEditPath(project.Id));
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for project and account managers; empty for team members.</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsProjectManager || IsAccountManager ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    #endregion
}
