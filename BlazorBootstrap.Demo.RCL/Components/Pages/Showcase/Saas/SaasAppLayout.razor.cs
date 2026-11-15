using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Layout of the customer workspace: the shell with the workspace's modules, the workspace name and plan in the top bar,
/// and the page behind a customer-only guard, with a trial or suspension banner above it.
/// </summary>
public partial class SaasAppLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the workspace.</summary>
    public const string Base = SaasLayout.Base + "/app";

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

    private static string TrialText(int daysLeft) => daysLeft switch
    {
        0 => "Your trial ends today.",
        1 => "1 day left in your trial.",
        _ => $"{daysLeft} days left in your trial.",
    };

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private string PlanLine(SaasWorkspace workspace) => workspace.Status == SaasWorkspaceStatus.Trial
        ? $"{State.PlanOf(workspace).Name} trial"
        : $"{State.PlanOf(workspace).Name} plan · {workspace.Seats} seats";

    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "dashboard", Href = Base, IconName = IconName.Grid1X2, Text = "Dashboard", Match = NavLinkMatch.All },
            new() { Id = "team", Href = $"{Base}/team", IconName = IconName.People, Text = "Team" },
            new() { Id = "billing", Href = $"{Base}/billing", IconName = IconName.CreditCard, Text = "Billing" },
            new() { Id = "settings", Href = $"{Base}/settings", IconName = IconName.Gear, Text = "Settings" },
            new() { Id = "notifications", Href = $"{Base}/notifications", IconName = IconName.Bell, Text = "Notifications" },
            new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonCircle, Text = "Your account" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private SaasState State { get; set; } = default!;

    #endregion
}
