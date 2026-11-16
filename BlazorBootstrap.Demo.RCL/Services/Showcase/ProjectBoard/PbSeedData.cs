using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

/// <summary>
/// Fixed, fictitious content for "Tandem", the project and client board of the made-up agency Brightline Studio: the team,
/// clients and their contacts, projects with milestones and tasks, the deal pipeline, and client activity. Every fresh tab
/// starts the same way.
/// </summary>
public static class PbSeedData
{
    #region Fields and Constants

    /// <summary>The identity role of account managers (clients, pipeline, forecast).</summary>
    public const string AccountManagerRole = "am";

    /// <summary>The fictitious agency.</summary>
    public const string AgencyName = "Brightline Studio";

    /// <summary>The identity role of team members (and of new accounts).</summary>
    public const string MemberRole = "member";

    /// <summary>Every signed-in role.</summary>
    public const string SignedInRoles = MemberRole + "," + ProjectManagerRole + "," + AccountManagerRole;

    /// <summary>The monthly won-revenue target of the agency, in dollars.</summary>
    public const int MonthlyTarget = 120_000;

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Tandem";

    /// <summary>The identity role of project managers (projects, workload, roadmap, reports).</summary>
    public const string ProjectManagerRole = "pm";

    /// <summary>The demo account manager's person ID.</summary>
    public const string SeededAccountManagerId = "elena";

    /// <summary>The demo member's person ID.</summary>
    public const string SeededMemberId = "jordan";

    /// <summary>The demo project manager's person ID.</summary>
    public const string SeededProjectManagerId = "nadia";

    /// <summary>When a fresh tab's clock starts (a Tuesday); the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 0, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The client activity timeline in a fresh tab.</summary>
    public static List<PbActivity> Activities()
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

    /// <summary>The client companies in a fresh tab.</summary>
    public static List<PbClient> Clients() => new()
    {
        new() { Id = 1, Name = "Harbor & Pine Outfitters", Industry = "Retail", Website = "harborpine.example", OwnerId = SeededAccountManagerId, CreatedAt = Now.AddDays(-210) },
        new() { Id = 2, Name = "Meridian Health Partners", Industry = "Healthcare", Website = "meridianhealth.example", OwnerId = SeededAccountManagerId, CreatedAt = Now.AddDays(-180) },
        new() { Id = 3, Name = "Copperleaf Coffee Roasters", Industry = "Food and beverage", Website = "copperleaf.example", OwnerId = SeededAccountManagerId, CreatedAt = Now.AddDays(-75) },
        new() { Id = 4, Name = "Atlas Freight Co.", Industry = "Logistics", Website = "atlasfreight.example", OwnerId = SeededAccountManagerId, CreatedAt = Now.AddDays(-60) },
        new() { Id = 5, Name = "Lumen Credit Union", Industry = "Financial services", Website = "lumencu.example", OwnerId = "hannah", CreatedAt = Now.AddDays(-30) },
        new() { Id = 6, Name = "Fernhill Academy", Industry = "Education", Website = "fernhill.example", OwnerId = "hannah", CreatedAt = Now.AddDays(-21) },
    };

    /// <summary>The client contacts in a fresh tab.</summary>
    public static List<PbContact> Contacts() => new()
    {
        new() { Id = 1, ClientId = 1, Name = "Grace Whitfield", Title = "Head of e-commerce", Email = "grace.whitfield@harborpine.example", Phone = "+1 555 0101", IsPrimary = true },
        new() { Id = 2, ClientId = 1, Name = "Tom Ashby", Title = "Marketing lead", Email = "tom.ashby@harborpine.example", Phone = "+1 555 0102" },
        new() { Id = 3, ClientId = 2, Name = "Dr. Anika Rao", Title = "Chief digital officer", Email = "anika.rao@meridianhealth.example", Phone = "+1 555 0201", IsPrimary = true },
        new() { Id = 4, ClientId = 2, Name = "Paul Hennessy", Title = "IT program manager", Email = "paul.hennessy@meridianhealth.example", Phone = "+1 555 0202" },
        new() { Id = 5, ClientId = 3, Name = "Marta Silva", Title = "Founder", Email = "marta@copperleaf.example", Phone = "+1 555 0301", IsPrimary = true },
        new() { Id = 6, ClientId = 4, Name = "Rafael Moreno", Title = "Operations director", Email = "rafael.moreno@atlasfreight.example", Phone = "+1 555 0401", IsPrimary = true },
        new() { Id = 7, ClientId = 4, Name = "Joy Bennett", Title = "Customer service manager", Email = "joy.bennett@atlasfreight.example", Phone = "+1 555 0402" },
        new() { Id = 8, ClientId = 5, Name = "Daniel Osei", Title = "VP member experience", Email = "daniel.osei@lumencu.example", Phone = "+1 555 0501", IsPrimary = true },
        new() { Id = 9, ClientId = 6, Name = "Ruth Adler", Title = "Director of admissions", Email = "ruth.adler@fernhill.example", Phone = "+1 555 0601", IsPrimary = true },
    };

    /// <summary>The deal pipeline in a fresh tab: four won deals that became the projects, one lost, and six open.</summary>
    public static List<PbDeal> Deals()
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

