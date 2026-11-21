namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// An in-app notification for one demo account, shown in the header's notifications menu. Workflow steps that need
/// someone's action create them.
/// </summary>
public sealed class ShowcaseNotification
{
    #region Properties, Indexers

    /// <summary>Gets the demo account it is for.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets when it was raised, on the site's clock.</summary>
    public DateTime At { get; init; }

    /// <summary>Gets the notification number, unique within the site.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets whether the account has read it.</summary>
    public bool IsRead { get; set; }

    /// <summary>Gets an optional second line, for example the record's name.</summary>
    public string? Text { get; init; }

    /// <summary>Gets the one-line message, for example "Ticket 4182 was escalated to you".</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the link to open (for example <c>/showcase/help-desk/agent/tickets/4182</c>), or <see langword="null" /> when there is nothing to open.</summary>
    public string? Url { get; init; }

    #endregion
}
