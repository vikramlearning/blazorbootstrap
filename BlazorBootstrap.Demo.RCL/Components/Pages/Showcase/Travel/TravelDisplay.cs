using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Formatting and focus helpers shared by the Driftwood Stays screens. Formatting uses the invariant culture, so the
/// Server and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class TravelDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Formats a date like "Dec 8".</summary>
    public static string Day(DateOnly date) => date.ToString("MMM d", CultureInfo.InvariantCulture);

    /// <summary>Formats a date range like "Dec 8 – Dec 12, 2026".</summary>
    public static string Dates(DateOnly checkIn, DateOnly checkOut) =>
        $"{Day(checkIn)} – {checkOut.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";

    /// <summary>Moves focus to an element, ignoring one that has left the page or a closed circuit (WCAG 2.4.3).</summary>
    public static async Task FocusAsync(ElementReference? element)
    {
        if (element is not ElementReference target)
            return;

        try
        {
            await target.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Nothing left to focus.
        }
    }

    /// <summary>Returns "1 guest" or "3 guests".</summary>
    public static string Guests(int count) => count == 1 ? "1 guest" : FormattableString.Invariant($"{count} guests");

    /// <summary>Returns the map location of a stay; the detail line is what the placeholder's list shows.</summary>
    public static ShowcaseLocation Location(TravelStay stay, double rating) =>
        new(stay.Name, $"{stay.Neighborhood} · {Money(stay.NightlyRate)} a night · rated {Rating(rating)}", stay.Latitude, stay.Longitude);

    /// <summary>Formats an amount in US dollars, without cents when it is whole.</summary>
    public static string Money(decimal value) =>
        value == decimal.Truncate(value) ? value.ToString("$#,0", CultureInfo.InvariantCulture) : value.ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Returns "1 night" or "4 nights".</summary>
    public static string Nights(int count) => count == 1 ? "1 night" : FormattableString.Invariant($"{count} nights");

    /// <summary>Formats an average rating like "4.5".</summary>
    public static string Rating(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>Returns the route of a stay page.</summary>
    public static string StayUrl(int stayId) => FormattableString.Invariant($"{TravelLayout.Base}/stays/{stayId}");

    /// <summary>Returns the route of a trip (booking) page.</summary>
    public static string TripUrl(string reference) => $"{TravelLayout.Base}/trips/{Uri.EscapeDataString(reference)}";

    /// <summary>Returns the route of a trip's printable receipt.</summary>
    public static string ReceiptUrl(string reference) => $"{TripUrl(reference)}/receipt";

    /// <summary>Returns the route of a conversation with a host.</summary>
    public static string ThreadUrl(int threadId) => FormattableString.Invariant($"{TravelLayout.Base}/messages/{threadId}");

    /// <summary>Returns the sign-in route that comes back to <paramref name="returnPath" /> (a path inside the site).</summary>
    public static string SignInUrl(string returnPath, bool register = false) =>
        $"{TravelLayout.SignInPath}?{(register ? "mode=register&" : string.Empty)}returnUrl={Uri.EscapeDataString(returnPath)}";

    /// <summary>Formats a date and time like "Nov 24, 10:05".</summary>
    public static string Stamp(DateTime value) => value.ToString("MMM d, HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Returns the guest-facing label of a trip phase.</summary>
    public static string PhaseText(TravelTripPhase phase) => phase switch
    {
        TravelTripPhase.Requested => "Request sent",
        TravelTripPhase.Upcoming => "Confirmed",
        TravelTripPhase.InProgress => "Staying now",
        TravelTripPhase.Completed => "Completed",
        TravelTripPhase.Cancelled => "Cancelled",
        TravelTripPhase.Expired => "Request expired",
        _ => "Declined by host",
    };

    /// <summary>Returns the badge color of a trip phase.</summary>
    public static BadgeColor PhaseColor(TravelTripPhase phase) => phase switch
    {
        TravelTripPhase.Requested => BadgeColor.Warning,
        TravelTripPhase.Upcoming or TravelTripPhase.InProgress => BadgeColor.Success,
        TravelTripPhase.Completed or TravelTripPhase.Expired => BadgeColor.Secondary,
        _ => BadgeColor.Danger,
    };

    /// <summary>Returns the plain-language rule of a cancellation policy (check-in starts at 3 pm on the check-in day).</summary>
    public static string PolicyText(TravelCancellationPolicy policy) => policy switch
    {
        TravelCancellationPolicy.Flexible => "Full refund until 24 hours before check-in. After that, the first night is kept.",
        TravelCancellationPolicy.Moderate => "Full refund until 5 days before check-in. After that, half of the nights are refunded.",
        _ => "Half of the nights are refunded until 7 days before check-in. After that, the nights aren't refunded.",
    };

    /// <summary>Returns the label of a stay type.</summary>
    public static string TypeLabel(TravelStayType type) => type switch
    {
        TravelStayType.Hotel => "Hotel room",
        TravelStayType.Apartment => "Apartment",
        TravelStayType.Cabin => "Cabin",
        _ => "Villa",
    };

    #endregion

    #endregion
}
