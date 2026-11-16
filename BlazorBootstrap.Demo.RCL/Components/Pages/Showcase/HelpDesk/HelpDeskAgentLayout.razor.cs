using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Layout of the "Beacon Desk" agent desk (D-031): a top bar with "View help center" and the account menu, a sidebar with
/// the agent's modules (and, for an admin, the admin modules), and the page behind a staff-only guard. The sidebar shows only to
/// signed-in staff.
/// </summary>
public partial class HelpDeskAgentLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the agent desk.</summary>
    public const string Base = HelpDeskLayout.Base + "/agent";

    /// <summary>Base route of the admin modules, inside the agent desk.</summary>
    public const string AdminBase = Base + "/admin";

    /// <summary>The roles that may open the agent desk.</summary>
    public const string StaffRoles = HelpDeskSeedData.AgentRole + "," + HelpDeskSeedData.AdminRole;

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

    /// <summary>The agent's modules. Each appears here once its screen exists, so the sidebar never links to a missing page.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "queue", Href = Base, IconName = IconName.Inbox, Text = "Queue", Match = NavLinkMatch.All },
            new() { Id = "new", Href = $"{Base}/tickets/new", IconName = IconName.PlusCircle, Text = "New ticket" },
            new() { Id = "replies", Href = $"{Base}/saved-replies", IconName = IconName.ChatQuote, Text = "Saved replies" },
            new() { Id = "kb", Href = $"{Base}/kb", IconName = IconName.Book, Text = "Knowledge base" },
            new() { Id = "reports", IconName = IconName.BarChartLine, Text = "Reports" },
            new() { Id = "report-sla", ParentId = "reports", Href = $"{Base}/reports/sla", IconName = IconName.Stopwatch, Text = "SLA" },
            new() { Id = "report-csat", ParentId = "reports", Href = $"{Base}/reports/satisfaction", IconName = IconName.EmojiSmile, Text = "Satisfaction" },
        };

        if (State.IsAdmin)
        {
            items.Add(new() { Id = "admin", IconName = IconName.Sliders, Text = "Admin" });
            items.Add(new() { Id = "admin-agents", ParentId = "admin", Href = $"{AdminBase}/agents", IconName = IconName.People, Text = "Agents & teams" });
            items.Add(new() { Id = "admin-sla", ParentId = "admin", Href = $"{AdminBase}/sla", IconName = IconName.Hourglass, Text = "SLA policies" });
            items.Add(new() { Id = "admin-categories", ParentId = "admin", Href = $"{AdminBase}/categories", IconName = IconName.Tags, Text = "Categories" });
        }

        items.Add(new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonGear, Text = "Account" });

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
