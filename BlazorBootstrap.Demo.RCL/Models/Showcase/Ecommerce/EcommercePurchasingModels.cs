namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;

/// <summary>A supplier the store buys stock from; purchase orders are in its currency.</summary>
/// <param name="Id">The supplier's ID, like <c>sup-03</c>.</param>
/// <param name="Name">The company name, unique in the store.</param>
/// <param name="ContactName">The person the store deals with.</param>
/// <param name="Email">Where purchase orders are sent.</param>
/// <param name="LeadTimeDays">How many days goods take to arrive after a purchase order is sent (1 to 90).</param>
/// <param name="Currency">The currency of its prices: USD, CAD, EUR, or GBP.</param>
/// <param name="PaymentTerms">When it is paid, like "Net 30".</param>
public sealed record EcommerceSupplier(string Id, string Name, string ContactName, string Email, int LeadTimeDays, string Currency, string PaymentTerms);

/// <summary>
/// A product's replenishment settings: at or below <paramref name="ReorderPoint" /> units it is flagged for reordering, and
/// a purchase order made from the flag buys <paramref name="ReorderQuantity" /> units from the preferred supplier at
/// <paramref name="UnitCost" /> (in the supplier's currency) under the supplier's own SKU.
/// </summary>
public sealed record EcommerceReplenishment(int ProductId, int ReorderPoint, int ReorderQuantity, string? SupplierId, string? SupplierSku, decimal UnitCost);

/// <summary>Where a purchase order is. Draft → Submitted → Approved (sent to the supplier) → Partly received → Received; or Cancelled.</summary>
public enum EcommercePurchaseOrderStatus
{
    /// <summary>Being prepared; lines can change freely.</summary>
    Draft,

    /// <summary>Waiting for approval.</summary>
    Submitted,

    /// <summary>Approved and sent to the supplier; nothing received yet.</summary>
    Approved,

    /// <summary>Some goods received; the rest is still outstanding.</summary>
    PartlyReceived,

    /// <summary>Everything received; closed.</summary>
    Received,

    /// <summary>Cancelled before anything was received, with a reason.</summary>
    Cancelled,
}

/// <summary>What happened to a purchase order, for its timeline.</summary>
public enum EcommercePurchaseOrderEventKind
{
    /// <summary>The draft was created.</summary>
    Created,

    /// <summary>Lines, dates, or the note changed.</summary>
    Edited,

    /// <summary>Submitted for approval.</summary>
    Submitted,

    /// <summary>Returned to draft by the approver, with a reason.</summary>
    Returned,

    /// <summary>Approved and sent to the supplier.</summary>
    Approved,

    /// <summary>Goods were received.</summary>
    Received,

    /// <summary>Cancelled, with a reason.</summary>
    Cancelled,
}

/// <summary>One step on a purchase order's timeline: when, who, what, and the details (a receipt lists its quantities).</summary>
public sealed record EcommercePurchaseOrderEvent(DateTime At, string Actor, EcommercePurchaseOrderEventKind Kind, string Text);

/// <summary>One line of a purchase order: a product, the quantity ordered and its unit cost, and how many arrived so far.</summary>
public sealed record EcommercePurchaseOrderLine(int ProductId, string Name, string? SupplierSku, int Quantity, decimal UnitCost, int Received = 0)
{
    #region Properties, Indexers

    /// <summary>Gets the quantity still to arrive.</summary>
    public int Outstanding => Math.Max(0, Quantity - Received);

    /// <summary>Gets the line's total: quantity times unit cost.</summary>
    public decimal Total => Quantity * UnitCost;

    #endregion
}

/// <summary>
/// A purchase order to a supplier. <paramref name="CreatedBy" /> is the account that raised it (approval of a large order
/// needs someone else); <paramref name="ExpectedOn" /> is when the goods should arrive; the timeline is in
/// <paramref name="Events" />.
/// </summary>
public sealed record EcommercePurchaseOrder(
    int Id,
    string SupplierId,
    IReadOnlyList<EcommercePurchaseOrderLine> Lines,
    EcommercePurchaseOrderStatus Status,
    DateTime CreatedOn,
    string CreatedBy,
    DateTime ExpectedOn,
    string? Note,
    IReadOnlyList<EcommercePurchaseOrderEvent> Events,
    string? ApprovedBy = null,
    DateTime? ApprovedOn = null,
    DateTime? ClosedOn = null,
    string? StatusReason = null)
{
    #region Properties, Indexers

    /// <summary>Gets whether goods can still arrive: approved, or partly received.</summary>
    public bool IsOpenForReceipt => Status is EcommercePurchaseOrderStatus.Approved or EcommercePurchaseOrderStatus.PartlyReceived;

    /// <summary>Gets whether the order still counts as stock on order: not received in full or cancelled.</summary>
    public bool IsOpen => Status is not (EcommercePurchaseOrderStatus.Received or EcommercePurchaseOrderStatus.Cancelled);

    /// <summary>Gets the order's number, like "PO-2026-0014".</summary>
    public string Number => FormattableString.Invariant($"PO-{CreatedOn.Year}-{Id:0000}");

    /// <summary>Gets the order's total in the supplier's currency.</summary>
    public decimal Total => Lines.Sum(line => line.Total);

    /// <summary>Gets the units ordered.</summary>
    public int Units => Lines.Sum(line => line.Quantity);

    /// <summary>Gets the units received so far.</summary>
    public int UnitsReceived => Lines.Sum(line => line.Received);

    #endregion
}

/// <summary>What a purchasing action did, in words the page maps to a message or a field.</summary>
public enum EcommercePurchaseResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>Nothing changed: it was already like that.</summary>
    NoChange,

    /// <summary>This person may not do this (the back office only, or an approval that needs someone else).</summary>
    NotAllowed,

    /// <summary>The record is gone.</summary>
    NotFound,

    /// <summary>The order isn't in a status this action applies to.</summary>
    WrongState,

    /// <summary>A value is missing, out of range, or repeated.</summary>
    Invalid,

    /// <summary>The name is already used by another record.</summary>
    Duplicate,
}
