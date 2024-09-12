using BlazorBootstrap;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class Config
{
    #region Methods

    /// <summary>
    /// Adds a bootstrap providers and component mappings.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddBlazorBootstrap(this IServiceCollection services, IConfiguration configuration = null!)
    {
        services.AddScoped<BreadcrumbService>();
        services.AddScoped<ModalService>();
        services.AddScoped<PreloadService>();
        services.AddScoped<ToastService>();

        services.AddScoped<PdfViewerJsInterop>();
        services.AddScoped<SortableListJsInterop>();

        return services;
    }

    #endregion
}
