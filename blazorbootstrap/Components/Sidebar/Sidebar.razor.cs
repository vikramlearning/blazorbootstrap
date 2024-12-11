using System.Diagnostics.Contracts;

namespace BlazorBootstrap;

/// <summary>
/// Sidebars are vertical navigation menus that are typically positioned on the left or right side of a page. <br/>
/// They are based on the <see href="https://getbootstrap.com/docs/5.0/examples/sidebars/">Bootstrap Sidebars example</see>
/// </summary>
public partial class Sidebar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool collapseNavMenu = true;

    private bool collapseSidebar = false;

    private bool isMobile = false;

    private IReadOnlyCollection<NavItem>? items = null;

    private DotNetObjectReference<Sidebar> objRef = default!;

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
    /// Invoked by the browser when the window is resized.
    /// If the <paramref name="width"/> is less than 641, <see cref="isMobile"/> is set to <see langword="true"/>.
    /// </summary>
    /// <param name="width">Width of the window</param>
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

        if (DataProvider != null)
        {
            var result = await DataProvider.Invoke();
            items = result != null ? result.Data : new List<NavItem>();
        }

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
                case var _ when String.Equals(parameter.Name, nameof(BadgeText), StringComparison.OrdinalIgnoreCase): BadgeText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(CustomIconName), StringComparison.OrdinalIgnoreCase): CustomIconName = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(DataProvider), StringComparison.OrdinalIgnoreCase): DataProvider = (SidebarDataProviderDelegate)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Href), StringComparison.OrdinalIgnoreCase): Href = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IconName), StringComparison.OrdinalIgnoreCase): IconName = (IconName)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ImageSrc), StringComparison.OrdinalIgnoreCase): ImageSrc = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Style), StringComparison.OrdinalIgnoreCase): Style = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Title), StringComparison.OrdinalIgnoreCase): Title = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Width), StringComparison.OrdinalIgnoreCase): Width = (CssPropertyValue)parameter.Value; break;
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        var classList = new HashSet<string>();

        if (collapseNavMenu)
            classList.Add("collapse");

        classList.Add("bb-sidebar-content nav-scrollable bb-scrollbar");

        if (collapseSidebar)
            classList.Add("bb-scrollbar-hidden");

        NavMenuCssClass = String.Join(" ", classList);
        
        return base.SetParametersAsync(ParameterView.Empty);
    }


    #endregion

    #region Properties, Indexers 
             
    /// <summary>
    /// Gets or sets the badge text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? BadgeText { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the data provider.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public SidebarDataProviderDelegate? DataProvider { get; set; } 

    /// <summary>
    /// Gets or sets the Href.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IconName.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter] public IconName IconName { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the sidebar logo.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? ImageSrc { get; set; }

    private string? NavMenuCssClass { get; set; }

    /// <summary>
    /// Gets or sets the sidebar title.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public string? Title { get; set; } 

    /// <summary>
    /// Gets or sets the sidebar width.
    /// </summary>
    /// <remarks>Default value is 270.</remarks>
    [Parameter] public CssPropertyValue Width { get; set; } = CssPropertyValue.Pixels(270);


    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    private string Style { get; set; } = "";

    #endregion
}
