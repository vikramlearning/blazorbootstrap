namespace BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;

/// <summary>Lifecycle state of a customer account.</summary>
public enum SaasAccountStatus
{
    /// <summary>A paying or free account in good standing.</summary>
    Active,

    /// <summary>An account inside its free trial.</summary>
    Trial,

    /// <summary>An account an administrator has suspended.</summary>
    Suspended,
}

/// <summary>Role of the account's primary contact.</summary>
public enum SaasAccountRole
{
    /// <summary>The person who created the workspace.</summary>
    Owner,

    /// <summary>A person who manages members and settings.</summary>
    Admin,

    /// <summary>A person who manages invoices only.</summary>
    Billing,

    /// <summary>A regular member.</summary>
    Member,
}

/// <summary>Period shown on the analytics screen.</summary>
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

/// <summary>A subscription plan.</summary>
public sealed record SaasPlan(string Id, string Name, decimal PricePerSeat, int ApiQuota, int StorageQuotaGb);

/// <summary>A customer account managed from the dashboard.</summary>
public sealed record SaasAccount(
    int Id,
    string Name,
    string Email,
    string Company,
    SaasAccountRole Role,
    string PlanId,
    int Seats,
    SaasAccountStatus Status,
    string Country,
    DateTime JoinedOn,
    DateTime LastSeenOn,
    int ApiUsagePercent,
    int StorageUsagePercent);

/// <summary>One entry in an account's activity history.</summary>
public sealed record SaasActivity(int AccountId, DateTime At, string Title, string Detail, IconName IconName);

/// <summary>A notification shown in the header.</summary>
public sealed record SaasNotification(int Id, string Title, string Detail, IconName IconName, DateTime At, bool IsRead);

/// <summary>An invoice listed on the billing tab.</summary>
public sealed record SaasInvoice(string Number, DateTime IssuedOn, decimal Amount, bool IsPaid);

/// <summary>Workspace preferences edited on the settings screen.</summary>
public sealed record SaasWorkspaceSettings(
    string WorkspaceName,
    string SupportEmail,
    string TimeZone,
    string WeekStartsOn,
    bool WeeklyDigest,
    bool SecurityAlerts,
    bool BillingEmails,
    bool ProductUpdates);

/// <summary>One point of a metric over time.</summary>
public sealed record SaasSeriesPoint(string Label, double Value);

/// <summary>A traffic source and its share of sessions.</summary>
public sealed record SaasTrafficSource(string Name, double Sessions);

/// <summary>A page and its traffic for the selected period.</summary>
public sealed record SaasTopPage(string Path, int Views, double ConversionPercent);

/// <summary>Everything the analytics screen shows for one period.</summary>
public sealed record SaasAnalyticsSnapshot(
    IReadOnlyList<SaasSeriesPoint> Sessions,
    IReadOnlyList<SaasSeriesPoint> Signups,
    IReadOnlyList<SaasTrafficSource> Sources,
    IReadOnlyList<SaasTopPage> TopPages,
    double BounceRatePercent,
    double AverageSessionMinutes);
