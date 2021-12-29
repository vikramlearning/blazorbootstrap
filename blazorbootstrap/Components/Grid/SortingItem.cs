using System.Linq.Expressions;

namespace BlazorBootstrap.Components;

/// <summary>
/// Item describes one sorting criteria.
/// </summary>
public sealed class SortingItem<TItem>
{
	/// <summary>
	/// Selector function of sorting key. To be used for automatic in-memory sorting.
	/// </summary>
	public Expression<Func<TItem, IComparable>> SortKeySelector { get; }

	/// <summary>
	/// Sort direction of SortString/SortKeySelector.
	/// </summary>
	public SortDirection SortDirection { get; }

	/// <summary>
	/// Constructor.
	/// </summary>
	public SortingItem(Expression<Func<TItem, IComparable>> sortKeySelector, SortDirection sortDirection)
	{
		SortKeySelector = sortKeySelector;
		SortDirection = sortDirection;
	}
}
