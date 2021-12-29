namespace BlazorBootstrap.Components;

/// <summary>
/// User state of the <see cref="HxGrid"/>.
/// </summary>
public class GridState<TItem>
{
    public GridState(int pageIndex, IReadOnlyList<SortingItem<TItem>> sorting)
    {
        PageIndex = pageIndex;
        Sorting = sorting;
    }

    /// <summary>
    /// Current page index.
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Current sorting.
    /// </summary>
    public IReadOnlyList<SortingItem<TItem>> Sorting { get; }
}
