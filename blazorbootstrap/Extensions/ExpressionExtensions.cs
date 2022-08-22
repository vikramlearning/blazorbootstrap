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
        Console.WriteLine($"PropertyName: {filterItem.PropertyName}, Value: {filterItem.Value}, Type: {propertyInfo.PropertyType.Name}");

        Expression value = null;
        if (propertyInfo.PropertyType.Name == "Int32")
        {
            var property = Expression.Property(parameterExpression, filterItem.PropertyName);
            _ = int.TryParse(filterItem.Value, out int filterValue);
            value = Expression.Constant(filterValue);
            var expression = Expression.Equal(property, value);
            return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
        }
        else if (propertyInfo.PropertyType.Name == "String")
        {
            switch (filterItem.Operator)
            {
                case FilterOperator.Equals:
                    return GetStringEqualsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.NotEquals:
                    return GetStringNotEqualsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.LessThan:
                    break;
                case FilterOperator.LessThanOrEquals:
                    break;
                case FilterOperator.GreaterThan:
                    break;
                case FilterOperator.GreaterThanOrEquals:
                    break;
                case FilterOperator.Contains:
                    return GetStringContainsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.StartsWith:
                    return GetStringStartsWithExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.EndsWith:
                    return GetStringEndsWithExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.DoesNotContain:
                    break;
                case FilterOperator.IsNull:
                    break;
                case FilterOperator.IsEmpty:
                    break;
                case FilterOperator.IsNotNull:
                    break;
                case FilterOperator.IsNotEmpty:
                    break;
                default:
                    break;
            }
        }

        return null;
    }

    #region string

    public static Expression<Func<TItem, bool>> GetStringContainsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        MethodInfo method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var containsMethodExp = Expression.Call(propertyExp, method, someValue);

        return Expression.Lambda<Func<TItem, bool>>(containsMethodExp, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        Expression value = Expression.Constant(filterItem.Value);
        var expression = Expression.Equal(property, value);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringNotEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        Expression value = Expression.Constant(filterItem.Value);
        var expression = Expression.NotEqual(property, value);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringStartsWithExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        MethodInfo method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var containsMethodExp = Expression.Call(propertyExp, method, someValue);

        return Expression.Lambda<Func<TItem, bool>>(containsMethodExp, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringEndsWithExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        MethodInfo method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var containsMethodExp = Expression.Call(propertyExp, method, someValue);

        return Expression.Lambda<Func<TItem, bool>>(containsMethodExp, parameterExpression);
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
