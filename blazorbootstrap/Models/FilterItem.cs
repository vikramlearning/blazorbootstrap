namespace BlazorBootstrap;

public sealed record class FilterItem
{
    public FilterItem(string propertyName, string value, FilterOperator @operator, StringComparison stringComparison)
    {
        PropertyName = propertyName;
        Value = value;
        Operator = @operator;
        StringComparison = stringComparison;
    }

    FilterItem(Type propertType, string propertyName, string value, FilterOperator @operator, StringComparison stringComparison)
    {
        PropertType = propertType;
        PropertyName = propertyName;
        Value = value;
        Operator = @operator;
        StringComparison = stringComparison;
    }

    public Type PropertType { get; private set; }
    public string PropertyName { get; private set; }
    public string Value { get; private set; }
    public FilterOperator Operator { get; private set; }
    public StringComparison StringComparison { get; private set; }
}
