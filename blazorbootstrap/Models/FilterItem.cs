namespace BlazorBootstrap;

public sealed record class FilterItem(string PropertyName, string Value, FilterOperator Operator);