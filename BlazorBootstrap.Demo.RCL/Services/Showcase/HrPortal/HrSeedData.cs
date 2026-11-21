using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

/// <summary>
/// Fixed, fictitious content for "Teamhaven", the HR portal of the made-up company Brightfield Robotics: departments,
/// people and the org chart, leave policies and requests, company holidays, onboarding templates and plans, and the audit
/// log and notifications a fresh tab starts with. The hand-written people and requests (the demo accounts' teams) come
/// first; about 170 more people across the seven departments and three locations, and their leave history, are generated
/// from small word lists through <see cref="Mix(int)" />, so every fresh tab starts the same way.
/// </summary>
public static class HrSeedData
{
    #region Fields and Constants

    /// <summary>The fictitious company.</summary>
    public const string CompanyName = "Brightfield Robotics";

    /// <summary>The managers' and HR's leave approvals page, where seeded leave entries link to.</summary>
    internal const string ApprovalsUrl = SiteBase + "/approvals";

    /// <summary>The managers' and HR's new hires page, where seeded onboarding entries link to.</summary>
    internal const string NewHiresUrl = SiteBase + "/team/onboarding";

    /// <summary>The ID of the first generated person; the hand-written people come before.</summary>
    private const int FirstGeneratedEmployeeId = 28;

    /// <summary>The base route of the site, for the links in seeded audit entries and notifications.</summary>
    private const string SiteBase = DemoRouteConstants.Showcase_URL_HrPortal;

    /// <summary>The identity role of employees (and of new accounts).</summary>
    public const string EmployeeRole = "employee";

    /// <summary>The identity role of the HR admin.</summary>
    public const string HrAdminRole = "hradmin";

    /// <summary>The identity role of managers.</summary>
    public const string ManagerRole = "manager";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Teamhaven";

    /// <summary>The demo employee's record ID.</summary>
    public const int SeededEmployeeId = 4;

    /// <summary>The demo HR admin's record ID.</summary>
    public const int SeededHrAdminId = 12;

    /// <summary>The demo manager's record ID.</summary>
    public const int SeededManagerId = 3;

    /// <summary>Every signed-in role.</summary>
    public const string SignedInRoles = EmployeeRole + "," + ManagerRole + "," + HrAdminRole;

