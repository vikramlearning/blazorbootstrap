namespace BlazorBootstrap;

public class TabsEventArgs : EventArgs
{
    public TabsEventArgs(string activeTabTitle, string previousActiveTabTitle)
    {
        ActiveTabTitle = activeTabTitle;
        PreviousActiveTabTitle = previousActiveTabTitle;
    }

    public string ActiveTabTitle { get; }
    public string PreviousActiveTabTitle { get; }
}