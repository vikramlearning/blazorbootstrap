using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's plan editor form: one plan's fields as typed. Numbers are kept as text, so a half-typed value never
/// throws, and <see cref="Validate" /> checks them against the rules of <c>SaasState.UpdatePlan</c> before saving, so each
/// field gets its own message. A limit of 0 means unlimited, which the form shows as an "Unlimited" checkbox.
/// </summary>
public sealed class SaasPlanDraft
{
    #region Fields and Constants

    /// <summary>The longest call to action.</summary>
    public const int MaxCallToActionLength = 40;

    /// <summary>The highest dashboard limit (0 is unlimited).</summary>
    public const int MaxDashboards = 10_000;

    /// <summary>The longest description.</summary>
    public const int MaxDescriptionLength = 200;

    /// <summary>The highest event limit (0 is unlimited).</summary>
    public const long MaxEvents = 10_000_000_000;

    /// <summary>The longest highlight line.</summary>
    public const int MaxHighlightLength = 80;

    /// <summary>The most highlight lines.</summary>
    public const int MaxHighlights = 8;

    /// <summary>The longest plan name.</summary>
    public const int MaxNameLength = 40;

    /// <summary>The highest price per seat per month, in dollars.</summary>
    public const decimal MaxPrice = 10_000m;

    /// <summary>The lowest event limit other than unlimited.</summary>
    public const long MinEvents = 1_000;

    /// <summary>The shortest plan name.</summary>
    public const int MinNameLength = 2;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns a form filled with a plan's current values.</summary>
    public static SaasPlanDraft From(SaasPlan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        Description = plan.Description,
        Price = plan.MonthlyPricePerSeat.ToString("0.00", CultureInfo.InvariantCulture),
        Highlights = string.Join("\n", plan.Highlights),
        CallToAction = plan.CallToAction,
        SeatLimit = plan.SeatLimit == 0 ? string.Empty : plan.SeatLimit.ToString(CultureInfo.InvariantCulture),
        SeatsUnlimited = plan.SeatLimit == 0,
        EventLimit = plan.EventLimit == 0 ? string.Empty : plan.EventLimit.ToString("#,0", CultureInfo.InvariantCulture),
        EventsUnlimited = plan.EventLimit == 0,
        DashboardLimit = plan.DashboardLimit == 0 ? string.Empty : plan.DashboardLimit.ToString(CultureInfo.InvariantCulture),
        DashboardsUnlimited = plan.DashboardLimit == 0,
        IsSelfServe = plan.IsSelfServe,
        IsPopular = plan.IsPopular,
    };

    /// <summary>
    /// Returns the message for a seat limit below the seats of a workspace on the plan, naming how many workspaces are on it
    /// and the largest seat count, which is the lowest limit allowed.
    /// </summary>
    public static string SeatLimitMessage(string planName, int workspaceCount, int largestSeats) =>
        $"{Count(workspaceCount)} {(workspaceCount == 1 ? "workspace is" : "workspaces are")} on {planName}, and the largest has {Count(largestSeats)} seats. "
        + $"Set the seat limit to {Count(largestSeats)} or more, or tick Unlimited.";

