namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// One line of a site's audit log: who did what to which record, and when. Sites keep these in their own data so they
/// are part of the tab's snapshot.
/// </summary>
public sealed class ShowcaseAuditEntry
{
    #region Properties, Indexers

    /// <summary>Gets what was done, in the past tense, for example "Escalated" or "Approved".</summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>Gets the name of the person who did it, or the system's name for automatic actions.</summary>
    public string Actor { get; init; } = string.Empty;

    /// <summary>Gets when it happened, on the site's clock.</summary>
    public DateTime At { get; init; }

    /// <summary>Gets an optional detail, for example "Priority High → Urgent".</summary>
    public string? Details { get; init; }

    /// <summary>Gets the entry number, unique within the site.</summary>
    public int Id { get; init; }

    /// <summary>Gets the record it was done to, for example "Ticket 4182".</summary>
    public string Target { get; init; } = string.Empty;

    /// <summary>Gets the link to the record (for example <c>/showcase/help-desk/agent/tickets/4182</c>), or <see langword="null" /> when the record has no page.</summary>
    public string? TargetUrl { get; init; }

    #endregion
}
