using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

/// <summary>
/// The signature workflow of <see cref="MonitoringState" /> (AC-7c): escalation policies with paging. An admin defines
/// policies (step 1 pages the primary on call at once, step 2 the secondary after a set number of minutes without an
/// acknowledgement, an optional step 3 a named engineer, and the steps can repeat). When an incident opens (a rule fires,
/// or an engineer declares one), the policy that covers its service is copied onto it, and the paging engine sends each
/// page when it is due on the site's clock: an in-app notification to the engineer (the page), a timeline entry naming
/// who was paged and by which channels, and an audit entry recorded as the system. Acknowledging stops the paging, the
/// earlier recipients are told their page is covered, and it is recorded too; resolving also stops it. When every page went
/// out unanswered, paging stops and the admins are told. The engine runs on every live tick, when a console page opens,
/// and after a snapshot is restored, so pages that fell due meanwhile go out at their due time.
/// </summary>
public sealed partial class MonitoringState
{
    #region Fields and Constants

    /// <summary>The most minutes a policy waits before a step.</summary>
    public const int MaxEscalationMinutes = 60;

    /// <summary>The longest escalation policy name.</summary>
    public const int MaxPolicyNameLength = 60;

    /// <summary>The most escalation policies.</summary>
    public const int MaxPolicies = 20;

    /// <summary>The most times a policy's steps run again when nobody acknowledges.</summary>
    public const int MaxPolicyRepeats = 3;

    /// <summary>The fewest minutes a policy waits before a step.</summary>
    public const int MinEscalationMinutes = 1;

    /// <summary>The admins' escalation policies page.</summary>
    private const string EscalationLink = ConsoleBase + "/admin/escalation";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the active policy that names another set of services including some of these, or (for a default) another active default; <see langword="null" /> when there is none.</summary>
    public MonitoringEscalationPolicy? CoverageConflict(bool isDefault, IEnumerable<string> serviceIds, int exceptPolicyId)
    {
        var ids = serviceIds.ToHashSet(StringComparer.Ordinal);

        return policies.Where(policy => policy.Id != exceptPolicyId && policy.IsActive)
            .FirstOrDefault(policy => isDefault ? policy.IsDefault : !policy.IsDefault && policy.ServiceIds.Any(ids.Contains));
    }

    /// <summary>Deletes an escalation policy (admins). Incidents already paging keep the copy they started with.</summary>
    public MonitoringResult DeletePolicy(int policyId)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (policies.FirstOrDefault(policy => policy.Id == policyId) is not { } doomed)
            return MonitoringResult.NotFound;

        policies.Remove(doomed);
        Record("Deleted escalation policy", PolicyTarget(doomed.Name), EscalationLink, CoverageText(doomed));
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>
    /// Says in words what a policy does with today's on-call rotation, for the policy dialog's preview and the on-call
    /// page, like "Pages Priya Nair (primary on call now) at once; if nobody acknowledges within 2 minutes, pages Marcus
    /// Osei (secondary on call now)." Out-of-range values are described as they are; the form says what is wrong.
    /// </summary>
    public string DescribePolicy(int secondaryAfterMinutes, string? thirdEngineerId, int? thirdAfterMinutes, int repeats)
    {
        var shift = ShiftForWeek(WeekIndex(Clock.Now));
        var text = $"Pages {EngineerName(shift.PrimaryId)} (primary on call now) at once; if nobody acknowledges within {Minutes(secondaryAfterMinutes)}, "
            + $"pages {EngineerName(shift.SecondaryId)} (secondary on call now)";

        if (thirdEngineerId is not null && thirdAfterMinutes is { } third)
            text += $"; {Minutes(third)} later, pages {EngineerName(thirdEngineerId)}";

        text += ".";
        var steps = thirdAfterMinutes is not null && thirdEngineerId is not null ? 3 : 2;

        if (repeats > 0)
            text += string.Create(CultureInfo.InvariantCulture, $" Then the steps run again{(repeats == 1 ? " once" : repeats == 2 ? " twice" : $" {repeats} times")} ({steps * (repeats + 1)} pages at most).");

        var total = PagingStopOffset(secondaryAfterMinutes, thirdEngineerId is null ? null : thirdAfterMinutes, Math.Max(0, repeats));

        return text + $" If nobody acknowledges within {Minutes((int)total.TotalMinutes)}, paging stops and the admins are told.";
    }

