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
            return filterItem.Operator switch
            {
                FilterOperator.Equals => GetNumberEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.NotEquals => GetNumberNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.LessThan => GetNumberLessThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.LessThanOrEquals => GetNumberLessThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.GreaterThan => GetNumberGreaterThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.GreaterThanOrEquals => GetNumberGreaterThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                _ => GetNumberEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
            };
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameString
            || propertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            return filterItem.Operator switch
            {
                FilterOperator.Contains => GetStringContainsExpressionDelegate<TItem>(parameterExpression, filterItem),
                FilterOperator.StartsWith => GetStringStartsWithExpressionDelegate<TItem>(parameterExpression, filterItem),
                FilterOperator.EndsWith => GetStringEndsWithExpressionDelegate<TItem>(parameterExpression, filterItem),
                FilterOperator.Equals => GetStringEqualsExpressionDelegate<TItem>(parameterExpression, filterItem),
                _ => GetStringContainsExpressionDelegate<TItem>(parameterExpression, filterItem),
            };
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            return filterItem.Operator switch
            {
                FilterOperator.Equals => GetDateEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.NotEquals => GetDateNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.LessThan => GetDateLessThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.LessThanOrEquals => GetDateLessThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.GreaterThan => GetDateGreaterThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.GreaterThanOrEquals => GetDateGreaterThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                _ => GetDateEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
            };
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            return filterItem.Operator switch
            {
                FilterOperator.Equals => GetBooleanEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.NotEquals => GetBooleanNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                _ => GetBooleanEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
            };
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
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var methodInfo = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);
        var expression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);
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
