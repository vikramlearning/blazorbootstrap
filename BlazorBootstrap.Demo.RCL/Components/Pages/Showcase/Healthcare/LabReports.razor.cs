using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's lab tests in a <see cref="Grid{TItem}" />: the test (a released report's row opens the report),
/// its status in words, how many results are out of range, when it was ordered and released, and the ordering doctor,
/// newest first, with paging, sorting, the column chooser, and CSV export. Tests still waiting for results show no values
/// until the doctor releases them. A filter shows the reports ready to view or the ones waiting; the grid's view is saved
/// for the visit and can be reset.
/// </summary>
public partial class LabReports : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private const string ReadyFilter = "ready";

    private const string WaitingFilter = "waiting";

    private ElementReference? countRegion;

    private Grid<ReportRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string readyFilter = string.Empty;

    private int statusCount;

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
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "my-lab-reports", DefaultPageSize, "ordered", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns how many of a released order's results are below or above their reference range.</summary>
    internal static int OutOfRange(HcLabOrder order, HcLabPanel? panel) =>
        panel is null || order.Status != HcLabStatus.Released ? 0 : panel.Analytes.Zip(order.Values).Count(pair => HcState.Flag(pair.First, pair.Second) != "Normal");

    /// <summary>Returns the out-of-range count in words, like "All results in range" or "2 results out of range".</summary>
    internal static string RangeText(int outOfRange) => outOfRange switch
    {
        0 => "All results in range",
        1 => "1 result out of range",
        _ => string.Create(CultureInfo.InvariantCulture, $"{outOfRange} results out of range"),
    };

    #endregion

    #region Private Methods

    private string CountText(int total)
    {
        var shown = Shown.Count;
        var count = total == 1 ? "1 lab test" : string.Create(CultureInfo.InvariantCulture, $"{total} lab tests");

        return shown == total ? $"{count}." : string.Create(CultureInfo.InvariantCulture, $"Showing {shown} of {count}.");
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnReadyChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        readyFilter = value is ReadyFilter or WaitingFilter ? value : string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the patient's lab orders, newest first.</summary>
    private IReadOnlyList<HcLabOrder> Orders() => State.CurrentPatient is { } patient ? State.LabOrdersOf(patient.Id) : Array.Empty<HcLabOrder>();

    /// <summary>The page of reports the grid asked for: newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ReportRow>> ProvideReportsAsync(GridDataProviderRequest<ReportRow> request) => view.ApplyAsync(request, Shown);

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
        statusCount = statusText == "reset" ? statusCount + 1 : 1;
        statusText = "reset";
        statusMessage = "The view is reset: newest first, 10 reports a page, and every column shown." + (statusCount == 1 ? null : string.Create(CultureInfo.InvariantCulture, $" ({statusCount})"));
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>The patient's lab tests that pass the filter, as grid rows.</summary>
    private IReadOnlyList<ReportRow> Shown =>
        Orders()
            .Where(order => readyFilter switch { ReadyFilter => order.Status == HcLabStatus.Released, WaitingFilter => order.Status != HcLabStatus.Released, _ => true })
            .Select(order =>
            {
                var panel = State.LabPanel(order.PanelId);
                var ready = order.Status == HcLabStatus.Released;
                var outside = ready ? OutOfRange(order, panel) : -1;

                return new ReportRow(
                    order.Id,
                    panel?.Name ?? "Lab test",
                    ready,
                    ready ? "Ready to view" : "Waiting for results",
                    outside,
                    ready ? RangeText(outside) : "Shown once your doctor releases them",
                    order.OrderedAt,
                    HcDisplay.Date(order.OrderedAt),
                    order.ReleasedAt,
                    order.ReleasedAt is { } released ? HcDisplay.Date(released) : "Not yet",
                    State.FindDoctor(order.DoctorId)?.Name ?? "Your doctor");
            })
            .ToList();

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One lab test as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record ReportRow(
        int Id,
        string Test,
        bool IsReady,
        string Status,
        int OutOfRange,
        string Results,
        DateTime Ordered,
        string OrderedText,
        DateTime? Released,
        string ReleasedText,
        string Doctor);

    #endregion
}
