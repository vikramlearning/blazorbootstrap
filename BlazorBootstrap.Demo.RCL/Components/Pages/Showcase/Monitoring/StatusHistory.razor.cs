using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The status page's incident history: every published incident grouped by the month it started, newest first, each with
/// its title (a link to its public page), component, start, duration or "Ongoing", and the stage of its latest public
/// update. A component filter narrows the list, and a status message says how many incidents it shows.
/// </summary>
public partial class StatusHistory : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? filterMessage;

    private string serviceFilter = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns whether a published incident is still ongoing on the status page (its latest public update isn't "Resolved").</summary>
    internal static bool IsOngoing(MonitoringIncident incident) => incident.PublicStage != MonitoringPublicStage.Resolved || incident.ResolvedAt is null;

    #endregion

    #region Private Methods

    private void OnServiceChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;
        serviceFilter = State.FindService(value) is null ? string.Empty : value;

        var count = Filtered.Count;
        var countText = count == 1 ? "1 incident" : $"{count.ToString(CultureInfo.InvariantCulture)} incidents";
        filterMessage = SelectedService is { } service ? $"Showing {countText} about {service.Name}." : $"Showing {countText} about all components.";
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    #endregion

    #endregion

    #region Properties, Indexers

    private IReadOnlyList<MonitoringIncident> Filtered =>
        State.PublicIncidents.Where(incident => serviceFilter.Length == 0 || incident.ServiceId == serviceFilter).ToList();

    /// <summary>The filtered incidents by the month they started, newest month first (each month's incidents newest first).</summary>
    private IReadOnlyList<IGrouping<DateTime, MonitoringIncident>> Months =>
        Filtered.GroupBy(incident => new DateTime(incident.StartedAt.Year, incident.StartedAt.Month, 1)).OrderByDescending(month => month.Key).ToList();

    private MonitoringService? SelectedService => State.FindService(serviceFilter);

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
