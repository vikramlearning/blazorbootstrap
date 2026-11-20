using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One customer, as operations see them: the profile (email, city, segment, customer since, status in text, accounts, and
/// total balance); for the demo customer, whose ledger is in the tab, also each account's balance and available amount and
/// the card's status, and every transaction in a <see cref="Grid{TItem}" /> (13 months, with column filters; a disputed
/// payment's row opens its dispute). For everyone, their disputes, their transfers in a <see cref="Grid{TItem}" /> (held and
/// scheduled first, then the most recent; the whole row opens the transfer's operations page), and the internal notes,
/// newest first with author and time, with an <see cref="EditForm" /> to add one (rule WR-002: required, 1–500 characters,
/// with a counter; customers never see notes). Both grids keep a saved view for the visit, with the column chooser and CSV
/// export. An unknown ID shows a not-found screen.
/// </summary>
public partial class OpsCustomer : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxNote = 500;

    private const string TransactionsResetText = "The transactions view is reset: newest first, 25 a page, and every column shown.";

    private const string TransfersResetText = "The transfers view is reset: waiting transfers first, 10 a page, and every column shown.";

    private bool focusNoteStatus;

    private string? loadedId;

    private EditContext noteContext = default!;

    private NoteForm noteForm = new();

    private InputTextArea? noteInput;

    private bool noteIsError;

    private int noteLength;

    private string? noteMessage;

    private ElementReference? noteStatus;

    private Grid<TransactionRow>? transactionsGrid;

    private int transactionsPageSize = 25;

    private int transactionsVersion;

    private ShowcaseGridView transactionsView = default!;

    private Grid<TransferRow>? transfersGrid;

    private int transfersPageSize = 10;

    private int transfersVersion;

    private ShowcaseGridView transfersView = default!;

    private string? viewMessage;

    private bool viewsLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await transfersView.LoadAsync();
            await transactionsView.LoadAsync();
            transfersPageSize = (await transfersView.GetSettingsAsync()).PageSize;
            transactionsPageSize = (await transactionsView.GetSettingsAsync()).PageSize;
            viewsLoaded = true;
            StateHasChanged();
        }

        if (focusNoteStatus)
        {
            focusNoteStatus = false;
            await FintechDisplay.FocusAsync(noteIsError ? noteInput?.Element : noteStatus);
        }
    }

    protected override void OnInitialized()
    {
        transfersView = GridViews.Get(State.SiteSlug, "ops-customer-transfers", 10, "status", SortDirection.Ascending);
        transactionsView = GridViews.Get(State.SiteSlug, "ops-customer-transactions", 25, "date", SortDirection.Descending);
        ResetNoteForm();
        State.Changed += OnStateChanged;
    }

    /// <summary>Another customer reuses the page: the note form and the grids start over for them.</summary>
    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        if (loadedId is not null)
        {
            transfersVersion++;
            transactionsVersion++;
        }

        loadedId = Id;
        noteMessage = null;
        noteIsError = false;
        ResetNoteForm();
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

    private void AddNote()
    {
        if (Customer is not { } customer || !noteContext.Validate())
            return;

        var result = State.AddCustomerNote(customer.Id, noteForm.Text.Trim());

        noteIsError = result != FintechResult.Ok;
        noteMessage = NoteResultText(result, customer.Name);

        if (result == FintechResult.Ok)
            ResetNoteForm();

        focusNoteStatus = true;
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridsAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<TransactionRow>> ProvideTransactionsAsync(GridDataProviderRequest<TransactionRow> request) => transactionsView.ApplyAsync(request, TransactionRows);

    private Task<GridDataProviderResult<TransferRow>> ProvideTransfersAsync(GridDataProviderRequest<TransferRow> request) => transfersView.ApplyAsync(request, TransferRows);

    private async Task RefreshGridsAsync()
    {
        if (transfersGrid is not null)
            await transfersGrid.RefreshDataAsync();

        if (transactionsGrid is not null)
            await transactionsGrid.RefreshDataAsync();
    }

    private void ResetNoteForm()
    {
        noteForm = new NoteForm();
        noteContext = new EditContext(noteForm);
        noteLength = 0;
    }

    private async Task ResetTransactionsViewAsync()
    {
        await transactionsView.ResetAsync();
        transactionsPageSize = 25;
        transactionsVersion++;
        viewMessage = viewMessage == TransactionsResetText ? TransactionsResetText + " (2)" : TransactionsResetText;
    }

    private async Task ResetTransfersViewAsync()
    {
        await transfersView.ResetAsync();
        transfersPageSize = 10;
        transfersVersion++;
        viewMessage = viewMessage == TransfersResetText ? TransfersResetText + " (2)" : TransfersResetText;
    }

    private async Task SaveTransactionsSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            transactionsPageSize = settings.PageSize;

        await transactionsView.SaveSettingsAsync(settings);
    }

    private async Task SaveTransfersSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            transfersPageSize = settings.PageSize;

        await transfersView.SaveSettingsAsync(settings);
    }

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

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets or sets the customer ID from the route.</summary>
    [Parameter] public string Id { get; set; } = string.Empty;

    private IReadOnlyList<FintechCustomerNote> Notes => State.NotesFor(Id);

    [Inject] private FintechState State { get; set; } = default!;

    /// <summary>Gets the demo customer's transactions up to today, newest first, as grid rows; nobody else's ledger is in the demo.</summary>
    private IReadOnlyList<TransactionRow> TransactionRows => Id != FintechSeedData.SeededCustomerId
        ? Array.Empty<TransactionRow>()
        : State.Transactions
            .Where(item => DateOnly.FromDateTime(item.Date) <= State.Today)
            .Select(item => new TransactionRow(item.Id, item.Date, item.Description, State.FindAccount(item.AccountId)?.Name ?? item.AccountId, State.FindCategory(item.CategoryId).Name,
                item.Amount, item.IsPending, item.DisputeId))
            .ToList();

    /// <summary>Gets the customer's transfers as grid rows: held ones, then scheduled ones by date, then the closed ones, newest first.</summary>
    private IReadOnlyList<TransferRow> TransferRows => State.AllTransfers
        .Where(transfer => transfer.CustomerId == Id)
        .Select(transfer => (Transfer: transfer, Order: transfer.Status switch { FintechTransferStatus.Held => 0, FintechTransferStatus.Scheduled => 1, _ => 2 }))
        .OrderBy(item => item.Order)
        .ThenBy(item => item.Order < 2 ? item.Transfer.Date.DayNumber : -(item.Transfer.ClosedAt ?? item.Transfer.CreatedAt).Ticks / TimeSpan.TicksPerDay)
        .Select((item, index) => new TransferRow(item.Transfer.Reference, item.Transfer.ToName, item.Transfer.Amount, Transfers.StatusText(item.Transfer.Status), item.Transfer.Status,
            index, TransferWhen(item.Transfer), item.Transfer.ClosedAt ?? item.Transfer.Date.ToDateTime(TimeOnly.MinValue),
            item.Transfer.HoldReason ?? item.Transfer.RejectReason ?? string.Empty))
        .ToList();

    #endregion

    #region Nested Types

    /// <summary>The note form's field: 1 to 500 characters.</summary>
    private sealed class NoteForm
    {
        [Required(ErrorMessage = "Write the note first: it can't be empty.")]
        [StringLength(MaxNote, ErrorMessage = "Keep the note to 500 characters.")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>One transaction as the grid shows and exports it.</summary>
    private sealed record TransactionRow(int Id, DateTime Date, string Description, string Account, string Category, decimal Amount, bool Pending, int? DisputeId);

    /// <summary>One transfer as the grid shows and exports it; <see cref="Order" /> keeps waiting transfers first.</summary>
    private sealed record TransferRow(string Reference, string To, decimal Amount, string Status, FintechTransferStatus StatusValue, int Order, string When, DateTime WhenAt, string Reason);

    #endregion
}
