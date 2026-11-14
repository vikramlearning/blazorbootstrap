using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

/// <summary>
/// State of the Kindred Goods store for the current browser tab, shared by the storefront (customers) and the back
/// office (the retailer): the catalog and stock, the cart, orders and their fulfilment, coupons, reviews and their
/// moderation, returns, and the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase
/// foundation, so it survives a reload in the tab; nothing is sent or charged.
/// </summary>
public sealed class EcommerceState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The ID of the first order placed in a tab; the seeded history uses the IDs below it.</summary>
    public const int FirstNewOrderId = 10481;

    private const int FirstHistoryOrderId = 10445;

    private const int MaxListLength = 500;

    private static readonly string[] cardBrands = ShowcaseCards.TestCards.Select(card => card.Brand).Distinct().ToArray();

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<EcommerceCartLine> cart = new();

    private readonly List<EcommerceCoupon> coupons;

    private readonly List<EcommerceOrder> orders;

    private readonly List<EcommerceProduct> products;

    private readonly List<EcommerceReturn> returns;

    private readonly List<EcommerceReview> reviews;

    private int nextOrderId = FirstNewOrderId;

    private int nextProductId;

    private int nextReturnId;

    private int nextReviewId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public EcommerceState()
    {
        products = EcommerceSeedData.Products().ToList();
        reviews = EcommerceSeedData.Reviews(products);
        coupons = EcommerceSeedData.Coupons();
        orders = EcommerceSeedData.HistoryOrders(products);
        returns = EcommerceSeedData.Returns(orders);
        nextProductId = products.Max(product => product.Id) + 1;
        nextReviewId = reviews.Max(review => review.Id) + 1;
        nextReturnId = returns.Max(item => item.Id) + 1;

        // The approved seeded return was refunded on its order.
        foreach (var approved in returns.Where(item => item.Status == EcommerceReturnStatus.Approved))
            ReplaceOrder(FindOrder(approved.OrderId)! with { RefundedAmount = approved.Amount }, notify: false);

        Identity = new ShowcaseIdentity(EcommerceSeedData.Roles(), EcommerceSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
    }

    #endregion

    #region Methods

    #region Public Methods

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

        Changed?.Invoke();

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

        return true;
    }

    /// <summary>Adds a review from the signed-in customer (or a guest name) and returns it.</summary>
    public EcommerceReview AddReview(int productId, string author, int rating, string title, string body)
    {
        var review = new EcommerceReview(nextReviewId++, productId, author.Trim(), Math.Clamp(rating, 1, 5), title.Trim(), body.Trim(), Clock.Today,
            IsFromThisSession: true, AuthorAccountId: Identity.Current?.Id);
        reviews.Add(review);
        Changed?.Invoke();

        return review;
    }

    /// <summary>Applies a coupon code to the cart when it passes every rule.</summary>
    public EcommerceCouponCheck ApplyPromoCode(string? code)
    {
        var check = CheckCoupon(code);

        if (check == EcommerceCouponCheck.Applied)
        {
            AppliedPromoCode = FindCoupon(code)!.Code;
            Changed?.Invoke();
        }

        return check;
    }

    /// <summary>
    /// Approves a return on a delivered order: the items go back into stock and their amount is refunded on the order. An
    /// order refunded in full in the meantime can't take a return; reject it instead.
    /// </summary>
    public bool ApproveReturn(int returnId, string? note)
    {
        if (returns.FirstOrDefault(item => item.Id == returnId) is not { Status: EcommerceReturnStatus.Requested } request
            || FindOrder(request.OrderId) is not { Status: EcommerceOrderStatus.Delivered } order)
            return false;

        foreach (var line in request.Lines)
            ChangeStock(line.ProductId, line.Quantity);

        ReplaceReturn(request with { Status = EcommerceReturnStatus.Approved, DecidedOn = Clock.Now, DecisionNote = Clean(note, 200) });
        ReplaceOrder(order with { RefundedAmount = Math.Min(order.Totals.Total, order.RefundedAmount + request.Amount) });

        return true;
    }

    /// <summary>Returns the average published rating of a product, or 0 without reviews.</summary>
    public double AverageRating(int productId)
    {
        var ratings = reviews.Where(review => review.ProductId == productId && review.Status == EcommerceReviewStatus.Published).Select(review => review.Rating).ToList();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    /// <summary>Returns whether a product can be deleted: no order, return, or cart line refers to it. Others can only be archived.</summary>
    public bool CanDeleteProduct(int productId) =>
        !orders.Any(order => order.Lines.Any(line => line.ProductId == productId))
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

        ReplaceOrder(order with
        {
            Status = EcommerceOrderStatus.Cancelled,
            RefundedAmount = order.Totals.Total,
            History = Append(order, EcommerceOrderStatus.Cancelled, Clean(note, 200)),
        });

        return true;
    }

    /// <summary>Returns the snapshot of the store: catalog, cart, orders, coupons, reviews, returns, and the ID counters.</summary>
    public object CaptureData() => new EcommerceSnapshot
    {
        Products = products.ToList(),
        Cart = cart.ToList(),
        PromoCode = AppliedPromoCode,
        Orders = orders.ToList(),
        Coupons = coupons.ToList(),
        Reviews = reviews.ToList(),
        Returns = returns.ToList(),
        NextOrderId = nextOrderId,
        NextProductId = nextProductId,
        NextReviewId = nextReviewId,
        NextReturnId = nextReturnId,
    };

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

    /// <summary>Adds a product from the editor's draft and returns it; it starts with a placeholder image.</summary>
    public EcommerceProduct CreateProduct(EcommerceProductDraft draft)
    {
        var product = FromDraft(nextProductId++, draft, addedOn: Clock.Today, isArchived: false);
        products.Add(product);
        Changed?.Invoke();

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
                    counted.Sum(order => order.Totals.Total - order.RefundedAmount), latest.PlacedOn);
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
        var removed = coupons.RemoveAll(coupon => string.Equals(coupon.Code, code, StringComparison.OrdinalIgnoreCase)) > 0;

        if (removed)
        {
            if (string.Equals(AppliedPromoCode, code, StringComparison.OrdinalIgnoreCase))
                AppliedPromoCode = null;

            Changed?.Invoke();
        }

        return removed;
    }

    /// <summary>Deletes a product nothing refers to (<see cref="CanDeleteProduct" />).</summary>
    public bool DeleteProduct(int productId)
    {
        if (!CanDeleteProduct(productId) || products.RemoveAll(product => product.Id == productId) == 0)
            return false;

        reviews.RemoveAll(review => review.ProductId == productId);
        Changed?.Invoke();

        return true;
    }

    /// <summary>Returns the discount a coupon gives on a subtotal.</summary>
    public static decimal Discount(EcommerceCoupon coupon, decimal subtotal) => coupon.Kind == EcommerceCouponKind.Percent
        ? Math.Round(subtotal * coupon.Value / 100m, 2, MidpointRounding.AwayFromZero)
        : Math.Min(coupon.Value, subtotal);

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
    public void NotifyFilterChanged() => Changed?.Invoke();

    /// <summary>Raises <see cref="Changed" /> after a stored snapshot was applied.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Returns a customer account's orders, newest first.</summary>
    public IReadOnlyList<EcommerceOrder> OrdersFor(string accountId) =>
        orders.Where(order => order.CustomerId == accountId).OrderByDescending(order => order.PlacedOn).ToList();

    /// <summary>
    /// Turns the cart into an order paid with an approved test card: takes the items out of stock, counts the coupon's
    /// use, links the order to the signed-in customer, empties the cart, and returns the order.
    /// </summary>
    public EcommerceOrder PlaceOrder(EcommerceAddress shipTo, EcommerceDelivery delivery, ShowcaseCardCharge payment)
    {
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
        cart.Clear();
        AppliedPromoCode = null;
        Changed?.Invoke();

        return order;
    }

    /// <summary>Refunds a shipped or delivered order in full (the remaining amount); stock is not returned.</summary>
    public bool RefundOrder(int orderId, string? note)
    {
        if (FindOrder(orderId) is not { Status: EcommerceOrderStatus.Shipped or EcommerceOrderStatus.Delivered } order)
            return false;

        ReplaceOrder(order with
        {
            Status = EcommerceOrderStatus.Refunded,
            RefundedAmount = order.Totals.Total,
            History = Append(order, EcommerceOrderStatus.Refunded, Clean(note, 200)),
        });

        return true;
    }

    /// <summary>Rejects a return request with a note.</summary>
    public bool RejectReturn(int returnId, string? note)
    {
        if (returns.FirstOrDefault(item => item.Id == returnId) is not { Status: EcommerceReturnStatus.Requested } request)
            return false;

        ReplaceReturn(request with { Status = EcommerceReturnStatus.Rejected, DecidedOn = Clock.Now, DecisionNote = Clean(note, 200) });

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
        Changed?.Invoke();
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

        ReplaceReview(review with { Reply = Clean(reply, 600) });

        return true;
    }

    /// <summary>
    /// Requests a return of some items from a delivered order. Each quantity is at most what was bought minus what is
    /// already in other open or approved returns.
    /// </summary>
    public EcommerceReturn? RequestReturn(int orderId, IReadOnlyList<(int ProductId, string Color, int Quantity)> items, string reason, string? comment)
    {
        if (FindOrder(orderId) is not { Status: EcommerceOrderStatus.Delivered } order || items.Count == 0 || string.IsNullOrWhiteSpace(reason))
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
        Changed?.Invoke();

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
    /// a new one. Returns <see langword="false" /> when another coupon already has the code.
    /// </summary>
    public bool SaveCoupon(EcommerceCoupon coupon, string? originalCode)
    {
        var code = coupon.Code.Trim().ToUpperInvariant();

        if (coupons.Any(existing => string.Equals(existing.Code, code, StringComparison.OrdinalIgnoreCase) && !string.Equals(existing.Code, originalCode, StringComparison.OrdinalIgnoreCase)))
            return false;

        var saved = coupon with
        {
            Code = code,
            Description = coupon.Description.Trim(),
            Value = Math.Max(0m, coupon.Kind == EcommerceCouponKind.Percent ? Math.Min(coupon.Value, 100m) : coupon.Value),
            MinSubtotal = Math.Max(0m, coupon.MinSubtotal),
        };
        var index = originalCode is null ? -1 : coupons.FindIndex(existing => string.Equals(existing.Code, originalCode, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
        {
            coupons.Add(saved with { Uses = 0 });
        }
        else
        {
            coupons[index] = saved with { Uses = coupons[index].Uses };

            if (string.Equals(AppliedPromoCode, originalCode, StringComparison.OrdinalIgnoreCase))
                AppliedPromoCode = code;
        }

        Changed?.Invoke();

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

        return true;
    }

    /// <summary>Turns a coupon on or off.</summary>
    public bool SetCouponActive(string code, bool active)
    {
        if (FindCoupon(code) is not { } coupon || coupon.IsActive == active)
            return false;

        ReplaceCoupon(coupon with { IsActive = active });

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

        Changed?.Invoke();
    }

    /// <summary>Publishes or hides a review; hidden reviews leave the product page and its rating.</summary>
    public bool SetReviewStatus(int reviewId, EcommerceReviewStatus status)
    {
        if (FindReview(reviewId) is not { } review || review.Status == status || !Enum.IsDefined(status))
            return false;

        ReplaceReview(review with { Status = status });

        return true;
    }

    /// <summary>Sets a product's stock (0 to 9,999); cart lines above the new stock shrink to fit.</summary>
    public bool SetStock(int productId, int stock)
    {
        if (FindAnyProduct(productId) is not { } product)
            return false;

        ReplaceProduct(product with { Stock = Math.Clamp(stock, 0, 9999) });

        return true;
    }

    /// <summary>Validates a stored snapshot and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        EcommerceSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<EcommerceSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }

        if (snapshot is not
            {
                Products: { Count: > 0 and <= MaxListLength } storedProducts,
                Cart: { Count: <= 50 } storedCart,
                Orders: { Count: <= MaxListLength } storedOrders,
                Coupons: { Count: <= 100 } storedCoupons,
                Reviews: { Count: <= 2000 } storedReviews,
                Returns: { Count: <= MaxListLength } storedReturns,
            }
            || snapshot.NextOrderId is < FirstNewOrderId or > FirstNewOrderId + 100_000
            || snapshot.NextProductId is < 1 or > 100_000
            || snapshot.NextReviewId is < 1 or > 1_000_000
            || snapshot.NextReturnId is < 1 or > 100_000)
            return false;

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
                || (review.Reply is not null && review.Reply.Length > 600) || !IsPlausibleDate(review.PostedOn))
            || storedReviews.Select(review => review.Id).Distinct().Count() != storedReviews.Count)
            return false;

        if (storedReturns.Any(item => item is null || item.Id < 1 || item.Id >= snapshot.NextReturnId || !orderIds.ContainsKey(item.OrderId) || !Enum.IsDefined(item.Status)
                || !IsText(item.Reason, 120) || (item.Comment is not null && item.Comment.Length > 500) || (item.DecisionNote is not null && item.DecisionNote.Length > 200)
                || !IsPlausibleDate(item.RequestedOn) || item.Lines is not { Count: >= 1 and <= 50 }
                || item.Lines.Any(line => line is null || !byId.ContainsKey(line.ProductId) || !IsText(line.Name, 120) || !IsText(line.Color, 40) || line.Quantity is < 1 or > EcommerceSeedData.MaxQuantity || !IsMoney(line.UnitPrice)))
            || storedReturns.Select(item => item.Id).Distinct().Count() != storedReturns.Count)
            return false;

        products.Clear();
        products.AddRange(storedProducts);
        cart.Clear();
        cart.AddRange(storedCart);
        AppliedPromoCode = snapshot.PromoCode;
        orders.Clear();
        orders.AddRange(storedOrders);
        coupons.Clear();
        coupons.AddRange(storedCoupons);
        reviews.Clear();
        reviews.AddRange(storedReviews);
        returns.Clear();
        returns.AddRange(storedReturns);
        nextOrderId = snapshot.NextOrderId;
        nextProductId = snapshot.NextProductId;
        nextReviewId = snapshot.NextReviewId;
        nextReturnId = snapshot.NextReturnId;

        return true;
    }

    /// <summary>Updates prices in one go: each entry sets a product's price and compare-at ("was") price. Returns how many changed.</summary>
    public int UpdatePrices(IEnumerable<(int ProductId, decimal Price, decimal? CompareAtPrice)> changes)
    {
        var changed = 0;

        foreach (var (productId, price, compareAt) in changes)
        {
            if (FindAnyProduct(productId) is not { } product || price is <= 0 or > 100_000)
                continue;

            var was = compareAt is { } value && value > price ? value : (decimal?)null;

            if (product.Price == price && product.CompareAtPrice == was)
                continue;

            ReplaceProduct(product with { Price = price, CompareAtPrice = was }, notify: false);
            changed++;
        }

        if (changed > 0)
            Changed?.Invoke();

        return changed;
    }

    /// <summary>Updates a product from the editor's draft.</summary>
    public bool UpdateProduct(int productId, EcommerceProductDraft draft)
    {
        if (FindAnyProduct(productId) is not { } product)
            return false;

        var updated = FromDraft(productId, draft, product.AddedOn, product.IsArchived);

        // Cart lines in a color the product no longer has are removed.
        cart.RemoveAll(line => line.ProductId == productId && !updated.Colors.Contains(line.Color));
        ReplaceProduct(updated);

        return true;
    }

    #endregion

    #region Private Methods

    private static IReadOnlyList<EcommerceStatusChange> Append(EcommerceOrder order, EcommerceOrderStatus status, string? note) =>
        order.History.Append(new EcommerceStatusChange(status, DateTime.MinValue, note)).ToList();

    private void ChangeStock(int productId, int delta)
    {
        if (FindAnyProduct(productId) is { } product)
            ReplaceProduct(product with { Stock = Math.Clamp(product.Stock + delta, 0, 9999) }, notify: false);
    }

    private static string? Clean(string? text, int maxLength)
    {
        var trimmed = text?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static EcommerceProduct FromDraft(int id, EcommerceProductDraft draft, DateTime addedOn, bool isArchived)
    {
        var colors = draft.Colors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(color => color.Length > 20 ? color[..20] : color).Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList();
        var highlights = draft.Highlights.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Length > 80 ? line[..80] : line).Take(6).ToList();
        var price = Math.Clamp(Math.Round(draft.Price, 2), 0.01m, 100_000m);
        var compareAt = draft.CompareAtPrice is { } was && Math.Round(was, 2) > price ? Math.Round(was, 2) : (decimal?)null;

        return new EcommerceProduct(id, draft.Name.Trim(), draft.CategoryId, price, compareAt, draft.Description.Trim(), highlights,
            colors.Count == 0 ? new List<string> { "Natural" } : colors, draft.IsFeatured, Math.Clamp(draft.Stock, 0, 9999), addedOn, isArchived);
    }

    /// <summary>Checks a money amount; the cap is above anything checkout can produce (50 lines × 10 × $100,000, plus tax).</summary>
    private static bool IsMoney(decimal value) => value is >= 0 and <= 100_000_000;

    /// <summary>Checks a date; the window is wider than anything the site clock or the editors can produce.</summary>
    private static bool IsPlausibleDate(DateTime value) => value >= EcommerceSeedData.Today.AddYears(-1) && value <= EcommerceSeedData.Today.AddYears(5);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private void ReplaceCoupon(EcommerceCoupon coupon)
    {
        var index = coupons.FindIndex(existing => existing.Code == coupon.Code);
        coupons[index] = coupon;
        Changed?.Invoke();
    }

    /// <summary>Replaces an order; a history entry added with <see cref="DateTime.MinValue" /> is stamped with the site clock.</summary>
    private void ReplaceOrder(EcommerceOrder order, bool notify = true)
    {
        var stamped = order.History.Any(change => change.At == DateTime.MinValue)
            ? order with { History = order.History.Select(change => change.At == DateTime.MinValue ? change with { At = Clock.Now } : change).ToList() }
            : order;

        orders[orders.FindIndex(existing => existing.Id == order.Id)] = stamped;

        if (notify)
            Changed?.Invoke();
    }

    private void ReplaceProduct(EcommerceProduct product, bool notify = true)
    {
        products[products.FindIndex(existing => existing.Id == product.Id)] = product;

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
            Changed?.Invoke();
    }

    private void ReplaceReturn(EcommerceReturn item)
    {
        returns[returns.FindIndex(existing => existing.Id == item.Id)] = item;
        Changed?.Invoke();
    }

    private void ReplaceReview(EcommerceReview review)
    {
        reviews[reviews.FindIndex(existing => existing.Id == review.Id)] = review;
        Changed?.Invoke();
    }

    private static bool ValidCoupons(List<EcommerceCoupon> list) =>
        list.All(coupon => coupon is not null && IsText(coupon.Code, 20) && coupon.Code.All(ch => char.IsAsciiLetterOrDigit(ch)) && coupon.Code == coupon.Code.ToUpperInvariant()
            && IsText(coupon.Description, 120) && Enum.IsDefined(coupon.Kind) && coupon.Value is > 0 and <= 10_000 && (coupon.Kind != EcommerceCouponKind.Percent || coupon.Value <= 100)
            && IsMoney(coupon.MinSubtotal) && coupon.Uses is >= 0 and <= 1_000_000 && coupon.UsageLimit is null or (>= 1 and <= 1_000_000)
            && (coupon.ExpiresOn is null || IsPlausibleDate(coupon.ExpiresOn.Value)))
        && list.Select(coupon => coupon.Code).Distinct().Count() == list.Count;

    private bool ValidOrder(EcommerceOrder? order, IReadOnlyDictionary<int, EcommerceProduct> byId, int nextId) =>
        order is not null
        && order.Id >= FirstHistoryOrderId && order.Id < nextId
        && IsPlausibleDate(order.PlacedOn)
        && Enum.IsDefined(order.Delivery) && Enum.IsDefined(order.Status)
        && (order.PromoCode is null || IsText(order.PromoCode, 20))
        && order.CardBrand is not null && cardBrands.Contains(order.CardBrand)
        && order.CardLast4 is { Length: 4 } last4 && last4.All(char.IsAsciiDigit)
        && (order.TrackingNumber is null || IsText(order.TrackingNumber, 40))
        && (order.CustomerId is null || Identity.DemoAccounts.Any(demo => demo.Id == order.CustomerId) || order.CustomerId.StartsWith("u-", StringComparison.Ordinal))
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
            && product.Stock is >= 0 and <= 9999 && IsPlausibleDate(product.AddedOn.AddYears(1)))
        && list.Select(product => product.Id).Distinct().Count() == list.Count;

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

    /// <summary>Gets the demo sign-in: a customer and a retailer account.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the site shows the dark color mode (not part of the snapshot; the theme switcher keeps its own choice).</summary>
    public bool IsDarkTheme { get; private set; }

    /// <summary>Gets whether the signed-in account is store staff (the retailer), who see the shop but don't buy.</summary>
    public bool IsStaff => Identity.IsInRole(new[] { EcommerceSeedData.RetailerRole });

    /// <summary>Gets all orders, oldest first: the seeded history, then orders placed in this tab.</summary>
    public IReadOnlyList<EcommerceOrder> Orders => orders;

    /// <summary>Gets the active (not archived) catalog the storefront shows.</summary>
    public IReadOnlyList<EcommerceProduct> Products => products.Where(product => !product.IsArchived).ToList();

    /// <summary>Gets all return requests.</summary>
    public IReadOnlyList<EcommerceReturn> Returns => returns;

    /// <summary>Gets the site slug used in the storage key.</summary>
    public string SiteSlug => "ecommerce";

    /// <summary>Gets the snapshot format version (2: orders with status, editable catalog, coupons, moderation, returns).</summary>
    public int SnapshotVersion => 2;

    #endregion

    #region Events

    /// <summary>Raised when anything in the store changes, including sign-in.</summary>
    public event Action? Changed;

    /// <summary>Raised when the color mode changes.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Nested Types

    /// <summary>The store's part of the tab snapshot.</summary>
    private sealed class EcommerceSnapshot
    {
        #region Properties, Indexers

        public List<EcommerceCartLine>? Cart { get; set; }

        public List<EcommerceCoupon>? Coupons { get; set; }

        public int NextOrderId { get; set; }

        public int NextProductId { get; set; }

        public int NextReturnId { get; set; }

        public int NextReviewId { get; set; }

        public List<EcommerceOrder>? Orders { get; set; }

        public List<EcommerceProduct>? Products { get; set; }

        public string? PromoCode { get; set; }

        public List<EcommerceReturn>? Returns { get; set; }

        public List<EcommerceReview>? Reviews { get; set; }

        #endregion
    }

    #endregion
}
