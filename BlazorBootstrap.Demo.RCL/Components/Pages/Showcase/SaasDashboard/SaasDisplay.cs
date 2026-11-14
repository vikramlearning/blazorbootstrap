using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// Display and focus helpers shared by the Pulseboard screens, so an account looks the same everywhere it appears.
/// </summary>
public static class SaasDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Returns a stable avatar color for an account.</summary>
    public static AvatarColor AvatarColorFor(SaasAccount account) => (account.Id % 5) switch
    {
        0 => AvatarColor.Primary,
        1 => AvatarColor.Success,
        2 => AvatarColor.Info,
        3 => AvatarColor.Warning,
        _ => AvatarColor.Danger,
    };

    /// <summary>Returns when an account was last seen, relative to the site's fixed "today".</summary>
    public static string LastSeen(SaasAccount account)
    {
        var days = (SaasDashboardSeedData.Today.Date - account.LastSeenOn.Date).Days;

        return days switch
        {
            <= 0 => $"Today, {account.LastSeenOn.ToString("h:mm tt", CultureInfo.InvariantCulture)}",
            1 => "Yesterday",
            < 30 => $"{days} days ago",
            _ => account.LastSeenOn.ToString("MMM d, yyyy", CultureInfo.InvariantCulture),
        };
    }

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

    /// <summary>Formats an amount as whole US dollars.</summary>
    public static string Money(decimal value) => value.ToString("$#,0", CultureInfo.InvariantCulture);

    /// <summary>Returns the badge color of a plan.</summary>
    public static BadgeColor PlanColor(string planId) => planId switch
    {
        "business" => BadgeColor.Primary,
        "pro" => BadgeColor.Info,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns the progress color for a usage percentage: warning from 75%, danger from 90%.</summary>
    public static ProgressColor QuotaColor(double percent) => percent switch
    {
        >= 90 => ProgressColor.Danger,
        >= 75 => ProgressColor.Warning,
        _ => ProgressColor.Primary,
    };

    /// <summary>Returns the badge color of an account status.</summary>
    public static BadgeColor StatusColor(SaasAccountStatus status) => status switch
    {
        SaasAccountStatus.Active => BadgeColor.Success,
        SaasAccountStatus.Trial => BadgeColor.Warning,
        _ => BadgeColor.Danger,
    };

    #endregion

    #endregion
}
