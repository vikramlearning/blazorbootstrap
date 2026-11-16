using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The console's services list: every service in a table with a caption and header scopes (name linking to its page,
/// kind, region, team, live status in words, latest latency, error rate, and throughput, its active incident, and how
/// the status page shows it), filtered by kind and live status. A timer moves the shown window every two seconds while
/// the page is open (unless paused) and is disposed with the page; only filter changes are announced, never the ticks.
/// </summary>
public partial class ConsoleServices : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool disposed;

    private string? filterMessage;

    private MonitoringServiceKind? kindFilter;

    private MonitoringStatus? statusFilter;

    private MonitoringLiveTimer? timer;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !disposed)
            timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

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

    private void Announce() => filterMessage = $"{CaptionText(Rows.Count)}.";

    private string CaptionText(int count)
    {
        var total = State.Services.Count.ToString(CultureInfo.InvariantCulture);

        return kindFilter is null && statusFilter is null
            ? $"All {total} services"
            : $"{count.ToString(CultureInfo.InvariantCulture)} of {total} services match the filters";
    }

    private void ClearFilters()
    {
        kindFilter = null;
        statusFilter = null;
        Announce();
    }

    private static string KindLabel(MonitoringServiceKind kind) => kind switch
    {
        MonitoringServiceKind.Api => "API",
        MonitoringServiceKind.Worker => "Worker",
        MonitoringServiceKind.Database => "Database",
        MonitoringServiceKind.Cache => "Cache",
        _ => "Devices",
    };

    private void OnKindChanged(ChangeEventArgs e)
    {
        kindFilter = Enum.TryParse<MonitoringServiceKind>(e.Value?.ToString(), out var kind) && Enum.IsDefined(kind) ? kind : null;
        Announce();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnStatusChanged(ChangeEventArgs e)
    {
        statusFilter = Enum.TryParse<MonitoringStatus>(e.Value?.ToString(), out var status) && Enum.IsDefined(status) ? status : null;
        Announce();
    }

    private async Task OnTickAsync()
    {
        if (disposed || State.IsPaused)
            return;

        try
        {
            await InvokeAsync(() =>
            {
                if (disposed)
                    return;

                State.Tick();
                StateHasChanged();
            });
        }
        catch (Exception ex) when (ex is JSDisconnectedException or ObjectDisposedException or TaskCanceledException)
        {
            // The page or circuit went away between ticks.
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The services that match the filters, in the site's order, with their live status.</summary>
    private IReadOnlyList<(MonitoringService Service, MonitoringStatus Status)> Rows =>
        State.Services
            .Where(service => kindFilter is null || service.Kind == kindFilter)
            .Select(service => (Service: service, Status: State.Status(service)))
            .Where(row => statusFilter is null || row.Status == statusFilter)
            .ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
