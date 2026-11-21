using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's review moderation: every review in a <see cref="Grid{TItem}" /> (title with an excerpt, product, rating,
/// author, posted, status, and reply) with paging, sorting, the column chooser, and CSV export, searched by product,
/// title, or author and filtered by status, rating, and whether it still needs a reply; its view is saved for the visit
/// and can be reset. A review has no page of its own: the row's stretched button opens it with the store's public reply
/// to post or edit. Each row can be hidden (after a confirmation: it leaves the product page and its rating) or published
/// again, and its reply removed; selected reviews are published or hidden together from a bulk bar.
/// </summary>
public partial class AdminReviews : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string NeedsReply = "needs";

    private const string Replied = "replied";

    private readonly Dictionary<int, Button> removeButtons = new();

    private readonly Dictionary<int, ElementReference> replyButtons = new();

    private readonly Dictionary<int, Button> statusButtons = new();

    /// <summary>The review the open dialog acts on.</summary>
    private int actingId;

    private ElementReference? countRegion;

    private Grid<ReviewRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private Modal hideModal = default!;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The element to focus once the next render has updated the list.</summary>
    private Func<ElementReference?>? pendingFocus;

    private int ratingFilter;

    private Modal removeModal = default!;

    private bool removed;

    private EditContext replyContext = default!;

    private string replyFilter = string.Empty;

    private ReplyForm replyForm = new();

    private Modal replyModal = default!;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The reviews selected in the grid (on its current page).</summary>
    private List<ReviewRow> selected = new();

    private int statusCount;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid; moves focus after a render that needs it.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (pendingFocus is { } target)
        {
            pendingFocus = null;
            await EcommerceDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "reviews", DefaultPageSize, "posted", SortDirection.Descending);
        replyContext = new EditContext(replyForm);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private async Task AskRemoveReplyAsync(int reviewId)
    {
        actingId = reviewId;
        removed = false;

        await removeModal.ShowAsync();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = replyFilter = string.Empty;
        ratingFilter = 0;
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ReviewRow>();
        SetStatus("Selection cleared.");
        pendingFocus = () => statusRegion;
    }

    private async Task ConfirmHideAsync()
    {
        if (Acting is { } review && State.SetReviewStatus(review.Id, EcommerceReviewStatus.Hidden))
            SetStatus($"The review “{review.Title}” is hidden. It no longer shows on the product page or counts toward its rating.");

        await hideModal.HideAsync();
    }

    private async Task ConfirmRemoveReplyAsync()
    {
        if (Acting is { } review && State.ReplyToReview(review.Id, null))
        {
            removed = true;
            SetStatus($"The reply to “{review.Title}” is removed.");
        }

        await removeModal.HideAsync();
    }

    /// <summary>
    /// Focus for after an action: the review's button when the review is still in the list, otherwise the status message
    /// (a filter may have taken the review out). Decided after the render, so it sees the updated list.
    /// </summary>
    private Func<ElementReference?> FocusTargetFor(int reviewId, Func<int, ElementReference?> button) => () =>
        Matching().Any(review => review.Id == reviewId) && button(reviewId) is { } element ? element : statusRegion;

    private IEnumerable<EcommerceReview> Matching()
    {
        IEnumerable<EcommerceReview> reviews = State.AllReviews;
        var term = search.Trim();

        if (term.Length > 0)
            reviews = reviews.Where(review => review.Title.Contains(term, StringComparison.OrdinalIgnoreCase) || review.Author.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (State.FindAnyProduct(review.ProductId)?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));

        if (Enum.TryParse<EcommerceReviewStatus>(statusFilter, out var status))
            reviews = reviews.Where(review => review.Status == status);

        if (ratingFilter > 0)
            reviews = reviews.Where(review => review.Rating == ratingFilter);

        if (replyFilter == NeedsReply)
            reviews = reviews.Where(review => review.Reply is null);
        else if (replyFilter == Replied)
            reviews = reviews.Where(review => review.Reply is not null);

        return reviews;
    }

    /// <summary>The dialogs open from code, so Bootstrap does not return focus: it goes back to the review's Hide/Publish button.</summary>
    private void OnHideHidden()
    {
        var id = actingId;
        pendingFocus = FocusTargetFor(id, key => statusButtons.TryGetValue(key, out var button) ? button.Element : null);
        actingId = 0;
    }

    private async Task OnRatingFilterChanged(ChangeEventArgs args)
    {
        ratingFilter = int.TryParse(args.Value?.ToString(), out var stars) && stars is >= 1 and <= 5 ? stars : 0;
        await RefreshAsync();
    }

    /// <summary>After removing a reply its button is gone, so focus moves to the review's row button; otherwise back to Remove reply.</summary>
    private void OnRemoveHidden()
    {
        var id = actingId;
        var wasRemoved = removed;
        pendingFocus = wasRemoved
            ? FocusTargetFor(id, key => replyButtons.TryGetValue(key, out var button) ? button : null)
            : FocusTargetFor(id, key => removeButtons.TryGetValue(key, out var button) ? button.Element : null);
        actingId = 0;
        removed = false;
    }

    private async Task OnReplyFilterChanged(ChangeEventArgs args)
    {
        replyFilter = args.Value?.ToString() is { } value && value is NeedsReply or Replied ? value : string.Empty;
        await RefreshAsync();
    }

    private void OnReplyHidden()
    {
        var id = actingId;
        pendingFocus = FocusTargetFor(id, key => replyButtons.TryGetValue(key, out var button) ? button : null);
        actingId = 0;
        replyForm = new ReplyForm();
        replyContext = new EditContext(replyForm);
    }

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    /// <summary>Filters on every keystroke without changing the box's bound value, so older text is never written back into it.</summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    private void OnSelectionChanged(HashSet<ReviewRow> items) => selected = items.ToList();

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        statusFilter = Enum.TryParse<EcommerceReviewStatus>(value, out var status) && Enum.IsDefined(status) ? status.ToString() : string.Empty;
        await RefreshAsync();
    }

    private async Task OpenReplyAsync(int reviewId)
    {
        if (State.FindReview(reviewId) is not { } review)
            return;

        actingId = reviewId;
        replyForm = new ReplyForm { Text = review.Reply ?? string.Empty };
        replyContext = new EditContext(replyForm);

        await replyModal.ShowAsync();
    }

    /// <summary>The page of reviews the grid asked for: the newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ReviewRow>> ProvideReviewsAsync(GridDataProviderRequest<ReviewRow> request) =>
        view.ApplyAsync(request, Matching().Select(review =>
        {
            var product = State.FindAnyProduct(review.ProductId);

            return new ReviewRow(
                review.Id,
                review.Title,
                review.Body.Length <= 90 ? review.Body : string.Concat(review.Body.AsSpan(0, 89), "…"),
                product?.Name ?? "Removed product",
                review.ProductId,
                State.FindProduct(review.ProductId) is not null,
                review.Rating,
                review.Author,
                review.PostedOn,
                review.Status.ToString(),
                review.Status,
                review.Reply is null ? "Needs a reply" : "Replied",
                review.Reply is not null);
        }).ToList());

    /// <summary>A filter changed: back to the first page of the filtered reviews.</summary>
    private async Task RefreshAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();

        StateHasChanged();
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<ReviewRow>();
        gridVersion++;
        SetStatus("The view is reset: the newest reviews first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ReviewRow row) => selected.Any(item => item.Id == row.Id) ? "ea-row-selected" : string.Empty;

    private async Task SaveReplyAsync()
    {
        if (Acting is { } review && State.ReplyToReview(review.Id, replyForm.Text))
            SetStatus(review.Reply is null ? $"Your reply to “{review.Title}” is posted." : $"Your reply to “{review.Title}” is updated.");

        await replyModal.HideAsync();
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>
    /// Publishes or hides the selected reviews together with <see cref="EcommerceState.BulkSetReviewStatus" />, then refreshes the
    /// grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task SetStatusAsync(EcommerceReviewStatus status)
    {
        var outcome = State.BulkSetReviewStatus(selected.Select(row => row.Id).ToList(), status);
        var verb = status == EcommerceReviewStatus.Hidden ? "Hid" : "Published";

        SetStatus(outcome.Result switch
        {
            EcommerceBulkResult.Ok => $"{verb} {EcommerceDisplay.Plural(outcome.Changed, "review")}."
                + (status == EcommerceReviewStatus.Hidden ? " They no longer show on their product pages or count toward ratings." : " They show on their product pages again.")
                + (outcome.Unchanged > 0 ? $" {EcommerceDisplay.Plural(outcome.Unchanged, "review was", "reviews were")} already {(status == EcommerceReviewStatus.Hidden ? "hidden" : "published")}." : string.Empty),
            EcommerceBulkResult.NoChange => $"No reviews changed: the selected reviews were already {(status == EcommerceReviewStatus.Hidden ? "hidden" : "published")}.",
            EcommerceBulkResult.NotAllowed => "Only the store team can moderate reviews. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        });

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ReviewRow>();
        pendingFocus = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>Hiding asks first, since it changes what shoppers see and the product's rating; publishing again happens at once.</summary>
    private async Task ToggleStatusAsync(int reviewId)
    {
        if (State.FindReview(reviewId) is not { } review)
            return;

        if (review.Status == EcommerceReviewStatus.Published)
        {
            actingId = reviewId;
            await hideModal.ShowAsync();

            return;
        }

        if (State.SetReviewStatus(review.Id, EcommerceReviewStatus.Published))
        {
            SetStatus($"The review “{review.Title}” is published. It shows on the product page and counts toward its rating.");
            pendingFocus = FocusTargetFor(review.Id, key => statusButtons.TryGetValue(key, out var button) ? button.Element : null);
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private EcommerceReview? Acting => actingId == 0 ? null : State.FindReview(actingId);

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || ratingFilter > 0 || replyFilter.Length > 0;

    private string MatchText
    {
        get
        {
            var total = State.AllReviews.Count;
            var count = Matching().Count();

            return count == total ? $"Showing all {EcommerceDisplay.Plural(total, "review")}." : $"{EcommerceDisplay.Count(count)} of {EcommerceDisplay.Plural(total, "review")} match.";
        }
    }

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

            return $"{average} · {EcommerceDisplay.Plural(reviews.Count, "review")}: {published.Count} published, {hidden} hidden · "
                + $"{unanswered} {(unanswered == 1 ? "needs" : "need")} a reply";
        }
    }

    #endregion

    #region Nested Types

    private sealed class ReplyForm
    {
        private string text = string.Empty;

        [Required(ErrorMessage = "Write a reply of 2 to 600 characters.")]
        [StringLength(600, MinimumLength = 2, ErrorMessage = "Use 2 to 600 characters for the reply.")]
        public string Text { get => text; set => text = value?.Trim() ?? string.Empty; }
    }

    /// <summary>One review as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record ReviewRow(
        int Id,
        string Title,
        string Excerpt,
        string Product,
        int ProductId,
        bool IsLive,
        int Rating,
        string Author,
        DateTime PostedOn,
        string Status,
        EcommerceReviewStatus StatusValue,
        string Reply,
        bool HasReply)
    {
        /// <summary>Gets the posting date as it is exported, like "2026-11-06".</summary>
        public string PostedText => PostedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    #endregion
}
