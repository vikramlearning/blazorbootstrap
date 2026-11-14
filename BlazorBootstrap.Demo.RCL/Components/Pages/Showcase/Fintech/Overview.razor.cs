using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Overview: net worth and account balances, the net worth trend and this month's spending charts, recent activity,
/// and the budgets closest to their limit.
/// </summary>
public partial class Overview : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool chartsReady;

    private LineChart netWorthChart = default!;

    private DoughnutChart spendingChart = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var initializedDark = State.IsDarkTheme;

            await netWorthChart.InitializeAsync(NetWorthData(), NetWorthOptions());
            await spendingChart.InitializeAsync(SpendingData(), SpendingOptions());
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

    private static IconName AccountIcon(FintechAccountKind kind) => kind switch
    {
        FintechAccountKind.Savings => IconName.PiggyBank,
        FintechAccountKind.CreditCard => IconName.CreditCard,
        _ => IconName.Wallet2,
    };

    private static ProgressColor BudgetColor(double percent) => percent switch
    {
        > 100 => ProgressColor.Danger,
        >= 80 => ProgressColor.Warning,
        _ => ProgressColor.Success,
    };

    private ChartData NetWorthData()
    {
        var isDark = State.IsDarkTheme;
        var line = new LineChartDataset
        {
            Label = "Net worth",
            Data = State.NetWorthHistory.Select(value => (double?)value).ToList(),
            BorderColor = FintechChartTheme.Primary(isDark),
            BackgroundColor = FintechChartTheme.Primary(isDark, .14),
            BorderWidth = 3,
            Tension = .35,
            PointRadius = new List<double> { 0 },
            PointHoverRadius = new List<double> { 5 },
            PointBackgroundColor = new List<string> { FintechChartTheme.Primary(isDark) },
        };
        line.FillToStart();

        return new ChartData { Labels = FintechSeedData.NetWorthMonths().ToList(), Datasets = new List<IChartDataset> { line } };
    }

    private LineChartOptions NetWorthOptions()
    {
        var options = new LineChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        FintechChartTheme.Apply(options, State.IsDarkTheme);
        options.Plugins.Legend.Display = false;

        return options;
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        StateHasChanged();
        await RedrawChartsAsync();
    });

    private void OnThemeChanged() => InvokeAsync(RedrawChartsAsync);

    private async Task RedrawChartsAsync()
    {
        if (!chartsReady)
            return;

        await netWorthChart.UpdateAsync(NetWorthData(), NetWorthOptions());
        await spendingChart.UpdateAsync(SpendingData(), SpendingOptions());
    }

    private ChartData SpendingData()
    {
        var isDark = State.IsDarkTheme;
        var colors = FintechChartTheme.Series(isDark);
        var spending = Spending;

        return new ChartData
        {
            Labels = spending.Select(entry => entry.Category.Name).ToList(),
            Datasets = new List<IChartDataset>
            {
                new DoughnutChartDataset
                {
                    Label = "Spent",
                    Data = spending.Select(entry => (double?)entry.Spent).ToList(),
                    BackgroundColor = spending.Select((_, index) => colors[index % colors.Count]).ToList(),
                    BorderColor = spending.Select(_ => FintechChartTheme.Surface(isDark)).ToList(),
                    BorderWidth = spending.Select(_ => 3d).ToList(),
                    HoverOffset = spending.Select(_ => 6d).ToList(),
                },
            },
        };
    }

    private DoughnutChartOptions SpendingOptions()
    {
        var options = new DoughnutChartOptions { Responsive = true, MaintainAspectRatio = false };
        FintechChartTheme.Apply(options, State.IsDarkTheme);

        return options;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private IEnumerable<FintechBudget> BudgetsToWatch => State.Budgets
        .OrderByDescending(budget => State.SpentThisMonth(budget.CategoryId) / budget.Limit)
        .Take(4);

    private decimal MonthChange
    {
        get
        {
            var history = State.NetWorthHistory;

            return (decimal)(history[^1] - history[^2]);
        }
    }

    private string NetWorthChartLabel
    {
        get
        {
            var history = State.NetWorthHistory;

            return $"Line chart of net worth over 12 months, from {FintechDisplay.Money((decimal)history[0])} to {FintechDisplay.Money((decimal)history[^1])}.";
        }
    }

    private IReadOnlyList<(FintechCategory Category, decimal Spent)> Spending => State.Categories
        .Where(category => category.IsSpending)
        .Select(category => (category, State.SpentThisMonth(category.Id)))
        .Where(entry => entry.Item2 > 0)
        .OrderByDescending(entry => entry.Item2)
        .ToList();

    private string SpendingChartLabel => "Spending this month: " + string.Join(", ", Spending.Select(entry => $"{entry.Category.Name} {FintechDisplay.Money(entry.Spent)}"));

    [Inject] private FintechState State { get; set; } = default!;

    private decimal TotalSpent => Spending.Sum(entry => entry.Spent);

    #endregion
}
