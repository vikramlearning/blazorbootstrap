using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// Shows its content only to accounts in the given roles. It waits until the tab's snapshot is restored, then sends a
/// signed-out visitor to the site's sign-in page with this page as the return address, and shows an access-denied screen
/// with "Switch account" to a signed-in account in another role. The guard is client-side: it protects demo screens
/// with fictitious data, not real resources.
/// </summary>
public partial class ShowcaseRoleGuard : ComponentBase
{
    #region Fields and Constants

    private bool redirecting;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnAfterRender(bool firstRender)
    {
        if (!Session.IsRestored || Identity.IsSignedIn)
        {
            redirecting = false;
            return;
        }

        if (redirecting)
            return;

        redirecting = true;
        Navigation.NavigateTo(SignInUrl(), replace: true);
    }

    #endregion

    #region Private Methods

    private void GoHome() => Navigation.NavigateTo(Identity.CurrentRole?.HomePath ?? SignInPath);

    private string SignInUrl() =>
        $"{SignInPath}?returnUrl={Uri.EscapeDataString("/" + Navigation.ToBaseRelativePath(Navigation.Uri))}";

    private void SwitchAccount()
    {
        var target = SignInUrl();
        redirecting = true;
        Identity.SignOut();
        Navigation.NavigateTo(target);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string AllowedLabels => string.Join(" or ", RoleIds.Select(roleId => Identity.FindRole(roleId)?.Label.ToLowerInvariant() ?? roleId).Select(label => $"{label}s"));

    /// <summary>Gets or sets the content shown to an allowed account.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private ShowcaseIdentity Identity => Session.Identity;

    private bool IsAllowed => Identity.IsInRole(RoleIds);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<string> RoleIds => Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Gets or sets the allowed role IDs, comma-separated, such as <c>customer</c> or <c>agent,admin</c>.</summary>
    [Parameter, EditorRequired] public string Roles { get; set; } = default!;

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    /// <summary>Gets or sets the path of the site's sign-in page.</summary>
    [Parameter, EditorRequired] public string SignInPath { get; set; } = default!;

    #endregion
}
