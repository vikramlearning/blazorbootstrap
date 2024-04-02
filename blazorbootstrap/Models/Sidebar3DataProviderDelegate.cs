

namespace BlazorBootstrap;
/// <summary>
/// Data provider (delegate).
/// </summary>
public delegate Task<Sidebar3DataProviderResult> Sidebar3DataProviderDelegate(Sidebar3DataProviderRequest request);

public class Sidebar3DataProviderResult
{
    #region Properties, Indexers

    /// <summary>
    /// The provided items by the request.
    /// </summary>
    public IEnumerable<Sidebar3NavItem>? Data { get; init; }

    #endregion
}
