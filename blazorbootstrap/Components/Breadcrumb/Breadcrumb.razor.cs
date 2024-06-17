
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
        if (disposing && BreadcrumbService is not null)
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


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Items): Items = (IReadOnlyCollection<BreadcrumbItem>?)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;

                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
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
