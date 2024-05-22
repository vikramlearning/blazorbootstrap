namespace BlazorBootstrap;

public class RibbonEventArgs : EventArgs
{
    #region Constructors

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
    public string ActiveTabTitle { get; }

    /// <summary>
    /// Gets the previous active tab title.
    /// </summary>
    public string PreviousActiveTabTitle { get; }

    #endregion
}
