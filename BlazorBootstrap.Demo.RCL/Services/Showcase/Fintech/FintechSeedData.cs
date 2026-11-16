using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>
/// Fixed, fictitious content for the "Tallymint" personal finance site. Transactions follow fixed monthly and weekly
/// patterns over the 90 days before the site's "today", so every load shows the same history.
/// </summary>
public static class FintechSeedData
{
    #region Fields and Constants

    /// <summary>The role of a bank customer.</summary>
    public const string CustomerRole = "customer";

    /// <summary>The demo customer's name.</summary>
    public const string CustomerName = "Jordan Ashdown";

    /// <summary>The role of a bank operations user (the back office).</summary>
    public const string OpsRole = "ops";

    /// <summary>The ID of the demo customer account.</summary>
    public const string SeededCustomerId = "jordan";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Tallymint";

    /// <summary>When a fresh tab's clock starts; the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 0, 0);

    /// <summary>The seed's "today" all seeded dates are relative to (late in the month, so budgets have real activity).</summary>
    public static readonly DateTime Today = Now.Date;

    private static readonly double[] netWorthFactors = { .78, .8, .79, .83, .85, .84, .88, .9, .92, .91, .96, 1 };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The customer's accounts and their current balances.</summary>
    public static List<FintechAccount> Accounts() => new()
    {
        new("checking", "Everyday Checking", FintechAccountKind.Checking, "•••• 4821", 4820.55m, null) { CardLast4 = "3307" },
        new("savings", "High-Yield Savings", FintechAccountKind.Savings, "•••• 7350", 18250.00m, null),
        new("card", "Rewards Credit Card", FintechAccountKind.CreditCard, "•••• 9012", -1284.30m, 6000m) { CardLast4 = "9012" },
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

    /// <summary>
    /// The audit log when the tab starts, oldest first: the operations and customer actions behind the seeded transfers and
    /// disputes.
    /// </summary>
    public static List<FintechAuditEntry> Audit() => new()
    {
        new(1, Now.AddDays(-12), "Priya Raman", "Operations", "Dispute refunded", "Dispute 3088 · Lena Vogel", "Merchant confirmed the double charge; $64.20 credited."),
        new(2, Now.AddDays(-5).AddHours(2), CustomerName, "Customer", "Dispute opened", "Dispute 3101 · Jordan Ashdown", "Duplicate charge at a restaurant."),
        new(3, Now.AddDays(-4), "Priya Raman", "Operations", "Dispute under review", "Dispute 3101 · Jordan Ashdown", "Merchant contacted."),
        new(4, Now.AddDays(-3), CustomerName, "Customer", "Transfer scheduled", "TM-260415 · Jordan Ashdown", "$250.00 to High-Yield Savings on Dec 1."),
        new(5, Now.AddDays(-2), CustomerName, "Customer", "Payee added", "Rowan Calloway · Jordan Ashdown", "Name confirmed; Northline Bank •••• 3076."),
        new(6, Now.AddHours(-5), "System", "System", "Transfer held", "TM-260411 · Omar Haddad", "A payment to a payee whose name couldn't be confirmed."),
        new(7, Now.AddHours(-3), "System", "System", "Transfer held", "TM-260412 · Jordan Ashdown", "A large payment to a payee added in the last 7 days."),
        new(8, Now.AddHours(-1), "System", "System", "Transfer held", "TM-260409 · Lena Vogel", "A large payment to a payee."),
    };

    /// <summary>Internal notes operations wrote about customers.</summary>
    public static List<FintechCustomerNote> CustomerNotes() => new()
    {
        new(1, "c-omar", Now.AddDays(-30), "Priya Raman", "Called about two card payments abroad; both genuine (travelling until Dec 10)."),
    };

    /// <summary>The bank's customers, as operations see them. Jordan's figures are live in the tab; the others are fixed summaries.</summary>
    public static IReadOnlyList<FintechCustomer> Customers() => new List<FintechCustomer>
    {
        new(SeededCustomerId, CustomerName, "jordan.ashdown@example.com", "Portland", new DateOnly(2019, 4, 2), "Personal", FintechCustomerStatus.Active, 3, 0m),
        new("c-lena", "Lena Vogel", "lena.vogel@example.com", "Denver", new DateOnly(2016, 9, 14), "Premier", FintechCustomerStatus.Active, 4, 84_210.55m),
        new("c-omar", "Omar Haddad", "omar.haddad@example.com", "Austin", new DateOnly(2022, 1, 20), "Personal", FintechCustomerStatus.Watch, 2, 6_402.18m),
        new("c-kenji", "Kenji Mori", "kenji.mori@example.com", "Seattle", new DateOnly(2020, 6, 8), "Personal", FintechCustomerStatus.Active, 2, 12_940.00m),
        new("c-sofia", "Sofia Romano", "sofia.romano@example.com", "Chicago", new DateOnly(2018, 11, 30), "Premier", FintechCustomerStatus.Active, 3, 41_775.90m),
    };

    /// <summary>The demo accounts: one bank customer and one operations user. There is no self-registration: banks open accounts in a branch or an app.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededCustomerId, CustomerRole, CustomerName, "jordan.ashdown@example.com", "bank-demo-26", "JA"),
        new("priya", OpsRole, "Priya Raman", "priya.raman@tallymint.example", "ops-demo-26", "PR"),
    };

