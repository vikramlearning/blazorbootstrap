using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Layout of the "Quillpath Academy" learning site: the site bar, a Navbar with the learner's modules and the account menu,
/// the page body inside the showcase session (per-tab data and the demo sign-in), one ad slot, the footer, and the toasts.
/// </summary>
public partial class LearningLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the learning site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_Learning;

    /// <summary>Route of the demo sign-in page.</summary>
    public const string SignInPath = Base + "/sign-in";

    private readonly ShowcaseSite site = ShowcaseCatalog.Get("learning");

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
