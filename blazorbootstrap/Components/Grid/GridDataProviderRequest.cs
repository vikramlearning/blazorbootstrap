using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorBootstrap;

public class GridDataProviderRequest<TItem>
{
    /// <summary>
    /// Page number.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Size of the page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Current sorting.
    /// </summary>
    public IReadOnlyList<SortingItem<TItem>> Sorting { get; init; }

    /// <summary>
    /// Current filters.
    /// </summary>
    public IReadOnlyList<FilterItem> Filters { get; init; }

    public GridDataProviderResult<TItem> ApplyTo(IEnumerable<TItem> data)
    {
        if (data == null)
            return new GridDataProviderResult<TItem> { Data = null, TotalCount = null };

        IEnumerable<TItem> resultData;
        IEnumerable<TItem> filteredData = resultData = data;

        // apply filter
        if (Filters != null && Filters.Any())
        {
            var propertyInfo = typeof(TItem).GetProperty(Filters[3].PropertyName);
            if (propertyInfo != null)
            {
                try
                {
                    var parameterExpression = Expression.Parameter(typeof(TItem)); // second param optional

                    Console.WriteLine($"{Filters[0].PropertyName}");

                    var property1 = Expression.Property(parameterExpression, Filters[0].PropertyName);
                    var expression1 = Expression.Equal(property1, Expression.Constant(104)); // value conversion required.
                    var lambda1 = Expression.Lambda<Func<TItem, bool>>(expression1, parameterExpression);

                    Console.WriteLine($"{Filters[1].PropertyName}");
                    var property2 = Expression.Property(parameterExpression, Filters[1].PropertyName);
                    var expression2 = Expression.Equal(property2, Expression.Constant("Ronald")); // value conversion required.

                    var lambda2 = Expression.Lambda<Func<TItem, bool>>(expression2, parameterExpression);
                    var finalLambda = lambda1.And(lambda2);

                    filteredData = resultData.Where(finalLambda.Compile());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // apply sorting
        if (Sorting != null && Sorting.Any())
        {
            IOrderedEnumerable<TItem> orderedData = (Sorting[0].SortDirection == SortDirection.Ascending)
                       ? (filteredData ?? resultData).OrderBy(Sorting[0].SortKeySelector.Compile())
                       : (filteredData ?? resultData).OrderByDescending(Sorting[0].SortKeySelector.Compile());

            for (int i = 1; i < Sorting.Count; i++)
            {
                orderedData = (Sorting[i].SortDirection == SortDirection.Ascending)
                    ? orderedData.ThenBy(Sorting[i].SortKeySelector.Compile())
                    : orderedData.ThenByDescending(Sorting[i].SortKeySelector.Compile());
            }
            resultData = orderedData;
        }

        // apply paging
        var skip = 0;
        var take = data.Count();

        if (PageNumber > 0 && PageSize > 0)
        {
            skip = (PageNumber - 1) * PageSize;
            take = PageSize;
        }
        resultData = resultData.Skip(skip).Take(take);

        return new GridDataProviderResult<TItem>
        {
            Data = resultData.ToList(),
            TotalCount = data.Count()
        };
    }
}

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
    {
        ParameterExpression p = a.Parameters[0];

        SubstExpressionVisitor visitor = new SubstExpressionVisitor();
        visitor.subst[b.Parameters[0]] = p;

        Expression body = Expression.AndAlso(a.Body, visitor.Visit(b.Body));
        return Expression.Lambda<Func<T, bool>>(body, p);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
    {
        ParameterExpression p = a.Parameters[0];

        SubstExpressionVisitor visitor = new SubstExpressionVisitor();
        visitor.subst[b.Parameters[0]] = p;

        Expression body = Expression.OrElse(a.Body, visitor.Visit(b.Body));
        return Expression.Lambda<Func<T, bool>>(body, p);
    }
}

internal class SubstExpressionVisitor : ExpressionVisitor
{
    public Dictionary<Expression, Expression> subst = new Dictionary<Expression, Expression>();

    protected override Expression VisitParameter(ParameterExpression node)
    {
        Expression newValue;
        if (subst.TryGetValue(node, out newValue))
        {
            return newValue;
        }
        return node;
    }
}