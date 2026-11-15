using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Lumora's "Contact sales" page for Enterprise: name, work email, company, team size, and a message, with a short summary
/// of the Enterprise plan beside it. A sent message waits in the sales inbox in this tab (the leads the operator console
/// reads); nothing is emailed.
/// </summary>
public partial class Contact : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest message the sales inbox keeps.</summary>
    private const int MaxMessageLength = 1000;

    /// <summary>The longest name or company the sales inbox keeps.</summary>
    private const int MaxTextLength = 60;

    private int attempts;

    private string? error;

    private ContactForm form = new();

    private InputText? nameInput;

    private SaasLead? sent;

    private ElementReference statusRegion;

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

    private static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task SendAnotherAsync()
    {
        sent = null;
        error = null;
        attempts = 0;
        form = new ContactForm();
        StateHasChanged();
        await Task.Yield();
        await SaasDisplay.FocusAsync(nameInput?.Element);
    }

    private async Task Submit()
    {
        var lead = State.SubmitLead(form.Name, form.Email, form.Company, form.TeamSize, form.Message);

        if (lead is null)
        {
            // A count in the text makes a repeated alert announce again.
            attempts++;
            var message = "The message couldn't be sent. Check each field, or reset the demo data if this tab's sales inbox is full.";
            error = attempts == 1 ? message : $"{message} (attempt {attempts})";

            return;
        }

        error = null;
        attempts = 0;
        sent = lead;
        StateHasChanged();
        await Task.Yield();

        // The form and its button are gone: focus the confirmation.
        await SaasDisplay.FocusAsync(statusRegion);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The plan sold through sales, summarized beside the form.</summary>
    private SaasPlan? EnterprisePlan => State.Plans.FirstOrDefault(plan => !plan.IsSelfServe);

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class ContactForm
    {
        [Required(ErrorMessage = "Enter your company.")]
        [StringLength(MaxTextLength, ErrorMessage = "Use {1} characters or fewer.")]
        public string Company { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your work email.")]
        [EmailAddress(ErrorMessage = "Enter an email address like name@example.com.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use {1} characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tell us how we can help.")]
        [StringLength(MaxMessageLength, ErrorMessage = "Use {1} characters or fewer.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your name.")]
        [StringLength(MaxTextLength, ErrorMessage = "Use {1} characters or fewer.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose your team size.")]
        public string TeamSize { get; set; } = string.Empty;
    }

    #endregion
}
