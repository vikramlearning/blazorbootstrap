using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// The enterprise baseline of <see cref="EcommerceState" /> (rule WR-001): the audit log, the header's notifications (one
/// list per account: for the customer, the same notices as the account overview, so the bell and the page always agree;
/// for the retailer, notices raised where someone must act: an order to fulfil, a return to decide, low or no stock, a
/// review to moderate), the header's global search, the lists' bulk actions (orders, products, returns, reviews), and the
/// checks the forms ask while someone types (SKUs, coupon codes, address labels). Audit entries and notifications link
/// only to pages on this site.
/// </summary>
public sealed partial class EcommerceState
{
    #region Fields and Constants

    /// <summary>The longest text alternative of a product photo.</summary>
    public const int MaxImageAltLength = 160;

    /// <summary>The back office's base route (the same as <c>EcommerceAdminLayout.Base</c>).</summary>
    private const string AdminBase = EcommerceSeedData.AdminBase;

    private const string OffersPath = AdminBase + "/offers";

    private const string ReviewsPath = AdminBase + "/reviews";

    /// <summary>The site's base route (the same as <c>EcommerceLayout.Base</c>).</summary>
    private const string SiteBase = EcommerceSeedData.SiteBase;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Moves several orders one step on from the order list (the retailer), each like <see cref="AdvanceOrder" />: placed
    /// ones to packed, packed ones to shipped with the carrier's label (its tracking number), or shipped ones to delivered,
    /// as <paramref name="from" /> says. Orders in another status are left as they are; each customer hears about their
    /// order.
    /// </summary>
    public EcommerceBulkOutcome BulkAdvanceOrders(IReadOnlyCollection<int> orderIds, EcommerceOrderStatus from) =>
        Bulk(orderIds, id => FindOrder(id) is { } order && order.Status == from
                && AdvanceOrder(id, from == EcommerceOrderStatus.Packed ? EcommerceSeedData.TrackingNumberFor(id) : null),
            NextStatus(from) is null);

    /// <summary>
    /// Approves several waiting returns at once (the retailer), each like <see cref="ApproveReturn" /> with its full refund
    /// (up to what's left of its order) and one optional note. Returns no longer waiting, or whose order was refunded in the
    /// meantime, are left as they are.
    /// </summary>
    public EcommerceBulkOutcome BulkApproveReturns(IReadOnlyCollection<int> returnIds, string? note) =>
        Bulk(returnIds, id => FindReturn(id) is { Status: EcommerceReturnStatus.Requested } item && FindOrder(item.OrderId) is { } order
                && MaxRefundFor(item, order) > 0 && ApproveReturn(id, note, MaxRefundFor(item, order) < item.Amount ? null : item.Amount),
            note?.Trim().Length > MaxDecisionNoteLength);

    /// <summary>
    /// Rejects several waiting returns at once (the retailer), each like <see cref="RejectReturn" />, with one reason the
    /// customers read (<see cref="MinReasonLength" /> to <see cref="MaxDecisionNoteLength" /> characters).
    /// </summary>
    public EcommerceBulkOutcome BulkRejectReturns(IReadOnlyCollection<int> returnIds, string reason) =>
        Bulk(returnIds, id => RejectReturn(id, reason), reason?.Trim().Length is not (>= MinReasonLength and <= MaxDecisionNoteLength));

    /// <summary>Archives (unpublishes) or restores (publishes) several products at once from the product list (the retailer).</summary>
    public EcommerceBulkOutcome BulkSetArchived(IReadOnlyCollection<int> productIds, bool archived) =>
        Bulk(productIds, id => SetArchived(id, archived));

    /// <summary>Moves several products to one category at once from the product list (the retailer); each move is audited.</summary>
    public EcommerceBulkOutcome BulkSetCategory(IReadOnlyCollection<int> productIds, string categoryId) =>
        Bulk(productIds, id =>
        {
            if (FindAnyProduct(id) is not { } product || FindCategory(categoryId) is not { } category || product.CategoryId == category.Id)
                return false;

            var before = FindCategory(product.CategoryId)?.Name ?? product.CategoryId;
            ReplaceProduct(product with { CategoryId = category.Id });
            Record("Category changed", ProductTarget(product), EcommerceSeedData.AdminProductPath(product.Id), $"{before} → {category.Name}, from the product list.");

            return true;
        }, FindCategory(categoryId) is null);

