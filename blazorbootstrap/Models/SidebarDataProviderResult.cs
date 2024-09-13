namespace BlazorBootstrap;

public class SidebarDataProviderResult
{
    #region Properties, Indexers

    /// <summary>
    /// The provided items by the request.
    /// </summary>
    public IReadOnlyCollection<NavItem>? Data { get; init; }

    #endregion
}
