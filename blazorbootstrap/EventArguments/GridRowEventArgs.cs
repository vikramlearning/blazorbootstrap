namespace BlazorBootstrap;

public class GridRowEventArgs<TItem> : EventArgs
{
    #region Constructors

    public GridRowEventArgs(TItem item, MouseEventArgs mouseEventArgs)
    {
        Item = item;
        MouseEventArgs = mouseEventArgs;
    }

    #endregion

    #region Properties, Indexers

    public TItem Item { get; }
    
    public MouseEventArgs MouseEventArgs { get; }

    #endregion
}
