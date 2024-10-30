namespace BlazorBootstrap;

public partial class ThemeSwitcher : BlazorBootstrapComponentBase
{
    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await ThemeSwitcherJsInterop.InitializeAsync();

        await base.OnAfterRenderAsync(firstRender);
    }

    internal Task SetAutoTheme() => ThemeSwitcherJsInterop.SetAutoThemeAsync();

    internal Task SetDarkTheme() => ThemeSwitcherJsInterop.SetDarkThemeAsync();

    internal Task SetLightTheme() => ThemeSwitcherJsInterop.SetLightThemeAsync();

    #endregion

    #region Properties, Indexers

    [Inject] private ThemeSwitcherJsInterop ThemeSwitcherJsInterop { get; set; } = default!;

    #endregion
}
