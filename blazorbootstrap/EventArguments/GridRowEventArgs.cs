namespace BlazorBootstrap;

public class GridRowEventArgs<TItem> : EventArgs
{
    public GridRowEventArgs(TItem item)
    {
        Item = item;
    }

    public TItem Item { get; set; }
}
