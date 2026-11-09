namespace BlazorBootstrap;

/// <summary>
/// A responsive navigation header with a brand, navigation, and other content, which collapses behind a toggler below a breakpoint. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/navbar/" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class Navbar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// The parameters that decide which menu and toggler elements the interop holds, as of its last initialization.
    /// </summary>
    private string? initializedInteropKey;

    private bool isLocationChangedSubscribed;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            if (isLocationChangedSubscribed)
            {
                NavigationManager.LocationChanged -= OnLocationChanged;
                isLocationChangedSubscribed = false;
            }

            if (IsRenderComplete)
                await SafeInvokeVoidAsync("window.blazorBootstrap.navbar.dispose", Id);
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // These parameters replace the menu or toggler element, so the interop must pick up the new ones (CS-004).
        var interopKey = $"{togglerTargetId}|{hasExternalCollapse}|{CollapseMode}|{TogglerPosition}|{hasToggler}|{hasMenu}";
        if (interopKey != initializedInteropKey)
        {
            initializedInteropKey = interopKey;
            await SafeInvokeVoidAsync("window.blazorBootstrap.navbar.initialize", Id, togglerTargetId, hasExternalCollapse);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        NavigationManager.LocationChanged += OnLocationChanged;
        isLocationChangedSubscribed = true;
    }

    /// <summary>
    /// Hides the collapsed menu. Does nothing when the navbar is expanded or the menu is already hidden.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Hides the collapsed menu. Does nothing when the navbar is expanded or the menu is already hidden.")]
    public async Task HideAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.navbar.hide", Id);

    /// <summary>
    /// Shows the collapsed menu.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the collapsed menu.")]
    public async Task ShowAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.navbar.show", Id);

    /// <summary>
    /// Shows the collapsed menu if it is hidden, or hides it if it is shown.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the collapsed menu if it is hidden, or hides it if it is shown.")]
    public async Task ToggleAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.navbar.toggle", Id);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) =>
        _ = InvokeAsync(HideAsync);

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible name of the navigation landmark, read by screen readers.
    /// Set it when the page has more than one navigation landmark.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of the navigation landmark, read by screen readers. Set it when the page has more than one navigation landmark.")]
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the background color, applied with Bootstrap's <c>bg-*</c> classes.
    /// <para>
    /// Default value is <see cref="BackgroundColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(BackgroundColor.None)]
    [Description("Gets or sets the background color, applied with Bootstrap's <b>bg-*</b> classes.")]
    [Parameter]
    public BackgroundColor BackgroundColor { get; set; } = BackgroundColor.None;

    /// <summary>
    /// Gets or sets the brand, usually a <see cref="NavbarBrand" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the brand, usually a <b>NavbarBrand</b>.")]
    [Parameter]
    public RenderFragment? Brand { get; set; }

    /// <summary>
    /// Gets or sets whether the brand is placed inside the collapsed menu, so it is hidden until the menu expands.
    /// Ignored when <see cref="ExternalCollapseId" /> is set, because there is no menu inside the navbar to hold it.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the brand is placed inside the collapsed menu, so it is hidden until the menu expands. Ignored when <b>ExternalCollapseId</b> is set, because there is no menu inside the navbar to hold it.")]
    [Parameter]
    public bool BrandInCollapse { get; set; }

    /// <summary>
    /// Gets or sets the content of the menu: <see cref="NavbarNav" />, <see cref="NavbarText" />, forms, input groups, or buttons.
    /// It collapses behind the toggler below <see cref="ExpandBreakpoint" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the menu: <b>NavbarNav</b>, <b>NavbarText</b>, forms, input groups, or buttons. It collapses behind the toggler below <b>ExpandBreakpoint</b>.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Navbar, true),
            (ExpandBreakpoint.ToNavbarExpandClass(), ExpandBreakpoint != NavbarExpand.Never),
            (Color.ToNavbarColorClass(), Color != NavbarColor.None),
            (BackgroundColor.ToBackgroundClass(), BackgroundColor != BackgroundColor.None),
            (Placement.ToNavbarPlacementClass(), Placement != NavbarPlacement.Static));

    /// <summary>
    /// Gets or sets how the menu is shown below <see cref="ExpandBreakpoint" />: in a collapse region or an offcanvas panel.
    /// <para>
    /// Default value is <see cref="NavbarCollapseMode.Collapse" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavbarCollapseMode.Collapse)]
    [Description("Gets or sets how the menu is shown below <b>ExpandBreakpoint</b>: in a collapse region or an offcanvas panel.")]
    [Parameter]
    public NavbarCollapseMode CollapseMode { get; set; } = NavbarCollapseMode.Collapse;

    /// <summary>
    /// Gets or sets the contextual background color, applied with Bootstrap's <c>text-bg-*</c> classes.
    /// Pair a dark color with <see cref="NavbarTheme.Dark" />.
    /// <para>
    /// Default value is <see cref="NavbarColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavbarColor.None)]
    [Description("Gets or sets the contextual background color, applied with Bootstrap's <b>text-bg-*</b> classes. Pair a dark color with <b>NavbarTheme.Dark</b>.")]
    [Parameter]
    public NavbarColor Color { get; set; } = NavbarColor.None;

    /// <summary>
    /// Gets or sets the width of the container inside the navbar.
    /// <para>
    /// Default value is <see cref="NavbarContainer.Fluid" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavbarContainer.Fluid)]
    [Description("Gets or sets the width of the container inside the navbar.")]
    [Parameter]
    public NavbarContainer Container { get; set; } = NavbarContainer.Fluid;

    /// <summary>
    /// Gets or sets the breakpoint at which the navbar expands from its collapsed menu to a horizontal bar.
    /// <para>
    /// Default value is <see cref="NavbarExpand.Large" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavbarExpand.Large)]
    [Description("Gets or sets the breakpoint at which the navbar expands from its collapsed menu to a horizontal bar.")]
    [Parameter]
    public NavbarExpand ExpandBreakpoint { get; set; } = NavbarExpand.Large;

    /// <summary>
    /// Gets or sets the id of a <see cref="Collapse" /> outside the navbar that the toggler shows and hides.
    /// When set, <see cref="ChildContent" /> is not collapsed, and focus moves into the collapse when it opens.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the id of a <b>Collapse</b> outside the navbar that the toggler shows and hides. When set, <b>ChildContent</b> is not collapsed, and focus moves into the collapse when it opens.")]
    [Parameter]
    public string? ExternalCollapseId { get; set; }

    private bool hasExternalCollapse => !string.IsNullOrWhiteSpace(ExternalCollapseId);

    private bool hasMenu => ChildContent is not null || (BrandInCollapse && Brand is not null);

    private bool hasToggler => hasExternalCollapse || (hasMenu && ExpandBreakpoint != NavbarExpand.Always);

    private string menuId => $"{Id}-menu";

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private string offcanvasClassNames =>
        BuildClassNames(
            (BootstrapClass.Offcanvas, true),
            (OffcanvasPlacement.ToOffcanvasPlacementClass(), true));

    /// <summary>
    /// Gets or sets the accessible label of the offcanvas close button.
    /// <para>
    /// Default value is "Close".
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Close")]
    [Description("Gets or sets the accessible label of the offcanvas close button.")]
    [Parameter]
    public string OffcanvasCloseButtonAriaLabel { get; set; } = "Close";

    /// <summary>
    /// Gets or sets the side the offcanvas panel opens from when <see cref="CollapseMode" /> is <see cref="NavbarCollapseMode.Offcanvas" />.
    /// <para>
    /// Default value is <see cref="Placement.End" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(BlazorBootstrap.Placement.End)]
    [Description("Gets or sets the side the offcanvas panel opens from when <b>CollapseMode</b> is <b>Offcanvas</b>.")]
    [Parameter]
    public Placement OffcanvasPlacement { get; set; } = BlazorBootstrap.Placement.End;

    /// <summary>
    /// Gets or sets the title of the offcanvas panel. It also names the panel for screen readers.
    /// Without a title, the panel is named by <see cref="AriaLabel" />, so set one of the two.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title of the offcanvas panel. It also names the panel for screen readers. Without a title, the panel is named by <b>AriaLabel</b>, so set one of the two.")]
    [Parameter]
    public string? OffcanvasTitle { get; set; }

    private bool hasOffcanvasTitle => !string.IsNullOrWhiteSpace(OffcanvasTitle);

    private string offcanvasTitleId => $"{Id}-offcanvas-title";

    /// <summary>
    /// Gets or sets where the navbar is placed on the page.
    /// <para>
    /// Default value is <see cref="NavbarPlacement.Static" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavbarPlacement.Static)]
    [Description("Gets or sets where the navbar is placed on the page.")]
    [Parameter]
    public NavbarPlacement Placement { get; set; } = NavbarPlacement.Static;

    /// <summary>
    /// Gets or sets the color theme, set through Bootstrap's <c>data-bs-theme</c> attribute.
    /// <para>
    /// Default value is <see cref="NavbarTheme.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavbarTheme.None)]
    [Description("Gets or sets the color theme, set through Bootstrap's <b>data-bs-theme</b> attribute.")]
    [Parameter]
    public NavbarTheme Theme { get; set; } = NavbarTheme.None;

    /// <summary>
    /// Gets or sets the accessible label of the toggler button.
    /// <para>
    /// Default value is "Toggle navigation".
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Toggle navigation")]
    [Description("Gets or sets the accessible label of the toggler button.")]
    [Parameter]
    public string TogglerAriaLabel { get; set; } = "Toggle navigation";

    /// <summary>
    /// Gets or sets where the toggler is placed relative to the brand.
    /// <para>
    /// Default value is <see cref="NavbarTogglerPosition.End" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavbarTogglerPosition.End)]
    [Description("Gets or sets where the toggler is placed relative to the brand.")]
    [Parameter]
    public NavbarTogglerPosition TogglerPosition { get; set; } = NavbarTogglerPosition.End;

    private string togglerTargetId => hasExternalCollapse ? ExternalCollapseId! : menuId;

    #endregion
}
