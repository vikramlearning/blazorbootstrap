using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Usage analytics of the Parley workspace admin back office: KPI cards (active members and pending invitations, this
/// month's messages and tokens, members at or over 80 % of the quota), workspace tokens per day for the last 30 days
/// stacked by model, this month's tokens by model, and the members who used the most this month with their share of the
/// quota. Figures come from <see cref="AiState.Usage" /> (generated history plus this tab's messages), so members without a
/// demo account appear too. Each chart has a text summary and a data table; charts use <see cref="AiChartTheme" /> and
/// redraw when the theme or the state changes.
/// </summary>
public partial class AdminOverview : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many members the top-members table lists.</summary>
    private const int TopCount = 10;

    private bool chartsReady;

    private BarChart dailyChart = default!;

    private DoughnutChart modelsChart = default!;

    /// <summary>The usage points of this render; cleared when the state changes.</summary>
    private IReadOnlyList<AiUsagePoint>? usage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var initializedDark = State.IsDarkTheme;

            await dailyChart.InitializeAsync(DailyData(), DailyOptions());
            await modelsChart.InitializeAsync(ModelsData(), ModelsOptions());
            chartsReady = true;

            // The theme switcher can report the color mode while the charts are still initializing; catch up once ready.
            if (State.IsDarkTheme != initializedDark)
                await RedrawChartsAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        State.ThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and theme changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        State.ThemeChanged -= OnThemeChanged;
    }

    #endregion

    #region Private Methods

    private static string Count(long value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string DayText(DateTime day) => day.ToString("MMM d", CultureInfo.InvariantCulture);

    private static string Percent(long part, long whole) => whole <= 0 ? "0%" : (part * 100.0 / whole).ToString("0", CultureInfo.InvariantCulture) + "%";

    private static ProgressColor QuotaColor(double percent) => percent >= 100 ? ProgressColor.Danger : percent >= 80 ? ProgressColor.Warning : ProgressColor.Primary;

    private static string QuotaText(double percent) => percent.ToString("0", CultureInfo.InvariantCulture) + "% of quota" + (percent >= 100 ? ", used up" : percent >= 80 ? ", nearly used" : null);

    private static double SharePercent(long tokens, int quota) => quota <= 0 ? 0 : tokens * 100.0 / quota;

    private ChartData DailyData()
    {
        var colors = AiChartTheme.Series(State.IsDarkTheme);
        var days = DailyTotals;
        var datasets = State.Models.Select((model, index) =>
        {
            var color = colors[index % colors.Count];

            return (IChartDataset)new BarChartDataset
            {
                Label = model.Name,
                Data = days.Select(day => (double?)day.ByModel.GetValueOrDefault(model.Id)).ToList(),
                BackgroundColor = new List<string> { color },
                BorderColor = new List<string> { color },
                BorderWidth = new List<double> { 0 },
                MaxBarThickness = 22,
            };
        }).ToList();

        return new ChartData { Labels = days.Select(day => DayText(day.Date)).ToList(), Datasets = datasets };
    }

    private BarChartOptions DailyOptions()
    {
        var options = new BarChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        AiChartTheme.Apply(options, State.IsDarkTheme);
        options.Scales.X!.Stacked = true;
        options.Scales.Y!.Stacked = true;

        return options;
    }

    private string DailySummary(IReadOnlyList<DayTotal> days)
    {
        var total = days.Sum(day => day.Total);

        if (total == 0)
            return "No tokens were used in the last 30 days.";

        var busiest = days.MaxBy(day => day.Total)!;
        var activeDays = days.Count(day => day.Total > 0);
        var topModel = State.Models.MaxBy(model => days.Sum(day => day.ByModel.GetValueOrDefault(model.Id)))!;
        var topTokens = days.Sum(day => day.ByModel.GetValueOrDefault(topModel.Id));

        return $"{AiDisplay.Tokens(total)} tokens in the last 30 days, on {activeDays} days with use (weekends are quiet). "
            + $"The busiest day was {busiest.Date.ToString("dddd, MMM d", CultureInfo.InvariantCulture)} with {AiDisplay.Tokens(busiest.Total)}. "
            + $"{topModel.Name} wrote the most: {Percent(topTokens, total)} of the tokens.";
    }

    private string ModelSummary(IReadOnlyList<ModelTotal> models)
    {
        var total = models.Sum(item => item.Tokens);

        return total == 0
            ? "No tokens were used this month yet."
            : $"{AiDisplay.Tokens(total)} tokens this month: " + string.Join(", ", models.Where(item => item.Tokens > 0).Select(item => $"{item.Model.Name} {Percent(item.Tokens, total)}")) + ".";
    }

    private ChartData ModelsData()
    {
        var isDark = State.IsDarkTheme;
        var colors = AiChartTheme.Series(isDark);
        var models = State.Models.ToList();
        var totals = ModelTotals;

        return new ChartData
        {
            Labels = models.Select(model => model.Name).ToList(),
            Datasets = new List<IChartDataset>
            {
                new DoughnutChartDataset
                {
                    Label = "Tokens",
                    Data = models.Select(model => (double?)(totals.FirstOrDefault(item => item.Model.Id == model.Id)?.Tokens ?? 0)).ToList(),
                    BackgroundColor = models.Select((_, index) => colors[index % colors.Count]).ToList(),
                    BorderColor = models.Select(_ => AiChartTheme.Surface(isDark)).ToList(),
                    BorderWidth = models.Select(_ => 3d).ToList(),
                    HoverOffset = models.Select(_ => 6d).ToList(),
                },
            },
        };
    }

    private DoughnutChartOptions ModelsOptions()
    {
        var options = new DoughnutChartOptions { Responsive = true, MaintainAspectRatio = false };
        AiChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        usage = null;
        StateHasChanged();
        await RedrawChartsAsync();
    });

    private void OnThemeChanged() => InvokeAsync(RedrawChartsAsync);

    private async Task RedrawChartsAsync()
    {
        if (!chartsReady)
            return;

        await dailyChart.UpdateAsync(DailyData(), DailyOptions());
        await modelsChart.UpdateAsync(ModelsData(), ModelsOptions());
    }

    private IEnumerable<Total> Totals(IReadOnlyList<MemberRow> rows, int quota)
    {
        var members = State.Members;
        var joined = members.Count(member => member.Status != AiMemberStatus.Invited);
        var active = members.Count(member => member.Status == AiMemberStatus.Active);
        var invited = members.Count(member => member.Status == AiMemberStatus.Invited);
        var near = rows.Count(row => row.Member.Status == AiMemberStatus.Active && SharePercent(row.Tokens, quota) >= 80);
        var over = rows.Count(row => row.Member.Status == AiMemberStatus.Active && SharePercent(row.Tokens, quota) >= 100);

        return new[]
        {
            new Total("Active members", Count(active), $"of {AdminMembers.Plural(joined, "member")} · {AdminMembers.Plural(invited, "invitation")} pending"),
            new Total("Messages this month", Count(rows.Sum(row => (long)row.Messages)), "Sent by members to the assistant"),
            new Total("Tokens this month", AiDisplay.Tokens(rows.Sum(row => row.Tokens)), "Messages and replies, every member"),
            new Total("At 80 % of quota or more", Count(near), near == 0 ? "No active member is close to the quota" : over > 0 ? $"{Count(over)} used it up" : "None used it up yet"),
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the workspace's tokens per day and model for the last 30 days (today included), oldest first.</summary>
    private IReadOnlyList<DayTotal> DailyTotals
    {
        get
        {
            var today = State.Clock.Now.Date;
            var points = Usage.Where(point => point.Date.Date > today.AddDays(-30) && point.Date.Date <= today).ToList();

            return Enumerable.Range(0, 30).Select(offset =>
            {
                var day = today.AddDays(offset - 29);
                var byModel = points.Where(point => point.Date.Date == day).GroupBy(point => point.ModelId).ToDictionary(group => group.Key, group => group.Sum(point => (long)point.Tokens));

                return new DayTotal(day, byModel, byModel.Values.Sum());
            }).ToList();
        }
    }

    /// <summary>Gets every member who joined (not invitations), with this month's messages and tokens, the most tokens first.</summary>
    private IReadOnlyList<MemberRow> MemberRows
    {
        get
        {
            var month = MonthPoints.ToLookup(point => point.MemberKey);

            return State.Members.Where(member => member.Status != AiMemberStatus.Invited)
                .Select(member => new MemberRow(member, month[member.Email].Sum(point => point.Messages), month[member.Email].Sum(point => (long)point.Tokens)))
                .OrderByDescending(row => row.Tokens)
                .ThenBy(row => row.Member.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    /// <summary>Gets the other admin modules, for the links at the bottom.</summary>
    private static IReadOnlyList<Module> Modules { get; } = new[]
    {
        new Module($"{AiAdminLayout.Base}/members", IconName.People, "Members and roles", "Invite people, change roles, and deactivate or reactivate members."),
        new Module($"{AiAdminLayout.Base}/prompts", IconName.JournalText, "Shared prompts", "The prompts every member sees in their prompt library."),
        new Module($"{AiAdminLayout.Base}/settings", IconName.Sliders, "Models and policy", "Models, the monthly quota, attachments, retention, and blocked terms."),
    };

    /// <summary>Gets this calendar month's tokens by model, every model of the catalog, the most first.</summary>
    private IReadOnlyList<ModelTotal> ModelTotals
    {
        get
        {
            var month = MonthPoints.ToList();

            return State.Models.Select(model => new ModelTotal(model, month.Where(point => point.ModelId == model.Id).Sum(point => (long)point.Tokens)))
                .OrderByDescending(item => item.Tokens)
                .ToList();
        }
    }

    private IEnumerable<AiUsagePoint> MonthPoints => Usage.Where(point => point.Date.Year == State.Clock.Now.Year && point.Date.Month == State.Clock.Now.Month && point.Date <= State.Clock.Now);

    [Inject] private AiState State { get; set; } = default!;

    private IReadOnlyList<AiUsagePoint> Usage => usage ??= State.Usage();

    #endregion

    #region Nested Types

    /// <summary>The workspace's tokens on one day, by model ID.</summary>
    private sealed record DayTotal(DateTime Date, IReadOnlyDictionary<string, long> ByModel, long Total);

    /// <summary>One member's use this month.</summary>
    private sealed record MemberRow(AiMember Member, int Messages, long Tokens);

    /// <summary>A link to another admin module.</summary>
    private sealed record Module(string Href, IconName Icon, string Title, string Description);

    /// <summary>One model's tokens this month.</summary>
    private sealed record ModelTotal(AiModel Model, long Tokens);

    /// <summary>A KPI card.</summary>
    private sealed record Total(string Label, string Value, string Note);

    #endregion
}
