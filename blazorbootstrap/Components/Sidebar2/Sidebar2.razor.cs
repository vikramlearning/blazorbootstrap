namespace BlazorBootstrap;

/// <summary>
/// Sidebars are vertical navigation menus that are typically positioned on the left or right side of a page. <br/>
/// They are based on the <see href="https://getbootstrap.com/docs/5.0/examples/sidebars/">Bootstrap Sidebars example</see>
/// </summary>
public partial class Sidebar2 : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool collapseNavMenu = true;

    private bool collapseSidebar = false;

    private bool isMobile = false;

    private IReadOnlyCollection<NavItem>? items = null;

    private DotNetObjectReference<Sidebar2> objRef = default!;

    private bool requestInProgress = false;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.sidebar.initialize", Id, objRef);

            var width = await JsRuntime.InvokeAsync<int>("window.blazorBootstrap.sidebar.windowSize");

            BsWindowResize(width);

            await RefreshDataAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// The browser calls this method to tell us the <paramref name="width"/> of the window. <br/>
    /// <see cref="isMobile"/> is set to <see langword="true"/> if the <paramref name="width"/> is less than 641 pixels.
    /// </summary>
    /// <param name="width"></param>
    [JSInvokable("bsWindowResize")]
    public void BsWindowResize(int width)
    {
        isMobile = width < 641; // mobile
    }

    /// <summary>
    /// Refresh the sidebar data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync()
    {
        if (requestInProgress)
            return;

        requestInProgress = true;

            var result = await DataProvider.Invoke();
            items = result != null ? result.Data : new List<NavItem>();
        
        requestInProgress = false;

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Toggles sidebar.
    /// </summary>
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
                case nameof(BadgeText): BadgeText = (string)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(CustomIconName): CustomIconName = (string)parameter.Value; break;
                case nameof(DataProvider): DataProvider = (Sidebar2DataProviderDelegate)parameter.Value; break;
                case nameof(Href): Href = (string)parameter.Value; break;
                case nameof(IconName): IconName = (IconName)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(ImageSrc): ImageSrc = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(Title): Title = (string)parameter.Value; break;

                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-sidebar2", true),
            ("collapsed", collapseSidebar),
            ("expanded", !collapseSidebar));

    protected override string? StyleNames =>
        BuildStyleNames(Style,
            ($"--bb-sidebar2-width: {Width.ToString(CultureInfo.InvariantCulture)}{WidthUnit.ToCssString()};", Width > 0));


    /// <summary>
    /// Gets or sets the badge text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? BadgeText { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the data provider.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public Sidebar2DataProviderDelegate DataProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the Href.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IconName.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName IconName { get; set; }

    /// <summary>
    /// Gets or sets the sidebar logo.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? ImageSrc { get; set; }

    private string? NavMenuCssClass => GetNavMenuCssClass();

    /// <summary>
    /// Gets or sets the sidebar title.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public string? Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the sidebar width.
    /// </summary>
    /// <remarks>Default value is 270.</remarks>
    [Parameter]
    public float Width { get; set; } = 270;

    /// <summary>
    /// Gets or sets the sidebar width unit.
    /// </summary>
    /// <remarks>Default value is <see cref="Unit.Px" />.</remarks>
    [Parameter]
    public Unit WidthUnit { get; set; } = Unit.Px;

    #endregion
}
