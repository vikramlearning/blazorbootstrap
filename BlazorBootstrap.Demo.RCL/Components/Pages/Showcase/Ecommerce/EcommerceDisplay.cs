using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Formatting and focus helpers shared by the Kindred Goods screens. Formatting uses the invariant culture, so the
/// Server and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class EcommerceDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Returns the delivery window text for a delivery speed.</summary>
    public static string DeliveryWindow(EcommerceDelivery delivery) => delivery == EcommerceDelivery.Express ? "1–2 business days" : "3–5 business days";

    /// <summary>
    /// Moves focus to an element, ignoring one that has left the page or a closed circuit.
    /// Dialogs and drawers opened from code don't get Bootstrap's focus return, so screens call this (WCAG 2.4.3).
    /// </summary>
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

    /// <summary>Formats an amount in US dollars with cents.</summary>
    public static string Money(decimal value) => value.ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Formats an average rating with one decimal.</summary>
    public static string Rating(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>Returns the star icons for an average rating, rounded to the nearest half star.</summary>
    public static IEnumerable<IconName> Stars(double rating)
    {
        var halves = (int)Math.Round(rating * 2, MidpointRounding.AwayFromZero);

        for (var star = 1; star <= 5; star++)
            yield return halves >= star * 2 ? IconName.StarFill : halves == (star * 2) - 1 ? IconName.StarHalf : IconName.Star;
    }

    #endregion

    #endregion
}
