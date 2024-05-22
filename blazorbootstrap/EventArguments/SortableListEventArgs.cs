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

    /// <summary>
    /// Gets the from list name.
    /// </summary>
    public string? FromListName { get; }

    /// <summary>
    /// Gets the new index.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Gets the old index.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets the to list name.
    /// </summary>
    public string? ToListName { get; }

    #endregion
}
