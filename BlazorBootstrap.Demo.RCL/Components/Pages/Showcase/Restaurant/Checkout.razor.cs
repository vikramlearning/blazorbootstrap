using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Restaurant;

/// <summary>
/// Checkout: delivery or pickup, a delivery address on the map (or its placeholder), contact details, a tip, and the
/// totals. A valid order empties the cart and opens its tracking page. No payment details are asked.
/// </summary>
public partial class Checkout : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly int[] tipChoices = { 0, 10, 15, 20 };

    private readonly List<BreadcrumbItem> breadcrumbItems = new()
    {
        new() { Text = "Menu", Href = RestaurantLayout.Base },
        new() { Text = "Your order", Href = $"{RestaurantLayout.Base}/cart" },
        new() { Text = "Checkout", IsCurrentPage = true },
    };

    private EditContext editContext = default!;

    private CheckoutForm form = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        editContext = new EditContext(form);

        // The drawer in the layout can change the cart while this page is open; re-render so the summary and total match.
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnAddressChanged(string value)
    {
        form.AddressId = value;
        editContext.NotifyFieldChanged(editContext.Field(nameof(CheckoutForm.AddressId)));
    }

    private void OnFulfillmentChanged(RestaurantFulfillment value)
    {
        form.Fulfillment = value;

        // The address is only required for delivery; re-check it so a stale message disappears.
        editContext.NotifyFieldChanged(editContext.Field(nameof(CheckoutForm.AddressId)));
    }

    private void OnTipChanged(int value) => form.TipPercent = value;

    private void PlaceOrder()
    {
        if (State.Cart.Count == 0)
            return;

        var order = State.PlaceOrder(form.Name, form.Fulfillment, State.FindAddress(form.AddressId), form.AddressNote, form.TipPercent);

        Toasts.Notify(new ToastMessage(ToastType.Success, IconName.CheckCircleFill, "Order placed", $"Order #{order.Id} is with the kitchen."));
        Navigation.NavigateTo(RestaurantDisplay.OrderUrl(order.Id));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string ArrivalText
    {
        get
        {
            var address = State.FindAddress(form.AddressId);

            return form.Fulfillment == RestaurantFulfillment.Pickup
                ? $"Collect from {RestaurantSeedData.RestaurantName}, 9 Curtain Road, Shoreditch."
                : address is null
                    ? "Choose an address to see the delivery time."
                    : $"About {address.Minutes} minutes' ride from the kitchen to {address.Street}.";
        }
    }

    private string MapLabel => form.Fulfillment == RestaurantFulfillment.Delivery && State.FindAddress(form.AddressId) is { } address
        ? $"Map preview: {RestaurantSeedData.RestaurantName} and {address.Street}"
        : $"Map preview: {RestaurantSeedData.RestaurantName}";

    private IReadOnlyList<ShowcaseLocation> MapLocations
    {
        get
        {
            var locations = new List<ShowcaseLocation>
            {
                new(RestaurantSeedData.RestaurantName, "Kitchen · 9 Curtain Road, Shoreditch", RestaurantSeedData.RestaurantLatitude, RestaurantSeedData.RestaurantLongitude),
            };

            if (form.Fulfillment == RestaurantFulfillment.Delivery && State.FindAddress(form.AddressId) is { } address)
                locations.Add(new ShowcaseLocation($"{address.Label}: {address.Street}", $"{address.Neighborhood} · about {address.Minutes} min", address.Latitude, address.Longitude));

            return locations;
        }
    }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private RestaurantState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    private RestaurantTotals Totals => State.CartTotals(form.Fulfillment, form.TipPercent);

    #endregion

    #region Nested Types

    private sealed class CheckoutForm
    {
        [RequiredForDelivery(ErrorMessage = "Choose where to deliver the order.")]
        public string AddressId { get; set; } = string.Empty;

        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string? AddressNote { get; set; }

        public RestaurantFulfillment Fulfillment { get; set; } = RestaurantFulfillment.Delivery;

        [Required(ErrorMessage = "Enter a name for the order.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Use 2 to 40 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a phone number.")]
        [RegularExpression(@"^(?=(?:\D*\d){7})\+?[0-9 ()-]{7,20}$", ErrorMessage = "Use at least 7 digits, with spaces, hyphens, parentheses, and an optional leading + (up to 20 characters).")]
        public string Phone { get; set; } = string.Empty;

        public int TipPercent { get; set; } = 10;
    }

    /// <summary>
    /// Requires a value only for delivery. A field attribute (rather than <see cref="IValidatableObject" />, which runs only
    /// after every other rule passes) shows the message in the same round as the other fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    private sealed class RequiredForDeliveryAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
            validationContext.ObjectInstance is CheckoutForm { Fulfillment: RestaurantFulfillment.Delivery } && string.IsNullOrEmpty(value as string)
                ? new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! })
                : ValidationResult.Success;
    }

    #endregion
}
