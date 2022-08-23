using System.Linq.Expressions;
using System.Reflection;

namespace BlazorBootstrap;

public static class ExpressionExtensions
{
    public static Expression<Func<TItem, bool>> And<TItem>(this Expression<Func<TItem, bool>> leftExpression, Expression<Func<TItem, bool>> rightExpression)
    {
        var parameterExpression = leftExpression.Parameters[0];

        SubstExpressionVisitor substExpressionVisitor = new();
        substExpressionVisitor.subst[rightExpression.Parameters[0]] = parameterExpression;

        Expression body = Expression.AndAlso(leftExpression.Body, substExpressionVisitor.Visit(rightExpression.Body));
        return Expression.Lambda<Func<TItem, bool>>(body, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> Or<TItem>(this Expression<Func<TItem, bool>> leftExpression, Expression<Func<TItem, bool>> rightExpression)
    {
        var parameterExpression = leftExpression.Parameters[0];

        SubstExpressionVisitor substExpressionVisitor = new();
        substExpressionVisitor.subst[rightExpression.Parameters[0]] = parameterExpression;

        Expression body = Expression.OrElse(leftExpression.Body, substExpressionVisitor.Visit(rightExpression.Body));
        return Expression.Lambda<Func<TItem, bool>>(body, parameterExpression);
    }

    #region Expression Delegate

    public static Expression<Func<TItem, bool>> GetExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyInfo = typeof(TItem).GetProperty(filterItem.PropertyName);
        // TODO: remove below console log
        Console.WriteLine($"PropertyName: {filterItem.PropertyName}, Value: {filterItem.Value}, Type: {propertyInfo.PropertyType.Name}, Operator={filterItem.Operator}");

        if (propertyInfo.PropertyType.Name == "Int32")
        {
            switch (filterItem.Operator)
            {
                case FilterOperator.Equals:
                    return GetInt32EqualExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.NotEquals:
                    return GetInt32NotEqualExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.LessThan:
                    return GetInt32LessThanExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.LessThanOrEquals:
                    return GetInt32LessThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.GreaterThan:
                    return GetInt32GreaterThanExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.GreaterThanOrEquals:
                    return GetInt32GreaterThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem);
                default:
                    break;
            }
        }
        else if (propertyInfo.PropertyType.Name == "String")
        {
            switch (filterItem.Operator)
            {
                case FilterOperator.Contains:
                    return GetStringContainsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.DoesNotContain:
                    break;
                case FilterOperator.StartsWith:
                    return GetStringStartsWithExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.EndsWith:
                    return GetStringEndsWithExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.IsEmpty:
                    break;
                case FilterOperator.IsNotEmpty:
                    break;
                case FilterOperator.Equals:
                    return GetStringEqualsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.NotEquals:
                    return GetStringNotEqualsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.IsNull:
                    break;
                case FilterOperator.IsNotNull:
                    break;
                default:
                    break;
            }
        }

        return null;
    }

    #region Int32

    public static Expression<Func<TItem, bool>> GetInt32EqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        _ = int.TryParse(filterItem.Value, out int filterValue);
        var value = Expression.Constant(filterValue);
        var expression = Expression.Equal(property, Expression.Constant(filterValue));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetInt32NotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        _ = int.TryParse(filterItem.Value, out int filterValue);
        var value = Expression.Constant(filterValue);
        var expression = Expression.NotEqual(property, Expression.Constant(filterValue));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetInt32LessThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        _ = int.TryParse(filterItem.Value, out int filterValue);
        var value = Expression.Constant(filterValue);
        var expression = Expression.LessThan(property, Expression.Constant(filterValue));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetInt32LessThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        _ = int.TryParse(filterItem.Value, out int filterValue);
        var value = Expression.Constant(filterValue);
        var expression = Expression.LessThanOrEqual(property, Expression.Constant(filterValue));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetInt32GreaterThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        _ = int.TryParse(filterItem.Value, out int filterValue);
        var value = Expression.Constant(filterValue);
        var expression = Expression.GreaterThan(property, Expression.Constant(filterValue));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetInt32GreaterThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        _ = int.TryParse(filterItem.Value, out int filterValue);
        var value = Expression.Constant(filterValue);
        var expression = Expression.GreaterThanOrEqual(property, Expression.Constant(filterValue));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    #endregion Int32

    #region string

    public static Expression<Func<TItem, bool>> GetStringContainsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var expression = Expression.Call(propertyExp, method, someValue);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var value = Expression.Constant(filterItem.Value);
        var expression = Expression.Equal(property, value);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringNotEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var value = Expression.Constant(filterItem.Value);
        var expression = Expression.NotEqual(property, value);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringStartsWithExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var expression = Expression.Call(propertyExp, method, someValue);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringEndsWithExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var expression = Expression.Call(propertyExp, method, someValue);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    #endregion string

    #endregion Expression Delegate
}

internal class SubstExpressionVisitor : ExpressionVisitor
{
    public Dictionary<Expression, Expression> subst = new();

    protected override Expression VisitParameter(ParameterExpression parameterExpression)
    {
        if (subst.TryGetValue(parameterExpression, out Expression newExpression))
        {
            return newExpression;
        }

        return parameterExpression;
    }
}
