namespace BlazorBootstrap;

public class ThemeSwitcherJsInterop : JsInteropBase
{
    #region Constructors

    public ThemeSwitcherJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, "./_content/Blazor.Bootstrap/blazor.bootstrap.theme-switcher.js")
    {
    }

    #endregion

    #region Methods

    public async Task InitializeAsync(DotNetObjectReference<ThemeSwitcher>? objRef)
    {
        await SafeInvokeVoidAsync("initializeTheme", objRef);
    }

    internal Task SetAutoThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef) => SetThemeAsync(objRef, "system");

    internal Task SetDarkThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef) => SetThemeAsync(objRef, "dark");

    internal Task SetLightThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef) => SetThemeAsync(objRef, "light");

    internal async Task SetThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef, string themeName)
    {
        await SafeInvokeVoidAsync("setTheme", objRef, themeName);
    }

    #endregion
}
