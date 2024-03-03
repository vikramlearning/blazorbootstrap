namespace BlazorBootstrap;

public class TreeViewDataProviderResult
{
    #region Properties, Indexers

    /// <summary>
    /// The provided items by the request.
    /// </summary>
    public IEnumerable<NavItem>? Data { get; init; }

    #endregion
}
