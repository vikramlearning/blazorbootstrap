using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Product page: image, rating, price, color and quantity, add to cart, details, reviews with a write-a-review form,
/// and related products. An unknown ID renders the site's not-found screen.
/// </summary>
public partial class ProductDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? addedMessage;

    private int? loadedId;

    private int quantity = 1;

    private EditContext reviewContext = default!;

    private ReviewForm reviewForm = new();

    private bool reviewPosted;

    private string selectedColor = string.Empty;

    private Tabs? tabs;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Only a new product (a related-product link) starts fresh choices and an empty review form.
        if (loadedId == Id)
            return;

        loadedId = Id;
        selectedColor = Product?.Colors[0] ?? string.Empty;
        quantity = 1;
        addedMessage = null;
        reviewPosted = false;
        reviewForm = new ReviewForm();
        reviewContext = new EditContext(reviewForm);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void AddToCart()
    {
        if (Product is null)
            return;

        var added = State.AddToCart(Product.Id, selectedColor, quantity);

        addedMessage = added > 0
            ? $"Added {added} × {Product.Name} ({selectedColor}) to your cart."
            : $"Your cart already holds the most we can sell of {Product.Name} ({selectedColor}).";

        Toasts.Notify(added > 0
            ? new ToastMessage(ToastType.Success, IconName.BagCheck, "Added to cart", addedMessage)
            : new ToastMessage(ToastType.Warning, IconName.ExclamationTriangle, "Limit reached", addedMessage));
    }

    private void OnColorChanged(string color) => selectedColor = color;

    private void OnQuantityChanged(int value) => quantity = value;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task ShowReviewsAsync()
    {
        if (tabs is not null)
            await tabs.ShowTabByNameAsync("reviews");
    }

    private void SubmitReview()
    {
        if (Product is null)
            return;

        State.AddReview(Product.Id, reviewForm.Name, reviewForm.Rating, reviewForm.Title, reviewForm.Body);
        reviewForm = new ReviewForm();
        reviewContext = new EditContext(reviewForm);
        reviewPosted = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private decimal AverageRating => Product is null ? 0 : Math.Round((decimal)State.AverageRating(Product.Id) * 2, MidpointRounding.AwayFromZero) / 2;

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Home", Href = EcommerceLayout.Base },
        new() { Text = Category?.Name ?? "Shop", Href = $"{EcommerceLayout.Base}/products?category={Product?.CategoryId}" },
        new() { Text = Product?.Name, IsCurrentPage = true },
    };

    private EcommerceCategory? Category => State.FindCategory(Product?.CategoryId);

    /// <summary>Gets or sets the product ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private EcommerceProduct? Product => State.FindProduct(Id);

    private IReadOnlyList<EcommerceProduct> Related => Product is null
        ? Array.Empty<EcommerceProduct>()
        : State.Products.Where(product => product.CategoryId == Product.CategoryId && product.Id != Product.Id).Take(4).ToList();

    private IReadOnlyList<EcommerceReview> Reviews => State.ReviewsFor(Id);

    [Inject] private EcommerceState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class ReviewForm
    {
        [Required(ErrorMessage = "Write a few words about the product.")]
        [StringLength(600, MinimumLength = 10, ErrorMessage = "Use 10 to 600 characters.")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the name to show with your review.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Use 2 to 40 characters.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Choose a rating from 1 to 5 stars.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Add a short title.")]
        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string Title { get; set; } = string.Empty;
    }

    #endregion
}
