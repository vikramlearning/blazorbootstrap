using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Product page: image, rating, price, color and quantity, add to cart, the wishlist heart, details, reviews with a
/// write-a-review form, and related products. An unknown ID renders the site's not-found screen. A signed-in customer's
/// name is filled into the review form, and their own reviews have Edit (a dialog) and Delete (confirmed) buttons; focus
/// returns to the button that opened a dialog, or to the reviews heading once the review is gone.
/// </summary>
public partial class ProductDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, Button> deleteReviewButtons = new();

    private readonly Dictionary<int, Button> editReviewButtons = new();

    private string? addedMessage;

    private Modal deleteReviewModal = default!;

    private EcommerceReview? deletingReview;

    private EditContext editReviewContext = default!;

    private EditReviewForm editReviewForm = new();

    private Modal editReviewModal = default!;

    private int? editingReviewId;

    private int? loadedId;

    private int quantity = 1;

    private EditContext reviewContext = default!;

    /// <summary>Whether the delete dialog removed (or tried to remove) the review, so focus goes to the reviews heading.</summary>
    private bool reviewDeleted;

    private ReviewForm reviewForm = new();

    /// <summary>Whether the signed-in customer's name was filled into the review form; it happens once per product.</summary>
    private bool reviewNamePrefilled;

    private bool reviewPosted;

    private ElementReference reviewsHeading;

    private string? reviewStatus;

    private int reviewStatusCount;

    private string? reviewStatusText;

    private string selectedColor = string.Empty;

    private Tabs? tabs;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        editReviewContext = new EditContext(editReviewForm);
        State.Changed += OnStateChanged;
    }

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
        reviewStatus = null;
        reviewStatusCount = 0;
        reviewStatusText = null;
        reviewForm = new ReviewForm();
        reviewContext = new EditContext(reviewForm);
        reviewNamePrefilled = false;
        PrefillReviewName();
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

    private async Task AskDeleteReviewAsync(EcommerceReview review)
    {
        deletingReview = review;
        reviewDeleted = false;

        await deleteReviewModal.ShowAsync();
    }

    private async Task ConfirmDeleteReviewAsync()
    {
        if (deletingReview is { } review)
        {
            SetReviewStatus(State.DeleteOwnReview(review.Id)
                ? $"Your review \"{review.Title}\" is deleted."
                : "Your review couldn't be deleted. It may already be gone.");
            reviewDeleted = true;
        }

        await deleteReviewModal.HideAsync();
    }

    /// <summary>Opens the edit dialog with the review's rating, title, and text; a new form (and EditContext) each time.</summary>
    private async Task EditReviewAsync(EcommerceReview review)
    {
        editingReviewId = review.Id;
        editReviewForm = new EditReviewForm { Body = review.Body, Rating = review.Rating, Title = review.Title };
        editReviewContext = new EditContext(editReviewForm);

        await editReviewModal.ShowAsync();
    }

    /// <summary>Returns whether the signed-in account wrote a review (a guest's review has no account, so it never matches).</summary>
    private bool IsOwnReview(EcommerceReview review) => State.Identity.Current is { } account && review.AuthorAccountId == account.Id;

    private void OnColorChanged(string color) => selectedColor = color;

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: back to Delete when kept, to the heading once the review is gone.</summary>
    private async Task OnDeleteReviewHiddenAsync()
    {
        var review = deletingReview;
        deletingReview = null;

        if (reviewDeleted)
        {
            reviewDeleted = false;
            await EcommerceDisplay.FocusAsync(reviewsHeading);
        }
        else if (review is not null && deleteReviewButtons.TryGetValue(review.Id, out var button))
        {
            await EcommerceDisplay.FocusAsync(button.Element);
        }
    }

    /// <summary>Focus goes back to the review's Edit button (saved or cancelled); if the review is gone, to the reviews heading.</summary>
    private async Task OnEditReviewHiddenAsync()
    {
        var reviewId = editingReviewId;
        editingReviewId = null;

        if (reviewId is { } id && Reviews.Any(review => review.Id == id) && editReviewButtons.TryGetValue(id, out var button))
            await EcommerceDisplay.FocusAsync(button.Element);
        else
            await EcommerceDisplay.FocusAsync(reviewsHeading);
    }

    private void OnQuantityChanged(int value) => quantity = value;

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // The signed-in account may only be known once the tab's stored session is restored, after the first render.
        PrefillReviewName();
        StateHasChanged();
    });

    /// <summary>Fills the signed-in customer's name into an empty review name, once per product.</summary>
    private void PrefillReviewName()
    {
        if (reviewNamePrefilled || Customer is not { } account)
            return;

        reviewNamePrefilled = true;

        if (string.IsNullOrWhiteSpace(reviewForm.Name))
            reviewForm.Name = account.Name;
    }

    private async Task SaveReviewEditAsync()
    {
        if (editingReviewId is not { } id)
            return;

        SetReviewStatus(State.UpdateOwnReview(id, editReviewForm.Rating, editReviewForm.Title, editReviewForm.Body)
            ? $"Your review \"{editReviewForm.Title.Trim()}\" is updated."
            : "Your review couldn't be saved. It may have been removed.");

        await editReviewModal.HideAsync();
    }

    /// <summary>Shows a review status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetReviewStatus(string message)
    {
        reviewStatusCount = message == reviewStatusText ? reviewStatusCount + 1 : 1;
        reviewStatusText = message;
        reviewStatus = reviewStatusCount == 1 ? message : $"{message} ({reviewStatusCount})";
    }

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

        // Reset the fields on the same model: a new EditContext would rebuild the whole form and drop focus from "Post review".
        // A signed-in customer keeps their name for the next review.
        reviewForm.Body = string.Empty;
        reviewForm.Name = Customer?.Name ?? string.Empty;
        reviewForm.Rating = 0;
        reviewForm.Title = string.Empty;
        reviewContext.MarkAsUnmodified();
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

    /// <summary>Gets the signed-in customer, or <see langword="null" /> when signed out or signed in as staff.</summary>
    private ShowcaseAccount? Customer => State.Identity.Current is { RoleId: EcommerceSeedData.CustomerRole } account ? account : null;

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

    /// <summary>The fields of an own review that can be edited, with the same rules as <see cref="ReviewForm" />.</summary>
    private sealed class EditReviewForm
    {
        [Required(ErrorMessage = "Write a few words about the product.")]
        [StringLength(600, MinimumLength = 10, ErrorMessage = "Use 10 to 600 characters.")]
        public string Body { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Choose a rating from 1 to 5 stars.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Add a short title.")]
        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string Title { get; set; } = string.Empty;
    }

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
