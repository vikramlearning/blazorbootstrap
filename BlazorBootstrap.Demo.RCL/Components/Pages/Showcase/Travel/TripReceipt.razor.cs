using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// A printable receipt of one of the signed-in guest's trips: the site, the reference, the guest, the stay and dates, the
/// price lines, every payment and refund with its date, the card's brand and last four digits, and the totals, marked as a
/// demo receipt. The print button opens the browser's print dialog; the site's chrome and the buttons don't print.
/// Someone else's or an unknown reference shows a not-found screen.
/// </summary>
public partial class TripReceipt : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the check-in time, like "3 pm".</summary>
    private static string CheckInTime(TravelBooking trip) => TravelState.CheckInAt(trip).ToString("h tt", CultureInfo.InvariantCulture).ToLowerInvariant();

    /// <summary>Formats a charge; a refund gets a minus sign, which screen readers read.</summary>
    private static string SignedMoney(decimal amount) => amount < 0 ? $"−{TravelDisplay.Money(-amount)}" : TravelDisplay.Money(amount);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task PrintAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("print");
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            // Printing is a convenience; the receipt stays on screen, and the browser's own print command still works.
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private TravelBooking? Booking => State.FindBooking(Reference);

    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>Gets or sets the booking reference from the route.</summary>
    [Parameter] public string Reference { get; set; } = string.Empty;

    [Inject] private TravelState State { get; set; } = default!;

    private static string TaxRateText => (TravelSeedData.TaxRate * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";

    #endregion
}