    /// <summary>The demo accounts: a team member, a project manager, and an account manager.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededMemberId, MemberRole, "Jordan Lee", "jordan.lee@example.com", "board-demo-26", "JL"),
        new(SeededProjectManagerId, ProjectManagerRole, "Nadia Okafor", "nadia.okafor@example.com", "board-pm-26", "NO"),
        new(SeededAccountManagerId, AccountManagerRole, "Elena Vasquez", "elena.vasquez@example.com", "board-am-26", "EV"),
    };

    /// <summary>The team in a fresh tab: the three demo accounts and five teammates who can't sign in.</summary>
    public static List<PbPerson> People() => new()
    {
        new() { Id = SeededMemberId, AccountId = SeededMemberId, Name = "Jordan Lee", Initials = "JL", Title = "Front-end developer", CapacityHours = 32 },
        new() { Id = SeededProjectManagerId, AccountId = SeededProjectManagerId, Name = "Nadia Okafor", Initials = "NO", Title = "Project manager", CapacityHours = 16 },
        new() { Id = SeededAccountManagerId, AccountId = SeededAccountManagerId, Name = "Elena Vasquez", Initials = "EV", Title = "Account manager", CapacityHours = 8 },
        new() { Id = "kai", Name = "Kai Nakamura", Initials = "KN", Title = "Product designer", CapacityHours = 32 },
        new() { Id = "amara", Name = "Amara Diallo", Initials = "AD", Title = "Back-end developer", CapacityHours = 32 },
        new() { Id = "ben", Name = "Ben Carter", Initials = "BC", Title = "QA engineer", CapacityHours = 30 },
        new() { Id = "lucia", Name = "Lucia Romano", Initials = "LR", Title = "Content strategist", CapacityHours = 20 },
        new() { Id = "hannah", Name = "Hannah Brooks", Initials = "HB", Title = "Account manager", CapacityHours = 8 },
    };

    /// <summary>The projects in a fresh tab (each came from a won deal), with their milestones.</summary>
    public static List<PbProject> Projects()
    {
        var today = Now.Date;

        PbProject Project(int id, string key, string name, int clientId, PbProjectStatus status, int startDays, int dueDays, string description, string[] memberIds, params (string Title, int Days)[] milestones)
        {
            var project = new PbProject
            {
                Id = id, Key = key, Name = name, ClientId = clientId, Status = status, StartDate = today.AddDays(startDays), DueDate = today.AddDays(dueDays), Description = description,
                OwnerId = SeededProjectManagerId, MemberIds = memberIds.ToList(), FromDealId = id, CreatedAt = today.AddDays(Math.Min(startDays, 0) - 3).AddHours(9),
            };

            foreach (var milestone in milestones)
                project.Milestones.Add(new PbMilestone { Id = project.NextMilestoneId++, Title = milestone.Title, Due = today.AddDays(milestone.Days) });

            return project;
        }

        return new List<PbProject>
        {
            Project(1, "WEB", "Harbor & Pine storefront redesign", 1, PbProjectStatus.Active, -29, 24,
                "A faster, mobile-first storefront: new product pages, a simpler checkout, and a design system the client's team can extend.",
                new[] { SeededMemberId, "kai", "amara", "ben" }, ("Design sign-off", -11), ("Beta launch", 10), ("Go live", 24)),
            Project(2, "APP", "Meridian patient app", 2, PbProjectStatus.Active, -50, 66,
                "A patient app for booking, results, and messages with care teams, on iOS and Android.",
                new[] { SeededMemberId, "amara", "ben", "kai" }, ("Clickable prototype", -20), ("MVP", 17), ("App store review", 52), ("Launch", 66)),
            Project(3, "BRD", "Copperleaf brand refresh", 3, PbProjectStatus.Active, -22, 17,
                "New logo, packaging system, and brand guidelines for Copperleaf's wholesale and retail lines.",
                new[] { "kai", "lucia", SeededMemberId }, ("Concepts", -4), ("Final assets", 17)),
            Project(4, "OPS", "Atlas tracking portal", 4, PbProjectStatus.Planned, 13, 101,
                "A self-service portal where Atlas customers track shipments and download documents.",
                new[] { "amara", SeededMemberId }, ("Discovery", 24), ("Pilot", 73)),
        };
    }

    /// <summary>The roles of the site, with their start pages.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(MemberRole, "Team member", "/showcase/project-board/my-tasks", "/showcase/project-board/profile", "Works their tasks on project boards: comments, checklists, and due dates."),
        new(ProjectManagerRole, "Project manager", "/showcase/project-board/manage/projects", "/showcase/project-board/profile", "Also runs projects, milestones, the team's workload, the roadmap, and reports."),
        new(AccountManagerRole, "Account manager", "/showcase/project-board/clients", "/showcase/project-board/profile", "Also manages clients and contacts, the deal pipeline, activity, and the forecast."),
    };

    /// <summary>The tasks in a fresh tab: a few for each project, with some done in the past weeks for the reports.</summary>
    public static List<PbTask> Tasks()
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

        // OPS: Atlas tracking portal (planned; milestones 1 discovery, 2 pilot).
        Task(4, "Discovery workshop agenda", PbTaskStatus.Todo, "amara", PbTaskPriority.Medium, 12, 4, 1, 2);
        Task(4, "Inventory of Atlas's tracking data", PbTaskStatus.Todo, null, PbTaskPriority.Medium, 20, 8, 1, 2);

        return list;
    }

    #endregion

    #endregion
}
