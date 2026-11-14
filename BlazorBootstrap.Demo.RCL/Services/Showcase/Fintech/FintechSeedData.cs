using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>
/// Fixed, fictitious content for the "Tallymint" personal finance site. Transactions follow fixed monthly and weekly
/// patterns over the 90 days before the site's "today", so every load shows the same history.
/// </summary>
public static class FintechSeedData
{
    #region Fields and Constants

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Tallymint";

    /// <summary>The fixed "today" all dates on the site are relative to (late in the month, so budgets have real activity).</summary>
    public static readonly DateTime Today = new(2026, 11, 24);

    private static readonly double[] netWorthFactors = { .78, .8, .79, .83, .85, .84, .88, .9, .92, .91, .96, 1 };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The customer's accounts and their current balances.</summary>
    public static List<FintechAccount> Accounts() => new()
    {
        new("checking", "Everyday Checking", FintechAccountKind.Checking, "•••• 4821", 4820.55m, null),
        new("savings", "High-Yield Savings", FintechAccountKind.Savings, "•••• 7350", 18250.00m, null),
        new("card", "Rewards Credit Card", FintechAccountKind.CreditCard, "•••• 9012", -1284.30m, 6000m),
    };

    /// <summary>Monthly limits for the spending categories.</summary>
    public static List<FintechBudget> Budgets() => new()
    {
        new("housing", 1650m),
        new("groceries", 600m),
        new("dining", 300m),
        new("transport", 250m),
        new("bills", 350m),
        new("shopping", 300m),
        new("entertainment", 120m),
        new("health", 100m),
    };

    /// <summary>Spending and income categories.</summary>
    public static IReadOnlyList<FintechCategory> Categories() => new List<FintechCategory>
    {
        new("housing", "Housing", IconName.House, true),
        new("groceries", "Groceries", IconName.Basket, true),
        new("dining", "Dining", IconName.CupHot, true),
        new("transport", "Transport", IconName.CarFront, true),
        new("bills", "Bills & utilities", IconName.Lightning, true),
        new("shopping", "Shopping", IconName.Bag, true),
        new("entertainment", "Entertainment", IconName.Film, true),
        new("health", "Health", IconName.HeartPulse, true),
        new("income", "Income", IconName.Cash, false),
        new("transfer", "Transfers", IconName.ArrowLeftRight, false),
    };

    /// <summary>
    /// Net worth for the last 12 months. Past months scale the seeded net worth (<paramref name="seed" />), so they never move;
    /// only the last value, <paramref name="current" />, follows the session's transfers.
    /// </summary>
    public static IReadOnlyList<double> NetWorthHistory(decimal seed, decimal current)
    {
        var history = netWorthFactors.Select(factor => Math.Round((double)seed * factor)).ToList();
        history[^1] = (double)current;

        return history;
    }

    /// <summary>The month labels of the 12-month net worth history, oldest first.</summary>
    public static IReadOnlyList<string> NetWorthMonths() =>
        Enumerable.Range(0, netWorthFactors.Length)
            .Select(index => Today.AddMonths(index - (netWorthFactors.Length - 1)).ToString("MMM", CultureInfo.InvariantCulture))
            .ToList();

    /// <summary>Saved people and companies money can be sent to.</summary>
    public static IReadOnlyList<FintechPayee> Payees() => new List<FintechPayee>
    {
        new("payee-avery", "Avery Ashdown", "•••• 1188"),
        new("payee-larkspur", "Larkspur Residences (rent)", "•••• 5402"),
        new("payee-rowan", "Rowan Calloway", "•••• 3076"),
        new("payee-fairbank", "Fairbank Dental", "•••• 6614"),
    };

    /// <summary>Ninety days of transactions, newest first.</summary>
    public static List<FintechTransaction> Transactions()
    {
        var items = new List<FintechTransaction>();
        var id = 5000;

        void Add(DateTime date, string account, string description, string category, decimal amount, bool pending = false) =>
            items.Add(new FintechTransaction(id++, date, account, description, category, amount, pending));

        string[] groceries = { "Greenhollow Market", "Fairbank Grocers", "Mossgrove Co-op" };
        string[] dining = { "Ember & Rye", "Quillstone Café", "Noodle Harbor", "Juniper Bistro" };
        string[] shopping = { "Northwind Outfitters", "Kindred Goods", "Pinecrest Books" };

        for (var daysAgo = 89; daysAgo >= 0; daysAgo--)
        {
            var date = Today.AddDays(-daysAgo).AddHours(9 + (daysAgo % 9)).AddMinutes(daysAgo * 7 % 60);
            var day = date.Day;
            var pending = daysAgo <= 1;

            if (day is 1 or 15)
                Add(date, "checking", "Payroll · Orbitworks", "income", 3150.00m, pending);

            if (day == 1)
                Add(date, "checking", "Rent · Larkspur Residences", "housing", -1650.00m, pending);

            if (day == 5)
                Add(date, "card", "Streamline+ subscription", "entertainment", -15.99m, pending);

            if (day == 10)
                Add(date, "checking", "Brightwave Internet", "bills", -59.99m, pending);

            if (day == 12)
                Add(date, "checking", "Ridgeview Energy", "bills", -(84m + (date.Month * 7 % 31)), pending);

            if (day == 18)
                Add(date, "checking", "Skylark Mobile", "bills", -42.00m, pending);

            if (day == 16)
            {
                Add(date, "checking", "Transfer to High-Yield Savings", "transfer", -500.00m, pending);
                Add(date, "savings", "Transfer from Everyday Checking", "transfer", 500.00m, pending);
            }

            if (day == 20)
            {
                Add(date, "checking", "Credit card payment", "transfer", -900.00m, pending);
                Add(date, "card", "Payment received, thank you", "transfer", 900.00m, pending);
            }

            if (daysAgo % 3 == 0)
                Add(date, "card", groceries[daysAgo / 3 % groceries.Length], "groceries", -(38m + (daysAgo * 13 % 104) + .45m), pending);

            if (daysAgo % 4 == 1)
                Add(date, "card", dining[daysAgo / 4 % dining.Length], "dining", -(14m + (daysAgo * 11 % 46) + .80m), pending);

            if (daysAgo % 7 == 2)
                Add(date, "card", "Cinderpeak Fuel", "transport", -(41m + (daysAgo * 5 % 23) + .10m), pending);

            if (day == 3)
                Add(date, "checking", "Metro transit pass", "transport", -75.00m, pending);

            if (daysAgo % 11 == 5)
                Add(date, "card", shopping[daysAgo / 11 % shopping.Length], "shopping", -(24m + (daysAgo * 17 % 120) + .99m), pending);

            if (daysAgo % 13 == 6)
                Add(date, "card", "Harborline Cinema", "entertainment", -(18m + (daysAgo % 3 * 6)), pending);

            if (daysAgo % 17 == 8)
                Add(date, "card", "Tidewater Pharmacy", "health", -(12m + (daysAgo * 3 % 27) + .25m), pending);
        }

        return items.OrderByDescending(item => item.Date).ThenByDescending(item => item.Id).ToList();
    }

    #endregion

    #endregion
}