    /// <summary>The customer's disputes: one under review.</summary>
    public static List<FintechDispute> Disputes(IReadOnlyList<FintechTransaction> transactions)
    {
        var charge = transactions.First(item => item.CategoryId == "dining" && item.Date < Today.AddDays(-8) && item.Date > Today.AddDays(-20));

        return new List<FintechDispute>
        {
            new()
            {
                Id = 3101,
                CustomerId = SeededCustomerId,
                TransactionId = charge.Id,
                Merchant = charge.Description,
                Amount = -charge.Amount,
                Reason = FintechDisputeReason.Duplicate,
                Details = "I was charged twice for the same dinner; the restaurant ran the card again after the first payment went through.",
                CreatedAt = Now.AddDays(-5).AddHours(2),
                Status = FintechDisputeStatus.UnderReview,
            },
            new()
            {
                Id = 3095, CustomerId = "c-omar", Merchant = "Streamline+ subscription", Amount = 15.99m, Reason = FintechDisputeReason.Cancelled,
                Details = "I cancelled this subscription in October and was still charged in November.", CreatedAt = Now.AddDays(-2).AddHours(-6), Status = FintechDisputeStatus.Submitted,
            },
            new()
            {
                Id = 3088, CustomerId = "c-lena", Merchant = "Juniper Bistro", Amount = 64.20m, Reason = FintechDisputeReason.Duplicate,
                Details = "The same dinner was charged twice on the same evening.", CreatedAt = Now.AddDays(-20), Status = FintechDisputeStatus.Refunded,
                ClosedAt = Now.AddDays(-12), Resolution = "The merchant confirmed the double charge. We credited $64.20 to your card.",
            },
        };
    }

    /// <summary>The customer's alerts feed when the tab starts.</summary>
    public static List<FintechNotice> Notices() => new()
    {
        new() { Id = 3, At = Now.AddHours(-3), Kind = FintechAlertKind.Transfer, Title = "Transfer held for a check", Body = "Your $2,400.00 transfer to Rowan Calloway is held for a routine check. We'll update you within one business day.", Link = "/showcase/fintech/transfers/TM-260412" },
        new() { Id = 2, At = Now.AddDays(-1).AddHours(-4), Kind = FintechAlertKind.Budget, Title = "Dining is at 80% of its budget", Body = "You've spent most of this month's dining budget with a week to go.", Link = "/showcase/fintech/budgets" },
        new() { Id = 1, At = Now.AddDays(-5).AddHours(2), Kind = FintechAlertKind.Dispute, Title = "Dispute 3101 is under review", Body = "We've contacted the merchant about the duplicate charge. Most disputes are resolved within 10 business days.", Link = "/showcase/fintech/disputes/3101", IsRead = true },
    };

