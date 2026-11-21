using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

/// <summary>
/// Fixed, fictitious content for "Tandem", the project and client board of the made-up agency Brightline Studio: the team,
/// clients and their contacts, projects with milestones and tasks, the deal pipeline, client activity, the audit log, and
/// the demo accounts' notifications, and (in <c>PbSeedData.Timesheets.cs</c>) hours, timesheets, invoices, and the rate card.
/// The hand-written demo projects (WEB, APP, BRD, OPS) and their clients come first; a
/// deterministic generator adds the agency's other clients, their contacts, and two years of projects and tasks, so the
/// lists have realistic volume. Every fresh tab starts the same way, which lets the tab snapshot keep only what differs.
/// </summary>
public static partial class PbSeedData
{
    #region Fields and Constants

    /// <summary>The identity role of account managers (clients, pipeline, forecast).</summary>
    public const string AccountManagerRole = "am";

    /// <summary>The job title of account managers without a sign-in (they can own clients and deals).</summary>
    public const string AccountManagerTitle = "Account manager";

    /// <summary>The fictitious agency.</summary>
    public const string AgencyName = "Brightline Studio";

    /// <summary>The identity role of team members (and of new accounts).</summary>
    public const string MemberRole = "member";

    /// <summary>Every signed-in role.</summary>
    public const string SignedInRoles = MemberRole + "," + ProjectManagerRole + "," + AccountManagerRole;

    /// <summary>The roles that manage the work and see the audit log: project managers and account managers.</summary>
    public const string ManagerRoles = ProjectManagerRole + "," + AccountManagerRole;

    /// <summary>The monthly won-revenue target of the agency, in dollars.</summary>
    public const int MonthlyTarget = 120_000;

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Tandem";

    /// <summary>The identity role of project managers (projects, workload, roadmap, reports).</summary>
    public const string ProjectManagerRole = "pm";

    /// <summary>The job title of project managers without a sign-in (they can run projects).</summary>
    public const string ProjectManagerTitle = "Project manager";

    /// <summary>The demo account manager's person ID.</summary>
    public const string SeededAccountManagerId = "elena";

    /// <summary>The demo member's person ID.</summary>
    public const string SeededMemberId = "jordan";

    /// <summary>The demo project manager's person ID.</summary>
    public const string SeededProjectManagerId = "nadia";

    /// <summary>The site's base route (the same as <c>PbLayout.Base</c>).</summary>
    public const string SiteBase = "/showcase/project-board";

    /// <summary>The name the audit log uses for what Tandem does by itself.</summary>
    public const string SystemActorName = "Tandem";

    /// <summary>The currencies a client can be billed in (ISO 4217 codes).</summary>
    public static readonly IReadOnlyList<string> Currencies = new[] { "USD", "EUR", "GBP", "CAD", "AUD" };

    /// <summary>When a fresh tab's clock starts (a Tuesday); the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 0, 0);

    private static readonly string[] activityNotes =
    {
        "Checked in with {0} on how the launch went. Happy with the results; asked about a follow-up phase.",
        "Call with {0}: budget for next year is being set in January.",
        "Sent {0} the monthly status report and the hours used so far.",
        "Meeting with {0} and their team to review the roadmap for the spring.",
        "{0} asked for a quote for extra content pages.",
        "Emailed {0} the renewal terms for the support retainer.",
        "{0} introduced us to their new head of marketing.",
        "Quarterly review with {0}. Satisfaction is high; one concern about response times.",
    };

    private static readonly (string Suffix, string Industry)[] clientSuffixes =
    {
        ("Logistics", "Logistics"), ("Health", "Healthcare"), ("Credit Union", "Financial services"), ("Foods", "Food and beverage"), ("Academy", "Education"),
        ("Outfitters", "Retail"), ("Energy", "Energy"), ("Labs", "Technology"), ("Insurance", "Insurance"), ("Hotels", "Hospitality"), ("Media", "Media"), ("Motors", "Automotive"),
    };

    private static readonly string[] clientPrefixes =
    {
        "Northwind", "Bluefield", "Granite", "Silverline", "Redwood", "Oakridge", "Brightwater", "Kestrel", "Summit", "Harborview", "Cedarline", "Lakeshore", "Ironbridge", "Maplewood",
        "Starling", "Willowbrook", "Falconcrest", "Juniper", "Pioneer", "Riverstone", "Coastal", "Evergreen", "Highland", "Larkspur", "Meadowlark", "Orchid", "Quarry", "Sterling",
    };

    private static readonly string[] contactFirstNames =
    {
        "Alice", "Bruno", "Carmen", "Dmitri", "Esther", "Fatima", "George", "Helena", "Ivan", "Julia", "Kofi", "Laura", "Mateo", "Nadine", "Oscar", "Paula", "Quentin", "Rosa",
        "Stefan", "Tanya", "Umar", "Vera", "Walter", "Ximena", "Yara", "Zane",
    };

    private static readonly string[] contactLastNames =
    {
        "Abbott", "Barros", "Castillo", "Dubois", "Eriksen", "Fontaine", "Gallagher", "Hartmann", "Iqbal", "Jansen", "Kowalski", "Lambert", "Moreau", "Nilsson", "O'Neill", "Petrov",
        "Quinn", "Rossi", "Schultz", "Torres", "Ueda", "Vargas", "Whitaker", "Young",
    };

    private static readonly string[] projectTemplates =
    {
        "website redesign", "customer portal", "mobile app", "brand refresh", "design system", "analytics dashboard", "e-commerce replatform", "booking flow", "intranet",
        "accessibility audit", "content migration", "onboarding redesign", "marketing site", "partner portal", "checkout optimization", "loyalty app", "support center",
        "reporting suite", "pricing page tests", "annual report site",
    };

    private static readonly string[] taskTitles =
    {
        "Kickoff workshop", "Stakeholder interviews", "Requirements sign-off", "Content inventory", "Information architecture", "Wireframes", "Visual design",
        "Design review with the client", "Front-end build", "API integration", "Content migration", "Analytics setup", "Accessibility review", "Performance tuning", "QA pass",
        "Fix QA findings", "User acceptance testing", "Launch checklist", "Go-live", "Training session", "Handover documentation", "Post-launch review",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Builds a fresh tab's data: new objects every call, so a tab can change them without touching another copy.</summary>
    public static PbSeedSet Build()
    {
        var people = People();
        var clients = Clients();
        var contacts = Contacts();
        var projects = Projects();
        var tasks = Tasks();
        var deals = Deals();
        var activities = Activities();

        // Task numbers are never reused: each hand-written project continues after its highest.
        foreach (var project in projects)
            project.NextTaskNumber = tasks.Where(task => task.ProjectId == project.Id).Select(task => task.Number).DefaultIfEmpty(0).Max() + 1;

        Generate(people, clients, contacts, projects, tasks, activities);

        var rates = Rates();
        var (entries, timesheets, invoices) = Timesheets(people, clients, projects, tasks, rates);

        return new PbSeedSet(activities, Audit(people, clients, projects, tasks, deals, activities, entries, timesheets, invoices), clients, contacts, deals,
            Notifications(people, clients, contacts, projects, tasks, deals, entries, timesheets, invoices), people, projects, tasks, entries, timesheets, invoices, rates);
    }

    /// <summary>The demo accounts: a team member, a project manager, and an account manager.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededMemberId, MemberRole, "Jordan Lee", "jordan.lee@example.com", "board-demo-26", "JL"),
        new(SeededProjectManagerId, ProjectManagerRole, "Nadia Okafor", "nadia.okafor@example.com", "board-pm-26", "NO"),
        new(SeededAccountManagerId, AccountManagerRole, "Elena Vasquez", "elena.vasquez@example.com", "board-am-26", "EV"),
    };

