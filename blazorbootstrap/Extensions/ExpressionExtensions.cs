using System.Linq.Expressions;

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

        var propertyTypeName = propertyInfo.PropertyType.Name;
        if (propertyTypeName == StringConstants.PropertyTypeNameInt32)
        {
            switch (filterItem.Operator)
            {
                case FilterOperator.Equals:
                    return GetNumberEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.NotEquals:
                    return GetNumberNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.LessThan:
                    return GetNumberLessThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.LessThanOrEquals:
                    return GetNumberLessThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.GreaterThan:
                    return GetNumberGreaterThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.GreaterThanOrEquals:
                    return GetNumberGreaterThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                default:
                    break;
            }
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameString)
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

    #region Number

    public static Expression<Func<TItem, bool>> GetNumberEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.Equal(property, GetNumberConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.NotEqual(property, GetNumberConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberLessThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.LessThan(property, GetNumberConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberLessThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.LessThanOrEqual(property, GetNumberConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberGreaterThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.GreaterThan(property, GetNumberConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberGreaterThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.GreaterThanOrEqual(property, GetNumberConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static ConstantExpression GetNumberConstantExpression(FilterItem filterItem, string propertyTypeName)
    {
        ConstantExpression value = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameInt32)
        {
            _ = int.TryParse(filterItem.Value, out int filterValue);
            value = Expression.Constant(filterValue);
        }

        return value;
    }

    #endregion Number

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
