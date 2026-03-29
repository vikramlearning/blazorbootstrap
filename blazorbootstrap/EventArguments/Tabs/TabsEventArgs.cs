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
    [AddedVersion("1.0.0")]
    [Description("Gets the active <b>Tab</b> name.")]
    public string ActiveTabName { get; }

    /// <summary>
    /// Gets the active <see cref="Tab" /> title.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets the active <b>Tab</b> title.")]
    public string ActiveTabTitle { get; }

    /// <summary>
    /// Gets the previous active <see cref="Tab" /> name.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets the previous active <b>Tab</b> name.")]
    public string PreviousActiveTabName { get; }

    /// <summary>
    /// Gets the previous active <see cref="Tab" /> title.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets the previous active <b>Tab</b> title.")]
    public string PreviousActiveTabTitle { get; }

    #endregion
}
