using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// The add and edit user dialog shared by the users list and the user detail screen.
/// Saving updates <see cref="SaasDashboardState" />, so every screen that shows the account refreshes.
/// </summary>
public partial class SaasAccountEditor : ComponentBase
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private int? editingId;

    private AccountForm form = new();

    private Modal modal = default!;

    private Func<ElementReference?>? returnFocusTo;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => editContext = new EditContext(form);

    #endregion

    #region Public Methods

    /// <summary>
    /// Opens the dialog for a new account. When it closes, focus moves to the element <paramref name="returnFocusTo" /> returns then,
    /// so an opener that was re-rendered meanwhile is still found.
    /// </summary>
    public Task AddAsync(Func<ElementReference?>? returnFocusTo = null) => OpenAsync(null, returnFocusTo);

    /// <summary>
    /// Opens the dialog for an existing account. When it closes, focus moves to the element <paramref name="returnFocusTo" /> returns then.
    /// </summary>
    public Task EditAsync(SaasAccount account, Func<ElementReference?>? returnFocusTo = null) => OpenAsync(account, returnFocusTo);

    #endregion

    #region Private Methods

    private Task HideAsync() => modal.HideAsync();

    /// <summary>
    /// The modal opens from code, so Bootstrap does not move focus back to what opened it; this does (WCAG 2.4.3).
    /// </summary>
    private async Task OnHidden()
    {
        var target = returnFocusTo;

        editingId = null;
        returnFocusTo = null;
        form = new AccountForm();
        editContext = new EditContext(form);

        await SaasDisplay.FocusAsync(target?.Invoke());
    }

    private async Task OpenAsync(SaasAccount? account, Func<ElementReference?>? focusTarget)
    {
        editingId = account?.Id;
        returnFocusTo = focusTarget;
        form = account is null
            ? new AccountForm()
            : new AccountForm
            {
                Name = account.Name,
                Email = account.Email,
                Company = account.Company,
                Role = account.Role,
                PlanId = account.PlanId,
                Seats = account.Seats,
                Status = account.Status,
            };
        editContext = new EditContext(form);

        await modal.ShowAsync();
    }

    private async Task SaveAsync()
    {
        SaasAccount saved;

        if (editingId is int id && State.FindAccount(id) is { } existing)
        {
            saved = existing with
            {
                Name = form.Name,
                Email = form.Email,
                Company = form.Company,
                Role = form.Role,
                PlanId = form.PlanId,
                Seats = form.Seats,
                Status = form.Status,
            };
            State.UpdateAccount(saved);
            saved = State.FindAccount(id)!;
        }
        else
        {
            saved = State.AddAccount(form.Name, form.Email, form.Company, form.Role, form.PlanId, form.Seats, form.Status);
        }

        var isNew = editingId is null;

        await modal.HideAsync();

        if (OnSaved.HasDelegate)
            await OnSaved.InvokeAsync(new SaasAccountSavedEventArgs(saved, isNew));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the callback fired after an account is added or updated.</summary>
    [Parameter] public EventCallback<SaasAccountSavedEventArgs> OnSaved { get; set; }

    [Inject] private SaasDashboardState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class AccountForm
    {
        [Required(ErrorMessage = "Enter the company name.")]
        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string Company { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter an email address.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(80, ErrorMessage = "Use 80 characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the person's name.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Use 2 to 60 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose a plan.")]
        public string PlanId { get; set; } = "pro";

        public SaasAccountRole Role { get; set; } = SaasAccountRole.Member;

        [Range(1, 500, ErrorMessage = "Seats must be between 1 and 500.")]
        public int Seats { get; set; } = 5;

        public SaasAccountStatus Status { get; set; } = SaasAccountStatus.Trial;
    }

    #endregion
}

/// <summary>Describes an account saved by <see cref="SaasAccountEditor" />.</summary>
/// <param name="Account">The account as stored after the save.</param>
/// <param name="IsNew">Whether the account was just added.</param>
public sealed record SaasAccountSavedEventArgs(SaasAccount Account, bool IsNew);
