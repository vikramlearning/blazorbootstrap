namespace BlazorBootstrap;

public class BreadcrumbService
{
    #region Events

    internal event Action<IReadOnlyCollection<BreadcrumbItem>> OnNotify = default!;

    #endregion

    #region Methods

    public void Notify(IReadOnlyCollection<BreadcrumbItem> items) => OnNotify?.Invoke(items);

    #endregion
}
