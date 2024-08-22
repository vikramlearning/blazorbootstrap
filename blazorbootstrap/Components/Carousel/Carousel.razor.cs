namespace BlazorBootstrap;

public partial class Carousel : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<Carousel>? objRef;

    #endregion

    #region Methods

    /// <inheritdoc />
    //protected override async ValueTask DisposeAsyncCore(bool disposing)
    //{
    //    if (disposing)
    //    {
    //        try
    //        {
    //            if (IsRenderComplete)
    //                await JSRuntime.InvokeVoidAsync(CarouselInterop.Dispose, Id);
    //        }
    //        catch (JSDisconnectedException)
    //        {
    //            // do nothing
    //        }

    //        objRef?.Dispose();
    //    }

    //    await base.DisposeAsyncCore(disposing);
    //}

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender)
    //        await JSRuntime.InvokeVoidAsync(CarouselInterop.Initialize, Id, objRef);

    //    await base.OnAfterRenderAsync(firstRender);
    //}

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Carousel, true),
            (BootstrapClass.CarouselSlide, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
