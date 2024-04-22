namespace BlazorBootstrap;

public class SortableListEventArgs : EventArgs
{
    #region Constructors

    public SortableListEventArgs(int oldIndex, int newIndex)
    {
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    public SortableListEventArgs(int oldIndex, int newIndex, string fromListName, string toListName)
    {
        OldIndex = oldIndex;
        NewIndex = newIndex;
        FromListName = fromListName;
        ToListName = toListName;
    }

    #endregion

    #region Properties, Indexers

    public string? FromListName { get; }
    public int NewIndex { get; }
    public int OldIndex { get; }
    public string? ToListName { get; }

    #endregion
}
