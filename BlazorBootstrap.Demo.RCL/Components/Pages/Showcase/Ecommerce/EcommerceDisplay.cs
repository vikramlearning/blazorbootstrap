using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Formatting and focus helpers shared by the Kindred Goods screens. Formatting uses the invariant culture, so the
/// Server and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class EcommerceDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Explains why a coupon doesn't apply, in a shopper's words.</summary>
    public static string CouponProblem(EcommerceCouponCheck check, EcommerceCoupon? coupon) => check switch
    {
        EcommerceCouponCheck.Inactive => "This code isn't active right now.",
        EcommerceCouponCheck.Expired => "This code has expired.",
        EcommerceCouponCheck.NotStarted => coupon?.StartsOn is { } starts ? $"This code starts on {starts.ToString("MMM d", CultureInfo.InvariantCulture)}." : "This code hasn't started yet.",
        EcommerceCouponCheck.UsedUp => "This code has been used as many times as allowed.",
        EcommerceCouponCheck.BelowMinimum => $"This code needs a subtotal of {Money(coupon?.MinSubtotal ?? 0m)} or more.",
        EcommerceCouponCheck.NotFirstOrder => "This code is for a first order only.",
        EcommerceCouponCheck.Applied => "The code is applied.",
        _ => "That code isn't valid.",
    };

    /// <summary>Formats a count with thousands separators, like "1,204".</summary>
    public static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>Formats a date like "Nov 9, 2026".</summary>
    public static string Date(DateTime value) => value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Formats a date and time like "Nov 9, 2:15 PM".</summary>
    public static string DateTimeShort(DateTime value) => value.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);

    /// <summary>Returns a count with its noun, like "1 order" or "3 orders".</summary>
    public static string Plural(int count, string noun, string? plural = null) => $"{Count(count)} {(count == 1 ? noun : plural ?? noun + "s")}";

    /// <summary>Returns a return's number, like "R-0012".</summary>
    public static string ReturnNumber(int returnId) => EcommerceSeedData.ReturnNumber(returnId);

    /// <summary>Returns the delivery window text for a delivery speed.</summary>
    public static string DeliveryWindow(EcommerceDelivery delivery) => delivery switch
    {
        EcommerceDelivery.Express => "1–2 business days",
        EcommerceDelivery.Local => "within 3 hours by courier",
        _ => "3–5 business days",
    };

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

    /// <summary>Returns the badge color of an order status; the status name is always shown with it.</summary>
    public static BadgeColor StatusColor(EcommerceOrderStatus status) => status switch
    {
        EcommerceOrderStatus.Placed => BadgeColor.Warning,
        EcommerceOrderStatus.Packed => BadgeColor.Info,
        EcommerceOrderStatus.Shipped => BadgeColor.Primary,
        EcommerceOrderStatus.Delivered => BadgeColor.Success,
        EcommerceOrderStatus.Refunded => BadgeColor.Secondary,
        _ => BadgeColor.Danger,
    };

    /// <summary>Returns the badge color of a purchase-order status; the status name is always shown with it.</summary>
    public static BadgeColor PurchaseOrderColor(EcommercePurchaseOrderStatus status) => status switch
    {
        EcommercePurchaseOrderStatus.Draft => BadgeColor.Secondary,
        EcommercePurchaseOrderStatus.Submitted => BadgeColor.Warning,
        EcommercePurchaseOrderStatus.Approved => BadgeColor.Primary,
        EcommercePurchaseOrderStatus.PartlyReceived => BadgeColor.Info,
        EcommercePurchaseOrderStatus.Received => BadgeColor.Success,
        _ => BadgeColor.Dark,
    };

    /// <summary>Returns a purchase-order status as a label, like "Waiting for approval".</summary>
    public static string PurchaseOrderLabel(EcommercePurchaseOrderStatus status) => status switch
    {
        EcommercePurchaseOrderStatus.Draft => "Draft",
        EcommercePurchaseOrderStatus.Submitted => "Waiting for approval",
        EcommercePurchaseOrderStatus.Approved => "Approved, sent",
        EcommercePurchaseOrderStatus.PartlyReceived => "Partly received",
        EcommercePurchaseOrderStatus.Received => "Received",
        _ => "Cancelled",
    };

    /// <summary>Formats an average rating with one decimal.</summary>
    public static string Rating(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>Returns a product's stock with what's on order, like "3 left · 24 on order".</summary>
    public static string StockWithOnOrder(int stock, int onOrder) =>
        (stock == 0 ? "Out of stock" : $"{Count(stock)} in stock") + (onOrder > 0 ? $" · {Count(onOrder)} on order" : string.Empty);

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
