using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Logs: a searchable, sortable, paged grid with service and level filters. The page shows a <see cref="Spinner" />
/// while it "loads" (a short pause after the first render and on Refresh, which also fetches the newest lines).
/// <c>?service=</c> preselects a service.
/// </summary>
public partial class Logs : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int LoadMilliseconds = 600;

    private string? appliedQuery;

    private bool disposed;

    private Grid<MonitoringLogEntry>? grid;

    private string levelFilter = string.Empty;

    private bool loading = true;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private string serviceFilter = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // A short simulated load after the first render, so the page shows its spinner; prerendering is not delayed.
        if (firstRender)
        {
            await Task.Delay(LoadMilliseconds);

            if (disposed)
                return;

            loading = false;
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>
    /// A new <c>?service=</c> value (also on a link from this page to itself) selects that service once and reloads the
    /// grid, which does not re-query a <c>DataProvider</c> on its own.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (ServiceQuery == appliedQuery)
            return;

        appliedQuery = ServiceQuery;

        if (State.FindService(ServiceQuery) is { } service && serviceFilter != service.Id)
        {
            serviceFilter = service.Id;
            await ResetGridAsync();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose()
    {
        disposed = true;
        State.Changed -= OnStateChanged;
    }

    #endregion

    #region Private Methods

    private IEnumerable<MonitoringLogEntry> Filtered()
    {
        var term = search.Trim();

        return State.Logs.Where(entry =>
            (serviceFilter.Length == 0 || entry.ServiceId == serviceFilter)
            && (levelFilter.Length == 0 || entry.Level.ToString() == levelFilter)
            && (term.Length == 0 || entry.Message.Contains(term, StringComparison.OrdinalIgnoreCase) || entry.TraceId.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private Task<GridDataProviderResult<MonitoringLogEntry>> LogsDataProvider(GridDataProviderRequest<MonitoringLogEntry> request) =>
        Task.FromResult(request.ApplyTo(Filtered().ToList()));

    private async Task OnLevelChangedAsync(string value)
    {
        if (levelFilter == value)
            return;

        levelFilter = value;
        await ResetGridAsync();
    }

    private async Task OnSearchChangedAsync(string value)
    {
        searchBoxValue = value;
        await OnSearchValueAsync(value);
    }

    private Task OnSearchInputAsync(ChangeEventArgs args) => OnSearchValueAsync(args.Value?.ToString() ?? string.Empty);

    private async Task OnSearchValueAsync(string value)
    {
        if (search == value)
            return;

        search = value;
        await ResetGridAsync();
    }

    private async Task OnServiceChangedAsync(ChangeEventArgs args)
    {
        serviceFilter = args.Value?.ToString() ?? string.Empty;
        await ResetGridAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        if (grid is not null)
            await grid.RefreshDataAsync();

        StateHasChanged();
    });

    /// <summary>Shows the spinner briefly while the newest lines are "fetched". The button stays enabled (so it keeps focus) and ignores presses meanwhile.</summary>
    private async Task RefreshAsync()
    {
        if (loading)
            return;

        loading = true;
        StateHasChanged();

        await Task.Delay(LoadMilliseconds);

        if (disposed)
            return;

        State.RefreshLogs();
        loading = false;
        StateHasChanged();
    }

    private async Task ResetGridAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();

        StateHasChanged();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText
    {
        get
        {
            var count = Filtered().Count();

            return count == 1 ? "1 line" : $"{count} lines";
        }
    }

    [Inject] private MonitoringState State { get; set; } = default!;

    /// <summary>Gets or sets the service to preselect, from <c>?service=</c>.</summary>
    [SupplyParameterFromQuery(Name = "service")] public string? ServiceQuery { get; set; }

    #endregion
}
