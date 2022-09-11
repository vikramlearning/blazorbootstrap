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
        if (propertyInfo == null)
            return null;

        var propertyTypeName = propertyInfo.PropertyType.Name;

        if (propertyTypeName == StringConstants.PropertyTypeNameInt16
            || propertyTypeName == StringConstants.PropertyTypeNameInt32
            || propertyTypeName == StringConstants.PropertyTypeNameInt64
            || propertyTypeName == StringConstants.PropertyTypeNameSingle
            || propertyTypeName == StringConstants.PropertyTypeNameDecimal
            || propertyTypeName == StringConstants.PropertyTypeNameDouble)
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
        else if (propertyTypeName == StringConstants.PropertyTypeNameString
            || propertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            switch (filterItem.Operator)
            {
                case FilterOperator.Contains:
                    return GetStringContainsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.StartsWith:
                    return GetStringStartsWithExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.EndsWith:
                    return GetStringEndsWithExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.Equals:
                    return GetStringEqualsExpressionDelegate<TItem>(parameterExpression, filterItem);
                case FilterOperator.NotEquals:
                    return GetStringNotEqualsExpressionDelegate<TItem>(parameterExpression, filterItem);
                default:
                    break;
            }
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            switch (filterItem.Operator)
            {
                case FilterOperator.Equals:
                    return GetDateEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.NotEquals:
                    return GetDateNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.LessThan:
                    return GetDateLessThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.LessThanOrEquals:
                    return GetDateLessThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.GreaterThan:
                    return GetDateGreaterThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.GreaterThanOrEquals:
                    return GetDateGreaterThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                default:
                    break;
            }
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            switch (filterItem.Operator)
            {
                case FilterOperator.Equals:
                    return GetBooleanEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
                case FilterOperator.NotEquals:
                    return GetBooleanNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName);
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

        if (propertyTypeName == StringConstants.PropertyTypeNameInt16)
        {
            _ = short.TryParse(filterItem.Value, out short filterValue);
            value = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameInt32)
        {
            _ = int.TryParse(filterItem.Value, out int filterValue);
            value = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameInt64)
        {
            _ = long.TryParse(filterItem.Value, out long filterValue);
            value = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameSingle)
        {
            _ = float.TryParse(filterItem.Value, out float filterValue);
            value = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDecimal)
        {
            _ = decimal.TryParse(filterItem.Value, out decimal filterValue);
            value = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            _ = double.TryParse(filterItem.Value, out double filterValue);
            value = Expression.Constant(filterValue);
        }

        return value;
    }

    #endregion Number

    #region string

    public static Expression<Func<TItem, bool>> GetStringContainsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var methodInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);
        var expression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);
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
        var methodInfo = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string), typeof(StringComparison) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);
        var expression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringEndsWithExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string), typeof(StringComparison) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);
        var expression = Expression.Call(propertyExp, method, someValue, comparisonExpression);
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    #endregion string

    #region Date

    public static Expression<Func<TItem, bool>> GetDateEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.Equal(property, GetDateConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.NotEqual(property, GetDateConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateLessThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.LessThan(property, GetDateConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateLessThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.LessThanOrEqual(property, GetDateConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateGreaterThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.GreaterThan(property, GetDateConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateGreaterThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.GreaterThanOrEqual(property, GetDateConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static ConstantExpression GetDateConstantExpression(FilterItem filterItem, string propertyTypeName)
    {
        ConstantExpression value = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly)
        {
            _ = DateOnly.TryParse(filterItem.Value, out DateOnly filterValue);
            value = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            _ = DateTime.TryParse(filterItem.Value, out DateTime filterValue);
            value = Expression.Constant(filterValue);
        }

        return value;
    }

    #endregion Date

    #region Boolean

    public static Expression<Func<TItem, bool>> GetBooleanEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.Equal(property, GetBooleanConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetBooleanNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.NotEqual(property, GetBooleanConstantExpression(filterItem, propertyTypeName));
        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static ConstantExpression GetBooleanConstantExpression(FilterItem filterItem, string propertyTypeName)
    {
        ConstantExpression value = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            _ = bool.TryParse(filterItem.Value, out bool filterValue);
            value = Expression.Constant(filterValue);
        }

        return value;
    }

    #endregion Boolean

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
