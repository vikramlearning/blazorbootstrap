namespace BlazorBootstrap;

public class TabsEventArgs : EventArgs
{
    #region Constructors

    public TabsEventArgs(string activeTabName, string activeTabTitle, string previousActiveTabName, string previousActiveTabTitle)
    {
        ActiveTabName = activeTabName;
        ActiveTabTitle = activeTabTitle;
        PreviousActiveTabName = previousActiveTabName;
        PreviousActiveTabTitle = previousActiveTabTitle;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the active <see cref="Tab" /> name.
    /// </summary>
    public string ActiveTabName { get; }

    /// <summary>
    /// Gets the active <see cref="Tab" /> title.
    /// </summary>
    public string ActiveTabTitle { get; }

    /// <summary>
    /// Gets the previous active <see cref="Tab" /> name.
    /// </summary>
    public string PreviousActiveTabName { get; }

    /// <summary>
    /// Gets the previous active <see cref="Tab" /> title.
    /// </summary>
    public string PreviousActiveTabTitle { get; }

    #endregion
}
