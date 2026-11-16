using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Layout of the Driftwood Stays host back office (D-031): a top bar with "View site" and the account menu, a sidebar with
/// the host's modules, and the page behind a host-only guard. The sidebar shows only to a signed-in host.
/// </summary>
public partial class HostLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the host back office.</summary>
    public const string Base = TravelLayout.Base + "/host";

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

    /// <summary>The host's modules.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "today", Href = Base, IconName = IconName.Speedometer2, Text = "Today", Match = NavLinkMatch.All },
            new() { Id = "reservations", Href = $"{Base}/reservations", IconName = IconName.CalendarCheck, Text = "Reservations" },
            new() { Id = "calendar", Href = $"{Base}/calendar", IconName = IconName.Calendar3, Text = "Calendar" },
            new() { Id = "listings", Href = $"{Base}/listings", IconName = IconName.HouseDoor, Text = "Listings" },
            new() { Id = "inbox", Href = $"{Base}/inbox", IconName = IconName.Chat, Text = "Inbox" },
            new() { Id = "reviews", Href = $"{Base}/reviews", IconName = IconName.Star, Text = "Reviews" },
            new() { Id = "earnings", Href = $"{Base}/earnings", IconName = IconName.Cash, Text = "Earnings" },
            new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonGear, Text = "Account" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
