namespace BlazorBootstrap;

public partial class Carousel : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<Carousel>? objRef;
    private List<CarouselItem> items = new();
    private bool HasItems => items.Any();
    private int ItemCount => items.Count;

    #endregion

    #region Methods

    internal void AddItem(CarouselItem carouselItem) => items.Add(carouselItem);

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (IsRenderComplete)
                    await JSRuntime.InvokeVoidAsync(CarouselInterop.Dispose, Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        CarouselOptions options = new();

        if (firstRender)
            await JSRuntime.InvokeVoidAsync(CarouselInterop.Initialize, Id, options, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine($"START: Carousel.OnInitializedAsync() called");
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();
        Console.WriteLine($"END: Carousel.OnInitializedAsync() called");
    }

    [JSInvokable]
    public async Task bsSlid(CarouselEventArgs args) => await Onslid.InvokeAsync(args);

    /// <summary>
    /// Fired when the carousel has completed its slide transition.
    /// </summary>
    [Parameter]
    public EventCallback<CarouselEventArgs> Onslid { get; set; }

    [JSInvokable]
    public async Task bslide(CarouselEventArgs args) => await Onslide.InvokeAsync(args);

    /// <summary>
    /// Fires immediately when the slide instance method is invoked.
    /// </summary>
    [Parameter]
    public EventCallback<CarouselEventArgs> Onslide { get; set; }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.Carousel, true),
            (BootstrapClass.CarouselSlide, true),
            (BootstrapClass.CarouselFade, Crossfade)
        );

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Crossfade { get; set; }

    [Parameter]
    public bool ShowIndicators { get; set; }

    [Parameter]
    public bool ShowPreviousNextControls { get; set; } = true;

    #endregion
}

public class CarouselEventArgs : EventArgs
{
    public string? Direction { get; set; }
    public int From { get; set; }
    public int To { get; set; }
}