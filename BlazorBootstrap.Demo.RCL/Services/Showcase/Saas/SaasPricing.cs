using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

/// <summary>
/// Lumora's price arithmetic, shared by the pricing page, trial sign-up, billing, the operator console, and the seed, so a
/// price is computed one way everywhere.
/// </summary>
public static class SaasPricing
{
    #region Fields and Constants

    /// <summary>Annual billing discount (about two months free).</summary>
    public const decimal AnnualDiscount = 0.20m;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the price per seat per month for a billing period: the list price, or the annual price rounded to whole dollars.</summary>
    public static decimal MonthlySeatPrice(SaasPlan plan, SaasBillingPeriod period) => period == SaasBillingPeriod.Annual
        ? Math.Round(plan.MonthlyPricePerSeat * (1 - AnnualDiscount), 0, MidpointRounding.AwayFromZero)
        : plan.MonthlyPricePerSeat;

    /// <summary>Returns what a workspace pays per month on average, for revenue figures (MRR).</summary>
    public static decimal MonthlyAmount(SaasPlan plan, SaasBillingPeriod period, int seats) => MonthlySeatPrice(plan, period) * seats;

    /// <summary>Returns the amount of one billing period: a month, or a year for annual billing.</summary>
    public static decimal PeriodAmount(SaasPlan plan, SaasBillingPeriod period, int seats) =>
        MonthlyAmount(plan, period, seats) * (period == SaasBillingPeriod.Annual ? 12 : 1);

    /// <summary>Returns the length of a billing period that starts on a date.</summary>
    public static DateTime PeriodEnd(DateTime start, SaasBillingPeriod period) =>
        period == SaasBillingPeriod.Annual ? start.AddYears(1) : start.AddMonths(1);

    /// <summary>Returns the share of the current period still to come on a date (0 to 1), to prorate a change to the day.</summary>
    public static decimal RemainingShare(DateTime today, DateTime renewsOn, SaasBillingPeriod period)
    {
        var start = period == SaasBillingPeriod.Annual ? renewsOn.AddYears(-1) : renewsOn.AddMonths(-1);
        var total = (renewsOn.Date - start.Date).Days;
        var left = (renewsOn.Date - today.Date).Days;

        return total <= 0 ? 0m : Math.Clamp((decimal)left / total, 0m, 1m);
    }

    /// <summary>Rounds an amount to cents.</summary>
    public static decimal Cents(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    #endregion

    #endregion
}
