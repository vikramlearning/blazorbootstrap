using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Checkout: contact and shipping form, delivery choice, the test-card payment form, and the order summary. A declined
/// test card shows an error and keeps the cart; an approved one stores the order in <see cref="EcommerceState" /> with the
/// card's brand and last four digits only, empties the cart, and opens the confirmation.
/// </summary>
public partial class Checkout : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly string[] countries = { "United States", "Canada", "United Kingdom", "Germany", "India", "Australia" };

    private ShowcaseCardInput card = new();

    private int declines;

    private EditContext editContext = default!;

    private CheckoutForm form = new();

    private string? paymentError;

    private bool placing;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        editContext = new EditContext(form);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Fills fictitious details, so nobody has to type real personal data into a demo.</summary>
    private void FillSample()
    {
        form.FullName = "Jordan Ashdown";
        form.Email = "jordan.ashdown@example.com";
        form.Street = "42 Larkspur Lane";
        form.City = "Fairbank";
        form.PostalCode = "10481";
        form.Country = countries[0];
        editContext.Validate();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void PlaceOrder()
    {
        // Guards a double submit: the first click empties the cart and navigates away.
        if (placing || State.Cart.Count == 0)
            return;

        var charge = ShowcaseCards.Charge(card);

        if (!charge.IsApproved)
        {
            // A different text each attempt, so the alert is announced again.
            declines++;
            paymentError = declines == 1 ? charge.FailureMessage : $"{charge.FailureMessage} (attempt {declines})";

            return;
        }

        placing = true;
        paymentError = null;

        var order = State.PlaceOrder(
            new EcommerceAddress(form.FullName.Trim(), form.Email.Trim(), form.Street.Trim(), form.City.Trim(), form.PostalCode.Trim(), form.Country),
            form.Delivery,
            charge);

        card = new ShowcaseCardInput();
        Navigation.NavigateTo($"{EcommerceLayout.Base}/orders/{order.Id}");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string StandardPrice => State.CartTotals(EcommerceDelivery.Standard).Shipping == 0
        ? "Free"
        : EcommerceDisplay.Money(EcommerceSeedData.StandardShipping);

    [Inject] private EcommerceState State { get; set; } = default!;

    private EcommerceTotals Totals => State.CartTotals(form.Delivery);

    #endregion

    #region Nested Types

    private sealed class CheckoutForm
    {
        [Required(ErrorMessage = "Enter the city.")]
        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = "United States";

        public EcommerceDelivery Delivery { get; set; } = EcommerceDelivery.Standard;

        [Required(ErrorMessage = "Enter an email for the order confirmation.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the full name for delivery.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Use 2 to 60 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the postal code.")]
        [RegularExpression(@"^[A-Za-z0-9 \-]{3,10}$", ErrorMessage = "Use 3 to 10 letters, digits, spaces, or hyphens.")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the street address.")]
        [StringLength(100, ErrorMessage = "Use 100 characters or fewer.")]
        public string Street { get; set; } = string.Empty;
    }

    #endregion
}
