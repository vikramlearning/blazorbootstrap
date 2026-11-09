namespace BlazorBootstrap;

/// <summary>
/// Provides the tab names for the tab events of a <see cref="ListGroup{TItem}" /> in tab mode.
/// </summary>
[AddedVersion("4.1.0")]
public class ListGroupTabEventArgs : EventArgs
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ListGroupTabEventArgs" /> class.
    /// </summary>
    /// <param name="activeTabName">The name of the tab that is, or is becoming, active.</param>
    /// <param name="previousTabName">The name of the tab that was active before, if any.</param>
    public ListGroupTabEventArgs(string? activeTabName, string? previousTabName)
    {
        ActiveTabName = activeTabName;
        PreviousTabName = previousTabName;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the name of the tab that is, or is becoming, active.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets the name of the tab that is, or is becoming, active.")]
    public string? ActiveTabName { get; }

    /// <summary>
    /// Gets the name of the tab that was active before, if any.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets the name of the tab that was active before, if any.")]
    public string? PreviousTabName { get; }

    #endregion
}
