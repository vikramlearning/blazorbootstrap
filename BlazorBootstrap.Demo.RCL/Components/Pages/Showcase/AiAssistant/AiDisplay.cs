using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Routes, formatting, and focus helpers shared by the Parley screens. Formatting uses the invariant culture, so the Server
/// and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class AiDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Formats how long ago a time was, like "just now", "5 min ago", "3 h ago", or "Nov 20".</summary>
    public static string Ago(DateTime time, DateTime now)
    {
        var age = now - time;

        return age.TotalMinutes < 1 ? "just now"
            : age.TotalMinutes < 60 ? FormattableString.Invariant($"{(int)age.TotalMinutes} min ago")
            : age.TotalHours < 24 ? FormattableString.Invariant($"{(int)age.TotalHours} h ago")
            : time.ToString("MMM d", CultureInfo.InvariantCulture);
    }

    /// <summary>Returns the route of a conversation.</summary>
    public static string ConversationUrl(int conversationId) => FormattableString.Invariant($"{AiLayout.Base}/chat/{conversationId}");

    /// <summary>Formats a file size like "820 KB" or "2.4 MB".</summary>
    public static string FileSize(long bytes) => bytes < 1024 * 1024
        ? FormattableString.Invariant($"{Math.Max(1, bytes / 1024)} KB")
        : (bytes / 1024d / 1024d).ToString("0.0", CultureInfo.InvariantCulture) + " MB";

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

    /// <summary>Returns a member role in words.</summary>
    public static string RoleLabel(AiMemberRole role) => role == AiMemberRole.Admin ? "Workspace admin" : "Member";

    /// <summary>Formats a date and time like "Nov 24, 09:30".</summary>
    public static string Stamp(DateTime time) => time.ToString("MMM d, HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Returns a member status in words.</summary>
    public static string StatusLabel(AiMemberStatus status) => status switch
    {
        AiMemberStatus.Invited => "Invited",
        AiMemberStatus.Deactivated => "Deactivated",
        _ => "Active",
    };

    /// <summary>Formats a token count like "1,234" or "1.2 M".</summary>
    public static string Tokens(long tokens) => tokens >= 1_000_000
        ? (tokens / 1_000_000d).ToString("0.0", CultureInfo.InvariantCulture) + " M"
        : tokens.ToString("#,0", CultureInfo.InvariantCulture);

    #endregion

    #endregion
}