    /// <summary>When a fresh tab's clock starts (a Tuesday); it moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 0, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The demo accounts: an employee, a manager, and the HR admin.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new("theo", EmployeeRole, "Theo Martin", "theo.martin@example.com", "hr-demo-26", "TM"),
        new("rafael", ManagerRole, "Rafael Costa", "rafael.costa@example.com", "hr-manager-26", "RC"),
        new("ingrid", HrAdminRole, "Ingrid Larsen", "ingrid.larsen@example.com", "hr-admin-26", "IL"),
    };

    /// <summary>The departments.</summary>
    public static IReadOnlyList<HrDepartment> Departments() => new List<HrDepartment>
    {
        new("leadership", "Leadership"),
        new("engineering", "Engineering"),
        new("product", "Product"),
        new("design", "Design"),
        new("sales", "Sales"),
        new("people", "People"),
        new("finance", "Finance and operations"),
    };

    /// <summary>
    /// The people in a fresh tab: the hand-written org chart under the CEO (the demo accounts' teams, a new hire joining next
    /// week, and three who left this year), then the generated teams (<see cref="GeneratedEmployees" />).
    /// </summary>
    public static List<HrEmployee> Employees()
    {
        var list = new List<HrEmployee>();

        void Add(int id, string name, string title, string departmentId, int? managerId, string location, DateTime start, string? accountId = null, DateTime? left = null, string pronouns = "", string bio = "")
        {
            var parts = name.Split(' ');
            var email = $"{parts[0].ToLowerInvariant()}.{parts[^1].ToLowerInvariant()}@example.com";

            list.Add(new HrEmployee
            {
                Id = id, Name = name, Initials = string.Concat(parts[0][..1], parts[^1][..1]), Title = title, DepartmentId = departmentId, ManagerId = managerId, Location = location,
                StartDate = start, AccountId = accountId, Email = email, Phone = $"+1 555 01{id:00}", Pronouns = pronouns, Bio = bio, EndDate = left,
                Status = left is not null ? HrEmployeeStatus.Left : start > Now.Date ? HrEmployeeStatus.Joining : HrEmployeeStatus.Active,
                InvitePending = id == 8,
            });
        }

        Add(1, "Mara Lindqvist", "Chief executive officer", "leadership", null, "Lisbon", new DateTime(2019, 3, 1), pronouns: "she/her");
        Add(2, "Samuel Reyes", "Chief technology officer", "engineering", 1, "Lisbon", new DateTime(2019, 5, 6), pronouns: "he/him");
        Add(3, "Rafael Costa", "Engineering manager, Robotics software", "engineering", 2, "Lisbon", new DateTime(2021, 2, 1), "rafael", pronouns: "he/him",
            bio: "I lead the team that writes the software for our warehouse robots. Ask me about motion planning or coffee.");
        Add(4, "Theo Martin", "Software engineer", "engineering", 3, "Remote", new DateTime(2026, 11, 2), "theo", pronouns: "they/them",
            bio: "New on the robotics software team. I like testing tools and long bike rides.");
        Add(5, "Leila Haddad", "Senior software engineer", "engineering", 3, "Lisbon", new DateTime(2022, 9, 12), pronouns: "she/her");
        Add(6, "Jonas Berg", "Software engineer", "engineering", 3, "Remote", new DateTime(2023, 4, 3));
        Add(7, "Mei Tanaka", "QA engineer", "engineering", 3, "Austin", new DateTime(2024, 1, 15), pronouns: "she/her");
        Add(8, "Ana Pereira", "Software engineer", "engineering", 3, "Lisbon", new DateTime(2026, 12, 1));
        Add(9, "Kwame Asante", "Engineering manager, Platform", "engineering", 2, "Austin", new DateTime(2020, 8, 17), pronouns: "he/him");
        Add(10, "Sofia Marino", "Site reliability engineer", "engineering", 9, "Remote", new DateTime(2023, 10, 2));
        Add(11, "Lucas Braun", "Data engineer", "engineering", 9, "Austin", new DateTime(2026, 1, 12));
        Add(12, "Ingrid Larsen", "People partner", "people", 13, "Lisbon", new DateTime(2020, 1, 13), "ingrid", pronouns: "she/her",
            bio: "I look after leave policies, onboarding, and anything that helps people do their best work.");
        Add(13, "Carmen Vidal", "Head of people", "people", 1, "Lisbon", new DateTime(2019, 9, 2));
        Add(14, "Owen Price", "Recruiter", "people", 13, "Remote", new DateTime(2026, 5, 4));
        Add(15, "Hana Novak", "Head of product", "product", 1, "Austin", new DateTime(2020, 4, 20));
        Add(16, "Priyanka Rao", "Product manager", "product", 15, "Austin", new DateTime(2022, 2, 14));
        Add(17, "Elliot Grant", "Product designer", "design", 18, "Remote", new DateTime(2023, 7, 10));
        Add(18, "Zoe Chen", "Design lead", "design", 15, "Lisbon", new DateTime(2021, 5, 3));
        Add(19, "Marcus Hale", "Head of sales", "sales", 1, "Austin", new DateTime(2020, 11, 2));
        Add(20, "Bea Santos", "Account executive", "sales", 19, "Austin", new DateTime(2023, 3, 20));
        Add(21, "Tom Fischer", "Account executive", "sales", 19, "Lisbon", new DateTime(2026, 7, 13));
        Add(22, "Nina Ivanova", "Finance manager", "finance", 1, "Lisbon", new DateTime(2021, 8, 16));
        Add(23, "Felix Wright", "Accountant", "finance", 22, "Remote", new DateTime(2026, 3, 2));
        Add(24, "Oscar Lund", "Office manager", "finance", 22, "Lisbon", new DateTime(2022, 5, 9));
        Add(25, "Ravi Menon", "Software engineer", "engineering", 3, "Remote", new DateTime(2022, 1, 10), left: new DateTime(2026, 8, 28));
        Add(26, "Clara Nunes", "Account executive", "sales", 19, "Austin", new DateTime(2023, 2, 1), left: new DateTime(2026, 6, 30));
        Add(27, "Paulo Dias", "Product designer", "design", 18, "Lisbon", new DateTime(2024, 3, 4), left: new DateTime(2026, 2, 27));

        // The newest starters are still on probation: six months from their first day.
        foreach (var recent in list.Where(item => item.Id is SeededEmployeeId or 8 or 21))
            recent.ProbationEndDate = recent.StartDate.AddMonths(6);

        list.AddRange(GeneratedEmployees(list.Select(item => item.Email).ToHashSet(StringComparer.Ordinal)));

        return list;
    }

    /// <summary>The company holidays (no leave day is counted on them).</summary>
    public static IReadOnlyList<HrHoliday> Holidays() => new List<HrHoliday>
    {
        new(new DateTime(2026, 1, 1), "New Year's Day"),
        new(new DateTime(2026, 4, 3), "Spring holiday"),
        new(new DateTime(2026, 7, 3), "Summer holiday"),
        new(new DateTime(2026, 11, 26), "Thanksgiving"),
        new(new DateTime(2026, 12, 24), "Christmas Eve"),
        new(new DateTime(2026, 12, 25), "Christmas Day"),
        new(new DateTime(2027, 1, 1), "New Year's Day"),
        new(new DateTime(2027, 4, 2), "Spring holiday"),
    };

    /// <summary>The leave policies in a fresh tab.</summary>
    public static List<HrLeavePolicy> LeavePolicies() => new()
    {
        new() { Id = 1, Name = "Vacation", Description = "Paid time off to rest and travel.", AnnualDays = 25, MinNoticeDays = 7 },
        new() { Id = 2, Name = "Sick leave", Description = "When you're unwell. No limit; tell your manager as soon as you can.", IsUnlimited = true },
        new() { Id = 3, Name = "Personal days", Description = "For moving house, appointments, or family matters.", AnnualDays = 3, MinNoticeDays = 2 },
        new() { Id = 4, Name = "Volunteer day", Description = "One paid day a year to volunteer for a cause you care about.", AnnualDays = 1, MinNoticeDays = 7 },
    };

    /// <summary>
    /// The leave requests in a fresh tab: the hand-written ones (the robotics team's, with two waiting for Rafael), then the
    /// generated people's history (<see cref="GeneratedLeave" />).
    /// </summary>
    public static List<HrLeaveRequest> LeaveRequests()
    {
        var id = 1;
        var list = new List<HrLeaveRequest>();

        void Add(int employeeId, int policyId, DateTime from, DateTime to, int days, HrLeaveStatus status, int createdDaysAgo, string note = "", int? deciderId = null, string decisionNote = "")
        {
            var created = Now.Date.AddDays(-createdDaysAgo).AddHours(10);
            list.Add(new HrLeaveRequest
            {
                Id = id++, EmployeeId = employeeId, PolicyId = policyId, From = from, To = to, Days = days, Status = status, Note = note, CreatedAt = created,
                DecidedAt = status is HrLeaveStatus.Approved or HrLeaveStatus.Rejected ? created.AddHours(26) : null, DecidedById = status is HrLeaveStatus.Approved or HrLeaveStatus.Rejected ? deciderId : null,
                DecisionNote = decisionNote,
            });
        }

        Add(SeededManagerId, 1, new DateTime(2026, 10, 5), new DateTime(2026, 10, 9), 5, HrLeaveStatus.Approved, 70, "Family trip.", 2);
        Add(SeededEmployeeId, 2, new DateTime(2026, 11, 13), new DateTime(2026, 11, 13), 1, HrLeaveStatus.Approved, 11, "Flu.", SeededManagerId, "Get well soon!");
        Add(5, 1, new DateTime(2026, 8, 3), new DateTime(2026, 8, 14), 10, HrLeaveStatus.Approved, 120, "Summer holiday.", SeededManagerId);
        Add(5, 1, new DateTime(2026, 12, 21), new DateTime(2026, 12, 23), 3, HrLeaveStatus.Pending, 6, "Visiting family before the holidays.");
        Add(6, 3, new DateTime(2026, 12, 4), new DateTime(2026, 12, 4), 1, HrLeaveStatus.Pending, 3, "Moving to a new flat.");
        Add(7, 1, new DateTime(2026, 12, 28), new DateTime(2026, 12, 31), 4, HrLeaveStatus.Approved, 30, "End-of-year break.", SeededManagerId);
        Add(6, 1, new DateTime(2026, 6, 15), new DateTime(2026, 6, 26), 10, HrLeaveStatus.Approved, 180, string.Empty, SeededManagerId);
        Add(10, 2, new DateTime(2026, 9, 21), new DateTime(2026, 9, 22), 2, HrLeaveStatus.Approved, 64, string.Empty, 9);
        Add(16, 1, new DateTime(2026, 7, 6), new DateTime(2026, 7, 17), 10, HrLeaveStatus.Approved, 160, string.Empty, 15);
        Add(20, 1, new DateTime(2026, 12, 14), new DateTime(2026, 12, 18), 5, HrLeaveStatus.Rejected, 20, "Ski trip.", 19, "That's the last week of the quarter. Could you take the first week of January instead?");
        Add(21, 3, new DateTime(2026, 11, 20), new DateTime(2026, 11, 20), 1, HrLeaveStatus.Cancelled, 12);

        list.AddRange(GeneratedLeave(Employees().Where(item => item.Id >= FirstGeneratedEmployeeId), id));

        return list;
    }

    /// <summary>
    /// The audit log a fresh tab starts with, numbered oldest first: the last month's leave requests and decisions, the
    /// HR admin adding the newest hires, marking people as left, and starting onboarding, the ticked onboarding tasks, and a
    /// leave policy change.
    /// </summary>
    public static List<ShowcaseAuditEntry> AuditEntries(IReadOnlyList<HrEmployee> employees, IReadOnlyList<HrLeaveRequest> leave, IReadOnlyList<HrOnboarding> onboardings,
        IReadOnlyList<HrLeavePolicy> policies, IEnumerable<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>? more = null)
    {
        var byId = employees.ToDictionary(item => item.Id);
        var hrName = byId[SeededHrAdminId].Name;
        var since = Now.AddDays(-30);
        var lines = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();

        string NameOf(int? id) => id is { } key && byId.TryGetValue(key, out var person) ? person.Name : CompanyName;

        foreach (var request in leave)
        {
            var details = $"{NameOf(request.EmployeeId)}: {policies.FirstOrDefault(policy => policy.Id == request.PolicyId)?.Name ?? "Leave"}, {Range(request.From, request.To)}";

            if (request.CreatedAt >= since && request.CreatedAt <= Now)
                lines.Add((request.CreatedAt, NameOf(request.EmployeeId), "Requested", LeaveTarget(request.Id), ApprovalsUrl, details));

            if (request.DecidedAt is { } decided && decided >= since && decided <= Now)
                lines.Add((decided, NameOf(request.DecidedById), request.Status == HrLeaveStatus.Approved ? "Approved" : "Rejected", LeaveTarget(request.Id), ApprovalsUrl, details));
        }

        foreach (var person in employees.Where(item => item.Id != SeededHrAdminId && item.StartDate >= Now.Date.AddDays(-75)))
        {
            var added = person.StartDate.AddDays(-24).AddHours(11);

            if (added <= Now)
                lines.Add((added, hrName, "Added", $"Employee: {person.Name}", EmployeeEditUrl(person.Id), $"{person.Title}, starts {person.StartDate.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}"));
        }

        foreach (var person in employees.Where(item => item.EndDate is { } end && end >= Now.Date.AddDays(-120)))
        {
            var marked = person.EndDate!.Value.AddDays(1).AddHours(9);

            if (marked <= Now)
                lines.Add((marked, hrName, "Marked left", $"Employee: {person.Name}", EmployeeEditUrl(person.Id), $"Last day {person.EndDate.Value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}"));
        }

        foreach (var plan in onboardings)
        {
            lines.Add((plan.CreatedAt, hrName, "Started onboarding", $"Onboarding: {NameOf(plan.EmployeeId)}", NewHiresUrl, $"Template “{plan.TemplateName}”"));

            foreach (var task in plan.Tasks.Where(task => task.DoneAt is { } done && done <= Now))
                lines.Add((task.DoneAt!.Value, NameOf(task.DoneById), "Ticked off", $"Onboarding: {NameOf(plan.EmployeeId)}", NewHiresUrl, task.Title));
        }

        lines.Add((Now.AddDays(-12).AddHours(2), hrName, "Updated", "Leave policy: Personal days", SiteBase + "/admin/leave-policies", "Minimum notice 1 → 2 days"));

        if (more is not null)
            lines.AddRange(more.Where(line => line.At <= Now));

        return lines
            .OrderByDescending(line => line.At)
            .Take(240)
            .OrderBy(line => line.At)
            .Select((line, index) => new ShowcaseAuditEntry
            {
                Id = index + 1,
                At = line.At,
                Actor = line.Actor,
                Action = line.Action,
                Target = Clip(line.Target),
                TargetUrl = line.Url,
                Details = line.Details is null ? null : Clip(line.Details),
            })
            .ToList();
    }

    /// <summary>
    /// The notifications a fresh tab starts with, for each demo account: the manager's direct reports' waiting requests and
    /// the new hire's onboarding, the employee's decided leave and their onboarding plan, and for the HR admin the waiting
    /// requests of people whose manager can't sign in. The two newest of each account are unread.
    /// </summary>
    public static List<ShowcaseNotification> Notifications(IReadOnlyList<HrEmployee> employees, IReadOnlyList<HrLeaveRequest> leave, IReadOnlyList<HrOnboarding> onboardings,
        IReadOnlyList<HrLeavePolicy> policies, IEnumerable<(string AccountId, DateTime At, string Title, string? Text, string Url)>? more = null)
    {
        var extra = more?.Where(item => item.At <= Now).ToList() ?? new List<(string AccountId, DateTime At, string Title, string? Text, string Url)>();
        var byId = employees.ToDictionary(item => item.Id);
        var items = new List<(string AccountId, DateTime At, string Title, string? Text, string Url, bool IsRead)>();

        string PolicyName(int policyId) => policies.FirstOrDefault(policy => policy.Id == policyId)?.Name ?? "Leave";

        foreach (var account in DemoAccounts())
        {
            if (employees.FirstOrDefault(item => item.AccountId == account.Id) is not { } me)
                continue;

            var mine = new List<(DateTime At, string Title, string? Text, string Url)>();

            foreach (var request in leave.Where(item => item.Status == HrLeaveStatus.Pending && item.CreatedAt <= Now && item.EmployeeId != me.Id))
            {
                var employee = byId[request.EmployeeId];
                var managerAccount = employee.ManagerId is { } managerId && byId.TryGetValue(managerId, out var manager) ? manager.AccountId : null;
                var isMine = account.RoleId == ManagerRole ? employee.ManagerId == me.Id : account.RoleId == HrAdminRole && managerAccount is null;

                if (isMine)
                    mine.Add((request.CreatedAt, $"{employee.Name} requested leave", LeaveSummary(PolicyName(request.PolicyId), request), ApprovalsUrl));
            }

            foreach (var request in leave.Where(item => item.EmployeeId == me.Id && item.DecidedAt is { } at && at <= Now))
            {
                var approved = request.Status == HrLeaveStatus.Approved;
                mine.Add((request.DecidedAt!.Value, $"Your leave request was {(approved ? "approved" : "rejected")}",
                    $"{PolicyName(request.PolicyId)}, {Range(request.From, request.To)}" + (request.DecisionNote.Length > 0 ? $" · {request.DecisionNote}" : null), SiteBase + "/leave"));
            }

            foreach (var plan in onboardings.Where(plan => plan.CreatedAt <= Now))
            {
                var hire = byId[plan.EmployeeId];

                if (hire.Id == me.Id)
                    mine.Add((plan.CreatedAt, "Your onboarding plan is ready", $"{plan.TemplateName}: {plan.Tasks.Count} tasks", SiteBase + "/onboarding"));
                else if (account.RoleId == ManagerRole && hire.ManagerId == me.Id)
                    mine.Add((plan.CreatedAt, $"Onboarding started for {hire.Name}", $"Starts {hire.StartDate.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}", NewHiresUrl));
            }

            mine.AddRange(extra.Where(item => item.AccountId == account.Id).Select(item => (item.At, item.Title, item.Text, item.Url)));

            items.AddRange(mine
                .OrderByDescending(item => item.At)
                .Take(8)
                .Select((item, index) => (account.Id, item.At, item.Title, item.Text, item.Url, IsRead: index >= 2)));
        }

        return items
            .OrderBy(item => item.At)
            .Select((item, index) => new ShowcaseNotification
            {
                Id = index + 1,
                AccountId = item.AccountId,
                At = item.At,
                Title = Clip(item.Title),
                Text = item.Text is null ? null : Clip(item.Text),
                Url = item.Url,
                IsRead = item.IsRead,
            })
            .ToList();
    }

    /// <summary>The office locations (and remote).</summary>
    public static IReadOnlyList<string> Locations() => new[] { "Lisbon", "Austin", "Remote" };

    /// <summary>The onboarding plans in a fresh tab: Theo's (started three weeks ago) and Ana's (joining next week).</summary>
    public static List<HrOnboarding> Onboardings()
    {
        var templates = OnboardingTemplates();
        var theo = Plan(1, SeededEmployeeId, new DateTime(2026, 11, 2), templates[0], new DateTime(2026, 10, 22));
        var ana = Plan(2, 8, new DateTime(2026, 12, 1), templates[0], new DateTime(2026, 11, 20));

        // Theo: everything before day one and the first week is done, except security training; the two-week check-in is late.
        foreach (var task in theo.Tasks.Where(task => task.Due <= new DateTime(2026, 11, 3)))
            Tick(task, task.Owner);

        Tick(theo.Tasks.Single(task => task.Title.StartsWith("Ship", StringComparison.Ordinal)), HrTaskOwner.NewHire);

        // Ana: HR sent the welcome email.
        Tick(ana.Tasks[0], HrTaskOwner.Hr);

        return new List<HrOnboarding> { theo, ana };

        static void Tick(HrOnboardingTask task, HrTaskOwner owner)
        {
            task.DoneAt = task.Due.AddHours(15);
            task.DoneById = owner switch { HrTaskOwner.NewHire => SeededEmployeeId, HrTaskOwner.Manager => SeededManagerId, _ => SeededHrAdminId };
        }
    }

    /// <summary>The onboarding templates in a fresh tab.</summary>
    public static List<HrOnboardingTemplate> OnboardingTemplates()
    {
        HrOnboardingTemplate Template(int id, string name, string description, params (HrTaskOwner Owner, int Offset, string Title)[] tasks)
        {
            var template = new HrOnboardingTemplate { Id = id, Name = name, Description = description };

            foreach (var task in tasks)
                template.Tasks.Add(new HrTemplateTask { Id = template.NextTaskId++, Owner = task.Owner, DueOffsetDays = task.Offset, Title = task.Title });

            return template;
        }

        return new List<HrOnboardingTemplate>
        {
            Template(1, "Engineering new hire", "Accounts, a buddy, security training, and a first change shipped in week one.",
                (HrTaskOwner.Hr, -7, "Send the welcome email and contract"),
                (HrTaskOwner.Hr, -3, "Order the laptop and create accounts"),
                (HrTaskOwner.Manager, -2, "Pick an onboarding buddy"),
                (HrTaskOwner.NewHire, 0, "Read and sign the employee handbook"),
                (HrTaskOwner.NewHire, 0, "Set up your laptop and two-factor sign-in"),
                (HrTaskOwner.Manager, 1, "First 1:1 and a 30-60-90 day plan"),
                (HrTaskOwner.NewHire, 3, "Complete the security training"),
                (HrTaskOwner.NewHire, 5, "Ship a first small change"),
                (HrTaskOwner.Manager, 14, "Two-week check-in"),
                (HrTaskOwner.Hr, 30, "Send the 30-day feedback survey")),
            Template(2, "General new hire", "The basics for anyone joining outside engineering.",
                (HrTaskOwner.Hr, -7, "Send the welcome email and contract"),
                (HrTaskOwner.Hr, -3, "Order the laptop and create accounts"),
                (HrTaskOwner.NewHire, 0, "Read and sign the employee handbook"),
                (HrTaskOwner.Manager, 1, "First 1:1 and goals for the first month"),
                (HrTaskOwner.NewHire, 5, "Meet someone from each department"),
                (HrTaskOwner.Manager, 14, "Two-week check-in"),
                (HrTaskOwner.Hr, 30, "Send the 30-day feedback survey")),
        };
    }

    /// <summary>Builds a plan from a template for someone starting on a day.</summary>
    public static HrOnboarding Plan(int id, int employeeId, DateTime startDate, HrOnboardingTemplate template, DateTime createdAt)
    {
        var plan = new HrOnboarding { Id = id, EmployeeId = employeeId, TemplateName = template.Name, CreatedAt = createdAt };
        var taskId = 1;

        foreach (var task in template.Tasks.OrderBy(task => task.DueOffsetDays).ThenBy(task => task.Id))
            plan.Tasks.Add(new HrOnboardingTask { Id = taskId++, Title = task.Title, Owner = task.Owner, Due = startDate.Date.AddDays(task.DueOffsetDays) });

        return plan;
    }

    /// <summary>The roles of the site, with their start pages.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(EmployeeRole, "Employee", "/showcase/hr-portal/home", "/showcase/hr-portal/profile", "Finds colleagues, requests leave, and works through their onboarding checklist."),
        new(ManagerRole, "Manager", "/showcase/hr-portal/team", "/showcase/hr-portal/profile", "Also sees their team, approves or rejects leave, and follows new hires' onboarding."),
        new(HrAdminRole, "HR admin", "/showcase/hr-portal/admin/employees", "/showcase/hr-portal/profile", "Also manages employees, leave policies, onboarding templates, and headcount analytics."),
    };

    #endregion

    #region Internal Methods

    /// <summary>Shortens a text to the audit log's and notifications' limit.</summary>
    internal static string Clip(string text, int maxLength = 200) => text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength - 1), "…");

    /// <summary>Formats a day count like "1 day" or "3 days".</summary>
    internal static string Days(int days) => days == 1 ? "1 day" : string.Create(CultureInfo.InvariantCulture, $"{days} days");

    /// <summary>The HR admin's editor of someone's record, where audit entries about people link to.</summary>
    internal static string EmployeeEditUrl(int employeeId) => FormattableString.Invariant($"{SiteBase}/admin/employees/{employeeId}/edit");

    /// <summary>A leave request in one line, like "Vacation, Dec 21–23, 2026 · 3 days".</summary>
    internal static string LeaveSummary(string policyName, HrLeaveRequest request) => $"{policyName}, {Range(request.From, request.To)} · {Days(request.Days)}";

    /// <summary>The audit log's name for a leave request, like "Leave request #42".</summary>
    internal static string LeaveTarget(int requestId) => FormattableString.Invariant($"Leave request #{requestId}");

    /// <summary>Formats a leave range like "Dec 21–23, 2026" or "Dec 28, 2026 – Jan 1, 2027" (the wording of the pages' <c>HrDisplay.Range</c>).</summary>
    internal static string Range(DateTime from, DateTime to)
    {
        static string Day(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

        return from.Date == to.Date ? Day(from)
            : from.Year == to.Year && from.Month == to.Month ? FormattableString.Invariant($"{from.ToString("MMM d", CultureInfo.InvariantCulture)}–{to.Day}, {to.Year}")
            : from.Year == to.Year ? FormattableString.Invariant($"{from.ToString("MMM d", CultureInfo.InvariantCulture)} – {to.ToString("MMM d", CultureInfo.InvariantCulture)}, {to.Year}")
            : $"{Day(from)} – {Day(to)}";
    }

    /// <summary>Counts the working days of a range: weekdays that aren't company holidays (the count <c>HrState.WorkingDays</c> makes).</summary>
    internal static int WorkingDays(DateTime from, DateTime to, IReadOnlyList<HrHoliday> holidays)
    {
        var count = 0;

        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && holidays.All(holiday => holiday.Date != day))
                count++;
        }

        return count;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// The generated people: one entry of <see cref="GeneratedTeams" /> at a time, its new lead first (reporting to a
    /// hand-written manager), then its members, who take the team's titles in turn and mostly its location (one in four
    /// works somewhere else). Start dates spread from the team's start to four weeks ago. About one in fifteen who have been
    /// here half a year left on a Friday since, about one in twenty-five joins in the next six weeks, and anyone who started
    /// in the last six months is on probation until six months after their first day.
    /// </summary>
    private static List<HrEmployee> GeneratedEmployees(HashSet<string> takenEmails)
    {
        var firstNames = new[]
        {
            "Aisha", "Ben", "Chloe", "Daniel", "Elena", "Farid", "Grace", "Hugo", "Ines", "Jamal", "Kira", "Liam", "Maya", "Noah", "Olga", "Pedro",
            "Rosa", "Sven", "Tara", "Umar", "Vera", "Will", "Ximena", "Yara", "Zane", "Amira", "Bruno", "Carla", "Dmitri", "Esther", "Finn", "Gabriela",
            "Henrik", "Ivy", "Joao", "Keiko", "Luca", "Marta", "Nikhil", "Omar", "Petra", "Rui", "Sana", "Tobias", "Ursula", "Victor", "Wen", "Zainab",
        };
        var lastNames = new[]
        {
            "Almeida", "Baker", "Carvalho", "Dalton", "Edwards", "Ferreira", "Garcia", "Holm", "Ito", "Jansen", "Khan", "Lopes", "Martins", "Nguyen", "Okafor", "Patel",
            "Ortiz", "Park", "Rahman", "Silva", "Torres", "Usman", "Vos", "Walsh", "Xu", "Yilmaz", "Zhou", "Andersen", "Brooks", "Cruz", "Dahl", "Esposito",
            "Fonseca", "Gomes", "Hughes", "Iqbal", "Jovanovic", "Kim", "Larsson", "Moreno", "Nilsson", "Owens", "Pinto", "Reid", "Sato", "Teixeira", "Varga", "Wagner",
            "Young", "Ward",
        };
        var locations = Locations();
        var teamStart = new DateTime(2019, 6, 3);
        var list = new List<HrEmployee>();
        var id = FirstGeneratedEmployeeId;
        var nameIndex = 0;

        // Names step through both lists, so they read as a mix; a name whose email is taken is skipped.
        (string Name, string Email) NextName()
        {
            while (true)
            {
                var index = nameIndex++;
                var first = firstNames[index * 7 % firstNames.Length];
                var last = lastNames[((index * 11) + (index / firstNames.Length)) % lastNames.Length];
                var email = $"{first}.{last}@example.com".ToLowerInvariant();

                if (takenEmails.Add(email))
                    return ($"{first} {last}", email);
            }
        }

        HrEmployee Person(string title, string departmentId, int managerId, string location, DateTime start)
        {
            var (name, email) = NextName();
            var parts = name.Split(' ');
            var person = new HrEmployee
            {
                Id = id, Name = name, Email = email, Initials = string.Concat(parts[0][..1], parts[^1][..1]), Title = title, DepartmentId = departmentId, ManagerId = managerId,
                Location = location, StartDate = start, Phone = FormattableString.Invariant($"+1 555 {2000 + id}"),
                Status = start > Now.Date ? HrEmployeeStatus.Joining : HrEmployeeStatus.Active,
            };
            id++;
            list.Add(person);

            return person;
        }

        foreach (var team in GeneratedTeams())
        {
            var leadId = team.ParentId;
            var leadStart = teamStart;

            if (team.LeadTitle is not null)
            {
                leadStart = teamStart.AddDays(7 * (int)(Mix(id * 13) % 220));
                leadId = Person(team.LeadTitle, team.DepartmentId, team.ParentId, team.Location, leadStart).Id;
            }

            var weeks = Math.Max(1, (int)((Now.Date.AddDays(-28) - leadStart).TotalDays / 7));

            for (var index = 0; index < team.Size; index++)
            {
                var seed = id;
                var location = Mix(seed * 3) % 4 == 0 ? locations[(int)(Mix(seed * 37) % (ulong)locations.Count)] : team.Location;
                var joining = Mix(seed * 19) % 25 == 3;
                var start = joining ? NextMonday(Now.Date).AddDays(7 * (int)(Mix(seed * 41) % 6)) : leadStart.AddDays(7 * (1 + (int)(Mix(seed * 17) % (ulong)weeks)));
                var person = Person(team.Titles[index % team.Titles.Length], team.DepartmentId, leadId, location, start);

                if (!joining && Mix(seed * 23) % 15 == 4 && start <= Now.Date.AddDays(-200))
                {
                    var span = (int)(Now.Date.AddDays(-21) - start.AddDays(150)).TotalDays;
                    var end = start.AddDays(150 + (int)(Mix(seed * 29) % (ulong)span));

                    while (end.DayOfWeek != DayOfWeek.Friday)
                        end = end.AddDays(-1);

                    person.Status = HrEmployeeStatus.Left;
                    person.EndDate = end;
                }
                else if (person.StartDate > Now.Date.AddMonths(-6))
                {
                    person.ProbationEndDate = person.StartDate.AddMonths(6);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// The generated people's leave, numbered from <paramref name="firstId" />: most took a week or two of vacation earlier
    /// this year, some a day or two off sick or a volunteer day; of those still here, about a quarter have vacation or a
    /// personal day waiting for a decision, some have approved or rejected vacation ahead, and a few cancelled a personal day.
    /// A person's waiting and approved requests never share a day, and nothing falls before their first or after their last day.
    /// </summary>
    private static List<HrLeaveRequest> GeneratedLeave(IEnumerable<HrEmployee> people, int firstId)
    {
        var holidays = Holidays();
        var yearStart = new DateTime(2026, 1, 5);
        var futureWeeks = new[]
        {
            new DateTime(2026, 12, 7), new DateTime(2026, 12, 14), new DateTime(2027, 1, 11), new DateTime(2027, 1, 18), new DateTime(2027, 1, 25),
            new DateTime(2027, 2, 8), new DateTime(2027, 2, 15),
        };
        var vacationNotes = new[] { "Summer holiday.", string.Empty, "Family visit.", string.Empty, "A week by the sea.", string.Empty, "Hiking trip." };
        var waitingNotes = new[] { "Visiting family.", string.Empty, "Long weekend away.", "School holidays.", string.Empty, "Moving house." };
        var rejectionNotes = new[]
        {
            "That's the last week of the quarter. Could you take the week after instead?",
            "Two others on the team are away then. Could you pick another week?",
            "We have the customer launch that week. Let's find another date together.",
        };
        var list = new List<HrLeaveRequest>();
        var id = firstId;

        foreach (var person in people.Where(item => item.Status != HrEmployeeStatus.Joining))
        {
            var mine = new List<HrLeaveRequest>();
            var seed = person.Id;
            var lastDay = person.EndDate ?? Now.Date.AddYears(1);

            void Add(int policyId, DateTime from, DateTime to, HrLeaveStatus status, DateTime created, string note, string decisionNote = "")
            {
                var days = WorkingDays(from, to, holidays);

                if (days == 0 || from < person.StartDate || to > lastDay || from.Year != to.Year || created > Now.AddHours(-2) || created < person.StartDate)
                    return;

                if (status is HrLeaveStatus.Pending or HrLeaveStatus.Approved && mine.Any(other => other.Status is HrLeaveStatus.Pending or HrLeaveStatus.Approved && other.From <= to && from <= other.To))
                    return;

                DateTime? decided = status is HrLeaveStatus.Approved or HrLeaveStatus.Rejected ? created.AddHours(20 + (int)(Mix(id * 3) % 30)) : null;

                if (decided > Now.AddHours(-1))
                    decided = Now.AddHours(-1);

                mine.Add(new HrLeaveRequest
                {
                    Id = id++, EmployeeId = person.Id, PolicyId = policyId, From = from, To = to, Days = days, Status = status, Note = note, CreatedAt = created,
                    DecidedAt = decided, DecidedById = decided is null ? null : person.ManagerId, DecisionNote = decisionNote,
                });
            }

            if (Mix(seed * 41) % 10 < 7)
            {
                var from = yearStart.AddDays(7 * (4 + (int)(Mix(seed * 43) % 38)));
                var length = new[] { 3, 5, 5, 10 }[Mix(seed * 47) % 4];
                var to = from.AddDays(length == 10 ? 11 : length - 1);

                if (to < Now.Date)
                    Add(1, from, to, HrLeaveStatus.Approved, from.AddDays(-(10 + (int)(Mix(seed * 53) % 25))).AddHours(9 + (int)(Mix(seed * 7) % 7)), vacationNotes[Mix(seed * 11) % (ulong)vacationNotes.Length]);
            }

            // Half also took a long weekend or a short week.
            if (Mix(seed * 157) % 10 < 5)
            {
                var from = yearStart.AddDays(7 * (2 + (int)(Mix(seed * 163) % 44))).AddDays(Mix(seed * 167) % 2 == 0 ? 3 : 0);
                var to = from.AddDays(from.DayOfWeek == DayOfWeek.Thursday ? 1 : 2);

                if (to < Now.Date)
                    Add(1, from, to, HrLeaveStatus.Approved, from.AddDays(-(8 + (int)(Mix(seed * 173) % 20))).AddHours(10 + (int)(Mix(seed * 179) % 6)), string.Empty);
            }

            if (Mix(seed * 59) % 20 < 9)
            {
                var day = yearStart.AddDays((7 * (int)(Mix(seed * 61) % 46)) + (int)(Mix(seed * 67) % 5));
                var to = Mix(seed * 71) % 3 == 0 ? day.AddDays(1) : day;

                if (to < Now.Date)
                    Add(2, day, to, HrLeaveStatus.Approved, day.AddHours(8).AddMinutes(30), Mix(seed * 73) % 2 == 0 ? "Unwell." : string.Empty);
            }

            if (Mix(seed * 79) % 10 == 0)
            {
                var day = yearStart.AddDays((7 * (6 + (int)(Mix(seed * 83) % 36))) + 4);

                if (day < Now.Date)
                    Add(4, day, day, HrLeaveStatus.Approved, day.AddDays(-14).AddHours(10), "Beach clean-up with a local charity.");
            }

            if (person.Status != HrEmployeeStatus.Left)
            {
                var pick = (int)(Mix(seed * 89) % 100);
                var week = futureWeeks[Mix(seed * 97) % (ulong)futureWeeks.Length];
                var to = week.AddDays(Mix(seed * 101) % 2 == 0 ? 4 : 2);
                var created = Now.AddDays(-(1 + (int)(Mix(seed * 103) % 12))).AddHours((int)(Mix(seed * 107) % 8)).AddMinutes((int)(Mix(seed * 109) % 60));

                if (pick < 24)
                {
                    Add(1, week, to, HrLeaveStatus.Pending, created, waitingNotes[Mix(seed * 109) % (ulong)waitingNotes.Length]);
                }
                else if (pick < 31)
                {
                    var day = new DateTime(2026, 12, 2).AddDays((int)(Mix(seed * 113) % 16));
                    Add(3, day, day, HrLeaveStatus.Pending, created, Mix(seed * 127) % 2 == 0 ? "Doctor's appointment." : string.Empty);
                }
                else if (pick < 46)
                {
                    Add(1, week, to, HrLeaveStatus.Approved, Now.AddDays(-(15 + (int)(Mix(seed * 131) % 25))), vacationNotes[Mix(seed * 137) % (ulong)vacationNotes.Length]);
                }
                else if (pick < 53)
                {
                    Add(1, week, week.AddDays(4), HrLeaveStatus.Rejected, Now.AddDays(-(4 + (int)(Mix(seed * 139) % 10))), "End-of-year trip.",
                        rejectionNotes[Mix(seed * 149) % (ulong)rejectionNotes.Length]);
                }
                else if (pick < 57)
                {
                    var day = yearStart.AddDays((7 * (10 + (int)(Mix(seed * 151) % 30))) + 2);

                    if (day < Now.Date)
                        Add(3, day, day, HrLeaveStatus.Cancelled, day.AddDays(-6).AddHours(10), string.Empty);
                }
            }

            list.AddRange(mine);
        }

        return list;
    }

    /// <summary>
    /// The generated teams, department by department: who leads each (a generated lead with a title, or a hand-written
    /// manager), the hand-written manager the lead reports to, the team's main location, how many people it has, and their
    /// titles. The org chart stays a tree under the CEO, and the demo manager's own team is left as written.
    /// </summary>
    private static IEnumerable<SeedTeam> GeneratedTeams() => new SeedTeam[]
    {
        new(null, 9, "engineering", "Austin", 8, new[] { "Site reliability engineer", "Platform engineer", "Senior platform engineer", "Data engineer", "Security engineer" }),
        new("Engineering manager, Perception", 2, "engineering", "Lisbon", 13, new[] { "Computer vision engineer", "Senior software engineer", "Software engineer", "Machine learning engineer", "QA engineer" }),
        new("Engineering manager, Firmware", 2, "engineering", "Austin", 12, new[] { "Firmware engineer", "Senior firmware engineer", "Embedded software engineer", "Hardware test engineer" }),
        new("Engineering manager, Fleet cloud", 2, "engineering", "Remote", 12, new[] { "Backend engineer", "Senior backend engineer", "Frontend engineer", "Software engineer", "QA engineer" }),
        new("Engineering manager, Mobile robotics", 2, "engineering", "Lisbon", 11, new[] { "Robotics engineer", "Senior robotics engineer", "Motion planning engineer", "Software engineer" }),
        new("Hardware engineering manager", 2, "engineering", "Austin", 10, new[] { "Mechanical engineer", "Electrical engineer", "Senior mechanical engineer", "Manufacturing engineer" }),
        new("Group product manager, Fleet", 15, "product", "Austin", 5, new[] { "Product manager", "Senior product manager", "Product analyst" }),
        new("Group product manager, Platform", 15, "product", "Lisbon", 5, new[] { "Product manager", "Senior product manager", "Product analyst" }),
        new(null, 18, "design", "Lisbon", 7, new[] { "Product designer", "Senior product designer", "UX researcher", "Content designer" }),
        new("Design manager, Research", 18, "design", "Remote", 4, new[] { "UX researcher", "Senior UX researcher" }),
        new("Sales manager, Europe", 19, "sales", "Lisbon", 11, new[] { "Account executive", "Senior account executive", "Sales development representative", "Solutions engineer" }),
        new("Sales manager, North America", 19, "sales", "Austin", 12, new[] { "Account executive", "Senior account executive", "Sales development representative", "Solutions engineer" }),
        new("Head of customer success", 19, "sales", "Remote", 10, new[] { "Customer success manager", "Support engineer", "Implementation specialist" }),
        new(null, 19, "sales", "Austin", 3, new[] { "Sales operations analyst", "Revenue operations manager", "Partnerships manager" }),
        new(null, 13, "people", "Lisbon", 3, new[] { "People partner", "People operations specialist", "Learning and development partner" }),
        new("Talent acquisition lead", 13, "people", "Remote", 5, new[] { "Recruiter", "Senior recruiter", "Recruiting coordinator" }),
        new(null, 22, "finance", "Lisbon", 5, new[] { "Accountant", "Financial analyst", "Payroll specialist", "Procurement specialist" }),
        new("Operations manager", 22, "finance", "Austin", 9, new[] { "Supply chain planner", "Warehouse coordinator", "Logistics specialist", "Facilities coordinator", "IT support specialist" }),
        new(null, 1, "leadership", "Lisbon", 2, new[] { "Chief of staff", "Executive assistant" }),
    };

    /// <summary>A deterministic number from 0 to 999 for a seed value, so generated data is the same in every tab.</summary>
    private static ulong Mix(int value)
    {
        var x = (ulong)(value + 0x9E37) * 0x9E3779B97F4A7C15UL;
        x ^= x >> 29;

        return x % 1000;
    }

    /// <summary>The first Monday after a day.</summary>
    private static DateTime NextMonday(DateTime day)
    {
        var next = day.AddDays(1);

        while (next.DayOfWeek != DayOfWeek.Monday)
            next = next.AddDays(1);

        return next;
    }

    #endregion

    #endregion

    #region Nested Types

    /// <summary>One generated team: a lead (a new person with <paramref name="LeadTitle" />, or the hand-written manager <paramref name="ParentId" /> when it's <see langword="null" />) and its members.</summary>
    /// <param name="LeadTitle">The generated lead's title, or <see langword="null" /> when the hand-written manager leads the team.</param>
    /// <param name="ParentId">The hand-written manager the lead reports to (or who leads the team).</param>
    /// <param name="DepartmentId">The team's department.</param>
    /// <param name="Location">The team's main location.</param>
    /// <param name="Size">How many members it has, not counting the lead.</param>
    /// <param name="Titles">The members' titles, taken in turn.</param>
    private sealed record SeedTeam(string? LeadTitle, int ParentId, string DepartmentId, string Location, int Size, string[] Titles);

    #endregion
}
