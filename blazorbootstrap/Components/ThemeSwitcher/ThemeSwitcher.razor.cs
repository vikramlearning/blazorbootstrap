namespace BlazorBootstrap;

public partial class ThemeSwitcher : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<ThemeSwitcher>? objRef;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await ThemeSwitcherJsInterop.InitializeAsync(objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        return base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task OnThemeChangedJS(string themeName)
    {
        if (OnThemeChanged.HasDelegate)
            await OnThemeChanged.InvokeAsync(themeName);
    }

    internal Task SetAutoTheme() => ThemeSwitcherJsInterop.SetAutoThemeAsync(objRef);

    internal Task SetDarkTheme() => ThemeSwitcherJsInterop.SetDarkThemeAsync(objRef);

    internal Task SetLightTheme() => ThemeSwitcherJsInterop.SetLightThemeAsync(objRef);

    #endregion

    #region Properties, Indexers
       
    /// <summary>
    /// Fired when the theme is changed.
    /// </summary>
    [Parameter] public EventCallback<string> OnThemeChanged { get; set; }

    /// <summary>
    /// Gets or sets the dropdown menu position.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownMenuPosition.Start" />.
    /// </remarks>
    [Parameter] public DropdownMenuPosition Position { get; set; } = DropdownMenuPosition.Start;

    [Inject] private ThemeSwitcherJsInterop ThemeSwitcherJsInterop { get; set; } = default!;

    #endregion
}
