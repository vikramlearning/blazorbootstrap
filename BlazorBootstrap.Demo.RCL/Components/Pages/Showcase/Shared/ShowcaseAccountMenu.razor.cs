using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The account menu in a site's header: "Sign in" while signed out; otherwise who is signed in and in which role, the
/// role's start and account pages, Switch account, Reset demo data (after a confirmation), and Sign out.
/// </summary>
public partial class ShowcaseAccountMenu : ComponentBase
{
    #region Fields and Constants

    private bool resetting;

    private Modal resetModal = default!;

    private DropdownToggleButton? toggle;

    #endregion

    #region Methods

    #region Private Methods

    private async Task AskResetAsync() => await resetModal.ShowAsync();

    private static string HomeLabel(ShowcaseRole role) => $"{role.Label} home";

    /// <summary>The dialog opens from code, so focus goes back to the menu button when it's cancelled.</summary>
    private async Task OnResetHiddenAsync()
    {
        if (resetting || toggle?.Element is not { } element)
            return;

        try
        {
            await element.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // The menu has gone.
        }
    }

    /// <summary>Clears the site's stored snapshot and reloads the site, which starts it from seed data, signed out.</summary>
    private async Task ResetAsync()
    {
        resetting = true;
        Session.IsResetting = true;
        await Store.ClearAsync(Session.State.SiteSlug);
        Navigation.NavigateTo(SiteBase, forceLoad: true);
    }

    private void SignOut()
    {
        Identity.SignOut();
        Navigation.NavigateTo(SiteBase);
    }

    private void SwitchAccount()
    {
        Identity.SignOut();
        Navigation.NavigateTo(SignInPath);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets extra CSS classes.</summary>
    [Parameter] public string? Class { get; set; }

    private ShowcaseIdentity Identity => Session.Identity;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    /// <summary>Gets or sets the site's public start page, where Sign out and Reset go.</summary>
    [Parameter, EditorRequired] public string SiteBase { get; set; } = default!;

    /// <summary>Gets or sets the site's name, used in the reset confirmation.</summary>
    [Parameter, EditorRequired] public string SiteName { get; set; } = default!;

    /// <summary>Gets or sets the path of the site's sign-in page.</summary>
    [Parameter, EditorRequired] public string SignInPath { get; set; } = default!;

    [Inject] private ShowcaseSessionStore Store { get; set; } = default!;

    #endregion
}
