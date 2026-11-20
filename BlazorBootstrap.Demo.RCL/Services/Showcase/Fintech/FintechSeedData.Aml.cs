using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>
/// The anti-money-laundering (AML) part of the seed: the bank's AML staff, about 100 alerts the monitoring rules raised over
/// the last 90 days (most dismissed as false positives or linked to cases, the newest still waiting), and 22 cases in every
/// state with their timelines: closed ones with dispositions (two reported with a SAR), investigations (two overdue), one
/// escalated case waiting for the senior reviewer, one proposing a report and waiting for approval, and one returned to the
/// analyst. Everything is generated from fixed patterns relative to the site's "today".
/// </summary>
public static partial class FintechSeedData
{
    #region Fields and Constants

    /// <summary>The role of a senior reviewer: bank operations who also decide escalated AML cases (four-eyes rule).</summary>
    public const string SeniorRole = "senior";

    /// <summary>The demo senior reviewer's account ID.</summary>
    public const string SeniorAccountId = "mei";

    /// <summary>The demo senior reviewer's name.</summary>
    public const string SeniorName = "Mei Lin";

    /// <summary>How many days an AML case's investigation may take before it is overdue.</summary>
    public const int AmlCaseDays = 30;

    private static readonly string[] amlNotes =
    {
        "Pulled 12 months of statements. The pattern starts in the last quarter; nothing like it before.",
        "Called the customer. They say the payments are for a family member's tuition abroad; asked for the invoices.",
        "Payee account is at a bank on our high-risk list. Checked the name against the sanctions lists: no match.",
        "Customer sent the sale contract for the house; the incoming amount matches the completion statement.",
        "Several payees share an address with the customer. Checking whether they are related businesses.",
        "Amounts stay just under the check threshold on consecutive days, which looks deliberate.",
        "No answer from the customer after two calls and an email. Card activity carries on as usual.",
        "Employer confirmed the bonus payment; the outbound transfers went to the customer's own brokerage account.",
    };

    private static readonly string[] dismissReasons =
    {
        "Regular rent payment to a long-standing landlord; matches the lease on file.",
        "Payroll arrived and went to the customer's own savings, as every month.",
        "Payee is the customer's own account at another bank, confirmed by name check.",
        "One-off purchase explained by the customer (a used car); invoice on file.",
        "Seasonal business income for a small business customer, in line with last year.",
        "Duplicate of an alert already reviewed this month.",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The AML staff, analysts first: two of them can sign in to the demo (an analyst and a senior reviewer).</summary>
    public static IReadOnlyList<FintechStaffMember> Staff() => new List<FintechStaffMember>
    {
        new(OpsAccountId, OpsName, false),
        new("daniel", "Daniel Okoro", false),
        new("samuel", "Samuel Ortiz", false),
        new(SeniorAccountId, SeniorName, true),
        new("helen", "Helen Ward", true),
    };

    /// <summary>Returns a staff member's name, or <see langword="null" /> for an unknown ID.</summary>
    public static string? StaffName(string? staffId) => Staff().FirstOrDefault(member => member.Id == staffId)?.Name;

    #endregion

    #region Private Methods

    /// <summary>Adds the audit entries of the seeded AML work: alerts raised and dismissed, cases opened, escalated, returned, and closed.</summary>
    private static void AmlAudit(List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)> entries, IReadOnlyList<FintechAmlAlert> alerts,
        IReadOnlyList<FintechAmlCase> cases)
    {
        foreach (var alert in alerts)
        {
            var target = $"Alert {Number(alert.Id)} · {NameOf(alert.CustomerId)}";
            var url = $"{OpsBase}/aml/alerts/{Number(alert.Id)}";
            entries.Add((alert.CreatedAt, SystemActor, "AML alert raised", target, url, $"{RuleName(alert.Rule)}, {alert.Risk} risk, score {Number(alert.Score)}."));

            if (alert.Status == FintechAmlAlertStatus.Dismissed)
                entries.Add((alert.HandledAt!.Value, StaffName(alert.AssigneeId) ?? OpsName, "AML alert dismissed", target, url, $"False positive: {alert.DismissReason}"));
        }

        foreach (var amlCase in cases)
        {
            var target = $"Case {Number(amlCase.Id)} · {NameOf(amlCase.CustomerId)}";
            var url = $"{OpsBase}/aml/cases/{Number(amlCase.Id)}";

            foreach (var entry in amlCase.Events)
            {
                var action = entry.Kind switch
                {
                    FintechAmlEventKind.Opened => "AML case opened",
                    FintechAmlEventKind.Investigating => "AML investigation started",
                    FintechAmlEventKind.Note => "AML note added",
                    FintechAmlEventKind.Evidence => "AML evidence added",
                    FintechAmlEventKind.Assigned => "AML case assigned",
                    FintechAmlEventKind.Escalated => "AML case escalated",
                    FintechAmlEventKind.Returned => "AML case returned",
                    _ => "AML case closed",
                };
                var details = entry.Kind switch
                {
                    FintechAmlEventKind.Note => $"A note of {Number(entry.Text.Length)} characters.",
                    FintechAmlEventKind.Evidence => $"\"{entry.EvidenceName}\".",
                    FintechAmlEventKind.Opened or FintechAmlEventKind.Investigating => null,
                    _ => entry.Text,
                };
                entries.Add((entry.At, entry.Actor, action, target, url, details));
            }
        }
    }

