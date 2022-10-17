﻿namespace BlazorBootstrap;

/// <summary>
/// User state of the <see cref="Grid"/>.
/// </summary>
public class GridState<TItem>
{
    public GridState(int pageIndex, IEnumerable<SortingItem<TItem>> sorting)
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
    public IEnumerable<SortingItem<TItem>> Sorting { get; }
}
