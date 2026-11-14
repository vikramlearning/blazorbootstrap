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
