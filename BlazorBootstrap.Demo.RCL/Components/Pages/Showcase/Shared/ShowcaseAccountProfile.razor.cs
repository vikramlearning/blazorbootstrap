using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The account settings every role gets: edit the profile (name and email) and change the demo password. Sites place
/// their role-specific settings (addresses, notifications) next to it.
/// </summary>
public partial class ShowcaseAccountProfile : ComponentBase
{
    #region Fields and Constants

    private string? loadedAccountId;

    private EditContext passwordContext = default!;

    private PasswordForm passwordForm = new();

    private int passwordSaves;

    private string? passwordStatus;

    private ProfileForm profileForm = new();

    private int profileSaves;

    private string? profileStatus;

    private ValidationMessageStore passwordMessages = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        passwordContext = new EditContext(passwordForm);
        passwordMessages = new ValidationMessageStore(passwordContext);
        passwordContext.OnFieldChanged += (_, args) =>
        {
            if (args.FieldIdentifier.FieldName == nameof(PasswordForm.Current))
                passwordMessages.Clear(args.FieldIdentifier);
        };
    }

    protected override void OnParametersSet()
    {
        // Load the form once per account, so a re-render doesn't overwrite what is being typed.
        if (Identity.Current is not { } account || account.Id == loadedAccountId)
            return;

        loadedAccountId = account.Id;
        profileForm = new ProfileForm { Name = account.Name, Email = account.Email };
    }

    #endregion

    #region Private Methods

    private void ChangePassword()
    {
        var result = Identity.ChangePassword(passwordForm.Current!, passwordForm.New!);

        if (result == ShowcaseAccountResult.WrongPassword)
        {
            var field = passwordContext.Field(nameof(PasswordForm.Current));
            passwordMessages.Clear(field);
            passwordMessages.Add(field, "The current password is incorrect.");
            passwordContext.NotifyValidationStateChanged();
            passwordStatus = null;

            return;
        }

        passwordSaves++;
        passwordStatus = passwordSaves == 1 ? "Password changed." : $"Password changed ({passwordSaves} times this visit).";
        passwordForm.Current = passwordForm.New = passwordForm.Confirm = null;
    }

    private void SaveProfile()
    {
        var result = Identity.UpdateProfile(profileForm.Name, profileForm.Email);

        profileSaves++;
        profileStatus = result == ShowcaseAccountResult.EmailInUse
            ? $"Another account already uses {profileForm.Email.Trim()}."
            : profileSaves == 1 ? "Profile saved." : $"Profile saved ({profileSaves} times this visit).";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private ShowcaseIdentity Identity => Session.Identity;

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class PasswordForm
    {
        [Required(ErrorMessage = "Confirm the new password.")]
        [Compare(nameof(New), ErrorMessage = "The passwords don't match.")]
        public string? Confirm { get; set; }

        [Required(ErrorMessage = "Enter your current password.")]
        public string? Current { get; set; }

        [Required(ErrorMessage = "Make up a new password.")]
        [StringLength(128, MinimumLength = ShowcaseIdentity.MinPasswordLength, ErrorMessage = "Use at least 8 characters.")]
        public string? New { get; set; }
    }

    private sealed class ProfileForm
    {
        [Required(ErrorMessage = "Enter your email address.")]
        [EmailAddress(ErrorMessage = "Enter an email address like name@example.com.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your name.")]
        [StringLength(ShowcaseIdentity.MaxNameLength, MinimumLength = 2, ErrorMessage = "Use 2 to 80 characters.")]
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
