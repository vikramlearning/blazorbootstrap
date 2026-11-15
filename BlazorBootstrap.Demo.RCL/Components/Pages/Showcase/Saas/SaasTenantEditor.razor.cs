using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's dialog for one tenant's plan, seats, and status. Saving calls
/// <see cref="SaasState.UpdateWorkspaceAsOperator" /> and turns each <see cref="SaasResult" /> into a message; a suspension
/// (or making the tenant active again) notifies the workspace's owners who can sign in, which the dialog says beforehand.
/// Creating tenants isn't part of this dialog.
/// </summary>
public partial class SaasTenantEditor : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private TenantForm form = new();

    private Modal modal = default!;

    private int noteCount;

    private string? noteMessage;

    private string? noteText;

    private InputSelect<string>? planSelect;

    private Func<ElementReference?>? returnFocusTo;

    private int workspaceId;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => ResetForm(new TenantForm());

    #endregion

    #region Public Methods

    /// <summary>Stops listening to form changes.</summary>
    public void Dispose() => editContext.OnFieldChanged -= OnFieldChanged;

    /// <summary>
    /// Opens the dialog for a tenant. When it closes, focus moves to the element <paramref name="returnFocusTo" /> returns then,
    /// so an opener that was re-rendered meanwhile is still found.
    /// </summary>
    public async Task EditAsync(int tenantId, Func<ElementReference?>? returnFocusTo = null)
    {
        if (State.FindWorkspace(tenantId) is not { } workspace)
            return;

        workspaceId = workspace.Id;
        this.returnFocusTo = returnFocusTo;
        ClearMessages();
        ResetForm(new TenantForm { PlanId = workspace.PlanId, Seats = workspace.Seats, Status = workspace.Status });

        await modal.ShowAsync();
    }

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string JoinNames(IReadOnlyList<string> names) =>
        names.Count <= 2 ? string.Join(" and ", names) : $"{string.Join(", ", names.Take(names.Count - 1))}, and {names[^1]}";

    private static string PeriodName(SaasBillingPeriod period) => period == SaasBillingPeriod.Annual ? "annual" : "monthly";

    private static string PlanOptionText(SaasPlan plan)
    {
        var price = plan.MonthlyPricePerSeat == 0 ? "free" : $"{SaasDisplay.Money(plan.MonthlyPricePerSeat)} per seat a month";
        var limit = plan.SeatLimit == 0 ? "no seat limit" : $"up to {Count(plan.SeatLimit)} seats";

        return $"{plan.Name}: {price}, {limit}";
    }

    private static int SeatCap(SaasPlan plan) => plan.SeatLimit == 0 ? SaasSeedData.MaxSeats : plan.SeatLimit;

    private static string SeatsText(int seats) => seats == 1 ? "1 seat" : $"{Count(seats)} seats";

    private static string StatusHint(SaasWorkspaceStatus status) => status switch
    {
        SaasWorkspaceStatus.Trial => "Inside the free trial; not billed",
        SaasWorkspaceStatus.Active => "In good standing; a paid plan counts toward MRR",
        SaasWorkspaceStatus.PastDue => "A renewal payment failed; still counts toward MRR",
        SaasWorkspaceStatus.Suspended => "Members can't use the workspace",
        _ => "Closed and kept for the record",
    };

    private void ClearMessages()
    {
        errorMessage = errorText = noteMessage = noteText = null;
        errorCount = noteCount = 0;
    }

    private string ErrorFor(SaasResult result, SaasWorkspace workspace) => result switch
    {
        SaasResult.SeatsOutOfRange => SeatsError(workspace),
        SaasResult.NotAllowed => "Only a signed-in Lumora operator can change a tenant. Sign in as an operator and try again.",
        SaasResult.NotFound => "That tenant or plan no longer exists in this tab. Close the dialog and open the tenant again.",
        _ => "The changes couldn't be saved. Check the plan, seats, and status, and try again.",
    };

    private Task HideAsync() => modal.HideAsync();

    /// <summary>The owners who would get a notification: those with a sign-in in this demo.</summary>
    private IReadOnlyList<string> NotifiedOwners(int tenantId) =>
        State.MembersOf(tenantId).Where(member => member.Role == SaasMemberRole.Owner && member.AccountId is not null).Select(member => member.Name).ToList();

    /// <summary>The sentence about who is told when a tenant is suspended or made active again.</summary>
    private string NotifySentence(SaasWorkspace workspace, bool done)
    {
        var owners = NotifiedOwners(workspace.Id);

        if (owners.Count > 0)
            return done
                ? $"{JoinNames(owners)} got a notification in their workspace."
                : $"{JoinNames(owners)} {(owners.Count == 1 ? "gets" : "get")} a notification in their workspace.";

        var owner = State.OwnerOf(workspace.Id)?.Name ?? "The owner";

        return done
            ? $"{owner}, the owner, has no sign-in in this demo, so no notification was created."
            : $"The owner would get a notification, but {owner} has no sign-in in this demo, so no one sees it.";
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args) => ClearMessages();

    /// <summary>The modal opens from code, so Bootstrap doesn't move focus back to what opened it; this does (WCAG 2.4.3).</summary>
    private async Task OnHiddenAsync()
    {
        var target = returnFocusTo;

        returnFocusTo = null;
        workspaceId = 0;
        ClearMessages();
        ResetForm(new TenantForm());

        await SaasDisplay.FocusAsync(target?.Invoke());
    }

    /// <summary><see cref="NumberInput{TValue}" /> raises ValueChanged on its first render; an unchanged value is ignored.</summary>
    private void OnSeatsChanged(int value)
    {
        if (value == form.Seats)
            return;

        form.Seats = value;
        ClearMessages();
    }

    private Task OnShownAsync() => SaasDisplay.FocusAsync(planSelect?.Element);

    private void ResetForm(TenantForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    private string SavedMessage(SaasWorkspace before)
    {
        var after = State.FindWorkspace(before.Id) ?? before;
        var message = $"Saved {after.Name}: {State.PlanOf(after).Name}, {SeatsText(after.Seats)}, {SaasDisplay.StatusName(after.Status)}.";

        if (SaasState.NotifiesOwner(before.Status, after.Status))
            message += " " + NotifySentence(after, done: true);

        return message;
    }

    private async Task SaveAsync()
    {
        if (Workspace is not { } before)
        {
            ShowError("That tenant no longer exists in this tab. Close the dialog and open the tenant again.");

            return;
        }

        var result = State.UpdateWorkspaceAsOperator(before.Id, form.PlanId, form.Seats, form.Status);

        switch (result)
        {
            case SaasResult.Ok:
                var message = SavedMessage(before);

                if (OnSaved.HasDelegate)
                    await OnSaved.InvokeAsync(new SaasTenantSavedEventArgs(before.Id, message));

                await modal.HideAsync();
                break;

            case SaasResult.NoChange:
                ShowNote($"Nothing to save: {before.Name} already has this plan, these seats, and this status.");
                break;

            default:
                ShowError(ErrorFor(result, before));
                break;
        }
    }

    private string SeatsError(SaasWorkspace workspace)
    {
        var plan = SelectedPlan;
        var used = State.SeatsUsed(workspace.Id);
        var min = Math.Max(1, used);
        var max = SeatCap(plan);
        var limit = plan.SeatLimit == 0 ? $"the {plan.Name} plan has no seat limit, and Lumora allows up to {Count(max)}" : $"the {plan.Name} plan allows up to {Count(max)}";

        return min > max
            ? $"{workspace.Name} has {Count(used)} members and invitations, more than {limit}. Choose a bigger plan, or ask the customer to remove members first."
            : $"Choose {Count(min)} to {Count(max)} seats. The minimum is the {Count(used)} members and invitations of {workspace.Name}, and {limit}.";
    }

    private string SeatsHelp(SaasWorkspace workspace, SaasPlan plan)
    {
        var used = State.SeatsUsed(workspace.Id);
        var min = Math.Max(1, used);
        var max = SeatCap(plan);

        return min > max
            ? $"{Count(used)} in use (members and invitations), more than the {Count(max)} {plan.Name} allows."
            : $"{Count(used)} in use (members and invitations). {plan.Name} allows {Count(min)} to {Count(max)}.";
    }

    /// <summary>Shows an error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowError(string message)
    {
        noteMessage = noteText = null;
        noteCount = 0;
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : $"{message} ({errorCount})";
    }

    /// <summary>Shows a note; the same note again gets a counter, so a screen reader announces it again.</summary>
    private void ShowNote(string message)
    {
        errorMessage = errorText = null;
        errorCount = 0;
        noteCount = message == noteText ? noteCount + 1 : 1;
        noteText = message;
        noteMessage = noteCount == 1 ? message : $"{message} ({noteCount})";
    }

    /// <summary>What the chosen status does, said before saving: who is notified, or when a new trial ends.</summary>
    private string StatusNote(SaasWorkspace workspace)
    {
        if (form.Status == workspace.Status)
            return "Suspending the tenant, or making it active again, notifies its owner in their workspace.";

        return form.Status switch
        {
            SaasWorkspaceStatus.Suspended => $"Suspending stops the members of {workspace.Name} from using it. {NotifySentence(workspace, done: false)}",
            SaasWorkspaceStatus.Active when SaasState.NotifiesOwner(workspace.Status, SaasWorkspaceStatus.Active) => $"Making {workspace.Name} active again lets its members use it. {NotifySentence(workspace, done: false)}",
            SaasWorkspaceStatus.Active => "The owner isn't notified of this change.",
            SaasWorkspaceStatus.Trial => $"The trial ends on {SaasDisplay.Date(workspace.TrialEndsOn ?? State.Clock.Today.AddDays(SaasSeedData.TrialDays))}.",
            _ => "The owner isn't notified of this change.",
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the callback fired after a tenant was saved, with the message to show.</summary>
    [Parameter] public EventCallback<SaasTenantSavedEventArgs> OnSaved { get; set; }

    private SaasPlan SelectedPlan => State.FindPlan(form.PlanId) ?? State.Plans[0];

    [Inject] private SaasState State { get; set; } = default!;

    private SaasWorkspace? Workspace => workspaceId == 0 ? null : State.FindWorkspace(workspaceId);

    #endregion

    #region Nested Types

    private sealed class TenantForm
    {
        public string PlanId { get; set; } = string.Empty;

        public int Seats { get; set; } = 1;

        public SaasWorkspaceStatus Status { get; set; } = SaasWorkspaceStatus.Active;
    }

    #endregion
}

/// <summary>Describes a tenant saved by <see cref="SaasTenantEditor" />.</summary>
/// <param name="WorkspaceId">The ID of the saved tenant.</param>
/// <param name="Message">What changed, ready to show in a status message.</param>
public sealed record SaasTenantSavedEventArgs(int WorkspaceId, string Message);
