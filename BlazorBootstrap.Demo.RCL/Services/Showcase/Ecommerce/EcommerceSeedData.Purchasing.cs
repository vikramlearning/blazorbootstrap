using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// The purchasing part of the Kindred Goods seed (AC-7g, stock replenishment by purchase order): ten fictitious suppliers,
/// every product's reorder point, reorder quantity, preferred supplier, supplier SKU, and unit cost, and four months of
/// purchase orders in every status, so the demo starts with work to do: products below their reorder point with nothing
/// on order, a purchase order waiting for the purchasing manager's approval, one overdue, one partly received, and a draft.
/// </summary>
public static partial class EcommerceSeedData
{
    #region Fields and Constants

    /// <summary>
    /// A purchase order above this total (in US dollars, at the fixed demo rates) needs a second person to approve it: the
    /// purchasing manager approves one someone else raised; one the purchasing manager raised is approved by the retailer.
    /// At or below it, anyone in the back office approves, their own orders included, after a confirmation.
    /// </summary>
    public const decimal ApprovalThreshold = 5000m;

    /// <summary>The demo purchasing manager's account ID.</summary>
    public const string DemoPurchasingId = "lena";

    /// <summary>The role ID of the purchasing manager.</summary>
    public const string PurchasingRole = "purchasing";

    /// <summary>The payment terms a supplier can have.</summary>
    public static readonly IReadOnlyList<string> PaymentTermsOptions = new[] { "Due on receipt", "Net 15", "Net 30", "Net 45", "Net 60" };

    /// <summary>The currencies a supplier can price in.</summary>
    public static readonly IReadOnlyList<string> SupplierCurrencies = new[] { "USD", "CAD", "EUR", "GBP" };

    /// <summary>The preferred supplier of each original product (101 to 131); its sizes and editions share it.</summary>
    private static readonly string[] supplierByOriginal =
    {
        "sup-01", "sup-02", "sup-03", "sup-04", "sup-02", "sup-01", "sup-02", "sup-05", "sup-04", "sup-06", "sup-05", "sup-04", "sup-02", "sup-07", "sup-06",
        "sup-07", "sup-07", "sup-07", "sup-07", "sup-08", "sup-08", "sup-08", "sup-06", "sup-08", "sup-08", "sup-04", "sup-09", "sup-09", "sup-04", "sup-09",
        "sup-10",
    };

