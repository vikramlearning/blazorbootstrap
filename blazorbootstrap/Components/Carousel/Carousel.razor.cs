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

    private List<CarouselItem> items = new();

    private DotNetObjectReference<Carousel>? objRef;

    #endregion

    #region Methods

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
            CarouselOptions options = new() { Interval = Interval, Keyboard = Keyboard, Ride = Autoplay.ToCarouselAutoPlayString(), Touch = Touch };
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

    [JSInvokable]
    public async Task bslide(CarouselEventArgs args)
    {
        activeIndex = args.To;
        await Onslide.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task bsSlid(CarouselEventArgs args) => await Onslid.InvokeAsync(args);

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

    internal void AddItem(CarouselItem carouselItem)
    {
        items.Add(carouselItem);

        if (carouselItem.Active)
            activeIndex = items.Count - 1;
    }

    /// <summary>
    /// Shows next <see cref="CarouselItem" />.
    /// </summary>
    public ValueTask PauseCarouselAsync() => JSRuntime.InvokeVoidAsync(CarouselInterop.Pause, Id);

    /// <summary>
    /// Shows next <see cref="CarouselItem" />.
    /// </summary>
    public ValueTask ShowNextItemAsync()
    {
        var nextIndex = activeIndex + 1;
        activeIndex = nextIndex > items.Count - 1 ? 0 : nextIndex;

        return JSRuntime.InvokeVoidAsync(CarouselInterop.Next, Id);
    }

    /// <summary>
    /// Shows previous <see cref="CarouselItem" />.
    /// </summary>
    public ValueTask ShowPreviousItemAsync()
    {
        var previousIndex = activeIndex - 1;
        activeIndex = previousIndex < 0 ? items.Count - 1 : previousIndex;

        return JSRuntime.InvokeVoidAsync(CarouselInterop.Previous, Id);
    }

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
    /// Controls the autoplay behavior of the carousel.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CarouselAutoPlay.None" />.
    /// </remarks>
    [Parameter]
    public CarouselAutoPlay Autoplay { get; set; } = CarouselAutoPlay.None;

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
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Crossfade { get; set; }

    private bool HasItems => items.Any();

    /// <summary>
    /// The amount of time to delay between automatically cycling an item.
    /// </summary>
    /// <remarks>
    /// Default value is 5000 milliseconds.
    /// </remarks>
    [Parameter]
    public int? Interval { get; set; } = 5000;

    private int ItemCount => items.Count;

    /// <summary>
    /// Whether the carousel should react to keyboard events.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter]
    public bool Keyboard { get; set; } = true;

    /// <summary>
    /// Fired when the carousel has completed its slide transition.
    /// </summary>
    [Parameter]
    public EventCallback<CarouselEventArgs> Onslid { get; set; }

    /// <summary>
    /// Fires immediately when the slide instance method is invoked.
    /// </summary>
    [Parameter]
    public EventCallback<CarouselEventArgs> Onslide { get; set; }

    /// <summary>
    /// Indicates whether to show indicators (dots) below the carousel to navigate between slides.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool ShowIndicators { get; set; }

    /// <summary>
    /// Specifies whether to display the previous and next controls (arrows) for navigating slides.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter]
    public bool ShowPreviousNextControls { get; set; } = true;

    /// <summary>
    /// Carousels support swiping left/right on touchscreen devices to move between slides.
    /// This can be disabled by setting the <see cref="Touch" /> parameter to <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter]
    public bool Touch { get; set; } = true;

    #endregion
}
