using BlazorBootstrap;

namespace Microsoft.Extensions.DependencyInjection;

public static class Config
{
    #region Methods

    /// <summary>
    /// Adds a bootstrap providers and component mappings.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddBlazorBootstrap(this IServiceCollection services)
    {
        services.AddScoped<BreadcrumbService>();
        services.AddScoped<ModalService>();
        services.AddScoped<PreloadService>();
        services.AddScoped<ToastService>();

        services.AddScoped<PdfViewerJsInterop>();
        services.AddScoped<SortableListJsInterop>();
        services.AddScoped<ThemeSwitcherJsInterop>();

        return services;
    }

    #endregion
}
