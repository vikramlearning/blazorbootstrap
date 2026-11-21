using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

/// <summary>
/// Fixed, fictitious content for the "Tallymint" bank, a banking operations back office with its customer bank. Everything
/// is generated from fixed patterns relative to the site's "today", so every tab starts with exactly the same data: about
/// 240 customers, 13 months of the demo customer's transactions, the transfers of every customer (dozens held for a check
/// and scheduled), about 60 disputes in every state, the audit log behind them, and the alerts and staff notifications.
/// The tab's snapshot stores only what differs from this seed (<see cref="FintechState.CaptureData" />). The AML alerts and
/// cases are in <c>FintechSeedData.Aml.cs</c>.
/// </summary>
public static partial class FintechSeedData
{
    #region Fields and Constants

    /// <summary>The role of a bank customer.</summary>
    public const string CustomerRole = "customer";

    /// <summary>The demo customer's name.</summary>
    public const string CustomerName = "Jordan Ashdown";

    /// <summary>The role of a bank operations user (the back office).</summary>
    public const string OpsRole = "ops";

    /// <summary>The demo operations user's account ID.</summary>
    public const string OpsAccountId = "priya";

    /// <summary>The demo operations user's name.</summary>
    public const string OpsName = "Priya Raman";

    /// <summary>The fictitious product name.</summary>
    public const string ProductName = "Tallymint";

    /// <summary>The ID of the demo customer account.</summary>
    public const string SeededCustomerId = "jordan";

    /// <summary>The name the audit log uses for automatic actions (holds, postings, escalations).</summary>
    public const string SystemActor = "System";

    /// <summary>The site's base route (the same as <c>FintechLayout.Base</c>).</summary>
    internal const string SiteBase = "/showcase/fintech";

    /// <summary>The operations console's base route (the same as <c>FintechOpsLayout.Base</c>).</summary>
    internal const string OpsBase = SiteBase + "/ops";

    /// <summary>When a fresh tab's clock starts; the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 0, 0);

    /// <summary>The seed's "today" all seeded dates are relative to (late in the month, so budgets have real activity).</summary>
    public static readonly DateTime Today = Now.Date;

    /// <summary>How many days of the demo customer's transactions are seeded: 13 months.</summary>
    private const int HistoryDays = 395;

    private static readonly string[] cities =
    {
        "Portland", "Denver", "Austin", "Seattle", "Chicago", "Boston", "Atlanta", "Phoenix", "Minneapolis", "San Diego",
        "Nashville", "Columbus", "Raleigh", "Salt Lake City", "Pittsburgh", "Sacramento", "Kansas City", "Madison", "Tucson", "Richmond",
    };

    private static readonly Lazy<IReadOnlyList<FintechCustomer>> customers = new(BuildCustomers);

    private static readonly string[] disputeMerchants =
    {
        "Northwind Outfitters", "Brightline Electronics", "Cobalt Air", "Summit Fitness Club", "Lakeside Hotel & Spa", "Parcelpoint Delivery",
        "Streamline+ subscription", "Juniper Bistro", "Copperleaf Furniture", "Harbor Home Goods", "Quickfare Rideshare", "Redwood Car Rental",
        "Silverline Mobile", "Evergreen Pharmacy", "Maple & Main Grocers", "Atlas Ticketing", "Orchard Online Market", "Beacon Insurance",
        "Prairie Pet Supplies", "Glimmer Jewelry",
    };

    private static readonly string[] firstNames =
    {
        "Avery", "Blake", "Camila", "Dmitri", "Elena", "Farah", "Gabriel", "Hana", "Isaac", "Jasmine", "Kai", "Leah", "Mateo", "Nadia",
        "Oscar", "Priyanka", "Quinn", "Rafael", "Sana", "Tomas", "Uma", "Victor", "Wren", "Ximena", "Yusuf", "Zoe", "Amara", "Bruno",
        "Chloe", "Diego", "Esther", "Felix", "Grace", "Hugo", "Ingrid", "Jonah", "Keira", "Liam", "Maya", "Noah",
    };

    private static readonly string[] lastNames =
    {
        "Alvarez", "Brennan", "Castillo", "Dubois", "Eriksen", "Fischer", "Garcia", "Hoffman", "Ibrahim", "Jensen", "Kowalski", "Lindqvist",
        "Moreau", "Nakamura", "Okafor", "Petrov", "Quintero", "Rossi", "Schmidt", "Tanaka", "Ueda", "Varga", "Whitaker", "Xu", "Yilmaz",
        "Zimmerman", "Abbott", "Bianchi", "Chen", "Delgado", "Ellison", "Foster", "Gupta", "Hayes", "Iverson", "Jovanovic", "Kim", "Larsen",
        "Mensah", "Novak",
    };

    private static readonly double[] netWorthFactors = { .78, .8, .79, .83, .85, .84, .88, .9, .92, .91, .96, 1 };

    /// <summary>Staff who worked the seeded history; only <see cref="OpsName" /> can sign in to the demo.</summary>
    private static readonly string[] opsStaff = { OpsName, "Daniel Okoro", "Mei Lin", "Samuel Ortiz" };

    private static readonly string[] payeeNames =
    {
        "Northgate Motors", "Fairview Mortgage", "Bluebird Property Group", "Crestline Builders", "Harbor Tuition", "S. Brennan", "Ridgeway Dental",
        "Oakmont Auto Repair", "Pinecrest Storage", "Clearwater Roofing", "M. Delgado", "Summit Legal Services", "Lumen Solar", "T. Whitaker",
        "Granite State Movers", "Willow Creek HOA", "Valley Veterinary", "Brookside Daycare", "R. Castillo", "Keystone Contractors",
    };

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

