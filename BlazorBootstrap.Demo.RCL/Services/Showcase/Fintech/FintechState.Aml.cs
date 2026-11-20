using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>
/// AML case management (AC-7e), the Banking Operations signature workflow. Monitoring rules raise alerts on customers'
/// payments; an analyst triages them (assigns, dismisses a false positive with a reason, or opens a case from one or more
/// alerts on the same customer), investigates with notes and named evidence, and closes the case with a disposition, or
/// escalates it to a senior reviewer. The four-eyes rule: only a senior reviewer closes an escalated case, never the person
/// who escalated it, and "Reported" (a SAR filed) always needs a senior reviewer's approval. Customers are never told about
/// alerts or cases (no tipping-off): every notification here goes to staff, and every state change is audited (a note's
/// length is logged, not its text).
/// </summary>
public sealed partial class FintechState
{
    #region Fields and Constants

    /// <summary>The longest closing summary.</summary>
    public const int MaxAmlSummary = 2000;

    /// <summary>The longest investigation note.</summary>
    public const int MaxAmlNote = 2000;

    /// <summary>The longest reason (dismiss, escalate, return).</summary>
    public const int MaxAmlReason = 500;

    /// <summary>The longest evidence name.</summary>
    public const int MaxEvidenceName = 80;

    /// <summary>The shortest closing summary.</summary>
    public const int MinAmlSummary = 20;

    /// <summary>The shortest investigation note.</summary>
    public const int MinAmlNote = 10;

    /// <summary>The amount just under the check threshold from which the structuring rule counts a payment.</summary>
    public const decimal StructuringFloor = 1800m;

    private const int MaxAmlAlerts = 1000;

    private const int MaxAmlCases = 300;

    private const int MaxAmlEvents = 200;

    private static readonly Regex filingReference = new(@"^SAR-\d{4}-\d{5}$", RegexOptions.CultureInvariant);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Starts the investigation of an open case (any staff member); the timeline and the audit log say who.</summary>
    public FintechResult StartInvestigation(int caseId)
    {
        if (!IsOps || CurrentStaffId is not { } me)
            return FintechResult.NotAllowed;

        if (FindAmlCase(caseId) is not { } amlCase)
            return FintechResult.NotFound;

        if (amlCase.Status != FintechAmlCaseStatus.Open || amlCase.Events.Count >= MaxAmlEvents)
            return FintechResult.WrongState;

        amlCase.Status = FintechAmlCaseStatus.Investigating;
        AddEvent(amlCase, FintechAmlEventKind.Investigating, amlCase.AssigneeId == me ? "Investigation started." : $"Investigation started by {StaffNameOf(me)}.");
        Record("AML investigation started", CaseTarget(amlCase), CasePath(amlCase.Id), null);
        RaiseChanged();

        return FintechResult.Ok;
    }

    /// <summary>
    /// Adds an investigation note (<see cref="MinAmlNote" /> to <see cref="MaxAmlNote" /> characters) to a case that isn't
    /// closed. The audit log records that a note was added and its length, not its text.
    /// </summary>
    public FintechResult AddCaseNote(int caseId, string text)
    {
        if (!IsOps)
            return FintechResult.NotAllowed;

        if (FindAmlCase(caseId) is not { } amlCase)
            return FintechResult.NotFound;

        text = text?.Trim() ?? string.Empty;

        if (text.Length is < MinAmlNote or > MaxAmlNote)
            return FintechResult.Invalid;

        if (amlCase.Status == FintechAmlCaseStatus.Closed || amlCase.Events.Count >= MaxAmlEvents)
            return FintechResult.WrongState;

        AddEvent(amlCase, FintechAmlEventKind.Note, text);
        Record("AML note added", CaseTarget(amlCase), CasePath(amlCase.Id), $"A note of {Number(text.Length)} characters.");
        RaiseChanged();

        return FintechResult.Ok;
    }

    /// <summary>
    /// Adds a named piece of evidence (a name of 3 to <see cref="MaxEvidenceName" /> characters and a description of
    /// <see cref="MinAmlNote" /> to <see cref="MaxAmlReason" />); the demo keeps no files. Two pieces can't share a name.
    /// </summary>
    public FintechResult AddCaseEvidence(int caseId, string name, string description)
    {
        if (!IsOps)
            return FintechResult.NotAllowed;

        if (FindAmlCase(caseId) is not { } amlCase)
            return FintechResult.NotFound;

        name = name?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;

        if (name.Length is < 3 or > MaxEvidenceName || description.Length is < MinAmlNote or > MaxAmlReason)
            return FintechResult.Invalid;

        if (amlCase.Status == FintechAmlCaseStatus.Closed || amlCase.Events.Count >= MaxAmlEvents)
            return FintechResult.WrongState;

        if (IsEvidenceNameTaken(caseId, name))
            return FintechResult.Duplicate;

        AddEvent(amlCase, FintechAmlEventKind.Evidence, description, name);
        Record("AML evidence added", CaseTarget(amlCase), CasePath(amlCase.Id), $"\"{name}\".");
        RaiseChanged();

        return FintechResult.Ok;
    }

