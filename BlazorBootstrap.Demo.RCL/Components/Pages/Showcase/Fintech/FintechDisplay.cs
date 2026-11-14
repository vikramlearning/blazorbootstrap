using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Formatting and focus helpers shared by the Tallymint screens. Formatting uses the invariant culture, so the Server and
/// WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class FintechDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Returns the CSS class that tints an amount; the sign in <see cref="SignedMoney" /> carries the meaning too.</summary>
    public static string AmountClass(decimal amount) => amount >= 0 ? "tm-in" : "tm-out";

    /// <summary>Formats a date like "Nov 24".</summary>
    public static string Day(DateTime date) => date.ToString("MMM d", CultureInfo.InvariantCulture);

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

    /// <summary>Returns the label of an account kind.</summary>
    public static string KindLabel(FintechAccountKind kind) => kind switch
    {
        FintechAccountKind.Savings => "Savings",
        FintechAccountKind.CreditCard => "Credit card",
        _ => "Checking",
    };

    /// <summary>Formats an amount in US dollars with cents, without a sign.</summary>
    public static string Money(decimal value) => Math.Abs(value).ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Formats an amount with an explicit sign, so money in and out never relies on color alone.</summary>
    public static string SignedMoney(decimal value) => (value >= 0 ? "+" : "−") + Money(value);

    #endregion

    #endregion
}
