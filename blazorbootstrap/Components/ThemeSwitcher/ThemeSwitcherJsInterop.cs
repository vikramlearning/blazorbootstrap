namespace BlazorBootstrap;

public class ThemeSwitcherJsInterop : IAsyncDisposable
{
    #region Fields and Constants

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    #endregion

    #region Constructors

    public ThemeSwitcherJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Blazor.Bootstrap/blazor.bootstrap.theme-switcher.js").AsTask());
    }

    #endregion

    #region Methods

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // do nothing
        }
    }

    public async Task InitializeAsync(DotNetObjectReference<ThemeSwitcher>? objRef)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("initializeTheme", objRef);
    }

    internal Task SetAutoThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef) => SetThemeAsync(objRef, "system");

    internal Task SetDarkThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef) => SetThemeAsync(objRef, "dark");

    internal Task SetLightThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef) => SetThemeAsync(objRef, "light");

    internal async Task SetThemeAsync(DotNetObjectReference<ThemeSwitcher>? objRef, string themeName)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("setTheme", objRef, themeName);
    }

    #endregion
}
