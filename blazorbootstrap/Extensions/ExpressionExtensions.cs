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

    #region Boolean

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

    #endregion Boolean

    #region Date

    public static ConstantExpression GetDateConstantExpression(FilterItem filterItem, string propertyTypeName)
    {
        if (filterItem.Value == null)
            return Expression.Constant(null);

        ConstantExpression? constantExpression = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly)
        {
            _ = DateOnly.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant(filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            _ = DateTime.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant(filterValue);
        }

        return constantExpression!;
    }

    public static Expression<Func<TItem, bool>> GetDateEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem, propertyTypeName);

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
        var dateConstantExpression = GetDateConstantExpression(filterItem, propertyTypeName);

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
        var dateConstantExpression = GetDateConstantExpression(filterItem, propertyTypeName);

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

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateLessThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem, propertyTypeName);

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
        var dateConstantExpression = GetDateConstantExpression(filterItem, propertyTypeName);

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

        return Expression.Lambda<Func<TItem, bool>>(nonNullComparisonExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetDateNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var dateConstantExpression = GetDateConstantExpression(filterItem, propertyTypeName);

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

    #endregion Date

    #region Enum

    public static ConstantExpression GetEnumConstantExpression<TItem>(FilterItem filterItem, Type propertyType, string propertyTypeName)
    {
        ConstantExpression? value = null;

        if (propertyType is not null && propertyType.IsEnum)
        {
            _ = Enum.TryParse(propertyType, filterItem.Value, out object filterValue);
            value = Expression.Constant(filterValue);
        }

        return value!;
    }

    public static Expression<Func<TItem, bool>> GetEnumEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, Type propertyType, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.Equal(property, GetEnumConstantExpression<TItem>(filterItem, propertyType, propertyTypeName));

        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetEnumNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, Type propertyType, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.NotEqual(property, GetEnumConstantExpression<TItem>(filterItem, propertyType, propertyTypeName));

        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    #endregion Enum

    #region Guid

    public static ConstantExpression GetGuidConstantExpression<TItem>(FilterItem filterItem, string propertyTypeName)
    {
        ConstantExpression? value = null;

        _ = Guid.TryParse(filterItem.Value, out Guid filterValue);
        value = Expression.Constant(filterValue);

        return value!;
    }

    public static Expression<Func<TItem, bool>> GetGuidEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.Equal(property, GetGuidConstantExpression<TItem>(filterItem, propertyTypeName));

        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetGuidNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var property = Expression.Property(parameterExpression, filterItem.PropertyName);
        var expression = Expression.NotEqual(property, GetGuidConstantExpression<TItem>(filterItem, propertyTypeName));

        return Expression.Lambda<Func<TItem, bool>>(expression, parameterExpression);
    }

    #endregion Guid

    public static Expression<Func<TItem, bool>>? GetExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem)
    {
        var propertyType = typeof(TItem).GetPropertyType(filterItem.PropertyName);
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
                _ => GetNumberEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName)
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
                _ => GetStringContainsExpressionDelegate<TItem>(parameterExpression, filterItem)
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
                _ => GetDateEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName)
            };

        if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
            return filterItem.Operator switch
            {
                FilterOperator.Equals => GetBooleanEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.NotEquals => GetBooleanNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                _ => GetBooleanEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName)
            };

        // Enum
        if (propertyTypeName == StringConstants.PropertyTypeNameEnum)
            return filterItem.Operator switch
            {
                FilterOperator.Equals => GetEnumEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyType!, propertyTypeName),
                FilterOperator.NotEquals => GetEnumNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyType!, propertyTypeName),
                _ => GetEnumEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyType!, propertyTypeName)
            };

        // Guid
        if (propertyTypeName == StringConstants.PropertyTypeNameGuid)
            return filterItem.Operator switch
            {
                FilterOperator.Equals => GetGuidEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                FilterOperator.NotEquals => GetGuidNotEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName),
                _ => GetGuidEqualExpressionDelegate<TItem>(parameterExpression, filterItem, propertyTypeName)
            };

        return null;
    }

    #region Number

    public static ConstantExpression GetNumberConstantExpression(FilterItem filterItem, string propertyTypeName)
    {
        if (filterItem.Value is null)
            return Expression.Constant(null);

        ConstantExpression? constantExpression = null;

        if (propertyTypeName == StringConstants.PropertyTypeNameInt16)
        {
            _ = short.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant((short?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameInt32)
        {
            _ = int.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant((int?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameInt64)
        {
            _ = long.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant((long?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameSingle)
        {
            _ = float.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant((float?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDecimal)
        {
            _ = decimal.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant((decimal?)filterValue);
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            _ = double.TryParse(filterItem.Value, out var filterValue);
            constantExpression = Expression.Constant((double?)filterValue);
        }

        return constantExpression!;
    }

    public static Expression<Func<TItem, bool>> GetNumberEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem, propertyTypeName);

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
        var constantExpression = GetNumberConstantExpression(filterItem, propertyTypeName);

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

    public static Expression<Func<TItem, bool>> GetNumberGreaterThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem, propertyTypeName);

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

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberLessThanExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem, propertyTypeName);

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

    public static Expression<Func<TItem, bool>> GetNumberLessThanOrEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem, propertyTypeName);

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

        return Expression.Lambda<Func<TItem, bool>>(finalExpression, parameterExpression);
    }

    public static Expression<Func<TItem, bool>> GetNumberNotEqualExpressionDelegate<TItem>(ParameterExpression parameterExpression, FilterItem filterItem, string propertyTypeName)
    {
        var propertyExpression = Expression.Property(parameterExpression, filterItem.PropertyName);
        var constantExpression = GetNumberConstantExpression(filterItem, propertyTypeName);

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

    #endregion Number

    #region String

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
        var containsExpression = Expression.Call(propertyExp, methodInfo!, someValue, comparisonExpression);
        
        // "not contains" expression
        var notContainsExpression = Expression.Not(containsExpression);
        
        // Combine null check and contains expression using AndAlso
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
        var notEqualsExpresion = Expression.Equal(equalsExpression, Expression.Constant(false, typeof(bool)));

        // Combine null check and equals expression using AndAlso
        var finalExpression = Expression.AndAlso(nullCheckExpression, notEqualsExpresion);

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

    #endregion String

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
