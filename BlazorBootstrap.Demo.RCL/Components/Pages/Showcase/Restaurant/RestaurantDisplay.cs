using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Restaurant;

/// <summary>
/// Formatting and focus helpers shared by the Saffron Table screens. Formatting uses the invariant culture, so the
/// Server and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class RestaurantDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Returns the dietary labels of an item, in a fixed order.</summary>
    public static IEnumerable<(string Label, BadgeColor Color)> DietLabels(RestaurantDiet diet)
    {
        if (diet.HasFlag(RestaurantDiet.Vegan))
            yield return ("Vegan", BadgeColor.Success);
        else if (diet.HasFlag(RestaurantDiet.Vegetarian))
            yield return ("Vegetarian", BadgeColor.Success);

        if (diet.HasFlag(RestaurantDiet.GlutenFree))
            yield return ("Gluten-free", BadgeColor.Info);

        if (diet.HasFlag(RestaurantDiet.Spicy))
            yield return ("Spicy", BadgeColor.Danger);
    }

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

    /// <summary>Returns the route of a menu item.</summary>
    public static string ItemUrl(int itemId) => FormattableString.Invariant($"{RestaurantLayout.Base}/menu/{itemId}");

    /// <summary>Formats an amount in US dollars with cents.</summary>
    public static string Money(decimal value) => value.ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Returns the route of an order's tracking page.</summary>
    public static string OrderUrl(int orderId) => FormattableString.Invariant($"{RestaurantLayout.Base}/orders/{orderId}");

    /// <summary>Returns the name of a status for delivery or pickup.</summary>
    public static string StatusLabel(RestaurantOrderStatus status, RestaurantFulfillment fulfillment) => status switch
    {
        RestaurantOrderStatus.Received => "Order received",
        RestaurantOrderStatus.Preparing => "Preparing",
        RestaurantOrderStatus.OnTheWay => fulfillment == RestaurantFulfillment.Delivery ? "Out for delivery" : "Ready for pickup",
        _ => fulfillment == RestaurantFulfillment.Delivery ? "Delivered" : "Picked up",
    };

    /// <summary>Formats a time like "7:05 PM".</summary>
    public static string Time(DateTime time) => time.ToString("h:mm tt", CultureInfo.InvariantCulture);

    #endregion

    #endregion
}
