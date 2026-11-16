using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Logs and traces: a live tail of every service's log lines, newest first, filtered by service, level, and a search in
/// the message or trace ID. A <see cref="MonitoringLiveTimer" /> moves the shown window every tick unless live updates are
/// paused (the shared <see cref="MonitoringState.IsPaused" />), so new lines arrive at the top. The status region stays
/// quiet on ticks: it speaks when the filters change, and once after a pause with how many lines arrived meanwhile.
/// While paused, "Load new lines now" fetches the newest lines once. Each line links its trace.
/// <c>?service=</c> preselects a service and <c>?search=</c> a search (the trace page links its lines this way).
/// </summary>
public partial class Logs : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many more lines "Show more" adds, and how many show at first.</summary>
    private const int PageSize = 50;

    private string? appliedSearchQuery;

    private string? appliedServiceQuery;

    private (IReadOnlyList<MonitoringLogEntry> Source, string Service, string Level, string Search, List<MonitoringLogEntry> Lines)? cache;

    private bool disposed;

    /// <summary>Set when "Clear filters" (which then disappears) ran, so focus moves to the search box after that render.</summary>
    private bool focusSearchAfterRender;

    private string levelFilter = string.Empty;

    /// <summary>The newest line shown when live updates were paused, so resuming can say how many lines arrived since.</summary>
    private MonitoringLogEntry? newestAtPause;

    private string search = string.Empty;

    private ElementReference? searchInput;

    private string serviceFilter = string.Empty;

    private int shownCount = PageSize;

    private string? statusMessage;

    private MonitoringLiveTimer? timer;

    /// <summary>Whether the page last saw live updates paused.</summary>
    private bool wasPaused;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !disposed)
            timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);

        if (focusSearchAfterRender)
        {
            focusSearchAfterRender = false;
            await MonitoringDisplay.FocusAsync(searchInput);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        wasPaused = State.IsPaused;

        if (wasPaused)
            newestAtPause = State.Logs.FirstOrDefault();
    }

    /// <summary>A new <c>?service=</c> or <c>?search=</c> value (also on a link from this page to itself) applies once.</summary>
    protected override void OnParametersSet()
    {
        if (ServiceQuery != appliedServiceQuery)
        {
            appliedServiceQuery = ServiceQuery;
            serviceFilter = State.FindService(ServiceQuery)?.Id ?? string.Empty;
            shownCount = PageSize;
        }

        if (SearchQuery != appliedSearchQuery)
        {
            appliedSearchQuery = SearchQuery;
            search = (SearchQuery ?? string.Empty).Trim();
            shownCount = PageSize;
        }
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

    private static string Lines(int count) => count == 1 ? "1 line" : $"{count} lines";

    private void ClearFilters()
    {
        search = string.Empty;
        serviceFilter = string.Empty;
        levelFilter = string.Empty;
        focusSearchAfterRender = true;
        FiltersChanged();
    }

    /// <summary>Counts the lines newer than <paramref name="newest" />, in all and among those matching the filters.</summary>
    private (int All, int Matching) CountNewerThan(MonitoringLogEntry? newest)
    {
        if (newest is null)
            return (State.Logs.Count, Filtered.Count);

        bool IsNewer(MonitoringLogEntry entry) => entry.Time > newest.Time || (entry.Time == newest.Time && entry.Id > newest.Id);

        return (State.Logs.TakeWhile(IsNewer).Count(), Filtered.TakeWhile(IsNewer).Count());
    }

    /// <summary>Starts again from the newest lines and announces the new count (filter changes are the only announced counts).</summary>
    private void FiltersChanged()
    {
        shownCount = PageSize;
        statusMessage = $"{Lines(Filtered.Count)} match.";
    }

    /// <summary>Fetches the newest lines once while paused (the person asked for it, so it is not an automatic update).</summary>
    private void LoadNewLines()
    {
        var before = State.Logs.FirstOrDefault();
        State.Tick();
        var (all, matching) = CountNewerThan(before);
        newestAtPause = State.Logs.FirstOrDefault();
        statusMessage = all == 0 ? "No new lines yet." : $"Loaded {NewText(all, matching)}. Live updates are still paused.";
    }

    private string NewText(int all, int matching) =>
        (all == 1 ? "1 new line" : $"{all} new lines") + (HasFilters ? $", {matching} matching your filters" : string.Empty);

    private void OnLevelChanged(ChangeEventArgs args)
    {
        levelFilter = args.Value?.ToString() ?? string.Empty;
        FiltersChanged();
    }

    private void OnSearchInput(ChangeEventArgs args)
    {
        search = args.Value?.ToString() ?? string.Empty;
        FiltersChanged();
    }

    private void OnServiceChanged(ChangeEventArgs args)
    {
        serviceFilter = args.Value?.ToString() ?? string.Empty;
        FiltersChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Moves the window on while live. Pausing (from the shared button) is noticed here: the newest line is remembered, and
    /// the first tick after resuming announces how many lines arrived in between.
    /// </summary>
    private async Task OnTickAsync()
    {
        if (disposed)
            return;

        try
        {
            await InvokeAsync(() =>
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

                StateHasChanged();
            });
        }
        catch (Exception ex) when (ex is JSDisconnectedException or ObjectDisposedException or TaskCanceledException)
        {
            // The page or circuit went away between ticks.
        }
    }

    private void ShowMore() => shownCount += PageSize;

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText
    {
        get
        {
            var count = Filtered.Count;

            return count <= shownCount ? Lines(count) : $"Showing the newest {shownCount} of {Lines(count)}";
        }
    }

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

    private bool HasFilters => search.Trim().Length > 0 || serviceFilter.Length > 0 || levelFilter.Length > 0;

    /// <summary>Gets or sets the search to preselect, from <c>?search=</c>.</summary>
    [SupplyParameterFromQuery(Name = "search")] public string? SearchQuery { get; set; }

    /// <summary>Gets or sets the service to preselect, from <c>?service=</c>.</summary>
    [SupplyParameterFromQuery(Name = "service")] public string? ServiceQuery { get; set; }

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