    /// <summary>
    /// Assigns alerts to a staff member (analysts and senior reviewers can work alerts); a new alert moves to triage. Dismissed
    /// alerts and alerts already in a case are left as they are. The assignee is told, unless they assigned it to themselves.
    /// </summary>
    public FintechBulkOutcome AssignAlerts(IReadOnlyCollection<int> alertIds, string staffId)
    {
        if (!IsOps)
            return new FintechBulkOutcome(FintechResult.NotAllowed, 0, 0);

        if (FintechSeedData.StaffName(staffId) is not { } name)
            return new FintechBulkOutcome(FintechResult.NotFound, 0, 0);

        int changed = 0, unchanged = 0;
        var assigned = new List<FintechAmlAlert>();

        foreach (var id in (alertIds ?? Array.Empty<int>()).Distinct())
        {
            if (FindAmlAlert(id) is not { Status: FintechAmlAlertStatus.New or FintechAmlAlertStatus.InTriage } alert || (alert.AssigneeId == staffId && alert.Status == FintechAmlAlertStatus.InTriage))
            {
                unchanged++;
                continue;
            }

            alert.AssigneeId = staffId;
            alert.Status = FintechAmlAlertStatus.InTriage;
            Record("AML alert assigned", AlertTarget(alert), AlertPath(alert.Id), $"To {name}.");
            assigned.Add(alert);
            changed++;
        }

        if (changed == 0)
            return new FintechBulkOutcome(unchanged == 0 ? FintechResult.Invalid : FintechResult.NoChange, 0, unchanged);

        NotifyStaff(staffId, assigned.Count == 1 ? $"Alert {Number(assigned[0].Id)} is assigned to you" : $"{Number(assigned.Count)} AML alerts are assigned to you",
            assigned.Count == 1 ? $"{FintechSeedData.RuleName(assigned[0].Rule)} on {CustomerName(assigned[0].CustomerId)}, {assigned[0].Risk} risk." : $"From {StaffNameOf(CurrentStaffId)}, in the AML alert queue.",
            assigned.Count == 1 ? AlertPath(assigned[0].Id) : OpsBase + "/aml/alerts");
        RaiseChanged();

        return new FintechBulkOutcome(FintechResult.Ok, changed, unchanged);
    }

    /// <summary>
    /// Closes a case with a disposition and a closing summary (<see cref="MinAmlSummary" /> to <see cref="MaxAmlSummary" />
    /// characters); Reported needs a filing reference like <c>SAR-2026-00412</c>. The four-eyes rule: an escalated case is
    /// closed only by a senior reviewer who didn't escalate it (SelfApproval otherwise), and a case that isn't escalated can't
    /// be closed as Reported (ApprovalRequired: escalate it, proposing Reported). The analyst and whoever escalated are told.
    /// </summary>
    public FintechResult CloseCase(int caseId, FintechAmlDisposition disposition, string? filing, string summary)
    {
        if (!IsOps || CurrentStaffId is not { } me)
            return FintechResult.NotAllowed;

        if (FindAmlCase(caseId) is not { } amlCase)
            return FintechResult.NotFound;

        summary = summary?.Trim() ?? string.Empty;
        filing = string.IsNullOrWhiteSpace(filing) ? null : filing.Trim().ToUpperInvariant();

        if (!Enum.IsDefined(disposition) || summary.Length is < MinAmlSummary or > MaxAmlSummary
            || (disposition == FintechAmlDisposition.Reported ? filing is null || !filingReference.IsMatch(filing) : filing is not null))
            return FintechResult.Invalid;

        if (amlCase.Status == FintechAmlCaseStatus.Closed || amlCase.Events.Count >= MaxAmlEvents)
            return FintechResult.WrongState;

        if (amlCase.Status == FintechAmlCaseStatus.Escalated)
        {
            if (!IsSenior)
                return FintechResult.NotAllowed;

            if (amlCase.EscalatedById == me)
                return FintechResult.SelfApproval;
        }
        else if (disposition == FintechAmlDisposition.Reported)
        {
            return FintechResult.ApprovalRequired;
        }

        var escalatedBy = amlCase.EscalatedById;
        var approving = amlCase.Status == FintechAmlCaseStatus.Escalated;
        amlCase.Status = FintechAmlCaseStatus.Closed;
        amlCase.Disposition = disposition;
        amlCase.FilingReference = filing;
        amlCase.ClosingSummary = summary;
        amlCase.ClosedAt = Clock.Now;
        amlCase.ClosedById = me;
        amlCase.ProposedDisposition = null;
        amlCase.ProposedSummary = null;
        amlCase.EscalatedById = null;
        amlCase.EscalationReason = null;
        amlCase.ReviewerId = null;

        var outcome = $"{FintechSeedData.DispositionName(disposition)}{(filing is null ? string.Empty : $" ({filing})")}";
        AddEvent(amlCase, FintechAmlEventKind.Closed, $"{outcome}: {summary}");
        Record(approving ? "AML case closed on review" : "AML case closed", CaseTarget(amlCase), CasePath(amlCase.Id), outcome + ".");

        foreach (var staffId in new[] { amlCase.AssigneeId, escalatedBy }.OfType<string>().Distinct())
            NotifyStaff(staffId, $"Case {Number(amlCase.Id)} is closed", $"{StaffNameOf(me)} closed it: {outcome}.", CasePath(amlCase.Id));

        RaiseChanged();

        return FintechResult.Ok;
    }