    /// <summary>
    /// Builds a fresh tab's data, with every cross-reference in place: the demo customer's transactions linked to their
    /// transfers and disputes, the transfers and disputes of every customer, and the audit log, alerts, and staff
    /// notifications that go with them. Every call returns new objects with the same content.
    /// </summary>
    public static FintechSeedSet Build()
    {
        var transactions = Transactions();
        var transfers = new List<FintechTransfer>();
        var reference = 250001;

        // The demo customer's own transfers in the ledger (savings top-ups and card payments) become completed transfers.
        foreach (var debit in transactions.Where(item => item.CategoryId == "transfer" && item.Amount < 0).OrderBy(item => item.Date).ToList())
        {
            var toSavings = debit.Description.Contains("Savings", StringComparison.Ordinal);
            var credit = transactions.First(item => item.CategoryId == "transfer" && item.Amount == -debit.Amount && item.Date == debit.Date && item.AccountId != debit.AccountId);
            var code = Ref(reference++);

            transfers.Add(new FintechTransfer
            {
                Reference = code, CustomerId = SeededCustomerId, CustomerName = CustomerName, FromAccountId = debit.AccountId, ToId = credit.AccountId,
                ToName = toSavings ? "High-Yield Savings" : "Rewards Credit Card", ToOwnAccount = true, Amount = -debit.Amount, Date = DateOnly.FromDateTime(debit.Date),
                Note = toSavings ? "Monthly savings" : null, Status = FintechTransferStatus.Completed, CreatedAt = debit.Date.AddMinutes(-2), ClosedAt = debit.Date,
            });
            Link(transactions, debit.Id, item => item with { TransferReference = code });
            Link(transactions, credit.Id, item => item with { TransferReference = code });
        }

        transfers.AddRange(DemoCustomerTransfers());
        transfers.AddRange(OtherTransfers(ref reference));

        var disputes = Disputes(transactions);

        foreach (var dispute in disputes.Where(dispute => dispute.CustomerId == SeededCustomerId))
        {
            Link(transactions, dispute.TransactionId, item => item with { DisputeId = dispute.Id });

            // A refunded dispute of the demo customer has its credit in the ledger.
            if (dispute.Status == FintechDisputeStatus.Refunded)
            {
                var charge = transactions.First(item => item.Id == dispute.TransactionId);
                transactions.Add(new FintechTransaction(transactions.Max(item => item.Id) + 1, dispute.ClosedAt!.Value, charge.AccountId,
                    $"Refund · dispute {Number(dispute.Id)} · {dispute.Merchant}", charge.CategoryId, dispute.Amount, false));
            }
        }

        var notes = CustomerNotes();
        var (alerts, cases) = Aml(transfers);

        return new FintechSeedSet(
            Accounts(),
            alerts,
            cases,
            Audit(transfers, disputes, notes, alerts, cases),
            Budgets(),
            notes,
            disputes,
            Notices(),
            Payees(),
            StaffNotifications(transfers, disputes, alerts, cases),
            transactions.OrderByDescending(item => item.Date).ThenByDescending(item => item.Id).ToList(),
            transfers);
    }

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
    /// The bank's customers, as operations see them, by ID order: the demo customer (whose figures are live in the tab), four
    /// named customers the seeded transfers and disputes refer to, and about 235 more. Built once; the records never change.
    /// </summary>
    public static IReadOnlyList<FintechCustomer> Customers() => customers.Value;

