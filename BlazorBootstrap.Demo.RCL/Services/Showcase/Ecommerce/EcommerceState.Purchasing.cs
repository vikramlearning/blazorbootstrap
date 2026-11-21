using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// Stock replenishment by purchase order (AC-7g), the Commerce Back Office's signature workflow. Products at or below their
/// reorder point are flagged (with what is already on order, so nothing is ordered twice); the back office turns flagged
/// products into one draft purchase order per preferred supplier, edits and submits it, and an approver approves it (at or
/// below <see cref="EcommerceSeedData.ApprovalThreshold" /> anyone in the back office, their own orders included; above it
/// someone other than the creator, with the purchasing manager on one side); received goods, in full or partly, go into
/// stock at once. Every step is audited and raises a notification where someone must act: products falling below their
/// reorder point (batched), an order waiting for approval, an order approved or returned, goods received by someone
/// else, and an order overdue (reminded at sign-in).
/// </summary>
public sealed partial class EcommerceState
{
    #region Fields and Constants

    /// <summary>The longest purchase-order note.</summary>
    public const int MaxPurchaseOrderNoteLength = 300;

    /// <summary>The most lines one purchase order holds.</summary>
    public const int MaxPurchaseOrderLines = 50;

    /// <summary>The largest quantity of one purchase-order line.</summary>
    public const int MaxPurchaseOrderQuantity = 10_000;

    /// <summary>The longest lead time a supplier can have, in days.</summary>
    public const int MaxLeadTimeDays = 90;

    /// <summary>The highest reorder point.</summary>
    public const int MaxReorderPoint = 1000;

    /// <summary>The largest reorder quantity.</summary>
    public const int MaxReorderQuantity = 5000;

    /// <summary>The longest supplier name.</summary>
    public const int MaxSupplierNameLength = 60;

    /// <summary>The longest supplier SKU.</summary>
    public const int MaxSupplierSkuLength = 30;

    private const int MaxPurchaseOrderEvents = 100;

    private const int MaxPurchaseOrders = 2000;

    private const int MaxSuppliers = 100;

    /// <summary>Products that fell to their reorder point during the current change; one notification covers them all.</summary>
    private readonly List<int> lowStockCrossings = new();

    private readonly List<EcommercePurchaseOrder> purchaseOrders = seed.Value.PurchaseOrders.ToList();

    private readonly List<EcommerceReplenishment> replenishment = seed.Value.Replenishment.ToList();

    private readonly List<EcommerceSupplier> suppliers = seed.Value.Suppliers.ToList();