    /// <summary>
    /// The fictitious account directory the payee name check uses: account numbers and their holders. Any other account
    /// can't be checked (the result is "unavailable").
    /// </summary>
    public static IReadOnlyDictionary<string, string> PayeeDirectory() => new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["40001001"] = "Morgan Ellis",
        ["40001002"] = "Brightwater Plumbing",
        ["40001003"] = "Sam Okafor",
        ["40001004"] = "Cedar Hill Nursery School",
    };

    /// <summary>Saved people and companies money can be sent to.</summary>
    public static List<FintechPayee> Payees() => new()
    {
        new("payee-avery", "Avery Ashdown", "•••• 1188") { AddedAt = Now.AddYears(-2), BankName = "Harborview Credit Union" },
        new("payee-larkspur", "Larkspur Residences (rent)", "•••• 5402") { AddedAt = Now.AddMonths(-14), BankName = "First Meridian Bank" },
        new("payee-rowan", "Rowan Calloway", "•••• 3076") { AddedAt = Now.AddDays(-2), BankName = "Northline Bank" },
        new("payee-fairbank", "Fairbank Dental", "•••• 6614") { AddedAt = Now.AddMonths(-7), BankName = "First Meridian Bank", Nickname = "Dentist" },
    };

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(CustomerRole, "Customer", "/showcase/fintech", "/showcase/fintech/profile", "Checks accounts, moves money, manages payees, budgets, and alerts."),
        new(OpsRole, "Bank operations", "/showcase/fintech/ops", "/showcase/fintech/ops/account", "Reviews held transfers, resolves disputes, follows customers and scheduled postings, and reads the audit log."),
    };

    /// <summary>The customer's transfers when the tab starts: a savings top-up scheduled for next week, and a large payment held for a check.</summary>
    public static List<FintechTransfer> Transfers() => new()
    {
        new()
        {
            Reference = "TM-260415", CustomerId = SeededCustomerId, CustomerName = CustomerName, FromAccountId = "checking", ToId = "savings", ToName = "High-Yield Savings",
            ToOwnAccount = true, Amount = 250m, Date = DateOnly.FromDateTime(Today.AddDays(7)), Note = "Holiday fund", Status = FintechTransferStatus.Scheduled, CreatedAt = Now.AddDays(-3),
        },
        new()
        {
            Reference = "TM-260412", CustomerId = SeededCustomerId, CustomerName = CustomerName, FromAccountId = "checking", ToId = "payee-rowan", ToName = "Rowan Calloway",
            Amount = 2400m, Date = DateOnly.FromDateTime(Today), Note = "Car deposit", Status = FintechTransferStatus.Held, CreatedAt = Now.AddHours(-3),
            HoldReason = "A large payment to a payee added in the last 7 days",
        },
        new()
        {
            Reference = "TM-260409", CustomerId = "c-lena", CustomerName = "Lena Vogel", FromAccountId = "Checking •••• 2290", ToId = "Northgate Motors •••• 7710", ToName = "Northgate Motors",
            Amount = 4850m, Date = DateOnly.FromDateTime(Today), Note = "Car balance", Status = FintechTransferStatus.Held, CreatedAt = Now.AddHours(-1),
            HoldReason = "A large payment to a payee",
        },
        new()
        {
            Reference = "TM-260411", CustomerId = "c-omar", CustomerName = "Omar Haddad", FromAccountId = "Checking •••• 5521", ToId = "S. Brennan •••• 4407", ToName = "S. Brennan",
            Amount = 780m, Date = DateOnly.FromDateTime(Today), Note = "Deposit", Status = FintechTransferStatus.Held, CreatedAt = Now.AddHours(-5),
            HoldReason = "A payment to a payee whose name couldn't be confirmed",
        },
        new()
        {
            Reference = "TM-260405", CustomerId = "c-lena", CustomerName = "Lena Vogel", FromAccountId = "Checking •••• 2290", ToId = "Fairview Mortgage •••• 3012", ToName = "Fairview Mortgage",
            Amount = 1920m, Date = DateOnly.FromDateTime(Today.AddDays(2)), Note = "December mortgage", Status = FintechTransferStatus.Scheduled, CreatedAt = Now.AddDays(-6),
        },
        new()
        {
            Reference = "TM-260407", CustomerId = "c-kenji", CustomerName = "Kenji Mori", FromAccountId = "Checking •••• 6043", ToId = "Kenji Mori savings •••• 8812", ToName = "Kenji Mori (savings)",
            Amount = 400m, Date = DateOnly.FromDateTime(Today.AddDays(6)), Status = FintechTransferStatus.Scheduled, CreatedAt = Now.AddDays(-4),
        },
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
