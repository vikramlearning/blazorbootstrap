using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The Lab results tab of a patient's chart: every lab order in a <see cref="Grid{TItem}" /> (the test, linking to the
/// order; number, when and by whom it was ordered, status in words, how many results are out of range, and when it was
/// released), newest first, with paging, sorting, the column chooser, and CSV export. The values stay on the order page.
/// The grid's view is saved for the visit and can be reset.
/// </summary>
public partial class PatientChartLabs : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private Grid<LabRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

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
        view = GridViews.Get(State.SiteSlug, "chart-labs", DefaultPageSize, "ordered", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of lab orders the grid asked for: newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<LabRow>> ProvideLabsAsync(GridDataProviderRequest<LabRow> request)
    {
        var doctors = State.Doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);
        var me = State.CurrentDoctor?.Id;
        var rows = State.LabOrdersOf(PatientId).Select(order =>
        {
            var panel = State.LabPanel(order.PanelId);
            var outside = panel is null || order.Values.Count != panel.Analytes.Count ? -1 : panel.Analytes.Zip(order.Values).Count(pair => HcState.Flag(pair.First, pair.Second) != "Normal");

            return new LabRow(
                order.Id,
                panel?.Name ?? order.PanelId,
                order.OrderedAt,
                HcDisplay.Date(order.OrderedAt),
                (doctors.GetValueOrDefault(order.DoctorId) ?? "A former doctor") + (order.DoctorId == me ? " (you)" : null),
                Labs.StatusLabel(order.Status),
                order.Status,
                outside,
                outside < 0 ? "Not entered yet" : LabReports.RangeText(outside),
                order.ReleasedAt,
                order.ReleasedAt is { } released ? HcDisplay.Date(released) : "Not released");
        });

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the announcement.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The lab orders view is reset: newest first, 10 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets or sets the patient whose lab orders are listed.</summary>
    [Parameter, EditorRequired] public int PatientId { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One lab order as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record LabRow(
        int Id,
        string Test,
        DateTime Ordered,
        string OrderedText,
        string Doctor,
        string Status,
        HcLabStatus StatusValue,
        int OutOfRange,
        string Results,
        DateTime? Released,
        string ReleasedText);

    #endregion
}
