using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The sensitive data patterns (AC-7i), shared by the state and the chat: finding them in a message (payment cards that pass
/// the Luhn check, US Social Security numbers outside the impossible ranges, IBANs whose check digits are right, and
/// <c>sk-</c> API keys), masking a match for the review queue (only the last four characters stay), and replacing matches with
/// a placeholder like "[card number removed]". Nothing here keeps or sends a raw value.
/// </summary>
public static partial class AiDlp
{
    #region Methods

    #region Public Methods

    /// <summary>Returns the policy a fresh workspace starts with: cards and API keys blocked, ID numbers and IBANs redacted.</summary>
    public static List<AiDlpRule> DefaultRules() => new()
    {
        new() { Kind = AiDlpKind.PaymentCard, Enabled = true, Action = AiDlpAction.Block },
        new() { Kind = AiDlpKind.NationalId, Enabled = true, Action = AiDlpAction.Redact },
        new() { Kind = AiDlpKind.Iban, Enabled = true, Action = AiDlpAction.Redact },
        new() { Kind = AiDlpKind.ApiKey, Enabled = true, Action = AiDlpAction.Block },
    };

    /// <summary>
    /// Finds the matches of the enabled rules in a text, without overlaps (IBANs and API keys first, then ID numbers, then
    /// cards), in the order they appear.
    /// </summary>
    public static IReadOnlyList<AiDlpMatch> Find(string? text, IReadOnlyList<AiDlpRule> rules)
    {
        text ??= string.Empty;
        var accepted = new List<AiDlpMatch>();

        foreach (var kind in new[] { AiDlpKind.Iban, AiDlpKind.ApiKey, AiDlpKind.NationalId, AiDlpKind.PaymentCard })
        {
            if (rules.FirstOrDefault(rule => rule.Kind == kind) is not { Enabled: true } rule)
                continue;

            foreach (Match match in PatternOf(kind).Matches(text))
            {
                var value = match.Value;

                if (!IsValid(kind, value) || accepted.Any(other => match.Index < other.Index + other.Length && other.Index < match.Index + match.Length))
                    continue;

                accepted.Add(new AiDlpMatch(kind, rule.Action, match.Index, match.Length, Mask(kind, value)));
            }
        }

        return accepted.OrderBy(match => match.Index).ToList();
    }

    /// <summary>Returns the kind of data in words for the middle of a sentence, like "a payment card number".</summary>
    public static string KindArticle(AiDlpKind kind) => kind switch
    {
        AiDlpKind.PaymentCard => "a payment card number",
        AiDlpKind.NationalId => "a national ID number",
        AiDlpKind.Iban => "a bank account number (IBAN)",
        _ => "an API key",
    };

    /// <summary>Returns the kind of data in words, like "Payment card number".</summary>
    public static string KindLabel(AiDlpKind kind) => kind switch
    {
        AiDlpKind.PaymentCard => "Payment card number",
        AiDlpKind.NationalId => "National ID (SSN)",
        AiDlpKind.Iban => "Bank account (IBAN)",
        _ => "API key",
    };

    /// <summary>Returns a match masked for the review queue: only its last four characters (and an IBAN's country) show.</summary>
    public static string Mask(AiDlpKind kind, string value)
    {
        var compact = new string(value.Where(char.IsLetterOrDigit).ToArray());
        var last = compact.Length >= 4 ? compact[^4..] : compact;

        return kind switch
        {
            AiDlpKind.PaymentCard => $"•••• •••• •••• {last}",
            AiDlpKind.NationalId => $"•••-••-{last}",
            AiDlpKind.Iban => $"{compact[..2].ToUpperInvariant()}•• •••• •••• {last.ToUpperInvariant()}",
            _ => $"sk-••••••••{last}",
        };
    }

    /// <summary>Returns the placeholder a redacted match becomes, like "[card number removed]".</summary>
    public static string Placeholder(AiDlpKind kind) => kind switch
    {
        AiDlpKind.PaymentCard => "[card number removed]",
        AiDlpKind.NationalId => "[ID number removed]",
        AiDlpKind.Iban => "[bank account removed]",
        _ => "[API key removed]",
    };

    /// <summary>Returns the text with every given match replaced by its placeholder.</summary>
    public static string Redact(string text, IEnumerable<AiDlpMatch> matches)
    {
        var builder = new StringBuilder(text);

        foreach (var match in matches.OrderByDescending(item => item.Index))
            builder.Remove(match.Index, match.Length).Insert(match.Index, Placeholder(match.Kind));

        return builder.ToString();
    }

    /// <summary>Returns whether a stored masked match has the form <see cref="Mask" /> writes (so no raw value can be restored).</summary>
    public static bool IsMasked(AiDlpKind kind, string? masked) => masked is not null && kind switch
    {
        AiDlpKind.PaymentCard => MaskedCardPattern().IsMatch(masked),
        AiDlpKind.NationalId => MaskedIdPattern().IsMatch(masked),
        AiDlpKind.Iban => MaskedIbanPattern().IsMatch(masked),
        _ => MaskedKeyPattern().IsMatch(masked),
    };

    #endregion

    #region Private Methods

    [GeneratedRegex(@"\bsk-[A-Za-z0-9_-]{20,}", RegexOptions.CultureInvariant)]
    private static partial Regex ApiKeyPattern();

    [GeneratedRegex(@"(?<![0-9])(?:[0-9][ -]?){12,18}[0-9](?![0-9])", RegexOptions.CultureInvariant)]
    private static partial Regex CardPattern();

    [GeneratedRegex(@"\b[A-Za-z]{2}[0-9]{2}(?: ?[A-Za-z0-9]{4}){2,7}(?: ?[A-Za-z0-9]{1,3})?\b", RegexOptions.CultureInvariant)]
    private static partial Regex IbanPattern();

    /// <summary>Whether a candidate is real data of its kind: the Luhn check, the SSN ranges, or the IBAN's check digits.</summary>
    private static bool IsValid(AiDlpKind kind, string value)
    {
        switch (kind)
        {
            case AiDlpKind.PaymentCard:
                var digits = value.Where(char.IsAsciiDigit).Select(character => character - '0').ToList();

                if (digits.Count is < 13 or > 19)
                    return false;

                var sum = 0;

                for (var index = 0; index < digits.Count; index++)
                {
                    var digit = digits[digits.Count - 1 - index];

                    if (index % 2 == 1)
                        digit = digit * 2 > 9 ? (digit * 2) - 9 : digit * 2;

                    sum += digit;
                }

                return sum % 10 == 0;

            case AiDlpKind.NationalId:
                var area = int.Parse(value[..3], System.Globalization.CultureInfo.InvariantCulture);

                return area is not (0 or 666) && area < 900 && value.Substring(4, 2) != "00" && value[^4..] != "0000";

            case AiDlpKind.Iban:
                var compact = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

                if (compact.Length is < 15 or > 34)
                    return false;

                var rearranged = compact[4..] + compact[..4];
                var number = new StringBuilder();

                foreach (var character in rearranged)
                    number.Append(char.IsAsciiDigit(character) ? character.ToString() : (character - 'A' + 10).ToString(System.Globalization.CultureInfo.InvariantCulture));

                return BigInteger.Parse(number.ToString(), System.Globalization.CultureInfo.InvariantCulture) % 97 == 1;

            default:
                return true;
        }
    }

    [GeneratedRegex(@"^•••• •••• •••• [0-9]{4}$")]
    private static partial Regex MaskedCardPattern();

    [GeneratedRegex(@"^•••-••-[0-9]{4}$")]
    private static partial Regex MaskedIdPattern();

    [GeneratedRegex(@"^[A-Z]{2}•• •••• •••• [A-Z0-9]{1,4}$")]
    private static partial Regex MaskedIbanPattern();

    [GeneratedRegex(@"^sk-••••••••[A-Za-z0-9_-]{1,4}$")]
    private static partial Regex MaskedKeyPattern();

    private static Regex PatternOf(AiDlpKind kind) => kind switch
    {
        AiDlpKind.PaymentCard => CardPattern(),
        AiDlpKind.NationalId => SsnPattern(),
        AiDlpKind.Iban => IbanPattern(),
        _ => ApiKeyPattern(),
    };

    [GeneratedRegex(@"(?<![0-9])[0-9]{3}-[0-9]{2}-[0-9]{4}(?![0-9])", RegexOptions.CultureInvariant)]
    private static partial Regex SsnPattern();

    #endregion

    #endregion
}

/// <summary>One match of the sensitive data policy in a message: its kind, the rule's action, where it is, and the match masked.</summary>
public sealed record AiDlpMatch(AiDlpKind Kind, AiDlpAction Action, int Index, int Length, string Masked);