    /// <summary>The roles of the site, with their start pages.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(MemberRole, "Team member", "/showcase/project-board/my-tasks", "/showcase/project-board/profile", "Works their tasks on project boards: comments, checklists, and due dates."),
        new(ProjectManagerRole, "Project manager", "/showcase/project-board/manage/projects", "/showcase/project-board/profile", "Also runs projects, milestones, the team's workload, the roadmap, and reports."),
        new(AccountManagerRole, "Account manager", "/showcase/project-board/clients", "/showcase/project-board/profile", "Also manages clients and contacts, the deal pipeline, activity, and the forecast."),
    };

    #endregion

    #region Private Methods

    /// <summary>The client activity timeline of the hand-written clients.</summary>
    private static List<PbActivity> Activities()
    {
        var id = 1;

        PbActivity Entry(int clientId, int daysAgo, int hour, PbActivityKind kind, string authorId, string text, int? dealId = null) =>
            new() { Id = id++, ClientId = clientId, At = Now.Date.AddDays(-daysAgo).AddHours(hour), Kind = kind, AuthorId = authorId, Text = text, DealId = dealId };

        return new List<PbActivity>
        {
            Entry(2, 63, 15, PbActivityKind.Deal, SeededAccountManagerId, "Won “Meridian patient app” ($140,000). Project APP created.", 2),
            Entry(1, 43, 11, PbActivityKind.Deal, SeededAccountManagerId, "Won “Harbor & Pine storefront redesign” ($85,000). Project WEB created.", 1),
            Entry(3, 27, 16, PbActivityKind.Deal, SeededAccountManagerId, "Won “Copperleaf brand refresh” ($32,000). Project BRD created.", 3),
            Entry(1, 20, 10, PbActivityKind.Meeting, SeededAccountManagerId, "Quarterly review with Grace and Tom. They want a holiday campaign landing page before December.", 5),
            Entry(5, 18, 14, PbActivityKind.Call, "hannah", "Intro call with Daniel Osei. Members complain the online banking flow is confusing; asked for a UX audit proposal.", 6),
            Entry(4, 14, 9, PbActivityKind.Deal, SeededAccountManagerId, "Won “Atlas tracking portal” ($96,000). Project OPS created.", 4),
            Entry(6, 12, 13, PbActivityKind.Email, "hannah", "Sent case studies to Ruth Adler for the admissions site.", 7),
            Entry(5, 9, 11, PbActivityKind.Email, "hannah", "Sent the UX audit proposal: two weeks, five deliverables.", 6),
            Entry(3, 7, 15, PbActivityKind.Note, SeededAccountManagerId, "Marta liked the first brand concepts. Asked whether we also build subscription shops.", 9),
            Entry(1, 6, 10, PbActivityKind.Call, SeededAccountManagerId, "Tom asked for a revised quote for the holiday campaign with two landing pages.", 5),
            Entry(2, 5, 16, PbActivityKind.Meeting, SeededAccountManagerId, "Steering meeting: phase 2 (clinician dashboard) could start in spring.", 8),
            Entry(4, 4, 12, PbActivityKind.Call, SeededAccountManagerId, "Rafael mentioned a driver app for next year. Budget not set yet.", 10),
            Entry(5, 3, 9, PbActivityKind.Note, "hannah", "Lumen's board meets on December 8 to approve the audit.", 6),
            Entry(1, 1, 17, PbActivityKind.Email, SeededAccountManagerId, "Sent the revised holiday campaign quote ($22,000).", 5),
        };
    }

    /// <summary>
    /// The audit log when the tab starts, oldest first: the last 90 days of clients and projects added, tasks created and
    /// completed, comments, deals won and lost, and activity logged, each by the person who did it.
    /// </summary>
    private static List<ShowcaseAuditEntry> Audit(IReadOnlyList<PbPerson> people, IReadOnlyList<PbClient> clients, IReadOnlyList<PbProject> projects, IReadOnlyList<PbTask> tasks,
        IReadOnlyList<PbDeal> deals, IReadOnlyList<PbActivity> activities, IReadOnlyList<PbTimeEntry> timeEntries, IReadOnlyList<PbTimesheet> timesheets, IReadOnlyList<PbInvoice> invoices)
    {
        var since = Now.AddDays(-90);
        var names = people.ToDictionary(person => person.Id, person => person.Name);
        var projectsById = projects.ToDictionary(project => project.Id);
        var clientsById = clients.ToDictionary(client => client.Id);
        var entries = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();

        string Name(string? personId) => personId is not null && names.TryGetValue(personId, out var name) ? name : SystemActorName;

        foreach (var client in clients.Where(client => client.CreatedAt >= since))
            entries.Add((client.CreatedAt, Name(client.OwnerId), "Client added", $"Client {client.Name}", ClientPath(client.Id), $"{client.Industry}, billed in {client.BillingCurrency}"));

        foreach (var project in projects.Where(project => project.CreatedAt >= since))
        {
            entries.Add((project.CreatedAt, Name(project.OwnerId), "Project created", $"Project {project.Key} · {project.Name}", ProjectPath(project.Id),
                $"{(project.Status == PbProjectStatus.OnHold ? "On hold" : project.Status.ToString())}, {Day(project.StartDate)} to {Day(project.DueDate)}"));
        }

        foreach (var task in tasks)
        {
            var project = projectsById[task.ProjectId];
            var target = Invariant($"Task {project.Key}-{task.Number}");

            if (task.CreatedAt >= since.AddDays(60))
                entries.Add((task.CreatedAt, Name(project.OwnerId), "Task created", target, TaskPath(task.Id), task.Title));

            if (task.CompletedAt is { } completed && completed >= since.AddDays(60))
                entries.Add((completed, Name(task.AssigneeId ?? project.OwnerId), "Task completed", target, TaskPath(task.Id), $"{task.Title}: moved to Done"));

            foreach (var comment in task.Comments.Where(comment => comment.At >= since))
                entries.Add((comment.At, Name(comment.AuthorId), "Comment added", target, TaskPath(task.Id), Clip(comment.Text, 120)));
        }

        foreach (var deal in deals.Where(deal => deal.ClosedAt >= since))
        {
            entries.Add((deal.ClosedAt!.Value, Name(deal.OwnerId), deal.Stage == PbDealStage.Won ? "Deal won" : "Deal lost", $"Deal “{deal.Title}”", SiteBase + "/pipeline",
                Invariant($"${deal.Value:#,0}")));
        }

        foreach (var activity in activities.Where(activity => activity.At >= since && activity.Kind is not (PbActivityKind.Deal or PbActivityKind.Project)))
        {
            entries.Add((activity.At, Name(activity.AuthorId), "Activity logged", $"Client {clientsById[activity.ClientId].Name}", ClientPath(activity.ClientId),
                $"{activity.Kind}: {Clip(activity.Text, 120)}"));
        }

        TimesheetAudit(entries, people, clients, projects, timeEntries, timesheets, invoices);

        return entries
            .Where(entry => entry.At <= Now)
            .OrderBy(entry => entry.At)
            .Select((entry, index) => new ShowcaseAuditEntry
            {
                Id = index + 1, At = entry.At, Actor = entry.Actor, Action = entry.Action, Target = entry.Target, TargetUrl = entry.Url, Details = entry.Details,
            })
            .ToList();
    }

    /// <summary>Returns a client's page.</summary>
    private static string ClientPath(int clientId) => Invariant($"{SiteBase}/clients/{clientId}");

    private static string Clip(string text, int length) => text.Length <= length ? text : string.Concat(text.AsSpan(0, length - 1), "…");

    /// <summary>The hand-written client companies (the demo projects' clients and two prospects).</summary>
    private static List<PbClient> Clients()
    {
        PbClient Client(int id, string name, string industry, string slug, string ownerId, int createdDaysAgo, string phone, bool contract) => new()
        {
            Id = id, Name = name, Industry = industry, Website = $"{slug}.example", OwnerId = ownerId, CreatedAt = Now.AddDays(-createdDaysAgo), BillingCurrency = "USD",
            BillingEmail = $"accounts@{slug}.example", Phone = phone, ContractStart = contract ? Now.Date.AddDays(-createdDaysAgo + 14) : null,
            ContractEnd = contract ? Now.Date.AddDays(-createdDaysAgo + 14 + 364) : null,
        };

        return new List<PbClient>
        {
            Client(1, "Harbor & Pine Outfitters", "Retail", "harborpine", SeededAccountManagerId, 210, "+1 555 0100", true),
            Client(2, "Meridian Health Partners", "Healthcare", "meridianhealth", SeededAccountManagerId, 180, "+1 555 0200", true),
            Client(3, "Copperleaf Coffee Roasters", "Food and beverage", "copperleaf", SeededAccountManagerId, 75, "+1 555 0300", true),
            Client(4, "Atlas Freight Co.", "Logistics", "atlasfreight", SeededAccountManagerId, 60, "+1 555 0400", true),
            Client(5, "Lumen Credit Union", "Financial services", "lumencu", "hannah", 30, "+1 555 0500", false),
            Client(6, "Fernhill Academy", "Education", "fernhill", "hannah", 21, "+1 555 0600", false),
        };
    }

    /// <summary>The hand-written client contacts.</summary>
    private static List<PbContact> Contacts() => new()
    {
        new() { Id = 1, ClientId = 1, Name = "Grace Whitfield", Title = "Head of e-commerce", Email = "grace.whitfield@harborpine.example", Phone = "+1 555 0101", IsPrimary = true, Role = PbContactRole.DecisionMaker },
        new() { Id = 2, ClientId = 1, Name = "Tom Ashby", Title = "Marketing lead", Email = "tom.ashby@harborpine.example", Phone = "+1 555 0102", Role = PbContactRole.DayToDay },
        new() { Id = 3, ClientId = 2, Name = "Dr. Anika Rao", Title = "Chief digital officer", Email = "anika.rao@meridianhealth.example", Phone = "+1 555 0201", IsPrimary = true, Role = PbContactRole.DecisionMaker },
        new() { Id = 4, ClientId = 2, Name = "Paul Hennessy", Title = "IT program manager", Email = "paul.hennessy@meridianhealth.example", Phone = "+1 555 0202", Role = PbContactRole.Technical },
        new() { Id = 5, ClientId = 3, Name = "Marta Silva", Title = "Founder", Email = "marta@copperleaf.example", Phone = "+1 555 0301", IsPrimary = true, Role = PbContactRole.DecisionMaker },
        new() { Id = 6, ClientId = 4, Name = "Rafael Moreno", Title = "Operations director", Email = "rafael.moreno@atlasfreight.example", Phone = "+1 555 0401", IsPrimary = true, Role = PbContactRole.DecisionMaker },
        new() { Id = 7, ClientId = 4, Name = "Joy Bennett", Title = "Customer service manager", Email = "joy.bennett@atlasfreight.example", Phone = "+1 555 0402", Role = PbContactRole.DayToDay },
        new() { Id = 8, ClientId = 5, Name = "Daniel Osei", Title = "VP member experience", Email = "daniel.osei@lumencu.example", Phone = "+1 555 0501", IsPrimary = true, Role = PbContactRole.DecisionMaker },
        new() { Id = 9, ClientId = 6, Name = "Ruth Adler", Title = "Director of admissions", Email = "ruth.adler@fernhill.example", Phone = "+1 555 0601", IsPrimary = true, Role = PbContactRole.DecisionMaker },
    };

    private static string Day(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>The deal pipeline in a fresh tab: four won deals that became the projects, one lost, and six open.</summary>
    private static List<PbDeal> Deals()
    {
        PbDeal Deal(int id, int clientId, string title, int value, PbDealStage stage, int position, DateTime closeDate, int createdDaysAgo, string ownerId, int? projectId = null) => new()
        {
            Id = id, ClientId = clientId, Title = title, Value = value, Stage = stage, Position = position, CloseDate = closeDate, CreatedAt = Now.Date.AddDays(-createdDaysAgo).AddHours(10),
            OwnerId = ownerId, ProjectId = projectId, ClosedAt = stage is PbDealStage.Won or PbDealStage.Lost ? closeDate.AddHours(15) : null,
        };

        return new List<PbDeal>
        {
            Deal(1, 1, "Harbor & Pine storefront redesign", 85_000, PbDealStage.Won, 1, Now.Date.AddDays(-43), 90, SeededAccountManagerId, 1),
            Deal(2, 2, "Meridian patient app", 140_000, PbDealStage.Won, 0, Now.Date.AddDays(-63), 120, SeededAccountManagerId, 2),
            Deal(3, 3, "Copperleaf brand refresh", 32_000, PbDealStage.Won, 2, Now.Date.AddDays(-27), 45, SeededAccountManagerId, 3),
            Deal(4, 4, "Atlas tracking portal", 96_000, PbDealStage.Won, 3, Now.Date.AddDays(-14), 50, SeededAccountManagerId, 4),
            Deal(5, 1, "Harbor & Pine holiday campaign", 22_000, PbDealStage.Negotiation, 0, Now.Date.AddDays(7), 20, SeededAccountManagerId),
            Deal(6, 5, "Lumen online banking UX audit", 48_000, PbDealStage.Proposal, 0, Now.Date.AddDays(21), 18, "hannah"),
            Deal(7, 6, "Fernhill admissions site", 36_000, PbDealStage.Qualified, 0, Now.Date.AddDays(57), 12, "hannah"),
            Deal(8, 2, "Meridian clinician dashboard (phase 2)", 120_000, PbDealStage.Lead, 0, Now.Date.AddDays(97), 5, SeededAccountManagerId),
            Deal(9, 3, "Copperleaf subscription shop", 28_000, PbDealStage.Proposal, 1, Now.Date.AddDays(45), 7, SeededAccountManagerId),
            Deal(10, 4, "Atlas driver app", 64_000, PbDealStage.Qualified, 1, Now.Date.AddDays(78), 4, SeededAccountManagerId),
            Deal(11, 5, "Lumen mobile app", 90_000, PbDealStage.Lead, 1, Now.Date.AddDays(116), 3, "hannah"),
            Deal(12, 6, "Fernhill parent portal", 18_000, PbDealStage.Lost, 0, Now.Date.AddDays(-35), 70, "hannah"),
        };
    }

    /// <summary>
    /// Adds the agency's other clients (84), their contacts (one to four each), and their projects of the last two years
    /// (one to four per client, with milestones and five to ten tasks each), and a little activity per client. Everything is
    /// derived from the index with a small hash, so every tab gets the same records.
    /// </summary>
    private static void Generate(List<PbPerson> people, List<PbClient> clients, List<PbContact> contacts, List<PbProject> projects, List<PbTask> tasks, List<PbActivity> activities)
    {
        var today = Now.Date;
        var accountManagers = people.Where(person => person.Title == AccountManagerTitle || person.Id == SeededAccountManagerId).Select(person => person.Id).ToArray();
        var projectManagers = people.Where(person => person.Title == ProjectManagerTitle && person.Id != SeededProjectManagerId).Select(person => person.Id).ToArray();
        var delivery = people.Where(person => person.Title is not (ProjectManagerTitle or AccountManagerTitle) && person.Id != SeededMemberId).Select(person => person.Id).ToArray();
        var names = clients.Select(client => client.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var keys = projects.Select(project => project.Key).ToHashSet(StringComparer.Ordinal);
        var nextContactId = contacts.Max(contact => contact.Id) + 1;
        var nextProjectId = projects.Max(project => project.Id) + 1;
        var nextTaskId = tasks.Max(task => task.Id) + 1;
        var nextActivityId = activities.Max(activity => activity.Id) + 1;

        for (var index = 0; clients.Count < 90; index++)
        {
            var (suffix, industry) = clientSuffixes[(index * 5 + index / clientPrefixes.Length) % clientSuffixes.Length];
            var name = $"{clientPrefixes[index % clientPrefixes.Length]} {suffix}";

            if (!names.Add(name))
                continue;

            var clientId = 100 + index;
            var slug = new string(name.ToLowerInvariant().Where(char.IsAsciiLetterLower).ToArray());
            var createdDaysAgo = 45 + Pick(index, 1, 940);
            var hasContract = Pick(index, 2, 5) != 0;
            var currency = Pick(index, 3, 10) switch { < 6 => "USD", < 8 => "EUR", 8 => "GBP", _ => index % 2 == 0 ? "CAD" : "AUD" };
            var ownerId = Pick(index, 4, 10) < 3 ? SeededAccountManagerId : accountManagers[Pick(index, 5, accountManagers.Length)];
            var client = new PbClient
            {
                Id = clientId, Name = name, Industry = industry, Website = $"{slug}.example", OwnerId = ownerId, CreatedAt = today.AddDays(-createdDaysAgo).AddHours(9 + Pick(index, 6, 8)),
                BillingCurrency = currency, BillingEmail = $"accounts@{slug}.example", Phone = Invariant($"+1 555 {1000 + Pick(index, 7, 9000):0000}"),
                ContractStart = hasContract ? today.AddDays(-createdDaysAgo + 7) : null, ContractEnd = hasContract ? today.AddDays(-createdDaysAgo + 7 + (Pick(index, 8, 2) == 0 ? 364 : 729)) : null,
            };
            clients.Add(client);

            // Contacts: the first is the primary decision maker; first and last names pair up differently for every client.
            var contactCount = 1 + Pick(index, 9, 4);
            string? primaryFirstName = null;

            for (var number = 0; number < contactCount; number++)
            {
                var first = contactFirstNames[(index * 3 + number * 7) % contactFirstNames.Length];
                var last = contactLastNames[(index * 5 + number * 11) % contactLastNames.Length];
                var role = number == 0 ? PbContactRole.DecisionMaker : (PbContactRole)((number + index) % 4 + 1);
                primaryFirstName ??= first;
                contacts.Add(new PbContact
                {
                    Id = nextContactId++, ClientId = clientId, Name = $"{first} {last}", Role = role, IsPrimary = number == 0,
                    Title = role switch
                    {
                        PbContactRole.DecisionMaker => index % 3 == 0 ? "Chief marketing officer" : index % 3 == 1 ? "Head of digital" : "Managing director",
                        PbContactRole.DayToDay => "Digital product manager",
                        PbContactRole.Billing => "Finance manager",
                        PbContactRole.Technical => "IT lead",
                        _ => "Communications officer",
                    },
                    Email = $"{new string(first.ToLowerInvariant().Where(char.IsAsciiLetterLower).ToArray())}.{new string(last.ToLowerInvariant().Where(char.IsAsciiLetterLower).ToArray())}@{slug}.example",
                    Phone = Invariant($"+1 555 {1000 + Pick(index * 10 + number, 10, 9000):0000}"),
                });
            }

            // Projects: one to four, one after another from soon after the client joined; the later ones may be planned.
            var projectCount = 1 + Pick(index, 11, 4);
            var initials = new string(name.Split(' ').Select(word => char.ToUpperInvariant(word[0])).ToArray());
            var cursor = today.AddDays(-createdDaysAgo + 10 + Pick(index, 12, 30));

            for (var number = 0; number < projectCount; number++)
            {
                var seed = index * 4 + number;
                var start = cursor > today.AddDays(40) ? today.AddDays(7 + Pick(seed, 13, 30)) : cursor;
                var due = start.AddDays(35 + Pick(seed, 14, 150));
                cursor = due.AddDays(10 + Pick(seed, 15, 90));
                var status = start > today ? PbProjectStatus.Planned
                    : due < today.AddDays(-3) ? (Pick(seed, 16, 20) == 0 ? PbProjectStatus.OnHold : PbProjectStatus.Completed)
                    : Pick(seed, 16, 10) == 0 ? PbProjectStatus.OnHold : PbProjectStatus.Active;
                var template = projectTemplates[(index * 7 + number * 3) % projectTemplates.Length];
                var key = FreeKey(initials, keys);

                if (key is null)
                    break;

                var members = new List<string>();

                for (var count = 2 + Pick(seed, 17, 4); members.Count < count;)
                {
                    var personId = delivery[Pick(seed * 13 + members.Count, 18, delivery.Length)];

                    if (!members.Contains(personId))
                        members.Add(personId);
                    else
                        count--;
                }

                if (Pick(seed, 19, 100) < 16)
                    members.Insert(0, SeededMemberId);

                var hours = 80 + Pick(seed, 20, 30) * 20;
                var project = new PbProject
                {
                    Id = nextProjectId++, Key = key, Name = $"{name} {template}", ClientId = clientId, Status = status, StartDate = start, DueDate = due,
                    Description = $"The {template} for {name}: scope, design, build, and launch with the client's team.",
                    OwnerId = Pick(seed, 21, 10) < 3 ? SeededProjectManagerId : projectManagers[Pick(seed, 22, projectManagers.Length)], MemberIds = members,
                    CreatedAt = Min(Max(start.AddDays(-(5 + Pick(seed, 39, 25))), client.CreatedAt.Date.AddDays(1)).AddHours(10), Now.AddHours(-2)), BudgetHours = hours, BudgetAmount = hours * (110 + Pick(seed, 23, 8) * 10),
                };

                if (status != PbProjectStatus.Completed)
                    project.Milestones.Add(new PbMilestone { Id = project.NextMilestoneId++, Title = "Design sign-off", Due = start.AddDays((due - start).Days * 2 / 5) });

                project.Milestones.Add(new PbMilestone { Id = project.NextMilestoneId++, Title = "Launch", Due = due });
                projects.Add(project);
                AddTasks(project, seed, tasks, ref nextTaskId);
            }

            // A little relationship activity for two clients in three.
            if (Pick(index, 24, 3) != 0)
            {
                for (var number = 0; number < 1 + Pick(index, 25, 2); number++)
                {
                    var kind = (PbActivityKind)Pick(index * 2 + number, 26, 4);
                    activities.Add(new PbActivity
                    {
                        Id = nextActivityId++, ClientId = clientId, Kind = kind, AuthorId = ownerId,
                        At = Max(today.AddDays(-(2 + Pick(index * 2 + number, 27, 88))).AddHours(9 + Pick(index, 28, 8)), client.CreatedAt.AddDays(1)),
                        Text = string.Format(CultureInfo.InvariantCulture, activityNotes[Pick(index * 2 + number, 29, activityNotes.Length)], primaryFirstName),
                    });
                }
            }
        }
    }

    /// <summary>
    /// Adds a generated project's tasks in delivery order, spread over its dates: a completed project's are done, a planned
    /// one's are to do, and an active or paused one's are done up to about today, with the rest in progress, in review, or
    /// to do (a few are overdue).
    /// </summary>
    private static void AddTasks(PbProject project, int seed, List<PbTask> tasks, ref int nextTaskId)
    {
        var today = Now.Date;
        var count = 5 + Pick(seed, 30, 6);
        var span = Math.Max(1, (project.DueDate - project.StartDate).Days);
        var positions = new Dictionary<PbTaskStatus, int>();

        for (var number = 0; number < count; number++)
        {
            var mix = seed * 17 + number;
            var title = taskTitles[(number * taskTitles.Length / count + Pick(seed, 31, 2)) % taskTitles.Length];
            var due = project.StartDate.AddDays(Math.Min(span, (number + 1) * span / count));
            var created = Min(project.CreatedAt.AddDays(number / 2).AddHours(1 + number % 5), Now.AddHours(-1));
            var status = project.Status switch
            {
                PbProjectStatus.Completed => PbTaskStatus.Done,
                PbProjectStatus.Planned => PbTaskStatus.Todo,
                PbProjectStatus.OnHold => due < today ? PbTaskStatus.Done : PbTaskStatus.Todo,
                _ when due < today.AddDays(-2) => Pick(mix, 32, 10) == 0 ? PbTaskStatus.InProgress : PbTaskStatus.Done,
                _ when due <= today.AddDays(7) => (PbTaskStatus)Pick(mix, 33, 3),
                _ => PbTaskStatus.Todo,
            };
            DateTime? completed = status == PbTaskStatus.Done ? Max(Min(due.AddHours(15 - Pick(mix, 34, 30)), Now.AddHours(-1)), created.AddHours(1)) : null;
            var position = positions.GetValueOrDefault(status);
            positions[status] = position + 1;

            tasks.Add(new PbTask
            {
                Id = nextTaskId++, ProjectId = project.Id, Number = project.NextTaskNumber++, Title = title, Status = status, Position = position,
                AssigneeId = status != PbTaskStatus.Done && Pick(mix, 35, 16) == 0 ? null : project.MemberIds[Pick(mix, 36, project.MemberIds.Count)],
                Priority = Pick(mix, 37, 50) switch { < 10 => PbTaskPriority.Low, < 35 => PbTaskPriority.Medium, < 46 => PbTaskPriority.High, _ => PbTaskPriority.Urgent },
                DueDate = due, EstimateHours = 2 + Pick(mix, 38, 12) * 2, CreatedAt = created, CompletedAt = completed,
                MilestoneId = project.Milestones.Count == 1 || number >= count * 2 / 5 ? project.Milestones[^1].Id : project.Milestones[0].Id,
            });
        }
    }

    /// <summary>A free project key from a client's initials and a letter or two, like "NLA"; <see langword="null" /> when none is left.</summary>
    private static string? FreeKey(string initials, HashSet<string> keys)
    {
        var stem = initials.Length >= 2 ? initials[..Math.Min(3, initials.Length)] : initials + "X";

        for (var first = 'A'; first <= 'Z'; first++)
        {
            if (keys.Add(stem + first))
                return stem + first;
        }

        for (var first = 'A'; first <= 'Z'; first++)
        {
            for (var second = 'A'; second <= 'Z'; second++)
            {
                if (keys.Add($"{stem}{first}{second}"))
                    return $"{stem}{first}{second}";
            }
        }

        return null;
    }

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    private static DateTime Max(DateTime left, DateTime right) => left > right ? left : right;

    private static DateTime Min(DateTime left, DateTime right) => left < right ? left : right;

    /// <summary>
    /// The demo accounts' notifications when the tab starts, newest first; each account's newest two are unread. The team
    /// member hears about tasks assigned to them, comments on them, and what's due; the project manager about a project from
    /// a won deal, tasks waiting for review, and overdue tasks in their projects; the account manager about new clients and
    /// contacts of theirs and a deal about to close.
    /// </summary>
    private static List<ShowcaseNotification> Notifications(IReadOnlyList<PbPerson> people, IReadOnlyList<PbClient> clients, IReadOnlyList<PbContact> contacts,
        IReadOnlyList<PbProject> projects, IReadOnlyList<PbTask> tasks, IReadOnlyList<PbDeal> deals, IReadOnlyList<PbTimeEntry> timeEntries, IReadOnlyList<PbTimesheet> timesheets,
        IReadOnlyList<PbInvoice> invoices)
    {
        var today = Now.Date;
        var names = people.ToDictionary(person => person.Id, person => person.Name);
        var projectsById = projects.ToDictionary(project => project.Id);
        var items = new List<(string AccountId, DateTime At, string Title, string Text, string Url)>();

        string Key(PbTask task) => Invariant($"{projectsById[task.ProjectId].Key}-{task.Number}");

        // The team member: assignments by the project manager, comments by others, and what's due.
        foreach (var task in tasks.Where(task => task.AssigneeId == SeededMemberId && task.Status != PbTaskStatus.Done && task.CreatedAt >= Now.AddDays(-14)))
        {
            items.Add((SeededMemberId, task.CreatedAt.AddMinutes(5), $"{Key(task)} is assigned to you",
                $"{names[projectsById[task.ProjectId].OwnerId]} assigned “{task.Title}” to you on {projectsById[task.ProjectId].Name}.", TaskPath(task.Id)));
        }

        foreach (var task in tasks.Where(task => task.AssigneeId == SeededMemberId))
        {
            foreach (var comment in task.Comments.Where(comment => comment.AuthorId != SeededMemberId))
                items.Add((SeededMemberId, comment.At, $"{names[comment.AuthorId]} commented on {Key(task)}", Clip(comment.Text, 160), TaskPath(task.Id)));
        }

        foreach (var task in tasks.Where(task => task.AssigneeId == SeededMemberId && task.Status != PbTaskStatus.Done && task.DueDate <= today.AddDays(1)
                     && projectsById[task.ProjectId].Status != PbProjectStatus.Completed))
        {
            var days = (task.DueDate!.Value.Date - today).Days;
            items.Add((SeededMemberId, Now.Date.AddHours(7), days switch { < 0 => $"{Key(task)} is overdue", 0 => $"{Key(task)} is due today", _ => $"{Key(task)} is due tomorrow" },
                $"“{task.Title}” on {projectsById[task.ProjectId].Name}.", TaskPath(task.Id)));
        }

        // The project manager: a project from a won deal, tasks in review, and overdue tasks in their projects.
        foreach (var deal in deals.Where(deal => deal.ProjectId is { } id && projectsById.TryGetValue(id, out var project) && project.OwnerId == SeededProjectManagerId && deal.ClosedAt >= Now.AddDays(-30)))
        {
            var project = projectsById[deal.ProjectId!.Value];
            items.Add((SeededProjectManagerId, deal.ClosedAt!.Value.AddMinutes(2), $"{project.Key} was created from a won deal",
                $"{names[deal.OwnerId]} won “{deal.Title}”. Plan its team and milestones.", Invariant($"{SiteBase}/manage/projects/{project.Id}/edit")));
        }

        foreach (var task in tasks.Where(task => task.Status == PbTaskStatus.Review && projectsById[task.ProjectId] is { OwnerId: SeededProjectManagerId, Status: PbProjectStatus.Active }))
        {
            items.Add((SeededProjectManagerId, Max(task.CreatedAt.AddHours(2), Now.AddDays(-2).AddHours(-(task.Id % 20))), $"{Key(task)} is ready for review",
                $"{(task.AssigneeId is { } assignee ? names[assignee] : "Someone")} moved “{task.Title}” to In review.", TaskPath(task.Id)));
        }

        foreach (var task in tasks.Where(task => task.Status != PbTaskStatus.Done && task.DueDate < today && projectsById[task.ProjectId] is { OwnerId: SeededProjectManagerId, Status: PbProjectStatus.Active }))
        {
            items.Add((SeededProjectManagerId, Now.Date.AddHours(7).AddMinutes(-(task.Id % 30)), $"{Key(task)} is overdue",
                $"“{task.Title}” on {projectsById[task.ProjectId].Name} was due {task.DueDate!.Value.ToString("MMM d", CultureInfo.InvariantCulture)}.", TaskPath(task.Id)));
        }

        // The account manager: new clients handed to them, contacts added to their clients, and a deal about to close.
        foreach (var client in clients.Where(client => client.OwnerId == SeededAccountManagerId && client.CreatedAt >= Now.AddDays(-120)).OrderByDescending(client => client.CreatedAt).Take(3))
        {
            items.Add((SeededAccountManagerId, client.CreatedAt.AddMinutes(10), $"You own a new client: {client.Name}", $"Hannah Brooks added {client.Name} ({client.Industry}) and made you its owner.",
                ClientPath(client.Id)));

            if (contacts.FirstOrDefault(contact => contact.ClientId == client.Id && contact.IsPrimary) is { } contact)
                items.Add((SeededAccountManagerId, client.CreatedAt.AddHours(1), $"A contact was added at {client.Name}", $"{contact.Name}, {contact.Title}.", ClientPath(client.Id)));
        }

        foreach (var deal in deals.Where(deal => deal.OwnerId == SeededAccountManagerId && deal.ClosedAt is null && deal.CloseDate <= today.AddDays(14)))
        {
            items.Add((SeededAccountManagerId, Now.Date.AddHours(7), $"“{deal.Title}” closes on {deal.CloseDate.ToString("MMM d", CultureInfo.InvariantCulture)}",
                Invariant($"${deal.Value:#,0}, in {deal.Stage}. Update the deal or move it on the pipeline."), SiteBase + "/pipeline"));
        }

        // Timesheet steps that wait for someone (a week to approve, hours to invoice, an overdue invoice) stay unread too.
        var actions = new List<(string AccountId, DateTime At, string Title, string Text, string Url, bool Action)>();
        TimesheetNotifications(actions, people, clients, projects, timeEntries, timesheets, invoices);

        var all = items.Select(item => (item.AccountId, item.At, item.Title, item.Text, item.Url, Action: false)).Concat(actions)
            .Where(item => item.At <= Now).OrderByDescending(item => item.At).ToList();
        var unread = new Dictionary<string, int>(StringComparer.Ordinal);

        return all.Select((item, index) => new ShowcaseNotification
        {
            Id = all.Count - index, AccountId = item.AccountId, At = item.At, Title = Clip(item.Title, 80), Text = Clip(item.Text, 200), Url = item.Url,
            IsRead = (unread[item.AccountId] = unread.GetValueOrDefault(item.AccountId) + 1) > 2 && !item.Action,
        }).ToList();
    }

    /// <summary>The team in a fresh tab: the three demo accounts, the hand-written teammates, and the rest of the agency, none of whom can sign in.</summary>
    private static List<PbPerson> People()
    {
        var list = new List<PbPerson>
        {
            new() { Id = SeededMemberId, AccountId = SeededMemberId, Name = "Jordan Lee", Initials = "JL", Title = "Front-end developer", CapacityHours = 32 },
            new() { Id = SeededProjectManagerId, AccountId = SeededProjectManagerId, Name = "Nadia Okafor", Initials = "NO", Title = ProjectManagerTitle, CapacityHours = 16 },
            new() { Id = SeededAccountManagerId, AccountId = SeededAccountManagerId, Name = "Elena Vasquez", Initials = "EV", Title = AccountManagerTitle, CapacityHours = 8 },
            new() { Id = "kai", Name = "Kai Nakamura", Initials = "KN", Title = "Product designer", CapacityHours = 32 },
            new() { Id = "amara", Name = "Amara Diallo", Initials = "AD", Title = "Back-end developer", CapacityHours = 32 },
            new() { Id = "ben", Name = "Ben Carter", Initials = "BC", Title = "QA engineer", CapacityHours = 30 },
            new() { Id = "lucia", Name = "Lucia Romano", Initials = "LR", Title = "Content strategist", CapacityHours = 20 },
            new() { Id = "hannah", Name = "Hannah Brooks", Initials = "HB", Title = AccountManagerTitle, CapacityHours = 8 },
        };

        (string Id, string Name, string Title, int Capacity)[] others =
        {
            ("farah", "Farah Haddad", ProjectManagerTitle, 16), ("marcus", "Marcus Webb", ProjectManagerTitle, 16), ("ingrid", "Ingrid Solberg", ProjectManagerTitle, 20),
            ("tomas", "Tomas Kral", ProjectManagerTitle, 16), ("rachel", "Rachel Kim", AccountManagerTitle, 8), ("victor", "Victor Mensah", AccountManagerTitle, 8),
            ("aisha", "Aisha Bello", AccountManagerTitle, 10), ("priya", "Priya Raman", "Product designer", 32), ("leo", "Leo Fischer", "Front-end developer", 32),
            ("mei", "Mei Tanaka", "Back-end developer", 32), ("sam", "Sam Okoro", "Full-stack developer", 32), ("nora", "Nora Lindqvist", "UX researcher", 24),
            ("ravi", "Ravi Patel", "Back-end developer", 32), ("chloe", "Chloe Martin", "Visual designer", 30), ("diego", "Diego Alvarez", "Front-end developer", 32),
            ("zoe", "Zoe Clarke", "QA engineer", 30), ("felix", "Felix Braun", "DevOps engineer", 28), ("isla", "Isla Murray", "Content strategist", 24),
            ("omar", "Omar Farouk", "Mobile developer", 32), ("hana", "Hana Sato", "Product designer", 32), ("jonas", "Jonas Weber", "Front-end developer", 32),
            ("grace", "Grace Liu", "Data analyst", 24), ("ethan", "Ethan Brooks", "QA engineer", 30), ("maya", "Maya Cohen", "Copywriter", 20),
            ("noah", "Noah Evans", "Mobile developer", 32), ("ella", "Ella Novak", "Back-end developer", 32), ("yusuf", "Yusuf Demir", "Full-stack developer", 32),
            ("lena", "Lena Hoffmann", "Motion designer", 24),
        };

        foreach (var (id, name, title, capacity) in others)
        {
            var parts = name.Split(' ');
            list.Add(new PbPerson { Id = id, Name = name, Initials = string.Concat(parts[0][..1], parts[^1][..1]), Title = title, CapacityHours = capacity });
        }

        return list;
    }

    /// <summary>Spreads a number over 0 to <paramref name="count" /> − 1, the same for the same inputs (a small integer hash).</summary>
    private static int Pick(int value, int salt, int count)
    {
        unchecked
        {
            var hash = (uint)value * 2654435761u ^ (uint)(salt + 1) * 2246822519u;
            hash ^= hash >> 15;
            hash *= 2246822519u;
            hash ^= hash >> 13;
            hash *= 3266489917u;
            hash ^= hash >> 16;

            return (int)(hash % (uint)count);
        }
    }

    private static string ProjectPath(int projectId) => Invariant($"{SiteBase}/projects/{projectId}");

    /// <summary>The hand-written projects (each came from a won deal), with their milestones and budgets.</summary>
    private static List<PbProject> Projects()
    {
        var today = Now.Date;

        PbProject Project(int id, string key, string name, int clientId, PbProjectStatus status, int startDays, int dueDays, int budgetHours, int budgetAmount, string description,
            string[] memberIds, params (string Title, int Days)[] milestones)
        {
            var project = new PbProject
            {
                Id = id, Key = key, Name = name, ClientId = clientId, Status = status, StartDate = today.AddDays(startDays), DueDate = today.AddDays(dueDays), Description = description,
                OwnerId = SeededProjectManagerId, MemberIds = memberIds.ToList(), FromDealId = id, CreatedAt = today.AddDays(Math.Min(startDays, 0) - 3).AddHours(9),
                BudgetHours = budgetHours, BudgetAmount = budgetAmount,
            };

            foreach (var milestone in milestones)
                project.Milestones.Add(new PbMilestone { Id = project.NextMilestoneId++, Title = milestone.Title, Due = today.AddDays(milestone.Days) });

            return project;
        }

        return new List<PbProject>
        {
            Project(1, "WEB", "Harbor & Pine storefront redesign", 1, PbProjectStatus.Active, -29, 24, 600, 85_000,
                "A faster, mobile-first storefront: new product pages, a simpler checkout, and a design system the client's team can extend.",
                new[] { SeededMemberId, "kai", "amara", "ben" }, ("Design sign-off", -11), ("Beta launch", 10), ("Go live", 24)),
            Project(2, "APP", "Meridian patient app", 2, PbProjectStatus.Active, -50, 66, 1000, 140_000,
                "A patient app for booking, results, and messages with care teams, on iOS and Android.",
                new[] { SeededMemberId, "amara", "ben", "kai" }, ("Clickable prototype", -20), ("MVP", 17), ("App store review", 52), ("Launch", 66)),
            Project(3, "BRD", "Copperleaf brand refresh", 3, PbProjectStatus.Active, -22, 17, 240, 32_000,
                "New logo, packaging system, and brand guidelines for Copperleaf's wholesale and retail lines.",
                new[] { "kai", "lucia", SeededMemberId }, ("Concepts", -4), ("Final assets", 17)),
            Project(4, "OPS", "Atlas tracking portal", 4, PbProjectStatus.Planned, 13, 101, 700, 96_000,
                "A self-service portal where Atlas customers track shipments and download documents.",
                new[] { "amara", SeededMemberId }, ("Discovery", 24), ("Pilot", 73)),
        };
    }

    private static string TaskPath(int taskId) => Invariant($"{SiteBase}/tasks/{taskId}");

    /// <summary>The hand-written tasks: a few for each demo project, with some done in the past weeks for the reports.</summary>
    private static List<PbTask> Tasks()
    {
        var today = Now.Date;
        var list = new List<PbTask>();
        var numbers = new Dictionary<int, int>();
        var positions = new Dictionary<(int, PbTaskStatus), int>();

        void Task(int projectId, string title, PbTaskStatus status, string? assigneeId, PbTaskPriority priority, int? dueDays, int estimate, int? milestoneId,
            int createdDaysAgo, int? completedDaysAgo = null, string description = "", string[]? checklist = null, int checklistDone = 0, (string AuthorId, int DaysAgo, string Text)[]? comments = null)
        {
            numbers[projectId] = numbers.GetValueOrDefault(projectId) + 1;
            var position = positions.GetValueOrDefault((projectId, status));
            positions[(projectId, status)] = position + 1;

            var task = new PbTask
            {
                Id = list.Count + 1, ProjectId = projectId, Number = numbers[projectId], Title = title, Status = status, Position = position, AssigneeId = assigneeId, Priority = priority,
                DueDate = dueDays is { } days ? today.AddDays(days) : null, EstimateHours = estimate, MilestoneId = milestoneId, Description = description,
                CreatedAt = today.AddDays(-createdDaysAgo).AddHours(10), CompletedAt = completedDaysAgo is { } done ? today.AddDays(-done).AddHours(16) : null,
            };

            foreach (var (text, index) in (checklist ?? Array.Empty<string>()).Select((text, index) => (text, index)))
                task.Checklist.Add(new PbChecklistItem { Id = task.NextItemId++, Text = text, Done = index < checklistDone });

            foreach (var comment in comments ?? Array.Empty<(string, int, string)>())
                task.Comments.Add(new PbComment { Id = task.NextItemId++, AuthorId = comment.AuthorId, At = today.AddDays(-comment.DaysAgo).AddHours(11), Text = comment.Text });

            list.Add(task);
        }

        // WEB: Harbor & Pine storefront redesign (milestones 1 design sign-off, 2 beta, 3 go live).
        Task(1, "Audit the current storefront's performance", PbTaskStatus.Done, "amara", PbTaskPriority.Medium, -24, 6, 1, 28, 25);
        Task(1, "Design the product page", PbTaskStatus.Done, "kai", PbTaskPriority.High, -12, 16, 1, 27, 12);
        Task(1, "Design the checkout flow", PbTaskStatus.Done, "kai", PbTaskPriority.High, -11, 12, 1, 25, 11);
        Task(1, "Set up the design system tokens", PbTaskStatus.Done, SeededMemberId, PbTaskPriority.Medium, -8, 8, 2, 20, 9);
        Task(1, "Build the product page", PbTaskStatus.InProgress, SeededMemberId, PbTaskPriority.High, 2, 16, 2, 10,
            description: "Gallery, size picker, stock message, and reviews, from Kai's design. Mobile first.",
            checklist: new[] { "Image gallery with zoom", "Size and color pickers", "Stock and delivery message", "Reviews summary", "Analytics events" }, checklistDone: 2,
            comments: new[] { ("kai", 3, "The zoom interaction is in the Figma prototype, frame 14."), (SeededMemberId, 2, "Gallery and pickers are done; starting on the stock message.") });
        Task(1, "Build the cart drawer", PbTaskStatus.Todo, SeededMemberId, PbTaskPriority.Medium, 6, 10, 2, 9,
            checklist: new[] { "Line items with quantity", "Free shipping progress", "Promo code field" });
        Task(1, "Checkout API integration", PbTaskStatus.InProgress, "amara", PbTaskPriority.Urgent, -1, 20, 2, 12,
            description: "Connect the new checkout to the payment provider's sandbox and the order service.",
            comments: new[] { ("amara", 1, "Blocked on the sandbox keys from the client; asked Grace by email.") });
        Task(1, "Accessibility review of the checkout", PbTaskStatus.Review, "ben", PbTaskPriority.High, 1, 6, 2, 6,
            checklist: new[] { "Keyboard only", "Screen reader pass", "Color contrast", "Error messages" }, checklistDone: 3);
        Task(1, "Write the launch test plan", PbTaskStatus.Todo, "ben", PbTaskPriority.Medium, 12, 6, 3, 4);
        Task(1, "Redirect map for old URLs", PbTaskStatus.Todo, null, PbTaskPriority.Low, 20, 4, 3, 3);

        // APP: Meridian patient app (milestones 1 prototype, 2 MVP, 3 store review, 4 launch).
        Task(2, "Clickable prototype for user testing", PbTaskStatus.Done, "kai", PbTaskPriority.High, -21, 24, 1, 45, 20);
        Task(2, "Run five patient interviews", PbTaskStatus.Done, "kai", PbTaskPriority.Medium, -17, 10, 1, 40, 16);
        Task(2, "Sign-in with the patient portal", PbTaskStatus.Done, "amara", PbTaskPriority.High, -6, 16, 2, 30, 6);
        Task(2, "Appointment booking screens", PbTaskStatus.InProgress, SeededMemberId, PbTaskPriority.High, 0, 18, 2, 15,
            description: "List of slots by clinic and doctor, booking, rescheduling, and cancellation.",
            checklist: new[] { "Slot list", "Booking confirmation", "Reschedule", "Cancel with reason" }, checklistDone: 1,
            comments: new[] { ("nadia", 4, "Meridian wants cancellation reasons to be optional.") });
        Task(2, "Lab results view", PbTaskStatus.Todo, SeededMemberId, PbTaskPriority.Medium, 9, 12, 2, 12);
        Task(2, "Secure messaging API", PbTaskStatus.Review, "amara", PbTaskPriority.High, -2, 20, 2, 18,
            comments: new[] { ("ben", 1, "Two failing tests on attachment size limits; details in the test run.") });
        Task(2, "Push notification setup", PbTaskStatus.Todo, "amara", PbTaskPriority.Medium, 14, 8, 2, 8);
        Task(2, "Regression suite for booking", PbTaskStatus.Todo, "ben", PbTaskPriority.Medium, 15, 10, 2, 7);
        Task(2, "App store listing copy", PbTaskStatus.Todo, null, PbTaskPriority.Low, 45, 4, 3, 5);
        Task(2, "Fix date picker on small screens", PbTaskStatus.Done, SeededMemberId, PbTaskPriority.Low, -3, 3, 2, 9, 3);

        // BRD: Copperleaf brand refresh (milestones 1 concepts, 2 final assets).
        Task(3, "Mood boards", PbTaskStatus.Done, "kai", PbTaskPriority.Medium, -15, 8, 1, 21, 15);
        Task(3, "Three logo concepts", PbTaskStatus.Done, "kai", PbTaskPriority.High, -5, 20, 1, 18, 5);
        Task(3, "Brand voice and tagline options", PbTaskStatus.Done, "lucia", PbTaskPriority.Medium, -4, 10, 1, 18, 4);
        Task(3, "Packaging system", PbTaskStatus.InProgress, "kai", PbTaskPriority.High, 8, 24, 2, 4,
            checklist: new[] { "Bag front and back", "Labels for blends", "Gift box" }, checklistDone: 1);
        Task(3, "Brand guidelines document", PbTaskStatus.Todo, "lucia", PbTaskPriority.Medium, 14, 16, 2, 4);
        Task(3, "Website hero refresh", PbTaskStatus.Todo, SeededMemberId, PbTaskPriority.Low, 16, 6, 2, 2);

        // OPS: Atlas tracking portal (planned; milestones 1 discovery, 2 pilot). Its tasks fall within its dates.
        Task(4, "Discovery workshop agenda", PbTaskStatus.Todo, "amara", PbTaskPriority.Medium, 14, 4, 1, 2);
        Task(4, "Inventory of Atlas's tracking data", PbTaskStatus.Todo, null, PbTaskPriority.Medium, 20, 8, 1, 2);

        return list;
    }

    #endregion

    #endregion
}

/// <summary>A fresh tab's data, built by <see cref="PbSeedData.Build" />.</summary>
public sealed record PbSeedSet(
    List<PbActivity> Activities,
    List<ShowcaseAuditEntry> Audit,
    List<PbClient> Clients,
    List<PbContact> Contacts,
    List<PbDeal> Deals,
    List<ShowcaseNotification> Notifications,
    List<PbPerson> People,
    List<PbProject> Projects,
    List<PbTask> Tasks,
    List<PbTimeEntry> TimeEntries,
    List<PbTimesheet> Timesheets,
    List<PbInvoice> Invoices,
    List<PbRate> Rates);