    /// <summary>Publishes or hides several reviews at once from the review list (the retailer).</summary>
    public EcommerceBulkOutcome BulkSetReviewStatus(IReadOnlyCollection<int> reviewIds, EcommerceReviewStatus status) =>
        Bulk(reviewIds, id => SetReviewStatus(id, status), !Enum.IsDefined(status));

    /// <summary>
    /// Returns the signed-in account's notifications for the header's menu, newest first: the account's notices (for the
    /// customer, the same list, IDs, and read state as the account overview). Empty when nobody is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Identity.Current is not { } account
            ? Array.Empty<ShowcaseNotification>()
            : NoticesFor(account.Id).Select(notice => new ShowcaseNotification
            {
                Id = notice.Id,
                AccountId = notice.AccountId,
                At = notice.CreatedOn,
                IsRead = notice.IsRead,
                Title = notice.Title,
                Text = notice.Text,
                Url = IsSiteLink(notice.Link) ? notice.Link : null,
            }).ToList();

    /// <summary>Returns whether another of the signed-in customer's addresses has this label, ignoring case and surrounding spaces.</summary>
    public bool IsAddressLabelTaken(string? label, int exceptAddressId = 0) =>
        !string.IsNullOrWhiteSpace(label) && Identity.Current is { } account
        && addresses.Any(address => address.AccountId == account.Id && address.Id != exceptAddressId && string.Equals(address.Label.Trim(), label.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether another coupon has this code, ignoring case and surrounding spaces; the coupon being edited doesn't count.</summary>
    public bool IsCouponCodeTaken(string? code, string? exceptCode = null) =>
        !string.IsNullOrWhiteSpace(code)
        && coupons.Any(coupon => string.Equals(coupon.Code, code.Trim(), StringComparison.OrdinalIgnoreCase) && !string.Equals(coupon.Code, exceptCode?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns whether a SKU has the store's form: 4 to 20 letters, digits, and single dashes, starting with a letter, like
    /// KG-KIT-0101 (any case; it is saved in capitals).
    /// </summary>
    public static bool IsSku(string? value) =>
        value?.Trim().ToUpperInvariant() is { Length: >= 4 and <= MaxSkuLength } sku && SkuPattern().IsMatch(sku);

    /// <summary>Returns whether another product has this SKU, ignoring case and surrounding spaces; the product being edited doesn't count.</summary>
    public bool IsSkuTaken(string? sku, int exceptProductId = 0) =>
        !string.IsNullOrWhiteSpace(sku) && products.Any(product => product.Id != exceptProductId && string.Equals(product.Sku, sku.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Marks every notification of the signed-in account as read (the header's "Mark all as read").</summary>
    public void MarkAllNotificationsRead()
    {
        if (Identity.Current is { } account)
            MarkNoticesRead(account.Id);
    }

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (Identity.Current is not { } account || notices.FindIndex(notice => notice.Id == id && notice.AccountId == account.Id) is var index && index < 0 || notices[index].IsRead)
            return;

        notices[index] = notices[index] with { IsRead = true };
        RaiseChanged();
    }

    /// <summary>
    /// The header's global search, best matches first (exactly the term, then starting with it, then containing it), and
    /// among equal matches by kind and then newest first. The retailer finds customers (by name or email), orders (by
    /// number, like "10481" or "#10481", or by the customer's name or email), returns (by number, like "R-0012"), products
    /// (by name or SKU), and coupons (by code), each opening its back-office page; a customer finds their own orders (by number)
    /// and the store's products (by name), each opening its storefront page. Returns nothing for a blank term or when
    /// nobody is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || Identity.Current is not { } account)
            return Array.Empty<ShowcaseSearchResult>();

        var number = term.TrimStart('#').Trim();
        var hasDigit = number.Any(char.IsAsciiDigit);
        var matches = new List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)>();


        if (IsStaff)
        {
            // Kind: 0 a customer, 1 an order, 2 a return, 3 a product, 4 a coupon.
            if (term.Length >= 2)
            {
                foreach (var customer in Customers())
                {
                    if (ShowcaseSearch.Rank(term, customer.Name, customer.Email) is { } found)
                    {
                        matches.Add((found, 0, -(customer.LastOrderOn ?? DateTime.MinValue).Ticks, new ShowcaseSearchResult("Customer", customer.Name,
                            $"{customer.Email} · {(customer.OrderCount == 1 ? "1 order" : string.Create(CultureInfo.InvariantCulture, $"{customer.OrderCount} orders"))} · {EcommerceSeedData.Money(customer.Spent)}",
                            $"{AdminBase}/customers?email={Uri.EscapeDataString(customer.Email)}")));
                    }
                }
            }

            foreach (var order in orders)
            {
                var found = hasDigit ? ShowcaseSearch.Rank(number, order.Id.ToString(CultureInfo.InvariantCulture)) : null;
                found = Min(found, term.Length >= 2 ? ShowcaseSearch.Rank(term, order.ShipTo.FullName, order.ShipTo.Email) : null);

                if (found is { } rank)
                {
                    matches.Add((rank, 1, -order.PlacedOn.Ticks, new ShowcaseSearchResult("Order", $"Order #{order.Id}",
                        $"{order.ShipTo.FullName} · {EcommerceSeedData.Money(order.Totals.Total)} · {StatusWord(order.Status)} · {order.PlacedOn.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}",
                        EcommerceSeedData.AdminOrderPath(order.Id))));
                }
            }

            if (hasDigit)
            {
                foreach (var item in returns)
                {
                    if (ShowcaseSearch.Rank(term, EcommerceSeedData.ReturnNumber(item.Id), item.Id.ToString(CultureInfo.InvariantCulture)) is { } found)
                    {
                        matches.Add((found, 2, -item.RequestedOn.Ticks, new ShowcaseSearchResult("Return", EcommerceSeedData.ReturnNumber(item.Id),
                            $"{FindOrder(item.OrderId)?.ShipTo.FullName ?? "Unknown customer"} · order #{item.OrderId} · {ReturnWord(item.Status)}", EcommerceSeedData.AdminReturnPath(item.Id))));
                    }
                }
            }

            foreach (var product in products)
            {
                if (ShowcaseSearch.Rank(term, product.Name, product.Sku) is { } found)
                {
                    matches.Add((found, 3, product.IsArchived ? 1 : 0, new ShowcaseSearchResult("Product", product.Name,
                        string.Create(CultureInfo.InvariantCulture, $"{product.Sku} · {EcommerceSeedData.Money(product.Price)} · {product.Stock} in stock{(product.IsArchived ? " · archived" : null)}"),
                        EcommerceSeedData.AdminProductPath(product.Id))));
                }
            }

            foreach (var coupon in coupons)
            {
                if (ShowcaseSearch.Rank(term, coupon.Code) is { } found)
                    matches.Add((found, 4, 0, new ShowcaseSearchResult("Coupon", coupon.Code, $"{CouponValue(coupon)} · {coupon.Description}", $"{OffersPath}?code={Uri.EscapeDataString(coupon.Code)}")));
            }

            // Kind 5: a purchase order, by number (like "PO-2026-0014" or "14"); kind 6: a supplier, by name or email.
            if (hasDigit)
            {
                foreach (var order in purchaseOrders)
                {
                    if (ShowcaseSearch.Rank(term, order.Number, order.Id.ToString(CultureInfo.InvariantCulture)) is { } found)
                    {
                        matches.Add((found, 5, -order.CreatedOn.Ticks, new ShowcaseSearchResult("Purchase order", order.Number,
                            $"{FindSupplier(order.SupplierId)?.Name} · {MoneyIn(order)} · {PurchaseOrderStatusWord(order.Status)}{(IsOverdue(order) ? " · overdue" : null)}",
                            EcommerceSeedData.AdminPurchaseOrderPath(order.Id))));
                    }
                }
            }

            foreach (var supplier in suppliers)
            {
                if (ShowcaseSearch.Rank(term, supplier.Name, supplier.Email) is { } found)
                {
                    matches.Add((found, 6, 0, new ShowcaseSearchResult("Supplier", supplier.Name,
                        string.Create(CultureInfo.InvariantCulture, $"{supplier.ContactName} · {supplier.LeadTimeDays} days · {supplier.Currency} · {supplier.PaymentTerms}"),
                        EcommerceSeedData.AdminSupplierPath(supplier.Id))));
                }
            }
        }
        else
        {
            // Kind: 0 one of the customer's orders, 1 a product in the store.
            if (hasDigit)
            {
                foreach (var order in orders.Where(order => order.CustomerId == account.Id))
                {
                    if (ShowcaseSearch.Rank(number, order.Id.ToString(CultureInfo.InvariantCulture)) is { } found)
                    {
                        matches.Add((found, 0, -order.PlacedOn.Ticks, new ShowcaseSearchResult("Your order", $"Order #{order.Id}",
                            $"Placed {order.PlacedOn.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} · {StatusWord(order.Status)} · {EcommerceSeedData.Money(order.Totals.Total)}",
                            EcommerceSeedData.OrderPath(order.Id))));
                    }
                }
            }

            foreach (var product in products.Where(product => !product.IsArchived))
            {
                if (ShowcaseSearch.Rank(term, product.Name) is { } found)
                {
                    matches.Add((found, 1, 0, new ShowcaseSearchResult("Product", product.Name,
                        $"{FindCategory(product.CategoryId)?.Name} · {EcommerceSeedData.Money(product.Price)}{(product.Stock == 0 ? " · out of stock" : null)}",
                        string.Create(CultureInfo.InvariantCulture, $"{SiteBase}/products/{product.Id}"))));
                }
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Order)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    /// <summary>Returns an order status in words for a sentence, like "in transit".</summary>
    public static string StatusWord(EcommerceOrderStatus status) => status switch
    {
        EcommerceOrderStatus.Placed => "to pack",
        EcommerceOrderStatus.Packed => "to ship",
        EcommerceOrderStatus.Shipped => "in transit",
        EcommerceOrderStatus.Delivered => "delivered",
        EcommerceOrderStatus.Cancelled => "cancelled",
        _ => "refunded",
    };

    /// <summary>Suggests a SKU for a new product in a category, like KG-KIT-0221 (the next product number).</summary>
    public string SuggestSku(string? categoryId) => EcommerceSeedData.SkuFor(categoryId, nextProductId);

    #endregion

    #region Private Methods

    /// <summary>Checks a stored link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) => ShowcaseLinks.IsSiteLink(url, SiteBase, allowSiteBase: true);

    private static int? Min(int? left, int? right) => left is null ? right : right is null ? left : Math.Min(left.Value, right.Value);

    /// <summary>An order as the audit log names it: its number and the customer, like "Order #10481 · Maya Lindqvist".</summary>
    private static string OrderTarget(EcommerceOrder order) => $"Order #{order.Id} · {order.ShipTo.FullName}";

    /// <summary>A product as the audit log names it: its name and SKU.</summary>
    private static string ProductTarget(EcommerceProduct product) => Clip($"Product {product.Name} ({product.Sku})", 120);

    /// <summary>A return as the audit log names it: its number and order, like "Return R-0012 · order #10233".</summary>
    private static string ReturnTarget(EcommerceReturn item) => $"Return {EcommerceSeedData.ReturnNumber(item.Id)} · order #{item.OrderId}";

    private static string ReturnWord(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => "waiting for a decision",
        EcommerceReturnStatus.Approved => "approved",
        _ => "rejected",
    };

    [GeneratedRegex("^[A-Z][A-Z0-9]*(?:-[A-Z0-9]+)*$")]
    private static partial Regex SkuPattern();

    /// <summary>Runs one action on several records, raising <see cref="Changed" /> once; NotAllowed for anyone but the retailer, Invalid for an empty list or a bad value.</summary>
    private EcommerceBulkOutcome Bulk<TKey>(IReadOnlyCollection<TKey> ids, Func<TKey, bool> change, bool invalid = false)
    {
        if (!IsStaff)
            return new EcommerceBulkOutcome(EcommerceBulkResult.NotAllowed, 0, 0);

        var list = (ids ?? Array.Empty<TKey>()).Distinct().ToList();

        if (list.Count == 0 || invalid)
            return new EcommerceBulkOutcome(EcommerceBulkResult.Invalid, 0, 0);

        int changed = 0, unchanged = 0;
        batchDepth++;

        try
        {
            foreach (var id in list)
            {
                if (change(id))
                    changed++;
                else
                    unchanged++;
            }
        }
        finally
        {
            batchDepth--;
        }

        if (batchDepth == 0 && changedInBatch)
        {
            changedInBatch = false;
            FlushLowStock();
            Changed?.Invoke();
        }

        return new EcommerceBulkOutcome(changed == 0 ? EcommerceBulkResult.NoChange : EcommerceBulkResult.Ok, changed, unchanged);
    }

    /// <summary>
    /// Raises a notification for an account that can sign in (rule WR-001): nothing for a seeded customer without a sign-in
    /// or a guest, and nothing for the person who caused it, unless the system did (<paramref name="fromSystem" />). Each
    /// account keeps its newest <see cref="MaxNoticesPerAccount" />.
    /// </summary>
    private void Notify(string? accountId, string title, string text, string? link, bool fromSystem = false)
    {
        if (accountId is null || Identity.FindAccount(accountId) is null || (!fromSystem && accountId == Identity.Current?.Id) || nextNoticeId >= NextIdLimit)
            return;

        notices.Add(new EcommerceNotice(nextNoticeId++, accountId, Clip(title.Trim(), 120), Clip(string.IsNullOrWhiteSpace(text) ? title.Trim() : text.Trim(), 300),
            IsSiteLink(link) ? link : null, Clock.Now, IsRead: false));

        var old = notices.Where(notice => notice.AccountId == accountId).OrderByDescending(notice => notice.Id).Skip(MaxNoticesPerAccount).Select(notice => notice.Id).ToHashSet();

        if (old.Count > 0)
            notices.RemoveAll(notice => old.Contains(notice.Id));

        while (notices.Count > MaxNotices)
            notices.RemoveAt(0);
    }

    /// <summary>Tells every retailer account that can sign in, except the one who caused it: an order to fulfil, a return to decide, a review to moderate.</summary>
    private void NotifyRetailers(string title, string text, string? link)
    {
        foreach (var account in Identity.Accounts.Where(account => account.RoleId == EcommerceSeedData.RetailerRole).ToList())
            Notify(account.Id, title, text, link);
    }

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person (or <paramref name="actor" />, like a guest's name) did
    /// <paramref name="action" /> (past tense) to <paramref name="target" />, now, on the site's clock; the store's system
    /// when nobody is named. The log keeps its newest entries.
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details, string? actor = null)
    {
        if (nextAuditId >= NextIdLimit)
            return;

        var who = Identity.Current?.Name ?? actor;

        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = Clock.Now,
            Actor = Clip(string.IsNullOrWhiteSpace(who) ? EcommerceSeedData.SystemActor : who.Trim(), ShowcaseIdentity.MaxNameLength),
            Action = Clip(action, 80),
            Target = Clip(target, 120),
            TargetUrl = IsSiteLink(targetUrl) ? targetUrl : null,
            Details = string.IsNullOrWhiteSpace(details) ? null : Clip(details.Trim(), 420),
        });

        while (audit.Count > MaxAudit)
            audit.RemoveAt(0);
    }

    /// <summary>A review as the audit log names it: its title and product.</summary>
    private string ReviewTarget(EcommerceReview review) => Clip($"Review “{Clip(review.Title, 40)}” on {FindAnyProduct(review.ProductId)?.Name ?? "a removed product"}", 120);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for the retailer; empty for customers (they see their own notices instead).</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsStaff ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    #endregion
}
