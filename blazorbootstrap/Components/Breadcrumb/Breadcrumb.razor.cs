namespace BlazorBootstrap;

/// <summary>
/// Blazor Bootstrap breadcrumb component indicates the current page's location within a navigational hierarchy that automatically adds separators. <br/>
/// This component is based on the <see href="https://getbootstrap.com/docs/5.0/components/breadcrumb/">Bootstrap Breadcrumb</see> component.
/// </summary>
public partial class Breadcrumb : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing) 
            if (BreadcrumbService is not null)
                BreadcrumbService.OnNotify -= OnNotify;

        return base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (BreadcrumbService is not null)
            BreadcrumbService.OnNotify += OnNotify;

        base.OnInitialized();
    }

    private void OnNotify(IReadOnlyCollection<BreadcrumbItem>? items)
    {
        if (items is null)
            return;

        Items = items;

        StateHasChanged();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Dependency injected service for the <see cref="BlazorBootstrap.BreadcrumbService"/>.
    /// </summary>
    [Inject] private BreadcrumbService? BreadcrumbService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public IReadOnlyCollection<BreadcrumbItem>? Items { get; set; } = default!;

    #endregion
}
