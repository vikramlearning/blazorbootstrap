using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The demo sign-in screen every site uses: a sign-in form, an optional register form, and the site's demo accounts with
/// one-click buttons. It checks only the accounts held in this tab, then returns to the requested page when that page is
/// inside the site (<see cref="ShowcaseReturnUrl" />), or to the role's start page.
/// </summary>
public partial class ShowcaseSignIn : ComponentBase
{
    #region Fields and Constants

    private ElementReference heading;

    private bool navigating;

    private bool registering;

    private string? registerError;

    private RegisterForm registerForm = new();

    private int signInAttempts;

    private string? signInError;

    private SignInForm signInForm = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => registering = StartWithRegister && RegisterRoleId is not null;

    #endregion

    #region Private Methods

    private void Continue()
    {
        if (Identity.Current is not null)
            GoToTarget();
    }

    private void GoToTarget()
    {
        navigating = true;
        var target = ShowcaseReturnUrl.Resolve(ReturnUrl, SiteBase) ?? Identity.CurrentRole?.HomePath ?? SiteBase;
        Navigation.NavigateTo(target);
    }

    private void Register()
    {
        var result = Identity.Register(RegisterRoleId!, registerForm.Name, registerForm.Email, registerForm.Password!, out _);

        if (result == ShowcaseAccountResult.Success)
        {
            GoToTarget();
            return;
        }

        registerError = result == ShowcaseAccountResult.EmailInUse
            ? $"An account already uses {registerForm.Email.Trim()}. Sign in instead, or use another email."
            : "This demo can't create more accounts in this tab. Reset the demo data to start over.";
    }

    private void SignIn()
    {
        if (Identity.SignIn(signInForm.Email, signInForm.Password) is not null)
        {
            GoToTarget();
            return;
        }

        // A different text each time, so a screen reader announces the alert again.
        signInAttempts++;
        signInError = signInAttempts == 1
            ? "The email or password is incorrect."
            : $"The email or password is still incorrect (attempt {signInAttempts}). Use a demo account from the list.";
    }

    private void SignInAs(ShowcaseDemoAccount demo)
    {
        if (Identity.SignInAs(demo.Id) is not null)
            GoToTarget();
    }

    private async Task SignOutAsync()
    {
        Identity.SignOut();
        StateHasChanged();
        await Task.Yield();
        await FocusAsync(heading);
    }

    private async Task SwitchModeAsync(bool register)
    {
        registering = register;
        registerError = null;
        signInError = null;
        StateHasChanged();
        await Task.Yield();
        await FocusAsync(heading);
    }

    private static async Task FocusAsync(ElementReference element)
    {
        try
        {
            await element.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Nothing left to focus.
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private ShowcaseIdentity Identity => Session.Identity;

    private bool IsBusy => navigating;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the role new accounts get; <see langword="null" /> hides "Create an account".</summary>
    [Parameter] public string? RegisterRoleId { get; set; }

    private string RegisterRoleLabel => Identity.FindRole(RegisterRoleId)?.Label.ToLowerInvariant() ?? "site";

    /// <summary>Gets or sets the requested page from <c>?returnUrl=</c>; only a page inside the site is used.</summary>
    [Parameter] public string? ReturnUrl { get; set; }

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    /// <summary>Gets or sets the site's base path, such as <c>/showcase/ecommerce</c>.</summary>
    [Parameter, EditorRequired] public string SiteBase { get; set; } = default!;

    /// <summary>Gets or sets the site's name shown in the headings.</summary>
    [Parameter, EditorRequired] public string SiteName { get; set; } = default!;

    private string SiteSlug => Session.State.SiteSlug;

    /// <summary>Gets or sets whether the screen opens on the register form.</summary>
    [Parameter] public bool StartWithRegister { get; set; }

    #endregion

    #region Nested Types

    private sealed class RegisterForm
    {
        [Required(ErrorMessage = "Enter your email address.")]
        [EmailAddress(ErrorMessage = "Enter an email address like name@example.com.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your name.")]
        [StringLength(ShowcaseIdentity.MaxNameLength, MinimumLength = 2, ErrorMessage = "Use 2 to 80 characters.")]
        [RegularExpression(@".*\S.*\S.*", ErrorMessage = "Use 2 to 80 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Make up a password.")]
        [StringLength(128, MinimumLength = ShowcaseIdentity.MinPasswordLength, ErrorMessage = "Use at least 8 characters.")]
        public string? Password { get; set; }
    }

    private sealed class SignInForm
    {
        [Required(ErrorMessage = "Enter the email address.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the password.")]
        [StringLength(128, ErrorMessage = "Use 128 characters or fewer.")]
        public string? Password { get; set; }
    }

    #endregion
}
