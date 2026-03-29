namespace BlazorBootstrap;

/// <summary>
/// Provides a simple publish/subscribe mechanism to update `Breadcrumb` items from anywhere in the application.
/// </summary>
public class BreadcrumbService
{
    #region Events

    /// <summary>
    /// Occurs when breadcrumb items are updated via <see cref="Notify(List{BreadcrumbItem})" />.
    /// </summary>
    [AddedVersion("1.9.2")]
    [Description("Occurs when breadcrumb items are updated via Notify(List<BreadcrumbItem>).")]
    internal event Action<List<BreadcrumbItem>> OnNotify = default!;

    #endregion

    #region Methods

    /// <summary>
    /// Notifies subscribers with the latest breadcrumb items.
    /// </summary>
    /// <param name="items">The breadcrumb items.</param>
    [AddedVersion("1.9.2")]
    [Description("Notifies subscribers with the latest breadcrumb items.")]
    [MethodName("Notify(List<BreadcrumbItem> items)")]
    public void Notify(List<BreadcrumbItem> items) => OnNotify?.Invoke(items);

    #endregion
}
