namespace BlazorBootstrap;

/// <summary>
/// Data provider (delegate).
/// </summary>
public delegate Task<AutoCompleteDataProviderResult<TItem>> AutoCompleteDataProviderDelegate<TItem>(AutoCompleteDataProviderRequest<TItem> request);