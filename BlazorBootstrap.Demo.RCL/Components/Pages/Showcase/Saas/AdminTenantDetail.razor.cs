using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Tenant detail in the operator console: the workspace's subscription summary, members, invoices, and activity, with an
/// "Edit tenant" action that opens <see cref="SaasTenantEditor" />. An unknown ID renders the not-found screen.
/// </summary>
public partial class AdminTenantDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? editButton;

    private SaasTenantEditor? editor;

    private int shownId;

    private int statusCount;

    private string? statusMessage;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>A message about one tenant is cleared when the route moves to another tenant.</summary>
    protected override void OnParametersSet()
    {
        if (shownId == Id)
            return;

        shownId = Id;
        statusMessage = statusText = null;
        statusCount = 0;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static double EventsPercent(SaasWorkspace workspace, SaasPlan plan) =>
        plan.EventLimit == 0 ? 0 : workspace.EventsThisMonth * 100d / plan.EventLimit;

    private static string EventsText(SaasWorkspace workspace, SaasPlan plan) => plan.EventLimit == 0
        ? $"{SaasDisplay.Events(workspace.EventsThisMonth)} (no limit)"
        : $"{SaasDisplay.Events(workspace.EventsThisMonth)} of {SaasDisplay.Events(plan.EventLimit)}";

    private static BadgeColor InvoiceColor(SaasInvoiceStatus status) => status switch
    {
        SaasInvoiceStatus.Paid => BadgeColor.Success,
        SaasInvoiceStatus.Open => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    private static string MembersText(IReadOnlyList<SaasMember> members)
    {
        var invited = members.Count(member => member.Status == SaasMemberStatus.Invited);
        var active = members.Count - invited;

        return invited == 0
            ? $"{Count(active)} {(active == 1 ? "member" : "members")}"
            : $"{Count(active)} {(active == 1 ? "member" : "members")} and {Count(invited)} open {(invited == 1 ? "invitation" : "invitations")}, each holding a seat";
    }

    private static string PlanSeatText(SaasPlan plan) =>
        plan.SeatLimit == 0 ? $"{plan.Name} has no seat limit" : $"{plan.Name} allows up to {Count(plan.SeatLimit)}";

    private string DaysLeftText(SaasWorkspace workspace) => State.TrialDaysLeft(workspace) switch
    {
        null => string.Empty,
        0 => "Ends today",
        1 => "1 day left",
        var days => $"{days} days left",
    };

    private Task EditAsync() => Workspace is null || editor is null ? Task.CompletedTask : editor.EditAsync(Workspace.Id, () => editButton?.Element);

    private string MrrNote(SaasWorkspace workspace, SaasPlan plan, decimal mrr) => mrr > 0
        ? workspace.Period == SaasBillingPeriod.Annual ? "Per month, from the annual price" : "Per month"
        : workspace.Status switch
        {
            SaasWorkspaceStatus.Trial => "Not billed during the trial",
            SaasWorkspaceStatus.Suspended => "Not counted while suspended",
            SaasWorkspaceStatus.Archived => "Not counted once archived",
            _ when plan.MonthlyPricePerSeat == 0 => $"{plan.Name} is free",
            _ => "Nothing billed",
        };

    private string NoInvoicesText(SaasWorkspace workspace, SaasPlan plan) => workspace.Status == SaasWorkspaceStatus.Trial
        ? "No invoices yet: trials aren't billed."
        : plan.MonthlyPricePerSeat == 0 ? $"No invoices: the {plan.Name} plan is free." : "No invoices yet.";

    private void OnSaved(SaasTenantSavedEventArgs args)
    {
        statusCount = args.Message == statusText ? statusCount + 1 : 1;
        statusText = args.Message;
        statusMessage = statusCount == 1 ? args.Message : $"{args.Message} ({statusCount})";
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Overview", Href = SaasAdminLayout.Base },
        new() { Text = "Tenants", Href = $"{SaasAdminLayout.Base}/tenants" },
        new() { Text = Workspace?.Name, IsCurrentPage = true },
    };

    /// <summary>Gets or sets the tenant (workspace) ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private SaasState State { get; set; } = default!;

    private SaasWorkspace? Workspace => State.FindWorkspace(Id);

    #endregion
}
