using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in doctor's lab work (<see cref="HcState.MyLabWork" />) in a <see cref="Grid{TItem}" />: the test (the row opens
/// the order, <see cref="LabOrder" />, to enter, correct, and release its results), order number, patient and record number,
/// when it was ordered, status in words, how many results are out of range, and when they were entered and released, oldest
/// first, with paging, sorting, the column chooser, and CSV export. The status filter shows what's to do (waiting for
/// results or entered, the default), one status, or all, and a search narrows by patient, record number, test, or order
/// number; the grid's view is saved for the visit and can be reset. Selected entered results can be released together after
/// a confirmation (<see cref="HcState.BulkReleaseLabs" />). <c>?order={id}</c> (older links) opens that order.
/// </summary>
public partial class Labs : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    /// <summary>The status filter's value for everything not released yet.</summary>
    private const string ToDoFilter = "todo";

    private ElementReference? countRegion;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<OrderRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>Whether Release was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingRelease;

    private ElementReference? releaseCancel;

    /// <summary>The orders the Release dialog is about.</summary>
    private List<int> releaseIds = new();

    private Modal releaseModal = default!;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The orders selected in the grid (on its current page).</summary>
    private List<OrderRow> selected = new();

    private int statusCount;

    private string statusFilter = ToDoFilter;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HcDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "lab-orders", DefaultPageSize, "ordered", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    /// <summary>An older link with <c>?order={id}</c> opens that order's page.</summary>
    protected override void OnParametersSet()
    {
        if (int.TryParse(OrderQuery, NumberStyles.None, CultureInfo.InvariantCulture, out var orderId))
            Navigation.NavigateTo(HcDisplay.LabOrderUrl(orderId), replace: true);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the staff route of one lab order.</summary>
    public static string OrderUrl(int orderId) => HcDisplay.LabOrderUrl(orderId);

    /// <summary>Returns an analyte's reference range like "4–11 ×10⁹/L" or "Up to 200 mg/dL".</summary>
    public static string RangeText(HcAnalyte analyte) =>
        analyte.Low <= 0 ? $"Up to {HcDisplay.Number(analyte.High)} {analyte.Unit}" : $"{HcDisplay.Number(analyte.Low)}–{HcDisplay.Number(analyte.High)} {analyte.Unit}";

    /// <summary>Returns the Bootstrap color of a lab status (always shown with its words).</summary>
    public static string StatusColor(HcLabStatus status) => status switch
    {
        HcLabStatus.Ordered => "secondary",
        HcLabStatus.Resulted => "warning",
        _ => "success",
    };

    /// <summary>Returns a lab status in words for staff.</summary>
    public static string StatusLabel(HcLabStatus status) => status switch
    {
        HcLabStatus.Ordered => "Waiting for results",
        HcLabStatus.Resulted => "Entered, not released",
        _ => "Released to the patient",
    };

    #endregion

    #region Private Methods

    private async Task AskReleaseAsync()
    {
        releaseIds = selected.Where(row => row.StatusValue == HcLabStatus.Resulted).Select(row => row.Id).ToList();
        pendingRelease = false;
        StateHasChanged();

        await releaseModal.ShowAsync();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = ToDoFilter;
        await FiltersChangedAsync();
        await HcDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<OrderRow>();
        SetStatus("Selection cleared.", false);
        focusTarget = () => statusRegion;
    }

    private async Task ConfirmReleaseAsync()
    {
        pendingRelease = true;

        await releaseModal.HideAsync();
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return shown == total ? $"{HcDisplay.Plural(total, "order", "orders")}." : $"Showing {shown.ToString(CultureInfo.InvariantCulture)} of {HcDisplay.Plural(total, "order", "orders")}.";
    }

    /// <summary>A filter changed: back to the first page of the filtered orders.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Releases the confirmed orders now that the dialog is hidden, or returns focus to the status line.</summary>
    private async Task OnReleaseHiddenAsync()
    {
        var ids = releaseIds;
        var accepted = pendingRelease;
        pendingRelease = false;
        releaseIds = new List<int>();

        if (!accepted)
        {
            focusTarget = () => statusRegion;
            StateHasChanged();

            return;
        }

        var outcome = State.BulkReleaseLabs(ids);

        SetStatus(outcome.Result switch
        {
            HcResult.Ok => $"Released {HcDisplay.Plural(outcome.Changed, "result", "results")}: each patient sees theirs in Lab reports and was billed {HcDisplay.Money(HcSeedData.LabFeeCents)}."
                + (outcome.Unchanged > 0 ? $" {HcDisplay.Plural(outcome.Unchanged, "order was", "orders were")} left as they were: released already, or the storage for this demo tab is full." : string.Empty),
            HcResult.NoChange => "Nothing was released: those orders were released already, or the storage for this demo tab is full.",
            HcResult.NotAllowed => "Only the ordering doctor can release results. Sign in again; nothing was released.",
            _ => "Those results couldn't be released. Nothing changed.",
        }, outcome.Result is not (HcResult.Ok or HcResult.NoChange));

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<OrderRow>();
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    private Task OnReleaseShownAsync() => HcDisplay.FocusAsync(releaseCancel);

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    /// <summary>Filters on every keystroke without changing the box's bound value, so a slower render never overwrites newer typing.</summary>
    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<OrderRow> items) => selected = items.ToList();

    /// <summary>Another screen or a change here changed the state: the grid reads the orders again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        statusFilter = value == ToDoFilter || Enum.TryParse<HcLabStatus>(value, out _) ? value : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>The page of orders the grid asked for: oldest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<OrderRow>> ProvideOrdersAsync(GridDataProviderRequest<OrderRow> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<OrderRow>();
        gridVersion++;
        SetStatus("The view is reset: oldest order first, 25 a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
    }

    private string RowClass(OrderRow row) => selected.Any(item => item.Id == row.Id) ? "hc-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter != ToDoFilter;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the order to open, from <c>?order=</c> (older links).</summary>
    [SupplyParameterFromQuery(Name = "order")] public string? OrderQuery { get; set; }

    /// <summary>The signed-in doctor's orders that pass the filters, as grid rows.</summary>
    private IReadOnlyList<OrderRow> Shown
    {
        get
        {
            var term = search.Trim();
            var number = term.TrimStart('#').Trim();

            return State.MyLabWork
                .Where(order => statusFilter switch
                {
                    ToDoFilter => order.Status != HcLabStatus.Released,
                    "" => true,
                    _ => order.Status.ToString() == statusFilter,
                })
                .Select(order => (Order: order, Patient: State.FindPatient(order.PatientId), Panel: State.LabPanel(order.PanelId)))
                .Where(item => term.Length == 0 || item.Order.Id.ToString(CultureInfo.InvariantCulture) == number || (item.Panel?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (item.Patient is not null && (item.Patient.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || item.Patient.Mrn.Contains(term, StringComparison.OrdinalIgnoreCase))))
                .Select(item =>
                {
                    var outside = item.Panel is null || item.Order.Values.Count != item.Panel.Analytes.Count ? -1
                        : item.Panel.Analytes.Zip(item.Order.Values).Count(pair => HcState.Flag(pair.First, pair.Second) != "Normal");

                    return new OrderRow(
                        item.Order.Id,
                        item.Panel?.Name ?? item.Order.PanelId,
                        item.Patient?.Name ?? "A former patient",
                        item.Patient?.Mrn ?? string.Empty,
                        item.Order.OrderedAt,
                        HcDisplay.When(item.Order.OrderedAt),
                        StatusLabel(item.Order.Status),
                        item.Order.Status,
                        outside,
                        outside < 0 ? "Not entered yet" : LabReports.RangeText(outside),
                        item.Order.ResultedAt,
                        item.Order.ResultedAt is { } entered ? HcDisplay.Date(entered) : "Not yet",
                        item.Order.ReleasedAt,
                        item.Order.ReleasedAt is { } released ? HcDisplay.Date(released) : "Not yet");
                })
                .ToList();
        }
    }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One lab order as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record OrderRow(
        int Id,
        string Test,
        string Patient,
        string Mrn,
        DateTime Ordered,
        string OrderedText,
        string Status,
        HcLabStatus StatusValue,
        int OutOfRange,
        string Results,
        DateTime? Entered,
        string EnteredText,
        DateTime? Released,
        string ReleasedText);

    #endregion
}
