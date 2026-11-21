using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Display and focus helpers shared by Lumora's screens, so a workspace, a person, or an amount looks the same everywhere.
/// </summary>
public static class SaasDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Lower-cases the first letter of a sentence fragment, so it reads inside a sentence while dates and names keep their capitals.</summary>
    public static string LowerFirst(string text) => text.Length == 0 ? text : string.Concat(char.ToLowerInvariant(text[0]).ToString(), text.AsSpan(1));

    /// <summary>Returns a stable avatar color for an ID.</summary>
    public static AvatarColor AvatarColorFor(int id) => (id % 5) switch
    {
        0 => AvatarColor.Primary,
        1 => AvatarColor.Success,
        2 => AvatarColor.Info,
        3 => AvatarColor.Warning,
        _ => AvatarColor.Danger,
    };

    /// <summary>Formats a date, such as "Nov 14, 2026".</summary>
    public static string Date(DateTime value) => value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Formats a date and time, such as "Nov 9, 9:05 AM".</summary>
    public static string DateTimeShort(DateTime value) => value.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);

    /// <summary>Formats an event count, such as "10B", "3.6M", "820k", or "940".</summary>
    public static string Events(long value) => value switch
    {
        >= 1_000_000_000 => (value / 1_000_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + "B",
        >= 1_000_000 => (value / 1_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + "M",
        >= 1_000 => (value / 1_000d).ToString("0.#", CultureInfo.InvariantCulture) + "k",
        _ => value.ToString(CultureInfo.InvariantCulture),
    };

    /// <summary>
    /// Moves focus to an element, ignoring one that has left the page (a row filtered out or paged away) or a closed circuit.
    /// Dialogs and menus opened from code don't get Bootstrap's focus return, so screens call this (WCAG 2.4.3).
    /// </summary>
    public static async Task FocusAsync(ElementReference? element)
    {
        if (element is not ElementReference target)
            return;

        try
        {
            await target.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Nothing left to focus.
        }
    }

    /// <summary>Returns when someone was last active, relative to the site's clock ("Never" for an open invitation).</summary>
    public static string LastSeen(DateTime? lastActiveOn, DateTime now)
    {
        if (lastActiveOn is not { } seen)
            return "Never";

        var days = (now.Date - seen.Date).Days;

        return days switch
        {
            <= 0 => $"Today, {seen.ToString("h:mm tt", CultureInfo.InvariantCulture)}",
            1 => "Yesterday",
            < 30 => $"{days} days ago",
            _ => Date(seen),
        };
    }

    /// <summary>Returns the badge color of a dunning stage: warning while retrying or in grace, danger once suspended.</summary>
    public static BadgeColor DunningColor(SaasDunningStage stage) => stage switch
    {
        SaasDunningStage.Retrying => BadgeColor.Warning,
        SaasDunningStage.Grace => BadgeColor.Warning,
        SaasDunningStage.Suspended => BadgeColor.Danger,
        _ => BadgeColor.Success,
    };

    /// <summary>Returns a dunning stage in words: "Retrying", "In grace", "Suspended", or "Recovered".</summary>
    public static string DunningStageName(SaasDunningStage stage) => stage == SaasDunningStage.Grace ? "In grace" : stage.ToString();

    /// <summary>Returns a failure reason as a label, such as "Card declined".</summary>
    public static string FailureName(SaasFailureReason reason) => reason switch
    {
        SaasFailureReason.ExpiredCard => "Expired card",
        SaasFailureReason.InsufficientFunds => "Insufficient funds",
        SaasFailureReason.NoCard => "No card on file",
        _ => "Card declined",
    };

    /// <summary>Returns the badge color of a sales lead's status.</summary>
    public static BadgeColor LeadColor(SaasLeadStatus status) => status switch
    {
        SaasLeadStatus.New => BadgeColor.Primary,
        SaasLeadStatus.Contacted => BadgeColor.Info,
        SaasLeadStatus.Qualified => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    /// <summary>
    /// Formats an amount in US dollars for prices and revenue: whole dollars, such as "$1,250", or with cents when it has any,
    /// such as "$49.50" (the plan editor allows cents).
    /// </summary>
    public static string Money(decimal value) => value.ToString(value == decimal.Truncate(value) ? "$#,0" : "$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Formats an amount with cents, such as "$1,249.50", for invoices and charges.</summary>
    public static string MoneyCents(decimal value) => value.ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Returns the badge color of a plan.</summary>
    public static BadgeColor PlanColor(string planId) => planId switch
    {
        "enterprise" => BadgeColor.Primary,
        "growth" => BadgeColor.Info,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns the progress color for a usage percentage: warning from 75%, danger from 90%.</summary>
    public static ProgressColor QuotaColor(double percent) => percent switch
    {
        >= 90 => ProgressColor.Danger,
        >= 75 => ProgressColor.Warning,
        _ => ProgressColor.Primary,
    };

    /// <summary>Returns the name of a member role, such as "Owner".</summary>
    public static string RoleName(SaasMemberRole role) => role.ToString();

    /// <summary>Returns what a member role may do, for role pickers.</summary>
    public static string RoleSummary(SaasMemberRole role) => role switch
    {
        SaasMemberRole.Owner => "Everything, including billing and other owners",
        SaasMemberRole.Admin => "Members and settings",
        SaasMemberRole.Billing => "Plan, payment method, and invoices",
        _ => "Uses dashboards and reports",
    };

    /// <summary>Returns the badge color of a workspace status.</summary>
    public static BadgeColor StatusColor(SaasWorkspaceStatus status) => status switch
    {
        SaasWorkspaceStatus.Active => BadgeColor.Success,
        SaasWorkspaceStatus.Trial => BadgeColor.Warning,
        SaasWorkspaceStatus.Archived => BadgeColor.Secondary,
        _ => BadgeColor.Danger,
    };

    /// <summary>Returns the name of a workspace status, such as "Past due".</summary>
    public static string StatusName(SaasWorkspaceStatus status) => status == SaasWorkspaceStatus.PastDue ? "Past due" : status.ToString();

    #endregion

    #endregion
}