    /// <summary>
    /// Builds the AML alerts and cases. Each case spec says how long ago it was opened, how many alerts it came from, where it
    /// is, and who works it; the rest of the alerts are dismissed, in triage, or new.
    /// </summary>
    private static (List<FintechAmlAlert> Alerts, List<FintechAmlCase> Cases) Aml(IReadOnlyList<FintechTransfer> transfers)
    {
        var others = Customers().Where(customer => customer.Id != SeededCustomerId).ToList();
        var watched = others.Where(customer => customer.Status == FintechCustomerStatus.Watch).ToList();

        // Opened days ago, alerts, status, analyst, disposition, closed by, closed days after opening, and the special story, if any.
        var specs = new (double Days, int Alerts, FintechAmlCaseStatus Status, string Analyst, FintechAmlDisposition? Disposition, string? Closer, int CloseAfter, string Story)[]
        {
            (85, 2, FintechAmlCaseStatus.Closed, "daniel", FintechAmlDisposition.Reported, SeniorAccountId, 15, "reported"),
            (80, 1, FintechAmlCaseStatus.Closed, OpsAccountId, FintechAmlDisposition.ClearedExplained, OpsAccountId, 6, string.Empty),
            (76, 1, FintechAmlCaseStatus.Closed, "samuel", FintechAmlDisposition.ClearedNoSuspicion, "samuel", 4, string.Empty),
            (71, 3, FintechAmlCaseStatus.Closed, "daniel", FintechAmlDisposition.CustomerExited, SeniorAccountId, 20, "exited"),
            (66, 1, FintechAmlCaseStatus.Closed, OpsAccountId, FintechAmlDisposition.ClearedExplained, OpsAccountId, 9, string.Empty),
            (60, 2, FintechAmlCaseStatus.Closed, "samuel", FintechAmlDisposition.ClearedNoSuspicion, "samuel", 5, string.Empty),
            (55, 1, FintechAmlCaseStatus.Closed, OpsAccountId, FintechAmlDisposition.Reported, SeniorAccountId, 18, "reported"),
            (50, 1, FintechAmlCaseStatus.Closed, "daniel", FintechAmlDisposition.ClearedExplained, "daniel", 7, string.Empty),
            (46, 2, FintechAmlCaseStatus.Investigating, "samuel", null, null, 0, string.Empty),
            (41, 1, FintechAmlCaseStatus.Closed, OpsAccountId, FintechAmlDisposition.ClearedNoSuspicion, OpsAccountId, 3, string.Empty),
            (37, 1, FintechAmlCaseStatus.Investigating, "daniel", null, null, 0, string.Empty),
            (33, 2, FintechAmlCaseStatus.Closed, "samuel", FintechAmlDisposition.ClearedExplained, "samuel", 8, string.Empty),
            (29, 1, FintechAmlCaseStatus.Escalated, "daniel", null, null, 0, "escalated"),
            (25, 1, FintechAmlCaseStatus.Closed, "daniel", FintechAmlDisposition.ClearedNoSuspicion, "daniel", 4, string.Empty),
            (21, 2, FintechAmlCaseStatus.Investigating, OpsAccountId, null, null, 0, "returned"),
            (17, 1, FintechAmlCaseStatus.Closed, OpsAccountId, FintechAmlDisposition.ClearedExplained, OpsAccountId, 5, string.Empty),
            (14, 2, FintechAmlCaseStatus.Escalated, OpsAccountId, null, null, 0, "proposed"),
            (10, 1, FintechAmlCaseStatus.Investigating, "samuel", null, null, 0, string.Empty),
            (7, 1, FintechAmlCaseStatus.Open, OpsAccountId, null, null, 0, string.Empty),
            (5, 1, FintechAmlCaseStatus.Investigating, "daniel", null, null, 0, string.Empty),
            (3, 2, FintechAmlCaseStatus.Open, "samuel", null, null, 0, string.Empty),
            (1, 1, FintechAmlCaseStatus.Open, OpsAccountId, null, null, 0, string.Empty),
        };

        // Alerts are made first without numbers, then numbered oldest first.
        var drafts = new List<(DateTime At, FintechCustomer Customer, FintechAmlRule Rule, int Score, FintechAmlAlertStatus Status, string? Assignee, string? Reason, DateTime? Handled, int CaseIndex)>();

        for (var index = 0; index < specs.Length; index++)
        {
            var spec = specs[index];
            var customer = index % 3 == 0 && watched.Count > 0 ? watched[index / 3 % watched.Count] : others[Pick(index, 101, others.Count)];
            var opened = Now.AddDays(-spec.Days).AddHours(-Pick(index, 102, 6));

            for (var alert = 0; alert < spec.Alerts; alert++)
            {
                var at = opened.AddDays(-(alert * 2 + 1)).AddHours(-Pick(index * 7 + alert, 103, 10));
                drafts.Add((at, customer, (FintechAmlRule)Pick(index * 7 + alert, 104, 5), 62 + Pick(index * 7 + alert, 105, 38), FintechAmlAlertStatus.LinkedToCase, spec.Analyst, null, opened, index));
            }
        }

        // The free alerts: false positives dismissed with a reason, some in triage, and the newest still new.
        for (var index = 0; index < 75; index++)
        {
            var age = 89 - index * 1.18;
            var at = Now.AddDays(-age).AddHours(-Pick(index, 111, 12));
            var customer = Pick(index, 112, 5) == 0 && watched.Count > 0 ? watched[Pick(index, 113, watched.Count)] : others[Pick(index, 114, others.Count)];
            var roll = Pick(index, 115, 10);
            var analyst = Staff()[Pick(index, 116, 3)].Id;
            var status = age switch
            {
                < 6 => roll < 7 ? FintechAmlAlertStatus.New : FintechAmlAlertStatus.InTriage,
                < 14 => roll < 3 ? FintechAmlAlertStatus.New : roll < 6 ? FintechAmlAlertStatus.InTriage : FintechAmlAlertStatus.Dismissed,
                _ => roll < 1 ? FintechAmlAlertStatus.InTriage : FintechAmlAlertStatus.Dismissed,
            };

            drafts.Add((at, customer, (FintechAmlRule)Pick(index, 117, 5), 30 + Pick(index, 118, 60), status, status == FintechAmlAlertStatus.New ? null : analyst,
                status == FintechAmlAlertStatus.Dismissed ? dismissReasons[Pick(index, 119, dismissReasons.Length)] : null,
                status == FintechAmlAlertStatus.Dismissed ? at.AddHours(6 + Pick(index, 120, 40)) : null, -1));
        }

        // The demo customer's large first payment to a new payee (TM-260412, held for a check) tripped the new-payee rule.
        var held = transfers.First(transfer => transfer.Reference == "TM-260412");
        drafts.Add((held.CreatedAt.AddSeconds(2), Customers()[0], FintechAmlRule.NewPayeeLargePayment, 74, FintechAmlAlertStatus.New, null, null, null, -1));

        var alerts = new List<FintechAmlAlert>();
        var caseAlerts = specs.Select(_ => new List<int>()).ToArray();
        var ordered = drafts.OrderBy(draft => draft.At).ToList();

        for (var index = 0; index < ordered.Count; index++)
        {
            var draft = ordered[index];
            var id = 7001 + index;
            var references = draft.Customer.Id == SeededCustomerId
                ? new List<string> { held.Reference }
                : transfers.Where(transfer => transfer.CustomerId == draft.Customer.Id && transfer.CreatedAt <= draft.At).OrderByDescending(transfer => transfer.CreatedAt)
                    .Take(2).Select(transfer => transfer.Reference).ToList();

            alerts.Add(new FintechAmlAlert
            {
                Id = id, CustomerId = draft.Customer.Id, Rule = draft.Rule, Score = draft.Score, Risk = RiskFor(draft.Score), CreatedAt = draft.At, Status = draft.Status,
                AssigneeId = draft.Assignee, DismissReason = draft.Reason, HandledAt = draft.Handled,
                Summary = draft.Customer.Id == SeededCustomerId
                    ? $"First payment of {Money(held.Amount)} to {held.ToName}, a payee added 2 days earlier."
                    : AlertSummary(draft.Rule, id),
                TransferReferences = references,
            });

            if (draft.CaseIndex >= 0)
                caseAlerts[draft.CaseIndex].Add(id);
        }

        var cases = new List<FintechAmlCase>();
        var eventId = 1;

        for (var index = 0; index < specs.Length; index++)
        {
            var spec = specs[index];
            var id = 501 + index;
            var linked = alerts.Where(alert => caseAlerts[index].Contains(alert.Id)).ToList();
            var opened = linked.Max(alert => alert.HandledAt!.Value);
            var analyst = StaffName(spec.Analyst)!;
            var events = new List<FintechAmlEvent>();

            FintechAmlEvent Event(DateTime at, string actor, FintechAmlEventKind kind, string text, string? evidence = null) =>
                new() { Id = eventId++, At = at, Actor = actor, Kind = kind, Text = text, EvidenceName = evidence };

            foreach (var alert in linked)
                alert.CaseId = id;

            events.Add(Event(opened, analyst, FintechAmlEventKind.Opened, $"Opened from {(linked.Count == 1 ? "1 alert" : $"{Number(linked.Count)} alerts")}."));

            if (spec.Status != FintechAmlCaseStatus.Open)
            {
                events.Add(Event(opened.AddHours(3), analyst, FintechAmlEventKind.Investigating, "Investigation started."));
                events.Add(Event(opened.AddDays(1).AddHours(2), analyst, FintechAmlEventKind.Note, amlNotes[Pick(index, 121, amlNotes.Length)]));
                events.Add(Event(opened.AddDays(1).AddHours(4), analyst, FintechAmlEventKind.Evidence, "Statements for the last 12 months, exported from the core banking system.", "Account statements, 12 months"));

                if (index % 2 == 0)
                    events.Add(Event(opened.AddDays(2).AddHours(1), analyst, FintechAmlEventKind.Note, amlNotes[Pick(index, 122, amlNotes.Length)]));
            }

            var amlCase = new FintechAmlCase
            {
                Id = id, CustomerId = linked[0].CustomerId, AlertIds = linked.Select(alert => alert.Id).ToList(), AssigneeId = spec.Analyst, OpenedAt = opened,
                Priority = linked.Max(alert => alert.Risk), DueOn = DateOnly.FromDateTime(opened.AddDays(AmlCaseDays)), Status = spec.Status, Events = events,
            };

            switch (spec.Story)
            {
                case "escalated":
                    amlCase.EscalatedById = spec.Analyst;
                    amlCase.ReviewerId = SeniorAccountId;
                    amlCase.EscalationReason = "Customer won't explain the source of the funds, and the payee bank is on the high-risk list. Needs a decision on reporting.";
                    events.Add(Event(opened.AddDays(9), analyst, FintechAmlEventKind.Escalated, $"To {SeniorName}: {amlCase.EscalationReason}"));
                    break;

                case "proposed":
                    amlCase.EscalatedById = spec.Analyst;
                    amlCase.ReviewerId = SeniorAccountId;
                    amlCase.EscalationReason = "Proposing a SAR: structuring pattern confirmed over three weeks, no business reason given.";
                    amlCase.ProposedDisposition = FintechAmlDisposition.Reported;
                    amlCase.FilingReference = "SAR-2026-00431";
                    amlCase.ProposedSummary = "Eleven payments just under the check threshold to three payees in 20 days, with no explanation from the customer. A suspicious activity report is drafted.";
                    events.Add(Event(Now.AddHours(-20), analyst, FintechAmlEventKind.Escalated, $"To {SeniorName}, proposing Reported (SAR-2026-00431): {amlCase.EscalationReason}"));
                    break;

                case "returned":
                    events.Add(Event(Now.AddDays(-6), analyst, FintechAmlEventKind.Escalated, $"To {SeniorName}: activity looks like layering through two new payees."));
                    events.Add(Event(Now.AddDays(-3), SeniorName, FintechAmlEventKind.Returned, "Get the customer's explanation and the payees' details first, then escalate again if it's still unexplained."));
                    break;
            }

            if (spec.Status == FintechAmlCaseStatus.Closed)
            {
                var closedAt = opened.AddDays(spec.CloseAfter).AddHours(2);
                var closer = StaffName(spec.Closer)!;

                if (spec.Closer == SeniorAccountId)
                    events.Add(Event(closedAt.AddDays(-2), analyst, FintechAmlEventKind.Escalated, $"To {SeniorName}: {(spec.Disposition == FintechAmlDisposition.Reported ? "proposing a SAR." : "proposing to exit the customer.")}"));

                amlCase.Disposition = spec.Disposition;
                amlCase.ClosedAt = closedAt;
                amlCase.ClosedById = spec.Closer;
                amlCase.FilingReference = spec.Disposition == FintechAmlDisposition.Reported ? $"SAR-2026-{Number(380 + index * 3).PadLeft(5, '0')}" : null;
                amlCase.ClosingSummary = spec.Disposition switch
                {
                    FintechAmlDisposition.Reported => "Unexplained third-party funds moved on within a day, repeatedly. A suspicious activity report was filed.",
                    FintechAmlDisposition.CustomerExited => "The customer refused to explain repeated cash-like movements. The relationship is ended with notice.",
                    FintechAmlDisposition.ClearedExplained => "The customer documented the source of the funds (a property sale and a bonus). Nothing further to do.",
                    _ => "The activity fits the customer's usual pattern and stated income. No suspicion.",
                };
                events.Add(Event(closedAt, closer, FintechAmlEventKind.Closed,
                    $"{DispositionName(spec.Disposition!.Value)}{(amlCase.FilingReference is { } filing ? $" ({filing})" : string.Empty)}: {amlCase.ClosingSummary}"));
            }

            cases.Add(amlCase);
        }

        return (alerts, cases);
    }

