namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;

/// <summary>Dietary labels shown on menu items.</summary>
[Flags]
public enum RestaurantDiet
{
    /// <summary>No label.</summary>
    None = 0,

    /// <summary>No meat or fish.</summary>
    Vegetarian = 1,

    /// <summary>No animal products.</summary>
    Vegan = 2,

    /// <summary>Made without gluten.</summary>
    GlutenFree = 4,

    /// <summary>Spicy.</summary>
    Spicy = 8,
}

/// <summary>How the order reaches the customer.</summary>
public enum RestaurantFulfillment
{
    /// <summary>A courier brings it.</summary>
    Delivery,

    /// <summary>The customer collects it.</summary>
    Pickup,
}

/// <summary>Where an order is.</summary>
public enum RestaurantOrderStatus
{
    /// <summary>The kitchen has the order.</summary>
    Received,

    /// <summary>Being cooked.</summary>
    Preparing,

    /// <summary>With the courier, or ready at the counter for pickup.</summary>
    OnTheWay,

    /// <summary>Delivered or collected.</summary>
    Completed,
}

/// <summary>A menu category.</summary>
public sealed record RestaurantCategory(string Id, string Name, IconName IconName, string Accent);

/// <summary>A dish or drink on the menu. Names and descriptions are fictitious.</summary>
public sealed record RestaurantMenuItem(int Id, string Name, string CategoryId, string Description, decimal Price, RestaurantDiet Diet, int Calories, int PrepMinutes, bool IsPopular)
{
    #region Properties, Indexers

    /// <summary>Gets the photo path; <c>ShowcaseImage</c> shows a placeholder until the file is supplied.</summary>
    public string ImagePath => $"_content/BlazorBootstrap.Demo.RCL/showcase/restaurant/menu/{Id}.webp";

    #endregion
}

/// <summary>A line in the cart. The note is the customer's own text and is only ever rendered as text.</summary>
public sealed class RestaurantCartLine
{
    #region Properties, Indexers

    /// <summary>Gets the menu item ID.</summary>
    public int ItemId { get; init; }

    /// <summary>Gets or sets the special instructions, if any.</summary>
    public string? Note { get; set; }

    /// <summary>Gets or sets the quantity, from 1 to the per-item maximum.</summary>
    public int Quantity { get; set; }

    #endregion
}

/// <summary>A fictitious delivery address with a map position near the restaurant.</summary>
public sealed record RestaurantAddress(string Id, string Label, string Street, string Neighborhood, double Latitude, double Longitude, int Minutes);

/// <summary>The price of a cart or order.</summary>
public sealed record RestaurantTotals(decimal Subtotal, decimal DeliveryFee, decimal Tax, decimal Tip)
{
    #region Properties, Indexers

    /// <summary>Gets the total to pay.</summary>
    public decimal Total => Subtotal + DeliveryFee + Tax + Tip;

    #endregion
}

/// <summary>A placed order with its tracking timeline.</summary>
public sealed class RestaurantOrder
{
    #region Properties, Indexers

    /// <summary>Gets the delivery address, or <see langword="null" /> for pickup.</summary>
    public RestaurantAddress? Address { get; init; }

    /// <summary>Gets the courier's notes for the address (apartment, gate code), if any.</summary>
    public string? AddressNote { get; init; }

    /// <summary>Gets the customer's name.</summary>
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>Gets delivery or pickup.</summary>
    public RestaurantFulfillment Fulfillment { get; init; }

    /// <summary>Gets the order number.</summary>
    public int Id { get; init; }

    /// <summary>Gets the ordered lines.</summary>
    public List<RestaurantCartLine> Lines { get; init; } = new();

    /// <summary>Gets when the order was placed.</summary>
    public DateTime PlacedAt { get; init; }

    /// <summary>Gets when the order is expected.</summary>
    public DateTime ReadyBy { get; init; }

    /// <summary>Gets or sets the status.</summary>
    public RestaurantOrderStatus Status { get; set; }

    /// <summary>Gets the time each status was reached.</summary>
    public Dictionary<RestaurantOrderStatus, DateTime> StatusTimes { get; init; } = new();

    /// <summary>Gets the prices at the time of ordering.</summary>
    public RestaurantTotals Totals { get; init; } = new(0, 0, 0, 0);

    #endregion
}

/// <summary>Menu filters, kept in the site state so they survive opening an item and coming back.</summary>
public sealed class RestaurantFilter
{
    #region Properties, Indexers

    /// <summary>Gets or sets the category ID; empty means the whole menu.</summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>Gets whether any filter is set.</summary>
    public bool IsActive => CategoryId.Length > 0 || VegetarianOnly || !string.IsNullOrWhiteSpace(Search);

    /// <summary>Gets or sets the search text, matched against names and descriptions.</summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>Gets or sets whether only vegetarian (and vegan) items show.</summary>
    public bool VegetarianOnly { get; set; }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Clears every filter.</summary>
    public void Clear()
    {
        CategoryId = string.Empty;
        Search = string.Empty;
        VegetarianOnly = false;
    }

    #endregion

    #endregion
}
