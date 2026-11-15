namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;

#region Public site content

/// <summary>A product capability shown on the public site.</summary>
public sealed record SaasFeature(string Id, string Title, string Summary, string Details, IconName IconName, string Category);

/// <summary>One row of the plan comparison table.</summary>
public sealed record SaasComparisonRow(string Feature, string Starter, string Growth, string Enterprise);

/// <summary>A customer quote.</summary>
public sealed record SaasTestimonial(string Quote, string Person, string Role, string Company, int Rating, string Metric, string MetricLabel);

/// <summary>A short customer story.</summary>
public sealed record SaasCaseStudy(string Company, string Industry, string Headline, string Result, IconName IconName);

/// <summary>A frequently asked question.</summary>
public sealed record SaasFaq(string Id, string Category, string Question, string Answer);

#endregion

#region Plans and billing

/// <summary>How often a workspace is billed.</summary>
public enum SaasBillingPeriod
{
    /// <summary>Billed every month at the list price.</summary>
    Monthly,

    /// <summary>Billed once a year at the annual discount.</summary>
    Annual,
}

/// <summary>
/// A subscription plan. The public pricing page, trial sign-up, and billing all read the plans from state, so an operator's
/// change shows everywhere. A limit of 0 means unlimited.
/// </summary>
public sealed record SaasPlan(
    string Id,
    string Name,
    string Description,
    decimal MonthlyPricePerSeat,
    bool IsPopular,
    IReadOnlyList<string> Highlights,
    string CallToAction,
    int SeatLimit,
    long EventLimit,
    int DashboardLimit,
    bool IsSelfServe);

/// <summary>Status of an invoice.</summary>
public enum SaasInvoiceStatus
{
    /// <summary>Charged to the card on file.</summary>
    Paid,

    /// <summary>Issued and not yet paid.</summary>
    Open,

    /// <summary>Paid and then refunded in full or in part.</summary>
    Refunded,
}

/// <summary>An invoice of a workspace. Only the card brand and last four digits are kept (CR-001-AC-11).</summary>
public sealed record SaasInvoice(
    string Number,
    int WorkspaceId,
    DateTime IssuedOn,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string Description,
    decimal Amount,
    SaasInvoiceStatus Status,
    string? CardBrand,
    string? CardLast4,
    decimal RefundedAmount);

/// <summary>What a plan change costs today and what it leaves.</summary>
/// <param name="ChargeToday">The amount charged now to the card on file, after the credit balance is used (0 when nothing is due).</param>
/// <param name="CreditAfter">The workspace's credit balance after the change: unused time of a downgrade is credited here.</param>
/// <param name="NextAmount">The full amount of the next renewal at the new plan, period, and seats.</param>
/// <param name="NextRenewal">When the next renewal is due.</param>
public sealed record SaasPlanQuote(decimal ChargeToday, decimal CreditAfter, decimal NextAmount, DateTime NextRenewal);

/// <summary>The result of a billing or team action.</summary>
public enum SaasResult
{
    /// <summary>The change was applied.</summary>
    Ok,

    /// <summary>The signed-in account may not do this in the workspace.</summary>
    NotAllowed,

    /// <summary>The plan, member, or invoice is unknown, or the plan is only sold through sales.</summary>
    NotFound,

    /// <summary>Fewer seats than the workspace's members and invitations, or more than the plan allows.</summary>
    SeatsOutOfRange,

    /// <summary>No free seat for an invitation.</summary>
    NoFreeSeat,

    /// <summary>Someone with that email is already a member or invited.</summary>
    AlreadyMember,

    /// <summary>A paid plan needs a card on file.</summary>
    CardRequired,

    /// <summary>The card was declined or failed (the test cards that fail).</summary>
    CardDeclined,

    /// <summary>The input is out of range (text too long, a bad email).</summary>
    Invalid,

    /// <summary>Nothing changes.</summary>
    NoChange,
}

#endregion

#region Workspaces and teams

/// <summary>Lifecycle of a customer workspace (a tenant).</summary>
public enum SaasWorkspaceStatus
{
    /// <summary>Inside its free trial.</summary>
    Trial,

    /// <summary>On a free or paid plan in good standing.</summary>
    Active,

    /// <summary>A renewal payment failed.</summary>
    PastDue,

    /// <summary>Suspended by the operator; members can't use it.</summary>
    Suspended,

    /// <summary>Closed and kept for the record.</summary>
    Archived,
}

