using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The signed-in person's profile: name, email, and password (<see cref="ShowcaseAccountProfile" />), then their work at
/// the agency: job title, weekly capacity, open and overdue task counts, what's due by Sunday against capacity, and links
/// to the projects they're on.
/// </summary>
public partial class Profile : ComponentBase, IDisposable
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

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
