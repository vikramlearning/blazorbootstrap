using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Tenant detail in the operator console: the workspace's subscription summary (with its subdomain and account manager),
/// members and invoices as <see cref="Grid{TItem}" /> lists (each with a saved view, sorting, paging, the column chooser, and
/// CSV export; an invoice row opens the invoice), and activity, with an "Edit tenant" action that opens
/// <see cref="SaasTenantEditor" />. A "Payments and dunning" card shows an open failed payment (invoice, reason, attempts,
/// the next step, and its timeline) with Retry now, Extend grace, and Suspend now (validated dialogs with a reason and a
/// preview, rule WR-002), earlier failed payments, and a demo action that simulates a failed charge on a tenant in good
/// standing. An archived tenant can be deleted after a confirmation
/// (<see cref="SaasState.DeleteWorkspaceAsOperator" />), which opens the tenant list; a tenant someone signs in to in this
/// demo is kept, and the dialog says why. An unknown ID renders the not-found screen.
/// </summary>
public partial class AdminTenantDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

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

    private InputNumber<int?>? daysInput;

    private EditContext dunningContext = new(new DunningForm());

    private DunningDialog dunningDialog;

    private string? dunningError;

    private DunningForm dunningForm = new();

    private Modal dunningModal = default!;

    /// <summary>Whether the dunning dialog closed after its action, so focus goes to the status message.</summary>
    private bool dunningDone;

    private Button? extendButton;

    private bool focusStatus;

    private InputTextArea? reasonInput;

    private Button? retryButton;

    private Button? simulateButton;

    private ElementReference? statusRegion;

    private Button? suspendButton;

    private SaasTenantEditor? editor;

    private Grid<SaasInvoice>? invoicesGrid;

    private int invoicesPageSize = DefaultPageSize;

    /// <summary>Changes when the tenant changes, so the invoice grid starts on its first page.</summary>
    private int invoicesVersion;

    private ShowcaseGridView invoicesView = default!;

    private Grid<SaasMember>? membersGrid;

    private int membersPageSize = DefaultPageSize;

    /// <summary>Changes when the tenant changes, so the member grid starts on its first page.</summary>
    private int membersVersion;

    private ShowcaseGridView membersView = default!;

    private int shownId;

    private int statusCount;

    private string? statusMessage;

    private string? statusText;

    /// <summary>Whether the lists' saved views are loaded (they live in the tab's session storage).</summary>
    private bool viewsLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the lists' saved views once the page is interactive, then shows the grids.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await SaasDisplay.FocusAsync(statusRegion);
        }

        if (!firstRender)
            return;

        await membersView.LoadAsync();
        await invoicesView.LoadAsync();
        membersPageSize = (await membersView.GetSettingsAsync()).PageSize;
        invoicesPageSize = (await invoicesView.GetSettingsAsync()).PageSize;
        viewsLoaded = true;
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        membersView = GridViews.Get(State.SiteSlug, "tenant-members", DefaultPageSize, "role");
        invoicesView = GridViews.Get(State.SiteSlug, "tenant-invoices", DefaultPageSize, "issued", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    /// <summary>A message about one tenant is cleared when the route moves to another tenant, and its lists start again on their first page.</summary>
    protected override void OnParametersSet()
    {
        if (shownId == Id)
            return;

        shownId = Id;
        statusMessage = statusText = null;
        statusCount = 0;
        deletedName = null;
        membersVersion++;
        invoicesVersion++;
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

    private void OnSaved(SaasTenantSavedEventArgs args) => ShowStatus(args.Message);

    private static string CardOnFileText(SaasWorkspace workspace) => workspace.CardLast4 is null
        ? "No card on file"
        : $"{workspace.CardBrand} ending in {workspace.CardLast4}{(workspace.CardFails is { } fails ? $" ({SaasDisplay.FailureName(fails).ToLowerInvariant()})" : string.Empty)}";

    /// <summary>Runs the dialog's action once its form is valid (rule WR-002); the state checks again, and its answer becomes the status message.</summary>
    private async Task ConfirmDunningAsync()
    {
        if (!await dunningContext.ValidateAsync() || Workspace is not { } workspace || State.OpenDunningCase(workspace.Id) is not { } item)
            return;

        var reason = dunningForm.Reason.Trim();
        var result = dunningDialog == DunningDialog.Extend ? State.ExtendGrace(item.Id, dunningForm.Days ?? 0, reason) : State.SuspendForNonPayment(item.Id, reason);

        if (result != SaasResult.Ok)
        {
            dunningError = result switch
            {
                SaasResult.WrongState => dunningDialog == DunningDialog.Extend ? "Only a case in grace or suspended can be extended." : "Only a case retrying or in grace can be suspended now.",
                SaasResult.NotAllowed => "Sign in as a Lumora operator to do this.",
                _ => "That didn't work. Check the days and the reason, and try again.",
            };

            return;
        }

        var after = State.FindDunningCase(item.Id);
        ShowStatus(dunningDialog == DunningDialog.Extend
            ? $"{workspace.Name} has until {SaasDisplay.Date(after?.GraceEndsAt ?? State.Clock.Now)} to pay. Its billing contacts who can sign in were told."
            : $"{workspace.Name} is suspended until {item.InvoiceNumber} is paid. Its billing contacts who can sign in were told.");
        dunningDone = true;
        await dunningModal.HideAsync();
    }

    /// <summary>What the dialog's action does, in words, as the form is filled in.</summary>
    private string DunningPreview
    {
        get
        {
            if (Workspace is not { } workspace || State.OpenDunningCase(workspace.Id) is not { } item)
                return "This tenant has no open failed payment.";

            if (dunningDialog == DunningDialog.Suspend)
                return $"{workspace.Name} is suspended now: its members can't change anything until {SaasDisplay.MoneyCents(item.Amount)} is paid. Paying restores it at once.";

            return dunningForm.Days is { } days and >= 1 and <= SaasState.MaxExtensionDays && State.GraceEndAfterExtension(item.Id, days) is { } ends
                ? $"Grace ends on {SaasDisplay.Date(ends)}{(item.Stage == SaasDunningStage.Suspended ? ", and the workspace is usable again until then" : string.Empty)}. If nothing is paid by then, it is suspended."
                : string.Create(CultureInfo.InvariantCulture, $"Choose 1 to {SaasState.MaxExtensionDays} days to see the new end of grace.");
        }
    }

    /// <summary>The dialog opens from code, so focus returns by hand: to its button, or to the status message after its action.</summary>
    private async Task OnDunningHiddenAsync()
    {
        if (dunningDone)
        {
            dunningDone = false;
            focusStatus = true;
            StateHasChanged();

            return;
        }

        await SaasDisplay.FocusAsync(dunningDialog == DunningDialog.Extend ? extendButton?.Element : suspendButton?.Element);
    }

    private Task OnDunningShownAsync() => dunningDialog == DunningDialog.Extend ? SaasDisplay.FocusAsync(daysInput?.Element) : SaasDisplay.FocusAsync(reasonInput?.Element);

    private async Task OpenDunningDialogAsync(DunningDialog dialog)
    {
        dunningDialog = dialog;
        dunningForm = new DunningForm { IsExtension = dialog == DunningDialog.Extend, Days = dialog == DunningDialog.Extend ? 7 : null };
        dunningContext = new EditContext(dunningForm);
        dunningError = null;
        dunningDone = false;
        await dunningModal.ShowAsync();
    }

    /// <summary>Retries the open case now; the result goes in the status message, and focus stays on the button (or moves to the message once the case is recovered).</summary>
    private async Task RetryNowAsync()
    {
        if (Workspace is not { } workspace || State.OpenDunningCase(workspace.Id) is not { } item)
            return;

        var result = State.RetryDunningNow(item.Id);
        ShowStatus(result switch
        {
            SaasResult.Ok => $"The payment went through: {item.InvoiceNumber} is paid, and {workspace.Name} is recovered.",
            SaasResult.CardRequired => "The retry failed: there's no card on file. The customer needs to add one in Billing.",
            SaasResult.CardDeclined => $"The retry failed again ({SaasDisplay.FailureName(State.FindWorkspace(workspace.Id)?.CardFails ?? item.Reason).ToLowerInvariant()}). The customer needs to replace the card in Billing.",
            _ => "That didn't work: the case is no longer open.",
        });

        if (result == SaasResult.Ok)
            focusStatus = true;

        await Task.CompletedTask;
    }

    /// <summary>Starts dunning on a tenant in good standing (demo); the card's button goes away, so focus moves to the status message.</summary>
    private Task SimulateFailureAsync()
    {
        if (Workspace is not { } workspace)
            return Task.CompletedTask;

        var result = State.SimulateFailedCharge(workspace.Id);
        ShowStatus(result == SaasResult.Ok && State.OpenDunningCase(workspace.Id) is { } item
            ? $"The renewal charge of {SaasDisplay.MoneyCents(item.Amount)} failed ({SaasDisplay.FailureName(item.Reason).ToLowerInvariant()}). Dunning started: {SaasDisplay.LowerFirst(AdminDunning.NextStepText(item))}. The workspace's billing contacts were notified."
            : "A failed charge can only be simulated on an active tenant on a paid plan without an open failed payment.");
        focusStatus = true;

        return Task.CompletedTask;
    }

    private static IconName StepIcon(SaasDunningEventKind kind) => kind switch
    {
        SaasDunningEventKind.Paid => IconName.CheckCircle,
        SaasDunningEventKind.GraceStarted or SaasDunningEventKind.GraceExtended => IconName.HourglassSplit,
        SaasDunningEventKind.Suspended => IconName.SlashCircle,
        SaasDunningEventKind.Reactivated => IconName.ArrowCounterclockwise,
        SaasDunningEventKind.CardUpdated => IconName.CreditCard,
        SaasDunningEventKind.RetryFailed => IconName.ArrowRepeat,
        _ => IconName.ExclamationTriangle,
    };

    private static string StepTitle(SaasDunningEventKind kind) => kind switch
    {
        SaasDunningEventKind.Failed => "Charge failed",
        SaasDunningEventKind.RetryFailed => "Retry failed",
        SaasDunningEventKind.Paid => "Paid",
        SaasDunningEventKind.GraceStarted => "Grace period started",
        SaasDunningEventKind.GraceExtended => "Grace extended",
        SaasDunningEventKind.Suspended => "Suspended",
        SaasDunningEventKind.Reactivated => "Usable again",
        _ => "Card updated",
    };

    /// <summary>Another screen or the editor changed the state: the lists read the tenant's members and invoices again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridsAsync();
        StateHasChanged();
    });

    /// <summary>The page of invoices the grid asked for, newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SaasInvoice>> ProvideInvoicesAsync(GridDataProviderRequest<SaasInvoice> request) =>
        invoicesView.ApplyAsync(request, State.InvoicesOf(Id));

    /// <summary>The page of members the grid asked for: owners first, then by role and name, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SaasMember>> ProvideMembersAsync(GridDataProviderRequest<SaasMember> request) =>
        membersView.ApplyAsync(request, State.MembersOf(Id));

    private async Task RefreshGridsAsync()
    {
        if (membersGrid is not null)
            await membersGrid.RefreshDataAsync();

        if (invoicesGrid is not null)
            await invoicesGrid.RefreshDataAsync();
    }

    /// <summary>Forgets the invoice list's saved view and builds it again with its defaults; the status message says so.</summary>
    private async Task ResetInvoicesViewAsync()
    {
        await invoicesView.ResetAsync();
        invoicesPageSize = DefaultPageSize;
        invoicesVersion++;
        ShowStatus("The invoice list's view is reset: newest first, 10 a page, and every column shown.");
    }

    /// <summary>Forgets the member list's saved view and builds it again with its defaults; the status message says so.</summary>
    private async Task ResetMembersViewAsync()
    {
        await membersView.ResetAsync();
        membersPageSize = DefaultPageSize;
        membersVersion++;
        ShowStatus("The member list's view is reset: owners first, 10 a page, and every column shown.");
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void ShowStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>Keeps the invoice grid's settings in its saved view, and follows the page size the selector chose.</summary>
    private async Task SaveInvoicesSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            invoicesPageSize = settings.PageSize;

        await invoicesView.SaveSettingsAsync(settings);
    }

    /// <summary>Keeps the member grid's settings in its saved view, and follows the page size the selector chose.</summary>
    private async Task SaveMembersSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            membersPageSize = settings.PageSize;

        await membersView.SaveSettingsAsync(settings);
    }

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

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets or sets the tenant (workspace) ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private string InvoicesExportName => string.Create(CultureInfo.InvariantCulture, $"saas-tenant-{Id}-invoices");

    private string MembersExportName => string.Create(CultureInfo.InvariantCulture, $"saas-tenant-{Id}-members");

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private SaasState State { get; set; } = default!;

    private SaasWorkspace? Workspace => State.FindWorkspace(Id);

    #endregion

    #region Nested Types

    /// <summary>Which dunning dialog is open.</summary>
    private enum DunningDialog
    {
        Extend,
        Suspend,
    }

    /// <summary>The dunning dialog's fields, with the rules of <see cref="SaasState.ExtendGrace" /> and <see cref="SaasState.SuspendForNonPayment" />.</summary>
    private sealed class DunningForm : IValidatableObject
    {
        [Range(1, SaasState.MaxExtensionDays, ErrorMessage = "Extend by 1 to 14 days.")]
        public int? Days { get; set; }

        /// <summary>Whether the dialog extends grace (the days are needed); set by the page.</summary>
        public bool IsExtension { get; set; }

        [Required(ErrorMessage = "Give a reason: it goes on the timeline and the audit log.")]
        [StringLength(SaasState.MaxDunningReasonLength, ErrorMessage = "Use 200 characters or fewer for the reason.")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>An extension needs its number of days.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsExtension && Days is null)
                yield return new ValidationResult("Enter how many days to add, from 1 to 14.", new[] { nameof(Days) });
        }
    }

    /// <summary>What the delete dialog says about the tenant: its members and invitations, and its invoices.</summary>
    private sealed record DeleteInfo(int Id, string Name, int Members, int Invoices);

    #endregion
}
