namespace BlazorBootstrap;

public static class FilterOperatorHelper
{
    public static IEnumerable<FilterOperatorInfo> GetFilterOperators(string propertyTypeName)
    {
        if (propertyTypeName is StringConstants.PropertyTypeNameInt16
            or StringConstants.PropertyTypeNameInt32
            or StringConstants.PropertyTypeNameInt64
            or StringConstants.PropertyTypeNameSingle // float
            or StringConstants.PropertyTypeNameDecimal
            or StringConstants.PropertyTypeNameDouble)
        {
            return GetNumberFilterOperators();
        }
        else if (propertyTypeName is StringConstants.PropertyTypeNameString
            or StringConstants.PropertyTypeNameChar)
        {
            return GetStringFilterOperators();
        }
        else if (propertyTypeName is StringConstants.PropertyTypeNameDateOnly
            or StringConstants.PropertyTypeNameDateTime)
        {
            return GetDateFilterOperators();
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            return GetBooleanFilterOperators();
        }

        return new List<FilterOperatorInfo>();
    }

    public static IEnumerable<FilterOperatorInfo> GetNumberFilterOperators()
    {
        List<FilterOperatorInfo> result = new()
        {
            new("=", "Equals", FilterOperator.Equals),
            new("!=", "Not equals", FilterOperator.NotEquals),
            new("<", "Less than", FilterOperator.LessThan),
            new("<=", "Less than or equals", FilterOperator.LessThanOrEquals),
            new(">", "Greater than", FilterOperator.GreaterThan),
            new(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals),
            new("x", "Clear", FilterOperator.Equals)
        };

        return result;
    }

    public static IEnumerable<FilterOperatorInfo> GetStringFilterOperators()
    {
        List<FilterOperatorInfo> result = new()
        {
            new("*a*", "Contains", FilterOperator.Contains),
            //result.Add(new("!*a*", "Does not contain", FilterOperator.DoesNotContain));
            new("a**", "Starts with", FilterOperator.StartsWith),
            new("**a", "Ends with", FilterOperator.EndsWith),
            //result.Add(new("=''", "Is empty", FilterOperator.IsEmpty));
            //result.Add(new("!=''", "Is not empty", FilterOperator.IsNotEmpty));
            new("=", "Equals", FilterOperator.Equals),
            //result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
            //result.Add(new("null", "Is null", FilterOperator.IsNull));
            //result.Add(new("!null", "Is not null", FilterOperator.IsNotNull));
            new("x", "Clear", FilterOperator.Contains)
        };

        return result;
    }

    public static IEnumerable<FilterOperatorInfo> GetDateFilterOperators()
    {
        List<FilterOperatorInfo> result = new()
        {
            new("=", "Equals", FilterOperator.Equals),
            new("!=", "Not equals", FilterOperator.NotEquals),
            new("<", "Less than", FilterOperator.LessThan),
            new("<=", "Less than or equals", FilterOperator.LessThanOrEquals),
            new(">", "Greater than", FilterOperator.GreaterThan),
            new(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals),
            new("x", "Clear", FilterOperator.Equals)
        };

        return result;
    }

    public static IEnumerable<FilterOperatorInfo> GetBooleanFilterOperators()
    {
        List<FilterOperatorInfo> result = new()
        {
            new("=", "Equals", FilterOperator.Equals),
            new("!=", "Not equals", FilterOperator.NotEquals),
            new("x", "Clear", FilterOperator.Equals)
        };

        return result;
    }
}
