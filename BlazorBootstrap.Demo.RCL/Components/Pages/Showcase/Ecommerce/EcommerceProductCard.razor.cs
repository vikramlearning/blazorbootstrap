using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// A catalog card: image, category, name, star rating, price with sale badge, and a quick "Add to cart" in the first color.
/// Stars are plain icons with a text alternative, so a grid of cards adds no extra tab stops.
/// </summary>
public partial class EcommerceProductCard : ComponentBase
{
    #region Methods

    #region Private Methods

    private void Add()
    {
        var added = State.AddToCart(Product.Id, Product.Colors[0], 1);

        Toasts.Notify(added > 0
            ? new ToastMessage(ToastType.Success, IconName.BagCheck, "Added to cart", $"{Product.Name} ({Product.Colors[0]}) is in your cart.")
            : new ToastMessage(ToastType.Warning, IconName.ExclamationTriangle, "Limit reached", $"You already have the most we can sell of {Product.Name}."));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private EcommerceCategory? Category => State.FindCategory(Product.CategoryId);

    /// <summary>Gets or sets the product shown.</summary>
    [Parameter, EditorRequired] public EcommerceProduct Product { get; set; } = default!;

    private string ProductUrl => $"{EcommerceLayout.Base}/products/{Product.Id}";

    private double Rating => State.AverageRating(Product.Id);

    private int ReviewCount => State.ReviewsFor(Product.Id).Count;

    [Inject] private EcommerceState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
