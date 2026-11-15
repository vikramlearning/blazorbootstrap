using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's review moderation: every review, newest first, filtered by status, rating, product, and whether it still
/// needs a reply, and paged. A review can be hidden (after a confirmation: it leaves the product page and its rating) or
/// published again, and the store can post, edit, or remove a public reply. The summary shows the store's average
/// published rating and the counts.
/// </summary>
public partial class AdminReviews : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int PageSize = 10;

    private readonly Dictionary<int, Button> removeButtons = new();

    private readonly Dictionary<int, Button> replyButtons = new();

    private readonly Dictionary<int, Button> statusButtons = new();

    /// <summary>The review the open dialog acts on.</summary>
    private EcommerceReview? acting;

    private Modal hideModal = default!;

    private bool needsReply;

    private int page = 1;

    /// <summary>The element to focus once the next render has updated the list.</summary>
    private Func<ElementReference?>? pendingFocus;

    private int productFilter;

    private int ratingFilter;

    private Modal removeModal = default!;

    private bool removed;

    private EditContext replyContext = default!;

    private ReplyForm replyForm = new();

    private Modal replyModal = default!;

    private int statusCount;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingFocus is { } target)
        {
            pendingFocus = null;
            await EcommerceDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        replyContext = new EditContext(replyForm);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private async Task AskRemoveReplyAsync(EcommerceReview review)
    {
        acting = review;
        removed = false;

        await removeModal.ShowAsync();
    }

    private async Task ConfirmHideAsync()
    {
        if (acting is { } review && State.SetReviewStatus(review.Id, EcommerceReviewStatus.Hidden))
            SetStatus($"The review “{review.Title}” is hidden. It no longer shows on the product page or counts toward its rating.");

        await hideModal.HideAsync();
    }

    private async Task ConfirmRemoveReplyAsync()
    {
        if (acting is { } review && State.ReplyToReview(review.Id, null))
        {
            removed = true;
            SetStatus($"The reply to “{review.Title}” is removed.");
        }

        await removeModal.HideAsync();
    }

    /// <summary>
    /// Focus for after an action: the review's button when the review is still on this page, otherwise the status message
    /// (a filter may have taken the review out of the list). Decided after the render, so it sees the updated list.
    /// </summary>
    private Func<ElementReference?> FocusTargetFor(int reviewId, Dictionary<int, Button> buttons) => () =>
        PageItems.Any(review => review.Id == reviewId) && buttons.TryGetValue(reviewId, out var button) ? button.Element : statusRegion;

    /// <summary>The dialogs open from code, so Bootstrap does not return focus: it goes back to the review's Hide/Publish button.</summary>
    private void OnHideHidden()
    {
        if (acting is { } review)
            pendingFocus = FocusTargetFor(review.Id, statusButtons);

        acting = null;
    }

    private void OnNeedsReplyChanged(bool value)
    {
        if (needsReply == value)
            return;

        needsReply = value;
        page = 1;
    }

    private void OnPageChanged(int value) => page = value;

    private void OnProductFilterChanged(ChangeEventArgs args)
    {
        productFilter = int.TryParse(args.Value?.ToString(), out var id) && id > 0 ? id : 0;
        page = 1;
    }

    private void OnRatingFilterChanged(ChangeEventArgs args)
    {
        ratingFilter = int.TryParse(args.Value?.ToString(), out var stars) && stars is >= 1 and <= 5 ? stars : 0;
        page = 1;
    }

    /// <summary>After removing a reply its button is gone, so focus moves to the review's Reply button; otherwise back to Remove reply.</summary>
    private void OnRemoveHidden()
    {
        if (acting is { } review)
            pendingFocus = FocusTargetFor(review.Id, removed ? replyButtons : removeButtons);

        acting = null;
        removed = false;
    }

    private void OnReplyHidden()
    {
        if (acting is { } review)
            pendingFocus = FocusTargetFor(review.Id, replyButtons);

        acting = null;
        replyForm = new ReplyForm();
        replyContext = new EditContext(replyForm);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnStatusFilterChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        statusFilter = Enum.TryParse<EcommerceReviewStatus>(value, out var status) && Enum.IsDefined(status) ? status.ToString() : string.Empty;
        page = 1;
    }

    private async Task OpenReplyAsync(EcommerceReview review)
    {
        acting = review;
        replyForm = new ReplyForm { Text = review.Reply ?? string.Empty };
        replyContext = new EditContext(replyForm);

        await replyModal.ShowAsync();
    }

    private async Task SaveReplyAsync()
    {
        if (acting is { } review && State.ReplyToReview(review.Id, replyForm.Text))
            SetStatus(review.Reply is null ? $"Your reply to “{review.Title}” is posted." : $"Your reply to “{review.Title}” is updated.");

        await replyModal.HideAsync();
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>Hiding asks first, since it changes what shoppers see and the product's rating; publishing again happens at once.</summary>
    private async Task ToggleStatusAsync(EcommerceReview review)
    {
        if (review.Status == EcommerceReviewStatus.Published)
        {
            acting = review;
            await hideModal.ShowAsync();
            return;
        }

        if (State.SetReviewStatus(review.Id, EcommerceReviewStatus.Published))
        {
            SetStatus($"The review “{review.Title}” is published. It shows on the product page and counts toward its rating.");
            pendingFocus = FocusTargetFor(review.Id, statusButtons);
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private int CurrentPage => Math.Clamp(page, 1, TotalPages);

    private IReadOnlyList<EcommerceReview> Matches
    {
        get
        {
            IEnumerable<EcommerceReview> reviews = State.AllReviews;

            if (Enum.TryParse<EcommerceReviewStatus>(statusFilter, out var status))
                reviews = reviews.Where(review => review.Status == status);

            if (ratingFilter > 0)
                reviews = reviews.Where(review => review.Rating == ratingFilter);

            if (productFilter > 0)
                reviews = reviews.Where(review => review.ProductId == productFilter);

            if (needsReply)
                reviews = reviews.Where(review => review.Reply is null);

            return reviews.OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).ToList();
        }
    }

    private string MatchText
    {
        get
        {
            var total = State.AllReviews.Count;
            var count = Matches.Count;
            var text = count == total ? $"All {total} reviews" : $"{count} of {total} reviews";

            return TotalPages > 1 ? $"{text} · page {CurrentPage} of {TotalPages}" : text;
        }
    }

    private IReadOnlyList<EcommerceReview> PageItems => Matches.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>The products that have at least one review, for the product filter.</summary>
    private IReadOnlyList<EcommerceProduct> ReviewedProducts => State.AllProducts
        .Where(product => State.AllReviews.Any(review => review.ProductId == product.Id))
        .OrderBy(product => product.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    [Inject] private EcommerceState State { get; set; } = default!;

    private string SummaryText
    {
        get
        {
            var reviews = State.AllReviews;
            var published = reviews.Where(review => review.Status == EcommerceReviewStatus.Published).ToList();
            var hidden = reviews.Count - published.Count;
            var unanswered = reviews.Count(review => review.Reply is null);
            var average = published.Count == 0
                ? "No published reviews yet"
                : $"Average published rating {EcommerceDisplay.Rating(published.Average(review => review.Rating))} out of 5";

            return $"{average} · {reviews.Count} {(reviews.Count == 1 ? "review" : "reviews")}: {published.Count} published, {hidden} hidden · "
                + $"{unanswered} {(unanswered == 1 ? "needs" : "need")} a reply";
        }
    }

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(Matches.Count / (double)PageSize));

    #endregion

    #region Nested Types

    private sealed class ReplyForm
    {
        private string text = string.Empty;

        [Required(ErrorMessage = "Write a reply.")]
        [StringLength(600, MinimumLength = 2, ErrorMessage = "Use 2 to 600 characters.")]
        public string Text { get => text; set => text = value?.Trim() ?? string.Empty; }
    }

    #endregion
}