    private static string AlertSummary(FintechAmlRule rule, int seed) => rule switch
    {
        FintechAmlRule.Structuring => $"{Number(3 + Pick(seed, 131, 4))} payments of {Money(1800m + Pick(seed, 132, 19_000) / 100m)} or so in {Number(3 + Pick(seed, 133, 5))} days, each just under the $2,000.00 check threshold.",
        FintechAmlRule.RapidMovement => $"{Money(8000m + Pick(seed, 134, 4_000_000) / 100m)} came in and {Number(80 + Pick(seed, 135, 19))}% of it went out again within {Number(4 + Pick(seed, 136, 20))} hours.",
        FintechAmlRule.NewPayeeLargePayment => $"First payment of {Money(2000m + Pick(seed, 137, 1_500_000) / 100m)} to a payee added {Number(1 + Pick(seed, 138, 6))} days earlier.",
        FintechAmlRule.HighRiskJurisdiction => $"{Money(900m + Pick(seed, 139, 2_000_000) / 100m)} sent to a bank on the high-risk jurisdiction list.",
        _ => $"{Money(3000m + Pick(seed, 140, 2_500_000) / 100m)} out in 7 days, {Number(3 + Pick(seed, 141, 7))} times the customer's 90-day weekly average.",
    };

    /// <summary>Returns a disposition in words.</summary>
    internal static string DispositionName(FintechAmlDisposition disposition) => disposition switch
    {
        FintechAmlDisposition.ClearedNoSuspicion => "Cleared, no suspicion",
        FintechAmlDisposition.ClearedExplained => "Cleared, explained",
        FintechAmlDisposition.Reported => "Reported (SAR filed)",
        _ => "Customer exited",
    };

    /// <summary>Returns the risk of a rule score: 80 and up is high, 55 and up medium.</summary>
    internal static FintechAmlRisk RiskFor(int score) => score >= 80 ? FintechAmlRisk.High : score >= 55 ? FintechAmlRisk.Medium : FintechAmlRisk.Low;

    /// <summary>Returns a rule's name in words.</summary>
    internal static string RuleName(FintechAmlRule rule) => rule switch
    {
        FintechAmlRule.Structuring => "Structuring",
        FintechAmlRule.RapidMovement => "Rapid movement of funds",
        FintechAmlRule.NewPayeeLargePayment => "New payee, large first payment",
        FintechAmlRule.HighRiskJurisdiction => "High-risk jurisdiction",
        _ => "Velocity spike",
    };

    #endregion

    #endregion
}
