using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// A published incident's public page: title, affected component, status (ongoing with the component status it causes, or
/// resolved with its duration), its status page updates as a timeline newest first (stage, time, text), and the
/// postmortem once engineers publish it (summary, root cause, action items). An incident that doesn't exist or was never
/// published shows a not-found panel, so engineers-only incidents can't be found by their number.
/// </summary>
public partial class StatusIncident : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Status", Href = MonitoringLayout.Base },
        new() { Text = "Incident history", Href = StatusPage.HistoryPath },
        new() { Text = $"Incident #{Id.ToString(CultureInfo.InvariantCulture)}", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the incident number from the route.</summary>
    [Parameter] public int Id { get; set; }

    /// <summary>The incident, only when it is published.</summary>
    private MonitoringIncident? Incident => State.FindPublicIncident(Id);

    [Inject] private MonitoringState State { get; set; } = default!;

    /// <summary>The subscribe page, with the incident's component chosen.</summary>
    private string SubscribeLink => Incident is { } incident ? $"{StatusPage.SubscribePath}?component={Uri.EscapeDataString(incident.ServiceId)}" : StatusPage.SubscribePath;

    #endregion
}
