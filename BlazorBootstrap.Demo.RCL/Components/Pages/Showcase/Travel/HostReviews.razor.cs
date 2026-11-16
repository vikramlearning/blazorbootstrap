using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Reviews of the signed-in host's listings, the newest first: the average rating of each listing at the top, then the
/// reviews, filtered by listing and by "Needs a reply". Each review shows its rating in words ("Rated 4 out of 5"), the
/// author, the date, the listing, and the text, then the host's public reply with Edit and Remove, or a Reply button.
/// Replying or editing opens a box with a character counter (<see cref="TravelState.ReplyToReview" />); removing asks
/// first in a <see cref="Modal" /> that names the review. The page explains that replies are public on the stay page.
/// Focus moves into the box when it opens and back to the review's button when it closes, or to the status message when
/// the review leaves the filtered list; every result is announced in the status message.
/// </summary>
public partial class HostReviews : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Reply or Edit button of each review shown, so focus can come back to it.</summary>
    private readonly Dictionary<int, ElementReference> actionButtons = new();

    /// <summary>The Remove button of each review with a reply, so focus can go back to it when the dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> removeButtons = new();

    private string? dialogError;

    /// <summary>The review whose reply box is open, or <see langword="null" />.</summary>
    private int? editingId;

    /// <summary>The review the next focus move is about.</summary>
    private int focusReviewId;

    private bool needsReplyOnly;

    private FocusTarget pendingFocus;

    /// <summary>Whether the reply in the remove dialog was removed, so focus goes to the Reply button instead of Remove.</summary>
    private bool removed;

    private Modal removeModal = default!;

    /// <summary>The review whose reply the remove dialog is about.</summary>
    private TravelReview? removing;

    private string? replyError;

    private ElementReference? replyInput;

    private string replyText = string.Empty;

    /// <summary>The listing the reviews are limited to, as an ID, or an empty string for every listing.</summary>
    private string stayFilter = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var target = pendingFocus;
        pendingFocus = FocusTarget.None;

        switch (target)
        {
            case FocusTarget.Input:
                await TravelDisplay.FocusAsync(replyInput);
                break;

            case FocusTarget.Action:
                // A review that left the filtered list (it no longer needs a reply) has no button: the status message takes focus.
                if (IsShown(focusReviewId) && actionButtons.TryGetValue(focusReviewId, out var button))
                    await TravelDisplay.FocusAsync(button);
                else
                    await TravelDisplay.FocusAsync(statusRegion);
                break;
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private async Task AskRemoveAsync(TravelReview review)
    {
        CloseEditor();
        removing = review;
        removed = false;
        dialogError = null;
        await removeModal.ShowAsync();
    }

    private void CancelEdit(int reviewId)
    {
        CloseEditor();
        focusReviewId = reviewId;
        pendingFocus = FocusTarget.Action;
    }

    private void CloseEditor()
    {
        editingId = null;
        replyText = string.Empty;
        replyError = null;
    }

    private async Task ConfirmRemoveAsync()
    {
        if (removing is not { } review)
            return;

        var result = State.ReplyToReview(review.Id, null);

        if (result is not (TravelResult.Ok or TravelResult.NoChange))
        {
            dialogError = ResultMessage(result);
            return;
        }

        statusMessage = $"Your reply to {review.Author} was removed. It no longer shows on the stay page.";
        removed = true;
        await removeModal.HideAsync();
    }

    private string EmptyText(int total) => total == 0
        ? "Guests can review a stay after their trip. Their reviews show up here so you can reply."
        : needsReplyOnly
            ? "You've replied to every review in this view."
            : "This listing has no reviews yet.";

    private string EmptyTitle(int total) => total == 0
        ? "No reviews yet"
        : needsReplyOnly ? "Nothing needs a reply" : "No reviews match";

    private List<TravelReview> Filtered(IReadOnlyList<TravelReview> reviews) => reviews
        .Where(review => stayFilter.Length == 0 || review.StayId.ToString(CultureInfo.InvariantCulture) == stayFilter)
        .Where(review => !needsReplyOnly || review.HostReply is null)
        .ToList();

    private bool IsShown(int reviewId) => Filtered(State.HostReviews()).Any(review => review.Id == reviewId);

    private void OnListingChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        stayFilter = State.HostStays().Any(stay => stay.Id.ToString(CultureInfo.InvariantCulture) == value) ? value : string.Empty;
        statusMessage = null;
    }

    private void OnNeedsReplyChanged(ChangeEventArgs args)
    {
        needsReplyOnly = args.Value is true;
        statusMessage = null;
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After a removal, focus goes to the review's Reply
    /// button (or the status message when the review is no longer listed); otherwise back to the Remove button.
    /// </summary>
    private async Task OnRemoveHiddenAsync()
    {
        var review = removing;
        removing = null;
        dialogError = null;

        if (review is null)
            return;

        if (removed)
        {
            removed = false;
            focusReviewId = review.Id;
            pendingFocus = FocusTarget.Action;
            StateHasChanged();
        }
        else if (removeButtons.TryGetValue(review.Id, out var button))
        {
            await TravelDisplay.FocusAsync(button);
        }
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // A review removed elsewhere (or a sign-out) closes its open reply box.
        if (editingId is { } id && State.HostReviews().All(review => review.Id != id))
            CloseEditor();

        StateHasChanged();
    });

    private static string Plural(int count, string one, string many) =>
        count == 1 ? $"1 {one}" : $"{count.ToString("N0", CultureInfo.InvariantCulture)} {many}";

    private static string PostedText(DateOnly date) => FormattableString.Invariant($"{TravelDisplay.Day(date)}, {date.Year}");

    private static string ResultMessage(TravelResult result) => result switch
    {
        TravelResult.NotAllowed => "Sign in with the host account to reply to reviews.",
        TravelResult.NotFound => "This review is no longer on one of your listings, so nothing was saved.",
        TravelResult.Invalid => $"Use {TravelState.MaxReviewLength.ToString("N0", CultureInfo.InvariantCulture)} characters or fewer.",
        _ => "The reply couldn't be saved. Try again.",
    };

    private void SaveReply(TravelReview review)
    {
        replyError = null;

        var text = replyText.Trim();

        if (text.Length == 0)
        {
            replyError = review.HostReply is null
                ? "Write a reply before you post it."
                : "Write a reply before you save it. To take your reply down, cancel and choose Remove.";
            pendingFocus = FocusTarget.Input;
            return;
        }

        if (text.Length > TravelState.MaxReviewLength)
        {
            replyError = ResultMessage(TravelResult.Invalid);
            pendingFocus = FocusTarget.Input;
            return;
        }

        var hadReply = review.HostReply is not null;
        var result = State.ReplyToReview(review.Id, text);

        if (result is not (TravelResult.Ok or TravelResult.NoChange))
        {
            replyError = ResultMessage(result);
            pendingFocus = FocusTarget.Input;
            return;
        }

        var stayName = State.FindStay(review.StayId)?.Name ?? "listing's";

        statusMessage = result == TravelResult.NoChange
            ? $"Your reply to {review.Author} is unchanged."
            : hadReply
                ? $"Your reply to {review.Author} was updated on the {stayName} stay page."
                : $"Your reply to {review.Author} is posted on the {stayName} stay page.";

        CloseEditor();
        focusReviewId = review.Id;
        pendingFocus = FocusTarget.Action;
    }

    private string ScopeText(int shown, int total) => total == 0
        ? "No reviews yet."
        : shown == total ? $"Showing all {Plural(total, "review", "reviews")}." : $"Showing {shown.ToString("N0", CultureInfo.InvariantCulture)} of {Plural(total, "review", "reviews")}.";

    private void StartEdit(TravelReview review)
    {
        editingId = review.Id;
        replyText = review.HostReply ?? string.Empty;
        replyError = null;
        statusMessage = null;
        pendingFocus = FocusTarget.Input;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText => FormattableString.Invariant($"{replyText.Length:N0} of {TravelState.MaxReviewLength:N0} characters");

    [Inject] private TravelState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Input,
        Action,
    }

    #endregion
}