    /// <summary>
    /// Dismisses alerts as false positives with a reason (<see cref="MinAmlNote" /> to <see cref="MaxAmlReason" /> characters).
    /// Alerts already dismissed or in a case are left as they are.
    /// </summary>
    public FintechBulkOutcome DismissAlerts(IReadOnlyCollection<int> alertIds, string reason)
    {
        if (!IsOps || CurrentStaffId is not { } me)
            return new FintechBulkOutcome(FintechResult.NotAllowed, 0, 0);

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length is < MinAmlNote or > MaxAmlReason)
            return new FintechBulkOutcome(FintechResult.Invalid, 0, 0);

        int changed = 0, unchanged = 0;

        foreach (var id in (alertIds ?? Array.Empty<int>()).Distinct())
        {
            if (FindAmlAlert(id) is not { Status: FintechAmlAlertStatus.New or FintechAmlAlertStatus.InTriage } alert)
            {
                unchanged++;
                continue;
            }

            alert.Status = FintechAmlAlertStatus.Dismissed;
            alert.DismissReason = reason;
            alert.HandledAt = Clock.Now;
            alert.AssigneeId ??= me;
            Record("AML alert dismissed", AlertTarget(alert), AlertPath(alert.Id), $"False positive: {reason}");
            changed++;
        }

        if (changed == 0)
            return new FintechBulkOutcome(unchanged == 0 ? FintechResult.Invalid : FintechResult.NoChange, 0, unchanged);

        RaiseChanged();

