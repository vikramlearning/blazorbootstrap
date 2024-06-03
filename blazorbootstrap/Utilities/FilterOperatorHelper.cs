namespace BlazorBootstrap;

public static class FilterOperatorHelper
{
    #region Methods

    public static IReadOnlyCollection<FilterOperatorInfo> GetBooleanFilterOperators()
    {
        return new List<FilterOperatorInfo>()
        {
            new ("=", "Equals", FilterOperator.Equals), 
            new ("!=", "Not equals", FilterOperator.NotEquals), 
            new ("x", "Clear", FilterOperator.Clear)
        };
    }

    public static IReadOnlyCollection<FilterOperatorInfo> GetDateFilterOperators()
    {
return new List<FilterOperatorInfo>()
                                          {
                                              new("=", "Equals", FilterOperator.Equals),
                                              new("!=", "Not equals", FilterOperator.NotEquals),
                                              new("<", "Less than", FilterOperator.LessThan),
                                              new("<=", "Less than or equals", FilterOperator.LessThanOrEquals),
                                              new(">", "Greater than", FilterOperator.GreaterThan),
                                              new(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals),
                                              new("x", "Clear", FilterOperator.Clear)
                                          };
    }

    public static IReadOnlyCollection<FilterOperatorInfo> GetEnumFilterOperators()
    {
        return new List<FilterOperatorInfo>()
                                          {
                                              new ("=", "Equals", FilterOperator.Equals),
                                              new ("!=", "Not equals", FilterOperator.NotEquals),
                                              new ("x", "Clear", FilterOperator.Clear)
                                          };
    }

    public static IReadOnlyCollection<FilterOperatorInfo> GetFilterOperators(string propertyTypeName, IReadOnlyCollection<FilterOperatorInfo>? filtersTranslations)
    {
        if (filtersTranslations is null || filtersTranslations.Count == 0)
            return GetFilterOperators(propertyTypeName);

        var filters = new List<FilterOperatorInfo>();
        var defaultFilters = GetFilterOperators(propertyTypeName);

        foreach (var filter in defaultFilters)
        {
            var filterTranslation = filtersTranslations.FirstOrDefault(x => x.FilterOperator == filter.FilterOperator);

            if (filterTranslation is null)
                filters.Add(filter);
            else
                filters.Add(filter with { Symbol = filterTranslation.Symbol, Text = filterTranslation.Text });
        }

        return filters;
    }

    public static IReadOnlyCollection<FilterOperatorInfo> GetFilterOperators(string propertyTypeName)
    {
        if (propertyTypeName is StringConstants.PropertyTypeNameInt16
                                or StringConstants.PropertyTypeNameInt32
                                or StringConstants.PropertyTypeNameInt64
                                or StringConstants.PropertyTypeNameSingle // float
                                or StringConstants.PropertyTypeNameDecimal
                                or StringConstants.PropertyTypeNameDouble)
            return GetNumberFilterOperators();

        if (propertyTypeName is StringConstants.PropertyTypeNameString
                                or StringConstants.PropertyTypeNameChar)
            return GetStringFilterOperators();

        if (propertyTypeName is StringConstants.PropertyTypeNameDateOnly
                                or StringConstants.PropertyTypeNameDateTime)
            return GetDateFilterOperators();

        if (propertyTypeName == StringConstants.PropertyTypeNameBoolean) return GetBooleanFilterOperators();

        if (propertyTypeName == StringConstants.PropertyTypeNameEnum) return GetEnumFilterOperators();

        if (propertyTypeName == StringConstants.PropertyTypeNameGuid) return GetEnumFilterOperators();

        return new List<FilterOperatorInfo>();
    }

    public static IReadOnlyCollection<FilterOperatorInfo> GetGuidFilterOperators()
    {
        List<FilterOperatorInfo> result = new()
                                          {
                                              new FilterOperatorInfo("=", "Equals", FilterOperator.Equals),
                                              new FilterOperatorInfo("!=", "Not equals", FilterOperator.NotEquals),
                                              new FilterOperatorInfo("x", "Clear", FilterOperator.Clear)
                                          };

        return result;
    }

    public static IReadOnlyCollection<FilterOperatorInfo> GetNumberFilterOperators()
    {
        List<FilterOperatorInfo> result = new()
                                          {
                                              new FilterOperatorInfo("=", "Equals", FilterOperator.Equals),
                                              new FilterOperatorInfo("!=", "Not equals", FilterOperator.NotEquals),
                                              new FilterOperatorInfo("<", "Less than", FilterOperator.LessThan),
                                              new FilterOperatorInfo("<=", "Less than or equals", FilterOperator.LessThanOrEquals),
                                              new FilterOperatorInfo(">", "Greater than", FilterOperator.GreaterThan),
                                              new FilterOperatorInfo(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals),
                                              new FilterOperatorInfo("x", "Clear", FilterOperator.Clear)
                                          };

        return result;
    }

    public static IReadOnlyCollection<FilterOperatorInfo> GetStringFilterOperators()
    {
        List<FilterOperatorInfo> result = new()
                                          {
                                              new FilterOperatorInfo("*a*", "Contains", FilterOperator.Contains),
                                              new FilterOperatorInfo("!*a*", "Does not contain", FilterOperator.DoesNotContain),
                                              new FilterOperatorInfo("a**", "Starts with", FilterOperator.StartsWith),
                                              new FilterOperatorInfo("**a", "Ends with", FilterOperator.EndsWith),
                                              //result.Add(new("=''", "Is empty", FilterOperator.IsEmpty));
                                              //result.Add(new("!=''", "Is not empty", FilterOperator.IsNotEmpty));
                                              new FilterOperatorInfo("=", "Equals", FilterOperator.Equals),
                                              new FilterOperatorInfo("!=", "Not equals", FilterOperator.NotEquals),
                                              //result.Add(new("null", "Is null", FilterOperator.IsNull));
                                              //result.Add(new("!null", "Is not null", FilterOperator.IsNotNull));
                                              new FilterOperatorInfo("x", "Clear", FilterOperator.Clear)
                                          };

        return result;
    }

    #endregion
}
