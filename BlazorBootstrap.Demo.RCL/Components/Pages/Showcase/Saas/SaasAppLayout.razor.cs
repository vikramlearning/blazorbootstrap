using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Layout of the customer workspace: the shell with the workspace's modules, the workspace name and plan, the global search
/// (the team and the workspace's invoices), and the notifications bell (the same list as the Notifications page) in the top
/// bar, and the page behind a customer-only guard, with a banner above it: a failed payment (with the amount, the next
/// retry or the end of grace, and "Update card"), a suspension, a past-due invoice, or the trial's days left.
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

    private static string DunningTitle(SaasDunningCase failing) => failing.Stage switch
    {
        SaasDunningStage.Retrying => "Payment failed",
        SaasDunningStage.Grace => "Payment overdue: last days to pay",
        _ => "Workspace suspended for an unpaid invoice",
    };

    private static string DunningText(SaasWorkspace workspace, SaasDunningCase failing)
    {
        var amount = SaasDisplay.MoneyCents(failing.Amount);

        return failing.Stage switch
        {
            SaasDunningStage.Retrying => $"{amount} for invoice {failing.InvoiceNumber} couldn't be charged ({SaasSeedData.FailureText(failing.Reason)}). "
                + $"We'll try again on {SaasDisplay.Date(failing.NextRetryAt ?? failing.FailedAt)}; update your card to keep {workspace.Name} running.",
            SaasDunningStage.Grace => $"Every retry of {amount} failed. {workspace.Name} is suspended on {SaasDisplay.Date(failing.GraceEndsAt ?? failing.FailedAt)} unless invoice {failing.InvoiceNumber} is paid.",
            _ => $"{workspace.Name} is read-only until {amount} (invoice {failing.InvoiceNumber}) is paid. Update the card and pay in Billing to restore it at once.",
        };
    }

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
