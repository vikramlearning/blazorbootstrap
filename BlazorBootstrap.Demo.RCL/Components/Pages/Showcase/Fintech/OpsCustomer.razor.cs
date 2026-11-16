using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One customer, as operations see them: the profile (email, city, segment, customer since, status in text, accounts, and
/// total balance). For the demo customer, whose ledger is in the tab, also each account's balance and available amount,
/// the card's status (frozen or active), and the last 10 transactions. For everyone, their transfers (held and scheduled
/// first, then the most recent, each linking to its operations page) and disputes (linking to the dispute), and the
/// internal notes, newest first with author and time, with a form to add one (1–500 characters, with a counter; customers
/// never see notes). The outcome of adding a note is announced in a status message. An unknown ID shows a not-found screen.
/// </summary>
public partial class OpsCustomer : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxNote = 500;

    /// <summary>How many closed transfers (sent, rejected, cancelled) the page lists after the waiting ones.</summary>
    private const int RecentClosedTransfers = 10;

    /// <summary>How many of the demo customer's transactions the page lists.</summary>
    private const int RecentTransactionCount = 10;

    private bool focusNoteStatus;

    private string? loadedId;

    private string? noteError;

    private ElementReference? noteInput;

    private bool noteIsError;

    private string? noteMessage;

    private ElementReference? noteStatus;

    private string? noteText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusNoteStatus)
        {
            focusNoteStatus = false;
            await FintechDisplay.FocusAsync(noteIsError ? noteInput : noteStatus);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        noteText = null;
        noteError = null;
        noteMessage = null;
        noteIsError = false;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string NoteResultText(FintechResult result, string customerName) => result switch
    {
        FintechResult.Ok => $"Note added to {customerName}'s profile. Only operations can see it.",
        FintechResult.Invalid => $"Write a note of 1 to {MaxNote} characters.",
        FintechResult.LimitReached => "The demo's notes are full, so this note wasn't added. Reset the demo to start again.",
        FintechResult.NotFound => "We couldn't find this customer any more, so the note wasn't added.",
        FintechResult.NotAllowed => "Sign in again as operations, with a one-time code, to add notes. The note wasn't added.",
        _ => "That didn't work, and the note wasn't added. Try again.",
    };

    private static string TransferPath(string reference) => $"{FintechOpsLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    private void AddNote()
    {
        if (Customer is not { } customer)
            return;

        var text = noteText?.Trim() ?? string.Empty;

        if (text.Length == 0)
        {
            noteError = "Write the note first.";
            noteIsError = true;
            noteMessage = "Write the note first: it can't be empty.";
            focusNoteStatus = true;
            return;
        }

        var result = State.AddCustomerNote(customer.Id, text);

        noteIsError = result != FintechResult.Ok;
        noteError = noteIsError ? NoteResultText(result, customer.Name) : null;
        noteMessage = NoteResultText(result, customer.Name);

        if (result == FintechResult.Ok)
            noteText = null;

        focusNoteStatus = true;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Says when a transfer goes or went, and where from.</summary>
    private string TransferWhen(FintechTransfer transfer)
    {
        var from = State.FindAccount(transfer.FromAccountId)?.Name ?? transfer.FromAccountId;

        return transfer.Status switch
        {
            FintechTransferStatus.Held => $"For {Transfers.DateText(transfer.Date)} · from {from}",
            FintechTransferStatus.Scheduled => $"Goes on {Transfers.DateText(transfer.Date)} · from {from}",
            _ when transfer.ClosedAt is { } closedAt => $"{Transfers.StampText(closedAt)} · from {from}",
            _ => $"{Transfers.DateText(transfer.Date)} · from {from}",
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private FintechCustomer? Customer => State.FindCustomer(Id);

    /// <summary>Gets the customer's disputes, newest first.</summary>
    private IReadOnlyList<FintechDispute> CustomerDisputes => State.AllDisputes.Where(dispute => dispute.CustomerId == Id).ToList();

    private static string CustomersPath => $"{FintechOpsLayout.Base}/customers";

    /// <summary>Gets the customer's transfers: held ones, then scheduled ones by date, then the most recent closed ones.</summary>
    private IReadOnlyList<FintechTransfer> CustomerTransfers
    {
        get
        {
            var mine = State.AllTransfers.Where(transfer => transfer.CustomerId == Id).ToList();
            var waiting = mine
                .Where(transfer => transfer.Status is FintechTransferStatus.Held or FintechTransferStatus.Scheduled)
                .OrderBy(transfer => transfer.Status == FintechTransferStatus.Held ? 0 : 1)
                .ThenBy(transfer => transfer.Date);
            var closed = mine
                .Where(transfer => transfer.Status is not (FintechTransferStatus.Held or FintechTransferStatus.Scheduled))
                .OrderByDescending(transfer => transfer.ClosedAt ?? transfer.CreatedAt)
                .Take(RecentClosedTransfers);

            return waiting.Concat(closed).ToList();
        }
    }

    /// <summary>Gets or sets the customer ID from the route.</summary>
    [Parameter] public string Id { get; set; } = string.Empty;

    private IReadOnlyList<FintechCustomerNote> Notes => State.NotesFor(Id);

    /// <summary>Gets the demo customer's 10 most recent transactions, newest first.</summary>
    private IReadOnlyList<FintechTransaction> RecentTransactions => State.Transactions
        .Where(item => DateOnly.FromDateTime(item.Date) <= State.Today)
        .OrderByDescending(item => item.Date)
        .ThenByDescending(item => item.Id)
        .Take(RecentTransactionCount)
        .ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
