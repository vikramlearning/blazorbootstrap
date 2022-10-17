namespace BlazorBootstrap;

public static class FilterOperatorHelper
{
    public static IEnumerable<FilterOperatorInfo> GetFilterOperators(string propertyTypeName)
    {
        if (propertyTypeName == StringConstants.PropertyTypeNameInt16
            || propertyTypeName == StringConstants.PropertyTypeNameInt32
            || propertyTypeName == StringConstants.PropertyTypeNameInt64
            || propertyTypeName == StringConstants.PropertyTypeNameSingle // float
            || propertyTypeName == StringConstants.PropertyTypeNameDecimal
            || propertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            return GetNumberFilterOperators();
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameString
            || propertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            return GetStringFilterOperators();
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            return GetDateFilterOperators();
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            return GetBooleanFilterOperators();
        }

        return null;
    }

    public static IEnumerable<FilterOperatorInfo> GetNumberFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("=", "Equals", FilterOperator.Equals));
        result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        result.Add(new("<", "Less than", FilterOperator.LessThan));
        result.Add(new("<=", "Less than or equals", FilterOperator.LessThanOrEquals));
        result.Add(new(">", "Greater than", FilterOperator.GreaterThan));
        result.Add(new(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals));
        result.Add(new("x", "Clear", FilterOperator.Equals));

        return result;
    }

    public static IEnumerable<FilterOperatorInfo> GetStringFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("*a*", "Contains", FilterOperator.Contains));
        //result.Add(new("!*a*", "Does not contain", FilterOperator.DoesNotContain));
        result.Add(new("a**", "Starts with", FilterOperator.StartsWith));
        result.Add(new("**a", "Ends with", FilterOperator.EndsWith));
        //result.Add(new("=''", "Is empty", FilterOperator.IsEmpty));
        //result.Add(new("!=''", "Is not empty", FilterOperator.IsNotEmpty));
        result.Add(new("=", "Equals", FilterOperator.Equals));
        //result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        //result.Add(new("null", "Is null", FilterOperator.IsNull));
        //result.Add(new("!null", "Is not null", FilterOperator.IsNotNull));
        result.Add(new("x", "Clear", FilterOperator.Contains));

        return result;
    }

    public static IEnumerable<FilterOperatorInfo> GetDateFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("=", "Equals", FilterOperator.Equals));
        result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        result.Add(new("<", "Less than", FilterOperator.LessThan));
        result.Add(new("<=", "Less than or equals", FilterOperator.LessThanOrEquals));
        result.Add(new(">", "Greater than", FilterOperator.GreaterThan));
        result.Add(new(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals));
        result.Add(new("x", "Clear", FilterOperator.Equals));

        return result;
    }

    public static IEnumerable<FilterOperatorInfo> GetBooleanFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("=", "Equals", FilterOperator.Equals));
        result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        result.Add(new("x", "Clear", FilterOperator.Equals));

        return result;
    }
}
