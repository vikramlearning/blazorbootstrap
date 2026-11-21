using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

/// <summary>
/// State of "Teamhaven", the HR portal of Brightfield Robotics, for the current browser tab: people and the org chart, leave
/// policies and requests, onboarding templates and plans, and the demo sign-in. It is kept in the tab's
/// <c>sessionStorage</c> through the showcase foundation; nothing is sent anywhere. It also keeps the enterprise baseline
/// (rule WR-001): an audit log of every create, change, decision, and delete, in-app notifications for the people who must
/// act, and the header's global search.
/// </summary>
/// <remarks>
/// Every signed-in person is linked to their employee record (a new account takes up the record with its work email, if one
/// is waiting). An employee requests, edits, and cancels their own leave and ticks their own onboarding tasks. A manager
/// also decides their direct reports' leave and ticks the manager's onboarding tasks for them. The HR admin also manages
/// employees, leave policies, and onboarding templates, starts onboarding, and may decide any leave or tick any task.
/// Leave days are weekdays that aren't company holidays; balances are per calendar year.
/// </remarks>
public sealed partial class HrState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>The longest bio.</summary>
    public const int MaxBioLength = 300;

    /// <summary>The longest leave note or decision note.</summary>
    public const int MaxNoteLength = 300;

    /// <summary>The most days a leave policy can give a year.</summary>
    public const int MaxAnnualDays = 60;

    /// <summary>The most employees.</summary>
    public const int MaxEmployees = 400;

    /// <summary>The longest probation period, in months from the start date.</summary>
    public const int MaxProbationMonths = 12;

    /// <summary>The longest leave request, in calendar days.</summary>
    public const int MaxLeaveSpanDays = 60;

    /// <summary>The longest name, title, policy name, template name, or task title.</summary>
    public const int MaxNameLength = 80;

    /// <summary>The longest description.</summary>
    public const int MaxDescriptionLength = 200;

    /// <summary>The longest minimum notice for a policy, in days.</summary>
    public const int MaxNoticeDays = 60;

    /// <summary>The longest phone number.</summary>
    public const int MaxPhoneLength = 30;

    /// <summary>The most leave policies.</summary>
    public const int MaxPolicies = 12;

    /// <summary>The longest pronouns.</summary>
    public const int MaxPronounsLength = 20;

    /// <summary>The most onboarding templates.</summary>
    public const int MaxTemplates = 20;

    /// <summary>The most tasks in a template.</summary>
    public const int MaxTemplateTasks = 30;

    /// <summary>How far before or after day one a template task may be due, in days.</summary>
    public const int MaxTaskOffsetDays = 90;

    /// <summary>The most audit entries kept; the oldest go first.</summary>
    private const int MaxAuditEntries = 500;

    /// <summary>The longest audit or notification text (action, record, details, title).</summary>
    private const int MaxAuditTextLength = 200;

    /// <summary>The most notifications kept, for every account together; the oldest go first.</summary>
    private const int MaxNotifications = 200;

    /// <summary>The site's base route (the same as <c>HrLayout.Base</c>); audit, notification, and search links stay under it.</summary>
    private const string SiteBase = DemoRouteConstants.Showcase_URL_HrPortal;

    /// <summary>The leave approvals page (managers and HR), where audit entries and notifications about waiting leave link to.</summary>
    private const string ApprovalsLink = SiteBase + "/approvals";

    /// <summary>The employee's own leave page.</summary>
    private const string MyLeaveLink = SiteBase + "/leave";

    /// <summary>The new hire's own onboarding checklist.</summary>
    private const string MyOnboardingLink = SiteBase + "/onboarding";

    /// <summary>The managers' and HR's new hires page, where onboarding progress is followed.</summary>
    private const string NewHiresLink = SiteBase + "/team/onboarding";

    /// <summary>The HR admin's leave policies page.</summary>
    private const string PoliciesLink = SiteBase + "/admin/leave-policies";

    /// <summary>The manager's team page.</summary>
    private const string TeamLink = SiteBase + "/team";

    /// <summary>The HR admin's onboarding templates page.</summary>
    private const string TemplatesLink = SiteBase + "/admin/onboarding-templates";

    private const int MaxLeaveRequests = 1500;

    private const int MaxOnboardings = 250;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int NextIdLimit = 1_000_000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<ShowcaseAuditEntry> audit = new();

    private readonly List<HrEmployee> employees = HrSeedData.Employees();

    private readonly List<HrLeaveRequest> leaveRequests = HrSeedData.LeaveRequests();

    private readonly List<ShowcaseNotification> notifications = new();

    private readonly List<HrOnboarding> onboardings = HrSeedData.Onboardings();

    private readonly List<HrLeavePolicy> policies = HrSeedData.LeavePolicies();

    private readonly List<HrOnboardingTemplate> templates = HrSeedData.OnboardingTemplates();

    private int nextAuditId;

    private int nextEmployeeId;

    private int nextLeaveId;

    private int nextNotificationId;

    private int nextOnboardingId;

    private int nextPolicyId;

    private int nextTemplateId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public HrState()
    {
        nextEmployeeId = employees.Max(item => item.Id) + 1;
        nextLeaveId = leaveRequests.Max(item => item.Id) + 1;
        nextOnboardingId = onboardings.Max(item => item.Id) + 1;
        nextPolicyId = policies.Max(item => item.Id) + 1;
        nextTemplateId = templates.Max(item => item.Id) + 1;
        nextCycleId = cycles.Max(item => item.Id) + 1;
        nextReviewId = reviews.Max(item => item.Id) + 1;
        audit.AddRange(HrSeedData.AuditEntries(employees, leaveRequests, onboardings, policies, HrReviewSeedData.AuditLines(employees, cycles, reviews)));
        notifications.AddRange(HrSeedData.Notifications(employees, leaveRequests, onboardings, policies, HrReviewSeedData.NotificationLines(employees, cycles, reviews)));
        nextAuditId = audit.Count == 0 ? 1 : audit.Max(entry => entry.Id) + 1;
        nextNotificationId = notifications.Count == 0 ? 1 : notifications.Max(notification => notification.Id) + 1;
        Identity = new ShowcaseIdentity(HrSeedData.Roles(), HrSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Returns an employee's leave balances for the current year, one per active policy (themselves, their manager, or HR).
    /// </summary>
    public IReadOnlyList<HrBalance> Balances(int employeeId)
    {
        if (!CanSeeLeave(employeeId))
            return Array.Empty<HrBalance>();

        var year = Clock.Today.Year;

        return policies.Where(policy => policy.IsActive || leaveRequests.Any(request => request.EmployeeId == employeeId && request.PolicyId == policy.Id && request.From.Year == year))
            .OrderBy(policy => policy.Id)
            .Select(policy => Balance(employeeId, policy, year))
            .ToList();
    }

    /// <summary>
    /// Cancels one of the signed-in employee's own requests: a pending one, or an approved one that hasn't started yet. The
    /// days go back to the balance.
    /// </summary>
    public HrResult CancelLeave(int requestId)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        if (leaveRequests.FirstOrDefault(item => item.Id == requestId && item.EmployeeId == me.Id) is not { } request)
            return HrResult.NotFound;

        if (!(request.Status == HrLeaveStatus.Pending || (request.Status == HrLeaveStatus.Approved && request.From > Clock.Today)))
            return HrResult.WrongState;

        var previous = request.Status;
        request.Status = HrLeaveStatus.Cancelled;

        return Commit(() => request.Status = previous, () =>
        {
            Record("Cancelled", HrSeedData.LeaveTarget(request.Id), ApprovalsLink, LeaveDetails(request) + (previous == HrLeaveStatus.Approved ? "; was approved" : "; was waiting"));

            // Approved leave was planned around: whoever approved it hears it's off. A waiting request just leaves their queue.
            if (previous == HrLeaveStatus.Approved)
                Notify(AccountOf(request.DecidedById), $"{me.Name} cancelled approved leave", HrSeedData.LeaveSummary(PolicyName(request.PolicyId), request), TeamLink);
        });
    }

    /// <summary>Returns the snapshot: every collection, the review cycles, the audit log and notifications, and the ID counters.</summary>
    public object CaptureData() => new HrSnapshot
    {
        Audit = audit.ToList(),
        Cycles = cycles.ToList(),
        Employees = employees.ToList(),
        LeaveRequests = leaveRequests.ToList(),
        NextAuditId = nextAuditId,
        NextCycleId = nextCycleId,
        NextEmployeeId = nextEmployeeId,
        NextLeaveId = nextLeaveId,
        NextNotificationId = nextNotificationId,
        NextOnboardingId = nextOnboardingId,
        NextPolicyId = nextPolicyId,
        NextReviewId = nextReviewId,
        NextTemplateId = nextTemplateId,
        Notifications = notifications.ToList(),
        Onboardings = onboardings.ToList(),
        Policies = policies.ToList(),
        Reviews = reviews.ToList(),
        Templates = templates.ToList(),
    };

    /// <summary>
    /// Approves or rejects a pending request (the employee's manager, or the HR admin; not your own). Rejecting needs a note
    /// for the employee; approving checks the balance still covers it (InsufficientBalance). The employee is notified.
    /// </summary>
    public HrResult DecideLeave(int requestId, bool approve, string note)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        var result = TryDecide(me, requestId, approve, note, out var decided, out var undo);

        return result != HrResult.Ok ? result : Commit(undo!, () => RecordDecision(decided!, approve, bulk: false));
    }

    /// <summary>
    /// Approves or rejects several pending requests with the same note (the approvals queue's bulk actions), with the checks
    /// of <see cref="DecideLeave" /> for each one. Returns each request's result; the ones that pass are decided together,
    /// in one save, and each employee is notified.
    /// </summary>
    public IReadOnlyList<(int RequestId, HrResult Result)> DecideLeaveMany(IEnumerable<int> requestIds, bool approve, string note)
    {
        var ids = requestIds.Distinct().ToList();

        if (Me() is not { } me)
            return ids.Select(id => (id, HrResult.NotAllowed)).ToList();

        var results = new List<(int RequestId, HrResult Result)>();
        var decided = new List<HrLeaveRequest>();
        var undos = new List<Action>();

        foreach (var id in ids)
        {
            var result = TryDecide(me, id, approve, note, out var request, out var undo);
            results.Add((id, result));

            if (result == HrResult.Ok)
            {
                decided.Add(request!);
                undos.Add(undo!);
            }
        }

        if (decided.Count == 0)
            return results;

        var saved = Commit(() =>
        {
            for (var index = undos.Count - 1; index >= 0; index--)
                undos[index]();
        }, () =>
        {
            foreach (var request in decided)
                RecordDecision(request, approve, bulk: decided.Count > 1);
        });

        return saved == HrResult.Ok ? results : results.Select(item => (item.RequestId, item.Result == HrResult.Ok ? saved : item.Result)).ToList();
    }

    /// <summary>Deletes a leave policy no request uses (HR admin); one in use can only be turned off.</summary>
    public HrResult DeletePolicy(int policyId)
    {
        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        if (FindPolicy(policyId) is not { } policy)
            return HrResult.NotFound;

        if (leaveRequests.Any(request => request.PolicyId == policyId))
            return HrResult.WrongState;

        policies.Remove(policy);
        Record("Deleted", $"Leave policy: {policy.Name}", PoliciesLink, PolicySummary(policy));
        Changed?.Invoke();

        return HrResult.Ok;
    }

    /// <summary>Deletes an onboarding template (HR admin); plans already started from it keep their tasks.</summary>
    public HrResult DeleteTemplate(int templateId)
    {
        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        if (FindTemplate(templateId) is not { } template)
            return HrResult.NotFound;

        templates.Remove(template);
        Record("Deleted", $"Onboarding template: {template.Name}", TemplatesLink, string.Create(CultureInfo.InvariantCulture, $"{template.Tasks.Count} {(template.Tasks.Count == 1 ? "task" : "tasks")}; plans already started keep theirs"));
        Changed?.Invoke();

        return HrResult.Ok;
    }

    /// <summary>Removes a task from an onboarding template (HR admin); plans already started keep it.</summary>
    public HrResult DeleteTemplateTask(int templateId, int taskId)
    {
        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        if (FindTemplate(templateId)?.Tasks.FirstOrDefault(item => item.Id == taskId) is not { } task)
            return HrResult.NotFound;

        var template = FindTemplate(templateId)!;
        template.Tasks.Remove(task);
        Record("Removed a task", $"Onboarding template: {template.Name}", TemplateLink(template.Id), $"“{task.Title}”");
        Changed?.Invoke();

        return HrResult.Ok;
    }

    /// <summary>Returns the people who report to someone (not those who left), by name.</summary>
    public IReadOnlyList<HrEmployee> DirectReports(int employeeId) =>
        IsSignedIn ? employees.Where(item => item.ManagerId == employeeId && item.Status != HrEmployeeStatus.Left).OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList() : Array.Empty<HrEmployee>();

    /// <summary>Finds a department, or returns <see langword="null" />.</summary>
    public HrDepartment? FindDepartment(string? departmentId) => Departments.FirstOrDefault(item => item.Id == departmentId);

    /// <summary>
    /// Finds someone the signed-in person may see: anyone in the directory (not those who left); the HR admin sees everyone.
    /// </summary>
    public HrEmployee? FindEmployee(int employeeId) =>
        IsSignedIn ? employees.FirstOrDefault(item => item.Id == employeeId && (item.Status != HrEmployeeStatus.Left || IsHrAdmin)) : null;

    /// <summary>Finds a leave request the signed-in person may see (their own, their direct report's, or any for HR), or returns <see langword="null" />.</summary>
    public HrLeaveRequest? FindLeave(int requestId) => leaveRequests.FirstOrDefault(item => item.Id == requestId && CanSeeLeave(item.EmployeeId));

    /// <summary>Finds a leave policy, or returns <see langword="null" />.</summary>
    public HrLeavePolicy? FindPolicy(int policyId) => policies.FirstOrDefault(item => item.Id == policyId);

    /// <summary>Finds an onboarding template, or returns <see langword="null" />.</summary>
    public HrOnboardingTemplate? FindTemplate(int templateId) => templates.FirstOrDefault(item => item.Id == templateId);

    /// <summary>Returns the signed-in account's notifications, newest first; empty when nobody is signed in.</summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Identity.Current is { } account
            ? notifications.Where(notification => notification.AccountId == account.Id).OrderByDescending(notification => notification.At).ThenByDescending(notification => notification.Id).ToList()
            : Array.Empty<ShowcaseNotification>();

    /// <summary>Returns an employee's leave requests, newest first (themselves, their manager, or HR).</summary>
    public IReadOnlyList<HrLeaveRequest> LeaveOf(int employeeId) =>
        CanSeeLeave(employeeId) ? leaveRequests.Where(item => item.EmployeeId == employeeId).OrderByDescending(item => item.From).ToList() : Array.Empty<HrLeaveRequest>();

    /// <summary>
    /// Marks someone as having left on a day, today or earlier (HR admin): not while people still report to them (WrongState:
    /// move them first). Their pending leave is cancelled.
    /// </summary>
    public HrResult MarkLeft(int employeeId, DateTime lastDay)
    {
        if (!IsHrAdmin || Me() is not { } me)
            return HrResult.NotAllowed;

        if (FindEmployeeRecord(employeeId) is not { } employee)
            return HrResult.NotFound;

        if (employee.Id == me.Id || employee.AccountId is not null && Identity.Current?.Id == employee.AccountId)
            return HrResult.WrongState;

        if (employee.Status == HrEmployeeStatus.Left || employees.Any(item => item.ManagerId == employeeId && item.Status != HrEmployeeStatus.Left))
            return HrResult.WrongState;

        // A last day in the future isn't scheduled: HR marks someone as left on or after their last day.
        if (lastDay.Date < employee.StartDate || lastDay.Date > Clock.Today || !IsPlausible(lastDay))
            return HrResult.Invalid;

        var previous = (employee.Status, employee.EndDate, employee.InvitePending);
        var cancelled = leaveRequests.Where(item => item.EmployeeId == employeeId && item.Status == HrLeaveStatus.Pending).ToList();
        employee.Status = HrEmployeeStatus.Left;
        employee.EndDate = lastDay.Date;
        employee.InvitePending = false;

        foreach (var request in cancelled)
            request.Status = HrLeaveStatus.Cancelled;

        return Commit(() =>
        {
            (employee.Status, employee.EndDate, employee.InvitePending) = previous;

            foreach (var request in cancelled)
                request.Status = HrLeaveStatus.Pending;
        }, () =>
        {
            Record("Marked left", EmployeeTarget(employee), HrSeedData.EmployeeEditUrl(employee.Id), $"Last day {Day(lastDay)}"
                + (cancelled.Count == 0 ? null : string.Create(CultureInfo.InvariantCulture, $"; {cancelled.Count} waiting leave {(cancelled.Count == 1 ? "request" : "requests")} cancelled")));
            Notify(AccountOf(employee.ManagerId), $"{employee.Name} has left {HrSeedData.CompanyName}", $"Last day {Day(lastDay)}", TeamLink);
        });
    }

    /// <summary>Marks every notification of the signed-in account as read.</summary>
    public void MarkAllNotificationsRead()
    {
        if (Identity.Current is not { } account)
            return;

        var unread = notifications.Where(notification => notification.AccountId == account.Id && !notification.IsRead).ToList();

        if (unread.Count == 0)
            return;

        foreach (var notification in unread)
            notification.IsRead = true;

        Changed?.Invoke();
    }

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (Identity.Current is not { } account
            || notifications.FirstOrDefault(notification => notification.Id == id && notification.AccountId == account.Id) is not { IsRead: false } notification)
            return;

        notification.IsRead = true;
        Changed?.Invoke();
    }

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Returns an employee's onboarding plan (themselves, their manager, or HR), or <see langword="null" />.</summary>
    public HrOnboarding? OnboardingOf(int employeeId) => CanSeeLeave(employeeId) ? onboardings.FirstOrDefault(item => item.EmployeeId == employeeId) : null;

    /// <summary>Returns how many of a plan's tasks are done, and how many it has.</summary>
    public static (int Done, int Total) Progress(HrOnboarding plan) => (plan.Tasks.Count(task => task.DoneAt is not null), plan.Tasks.Count);

    /// <summary>
    /// Requests leave for the signed-in employee. Invalid: dates out of order, across two years, longer than
    /// <see cref="MaxLeaveSpanDays" />, no working days, a turned-off policy, or less notice than the policy needs;
    /// Overlap: another pending or approved request covers a day; InsufficientBalance: not enough days left. Whoever decides
    /// it is notified: the manager, or the HR admin when the manager can't sign in.
    /// </summary>
    public HrResult RequestLeave(int policyId, DateTime from, DateTime to, string note, out HrLeaveRequest? requested)
    {
        requested = null;

        if (Me() is not { } me)
            return HrResult.NotAllowed;

        if (FindPolicy(policyId) is not { } policy)
            return HrResult.NotFound;

        if (leaveRequests.Count >= MaxLeaveRequests || nextLeaveId >= NextIdLimit)
            return HrResult.Invalid;

        var check = CheckLeave(me.Id, policy, from, to, note, null, out var days);

        if (check != HrResult.Ok)
            return check;

        var request = new HrLeaveRequest
        {
            Id = nextLeaveId++, EmployeeId = me.Id, PolicyId = policy.Id, From = from.Date, To = to.Date, Days = days, Note = note.Trim(), Status = HrLeaveStatus.Pending, CreatedAt = Clock.Now,
        };
        leaveRequests.Add(request);
        var result = Commit(() =>
        {
            leaveRequests.Remove(request);
            nextLeaveId--;
        }, () =>
        {
            Record("Requested", HrSeedData.LeaveTarget(request.Id), ApprovalsLink, LeaveDetails(request));
            NotifyApprovers(me, $"{me.Name} requested leave", HrSeedData.LeaveSummary(policy.Name, request));
        });

        if (result == HrResult.Ok)
            requested = request;

        return result;
    }

    /// <summary>
    /// Creates (<paramref name="employeeId" /> 0) or edits an employee (HR admin): unique work email, an existing department,
    /// a manager who works here and isn't in their own reporting line, and an optional probation end date after the start date
    /// (up to <see cref="MaxProbationMonths" /> months later). Someone starting after today is "Joining". The manager hears of
    /// a new report.
    /// </summary>
    public HrResult SaveEmployee(int employeeId, HrEmployeeInput input, out HrEmployee? saved)
    {
        saved = null;

        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        var existing = employeeId == 0 ? null : FindEmployeeRecord(employeeId);

        if (employeeId != 0 && existing is null)
            return HrResult.NotFound;

        if (existing?.Status == HrEmployeeStatus.Left)
            return HrResult.WrongState;

        var name = input?.Name?.Trim() ?? string.Empty;
        var email = input?.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var title = input?.Title?.Trim() ?? string.Empty;
        var phone = input?.Phone?.Trim() ?? string.Empty;

        if (input is null || !IsText(name, MaxNameLength) || !IsEmail(email) || !IsText(title, MaxNameLength) || FindDepartment(input.DepartmentId) is null || !Locations.Contains(input.Location)
            || phone.Length > MaxPhoneLength || !IsPlausible(input.StartDate) || !IsProbation(input.ProbationEndDate, input.StartDate))
            return HrResult.Invalid;

        if (input.ManagerId is { } managerId && (FindEmployeeRecord(managerId) is not { Status: not HrEmployeeStatus.Left } || (existing is not null && ReportsTo(managerId, existing.Id))))
            return HrResult.Invalid;

        if (employees.Any(item => item != existing && item.Email == email))
            return HrResult.Duplicate;

        if (existing is null)
        {
            if (employees.Count >= MaxEmployees || nextEmployeeId >= NextIdLimit)
                return HrResult.Invalid;

            var employee = new HrEmployee { Id = nextEmployeeId++, InvitePending = true };
            Apply(employee, name, email, title, phone, input);
            employees.Add(employee);
            var result = Commit(() =>
            {
                employees.Remove(employee);
                nextEmployeeId--;
            }, () =>
            {
                Record("Added", EmployeeTarget(employee), HrSeedData.EmployeeEditUrl(employee.Id), $"{employee.Title}, {FindDepartment(employee.DepartmentId)!.Name}, starts {Day(employee.StartDate)}");
                Notify(AccountOf(employee.ManagerId), $"{employee.Name} joins your team", $"{employee.Title} · starts {Day(employee.StartDate)}", TeamLink);
            });

            if (result == HrResult.Ok)
                saved = employee;

            return result;
        }

        if (existing.Name == name && existing.Email == email && existing.Title == title && existing.Phone == phone && existing.DepartmentId == input.DepartmentId
            && existing.ManagerId == input.ManagerId && existing.Location == input.Location && existing.StartDate == input.StartDate.Date
            && existing.ProbationEndDate == input.ProbationEndDate?.Date)
            return HrResult.NoChange;

        var before = new HrEmployeeInput(existing.Name, existing.Email, existing.Title, existing.DepartmentId, existing.ManagerId, existing.Location, existing.StartDate, existing.Phone,
            existing.ProbationEndDate);
        var changes = EmployeeChanges(before, input with { Name = name, Email = email, Title = title, Phone = phone });
        Apply(existing, name, email, title, phone, input);
        saved = existing;

        return Commit(() => Apply(existing, before.Name, before.Email, before.Title, before.Phone, before), () =>
        {
            Record("Updated", EmployeeTarget(existing), HrSeedData.EmployeeEditUrl(existing.Id), changes);

            if (existing.ManagerId != before.ManagerId)
                Notify(AccountOf(existing.ManagerId), $"{existing.Name} now reports to you", existing.Title, TeamLink);
        });
    }

    /// <summary>Saves the signed-in employee's own phone, pronouns, and bio (plain text).</summary>
    public HrResult SaveMyProfile(string phone, string pronouns, string bio)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        phone = phone?.Trim() ?? string.Empty;
        pronouns = pronouns?.Trim() ?? string.Empty;
        bio = bio?.Trim() ?? string.Empty;

        if (phone.Length > MaxPhoneLength || pronouns.Length > MaxPronounsLength || bio.Length > MaxBioLength)
            return HrResult.Invalid;

        if (me.Phone == phone && me.Pronouns == pronouns && me.Bio == bio)
            return HrResult.NoChange;

        var previous = (me.Phone, me.Pronouns, me.Bio);
        var fields = new[] { (Name: "phone", Changed: me.Phone != phone), (Name: "pronouns", Changed: me.Pronouns != pronouns), (Name: "bio", Changed: me.Bio != bio) }
            .Where(field => field.Changed).Select(field => field.Name).ToList();
        (me.Phone, me.Pronouns, me.Bio) = (phone, pronouns, bio);

        return Commit(() => (me.Phone, me.Pronouns, me.Bio) = previous,
            () => Record("Updated profile", EmployeeTarget(me), PersonLink(me.Id), "Changed " + string.Join(", ", fields)));
    }

    /// <summary>
    /// Creates (<paramref name="policyId" /> 0) or edits a leave policy (HR admin): unique name, days a year (or unlimited),
    /// minimum notice, and whether it's offered for new requests. Balances follow at once.
    /// </summary>
    public HrResult SavePolicy(int policyId, string name, string description, int annualDays, bool isUnlimited, int minNoticeDays, bool isActive, out HrLeavePolicy? saved)
    {
        saved = null;

        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        var existing = policyId == 0 ? null : FindPolicy(policyId);

        if (policyId != 0 && existing is null)
            return HrResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;
        annualDays = isUnlimited ? 0 : annualDays;

        if (!IsText(name, MaxNameLength) || description.Length > MaxDescriptionLength || annualDays is < 0 or > MaxAnnualDays || (!isUnlimited && annualDays == 0) || minNoticeDays is < 0 or > MaxNoticeDays)
            return HrResult.Invalid;

        if (policies.Any(item => item != existing && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
            return HrResult.Duplicate;

        if (existing is null)
        {
            if (policies.Count >= MaxPolicies || nextPolicyId >= NextIdLimit)
                return HrResult.Invalid;

            var policy = new HrLeavePolicy { Id = nextPolicyId++, Name = name, Description = description, AnnualDays = annualDays, IsUnlimited = isUnlimited, MinNoticeDays = minNoticeDays, IsActive = isActive };
            policies.Add(policy);
            var result = Commit(() =>
            {
                policies.Remove(policy);
                nextPolicyId--;
            }, () => Record("Added", $"Leave policy: {policy.Name}", PoliciesLink, PolicySummary(policy)));

            if (result == HrResult.Ok)
                saved = policy;

            return result;
        }

        if (existing.Name == name && existing.Description == description && existing.AnnualDays == annualDays && existing.IsUnlimited == isUnlimited && existing.MinNoticeDays == minNoticeDays
            && existing.IsActive == isActive)
            return HrResult.NoChange;

        var previous = (existing.Name, existing.Description, existing.AnnualDays, existing.IsUnlimited, existing.MinNoticeDays, existing.IsActive);
        var policyChanges = new List<string>();

        if (existing.Name != name)
            policyChanges.Add($"Name {existing.Name} → {name}");

        if (existing.IsUnlimited != isUnlimited || existing.AnnualDays != annualDays)
            policyChanges.Add($"Days a year {(existing.IsUnlimited ? "unlimited" : HrSeedData.Days(existing.AnnualDays))} → {(isUnlimited ? "unlimited" : HrSeedData.Days(annualDays))}");

        if (existing.MinNoticeDays != minNoticeDays)
            policyChanges.Add($"Minimum notice {HrSeedData.Days(existing.MinNoticeDays)} → {HrSeedData.Days(minNoticeDays)}");

        if (existing.Description != description)
            policyChanges.Add("New description");

        if (existing.IsActive != isActive)
            policyChanges.Add(isActive ? "Offered again" : "Turned off");

        var action = policyChanges.Count == 1 && existing.IsActive && !isActive ? "Turned off" : "Updated";
        (existing.Name, existing.Description, existing.AnnualDays, existing.IsUnlimited, existing.MinNoticeDays, existing.IsActive) = (name, description, annualDays, isUnlimited, minNoticeDays, isActive);
        saved = existing;

        return Commit(() => (existing.Name, existing.Description, existing.AnnualDays, existing.IsUnlimited, existing.MinNoticeDays, existing.IsActive) = previous,
            () => Record(action, $"Leave policy: {existing.Name}", PoliciesLink, string.Join("; ", policyChanges)));
    }

    /// <summary>Creates (<paramref name="templateId" /> 0) or renames an onboarding template (HR admin); names are unique.</summary>
    public HrResult SaveTemplate(int templateId, string name, string description, out HrOnboardingTemplate? saved)
    {
        saved = null;

        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        var existing = templateId == 0 ? null : FindTemplate(templateId);

        if (templateId != 0 && existing is null)
            return HrResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;

        if (!IsText(name, MaxNameLength) || description.Length > MaxDescriptionLength)
            return HrResult.Invalid;

        if (templates.Any(item => item != existing && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
            return HrResult.Duplicate;

        if (existing is null)
        {
            if (templates.Count >= MaxTemplates || nextTemplateId >= NextIdLimit)
                return HrResult.Invalid;

            var template = new HrOnboardingTemplate { Id = nextTemplateId++, Name = name, Description = description };
            templates.Add(template);
            var result = Commit(() =>
            {
                templates.Remove(template);
                nextTemplateId--;
            }, () => Record("Created", $"Onboarding template: {template.Name}", TemplateLink(template.Id)));

            if (result == HrResult.Ok)
                saved = template;

            return result;
        }

        if (existing.Name == name && existing.Description == description)
            return HrResult.NoChange;

        var previous = (existing.Name, existing.Description);
        var templateChanges = string.Join("; ", new[]
        {
            existing.Name != name ? $"Name {existing.Name} → {name}" : null,
            existing.Description != description ? "New description" : null,
        }.OfType<string>());
        (existing.Name, existing.Description) = (name, description);
        saved = existing;

        return Commit(() => (existing.Name, existing.Description) = previous,
            () => Record(previous.Name != name ? "Renamed" : "Updated", $"Onboarding template: {existing.Name}", TemplateLink(existing.Id), templateChanges));
    }

    /// <summary>
    /// Adds (<paramref name="taskId" /> 0) or edits a template task (HR admin): what to do, who does it, and the day it's due
    /// relative to day one (within <see cref="MaxTaskOffsetDays" /> either way).
    /// </summary>
    public HrResult SaveTemplateTask(int templateId, int taskId, string title, HrTaskOwner owner, int dueOffsetDays)
    {
        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        if (FindTemplate(templateId) is not { } template)
            return HrResult.NotFound;

        var existing = taskId == 0 ? null : template.Tasks.FirstOrDefault(item => item.Id == taskId);

        if (taskId != 0 && existing is null)
            return HrResult.NotFound;

        title = title?.Trim() ?? string.Empty;

        if (!IsText(title, MaxNameLength) || !Enum.IsDefined(owner) || dueOffsetDays is < -MaxTaskOffsetDays or > MaxTaskOffsetDays
            || (existing is null && (template.Tasks.Count >= MaxTemplateTasks || template.NextTaskId >= NextIdLimit)))
            return HrResult.Invalid;

        if (existing is not null && existing.Title == title && existing.Owner == owner && existing.DueOffsetDays == dueOffsetDays)
            return HrResult.NoChange;

        var task = existing ?? new HrTemplateTask { Id = template.NextTaskId++ };
        var previous = (task.Title, task.Owner, task.DueOffsetDays);
        (task.Title, task.Owner, task.DueOffsetDays) = (title, owner, dueOffsetDays);

        if (existing is null)
            template.Tasks.Add(task);

        template.Tasks.Sort((a, b) => a.DueOffsetDays != b.DueOffsetDays ? a.DueOffsetDays.CompareTo(b.DueOffsetDays) : a.Id.CompareTo(b.Id));

        return Commit(() =>
        {
            if (existing is null)
            {
                template.Tasks.Remove(task);
                template.NextTaskId--;
            }
            else
            {
                (task.Title, task.Owner, task.DueOffsetDays) = previous;
                template.Tasks.Sort((a, b) => a.DueOffsetDays != b.DueOffsetDays ? a.DueOffsetDays.CompareTo(b.DueOffsetDays) : a.Id.CompareTo(b.Id));
            }
        }, () => Record(existing is null ? "Added a task" : "Changed a task", $"Onboarding template: {template.Name}", TemplateLink(template.Id),
            $"“{title}”, {OwnerWords(owner)}, {OffsetWords(dueOffsetDays)}"));
    }

    /// <summary>
    /// The header's global search, best matches first (exactly the term, then starting with it, then containing it).
    /// Everyone signed in finds the people in the directory by name, work email, or title (the HR admin also finds those who
    /// left, and opens a record in the employee editor), and their own leave requests by number or type. Managers also find
    /// their direct reports' requests, and the HR admin everyone's, by number, type, or the employee's name; the HR admin
    /// also finds leave policies and onboarding templates by name. Returns nothing for a blank term or when nobody with an
    /// employee record is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || Me() is not { } me)
            return Array.Empty<ShowcaseSearchResult>();

        var number = term.TrimStart('#');

        int? Rank(params string?[] values) => ShowcaseSearch.Rank(term, values);

        int? NumberRank(int id)
        {
            var text = id.ToString(CultureInfo.InvariantCulture);

            return number.Length == 0 || !number.All(char.IsAsciiDigit) ? null : text == number ? 0 : text.StartsWith(number, StringComparison.Ordinal) ? 1 : null;
        }

        var matches = new List<(int Rank, int Kind, ShowcaseSearchResult Result)>();

        foreach (var person in IsHrAdmin ? employees : employees.Where(item => item.Status != HrEmployeeStatus.Left))
        {
            if (Rank(person.Name, person.Email, person.Title) is not { } found)
                continue;

            var department = FindDepartment(person.DepartmentId)?.Name;
            matches.Add(person.Status == HrEmployeeStatus.Left
                ? (found, 0, new ShowcaseSearchResult("Employee", person.Name, $"{person.Title} · Left", HrSeedData.EmployeeEditUrl(person.Id)))
                : (found, 0, new ShowcaseSearchResult("Employee", person.Name, $"{person.Title} · {department}", PersonLink(person.Id))));
        }

        foreach (var request in leaveRequests.Where(item => item.EmployeeId == me.Id || (item.EmployeeId != me.Id && CanSeeLeave(item.EmployeeId))))
        {
            var own = request.EmployeeId == me.Id;
            var employee = FindEmployeeRecord(request.EmployeeId);
            var policyName = PolicyName(request.PolicyId);
            var found = new[] { NumberRank(request.Id), own ? Rank(policyName) : Rank(employee?.Name, policyName) }.Min();

            // Only a number or a name finds someone else's request, so a type like "Vacation" doesn't list the whole company's.
            if (found is not { } rank || (!own && NumberRank(request.Id) is null && Rank(employee?.Name) is null))
                continue;

            var label = FormattableString.Invariant($"#{request.Id} {policyName}, {HrSeedData.Range(request.From, request.To)}");
            var status = request.Status == HrLeaveStatus.Pending ? "Waiting" : request.Status.ToString();
            matches.Add(own
                ? (rank, 1, new ShowcaseSearchResult("Your leave", label, status, MyLeaveLink))
                : (rank, 1, new ShowcaseSearchResult("Leave request", label, $"{employee?.Name ?? "A former employee"} · {status}", ApprovalsLink)));
        }

        foreach (var cycle in ReviewCycles())
        {
            if (Rank(cycle.Name) is not { } found)
                continue;

            var status = cycle.Status == HrCycleStatus.SignOff ? "Sign-off" : cycle.Status.ToString();

            if (IsHrAdmin)
                matches.Add((found, 4, new ShowcaseSearchResult("Review cycle", cycle.Name, $"{status} · {ScopeText(cycle)}", HrReviewSeedData.CycleUrl(cycle.Id))));
            else if (reviews.FirstOrDefault(review => review.CycleId == cycle.Id && review.EmployeeId == me.Id) is { } own)
                matches.Add((found, 4, new ShowcaseSearchResult("Your review", cycle.Name, status, HrReviewSeedData.ReviewUrl(own.Id))));
        }

        if (IsHrAdmin)
        {
            foreach (var policy in policies)
            {
                if (Rank(policy.Name) is { } found)
                    matches.Add((found, 2, new ShowcaseSearchResult("Leave policy", policy.Name, policy.IsActive ? PolicySummary(policy) : "Turned off", PoliciesLink)));
            }

            foreach (var template in templates)
            {
                if (Rank(template.Name) is { } found)
                    matches.Add((found, 3, new ShowcaseSearchResult("Onboarding template", template.Name,
                        string.Create(CultureInfo.InvariantCulture, $"{template.Tasks.Count} {(template.Tasks.Count == 1 ? "task" : "tasks")}"), TemplateLink(template.Id))));
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    /// <summary>
    /// Ticks or unticks an onboarding task: the new hire their own tasks, their manager the manager's tasks, the HR admin any
    /// task (WrongState: someone else's task).
    /// </summary>
    public HrResult SetTaskDone(int employeeId, int taskId, bool done)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        if (onboardings.FirstOrDefault(item => item.EmployeeId == employeeId) is not { } plan || plan.Tasks.FirstOrDefault(item => item.Id == taskId) is not { } task
            || FindEmployeeRecord(employeeId) is not { } employee)
            return HrResult.NotFound;

        var allowed = IsHrAdmin || task.Owner switch
        {
            HrTaskOwner.NewHire => employee.Id == me.Id,
            HrTaskOwner.Manager => IsManager && employee.ManagerId == me.Id,
            _ => false,
        };

        if (!allowed)
            return HrResult.WrongState;

        if ((task.DoneAt is not null) == done)
            return HrResult.NoChange;

        var previous = (task.DoneAt, task.DoneById);
        task.DoneAt = done ? Clock.Now : null;
        task.DoneById = done ? me.Id : null;

        return Commit(() => (task.DoneAt, task.DoneById) = previous,
            () => Record(done ? "Ticked off" : "Unticked", OnboardingTarget(employee), NewHiresLink, task.Title));
    }

    /// <summary>
    /// Starts onboarding for someone who works here from a template (HR admin); one plan per person (Duplicate). WrongState:
    /// they left, the template has no tasks, or a due date would fall outside the dates the demo keeps. The new hire (once they
    /// can sign in) and their manager are notified.
    /// </summary>
    public HrResult StartOnboarding(int employeeId, int templateId, out HrOnboarding? started)
    {
        started = null;

        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        if (FindEmployeeRecord(employeeId) is not { } employee || FindTemplate(templateId) is not { } template)
            return HrResult.NotFound;

        if (employee.Status == HrEmployeeStatus.Left || template.Tasks.Count == 0
            || template.Tasks.Any(task => !IsPlausible(employee.StartDate.Date.AddDays(task.DueOffsetDays))))
            return HrResult.WrongState;

        if (onboardings.Any(item => item.EmployeeId == employeeId))
            return HrResult.Duplicate;

        if (onboardings.Count >= MaxOnboardings || nextOnboardingId >= NextIdLimit)
            return HrResult.Invalid;

        var plan = HrSeedData.Plan(nextOnboardingId++, employee.Id, employee.StartDate, template, Clock.Now);
        onboardings.Add(plan);
        var result = Commit(() =>
        {
            onboardings.Remove(plan);
            nextOnboardingId--;
        }, () =>
        {
            var tasks = string.Create(CultureInfo.InvariantCulture, $"{plan.Tasks.Count} {(plan.Tasks.Count == 1 ? "task" : "tasks")}");
            Record("Started onboarding", OnboardingTarget(employee), NewHiresLink, $"Template “{template.Name}”, {tasks}");
            Notify(employee.AccountId, "Your onboarding plan is ready", $"{template.Name}: {tasks}", MyOnboardingLink);
            Notify(AccountOf(employee.ManagerId), $"Onboarding started for {employee.Name}", $"Starts {Day(employee.StartDate)} · {template.Name}", NewHiresLink);
        });

        if (result == HrResult.Ok)
            started = plan;

        return result;
    }

    /// <summary>Validates a stored snapshot against its own data and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        HrSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<HrSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return false;
        }

        if (snapshot is not
            {
                Employees: { Count: > 0 and <= MaxEmployees } storedEmployees,
                LeaveRequests: { Count: <= MaxLeaveRequests } storedLeave,
                Onboardings: { Count: <= MaxOnboardings } storedPlans,
                Policies: { Count: <= MaxPolicies } storedPolicies,
                Templates: { Count: <= MaxTemplates } storedTemplates,
                Audit: { Count: <= MaxAuditEntries } storedAudit,
                Notifications: { Count: <= MaxNotifications } storedNotifications,
                Cycles: { Count: <= MaxCycles } storedCycles,
                Reviews: { Count: <= MaxReviews } storedReviews,
            }
            || new[] { snapshot.NextEmployeeId, snapshot.NextLeaveId, snapshot.NextOnboardingId, snapshot.NextPolicyId, snapshot.NextTemplateId }.Any(id => id is < 1 or > NextIdLimit)
            || snapshot.NextAuditId is < 1 or > 10_000_000
            || snapshot.NextNotificationId is < 1 or > 10_000_000
            || snapshot.NextCycleId is < 1 or > NextIdLimit
            || snapshot.NextReviewId is < 1 or > NextIdLimit)
            return false;

        // People: the demo accounts' records keep their account; any other record has no account or one registered account.
        var seededAccounts = HrSeedData.Employees().ToDictionary(item => item.Id, item => item.AccountId);

        if (storedEmployees.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextEmployeeId || !IsText(item.Name, MaxNameLength) || !IsText(item.Initials, 3) || !IsEmail(item.Email)
                || !IsText(item.Title, MaxNameLength) || FindDepartment(item.DepartmentId) is null || !Locations.Contains(item.Location) || item.Phone is not { Length: <= MaxPhoneLength }
                || item.Pronouns is not { Length: <= MaxPronounsLength } || item.Bio is not { Length: <= MaxBioLength } || !IsPlausible(item.StartDate) || !Enum.IsDefined(item.Status)
                || (item.Status == HrEmployeeStatus.Left) != (item.EndDate is not null) || (item.EndDate is { } end && (end < item.StartDate || !IsPlausible(end)))
                || (seededAccounts.TryGetValue(item.Id, out var seededAccount) && seededAccount is not null
                    ? item.AccountId != seededAccount
                    : item.AccountId is not null && !item.AccountId.StartsWith("u-", StringComparison.Ordinal))
                || item.ManagerId == item.Id || (item.InvitePending && (item.AccountId is not null || item.Status == HrEmployeeStatus.Left))
                || !IsProbation(item.ProbationEndDate, item.StartDate))
            || !Unique(storedEmployees.Select(item => item.Id)) || !Unique(storedEmployees.Select(item => item.Email))
            || !Unique(storedEmployees.Where(item => item.AccountId is not null).Select(item => item.AccountId)))
            return false;

        var byId = storedEmployees.ToDictionary(item => item.Id);

        // Managers exist and the reporting lines have no loops.
        if (storedEmployees.Any(item => item.ManagerId is { } managerId && !byId.ContainsKey(managerId)) || storedEmployees.Any(item => HasLoop(item, byId)))
            return false;

        if (storedPolicies.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextPolicyId || !IsText(item.Name, MaxNameLength) || item.Description is not { Length: <= MaxDescriptionLength }
                || item.AnnualDays is < 0 or > MaxAnnualDays || (item.IsUnlimited ? item.AnnualDays != 0 : item.AnnualDays == 0) || item.MinNoticeDays is < 0 or > MaxNoticeDays)
            || !Unique(storedPolicies.Select(item => item.Id)) || !Unique(storedPolicies.Select(item => item.Name.ToLowerInvariant())))
            return false;

        var policyIds = storedPolicies.Select(item => item.Id).ToHashSet();

        if (storedLeave.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextLeaveId || !byId.ContainsKey(item.EmployeeId) || !policyIds.Contains(item.PolicyId)
                || item.From != item.From.Date || item.To != item.To.Date || item.To < item.From || item.From.Year != item.To.Year || (item.To - item.From).TotalDays >= MaxLeaveSpanDays
                || !IsPlausible(item.From) || item.Days != WorkingDays(item.From, item.To) || item.Days < 1 || !Enum.IsDefined(item.Status) || item.Note is not { Length: <= MaxNoteLength }
                || item.DecisionNote is not { Length: <= MaxNoteLength } || !IsPlausible(item.CreatedAt)
                || (item.DecidedAt is null) != (item.DecidedById is null) || (item.DecidedById is { } decider && !byId.ContainsKey(decider))
                || (item.Status is HrLeaveStatus.Approved or HrLeaveStatus.Rejected && item.DecidedAt is null) || (item.Status == HrLeaveStatus.Pending && item.DecidedAt is not null)
                || (item.Status == HrLeaveStatus.Rejected && item.DecisionNote.Length == 0))
            || !Unique(storedLeave.Select(item => item.Id)))
            return false;

        // A person's pending and approved requests never share a day.
        if (storedLeave.Where(item => item.Status is HrLeaveStatus.Pending or HrLeaveStatus.Approved).GroupBy(item => item.EmployeeId)
            .Any(group => group.OrderBy(item => item.From).Zip(group.OrderBy(item => item.From).Skip(1)).Any(pair => pair.Second.From <= pair.First.To)))
            return false;

        if (storedTemplates.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextTemplateId || !IsText(item.Name, MaxNameLength) || item.Description is not { Length: <= MaxDescriptionLength }
                || item.NextTaskId is < 1 or >= NextIdLimit || item.Tasks is not { Count: <= MaxTemplateTasks }
                || item.Tasks.Any(task => task is null || task.Id < 1 || task.Id >= item.NextTaskId || !IsText(task.Title, MaxNameLength) || !Enum.IsDefined(task.Owner)
                    || task.DueOffsetDays is < -MaxTaskOffsetDays or > MaxTaskOffsetDays)
                || !Unique(item.Tasks.Select(task => task.Id)))
            || !Unique(storedTemplates.Select(item => item.Id)) || !Unique(storedTemplates.Select(item => item.Name.ToLowerInvariant())))
            return false;

        if (storedPlans.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextOnboardingId || !byId.ContainsKey(item.EmployeeId) || !IsText(item.TemplateName, MaxNameLength)
                || !IsPlausible(item.CreatedAt) || item.Tasks is not { Count: > 0 and <= MaxTemplateTasks }
                || item.Tasks.Any(task => task is null || task.Id < 1 || !IsText(task.Title, MaxNameLength) || !Enum.IsDefined(task.Owner) || !IsPlausible(task.Due)
                    || (task.DoneAt is null) != (task.DoneById is null) || (task.DoneById is { } doer && !byId.ContainsKey(doer)) || (task.DoneAt is { } at && !IsPlausible(at)))
                || !Unique(item.Tasks.Select(task => task.Id)))
            || !Unique(storedPlans.Select(item => item.Id)) || !Unique(storedPlans.Select(item => item.EmployeeId)))
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausible(entry.At)
                || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength) || !IsText(entry.Action, MaxAuditTextLength) || !IsText(entry.Target, MaxAuditTextLength)
                || (entry.Details is not null && !IsText(entry.Details, MaxAuditTextLength)) || !IsSiteLink(entry.TargetUrl))
            || !Unique(storedAudit.Select(entry => entry.Id)))
            return false;

        if (!ValidReviews(storedCycles, storedReviews, snapshot.NextCycleId, snapshot.NextReviewId, byId))
            return false;

        // Notifications are only ever raised for accounts that can sign in: the demo accounts and registered ones.
        if (storedNotifications.Any(notification => notification is null || notification.Id < 1 || notification.Id >= snapshot.NextNotificationId
                || !IsAccountId(notification.AccountId) || !IsPlausible(notification.At) || !IsText(notification.Title, MaxAuditTextLength)
                || (notification.Text is not null && !IsText(notification.Text, MaxAuditTextLength)) || !IsSiteLink(notification.Url))
            || !Unique(storedNotifications.Select(notification => notification.Id)))
            return false;

        Replace(employees, storedEmployees);
        Replace(leaveRequests, storedLeave);
        Replace(onboardings, storedPlans);
        Replace(policies, storedPolicies);
        Replace(templates, storedTemplates);
        Replace(audit, storedAudit.OrderBy(entry => entry.Id).ToList());
        Replace(notifications, storedNotifications.OrderBy(notification => notification.Id).ToList());
        Replace(cycles, storedCycles.OrderBy(cycle => cycle.Id).ToList());
        Replace(reviews, storedReviews.OrderBy(review => review.Id).ToList());
        nextCycleId = snapshot.NextCycleId;
        nextReviewId = snapshot.NextReviewId;
        nextAuditId = snapshot.NextAuditId;
        nextNotificationId = snapshot.NextNotificationId;
        nextEmployeeId = snapshot.NextEmployeeId;
        nextLeaveId = snapshot.NextLeaveId;
        nextOnboardingId = snapshot.NextOnboardingId;
        nextPolicyId = snapshot.NextPolicyId;
        nextTemplateId = snapshot.NextTemplateId;

        return true;
    }

    /// <summary>Changes one of the signed-in employee's pending requests (type, dates, note), with the same checks as a new one.</summary>
    public HrResult UpdateLeave(int requestId, int policyId, DateTime from, DateTime to, string note)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        if (leaveRequests.FirstOrDefault(item => item.Id == requestId && item.EmployeeId == me.Id) is not { } request || FindPolicy(policyId) is not { } policy)
            return HrResult.NotFound;

        if (request.Status != HrLeaveStatus.Pending)
            return HrResult.WrongState;

        var check = CheckLeave(me.Id, policy, from, to, note, request.Id, out var days);

        if (check != HrResult.Ok)
            return check;

        if (request.PolicyId == policyId && request.From == from.Date && request.To == to.Date && request.Note == note.Trim())
            return HrResult.NoChange;

        var previous = (request.PolicyId, request.From, request.To, request.Days, request.Note);
        var was = $"{PolicyName(request.PolicyId)}, {HrSeedData.Range(request.From, request.To)}";
        (request.PolicyId, request.From, request.To, request.Days, request.Note) = (policyId, from.Date, to.Date, days, note.Trim());

        return Commit(() => (request.PolicyId, request.From, request.To, request.Days, request.Note) = previous, () =>
        {
            var now = $"{policy.Name}, {HrSeedData.Range(request.From, request.To)}";
            Record("Changed", HrSeedData.LeaveTarget(request.Id), ApprovalsLink, was == now ? $"{me.Name}: {now}; new note" : $"{me.Name}: {was} → {now}");
            NotifyApprovers(me, $"{me.Name} changed a leave request", HrSeedData.LeaveSummary(policy.Name, request));
        });
    }

    /// <summary>Returns how many working days (weekdays that aren't company holidays) a range covers.</summary>
    public int WorkingDays(DateTime from, DateTime to)
    {
        var count = 0;

        for (var day = from.Date; day <= to.Date && count <= MaxLeaveSpanDays; day = day.AddDays(1))
        {
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && Holidays.All(holiday => holiday.Date != day))
                count++;
        }

        return count;
    }

    #endregion

    #region Private Methods

    /// <summary>Copies the editor's fields; someone who hasn't started yet is "Joining", otherwise "Active".</summary>
    private void Apply(HrEmployee employee, string name, string email, string title, string phone, HrEmployeeInput input)
    {
        employee.Status = input.StartDate.Date > Clock.Today ? HrEmployeeStatus.Joining : HrEmployeeStatus.Active;
        employee.Name = name;
        employee.Email = email;
        employee.Title = title;
        employee.Phone = phone;
        employee.DepartmentId = input.DepartmentId;
        employee.ManagerId = input.ManagerId;
        employee.Location = input.Location;
        employee.StartDate = input.StartDate.Date;
        employee.ProbationEndDate = input.ProbationEndDate?.Date;
        employee.Initials = Initials(name);
    }

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    /// <summary>Formats a date like "Dec 1, 2026", for audit details and notifications.</summary>
    private static string Day(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>The audit log's name for someone's record, like "Employee: Leila Haddad".</summary>
    private static string EmployeeTarget(HrEmployee employee) => $"Employee: {employee.Name}";

    /// <summary>Whether a stored notification's account can sign in: a demo account, or a registered one (<c>u-</c> and a number).</summary>
    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (HrSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    /// <summary>Whether a probation end date is valid for a start date: none, or after it and at most <see cref="MaxProbationMonths" /> months later.</summary>
    private static bool IsProbation(DateTime? probationEnd, DateTime start) =>
        probationEnd is not { } end || (end.Date > start.Date && end.Date <= start.Date.AddMonths(MaxProbationMonths) && IsPlausible(end));

    /// <summary>Checks a stored audit or notification link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) => ShowcaseLinks.IsSiteLink(url, SiteBase, allowSiteBase: false);

    /// <summary>A template task's due day in words, like "3 days before day one", "day one", or "day 14".</summary>
    private static string OffsetWords(int offset) => offset switch
    {
        0 => "day one",
        < 0 => $"{HrSeedData.Days(-offset)} before day one",
        _ => string.Create(CultureInfo.InvariantCulture, $"day {offset}"),
    };

    /// <summary>The audit log's name for someone's onboarding, like "Onboarding: Theo Martin".</summary>
    private static string OnboardingTarget(HrEmployee employee) => $"Onboarding: {employee.Name}";

    /// <summary>Who does a task, in words.</summary>
    private static string OwnerWords(HrTaskOwner owner) => owner switch
    {
        HrTaskOwner.NewHire => "the new hire",
        HrTaskOwner.Manager => "the manager",
        _ => "HR",
    };

    /// <summary>Someone's directory profile (the same link as <c>HrDisplay.EmployeeUrl</c>).</summary>
    private static string PersonLink(int employeeId) => FormattableString.Invariant($"{SiteBase}/people/{employeeId}");

    /// <summary>Describes a policy, like "25 days a year, 7 days' notice".</summary>
    private static string PolicySummary(HrLeavePolicy policy) =>
        (policy.IsUnlimited ? "No yearly limit" : $"{HrSeedData.Days(policy.AnnualDays)} a year")
        + (policy.MinNoticeDays == 0 ? ", no notice needed" : $", {HrSeedData.Days(policy.MinNoticeDays)}' notice")
        + (policy.IsActive ? null : ", turned off");

    /// <summary>An onboarding template's page (the same link as <c>AdminHelpers.TemplateUrl</c>).</summary>
    private static string TemplateLink(int templateId) => FormattableString.Invariant($"{TemplatesLink}/{templateId}");

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

    private static bool HasLoop(HrEmployee start, Dictionary<int, HrEmployee> byId)
    {
        var seen = new HashSet<int> { start.Id };

        for (var current = start; current.ManagerId is { } managerId && byId.TryGetValue(managerId, out var manager); current = manager)
        {
            if (!seen.Add(manager.Id))
                return true;
        }

        return false;
    }

    private static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return (parts.Length switch { 0 => "?", 1 => parts[0][..1], _ => string.Concat(parts[0][..1], parts[^1][..1]) }).ToUpperInvariant();
    }

    private static bool IsEmail(string? value) => value is not null && value.Length <= ShowcaseIdentity.MaxEmailLength && EmailPattern().IsMatch(value);

    private static bool IsPlausible(DateTime value) => value >= new DateTime(2015, 1, 1) && value <= HrSeedData.Now.AddYears(3);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

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

    /// <summary>The account of someone who works here and can sign in, or <see langword="null" />.</summary>
    private string? AccountOf(int? employeeId) =>
        employeeId is { } id && FindEmployeeRecord(id) is { Status: not HrEmployeeStatus.Left, AccountId: { } accountId } ? accountId : null;

    private HrBalance Balance(int employeeId, HrLeavePolicy policy, int year)
    {
        var requests = leaveRequests.Where(item => item.EmployeeId == employeeId && item.PolicyId == policy.Id && item.From.Year == year).ToList();

        return new HrBalance(policy, policy.AnnualDays, requests.Where(item => item.Status == HrLeaveStatus.Approved).Sum(item => item.Days),
            requests.Where(item => item.Status == HrLeaveStatus.Pending).Sum(item => item.Days));
    }

    /// <summary>Whether the signed-in person may see someone's leave and onboarding: themselves, their manager, or HR.</summary>
    private bool CanSeeLeave(int employeeId) =>
        Me() is { } me && (me.Id == employeeId || IsHrAdmin || (IsManager && employees.Any(item => item.Id == employeeId && item.ManagerId == me.Id)));

    /// <summary>The shared checks of a new or changed request; returns the working days in <paramref name="days" />.</summary>
    private HrResult CheckLeave(int employeeId, HrLeavePolicy policy, DateTime from, DateTime to, string note, int? exceptId, out int days)
    {
        from = from.Date;
        to = to.Date;
        days = 0;

        if (!policy.IsActive || from < (FindEmployeeRecord(employeeId)?.StartDate ?? from) || to < from || from.Year != to.Year || (to - from).TotalDays >= MaxLeaveSpanDays || !IsPlausible(from) || (note?.Trim().Length ?? 0) > MaxNoteLength
            || from < Clock.Today.AddDays(policy.MinNoticeDays) && !(policy.IsUnlimited && from >= Clock.Today.AddDays(-30)))
            return HrResult.Invalid;

        days = WorkingDays(from, to);

        if (days == 0)
            return HrResult.Invalid;

        var day0 = from;
        var day1 = to;

        if (leaveRequests.Any(item => item.EmployeeId == employeeId && item.Id != exceptId && item.Status is HrLeaveStatus.Pending or HrLeaveStatus.Approved && item.From <= day1 && day0 <= item.To))
            return HrResult.Overlap;

        if (!policy.IsUnlimited)
        {
            var balance = Balance(employeeId, policy, from.Year);
            var current = exceptId is { } id && leaveRequests.FirstOrDefault(item => item.Id == id) is { Status: HrLeaveStatus.Pending } edited && edited.PolicyId == policy.Id && edited.From.Year == from.Year
                ? edited.Days
                : 0;

            if (balance.Used + balance.Pending - current + days > balance.Allowance)
                return HrResult.InsufficientBalance;
        }

        return HrResult.Ok;
    }

    /// <summary>
    /// Keeps a change when the snapshot still fits: runs <paramref name="committed" /> (the audit entry and notifications),
    /// then raises <see cref="Changed" />. Otherwise undoes the change and reports Invalid.
    /// </summary>
    private HrResult Commit(Action undo, Action? committed = null)
    {
        if (!FitsSnapshot())
        {
            undo();

            return HrResult.Invalid;
        }

        committed?.Invoke();
        Changed?.Invoke();

        return HrResult.Ok;
    }

    /// <summary>What changed on someone's record, for the audit log, like "Title Engineer → Senior engineer; Manager Ana → Leila".</summary>
    private string EmployeeChanges(HrEmployeeInput before, HrEmployeeInput after)
    {
        var changes = new List<string>();

        void Compare(string field, string? from, string? to)
        {
            if (from != to)
                changes.Add($"{field} {(string.IsNullOrEmpty(from) ? "none" : from)} → {(string.IsNullOrEmpty(to) ? "none" : to)}");
        }

        Compare("Name", before.Name, after.Name);
        Compare("Email", before.Email, after.Email);
        Compare("Title", before.Title, after.Title);
        Compare("Department", FindDepartment(before.DepartmentId)?.Name, FindDepartment(after.DepartmentId)?.Name);
        Compare("Manager", before.ManagerId is { } oldManager ? FindEmployeeRecord(oldManager)?.Name : null, after.ManagerId is { } newManager ? FindEmployeeRecord(newManager)?.Name : null);
        Compare("Location", before.Location, after.Location);
        Compare("Start", Day(before.StartDate), Day(after.StartDate));
        Compare("Probation ends", before.ProbationEndDate is { } oldEnd ? Day(oldEnd) : null, after.ProbationEndDate is { } newEnd ? Day(newEnd) : null);

        if (before.Phone != after.Phone)
            changes.Add("New phone number");

        return string.Join("; ", changes);
    }

    private HrEmployee? FindEmployeeRecord(int employeeId) => employees.FirstOrDefault(item => item.Id == employeeId);

    /// <summary>The accounts of the HR admins who work here, other than <paramref name="exceptEmployeeId" />.</summary>
    private IEnumerable<string> HrAdminAccounts(int exceptEmployeeId) =>
        employees.Where(item => item.Id != exceptEmployeeId && item.Status != HrEmployeeStatus.Left && item.AccountId is not null
                                && Identity.FindAccount(item.AccountId)?.RoleId == HrSeedData.HrAdminRole)
            .Select(item => item.AccountId!);

    /// <summary>A leave request for the audit log, like "Leila Haddad: Vacation, Dec 21–23, 2026 (3 days)".</summary>
    private string LeaveDetails(HrLeaveRequest request) =>
        $"{FindEmployeeRecord(request.EmployeeId)?.Name ?? "A former employee"}: {PolicyName(request.PolicyId)}, {HrSeedData.Range(request.From, request.To)} ({HrSeedData.Days(request.Days)})";

    /// <summary>Whether the snapshot, as it would be saved now, still fits under <see cref="MaxSnapshotLength" />.</summary>
    private bool FitsSnapshot()
    {
        try
        {
            return JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length <= MaxSnapshotLength;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>The signed-in person's employee record (not one who left), or <see langword="null" />.</summary>
    private HrEmployee? Me() =>
        Identity.Current is { } account ? employees.FirstOrDefault(item => item.AccountId == account.Id && item.Status != HrEmployeeStatus.Left) : null;

    /// <summary>
    /// Raises a notification for an account (rule WR-001); nothing for someone without a sign-in, or for the person who caused
    /// it. The site keeps the newest <see cref="MaxNotifications" />.
    /// </summary>
    private void Notify(string? accountId, string title, string? text, string? url)
    {
        if (accountId is null || accountId == Identity.Current?.Id)
            return;

        notifications.Add(new ShowcaseNotification
        {
            Id = nextNotificationId++,
            AccountId = accountId,
            At = Clock.Now,
            Title = HrSeedData.Clip(title, MaxAuditTextLength),
            Text = string.IsNullOrWhiteSpace(text) ? null : HrSeedData.Clip(text, MaxAuditTextLength),
            Url = url,
        });

        if (notifications.Count > MaxNotifications)
            notifications.RemoveAt(0);
    }

    /// <summary>
    /// Notifies whoever decides someone's leave: their manager, when the manager can sign in as a manager or HR admin;
    /// otherwise the HR admins, who may decide anyone's leave.
    /// </summary>
    private void NotifyApprovers(HrEmployee employee, string title, string text)
    {
        var manager = employee.ManagerId is { } managerId ? FindEmployeeRecord(managerId) : null;
        var managerRole = Identity.FindAccount(AccountOf(manager?.Id))?.RoleId;

        if (managerRole is HrSeedData.ManagerRole or HrSeedData.HrAdminRole)
        {
            Notify(manager!.AccountId, title, text, ApprovalsLink);

            return;
        }

        foreach (var accountId in HrAdminAccounts(employee.Id))
            Notify(accountId, title, text, ApprovalsLink);
    }

    /// <summary>
    /// A new account takes up the record HR invited with its work email (only an invited record, so nobody can claim an
    /// existing colleague's), which is recorded and tells the HR admins; a linked person's name follows their profile.
    /// </summary>
    private void OnIdentityChanged()
    {
        if (Identity.Current is { } account)
        {
            var email = account.Email.Trim().ToLowerInvariant();

            if (employees.FirstOrDefault(item => item.AccountId == account.Id) is { } linked)
            {
                linked.Name = Clip(account.Name.Trim(), MaxNameLength);
                linked.Initials = Initials(linked.Name);
            }
            else if (account.Id.StartsWith("u-", StringComparison.Ordinal)
                && employees.FirstOrDefault(item => item.Email == email && item.InvitePending && item.AccountId is null && item.Status != HrEmployeeStatus.Left) is { } invited)
            {
                invited.AccountId = account.Id;
                invited.InvitePending = false;
                Record("Linked an account", EmployeeTarget(invited), HrSeedData.EmployeeEditUrl(invited.Id), $"Registered with {invited.Email}");

                foreach (var accountId in HrAdminAccounts(invited.Id))
                    Notify(accountId, $"{invited.Name} can sign in now", $"Their new account is linked to their employee record ({invited.Email})", HrSeedData.EmployeeEditUrl(invited.Id));
            }
        }

        Changed?.Invoke();
    }

    /// <summary>A leave policy's name, or "Leave" when it was deleted.</summary>
    private string PolicyName(int policyId) => FindPolicy(policyId)?.Name ?? "Leave";

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock. The log keeps the newest <see cref="MaxAuditEntries" />.
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details = null)
    {
        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = Clock.Now,
            Actor = HrSeedData.Clip(Me()?.Name ?? Identity.Current?.Name ?? HrSeedData.ProductName, ShowcaseIdentity.MaxNameLength),
            Action = action,
            Target = HrSeedData.Clip(target, MaxAuditTextLength),
            TargetUrl = targetUrl,
            Details = string.IsNullOrWhiteSpace(details) ? null : HrSeedData.Clip(details, MaxAuditTextLength),
        });

        if (audit.Count > MaxAuditEntries)
            audit.RemoveAt(0);
    }

    /// <summary>Records a decided request and tells the employee, with the manager's note.</summary>
    private void RecordDecision(HrLeaveRequest request, bool approve, bool bulk)
    {
        Record(approve ? "Approved" : "Rejected", HrSeedData.LeaveTarget(request.Id), ApprovalsLink,
            LeaveDetails(request) + (request.DecisionNote.Length > 0 ? $"; note: {request.DecisionNote}" : null) + (bulk ? " (bulk)" : null));
        Notify(AccountOf(request.EmployeeId), $"Your leave request was {(approve ? "approved" : "rejected")}",
            $"{PolicyName(request.PolicyId)}, {HrSeedData.Range(request.From, request.To)}" + (request.DecisionNote.Length > 0 ? $" · {request.DecisionNote}" : null), MyLeaveLink);
    }

    /// <summary>Whether <paramref name="employeeId" /> is <paramref name="managerId" /> or reports to them, directly or not.</summary>
    private bool ReportsTo(int employeeId, int managerId)
    {
        var seen = new HashSet<int>();

        for (var current = FindEmployeeRecord(employeeId); current is not null && seen.Add(current.Id); current = current.ManagerId is { } next ? FindEmployeeRecord(next) : null)
        {
            if (current.Id == managerId)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks and applies one decision (the shared part of <see cref="DecideLeave" /> and <see cref="DecideLeaveMany" />),
    /// without saving: the caller commits, and calls <paramref name="undo" /> when the snapshot no longer fits.
    /// </summary>
    private HrResult TryDecide(HrEmployee me, int requestId, bool approve, string note, out HrLeaveRequest? decided, out Action? undo)
    {
        decided = null;
        undo = null;

        if (leaveRequests.FirstOrDefault(item => item.Id == requestId) is not { } request || FindEmployeeRecord(request.EmployeeId) is not { } employee)
            return HrResult.NotFound;

        if (request.EmployeeId == me.Id || !((IsManager && employee.ManagerId == me.Id) || IsHrAdmin))
            return HrResult.NotAllowed;

        if (request.Status != HrLeaveStatus.Pending)
            return HrResult.WrongState;

        note = note?.Trim() ?? string.Empty;

        if (note.Length > MaxNoteLength || (!approve && note.Length == 0))
            return HrResult.Invalid;

        if (approve && FindPolicy(request.PolicyId) is { IsUnlimited: false } policy)
        {
            var balance = Balance(request.EmployeeId, policy, request.From.Year);

            // The request itself is among the pending days.
            if (balance.Used + request.Days > balance.Allowance)
                return HrResult.InsufficientBalance;
        }

        var previous = (request.Status, request.DecidedAt, request.DecidedById, request.DecisionNote);
        request.Status = approve ? HrLeaveStatus.Approved : HrLeaveStatus.Rejected;
        request.DecidedAt = Clock.Now;
        request.DecidedById = me.Id;
        request.DecisionNote = note;
        decided = request;
        undo = () => (request.Status, request.DecidedAt, request.DecidedById, request.DecisionNote) = previous;

        return HrResult.Ok;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for the HR admin; empty for anyone else.</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsHrAdmin ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    /// <summary>Gets everyone, including those who left (HR admin only), by name.</summary>
    public IReadOnlyList<HrEmployee> AllEmployees => IsHrAdmin ? employees.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList() : Array.Empty<HrEmployee>();

    /// <summary>Gets all leave policies, including turned-off ones.</summary>
    public IReadOnlyList<HrLeavePolicy> AllPolicies => policies.OrderBy(item => item.Id).ToList();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(HrSeedData.Now);

    /// <summary>Gets the signed-in person's employee record, or <see langword="null" /> (an account with no record, or someone who left).</summary>
    public HrEmployee? CurrentEmployee => Me();

    /// <summary>Gets the departments.</summary>
    public IReadOnlyList<HrDepartment> Departments { get; } = HrSeedData.Departments();

    /// <summary>Gets the directory (signed-in people only): everyone who works here or is joining, by name.</summary>
    public IReadOnlyList<HrEmployee> Directory =>
        IsSignedIn ? employees.Where(item => item.Status != HrEmployeeStatus.Left).OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList() : Array.Empty<HrEmployee>();

    /// <summary>Gets the company holidays.</summary>
    public IReadOnlyList<HrHoliday> Holidays { get; } = HrSeedData.Holidays();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether charts should use dark-theme colors.</summary>
    public bool IsDarkTheme { get; private set; }

    /// <summary>Gets whether the signed-in account is linked to the record of someone who left.</summary>
    public bool IsFormerEmployee => Identity.Current is { } account && employees.Any(item => item.AccountId == account.Id && item.Status == HrEmployeeStatus.Left);

    /// <summary>Gets whether the signed-in person is the HR admin (with an employee record).</summary>
    public bool IsHrAdmin => Identity.Current?.RoleId == HrSeedData.HrAdminRole && Identity.Current is { } account && employees.Any(item => item.AccountId == account.Id && item.Status != HrEmployeeStatus.Left);

    /// <summary>Gets whether the signed-in person is a manager (with an employee record).</summary>
    public bool IsManager => Identity.Current?.RoleId == HrSeedData.ManagerRole && Me() is not null;

    /// <summary>Gets whether someone with an employee record is signed in.</summary>
    public bool IsSignedIn => Me() is not null;

    /// <summary>Gets the office locations.</summary>
    public IReadOnlyList<string> Locations { get; } = HrSeedData.Locations();

    /// <summary>Gets every onboarding plan (HR admin), or the signed-in manager's direct reports' plans.</summary>
    public IReadOnlyList<HrOnboarding> Onboardings =>
        Me() is not { } me ? Array.Empty<HrOnboarding>()
            : onboardings.Where(plan => IsHrAdmin || (IsManager && employees.Any(item => item.Id == plan.EmployeeId && item.ManagerId == me.Id))).OrderBy(plan => plan.CreatedAt).ToList();

    /// <summary>Gets the leave policies offered for new requests.</summary>
    public IReadOnlyList<HrLeavePolicy> Policies => policies.Where(item => item.IsActive).OrderBy(item => item.Id).ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "hr-portal";

    /// <summary>Gets the snapshot format version (3: performance review cycles; 2 added the audit log, notifications, probation end dates, and the larger seed).</summary>
    public int SnapshotVersion => 3;

    /// <summary>
    /// Gets the leave requests the signed-in person decides: their direct reports' (managers), or everyone's (HR admin);
    /// pending first (oldest first), then the rest (most recently decided first).
    /// </summary>
    public IReadOnlyList<HrLeaveRequest> TeamLeave =>
        Me() is not { } me ? Array.Empty<HrLeaveRequest>()
            : leaveRequests.Where(request => request.EmployeeId != me.Id && (IsHrAdmin || (IsManager && employees.Any(item => item.Id == request.EmployeeId && item.ManagerId == me.Id))))
                .OrderBy(request => request.Status == HrLeaveStatus.Pending ? 0 : 1)
                .ThenBy(request => request.Status == HrLeaveStatus.Pending ? request.CreatedAt.Ticks : -(request.DecidedAt ?? request.CreatedAt).Ticks)
                .ToList();

    /// <summary>Gets the onboarding templates, by name.</summary>
    public IReadOnlyList<HrOnboardingTemplate> Templates => templates.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList();

    #endregion

    #region Events

    /// <summary>Raised after any change, so open pages re-render and the session store saves.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes, so charts can redraw.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Other

    /// <summary>Records the color mode the theme switcher applied, so charts can redraw in matching colors.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (IsDarkTheme == isDark)
            return;

        IsDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>The stored form of the state.</summary>
    private sealed class HrSnapshot
    {
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        public List<HrReviewCycle>? Cycles { get; set; }

        public List<HrEmployee>? Employees { get; set; }

        public List<HrLeaveRequest>? LeaveRequests { get; set; }

        public int NextAuditId { get; set; }

        public int NextCycleId { get; set; }

        public int NextEmployeeId { get; set; }

        public int NextLeaveId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextOnboardingId { get; set; }

        public int NextPolicyId { get; set; }

        public int NextReviewId { get; set; }

        public int NextTemplateId { get; set; }

        public List<ShowcaseNotification>? Notifications { get; set; }

        public List<HrOnboarding>? Onboardings { get; set; }

        public List<HrLeavePolicy>? Policies { get; set; }

        public List<HrReview>? Reviews { get; set; }

        public List<HrOnboardingTemplate>? Templates { get; set; }
    }

    #endregion
}
