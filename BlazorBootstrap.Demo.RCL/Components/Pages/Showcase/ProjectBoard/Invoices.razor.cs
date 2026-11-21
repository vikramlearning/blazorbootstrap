using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Invoices (account managers, AC-7f): "Ready to invoice", each client's approved billable hours not on an invoice yet at
/// today's rates (<see cref="PbState.ReadyToInvoice" />), and every invoice, both in a <see cref="Grid{TItem}" /> with a saved
/// view, the column chooser, and CSV export. Invoices are searched by number or client and filtered by status (Draft,
/// Issued, Overdue, Paid), client, and "Only clients I own"; a row opens the invoice. "Create draft invoice" (or a client's
/// row) opens a dialog with an <see cref="EditForm" /> (rule WR-002: a client with ready hours, and the last day of hours,
/// not in the future and on or after the client's first ready hours) and a live preview of the lines grouped by project and
/// person with the total; creating it (<see cref="PbState.CreateDraftInvoice" />) opens the draft.
/// </summary>
public partial class Invoices : ComponentBase, IDisposable
{
    #region Fields and Constants

    internal const string ListUrl = PbLayout.Base + "/invoices";

    private const string DraftFilter = "draft";

    private const string OpenFilter = "open";

    private const string OverdueFilter = "overdue";

    private const string PaidFilter = "paid";

    private int clientFilter;

    private ElementReference? countRegion;

    private ElementReference? createButton;

    private Modal createModal = default!;

    private int? createdId;

    private EditContext editContext = default!;

    private bool focusStatus;

    private DraftForm form = new();

    private int formErrorCount;

    private string? formMessage;

    private string? formText;

    private Grid<InvoiceRow>? grid;

    private int gridVersion;

    private bool mineOnly;

    private int pageSize = 25;

    private Grid<ReadyRow>? readyGrid;

    private int readyPageSize = 5;

    private ShowcaseGridView readyView = default!;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    private ValidationMessageStore? serverErrors;

    private int statusCount;

    private string statusFilter = string.Empty;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            await readyView.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            readyPageSize = (await readyView.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "invoices", 25, null, SortDirection.None);
        readyView = GridViews.Get(State.SiteSlug, "ready-to-invoice", 5, "amount", SortDirection.Descending);
        ResetForm(new DraftForm());
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the route of an invoice.</summary>
    internal static string InvoiceUrl(int invoiceId) => FormattableString.Invariant($"{ListUrl}/{invoiceId}");

    /// <summary>Returns an invoice's status in words: Draft, Issued, Overdue, or Paid.</summary>
    internal static string StatusWord(PbState state, PbInvoice invoice) => state.IsOverdue(invoice) ? "Overdue" : invoice.Status.ToString();

