using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Formatting and focus helpers shared by the Beacon Desk screens. Formatting uses the invariant culture, so the Server
/// and WebAssembly hosts show the same text whatever the browser locale. Every badge carries its meaning in words too.
/// </summary>
public static class HelpDeskDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Formats a time relative to the site's clock, like "35 min ago", "3 h ago", or "Nov 21".</summary>
    public static string Ago(DateTime time, DateTime now)
    {
        var age = now - time;

        return age.TotalMinutes < 1 ? "just now"
            : age.TotalMinutes < 60 ? FormattableString.Invariant($"{(int)age.TotalMinutes} min ago")
            : age.TotalHours < 24 ? FormattableString.Invariant($"{(int)age.TotalHours} h ago")
            : time.ToString("MMM d", CultureInfo.InvariantCulture);
    }

    /// <summary>Returns the route of an article.</summary>
    public static string ArticleUrl(int articleId) => FormattableString.Invariant($"{HelpDeskLayout.Base}/kb/{articleId}");

    /// <summary>Formats a date and time like "Nov 24, 2026, 9:15 AM".</summary>
    public static string Stamp(DateTime time) => time.ToString("MMM d, yyyy, h:mm tt", CultureInfo.InvariantCulture);

    /// <summary>Moves focus to an element, ignoring one that has left the page or a closed circuit (WCAG 2.4.3).</summary>
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

    /// <summary>Returns the badge color of a priority.</summary>
    public static BadgeColor PriorityColor(HelpDeskPriority priority) => priority switch
    {
        HelpDeskPriority.Urgent => BadgeColor.Danger,
        HelpDeskPriority.High => BadgeColor.Warning,
        HelpDeskPriority.Normal => BadgeColor.Info,
        _ => BadgeColor.Light,
    };

    /// <summary>Returns the badge color of an SLA state.</summary>
    public static BadgeColor SlaColor(HelpDeskSlaState state) => state switch
    {
        HelpDeskSlaState.Breached or HelpDeskSlaState.Missed => BadgeColor.Danger,
        HelpDeskSlaState.AtRisk => BadgeColor.Warning,
        HelpDeskSlaState.OnTrack or HelpDeskSlaState.Met => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns the SLA text of a ticket, like "Overdue by 2 h", "Due in 1 h 30 min", "Paused", or "Met".</summary>
    public static string SlaText(HelpDeskState helpDesk, HelpDeskTicket ticket)
    {
        var state = helpDesk.Sla(ticket);
        var left = helpDesk.DueAt(ticket) - helpDesk.Now;

        return state switch
        {
            HelpDeskSlaState.Breached => $"Overdue by {Span(-left)}",
            HelpDeskSlaState.AtRisk or HelpDeskSlaState.OnTrack => $"Due in {Span(left)}",
            HelpDeskSlaState.Paused => "Paused",
            HelpDeskSlaState.Met => "SLA met",
            _ => "SLA missed",
        };
    }

    /// <summary>Formats a length like "45 min", "3 h", "1 h 30 min", "3 d", or "2 d 4 h".</summary>
    public static string Span(TimeSpan span)
    {
        var minutes = (int)Math.Round(span.TotalMinutes);

        return minutes < 60 ? FormattableString.Invariant($"{minutes} min")
            : minutes < 24 * 60 ? (minutes % 60 == 0 ? FormattableString.Invariant($"{minutes / 60} h") : FormattableString.Invariant($"{minutes / 60} h {minutes % 60} min"))
            : minutes % (24 * 60) / 60 == 0 ? FormattableString.Invariant($"{minutes / (24 * 60)} d")
            : FormattableString.Invariant($"{minutes / (24 * 60)} d {minutes % (24 * 60) / 60} h");
    }

    /// <summary>Returns the badge color of a status.</summary>
    public static BadgeColor StatusColor(HelpDeskStatus status) => status switch
    {
        HelpDeskStatus.New => BadgeColor.Primary,
        HelpDeskStatus.Open => BadgeColor.Info,
        HelpDeskStatus.Pending => BadgeColor.Secondary,
        _ => BadgeColor.Success,
    };

    /// <summary>Returns the agent desk's editor of an article.</summary>
    public static string ArticleEditUrl(int articleId) => FormattableString.Invariant($"{HelpDeskAgentLayout.Base}/kb/{articleId}");

    /// <summary>Returns the route of a ticket.</summary>
    public static string TicketUrl(int ticketId) => FormattableString.Invariant($"{HelpDeskAgentLayout.Base}/tickets/{ticketId}");

    /// <summary>Returns the customer's page for one of their requests.</summary>
    public static string RequestUrl(int ticketId) => FormattableString.Invariant($"{HelpDeskLayout.Base}/requests/{ticketId}");

    /// <summary>Returns the customer-facing name of a status: "Awaiting your reply" for Pending, "Solved" for Resolved.</summary>
    public static string CustomerStatus(HelpDeskStatus status) => status switch
    {
        HelpDeskStatus.New => "Received",
        HelpDeskStatus.Open => "In progress",
        HelpDeskStatus.Pending => "Awaiting your reply",
        _ => "Solved",
    };

    #endregion

    /// <summary>Formats a count with its noun, like "1 ticket" or "1,204 tickets"; <paramref name="plural" /> defaults to the noun plus "s".</summary>
    public static string Plural(int count, string noun, string? plural = null) =>
        count == 1 ? $"1 {noun}" : $"{count.ToString("#,0", CultureInfo.InvariantCulture)} {plural ?? noun + "s"}";

    #endregion
}
