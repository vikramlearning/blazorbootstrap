using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Tenant detail in the operator console: the workspace's subscription summary, members, invoices, and activity, with an
/// "Edit tenant" action that opens <see cref="SaasTenantEditor" />. An archived tenant can be deleted after a confirmation
/// (<see cref="SaasState.DeleteWorkspaceAsOperator" />), which opens the tenant list; a tenant someone signs in to in this
/// demo is kept, and the dialog says why. An unknown ID renders the not-found screen.
/// </summary>
public partial class AdminTenantDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? deleteButton;

    private int deleteCount;

    /// <summary>The name of the tenant just deleted, shown while the dialog closes and the page moves to the tenant list.</summary>
    private string? deletedName;

    private string? deleteError;

    private string? deleteErrorText;

    /// <summary>The tenant the delete dialog is about, captured when it opens.</summary>
    private DeleteInfo? deleting;

    private Modal deleteModal = default!;

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
        deletedName = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string CountWords(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    private static string JoinNames(IReadOnlyList<string> names) =>
        names.Count <= 2 ? string.Join(" and ", names) : $"{string.Join(", ", names.Take(names.Count - 1))}, and {names[^1]}";

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

    private async Task AskDeleteAsync()
    {
        if (Workspace is not { } workspace)
            return;

        deleting = new DeleteInfo(workspace.Id, workspace.Name, State.SeatsUsed(workspace.Id), State.InvoicesOf(workspace.Id).Count);
        deleteError = deleteErrorText = null;
        deleteCount = 0;

        await deleteModal.ShowAsync();
    }

    /// <summary>Deletes the tenant; on success the dialog closes and <see cref="OnDeleteHiddenAsync" /> opens the tenant list.</summary>
    private async Task ConfirmDeleteAsync()
    {
        if (deleting is not { } info || deletedName is not null)
            return;

        var result = State.DeleteWorkspaceAsOperator(info.Id);

        if (result == SaasResult.Ok)
        {
            deletedName = info.Name;
            await deleteModal.HideAsync();

            return;
        }

        ShowDeleteError(DeleteErrorFor(result, info));
    }

    private string DeleteErrorFor(SaasResult result, DeleteInfo info)
    {
        if (result == SaasResult.NotFound)
            return $"{info.Name} is no longer in this tab. Close the dialog and go back to the tenant list.";

        var signsIn = State.MembersOf(info.Id).Where(member => member.AccountId is not null).Select(member => member.Name).ToList();

        if (signsIn.Count > 0)
        {
            return $"{info.Name} is kept: {JoinNames(signsIn)} {(signsIn.Count == 1 ? "signs" : "sign")} in to this workspace in the demo, "
                + "so deleting it would take away the workspace they use. It stays archived.";
        }

        return result == SaasResult.NotAllowed
            ? $"{info.Name} can't be deleted: only an archived tenant can be, by a signed-in Lumora operator."
            : $"{info.Name} couldn't be deleted. Close the dialog and try again.";
    }

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

    /// <summary>
    /// After a delete the page moves to the tenant list once the dialog is closed (so Bootstrap's backdrop is gone);
    /// otherwise focus returns to "Delete tenant", since a dialog opened from code doesn't get Bootstrap's focus return.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        deleting = null;
        deleteError = deleteErrorText = null;
        deleteCount = 0;

        if (deletedName is not null)
        {
            Navigation.NavigateTo($"{SaasAdminLayout.Base}/tenants");

            return;
        }

        await SaasDisplay.FocusAsync(deleteButton?.Element);
    }

    private void OnSaved(SaasTenantSavedEventArgs args)
    {
        statusCount = args.Message == statusText ? statusCount + 1 : 1;
        statusText = args.Message;
        statusMessage = statusCount == 1 ? args.Message : $"{args.Message} ({statusCount})";
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Shows why the delete was refused; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void ShowDeleteError(string message)
    {
        deleteCount = message == deleteErrorText ? deleteCount + 1 : 1;
        deleteErrorText = message;
        deleteError = deleteCount == 1 ? message : $"{message} ({deleteCount})";
    }

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

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private SaasState State { get; set; } = default!;

    private SaasWorkspace? Workspace => State.FindWorkspace(Id);

    #endregion

    #region Nested Types

    /// <summary>What the delete dialog says about the tenant: its members and invitations, and its invoices.</summary>
    private sealed record DeleteInfo(int Id, string Name, int Members, int Invoices);

    #endregion
}
