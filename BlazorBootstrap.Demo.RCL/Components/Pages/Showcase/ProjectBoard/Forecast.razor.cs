using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The sales forecast of Tandem (account managers): key figures (open pipeline, weighted pipeline, won this month against
/// <see cref="PbSeedData.MonthlyTarget" /> with a progress bar and words, and the win rate, won of won and lost); a bar
/// chart (<see cref="ForecastChart" />) of the last three months and the next four with won revenue by the month it was
/// won, the weighted open pipeline by expected close month (deals past their close date count in this month), and the
/// target as muted bars, with a text summary and a data table in <c>&lt;details&gt;</c>; the open deals expected to close
/// in the next 90 days, soonest first; and a summary by stage.
/// </summary>
public partial class Forecast : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many days ahead the closing table looks.</summary>
    private const int HorizonDays = 90;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string MonthLabel(DateTime start) => start.ToString("MMM yyyy", CultureInfo.InvariantCulture);

    private ForecastReport BuildReport()
    {
        var today = State.Clock.Today;
        var thisMonth = new DateTime(today.Year, today.Month, 1);
        var deals = State.Deals;
        var open = deals.Where(DealDisplay.IsOpen).ToList();
        var won = deals.Where(deal => deal.Stage == PbDealStage.Won && deal.ClosedAt is not null).ToList();
        var lost = deals.Count(deal => deal.Stage == PbDealStage.Lost);

        static DateTime MonthOf(DateTime date) => new(date.Year, date.Month, 1);

        var months = Enumerable.Range(-2, 7).Select(offset =>
        {
            var start = thisMonth.AddMonths(offset);
            var wonIn = won.Where(deal => MonthOf(deal.ClosedAt!.Value) == start).ToList();
            var openIn = open.Where(deal => (deal.CloseDate < thisMonth ? thisMonth : MonthOf(deal.CloseDate)) == start).ToList();

            return new ForecastMonth(start, MonthLabel(start), wonIn.Sum(deal => (long)deal.Value), wonIn.Count, openIn.Sum(deal => (long)PbState.Weighted(deal)), openIn.Count, offset == 0);
        }).ToList();

        var wonThisMonth = months.Single(month => month.IsCurrent).Won;
        var percent = (int)Math.Round(wonThisMonth * 100d / PbSeedData.MonthlyTarget);
        var monthName = thisMonth.ToString("MMMM", CultureInfo.InvariantCulture);
        var gap = PbSeedData.MonthlyTarget - wonThisMonth;
        var weightedNow = months.Single(month => month.IsCurrent).Weighted;
        var targetWords = gap <= 0
            ? $"{DealDisplay.Percent(percent)} of the {PbDisplay.Money(PbSeedData.MonthlyTarget)} target: met, {PbDisplay.Money(-gap)} over."
            : $"{DealDisplay.Percent(percent)} of the {PbDisplay.Money(PbSeedData.MonthlyTarget)} target: {PbDisplay.Money(gap)} to go, with {PbDisplay.Money(weightedNow)} weighted pipeline due this month.";

        var past = months.Where(month => month.Start <= thisMonth).Select(month => $"{month.Label} {PbDisplay.Money(month.Won)}");
        var ahead = months.Where(month => month.Start >= thisMonth).Select(month => $"{month.Label} {PbDisplay.Money(month.Weighted)}");
        var best = months.Where(month => month.Start < thisMonth).OrderByDescending(month => month.Won).FirstOrDefault();
        var summary = $"Won: {string.Join(", ", past)}. Weighted pipeline: {string.Join(", ", ahead)}. The target is {PbDisplay.Money(PbSeedData.MonthlyTarget)} a month"
            + (best is { Won: > 0 } ? $"; the best recent month was {best.Label}." : ".");

        var horizonEnd = today.AddDays(HorizonDays);
        var closing = open.Where(deal => deal.CloseDate.Date <= horizonEnd).OrderBy(deal => deal.CloseDate).ThenBy(deal => deal.Title, StringComparer.OrdinalIgnoreCase).ToList();

        var stages = DealDisplay.Stages.Select(stage =>
        {
            var inStage = deals.Where(deal => deal.Stage == stage).ToList();

            return new ForecastStage(stage, inStage.Count, inStage.Sum(deal => (long)deal.Value), inStage.Sum(deal => (long)PbState.Weighted(deal)));
        }).ToList();

        var series = new List<ForecastChartSeries>
        {
            new("Won", months.Select(month => (double)month.Won).ToList(), 1),
            new("Weighted pipeline", months.Select(month => (double)month.Weighted).ToList(), 0),
            new("Target", months.Select(_ => (double)PbSeedData.MonthlyTarget).ToList(), Muted: true),
        };

        return new ForecastReport
        {
            ChartSeries = series,
            ClosedCount = won.Count + lost,
            Closing = closing,
            HorizonEnd = horizonEnd,
            LostCount = lost,
            MonthName = monthName,
            Months = months,
            MonthSummary = summary,
            OpenCount = open.Count,
            OpenValue = open.Sum(deal => (long)deal.Value),
            Stages = stages,
            TargetPercent = percent,
            TargetWords = targetWords,
            WeightedValue = open.Sum(deal => (long)PbState.Weighted(deal)),
            WinRate = won.Count + lost == 0 ? 0 : (int)Math.Round(won.Count * 100d / (won.Count + lost)),
            WonCount = won.Count,
            WonThisMonth = wonThisMonth,
        };
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    private ForecastReport Report => BuildReport();

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One month of the chart.</summary>
    private sealed record ForecastMonth(DateTime Start, string Label, long Won, int WonCount, long Weighted, int OpenCount, bool IsCurrent);

    /// <summary>One row of the by-stage summary.</summary>
    private sealed record ForecastStage(PbDealStage Stage, int Count, long Value, long Weighted);

    /// <summary>Everything the page shows, computed once per render.</summary>
    private sealed class ForecastReport
    {
        public IReadOnlyList<ForecastChartSeries> ChartSeries { get; init; } = Array.Empty<ForecastChartSeries>();

        public int ClosedCount { get; init; }

        public IReadOnlyList<PbDeal> Closing { get; init; } = Array.Empty<PbDeal>();

        public DateTime HorizonEnd { get; init; }

        public int LostCount { get; init; }

        public string MonthName { get; init; } = string.Empty;

        public IReadOnlyList<ForecastMonth> Months { get; init; } = Array.Empty<ForecastMonth>();

        public string MonthSummary { get; init; } = string.Empty;

        public int OpenCount { get; init; }

        public long OpenValue { get; init; }

        public IReadOnlyList<ForecastStage> Stages { get; init; } = Array.Empty<ForecastStage>();

        public int TargetPercent { get; init; }

        public string TargetWords { get; init; } = string.Empty;

        public long WeightedValue { get; init; }

        public int WinRate { get; init; }

        public int WonCount { get; init; }

        public long WonThisMonth { get; init; }
    }

    #endregion
}
