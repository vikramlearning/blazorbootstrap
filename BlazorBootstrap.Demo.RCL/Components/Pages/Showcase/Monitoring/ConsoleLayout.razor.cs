using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Layout of the Stackwatch engineers' console (D-031): a top bar with "View status page" and the account menu (its
/// notice count is the engineer's unread notifications), a sidebar with the engineer's modules, and the page behind an
/// engineer-only guard. The sidebar shows only to a signed-in engineer.
/// </summary>
public partial class ConsoleLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the console.</summary>
    public const string Base = MonitoringLayout.Base + "/console";

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

    /// <summary>The engineer's modules.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "overview", Href = Base, IconName = IconName.Speedometer2, Text = "Overview", Match = NavLinkMatch.All },
            new() { Id = "services", Href = $"{Base}/services", IconName = IconName.HddStack, Text = "Services" },
            new() { Id = "incidents", Href = $"{Base}/incidents", IconName = IconName.ExclamationOctagon, Text = "Incidents" },
            new() { Id = "logs", Href = $"{Base}/logs", IconName = IconName.Terminal, Text = "Logs and traces" },
            new() { Id = "on-call", Href = $"{Base}/on-call", IconName = IconName.CalendarWeek, Text = "On-call" },
            new() { Id = "notifications", Href = $"{Base}/notifications", IconName = IconName.Bell, Text = "Notifications" },
            new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonGear, Text = "Account" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