    /// <summary>
    /// The demo accounts, in the sign-in page's order: an operations analyst, a senior reviewer (who also decides escalated AML
    /// cases), and one bank customer. There is no self-registration: banks open accounts in a branch or an app.
    /// </summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(OpsAccountId, OpsRole, OpsName, "priya.raman@tallymint.example", "ops-demo-26", "PR"),
        new(SeniorAccountId, SeniorRole, SeniorName, "mei.lin@tallymint.example", "senior-demo-26", "ML"),
        new(SeededCustomerId, CustomerRole, CustomerName, "jordan.ashdown@example.com", "bank-demo-26", "JA"),
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
        new("payee-larkspur", "Larkspur Residences (rent)", "•••• 5402") { AddedAt = Now.AddMonths(-14), BankName = "First Meridian Bank", Nickname = "Landlord" },
        new("payee-rowan", "Rowan Calloway", "•••• 3076") { AddedAt = Now.AddDays(-2), BankName = "Northline Bank" },
        new("payee-fairbank", "Fairbank Dental", "•••• 6614") { AddedAt = Now.AddMonths(-7), BankName = "First Meridian Bank", Nickname = "Dentist" },
    };

    /// <summary>The roles of the site, the back office first: the sign-in page lists them in this order.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(OpsRole, "Bank operations", OpsBase, OpsBase + "/account", "Works the transfer review queue and AML alerts and cases, decides disputes, follows customers and scheduled postings, and reads the audit log."),
        new(SeniorRole, "Senior reviewer", OpsBase, OpsBase + "/account", "Everything bank operations do, plus deciding escalated AML cases: approving a closure or a SAR, or returning the case to the analyst."),
        new(CustomerRole, "Customer", SiteBase, SiteBase + "/profile", "Checks accounts, moves money, manages payees, budgets, and alerts, and disputes charges."),
    };

    #endregion

    #region Private Methods

    /// <summary>The audit log when the tab starts, oldest first, built from the seeded transfers, disputes, and notes.</summary>
    private static List<ShowcaseAuditEntry> Audit(IReadOnlyList<FintechTransfer> transfers, IReadOnlyList<FintechDispute> disputes, IReadOnlyList<FintechCustomerNote> notes,
        IReadOnlyList<FintechAmlAlert> alerts, IReadOnlyList<FintechAmlCase> cases)
    {
        var entries = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();

        foreach (var dispute in disputes)
        {
            var target = $"Dispute {Number(dispute.Id)} · {NameOf(dispute.CustomerId)}";
            var url = OpsBase + "/disputes/" + Number(dispute.Id);
            var staff = opsStaff[Pick(dispute.Id, 5, opsStaff.Length)];

            entries.Add((dispute.CreatedAt, NameOf(dispute.CustomerId), "Dispute opened", target, url, $"{Money(dispute.Amount)} at {dispute.Merchant}."));

            if (dispute.Status != FintechDisputeStatus.Submitted)
            {
                var reviewAt = dispute.CreatedAt.AddHours(20 + Pick(dispute.Id, 6, 30));
                entries.Add((dispute.ClosedAt is { } closed && closed < reviewAt ? closed.AddHours(-1) : reviewAt, staff, "Dispute under review", target, url, "Merchant contacted."));
            }

            if (dispute.EscalatedAt is { } escalated)
                entries.Add((escalated, SystemActor, "Dispute escalated", target, url, $"Open more than {FintechState.DisputeTargetDays} days."));

            if (dispute.ClosedAt is { } closedAt)
                entries.Add((closedAt, staff, dispute.Status == FintechDisputeStatus.Refunded ? "Dispute refunded" : "Dispute declined", target, url, dispute.Resolution));
        }

        foreach (var transfer in transfers.Where(transfer => transfer.CustomerId != SeededCustomerId || transfer.Status != FintechTransferStatus.Completed))
        {
            var target = $"{transfer.Reference} · {transfer.CustomerName}";
            var url = OpsBase + "/transfers/" + transfer.Reference;
            var staff = opsStaff[Pick(transfer.Reference.Length + transfer.Reference[^1], 7, opsStaff.Length)];

            switch (transfer.Status)
            {
                case FintechTransferStatus.Held:
                    entries.Add((transfer.CreatedAt, transfer.CustomerName, "Transfer confirmed", target, url, $"{Money(transfer.Amount)} to {transfer.ToName}."));
                    entries.Add((transfer.CreatedAt.AddSeconds(1), SystemActor, "Transfer held", target, url, transfer.HoldReason + "."));
                    break;

                case FintechTransferStatus.Scheduled:
                    entries.Add((transfer.CreatedAt, transfer.CustomerName, "Transfer scheduled", target, url, $"{Money(transfer.Amount)} to {transfer.ToName} on {Day(transfer.Date)}."));
                    break;

                case FintechTransferStatus.Rejected:
                    entries.Add((transfer.CreatedAt.AddSeconds(1), SystemActor, "Transfer held", target, url, "A large payment to a payee."));
                    entries.Add((transfer.ClosedAt!.Value, staff, "Transfer rejected", target, url, transfer.RejectReason));
                    break;

                case FintechTransferStatus.Cancelled:
                    entries.Add((transfer.ClosedAt!.Value, transfer.CustomerName, "Transfer cancelled", target, url, $"{Money(transfer.Amount)} to {transfer.ToName}."));
                    break;

                default:
                    // Larger completed transfers of other customers were held and approved; the others went straight through.
                    if (transfer.Amount >= FintechState.HoldLargeAmount)
                    {
                        entries.Add((transfer.CreatedAt.AddSeconds(1), SystemActor, "Transfer held", target, url, "A large payment to a payee."));
                        entries.Add((transfer.ClosedAt!.Value, staff, "Transfer approved", target, url, $"{Money(transfer.Amount)} to {transfer.ToName}, sent."));
                    }

                    break;
            }
        }

        foreach (var note in notes)
            entries.Add((note.At, note.Author, "Customer note added", NameOf(note.CustomerId), OpsBase + "/customers/" + note.CustomerId, Clip(note.Text, 80)));

        entries.Add((Now.AddDays(-2), CustomerName, "Payee added", $"Rowan Calloway · {CustomerName}", SiteBase + "/payees", "Name confirmed; Northline Bank •••• 3076."));
        entries.Add((Now.AddDays(-1).AddHours(-1), OpsName, "Signed in", OpsName, null, "Password and one-time code."));
        AmlAudit(entries, alerts, cases);

        return entries
            .Where(entry => entry.At <= Now)
            .OrderBy(entry => entry.At)
            .Select((entry, index) => new ShowcaseAuditEntry
            {
                Id = index + 1, At = entry.At, Actor = entry.Actor, Action = entry.Action, Target = entry.Target, TargetUrl = entry.Url,
                Details = string.IsNullOrWhiteSpace(entry.Details) ? null : Clip(entry.Details, 420),
            })
            .ToList();
    }

    private static IReadOnlyList<FintechCustomer> BuildCustomers()
    {
        var list = new List<FintechCustomer>
        {
            new(SeededCustomerId, CustomerName, "jordan.ashdown@example.com", "Portland", new DateOnly(2019, 4, 2), "Personal", FintechCustomerStatus.Active, 3, 0m),
            new("c-lena", "Lena Vogel", "lena.vogel@example.com", "Denver", new DateOnly(2016, 9, 14), "Premier", FintechCustomerStatus.Active, 4, 84_210.55m),
            new("c-omar", "Omar Haddad", "omar.haddad@example.com", "Austin", new DateOnly(2022, 1, 20), "Personal", FintechCustomerStatus.Watch, 2, 6_402.18m),
            new("c-kenji", "Kenji Mori", "kenji.mori@example.com", "Seattle", new DateOnly(2020, 6, 8), "Personal", FintechCustomerStatus.Active, 2, 12_940.00m),
            new("c-sofia", "Sofia Romano", "sofia.romano@example.com", "Chicago", new DateOnly(2018, 11, 30), "Premier", FintechCustomerStatus.Active, 3, 41_775.90m),
        };
        var names = list.Select(customer => customer.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Each first name meets every last name once (the pairs are unique), so no two customers share a name.
        for (var index = 0; list.Count < 240; index++)
        {
            var name = $"{firstNames[index % firstNames.Length]} {lastNames[(index / firstNames.Length + 3 * index) % lastNames.Length]}";

            if (!names.Add(name))
                continue;

            var segment = Pick(index, 1, 10) switch { < 6 => "Personal", < 9 => "Premier", _ => "Small business" };
            var balance = segment switch
            {
                "Premier" => 25_000m + Pick(index, 2, 15_000_000) / 100m,
                "Small business" => 8_000m + Pick(index, 2, 9_000_000) / 100m,
                _ => 300m + Pick(index, 2, 3_500_000) / 100m,
            };

            list.Add(new FintechCustomer(
                string.Create(CultureInfo.InvariantCulture, $"c-{1001 + index}"),
                name,
                $"{name.ToLowerInvariant().Replace(' ', '.')}@example.com",
                cities[Pick(index, 3, cities.Length)],
                DateOnly.FromDateTime(Today.AddDays(-(120 + Pick(index, 4, 4200)))),
                segment,
                Pick(index, 5, 100) < 6 ? FintechCustomerStatus.Watch : FintechCustomerStatus.Active,
                1 + Pick(index, 6, segment == "Personal" ? 3 : 5),
                balance));
        }

        return list;
    }

    private static string Clip(string text, int length) => text.Length <= length ? text : string.Concat(text.AsSpan(0, length - 1), "…");

    /// <summary>Internal notes operations wrote about customers.</summary>
    private static List<FintechCustomerNote> CustomerNotes()
    {
        var notes = new List<FintechCustomerNote>
        {
            new(1, "c-omar", Now.AddDays(-30), OpsName, "Called about two card payments abroad; both genuine (travelling until Dec 10)."),
        };
        var watched = Customers().Where(customer => customer.Status == FintechCustomerStatus.Watch && customer.Id != "c-omar").Take(12).ToList();
        string[] texts =
        {
            "Several payments to a new payee in one week. Customer confirmed by phone; keep on watch for 30 days.",
            "Card used in two countries on the same day. Card replaced; customer confirmed the genuine payments.",
            "Asked about moving a large sum to a \"safe account\" after a phone call. Explained the scam; no transfer made.",
            "Address changed and a new payee added on the same day. Identity checked in branch.",
            "Chargebacks on three subscriptions this quarter. Reminded the customer how to cancel with the merchant.",
        };

        for (var index = 0; index < watched.Count; index++)
            notes.Add(new FintechCustomerNote(index + 2, watched[index].Id, Now.AddDays(-(3 + index * 4)).AddHours(-index), opsStaff[index % opsStaff.Length], texts[index % texts.Length]));

        return notes;
    }

    private static string Day(DateOnly date) => date.ToString("MMM d", CultureInfo.InvariantCulture);

    /// <summary>The demo customer's waiting transfers: a savings top-up scheduled for next week, and a large payment held for a check.</summary>
    private static IEnumerable<FintechTransfer> DemoCustomerTransfers() => new[]
    {
        new FintechTransfer
        {
            Reference = "TM-260415", CustomerId = SeededCustomerId, CustomerName = CustomerName, FromAccountId = "checking", ToId = "savings", ToName = "High-Yield Savings",
            ToOwnAccount = true, Amount = 250m, Date = DateOnly.FromDateTime(Today.AddDays(7)), Note = "Holiday fund", Status = FintechTransferStatus.Scheduled, CreatedAt = Now.AddDays(-3),
        },
        new FintechTransfer
        {
            Reference = "TM-260412", CustomerId = SeededCustomerId, CustomerName = CustomerName, FromAccountId = "checking", ToId = "payee-rowan", ToName = "Rowan Calloway",
            Amount = 2400m, Date = DateOnly.FromDateTime(Today), Note = "Car deposit", Status = FintechTransferStatus.Held, CreatedAt = Now.AddHours(-3),
            HoldReason = "A large payment to a payee added in the last 7 days",
        },
    };

    /// <summary>
    /// About 60 disputes: the demo customer's three (one under review, one refunded, one declined), linked to their
    /// transactions, and other customers' in every state. Open disputes past <see cref="FintechState.DisputeTargetDays" /> are
    /// already escalated.
    /// </summary>
    private static List<FintechDispute> Disputes(IReadOnlyList<FintechTransaction> transactions)
    {
        var dining = transactions.Where(item => item.CategoryId == "dining" && item.Date < Today.AddDays(-8) && item.Date > Today.AddDays(-20)).OrderByDescending(item => item.Date).First();
        var shopping = transactions.Where(item => item.CategoryId == "shopping" && item.Date < Today.AddDays(-50) && item.Date > Today.AddDays(-90)).OrderByDescending(item => item.Date).First();
        var cinema = transactions.Where(item => item.CategoryId == "entertainment" && item.Description.StartsWith("Harborline", StringComparison.Ordinal)
            && item.Date < Today.AddDays(-100) && item.Date > Today.AddDays(-160)).OrderByDescending(item => item.Date).First();

        var list = new List<FintechDispute>
        {
            new()
            {
                Id = 3101, CustomerId = SeededCustomerId, TransactionId = dining.Id, Merchant = dining.Description, Amount = -dining.Amount, Reason = FintechDisputeReason.Duplicate,
                Details = "I was charged twice for the same dinner; the restaurant ran the card again after the first payment went through.",
                CreatedAt = Now.AddDays(-5).AddHours(2), Status = FintechDisputeStatus.UnderReview,
            },
            new()
            {
                Id = 3061, CustomerId = SeededCustomerId, TransactionId = shopping.Id, Merchant = shopping.Description, Amount = -shopping.Amount, Reason = FintechDisputeReason.NotReceived,
                Details = "The order never arrived, and the shop stopped answering my emails after two weeks.",
                CreatedAt = shopping.Date.AddDays(9), Status = FintechDisputeStatus.Refunded, ClosedAt = shopping.Date.AddDays(18),
                Resolution = "The merchant couldn't show the parcel was delivered, so we refunded the full amount to your card.",
            },
            new()
            {
                Id = 3047, CustomerId = SeededCustomerId, TransactionId = cinema.Id, Merchant = cinema.Description, Amount = -cinema.Amount, Reason = FintechDisputeReason.NotRecognized,
                Details = "I don't remember going to the cinema that evening and don't recognize the charge.",
                CreatedAt = cinema.Date.AddDays(6), Status = FintechDisputeStatus.Declined, ClosedAt = cinema.Date.AddDays(15),
                Resolution = "The cinema showed the tickets were bought with your card and phone, and scanned at the door. The charge stands.",
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

        var fixedIds = list.Select(dispute => dispute.Id).ToHashSet();
        var ids = Enumerable.Range(3040, 61).Where(id => !fixedIds.Contains(id)).ToList();
        var others = Customers().Where(customer => customer.Id != SeededCustomerId).ToList();
        string[] details =
        {
            "I was charged twice for the same order, a few minutes apart.",
            "The parcel never arrived and the merchant hasn't replied in two weeks.",
            "I don't recognize this charge and I've never shopped there.",
            "I was charged more than the price I agreed at the counter.",
            "I cancelled the membership last month, but they charged me again.",
        };
        string[] refunds =
        {
            "The merchant agreed the charge was a mistake, so we refunded the full amount.",
            "The merchant couldn't show the order was delivered, so we refunded it.",
        };
        string[] declines =
        {
            "The merchant showed the order was delivered and signed for, so the charge stands.",
            "The payment was made with your card and the one-time code sent to your phone, so the charge stands.",
        };

        // Oldest first, so the numbers follow the dates; ages range from a few hours to about four months.
        for (var index = 0; index < ids.Count; index++)
        {
            var age = (ids.Count - index) * 2.1 + Pick(index, 11, 20) / 10.0;
            var created = Now.AddDays(-age).AddHours(-Pick(index, 12, 9));
            var customer = others[Pick(index, 13, others.Count)];
            var reason = (FintechDisputeReason)Pick(index, 14, 5);
            var amount = 12m + Pick(index, 15, 180_000) / 100m;
            var roll = Pick(index, 16, 10);
            var status = age switch
            {
                < 7 => FintechDisputeStatus.Submitted,
                < 12 => roll < 4 ? FintechDisputeStatus.Submitted : FintechDisputeStatus.UnderReview,
                < 25 => roll < 4 ? FintechDisputeStatus.UnderReview : roll < 8 ? FintechDisputeStatus.Refunded : FintechDisputeStatus.Declined,
                _ => roll < 1 ? FintechDisputeStatus.UnderReview : roll < 6 ? FintechDisputeStatus.Refunded : FintechDisputeStatus.Declined,
            };
            var closed = status is FintechDisputeStatus.Refunded or FintechDisputeStatus.Declined ? created.AddDays(4 + Pick(index, 17, 8)).AddHours(3) : (DateTime?)null;

            if (closed > Now)
                closed = Now.AddHours(-2);

            list.Add(new FintechDispute
            {
                Id = ids[index], CustomerId = customer.Id, Merchant = disputeMerchants[Pick(index, 18, disputeMerchants.Length)], Amount = amount, Reason = reason,
                Details = details[(int)reason], CreatedAt = created, Status = status, ClosedAt = closed,
                Resolution = status == FintechDisputeStatus.Refunded ? refunds[index % refunds.Length] : status == FintechDisputeStatus.Declined ? declines[index % declines.Length] : null,
            });
        }

        // An open dispute past the answer target was escalated when it passed it.
        foreach (var dispute in list.Where(dispute => dispute.ClosedAt is null && dispute.CreatedAt.AddDays(FintechState.DisputeTargetDays) <= Now))
            dispute.EscalatedAt = dispute.CreatedAt.AddDays(FintechState.DisputeTargetDays);

        return list.OrderBy(dispute => dispute.Id).ToList();
    }

    /// <summary>Changes one transaction in a list, by ID.</summary>
    private static void Link(List<FintechTransaction> transactions, int transactionId, Func<FintechTransaction, FintechTransaction> change)
    {
        var index = transactions.FindIndex(item => item.Id == transactionId);

        if (index >= 0)
            transactions[index] = change(transactions[index]);
    }

    /// <summary>Spreads a number over 0 to <paramref name="count" /> − 1, the same for the same inputs (a small integer hash).</summary>
    private static int Pick(int value, int salt, int count)
    {
        unchecked
        {
            var hash = (uint)value * 2654435761u ^ (uint)(salt + 1) * 2246822519u;
            hash ^= hash >> 15;
            hash *= 2246822519u;
            hash ^= hash >> 13;
            hash *= 3266489917u;
            hash ^= hash >> 16;

            return (int)(hash % (uint)count);
        }
    }

    private static string Money(decimal value) => value.ToString("$#,0.00", CultureInfo.InvariantCulture);

    private static string NameOf(string customerId) => Customers().FirstOrDefault(customer => customer.Id == customerId)?.Name ?? customerId;

    /// <summary>The customer's alerts feed when the tab starts, newest first.</summary>
    private static List<FintechNotice> Notices() => new()
    {
        new() { Id = 8, At = Now.AddHours(-3), Kind = FintechAlertKind.Transfer, Title = "Transfer held for a check", Body = "Your $2,400.00 transfer to Rowan Calloway is held for a routine check. We'll update you within one business day.", Link = SiteBase + "/transfers/TM-260412" },
        new() { Id = 7, At = Now.AddDays(-1).AddHours(-4), Kind = FintechAlertKind.Budget, Title = "Dining is at 80% of its budget", Body = "You've spent most of this month's dining budget with a week to go.", Link = SiteBase + "/budgets" },
        new() { Id = 6, At = Now.AddDays(-2), Kind = FintechAlertKind.Security, Title = "New payee added", Body = "Rowan Calloway (•••• 3076) was added to your payees. If this wasn't you, contact us straight away.", Link = SiteBase + "/payees", IsRead = true },
        new() { Id = 5, At = Now.AddDays(-3), Kind = FintechAlertKind.Transfer, Title = "Transfer scheduled", Body = "$250.00 to High-Yield Savings goes on Dec 1. The money is set aside until then.", Link = SiteBase + "/transfers/TM-260415", IsRead = true },
        new() { Id = 4, At = Now.AddDays(-5).AddHours(2), Kind = FintechAlertKind.Dispute, Title = "Dispute 3101 is under review", Body = "We've contacted the merchant about the duplicate charge. Most disputes are resolved within 10 business days.", Link = SiteBase + "/disputes/3101", IsRead = true },
        new() { Id = 3, At = Now.AddDays(-9), Kind = FintechAlertKind.LargeTransaction, Title = "Large payment received", Body = "$3,150.00 from Orbitworks payroll arrived in Everyday Checking.", Link = SiteBase + "/accounts/checking", IsRead = true },
        new() { Id = 2, At = Now.AddDays(-23), Kind = FintechAlertKind.LowBalance, Title = "Everyday Checking is below $1,000.00", Body = "The balance was $812.40 after rent went out.", Link = SiteBase + "/accounts/checking", IsRead = true },
        new() { Id = 1, At = Now.AddDays(-42), Kind = FintechAlertKind.Dispute, Title = "Dispute 3061: refunded", Body = "We credited the amount back for your missing order. Open the dispute to read our decision.", Link = SiteBase + "/disputes/3061", IsRead = true },
    };

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Other customers' transfers: dozens held for a check in the review queue, dozens scheduled over the next four weeks, and
    /// a history of sent, rejected, and cancelled ones over the last four months. Only labels of their accounts are kept.
    /// </summary>
    private static List<FintechTransfer> OtherTransfers(ref int reference)
    {
        var list = new List<FintechTransfer>
        {
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
        var others = Customers().Where(customer => customer.Id != SeededCustomerId).ToList();
        string[] notes = { "Invoice 2291", "Deposit", "December rent", "Final payment", "Tuition", "Repairs", "Consulting fee" };
        string[] rejectReasons =
        {
            "We couldn't confirm the payee's details with the customer, so the payment wasn't sent.",
            "The payee's account was reported to us as used in a scam. Please contact us before paying them again.",
            "The customer told us they didn't make this payment. We stopped it and replaced their card.",
        };

        FintechTransfer Make(int index, int salt, FintechTransferStatus status)
        {
            var customer = others[Pick(index, salt, others.Count)];
            var payee = payeeNames[Pick(index, salt + 1, payeeNames.Length)];
            var account = 1000 + Pick(index, salt + 2, 9000);
            var payeeAccount = 1000 + Pick(index, salt + 3, 9000);

            return new FintechTransfer
            {
                CustomerId = customer.Id, CustomerName = customer.Name, FromAccountId = string.Create(CultureInfo.InvariantCulture, $"Checking •••• {account}"),
                ToId = string.Create(CultureInfo.InvariantCulture, $"{payee} •••• {payeeAccount}"), ToName = payee, Status = status,
                Note = Pick(index, salt + 4, 3) == 0 ? null : notes[Pick(index, salt + 5, notes.Length)],
            };
        }

        // History: sent, rejected, and cancelled over the last 120 days, oldest first.
        for (var index = 0; index < 150; index++)
        {
            var created = Now.AddDays(-120 + index * 0.79).AddHours(-Pick(index, 21, 10));
            var roll = Pick(index, 22, 20);
            var status = roll switch { 0 or 1 => FintechTransferStatus.Rejected, 2 => FintechTransferStatus.Cancelled, _ => FintechTransferStatus.Completed };
            var template = Make(index, 30, status);
            var amount = status == FintechTransferStatus.Rejected ? 2000m + Pick(index, 23, 900_000) / 100m : 40m + Pick(index, 23, 480_000) / 100m;

            list.Add(new FintechTransfer
            {
                Reference = Ref(reference++), CustomerId = template.CustomerId, CustomerName = template.CustomerName, FromAccountId = template.FromAccountId, ToId = template.ToId,
                ToName = template.ToName, Note = template.Note, Amount = amount, Date = DateOnly.FromDateTime(created), CreatedAt = created, Status = status,
                ClosedAt = status == FintechTransferStatus.Cancelled ? created.AddMinutes(20) : created.AddHours(status == FintechTransferStatus.Completed && amount < FintechState.HoldLargeAmount ? 0 : 6),
                RejectReason = status == FintechTransferStatus.Rejected ? rejectReasons[Pick(index, 24, rejectReasons.Length)] : null,
            });
        }

        // The review queue: held over the last three days, for today or a few days ahead.
        string[] holdReasons =
        {
            "A large payment to a payee",
            "A large payment to a payee added in the last 7 days",
            "A payment to a payee added in the last 7 days",
            "A payment to a payee whose name couldn't be confirmed",
        };

        for (var index = 0; index < 34; index++)
        {
            var reasonIndex = Pick(index, 41, holdReasons.Length);
            var created = Now.AddHours(-(6 + index * 2.1)).AddMinutes(-Pick(index, 42, 50));
            var template = Make(index, 50, FintechTransferStatus.Held);

            list.Add(new FintechTransfer
            {
                Reference = Ref(reference++), CustomerId = template.CustomerId, CustomerName = template.CustomerName, FromAccountId = template.FromAccountId, ToId = template.ToId,
                ToName = template.ToName, Note = template.Note, Status = FintechTransferStatus.Held, CreatedAt = created, HoldReason = holdReasons[reasonIndex],
                Amount = reasonIndex < 2 ? FintechState.HoldLargeAmount + Pick(index, 43, 1_200_000) / 100m : FintechState.HoldNewPayeeAmount + Pick(index, 43, 150_000) / 100m,
                Date = DateOnly.FromDateTime(Today.AddDays(Pick(index, 44, 4))),
            });
        }

        // Scheduled postings over the next four weeks.
        for (var index = 0; index < 38; index++)
        {
            var template = Make(index, 60, FintechTransferStatus.Scheduled);

            list.Add(new FintechTransfer
            {
                Reference = Ref(reference++), CustomerId = template.CustomerId, CustomerName = template.CustomerName, FromAccountId = template.FromAccountId, ToId = template.ToId,
                ToName = template.ToName, Note = template.Note, Status = FintechTransferStatus.Scheduled, CreatedAt = Now.AddDays(-(1 + Pick(index, 61, 10))).AddHours(-Pick(index, 62, 8)),
                Amount = 60m + Pick(index, 63, 190_000) / 100m, Date = DateOnly.FromDateTime(Today.AddDays(1 + Pick(index, 64, 28))),
            });
        }

        return list;
    }

    private static string Ref(int number) => string.Create(CultureInfo.InvariantCulture, $"TM-{number}");

    /// <summary>
    /// The staff's notifications when the tab starts, newest first. Both staff accounts that can sign in get transfers held
    /// for review, new disputes, disputes escalated past the answer target, and new high-risk AML alerts; the analyst also gets
    /// the AML cases assigned or returned to her, and the senior reviewer the cases escalated to her. Each account's newest two
    /// are unread, and so are the AML cases waiting for that person.
    /// </summary>
    private static List<ShowcaseNotification> StaffNotifications(IReadOnlyList<FintechTransfer> transfers, IReadOnlyList<FintechDispute> disputes,
        IReadOnlyList<FintechAmlAlert> alerts, IReadOnlyList<FintechAmlCase> cases)
    {
        var items = new List<(DateTime At, string Title, string Text, string Url)>();

        foreach (var transfer in transfers.Where(transfer => transfer.Status == FintechTransferStatus.Held).OrderByDescending(transfer => transfer.CreatedAt).Take(4))
            items.Add((transfer.CreatedAt, $"{transfer.Reference} is held for review", $"{Money(transfer.Amount)} from {transfer.CustomerName} to {transfer.ToName}: {LowerFirst(transfer.HoldReason!)}.", OpsBase + "/transfers/" + transfer.Reference));

        foreach (var dispute in disputes.Where(dispute => dispute.Status == FintechDisputeStatus.Submitted).OrderByDescending(dispute => dispute.CreatedAt).Take(3))
            items.Add((dispute.CreatedAt, $"New dispute {Number(dispute.Id)}", $"{NameOf(dispute.CustomerId)} disputes {Money(dispute.Amount)} at {dispute.Merchant}.", OpsBase + "/disputes/" + Number(dispute.Id)));

        foreach (var dispute in disputes.Where(dispute => dispute.EscalatedAt is not null).OrderByDescending(dispute => dispute.EscalatedAt).Take(3))
            items.Add((dispute.EscalatedAt!.Value, $"Dispute {Number(dispute.Id)} is overdue", $"Open more than {FintechState.DisputeTargetDays} days: {Money(dispute.Amount)} at {dispute.Merchant} for {NameOf(dispute.CustomerId)}.", OpsBase + "/disputes/" + Number(dispute.Id)));

        foreach (var alert in alerts.Where(alert => alert.Status == FintechAmlAlertStatus.New && alert.Risk == FintechAmlRisk.High).OrderByDescending(alert => alert.CreatedAt).Take(2))
            items.Add((alert.CreatedAt, $"New AML alert {Number(alert.Id)}", $"{RuleName(alert.Rule)} on {NameOf(alert.CustomerId)}, high risk.", $"{OpsBase}/aml/alerts/{Number(alert.Id)}"));

        var personal = new List<(string AccountId, DateTime At, string Title, string Text, string Url)>();

        foreach (var amlCase in cases.Where(amlCase => amlCase.Status != FintechAmlCaseStatus.Closed))
        {
            var url = $"{OpsBase}/aml/cases/{Number(amlCase.Id)}";
            var customer = NameOf(amlCase.CustomerId);

            if (amlCase.Status == FintechAmlCaseStatus.Escalated)
            {
                var escalated = amlCase.Events.Last(entry => entry.Kind == FintechAmlEventKind.Escalated);
                personal.Add((SeniorAccountId, escalated.At, $"Case {Number(amlCase.Id)} is escalated to you",
                    amlCase.ProposedDisposition == FintechAmlDisposition.Reported ? $"{StaffName(amlCase.EscalatedById)} proposes a SAR for {customer}; it needs your approval." : $"{StaffName(amlCase.EscalatedById)} asks for a decision on {customer}.", url));
            }
            else if (amlCase.Events.LastOrDefault(entry => entry.Kind == FintechAmlEventKind.Returned) is { } returned && amlCase.AssigneeId == OpsAccountId)
            {
                personal.Add((OpsAccountId, returned.At, $"Case {Number(amlCase.Id)} is returned to you", $"{returned.Actor}: {returned.Text}", url));
            }
            else if (amlCase.AssigneeId == OpsAccountId && amlCase.Status == FintechAmlCaseStatus.Open)
            {
                personal.Add((OpsAccountId, amlCase.OpenedAt, $"Case {Number(amlCase.Id)} is assigned to you", $"AML case on {customer}, {amlCase.Priority} priority, due {amlCase.DueOn.ToString("MMM d", CultureInfo.InvariantCulture)}.", url));
            }
        }

        var all = items.SelectMany(item => new[] { OpsAccountId, SeniorAccountId }.Select(accountId => (AccountId: accountId, item.At, item.Title, item.Text, item.Url, Personal: false)))
            .Concat(personal.Select(item => (item.AccountId, item.At, item.Title, item.Text, item.Url, Personal: true)))
            .Where(item => item.At <= Now)
            .OrderByDescending(item => item.At)
            .ToList();
        var unread = new Dictionary<string, int>(StringComparer.Ordinal);

        return all.Select((item, index) => new ShowcaseNotification
        {
            Id = all.Count - index, AccountId = item.AccountId, At = item.At, Title = Clip(item.Title, 80), Text = Clip(item.Text, 200), Url = item.Url,
            IsRead = (unread[item.AccountId] = unread.GetValueOrDefault(item.AccountId) + 1) > 2 && !item.Personal,
        }).ToList();
    }

    internal static string LowerFirst(string text) => text.Length == 0 ? text : char.ToLowerInvariant(text[0]) + text[1..];

    /// <summary>Thirteen months of the demo customer's transactions, newest first; the last two days are pending.</summary>
    private static List<FintechTransaction> Transactions()
    {
        var items = new List<FintechTransaction>();
        var id = 5000;

        void Add(DateTime date, string account, string description, string category, decimal amount, bool pending = false) =>
            items.Add(new FintechTransaction(id++, date, account, description, category, amount, pending));

        string[] groceries = { "Greenhollow Market", "Fairbank Grocers", "Mossgrove Co-op" };
        string[] dining = { "Ember & Rye", "Quillstone Café", "Noodle Harbor", "Juniper Bistro" };
        string[] shopping = { "Northwind Outfitters", "Kindred Goods", "Pinecrest Books" };

        for (var daysAgo = HistoryDays - 1; daysAgo >= 0; daysAgo--)
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

            if (day == 25 && date.Month % 3 == 1)
                Add(date, "checking", "Keystone Mutual auto insurance", "bills", -312.40m, pending);

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

            if (day == 28)
                Add(date, "savings", "Interest paid", "income", Math.Round(18250m * .041m / 12, 2), pending);

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

/// <summary>A fresh tab's data, as <see cref="FintechSeedData.Build" /> makes it.</summary>
public sealed record FintechSeedSet(
    List<FintechAccount> Accounts,
    List<FintechAmlAlert> AmlAlerts,
    List<FintechAmlCase> AmlCases,
    List<ShowcaseAuditEntry> Audit,
    List<FintechBudget> Budgets,
    List<FintechCustomerNote> CustomerNotes,
    List<FintechDispute> Disputes,
    List<FintechNotice> Notices,
    List<FintechPayee> Payees,
    List<ShowcaseNotification> StaffNotifications,
    List<FintechTransaction> Transactions,
    List<FintechTransfer> Transfers);
