using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// New ticket: customer and request details with validation. A valid ticket is added to the top of the queue, a toast
/// confirms it, and the ticket page opens.
/// </summary>
public partial class NewTicket : ComponentBase
{
    #region Fields and Constants

    private readonly List<BreadcrumbItem> breadcrumbItems = new()
    {
        new() { Text = "Queue", Href = HelpDeskLayout.Base },
        new() { Text = "New ticket", IsCurrentPage = true },
    };

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
        var ticket = State.Create(form.CustomerName, form.Email, form.Company, form.CategoryId, form.Priority, form.Subject, form.Description);

        Toasts.Notify(new ToastMessage(ToastType.Success, IconName.CheckCircle, "Ticket created", $"Ticket #{ticket.Id} is in the queue."));
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
