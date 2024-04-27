namespace BlazorBootstrap;

public class TabsEventArgs : EventArgs
{
    #region Constructors

    public TabsEventArgs(string activeTabTitle, string previousActiveTabTitle)
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
