namespace BlazorBootstrap;

public partial class Breadcrumb : BaseComponent
{
    #region Members

    #endregion Members

    #region Methods

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

        if (Items is null)
            Items = new();

        Items = items;

        StateHasChanged();
    }

    protected override ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            if (BreadcrumbService is not null)
                BreadcrumbService.OnNotify -= OnNotify;
        }

        return base.DisposeAsync(disposing);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Inject] BreadcrumbService BreadcrumbService { get; set; } = default!;

    /// <summary>
    /// List of all the items.
    /// </summary>
    [Parameter] public List<BreadcrumbItem> Items { get; set; } = default!;

    #endregion Properties
}