    /// <summary>Returns the badge color of an invoice status word (always shown with the word).</summary>
    internal static string StatusColor(string status) => status switch
    {
        "Draft" => "secondary",
        "Overdue" => "danger",
        "Paid" => "success",
        _ => "info",
    };

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string Money(string currency, decimal amount) => string.Create(CultureInfo.InvariantCulture, $"{currency} {amount:#,0.00}");

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = string.Empty;
        clientFilter = 0;
        mineOnly = false;
        await FiltersChangedAsync();
        await PbDisplay.FocusAsync(countRegion);
    }

    /// <summary>
    /// Creates the draft once the form is valid (the page tells the form the client's first ready day; a failed submit shows
    /// the summary); the state checks again, and its answer maps to a field or the dialog's alert.
    /// </summary>
    private async Task CreateAsync()
    {
        form.FirstReady = State.ReadyToInvoice().FirstOrDefault(item => item.Client.Id == form.ClientId)?.From;
        form.Today = State.Clock.Today;
        serverErrors?.Clear();

        if (!editContext.Validate())
            return;

        var result = State.CreateDraftInvoice(form.ClientId, form.UpTo!.Value, out var created);

        switch (result)
        {
            case PbResult.Ok when created is not null:
                createdId = created.Id;
                await createModal.HideAsync();

                return;

            case PbResult.NoChange:
                serverErrors?.Add(FieldIdentifier.Create(() => form.UpTo), "There are no approved billable hours for this client up to that day. Choose a later day.");
                editContext.NotifyValidationStateChanged();

                return;

            case PbResult.NotFound:
                serverErrors?.Add(FieldIdentifier.Create(() => form.ClientId), "That client was deleted meanwhile. Choose another.");
                editContext.NotifyValidationStateChanged();

                return;

            case PbResult.NotAllowed:
                ShowFormError("Only account managers can create invoices. Nothing was created.");

                return;

            default:
                ShowFormError("The draft couldn't be created: the storage for this demo tab may be full. Nothing was created.");

                return;
        }
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private IEnumerable<InvoiceRow> Matching()
    {
        var term = search.Trim();
        var clients = State.Clients.ToDictionary(client => client.Id);
        var me = State.CurrentPerson?.Id;

        foreach (var invoice in State.Invoices)
        {
            var client = clients.GetValueOrDefault(invoice.ClientId);
            var number = invoice.Number ?? "Draft";
            var status = StatusWord(State, invoice);

            if (term.Length > 0 && !(number.Contains(term, StringComparison.OrdinalIgnoreCase) || (client?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
                continue;

            if (statusFilter.Length > 0 && statusFilter != (status == "Issued" ? OpenFilter : status.ToLowerInvariant()))
                continue;

            if (clientFilter != 0 && invoice.ClientId != clientFilter)
                continue;

            if (mineOnly && client?.OwnerId != me)
                continue;

            var total = PbState.TotalOf(invoice);

            yield return new InvoiceRow(invoice.Id, number, client?.Name ?? "Deleted client", status, status switch { "Draft" => 0, "Overdue" => 1, "Issued" => 2, _ => 3 },
                invoice.IssuedOn, invoice.IssuedOn is { } issued ? PbDisplay.Date(issued) : "Not issued", invoice.DueOn,
                invoice.DueOn is { } due ? $"{PbDisplay.Date(due)} (Net {invoice.TermsDays})" : "None yet", total, Money(invoice.Currency, total), invoice.HoursUpTo,
                PbDisplay.Date(invoice.HoursUpTo), invoice.PaidOn, invoice.PaidOn is { } paid ? PbDisplay.Date(paid) : "Not paid");
        }
    }

    /// <summary>Opens the draft once the dialog is hidden; a cancel returns focus to the button that opened it.</summary>
    private void OnCreateHidden()
    {
        if (createdId is { } id)
        {
            createdId = null;
            Navigation.NavigateTo(InvoiceUrl(id) + "?created=1");

            return;
        }

        focusStatus = false;
        _ = PbDisplay.FocusAsync(createButton);
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridsAsync();
        StateHasChanged();
    });

    /// <summary>Opens the draft dialog, for a client when given, with hours up to today.</summary>
    private async Task OpenCreateAsync(int? clientId)
    {
        var ready = State.ReadyToInvoice();

        if (ready.Count == 0)
        {
            SetStatus("There are no approved billable hours to invoice yet. Hours are ready once a project manager approves a week.", true);
            focusStatus = true;

            return;
        }

        ResetForm(new DraftForm { ClientId = clientId ?? (ready.Count == 1 ? ready[0].Client.Id : 0), UpTo = State.Clock.Today });
        formMessage = formText = null;
        formErrorCount = 0;
        await createModal.ShowAsync();
    }

    private Task<GridDataProviderResult<InvoiceRow>> ProvideInvoicesAsync(GridDataProviderRequest<InvoiceRow> request) => view.ApplyAsync(request, Shown);

    private Task<GridDataProviderResult<ReadyRow>> ProvideReadyAsync(GridDataProviderRequest<ReadyRow> request) =>
        readyView.ApplyAsync(request, State.ReadyToInvoice().Select(item => new ReadyRow(item.Client.Id, item.Client.Name, State.FindPerson(item.Client.OwnerId)?.Name ?? "Nobody", item.Hours,
            item.Amount, Money(item.Client.BillingCurrency, item.Amount), item.From, $"{PbDisplay.Date(item.From)} to {PbDisplay.Date(item.To)}")).ToList());

    private async Task RefreshGridsAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();

        if (readyGrid is not null)
            await readyGrid.RefreshDataAsync();
    }

    private void ResetForm(DraftForm next)
    {
        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) =>
        {
            serverErrors?.Clear(args.FieldIdentifier);
            formMessage = formText = null;
        };
    }

    private async Task ResetViewsAsync()
    {
        await view.ResetAsync();
        await readyView.ResetAsync();
        pageSize = 25;
        readyPageSize = 5;
        gridVersion++;
        SetStatus("The views are reset: invoices newest first, 25 a page; ready to invoice by amount, 5 a page; every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await PbDisplay.FocusAsync(countRegion);
    }

    private async Task SaveReadySettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            readyPageSize = settings.PageSize;

        await readyView.SaveSettingsAsync(settings);
    }

    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private void ShowFormError(string message)
    {
        formErrorCount = message == formText ? formErrorCount + 1 : 1;
        formText = message;
        formMessage = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || clientFilter != 0 || mineOnly;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<InvoiceRow> Shown => Matching().ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The draft invoice's fields: a client (required) and the last day of hours (required, not in the future, and not before
    /// the client's first ready hours: cross-field rules against values the page sets before it validates).
    /// </summary>
    private sealed class DraftForm : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "Choose the client to invoice.")]
        public int ClientId { get; set; }

        /// <summary>The client's first ready day, or <see langword="null" /> when it has no ready hours; set by the page.</summary>
        public DateTime? FirstReady { get; set; }

        /// <summary>The board's today, set by the page.</summary>
        public DateTime Today { get; set; } = DateTime.MaxValue;

        [Required(ErrorMessage = "Pick the last day of hours to bill.")]
        public DateTime? UpTo { get; set; }

        /// <summary>Runs once the fields are filled in: the client has ready hours, and the day is in range.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ClientId != 0 && FirstReady is null)
                yield return new ValidationResult("That client has no approved billable hours to invoice. Choose another.", new[] { nameof(ClientId) });

            if (UpTo is { } day && day.Date > Today.Date)
                yield return new ValidationResult("Pick today or an earlier day: only hours already worked are billed.", new[] { nameof(UpTo) });
            else if (UpTo is { } early && FirstReady is { } first && early.Date < first.Date)
                yield return new ValidationResult($"The client's first ready hours are from {PbDisplay.Date(first)}. Pick that day or later.", new[] { nameof(UpTo) });
        }
    }

    /// <summary>One invoice as the grid shows and exports it.</summary>
    private sealed record InvoiceRow(
        int Id,
        string Number,
        string Client,
        string Status,
        int StatusOrder,
        DateTime? Issued,
        string IssuedText,
        DateTime? Due,
        string DueText,
        decimal Total,
        string TotalText,
        DateTime UpTo,
        string UpToText,
        DateTime? Paid,
        string PaidText);

    /// <summary>One client's ready hours as the grid shows and exports them.</summary>
    private sealed record ReadyRow(int ClientId, string Client, string Owner, decimal Hours, decimal Amount, string AmountText, DateTime From, string Span);

    #endregion
}
