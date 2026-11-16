using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// The signed-in learner's certificates, newest first: the course, the issue date, the certificate ID, the public verify
/// link (shown as text to copy), and a link to the printable certificate. An empty state points to My learning.
/// </summary>
public partial class Certificates : ComponentBase, IDisposable
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

    private string VerifyUrl(string code) => Navigation.ToAbsoluteUri(Verify.PathFor(code)).ToString();

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
