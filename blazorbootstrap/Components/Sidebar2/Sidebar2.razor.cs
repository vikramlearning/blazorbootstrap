namespace BlazorBootstrap;

public partial class Sidebar2 : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool collapseNavMenu = true;

    private bool collapseSidebar = false;

    private bool isMobile = false;

    private IEnumerable<NavItem>? items = null;

    private DotNetObjectReference<Sidebar2> objRef = default!;

    private bool requestInProgress = false;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.sidebar.initialize", Id, objRef);

            var width = await JSRuntime.InvokeAsync<int>("window.blazorBootstrap.sidebar.windowSize");

            bsWindowResize(width);

            await RefreshDataAsync(firstRender);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        AdditionalAttributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public void bsWindowResize(int width)
    {
        if (width < 641) // mobile
            isMobile = true;
        else
            isMobile = false;
    }

    /// <summary>
    /// Refresh the <see cref="Sidebar2"/> data.
    /// </summary>
    /// <returns>Task</returns>
    [AddedVersion("2.1.0")]
    [Description("Refresh the <b>Sidebar2</b> data.")]
    public async Task RefreshDataAsync(bool firstRender = false)
    {
        if (requestInProgress)
            return;

        requestInProgress = true;

        if (DataProvider != null)
        {
            var request = new Sidebar2DataProviderRequest();
            var result = await DataProvider.Invoke(request);

            items = result != null ? result.Data : new List<NavItem>();
        }

        requestInProgress = false;

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Toggles <see cref="Sidebar2"/>.
    /// </summary>
    [AddedVersion("2.1.0")]
    [Description("Toggles <b>Sidebar2</b>.")]
    public void ToggleSidebar()
    {
        collapseSidebar = !collapseSidebar;
        StateHasChanged();
    }

    internal void HideNavMenuOnMobile()
    {
        if (isMobile && !collapseNavMenu)
            collapseNavMenu = true;
    }

    private string GetNavMenuCssClass()
    {
        var classList = new HashSet<string>();

        if (collapseNavMenu)
            classList.Add("collapse");

        classList.Add("bb-sidebar2-content nav-scrollable bb-scrollbar");

        if (collapseSidebar)
            classList.Add("bb-scrollbar-hidden");

        return string.Join(" ", classList);
    }

    private void ToggleNavMenu() => collapseNavMenu = !collapseNavMenu;

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-sidebar2", true),
            ("collapsed", collapseSidebar),
            ("expanded", !collapseSidebar));

    protected override string? StyleNames =>
        BuildStyleNames(Style,
            ($"--bb-sidebar2-width: {Width.ToString(CultureInfo.InvariantCulture)}{WidthUnit.ToCssString()};", Width > 0));

        private string? ImageStyleNames => BuildStyleNames("",
        ($"width: {ImageWidth.ToString(CultureInfo.InvariantCulture)}{WidthUnit.ToCssString()};", ImageWidth > 0));

    /// <summary>
    /// Gets or sets the badge text.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("2.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the badge text.")]
    [Parameter]
    public string? BadgeText { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("2.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the custom icon name.")]
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the data provider.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("2.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the data provider.")]
    [EditorRequired]
    [Parameter]
    public Sidebar2DataProviderDelegate? DataProvider { get; set; }

    /// <summary>
    /// Gets or sets the Href.
    /// </summary>
    /// <para>
    /// Default value is <see cref="string.Empty" />.
    /// </para>
    [AddedVersion("3.0.0")]
    [DefaultValue("Empty string")]
    [Description("Gets or sets the Href.")]
    [Parameter]
    public string? Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IconName.
    /// </summary>
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    [AddedVersion("2.1.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the IconName.")]
    [Parameter]
    public IconName IconName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Sidebar2"/> logo.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("2.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the <b>Sidebar2</b> logo.")]
    [Parameter]
    public string? ImageSrc { get; set; }

    /// <summary>
    /// Gets or sets the width of the image in pixels.
    /// You can change the unit by setting <see cref="WidthUnit" />.
    /// <para>
    /// Default value is 0.
    /// </para>
    /// </summary>
    [AddedVersion("3.4.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the width of the image in pixels. You can change the unit by setting <b>WidthUnit</b>.")]
    [Parameter]
    public float ImageWidth { get; set; } = 0;

    private string? navMenuCssClass => GetNavMenuCssClass();

    /// <summary>
    /// Gets or sets the <see cref="Sidebar2"/> title.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("2.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the <b>Sidebar2</b> title.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Sidebar2"/> width.
    /// </summary>
    /// <para>
    /// Default value is 270.
    /// </para>
    [AddedVersion("3.0.0")]
    [DefaultValue(270)]
    [Description("Gets or sets the <b>Sidebar2</b> width.")]
    [Parameter]
    public float Width { get; set; } = 270;

    /// <summary>
    /// Gets or sets the <see cref="Sidebar2"/> width unit.
    /// </summary>
    /// <para>
    /// Default value is <see cref="Unit.Px" />.
    /// </para>
    [AddedVersion("3.0.0")]
    [DefaultValue(Unit.Px)]
    [Description("Gets or sets the <b>Sidebar2</b> width unit.")]
    [Parameter]
    public Unit WidthUnit { get; set; } = Unit.Px;

    #endregion
}
