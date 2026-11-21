using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Layout of Parley, the members' app, inside <see cref="AiFrame" />: a Navbar with Chat, Prompts, and Usage for an active
/// member (and "Workspace admin", or "Shared prompts" for a prompt editor, for the roles with an admin page), the global
/// search and the notifications for an active member, the account menu or "Sign in", and the page body inside the showcase
/// session (per-tab data and the demo sign-in).
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

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        Navigation.LocationChanged += OnLocationChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and navigation changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        Navigation.LocationChanged -= OnLocationChanged;
    }

    #endregion

    #region Private Methods

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) => InvokeAsync(StateHasChanged);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Whether the sign-in page is open: it keeps the header to the brand, the links, and the account menu.</summary>
    private bool IsSignInPage => Navigation.ToBaseRelativePath(Navigation.Uri).Split('?', '#')[0].TrimEnd('/').Equals(SignInPath.TrimStart('/'), StringComparison.OrdinalIgnoreCase);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>What the header search finds, in words, for the signed-in role.</summary>
    private string SearchLabel => State.Can(AiPermission.ManageMembers)
        ? "Search members and invitations by name or email, prompts by title, and your conversations by title"
        : State.Can(AiPermission.ManageSharedPrompts)
            ? "Search your prompts and the shared prompts (drafts included) by title, and your conversations by title"
            : "Search your prompts and the shared prompts by title, and your conversations by title";

    private string SearchPlaceholder => State.Can(AiPermission.ManageMembers) ? "Search members, prompts, chats" : "Search prompts and chats";

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
