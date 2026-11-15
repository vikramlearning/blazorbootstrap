using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Layout of Lumora's operator console: the shell with the operator's modules and the page behind an operator-only guard.
/// Each module appears in the sidebar once its screen exists, so the sidebar never links to a missing page.
/// </summary>
public partial class SaasAdminLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the console.</summary>
    public const string Base = SaasLayout.Base + "/admin";

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "overview", Href = Base, IconName = IconName.Speedometer2, Text = "Overview", Match = NavLinkMatch.All },
            new() { Id = "tenants", Href = $"{Base}/tenants", IconName = IconName.Buildings, Text = "Tenants" },
            new() { Id = "analytics", Href = $"{Base}/analytics", IconName = IconName.BarChartLine, Text = "Site analytics" },
            new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonGear, Text = "Account" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private SaasState State { get; set; } = default!;

    #endregion
}