/// <summary>Workspace preferences edited on the settings screen.</summary>
public sealed record SaasWorkspaceSettings(
    string TimeZone,
    string WeekStartsOn,
    string DataRegion,
    bool WeeklyDigest,
    bool SecurityAlerts,
    bool BillingEmails,
    bool ProductUpdates);

/// <summary>
/// A customer workspace (a tenant): the company's subscription, usage, payment method, and settings. Only the card brand,
/// last four digits, and expiry are kept (CR-001-AC-11).
/// </summary>
public sealed record SaasWorkspace(
    int Id,
    string Name,
    string PlanId,
    SaasBillingPeriod Period,
    int Seats,
    SaasWorkspaceStatus Status,
    DateTime CreatedOn,
    DateTime? TrialEndsOn,
    DateTime RenewsOn,
    string Country,
    long EventsThisMonth,
    int Dashboards,
    decimal CreditBalance,
    string? CardBrand,
    string? CardLast4,
    int CardExpiryMonth,
    int CardExpiryYear,
    SaasWorkspaceSettings Settings);

/// <summary>A member's role in a workspace.</summary>
public enum SaasMemberRole
{
    /// <summary>Created the workspace; manages everything.</summary>
    Owner,

    /// <summary>Manages members and settings.</summary>
    Admin,

    /// <summary>Manages billing only.</summary>
    Billing,

    /// <summary>Uses the product.</summary>
    Member,
}

/// <summary>Whether a member has joined.</summary>
public enum SaasMemberStatus
{
    /// <summary>Joined and holds a seat.</summary>
    Active,

    /// <summary>Invited and holds a seat until the invitation is accepted or cancelled.</summary>
    Invited,
}

/// <summary>
/// A person in a workspace. <see cref="AccountId" /> links a member who can sign in to this demo (the seeded workspace
/// owner, or someone who started a trial); seeded teammates and invitations have none.
/// </summary>
public sealed record SaasMember(
    int Id,
    int WorkspaceId,
    string? AccountId,
    string Name,
    string Email,
    SaasMemberRole Role,
    SaasMemberStatus Status,
    DateTime AddedOn,
    DateTime? LastActiveOn);

/// <summary>One entry in a workspace's activity history.</summary>
public sealed record SaasActivity(int WorkspaceId, DateTime At, string Title, string Detail, IconName IconName);

/// <summary>A notification for one signed-in account.</summary>
public sealed record SaasNotice(int Id, string AccountId, string Title, string Text, string? Link, IconName IconName, DateTime CreatedOn, bool IsRead);

#endregion

#region Sales

/// <summary>Where a sales lead stands.</summary>
public enum SaasLeadStatus
{
    /// <summary>Just arrived from the contact form.</summary>
    New,

    /// <summary>Someone from sales replied.</summary>
    Contacted,

    /// <summary>A real opportunity.</summary>
    Qualified,

    /// <summary>Won, lost, or not a fit.</summary>
    Closed,
}

/// <summary>A message sent from the public "Contact sales" form.</summary>
public sealed record SaasLead(int Id, string Name, string Email, string Company, string TeamSize, string Message, DateTime CreatedOn, SaasLeadStatus Status);

#endregion

#region Analytics

/// <summary>Period shown on an analytics screen.</summary>
public enum SaasAnalyticsRange
{
    /// <summary>The last 7 days, one point per day.</summary>
    Last7Days,

    /// <summary>The last 30 days, one point per day.</summary>
    Last30Days,

    /// <summary>The last 90 days, one point per week.</summary>
    Last90Days,

    /// <summary>The last 12 months, one point per month.</summary>
    Last12Months,
}

/// <summary>One point of a metric over time.</summary>
public sealed record SaasSeriesPoint(string Label, double Value);

/// <summary>A traffic source and its share of sessions.</summary>
public sealed record SaasTrafficSource(string Name, double Sessions);

/// <summary>A page and its traffic for the selected period.</summary>
public sealed record SaasTopPage(string Path, int Views, double ConversionPercent);

/// <summary>Everything an analytics screen shows for one period.</summary>
public sealed record SaasAnalyticsSnapshot(
    IReadOnlyList<SaasSeriesPoint> Sessions,
    IReadOnlyList<SaasSeriesPoint> Signups,
    IReadOnlyList<SaasTrafficSource> Sources,
    IReadOnlyList<SaasTopPage> TopPages,
    double BounceRatePercent,
    double AverageSessionMinutes);

#endregion
