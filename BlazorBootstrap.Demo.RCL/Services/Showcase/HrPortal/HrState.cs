using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

/// <summary>
/// State of "Teamhaven", the HR portal of Brightfield Robotics, for the current browser tab: people and the org chart, leave
/// policies and requests, onboarding templates and plans, and the demo sign-in. It is kept in the tab's
/// <c>sessionStorage</c> through the showcase foundation; nothing is sent anywhere.
/// </summary>
/// <remarks>
/// Every signed-in person is linked to their employee record (a new account takes up the record with its work email, if one
/// is waiting). An employee requests, edits, and cancels their own leave and ticks their own onboarding tasks. A manager
/// also decides their direct reports' leave and ticks the manager's onboarding tasks for them. The HR admin also manages
/// employees, leave policies, and onboarding templates, starts onboarding, and may decide any leave or tick any task.
/// Leave days are weekdays that aren't company holidays; balances are per calendar year.
/// </remarks>
public sealed partial class HrState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The longest bio.</summary>
    public const int MaxBioLength = 300;

    /// <summary>The longest leave note or decision note.</summary>
    public const int MaxNoteLength = 300;

    /// <summary>The most days a leave policy can give a year.</summary>
    public const int MaxAnnualDays = 60;

    /// <summary>The most employees.</summary>
    public const int MaxEmployees = 250;

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

    private const int MaxLeaveRequests = 1500;

    private const int MaxOnboardings = 250;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int NextIdLimit = 1_000_000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<HrEmployee> employees = HrSeedData.Employees();

    private readonly List<HrLeaveRequest> leaveRequests = HrSeedData.LeaveRequests();

    private readonly List<HrOnboarding> onboardings = HrSeedData.Onboardings();

    private readonly List<HrLeavePolicy> policies = HrSeedData.LeavePolicies();

    private readonly List<HrOnboardingTemplate> templates = HrSeedData.OnboardingTemplates();

    private int nextEmployeeId;

    private int nextLeaveId;

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

        return Commit(() => request.Status = previous);
    }

    /// <summary>Returns the snapshot: every collection and the ID counters.</summary>
    public object CaptureData() => new HrSnapshot
    {
        Employees = employees.ToList(),
        LeaveRequests = leaveRequests.ToList(),
        NextEmployeeId = nextEmployeeId,
        NextLeaveId = nextLeaveId,
        NextOnboardingId = nextOnboardingId,
        NextPolicyId = nextPolicyId,
        NextTemplateId = nextTemplateId,
        Onboardings = onboardings.ToList(),
        Policies = policies.ToList(),
        Templates = templates.ToList(),
    };

    /// <summary>
    /// Approves or rejects a pending request (the employee's manager, or the HR admin; not your own). Rejecting needs a note
    /// for the employee; approving checks the balance still covers it (InsufficientBalance).
    /// </summary>
    public HrResult DecideLeave(int requestId, bool approve, string note)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

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

        return Commit(() => (request.Status, request.DecidedAt, request.DecidedById, request.DecisionNote) = previous);
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

        FindTemplate(templateId)!.Tasks.Remove(task);
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
        });
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
    /// Overlap: another pending or approved request covers a day; InsufficientBalance: not enough days left.
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
        });

        if (result == HrResult.Ok)
            requested = request;

        return result;
    }

    /// <summary>
    /// Creates (<paramref name="employeeId" /> 0) or edits an employee (HR admin): unique work email, an existing department,
    /// a manager who works here and isn't in their own reporting line. Someone starting after today is "Joining".
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
            || phone.Length > MaxPhoneLength || !IsPlausible(input.StartDate))
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
            });

            if (result == HrResult.Ok)
                saved = employee;

            return result;
        }

        if (existing.Name == name && existing.Email == email && existing.Title == title && existing.Phone == phone && existing.DepartmentId == input.DepartmentId
            && existing.ManagerId == input.ManagerId && existing.Location == input.Location && existing.StartDate == input.StartDate.Date)
            return HrResult.NoChange;

        var before = new HrEmployeeInput(existing.Name, existing.Email, existing.Title, existing.DepartmentId, existing.ManagerId, existing.Location, existing.StartDate, existing.Phone);
        Apply(existing, name, email, title, phone, input);
        saved = existing;

        return Commit(() => Apply(existing, before.Name, before.Email, before.Title, before.Phone, before));
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
        (me.Phone, me.Pronouns, me.Bio) = (phone, pronouns, bio);

        return Commit(() => (me.Phone, me.Pronouns, me.Bio) = previous);
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
            });

            if (result == HrResult.Ok)
                saved = policy;

            return result;
        }

        if (existing.Name == name && existing.Description == description && existing.AnnualDays == annualDays && existing.IsUnlimited == isUnlimited && existing.MinNoticeDays == minNoticeDays
            && existing.IsActive == isActive)
            return HrResult.NoChange;

        var previous = (existing.Name, existing.Description, existing.AnnualDays, existing.IsUnlimited, existing.MinNoticeDays, existing.IsActive);
        (existing.Name, existing.Description, existing.AnnualDays, existing.IsUnlimited, existing.MinNoticeDays, existing.IsActive) = (name, description, annualDays, isUnlimited, minNoticeDays, isActive);
        saved = existing;

        return Commit(() => (existing.Name, existing.Description, existing.AnnualDays, existing.IsUnlimited, existing.MinNoticeDays, existing.IsActive) = previous);
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
            });

            if (result == HrResult.Ok)
                saved = template;

            return result;
        }

        if (existing.Name == name && existing.Description == description)
            return HrResult.NoChange;

        var previous = (existing.Name, existing.Description);
        (existing.Name, existing.Description) = (name, description);
        saved = existing;

        return Commit(() => (existing.Name, existing.Description) = previous);
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
        });
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

        return Commit(() => (task.DoneAt, task.DoneById) = previous);
    }

    /// <summary>
    /// Starts onboarding for someone who works here from a template (HR admin); one plan per person (Duplicate). WrongState:
    /// they left, the template has no tasks, or a due date would fall outside the dates the demo keeps.
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
            }
            || new[] { snapshot.NextEmployeeId, snapshot.NextLeaveId, snapshot.NextOnboardingId, snapshot.NextPolicyId, snapshot.NextTemplateId }.Any(id => id is < 1 or > NextIdLimit))
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
                || item.ManagerId == item.Id || (item.InvitePending && (item.AccountId is not null || item.Status == HrEmployeeStatus.Left)))
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

        Replace(employees, storedEmployees);
        Replace(leaveRequests, storedLeave);
        Replace(onboardings, storedPlans);
        Replace(policies, storedPolicies);
        Replace(templates, storedTemplates);
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
        (request.PolicyId, request.From, request.To, request.Days, request.Note) = (policyId, from.Date, to.Date, days, note.Trim());

        return Commit(() => (request.PolicyId, request.From, request.To, request.Days, request.Note) = previous);
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
        employee.Initials = Initials(name);
    }

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

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

    /// <summary>Raises <see cref="Changed" /> when the snapshot still fits; otherwise undoes the change and reports Invalid.</summary>
    private HrResult Commit(Action undo)
    {
        if (!FitsSnapshot())
        {
            undo();

            return HrResult.Invalid;
        }

        Changed?.Invoke();

        return HrResult.Ok;
    }

    private HrEmployee? FindEmployeeRecord(int employeeId) => employees.FirstOrDefault(item => item.Id == employeeId);

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
    /// A new account takes up the record HR invited with its work email (only an invited record, so nobody can claim an
    /// existing colleague's); a linked person's name follows their profile.
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
            }
        }

        Changed?.Invoke();
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

    #endregion

    #endregion

    #region Properties, Indexers

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

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

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
        public List<HrEmployee>? Employees { get; set; }

        public List<HrLeaveRequest>? LeaveRequests { get; set; }

        public int NextEmployeeId { get; set; }

        public int NextLeaveId { get; set; }

        public int NextOnboardingId { get; set; }

        public int NextPolicyId { get; set; }

        public int NextTemplateId { get; set; }

        public List<HrOnboarding>? Onboardings { get; set; }

        public List<HrLeavePolicy>? Policies { get; set; }

        public List<HrOnboardingTemplate>? Templates { get; set; }
    }

    #endregion
}
