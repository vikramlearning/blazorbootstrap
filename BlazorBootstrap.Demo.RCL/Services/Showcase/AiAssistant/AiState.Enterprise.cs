using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The enterprise baseline of <see cref="AiState" /> (rule WR-001): the audit log (who changed the model policy, the
/// settings, shared prompts, roles, members, and ownership, and when; never chat content or prompt text, only lengths and
/// names), the header's notifications (raised where someone must act or hear back: a role changed, access restored, a
/// shared prompt published, a prompt you wrote edited or deleted by someone else, ownership transferred, someone joined),
/// and the header's global search for each role. Audit entries and notifications link only to pages on this site.
/// </summary>
public sealed partial class AiState
{
    #region Fields and Constants

    /// <summary>The longest notification text.</summary>
    private const int MaxNoticeTextLength = 200;

    /// <summary>The longest notification title.</summary>
    private const int MaxNoticeTitleLength = 80;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the signed-in account's notifications for the header's menu, newest first; empty when nobody is signed in.</summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Identity.Current is not { } account
            ? Array.Empty<ShowcaseNotification>()
            : notifications.Where(notice => notice.AccountId == account.Id).OrderByDescending(notice => notice.At).ThenByDescending(notice => notice.Id).ToList();

    /// <summary>Marks every notification of the signed-in account as read (the header's "Mark all as read").</summary>
    public void MarkAllNotificationsRead()
    {
        if (Identity.Current is not { } account || notifications.All(notice => notice.AccountId != account.Id || notice.IsRead))
            return;

        foreach (var notice in notifications.Where(notice => notice.AccountId == account.Id))
            notice.IsRead = true;

        RaiseChanged();
    }

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (Identity.Current is { } account && notifications.FirstOrDefault(notice => notice.Id == id && notice.AccountId == account.Id) is { IsRead: false } notice)
        {
            notice.IsRead = true;
            RaiseChanged();
        }
    }

    /// <summary>
    /// The header's global search, best matches first (exactly the term, then starting with it, then containing it), then by
    /// kind and newest first. Every active member finds their own conversations by title (opening the chat) and prompts by
    /// title in their scope: their own (opening the editor) and the published shared ones (opening the read-only page);
    /// prompt editors, admins, and the Owner also find shared drafts and open shared prompts in the back office editor;
    /// admins and the Owner also find members and invitations by name or email (opening the members list filtered to them).
    /// Returns nothing for a blank term, or when nobody active is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || ActiveAccount() is not { } account)
            return Array.Empty<ShowcaseSearchResult>();

        var matches = new List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)>();
        var editor = Can(AiPermission.ManageSharedPrompts);

        // 0: exactly the term, 1: starts with it, 2: contains it; null: no match.
        static int? Rank(string text, params string?[] values) => values
            .Select(value => value is null ? (int?)null
                : string.Equals(value, text, StringComparison.OrdinalIgnoreCase) ? 0
                : value.StartsWith(text, StringComparison.OrdinalIgnoreCase) ? 1
                : value.Contains(text, StringComparison.OrdinalIgnoreCase) ? 2
                : null)
            .Where(rank => rank is not null)
            .DefaultIfEmpty(null)
            .Min();

        // Kind: 0 a member, 1 a prompt, 2 a conversation.
        if (Can(AiPermission.ManageMembers) && term.Length >= 2)
        {
            foreach (var member in members)
            {
                if (Rank(term, member.Name, member.Email) is { } rank)
                {
                    var status = member.Status switch { AiMemberStatus.Invited => "invited", AiMemberStatus.Deactivated => "access removed", _ => "active" };
                    matches.Add((rank, 0, 0, new ShowcaseSearchResult(member.Status == AiMemberStatus.Invited ? "Invitation" : "Member", member.Name,
                        $"{member.Email} · {RoleLabel(member.Role)} · {status}", AiSeedData.MembersPath(member.Email))));
                }
            }
        }

        foreach (var prompt in prompts.Where(prompt => prompt.IsShared ? prompt.IsPublished || editor : prompt.OwnerId == account.Id))
        {
            if (Rank(term, prompt.Title) is not { } rank)
                continue;

            var kind = !prompt.IsShared ? "Your prompt" : prompt.IsPublished ? "Shared prompt" : "Draft shared prompt";
            var url = prompt.IsShared && editor ? AiSeedData.SharedPromptPath(prompt.Id) : AiSeedData.PromptPath(prompt.Id);
            matches.Add((rank, 1, -prompt.UpdatedAt.Ticks, new ShowcaseSearchResult(kind, prompt.Title,
                $"{prompt.Category} · {(prompt.IsShared ? $"by {prompt.Author} · " : null)}updated {prompt.UpdatedAt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}", url)));
        }

        foreach (var conversation in conversations.Where(conversation => conversation.AccountId == account.Id))
        {
            if (Rank(term, conversation.Title) is { } rank)
            {
                matches.Add((rank, 2, -conversation.UpdatedAt.Ticks, new ShowcaseSearchResult("Conversation", conversation.Title,
                    $"{FindModel(conversation.ModelId)?.Name} · {conversation.Messages.Count(message => message.Role == AiRole.User)} messages · {conversation.UpdatedAt.ToString("MMM d, HH:mm", CultureInfo.InvariantCulture)}",
                    AiSeedData.ConversationPath(conversation.Id))));
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Order)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    #endregion

    #region Private Methods

    /// <summary>Checks a stored audit or notification link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) =>
        url is null || (url.Length <= 200 && (url == AiSeedData.SiteBase || url.StartsWith(AiSeedData.SiteBase + "/", StringComparison.Ordinal)) && !url.Contains("//", StringComparison.Ordinal)
            && url.All(character => !char.IsWhiteSpace(character) && !char.IsControl(character) && character is not ('"' or '\'' or '<' or '>' or '\\')));

    /// <summary>
    /// Raises a notification for an account that can sign in (rule WR-001): nothing for a colleague without a sign-in, and
    /// nothing for the person who caused it, unless the workspace itself sends it (<paramref name="fromSystem" />, like a
    /// spend cap warning after their own message). The tab keeps its newest notifications within the limit a restore accepts.
    /// </summary>
    private void Notify(string? accountId, string title, string text, string? link, bool fromSystem = false)
    {
        if (accountId is null || Identity.FindAccount(accountId) is null || (!fromSystem && accountId == Identity.Current?.Id) || nextNotificationId >= NextIdLimit)
            return;

        notifications.Add(new ShowcaseNotification
        {
            Id = nextNotificationId++,
            AccountId = accountId,
            At = Clock.Now,
            Title = Clip(title, MaxNoticeTitleLength),
            Text = string.IsNullOrWhiteSpace(text) ? null : Clip(text, MaxNoticeTextLength),
            Url = IsSiteLink(link) ? link : null,
        });

        while (notifications.Count > MaxNotifications)
            notifications.RemoveAt(0);
    }

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock; the workspace itself for <paramref name="system" /> actions or
    /// when nobody is signed in. Details name what changed, never chat content or prompt text. The log keeps its newest
    /// entries (at most <see cref="MaxAddedAudit" /> added by the tab).
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details = null, bool system = false)
    {
        if (nextAuditId >= NextIdLimit)
            return;

        var actor = system ? null : Identity.Current?.Name;

        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = Clock.Now,
            Actor = Clip(string.IsNullOrWhiteSpace(actor) ? AiSeedData.SystemActor : actor.Trim(), ShowcaseIdentity.MaxNameLength),
            Action = Clip(action, MaxAuditTextLength),
            Target = Clip(target, MaxAuditTextLength),
            TargetUrl = IsSiteLink(targetUrl) ? targetUrl : null,
            Details = string.IsNullOrWhiteSpace(details) ? null : Clip(details, MaxAuditTextLength),
        });

        // The seeded log stays; what this tab added keeps its newest entries, so the snapshot (which stores only those) stays small.
        var firstAdded = seed.Value.Audit.Count + 1;

        if (audit.Count(entry => entry.Id >= firstAdded) > MaxAddedAudit)
            audit.RemoveAt(audit.FindIndex(entry => entry.Id >= firstAdded));

        while (audit.Count > MaxAudit)
            audit.RemoveAt(0);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for Admins and the Owner; empty for anyone else.</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        Can(AiPermission.ViewAudit) ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    #endregion
}
