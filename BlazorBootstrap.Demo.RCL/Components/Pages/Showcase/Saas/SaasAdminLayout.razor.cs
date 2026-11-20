using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Layout of Lumora's operator console: the shell with the operator's modules, a top bar with the global search (tenants,
/// invoices, leads, and plans) and the notifications bell, and the page behind an operator-only guard. Each module appears
/// in the sidebar once its screen exists, so the sidebar never links to a missing page; the Audit log is one of them.
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

    /// <summary>Each page opened bills whatever came due on the tab's clock (renewals and ended trials).</summary>
    protected override void OnParametersSet() => State.CatchUpBilling();

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
            new() { Id = "leads", Href = $"{Base}/leads", IconName = IconName.Inbox, Text = "Leads" },
            new() { Id = "invoices", Href = $"{Base}/invoices", IconName = IconName.Receipt, Text = "Invoices" },
            new() { Id = "dunning", Href = $"{Base}/dunning", IconName = IconName.ExclamationOctagon, Text = "Dunning" },
            new() { Id = "plans", Href = $"{Base}/plans", IconName = IconName.Tags, Text = "Plans & prices" },
            new() { Id = "revenue", Href = $"{Base}/revenue", IconName = IconName.GraphUp, Text = "Revenue" },
            new() { Id = "analytics", Href = $"{Base}/analytics", IconName = IconName.BarChartLine, Text = "Site analytics" },
            new() { Id = "audit", Href = $"{Base}/audit", IconName = IconName.ClockHistory, Text = "Audit log" },
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
