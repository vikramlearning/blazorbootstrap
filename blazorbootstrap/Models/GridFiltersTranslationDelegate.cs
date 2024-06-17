namespace BlazorBootstrap;

/// <summary>
/// Grid filters translation provider (delegate).
/// </summary>
public delegate Task<IReadOnlyCollection<FilterOperatorInfo>?> GridFiltersTranslationDelegate();
