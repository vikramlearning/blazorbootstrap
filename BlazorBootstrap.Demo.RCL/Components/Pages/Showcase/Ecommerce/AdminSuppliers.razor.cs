using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The suppliers (AC-7g) in a <see cref="Grid{TItem}" />: name, contact, email, lead time, currency, payment terms, how many
/// products prefer them, and their open purchase orders, with paging, sorting, the column chooser, and CSV export,
/// searched by name, contact, or email and filtered by currency; its view is saved for the visit and can be reset. A
/// supplier has no page of its own: the row's stretched button (or <c>?supplier=</c>, from the header's search) opens its
/// editor, which validates with one behavior (rule WR-002): a unique name checked while typing, a contact, an email's
/// form, a lead time of 1 to 90 days, a currency, and payment terms.
/// </summary>
public partial class AdminSuppliers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private readonly Dictionary<string, ElementReference> editButtons = new(StringComparer.Ordinal);

    private string? appliedQuery;

    private ElementReference? countRegion;

    private string currencyFilter = string.Empty;

    private EditContext editContext = default!;

    private string? editingId;

    private Modal editorModal = default!;

    private SupplierForm form = new();

    private Grid<SupplierRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastStatus;

    private ShowcaseUniqueCheck? nameCheck;

    private InputText? nameInput;

    private Button newButton = default!;

    /// <summary>The supplier <c>?supplier=</c> asks to open once the page is interactive.</summary>
    private string? openId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string? savedId;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private ValidationMessageStore serverErrors = default!;

    private string? statusMessage;

    private ElementReference statusRegion;

    private int statusRepeats;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive, then shows the grid; opens the supplier <c>?supplier=</c> names.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (openId is { } id)
        {
            openId = null;

            if (State.FindSupplier(id) is not null)
                await OpenEditorAsync(id);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "suppliers", DefaultPageSize, "name", SortDirection.Ascending);
        SetForm(new SupplierForm());
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        var query = string.IsNullOrWhiteSpace(SupplierQuery) ? null : SupplierQuery.Trim();

        if (query == appliedQuery)
            return;

        appliedQuery = query;
        openId = query is { Length: <= 12 } ? query : null;
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

    private static string NameTakenMessage(string name) => $"{name.Trim()} is already a supplier. Use a name no other supplier has.";

    private void Announce(string message)
    {
        statusRepeats = message == lastStatus ? statusRepeats + 1 : 1;
        lastStatus = message;
        statusMessage = statusRepeats == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusRepeats})");
    }

    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(State.IsSupplierNameTaken(name, editingId));

    private IEnumerable<EcommerceSupplier> Matching()
    {
        IEnumerable<EcommerceSupplier> suppliers = State.Suppliers;
        var term = search.Trim();

        if (term.Length > 0)
            suppliers = suppliers.Where(supplier => supplier.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || supplier.ContactName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || supplier.Email.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (currencyFilter.Length > 0)
            suppliers = suppliers.Where(supplier => supplier.Currency == currencyFilter);

        return suppliers;
    }

    private async Task OnCurrencyChanged(ChangeEventArgs args)
    {
        currencyFilter = args.Value?.ToString() is { } value && EcommerceSeedData.SupplierCurrencies.Contains(value) ? value : string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus: to the saved supplier's row (or the status after adding
    /// one), or back to the button that opened it.
    /// </summary>
    private async Task OnEditorHiddenAsync()
    {
        var original = editingId;
        var saved = savedId;

        editingId = null;
        savedId = null;
        SetForm(new SupplierForm());

        if (saved is not null && original is null)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else if ((saved ?? original) is { } id && editButtons.TryGetValue(id, out var button))
            await EcommerceDisplay.FocusAsync(button);
        else
            await EcommerceDisplay.FocusAsync(newButton.Element);
    }

    private Task OnEditorShownAsync() => EcommerceDisplay.FocusAsync(nameInput?.Element);

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args) => serverErrors.Clear(args.FieldIdentifier);

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Filters on every keystroke without changing the box's bound value, so older text is never written back into it.</summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenEditorAsync(string? supplierId)
    {
        editingId = supplierId;
        savedId = null;
        SetForm(State.FindSupplier(supplierId) is { } supplier
            ? new SupplierForm
            {
                Name = supplier.Name,
                ContactName = supplier.ContactName,
                Email = supplier.Email,
                LeadTimeDays = supplier.LeadTimeDays,
                Currency = supplier.Currency,
                PaymentTerms = supplier.PaymentTerms,
            }
            : new SupplierForm());

        await editorModal.ShowAsync();
    }

    /// <summary>The page of suppliers the grid asked for: by name unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SupplierRow>> ProvideRowsAsync(GridDataProviderRequest<SupplierRow> request) =>
        view.ApplyAsync(request, Matching().Select(supplier => new SupplierRow(
            supplier.Id,
            supplier.Name,
            supplier.ContactName,
            supplier.Email,
            supplier.LeadTimeDays,
            supplier.Currency,
            supplier.PaymentTerms,
            State.AllProducts.Count(product => State.ReplenishmentFor(product.Id).SupplierId == supplier.Id),
            State.PurchaseOrders.Count(order => order.SupplierId == supplier.Id && order.IsOpen))).ToList());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        Announce("The view is reset: suppliers by name, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>
    /// Saves once the form is valid: the name check finishes first, then every rule runs, and a failed save shows the
    /// summary (rule WR-002). The state checks again; a refusal shows on its field.
    /// </summary>
    private async Task SaveAsync()
    {
        serverErrors.Clear();
        editContext.NotifyValidationStateChanged();

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!editContext.Validate() || !nameFree)
            return;

        var input = new EcommerceSupplier(editingId ?? string.Empty, form.Name, form.ContactName, form.Email, form.LeadTimeDays!.Value, form.Currency, form.PaymentTerms);
        var result = State.SaveSupplier(input, out var saved);

        switch (result)
        {
            case EcommercePurchaseResult.Ok or EcommercePurchaseResult.NoChange when saved is not null:
                savedId = saved.Id;
                Announce(result == EcommercePurchaseResult.NoChange ? $"Nothing changed: {saved.Name} is already saved like this."
                    : editingId is null ? $"{saved.Name} is added. Set it as products' preferred supplier in their replenishment settings." : $"{saved.Name} is saved.");
                await editorModal.HideAsync();

                return;
            case EcommercePurchaseResult.Duplicate:
                serverErrors.Add(FieldIdentifier.Create(() => form.Name), NameTakenMessage(form.Name));
                break;
            case EcommercePurchaseResult.NotFound:
                serverErrors.Add(FieldIdentifier.Create(() => form.Name), "This supplier is gone, so it can't be saved. Cancel and add it again.");
                break;
            default:
                serverErrors.Add(FieldIdentifier.Create(() => form.Name), "The supplier couldn't be saved with these values. Check each field.");
                break;
        }

        editContext.NotifyValidationStateChanged();
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private void SetForm(SupplierForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private string MatchText
    {
        get
        {
            var count = Matching().Count();

            return count == State.Suppliers.Count ? $"Showing all {EcommerceDisplay.Plural(count, "supplier")}." : $"{count} of {EcommerceDisplay.Plural(State.Suppliers.Count, "supplier")} match.";
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    /// <summary>Gets or sets the ID of the supplier to open in the editor, from <c>?supplier=</c> (the header's search and the purchase-order pages).</summary>
    [SupplyParameterFromQuery(Name = "supplier")] public string? SupplierQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>The supplier editor's fields, with the limits of <see cref="EcommerceState.SaveSupplier" />.</summary>
    private sealed class SupplierForm
    {
        private string contactName = string.Empty;

        private string email = string.Empty;

        private string name = string.Empty;

        [Required(ErrorMessage = "Enter the contact's name.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Use 2 to 60 characters for the contact's name.")]
        public string ContactName { get => contactName; set => contactName = value?.Trim() ?? string.Empty; }

        [Required(ErrorMessage = "Choose the supplier's currency.")]
        public string Currency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the email purchase orders go to.")]
        [StringLength(120, ErrorMessage = "Use 120 characters or fewer for the email.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter an email like orders@example.com.")]
        public string Email { get => email; set => email = value?.Trim() ?? string.Empty; }

        [Required(ErrorMessage = "Enter the lead time, from 1 to 90 days.")]
        [Range(1, EcommerceState.MaxLeadTimeDays, ErrorMessage = "Enter a lead time from 1 to 90 days.")]
        public int? LeadTimeDays { get; set; } = 14;

        [Required(ErrorMessage = "Enter the company name.")]
        [StringLength(EcommerceState.MaxSupplierNameLength, MinimumLength = 2, ErrorMessage = "Use 2 to 60 characters for the company name.")]
        public string Name { get => name; set => name = value?.Trim() ?? string.Empty; }

        [Required(ErrorMessage = "Choose the payment terms.")]
        public string PaymentTerms { get; set; } = "Net 30";
    }

    /// <summary>One supplier as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record SupplierRow(string Id, string Name, string Contact, string Email, int LeadTimeDays, string Currency, string Terms, int Products, int OpenOrders);

    #endregion
}
