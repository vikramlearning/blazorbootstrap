using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorExpress.ChartJS;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's sales analytics: net revenue, orders, average order value, items sold, and refunds for the last 7, 14,
/// or 30 days, each compared with the period before; net revenue per day, revenue by category, and orders by status
/// charts, each with a text alternative; and the top products and coupon usage. Every figure is derived from
/// <see cref="EcommerceState.Orders" />, so the page redraws when an order is placed, cancelled, or refunded in this tab.
/// </summary>
/// <remarks>
/// Axis, legend, and grid colors follow the color mode the site bar reports (<see cref="EcommerceState.IsDarkTheme" />), and the
/// charts redraw when it changes. Series colors are mid-tones that keep 3:1 against both panels.
/// </remarks>
public partial class AdminAnalytics : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string FontFamily = "'DM Sans', system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif";

    /// <summary>Store orange, a mid-tone that keeps 3:1 against both the light and the dark panel.</summary>
    private const string SalesColor = "#ea580c";

    private const string SalesFill = "rgba(234, 88, 12, 0.14)";

    private static readonly (int Days, string Text)[] ranges = { (7, "Last 7 days"), (14, "Last 14 days"), (30, "Last 30 days") };

    /// <summary>Slice colors, mid-tones that keep 3:1 against both panels; the legend and the table name every status.</summary>
    private static readonly IReadOnlyDictionary<EcommerceOrderStatus, string> statusColors = new Dictionary<EcommerceOrderStatus, string>
    {
        [EcommerceOrderStatus.Placed] = "#d97706",
        [EcommerceOrderStatus.Packed] = "#0891b2",
        [EcommerceOrderStatus.Shipped] = "#2563eb",
        [EcommerceOrderStatus.Delivered] = "#16a34a",
        [EcommerceOrderStatus.Cancelled] = "#dc2626",
        [EcommerceOrderStatus.Refunded] = "#8b7d72",
    };

    private BarChart categoryChart = default!;

    private bool chartsReady;

    private int rangeDays = 30;

    private string rangeMessage = string.Empty;

    private Report report = default!;

    private LineChart revenueChart = default!;

    private DoughnutChart statusChart = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await revenueChart.InitializeAsync(RevenueData(), RevenueOptions());
            await categoryChart.InitializeAsync(CategoryData(), CategoryOptions());
            await statusChart.InitializeAsync(StatusData(), StatusOptions());
            chartsReady = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        report = BuildReport();
        State.Changed += OnStateChanged;
        State.ThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        State.ThemeChanged -= OnThemeChanged;
    }

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string DayLabel(DateTime day) => day.ToString("MMM d", CultureInfo.InvariantCulture);

    private static bool IsSale(EcommerceOrder order) => order.Status != EcommerceOrderStatus.Cancelled;

    private static decimal LineRevenue(EcommerceOrderLine line) => line.UnitPrice * line.Quantity;

    private static decimal NetOf(EcommerceOrder order) => order.Totals.Total - order.RefundedAmount;

    private static string StatusName(EcommerceOrderStatus status) => status.ToString();

    private static PeriodTotals Totals(IReadOnlyList<EcommerceOrder> orders)
    {
        var sales = orders.Where(order => IsSale(order)).ToList();

        return new PeriodTotals(
            sales.Count,
            sales.Sum(order => order.Totals.Total),
            sales.Sum(order => NetOf(order)),
            sales.Sum(order => order.Totals.ItemCount),
            sales.Sum(order => order.RefundedAmount),
            sales.Count(order => order.RefundedAmount > 0),
            orders.Count - sales.Count);
    }

    private void ApplyAxes(Scales scales, ChartAxes valueAxis, ChartAxes categoryAxis, string valueTitle)
    {
        foreach (var axis in new[] { scales.X!, scales.Y! })
        {
            axis.Ticks = new ChartAxesTicks { Color = AxisText };
            axis.Grid = new ChartAxesGrid { Color = GridLine, DrawTicks = false };
            axis.Border = new ChartAxesBorder { Display = false };
        }

        categoryAxis.Grid!.Display = false;
        valueAxis.BeginAtZero = true;
        valueAxis.Title = new ChartAxesTitle { Display = true, Text = valueTitle, Color = AxisText };
    }

    private void ApplyPlugins(ChartPlugins plugins, bool showLegend)
    {
        plugins.Legend.Display = showLegend;
        plugins.Legend.Position = "bottom";
        plugins.Legend.Align = "start";
        plugins.Legend.Labels = new ChartPluginsLegendLabels
        {
            Color = AxisText,
            Font = new ChartFont { Family = FontFamily, Size = 12, Weight = "500" },
            UsePointStyle = true,
            PointStyle = "circle",
            BoxWidth = 8,
            BoxHeight = 8,
            Padding = 14,
        };

        // A stone tooltip stands apart from both the white and the espresso panel.
        plugins.Tooltip = new ChartPluginsTooltip
        {
            Enabled = true,
            BackgroundColor = "#57534e",
            TitleColor = "#ffffff",
            BodyColor = "#f5f5f4",
            DisplayColors = true,
        };
    }

    private Report BuildReport()
    {
        var today = State.Clock.Today;
        var start = today.AddDays(1 - rangeDays);
        var end = today.AddDays(1);
        var previousStart = start.AddDays(-rangeDays);

        var inRange = State.Orders.Where(order => order.PlacedOn >= start && order.PlacedOn < end).ToList();
        var previous = State.Orders.Where(order => order.PlacedOn >= previousStart && order.PlacedOn < start).ToList();
        var sales = inRange.Where(order => IsSale(order)).ToList();
        var lines = sales.SelectMany(order => order.Lines).ToList();

        var daily = Enumerable.Range(0, rangeDays)
            .Select(offset => start.AddDays(offset))
            .Select(day => new DailyRevenue(day, sales.Where(order => order.PlacedOn.Date == day).Sum(order => NetOf(order))))
            .ToList();

        var byCategory = lines
            .GroupBy(line => CategoryName(line.ProductId))
            .ToDictionary(group => group.Key, group => group.Sum(line => LineRevenue(line)));
        var categories = State.Categories
            .Select(category => category.Name)
            .Union(byCategory.Keys)
            .Select(name => new CategoryRevenue(name, byCategory.GetValueOrDefault(name)))
            .OrderByDescending(row => row.Revenue)
            .ThenBy(row => row.Name, StringComparer.Ordinal)
            .ToList();

        var statuses = Enum.GetValues<EcommerceOrderStatus>()
            .Select(status => new StatusCount(status, inRange.Count(order => order.Status == status)))
            .Where(row => row.Orders > 0)
            .ToList();

        var topProducts = lines
            .GroupBy(line => line.ProductId)
            .Select(group => new ProductSales(
                group.Key,
                State.FindAnyProduct(group.Key)?.Name ?? group.First().Name,
                group.Sum(line => line.Quantity),
                group.Sum(line => LineRevenue(line))))
            .OrderByDescending(row => row.Units)
            .ThenByDescending(row => row.Revenue)
            .ThenBy(row => row.Name, StringComparer.Ordinal)
            .Take(10)
            .ToList();

        var coupons = sales
            .Where(order => !string.IsNullOrWhiteSpace(order.PromoCode))
            .GroupBy(order => order.PromoCode!.Trim().ToUpperInvariant())
            .Select(group => new CouponUsage(group.Key, group.Count(), group.Sum(order => order.Totals.Discount)))
            .OrderByDescending(row => row.Orders)
            .ThenBy(row => row.Code, StringComparer.Ordinal)
            .ToList();

        return new Report(start, today, Totals(inRange), Totals(previous), daily, categories, statuses, topProducts, coupons);
    }

    private ChartData CategoryData() => new()
    {
        Labels = report.Categories.Select(row => row.Name).ToList(),
        Datasets = new List<IChartDataset>
        {
            new BarChartDataset
            {
                Label = "Merchandise revenue",
                Data = report.Categories.Select(row => (double?)(double)row.Revenue).ToList(),
                BackgroundColor = new List<string> { SalesColor },
                BorderColor = new List<string> { SalesColor },
                BorderWidth = new List<double> { 0 },
                BorderRadius = new List<double> { 6 },
                MaxBarThickness = 28,
            },
        },
    };

    private string CategoryName(int productId) => State.FindCategory(State.FindAnyProduct(productId)?.CategoryId)?.Name ?? "Uncategorized";

    private BarChartOptions CategoryOptions()
    {
        var options = new BarChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            IndexAxis = "y",
            Locale = "en-US",
            Animation = new ChartAnimation { Duration = 0 },
        };

        ApplyPlugins(options.Plugins, showLegend: false);
        ApplyAxes(options.Scales, options.Scales.X!, options.Scales.Y!, "Revenue (USD)");

        return options;
    }

    /// <summary>Compares a figure with the previous period, in words: the change is never shown by color alone.</summary>
    private Change Compare(decimal? current, decimal? previous)
    {
        var period = $"previous {rangeDays} days";

        if (previous is not decimal before)
            return new Change($"— no sales in the {period}", IconName.Dash);

        if (current is not decimal now)
            return new Change("— no sales in this period", IconName.Dash);

        if (before == 0)
            return now == 0 ? new Change($"No change vs {period}", IconName.Dash) : new Change($"Up from none in the {period}", IconName.ArrowUpRight);

        var percent = Math.Round((now - before) / before * 100m, MidpointRounding.AwayFromZero);
        var text = Math.Abs(percent).ToString("#,0", CultureInfo.InvariantCulture);

        return percent switch
        {
            > 0m => new Change($"Up {text}% vs {period}", IconName.ArrowUpRight),
            < 0m => new Change($"Down {text}% vs {period}", IconName.ArrowDownRight),
            _ => new Change($"No change vs {period}", IconName.Dash),
        };
    }

    private async Task OnRangeChangedAsync(int days)
    {
        if (days == rangeDays)
            return;

        rangeDays = days;
        report = BuildReport();
        rangeMessage = $"Showing the {RangeDescription}.";
        await RedrawChartsAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        report = BuildReport();
        StateHasChanged();
        await RedrawChartsAsync();
    });

    private void OnThemeChanged() => InvokeAsync(RedrawChartsAsync);

    private async Task RedrawChartsAsync()
    {
        if (!chartsReady)
            return;

        await revenueChart.UpdateAsync(RevenueData(), RevenueOptions());
        await categoryChart.UpdateAsync(CategoryData(), CategoryOptions());
        await statusChart.UpdateAsync(StatusData(), StatusOptions());
    }

    private ChartData RevenueData()
    {
        var line = new LineChartDataset
        {
            Label = "Net revenue",
            Data = report.Daily.Select(day => (double?)(double)day.Net).ToList(),
            BorderColor = SalesColor,
            BackgroundColor = SalesFill,
            BorderWidth = 2.5,
            // Straight segments: a smoothed curve overshoots below zero between a busy day and an empty one.
            Tension = 0,
            PointRadius = new List<double> { rangeDays > 14 ? 2 : 3 },
            PointHoverRadius = new List<double> { 5 },
            PointBackgroundColor = new List<string> { SalesColor },
            PointBorderColor = new List<string> { SalesColor },
        };
        line.FillToStart();

        return new ChartData { Labels = report.Daily.Select(day => DayLabel(day.Day)).ToList(), Datasets = new List<IChartDataset> { line } };
    }

    private LineChartOptions RevenueOptions()
    {
        var options = new LineChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Locale = "en-US",
            Animation = new ChartAnimation { Duration = 0 },
            Interaction = new Interaction { Mode = InteractionMode.Index, Intersect = false },
        };

        ApplyPlugins(options.Plugins, showLegend: false);
        ApplyAxes(options.Scales, options.Scales.Y!, options.Scales.X!, "Net revenue (USD)");

        return options;
    }

    private ChartData StatusData() => new()
    {
        Labels = report.Statuses.Select(row => StatusName(row.Status)).ToList(),
        Datasets = new List<IChartDataset>
        {
            new DoughnutChartDataset
            {
                Label = "Orders",
                Data = report.Statuses.Select(row => (double?)row.Orders).ToList(),
                BackgroundColor = report.Statuses.Select(row => statusColors[row.Status]).ToList(),
                BorderColor = report.Statuses.Select(_ => "rgba(255, 255, 255, 0.6)").ToList(),
                BorderWidth = report.Statuses.Select(_ => 2d).ToList(),
                HoverOffset = report.Statuses.Select(_ => 6d).ToList(),
            },
        },
    };

    private DoughnutChartOptions StatusOptions()
    {
        var options = new DoughnutChartOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Locale = "en-US",
            Animation = new ChartAnimation { Duration = 0 },
        };

        ApplyPlugins(options.Plugins, showLegend: true);

        return options;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the axis, legend, and title text color: 4.5:1 or more on the panel in either color mode.</summary>
    private string AxisText => State.IsDarkTheme ? "#cdbfb1" : "#6b5d50";

    /// <summary>Gets the grid line color for the current color mode.</summary>
    private string GridLine => State.IsDarkTheme ? "rgba(205, 191, 177, 0.16)" : "rgba(107, 93, 80, 0.16)";

    private string CategorySummary
    {
        get
        {
            var total = report.Categories.Sum(row => row.Revenue);

            if (total == 0)
                return $"No sales in the {RangeDescription}.";

            var top = report.Categories[0];
            var share = (top.Revenue / total * 100m).ToString("0", CultureInfo.InvariantCulture);

            return $"{top.Name} led with {EcommerceDisplay.Money(top.Revenue)}, {share}% of {EcommerceDisplay.Money(total)} in merchandise sales.";
        }
    }

    private IReadOnlyList<Kpi> Kpis
    {
        get
        {
            var current = report.Current;
            var previous = report.Previous;
            var hadSales = previous.Orders > 0;
            decimal? Previous(decimal value) => hadSales ? value : null;

            return new[]
            {
                new Kpi("net", "Net revenue", EcommerceDisplay.Money(current.Net), "Order totals minus refunds",
                    Compare(current.Net, Previous(previous.Net))),
                new Kpi("orders", "Orders", Count(current.Orders),
                    current.Cancelled == 0 ? "No cancelled orders" : $"Not counting {EcommerceDisplay.Plural(current.Cancelled, "cancelled order", "cancelled orders")}",
                    Compare(current.Orders, Previous(previous.Orders))),
                new Kpi("aov", "Average order value", current.Orders == 0 ? "—" : EcommerceDisplay.Money(current.AverageOrderValue), "Before refunds, with shipping and tax",
                    Compare(current.Orders == 0 ? null : current.AverageOrderValue, Previous(previous.AverageOrderValue))),
                new Kpi("items", "Items sold", Count(current.Items), "Units across all orders",
                    Compare(current.Items, Previous(previous.Items))),
                new Kpi("refunds", "Refunds", EcommerceDisplay.Money(current.Refunded), $"On {EcommerceDisplay.Plural(current.RefundCount, "order", "orders")} placed in this period",
                    Compare(current.Refunded, Previous(previous.Refunded))),
            };
        }
    }

    private string RangeDates => $"{DayLabel(report.Start)} to {DayLabel(report.End)}";

    private string RangeDescription => $"last {rangeDays} days ({RangeDates})";

    private string RevenueSummary
    {
        get
        {
            var total = report.Daily.Sum(day => day.Net);

            if (report.Current.Orders == 0)
                return $"No sales in the {RangeDescription}.";

            var best = report.Daily.OrderByDescending(day => day.Net).ThenBy(day => day.Day).First();
            var quiet = report.Daily.Count(day => day.Net == 0);

            return $"{EcommerceDisplay.Money(total)} net over the {RangeDescription}. Best day: {DayLabel(best.Day)}, {EcommerceDisplay.Money(best.Net)}. "
                + (quiet == 0 ? "Every day had sales." : $"{EcommerceDisplay.Plural(quiet, "day", "days")} without sales.");
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    private string StatusSummary
    {
        get
        {
            var total = report.Statuses.Sum(row => row.Orders);

            if (total == 0)
                return $"No orders in the {RangeDescription}.";

            return $"{EcommerceDisplay.Plural(total, "order", "orders")} placed: " + string.Join(", ", report.Statuses.Select(row => $"{Count(row.Orders)} {StatusName(row.Status).ToLowerInvariant()}")) + ".";
        }
    }

    #endregion

    #region Nested Types

    private sealed record CategoryRevenue(string Name, decimal Revenue);

    private sealed record Change(string Text, IconName Icon);

    private sealed record CouponUsage(string Code, int Orders, decimal Discount);

    private sealed record DailyRevenue(DateTime Day, decimal Net);

    private sealed record Kpi(string Id, string Label, string Value, string Note, Change Change);

    /// <summary>Totals of one period. Cancelled orders are counted only in <see cref="Cancelled" />.</summary>
    private sealed record PeriodTotals(int Orders, decimal Gross, decimal Net, int Items, decimal Refunded, int RefundCount, int Cancelled)
    {
        /// <summary>Gets the average order total before refunds, or 0 without orders.</summary>
        public decimal AverageOrderValue => Orders == 0 ? 0m : Gross / Orders;
    }

    private sealed record ProductSales(int ProductId, string Name, int Units, decimal Revenue);

    private sealed record Report(
        DateTime Start,
        DateTime End,
        PeriodTotals Current,
        PeriodTotals Previous,
        IReadOnlyList<DailyRevenue> Daily,
        IReadOnlyList<CategoryRevenue> Categories,
        IReadOnlyList<StatusCount> Statuses,
        IReadOnlyList<ProductSales> TopProducts,
        IReadOnlyList<CouponUsage> Coupons);

    private sealed record StatusCount(EcommerceOrderStatus Status, int Orders);

    #endregion
}
