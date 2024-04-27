namespace BlazorBootstrap;

public class BreadcrumbService
{
    #region Events

    internal event Action<List<BreadcrumbItem>> OnNotify = default!;

    #endregion

    #region Methods

    public void Notify(List<BreadcrumbItem> items) => OnNotify?.Invoke(items);

    #endregion
}
