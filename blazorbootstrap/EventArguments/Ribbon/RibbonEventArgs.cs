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

    public string ActiveTabTitle { get; }
    public string PreviousActiveTabTitle { get; }

    #endregion
}
