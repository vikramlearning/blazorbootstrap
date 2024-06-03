namespace BlazorBootstrap;

public sealed record FilterItem(
    string PropertyName,
    string Value,
    FilterOperator Operator,
    StringComparison StringComparison)
{
    public string PropertyName { get; private set; } = PropertyName;
    public string Value { get; private set; } = Value;
    public FilterOperator Operator { get; private set; } = Operator;
    public StringComparison StringComparison { get; private set; } = StringComparison;
}
