namespace BlazorBootstrap;

public partial class ThemeSwitcher : BlazorBootstrapComponentBase
{

    private DotNetObjectReference<ThemeSwitcher>? objRef;

    #region Methods

    protected override Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await ThemeSwitcherJsInterop.InitializeAsync(objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    internal Task SetAutoTheme() => ThemeSwitcherJsInterop.SetAutoThemeAsync(objRef);

    internal Task SetDarkTheme() => ThemeSwitcherJsInterop.SetDarkThemeAsync(objRef);

    internal Task SetLightTheme() => ThemeSwitcherJsInterop.SetLightThemeAsync(objRef);

    [JSInvokable]
    public async Task OnThemeChangedJS(string themeName)
    {
        if (OnThemeChanged.HasDelegate)
            await OnThemeChanged.InvokeAsync(themeName);
    }

    #endregion

    #region Properties, Indexers

    [Inject] private ThemeSwitcherJsInterop ThemeSwitcherJsInterop { get; set; } = default!;

    #endregion

    /// <summary>
    /// Fired when the theme is changed.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnThemeChanged { get; set; }
}
