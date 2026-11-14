namespace BlazorBootstrap.Demo.RCL.Services.Showcase;

/// <summary>
/// Registers the in-memory state services used by the Showcase sample websites.
/// </summary>
public static class ShowcaseServiceCollectionExtensions
{
    #region Methods

    #region Public Methods

    /// <summary>
    /// Adds one scoped state service per Showcase site. Scoped lifetime gives per-circuit state on
    /// Blazor Server and per-app state on WebAssembly, so a full reload re-seeds every site.
    /// </summary>
    public static IServiceCollection AddShowcaseServices(this IServiceCollection services)
    {
        services.AddScoped<Marketing.MarketingState>();
        services.AddScoped<SaasDashboard.SaasDashboardState>();

        return services;
    }

    #endregion

    #endregion
}