    /// <summary>Returns the highlight lines: trimmed, without empty lines.</summary>
    public IReadOnlyList<string> HighlightLines() =>
        (Highlights ?? string.Empty).Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0).ToList();

    /// <summary>Returns the plan this form describes; call it only after <see cref="Validate" /> found nothing wrong.</summary>
    public SaasPlan ToPlan(SaasPlan current)
    {
        TryGetPrice(out var price);

        return current with
        {
            Name = (Name ?? string.Empty).Trim(),
            Description = (Description ?? string.Empty).Trim(),
            MonthlyPricePerSeat = price,
            Highlights = HighlightLines(),
            CallToAction = (CallToAction ?? string.Empty).Trim(),
            SeatLimit = SeatsUnlimited ? 0 : TryParseWhole(SeatLimit, out var seats) ? (int)seats : current.SeatLimit,
            EventLimit = EventsUnlimited ? 0 : TryParseWhole(EventLimit, out var events) ? events : current.EventLimit,
            DashboardLimit = DashboardsUnlimited ? 0 : TryParseWhole(DashboardLimit, out var dashboards) ? (int)dashboards : current.DashboardLimit,
            IsSelfServe = IsSelfServe,
            IsPopular = IsPopular,
        };
    }

    /// <summary>Reads the price as dollars and cents ("49", "49.50", "$1,200"); <see langword="false" /> when it isn't a number.</summary>
    public bool TryGetPrice(out decimal price)
    {
        var text = (Price ?? string.Empty).Trim().TrimStart('$').Replace(",", string.Empty, StringComparison.Ordinal).Trim();

        return decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price) && text.Length > 0;
    }

    /// <summary>Reads a whole number, allowing thousands separators ("5,000,000"); <see langword="false" /> when it isn't one.</summary>
    public static bool TryParseWhole(string? text, out long value)
    {
        var digits = (text ?? string.Empty).Trim().Replace(",", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal);

        return long.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out value) && digits.Length > 0;
    }

    /// <summary>
    /// Checks the form against the plan rules, in the order of the form's fields. Returns each field with a problem (by its
    /// property name) and the message to show; an empty list means the plan can be saved.
    /// </summary>
    /// <param name="current">The plan as it is now: a free plan stays free and a paid plan stays paid.</param>
    /// <param name="plans">All plans: at least one must stay self-serve. (Another plan with the same name is checked by the form as you type.)</param>
    /// <param name="workspaceCount">How many workspaces are on the plan.</param>
    /// <param name="largestSeats">The largest seat count of those workspaces, the lowest seat limit allowed.</param>
    public IReadOnlyList<(string Field, string Message)> Validate(SaasPlan current, IReadOnlyList<SaasPlan> plans, int workspaceCount, int largestSeats)
    {
        var errors = new List<(string Field, string Message)>();
        var name = (Name ?? string.Empty).Trim();
        var description = (Description ?? string.Empty).Trim();
        var callToAction = (CallToAction ?? string.Empty).Trim();

        if (name.Length < MinNameLength || name.Length > MaxNameLength)
            errors.Add((nameof(Name), $"Enter a name of {MinNameLength} to {MaxNameLength} characters."));

        if (description.Length == 0 || description.Length > MaxDescriptionLength)
            errors.Add((nameof(Description), $"Enter a description of up to {MaxDescriptionLength} characters."));

        if (current.MonthlyPricePerSeat == 0)
        {
            if (!TryGetPrice(out var free) || free != 0)
                errors.Add((nameof(Price), $"{current.Name} is a free plan and stays free, so its price stays $0."));
        }
        else if (!TryGetPrice(out var price))
            errors.Add((nameof(Price), "Enter the price per seat a month in dollars and cents, such as 49 or 49.50."));
        else if (price != Math.Round(price, 2))
            errors.Add((nameof(Price), "Use dollars and cents: at most two digits after the decimal point."));
        else if (price < SaasState.MinPaidSeatPrice)
            errors.Add((nameof(Price), $"{current.Name} is a paid plan and stays paid: enter a price from {Money(SaasState.MinPaidSeatPrice)} to {Money(MaxPrice)}, so annual billing never rounds to $0."));
        else if (price > MaxPrice)
            errors.Add((nameof(Price), $"Enter a price from {Money(SaasState.MinPaidSeatPrice)} to {Money(MaxPrice)} per seat a month."));

        var lines = HighlightLines();
        var tooLong = lines.ToList().FindIndex(line => line.Length > MaxHighlightLength);

        if (lines.Count == 0)
            errors.Add((nameof(Highlights), "Enter at least one highlight, one per line."));
        else if (lines.Count > MaxHighlights)
            errors.Add((nameof(Highlights), $"Use up to {MaxHighlights} highlights; there are {lines.Count}."));
        else if (tooLong >= 0)
            errors.Add((nameof(Highlights), $"Highlight {tooLong + 1} has {lines[tooLong].Length} characters. Keep each line to {MaxHighlightLength} or fewer."));

        if (callToAction.Length == 0 || callToAction.Length > MaxCallToActionLength)
            errors.Add((nameof(CallToAction), $"Enter the button text, up to {MaxCallToActionLength} characters."));

        if (!SeatsUnlimited)
        {
            if (!TryParseWhole(SeatLimit, out var seats) || seats < 1 || seats > SaasSeedData.MaxSeats)
                errors.Add((nameof(SeatLimit), $"Enter a seat limit from 1 to {Count(SaasSeedData.MaxSeats)}, or tick Unlimited."));
            else if (seats < largestSeats)
                errors.Add((nameof(SeatLimit), SeatLimitMessage(current.Name, workspaceCount, largestSeats)));
        }

        if (!EventsUnlimited && (!TryParseWhole(EventLimit, out var events) || events < MinEvents || events > MaxEvents))
            errors.Add((nameof(EventLimit), $"Enter an event limit from {Count(MinEvents)} to {Count(MaxEvents)} a month, or tick Unlimited."));

        if (!DashboardsUnlimited && (!TryParseWhole(DashboardLimit, out var dashboards) || dashboards < 1 || dashboards > MaxDashboards))
            errors.Add((nameof(DashboardLimit), $"Enter a dashboard limit from 1 to {Count(MaxDashboards)}, or tick Unlimited."));

        if (!IsSelfServe && !plans.Any(plan => plan.Id != current.Id && plan.IsSelfServe))
            errors.Add((nameof(IsSelfServe), "At least one plan must stay self-serve, so visitors can still sign up. Make another plan self-serve first."));

        // Cross-field: the "Most popular" badge sits on a plan visitors can start themselves.
        if (IsPopular && !IsSelfServe)
            errors.Add((nameof(IsPopular), "Only a self-serve plan can be Most popular: the badge points visitors to a plan they can start themselves. Turn on Self-serve, or turn this off."));

        return errors;
    }

    #endregion

    #region Private Methods

    private static string Count(long value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string Money(decimal value) => value.ToString("$#,0", CultureInfo.InvariantCulture);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the button text on the pricing page.</summary>
    public string CallToAction { get; set; } = string.Empty;

    /// <summary>Gets or sets the dashboard limit as typed.</summary>
    public string DashboardLimit { get; set; } = string.Empty;

    /// <summary>Gets or sets whether dashboards are unlimited.</summary>
    public bool DashboardsUnlimited { get; set; }

    /// <summary>Gets or sets the plan's description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the tracked-events limit a month as typed.</summary>
    public string EventLimit { get; set; } = string.Empty;

    /// <summary>Gets or sets whether tracked events are unlimited.</summary>
    public bool EventsUnlimited { get; set; }

    /// <summary>Gets or sets the highlights, one per line.</summary>
    public string Highlights { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID of the plan being edited (plans can't be added or removed).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the plan is marked "Most popular".</summary>
    public bool IsPopular { get; set; }

    /// <summary>Gets or sets whether visitors sign up for the plan themselves (otherwise it's sold through sales).</summary>
    public bool IsSelfServe { get; set; }

    /// <summary>Gets or sets the plan's name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the monthly price per seat as typed, in dollars and cents.</summary>
    public string Price { get; set; } = string.Empty;

    /// <summary>Gets or sets the seat limit as typed.</summary>
    public string SeatLimit { get; set; } = string.Empty;

    /// <summary>Gets or sets whether seats are unlimited.</summary>
    public bool SeatsUnlimited { get; set; }

    #endregion
}
