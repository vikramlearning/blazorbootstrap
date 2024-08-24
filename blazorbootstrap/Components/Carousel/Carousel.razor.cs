namespace BlazorBootstrap;

public partial class Carousel : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// Represents active <see cref="CarouselItem" /> index.
    /// </summary>
    private int activeIndex = 0;

    /// <summary>
    /// Determines whether the default active <see cref="CarouselItem" /> set.
    /// </summary>
    private bool isDefaultActiveCarouselItemSet = false;

    private DotNetObjectReference<Carousel>? objRef;
    private List<CarouselItem> items = new();
    private bool HasItems => items.Any();
    private int ItemCount => items.Count;

    #endregion

    #region Methods

    internal void AddItem(CarouselItem carouselItem)
    {
        items.Add(carouselItem);

        if (carouselItem.Active)
            activeIndex = items.Count - 1;
    }

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
        if (firstRender)
        {
            CarouselOptions options = new()
            {
                Interval = Interval,
                Keyboard = Keyboard,
                Ride = Autoplay.ToCarouselAutoPlayString(),
                Touch = Touch,
            };
            await JSRuntime.InvokeVoidAsync(CarouselInterop.Initialize, Id, options, objRef);
            StateHasChanged(); // Required
        }

        // Set active tab
        if (firstRender && !isDefaultActiveCarouselItemSet)
            await ShowItemByIndexAsync(activeIndex);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Shows <see cref="CarouselItem" /> by index.
    /// </summary>
    /// <param name="index"></param>
    public ValueTask ShowItemByIndexAsync(int index)
    {
        if (!isDefaultActiveCarouselItemSet)
            isDefaultActiveCarouselItemSet = true;

        return JSRuntime.InvokeVoidAsync(CarouselInterop.To, Id, index);
    }

    /// <summary>
    /// Shows next <see cref="CarouselItem" />.
    /// </summary>
    private ValueTask ShowNextItemAsync()
    {
        var nextIndex = activeIndex + 1;
        activeIndex = (nextIndex > items.Count - 1) ? 0 : nextIndex;

        return JSRuntime.InvokeVoidAsync(CarouselInterop.Next, Id);
    }

    /// <summary>
    /// Shows next <see cref="CarouselItem" />.
    /// </summary>
    private ValueTask PauseCarouselAsync() => JSRuntime.InvokeVoidAsync(CarouselInterop.Pause, Id);

    /// <summary>
    /// Shows previous <see cref="CarouselItem" />.
    /// </summary>
    private ValueTask ShowPreviousItemAsync()
    {
        var previousIndex = activeIndex - 1;
        activeIndex = (previousIndex < 0) ? items.Count - 1 : previousIndex;

        return JSRuntime.InvokeVoidAsync(CarouselInterop.Previous, Id);
    }

    [JSInvokable]
    public async Task bsSlid(CarouselEventArgs args) => await Onslid.InvokeAsync(args);

    /// <summary>
    /// Fired when the carousel has completed its slide transition.
    /// </summary>
    [Parameter]
    public EventCallback<CarouselEventArgs> Onslid { get; set; }

    [JSInvokable]
    public async Task bslide(CarouselEventArgs args)
    {
        activeIndex = args.To;
        await Onslide.InvokeAsync(args);
    }

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

    /// <summary>
    /// Determines whether to use a crossfade effect when transitioning between slides.
    /// </summary>
    [Parameter]
    public bool Crossfade { get; set; }

    /// <summary>
    /// Indicates whether to show indicators (dots) below the carousel to navigate between slides.
    /// </summary>
    [Parameter]
    public bool ShowIndicators { get; set; }

    /// <summary>
    /// Specifies whether to display the previous and next controls (arrows) for navigating slides.
    /// </summary>
    [Parameter]
    public bool ShowPreviousNextControls { get; set; } = true;

    /// <summary>
    /// Controls the autoplay behavior of the carousel.
    /// </summary>
    [Parameter]
    public CarouselAutoPlay Autoplay { get; set; }

    /// <summary>
    /// Carousels support swiping left/right on touchscreen devices to move between slides. 
    /// This can be disabled by setting the <see cref="Touch" /> option to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Touch { get; set; } = true;

    /// <summary>
    /// The amount of time to delay between automatically cycling an item.
    /// </summary>
    /// <remarks>
    /// Default value is 5000.
    /// </remarks>
    [Parameter]
    public int? Interval { get; set; } = 5000;

    /// <summary>
    /// Whether the carousel should react to keyboard events.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    [Parameter]
    public bool Keyboard { get; set; } = true;

    #endregion
}
