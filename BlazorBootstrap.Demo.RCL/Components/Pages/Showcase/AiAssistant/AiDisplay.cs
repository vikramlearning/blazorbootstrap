using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

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

    /// <summary>
    /// Returns where the back office opens for a role: usage analytics for Admins and the Owner, the shared prompts for a
    /// prompt editor (the only module they may use).
    /// </summary>
    public static string AdminHomeFor(AiState state) => state.Can(AiPermission.ViewAnalytics) ? AiAdminLayout.Base : AiAdminLayout.Base + "/prompts";

    /// <summary>Returns a permission in words, for the roles and permissions panel and the guards.</summary>
    public static string PermissionLabel(AiPermission permission) => permission switch
    {
        AiPermission.UseAssistant => "Chat, keep personal prompts, and see their own usage",
        AiPermission.ManageSharedPrompts => "Write, publish, and delete shared prompts",
        AiPermission.ViewAnalytics => "See the workspace's usage analytics",
        AiPermission.ManageMembers => "Invite people, change roles up to Admin, and remove or restore access",
        AiPermission.ManageSettings => "Change the workspace name, models, attachments, and blocked terms",
        AiPermission.ViewAudit => "Read the audit log",
        AiPermission.ManageSpend => "Set the workspace and member spend caps",
        AiPermission.ReviewSensitiveData => "Set the sensitive data policy and review its events",
        AiPermission.ManageRetention => "Change how long conversations are kept",
        _ => "Transfer ownership",
    };

    /// <summary>Returns a member role in words, like "Prompt editor".</summary>
    public static string RoleLabel(AiMemberRole role) => AiState.RoleLabel(role);

    /// <summary>Returns the roles that have a permission, in words, like "Admins and the Owner".</summary>
    public static string RolesWith(AiPermission permission)
    {
        // Every permission includes the Owner (each role has the rights of the roles below it), so the Owner closes the list.
        var others = Enum.GetValues<AiMemberRole>().Where(role => role != AiMemberRole.Owner && AiState.RoleAllows(role, permission)).Select(RolePlural).ToList();

        return others.Count switch
        {
            0 => "the Owner",
            1 => $"{others[0]} and the Owner",
            _ => string.Join(", ", others) + ", and the Owner",
        };
    }

    /// <summary>Returns what a role may do, in one sentence.</summary>
    public static string RoleSummary(AiMemberRole role) => AiState.RoleSummary(role);

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

    #region Private Methods

    private static string RolePlural(AiMemberRole role) => role switch
    {
        AiMemberRole.PromptEditor => "prompt editors",
        AiMemberRole.Admin => "Admins",
        AiMemberRole.Owner => "the Owner",
        _ => "members",
    };

    #endregion

    #endregion
}