    /// <summary>Each supplier's prefix for its own SKUs.</summary>
    private static readonly Dictionary<string, string> supplierSkuPrefix = new()
    {
        ["sup-01"] = "AAW", ["sup-02"] = "FBC", ["sup-03"] = "CPL", ["sup-04"] = "NLM", ["sup-05"] = "RRF",
        ["sup-06"] = "LUM", ["sup-07"] = "THO", ["sup-08"] = "DCS", ["sup-09"] = "PBC", ["sup-10"] = "WWC",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the back office's page of a purchase order.</summary>
    public static string AdminPurchaseOrderPath(int purchaseOrderId) => string.Create(CultureInfo.InvariantCulture, $"{AdminBase}/purchase-orders/{purchaseOrderId}");

    /// <summary>Returns the suppliers page with a supplier's editor open.</summary>
    public static string AdminSupplierPath(string supplierId) => $"{AdminBase}/suppliers?supplier={Uri.EscapeDataString(supplierId)}";

    /// <summary>Formats an amount in a supplier's currency, like "$1,250.00", "CA$1,250.00", "€1,250.00", or "£1,250.00".</summary>
    public static string MoneyIn(decimal amount, string? currency) => currency switch
    {
        "CAD" => amount.ToString("CA$#,0.00", CultureInfo.InvariantCulture),
        "EUR" => amount.ToString("€#,0.00", CultureInfo.InvariantCulture),
        "GBP" => amount.ToString("£#,0.00", CultureInfo.InvariantCulture),
        _ => Money(amount),
    };

    /// <summary>The fictitious suppliers the store buys from.</summary>
    public static List<EcommerceSupplier> Suppliers() => new()
    {
        new("sup-01", "Alder & Ash Woodworks", "Rosa Lindgren", "orders@alderash.example", 14, "USD", "Net 30"),
        new("sup-02", "Fairbank Ceramics", "Noah Reyes", "wholesale@fairbankceramics.example", 10, "USD", "Net 30"),
        new("sup-03", "Copperline Cookware", "Arjun Patel", "trade@copperline.example", 21, "USD", "Net 45"),
        new("sup-04", "Northwick Linen Mill", "Harriet Cole", "sales@northwicklinen.example", 28, "GBP", "Net 60"),
        new("sup-05", "Rattan & Reed Furniture", "Ines Duarte", "orders@rattanreed.example", 35, "EUR", "Net 60"),
        new("sup-06", "Lumen Lighting Supply", "Marcus Webb", "b2b@lumensupply.example", 12, "USD", "Net 30"),
        new("sup-07", "Trailhead Outdoor Goods", "Chloe Martin", "wholesale@trailhead.example", 18, "CAD", "Net 30"),
        new("sup-08", "Deskcraft Studio", "Kenji Mori", "hello@deskcraft.example", 20, "USD", "Net 15"),
        new("sup-09", "Pure Bath Co.", "Amara Osei", "trade@purebath.example", 9, "USD", "Net 30"),
        new("sup-10", "Wickworks Candles", "Tess Nolan", "orders@wickworks.example", 7, "USD", "Due on receipt"),
    };

    /// <summary>Converts an amount in a supplier's currency to US dollars at the fixed demo rates.</summary>
    public static decimal ToUsd(decimal amount, string? currency) => Math.Round(amount * (currency switch
    {
        "CAD" => .73m,
        "EUR" => 1.08m,
        "GBP" => 1.27m,
        _ => 1m,
    }), 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Every product's replenishment settings, by price: under $40 a reorder point of 12 and a reorder quantity of 48, under
    /// $120 8 and 24, otherwise 4 and 10; the original product's supplier (sizes and editions share it), the supplier's own
    /// SKU, and a unit cost of 45% of the price in the supplier's currency.
    /// </summary>
    public static List<EcommerceReplenishment> Replenishment(IReadOnlyList<EcommerceProduct> products)
    {
        var currencies = Suppliers().ToDictionary(supplier => supplier.Id, supplier => supplier.Currency);

        return products.Select(product =>
        {
            var supplierId = supplierByOriginal[(product.ImageId ?? product.Id) - EcommerceProduct.FirstLibraryPhotoId];
            var (point, quantity) = product.Price < 40m ? (12, 48) : product.Price < 120m ? (8, 24) : (4, 10);
            var cost = Math.Round(product.Price * .45m / (ToUsd(1m, currencies[supplierId])), 2, MidpointRounding.AwayFromZero);

            return new EcommerceReplenishment(product.Id, point, quantity, supplierId,
                string.Create(CultureInfo.InvariantCulture, $"{supplierSkuPrefix[supplierId]}-{1000 + (product.Id * 37 % 9000)}"), cost);
        }).ToList();
    }

    #endregion

    #region Private Methods

    /// <summary>The audit entries of the last month's purchase-order steps.</summary>
    private static IEnumerable<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)> PurchasingAudit(IReadOnlyList<EcommerceSupplier> suppliers,
        IReadOnlyList<EcommercePurchaseOrder> purchaseOrders)
    {
        var names = suppliers.ToDictionary(supplier => supplier.Id, supplier => supplier.Name);

        foreach (var order in purchaseOrders)
        {
            foreach (var step in order.Events.Where(step => step.At >= SeedTime.AddDays(-30) && step.At <= SeedTime))
            {
                yield return (step.At, step.Actor, PurchaseOrderAction(step.Kind), $"{order.Number} · {names[order.SupplierId]}", AdminPurchaseOrderPath(order.Id), step.Text);
            }
        }
    }

    /// <summary>
    /// The seeded notifications about purchasing: both back-office accounts hear that products are below their reorder point
    /// with nothing on order; the purchasing manager that a large order waits for her approval; each order's creator that it
    /// was approved, partly received, or is overdue.
    /// </summary>
    private static IEnumerable<(string AccountId, DateTime At, string Title, string Text, string Link)> PurchasingNotices(IReadOnlyList<EcommerceProduct> products,
        IReadOnlyList<EcommerceReplenishment> replenishment, IReadOnlyList<EcommerceSupplier> suppliers, IReadOnlyList<EcommercePurchaseOrder> purchaseOrders)
    {
        var settings = replenishment.ToDictionary(item => item.ProductId);
        var names = suppliers.ToDictionary(supplier => supplier.Id);
        var onOrder = purchaseOrders.Where(order => order.IsOpen).SelectMany(order => order.Lines).Where(line => line.Outstanding > 0).Select(line => line.ProductId).ToHashSet();
        var low = products.Where(product => !product.IsArchived && product.Stock <= settings[product.Id].ReorderPoint && !onOrder.Contains(product.Id)).ToList();

        if (low.Count > 0)
        {
            var text = $"{string.Join(", ", low.Take(3).Select(product => product.Name))}{(low.Count > 3 ? $", and {low.Count - 3} more" : null)}: nothing on order yet.";

            foreach (var account in new[] { DemoRetailerId, DemoPurchasingId })
                yield return (account, SeedTime.AddHours(-2), $"{low.Count} products are below their reorder point", text, $"{AdminBase}/reorder");
        }

        foreach (var order in purchaseOrders)
        {
            var supplier = names[order.SupplierId];
            var creator = order.CreatedBy;

            switch (order.Status)
            {
                case EcommercePurchaseOrderStatus.Submitted:
                    yield return (creator == DemoPurchasingId ? DemoRetailerId : DemoPurchasingId, order.Events[^1].At, $"{order.Number} waits for your approval",
                        $"{supplier.Name} · {MoneyIn(order.Total, supplier.Currency)} · raised by {ActorName(creator)}. Over {Money(ApprovalThreshold)}, so it needs a second approver.",
                        AdminPurchaseOrderPath(order.Id));
                    break;

                case EcommercePurchaseOrderStatus.Approved when order.ExpectedOn.Date < Today:
                    yield return (creator, order.ExpectedOn.Date.AddDays(1).AddHours(8), $"{order.Number} is overdue",
                        $"{supplier.Name} was to deliver it on {order.ExpectedOn.ToString("MMM d", CultureInfo.InvariantCulture)}; {order.Units - order.UnitsReceived} units still to come.",
                        AdminPurchaseOrderPath(order.Id));
                    break;

                case EcommercePurchaseOrderStatus.Approved when order.ApprovedBy is { } approver && approver != ActorName(creator):
                    yield return (creator, order.ApprovedOn!.Value, $"{order.Number} was approved",
                        $"{approver} approved it; {supplier.Name} is to deliver it on {order.ExpectedOn.ToString("MMM d", CultureInfo.InvariantCulture)}.", AdminPurchaseOrderPath(order.Id));
                    break;

                case EcommercePurchaseOrderStatus.PartlyReceived when order.Events[^1].Actor != ActorName(creator):
                    yield return (creator, order.Events[^1].At, $"Part of {order.Number} arrived",
                        $"{order.Events[^1].Actor} received {order.UnitsReceived} of {order.Units} units from {supplier.Name}.", AdminPurchaseOrderPath(order.Id));
                    break;
            }
        }
    }

    /// <summary>
    /// Four months of purchase orders, numbered by creation: eight received in full (one in two deliveries), one cancelled,
    /// one approved and due in two weeks, and, for products below their reorder point, one partly received, one approved and
    /// overdue, one waiting for the purchasing manager's approval (over the threshold), and a draft. The other products below
    /// their reorder point (Stoneware Bud Vase, on the demo customer's wishlist, among them) have nothing on order.
    /// </summary>
    private static List<EcommercePurchaseOrder> PurchaseOrders(IReadOnlyList<EcommerceProduct> products, IReadOnlyList<EcommerceSupplier> suppliers,
        IReadOnlyList<EcommerceReplenishment> replenishment)
    {
        var settings = replenishment.ToDictionary(item => item.ProductId);
        var byId = suppliers.ToDictionary(supplier => supplier.Id);
        var owen = ActorName(DemoRetailerId);
        var lena = ActorName(DemoPurchasingId);
        var orders = new List<EcommercePurchaseOrder>();
        var active = products.Where(product => !product.IsArchived).ToList();
        bool IsLow(EcommerceProduct product) => product.Stock <= settings[product.Id].ReorderPoint;

        EcommercePurchaseOrderLine Line(EcommerceProduct product, int quantity, int received = 0) =>
            new(product.Id, product.Name, settings[product.Id].SupplierSku, quantity, settings[product.Id].UnitCost, received);

        List<EcommerceProduct> Stocked(string supplierId, int skip, int take) =>
            active.Where(product => settings[product.Id].SupplierId == supplierId && !IsLow(product)).Skip(skip).Take(take).ToList();

        string Approver(string creatorId, IReadOnlyList<EcommercePurchaseOrderLine> lines, string supplierId) =>
            ToUsd(lines.Sum(line => line.Total), byId[supplierId].Currency) > ApprovalThreshold ? (creatorId == DemoPurchasingId ? owen : lena) : ActorName(creatorId);

        string Raised(IReadOnlyList<EcommercePurchaseOrderLine> lines, string supplierId) =>
            $"{(lines.Count == 1 ? "1 line" : $"{lines.Count} lines")}, {lines.Sum(line => line.Quantity)} units, {MoneyIn(lines.Sum(line => line.Total), byId[supplierId].Currency)}.";

        string Receipt(IEnumerable<(EcommercePurchaseOrderLine Line, int Quantity)> parts) =>
            "Received " + string.Join(", ", parts.Select(part => $"{part.Quantity} × {part.Line.Name}")) + ".";

        // Eight orders received in full, about two weeks apart; the third arrived in two deliveries, the second one late.
        var history = new[] { "sup-01", "sup-09", "sup-06", "sup-02", "sup-08", "sup-07", "sup-04", "sup-10" };

        for (var index = 0; index < history.Length; index++)
        {
            var supplier = byId[history[index]];
            var creatorId = index % 2 == 0 ? DemoRetailerId : DemoPurchasingId;
            var created = Today.AddDays(-(118 - (index * 12))).AddHours(9 + (index % 3));
            var lines = Stocked(supplier.Id, index % 2, 3).Select(product => Line(product, settings[product.Id].ReorderQuantity, settings[product.Id].ReorderQuantity)).ToList();
            var approver = Approver(creatorId, lines, supplier.Id);
            var approved = created.AddHours(4);
            var expected = approved.Date.AddDays(supplier.LeadTimeDays);
            var events = new List<EcommercePurchaseOrderEvent>
            {
                new(created, ActorName(creatorId), EcommercePurchaseOrderEventKind.Created, $"Raised with {Raised(lines, supplier.Id)}"),
                new(created.AddMinutes(20), ActorName(creatorId), EcommercePurchaseOrderEventKind.Submitted, $"Submitted for approval: {MoneyIn(lines.Sum(line => line.Total), supplier.Currency)}."),
                new(approved, approver, EcommercePurchaseOrderEventKind.Approved, $"Approved and sent to {supplier.Name}; expected {expected.ToString("MMM d", CultureInfo.InvariantCulture)}."),
            };
            DateTime closed;

            if (index == 2)
            {
                var first = expected.AddDays(-2).AddHours(10);
                closed = expected.AddDays(1).AddHours(15);
                events.Add(new(first, owen, EcommercePurchaseOrderEventKind.Received, Receipt(lines.Select(line => (line, line.Quantity / 2))) + " The rest is still to come."));
                events.Add(new(closed, owen, EcommercePurchaseOrderEventKind.Received, Receipt(lines.Select(line => (line, line.Quantity - (line.Quantity / 2)))) + " Received in full."));
            }
            else
            {
                closed = expected.AddDays(-1).AddHours(10 + index);
                events.Add(new(closed, index % 3 == 0 ? lena : owen, EcommercePurchaseOrderEventKind.Received, Receipt(lines.Select(line => (line, line.Quantity))) + " Received in full."));
            }

            orders.Add(new EcommercePurchaseOrder(0, supplier.Id, lines, EcommercePurchaseOrderStatus.Received, created, creatorId, expected, null, events,
                approver, approved, closed));
        }

        // Cancelled after approval, before anything arrived.
        {
            var supplier = byId["sup-05"];
            var created = Today.AddDays(-40).AddHours(11);
            var lines = Stocked(supplier.Id, 0, 2).Select(product => Line(product, settings[product.Id].ReorderQuantity)).ToList();
            var approver = Approver(DemoRetailerId, lines, supplier.Id);
            const string reason = "Rattan & Reed can't deliver before the holidays; we're ordering these from stock we already hold.";

            orders.Add(new EcommercePurchaseOrder(0, supplier.Id, lines, EcommercePurchaseOrderStatus.Cancelled, created, DemoRetailerId,
                created.Date.AddDays(1 + supplier.LeadTimeDays), null,
                new List<EcommercePurchaseOrderEvent>
                {
                    new(created, owen, EcommercePurchaseOrderEventKind.Created, $"Raised with {Raised(lines, supplier.Id)}"),
                    new(created.AddMinutes(15), owen, EcommercePurchaseOrderEventKind.Submitted, $"Submitted for approval: {MoneyIn(lines.Sum(line => line.Total), supplier.Currency)}."),
                    new(created.AddDays(1), approver, EcommercePurchaseOrderEventKind.Approved, $"Approved and sent to {supplier.Name}."),
                    new(created.AddDays(3), owen, EcommercePurchaseOrderEventKind.Cancelled, reason),
                },
                approver, created.AddDays(1), created.AddDays(3), reason));
        }

        // Approved and on its way, due in about two weeks.
        {
            var supplier = byId["sup-03"];
            var created = Today.AddDays(-6).AddHours(14);
            var lines = Stocked(supplier.Id, 0, 2).Select(product => Line(product, settings[product.Id].ReorderQuantity)).ToList();
            var expected = created.Date.AddDays(1 + supplier.LeadTimeDays);

            orders.Add(new EcommercePurchaseOrder(0, supplier.Id, lines, EcommercePurchaseOrderStatus.Approved, created, DemoRetailerId, expected, "Holiday stock for the cookware shelf.",
                new List<EcommercePurchaseOrderEvent>
                {
                    new(created, owen, EcommercePurchaseOrderEventKind.Created, $"Raised with {Raised(lines, supplier.Id)}"),
                    new(created.AddMinutes(10), owen, EcommercePurchaseOrderEventKind.Submitted, $"Submitted for approval: {MoneyIn(lines.Sum(line => line.Total), supplier.Currency)}."),
                    new(created.AddDays(1).AddHours(-4), lena, EcommercePurchaseOrderEventKind.Approved, $"Approved and sent to {supplier.Name}; expected {expected.ToString("MMM d", CultureInfo.InvariantCulture)}."),
                },
                lena, created.AddDays(1).AddHours(-4)));
        }

        // The low-stock products' suppliers, the longest lead time first; the Stoneware Bud Vase (113) stays off every order.
        var groups = active.Where(product => IsLow(product) && product.Id != 113)
            .GroupBy(product => settings[product.Id].SupplierId!)
            .OrderByDescending(group => byId[group.Key].LeadTimeDays).ThenBy(group => group.Key, StringComparer.Ordinal)
            .ToList();

        if (groups.Count > 0)
        {
            // Partly received: the stocked line arrived; the low products' lines are still to come.
            var supplier = byId[groups[0].Key];
            var created = Today.AddDays(-Math.Max(8, supplier.LeadTimeDays - 4)).AddHours(10);
            var extra = Stocked(supplier.Id, 0, 1);
            var lines = groups[0].Take(2).Select(product => Line(product, settings[product.Id].ReorderQuantity))
                .Concat(extra.Select(product => Line(product, settings[product.Id].ReorderQuantity, settings[product.Id].ReorderQuantity))).ToList();
            var approver = Approver(DemoPurchasingId, lines, supplier.Id);
            var expected = created.Date.AddDays(supplier.LeadTimeDays);
            var received = Today.AddDays(-3).AddHours(11);

            orders.Add(new EcommercePurchaseOrder(0, supplier.Id, lines, EcommercePurchaseOrderStatus.PartlyReceived, created, DemoPurchasingId, expected, null,
                new List<EcommercePurchaseOrderEvent>
                {
                    new(created, lena, EcommercePurchaseOrderEventKind.Created, $"Raised from the reorder list with {Raised(lines, supplier.Id)}"),
                    new(created.AddMinutes(25), lena, EcommercePurchaseOrderEventKind.Submitted, $"Submitted for approval: {MoneyIn(lines.Sum(line => line.Total), supplier.Currency)}."),
                    new(created.AddHours(3), approver, EcommercePurchaseOrderEventKind.Approved, $"Approved and sent to {supplier.Name}; expected {expected.ToString("MMM d", CultureInfo.InvariantCulture)}."),
                    new(received, owen, EcommercePurchaseOrderEventKind.Received, Receipt(lines.Where(line => line.Received > 0).Select(line => (line, line.Received))) + " The rest is still to come."),
                },
                approver, created.AddHours(3)));
        }

        if (groups.Count > 1)
        {
            // Approved and overdue: due five days ago, nothing arrived.
            var supplier = byId[groups[1].Key];
            var created = Today.AddDays(-(supplier.LeadTimeDays + 5)).AddHours(9);
            var lines = groups[1].Take(3).Select(product => Line(product, settings[product.Id].ReorderQuantity)).ToList();
            var approver = Approver(DemoRetailerId, lines, supplier.Id);
            var expected = created.Date.AddDays(supplier.LeadTimeDays);

            orders.Add(new EcommercePurchaseOrder(0, supplier.Id, lines, EcommercePurchaseOrderStatus.Approved, created, DemoRetailerId, expected, null,
                new List<EcommercePurchaseOrderEvent>
                {
                    new(created, owen, EcommercePurchaseOrderEventKind.Created, $"Raised from the reorder list with {Raised(lines, supplier.Id)}"),
                    new(created.AddMinutes(10), owen, EcommercePurchaseOrderEventKind.Submitted, $"Submitted for approval: {MoneyIn(lines.Sum(line => line.Total), supplier.Currency)}."),
                    new(created.AddHours(2), approver, EcommercePurchaseOrderEventKind.Approved, $"Approved and sent to {supplier.Name}; expected {expected.ToString("MMM d", CultureInfo.InvariantCulture)}."),
                },
                approver, created.AddHours(2)));
        }

        if (groups.Count > 2)
        {
            // Waiting for the purchasing manager: the retailer raised it yesterday, over the threshold.
            var supplier = byId[groups[2].Key];
            var created = Today.AddDays(-1).AddHours(10).AddMinutes(30);
            var lines = groups[2].Take(3).Select(product => Line(product, settings[product.Id].ReorderQuantity)).ToList();
            var usd = ToUsd(lines.Sum(line => line.Total), supplier.Currency);

            if (usd <= ApprovalThreshold)
            {
                var factor = (int)Math.Ceiling((ApprovalThreshold + 600m) / Math.Max(1m, usd));
                lines = lines.Select(line => line with { Quantity = line.Quantity * factor }).ToList();
            }

            orders.Add(new EcommercePurchaseOrder(0, supplier.Id, lines, EcommercePurchaseOrderStatus.Submitted, created, DemoRetailerId,
                created.Date.AddDays(1 + supplier.LeadTimeDays), "Stocking up for the holiday weeks.",
                new List<EcommercePurchaseOrderEvent>
                {
                    new(created, owen, EcommercePurchaseOrderEventKind.Created, $"Raised from the reorder list with {Raised(lines, supplier.Id)}"),
                    new(created.AddMinutes(40), owen, EcommercePurchaseOrderEventKind.Submitted, $"Submitted for approval: {MoneyIn(lines.Sum(line => line.Total), supplier.Currency)}."),
                }));
        }

        if (groups.Count > 3)
        {
            // A draft the retailer started this morning from the reorder list.
            var supplier = byId[groups[3].Key];
            var created = SeedTime.AddHours(-1);
            var lines = groups[3].Take(2).Select(product => Line(product, settings[product.Id].ReorderQuantity)).ToList();

            orders.Add(new EcommercePurchaseOrder(0, supplier.Id, lines, EcommercePurchaseOrderStatus.Draft, created, DemoRetailerId,
                created.Date.AddDays(1 + supplier.LeadTimeDays), null,
                new List<EcommercePurchaseOrderEvent> { new(created, owen, EcommercePurchaseOrderEventKind.Created, $"Raised from the reorder list with {Raised(lines, supplier.Id)}") }));
        }

        return orders.OrderBy(order => order.CreatedOn).Select((order, index) => order with { Id = index + 1 }).ToList();
    }

    /// <summary>Returns the audit log's word for a purchase-order step.</summary>
    internal static string PurchaseOrderAction(EcommercePurchaseOrderEventKind kind) => kind switch
    {
        EcommercePurchaseOrderEventKind.Created => "Purchase order created",
        EcommercePurchaseOrderEventKind.Edited => "Purchase order changed",
        EcommercePurchaseOrderEventKind.Submitted => "Purchase order submitted",
        EcommercePurchaseOrderEventKind.Returned => "Purchase order returned",
        EcommercePurchaseOrderEventKind.Approved => "Purchase order approved",
        EcommercePurchaseOrderEventKind.Received => "Goods received",
        _ => "Purchase order cancelled",
    };

    private static string ActorName(string accountId) => DemoAccounts().FirstOrDefault(account => account.Id == accountId)?.Name ?? SystemActor;

    #endregion

    #endregion
}
