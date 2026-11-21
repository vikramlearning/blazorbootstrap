using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The signed-in member's usage (<see cref="AiState.Usage" />): this month's tokens against their monthly spend cap (their
/// own, or the default; <see cref="AiState.CurrentSpend" />) as a progress bar and in words, with a warning in words from 80 % and when
/// over; messages sent this month, conversations, and the most used model; a bar chart of tokens per day over the last 30
/// days; and tokens by model this month. Each chart has a summary in words and a table of its data. The figures are worked
/// out on every render, so the page follows the cap when an admin changes it and the member's messages from the chat. The
/// workspace's own cap is said too when it is at 80 % or more.
/// </summary>
public partial class Usage : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>From this share of the quota (in percent), the page warns in words.</summary>
    private const double WarningPercent = 80;

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

    private static string Count(long value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string Percent(long part, long whole) =>
        whole <= 0 ? "0%" : (part * 100d / whole).ToString("0", CultureInfo.InvariantCulture) + "%";

    private static ProgressColor ProgressColorFor(QuotaLevel level) => level switch
    {
        QuotaLevel.Over => ProgressColor.Danger,
        QuotaLevel.Warning => ProgressColor.Warning,
        _ => ProgressColor.None,
    };

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Works out every figure of the page for the signed-in member, now.</summary>
    private UsageReport Report()
    {
        var now = State.Clock.Now;
        var today = now.Date;
        var points = State.Usage(State.CurrentMember?.Email ?? string.Empty);
        var monthPoints = points.Where(point => point.Date.Year == now.Year && point.Date.Month == now.Month).ToList();
        var quota = (int)Math.Min(int.MaxValue, State.CurrentSpend?.Cap ?? State.Policy.MonthlyTokenQuota);
        var monthTokens = (long)monthPoints.Sum(point => (long)point.Tokens);
        var percent = quota <= 0 ? 0 : monthTokens * 100d / quota;
        var level = monthTokens >= quota ? QuotaLevel.Over : percent >= WarningPercent ? QuotaLevel.Warning : QuotaLevel.Normal;
        var resetText = new DateTime(now.Year, now.Month, 1).AddMonths(1).ToString("MMMM d", CultureInfo.InvariantCulture);
        var percentText = percent.ToString(percent < 10 ? "0.#" : "0", CultureInfo.InvariantCulture) + "%";

        var quotaText = monthTokens >= quota
            ? $"You've used {Count(monthTokens)} tokens of your {Count(quota)}-token monthly cap ({percentText}): chat is paused. It starts again on {resetText}."
            : $"You've used {Count(monthTokens)} of {Count(quota)} tokens ({percentText}). {Count(quota - monthTokens)} are left until your cap starts again on {resetText}.";

        var days = Enumerable.Range(0, 30)
            .Select(offset => today.AddDays(offset - 29))
            .Select(date =>
            {
                var onDay = points.Where(point => point.Date.Date == date).ToList();

                return new UsageDay(date, onDay.Sum(point => point.Messages), onDay.Sum(point => (long)point.Tokens));
            })
            .ToList();

        var models = monthPoints.GroupBy(point => point.ModelId)
            .Select(group => new UsageModel(group.Key, State.FindModel(group.Key)?.Name ?? group.Key, State.Policy.EnabledModelIds.Contains(group.Key),
                group.Sum(point => point.Messages), group.Sum(point => (long)point.Tokens)))
            .OrderByDescending(model => model.Tokens)
            .ThenBy(model => model.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new UsageReport
        {
            Conversations = State.Conversations.Count,
            DailySummary = DailySummary(days),
            Days = days,
            DayLabels = days.Select(day => day.Date.ToString("MMM d", CultureInfo.InvariantCulture)).ToList(),
            DayTokens = days.Select(day => (double)day.Tokens).ToList(),
            Level = level,
            ModelSummary = ModelSummary(models, monthTokens, now),
            Models = models,
            MonthMessages = monthPoints.Sum(point => point.Messages),
            MonthName = now.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
            MonthTokens = monthTokens,
            Percent = percent,
            PercentText = percentText,
            Quota = quota,
            QuotaText = quotaText,
            ResetText = resetText,
            TopModel = models.FirstOrDefault()?.Name,
        };
    }

    private static string DailySummary(IReadOnlyList<UsageDay> days)
    {
        var active = days.Where(day => day.Tokens > 0).ToList();

        if (active.Count == 0)
            return "No tokens used in the last 30 days.";

        var busiest = active.OrderByDescending(day => day.Tokens).First();
        var total = active.Sum(day => day.Tokens);

        return $"{Count(total)} tokens over the last 30 days, on {active.Count} of them. "
            + $"The busiest day was {busiest.Date.ToString("dddd, MMMM d", CultureInfo.InvariantCulture)}, with {Count(busiest.Tokens)} tokens; "
            + $"an active day averaged {Count(total / active.Count)}.";
    }

    private static string ModelSummary(IReadOnlyList<UsageModel> models, long monthTokens, DateTime now)
    {
        var month = now.ToString("MMMM", CultureInfo.InvariantCulture);

        return models.Count == 0
            ? $"You haven't used any model in {month} yet."
            : $"In {month}: " + string.Join(", ", models.Select(model => $"{model.Name} {Count(model.Tokens)} tokens ({Percent(model.Tokens, monthTokens)})")) + ".";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private AiState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>How close this month's tokens are to the quota.</summary>
    private enum QuotaLevel
    {
        Normal,
        Warning,
        Over,
    }

    /// <summary>The member's tokens and messages on one day.</summary>
    private sealed record UsageDay(DateTime Date, int Messages, long Tokens);

    /// <summary>The member's tokens and messages with one model this month.</summary>
    private sealed record UsageModel(string Id, string Name, bool IsEnabled, int Messages, long Tokens);

    /// <summary>Every figure the page shows, worked out once per render.</summary>
    private sealed class UsageReport
    {
        #region Properties, Indexers

        public int Conversations { get; init; }

        public string DailySummary { get; init; } = string.Empty;

        public IReadOnlyList<string> DayLabels { get; init; } = Array.Empty<string>();

        public IReadOnlyList<UsageDay> Days { get; init; } = Array.Empty<UsageDay>();

        public IReadOnlyList<double> DayTokens { get; init; } = Array.Empty<double>();

        public QuotaLevel Level { get; init; }

        public IReadOnlyList<UsageModel> Models { get; init; } = Array.Empty<UsageModel>();

        public string ModelSummary { get; init; } = string.Empty;

        public int MonthMessages { get; init; }

        public string MonthName { get; init; } = string.Empty;

        public long MonthTokens { get; init; }

        public double Percent { get; init; }

        public string PercentText { get; init; } = string.Empty;

        public int Quota { get; init; }

        public string QuotaText { get; init; } = string.Empty;

        public string ResetText { get; init; } = string.Empty;

        public string? TopModel { get; init; }

        #endregion
    }

    #endregion
}
