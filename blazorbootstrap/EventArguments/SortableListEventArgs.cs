namespace BlazorBootstrap;

public class SortableListEventArgs : EventArgs
{
    #region Constructors

    public SortableListEventArgs(int oldIndex, int newIndex)
    {
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    #endregion

    #region Properties, Indexers

    public int OldIndex { get; }
    public int NewIndex { get; }

    #endregion
}
