using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Layout of Parley, the members' app, inside <see cref="AiFrame" />: a Navbar with Chat, Prompts, and Usage for an active
/// member ("Workspace admin" for an admin), the account menu or "Sign in", and the page body inside the showcase session
/// (per-tab data and the demo sign-in).
/// </summary>
public partial class AiLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the AI assistant site.</summary>
    public const string Base = DemoRouteConstants.Showcase_URL_AiAssistant;

    /// <summary>Route of the sign-in page.</summary>
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

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
