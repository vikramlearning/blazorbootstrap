namespace BlazorBootstrap;

public partial class Offcanvas : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Type? childComponent;

    private DotNetObjectReference<Offcanvas>? objRef;

    private Dictionary<string, object>? parameters;

    private string? title;

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
                    await SafeInvokeVoidAsync("window.blazorBootstrap.offcanvas.dispose", Id);
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
            await SafeInvokeVoidAsync("window.blazorBootstrap.offcanvas.initialize", Id, UseStaticBackdrop, CloseOnEscape, IsScrollable, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        title = Title;

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task bsHiddenOffcanvas() => await OnHidden.InvokeAsync();

    [JSInvokable]
    public async Task bsHideOffcanvas() => await OnHiding.InvokeAsync();

    [JSInvokable]
    public async Task bsShownOffcanvas() => await OnShown.InvokeAsync();

    [JSInvokable]
    public async Task bsShowOffcanvas() => await OnShowing.InvokeAsync();

    /// <summary>
    /// Hides an offcanvas.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Hides an offcanvas.")]
    public async Task HideAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.offcanvas.hide", Id);

    /// <summary>
    /// Shows an offcanvas.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Shows an offcanvas.")]
    public async Task ShowAsync() => await ShowAsync(null, null, null);

    /// <summary>
    /// Opens a offcanvas.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="parameters"></param>
    [AddedVersion("1.0.0")]
    [Description("Opens a offcanvas. T is component.")]
    public async Task ShowAsync<T>(string title, Dictionary<string, object>? parameters = null) => await ShowAsync(title, typeof(T), parameters);

    private async Task ShowAsync(string? title, Type? type, Dictionary<string, object>? parameters)
    {
        if (!string.IsNullOrWhiteSpace(title))
            this.title = title;

        childComponent = type;
        this.parameters = parameters!;
        await SafeInvokeVoidAsync("window.blazorBootstrap.offcanvas.show", Id);
        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Offcanvas, true),
            (Placement.ToOffcanvasPlacementClass(), true),
            (Size.ToOffcanvasSizeClass(), true));

    /// <summary>
    /// Gets or sets the body CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the body CSS class.")]
    [Parameter]
    public string? BodyCssClass { get; set; }

    /// <summary>
    /// Gets or sets the body template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the body template.")]
    [Parameter]
    public RenderFragment? BodyTemplate { get; set; }

    /// <summary>
    /// If <see langword="true" />, offcanvas closes when escape key is pressed.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(true)]
    [Description("If <b>true</b>, offcanvas closes when escape key is pressed.")]
    [Parameter]
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Gets or sets the footer CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the footer CSS class.")]
    [Parameter]
    public string? FooterCssClass { get; set; }

    /// <summary>
    /// Gets or sets the footer template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the footer template.")]
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the header CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the header CSS class.")]
    [Parameter]
    public string? HeaderCssClass { get; set; }

    /// <summary>
    /// Gets or sets the header template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the header template.")]
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Indicates whether body scrolling is allowed while offcanvas is open.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Indicates whether body scrolling is allowed while offcanvas is open.")]
    [Parameter]
    public bool IsScrollable { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).")]
    [Parameter]
    public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event is fired immediately when the hide method has been called.")]
    [Parameter]
    public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires immediately when the show instance method is called.")]
    [Parameter]
    public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).")]
    [Parameter]
    public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets the offcanvas placement.
    /// <para>
    /// Default value is <see cref="Placement.End" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(Placement.End)]
    [Description("Gets or sets the offcanvas placement.")]
    [Parameter]
    public Placement Placement { get; set; } = Placement.End;

    /// <summary>
    /// If <see langword="true" />, modal shows close button in the header.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(true)]
    [Description("If <b>true</b>, modal shows close button in the header.")]
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the offcanvas size.
    /// <para>
    /// Default value is <see cref="OffcanvasSize.Regular" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(OffcanvasSize.Regular)]
    [Description("Gets or sets the offcanvas size.")]
    [Parameter]
    public OffcanvasSize Size { get; set; } = OffcanvasSize.Regular;

    /// <summary>
    /// Gets or sets the tab index.
    /// <para>
    /// Default value is -1.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(-1)]
    [Description("Gets or sets the tab index.")]
    [Parameter]
    public int TabIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the offcanvas title.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the offcanvas title.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// When `UseStaticBackdrop` is set to <see langword="true"/>, the offcanvas will not close when clicking outside of it.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("When `UseStaticBackdrop` is set to <b>true</b>, the offcanvas will not close when clicking outside of it.")]
    [Parameter]
    public bool UseStaticBackdrop { get; set; }

    #endregion
}
