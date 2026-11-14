using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Pauses and resumes the live updates on every page (WCAG 2.2.2: content that updates on its own can be paused). The
/// choice is kept in the site state, and a status message announces it.
/// </summary>
public partial class LivePauseButton : ComponentBase
{
    #region Fields and Constants

    private string? statusMessage;

    #endregion

    #region Methods

    #region Private Methods

    private void Toggle()
    {
        State.IsPaused = !State.IsPaused;
        statusMessage = State.IsPaused ? "Live updates paused." : "Live updates resumed.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
