using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The account menu in a site's header. Signed out: Sign in, Create an account, and Reset demo data. Signed in: who is
/// signed in and in which role, the role's start and account pages, Switch account, Reset demo data, and Sign out. The
/// reset confirmation is the session host's dialog, which sits outside the sticky header.
/// </summary>
public partial class ShowcaseAccountMenu : ComponentBase
{
    #region Fields and Constants

    private DropdownToggleButton? toggle;

    #endregion

    #region Methods

    #region Private Methods

    /// <summary>
    /// The button's accessible name starts with the text it shows (the avatar initials, the name, and the notice count),
    /// so speech input users can say what they see (WCAG 2.5.3 Label in Name), then says what the button is.
    /// </summary>
    private string AccountLabel(ShowcaseAccount account)
    {
        var notices = NoticeCount > 0 ? $" {NoticeCountText} new {(NoticeCount == 1 ? "notice" : "notices")}" : null;

        var role = Identity.CurrentRole?.Label is { } label && !string.IsNullOrWhiteSpace(label) ? $", {label}" : null;

        return $"{account.Initials} {account.Name}{notices}{role}, account menu";
    }

    /// <summary>Opens the host's reset dialog; when it is cancelled, focus goes back to this menu's button.</summary>
    private Task AskResetAsync() => Session.RequestResetAsync(FocusToggleAsync);

    private async Task FocusToggleAsync()
    {
        if (toggle?.Element is not { } element)
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

    /// <summary>Gets or sets the number of unread notices; above zero, the button shows a count and names it.</summary>
    [Parameter] public int NoticeCount { get; set; }

    private string NoticeCountText => NoticeCount > 9 ? "9+" : NoticeCount.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Gets or sets where the account's notices are; <see langword="null" /> hides the menu item.</summary>
    [Parameter] public string? NoticesPath { get; set; }

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    /// <summary>Gets or sets the site's public start page, where Sign out goes.</summary>
    [Parameter, EditorRequired] public string SiteBase { get; set; } = default!;

    /// <summary>Gets or sets the path of the site's sign-in page.</summary>
    [Parameter, EditorRequired] public string SignInPath { get; set; } = default!;

    #endregion
}
