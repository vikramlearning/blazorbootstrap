using BlazorBootstrap.Demo.RCL.Models.Showcase.Restaurant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Restaurant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Restaurant;

/// <summary>
/// Menu item: photo, labels, description, a <see cref="NumberInput{TValue}" /> quantity, special instructions, and an
/// add button that announces the result. An unknown ID renders the site's not-found screen.
/// </summary>
public partial class ItemDetail : ComponentBase
{
    #region Fields and Constants

    private string? addedMessage;

    private string? error;

    private int? loadedId;

    private string? note;

    private int quantity = 1;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        // Another item starts with a quantity of 1, no note, and no messages.
        if (loadedId == Id)
            return;

        loadedId = Id;
        quantity = 1;
        note = null;
        error = null;
        addedMessage = null;
    }

    #endregion

    #region Private Methods

    private void Add()
    {
        if (Item is null)
            return;

        // NumberInput reports 0 when the box is cleared, so the range is checked here too.
        if (quantity is < 1 or > RestaurantSeedData.MaxQuantity)
        {
            error = $"Choose a quantity from 1 to {RestaurantSeedData.MaxQuantity}.";
            return;
        }

        error = null;
        var added = State.AddToCart(Item.Id, quantity, note);
        var inCart = State.Cart.FirstOrDefault(line => line.ItemId == Item.Id)?.Quantity ?? 0;

        addedMessage = added == quantity
            ? $"Added {added} × {Item.Name}. You have {inCart} in your order."
            : added > 0
                ? $"Added {added} × {Item.Name}; your order now has the most we can make ({RestaurantSeedData.MaxQuantity})."
                : $"Your order already has the most we can make of {Item.Name} ({RestaurantSeedData.MaxQuantity}).";

        Toasts.Notify(added > 0
            ? new ToastMessage(ToastType.Success, IconName.Basket2, "Added to your order", addedMessage)
            : new ToastMessage(ToastType.Warning, IconName.ExclamationTriangle, "Limit reached", addedMessage));
    }

    private void OnNoteChanged(string? value) => note = value;

    private void OnQuantityChanged(int value)
    {
        quantity = value;

        if (error is not null && value is >= 1 and <= RestaurantSeedData.MaxQuantity)
            error = null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Menu", Href = RestaurantLayout.Base },
        new() { Text = Item?.Name, IsCurrentPage = true },
    };

    private RestaurantCategory Category => State.FindCategory(Item?.CategoryId);

    /// <summary>Gets or sets the menu item ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private RestaurantMenuItem? Item => State.FindItem(Id);

    private IReadOnlyList<RestaurantMenuItem> Related => Item is null
        ? Array.Empty<RestaurantMenuItem>()
        : State.Menu.Where(other => other.CategoryId == Item.CategoryId && other.Id != Item.Id).ToList();

    [Inject] private RestaurantState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
