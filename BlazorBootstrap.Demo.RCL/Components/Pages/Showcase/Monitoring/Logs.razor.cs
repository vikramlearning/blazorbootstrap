using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Logs and traces: a live tail of every service's log lines, newest first, filtered by service, level, and a search in
/// the message or trace ID, in a <see cref="Grid{TItem}" /> with paging, sorting, the column chooser, and CSV export of the
/// filtered lines. The grid's view is saved for the visit and can be reset. Each line opens its trace. A
/// <see cref="MonitoringLiveTimer" /> moves the shown window every tick unless live updates are paused (the shared
/// <see cref="MonitoringState.IsPaused" />), and the grid reads the lines again, so new lines arrive at the top. The status
/// region stays quiet on ticks: it speaks when the filters change, and once after a pause with how many lines arrived
/// meanwhile. While paused, "Load new lines now" fetches the newest lines once. <c>?service=</c> preselects a service and
/// <c>?search=</c> a search (the trace page links its lines this way).
/// </summary>
public partial class Logs : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 50;

    private string? appliedSearchQuery;

    private string? appliedServiceQuery;

    private (IReadOnlyList<MonitoringLogEntry> Source, string Service, string Level, string Search, List<MonitoringLogEntry> Lines)? cache;

    private ElementReference? countRegion;

    private bool disposed;

    private Grid<MonitoringLogEntry>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string levelFilter = string.Empty;

    /// <summary>The newest line shown when live updates were paused, so resuming can say how many lines arrived since.</summary>
    private MonitoringLogEntry? newestAtPause;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    private ElementReference? searchInput;

    private string serviceFilter = string.Empty;

    private string? statusMessage;

    private MonitoringLiveTimer? timer;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    /// <summary>Whether the page last saw live updates paused.</summary>
    private bool wasPaused;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive, shows the grid, and starts the live timer.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || disposed)
            return;

        await view.LoadAsync();
        pageSize = (await view.GetSettingsAsync()).PageSize;
        viewLoaded = true;
        StateHasChanged();
        timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "logs", DefaultPageSize, "time", SortDirection.Descending);
        State.Changed += OnStateChanged;
        wasPaused = State.IsPaused;

        if (wasPaused)
            newestAtPause = State.Logs.FirstOrDefault();
    }

    /// <summary>A new <c>?service=</c> or <c>?search=</c> value (also on a link from this page to itself) applies once.</summary>
    protected override async Task OnParametersSetAsync()
    {
        var changed = false;

        if (ServiceQuery != appliedServiceQuery)
        {
            appliedServiceQuery = ServiceQuery;
            serviceFilter = State.FindService(ServiceQuery)?.Id ?? string.Empty;
            changed = true;
        }

        if (SearchQuery != appliedSearchQuery)
        {
            appliedSearchQuery = SearchQuery;
            search = (SearchQuery ?? string.Empty).Trim();
            changed = true;
        }

        if (changed && grid is not null)
            await grid.ResetPageNumber();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops the live timer and the state subscription, so leaving the page stops all updates.</summary>
    public void Dispose()
    {
        disposed = true;
        timer?.Dispose();
        State.Changed -= OnStateChanged;
    }

    #endregion

    #region Private Methods

    private static IconName LevelIcon(MonitoringLogLevel level) => level switch
    {
        MonitoringLogLevel.Error => IconName.XCircleFill,
        MonitoringLogLevel.Warning => IconName.ExclamationTriangleFill,
        _ => IconName.InfoCircleFill,
    };

    private static string LevelLabel(MonitoringLogLevel level) => level switch
    {
        MonitoringLogLevel.Error => "Error",
        MonitoringLogLevel.Warning => "Warning",
        _ => "Info",
    };

    private static string Lines(int count) => count == 1 ? "1 line" : string.Create(CultureInfo.InvariantCulture, $"{count} lines");

    /// <summary>The link disappears with the filters, so focus moves to the search box.</summary>
    private async Task ClearFiltersAsync()
    {
        search = string.Empty;
        serviceFilter = string.Empty;
        levelFilter = string.Empty;
        await FiltersChangedAsync();
        await MonitoringDisplay.FocusAsync(searchInput);
    }

    /// <summary>Counts the lines newer than <paramref name="newest" />, in all and among those matching the filters.</summary>
    private (int All, int Matching) CountNewerThan(MonitoringLogEntry? newest)
    {
        if (newest is null)
            return (State.Logs.Count, Filtered.Count);

        bool IsNewer(MonitoringLogEntry entry) => entry.Time > newest.Time || (entry.Time == newest.Time && entry.Id > newest.Id);

        return (State.Logs.TakeWhile(IsNewer).Count(), Filtered.TakeWhile(IsNewer).Count());
    }

    /// <summary>Starts again from the first page and announces the new count (filter changes are the only announced counts).</summary>
    private async Task FiltersChangedAsync()
    {
        statusMessage = $"{Lines(Filtered.Count)} match.";

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Fetches the newest lines once while paused (the person asked for it, so it is not an automatic update).</summary>
    private async Task LoadNewLinesAsync()
    {
        var before = State.Logs.FirstOrDefault();
        State.Tick();
        var (all, matching) = CountNewerThan(before);
        newestAtPause = State.Logs.FirstOrDefault();
        statusMessage = all == 0 ? "No new lines yet." : $"Loaded {NewText(all, matching)}. Live updates are still paused.";
        await RefreshGridAsync();
    }

    private string NewText(int all, int matching) =>
        (all == 1 ? "1 new line" : string.Create(CultureInfo.InvariantCulture, $"{all} new lines")) + (HasFilters ? $", {matching} matching your filters" : string.Empty);

    private async Task OnLevelChangedAsync(ChangeEventArgs args)
    {
        levelFilter = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        search = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    private async Task OnServiceChangedAsync(ChangeEventArgs args)
    {
        serviceFilter = args.Value?.ToString() ?? string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>Another screen changed the state (an incident, a service): the grid reads the lines again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>
    /// Moves the window on while live, and the grid reads the newest lines. Pausing (from the shared button) is noticed
    /// here: the newest line is remembered, and the first tick after resuming announces how many lines arrived in between.
    /// </summary>
    private async Task OnTickAsync()
    {
        if (disposed)
            return;

        try
        {
            await InvokeAsync(async () =>
            {
                if (disposed)
                    return;

                if (State.IsPaused)
                {
                    if (!wasPaused)
                    {
                        wasPaused = true;
                        newestAtPause = State.Logs.FirstOrDefault();
                        StateHasChanged();
                    }

                    return;
                }

                State.Tick();

                if (wasPaused)
                {
                    wasPaused = false;
                    var (all, matching) = CountNewerThan(newestAtPause);
                    statusMessage = all == 0 ? "No new lines since you paused." : $"{NewText(all, matching)} since you paused.";
                    newestAtPause = null;
                }

                await RefreshGridAsync();
                StateHasChanged();
            });
        }
        catch (Exception ex) when (ex is JSDisconnectedException or ObjectDisposedException or TaskCanceledException)
        {
            // The page or circuit went away between ticks.
        }
    }

    /// <summary>The page of lines the grid asked for, from the filtered lines, newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<MonitoringLogEntry>> ProvideLinesAsync(GridDataProviderRequest<MonitoringLogEntry> request) => view.ApplyAsync(request, Filtered);

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
        statusMessage = "The view is reset: newest first, 50 lines a page, and every column shown.";
        StateHasChanged();

        await Task.Yield();
        await MonitoringDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText => HasFilters ? $"{Lines(Filtered.Count)} of {Lines(State.Logs.Count)} match" : Lines(Filtered.Count);

    /// <summary>Gets the lines matching the filters, newest first; recomputed only when the window or a filter changes.</summary>
    private List<MonitoringLogEntry> Filtered
    {
        get
        {
            var source = State.Logs;
            var term = search.Trim();

            if (cache is { } hit && ReferenceEquals(hit.Source, source) && hit.Service == serviceFilter && hit.Level == levelFilter && hit.Search == term)
                return hit.Lines;

            var lines = source.Where(entry =>
                    (serviceFilter.Length == 0 || entry.ServiceId == serviceFilter)
                    && (levelFilter.Length == 0 || entry.Level.ToString() == levelFilter)
                    && (term.Length == 0 || entry.Message.Contains(term, StringComparison.OrdinalIgnoreCase) || entry.TraceId.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            cache = (source, serviceFilter, levelFilter, term, lines);

            return lines;
        }
    }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool HasFilters => search.Trim().Length > 0 || serviceFilter.Length > 0 || levelFilter.Length > 0;

    /// <summary>Gets or sets the search to preselect, from <c>?search=</c>.</summary>
    [SupplyParameterFromQuery(Name = "search")] public string? SearchQuery { get; set; }

    /// <summary>Gets or sets the service to preselect, from <c>?service=</c>.</summary>
    [SupplyParameterFromQuery(Name = "service")] public string? ServiceQuery { get; set; }

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
