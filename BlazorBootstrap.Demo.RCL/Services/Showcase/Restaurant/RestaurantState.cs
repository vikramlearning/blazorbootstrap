using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;

/// <summary>
/// In-memory state of the restaurant site for the current session: menu filters, the cart, and orders. Nothing is
/// persisted, sent, or charged.
/// </summary>
public sealed class RestaurantState
{
    #region Fields and Constants

    private readonly List<RestaurantCartLine> cart = new();

    private readonly List<RestaurantOrder> orders = new();

    private int nextOrderId = 7001;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session with one order delivered yesterday.</summary>
    public RestaurantState()
    {
        var placed = RestaurantSeedData.Now.AddDays(-1).AddHours(1.25);
        var lines = new List<RestaurantCartLine>
        {
            new() { ItemId = 409, Quantity = 1 },
            new() { ItemId = 412, Quantity = 1 },
            new() { ItemId = 418, Quantity = 2 },
        };

        orders.Add(new RestaurantOrder
        {
            Id = 7000,
            CustomerName = "Jordan Ashdown",
            Fulfillment = RestaurantFulfillment.Delivery,
            Address = Addresses[0],
            PlacedAt = placed,
            ReadyBy = placed.AddMinutes(32),
            Status = RestaurantOrderStatus.Completed,
            Lines = lines,
            Totals = Totals(lines, RestaurantFulfillment.Delivery, 10),
            StatusTimes =
            {
                [RestaurantOrderStatus.Received] = placed,
                [RestaurantOrderStatus.Preparing] = placed.AddMinutes(3),
                [RestaurantOrderStatus.OnTheWay] = placed.AddMinutes(16),
                [RestaurantOrderStatus.Completed] = placed.AddMinutes(30),
            },
        });
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Adds an item to the cart, or more of it; the line keeps at most <see cref="RestaurantSeedData.MaxQuantity" />.
    /// A non-empty note replaces the line's note. Returns how many were actually added.
    /// </summary>
    public int AddToCart(int itemId, int quantity, string? note = null)
    {
        if (FindItem(itemId) is null || quantity < 1)
            return 0;

        var line = cart.FirstOrDefault(item => item.ItemId == itemId);

        if (line is null)
        {
            line = new RestaurantCartLine { ItemId = itemId, Quantity = 0 };
            cart.Add(line);
        }

        var added = Math.Min(quantity, RestaurantSeedData.MaxQuantity - line.Quantity);
        line.Quantity += added;

        if (!string.IsNullOrWhiteSpace(note))
            line.Note = note.Trim();

        if (line.Quantity == 0)
            cart.Remove(line);

        Changed?.Invoke();

        return added;
    }

    /// <summary>Moves an order to its next status, a few minutes after the last one.</summary>
    public void Advance(RestaurantOrder order)
    {
        if (order.Status == RestaurantOrderStatus.Completed)
            return;

        var last = order.StatusTimes[order.Status];
        order.Status++;
        order.StatusTimes[order.Status] = last.AddMinutes(order.Status switch
        {
            RestaurantOrderStatus.Preparing => 3,
            RestaurantOrderStatus.OnTheWay => 12,
            _ => order.Address?.Minutes ?? 2,
        });

        Changed?.Invoke();
    }

    /// <summary>Returns the cart's prices for a fulfillment and tip.</summary>
    public RestaurantTotals CartTotals(RestaurantFulfillment fulfillment, int tipPercent) => Totals(cart, fulfillment, tipPercent);

    /// <summary>Finds a delivery address by its ID, or returns <see langword="null" />.</summary>
    public RestaurantAddress? FindAddress(string? addressId) => Addresses.FirstOrDefault(address => address.Id == addressId);

    /// <summary>Finds a category by its ID; unknown IDs fall back to the first category.</summary>
    public RestaurantCategory FindCategory(string? categoryId) => Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories[0];

    /// <summary>Finds a menu item by its ID, or returns <see langword="null" />.</summary>
    public RestaurantMenuItem? FindItem(int itemId) => Menu.FirstOrDefault(item => item.Id == itemId);

    /// <summary>Finds an order by its number, or returns <see langword="null" />.</summary>
    public RestaurantOrder? FindOrder(int orderId) => orders.FirstOrDefault(order => order.Id == orderId);

    /// <summary>Returns the menu for the current filters, in menu order.</summary>
    public IReadOnlyList<RestaurantMenuItem> FilteredMenu()
    {
        var search = Filter.Search.Trim();

        return Menu
            .Where(item => Filter.CategoryId.Length == 0 || item.CategoryId == Filter.CategoryId)
            .Where(item => !Filter.VegetarianOnly || item.Diet.HasFlag(RestaurantDiet.Vegetarian) || item.Diet.HasFlag(RestaurantDiet.Vegan))
            .Where(item => search.Length == 0
                || item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || item.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Places an order from the cart and empties the cart. The ready time adds the longest preparation time and, for
    /// delivery, the travel time to the address.
    /// </summary>
    public RestaurantOrder PlaceOrder(string customerName, RestaurantFulfillment fulfillment, RestaurantAddress? address, string? addressNote, int tipPercent)
    {
        if (cart.Count == 0)
            throw new InvalidOperationException("The cart is empty, so there is no order to place.");

        var lines = cart.Select(line => new RestaurantCartLine { ItemId = line.ItemId, Quantity = line.Quantity, Note = line.Note }).ToList();
        var prep = lines.Max(line => FindItem(line.ItemId)?.PrepMinutes ?? 10);
        var travel = fulfillment == RestaurantFulfillment.Delivery ? address?.Minutes ?? 20 : 0;
        var order = new RestaurantOrder
        {
            Id = nextOrderId++,
            CustomerName = customerName.Trim(),
            Fulfillment = fulfillment,
            Address = fulfillment == RestaurantFulfillment.Delivery ? address : null,
            AddressNote = fulfillment == RestaurantFulfillment.Delivery && !string.IsNullOrWhiteSpace(addressNote) ? addressNote.Trim() : null,
            PlacedAt = RestaurantSeedData.Now,
            ReadyBy = RestaurantSeedData.Now.AddMinutes(prep + travel + 5),
            Status = RestaurantOrderStatus.Received,
            Lines = lines,
            Totals = Totals(lines, fulfillment, tipPercent),
            StatusTimes = { [RestaurantOrderStatus.Received] = RestaurantSeedData.Now },
        };

        orders.Insert(0, order);
        cart.Clear();
        LastOrderId = order.Id;
        Changed?.Invoke();

        return order;
    }

    /// <summary>Removes an item from the cart.</summary>
    public void Remove(int itemId)
    {
        if (cart.RemoveAll(line => line.ItemId == itemId) > 0)
            Changed?.Invoke();
    }

    /// <summary>Sets a line's quantity, kept from 1 to <see cref="RestaurantSeedData.MaxQuantity" />; use <see cref="Remove" /> to take it out.</summary>
    public void SetQuantity(int itemId, int quantity)
    {
        var line = cart.FirstOrDefault(item => item.ItemId == itemId);

        if (line is null)
            return;

        var clamped = Math.Clamp(quantity, 1, RestaurantSeedData.MaxQuantity);

        if (line.Quantity == clamped)
            return;

        line.Quantity = clamped;
        Changed?.Invoke();
    }

    #endregion

    #region Private Methods

    private RestaurantTotals Totals(IReadOnlyList<RestaurantCartLine> lines, RestaurantFulfillment fulfillment, int tipPercent)
    {
        var subtotal = lines.Sum(line => (FindItem(line.ItemId)?.Price ?? 0) * line.Quantity);
        var delivery = fulfillment == RestaurantFulfillment.Pickup || subtotal == 0 || subtotal >= RestaurantSeedData.FreeDeliveryFrom ? 0 : RestaurantSeedData.DeliveryFee;
        var tax = Math.Round((subtotal + delivery) * RestaurantSeedData.TaxRate, 2, MidpointRounding.AwayFromZero);
        var tip = Math.Round(subtotal * Math.Clamp(tipPercent, 0, 30) / 100m, 2, MidpointRounding.AwayFromZero);

        return new RestaurantTotals(subtotal, delivery, tax, tip);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the delivery addresses.</summary>
    public IReadOnlyList<RestaurantAddress> Addresses { get; } = RestaurantSeedData.Addresses();

    /// <summary>Gets the cart lines.</summary>
    public IReadOnlyList<RestaurantCartLine> Cart => cart;

    /// <summary>Gets how many items are in the cart.</summary>
    public int CartCount => cart.Sum(line => line.Quantity);

    /// <summary>Gets the menu categories.</summary>
    public IReadOnlyList<RestaurantCategory> Categories { get; } = RestaurantSeedData.Categories();

    /// <summary>Gets the menu filters.</summary>
    public RestaurantFilter Filter { get; } = new();

    /// <summary>Gets the number of the order placed last in this session, if any.</summary>
    public int? LastOrderId { get; private set; }

    /// <summary>Gets the menu.</summary>
    public IReadOnlyList<RestaurantMenuItem> Menu { get; } = RestaurantSeedData.Menu();

    /// <summary>Gets the orders, newest first.</summary>
    public IReadOnlyList<RestaurantOrder> Orders => orders;

    #endregion

    #region Events

    /// <summary>Raised when the cart or orders change.</summary>
    public event Action? Changed;

    #endregion
}
