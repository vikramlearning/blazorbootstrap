using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The console's services list: every service in a <see cref="Grid{TItem}" /> (name linking to its page, kind, region,
/// team, live status in words, latest latency, error rate, and throughput, its active incident, and how the status page
/// shows it) with sorting, the column chooser, and CSV export of the shown rows with their latest values, filtered by kind
/// and live status. The grid's view is saved for the visit and can be reset. A timer moves the shown window every two
/// seconds while the page is open (unless paused), and the grid reads the rows again; it is disposed with the page. Only
/// filter changes are announced, never the ticks.
/// </summary>
public partial class ConsoleServices : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool disposed;

    private string? filterMessage;

    private Grid<ServiceRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private MonitoringServiceKind? kindFilter;

    private MonitoringStatus? statusFilter;

    private ElementReference? statusRegion;

    private MonitoringLiveTimer? timer;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive, shows the grid, and starts the live timer.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || disposed)
            return;

        await view.LoadAsync();
        viewLoaded = true;
        StateHasChanged();
        timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);
    }

    protected override void OnInitialized()
    {
        // With no column sorted, the rows keep the site's order of services.
        view = GridViews.Get(State.SiteSlug, "services", 25);
        State.Changed += OnStateChanged;
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

    private string CaptionText(int count)
    {
        var total = State.Services.Count.ToString(CultureInfo.InvariantCulture);

        return kindFilter is null && statusFilter is null
            ? $"All {total} services"
            : $"{count.ToString(CultureInfo.InvariantCulture)} of {total} services match the filters";
    }

    private async Task ClearFiltersAsync()
    {
        kindFilter = null;
        statusFilter = null;
        await FiltersChangedAsync();
        await MonitoringDisplay.FocusAsync(statusRegion);
    }

    private async Task FiltersChangedAsync()
    {
        filterMessage = $"{CaptionText(Rows.Count)}.";
        await RefreshGridAsync();
    }

    private async Task OnKindChangedAsync(ChangeEventArgs e)
    {
        kindFilter = Enum.TryParse<MonitoringServiceKind>(e.Value?.ToString(), out var kind) && Enum.IsDefined(kind) ? kind : null;
        await FiltersChangedAsync();
    }

    /// <summary>A service, incident, or the status page changed: the grid reads the rows again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusChangedAsync(ChangeEventArgs e)
    {
        statusFilter = Enum.TryParse<MonitoringStatus>(e.Value?.ToString(), out var status) && Enum.IsDefined(status) ? status : null;
        await FiltersChangedAsync();
    }

    private async Task OnTickAsync()
    {
        if (disposed || State.IsPaused)
            return;

        try
        {
            await InvokeAsync(async () =>
            {
                if (disposed)
                    return;

                State.Tick();
                await RefreshGridAsync();
                StateHasChanged();
            });
        }
        catch (Exception ex) when (ex is JSDisconnectedException or ObjectDisposedException or TaskCanceledException)
        {
            // The page or circuit went away between ticks.
        }
    }

    /// <summary>The rows the grid asked for, in the site's order of services unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ServiceRow>> ProvideRowsAsync(GridDataProviderRequest<ServiceRow> request) => view.ApplyAsync(request, Rows);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the announcement.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        gridVersion++;
        filterMessage = "The view is reset: services in the site's order, and every column shown.";
        StateHasChanged();

        await Task.Yield();
        await MonitoringDisplay.FocusAsync(statusRegion);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>The services that match the filters, in the site's order, with their latest sample and status.</summary>
    private IReadOnlyList<ServiceRow> Rows =>
        State.Services
            .Where(service => kindFilter is null || service.Kind == kindFilter)
            .Select(service =>
            {
                var latest = State.Latest(service.Id);
                var incident = State.ActiveIncident(service.Id);
                var publicStatus = State.PublicStatus(service.Id);

                return new ServiceRow(service.Id, service.Name, MonitoringDisplay.KindLabel(service.Kind), service.Kind, service.Region, service.Team, State.Status(service),
                    Math.Round(latest.LatencyMs, 1), Math.Round(latest.ErrorRate, 2), Math.Round(latest.Requests), incident,
                    incident is null ? "None" : FormattableString.Invariant($"#{incident.Id} {incident.Title}"), publicStatus,
                    service.IsPublic ? MonitoringDisplay.PublicStatusLabel(publicStatus) : "Not on the status page");
            })
            .Where(row => statusFilter is null || row.LiveStatus == statusFilter)
            .ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One row of the list: a service and its latest values, with properties the CSV export can read.</summary>
    private sealed record ServiceRow(string Id, string Name, string Kind, MonitoringServiceKind ServiceKind, string Region, string Team, MonitoringStatus LiveStatus,
        double LatencyMs, double ErrorRate, double Requests, MonitoringIncident? Incident, string ActiveIncident, MonitoringPublicStatus PublicStatus, string StatusPage);

    #endregion
}
