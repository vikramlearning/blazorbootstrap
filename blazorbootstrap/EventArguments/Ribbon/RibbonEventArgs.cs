namespace BlazorBootstrap;

public class RibbonEventArgs : EventArgs
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonEventArgs"/> class.
    /// </summary>
    /// <param name="activeTabTitle">The active tab title.</param>
    /// <param name="previousActiveTabTitle">The previous active tab title.</param>
    public RibbonEventArgs(string activeTabTitle, string previousActiveTabTitle)
    {
        ActiveTabTitle = activeTabTitle;
        PreviousActiveTabTitle = previousActiveTabTitle;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the active tab title.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets the active tab title.")]
    public string ActiveTabTitle { get; }

    /// <summary>
    /// Gets the previous active tab title.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets the previous active tab title.")]
    public string PreviousActiveTabTitle { get; }

    #endregion
}
