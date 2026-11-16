using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// On-call: who is primary and secondary this week (<see cref="MonitoringState.Shifts" />), the escalation policy, and the
/// next eight weeks with their overrides in words ("Priya Nair, covering for Marcus Osei"). The signed-in engineer can take
/// a position nobody has overridden (a dialog with an optional reason) and remove her own overrides (a confirmation).
/// Every result is announced in a status message that takes focus once the dialog closes; Cancel returns focus to the
/// button that opened the dialog.
/// </summary>
public partial class OnCall : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many weeks the rotation table shows, this week first.</summary>
    private const int Weeks = 8;

    /// <summary>Set when a dialog's action ran, so focus moves to the status message once it is hidden.</summary>
    private bool actionDone;

    /// <summary>Guards a double click: the first click acts and closes the dialog.</summary>
    private bool busy;

    private string? dialogError;

    /// <summary>Set when the reason was refused, so focus moves to it after the render that shows the error.</summary>
    private bool focusReasonAfterRender;

    /// <summary>The key of the button that opened the dialog, so Cancel can return focus to it.</summary>
    private string? openerKey;

    /// <summary>The Take and Remove buttons, by week and position.</summary>
    private readonly Dictionary<string, ElementReference> openers = new();

    private string pendingHolderName = string.Empty;

    private MonitoringOverride? pendingOverride;

    private MonitoringOnCallRole pendingRole;

    private DateTime pendingStart;

    private string reason = string.Empty;

    private string? reasonError;

    private ElementReference? reasonInput;

    private ElementReference? removeCancelButton;

    private Modal removeModal = default!;

    private AlertColor statusColor = AlertColor.Success;

    private string? statusMessage;

    private ElementReference statusRegion;

    private Modal takeModal = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusReasonAfterRender)
        {
            focusReasonAfterRender = false;
            await MonitoringDisplay.FocusAsync(reasonInput);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CellKey(MonitoringShift shift, MonitoringOnCallRole role) => FormattableString.Invariant($"{shift.Start:yyyyMMdd}-{role}");

    private static string Holder(MonitoringShift shift, MonitoringOnCallRole role) => role == MonitoringOnCallRole.Primary ? shift.PrimaryId : shift.SecondaryId;

    private static MonitoringOnCallRole Other(MonitoringOnCallRole role) => role == MonitoringOnCallRole.Primary ? MonitoringOnCallRole.Secondary : MonitoringOnCallRole.Primary;

    private static MonitoringOverride? Override(MonitoringShift shift, MonitoringOnCallRole role) => role == MonitoringOnCallRole.Primary ? shift.PrimaryOverride : shift.SecondaryOverride;

    private static string RoleHelp(MonitoringOnCallRole role) => role == MonitoringOnCallRole.Primary
        ? "Paged first for every alert."
        : "Paged when the primary doesn't acknowledge within 5 minutes.";

    private static string RoleLabel(MonitoringOnCallRole role) => role == MonitoringOnCallRole.Primary ? "Primary" : "Secondary";

    /// <summary>Formats a shift boundary like "Mon, Nov 30, 09:00".</summary>
    private static string WeekDay(DateTime time) => time.ToString("ddd, MMM d, HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Formats a week like "Nov 30 – Dec 7".</summary>
    private static string WeekText(DateTime start) =>
        start.ToString("MMM d", CultureInfo.InvariantCulture) + " – " + start.AddDays(7).ToString("MMM d", CultureInfo.InvariantCulture);

    private static string WeekText(MonitoringShift shift) => WeekText(shift.Start);

    private async Task AskRemoveAsync(MonitoringShift shift, MonitoringOnCallRole role, MonitoringOverride taken, string key)
    {
        pendingStart = shift.Start;
        pendingRole = role;
        pendingOverride = taken;
        pendingHolderName = Name(taken.OriginalId);
        await OpenAsync(removeModal, key);
    }

    private async Task AskTakeAsync(MonitoringShift shift, MonitoringOnCallRole role, string key)
    {
        pendingStart = shift.Start;
        pendingRole = role;
        pendingOverride = null;
        pendingHolderName = Name(Holder(shift, role));
        reason = string.Empty;
        reasonError = null;
        await OpenAsync(takeModal, key);
    }

    private async Task ConfirmRemoveAsync()
    {
        if (busy || pendingOverride is not { } taken)
            return;

        busy = true;
        var week = WeekText(pendingStart);
        var role = RoleLabel(pendingRole).ToLowerInvariant();
        var result = State.RemoveOverride(taken.Id);

        var message = result switch
        {
            MonitoringResult.Ok => $"Your override is removed. {pendingHolderName} is {role} on call for {week} again.",
            MonitoringResult.NotFound => $"Your override for {week} was already removed. Nothing changed.",
            MonitoringResult.WrongState => $"The week of {week} has already ended, so its override stays. Nothing changed.",
            MonitoringResult.NotAllowed => "Only the engineer who made an override can remove it, and you need to be signed in as an engineer. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        };

        await FinishAsync(removeModal, result == MonitoringResult.Ok ? AlertColor.Success : AlertColor.Warning, message);
    }

    private async Task ConfirmTakeAsync()
    {
        if (busy)
            return;

        busy = true;
        dialogError = null;
        reasonError = null;
        var week = WeekText(pendingStart);
        var role = RoleLabel(pendingRole).ToLowerInvariant();
        var result = State.TakeShift(pendingStart, pendingRole, reason, out _);

        if (result == MonitoringResult.Invalid)
        {
            if (reason.Trim().Length > MonitoringState.MaxReasonLength)
            {
                reasonError = $"Keep the reason to {MonitoringState.MaxReasonLength} characters or fewer.";
                focusReasonAfterRender = true;
            }
            else
            {
                dialogError = "The rotation has too many overrides to add another. Remove one of yours first; nothing changed.";
            }

            busy = false;

            return;
        }

        var message = result switch
        {
            MonitoringResult.Ok => $"You're now {role} on call for {week}, covering for {pendingHolderName}.",
            MonitoringResult.NoChange => $"You're already {role} on call for {week}. Nothing changed.",
            MonitoringResult.WrongState => WrongStateText(week, role),
            MonitoringResult.NotFound => $"There's no {role} shift for {week} in the rotation. Nothing changed.",
            MonitoringResult.NotAllowed => "Only engineers on the rotation can take shifts. Sign in again as an engineer; nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        };

        await FinishAsync(takeModal, result == MonitoringResult.Ok ? AlertColor.Success : AlertColor.Warning, message);
    }

    /// <summary>Records the result of an action and closes its dialog; focus then moves to the status message.</summary>
    private async Task FinishAsync(Modal modal, AlertColor color, string message)
    {
        statusColor = color;
        statusMessage = message;
        actionDone = true;

        await modal.HideAsync();
    }

    private string Name(string? engineerId) => State.FindEngineer(engineerId)?.Name ?? "Someone not on the rotation";

    /// <summary>
    /// The dialogs open from code, so Bootstrap does not return focus. After an action, focus goes to the status message
    /// (the button that opened the dialog may be gone); after Cancel, back to that button.
    /// </summary>
    private async Task OnDialogHiddenAsync()
    {
        dialogError = null;
        reasonError = null;
        busy = false;

        if (actionDone)
        {
            actionDone = false;
            StateHasChanged();
            await Task.Yield();
            await MonitoringDisplay.FocusAsync(statusRegion);
        }
        else if (openerKey is not null && openers.TryGetValue(openerKey, out var opener))
        {
            await MonitoringDisplay.FocusAsync(opener);
        }

        openerKey = null;
    }

    private Task OnRemoveShownAsync() => MonitoringDisplay.FocusAsync(removeCancelButton);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private Task OnTakeShownAsync() => MonitoringDisplay.FocusAsync(reasonInput);

    private async Task OpenAsync(Modal modal, string key)
    {
        dialogError = null;
        actionDone = false;
        busy = false;
        openerKey = key;

        await modal.ShowAsync();
    }

    /// <summary>Explains a refused take: the week ended, she holds the other position, or someone already took it over.</summary>
    private string WrongStateText(string week, string role)
    {
        if (pendingStart.AddDays(7) <= State.Clock.Now)
            return $"The week of {week} has already ended, so its shifts can't change. Nothing changed.";

        var shift = State.Shifts(0, Weeks).FirstOrDefault(item => item.Start == pendingStart);

        if (shift is not null && Holder(shift, Other(pendingRole)) == MyId)
            return $"You're {RoleLabel(Other(pendingRole)).ToLowerInvariant()} on call for {week}, and nobody holds both positions. Nothing changed.";

        if (shift is not null && Override(shift, pendingRole) is { } taken)
            return $"{Name(taken.EngineerId)} already covers the {role} shift for {week}. It can't be taken until that override is removed; nothing changed.";

        return $"The {role} shift for {week} can't be taken right now. Nothing changed.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the signed-in engineer's account ID (the rotation uses the same IDs).</summary>
    private string? MyId => State.Identity.Current?.Id;

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
