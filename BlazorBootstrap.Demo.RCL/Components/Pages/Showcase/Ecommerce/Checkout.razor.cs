using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Checkout: contact and shipping form, delivery choice, the test-card payment form, and the order summary. A declined
/// test card shows an error and keeps the cart; an approved one stores the order in <see cref="EcommerceState" /> with the
/// card's brand and last four digits only, empties the cart, and opens the confirmation.
/// A signed-in customer gets their name and email filled in, can ship to a saved address (the default is preselected) or
/// save a new one to their address book, and can choose same-day local delivery for an address in the local area, with a
/// map of the courier's route and the estimated arrival.
/// </summary>
public partial class Checkout : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string DefaultAddressLabel = "Home";

    private static readonly string LocalAreaNote = $"Same-day delivery covers {string.Join(" and ", EcommerceSeedData.LocalDeliveryCities)} ({EcommerceSeedData.LocalDeliveryCountry}).";

    /// <summary>The most addresses an address book holds (<see cref="EcommerceState.SaveAddress" />).</summary>
    private const int MaxSavedAddresses = EcommerceSeedData.MaxSavedAddresses;

    private static readonly IReadOnlyList<string> countries = EcommerceSeedData.Countries;

    private ShowcaseCardInput card = new();

    private int declines;

    private string? deliveryStatus;

    private int deliveryStatusCount;

    private string? deliveryStatusText;

    private EditContext editContext = default!;

    private CheckoutForm form = new();

    private string? paymentError;

    private bool placing;

    /// <summary>Whether the signed-in customer's details were filled in; it happens once, when the account is first known.</summary>
    private bool prefilled;

    /// <summary>The saved address the order ships to, or <see langword="null" /> for "Use a new address".</summary>
    private int? selectedAddressId;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
        State.Changed += OnStateChanged;
        TryPrefill();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    /// <summary>Ships to a saved address: fills the name and address fields from it and re-checks them.</summary>
    private void ChooseAddress(EcommerceSavedAddress address)
    {
        selectedAddressId = address.Id;
        form.FullName = address.FullName;
        form.Street = address.Street;
        form.City = address.City;
        form.PostalCode = address.PostalCode;
        form.Country = address.Country;
        form.SaveAddress = false;

        NotifyAddressFieldsChanged();
    }

    /// <summary>Fills fictitious details, so nobody has to type real personal data into a demo.</summary>
    private void FillSample()
    {
        form.FullName = "Jordan Ashdown";
        form.Email = "jordan.ashdown@example.com";
        form.Street = "42 Larkspur Lane";
        form.City = "Fairbank";
        form.PostalCode = "10481";
        form.Country = countries[0];

        // Re-check only the filled fields: a full Validate() would also flag the still-empty card fields.
        editContext.NotifyFieldChanged(editContext.Field(nameof(CheckoutForm.Email)));
        NotifyAddressFieldsChanged();
    }

    /// <summary>Returns whether the address fields still hold a saved address.</summary>
    private bool Matches(EcommerceSavedAddress address) =>
        string.Equals(address.Street.Trim(), form.Street.Trim(), StringComparison.OrdinalIgnoreCase)
        && string.Equals(address.City.Trim(), form.City.Trim(), StringComparison.OrdinalIgnoreCase)
        && string.Equals(address.PostalCode.Trim(), form.PostalCode.Trim(), StringComparison.OrdinalIgnoreCase)
        && string.Equals(address.Country.Trim(), form.Country.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>Re-checks the name and address fields; a full Validate() would also flag the still-empty card fields.</summary>
    private void NotifyAddressFieldsChanged()
    {
        foreach (var field in new[] { nameof(CheckoutForm.FullName), nameof(CheckoutForm.Street), nameof(CheckoutForm.City), nameof(CheckoutForm.PostalCode), nameof(CheckoutForm.Country) })
            editContext.NotifyFieldChanged(editContext.Field(field));
    }

    /// <summary>
    /// Keeps the delivery choice valid for the address: Local delivery switches back to Standard when the city leaves the
    /// local area. Editing a saved address's fields turns it into a new address, which can then be saved.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (!ReferenceEquals(args.FieldIdentifier.Model, form))
            return;

        switch (args.FieldIdentifier.FieldName)
        {
            case nameof(CheckoutForm.City):
            case nameof(CheckoutForm.Street):
            case nameof(CheckoutForm.PostalCode):
            case nameof(CheckoutForm.Country):
                if (SelectedAddress is { } address && !Matches(address))
                    selectedAddressId = null;

                if (form.Delivery == EcommerceDelivery.Local && !IsLocalAvailable)
                {
                    form.Delivery = EcommerceDelivery.Standard;
                    SetDeliveryStatus($"Local delivery isn't available for this address, so delivery is back to Standard. {LocalAreaNote}");
                }

                break;
        }
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // The signed-in account may only be known once the tab's stored session is restored, after the first render.
        TryPrefill();
        StateHasChanged();
    });

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

        SaveNewAddress();

        card = new ShowcaseCardInput();
        Navigation.NavigateTo($"{EcommerceLayout.Base}/orders/{order.Id}");
    }

    /// <summary>
    /// Saves the new address to the customer's address book when they asked for it. The order is already placed, so a
    /// refused save (a full address book) only shows a toast; it never blocks the order.
    /// </summary>
    private void SaveNewAddress()
    {
        if (!form.SaveAddress || selectedAddressId is not null || Customer is not { } account)
            return;

        var label = string.IsNullOrWhiteSpace(form.AddressLabel) ? DefaultAddressLabel : form.AddressLabel.Trim();
        var saved = State.SaveAddress(new EcommerceSavedAddress(0, account.Id, label, form.FullName.Trim(), form.Street.Trim(), form.City.Trim(),
            form.PostalCode.Trim(), form.Country, Phone: null, IsDefault: false));

        Toasts.Notify(saved is not null
            ? new ToastMessage(ToastType.Success, IconName.HouseCheck, "Address saved", $"The address is in your address book as \"{saved.Label}\".")
            : new ToastMessage(ToastType.Warning, IconName.ExclamationTriangle, "Address not saved", "Your order is placed, but the address couldn't be added to your address book. It may be full."));
    }

    /// <summary>Shows a delivery status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetDeliveryStatus(string message)
    {
        deliveryStatusCount = message == deliveryStatusText ? deliveryStatusCount + 1 : 1;
        deliveryStatusText = message;
        deliveryStatus = deliveryStatusCount == 1 ? message : $"{message} ({deliveryStatusCount})";
    }

    /// <summary>
    /// Fills a signed-in customer's name and email into empty fields and preselects their default address, once. The
    /// fields the customer already typed in are never overwritten.
    /// </summary>
    private void TryPrefill()
    {
        if (prefilled || Customer is not { } account)
            return;

        prefilled = true;

        var nameEmpty = string.IsNullOrWhiteSpace(form.FullName);
        var addressEmpty = string.IsNullOrWhiteSpace(form.Street) && string.IsNullOrWhiteSpace(form.City) && string.IsNullOrWhiteSpace(form.PostalCode);
        var preferred = addressEmpty ? SavedAddresses.FirstOrDefault(address => address.IsDefault) ?? SavedAddresses.FirstOrDefault() : null;

        if (nameEmpty)
            form.FullName = preferred?.FullName is { } addressName && !string.IsNullOrWhiteSpace(addressName) ? addressName : account.Name;

        if (string.IsNullOrWhiteSpace(form.Email))
            form.Email = account.Email;

        if (preferred is not null)
        {
            selectedAddressId = preferred.Id;
            form.Street = preferred.Street;
            form.City = preferred.City;
            form.PostalCode = preferred.PostalCode;
            form.Country = preferred.Country;
        }

        // No field-changed notifications here: the fields are valid or still empty, and empty ones shouldn't show errors before the customer types.
    }

    /// <summary>Switches to a new address: the address fields empty so the customer can type it, keeping their name and email.</summary>
    private void UseNewAddress()
    {
        if (selectedAddressId is null)
            return;

        selectedAddressId = null;
        form.Street = string.Empty;
        form.City = string.Empty;
        form.PostalCode = string.Empty;
        form.AddressLabel = DefaultAddressLabel;

        // The city is now empty, so Local delivery no longer applies.
        if (form.Delivery == EcommerceDelivery.Local)
        {
            form.Delivery = EcommerceDelivery.Standard;
            SetDeliveryStatus($"Delivery is back to Standard until you enter an address in the local area. {LocalAreaNote}");
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string ArrivalText
    {
        get
        {
            var now = State.Clock.Now;
            var arrival = now.AddHours(3);
            var day = arrival.Date == now.Date ? "today" : "tomorrow";

            return $"Arrives {day} by about {arrival.ToString("h:mm tt", CultureInfo.InvariantCulture)}";
        }
    }

    private IReadOnlyList<string> CountryOptions => string.IsNullOrWhiteSpace(form.Country) || countries.Contains(form.Country)
        ? countries
        : countries.Append(form.Country).ToArray();

    /// <summary>Gets the signed-in customer, or <see langword="null" /> when signed out or signed in as staff.</summary>
    private ShowcaseAccount? Customer => State.Identity.Current is { RoleId: EcommerceSeedData.CustomerRole } account ? account : null;

    private IReadOnlyList<ShowcaseLocation> DeliveryLocations
    {
        get
        {
            var store = EcommerceSeedData.Store;
            var (latitude, longitude) = EcommerceSeedData.PositionFor(form.City, form.Street);

            return new[]
            {
                new ShowcaseLocation(store.Name, store.Street, store.Latitude, store.Longitude),
                new ShowcaseLocation("Your delivery address", $"{form.Street.Trim()}, {form.City.Trim()}", latitude, longitude),
            };
        }
    }

    private string DeliveryMapLabel => $"Map preview of the courier route: pin 1 is {EcommerceSeedData.Store.Name} in {EcommerceSeedData.Store.Street}, pin 2 is your delivery address in {form.City.Trim()}";

    private bool IsLocalAvailable => EcommerceState.IsLocalDeliveryCity(form.City, form.Country);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<EcommerceSavedAddress> SavedAddresses => Customer is { } account
        ? State.AddressesFor(account.Id)
        : Array.Empty<EcommerceSavedAddress>();

    private EcommerceSavedAddress? SelectedAddress => selectedAddressId is { } id ? SavedAddresses.FirstOrDefault(address => address.Id == id) : null;

    private string StandardPrice => State.CartTotals(EcommerceDelivery.Standard).Shipping == 0
        ? "Free"
        : EcommerceDisplay.Money(EcommerceSeedData.StandardShipping);

    [Inject] private EcommerceState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    private EcommerceTotals Totals => State.CartTotals(form.Delivery);

    #endregion

    #region Nested Types

    private sealed class CheckoutForm
    {
        [StringLength(30, ErrorMessage = "Use 30 characters or fewer.")]
        public string AddressLabel { get; set; } = DefaultAddressLabel;

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

        public bool SaveAddress { get; set; }

        [Required(ErrorMessage = "Enter the street address.")]
        [StringLength(100, ErrorMessage = "Use 100 characters or fewer.")]
        public string Street { get; set; } = string.Empty;
    }

    #endregion
}
