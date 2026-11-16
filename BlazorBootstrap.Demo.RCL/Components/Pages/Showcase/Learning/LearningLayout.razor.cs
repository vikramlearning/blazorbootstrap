using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Layout of the "Quillpath Academy" school, inside <see cref="LearningFrame" />: a Navbar with the learner's modules (an
/// instructor gets "Teaching" and a bar back to the back office, D-032) and the account menu, and the page body inside the
/// showcase session (per-tab data and the demo sign-in).
/// </summary>
public partial class LearningLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the learning site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Learning;

    /// <summary>Route of the demo sign-in page.</summary>
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

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
