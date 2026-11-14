using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Incidents: an <see cref="Alert" /> summary, active incidents with their timeline and Acknowledge / Resolve actions,
/// and resolved incidents in <see cref="Callout" />s. Resolving an incident brings its service's metrics back to normal.
/// </summary>
public partial class Incidents : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The Acknowledge button disappears, so focus moves to the status message, which announces the change.</summary>
    private async Task AcknowledgeAsync(MonitoringIncident incident)
    {
        State.Acknowledge(incident);
        statusMessage = $"Incident #{incident.Id} acknowledged.";
        StateHasChanged();

        await MonitoringDisplay.FocusAsync(statusRegion);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The incident moves to the resolved list, so focus moves to the status message.</summary>
    private async Task ResolveAsync(MonitoringIncident incident)
    {
        State.Resolve(incident);
        statusMessage = $"Incident #{incident.Id} resolved. {State.FindService(incident.ServiceId)?.Name} returns to normal from the next sample.";
        StateHasChanged();

        await MonitoringDisplay.FocusAsync(statusRegion);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private IReadOnlyList<MonitoringIncident> Active => State.Incidents.Where(incident => incident.Status != MonitoringIncidentStatus.Resolved).ToList();

    private IReadOnlyList<MonitoringIncident> Resolved => State.Incidents.Where(incident => incident.Status == MonitoringIncidentStatus.Resolved).ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
