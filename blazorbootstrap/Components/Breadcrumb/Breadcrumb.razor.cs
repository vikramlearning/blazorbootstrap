namespace BlazorBootstrap;

public partial class Breadcrumb : BlazorBootstrapComponentBase
{
    #region Methods

    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
            if (BreadcrumbService is not null)
                BreadcrumbService.OnNotify -= OnNotify;

        return base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        if (BreadcrumbService is not null)
            BreadcrumbService.OnNotify += OnNotify;

        base.OnInitialized();
    }

    private void OnNotify(List<BreadcrumbItem> items)
    {
        if (items is null)
            return;

        Items ??= new List<BreadcrumbItem>();

        Items = items;

        StateHasChanged();
    }

    #endregion

    #region Properties, Indexers

    [Inject] private BreadcrumbService BreadcrumbService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public List<BreadcrumbItem> Items { get; set; } = default!;

    #endregion
}
