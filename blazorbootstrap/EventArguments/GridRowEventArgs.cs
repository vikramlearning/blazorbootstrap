namespace BlazorBootstrap;

public class GridRowEventArgs<TItem> : EventArgs
{
    #region Constructors

    public GridRowEventArgs(TItem item)
    {
        Item = item;
    }

    #endregion

    #region Properties, Indexers

    public TItem Item { get; set; }

    #endregion
}
