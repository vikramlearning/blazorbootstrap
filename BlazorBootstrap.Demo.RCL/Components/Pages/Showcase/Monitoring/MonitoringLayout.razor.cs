using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Layout of the public "Stackwatch Status" page, inside <see cref="MonitoringFrame" />: a Navbar with the status, the
/// incident history, and subscribing (an engineer gets "Console" and a bar back to it, D-032), the account menu or
/// "Engineer sign-in", and the page body inside the showcase session (per-tab data and the demo sign-in).
/// </summary>
public partial class MonitoringLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the monitoring site (the public status page).</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Monitoring;

    /// <summary>Route of the engineers' sign-in page.</summary>
    public const string SignInPath = Base + "/sign-in";

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion
}
