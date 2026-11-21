using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Alerts: the customer's feed, newest first (kind as an icon and a word, title, text, time, a link to what it is about,
/// and "Unread" as a badge), with "Mark as read" per alert and "Mark all as read"; opening an alert's link also marks it
/// read. Beside it, the alert settings: native switches for each kind and the low-balance and large-transaction amounts,
/// edited on a copy of the saved settings and saved with field errors for amounts out of range. Alerts only appear here
/// and in the header's bell, which reads this same feed, so its unread count always agrees with the page: the demo sends no
/// emails or texts. The settings form follows rule WR-002: each amount's range and cents, and one cross-field rule (with the
/// low-balance alert on, its amount must be below the Everyday Checking balance, or it would fire straight away); a failed
/// save shows a summary that links to each field. After marking an alert read, focus moves to its title, since the button
/// that had focus goes away.
/// </summary>
public partial class Alerts : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const decimal MaxAmount = 1_000_000m;

    private EditContext editContext = default!;

    private string? feedMessage;

    private ElementReference feedTitle;

    private CurrencyInput<decimal> largeAmountInput = default!;

    private CurrencyInput<decimal> lowAmountInput = default!;

    private ValidationMessageStore messages = default!;

    private readonly Dictionary<int, ElementReference> noticeTitles = new();

    /// <summary>The form's copy of the saved settings; the saved ones change only on Save.</summary>
    private FintechAlertSettings settings = new();

    private bool settingsError;

    private string? settingsMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        settings = Copy(State.AlertSettings);
        editContext = new EditContext(settings);
        messages = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    private string? AriaInvalid(FieldIdentifier field) => editContext.GetValidationMessages(field).Any() ? "true" : null;

    private static FintechAlertSettings Copy(FintechAlertSettings source) => new()
    {
        Budget = source.Budget,
        Dispute = source.Dispute,
        LargeTransaction = source.LargeTransaction,
        LargeTransactionAmount = source.LargeTransactionAmount,
        LowBalance = source.LowBalance,
        LowBalanceAmount = source.LowBalanceAmount,
        Security = source.Security,
        Transfer = source.Transfer,
    };

    private string? InvalidClass(FieldIdentifier field) => editContext.GetValidationMessages(field).Any() ? "is-invalid" : null;

    private static bool IsCents(decimal value) => decimal.Round(value, 2) == value;

    private static IconName KindIcon(FintechAlertKind kind) => kind switch
    {
        FintechAlertKind.LowBalance => IconName.Wallet2,
        FintechAlertKind.LargeTransaction => IconName.CashStack,
        FintechAlertKind.Budget => IconName.PieChart,
        FintechAlertKind.Transfer => IconName.ArrowLeftRight,
        FintechAlertKind.Dispute => IconName.Flag,
        _ => IconName.ShieldLock,
    };

    private static string KindLabel(FintechAlertKind kind) => kind switch
    {
        FintechAlertKind.LowBalance => "Low balance",
        FintechAlertKind.LargeTransaction => "Large transaction",
        FintechAlertKind.Budget => "Budget",
        FintechAlertKind.Transfer => "Transfer",
        FintechAlertKind.Dispute => "Dispute",
        _ => "Security",
    };

    private static string LinkText(FintechNotice notice) => notice.Kind switch
    {
        FintechAlertKind.Transfer => "View the transfer",
        FintechAlertKind.Dispute => "View the dispute",
        FintechAlertKind.Budget => "View budgets",
        FintechAlertKind.LowBalance => "View the account",
        FintechAlertKind.LargeTransaction => "View the transaction",
        _ => "View details",
    };

    private async Task MarkAllReadAsync()
    {
        var count = State.UnreadNoticeCount;
        State.MarkAllNoticesRead();
        feedMessage = count == 1 ? "1 alert marked as read." : $"{count.ToString(CultureInfo.InvariantCulture)} alerts marked as read.";

        // The button goes away with the last unread alert, so focus moves to the feed's heading.
        StateHasChanged();
        await Task.Yield();
        await FintechDisplay.FocusAsync(feedTitle);
    }

    private async Task MarkReadAsync(FintechNotice notice)
    {
        State.MarkNoticeRead(notice.Id);
        feedMessage = $"“{notice.Title}” marked as read.";

        // The button goes away, so focus moves to the alert's title.
        StateHasChanged();
        await Task.Yield();

        if (noticeTitles.TryGetValue(notice.Id, out var title))
            await FintechDisplay.FocusAsync(title);
    }

    private static string OnCountText(FintechAlertSettings saved)
    {
        var on = new[] { saved.LowBalance, saved.LargeTransaction, saved.Budget, saved.Transfer, saved.Dispute, saved.Security }.Count(value => value);

        return on switch
        {
            0 => "Every alert is off.",
            6 => "Every alert is on.",
            _ => $"{on.ToString(CultureInfo.InvariantCulture)} of 6 alerts are on.",
        };
    }

    /// <summary>An edited amount clears its own error, so a stale message never describes a corrected value.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        messages.Clear(e.FieldIdentifier);
        editContext.NotifyValidationStateChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Checks the amounts and the cross-field rule into the form's messages, then asks the form to validate, so a failed save
    /// shows the summary and moves focus to it (rule WR-002). The bank checks the amounts again.
    /// </summary>
    private Task SaveAsync()
    {
        messages.Clear();
        settingsError = false;

        if (settings.LowBalanceAmount is < 0 or > MaxAmount)
            messages.Add(LowAmountField, "Enter a low-balance amount from $0 to $1,000,000.");
        else if (!IsCents(settings.LowBalanceAmount))
            messages.Add(LowAmountField, "Enter the low-balance amount in dollars and cents.");

        if (settings.LargeTransactionAmount is < 1 or > MaxAmount)
            messages.Add(LargeAmountField, "Enter a large-transaction amount from $1 to $1,000,000.");
        else if (!IsCents(settings.LargeTransactionAmount))
            messages.Add(LargeAmountField, "Enter the large-transaction amount in dollars and cents.");

        if (settings.LowBalance && !editContext.GetValidationMessages(LowAmountField).Any() && settings.LowBalanceAmount >= CheckingBalance)
        {
            messages.Add(LowAmountField,
                $"Choose a low-balance amount below your Everyday Checking balance of {FintechDisplay.Money(CheckingBalance)}, or the alert would fire straight away. Or switch the alert off.");
        }

        if (!editContext.Validate())
        {
            settingsError = true;
            settingsMessage = "Check the highlighted amounts, then save again.";

            return Task.CompletedTask;
        }

        if (Same(settings, State.AlertSettings))
        {
            settingsMessage = "Nothing changed: these are already your settings.";

            return Task.CompletedTask;
        }

        var result = State.UpdateAlerts(settings);
        settingsError = result != FintechResult.Ok;
        settingsMessage = result switch
        {
            FintechResult.Ok => $"Alert settings saved. {OnCountText(State.AlertSettings)}",
            FintechResult.NotAllowed => "Your session needs your password and one-time code again. Sign in, then save again.",
            FintechResult.Invalid => "One of the amounts isn't allowed. Use $0 to $1,000,000 for low balance and $1 to $1,000,000 for large transactions.",
            _ => "Your settings weren't saved. Try again.",
        };

        return Task.CompletedTask;
    }

    private static bool Same(FintechAlertSettings left, FintechAlertSettings right) =>
        left.Budget == right.Budget && left.Dispute == right.Dispute && left.LargeTransaction == right.LargeTransaction
        && left.LargeTransactionAmount == right.LargeTransactionAmount && left.LowBalance == right.LowBalance
        && left.LowBalanceAmount == right.LowBalanceAmount && left.Security == right.Security && left.Transfer == right.Transfer;

    private string TimeText(DateTime at)
    {
        var day = DateOnly.FromDateTime(at);
        var dayText = day == State.Today ? "Today" : day == State.Today.AddDays(-1) ? "Yesterday" : FintechDisplay.Day(at);

        return $"{dayText}, {at.ToString("HH:mm", CultureInfo.InvariantCulture)}";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the Everyday Checking balance, which the low-balance amount must stay below while that alert is on.</summary>
    private decimal CheckingBalance => State.FindAccount("checking")?.Balance ?? 0;

    private FieldIdentifier LargeAmountField => new(settings, nameof(FintechAlertSettings.LargeTransactionAmount));

    private FieldIdentifier LowAmountField => new(settings, nameof(FintechAlertSettings.LowBalanceAmount));

    /// <summary>Gets the feed, newest first.</summary>
    private IReadOnlyList<FintechNotice> Notices => State.Notices.OrderByDescending(notice => notice.At).ThenByDescending(notice => notice.Id).ToList();

    [Inject] private FintechState State { get; set; } = default!;

    private string UnreadText => State.UnreadNoticeCount switch
    {
        0 when State.Notices.Count == 0 => "Nothing yet.",
        0 => "All read.",
        1 => "1 unread.",
        var count => $"{count.ToString(CultureInfo.InvariantCulture)} unread.",
    };

    #endregion
}