    private int nextPurchaseOrderId = seed.Value.PurchaseOrders.Count == 0 ? 1 : seed.Value.PurchaseOrders.Max(order => order.Id) + 1;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Approves a submitted purchase order and sends it to the supplier, when <see cref="CanApprove" /> allows it. An
    /// expected date that has passed meanwhile moves to today plus the supplier's lead time. The creator is told when
    /// someone else approved it.
    /// </summary>
    public EcommercePurchaseResult ApprovePurchaseOrder(int purchaseOrderId)
    {
        if (!IsStaff)
            return EcommercePurchaseResult.NotAllowed;

        if (FindPurchaseOrder(purchaseOrderId) is not { } order)
            return EcommercePurchaseResult.NotFound;

        if (order.Status != EcommercePurchaseOrderStatus.Submitted)
            return EcommercePurchaseResult.WrongState;

        if (!CanApprove(order))
            return EcommercePurchaseResult.NotAllowed;

        var supplier = FindSupplier(order.SupplierId);
        var expected = order.ExpectedOn.Date < Clock.Today ? Clock.Today.AddDays(supplier?.LeadTimeDays ?? 14) : order.ExpectedOn;
        var approved = order with
        {
            Status = EcommercePurchaseOrderStatus.Approved,
            ApprovedBy = Actor,
            ApprovedOn = Clock.Now,
            ExpectedOn = expected,
            StatusReason = null,
            Events = AppendEvent(order, EcommercePurchaseOrderEventKind.Approved, $"Approved and sent to {supplier?.Name ?? "the supplier"}; expected {Day(expected)}."),
        };

        ReplacePurchaseOrder(approved);
        Record("Purchase order approved", PurchaseOrderTarget(approved), EcommerceSeedData.AdminPurchaseOrderPath(order.Id),
            $"{MoneyIn(approved)} · {(NeedsSecondApprover(order) ? "over" : "within")} the {EcommerceSeedData.Money(EcommerceSeedData.ApprovalThreshold)} limit · expected {Day(expected)}.");
        Notify(order.CreatedBy, $"{order.Number} was approved", $"{Actor} approved it; {supplier?.Name ?? "the supplier"} is to deliver it on {Day(expected)}.",
            EcommerceSeedData.AdminPurchaseOrderPath(order.Id));
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>
    /// Returns whether the signed-in person may approve a submitted purchase order: anyone in the back office at or below
    /// <see cref="EcommerceSeedData.ApprovalThreshold" /> (US dollars); above it someone other than its creator, and the
    /// purchasing manager unless the purchasing manager raised it (then the retailer approves).
    /// </summary>
    public bool CanApprove(EcommercePurchaseOrder order)
    {
        if (!IsStaff || order.Status != EcommercePurchaseOrderStatus.Submitted || Identity.Current is not { } account)
            return false;

        if (!NeedsSecondApprover(order))
            return true;

        var creatorRole = Identity.FindAccount(order.CreatedBy)?.RoleId;

        return account.Id != order.CreatedBy && (account.RoleId == EcommerceSeedData.PurchasingRole || creatorRole == EcommerceSeedData.PurchasingRole);
    }

    /// <summary>
    /// Cancels an open purchase order that nothing was received on yet, with a reason (<see cref="MinReasonLength" /> to
    /// <see cref="MaxDecisionNoteLength" /> characters); its quantities no longer count as on order.
    /// </summary>
    public EcommercePurchaseResult CancelPurchaseOrder(int purchaseOrderId, string? reason)
    {
        if (!IsStaff)
            return EcommercePurchaseResult.NotAllowed;

        if (FindPurchaseOrder(purchaseOrderId) is not { } order)
            return EcommercePurchaseResult.NotFound;

        if (!order.IsOpen || order.UnitsReceived > 0)
            return EcommercePurchaseResult.WrongState;

        if (Clean(reason, MaxDecisionNoteLength) is not { Length: >= MinReasonLength } why)
            return EcommercePurchaseResult.Invalid;

        var cancelled = order with
        {
            Status = EcommercePurchaseOrderStatus.Cancelled,
            ClosedOn = Clock.Now,
            StatusReason = why,
            Events = AppendEvent(order, EcommercePurchaseOrderEventKind.Cancelled, why),
        };

        ReplacePurchaseOrder(cancelled);
        Record("Purchase order cancelled", PurchaseOrderTarget(cancelled), EcommerceSeedData.AdminPurchaseOrderPath(order.Id), $"Was {PurchaseOrderStatusWord(order.Status)}. {why}");
        Notify(order.CreatedBy, $"{order.Number} was cancelled", $"{Actor}: {why}", EcommerceSeedData.AdminPurchaseOrderPath(order.Id));
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>Creates an empty draft purchase order to a supplier, expected after the supplier's lead time; lines are added in its editor.</summary>
    public EcommercePurchaseResult CreatePurchaseOrder(string supplierId, out EcommercePurchaseOrder? created)
    {
        created = null;

        if (!IsStaff || Identity.Current is not { } account)
            return EcommercePurchaseResult.NotAllowed;

        if (FindSupplier(supplierId) is not { } supplier)
            return EcommercePurchaseResult.NotFound;

        if (purchaseOrders.Count >= MaxPurchaseOrders || nextPurchaseOrderId >= NextIdLimit)
            return EcommercePurchaseResult.Invalid;

        created = NewDraft(account.Id, supplier, new List<EcommercePurchaseOrderLine>(), "Raised as an empty draft.");
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>
    /// Turns products flagged for reordering into draft purchase orders, one per preferred supplier, each line the product's
    /// reorder quantity at its unit cost. Products without a preferred supplier, already on order, or no longer at or below
    /// their reorder point are left out and counted.
    /// </summary>
    public (EcommercePurchaseResult Result, IReadOnlyList<EcommercePurchaseOrder> Created, int NoSupplier, int OnOrder, int NotLow) CreatePurchaseOrdersFromLowStock(
        IReadOnlyCollection<int> productIds)
    {
        if (!IsStaff || Identity.Current is not { } account)
            return (EcommercePurchaseResult.NotAllowed, Array.Empty<EcommercePurchaseOrder>(), 0, 0, 0);

        var picked = (productIds ?? Array.Empty<int>()).Distinct().Select(FindProduct).Where(product => product is not null).Select(product => product!).ToList();

        if (picked.Count == 0)
            return (EcommercePurchaseResult.Invalid, Array.Empty<EcommercePurchaseOrder>(), 0, 0, 0);

        var notLow = picked.Count(product => !IsLowStock(product));
        var onOrder = picked.Count(product => IsLowStock(product) && OnOrderFor(product.Id) > 0);
        var ready = picked.Where(product => IsLowStock(product) && OnOrderFor(product.Id) == 0).ToList();
        var noSupplier = ready.Count(product => FindSupplier(ReplenishmentFor(product.Id).SupplierId) is null);
        var created = new List<EcommercePurchaseOrder>();

        foreach (var group in ready.Where(product => FindSupplier(ReplenishmentFor(product.Id).SupplierId) is not null)
                     .GroupBy(product => ReplenishmentFor(product.Id).SupplierId!).OrderBy(group => FindSupplier(group.Key)!.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (purchaseOrders.Count >= MaxPurchaseOrders || nextPurchaseOrderId >= NextIdLimit)
                break;

            var supplier = FindSupplier(group.Key)!;
            var lines = group.Take(MaxPurchaseOrderLines).Select(product =>
            {
                var setting = ReplenishmentFor(product.Id);

                return new EcommercePurchaseOrderLine(product.Id, product.Name, setting.SupplierSku, setting.ReorderQuantity, setting.UnitCost > 0 ? setting.UnitCost : Math.Round(product.Price * .45m, 2));
            }).ToList();

            created.Add(NewDraft(account.Id, supplier, lines, $"Raised from the reorder list with {LinesText(lines)}, {EcommerceSeedData.MoneyIn(lines.Sum(line => line.Total), supplier.Currency)}."));
        }

        if (created.Count > 0)
            RaiseChanged();

        return (created.Count == 0 ? EcommercePurchaseResult.NoChange : EcommercePurchaseResult.Ok, created, noSupplier, onOrder, notLow);
    }

    /// <summary>Finds a purchase order by its ID, or returns <see langword="null" />.</summary>
    public EcommercePurchaseOrder? FindPurchaseOrder(int purchaseOrderId) => purchaseOrders.FirstOrDefault(order => order.Id == purchaseOrderId);

    /// <summary>Finds a supplier by its ID, or returns <see langword="null" />.</summary>
    public EcommerceSupplier? FindSupplier(string? supplierId) => supplierId is null ? null : suppliers.FirstOrDefault(supplier => supplier.Id == supplierId);

    /// <summary>
    /// Returns whether a product is running low: at or below its reorder point (the store-wide default for a product without
    /// replenishment settings). Low products show on the reorder list.
    /// </summary>
    public bool IsLowStock(EcommerceProduct product) => product.Stock <= ReplenishmentFor(product.Id).ReorderPoint;

    /// <summary>Returns whether a purchase order is overdue: approved or partly received, and its expected date has passed.</summary>
    public bool IsOverdue(EcommercePurchaseOrder order) => order.IsOpenForReceipt && order.ExpectedOn.Date < Clock.Today;

    /// <summary>Returns whether an email address has the right form: something@domain.tld, up to 120 characters.</summary>
    public static bool IsSupplierEmail(string? value) => value is { Length: > 0 and <= ShowcaseIdentity.MaxEmailLength } && SupplierEmailPattern().IsMatch(value.Trim());

    /// <summary>Returns whether another supplier has this name, ignoring case and surrounding spaces; the supplier being edited doesn't count.</summary>
    public bool IsSupplierNameTaken(string? name, string? exceptSupplierId = null) =>
        !string.IsNullOrWhiteSpace(name) && suppliers.Any(supplier => supplier.Id != exceptSupplierId && string.Equals(supplier.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns a purchase order's total in its supplier's currency, like "€1,454.40".</summary>
    public string MoneyIn(EcommercePurchaseOrder order) => EcommerceSeedData.MoneyIn(order.Total, FindSupplier(order.SupplierId)?.Currency);

    /// <summary>Returns whether a purchase order is over the approval threshold, so it needs a second person to approve it.</summary>
    public bool NeedsSecondApprover(EcommercePurchaseOrder order) => TotalUsd(order) > EcommerceSeedData.ApprovalThreshold;

    /// <summary>Returns how many units of a product are on open purchase orders (drafts included) and haven't arrived yet.</summary>
    public int OnOrderFor(int productId) =>
        purchaseOrders.Where(order => order.IsOpen).SelectMany(order => order.Lines).Where(line => line.ProductId == productId).Sum(line => line.Outstanding);

    /// <summary>Returns a purchase-order status in words for a sentence, like "partly received".</summary>
    public static string PurchaseOrderStatusWord(EcommercePurchaseOrderStatus status) => status switch
    {
        EcommercePurchaseOrderStatus.Draft => "a draft",
        EcommercePurchaseOrderStatus.Submitted => "waiting for approval",
        EcommercePurchaseOrderStatus.Approved => "approved",
        EcommercePurchaseOrderStatus.PartlyReceived => "partly received",
        EcommercePurchaseOrderStatus.Received => "received",
        _ => "cancelled",
    };

    /// <summary>
    /// Records goods received on an approved or partly received purchase order: for each product, a quantity from 0 to what
    /// is outstanding (at least one above 0; more than outstanding is refused). The quantities go into stock at once (each
    /// change audited with stock before and after, and customers waiting for a sold-out product told it's back); the order
    /// becomes partly received, or received when nothing is outstanding.
    /// </summary>
    public EcommercePurchaseResult ReceivePurchaseOrder(int purchaseOrderId, IReadOnlyDictionary<int, int> quantities, string? note)
    {
        if (!IsStaff)
            return EcommercePurchaseResult.NotAllowed;

        if (FindPurchaseOrder(purchaseOrderId) is not { } order)
            return EcommercePurchaseResult.NotFound;

        if (!order.IsOpenForReceipt)
            return EcommercePurchaseResult.WrongState;

        var parts = new List<(EcommercePurchaseOrderLine Line, int Quantity)>();

        foreach (var (productId, quantity) in quantities ?? new Dictionary<int, int>())
        {
            if (order.Lines.FirstOrDefault(line => line.ProductId == productId) is not { } line || quantity < 0 || quantity > line.Outstanding)
                return EcommercePurchaseResult.Invalid;

            if (quantity > 0)
                parts.Add((line, quantity));
        }

        var why = Clean(note, MaxPurchaseOrderNoteLength);

        if (parts.Count == 0 || (note is not null && note.Trim().Length > MaxPurchaseOrderNoteLength))
            return EcommercePurchaseResult.Invalid;

        var lines = order.Lines.Select(line => parts.FirstOrDefault(part => part.Line.ProductId == line.ProductId) is { Line: not null } part
            ? line with { Received = line.Received + part.Quantity }
            : line).ToList();
        var complete = lines.All(line => line.Outstanding == 0);
        var text = $"Received {string.Join(", ", parts.Select(part => $"{part.Quantity} × {part.Line.Name}"))}.{(complete ? " Received in full." : " The rest is still to come.")}"
            + (why is null ? null : $" {why}");
        var received = order with
        {
            Lines = lines,
            Status = complete ? EcommercePurchaseOrderStatus.Received : EcommercePurchaseOrderStatus.PartlyReceived,
            ClosedOn = complete ? Clock.Now : null,
            Events = AppendEvent(order, EcommercePurchaseOrderEventKind.Received, Clip(text, 400)),
        };

        ReplacePurchaseOrder(received);

        foreach (var (line, quantity) in parts)
        {
            if (FindAnyProduct(line.ProductId) is not { } product)
                continue;

            var before = product.Stock;
            ChangeStock(product.Id, quantity);
            Record("Stock received", ProductTarget(product), EcommerceSeedData.AdminProductPath(product.Id),
                string.Create(CultureInfo.InvariantCulture, $"{before} → {FindAnyProduct(product.Id)?.Stock} in stock: {quantity} from {order.Number}."));
        }

        Record("Goods received", PurchaseOrderTarget(received), EcommerceSeedData.AdminPurchaseOrderPath(order.Id),
            Clip($"{string.Join(", ", parts.Select(part => $"{part.Quantity} × {part.Line.Name}"))}. {received.UnitsReceived} of {received.Units} units in; {PurchaseOrderStatusWord(received.Status)}.", 420));
        Notify(order.CreatedBy, complete ? $"{order.Number} arrived in full" : $"Part of {order.Number} arrived",
            $"{Actor} received {parts.Sum(part => part.Quantity)} units; {received.UnitsReceived} of {received.Units} are in.", EcommerceSeedData.AdminPurchaseOrderPath(order.Id));
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>Returns a product's replenishment settings, or the store-wide defaults (no supplier) when it has none yet.</summary>
    public EcommerceReplenishment ReplenishmentFor(int productId) =>
        replenishment.FirstOrDefault(item => item.ProductId == productId)
        ?? new EcommerceReplenishment(productId, EcommerceSeedData.LowStockThreshold, EcommerceSeedData.DefaultReorderQuantity, null, null, 0m);

    /// <summary>Returns the active products at or below their reorder point, for the reorder list.</summary>
    public IReadOnlyList<EcommerceProduct> ReorderList() => products.Where(product => !product.IsArchived && IsLowStock(product)).ToList();

    /// <summary>
    /// Returns a submitted purchase order to draft with a reason (<see cref="MinReasonLength" /> to
    /// <see cref="MaxDecisionNoteLength" /> characters), for its creator to change; the creator is told.
    /// </summary>
    public EcommercePurchaseResult ReturnPurchaseOrder(int purchaseOrderId, string? reason)
    {
        if (!IsStaff)
            return EcommercePurchaseResult.NotAllowed;

        if (FindPurchaseOrder(purchaseOrderId) is not { } order)
            return EcommercePurchaseResult.NotFound;

        if (order.Status != EcommercePurchaseOrderStatus.Submitted)
            return EcommercePurchaseResult.WrongState;

        if (Clean(reason, MaxDecisionNoteLength) is not { Length: >= MinReasonLength } why)
            return EcommercePurchaseResult.Invalid;

        var returned = order with { Status = EcommercePurchaseOrderStatus.Draft, StatusReason = why, Events = AppendEvent(order, EcommercePurchaseOrderEventKind.Returned, why) };

        ReplacePurchaseOrder(returned);
        Record("Purchase order returned", PurchaseOrderTarget(returned), EcommerceSeedData.AdminPurchaseOrderPath(order.Id), $"Back to draft: {why}");
        Notify(order.CreatedBy, $"{order.Number} was returned to you", $"{Actor}: {why}", EcommerceSeedData.AdminPurchaseOrderPath(order.Id));
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>
    /// Saves a draft or submitted purchase order's lines (product, quantity, unit cost), expected date, and note. The rules:
    /// 1 to 50 lines, each product once and active, quantities 1 to 10,000, unit costs $0.01 to $100,000, an expected date
    /// from today to a year ahead, and a note of up to 300 characters. A change after submitting is audited as such; an
    /// unchanged order returns <see cref="EcommercePurchaseResult.NoChange" />.
    /// </summary>
    public EcommercePurchaseResult SavePurchaseOrder(int purchaseOrderId, IReadOnlyList<(int ProductId, int Quantity, decimal UnitCost)> lines, DateTime expectedOn, string? note)
    {
        if (!IsStaff)
            return EcommercePurchaseResult.NotAllowed;

        if (FindPurchaseOrder(purchaseOrderId) is not { } order)
            return EcommercePurchaseResult.NotFound;

        if (order.Status is not (EcommercePurchaseOrderStatus.Draft or EcommercePurchaseOrderStatus.Submitted))
            return EcommercePurchaseResult.WrongState;

        if (lines is not { Count: >= 1 and <= MaxPurchaseOrderLines } || lines.Select(line => line.ProductId).Distinct().Count() != lines.Count
            || lines.Any(line => FindProduct(line.ProductId) is null || line.Quantity is < 1 or > MaxPurchaseOrderQuantity || line.UnitCost is < .01m or > 100_000m)
            || expectedOn.Date < Clock.Today || expectedOn.Date > Clock.Today.AddYears(1) || (note is not null && note.Trim().Length > MaxPurchaseOrderNoteLength))
            return EcommercePurchaseResult.Invalid;

        var newLines = lines.Select(line =>
        {
            var product = FindProduct(line.ProductId)!;
            var setting = ReplenishmentFor(product.Id);

            return new EcommercePurchaseOrderLine(product.Id, product.Name, setting.SupplierId == order.SupplierId ? setting.SupplierSku : null, line.Quantity,
                Math.Round(line.UnitCost, 2, MidpointRounding.AwayFromZero));
        }).ToList();
        var cleanNote = Clean(note, MaxPurchaseOrderNoteLength);

        if (newLines.SequenceEqual(order.Lines) && expectedOn.Date == order.ExpectedOn.Date && cleanNote == order.Note)
            return EcommercePurchaseResult.NoChange;

        var changes = PurchaseOrderChanges(order, newLines, expectedOn.Date, cleanNote);
        var saved = order with
        {
            Lines = newLines,
            ExpectedOn = expectedOn.Date,
            Note = cleanNote,
            Events = AppendEvent(order, EcommercePurchaseOrderEventKind.Edited, Clip(changes, 400)),
        };

        ReplacePurchaseOrder(saved);
        Record(order.Status == EcommercePurchaseOrderStatus.Submitted ? "Purchase order changed after submit" : "Purchase order changed", PurchaseOrderTarget(saved),
            EcommerceSeedData.AdminPurchaseOrderPath(order.Id), Clip(changes, 420));
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>
    /// Saves a product's replenishment settings: reorder point 0 to 1,000, reorder quantity 1 to 5,000 and not below the
    /// reorder point, an optional preferred supplier (with a unit cost of $0.01 to $100,000 when set), and the supplier's
    /// SKU (up to 30 letters, digits, and dashes). The change is audited.
    /// </summary>
    public EcommercePurchaseResult SaveReplenishment(int productId, int reorderPoint, int reorderQuantity, string? supplierId, string? supplierSku, decimal unitCost)
    {
        if (!IsStaff)
            return EcommercePurchaseResult.NotAllowed;

        if (FindAnyProduct(productId) is not { } product)
            return EcommercePurchaseResult.NotFound;

        var sku = Clean(supplierSku, MaxSupplierSkuLength)?.ToUpperInvariant();

        if (reorderPoint is < 0 or > MaxReorderPoint || reorderQuantity is < 1 or > MaxReorderQuantity || reorderQuantity < reorderPoint
            || (supplierId is not null && FindSupplier(supplierId) is null) || (supplierId is not null && unitCost is < .01m or > 100_000m) || unitCost is < 0 or > 100_000m
            || (sku is not null && !SupplierSkuPattern().IsMatch(sku)))
            return EcommercePurchaseResult.Invalid;

        var before = ReplenishmentFor(productId);
        var saved = new EcommerceReplenishment(productId, reorderPoint, reorderQuantity, supplierId, supplierId is null ? null : sku, supplierId is null ? 0m : Math.Round(unitCost, 2));

        if (saved == before)
            return EcommercePurchaseResult.NoChange;

        var index = replenishment.FindIndex(item => item.ProductId == productId);

        if (index < 0)
            replenishment.Add(saved);
        else
            replenishment[index] = saved;

        Record("Reorder settings changed", ProductTarget(product), EcommerceSeedData.AdminProductPath(productId), Clip(ReplenishmentChanges(before, saved), 420));
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>
    /// Adds or updates a supplier (<paramref name="input" /> with an empty ID adds one): a unique name of 2 to 60 characters,
    /// a contact name, a valid email, a lead time of 1 to 90 days, one of the supported currencies, and payment terms. The
    /// change is audited.
    /// </summary>
    public EcommercePurchaseResult SaveSupplier(EcommerceSupplier input, out EcommerceSupplier? saved)
    {
        saved = null;

        if (!IsStaff)
            return EcommercePurchaseResult.NotAllowed;

        var isNew = string.IsNullOrEmpty(input.Id);
        var existing = isNew ? null : FindSupplier(input.Id);

        if (!isNew && existing is null)
            return EcommercePurchaseResult.NotFound;

        var name = input.Name?.Trim() ?? string.Empty;

        if (IsSupplierNameTaken(name, input.Id))
            return EcommercePurchaseResult.Duplicate;

        if (name.Length is < 2 or > MaxSupplierNameLength || Clean(input.ContactName, 60) is not { Length: >= 2 } || !IsSupplierEmail(input.Email)
            || input.LeadTimeDays is < 1 or > MaxLeadTimeDays || !EcommerceSeedData.SupplierCurrencies.Contains(input.Currency)
            || !EcommerceSeedData.PaymentTermsOptions.Contains(input.PaymentTerms) || (isNew && suppliers.Count >= MaxSuppliers))
            return EcommercePurchaseResult.Invalid;

        var id = isNew ? NextSupplierId() : input.Id;
        var candidate = new EcommerceSupplier(id, name, input.ContactName.Trim(), input.Email.Trim(), input.LeadTimeDays, input.Currency, input.PaymentTerms);

        if (existing is not null && existing == candidate)
        {
            saved = existing;

            return EcommercePurchaseResult.NoChange;
        }

        if (isNew)
            suppliers.Add(candidate);
        else
            suppliers[suppliers.IndexOf(existing!)] = candidate;

        saved = candidate;
        Record(isNew ? "Supplier created" : "Supplier updated", $"Supplier {candidate.Name}", EcommerceSeedData.AdminSupplierPath(candidate.Id),
            isNew
                ? string.Create(CultureInfo.InvariantCulture, $"{candidate.ContactName} · {candidate.Email} · {candidate.LeadTimeDays} days · {candidate.Currency} · {candidate.PaymentTerms}")
                : Clip(SupplierChanges(existing!, candidate), 420));
        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>
    /// Submits a draft purchase order for approval: it needs at least one line and an expected date from today on. The people
    /// who may approve it (see <see cref="CanApprove" />), except the person submitting, are told.
    /// </summary>
    public EcommercePurchaseResult SubmitPurchaseOrder(int purchaseOrderId)
    {
        if (!IsStaff || Identity.Current is not { } account)
            return EcommercePurchaseResult.NotAllowed;

        if (FindPurchaseOrder(purchaseOrderId) is not { } order)
            return EcommercePurchaseResult.NotFound;

        if (order.Status != EcommercePurchaseOrderStatus.Draft)
            return EcommercePurchaseResult.WrongState;

        if (order.Lines.Count == 0 || order.ExpectedOn.Date < Clock.Today)
            return EcommercePurchaseResult.Invalid;

        var submitted = order with
        {
            Status = EcommercePurchaseOrderStatus.Submitted,
            StatusReason = null,
            Events = AppendEvent(order, EcommercePurchaseOrderEventKind.Submitted, $"Submitted for approval: {MoneyIn(order)}."),
        };

        ReplacePurchaseOrder(submitted);

        var second = NeedsSecondApprover(submitted);
        Record("Purchase order submitted", PurchaseOrderTarget(submitted), EcommerceSeedData.AdminPurchaseOrderPath(order.Id),
            $"{MoneyIn(submitted)} · {(second ? "over the limit: needs a second approver" : "within the limit")}.");

        var creatorRole = Identity.FindAccount(order.CreatedBy)?.RoleId;

        foreach (var approver in Identity.Accounts.Where(candidate => EcommerceSeedData.StaffRoles().Contains(candidate.RoleId)).ToList())
        {
            var eligible = !second || (approver.Id != order.CreatedBy && (approver.RoleId == EcommerceSeedData.PurchasingRole || creatorRole == EcommerceSeedData.PurchasingRole));

            if (eligible)
            {
                Notify(approver.Id, $"{order.Number} waits for your approval",
                    $"{FindSupplier(order.SupplierId)?.Name} · {MoneyIn(submitted)} · raised by {Identity.FindAccount(order.CreatedBy)?.Name ?? "someone"}."
                    + (second ? $" Over {EcommerceSeedData.Money(EcommerceSeedData.ApprovalThreshold)}, so it needs a second approver." : null),
                    EcommerceSeedData.AdminPurchaseOrderPath(order.Id));
            }
        }

        RaiseChanged();

        return EcommercePurchaseResult.Ok;
    }

    /// <summary>Returns a purchase order's total in US dollars at the fixed demo rates.</summary>
    public decimal TotalUsd(EcommercePurchaseOrder order) => EcommerceSeedData.ToUsd(order.Total, FindSupplier(order.SupplierId)?.Currency);

    #endregion

    #region Private Methods

    private static string Day(DateTime value) => value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    private static string LinesText(IReadOnlyCollection<EcommercePurchaseOrderLine> lines) =>
        string.Create(CultureInfo.InvariantCulture, $"{(lines.Count == 1 ? "1 line" : $"{lines.Count} lines")}, {lines.Sum(line => line.Quantity)} units");

    /// <summary>Names what an edit changed, for the timeline and the audit log.</summary>
    private static string PurchaseOrderChanges(EcommercePurchaseOrder before, IReadOnlyList<EcommercePurchaseOrderLine> lines, DateTime expected, string? note)
    {
        var changes = new List<string>();
        var added = lines.Where(line => before.Lines.All(old => old.ProductId != line.ProductId)).ToList();
        var removed = before.Lines.Where(old => lines.All(line => line.ProductId != old.ProductId)).ToList();

        if (added.Count > 0)
            changes.Add("added " + string.Join(", ", added.Select(line => $"{line.Quantity} × {line.Name}")));

        if (removed.Count > 0)
            changes.Add("removed " + string.Join(", ", removed.Select(line => line.Name)));

        foreach (var line in lines)
        {
            if (before.Lines.FirstOrDefault(old => old.ProductId == line.ProductId) is { } old && (old.Quantity != line.Quantity || old.UnitCost != line.UnitCost))
                changes.Add(string.Create(CultureInfo.InvariantCulture, $"{line.Name} {old.Quantity} × {old.UnitCost:0.00} → {line.Quantity} × {line.UnitCost:0.00}"));
        }

        if (expected != before.ExpectedOn.Date)
            changes.Add($"expected {Day(before.ExpectedOn)} → {Day(expected)}");

        if (note != before.Note)
            changes.Add(note is null ? "note removed" : "note changed");

        var text = changes.Count == 0 ? "Saved with no changes" : string.Join("; ", changes);

        return char.ToUpperInvariant(text[0]) + text[1..] + ".";
    }

    private static string ReplenishmentChanges(EcommerceReplenishment before, EcommerceReplenishment after)
    {
        var changes = new List<string>();

        if (before.ReorderPoint != after.ReorderPoint)
            changes.Add(string.Create(CultureInfo.InvariantCulture, $"reorder point {before.ReorderPoint} → {after.ReorderPoint}"));

        if (before.ReorderQuantity != after.ReorderQuantity)
            changes.Add(string.Create(CultureInfo.InvariantCulture, $"reorder quantity {before.ReorderQuantity} → {after.ReorderQuantity}"));

        if (before.SupplierId != after.SupplierId)
            changes.Add($"supplier {before.SupplierId ?? "none"} → {after.SupplierId ?? "none"}");

        if (before.SupplierSku != after.SupplierSku)
            changes.Add($"supplier SKU {before.SupplierSku ?? "none"} → {after.SupplierSku ?? "none"}");

        if (before.UnitCost != after.UnitCost)
            changes.Add(string.Create(CultureInfo.InvariantCulture, $"unit cost {before.UnitCost:0.00} → {after.UnitCost:0.00}"));

        var text = string.Join(", ", changes);

        return char.ToUpperInvariant(text[0]) + text[1..] + ".";
    }

    private static string SupplierChanges(EcommerceSupplier before, EcommerceSupplier after)
    {
        var changes = new List<string>();

        if (before.Name != after.Name)
            changes.Add($"name {before.Name} → {after.Name}");

        if (before.ContactName != after.ContactName)
            changes.Add($"contact {before.ContactName} → {after.ContactName}");

        if (before.Email != after.Email)
            changes.Add($"email {before.Email} → {after.Email}");

        if (before.LeadTimeDays != after.LeadTimeDays)
            changes.Add(string.Create(CultureInfo.InvariantCulture, $"lead time {before.LeadTimeDays} → {after.LeadTimeDays} days"));

        if (before.Currency != after.Currency)
            changes.Add($"currency {before.Currency} → {after.Currency}");

        if (before.PaymentTerms != after.PaymentTerms)
            changes.Add($"terms {before.PaymentTerms} → {after.PaymentTerms}");

        var text = string.Join(", ", changes);

        return char.ToUpperInvariant(text[0]) + text[1..] + ".";
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex SupplierEmailPattern();

    [GeneratedRegex("^[A-Z0-9]+(?:-[A-Z0-9]+)*$")]
    private static partial Regex SupplierSkuPattern();

    private IReadOnlyList<EcommercePurchaseOrderEvent> AppendEvent(EcommercePurchaseOrder order, EcommercePurchaseOrderEventKind kind, string text) =>
        order.Events.Append(new EcommercePurchaseOrderEvent(Clock.Now, Actor, kind, Clip(text, 400))).TakeLast(MaxPurchaseOrderEvents).ToList();

    /// <summary>
    /// Sends the one notification about the products that fell to their reorder point during the change that just ended:
    /// one product names it and links to it, several link to the reorder list. Both back-office roles hear about it, except
    /// the person who caused it.
    /// </summary>
    private void FlushLowStock()
    {
        if (lowStockCrossings.Count == 0)
            return;

        var crossed = lowStockCrossings.Select(FindAnyProduct).Where(product => product is { IsArchived: false }).Select(product => product!).ToList();
        lowStockCrossings.Clear();

        if (crossed.Count == 0)
            return;

        string title, text, link;

        if (crossed.Count == 1)
        {
            var product = crossed[0];
            var onOrder = OnOrderFor(product.Id);
            title = product.Stock == 0 ? $"Out of stock: {product.Name}" : $"Below reorder point: {product.Name}";
            text = string.Create(CultureInfo.InvariantCulture,
                $"{product.Stock} left of {product.Sku}; reorder point {ReplenishmentFor(product.Id).ReorderPoint}. {(onOrder > 0 ? $"{onOrder} already on order." : "Nothing on order yet.")}");
            link = $"{AdminBase}/reorder";
        }
        else
        {
            title = string.Create(CultureInfo.InvariantCulture, $"{crossed.Count} products fell below their reorder point");
            text = $"{string.Join(", ", crossed.Take(3).Select(product => product.Name))}{(crossed.Count > 3 ? $", and {crossed.Count - 3} more" : null)}.";
            link = $"{AdminBase}/reorder";
        }

        foreach (var account in Identity.Accounts.Where(account => EcommerceSeedData.StaffRoles().Contains(account.RoleId)).ToList())
            Notify(account.Id, title, text, link);
    }

    private EcommercePurchaseOrder NewDraft(string accountId, EcommerceSupplier supplier, List<EcommercePurchaseOrderLine> lines, string text)
    {
        var now = Clock.Now;
        var order = new EcommercePurchaseOrder(nextPurchaseOrderId++, supplier.Id, lines, EcommercePurchaseOrderStatus.Draft, now, accountId,
            Clock.Today.AddDays(supplier.LeadTimeDays), null, new List<EcommercePurchaseOrderEvent> { new(now, Actor, EcommercePurchaseOrderEventKind.Created, Clip(text, 400)) });

        purchaseOrders.Add(order);
        Record("Purchase order created", PurchaseOrderTarget(order), EcommerceSeedData.AdminPurchaseOrderPath(order.Id),
            lines.Count == 0 ? "An empty draft." : $"{LinesText(lines)}, {MoneyIn(order)}: {string.Join(", ", lines.Select(line => line.Name))}.");

        return order;
    }

    private string NextSupplierId()
    {
        var number = suppliers.Select(supplier => int.TryParse(supplier.Id.AsSpan(4), NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : 0).DefaultIfEmpty(0).Max() + 1;

        return string.Create(CultureInfo.InvariantCulture, $"sup-{number:00}");
    }

    /// <summary>A purchase order as the audit log names it: its number and supplier, like "PO-2026-0014 · Fairbank Ceramics".</summary>
    private string PurchaseOrderTarget(EcommercePurchaseOrder order) => Clip($"{order.Number} · {FindSupplier(order.SupplierId)?.Name ?? "a removed supplier"}", 120);

    /// <summary>
    /// Tells the signed-in back-office person, once per order, about the open purchase orders they raised whose expected date
    /// has passed (called at sign-in, so the bell says what needs chasing).
    /// </summary>
    private void RemindOverduePurchaseOrders()
    {
        if (!IsStaff || Identity.Current is not { } account)
            return;

        foreach (var order in purchaseOrders.Where(order => order.CreatedBy == account.Id && IsOverdue(order)).Take(5).ToList())
        {
            var title = $"{order.Number} is overdue";
            var link = EcommerceSeedData.AdminPurchaseOrderPath(order.Id);

            if (notices.Any(notice => notice.AccountId == account.Id && notice.Title == title && notice.Link == link))
                continue;

            Notify(account.Id, title, $"{FindSupplier(order.SupplierId)?.Name} was to deliver it on {Day(order.ExpectedOn)}; {order.Units - order.UnitsReceived} units still to come.",
                link, fromSystem: true);
        }
    }

    private void ReplacePurchaseOrder(EcommercePurchaseOrder order) => purchaseOrders[purchaseOrders.FindIndex(existing => existing.Id == order.Id)] = order;

    /// <summary>
    /// Rebuilds the purchasing lists from the seed and a snapshot's changes and validates them: suppliers with unique names
    /// and IDs, valid emails, lead times, currencies, and terms; replenishment settings for known products in range; and
    /// purchase orders whose lines, quantities, receipts, and status agree. Returns <see langword="false" /> to discard the
    /// snapshot.
    /// </summary>
    private bool TryRebuildPurchasing(EcommerceSnapshot snapshot, IReadOnlyDictionary<int, EcommerceProduct> productsById,
        out List<EcommerceSupplier> storedSuppliers, out List<EcommerceReplenishment> storedReplenishment, out List<EcommercePurchaseOrder> storedOrders)
    {
        storedSuppliers = new List<EcommerceSupplier>();
        storedReplenishment = new List<EcommerceReplenishment>();
        storedOrders = new List<EcommercePurchaseOrder>();
        var fresh = seed.Value;

        if (snapshot.NextPurchaseOrderId is < 1 or > NextIdLimit
            || Overlay(fresh.Suppliers, snapshot.Suppliers, snapshot.RemovedSupplierIds, supplier => supplier.Id) is not { Count: <= MaxSuppliers } suppliersList
            || Overlay(fresh.Replenishment, snapshot.Replenishment, snapshot.RemovedReplenishmentIds, item => item.ProductId) is not { } replenishmentList
            || Overlay(fresh.PurchaseOrders, snapshot.PurchaseOrders, snapshot.RemovedPurchaseOrderIds, order => order.Id) is not { Count: <= MaxPurchaseOrders } ordersList)
            return false;

        if (suppliersList.Any(supplier => supplier is null || supplier.Id is null || !Regex.IsMatch(supplier.Id, @"^sup-\d{2,4}$", RegexOptions.None, TimeSpan.FromMilliseconds(100))
                || !IsText(supplier.Name, MaxSupplierNameLength) || !IsText(supplier.ContactName, 60) || !IsSupplierEmail(supplier.Email)
                || supplier.LeadTimeDays is < 1 or > MaxLeadTimeDays || !EcommerceSeedData.SupplierCurrencies.Contains(supplier.Currency)
                || !EcommerceSeedData.PaymentTermsOptions.Contains(supplier.PaymentTerms))
            || suppliersList.Select(supplier => supplier.Id).Distinct().Count() != suppliersList.Count
            || suppliersList.Select(supplier => supplier.Name.Trim().ToUpperInvariant()).Distinct().Count() != suppliersList.Count)
            return false;

        var supplierIds = suppliersList.Select(supplier => supplier.Id).ToHashSet(StringComparer.Ordinal);

        if (replenishmentList.Any(item => item is null || !productsById.ContainsKey(item.ProductId) || item.ReorderPoint is < 0 or > MaxReorderPoint
                || item.ReorderQuantity is < 1 or > MaxReorderQuantity || item.ReorderQuantity < item.ReorderPoint
                || (item.SupplierId is not null && (!supplierIds.Contains(item.SupplierId) || item.UnitCost is < .01m or > 100_000m))
                || (item.SupplierId is null && (item.UnitCost != 0 || item.SupplierSku is not null))
                || (item.SupplierSku is not null && (item.SupplierSku.Length > MaxSupplierSkuLength || !SupplierSkuPattern().IsMatch(item.SupplierSku))))
            || replenishmentList.Select(item => item.ProductId).Distinct().Count() != replenishmentList.Count)
            return false;

        if (ordersList.Any(order => !ValidPurchaseOrder(order, productsById, supplierIds, snapshot.NextPurchaseOrderId)))
            return false;

        if (ordersList.Select(order => order.Id).Distinct().Count() != ordersList.Count)
            return false;

        storedSuppliers = suppliersList;
        storedReplenishment = replenishmentList;
        storedOrders = ordersList;

        return true;
    }

    private bool ValidPurchaseOrder(EcommercePurchaseOrder? order, IReadOnlyDictionary<int, EcommerceProduct> productsById, HashSet<string> supplierIds, int nextId)
    {
        if (order is null || order.Id < 1 || order.Id >= nextId || order.SupplierId is null || !supplierIds.Contains(order.SupplierId) || !Enum.IsDefined(order.Status)
            || !IsPlausibleDate(order.CreatedOn) || !IsPlausibleDate(order.ExpectedOn) || !IsAccountId(order.CreatedBy)
            || (order.Note is not null && !IsText(order.Note, MaxPurchaseOrderNoteLength)) || (order.ApprovedBy is not null && !IsText(order.ApprovedBy, ShowcaseIdentity.MaxNameLength))
            || (order.ApprovedOn is { } approvedOn && !IsPlausibleDate(approvedOn)) || (order.ClosedOn is { } closedOn && !IsPlausibleDate(closedOn))
            || (order.StatusReason is not null && !IsText(order.StatusReason, MaxDecisionNoteLength))
            || order.Lines is null || order.Lines.Count > MaxPurchaseOrderLines || (order.Lines.Count == 0 && order.Status != EcommercePurchaseOrderStatus.Draft)
            || order.Lines.Any(line => line is null || !productsById.ContainsKey(line.ProductId) || !IsText(line.Name, 120) || line.Quantity is < 1 or > MaxPurchaseOrderQuantity
                || line.UnitCost is < .01m or > 100_000m || line.Received < 0 || line.Received > line.Quantity
                || (line.SupplierSku is not null && line.SupplierSku.Length > MaxSupplierSkuLength))
            || order.Lines.Select(line => line.ProductId).Distinct().Count() != order.Lines.Count
            || order.Events is not { Count: >= 1 and <= MaxPurchaseOrderEvents }
            || order.Events.Any(step => step is null || !Enum.IsDefined(step.Kind) || !IsPlausibleDate(step.At) || !IsText(step.Actor, ShowcaseIdentity.MaxNameLength) || !IsText(step.Text, 400)))
            return false;

        var received = order.Lines.Sum(line => line.Received);
        var outstanding = order.Lines.Sum(line => line.Outstanding);

        return order.Status switch
        {
            EcommercePurchaseOrderStatus.Received => outstanding == 0 && order.ClosedOn is not null && order.ApprovedOn is not null,
            EcommercePurchaseOrderStatus.PartlyReceived => received > 0 && outstanding > 0 && order.ApprovedOn is not null,
            EcommercePurchaseOrderStatus.Approved => received == 0 && order.ApprovedOn is not null,
            EcommercePurchaseOrderStatus.Cancelled => received == 0 && order.StatusReason is not null,
            _ => received == 0,
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the purchase orders, oldest first.</summary>
    public IReadOnlyList<EcommercePurchaseOrder> PurchaseOrders => purchaseOrders;

    /// <summary>Gets the suppliers.</summary>
    public IReadOnlyList<EcommerceSupplier> Suppliers => suppliers;

    /// <summary>Gets the name the timeline and the audit log give the signed-in person, or the store's system.</summary>
    private string Actor => Identity.Current?.Name is { Length: > 0 } name ? Clip(name, ShowcaseIdentity.MaxNameLength) : EcommerceSeedData.SystemActor;

    #endregion
}
