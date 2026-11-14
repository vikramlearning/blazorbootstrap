using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// Layout of the "Pulseboard" SaaS admin and analytics dashboard: a sidebar, a top bar with notifications
/// and the account menu, and the page body.
/// </summary>
public partial class SaasDashboardLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the dashboard site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_SaasDashboard;

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("saas-dashboard");

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

    private void OnNotificationsShown() => State.MarkNotificationsRead();

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnThemeChanged(string themeName) => State.SetTheme(themeName);

    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "overview", Href = Base, IconName = IconName.Grid1X2, Text = "Overview", Match = NavLinkMatch.All },
            new() { Id = "analytics", Href = $"{Base}/analytics", IconName = IconName.BarChartLine, Text = "Analytics" },
            new() { Id = "users", Href = $"{Base}/users", IconName = IconName.People, Text = "Users" },
            new() { Id = "settings", Href = $"{Base}/settings", IconName = IconName.Gear, Text = "Settings" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string NotificationsLabel => State.UnreadNotificationCount == 0
        ? "Notifications"
        : $"Notifications, {State.UnreadNotificationCount} unread";

    [Inject] private SaasDashboardState State { get; set; } = default!;

    #endregion
}
