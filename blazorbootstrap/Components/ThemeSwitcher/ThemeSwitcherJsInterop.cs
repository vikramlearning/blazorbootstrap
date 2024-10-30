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
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    public async Task InitializeAsync()
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("initializeTheme");
    }

    internal Task SetAutoThemeAsync() => SetThemeAsync("system");

    internal Task SetDarkThemeAsync() => SetThemeAsync("dark");

    internal Task SetLightThemeAsync() => SetThemeAsync("light");

    internal async Task SetThemeAsync(string themeName)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("setTheme", themeName);
    }

    #endregion
}
