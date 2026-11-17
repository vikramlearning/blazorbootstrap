using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

/// <summary>
/// Fixed, fictitious content for "Teamhaven", the HR portal of the made-up company Brightfield Robotics: departments,
/// people and the org chart, leave policies and requests, company holidays, and onboarding templates and plans. Every
/// fresh tab starts the same way.
/// </summary>
public static class HrSeedData
{
    #region Fields and Constants

    /// <summary>The fictitious company.</summary>
    public const string CompanyName = "Brightfield Robotics";

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

    /// <summary>The people in a fresh tab: the org chart under the CEO, a new hire joining next week, and three who left this year.</summary>
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

    /// <summary>The leave requests in a fresh tab: the robotics team's, with two waiting for Rafael.</summary>
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

        return list;
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

    #endregion
}
