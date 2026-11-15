using System.Globalization;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>What a test card does when it is charged.</summary>
public enum ShowcaseCardOutcome
{
    /// <summary>The payment goes through.</summary>
    Approved,

    /// <summary>The bank declines the card.</summary>
    Declined,

    /// <summary>The bank declines for insufficient funds.</summary>
    InsufficientFunds,
}

/// <summary>A published test card number and what it does.</summary>
public sealed record ShowcaseTestCard(string Number, string Brand, ShowcaseCardOutcome Outcome)
{
    #region Properties, Indexers

    /// <summary>Gets the number grouped for display, such as "4242 4242 4242 4242".</summary>
    public string Display => ShowcaseCards.Group(Number);

    /// <summary>Gets what happens when the card is charged, in words.</summary>
    public string OutcomeText => Outcome switch
    {
        ShowcaseCardOutcome.Approved => "Approved",
        ShowcaseCardOutcome.Declined => "Declined",
        _ => "Insufficient funds",
    };

    #endregion
}

/// <summary>The card details typed into a payment form. Kept only while the form is open; never stored or sent.</summary>
public sealed class ShowcaseCardInput
{
    #region Properties, Indexers

    /// <summary>Gets or sets the CVC.</summary>
    public string Cvc { get; set; } = string.Empty;

    /// <summary>Gets or sets the expiry as MM/YY.</summary>
    public string Expiry { get; set; } = string.Empty;

    /// <summary>Gets or sets the name on the card.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the card number as typed.</summary>
    public string Number { get; set; } = string.Empty;

    #endregion
}

/// <summary>The result of charging a test card: only the brand and last four digits are kept.</summary>
public sealed record ShowcaseCardCharge(bool IsApproved, string Brand, string Last4, string? FailureMessage);

/// <summary>
/// Validation and charging for the showcase payment forms. Only the published test numbers in <see cref="TestCards" />
/// are accepted; any other number, including a real card, is rejected, so a real card can never pass.
/// </summary>
public static class ShowcaseCards
{
    #region Fields and Constants

    /// <summary>The test cards a payment form lists and accepts.</summary>
    public static readonly IReadOnlyList<ShowcaseTestCard> TestCards = new List<ShowcaseTestCard>
    {
        new("4242424242424242", "Visa", ShowcaseCardOutcome.Approved),
        new("5555555555554444", "Mastercard", ShowcaseCardOutcome.Approved),
        new("378282246310005", "American Express", ShowcaseCardOutcome.Approved),
        new("4000000000000002", "Visa", ShowcaseCardOutcome.Declined),
        new("4000000000009995", "Visa", ShowcaseCardOutcome.InsufficientFunds),
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the card brand for a number's first digits, or <see langword="null" /> when unknown.</summary>
    public static string? Brand(string? number)
    {
        var digits = Digits(number);

        if (digits.StartsWith('4'))
            return "Visa";

        if (digits.Length >= 2 && (digits[..2] is "34" or "37"))
            return "American Express";

        if (digits.Length >= 2 && int.Parse(digits[..2], CultureInfo.InvariantCulture) is >= 51 and <= 55)
            return "Mastercard";

        if (digits.Length >= 4 && int.Parse(digits[..4], CultureInfo.InvariantCulture) is >= 2221 and <= 2720)
            return "Mastercard";

        return null;
    }

    /// <summary>Charges a card that passed <see cref="Validate" />; returns the outcome with the brand and last four digits only.</summary>
    public static ShowcaseCardCharge Charge(ShowcaseCardInput card)
    {
        var digits = Digits(card.Number);
        var test = TestCards.First(candidate => candidate.Number == digits);

        return test.Outcome switch
        {
            ShowcaseCardOutcome.Approved => new(true, test.Brand, digits[^4..], null),
            ShowcaseCardOutcome.Declined => new(false, test.Brand, digits[^4..], "Your card was declined. Try another card."),
            _ => new(false, test.Brand, digits[^4..], "Your card has insufficient funds. Try another card."),
        };
    }

    /// <summary>Returns only the digits of a number as typed.</summary>
    public static string Digits(string? number) => new((number ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    /// <summary>Groups a card number for display: 4-6-5 for American Express, otherwise blocks of four.</summary>
    public static string Group(string number)
    {
        var digits = Digits(number);

        if (digits.Length == 15 && digits[0] == '3')
            return $"{digits[..4]} {digits[4..10]} {digits[10..]}";

        return string.Join(' ', digits.Chunk(4).Select(chunk => new string(chunk)));
    }

    /// <summary>Returns whether a digit string passes the Luhn check.</summary>
    public static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var doubleIt = false;

        for (var index = digits.Length - 1; index >= 0; index--)
        {
            var digit = digits[index] - '0';

            if (doubleIt)
            {
                digit *= 2;

                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            doubleIt = !doubleIt;
        }

        return digits.Length > 0 && sum % 10 == 0;
    }

    /// <summary>
    /// Validates the card fields against the site's current date. Returns one message per invalid field, keyed by the
    /// <see cref="ShowcaseCardInput" /> property name; an empty result means the card can be charged.
    /// </summary>
    public static Dictionary<string, string> Validate(ShowcaseCardInput card, DateTime today)
    {
        var errors = new Dictionary<string, string>();
        var digits = Digits(card.Number);
        var brand = Brand(digits);

        if (string.IsNullOrWhiteSpace(card.Name))
            errors[nameof(ShowcaseCardInput.Name)] = "Enter the name on the card.";
        else if (card.Name.Trim().Length > 60)
            errors[nameof(ShowcaseCardInput.Name)] = "Use 60 characters or fewer.";

        if (digits.Length == 0)
            errors[nameof(ShowcaseCardInput.Number)] = "Enter the card number.";
        else if (card.Number.Any(ch => !char.IsAsciiDigit(ch) && ch != ' ' && ch != '-') || digits.Length is < 13 or > 19 || !PassesLuhn(digits) || brand is null)
            errors[nameof(ShowcaseCardInput.Number)] = "This card number isn't valid. Check the digits.";
        else if (TestCards.All(test => test.Number != digits))
            errors[nameof(ShowcaseCardInput.Number)] = "This is a demo: use one of the test card numbers listed below. Real cards are never accepted.";

        if (!TryParseExpiry(card.Expiry, out var month, out var year))
            errors[nameof(ShowcaseCardInput.Expiry)] = "Enter the expiry as MM/YY, for example 08/29.";
        else if (year < today.Year || (year == today.Year && month < today.Month))
            errors[nameof(ShowcaseCardInput.Expiry)] = "This card has expired.";
        else if (year > today.Year + 20)
            errors[nameof(ShowcaseCardInput.Expiry)] = "Check the expiry year.";

        var cvcLength = brand == "American Express" ? 4 : 3;
        var cvc = card.Cvc.Trim();

        if (cvc.Length != cvcLength || !cvc.All(char.IsAsciiDigit))
            errors[nameof(ShowcaseCardInput.Cvc)] = $"Enter the {cvcLength}-digit security code.";

        return errors;
    }

    #endregion

    #region Private Methods

    private static bool TryParseExpiry(string? expiry, out int month, out int year)
    {
        month = 0;
        year = 0;
        var parts = (expiry ?? string.Empty).Trim().Split('/');

        if (parts.Length != 2 || parts[0].Length is < 1 or > 2 || parts[1].Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out month)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var shortYear)
            || month is < 1 or > 12)
            return false;

        year = 2000 + shortYear;

        return true;
    }

    #endregion

    #endregion
}
