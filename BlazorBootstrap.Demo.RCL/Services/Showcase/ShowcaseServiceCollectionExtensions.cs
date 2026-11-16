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
        services.AddScoped<Foundation.ShowcaseSessionStore>();
        services.AddScoped<AiAssistant.AiState>();
        services.AddScoped<ProjectBoard.PbState>();
        services.AddScoped<Ecommerce.EcommerceState>();
        services.AddScoped<Fintech.FintechState>();
        services.AddScoped<HelpDesk.HelpDeskState>();
        services.AddScoped<Learning.LearningState>();
        services.AddScoped<Monitoring.MonitoringState>();
        services.AddScoped<Saas.SaasState>();
        services.AddScoped<Travel.TravelState>();

        return services;
    }

    #endregion

    #endregion
}
