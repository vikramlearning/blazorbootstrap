namespace BlazorBootstrap;

public class GridRowEventArgs<TItem> : EventArgs
{
    public GridRowEventArgs(TItem item)
    {
        this.Item = item;
    }

    public TItem Item { get; set; }
}
