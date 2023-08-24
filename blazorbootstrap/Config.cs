using BlazorBootstrap;

namespace Microsoft.Extensions.DependencyInjection;

public static class Config
{
    #region Methods

    /// <summary>
    /// Adds a bootstrap providers and component mappings.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddBlazorBootstrap(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<BootstrapClassProvider>();
        serviceCollection.AddSingleton<BootstrapIconProvider>();
        serviceCollection.AddScoped<IIdGenerator, IdGenerator>();

        serviceCollection.AddScoped<BreadcrumbService>();
        serviceCollection.AddScoped<ModalService>();
        serviceCollection.AddScoped<PreloadService>();
        serviceCollection.AddScoped<ToastService>();

        serviceCollection.AddBootstrapComponents();

        return serviceCollection;
    }

    public static IServiceCollection AddBootstrapComponents(this IServiceCollection serviceCollection)
    {
        foreach (var mapping in ComponentMap) serviceCollection.AddTransient(mapping.Key, mapping.Value);

        return serviceCollection;
    }

    #endregion

    #region Properties, Indexers

    public static IDictionary<Type, Type> ComponentMap => new Dictionary<Type, Type> { { typeof(Button), typeof(Button) } };

    #endregion
}
