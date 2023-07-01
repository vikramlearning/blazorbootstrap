namespace BlazorBootstrap;

public class BreadcrumbService
{
    internal event Action<List<BreadcrumbItem>> OnNotify;

    public void Notify(List<BreadcrumbItem> items) => OnNotify?.Invoke(items);
}
