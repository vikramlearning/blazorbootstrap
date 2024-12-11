namespace BlazorBootstrap;

/// <summary>
/// User state of the <see cref="Grid{T}" />.
/// </summary>
public class GridState<TItem>
{
    #region Constructors

    public GridState(int pageIndex, IReadOnlyCollection<SortingItem<TItem>>? sorting)
    {
        PageIndex = pageIndex;
        Sorting = sorting;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Current page index.
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Current sorting.
    /// </summary>
    public IReadOnlyCollection<SortingItem<TItem>>? Sorting { get; }

    #endregion
}