    /// <summary>Returns an escalation policy by its number, or <see langword="null" />.</summary>
    public MonitoringEscalationPolicy? FindPolicy(int policyId) => policies.FirstOrDefault(policy => policy.Id == policyId);

    /// <summary>Returns whether another policy already has this name, ignoring case (the policy being edited doesn't count).</summary>
    public bool IsPolicyNameTaken(string? name, int exceptPolicyId = 0) =>
        policies.Any(policy => policy.Id != exceptPolicyId && string.Equals(policy.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns the next page an incident's paging will send: when, which step and round, and who it would reach today (the
    /// rotation at that time); <see langword="null" /> when paging stopped or every page went out.
    /// </summary>
    public (DateTime Due, int Step, int Round, string EngineerId)? NextPage(MonitoringIncident incident)
    {
        if (incident.Paging is not { IsActive: true } paging)
            return null;

        var plan = PagingSchedule(paging.SecondaryAfterMinutes, paging.ThirdEngineerId is null ? null : paging.ThirdAfterMinutes, paging.Repeats);

        if (paging.Pages.Count >= plan.Count)
            return null;

        var (step, round, offset) = plan[paging.Pages.Count];
        var due = paging.StartedAt + offset;

        return (due, step, round, PagedEngineerId(paging, step, due));
    }

    /// <summary>Returns when an incident's paging stops if nobody acknowledges, or <see langword="null" /> when it already stopped.</summary>
    public DateTime? PagingEndsAt(MonitoringIncident incident) =>
        incident.Paging is { IsActive: true } paging
            ? paging.StartedAt + PagingStopOffset(paging.SecondaryAfterMinutes, paging.ThirdEngineerId is null ? null : paging.ThirdAfterMinutes, paging.Repeats)
            : null;

    /// <summary>Counts the incidents paging now under a policy.</summary>
    public int PagingCount(int policyId) => incidents.Count(incident => incident.Paging is { IsActive: true } paging && paging.PolicyId == policyId);

    /// <summary>
    /// The pages an escalation policy sends, in order: (step, round, minutes after the start). Step 1 at once, step 2 after
    /// the secondary's minutes, step 3 that many minutes later; a round lasts until the last step plus the secondary's
    /// minutes, then the next round starts.
    /// </summary>
    public static IReadOnlyList<(int Step, int Round, TimeSpan Offset)> PagingSchedule(int secondaryAfterMinutes, int? thirdAfterMinutes, int repeats)
    {
        var steps = new List<(int Step, double Minutes)> { (1, 0), (2, secondaryAfterMinutes) };

        if (thirdAfterMinutes is { } third)
            steps.Add((3, secondaryAfterMinutes + third));

        var roundMinutes = steps[^1].Minutes + secondaryAfterMinutes;

        return Enumerable.Range(0, Math.Clamp(repeats, 0, MaxPolicyRepeats) + 1)
            .SelectMany(round => steps.Select(step => (step.Step, round, TimeSpan.FromMinutes((round * roundMinutes) + step.Minutes))))
            .ToList();
    }

    /// <summary>How long after the start paging stops when nobody acknowledges: every round, each ending a secondary's wait after its last page.</summary>
    public static TimeSpan PagingStopOffset(int secondaryAfterMinutes, int? thirdAfterMinutes, int repeats) =>
        TimeSpan.FromMinutes((Math.Clamp(repeats, 0, MaxPolicyRepeats) + 1) * (secondaryAfterMinutes + (thirdAfterMinutes ?? 0) + secondaryAfterMinutes));

    /// <summary>Returns the open incidents still paging that paged the engineer, newest first: the pages waiting for them.</summary>
    public IReadOnlyList<MonitoringIncident> PagesWaitingFor(string? engineerId) =>
        engineerId is null
            ? Array.Empty<MonitoringIncident>()
            : incidents.Where(incident => incident.Paging is { IsActive: true } paging && paging.Pages.Any(page => page.EngineerId == engineerId))
                .OrderByDescending(incident => incident.StartedAt).ToList();

    /// <summary>Returns the active policy for a service: the one that names it, else the active default; <see langword="null" /> for none.</summary>
    public MonitoringEscalationPolicy? PolicyFor(string serviceId) =>
        policies.FirstOrDefault(policy => policy.IsActive && !policy.IsDefault && policy.ServiceIds.Contains(serviceId))
        ?? policies.FirstOrDefault(policy => policy.IsActive && policy.IsDefault);

    /// <summary>
    /// Returns what is wrong with an escalation policy's settings, or <see langword="null" /> (the same check the state
    /// applies; coverage and the name's uniqueness are checked separately).
    /// </summary>
    public static string? PolicyProblem(string? name, bool isDefault, IReadOnlyCollection<string>? serviceIds, int secondaryAfterMinutes, string? thirdEngineerId, int? thirdAfterMinutes, int repeats)
    {
        if (!IsText(name, MaxPolicyNameLength) || name != name!.Trim())
            return string.Create(CultureInfo.InvariantCulture, $"Give the policy a name of 1 to {MaxPolicyNameLength} characters.");

        if (isDefault && serviceIds is { Count: > 0 })
            return "A default policy covers every service without its own policy, so it names none.";

        if (!isDefault && serviceIds is not { Count: > 0 })
            return "Choose at least one service, or make it the default policy.";

        if (secondaryAfterMinutes is < MinEscalationMinutes or > MaxEscalationMinutes)
            return string.Create(CultureInfo.InvariantCulture, $"Wait {MinEscalationMinutes} to {MaxEscalationMinutes} minutes before paging the secondary.");

        if ((thirdEngineerId is null) != (thirdAfterMinutes is null))
            return "Step 3 needs both an engineer and the minutes to wait, or neither.";

        if (thirdEngineerId is not null && MonitoringSeedData.Engineers().All(engineer => engineer.Id != thirdEngineerId))
            return "Step 3 pages an engineer on the rotation.";

        if (thirdAfterMinutes is < MinEscalationMinutes or > MaxEscalationMinutes)
            return string.Create(CultureInfo.InvariantCulture, $"Wait {MinEscalationMinutes} to {MaxEscalationMinutes} minutes before step 3.");

        return repeats is < 0 or > MaxPolicyRepeats ? string.Create(CultureInfo.InvariantCulture, $"Repeat the steps 0 to {MaxPolicyRepeats} times.") : null;
    }

    /// <summary>
    /// Sends every page that is due on the site's clock and stops paging that went unanswered; returns how many incidents
    /// changed. Console pages call it when they open (pages that fell due meanwhile go out at their due time).
    /// </summary>
    public int RunPaging()
    {
        var changed = AdvancePaging(Clock.Now);

        if (changed > 0)
            Commit();

        return changed;
    }

    /// <summary>
    /// Creates (<paramref name="policyId" /> 0) or edits an escalation policy (admins). See <see cref="PolicyProblem" />; the
    /// name is unique (<see cref="MonitoringResult.Duplicate" />), and an active policy may not cover a service another active
    /// policy names, nor be a second active default (<see cref="MonitoringResult.WrongState" />). Incidents already paging
    /// keep the copy they started with.
    /// </summary>
    public MonitoringResult SavePolicy(int policyId, string name, bool isDefault, IReadOnlyList<string> serviceIds, int secondaryAfterMinutes, string? thirdEngineerId, int? thirdAfterMinutes,
        int repeats, bool isActive, out MonitoringEscalationPolicy? saved)
    {
        saved = null;

        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        var existing = policyId == 0 ? null : FindPolicy(policyId);

        if (policyId != 0 && existing is null)
            return MonitoringResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        thirdEngineerId = string.IsNullOrWhiteSpace(thirdEngineerId) ? null : thirdEngineerId;
        var ids = (serviceIds ?? Array.Empty<string>()).Distinct().ToList();

        if (PolicyProblem(name, isDefault, ids, secondaryAfterMinutes, thirdEngineerId, thirdAfterMinutes, repeats) is not null)
            return MonitoringResult.Invalid;

        if (ids.Any(id => FindService(id) is null))
            return MonitoringResult.NotFound;

        if (IsPolicyNameTaken(name, policyId))
            return MonitoringResult.Duplicate;

        if (isActive && CoverageConflict(isDefault, ids, policyId) is not null)
            return MonitoringResult.WrongState;

        var isNew = existing is null;

        if (existing is null)
        {
            if (policies.Count >= MaxPolicies || nextPolicyId >= NextIdLimit)
                return MonitoringResult.Invalid;

            existing = new MonitoringEscalationPolicy { Id = nextPolicyId++ };
            policies.Add(existing);
        }
        else if (existing.Name == name && existing.IsDefault == isDefault && existing.ServiceIds.Order().SequenceEqual(ids.Order()) && existing.SecondaryAfterMinutes == secondaryAfterMinutes
                 && existing.ThirdEngineerId == thirdEngineerId && existing.ThirdAfterMinutes == thirdAfterMinutes && existing.Repeats == repeats && existing.IsActive == isActive)
        {
            saved = existing;

            return MonitoringResult.NoChange;
        }

        existing.Name = name;
        existing.IsDefault = isDefault;
        existing.ServiceIds = ids;
        existing.SecondaryAfterMinutes = secondaryAfterMinutes;
        existing.ThirdEngineerId = thirdEngineerId;
        existing.ThirdAfterMinutes = thirdAfterMinutes;
        existing.Repeats = repeats;
        existing.IsActive = isActive;
        saved = existing;
        Record(isNew ? "Created escalation policy" : "Updated escalation policy", PolicyTarget(name), EscalationLink,
            $"{CoverageText(existing)}; {StepsText(existing)}" + (isActive ? string.Empty : " (off)"));
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Turns an escalation policy on or off (admins); turning one on that would overlap another active policy is refused.</summary>
    public MonitoringResult SetPolicyActive(int policyId, bool isActive)
    {
        if (Admin() is null)
            return MonitoringResult.NotAllowed;

        if (FindPolicy(policyId) is not { } policy)
            return MonitoringResult.NotFound;

        if (policy.IsActive == isActive)
            return MonitoringResult.NoChange;

        if (isActive && CoverageConflict(policy.IsDefault, policy.ServiceIds, policy.Id) is not null)
            return MonitoringResult.WrongState;

        policy.IsActive = isActive;
        Record(isActive ? "Turned on escalation policy" : "Turned off escalation policy", PolicyTarget(policy.Name), EscalationLink, CoverageText(policy));
        Changed?.Invoke();

        return MonitoringResult.Ok;
    }

    /// <summary>Says which services a policy covers, like "Payments API, API gateway" or "Every service without its own policy".</summary>
    public string CoverageText(MonitoringEscalationPolicy policy) =>
        policy.IsDefault ? "Every service without its own policy" : string.Join(", ", policy.ServiceIds.Select(id => FindService(id)?.Name ?? id));

    /// <summary>Says a policy's steps briefly, like "Primary, then secondary after 2 min, then Daniel Okafor after 20 min; repeats once".</summary>
    public string StepsText(MonitoringEscalationPolicy policy) =>
        string.Create(CultureInfo.InvariantCulture, $"Primary, then secondary after {policy.SecondaryAfterMinutes} min")
        + (policy.ThirdEngineerId is { } third && policy.ThirdAfterMinutes is { } minutes ? string.Create(CultureInfo.InvariantCulture, $", then {EngineerName(third)} after {minutes} min") : string.Empty)
        + (policy.Repeats switch { 0 => string.Empty, 1 => "; repeats once", 2 => "; repeats twice", _ => string.Create(CultureInfo.InvariantCulture, $"; repeats {policy.Repeats} times") });

    #endregion

    #region Private Methods

    /// <summary>Sends the due pages of every incident still paging, and stops the ones nobody answered; returns how many incidents changed.</summary>
    private int AdvancePaging(DateTime now)
    {
        var changed = 0;

        foreach (var incident in incidents.Where(incident => incident.Paging is { IsActive: true }).ToList())
        {
            var paging = incident.Paging!;

            // Paging belongs to an open incident; one acknowledged or resolved elsewhere stops here too.
            if (incident.Status != MonitoringIncidentStatus.Open)
            {
                paging.StopReason = incident.Status == MonitoringIncidentStatus.Resolved ? MonitoringPagingStop.Resolved : MonitoringPagingStop.Acknowledged;
                paging.StoppedAt = now;

                if (paging.StopReason == MonitoringPagingStop.Acknowledged)
                {
                    paging.AcknowledgedBy = Clip(incident.Assignee ?? MonitoringSeedData.ProductName, ShowcaseIdentity.MaxNameLength);
                    paging.AcknowledgedAt = now;
                }

                changed++;
                continue;
            }

            var third = paging.ThirdEngineerId is null ? null : paging.ThirdAfterMinutes;
            var plan = PagingSchedule(paging.SecondaryAfterMinutes, third, paging.Repeats);
            var before = paging.Pages.Count;

            while (paging.Pages.Count < plan.Count && paging.StartedAt + plan[paging.Pages.Count].Offset <= now)
                SendPage(incident, paging, plan[paging.Pages.Count], plan);

            var stopAt = paging.StartedAt + PagingStopOffset(paging.SecondaryAfterMinutes, third, paging.Repeats);

            if (paging.Pages.Count >= plan.Count && stopAt <= now)
            {
                paging.StopReason = MonitoringPagingStop.NobodyAnswered;
                paging.StoppedAt = stopAt;
                var text = $"Nobody acknowledged after {Plural(paging.Pages.Count, "page")}; paging stopped.";
                AddSystemEntry(incident, stopAt, text);
                Record(stopAt, MonitoringSeedData.SystemAuthor, "Paging stopped", IncidentTarget(incident.Id), IncidentLink(incident.Id), $"Nobody acknowledged after {Plural(paging.Pages.Count, "page")}");

                foreach (var admin in MonitoringSeedData.DemoAccounts().Where(account => account.RoleId == MonitoringSeedData.AdminRole))
                    Notify(admin.Id, MonitoringNotificationKind.Page, FormattableString.Invariant($"Nobody acknowledged incident #{incident.Id}"),
                        $"{incident.Title}: {Plural(paging.Pages.Count, "page")} went out under \"{paging.PolicyName}\". Someone needs to pick it up.", incident.Id, fromSystem: true, at: stopAt);

                changed++;
            }
            else if (paging.Pages.Count != before)
            {
                changed++;
            }
        }

        return changed;
    }

    /// <summary>Adds a timeline entry by the monitoring system at its time, in time order, when the incident has room.</summary>
    private void AddSystemEntry(MonitoringIncident incident, DateTime at, string text)
    {
        if (incident.Updates.Count >= MaxUpdatesPerIncident - ReservedUpdates || nextUpdateId >= NextIdLimit)
            return;

        var index = incident.Updates.FindLastIndex(update => update.Time <= at) + 1;
        incident.Updates.Insert(index, new MonitoringIncidentUpdate(at, MonitoringSeedData.SystemAuthor, Clip(text, MaxUpdateLength)) { Id = nextUpdateId++, Kind = MonitoringUpdateKind.Page });
    }

    /// <summary>How a page reaches an engineer: always in the app, plus the channels of their notification settings (a text only for a critical incident).</summary>
    private string ChannelsFor(string engineerId, MonitoringSeverity severity)
    {
        var value = preferences.TryGetValue(engineerId, out var saved) ? saved : new MonitoringNotificationPreferences();
        var channels = new List<string> { "in-app" };

        if (value.Email)
            channels.Add("email");

        if (value.Push)
            channels.Add("push");

        if (value.SmsForCritical && severity == MonitoringSeverity.Critical)
            channels.Add("text message");

        return channels.Count switch
        {
            1 => channels[0],
            2 => $"{channels[0]} and {channels[1]}",
            _ => $"{string.Join(", ", channels.Take(channels.Count - 1))}, and {channels[^1]}",
        };
    }

    /// <summary>The acknowledgement's timeline text: who, and when paging was running, how many pages it stopped after.</summary>
    private static string AcknowledgedText(ShowcaseAccount account, MonitoringPaging? paging) =>
        paging is null ? "Acknowledged." : $"Acknowledged by {account.Name}; paging stopped after {Plural(paging.Pages.Count, "page")}.";

    private string EngineerName(string? engineerId) => FindEngineer(engineerId)?.Name ?? engineerId ?? "nobody";

    private static string Minutes(int minutes) => minutes == 1 ? "1 minute" : string.Create(CultureInfo.InvariantCulture, $"{minutes} minutes");

    /// <summary>Who a step pages at a time: the primary or secondary on call then (with overrides), or step 3's engineer.</summary>
    private string PagedEngineerId(MonitoringPaging paging, int step, DateTime at)
    {
        var shift = ShiftForWeek(WeekIndex(at));

        return step switch
        {
            1 => shift.PrimaryId,
            2 => shift.SecondaryId,
            _ => paging.ThirdEngineerId ?? shift.SecondaryId,
        };
    }

    private static string Plural(int count, string noun) => count == 1 ? $"1 {noun}" : string.Create(CultureInfo.InvariantCulture, $"{count} {noun}s");

    private static string PolicyTarget(string name) => $"Escalation policy: {name}";

    /// <summary>The role a step pages, in words, like "primary on call".</summary>
    private static string StepRole(int step) => step switch
    {
        1 => "primary on call",
        2 => "secondary on call",
        _ => "named in the policy",
    };

    /// <summary>
    /// Sends one page at its due time: the page record, an in-app notification to the engineer (it reaches the signed-in
    /// person too: the system sends it), a timeline entry with the channels, and an audit entry recorded as the system.
    /// </summary>
    private void SendPage(MonitoringIncident incident, MonitoringPaging paging, (int Step, int Round, TimeSpan Offset) planned, IReadOnlyList<(int Step, int Round, TimeSpan Offset)> plan)
    {
        var at = paging.StartedAt + planned.Offset;
        var engineerId = PagedEngineerId(paging, planned.Step, at);
        var name = EngineerName(engineerId);
        var steps = plan.Max(item => item.Step);
        var rounds = plan.Max(item => item.Round) + 1;
        var channels = ChannelsFor(engineerId, incident.Severity);
        var where = string.Create(CultureInfo.InvariantCulture, $"step {planned.Step} of {steps}") + (rounds > 1 ? string.Create(CultureInfo.InvariantCulture, $", round {planned.Round + 1} of {rounds}") : string.Empty);

        paging.Pages.Add(new MonitoringPage { Step = planned.Step, Round = planned.Round, EngineerId = engineerId, At = at, Channels = channels });
        AddSystemEntry(incident, at, $"Paged {name} ({StepRole(planned.Step)}, {where}) by {channels}.");
        Record(at, MonitoringSeedData.SystemAuthor, "Paged", IncidentTarget(incident.Id), IncidentLink(incident.Id), $"{name}, {StepRole(planned.Step)}, {where}");
        Notify(engineerId, MonitoringNotificationKind.Page, FormattableString.Invariant($"Paged: incident #{incident.Id} {incident.Title}"),
            $"{char.ToUpperInvariant(where[0])}{where[1..]}: you're {StepRole(planned.Step)}. Acknowledge to stop the escalation.", incident.Id,
            fromSystem: true, at: at);
    }

    /// <summary>
    /// Starts paging a new incident under the policy that covers its service, from <paramref name="at" />; returns whether a
    /// policy covered it. The first page goes out on the next <see cref="AdvancePaging" />.
    /// </summary>
    private bool StartPaging(MonitoringIncident incident, DateTime at)
    {
        if (PolicyFor(incident.ServiceId) is not { } policy)
            return false;

        incident.Paging = new MonitoringPaging
        {
            PolicyId = policy.Id, PolicyName = policy.Name, SecondaryAfterMinutes = policy.SecondaryAfterMinutes, ThirdEngineerId = policy.ThirdEngineerId,
            ThirdAfterMinutes = policy.ThirdEngineerId is null ? null : policy.ThirdAfterMinutes, Repeats = policy.Repeats, StartedAt = at,
        };

        return true;
    }

    /// <summary>
    /// Stops an incident's paging (an acknowledgement or a resolve) and tells the engineers it paged before, other than the
    /// person who did it, that their page is covered. Returns how many pages had gone out, or <see langword="null" /> when it
    /// wasn't paging.
    /// </summary>
    private int? StopPaging(MonitoringIncident incident, MonitoringPagingStop reason, ShowcaseAccount account)
    {
        if (incident.Paging is not { IsActive: true } paging)
            return null;

        paging.StopReason = reason;
        paging.StoppedAt = Clock.Now;

        if (reason == MonitoringPagingStop.Acknowledged)
        {
            paging.AcknowledgedBy = Clip(account.Name, ShowcaseIdentity.MaxNameLength);
            paging.AcknowledgedAt = Clock.Now;
        }

        foreach (var engineerId in paging.Pages.Select(page => page.EngineerId).Distinct())
            Notify(engineerId, MonitoringNotificationKind.Page, FormattableString.Invariant($"Your page for #{incident.Id} is covered"),
                $"{account.Name} {(reason == MonitoringPagingStop.Acknowledged ? "acknowledged" : "resolved")} {incident.Title}. You don't need to respond.", incident.Id);

        return paging.Pages.Count;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the escalation policies, in number order.</summary>
    public IReadOnlyList<MonitoringEscalationPolicy> Policies => policies.OrderBy(policy => policy.Id).ToList();

    #endregion
}