        return new FintechBulkOutcome(FintechResult.Ok, changed, unchanged);
    }

    /// <summary>
    /// Escalates an open or investigating case to a senior reviewer with a reason (<see cref="MinAmlNote" /> to
    /// <see cref="MaxAmlReason" /> characters). The reviewer must be a senior reviewer other than the person escalating
    /// (four-eyes rule). A proposal to close is optional, and needed to report: the proposed disposition, its filing reference
    /// for Reported, and the closing summary. The reviewer is told.
    /// </summary>
    public FintechResult EscalateCase(int caseId, string reviewerId, string reason, FintechAmlDisposition? proposal, string? filing, string? summary)
    {
        if (!IsOps || CurrentStaffId is not { } me)
            return FintechResult.NotAllowed;

        if (FindAmlCase(caseId) is not { } amlCase)
            return FintechResult.NotFound;

        reason = reason?.Trim() ?? string.Empty;
        summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        filing = string.IsNullOrWhiteSpace(filing) ? null : filing.Trim().ToUpperInvariant();

        if (FintechSeedData.Staff().FirstOrDefault(member => member.Id == reviewerId) is not { IsSeniorReviewer: true } reviewer)
            return FintechResult.NotFound;

        if (reviewer.Id == me)
            return FintechResult.SelfApproval;

        if (reason.Length is < MinAmlNote or > MaxAmlReason || (proposal is { } proposed && (!Enum.IsDefined(proposed) || summary is not { Length: >= MinAmlSummary and <= MaxAmlSummary }
                || (proposed == FintechAmlDisposition.Reported ? filing is null || !filingReference.IsMatch(filing) : filing is not null))))
            return FintechResult.Invalid;

        if (amlCase.Status is not (FintechAmlCaseStatus.Open or FintechAmlCaseStatus.Investigating) || amlCase.Events.Count >= MaxAmlEvents)
            return FintechResult.WrongState;

        amlCase.Status = FintechAmlCaseStatus.Escalated;
        amlCase.EscalatedById = me;
        amlCase.EscalationReason = reason;
        amlCase.ReviewerId = reviewer.Id;
        amlCase.ProposedDisposition = proposal;
        amlCase.ProposedSummary = proposal is null ? null : summary;
        amlCase.FilingReference = proposal is null ? null : filing;

        var proposing = proposal is { } value ? $", proposing {FintechSeedData.DispositionName(value)}{(filing is null ? string.Empty : $" ({filing})")}" : string.Empty;
        AddEvent(amlCase, FintechAmlEventKind.Escalated, $"To {reviewer.Name}{proposing}: {reason}");
        Record("AML case escalated", CaseTarget(amlCase), CasePath(amlCase.Id), $"To {reviewer.Name}{proposing}.");
        NotifyStaff(reviewer.Id, $"Case {Number(amlCase.Id)} is escalated to you",
            proposal == FintechAmlDisposition.Reported ? $"{StaffNameOf(me)} proposes a SAR for {CustomerName(amlCase.CustomerId)}; it needs your approval." : $"{StaffNameOf(me)} asks for a decision on {CustomerName(amlCase.CustomerId)}.",
            CasePath(amlCase.Id));
        RaiseChanged();

        return FintechResult.Ok;
    }

    /// <summary>Returns an alert (operations), or <see langword="null" />.</summary>
    public FintechAmlAlert? FindAmlAlert(int alertId) => IsOps ? amlAlerts.FirstOrDefault(alert => alert.Id == alertId) : null;

    /// <summary>Returns a case (operations), or <see langword="null" />.</summary>
    public FintechAmlCase? FindAmlCase(int caseId) => IsOps ? amlCases.FirstOrDefault(amlCase => amlCase.Id == caseId) : null;

    /// <summary>Returns whether a case already has a piece of evidence with this name, ignoring case and surrounding spaces.</summary>
    public bool IsEvidenceNameTaken(int caseId, string? name) =>
        !string.IsNullOrWhiteSpace(name) && amlCases.FirstOrDefault(amlCase => amlCase.Id == caseId) is { } found
        && found.Events.Any(entry => entry.Kind == FintechAmlEventKind.Evidence && string.Equals(entry.EvidenceName?.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether an open, investigating, or escalated case already uses this filing reference (or a closed one reported it).</summary>
    public bool IsFilingReferenceTaken(string? filing, int exceptCaseId) =>
        !string.IsNullOrWhiteSpace(filing) && amlCases.Any(amlCase => amlCase.Id != exceptCaseId && string.Equals(amlCase.FilingReference, filing.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether a filing reference has the form <c>SAR-yyyy-nnnnn</c>.</summary>
    public static bool IsFilingReference(string? filing) => filing is not null && filingReference.IsMatch(filing.Trim().ToUpperInvariant());

    /// <summary>
    /// Opens a case from one or more alerts on the same customer that aren't dismissed or already in a case, assigned to an
    /// analyst or senior reviewer, with the alerts' highest risk as its priority and a due date <see cref="FintechSeedData.AmlCaseDays" />
    /// days ahead. The assignee is told, unless they opened it themselves.
    /// </summary>
    public (FintechResult Result, FintechAmlCase? Case) OpenCase(IReadOnlyCollection<int> alertIds, string assigneeId)
    {
        if (!IsOps || CurrentStaffId is not { } me)
            return (FintechResult.NotAllowed, null);

        if (FintechSeedData.StaffName(assigneeId) is null)
            return (FintechResult.NotFound, null);

        var linked = (alertIds ?? Array.Empty<int>()).Distinct().Select(FindAmlAlert).ToList();

        if (linked.Count == 0 || linked.Any(alert => alert is null))
            return (FintechResult.NotFound, null);

        if (linked.Any(alert => alert!.Status is not (FintechAmlAlertStatus.New or FintechAmlAlertStatus.InTriage)))
            return (FintechResult.WrongState, null);

        if (linked.Select(alert => alert!.CustomerId).Distinct().Count() != 1)
            return (FintechResult.Invalid, null);

        if (amlCases.Count >= MaxAmlCases || nextAmlCaseId >= 1_000_000)
            return (FintechResult.LimitReached, null);

        var amlCase = new FintechAmlCase
        {
            Id = nextAmlCaseId++,
            CustomerId = linked[0]!.CustomerId,
            AlertIds = linked.Select(alert => alert!.Id).OrderBy(id => id).ToList(),
            AssigneeId = assigneeId,
            OpenedAt = Clock.Now,
            Priority = linked.Max(alert => alert!.Risk),
            DueOn = Today.AddDays(FintechSeedData.AmlCaseDays),
            Status = FintechAmlCaseStatus.Open,
        };

        foreach (var alert in linked)
        {
            alert!.Status = FintechAmlAlertStatus.LinkedToCase;
            alert.CaseId = amlCase.Id;
            alert.HandledAt = Clock.Now;
            alert.AssigneeId = assigneeId;
        }

        amlCases.Add(amlCase);
        AddEvent(amlCase, FintechAmlEventKind.Opened, $"Opened by {StaffNameOf(me)} from {(linked.Count == 1 ? "1 alert" : $"{Number(linked.Count)} alerts")}, assigned to {StaffNameOf(assigneeId)}.");
        Record("AML case opened", CaseTarget(amlCase), CasePath(amlCase.Id), $"From {string.Join(", ", amlCase.AlertIds.Select(id => $"alert {Number(id)}"))}; assigned to {StaffNameOf(assigneeId)}.");
        NotifyStaff(assigneeId, $"Case {Number(amlCase.Id)} is assigned to you",
            $"AML case on {CustomerName(amlCase.CustomerId)}, {amlCase.Priority} priority, due {amlCase.DueOn.ToString("MMM d", CultureInfo.InvariantCulture)}.", CasePath(amlCase.Id));
        RaiseChanged();

        return (FintechResult.Ok, amlCase);
    }

    /// <summary>
    /// Returns an escalated case to its analyst (senior reviewer; not the person who escalated it) with a reason
    /// (<see cref="MinAmlNote" /> to <see cref="MaxAmlReason" /> characters). It goes back to investigating, the proposal is
    /// dropped, and the analyst is told.
    /// </summary>
    public FintechResult ReturnCase(int caseId, string reason)
    {
        if (!IsOps || CurrentStaffId is not { } me)
            return FintechResult.NotAllowed;

        if (FindAmlCase(caseId) is not { } amlCase)
            return FintechResult.NotFound;

        if (!IsSenior)
            return FintechResult.NotAllowed;

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length is < MinAmlNote or > MaxAmlReason)
            return FintechResult.Invalid;

        if (amlCase.Status != FintechAmlCaseStatus.Escalated || amlCase.Events.Count >= MaxAmlEvents)
            return FintechResult.WrongState;

        if (amlCase.EscalatedById == me)
            return FintechResult.SelfApproval;

        var escalatedBy = amlCase.EscalatedById;
        amlCase.Status = FintechAmlCaseStatus.Investigating;
        amlCase.EscalatedById = null;
        amlCase.EscalationReason = null;
        amlCase.ReviewerId = null;
        amlCase.ProposedDisposition = null;
        amlCase.ProposedSummary = null;
        amlCase.FilingReference = null;
        AddEvent(amlCase, FintechAmlEventKind.Returned, reason);
        Record("AML case returned", CaseTarget(amlCase), CasePath(amlCase.Id), reason);

        foreach (var staffId in new[] { amlCase.AssigneeId, escalatedBy }.OfType<string>().Distinct())
            NotifyStaff(staffId, $"Case {Number(amlCase.Id)} is returned to you", $"{StaffNameOf(me)}: {reason}", CasePath(amlCase.Id));

        RaiseChanged();

        return FintechResult.Ok;
    }

    /// <summary>Starts the triage of a new alert: it is assigned to the signed-in staff member.</summary>
    public FintechResult StartAlertTriage(int alertId)
    {
        if (!IsOps || CurrentStaffId is not { } me)
            return FintechResult.NotAllowed;

        if (FindAmlAlert(alertId) is not { } alert)
            return FintechResult.NotFound;

        if (alert.Status != FintechAmlAlertStatus.New)
            return FintechResult.WrongState;

        return AssignAlerts(new[] { alertId }, me).Result;
    }

    #endregion

    #region Private Methods

    private static string AlertPath(int alertId) => $"{OpsBase}/aml/alerts/{Number(alertId)}";

    private static string AlertTarget(FintechAmlAlert alert) => $"Alert {Number(alert.Id)} · {CustomerName(alert.CustomerId)}";

    private static string CasePath(int caseId) => $"{OpsBase}/aml/cases/{Number(caseId)}";

    private static string CaseTarget(FintechAmlCase amlCase) => $"Case {Number(amlCase.Id)} · {CustomerName(amlCase.CustomerId)}";

    /// <summary>Returns whether a role works in the operations console: analysts and senior reviewers.</summary>
    private static bool IsStaffRole(string? roleId) => roleId is FintechSeedData.OpsRole or FintechSeedData.SeniorRole;

    /// <summary>Compares two records by their JSON, for the AML records whose lists make field-by-field comparison long.</summary>
    private static bool SameJson<T>(T left, T right) => JsonSerializer.Serialize(left, snapshotOptions) == JsonSerializer.Serialize(right, snapshotOptions);

    private static string StaffNameOf(string? staffId) => FintechSeedData.StaffName(staffId) ?? staffId ?? FintechSeedData.SystemActor;

    private void AddEvent(FintechAmlCase amlCase, FintechAmlEventKind kind, string text, string? evidenceName = null) =>
        amlCase.Events.Add(new FintechAmlEvent
        {
            Id = nextAmlEventId++,
            At = Clock.Now,
            Actor = Identity.Current?.Name ?? FintechSeedData.SystemActor,
            Kind = kind,
            Text = text.Length <= MaxAmlSummary + 200 ? text : string.Concat(text.AsSpan(0, MaxAmlSummary + 199), "…"),
            EvidenceName = evidenceName,
        });

    /// <summary>
    /// The live monitoring rules, run as the demo customer confirms a payment to a payee: a large first payment
    /// (<see cref="HoldLargeAmount" /> or more) to a payee added in the last <see cref="NewPayeeDays" /> days, and structuring:
    /// a third payment of <see cref="StructuringFloor" /> to just under <see cref="HoldLargeAmount" /> to payees within 7 days.
    /// Each raises an alert in the queue and tells the staff; the customer is never told.
    /// </summary>
    private void CheckAmlRules(FintechTransfer transfer)
    {
        if (transfer.ToOwnAccount || FindPayee(transfer.ToId) is not { } payee || amlAlerts.Count >= MaxAmlAlerts)
            return;

        var first = transfers.All(other => other.Reference == transfer.Reference || other.ToId != transfer.ToId || other.CustomerId != transfer.CustomerId
            || other.Status is FintechTransferStatus.Cancelled or FintechTransferStatus.Rejected);

        if (first && transfer.Amount >= HoldLargeAmount && payee.AddedAt > Clock.Now.AddDays(-NewPayeeDays))
        {
            var days = Math.Max(0, (int)(Clock.Now - payee.AddedAt).TotalDays);
            RaiseAlert(transfer.CustomerId, FintechAmlRule.NewPayeeLargePayment, transfer.Amount >= 5000m ? 88 : 72,
                $"First payment of {Money(transfer.Amount)} to {payee.Name}, a payee added {(days == 0 ? "today" : days == 1 ? "1 day earlier" : $"{Number(days)} days earlier")}.",
                new List<string> { transfer.Reference });
        }

        if (transfer.Amount is >= StructuringFloor and < HoldLargeAmount)
        {
            var since = Clock.Now.AddDays(-7);
            var nearThreshold = transfers.Where(other => other.CustomerId == transfer.CustomerId && !other.ToOwnAccount && other.CreatedAt >= since
                    && other.Amount is >= StructuringFloor and < HoldLargeAmount && other.Status is not (FintechTransferStatus.Cancelled or FintechTransferStatus.Rejected))
                .OrderBy(other => other.CreatedAt).ToList();
            var alreadyRaised = amlAlerts.Any(alert => alert.CustomerId == transfer.CustomerId && alert.Rule == FintechAmlRule.Structuring && alert.CreatedAt >= since
                && alert.Status is FintechAmlAlertStatus.New or FintechAmlAlertStatus.InTriage);

            if (nearThreshold.Count >= 3 && !alreadyRaised)
            {
                RaiseAlert(transfer.CustomerId, FintechAmlRule.Structuring, Math.Min(95, 70 + nearThreshold.Count * 5),
                    $"{Number(nearThreshold.Count)} payments of {Money(StructuringFloor)} to just under {Money(HoldLargeAmount)} in 7 days, totalling {Money(nearThreshold.Sum(other => other.Amount))}.",
                    nearThreshold.Select(other => other.Reference).TakeLast(10).ToList());
            }
        }
    }

    /// <summary>Raises a new alert from the monitoring rules, audited as the system, and tells every staff member who can sign in.</summary>
    private void RaiseAlert(string customerId, FintechAmlRule rule, int score, string summary, List<string> references)
    {
        var alert = new FintechAmlAlert
        {
            Id = nextAmlAlertId++, CustomerId = customerId, Rule = rule, Score = score, Risk = FintechSeedData.RiskFor(score), CreatedAt = Clock.Now,
            Status = FintechAmlAlertStatus.New, Summary = summary.Length <= 300 ? summary : string.Concat(summary.AsSpan(0, 299), "…"), TransferReferences = references,
        };
        amlAlerts.Add(alert);
        RecordAs(FintechSeedData.SystemActor, "AML alert raised", AlertTarget(alert), AlertPath(alert.Id), $"{FintechSeedData.RuleName(rule)}, {alert.Risk} risk, score {Number(score)}.");
        NotifyOps($"New AML alert {Number(alert.Id)}", $"{FintechSeedData.RuleName(rule)} on {CustomerName(customerId)}, {alert.Risk} risk.", AlertPath(alert.Id), fromSystem: true);
    }

    /// <summary>Adds the AML cases (by number, or "Case 517") and alerts (by number, or "Alert 7012") to the operations search.</summary>
    private void SearchAml(string term, Func<string?[], int?> rank, List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)> matches)
    {
        if (!term.Any(char.IsAsciiDigit))
            return;

        foreach (var amlCase in amlCases)
        {
            if (rank(new string?[] { Number(amlCase.Id), $"Case {Number(amlCase.Id)}" }) is { } found)
            {
                matches.Add((found, 3, -amlCase.OpenedAt.Ticks, new ShowcaseSearchResult("AML case", $"Case {Number(amlCase.Id)}",
                    $"{CustomerName(amlCase.CustomerId)} · {CaseStatusWord(amlCase)} · {StaffNameOf(amlCase.AssigneeId)}", CasePath(amlCase.Id))));
            }
        }

        foreach (var alert in amlAlerts)
        {
            if (rank(new string?[] { Number(alert.Id), $"Alert {Number(alert.Id)}" }) is { } found)
            {
                matches.Add((found, 4, -alert.CreatedAt.Ticks, new ShowcaseSearchResult("AML alert", $"Alert {Number(alert.Id)}",
                    $"{CustomerName(alert.CustomerId)} · {FintechSeedData.RuleName(alert.Rule)} · {alert.Risk} risk", AlertPath(alert.Id))));
            }
        }
    }

    private static string CaseStatusWord(FintechAmlCase amlCase) => amlCase.Status switch
    {
        FintechAmlCaseStatus.Open => "open",
        FintechAmlCaseStatus.Investigating => "investigating",
        FintechAmlCaseStatus.Escalated => "escalated",
        _ => $"closed, {LowerFirst(FintechSeedData.DispositionName(amlCase.Disposition ?? FintechAmlDisposition.ClearedNoSuspicion))}",
    };

    /// <summary>
    /// Rebuilds the AML alerts and cases from the seed and a snapshot's changes and validates them: known customers, staff, and
    /// transfers; defined enum values; bounded text; consistent states (a dismissed alert has its reason, a linked alert its
    /// case; a closed case its disposition, summary, and, when reported, a filing reference; an escalated case a senior reviewer
    /// other than the person who escalated it); and timelines within their limits.
    /// </summary>
    private bool TryRebuildAml(FintechSnapshot snapshot, FintechSeedSet fresh, IReadOnlyList<FintechTransfer> storedTransfers, out List<FintechAmlAlert> alerts, out List<FintechAmlCase> cases)
    {
        alerts = new List<FintechAmlAlert>();
        cases = new List<FintechAmlCase>();

        if (snapshot.NextAmlAlertId is < 7001 or > 1_000_000 || snapshot.NextAmlCaseId is < 501 or > 1_000_000 || snapshot.NextAmlEventId is < 1 or > 10_000_000)
            return false;

        if (Overlay(fresh.AmlAlerts, snapshot.AmlAlerts, snapshot.RemovedAmlAlertIds, alert => alert.Id) is not { Count: <= MaxAmlAlerts } storedAlerts
            || Overlay(fresh.AmlCases, snapshot.AmlCases, snapshot.RemovedAmlCaseIds, amlCase => amlCase.Id) is not { Count: <= MaxAmlCases } storedCases)
            return false;

        var staff = FintechSeedData.Staff().ToDictionary(member => member.Id, StringComparer.Ordinal);
        var references = storedTransfers.Select(transfer => transfer.Reference).ToHashSet(StringComparer.Ordinal);
        var caseIds = storedCases.Where(amlCase => amlCase is not null).Select(amlCase => amlCase.Id).ToHashSet();

        if (storedAlerts.Any(alert => alert is null || alert.Id < 7001 || alert.Id >= snapshot.NextAmlAlertId || !IsCustomerId(alert.CustomerId) || !Enum.IsDefined(alert.Rule)
                || !Enum.IsDefined(alert.Risk) || !Enum.IsDefined(alert.Status) || alert.Score is < 0 or > 100 || !IsPlausible(alert.CreatedAt) || !IsText(alert.Summary, 300)
                || alert.TransferReferences is not { Count: <= 10 } || alert.TransferReferences.Any(reference => reference is null || !references.Contains(reference))
                || (alert.AssigneeId is not null && !staff.ContainsKey(alert.AssigneeId))
                || (alert.Status == FintechAmlAlertStatus.Dismissed) != alert.DismissReason is not null || (alert.DismissReason is not null && !IsText(alert.DismissReason, MaxAmlReason))
                || (alert.Status == FintechAmlAlertStatus.LinkedToCase) != (alert.CaseId is { } caseId && caseIds.Contains(caseId))
                || (alert.Status is FintechAmlAlertStatus.Dismissed or FintechAmlAlertStatus.LinkedToCase) != alert.HandledAt is not null
                || (alert.HandledAt is { } handled && !IsPlausible(handled)))
            || !Unique(storedAlerts.Select(alert => alert.Id)))
            return false;

        var alertsById = storedAlerts.ToDictionary(alert => alert.Id);

        bool ValidDisposition(FintechAmlDisposition? disposition, string? filing) =>
            disposition is null ? filing is null : Enum.IsDefined(disposition.Value) && (disposition == FintechAmlDisposition.Reported ? filing is not null && filingReference.IsMatch(filing) : filing is null);

        if (storedCases.Any(amlCase => amlCase is null || amlCase.Id < 501 || amlCase.Id >= snapshot.NextAmlCaseId || !IsCustomerId(amlCase.CustomerId)
                || amlCase.AlertIds is not { Count: >= 1 and <= 50 } || amlCase.AlertIds.Any(id => !alertsById.TryGetValue(id, out var alert) || alert.CaseId != amlCase.Id || alert.CustomerId != amlCase.CustomerId)
                || !staff.ContainsKey(amlCase.AssigneeId ?? string.Empty) || !Enum.IsDefined(amlCase.Status) || !Enum.IsDefined(amlCase.Priority) || !IsPlausible(amlCase.OpenedAt)
                || amlCase.DueOn < DateOnly.FromDateTime(FintechSeedData.Now.AddYears(-3)) || amlCase.DueOn > DateOnly.FromDateTime(FintechSeedData.Now.AddYears(2))
                || amlCase.Events is not { Count: >= 1 and <= MaxAmlEvents }
                || amlCase.Events.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAmlEventId || !Enum.IsDefined(entry.Kind) || !IsPlausible(entry.At)
                    || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength) || !IsText(entry.Text, MaxAmlSummary + 200)
                    || (entry.Kind == FintechAmlEventKind.Evidence) != entry.EvidenceName is not null || (entry.EvidenceName is not null && !IsText(entry.EvidenceName, MaxEvidenceName)))
                || (amlCase.Status == FintechAmlCaseStatus.Closed
                    ? amlCase.Disposition is null || amlCase.ClosedAt is not { } closed || !IsPlausible(closed) || !staff.ContainsKey(amlCase.ClosedById ?? string.Empty)
                        || amlCase.ClosingSummary is not { Length: >= MinAmlSummary and <= MaxAmlSummary } || !ValidDisposition(amlCase.Disposition, amlCase.FilingReference)
                        || amlCase.EscalatedById is not null || amlCase.ReviewerId is not null || amlCase.ProposedDisposition is not null
                    : amlCase.Disposition is not null || amlCase.ClosedAt is not null || amlCase.ClosedById is not null || amlCase.ClosingSummary is not null)
                || (amlCase.Status == FintechAmlCaseStatus.Escalated
                    ? amlCase.ReviewerId is not { } reviewer || !staff.TryGetValue(reviewer, out var member) || !member.IsSeniorReviewer
                        || amlCase.EscalatedById is not { } escalator || escalator == reviewer || !staff.ContainsKey(escalator) || !IsText(amlCase.EscalationReason, MaxAmlReason)
                        || (amlCase.ProposedDisposition is null ? amlCase.ProposedSummary is not null || amlCase.FilingReference is not null
                            : !ValidDisposition(amlCase.ProposedDisposition, amlCase.FilingReference) || amlCase.ProposedSummary is not { Length: >= MinAmlSummary and <= MaxAmlSummary })
                    : amlCase.ReviewerId is not null || amlCase.EscalatedById is not null || amlCase.EscalationReason is not null || amlCase.ProposedDisposition is not null
                        || amlCase.ProposedSummary is not null || (amlCase.Status != FintechAmlCaseStatus.Closed && amlCase.FilingReference is not null)))
            || !Unique(storedCases.Select(amlCase => amlCase.Id)) || !Unique(storedCases.SelectMany(amlCase => amlCase.Events).Select(entry => entry.Id)))
            return false;

        alerts = storedAlerts;
        cases = storedCases;

        return true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets every AML alert, newest first (operations only).</summary>
    public IReadOnlyList<FintechAmlAlert> AmlAlerts => IsOps ? amlAlerts.OrderByDescending(alert => alert.CreatedAt).ThenByDescending(alert => alert.Id).ToList() : Array.Empty<FintechAmlAlert>();

    /// <summary>Gets every AML case, newest first (operations only).</summary>
    public IReadOnlyList<FintechAmlCase> AmlCases => IsOps ? amlCases.OrderByDescending(amlCase => amlCase.OpenedAt).ThenByDescending(amlCase => amlCase.Id).ToList() : Array.Empty<FintechAmlCase>();

    /// <summary>Gets the signed-in staff member's ID (their demo account), or <see langword="null" /> for anyone else.</summary>
    public string? CurrentStaffId => IsOps && FintechSeedData.StaffName(Identity.Current!.Id) is not null ? Identity.Current.Id : null;

    #endregion
}
