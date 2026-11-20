namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The opt-in contract for a site that shows the enterprise baseline (rule WR-001): an audit log, in-app notifications,
/// and a global search. A site state implements it alongside <see cref="IShowcaseSiteState" />; sites that don't keep
/// working as before, and the shared header parts render nothing for them.
/// </summary>
public interface IShowcaseEnterpriseState : IShowcaseSiteState
{
    #region Methods

    /// <summary>Gets the signed-in account's notifications, newest first; empty when nobody is signed in.</summary>
    IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount();

    /// <summary>Marks every notification of the signed-in account as read.</summary>
    void MarkAllNotificationsRead();

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    void MarkNotificationRead(int id);

    /// <summary>
    /// Finds the records the signed-in person may open whose name or number contains <paramref name="term" />, ignoring
    /// case, best matches first. Returns nothing for a blank term or when nobody is signed in.
    /// </summary>
    IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults);

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, as far as the signed-in person may see it.</summary>
    IReadOnlyList<ShowcaseAuditEntry> AuditEntries { get; }

    #endregion
}
