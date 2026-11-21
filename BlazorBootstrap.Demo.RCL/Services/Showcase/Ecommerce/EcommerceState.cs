using System.Globalization;
using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// State of the Kindred Goods store for the current browser tab, shared by the back office (the retailer) and the
/// storefront (customers): the catalog and stock, the cart, orders and their fulfilment, coupons, reviews and their
/// moderation, returns, the audit log, notifications, and the demo sign-in. It is kept in the tab's <c>sessionStorage</c>
/// through the showcase foundation, so it survives a reload in the tab; nothing is sent or charged.
/// </summary>
public sealed partial class EcommerceState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>The ID of the first order placed in a tab; the seeded history uses the IDs below it.</summary>
    public const int FirstNewOrderId = EcommerceSeedData.FirstHistoryOrderId + EcommerceSeedData.HistoryOrderCount;

    /// <summary>The longest refund or rejection note, kept with a return.</summary>
    public const int MaxDecisionNoteLength = 200;

    /// <summary>The longest reason for a price change, kept in the audit log.</summary>
    public const int MaxPriceReasonLength = 200;

    /// <summary>The longest SKU.</summary>
    public const int MaxSkuLength = 20;

    /// <summary>The shortest reason for a price change or a rejected return.</summary>
    public const int MinReasonLength = 5;

    private const int MaxAudit = 3000;

    private const int MaxCoupons = 200;

    private const int MaxNotices = 5000;

    private const int MaxNoticesPerAccount = 50;

    private const int MaxOrders = 5000;

    private const int MaxProducts = 1000;

    private const int MaxReturns = 2000;

    private const int MaxReviews = 5000;

    /// <summary>IDs stay below this, so a restored counter can't overflow.</summary>
    private const int NextIdLimit = 1_000_000;

    private static readonly string[] cardBrands = ShowcaseCards.TestCards.Select(card => card.Brand).Distinct().ToArray();

    private static readonly Lazy<EcommerceSeedSet> seed = new(EcommerceSeedData.Build);

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<EcommerceSavedAddress> addresses;

    private readonly List<ShowcaseAuditEntry> audit;

    private readonly List<EcommerceCartLine> cart = new();

    private readonly List<EcommerceCoupon> coupons;

    private readonly List<EcommerceNotice> notices;

    private readonly List<EcommerceOrder> orders;

    private readonly List<EcommerceProduct> products;

    private readonly List<EcommerceReturn> returns;

    private readonly List<EcommerceReview> reviews;

    private readonly List<EcommerceWishItem> wishlist;

    /// <summary>How many bulk actions are running; while above 0, <see cref="RaiseChanged" /> waits until the last one ends.</summary>
    private int batchDepth;

    private bool changedInBatch;

    private int nextAddressId;

    private int nextAuditId;

    private int nextNoticeId;

    private int nextOrderId = FirstNewOrderId;

    private int nextProductId;

    private int nextReturnId;

    private int nextReviewId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public EcommerceState()
    {
        var data = seed.Value;

        products = data.Products.ToList();
        reviews = data.Reviews.ToList();
        coupons = data.Coupons.ToList();
        orders = data.Orders.ToList();
        returns = data.Returns.ToList();
        notices = data.Notices.ToList();
        audit = data.Audit.ToList();
        addresses = EcommerceSeedData.Addresses();
        wishlist = EcommerceSeedData.Wishlist();
        nextAddressId = addresses.Max(address => address.Id) + 1;
        nextProductId = products.Max(product => product.Id) + 1;
        nextReviewId = reviews.Max(review => review.Id) + 1;
        nextReturnId = returns.Count == 0 ? 1 : returns.Max(item => item.Id) + 1;
        nextNoticeId = notices.Count == 0 ? 1 : notices.Max(notice => notice.Id) + 1;
        nextAuditId = audit.Count == 0 ? 1 : audit.Max(entry => entry.Id) + 1;

        Identity = new ShowcaseIdentity(EcommerceSeedData.Roles(), EcommerceSeedData.DemoAccounts());
        Identity.Changed += () =>
        {
            // A back-office sign-in hears about its overdue purchase orders.
            RemindOverduePurchaseOrders();
            Changed?.Invoke();
        };
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns an account's saved addresses, the default first.</summary>
    public IReadOnlyList<EcommerceSavedAddress> AddressesFor(string? accountId) =>
        addresses.Where(address => address.AccountId == accountId).OrderByDescending(address => address.IsDefault).ThenBy(address => address.Label).ToList();

    /// <summary>Returns whether the signed-in customer may cancel an order: their own, and not packed yet.</summary>
    public bool CanCustomerCancel(EcommerceOrder order) =>
        order.Status == EcommerceOrderStatus.Placed && order.CustomerId is not null && order.CustomerId == Identity.Current?.Id;

    /// <summary>Deletes one of the signed-in customer's addresses; when it was the default, the next one becomes the default.</summary>
    public bool DeleteAddress(int addressId)
    {
        if (OwnAddress(addressId) is not { } address)
            return false;

        addresses.Remove(address);

        if (address.IsDefault && addresses.FirstOrDefault(other => other.AccountId == address.AccountId) is { } next)
            addresses[addresses.IndexOf(next)] = next with { IsDefault = true };

        RaiseChanged();

        return true;
    }

    /// <summary>Deletes the signed-in customer's own review.</summary>
    public bool DeleteOwnReview(int reviewId)
    {
        if (OwnReview(reviewId) is not { } review)
            return false;

        reviews.Remove(review);
        Record("Review deleted", ReviewTarget(review), ReviewsPath, $"{review.Rating} of 5, by its author.");
        RaiseChanged();

        return true;
    }

    /// <summary>Returns whether an address (city and country) is in the same-day local delivery area.</summary>
    public static bool IsLocalDeliveryCity(string? city, string? country) =>
        string.Equals(country?.Trim(), EcommerceSeedData.LocalDeliveryCountry, StringComparison.OrdinalIgnoreCase)
        && EcommerceSeedData.LocalDeliveryCities.Any(local => string.Equals(local, city?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether a product is on the signed-in customer's wishlist.</summary>
    public bool IsWished(int productId) => Identity.Current is { } account && wishlist.Any(item => item.AccountId == account.Id && item.ProductId == productId);

    /// <summary>Marks every notice of an account as read.</summary>
    public void MarkNoticesRead(string accountId)
    {
        var changed = false;

        for (var index = 0; index < notices.Count; index++)
        {
            if (notices[index].AccountId == accountId && !notices[index].IsRead)
            {
                notices[index] = notices[index] with { IsRead = true };
                changed = true;
            }
        }

        if (changed)
            RaiseChanged();
    }

    /// <summary>Returns an account's notices, newest first.</summary>
    public IReadOnlyList<EcommerceNotice> NoticesFor(string? accountId) =>
        notices.Where(notice => notice.AccountId == accountId).OrderByDescending(notice => notice.CreatedOn).ThenByDescending(notice => notice.Id).ToList();

    /// <summary>
    /// Adds the lines of a past order to the cart, as far as the products are still sold and in stock, at today's prices.
    /// Returns how many items were added and how many couldn't be.
    /// </summary>
    public (int Added, int Skipped) Reorder(int orderId)
    {
        if (Identity.Current is not { RoleId: EcommerceSeedData.CustomerRole } account || FindOrder(orderId) is not { } order || order.CustomerId != account.Id)
            return (0, 0);

        var added = 0;
        var skipped = 0;

        foreach (var line in order.Lines)
        {
            var got = AddToCart(line.ProductId, line.Color, line.Quantity);
            added += got;
            skipped += line.Quantity - got;
        }

        return (added, skipped);
    }

    /// <summary>
    /// Saves an address for the signed-in customer: <paramref name="address" /> with ID 0 adds one (up to <see cref="EcommerceSeedData.MaxSavedAddresses" />), otherwise it
    /// updates that address. A default address replaces the previous default; an account's first address is the default.
    /// Returns <see langword="null" /> when another of the customer's addresses already has the label.
    /// </summary>
    public EcommerceSavedAddress? SaveAddress(EcommerceSavedAddress address)
    {
        if (Identity.Current is not { RoleId: EcommerceSeedData.CustomerRole } account)
            return null;

        var own = AddressesFor(account.Id);
        var isNew = address.Id == 0;

        if ((isNew && own.Count >= EcommerceSeedData.MaxSavedAddresses) || (!isNew && own.All(existing => existing.Id != address.Id)) || IsAddressLabelTaken(address.Label, address.Id))
            return null;

        var saved = address with
        {
            Id = isNew ? nextAddressId++ : address.Id,
            AccountId = account.Id,
            Label = address.Label.Trim(),
            FullName = address.FullName.Trim(),
            Street = address.Street.Trim(),
            City = address.City.Trim(),
            PostalCode = address.PostalCode.Trim(),
            Country = address.Country.Trim(),
            Phone = Clean(address.Phone, 20),
            IsDefault = address.IsDefault || own.Count == 0 || own.All(existing => existing.Id == address.Id),
        };

        if (saved.IsDefault)
        {
            for (var index = 0; index < addresses.Count; index++)
            {
                if (addresses[index].AccountId == account.Id && addresses[index].Id != saved.Id && addresses[index].IsDefault)
                    addresses[index] = addresses[index] with { IsDefault = false };
            }
        }
        else if (!isNew && own.First(existing => existing.Id == saved.Id).IsDefault)
        {
            // The default can only move to another address, not be switched off.
            saved = saved with { IsDefault = true };
        }

        if (isNew)
            addresses.Add(saved);
        else
            addresses[addresses.FindIndex(existing => existing.Id == saved.Id)] = saved;

        RaiseChanged();

        return saved;
    }

    /// <summary>Makes one of the signed-in customer's addresses the default.</summary>
    public bool SetDefaultAddress(int addressId) => OwnAddress(addressId) is { } address && SaveAddress(address with { IsDefault = true }) is not null;

    /// <summary>Turns the back-in-stock notice for a wished-for product on or off.</summary>
    public bool SetWishNotify(int productId, bool notify)
    {
        if (Identity.Current is not { } account || wishlist.FindIndex(item => item.AccountId == account.Id && item.ProductId == productId) is var index && index < 0)
            return false;

        wishlist[index] = wishlist[index] with { NotifyWhenInStock = notify };
        RaiseChanged();

        return true;
    }

    /// <summary>
    /// Adds a product to the signed-in customer's wishlist, or removes it. Returns whether it is on the list afterwards.
    /// An out-of-stock product is added with a back-in-stock notice requested.
    /// </summary>
    public bool ToggleWish(int productId)
    {
        if (Identity.Current is not { RoleId: EcommerceSeedData.CustomerRole } account || FindAnyProduct(productId) is not { } product)
            return false;

        var removed = wishlist.RemoveAll(item => item.AccountId == account.Id && item.ProductId == productId) > 0;

        if (!removed && wishlist.Count(item => item.AccountId == account.Id) < 100)
            wishlist.Add(new EcommerceWishItem(account.Id, productId, Clock.Today, NotifyWhenInStock: product.Stock == 0));

        RaiseChanged();

        return IsWished(productId);
    }

    /// <summary>Returns the number of unread notices of an account.</summary>
    public int UnreadNoticeCount(string? accountId) => notices.Count(notice => notice.AccountId == accountId && !notice.IsRead);

    /// <summary>Edits the signed-in customer's own review; it keeps its moderation status, so editing never undoes a hide.</summary>
    public bool UpdateOwnReview(int reviewId, int rating, string title, string body)
    {
        if (OwnReview(reviewId) is not { } review)
            return false;

        var updated = review with { Rating = Math.Clamp(rating, 1, 5), Title = title.Trim(), Body = body.Trim(), PostedOn = Clock.Today };
        ReplaceReview(updated);
        Record("Review edited", ReviewTarget(updated), ReviewsPath, review.Rating == updated.Rating ? null : $"Rating {review.Rating} → {updated.Rating} of 5.");

        return true;
    }

    /// <summary>Returns an account's wishlist, newest first.</summary>
    public IReadOnlyList<EcommerceWishItem> WishlistFor(string? accountId) =>
        wishlist.Where(item => item.AccountId == accountId).OrderByDescending(item => item.AddedOn).ThenByDescending(item => item.ProductId).ToList();

    /// <summary>Adds a quantity of a product in one color, merging with an existing line and capping at stock and <see cref="EcommerceSeedData.MaxQuantity" />.</summary>
    /// <returns>The quantity actually added (0 for staff, an unknown or archived product, or no stock).</returns>
    public int AddToCart(int productId, string color, int quantity)
    {
        // Staff accounts don't shop (D-032); the storefront hides the controls, and the state refuses too.
        if (IsStaff || FindProduct(productId) is not { } product || product.Stock == 0 || quantity <= 0 || !product.Colors.Contains(color))
            return 0;

        var index = cart.FindIndex(line => line.ProductId == productId && line.Color == color);
        var current = index < 0 ? 0 : cart[index].Quantity;
        var updated = Math.Min(current + quantity, MaxQuantityFor(product, color));

        if (updated == current)
            return 0;

        if (index < 0)
            cart.Add(new EcommerceCartLine(productId, color, updated));
        else
            cart[index] = cart[index] with { Quantity = updated };

        RaiseChanged();

        return updated - current;
    }

    /// <summary>Moves an order one step on: Placed → Packed → Shipped (needs a tracking number) → Delivered.</summary>
    /// <returns><see langword="false" /> when the order can't move on, or a shipment has no tracking number.</returns>
    public bool AdvanceOrder(int orderId, string? trackingNumber = null)
    {
        if (FindOrder(orderId) is not { } order || NextStatus(order.Status) is not { } next)
            return false;

        var tracking = order.TrackingNumber;

        if (next == EcommerceOrderStatus.Shipped)
        {
            tracking = trackingNumber?.Trim();

            if (string.IsNullOrEmpty(tracking) || tracking.Length > 40)
                return false;
        }

        ReplaceOrder(order with { Status = next, TrackingNumber = tracking, History = Append(order, next, null) });
        Record(next switch { EcommerceOrderStatus.Packed => "Order packed", EcommerceOrderStatus.Shipped => "Order shipped", _ => "Order delivered" }, OrderTarget(order),
            EcommerceSeedData.AdminOrderPath(order.Id), next == EcommerceOrderStatus.Shipped ? $"Tracking number {tracking}." : null);

        return true;
    }

    /// <summary>
    /// Adds a review from the signed-in customer (or a guest name) and returns it. The retailer is told, so it can be
    /// moderated and answered.
    /// </summary>
    public EcommerceReview AddReview(int productId, string author, int rating, string title, string body)
    {
        var review = new EcommerceReview(nextReviewId++, productId, author.Trim(), Math.Clamp(rating, 1, 5), title.Trim(), body.Trim(), Clock.Today,
            IsFromThisSession: true, AuthorAccountId: Identity.Current?.Id);
        reviews.Add(review);

        while (reviews.Count > MaxReviews)
            reviews.RemoveAt(0);

        Record("Review posted", ReviewTarget(review), ReviewsPath, $"{review.Rating} of 5, “{Clip(review.Title, 60)}”.", actor: review.Author);
        NotifyRetailers("New review to moderate", $"{review.Rating} of 5, “{Clip(review.Title, 60)}” on {FindAnyProduct(productId)?.Name ?? "a product"}", ReviewsPath);
        RaiseChanged();

        return review;
    }

    /// <summary>Applies a coupon code to the cart when it passes every rule.</summary>
    public EcommerceCouponCheck ApplyPromoCode(string? code)
    {
        var check = CheckCoupon(code);

        if (check == EcommerceCouponCheck.Applied)
        {
            AppliedPromoCode = FindCoupon(code)!.Code;
            RaiseChanged();
        }

        return check;
    }

    /// <summary>
    /// Approves a return on a delivered order: the items go back into stock and <paramref name="refundAmount" /> (the lines'
    /// total when <see langword="null" />) is refunded on the order, never more than the lines' total or what's left of the
    /// order's total. A refund below the lines' total needs a note saying why. An order refunded in full in the meantime
    /// can't take a return; reject it instead. The customer is told.
    /// </summary>
    public bool ApproveReturn(int returnId, string? note, decimal? refundAmount = null)
    {
        if (returns.FirstOrDefault(item => item.Id == returnId) is not { Status: EcommerceReturnStatus.Requested } request
            || FindOrder(request.OrderId) is not { Status: EcommerceOrderStatus.Delivered } order)
            return false;

        var refund = Math.Round(refundAmount ?? MaxRefundFor(request, order), 2, MidpointRounding.AwayFromZero);
        var cleanNote = Clean(note, MaxDecisionNoteLength);

        // Refunding less than the lines' total by choice needs a reason; less because the order was mostly refunded already doesn't.
        if (refund <= 0 || refund > request.Amount || refund > MaxRefundFor(request, order) || (refundAmount is not null && refund < request.Amount && cleanNote is null))
            return false;

        foreach (var line in request.Lines)
            ChangeStock(line.ProductId, line.Quantity);

        var approved = request with { Status = EcommerceReturnStatus.Approved, DecidedOn = Clock.Now, DecisionNote = cleanNote, RefundAmount = refund };
        ReplaceReturn(approved);
        ReplaceOrder(order with { RefundedAmount = Math.Min(order.Totals.Total, order.RefundedAmount + refund) });
        Record("Return approved", ReturnTarget(request), EcommerceSeedData.AdminReturnPath(request.Id),
            $"{EcommerceSeedData.Money(refund)} refunded{(refund < request.Amount ? $" of {EcommerceSeedData.Money(request.Amount)}" : null)}.{(cleanNote is null ? null : " " + cleanNote)}");
        Notify(order.CustomerId, $"Return {EcommerceSeedData.ReturnNumber(request.Id)} approved", $"{EcommerceSeedData.Money(refund)} is refunded to your card.", EcommerceSeedData.OrderPath(order.Id));

        return true;
    }

    /// <summary>Returns the average published rating of a product, or 0 without reviews.</summary>
    public double AverageRating(int productId)
    {
        var ratings = reviews.Where(review => review.ProductId == productId && review.Status == EcommerceReviewStatus.Published).Select(review => review.Rating).ToList();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    /// <summary>Returns whether a product can be deleted: no order, return, purchase order, or cart line refers to it. Others can only be archived.</summary>
    public bool CanDeleteProduct(int productId) =>
        !orders.Any(order => order.Lines.Any(line => line.ProductId == productId))
        && !purchaseOrders.Any(order => order.Lines.Any(line => line.ProductId == productId))
        && !returns.Any(item => item.Lines.Any(line => line.ProductId == productId))
        && !cart.Any(line => line.ProductId == productId);

    /// <summary>
    /// Cancels an order that hasn't shipped: its items go back into stock, the payment is refunded in full, and a coupon it
    /// used gets that use back. The note is the store's internal reason; customers see standard wording.
    /// </summary>
    public bool CancelOrder(int orderId, string? note)
    {
        if (FindOrder(orderId) is not { Status: EcommerceOrderStatus.Placed or EcommerceOrderStatus.Packed } order)
            return false;

        foreach (var line in order.Lines)
            ChangeStock(line.ProductId, line.Quantity);

        if (FindCoupon(order.PromoCode) is { Uses: > 0 } coupon)
            ReplaceCoupon(coupon with { Uses = coupon.Uses - 1 });

        var reason = Clean(note, 200);

        ReplaceOrder(order with
        {
            Status = EcommerceOrderStatus.Cancelled,
            RefundedAmount = order.Totals.Total,
            History = Append(order, EcommerceOrderStatus.Cancelled, reason),
        });
        Record("Order cancelled", OrderTarget(order), EcommerceSeedData.AdminOrderPath(order.Id),
            $"{EcommerceSeedData.Money(order.Totals.Total)} refunded; items back in stock.{(reason is null ? null : " " + reason)}");

        return true;
    }

    /// <summary>
    /// Returns the snapshot of the store: the cart, the address books, wishlists, and counters, and of the long lists
    /// (products, orders, coupons, reviews, returns, notices, and the audit log) only what differs from the fresh tab's
    /// seed: records changed or added, and the keys of seeded records removed. The seed is the same in every tab, so the
    /// restore rebuilds the rest from it; a fresh tab's snapshot is about a kilobyte although it holds hundreds of orders
    /// (devspec finding F-4).
    /// </summary>
    public object CaptureData()
    {
        var data = seed.Value;
        var (changedProducts, removedProducts) = Diff(products, data.Products, product => product.Id);
        var (changedOrders, removedOrders) = Diff(orders, data.Orders, order => order.Id);
        var (changedCoupons, removedCoupons) = Diff(coupons, data.Coupons, coupon => coupon.Code);
        var (changedReviews, removedReviews) = Diff(reviews, data.Reviews, review => review.Id);
        var (changedReturns, removedReturns) = Diff(returns, data.Returns, item => item.Id);
        var (changedNotices, removedNotices) = Diff(notices, data.Notices, notice => notice.Id);
        var (changedAudit, removedAudit) = Diff(audit, data.Audit, entry => entry.Id, SameAudit);
        var (changedSuppliers, removedSuppliers) = Diff(suppliers, data.Suppliers, supplier => supplier.Id);
        var (changedReplenishment, removedReplenishment) = Diff(replenishment, data.Replenishment, item => item.ProductId);
        var (changedPurchaseOrders, removedPurchaseOrders) = Diff(purchaseOrders, data.PurchaseOrders, order => order.Id);

        return new EcommerceSnapshot
        {
            NextPurchaseOrderId = nextPurchaseOrderId,
            PurchaseOrders = changedPurchaseOrders,
            RemovedPurchaseOrderIds = removedPurchaseOrders,
            RemovedReplenishmentIds = removedReplenishment,
            RemovedSupplierIds = removedSuppliers,
            Replenishment = changedReplenishment,
            Suppliers = changedSuppliers,
            Addresses = addresses.ToList(),
            Audit = changedAudit,
            Cart = cart.Count == 0 ? null : cart.ToList(),
            Coupons = changedCoupons,
            NextAddressId = nextAddressId,
            NextAuditId = nextAuditId,
            NextNoticeId = nextNoticeId,
            NextOrderId = nextOrderId,
            NextProductId = nextProductId,
            NextReturnId = nextReturnId,
            NextReviewId = nextReviewId,
            Notices = changedNotices,
            Orders = changedOrders,
            Products = changedProducts,
            PromoCode = AppliedPromoCode,
            RemovedAuditIds = removedAudit,
            RemovedCouponCodes = removedCoupons,
            RemovedNoticeIds = removedNotices,
            RemovedOrderIds = removedOrders,
            RemovedProductIds = removedProducts,
            RemovedReturnIds = removedReturns,
            RemovedReviewIds = removedReviews,
            Returns = changedReturns,
            Reviews = changedReviews,
            Wishlist = wishlist.ToList(),
        };
    }

    /// <summary>Returns the totals of the current cart for a delivery speed; a coupon that no longer applies gives no discount.</summary>
    public EcommerceTotals CartTotals(EcommerceDelivery delivery = EcommerceDelivery.Standard)
    {
        var subtotal = cart.Sum(line => (FindAnyProduct(line.ProductId)?.Price ?? 0m) * line.Quantity);
        var discount = AppliedCoupon is { } coupon && CheckCoupon(coupon.Code) == EcommerceCouponCheck.Applied ? Discount(coupon, subtotal) : 0m;

        return EcommerceSeedData.Totals(cart.Sum(line => line.Quantity), subtotal, discount, delivery);
    }

    /// <summary>Checks a coupon code against the cart and the signed-in customer, without applying it.</summary>
    public EcommerceCouponCheck CheckCoupon(string? code)
    {
        if (FindCoupon(code) is not { } coupon)
            return EcommerceCouponCheck.NotFound;

        if (!coupon.IsActive)
            return EcommerceCouponCheck.Inactive;

        if (coupon.StartsOn is { } starts && Clock.Today < starts.Date)
            return EcommerceCouponCheck.NotStarted;

        if (coupon.ExpiresOn is { } expires && Clock.Today > expires.Date)
            return EcommerceCouponCheck.Expired;

        if (coupon.UsageLimit is { } limit && coupon.Uses >= limit)
            return EcommerceCouponCheck.UsedUp;

        var subtotal = cart.Sum(line => (FindAnyProduct(line.ProductId)?.Price ?? 0m) * line.Quantity);

        if (subtotal < coupon.MinSubtotal)
            return EcommerceCouponCheck.BelowMinimum;

        if (coupon.FirstOrderOnly && Identity.Current is { } account && orders.Any(order => order.CustomerId == account.Id && order.Status != EcommerceOrderStatus.Cancelled))
            return EcommerceCouponCheck.NotFirstOrder;

        return EcommerceCouponCheck.Applied;
    }

    /// <summary>
    /// Adds a product from the editor's draft and returns it; it starts with a placeholder image. Returns
    /// <see langword="null" /> when the SKU is missing, malformed, or already used, or the catalog is full.
    /// </summary>
    public EcommerceProduct? CreateProduct(EcommerceProductDraft draft)
    {
        if (!IsSku(draft.Sku) || IsSkuTaken(draft.Sku) || products.Count >= MaxProducts || nextProductId >= NextIdLimit)
            return null;

        var product = FromDraft(nextProductId++, draft, addedOn: Clock.Today, isArchived: false, imageId: null);
        products.Add(product);
        Record("Product created", ProductTarget(product), EcommerceSeedData.AdminProductPath(product.Id),
            $"{EcommerceSeedData.Money(product.Price)} · {product.Stock.ToString(CultureInfo.InvariantCulture)} in stock");
        RaiseChanged();

        return product;
    }

    /// <summary>Returns the store's customers: everyone who ordered (by email) plus registered customers who haven't yet.</summary>
    public IReadOnlyList<EcommerceCustomerSummary> Customers()
    {
        var fromOrders = orders
            .GroupBy(order => order.ShipTo.Email, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var latest = group.OrderByDescending(order => order.PlacedOn).First();
                var accountId = group.Select(order => order.CustomerId).FirstOrDefault(id => id is not null);
                var counted = group.Where(order => order.Status != EcommerceOrderStatus.Cancelled).ToList();

                return new EcommerceCustomerSummary(latest.ShipTo.Email, latest.ShipTo.FullName, accountId, counted.Count,
                    counted.Sum(order => order.Totals.Total - order.RefundedAmount), latest.PlacedOn, latest.ShipTo.Country, group.Min(order => order.PlacedOn));
            })
            .ToList();

        var withoutOrders = Identity.Accounts
            .Where(account => account.RoleId == EcommerceSeedData.CustomerRole && fromOrders.All(customer => customer.AccountId != account.Id
                && !string.Equals(customer.Email, account.Email, StringComparison.OrdinalIgnoreCase)))
            .Select(account => new EcommerceCustomerSummary(account.Email, account.Name, account.Id, 0, 0m, null));

        return fromOrders.Concat(withoutOrders).OrderByDescending(customer => customer.LastOrderOn ?? DateTime.MinValue).ThenBy(customer => customer.Name).ToList();
    }

    /// <summary>Deletes a coupon; past orders keep the code they used as text.</summary>
    public bool DeleteCoupon(string code)
    {
        if (FindCoupon(code) is not { } coupon)
            return false;

        coupons.Remove(coupon);

        if (string.Equals(AppliedPromoCode, coupon.Code, StringComparison.OrdinalIgnoreCase))
            AppliedPromoCode = null;

        Record("Coupon deleted", $"Coupon {coupon.Code}", OffersPath, coupon.Description);
        RaiseChanged();

        return true;
    }

    /// <summary>Deletes a product nothing refers to (<see cref="CanDeleteProduct" />).</summary>
    public bool DeleteProduct(int productId)
    {
        if (!CanDeleteProduct(productId) || FindAnyProduct(productId) is not { } product)
            return false;

        products.Remove(product);
        reviews.RemoveAll(review => review.ProductId == productId);
        // A wish item for a missing product would make the next restore discard the whole snapshot.
        wishlist.RemoveAll(item => item.ProductId == productId);
        replenishment.RemoveAll(item => item.ProductId == productId);
        Record("Product deleted", ProductTarget(product), null, "Removed with its reviews.");
        RaiseChanged();

        return true;
    }

    /// <summary>Returns the discount a coupon gives on a subtotal.</summary>
    public static decimal Discount(EcommerceCoupon coupon, decimal subtotal) => EcommerceSeedData.CouponDiscount(coupon, subtotal);

    /// <summary>Returns the active (not archived) products that match the catalog filters, sorted.</summary>
    public IReadOnlyList<EcommerceProduct> FilteredProducts()
    {
        var filter = Filter;
        var term = filter.Search.Trim();

        IEnumerable<EcommerceProduct> query = Products.Where(product =>
            (filter.CategoryIds.Count == 0 || filter.CategoryIds.Contains(product.CategoryId))
            && product.Price <= filter.MaxPrice
            && (!filter.OnSaleOnly || product.IsOnSale)
            && (filter.MinRating == 0 || AverageRating(product.Id) >= filter.MinRating)
            && (term.Length == 0 || product.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));

        query = filter.Sort switch
        {
            EcommerceSort.PriceLowToHigh => query.OrderBy(product => product.Price).ThenBy(product => product.Name),
            EcommerceSort.PriceHighToLow => query.OrderByDescending(product => product.Price).ThenBy(product => product.Name),
            EcommerceSort.TopRated => query.OrderByDescending(product => AverageRating(product.Id)).ThenBy(product => product.Name),
            EcommerceSort.Newest => query.OrderByDescending(product => product.AddedOn).ThenBy(product => product.Name),
            _ => query.OrderByDescending(product => product.IsFeatured).ThenBy(product => product.Name),
        };

        return query.ToList();
    }

    /// <summary>Finds any product, including archived ones (for the back office and past orders), or returns <see langword="null" />.</summary>
    public EcommerceProduct? FindAnyProduct(int productId) => products.FirstOrDefault(product => product.Id == productId);

    /// <summary>Finds a category by its ID, or returns <see langword="null" />.</summary>
    public EcommerceCategory? FindCategory(string? categoryId) =>
        Categories.FirstOrDefault(category => string.Equals(category.Id, categoryId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Finds a coupon by its code (any case), or returns <see langword="null" />.</summary>
    public EcommerceCoupon? FindCoupon(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : coupons.FirstOrDefault(coupon => string.Equals(coupon.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Finds an order by its ID, or returns <see langword="null" />.</summary>
    public EcommerceOrder? FindOrder(int orderId) => orders.FirstOrDefault(order => order.Id == orderId);

    /// <summary>Finds an active (not archived) product, as the storefront shows it, or returns <see langword="null" />.</summary>
    public EcommerceProduct? FindProduct(int productId) => products.FirstOrDefault(product => product.Id == productId && !product.IsArchived);

    /// <summary>Finds a return by its ID, or returns <see langword="null" />.</summary>
    public EcommerceReturn? FindReturn(int returnId) => returns.FirstOrDefault(item => item.Id == returnId);

    /// <summary>Finds a review by its ID, or returns <see langword="null" />.</summary>
    public EcommerceReview? FindReview(int reviewId) => reviews.FirstOrDefault(review => review.Id == reviewId);

    /// <summary>
    /// Returns the most an approval of a return can refund on its order: the return's lines' total, or what's left of the
    /// order's total when less.
    /// </summary>
    public decimal MaxRefundFor(EcommerceReturn request, EcommerceOrder order) => Math.Max(0m, Math.Min(request.Amount, order.Totals.Total - order.RefundedAmount));

    /// <summary>
    /// Returns the highest quantity one cart line (a product in one color) may hold: the stock left after the product's
    /// other color lines, capped at <see cref="EcommerceSeedData.MaxQuantity" />. Stock is per product, not per color.
    /// </summary>
    public int MaxQuantityFor(EcommerceProduct product, string color)
    {
        var inOtherLines = cart.Where(line => line.ProductId == product.Id && line.Color != color).Sum(line => line.Quantity);

        return Math.Max(0, Math.Min(product.Stock - inOtherLines, EcommerceSeedData.MaxQuantity));
    }

    /// <summary>Returns the status after the given one in fulfilment, or <see langword="null" /> at the end.</summary>
    public static EcommerceOrderStatus? NextStatus(EcommerceOrderStatus status) => status switch
    {
        EcommerceOrderStatus.Placed => EcommerceOrderStatus.Packed,
        EcommerceOrderStatus.Packed => EcommerceOrderStatus.Shipped,
        EcommerceOrderStatus.Shipped => EcommerceOrderStatus.Delivered,
        _ => null,
    };

    /// <summary>Raises <see cref="Changed" /> after the catalog filter was edited in place.</summary>
    public void NotifyFilterChanged() => RaiseChanged();

    /// <summary>Raises <see cref="Changed" /> after a stored snapshot was applied.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Returns a customer account's orders, newest first.</summary>
    public IReadOnlyList<EcommerceOrder> OrdersFor(string accountId) =>
        orders.Where(order => order.CustomerId == accountId).OrderByDescending(order => order.PlacedOn).ToList();

    /// <summary>
    /// Turns the cart into an order paid with an approved test card: takes the items out of stock, counts the coupon's
    /// use, links the order to the signed-in customer, empties the cart, and returns the order. The retailer is told that
    /// a new order waits to be fulfilled.
    /// </summary>
    public EcommerceOrder PlaceOrder(EcommerceAddress shipTo, EcommerceDelivery delivery, ShowcaseCardCharge payment)
    {
        // Same-day courier delivery only reaches the local area; checkout offers it only there.
        if (delivery == EcommerceDelivery.Local && !IsLocalDeliveryCity(shipTo.City, shipTo.Country))
            delivery = EcommerceDelivery.Standard;

        var totals = CartTotals(delivery);
        var coupon = totals.Discount > 0 ? AppliedCoupon : null;
        var lines = cart.Select(line =>
        {
            var product = FindAnyProduct(line.ProductId)!;

            return new EcommerceOrderLine(product.Id, product.Name, line.Color, line.Quantity, product.Price);
        }).ToList();

        foreach (var line in lines)
            ChangeStock(line.ProductId, -line.Quantity);

        if (coupon is not null)
            ReplaceCoupon(coupon with { Uses = coupon.Uses + 1 });

        var now = Clock.Now;
        var order = new EcommerceOrder(nextOrderId++, now, lines, shipTo, delivery, coupon?.Code, totals, payment.Brand, payment.Last4,
            EcommerceOrderStatus.Placed, new[] { new EcommerceStatusChange(EcommerceOrderStatus.Placed, now, null) },
            CustomerId: Identity.Current is { RoleId: EcommerceSeedData.CustomerRole } account ? account.Id : null);

        orders.Add(order);

        while (orders.Count > MaxOrders)
            orders.RemoveAt(0);

        cart.Clear();
        AppliedPromoCode = null;
        Record("Order placed", OrderTarget(order), EcommerceSeedData.AdminOrderPath(order.Id),
            $"{ItemsText(totals.ItemCount)} · {EcommerceSeedData.Money(totals.Total)}{(coupon is null ? null : $" with {coupon.Code}")} · {payment.Brand} ending {payment.Last4}",
            actor: shipTo.FullName);
        NotifyRetailers($"New order #{order.Id} to fulfil", $"{shipTo.FullName} · {ItemsText(totals.ItemCount)} · {EcommerceSeedData.Money(totals.Total)}",
            EcommerceSeedData.AdminOrderPath(order.Id));
        RaiseChanged();

        return order;
    }

    /// <summary>Refunds a shipped or delivered order in full (the remaining amount); stock is not returned.</summary>
    public bool RefundOrder(int orderId, string? note)
    {
        if (FindOrder(orderId) is not { Status: EcommerceOrderStatus.Shipped or EcommerceOrderStatus.Delivered } order)
            return false;

        var reason = Clean(note, 200);

        ReplaceOrder(order with
        {
            Status = EcommerceOrderStatus.Refunded,
            RefundedAmount = order.Totals.Total,
            History = Append(order, EcommerceOrderStatus.Refunded, reason),
        });
        Record("Order refunded", OrderTarget(order), EcommerceSeedData.AdminOrderPath(order.Id),
            $"{EcommerceSeedData.Money(order.Totals.Total - order.RefundedAmount)} refunded.{(reason is null ? null : " " + reason)}");

        return true;
    }

    /// <summary>Rejects a return request with a note the customer reads; the customer is told.</summary>
    public bool RejectReturn(int returnId, string? note)
    {
        if (returns.FirstOrDefault(item => item.Id == returnId) is not { Status: EcommerceReturnStatus.Requested } request
            || Clean(note, MaxDecisionNoteLength) is not { Length: >= MinReasonLength } reason)
            return false;

        ReplaceReturn(request with { Status = EcommerceReturnStatus.Rejected, DecidedOn = Clock.Now, DecisionNote = reason });
        Record("Return rejected", ReturnTarget(request), EcommerceSeedData.AdminReturnPath(request.Id), reason);
        Notify(FindOrder(request.OrderId)?.CustomerId, $"Return {EcommerceSeedData.ReturnNumber(request.Id)} rejected", reason, EcommerceSeedData.OrderPath(request.OrderId));

        return true;
    }

    /// <summary>Returns how many more of a product, in any color, the cart can take before stock runs out.</summary>
    public int RemainingStockFor(EcommerceProduct product) =>
        Math.Max(0, product.Stock - cart.Where(line => line.ProductId == product.Id).Sum(line => line.Quantity));

    /// <summary>Removes the applied promo code.</summary>
    public void RemovePromoCode()
    {
        if (AppliedPromoCode is null)
            return;

        AppliedPromoCode = null;
        RaiseChanged();
    }

    /// <summary>Records the applied color mode ("light" or "dark") from the site bar, so charts can redraw in matching colors.</summary>
    public void SetTheme(string themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDark == IsDarkTheme)
            return;

        IsDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Sets or clears the store's public reply to a review.</summary>
    public bool ReplyToReview(int reviewId, string? reply)
    {
        if (FindReview(reviewId) is not { } review)
            return false;

        var text = Clean(reply, 600);
        ReplaceReview(review with { Reply = text });
        Record(text is null ? "Review reply removed" : review.Reply is null ? "Review reply posted" : "Review reply edited", ReviewTarget(review), ReviewsPath, null);

        return true;
    }

    /// <summary>
    /// Requests a return of some items from a delivered order. Each quantity is at most what was bought minus what is
    /// already in other open or approved returns. The retailer is told that a return waits for a decision.
    /// </summary>
    public EcommerceReturn? RequestReturn(int orderId, IReadOnlyList<(int ProductId, string Color, int Quantity)> items, string reason, string? comment)
    {
        if (FindOrder(orderId) is not { Status: EcommerceOrderStatus.Delivered } order || items.Count == 0 || string.IsNullOrWhiteSpace(reason) || returns.Count >= MaxReturns)
            return null;

        var lines = new List<EcommerceReturnLine>();

        foreach (var (productId, color, quantity) in items)
        {
            if (order.Lines.FirstOrDefault(line => line.ProductId == productId && line.Color == color) is not { } bought || quantity < 1 || quantity > ReturnableQuantity(order, bought))
                return null;

            lines.Add(new EcommerceReturnLine(productId, bought.Name, color, quantity, bought.UnitPrice));
        }

        var request = new EcommerceReturn(nextReturnId++, orderId, lines, reason.Trim(), Clean(comment, 500), EcommerceReturnStatus.Requested, Clock.Now);
        returns.Add(request);
        Record("Return requested", ReturnTarget(request), EcommerceSeedData.AdminReturnPath(request.Id), $"{ItemsText(lines.Sum(line => line.Quantity))} · {request.Reason}",
            actor: order.ShipTo.FullName);
        NotifyRetailers($"Return {EcommerceSeedData.ReturnNumber(request.Id)} requested", $"{order.ShipTo.FullName} · order #{order.Id} · {request.Reason}",
            EcommerceSeedData.AdminReturnPath(request.Id));
        RaiseChanged();

        return request;
    }

    /// <summary>Returns how many of an order line can still be returned.</summary>
    public int ReturnableQuantity(EcommerceOrder order, EcommerceOrderLine line) =>
        line.Quantity - returns
            .Where(item => item.OrderId == order.Id && item.Status != EcommerceReturnStatus.Rejected)
            .SelectMany(item => item.Lines)
            .Where(returned => returned.ProductId == line.ProductId && returned.Color == line.Color)
            .Sum(returned => returned.Quantity);

    /// <summary>Returns the published reviews of a product, newest first.</summary>
    public IReadOnlyList<EcommerceReview> ReviewsFor(int productId) =>
        reviews.Where(review => review.ProductId == productId && review.Status == EcommerceReviewStatus.Published)
            .OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).ToList();

    /// <summary>
    /// Creates or updates a coupon. <paramref name="originalCode" /> is the code being edited, or <see langword="null" /> for
    /// a new one. Returns <see langword="false" /> when another coupon already has the code, the dates are out of order, a
    /// percentage is above 90, or the usage limit is below the uses so far.
    /// </summary>
    public bool SaveCoupon(EcommerceCoupon coupon, string? originalCode)
    {
        var code = coupon.Code.Trim().ToUpperInvariant();
        var existing = originalCode is null ? null : FindCoupon(originalCode);

        if (IsCouponCodeTaken(code, originalCode) || (originalCode is not null && existing is null) || (originalCode is null && coupons.Count >= MaxCoupons)
            || (coupon.StartsOn is { } starts && coupon.ExpiresOn is { } ends && starts.Date > ends.Date)
            || (coupon.Kind == EcommerceCouponKind.Percent && coupon.Value > 90m)
            || (existing is not null && coupon.UsageLimit is { } limit && limit < existing.Uses))
            return false;

        var saved = coupon with
        {
            Code = code,
            Description = coupon.Description.Trim(),
            Value = Math.Max(0m, coupon.Kind == EcommerceCouponKind.Percent ? Math.Min(coupon.Value, 90m) : coupon.Value),
            MinSubtotal = Math.Max(0m, coupon.MinSubtotal),
            StartsOn = coupon.StartsOn?.Date,
            ExpiresOn = coupon.ExpiresOn?.Date,
        };

        if (existing is null)
        {
            coupons.Add(saved with { Uses = 0 });
            Record("Coupon created", $"Coupon {code}", OffersPath, $"{CouponValue(saved)} · {saved.Description}");
        }
        else
        {
            coupons[coupons.IndexOf(existing)] = saved with { Uses = existing.Uses };

            if (string.Equals(AppliedPromoCode, originalCode, StringComparison.OrdinalIgnoreCase))
                AppliedPromoCode = code;

            Record("Coupon updated", $"Coupon {code}", OffersPath,
                (existing.Code == code ? string.Empty : $"Code {existing.Code} → {code}. ") + $"{CouponValue(saved)} · {saved.Description}");
        }

        RaiseChanged();

        return true;
    }

    /// <summary>Archives a product (hidden from the storefront and removed from the cart) or brings it back.</summary>
    public bool SetArchived(int productId, bool archived)
    {
        if (FindAnyProduct(productId) is not { } product || product.IsArchived == archived)
            return false;

        if (archived)
            cart.RemoveAll(line => line.ProductId == productId);

        ReplaceProduct(product with { IsArchived = archived });
        Record(archived ? "Product archived" : "Product restored", ProductTarget(product), EcommerceSeedData.AdminProductPath(product.Id),
            archived ? "Hidden from the store and removed from carts." : "Back in the store.");

        return true;
    }

    /// <summary>Turns a coupon on or off.</summary>
    public bool SetCouponActive(string code, bool active)
    {
        if (FindCoupon(code) is not { } coupon || coupon.IsActive == active)
            return false;

        ReplaceCoupon(coupon with { IsActive = active });
        Record(active ? "Coupon resumed" : "Coupon paused", $"Coupon {coupon.Code}", OffersPath, null);

        return true;
    }

    /// <summary>Sets the quantity of a cart line; zero or less removes it.</summary>
    public void SetQuantity(int productId, string color, int quantity)
    {
        var index = cart.FindIndex(line => line.ProductId == productId && line.Color == color);

        if (index < 0 || FindAnyProduct(productId) is not { } product)
            return;

        if (quantity <= 0)
            cart.RemoveAt(index);
        else
            cart[index] = cart[index] with { Quantity = Math.Min(quantity, MaxQuantityFor(product, cart[index].Color)) };

        RaiseChanged();
    }

    /// <summary>Publishes or hides a review; hidden reviews leave the product page and its rating.</summary>
    public bool SetReviewStatus(int reviewId, EcommerceReviewStatus status)
    {
        if (FindReview(reviewId) is not { } review || review.Status == status || !Enum.IsDefined(status))
            return false;

        ReplaceReview(review with { Status = status });
        Record(status == EcommerceReviewStatus.Hidden ? "Review hidden" : "Review published", ReviewTarget(review), ReviewsPath, null);

        return true;
    }

    /// <summary>Sets a product's stock (0 to 9,999); cart lines above the new stock shrink to fit.</summary>
    public bool SetStock(int productId, int stock)
    {
        if (FindAnyProduct(productId) is not { } product)
            return false;

        var value = Math.Clamp(stock, 0, 9999);

        if (value == product.Stock)
            return true;

        ReplaceProduct(product with { Stock = value });
        Record("Stock adjusted", ProductTarget(product), EcommerceSeedData.AdminProductPath(product.Id),
            string.Create(CultureInfo.InvariantCulture, $"{product.Stock} → {value} in stock"));

        return true;
    }

    /// <summary>
    /// Rebuilds the store from the seed and a stored snapshot's changes, validates every part, and applies it only when
    /// everything is valid: known IDs and codes, defined enum values, amounts and quantities in range, bounded text, SKUs
    /// in their form and unique, and links that stay on this site.
    /// </summary>
    public bool TryRestoreData(JsonElement data)
    {
        EcommerceSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<EcommerceSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return false;
        }

        if (snapshot is null
            || snapshot.Cart is { Count: > 50 }
            || snapshot.NextOrderId is < FirstNewOrderId or > NextIdLimit
            || snapshot.NextProductId is < 1 or > NextIdLimit
            || snapshot.NextReviewId is < 1 or > NextIdLimit
            || snapshot.NextReturnId is < 1 or > NextIdLimit
            || snapshot.NextAddressId is < 1 or > NextIdLimit
            || snapshot.NextNoticeId is < 1 or > NextIdLimit
            || snapshot.NextAuditId is < 1 or > NextIdLimit)
            return false;

        var fresh = seed.Value;

        // Rebuild every long list from the seed and the stored changes; an inconsistent change list discards the snapshot.
        if (Overlay(fresh.Products, snapshot.Products, snapshot.RemovedProductIds, product => product.Id) is not { Count: > 0 and <= MaxProducts } storedProducts
            || Overlay(fresh.Orders, snapshot.Orders, snapshot.RemovedOrderIds, order => order.Id) is not { Count: <= MaxOrders } storedOrders
            || Overlay(fresh.Coupons, snapshot.Coupons, snapshot.RemovedCouponCodes, coupon => coupon.Code) is not { Count: <= MaxCoupons } storedCoupons
            || Overlay(fresh.Reviews, snapshot.Reviews, snapshot.RemovedReviewIds, review => review.Id) is not { Count: <= MaxReviews } storedReviews
            || Overlay(fresh.Returns, snapshot.Returns, snapshot.RemovedReturnIds, item => item.Id) is not { Count: <= MaxReturns } storedReturns
            || Overlay(fresh.Notices, snapshot.Notices, snapshot.RemovedNoticeIds, notice => notice.Id) is not { Count: <= MaxNotices } storedNotices
            || Overlay(fresh.Audit, snapshot.Audit, snapshot.RemovedAuditIds, entry => entry.Id) is not { Count: <= MaxAudit } storedAudit)
            return false;

        var storedCart = snapshot.Cart ?? new List<EcommerceCartLine>();

        if (!ValidProducts(storedProducts, snapshot.NextProductId)
            || !ValidCoupons(storedCoupons)
            || (snapshot.PromoCode is not null && !storedCoupons.Any(coupon => coupon.Code == snapshot.PromoCode)))
            return false;

        var byId = storedProducts.ToDictionary(product => product.Id);

        if (storedCart.Any(line => line is null || !byId.TryGetValue(line.ProductId, out var product) || product.IsArchived || line.Color is null || !product.Colors.Contains(line.Color)
                || line.Quantity is < 1 or > EcommerceSeedData.MaxQuantity)
            || storedCart.GroupBy(line => (line.ProductId, line.Color)).Any(group => group.Count() > 1)
            || storedCart.GroupBy(line => line.ProductId).Any(group => group.Sum(line => line.Quantity) > byId[group.Key].Stock))
            return false;

        if (storedOrders.Any(order => !ValidOrder(order, byId, snapshot.NextOrderId)) || storedOrders.Select(order => order.Id).Distinct().Count() != storedOrders.Count)
            return false;

        var orderIds = storedOrders.ToDictionary(order => order.Id);

        if (storedReviews.Any(review => review is null || !byId.ContainsKey(review.ProductId) || review.Id < 1 || review.Id >= snapshot.NextReviewId
                || review.Rating is < 1 or > 5 || !Enum.IsDefined(review.Status) || !IsText(review.Author, 60) || !IsText(review.Title, 120) || !IsText(review.Body, 2000)
                || (review.Reply is not null && review.Reply.Length > 600) || !IsPlausibleDate(review.PostedOn)
                || (review.AuthorAccountId is not null && !IsAccountId(review.AuthorAccountId)))
            || storedReviews.Select(review => review.Id).Distinct().Count() != storedReviews.Count)
            return false;

        if (storedReturns.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextReturnId || !orderIds.ContainsKey(item.OrderId) || !Enum.IsDefined(item.Status)
                || !IsText(item.Reason, 120) || (item.Comment is not null && item.Comment.Length > 500) || (item.DecisionNote is not null && item.DecisionNote.Length > MaxDecisionNoteLength)
                || !IsPlausibleDate(item.RequestedOn) || (item.DecidedOn is { } decided && (!IsPlausibleDate(decided) || decided < item.RequestedOn))
                || (item.Status == EcommerceReturnStatus.Requested) != (item.DecidedOn is null)
                || item.Lines is not { Count: >= 1 and <= 50 }
                || item.Lines.Any(line => line is null || !byId.ContainsKey(line.ProductId) || !IsText(line.Name, 120) || !IsText(line.Color, 40) || line.Quantity is < 1 or > EcommerceSeedData.MaxQuantity || !IsMoney(line.UnitPrice))
                || (item.RefundAmount is { } refund && (item.Status != EcommerceReturnStatus.Approved || refund <= 0 || refund > item.Amount)))
            || storedReturns.Select(item => item.Id).Distinct().Count() != storedReturns.Count)
            return false;

        if (snapshot.Addresses is not { Count: <= 2000 } storedAddresses || (snapshot.Wishlist ?? new List<EcommerceWishItem>()) is not { Count: <= 5000 } storedWishlist)
            return false;

        if (storedAddresses.Any(address => address is null || address.Id < 1 || address.Id >= snapshot.NextAddressId || !IsAccountId(address.AccountId)
                || !IsText(address.Label, 30) || !IsText(address.FullName, 60) || !IsText(address.Street, 100) || !IsText(address.City, 60)
                || !IsText(address.PostalCode, 10) || !IsText(address.Country, 60) || (address.Phone is not null && address.Phone.Length > 20))
            || storedAddresses.Select(address => address.Id).Distinct().Count() != storedAddresses.Count
            || storedAddresses.GroupBy(address => address.AccountId).Any(group => group.Count() > 10 || group.Count(address => address.IsDefault) != 1
                || group.Select(address => address.Label.Trim().ToUpperInvariant()).Distinct().Count() != group.Count()))
            return false;

        if (storedWishlist.Any(item => item is null || !IsAccountId(item.AccountId) || !byId.ContainsKey(item.ProductId) || !IsPlausibleDate(item.AddedOn))
            || storedWishlist.GroupBy(item => (item.AccountId, item.ProductId)).Any(group => group.Count() > 1))
            return false;

        if (storedNotices.Any(notice => notice is null || notice.Id < 1 || notice.Id >= snapshot.NextNoticeId || !IsAccountId(notice.AccountId)
                || !IsText(notice.Title, 120) || !IsText(notice.Text, 300) || !IsPlausibleDate(notice.CreatedOn) || (notice.Link is not null && !IsSiteLink(notice.Link)))
            || storedNotices.Select(notice => notice.Id).Distinct().Count() != storedNotices.Count)
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausibleDate(entry.At) || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength)
                || !IsText(entry.Action, 80) || !IsText(entry.Target, 120) || (entry.Details is not null && !IsText(entry.Details, 420)) || !IsSiteLink(entry.TargetUrl))
            || storedAudit.Select(entry => entry.Id).Distinct().Count() != storedAudit.Count)
            return false;

        if (!TryRebuildPurchasing(snapshot, byId, out var storedSuppliers, out var storedReplenishment, out var storedPurchaseOrders))
            return false;

        Reload(products, storedProducts);
        Reload(cart, storedCart);
        AppliedPromoCode = snapshot.PromoCode;
        Reload(orders, storedOrders);
        Reload(coupons, storedCoupons);
        Reload(reviews, storedReviews);
        Reload(returns, storedReturns);
        Reload(addresses, storedAddresses);
        Reload(wishlist, storedWishlist);
        Reload(notices, storedNotices);
        Reload(audit, storedAudit);
        Reload(suppliers, storedSuppliers);
        Reload(replenishment, storedReplenishment);
        Reload(purchaseOrders, storedPurchaseOrders);
        lowStockCrossings.Clear();
        nextPurchaseOrderId = snapshot.NextPurchaseOrderId;
        nextOrderId = snapshot.NextOrderId;
        nextProductId = snapshot.NextProductId;
        nextReviewId = snapshot.NextReviewId;
        nextReturnId = snapshot.NextReturnId;
        nextAddressId = snapshot.NextAddressId;
        nextNoticeId = snapshot.NextNoticeId;
        nextAuditId = snapshot.NextAuditId;

        return true;
    }

    /// <summary>
    /// Updates prices in one go: each entry sets a product's price and compare-at ("was") price. Every change is audited with
    /// the old and new prices and <paramref name="reason" /> when given. Returns how many changed.
    /// </summary>
    public int UpdatePrices(IEnumerable<(int ProductId, decimal Price, decimal? CompareAtPrice)> changes, string? reason = null)
    {
        var changed = 0;
        var why = Clean(reason, MaxPriceReasonLength);

        foreach (var (productId, price, compareAt) in changes)
        {
            if (FindAnyProduct(productId) is not { } product || price is <= 0 or > 100_000)
                continue;

            var was = compareAt is { } value && value > price ? value : (decimal?)null;

            if (product.Price == price && product.CompareAtPrice == was)
                continue;

            ReplaceProduct(product with { Price = price, CompareAtPrice = was }, notify: false);
            Record("Price changed", ProductTarget(product), EcommerceSeedData.AdminProductPath(product.Id),
                $"{PriceText(product.Price, product.CompareAtPrice)} → {PriceText(price, was)}{(why is null ? null : $". {why}")}");
            changed++;
        }

        if (changed > 0)
            RaiseChanged();

        return changed;
    }

    /// <summary>
    /// Updates a product from the editor's draft; the audit log names what changed. Returns <see langword="false" /> for an
    /// unknown product or a SKU that is malformed or used by another product.
    /// </summary>
    public bool UpdateProduct(int productId, EcommerceProductDraft draft)
    {
        if (FindAnyProduct(productId) is not { } product || !IsSku(draft.Sku) || IsSkuTaken(draft.Sku, productId))
            return false;

        var updated = FromDraft(productId, draft, product.AddedOn, product.IsArchived, product.ImageId);

        // Cart lines in a color the product no longer has are removed.
        cart.RemoveAll(line => line.ProductId == productId && !updated.Colors.Contains(line.Color));
        ReplaceProduct(updated);
        Record("Product updated", ProductTarget(updated), EcommerceSeedData.AdminProductPath(productId), ProductChanges(product, updated));

        return true;
    }

    #endregion

    #region Private Methods

    private static IReadOnlyList<EcommerceStatusChange> Append(EcommerceOrder order, EcommerceOrderStatus status, string? note) =>
        order.History.Append(new EcommerceStatusChange(status, DateTime.MinValue, note)).ToList();

    private static string? Clean(string? text, int maxLength)
    {
        var trimmed = text?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    /// <summary>Shortens a text to a length, with an ellipsis when cut.</summary>
    private static string Clip(string text, int maxLength) => text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength - 1), "…");

    /// <summary>A coupon's discount in words, like "15% off" or "$20.00 off".</summary>
    private static string CouponValue(EcommerceCoupon coupon) => coupon.Kind == EcommerceCouponKind.Percent
        ? $"{coupon.Value.ToString("0.##", CultureInfo.InvariantCulture)}% off"
        : $"{EcommerceSeedData.Money(coupon.Value)} off";

    /// <summary>
    /// Returns the records that differ from the seed (changed or added, in list order) and the keys of seeded records that
    /// were removed; <see langword="null" /> for an empty list, so it is left out of the snapshot.
    /// </summary>
    private static (List<T>? Changed, List<TKey>? Removed) Diff<T, TKey>(IReadOnlyList<T> current, IReadOnlyList<T> seeded, Func<T, TKey> key, Func<T, T, bool>? equals = null)
        where TKey : notnull
    {
        var seededByKey = seeded.ToDictionary(key);
        var currentKeys = new HashSet<TKey>();
        var changed = new List<T>();
        equals ??= EqualityComparer<T>.Default.Equals;

        foreach (var item in current)
        {
            var itemKey = key(item);
            currentKeys.Add(itemKey);

            if (!seededByKey.TryGetValue(itemKey, out var original) || !equals(original, item))
                changed.Add(item);
        }

        var removed = seeded.Where(item => !currentKeys.Contains(key(item))).Select(key).ToList();

        return (changed.Count == 0 ? null : changed, removed.Count == 0 ? null : removed);
    }

    private static EcommerceProduct FromDraft(int id, EcommerceProductDraft draft, DateTime addedOn, bool isArchived, int? imageId)
    {
        var colors = draft.Colors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(color => color.Length > 20 ? color[..20] : color).Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList();
        var highlights = draft.Highlights.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Length > 80 ? line[..80] : line).Take(6).ToList();
        var price = Math.Clamp(Math.Round(draft.Price, 2), 0.01m, 100_000m);
        var compareAt = draft.CompareAtPrice is { } was && Math.Round(was, 2) > price ? Math.Round(was, 2) : (decimal?)null;
        var hasPhoto = (imageId ?? id) is >= EcommerceProduct.FirstLibraryPhotoId and <= EcommerceProduct.LastLibraryPhotoId;

        return new EcommerceProduct(id, draft.Name.Trim(), draft.CategoryId, price, compareAt, draft.Description.Trim(), highlights,
            colors.Count == 0 ? new List<string> { "Natural" } : colors, draft.IsFeatured, Math.Clamp(draft.Stock, 0, 9999), addedOn, isArchived,
            draft.Sku.Trim().ToUpperInvariant(), imageId, hasPhoto ? Clean(draft.ImageAlt, MaxImageAltLength) : null);
    }

    /// <summary>Checks a money amount; the cap is above anything checkout can produce (50 lines × 10 × $100,000, plus tax).</summary>
    private static bool IsMoney(decimal value) => value is >= 0 and <= 100_000_000;

    /// <summary>Checks a date; the window is wider than anything the seed, the site clock, or the editors can produce.</summary>
    private static bool IsPlausibleDate(DateTime value) => value >= EcommerceSeedData.Today.AddYears(-2) && value <= EcommerceSeedData.Today.AddYears(5);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static string ItemsText(int count) => count == 1 ? "1 item" : string.Create(CultureInfo.InvariantCulture, $"{count} items");

    /// <summary>
    /// Rebuilds a list from the seed and a snapshot's changes: seeded records in their order (replaced by a stored record
    /// with the same key, left out when removed), then the added records in their stored order. Returns
    /// <see langword="null" /> for an inconsistent change list (a missing record, a repeated key, or a removed key that isn't
    /// seeded), which discards the snapshot. Seeded records are immutable, so the tab keeps them as they are.
    /// </summary>
    private static List<T>? Overlay<T, TKey>(IReadOnlyList<T> seeded, List<T>? changed, List<TKey>? removed, Func<T, TKey> key)
        where TKey : notnull
    {
        changed ??= new List<T>();
        removed ??= new List<TKey>();

        if (changed.Count > 10_000 || removed.Count > 10_000 || changed.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var replacements = new Dictionary<TKey, T>();

        foreach (var item in changed)
        {
            if (key(item) is not { } itemKey || !replacements.TryAdd(itemKey, item))
                return null;
        }

        var seededKeys = seeded.Select(key).ToHashSet();
        var removedKeys = removed.ToHashSet();

        if (removedKeys.Count != removed.Count || removedKeys.Any(item => !seededKeys.Contains(item) || replacements.ContainsKey(item)))
            return null;

        var result = new List<T>(seeded.Count + changed.Count);

        foreach (var item in seeded)
        {
            var itemKey = key(item);

            if (replacements.TryGetValue(itemKey, out var replacement))
                result.Add(replacement);
            else if (!removedKeys.Contains(itemKey))
                result.Add(item);
        }

        result.AddRange(changed.Where(item => !seededKeys.Contains(key(item))));

        return result;
    }

    /// <summary>A price and its compare-at price in words, like "$48.00 (was $60.00)".</summary>
    private static string PriceText(decimal price, decimal? compareAt) =>
        compareAt is { } was ? $"{EcommerceSeedData.Money(price)} (was {EcommerceSeedData.Money(was)})" : EcommerceSeedData.Money(price);

    /// <summary>Names what an edit changed, for the audit log.</summary>
    private static string? ProductChanges(EcommerceProduct before, EcommerceProduct after)
    {
        var changes = new List<string>();

        if (before.Name != after.Name)
            changes.Add($"name “{Clip(before.Name, 40)}” → “{Clip(after.Name, 40)}”");

        if (before.Sku != after.Sku)
            changes.Add($"SKU {before.Sku} → {after.Sku}");

        if (before.CategoryId != after.CategoryId)
            changes.Add($"category {before.CategoryId} → {after.CategoryId}");

        if (before.Price != after.Price || before.CompareAtPrice != after.CompareAtPrice)
            changes.Add($"price {PriceText(before.Price, before.CompareAtPrice)} → {PriceText(after.Price, after.CompareAtPrice)}");

        if (before.Stock != after.Stock)
            changes.Add(string.Create(CultureInfo.InvariantCulture, $"stock {before.Stock} → {after.Stock}"));

        if (before.IsFeatured != after.IsFeatured)
            changes.Add(after.IsFeatured ? "now featured" : "no longer featured");

        if (before.Description != after.Description || !before.Highlights.SequenceEqual(after.Highlights) || !before.Colors.SequenceEqual(after.Colors) || before.ImageAlt != after.ImageAlt)
            changes.Add("details");

        if (changes.Count == 0)
            return "Saved with no changes.";

        var text = string.Join(", ", changes);

        return char.ToUpperInvariant(text[0]) + text[1..] + ".";
    }

    private static void Reload<T>(List<T> target, IEnumerable<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private static bool SameAudit(ShowcaseAuditEntry left, ShowcaseAuditEntry right) =>
        left.Id == right.Id && left.At == right.At && left.Actor == right.Actor && left.Action == right.Action && left.Target == right.Target
        && left.TargetUrl == right.TargetUrl && left.Details == right.Details;

    private static bool ValidCoupons(List<EcommerceCoupon> list) =>
        list.All(coupon => coupon is not null && IsText(coupon.Code, 20) && coupon.Code.Length >= 3 && coupon.Code.All(char.IsAsciiLetterOrDigit) && coupon.Code == coupon.Code.ToUpperInvariant()
            && IsText(coupon.Description, 120) && Enum.IsDefined(coupon.Kind) && coupon.Value is > 0 and <= 10_000 && (coupon.Kind != EcommerceCouponKind.Percent || coupon.Value <= 100)
            && IsMoney(coupon.MinSubtotal) && coupon.Uses is >= 0 and <= 1_000_000 && coupon.UsageLimit is null or (>= 1 and <= 1_000_000)
            && (coupon.ExpiresOn is null || IsPlausibleDate(coupon.ExpiresOn.Value)) && (coupon.StartsOn is null || IsPlausibleDate(coupon.StartsOn.Value))
            && (coupon.StartsOn is not { } starts || coupon.ExpiresOn is not { } ends || starts.Date <= ends.Date))
        && list.Select(coupon => coupon.Code).Distinct().Count() == list.Count;

    /// <summary>Checks an account ID's shape: a demo account, a seeded customer (without a sign-in), or one registered in the tab (identity validates the accounts themselves).</summary>
    private bool IsAccountId(string? accountId) =>
        accountId is not null
        && (Identity.DemoAccounts.Any(demo => demo.Id == accountId) || EcommerceSeedData.IsSeededCustomerId(accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private void ChangeStock(int productId, int delta)
    {
        if (FindAnyProduct(productId) is { } product)
            ReplaceProduct(product with { Stock = Math.Clamp(product.Stock + delta, 0, 9999) }, notify: false);
    }

    private EcommerceSavedAddress? OwnAddress(int addressId) =>
        Identity.Current is { } account ? addresses.FirstOrDefault(address => address.Id == addressId && address.AccountId == account.Id) : null;

    private EcommerceReview? OwnReview(int reviewId) =>
        Identity.Current is { } account ? reviews.FirstOrDefault(review => review.Id == reviewId && review.AuthorAccountId == account.Id) : null;

    /// <summary>
    /// Raises <see cref="Changed" />, or, during a bulk action, once when the last bulk action ends; first sends the one
    /// notification about products that fell to their reorder point meanwhile.
    /// </summary>
    private void RaiseChanged()
    {
        if (batchDepth > 0)
        {
            changedInBatch = true;

            return;
        }

        FlushLowStock();
        Changed?.Invoke();
    }

    private void ReplaceCoupon(EcommerceCoupon coupon)
    {
        var index = coupons.FindIndex(existing => existing.Code == coupon.Code);
        coupons[index] = coupon;
        RaiseChanged();
    }

    /// <summary>Replaces an order; a history entry added with <see cref="DateTime.MinValue" /> is stamped with the site clock.</summary>
    private void ReplaceOrder(EcommerceOrder order, bool notify = true)
    {
        var stamped = order.History.Any(change => change.At == DateTime.MinValue)
            ? order with { History = order.History.Select(change => change.At == DateTime.MinValue ? change with { At = Clock.Now } : change).ToList() }
            : order;

        var index = orders.FindIndex(existing => existing.Id == order.Id);
        var previous = orders[index];
        orders[index] = stamped;

        // The customer hears about every step the store takes on their order (CR-001-AC-07), but not about their own cancel.
        if (stamped.CustomerId is { } customer && previous.Status != stamped.Status)
            Notify(customer, $"Order #{stamped.Id} {EcommerceSeedData.StatusVerb(stamped.Status)}", EcommerceSeedData.OrderNoticeText(stamped), EcommerceSeedData.OrderPath(stamped.Id));

        if (notify)
            RaiseChanged();
    }

    /// <summary>
    /// Replaces a product. A product back in stock tells everyone who asked to hear about it (once); a product that falls to
    /// its reorder point or sells out is noted, and the back office hears about every such product in one notification when
    /// the change ends (<see cref="FlushLowStock" />), except the person who caused it.
    /// </summary>
    private void ReplaceProduct(EcommerceProduct product, bool notify = true)
    {
        var productIndex = products.FindIndex(existing => existing.Id == product.Id);
        var before = products[productIndex];
        products[productIndex] = product;

        if (before.Stock == 0 && product.Stock > 0 && !product.IsArchived)
        {
            for (var wishIndex = 0; wishIndex < wishlist.Count; wishIndex++)
            {
                if (wishlist[wishIndex].ProductId == product.Id && wishlist[wishIndex].NotifyWhenInStock)
                {
                    Notify(wishlist[wishIndex].AccountId, $"{product.Name} is back in stock", $"{product.Stock} available now.", $"{EcommerceSeedData.SiteBase}/products/{product.Id}", fromSystem: true);
                    wishlist[wishIndex] = wishlist[wishIndex] with { NotifyWhenInStock = false };
                }
            }
        }

        if (!product.IsArchived && product.Stock < before.Stock && ((IsLowStock(product) && !IsLowStock(before)) || product.Stock == 0) && !lowStockCrossings.Contains(product.Id))
            lowStockCrossings.Add(product.Id);

        // Stock or archive changes can leave cart lines over what's left.
        foreach (var line in cart.Where(line => line.ProductId == product.Id).ToList())
        {
            var max = MaxQuantityFor(product, line.Color);
            var index = cart.IndexOf(line);

            if (max <= 0)
                cart.RemoveAt(index);
            else if (line.Quantity > max)
                cart[index] = line with { Quantity = max };
        }

        if (notify)
            RaiseChanged();
    }

    private void ReplaceReturn(EcommerceReturn item)
    {
        returns[returns.FindIndex(existing => existing.Id == item.Id)] = item;
        RaiseChanged();
    }

    private void ReplaceReview(EcommerceReview review)
    {
        reviews[reviews.FindIndex(existing => existing.Id == review.Id)] = review;
        RaiseChanged();
    }

    private bool ValidOrder(EcommerceOrder? order, IReadOnlyDictionary<int, EcommerceProduct> byId, int nextId) =>
        order is not null
        && order.Id >= EcommerceSeedData.FirstHistoryOrderId && order.Id < nextId
        && IsPlausibleDate(order.PlacedOn)
        && Enum.IsDefined(order.Delivery) && Enum.IsDefined(order.Status)
        && (order.PromoCode is null || IsText(order.PromoCode, 20))
        && order.CardBrand is not null && cardBrands.Contains(order.CardBrand)
        && order.CardLast4 is { Length: 4 } last4 && last4.All(char.IsAsciiDigit)
        && (order.TrackingNumber is null || IsText(order.TrackingNumber, 40))
        && (order.CustomerId is null || IsAccountId(order.CustomerId))
        && IsMoney(order.RefundedAmount) && order.Totals is { } totals && order.RefundedAmount <= totals.Total
        && totals.ItemCount is >= 1 and <= 500 && IsMoney(totals.Subtotal) && IsMoney(totals.Discount) && IsMoney(totals.Shipping) && IsMoney(totals.Tax) && IsMoney(totals.Total)
        && order.Lines is { Count: >= 1 and <= 50 }
        && order.Lines.All(line => line is not null && byId.ContainsKey(line.ProductId) && IsText(line.Name, 120) && IsText(line.Color, 40)
            && line.Quantity is >= 1 and <= EcommerceSeedData.MaxQuantity && IsMoney(line.UnitPrice))
        && order.ShipTo is { } address
        && IsText(address.FullName, 60) && IsText(address.Email, 120) && IsText(address.Street, 100) && IsText(address.City, 60) && IsText(address.PostalCode, 10) && IsText(address.Country, 60)
        && order.History is { Count: >= 1 and <= 20 }
        && order.History.All(change => change is not null && Enum.IsDefined(change.Status) && IsPlausibleDate(change.At) && (change.Note is null || change.Note.Length <= 200))
        && order.History[^1].Status == order.Status;

    private bool ValidProducts(List<EcommerceProduct> list, int nextId) =>
        list.All(product => product is not null && product.Id >= 1 && product.Id < nextId && IsText(product.Name, 80) && FindCategory(product.CategoryId) is not null
            && product.Price is > 0 and <= 100_000 && (product.CompareAtPrice is null || product.CompareAtPrice is > 0 and <= 100_000)
            && IsText(product.Description, 1000) && product.Highlights is { Count: <= 6 } && product.Highlights.All(line => IsText(line, 80))
            && product.Colors is { Count: >= 1 and <= 6 } && product.Colors.All(color => IsText(color, 20))
            && product.Stock is >= 0 and <= 9999 && IsPlausibleDate(product.AddedOn)
            && IsSku(product.Sku) && product.Sku == product.Sku.ToUpperInvariant()
            && (product.ImageId is null || product.ImageId is >= EcommerceProduct.FirstLibraryPhotoId and <= EcommerceProduct.LastLibraryPhotoId)
            && (product.ImageAlt is null || IsText(product.ImageAlt, MaxImageAltLength)))
        && list.Select(product => product.Id).Distinct().Count() == list.Count
        && list.Select(product => product.Sku).Distinct(StringComparer.OrdinalIgnoreCase).Count() == list.Count;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets every product, including archived ones (for the back office).</summary>
    public IReadOnlyList<EcommerceProduct> AllProducts => products;

    /// <summary>Gets every review, including hidden ones (for moderation).</summary>
    public IReadOnlyList<EcommerceReview> AllReviews => reviews;

    /// <summary>Gets the applied coupon, or <see langword="null" />.</summary>
    public EcommerceCoupon? AppliedCoupon => FindCoupon(AppliedPromoCode);

    /// <summary>Gets the applied coupon code, or <see langword="null" />.</summary>
    public string? AppliedPromoCode { get; private set; }

    /// <summary>Gets the cart lines in the order they were added.</summary>
    public IReadOnlyList<EcommerceCartLine> Cart => cart;

    /// <summary>Gets the number of items in the cart.</summary>
    public int CartCount => cart.Sum(line => line.Quantity);

    /// <summary>Gets the product categories.</summary>
    public IReadOnlyList<EcommerceCategory> Categories { get; } = EcommerceSeedData.Categories();

    /// <summary>Gets the site clock, which starts at <see cref="EcommerceSeedData.SeedTime" />.</summary>
    public ShowcaseClock Clock { get; } = new(EcommerceSeedData.SeedTime);

    /// <summary>Gets the store's coupons.</summary>
    public IReadOnlyList<EcommerceCoupon> Coupons => coupons;

    /// <summary>Gets the catalog filters.</summary>
    public EcommerceCatalogFilter Filter { get; } = new();

    /// <summary>Gets the demo sign-in: a retailer and a customer account.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the site shows the dark color mode (not part of the snapshot; the theme switcher keeps its own choice).</summary>
    public bool IsDarkTheme { get; private set; }

    /// <summary>Gets whether the signed-in account is store staff (the retailer), who see the shop but don't buy.</summary>
    public bool IsStaff => Identity.IsInRole(EcommerceSeedData.StaffRoles());

    /// <summary>Gets all orders, oldest first: the seeded history, then orders placed in this tab.</summary>
    public IReadOnlyList<EcommerceOrder> Orders => orders;

    /// <summary>Gets the active (not archived) catalog the storefront shows.</summary>
    public IReadOnlyList<EcommerceProduct> Products => products.Where(product => !product.IsArchived).ToList();

    /// <summary>Gets all return requests.</summary>
    public IReadOnlyList<EcommerceReturn> Returns => returns;

    /// <summary>Gets the site slug used in the storage key.</summary>
    public string SiteSlug => "ecommerce";

    /// <summary>
    /// Gets the snapshot format version (5: suppliers, replenishment settings, and purchase orders, after 4: the enterprise
    /// baseline with a year of seeded history and only what differs from the seed).
    /// </summary>
    public int SnapshotVersion => 5;

    #endregion

    #region Events

    /// <summary>Raised when anything in the store changes, including sign-in.</summary>
    public event Action? Changed;

    /// <summary>Raised when the color mode changes.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Nested Types

    /// <summary>
    /// The store's part of the tab snapshot: the small lists whole, and of the seeded lists only the records changed or added
    /// and the keys of the ones removed (<see langword="null" /> when there are none).
    /// </summary>
    private sealed class EcommerceSnapshot
    {
        #region Properties, Indexers

        public List<EcommerceSavedAddress>? Addresses { get; set; }

        public List<ShowcaseAuditEntry>? Audit { get; set; }

        public List<EcommerceCartLine>? Cart { get; set; }

        public List<EcommerceCoupon>? Coupons { get; set; }

        public int NextAddressId { get; set; }

        public int NextAuditId { get; set; }

        public int NextNoticeId { get; set; }

        public int NextOrderId { get; set; }

        public int NextProductId { get; set; }

        public int NextReturnId { get; set; }

        public int NextReviewId { get; set; }

        public List<EcommerceNotice>? Notices { get; set; }

        public List<EcommerceOrder>? Orders { get; set; }

        public List<EcommerceProduct>? Products { get; set; }

        public string? PromoCode { get; set; }

        public List<int>? RemovedAuditIds { get; set; }

        public List<string>? RemovedCouponCodes { get; set; }

        public List<int>? RemovedNoticeIds { get; set; }

        public List<int>? RemovedOrderIds { get; set; }

        public List<int>? RemovedProductIds { get; set; }

        public List<int>? RemovedReturnIds { get; set; }

        public List<int>? RemovedReviewIds { get; set; }

        public List<EcommerceReturn>? Returns { get; set; }

        public List<EcommerceReview>? Reviews { get; set; }

        public List<EcommerceWishItem>? Wishlist { get; set; }

        public int NextPurchaseOrderId { get; set; }

        public List<EcommercePurchaseOrder>? PurchaseOrders { get; set; }

        public List<int>? RemovedPurchaseOrderIds { get; set; }

        public List<int>? RemovedReplenishmentIds { get; set; }

        public List<string>? RemovedSupplierIds { get; set; }

        public List<EcommerceReplenishment>? Replenishment { get; set; }

        public List<EcommerceSupplier>? Suppliers { get; set; }

        #endregion
    }

    #endregion
}
