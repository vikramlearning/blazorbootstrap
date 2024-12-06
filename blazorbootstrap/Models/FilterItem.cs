namespace BlazorBootstrap;

/// <summary>
/// Filter settings for Grids and other collection constructs. 
/// </summary>
/// <param name="PropertyName">Name of the property to filter on</param>
/// <param name="Value">Value to filter on</param>
/// <param name="Operator">Type of filter to apply</param>
/// <param name="StringComparison">How to apply string comparisons (if applicable)</param>
public sealed record FilterItem(
    string PropertyName,
    string Value,
    FilterOperator Operator,
    StringComparison StringComparison)
{
    /// <summary>
    /// Name of the property to filter on
    /// </summary>
    public string PropertyName { get; private set; } = PropertyName;
    /// <summary>
    /// Value to filter on
    /// </summary>
    public string Value { get; private set; } = Value;
    /// <summary>
    /// Type of filter to apply 
    /// </summary>
    public FilterOperator Operator { get; private set; } = Operator;
    /// <summary>
    /// How to apply string comparisons 
    /// </summary>
    public StringComparison StringComparison { get; private set; } = StringComparison;
}
