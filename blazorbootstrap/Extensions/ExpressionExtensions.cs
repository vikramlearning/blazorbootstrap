using Microsoft.Extensions.DependencyInjection;

namespace BlazorBootstrap;

public static class ExpressionExtensions
{
    #region Methods

    public static Expression<Func<TItem, bool>> And<TItem>(this Expression<Func<TItem, bool>> leftExpression, Expression<Func<TItem, bool>> rightExpression)
    {
        var parameterExpression = leftExpression.Parameters[0];

        SubstExpressionVisitor substExpressionVisitor = new();
        substExpressionVisitor.subst[rightExpression.Parameters[0]] = parameterExpression;

        Expression body = Expression.AndAlso(leftExpression.Body, substExpressionVisitor.Visit(rightExpression.Body));

        return Expression.Lambda<Func<TItem, bool>>(body, parameterExpression);
    }

    public static ConstantExpression GetBooleanConstantExpression(FilterItem filterItem, string propertyTypeName)
    {
        ConstantExpression? value = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            _ = bool.TryParse(filterItem.Value, out var filterValue);
            value = Expression.Constant(filterValue);
        }

        return value!;
    }

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

    public static ConstantExpression GetDateConstantExpression(string? value, string propertyTypeName)
    {
        if (value == null)
            return Expression.Constant(null);

        ConstantExpression? constantExpression = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly)
        {
            _ = DateOnly.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            _ = DateTime.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant(filterValue);
        }

        return constantExpression!;
    }

    public static Expression<Func<TItem, bool>> GetDateEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem.Value, propertyTypeName);

        Expression nonNullComparisonExpression;

        if (propertyExpression.Type == typeof(DateOnly))
        {
            nonNullComparisonExpression = Expression.Equal(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
        }
        else if (propertyExpression.Type == typeof(DateTime))
        {
            nonNullComparisonExpression = Expression.Equal(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
        }
        else if (propertyExpression.Type == typeof(DateOnly?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.Equal(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else if (propertyExpression.Type == typeof(DateTime?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.Equal(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else
        {
            // Property type is not supported
            throw new ArgumentException($"Unsupported property type: {propertyExpression.Type}");
        }

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateGreaterThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem.Value, propertyTypeName);

        Expression nonNullComparisonExpression;

        if (propertyExpression.Type == typeof(DateOnly))
        {
            nonNullComparisonExpression = Expression.GreaterThan(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
        }
        else if (propertyExpression.Type == typeof(DateTime))
        {
            nonNullComparisonExpression = Expression.GreaterThan(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
        }
        else if (propertyExpression.Type == typeof(DateOnly?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.GreaterThan(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else if (propertyExpression.Type == typeof(DateTime?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.GreaterThan(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else
        {
            // Property type is not supported
            throw new ArgumentException($"Unsupported property type: {propertyExpression.Type}");
        }

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateGreaterThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem.Value, propertyTypeName);

        var nonNullComparisonExpression = GetDateGreaterThanOrEqualExpression(propertyExpression, dateConstantExpression);

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    private static Expression GetDateGreaterThanOrEqualExpression(
        MemberExpression propertyExpression,
        ConstantExpression dateConstantExpression
    ) {
        Expression nonNullComparisonExpression;

        if (propertyExpression.Type == typeof(DateOnly))
        {
            nonNullComparisonExpression = Expression.GreaterThanOrEqual(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
        }
        else if (propertyExpression.Type == typeof(DateTime))
        {
            nonNullComparisonExpression = Expression.GreaterThanOrEqual(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
        }
        else if (propertyExpression.Type == typeof(DateOnly?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.GreaterThanOrEqual(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else if (propertyExpression.Type == typeof(DateTime?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.GreaterThanOrEqual(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else
        {
            // Property type is not supported
            throw new ArgumentException($"Unsupported property type: {propertyExpression.Type}");
        }

        return nonNullComparisonExpression;
    }

    public static Expression<Func<TItem, bool>> GetDateLessThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem.Value, propertyTypeName);

        Expression nonNullComparisonExpression;

        if (propertyExpression.Type == typeof(DateOnly))
        {
            nonNullComparisonExpression = Expression.LessThan(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
        }
        else if (propertyExpression.Type == typeof(DateTime))
        {
            nonNullComparisonExpression = Expression.LessThan(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
        }
        else if (propertyExpression.Type == typeof(DateOnly?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.LessThan(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else if (propertyExpression.Type == typeof(DateTime?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.LessThan(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else
        {
            // Property type is not supported
            throw new ArgumentException($"Unsupported property type: {propertyExpression.Type}");
        }

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateLessThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem.Value, propertyTypeName);

        var nonNullComparisonExpression = GetDateLessThanOrEqualExpression(propertyExpression, dateConstantExpression);

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    private static Expression GetDateLessThanOrEqualExpression(
        MemberExpression propertyExpression,
        ConstantExpression dateConstantExpression
    ) {
        Expression nonNullComparisonExpression;

        if (propertyExpression.Type == typeof(DateOnly))
        {
            nonNullComparisonExpression = Expression.LessThanOrEqual(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
        }
        else if (propertyExpression.Type == typeof(DateTime))
        {
            nonNullComparisonExpression = Expression.LessThanOrEqual(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
        }
        else if (propertyExpression.Type == typeof(DateOnly?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.LessThanOrEqual(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else if (propertyExpression.Type == typeof(DateTime?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.LessThanOrEqual(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else
        {
            // Property type is not supported
            throw new ArgumentException($"Unsupported property type: {propertyExpression.Type}");
        }

        return nonNullComparisonExpression;
    }

    public static Expression<Func<TItem, bool>> GetDateBetweenExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var values = filterItem.Value.Split(Config.FilterBetweenSeparator);
        
        var leftValue = values[0];
        var rightValue = values.Length > 1 ? values[1] : leftValue;
        
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        
        var constantLeftExpression = GetDateConstantExpression(leftValue, propertyTypeName);
        var constantRightExpression = GetDateConstantExpression(rightValue, propertyTypeName);

        var finalExpression = Expression.AndAlso(
            GetNumberGreaterThanOrEqualExpression(propertyExpression, constantLeftExpression), 
            GetNumberLessThanOrEqualExpression(propertyExpression, constantRightExpression)
        );

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }


    public static Expression<Func<TItem, bool>> GetDateNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem.Value, propertyTypeName);

        Expression nonNullComparisonExpression;

        if (propertyExpression.Type == typeof(DateOnly))
        {
            nonNullComparisonExpression = Expression.NotEqual(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
        }
        else if (propertyExpression.Type == typeof(DateTime))
        {
            nonNullComparisonExpression = Expression.NotEqual(propertyExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
        }
        else if (propertyExpression.Type == typeof(DateOnly?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.NotEqual(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateOnly)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else if (propertyExpression.Type == typeof(DateTime?))
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            nonNullComparisonExpression = Expression.NotEqual(nullableValueExpression, Expression.Convert(dateConstantExpression, typeof(DateTime)));
            var nullCheckExpression = Expression.Property(propertyExpression, "HasValue");
            nonNullComparisonExpression = Expression.Condition(nullCheckExpression, nonNullComparisonExpression, Expression.Constant(false));
        }
        else
        {
            // Property type is not supported
            throw new ArgumentException($"Unsupported property type: {propertyExpression.Type}");
        }

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>>? GetExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyTypeName = typeof(TItem).GetPropertyTypeName(filterItem.PropertyName);

        if (propertyTypeName is StringConstants.PropertyTypeNameInt16
                                or StringConstants.PropertyTypeNameInt32
                                or StringConstants.PropertyTypeNameInt64
                                or StringConstants.PropertyTypeNameSingle
                                or StringConstants.PropertyTypeNameDecimal
                                or StringConstants.PropertyTypeNameDouble)
            return filterItem.Operator switch
                   {
                       FilterOperator.Equals => GetNumberEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.NotEquals => GetNumberNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.LessThan => GetNumberLessThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.LessThanOrEquals => GetNumberLessThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.GreaterThan => GetNumberGreaterThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.GreaterThanOrEquals => GetNumberGreaterThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.Between => GetNumberBetweenExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       _ => GetEmptyExpressionDelegate<TItem>(parameterExpression, filterItem)
                   };

        if (propertyTypeName is StringConstants.PropertyTypeNameString
                                or StringConstants.PropertyTypeNameChar)
            return filterItem.Operator switch
                   {
                       FilterOperator.Contains => GetStringContainsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.DoesNotContain => GetStringDoesNotContainExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.StartsWith => GetStringStartsWithExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.EndsWith => GetStringEndsWithExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.Equals => GetStringEqualsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.NotEquals => GetStringNotEqualsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       _ => GetEmptyExpressionDelegate<TItem>(parameterExpression, filterItem)
                   };

        if (propertyTypeName is StringConstants.PropertyTypeNameDateOnly
                                or StringConstants.PropertyTypeNameDateTime)
            return filterItem.Operator switch
                   {
                       FilterOperator.Equals => GetDateEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.NotEquals => GetDateNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.LessThan => GetDateLessThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.LessThanOrEquals => GetDateLessThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.GreaterThan => GetDateGreaterThanExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.GreaterThanOrEquals => GetDateGreaterThanOrEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.Between => GetDateBetweenExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       _ => GetEmptyExpressionDelegate<TItem>(parameterExpression, filterItem)
                   };

        if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
            return filterItem.Operator switch
                   {
                       FilterOperator.Equals => GetBooleanEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       FilterOperator.NotEquals => GetBooleanNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                       _ => GetEmptyExpressionDelegate<TItem>(parameterExpression, filterItem)
                   };

        if (propertyTypeName == StringConstants.PropertyTypeNameEnum)
            return filterItem.Operator switch
                   {
                       FilterOperator.Contains => GetEnumContainsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.DoesNotContain => GetEnumDoesNotContainExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.Equals => GetEnumEqualsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.NotEquals => GetEnumNotEqualsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.In => GetEnumInExpressionDelegate<TItem>(parameterExpression, filterItem),
                       _ => GetEmptyExpressionDelegate<TItem>(parameterExpression, filterItem)
                   };

        if (propertyTypeName == StringConstants.PropertyTypeNameGuid)
            return filterItem.Operator switch
                   {
                       FilterOperator.Contains => GetGuidContainsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       FilterOperator.Equals => GetGuidEqualsExpressionDelegate<TItem>(parameterExpression, filterItem),
                       _ => GetEmptyExpressionDelegate<TItem>(parameterExpression, filterItem)
                   };

        return null;
    }

    public static ConstantExpression GetNumberConstantExpression(string value, string propertyTypeName)
    {
        if (value is null)
            return Expression.Constant(null);

        ConstantExpression? constantExpression = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameInt16)
        {
            _ = short.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant((short?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameInt32)
        {
            _ = int.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant((int?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameInt64)
        {
            _ = long.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant((long?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameSingle)
        {
            _ = float.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant((float?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDecimal)
        {
            _ = decimal.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant((decimal?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            _ = double.TryParse(value, out var filterValue);
            constantExpression = Expression.Constant((double?)filterValue);
        }

        return constantExpression!;
    }

    public static Expression<Func<TItem, bool>> GetNumberEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem.Value, propertyTypeName);

        // Handle null check
        var nullCheckExpression = propertyExpression.Type.IsNullableType()
                                      ? Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type))
                                      : (Expression)Expression.Constant(true);

        // Perform the greater than or equal comparison
        Expression comparisonExpression;

        if (propertyExpression.Type.IsNullableType())
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            comparisonExpression = Expression.Equal(nullableValueExpression, constantExpression);
        }
        else
        {
            comparisonExpression = Expression.Equal(propertyExpression, constantExpression);
        }

        var finalExpression = Expression.AndAlso(nullCheckExpression, comparisonExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberGreaterThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem.Value, propertyTypeName);

        // Handle null check
        var nullCheckExpression = propertyExpression.Type.IsNullableType()
                                      ? Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type))
                                      : (Expression)Expression.Constant(true);

        // Perform the greater than or equal comparison
        Expression comparisonExpression;

        if (propertyExpression.Type.IsNullableType())
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            comparisonExpression = Expression.GreaterThan(nullableValueExpression, constantExpression);
        }
        else
        {
            comparisonExpression = Expression.GreaterThan(propertyExpression, constantExpression);
        }

        var finalExpression = Expression.AndAlso(nullCheckExpression, comparisonExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberGreaterThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName) {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem.Value, propertyTypeName);

        var finalExpression = GetNumberGreaterThanOrEqualExpression(propertyExpression, constantExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    private static BinaryExpression GetNumberGreaterThanOrEqualExpression(
        MemberExpression propertyExpression,
        ConstantExpression constantExpression
    ) {
        // Handle null check
        var nullCheckExpression = propertyExpression.Type.IsNullableType()
            ? Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type))
            : (Expression)Expression.Constant(true);

        // Perform the greater than or equal comparison
        Expression comparisonExpression;

        if (propertyExpression.Type.IsNullableType())
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            comparisonExpression = Expression.GreaterThanOrEqual(nullableValueExpression, constantExpression);
        }
        else
        {
            comparisonExpression = Expression.GreaterThanOrEqual(propertyExpression, constantExpression);
        }

        var finalExpression = Expression.AndAlso(nullCheckExpression, comparisonExpression);
        return finalExpression;
    }

    public static Expression<Func<TItem, bool>> GetNumberLessThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem.Value, propertyTypeName);

        // Handle null check
        var nullCheckExpression = propertyExpression.Type.IsNullableType()
                                      ? Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type))
                                      : (Expression)Expression.Constant(true);

        // Perform the greater than or equal comparison
        Expression comparisonExpression;

        if (propertyExpression.Type.IsNullableType())
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            comparisonExpression = Expression.LessThan(nullableValueExpression, constantExpression);
        }
        else
        {
            comparisonExpression = Expression.LessThan(propertyExpression, constantExpression);
        }

        var finalExpression = Expression.AndAlso(nullCheckExpression, comparisonExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberLessThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName) {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem.Value, propertyTypeName);

        var finalExpression = GetNumberLessThanOrEqualExpression(propertyExpression, constantExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    private static Expression GetNumberLessThanOrEqualExpression(
        MemberExpression propertyExpression,
        ConstantExpression constantExpression
    ) {
        // Handle null check
        var nullCheckExpression = propertyExpression.Type.IsNullableType()
            ? Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type))
            : (Expression)Expression.Constant(true);

        // Perform the greater than or equal comparison
        Expression comparisonExpression;

        if (propertyExpression.Type.IsNullableType())
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            comparisonExpression = Expression.LessThanOrEqual(nullableValueExpression, constantExpression);
        }
        else
        {
            comparisonExpression = Expression.LessThanOrEqual(propertyExpression, constantExpression);
        }

        var finalExpression = Expression.AndAlso(nullCheckExpression, comparisonExpression);
        return finalExpression;
    }


    public static Expression<Func<TItem, bool>> GetNumberBetweenExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var values = filterItem.Value.Split(Config.FilterBetweenSeparator);
        
        var leftValue = values[0];
        var rightValue = values.Length > 1 ? values[1] : leftValue;
        
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        
        var constantLeftExpression = GetNumberConstantExpression(leftValue, propertyTypeName);
        var constantRightExpression = GetNumberConstantExpression(rightValue, propertyTypeName);

        var finalExpression = Expression.AndAlso(
            GetNumberGreaterThanOrEqualExpression(propertyExpression, constantLeftExpression), 
            GetNumberLessThanOrEqualExpression(propertyExpression, constantRightExpression)
            );

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem.Value, propertyTypeName);

        // Handle null check
        var nullCheckExpression = propertyExpression.Type.IsNullableType()
                                      ? Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type))
                                      : (Expression)Expression.Constant(true);

        // Perform the greater than or equal comparison
        Expression comparisonExpression;

        if (propertyExpression.Type.IsNullableType())
        {
            var nullableValueExpression = Expression.Property(propertyExpression, "Value");
            comparisonExpression = Expression.NotEqual(nullableValueExpression, constantExpression);
        }
        else
        {
            comparisonExpression = Expression.NotEqual(propertyExpression, constantExpression);
        }

        var finalExpression = Expression.AndAlso(nullCheckExpression, comparisonExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringContainsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Contains method
        var methodInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) });
        var containsExpression = Expression.Call(propertyExp, methodInfo!, someValue, comparisonExpression);

        // Combine null check and contains expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, containsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringDoesNotContainExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Contains method
        var methodInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) });
        var containsExpression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);
        var notContainsExpression = Expression.Not(containsExpression);

        // Combine null check and not contains expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, notContainsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringEndsWithExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string), typeof(StringComparison) });
        var equalsExpression = Expression.Call(propertyExp, methodInfo!, someValue, comparisonExpression);

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, equalsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        var equalsExpression = Expression.Call(propertyExp, methodInfo!, someValue, comparisonExpression);

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, equalsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringNotEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        var equalsExpression = Expression.Call(propertyExp, methodInfo!, someValue, comparisonExpression);
        var notEqualsExpression = Expression.Not(equalsExpression);

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, notEqualsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetStringStartsWithExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Property(parameterExpression, filterItem.PropertyName);
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string), typeof(StringComparison) });
        var equalsExpression = Expression.Call(propertyExp, methodInfo!, someValue, comparisonExpression);

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, equalsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }
    
    public static string EnumToString(Enum e) {
        return e.ToString();
    }
    public static Expression<Func<TItem, bool>> GetEnumContainsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Convert(
            Expression.Convert(Expression.Property(parameterExpression, filterItem.PropertyName), typeof(Enum)), 
            typeof(string),
            typeof(ExpressionExtensions).GetMethod(nameof(EnumToString), new [] {typeof(Enum)})
        );
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Contains method
        var methodInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) });
        var containsExpression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);

        // Combine null check and contains expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, containsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetEnumDoesNotContainExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Convert(
            Expression.Convert(Expression.Property(parameterExpression, filterItem.PropertyName), typeof(Enum)), 
            typeof(string),
            typeof(ExpressionExtensions).GetMethod(nameof(EnumToString), new [] {typeof(Enum)})
        );
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Contains method
        var methodInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) });
        var containsExpression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);
        var notContainsExpression = Expression.Not(containsExpression);

        // Combine null check and not contains expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, notContainsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetEnumEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Convert(
            Expression.Convert(Expression.Property(parameterExpression, filterItem.PropertyName), typeof(Enum)), 
            typeof(string),
            typeof(ExpressionExtensions).GetMethod(nameof(EnumToString), new [] {typeof(Enum)})
        );
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        var equalsExpression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, equalsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }
    
    public static Expression<Func<TItem, bool>> GetEnumNotEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Convert(
            Expression.Convert(Expression.Property(parameterExpression, filterItem.PropertyName), typeof(Enum)), 
            typeof(string),
            typeof(ExpressionExtensions).GetMethod(nameof(EnumToString), new [] {typeof(Enum)})
        );
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        var equalsExpression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);
        var notEqualsExpression = Expression.Not(equalsExpression);

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, notEqualsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }
    
    public static Expression<Func<TItem, bool>> GetEnumInExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Convert(
            Expression.Convert(Expression.Property(parameterExpression, filterItem.PropertyName), typeof(Enum)), 
            typeof(string),
            typeof(ExpressionExtensions).GetMethod(nameof(EnumToString), new [] {typeof(Enum)})
        );
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        var values = filterItem.Value.Split(',');
        
        Expression equalsExpression = Expression.Constant(false);
        foreach(var value in values) {
            var someValue = Expression.Constant(value, typeof(string));
            var equalsExpressionPart = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);
            equalsExpression = Expression.Or(equalsExpression, equalsExpressionPart);
        }

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, equalsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetGuidEqualsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Convert(
            Expression.Convert(Expression.Property(parameterExpression, filterItem.PropertyName), typeof(Guid)), 
            typeof(string),
            typeof(ExpressionExtensions).GetMethod(nameof(GuidToString), new [] {typeof(Guid)})
        );
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Equals method
        var methodInfo = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        var equalsExpression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, equalsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }


    public static string GuidToString(Guid e) {
        return e.ToString();
    }
    public static Expression<Func<TItem, bool>> GetGuidContainsExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyExp = Expression.Convert(
            Expression.Convert(Expression.Property(parameterExpression, filterItem.PropertyName), typeof(Guid)), 
            typeof(string),
            typeof(ExpressionExtensions).GetMethod(nameof(GuidToString), new [] {typeof(Guid)})
        );
        var someValue = Expression.Constant(filterItem.Value, typeof(string));
        var comparisonExpression = Expression.Constant(filterItem.StringComparison);

        // Handle null check
        var nullCheckExpression = Expression.NotEqual(propertyExp, Expression.Constant(null, typeof(string)));

        // Create method call expression for Contains method
        var methodInfo = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) });
        var containsExpression = Expression.Call(propertyExp, methodInfo, someValue, comparisonExpression);

        // Combine null check and contains expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, containsExpression);

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }


    public static Expression<Func<TItem, bool>> GetEmptyExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        return Expression.Lambda<Func<TItem, bool>>(Expression.Constant(true), parameterExpression);
    }



    public static bool IsNullableType(this Type type) => Nullable.GetUnderlyingType(type) != null;

    public static Expression<Func<TItem, bool>> Or<TItem>(this Expression<Func<TItem, bool>> leftExpression, Expression<Func<TItem, bool>> rightExpression)
    {
        var parameterExpression = leftExpression.Parameters[0];

        SubstExpressionVisitor substExpressionVisitor = new();
        substExpressionVisitor.subst[rightExpression.Parameters[0]] = parameterExpression;

        Expression body = Expression.OrElse(leftExpression.Body, substExpressionVisitor.Visit(rightExpression.Body));

        return Expression.Lambda<Func<TItem, bool>>(body, parameterExpression);
    }

    #endregion
}

internal class SubstExpressionVisitor : ExpressionVisitor
{
    #region Fields and Constants

    public Dictionary<Expression, Expression> subst = new();

    #endregion

    #region Methods

    protected override Expression VisitParameter(ParameterExpression parameterExpression) => subst.TryGetValue(parameterExpression, out var newExpression) ? newExpression : parameterExpression;

    #endregion
}
