using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agent's "New ticket", opened on a customer's behalf (for example after a phone call): customer and request
/// details with validation. A valid ticket is added to the top of the queue, a toast confirms it, and the agent ticket page
/// opens. A ticket from the email of a customer account belongs to that account, so it also shows in the customer's
/// requests.
/// </summary>
public partial class AgentNewTicket : ComponentBase
{
    #region Fields and Constants

    private readonly List<BreadcrumbItem> breadcrumbItems = new()
    {
        new() { Text = "Queue", Href = HelpDeskAgentLayout.Base },
        new() { Text = "New ticket", IsCurrentPage = true },
    };

    private string? createError;

    private int createErrors;

    private EditContext editContext = default!;

    private TicketForm form = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => editContext = new EditContext(form);

    #endregion

    #region Private Methods

    private void Create()
    {
        if (State.Create(form.CustomerName, form.Email, form.Company, form.CategoryId, form.Priority, form.Subject, form.Description) is not { } ticket)
        {
            createErrors++;
            const string message = "The ticket couldn't be created. Check the email address (no spaces) and try again.";
            createError = createErrors == 1 ? message : FormattableString.Invariant($"{message} ({createErrors})");

            return;
        }

        var text = ticket.CustomerAccountId is null
            ? $"Ticket #{ticket.Id} is in the queue."
            : $"Ticket #{ticket.Id} is in the queue and in {ticket.CustomerName}'s requests.";

        Toasts.Notify(new ToastMessage(ToastType.Success, IconName.CheckCircle, "Ticket created", text));
        Navigation.NavigateTo(HelpDeskDisplay.TicketUrl(ticket.Id));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HelpDeskState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class TicketForm
    {
        [Required(ErrorMessage = "Choose a category.")]
        public string CategoryId { get; set; } = string.Empty;

        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string? Company { get; set; }

        [Required(ErrorMessage = "Enter the customer's name.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Use 2 to 60 characters.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Describe the request.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Use 10 to 2,000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the customer's email address.")]
        [EmailAddress(ErrorMessage = "Enter an email address like name@example.com.")]
        [StringLength(80, ErrorMessage = "Use 80 characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        public HelpDeskPriority Priority { get; set; } = HelpDeskPriority.Normal;

        [Required(ErrorMessage = "Add a short subject.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Use 4 to 100 characters.")]
        public string Subject { get; set; } = string.Empty;
    }

    #endregion
}
