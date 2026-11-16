using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The host's earnings, for all listings or one: what was paid out this month and in the last 6 months, the upcoming
/// payouts, and the average per stay; a stacked bar chart of payouts per month, paid and upcoming, for the last 6 months
/// and the next 3 (with a summary label and a visually hidden table); the payouts table (pay-out date, reservation, guest,
/// listing, dates, amount, and Paid or Upcoming in words); and how a payout is worked out. The figures come from
/// <see cref="TravelState.HostPayouts" />; the page rebuilds them when the listing changes and whenever the state changes.
/// </summary>
public partial class HostEarnings : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The months shown after the current one.</summary>
    private const int MonthsAhead = 3;

    /// <summary>The months shown up to and including the current one.</summary>
    private const int MonthsBack = 6;

    private bool chartReady;

    private List<MonthTotal> months = new();

    private BarChart payoutChart = default!;

    /// <summary>The payouts of the listing chosen (or every listing), the latest pay-out date first.</summary>
    private List<TravelPayout> payouts = new();

    /// <summary>The listing the figures are limited to, as an ID, or an empty string for every listing.</summary>
    private string stayFilter = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await payoutChart.InitializeAsync(ChartData(), ChartOptions());
            chartReady = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        Rebuild();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private ChartData ChartData() => new()
    {
        Labels = months.Select(month => MonthText(month.Start)).ToList(),
        Datasets = new List<IChartDataset>
        {
            TravelChartTheme.Bars("Paid", months.Select(month => (double)month.Paid), TravelChartTheme.Paid, "payouts"),
            TravelChartTheme.Bars("Upcoming", months.Select(month => (double)month.Upcoming), TravelChartTheme.Upcoming, "payouts"),
        },
    };

    private static BarChartOptions ChartOptions() => TravelChartTheme.MonthlyBarOptions("Payouts (USD)", stacked: true);

    /// <summary>A month like "Nov 2026".</summary>
    private static string MonthText(DateOnly month) => month.ToString("MMM yyyy", CultureInfo.InvariantCulture);

    private async Task OnListingChangedAsync(ChangeEventArgs args)
    {
        stayFilter = args.Value?.ToString() ?? string.Empty;

        Rebuild();
        await RedrawChartAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        Rebuild();
        StateHasChanged();
        await RedrawChartAsync();
    });

    private static string PayoutCount(int count) => count == 1 ? "1 payout" : $"{count.ToString("N0", CultureInfo.InvariantCulture)} payouts";

    /// <summary>Works the figures out again; a listing deleted in the meantime falls back to every listing.</summary>
    private void Rebuild()
    {
        if (stayFilter.Length > 0 && State.HostStays().All(stay => stay.Id.ToString(CultureInfo.InvariantCulture) != stayFilter))
            stayFilter = string.Empty;

        payouts = State.HostPayouts()
            .Where(payout => stayFilter.Length == 0 || payout.StayId.ToString(CultureInfo.InvariantCulture) == stayFilter)
            .ToList();

        months = Enumerable.Range(0, MonthsBack + MonthsAhead)
            .Select(index => FirstMonth.AddMonths(index))
            .Select(start =>
            {
                var inMonth = payouts.Where(payout => payout.PaysOn >= start && payout.PaysOn < start.AddMonths(1)).ToList();

                return new MonthTotal(start, inMonth.Where(payout => payout.IsPaid).Sum(payout => payout.Amount), inMonth.Where(payout => !payout.IsPaid).Sum(payout => payout.Amount));
            })
            .ToList();
    }

    private async Task RedrawChartAsync()
    {
        if (!chartReady)
            return;

        await payoutChart.UpdateAsync(ChartData(), ChartOptions());
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string ChartLabel
    {
        get
        {
            if (ChartTotal == 0)
                return $"Stacked bar chart of payouts per month, {RangeText}: no payouts.";

            var largest = months.OrderByDescending(month => month.Paid + month.Upcoming).ThenBy(month => month.Start).First();

            return $"Stacked bar chart of payouts per month, {RangeText}: {TravelDisplay.Money(months.Sum(month => month.Paid))} paid and "
                   + $"{TravelDisplay.Money(months.Sum(month => month.Upcoming))} upcoming; the largest month is {MonthText(largest.Start)}, "
                   + $"{TravelDisplay.Money(largest.Paid + largest.Upcoming)}.";
        }
    }

    private decimal ChartTotal => months.Sum(month => month.Paid + month.Upcoming);

    /// <summary>The host fee, like "3%".</summary>
    private static string FeeText => FormattableString.Invariant($"{TravelState.HostFeeRate * 100:0.#}%");

    /// <summary>The first day of the earliest month in the chart and the "last 6 months" figure.</summary>
    private DateOnly FirstMonth => ThisMonth.AddMonths(1 - MonthsBack);

    /// <summary>The number of upcoming payouts after the last month in the chart.</summary>
    private int LaterCount => payouts.Count(payout => payout.PaysOn >= ThisMonth.AddMonths(MonthsAhead + 1));

    private string ListingText => stayFilter.Length == 0
        ? "all listings"
        : State.HostStays().FirstOrDefault(stay => stay.Id.ToString(CultureInfo.InvariantCulture) == stayFilter)?.Name ?? "all listings";

    private List<TravelPayout> PaidSixMonths => payouts.Where(payout => payout.IsPaid && payout.PaysOn >= FirstMonth).ToList();

    private List<TravelPayout> PaidThisMonth => payouts.Where(payout => payout.IsPaid && payout.PaysOn >= ThisMonth && payout.PaysOn < ThisMonth.AddMonths(1)).ToList();

    private string RangeText => months.Count == 0 ? string.Empty : $"{MonthText(months[0].Start)} to {MonthText(months[^1].Start)}";

    /// <summary>What the figures cover, such as "All listings · 9 payouts".</summary>
    private string ScopeText
    {
        get
        {
            var listing = ListingText;

            return $"{char.ToUpperInvariant(listing[0])}{listing[1..]} · {PayoutCount(payouts.Count)}";
        }
    }

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>The first day of the current month.</summary>
    private DateOnly ThisMonth => new(State.Today.Year, State.Today.Month, 1);

    private List<TravelPayout> Upcoming => payouts.Where(payout => !payout.IsPaid).ToList();

    private string UpcomingNote
    {
        get
        {
            var upcoming = Upcoming;

            return upcoming.Count == 0
                ? "Nothing is waiting to be paid out."
                : $"{PayoutCount(upcoming.Count)}; the next pays on {TravelDisplay.Day(upcoming.Min(payout => payout.PaysOn))}";
        }
    }

    #endregion

    #region Nested Types

    /// <summary>The payouts of one month: paid, and still to be paid.</summary>
    private sealed record MonthTotal(DateOnly Start, decimal Paid, decimal Upcoming);

    #endregion
}
