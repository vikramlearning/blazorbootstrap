using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Reviews of the signed-in instructor's courses, hidden ones included, the newest first (<see cref="LearningState.InstructorReviews" />),
/// filtered by course, by rating, and by status (All, Needs a reply, Hidden, each with its count). Each review shows the author,
/// the course, the rating in words, when it was posted and edited, the text, why it is hidden (when it is), and the public reply
/// with its date. Reply and Edit reply open a box with a character counter; Delete reply asks first; Hide asks for one of
/// <see cref="LearningReviewHideReasons.All" /> in a dialog that says what hiding does; Show again puts the review back at once.
/// Every result is announced in an always-rendered status message; focus moves into the reply box when it opens, back to the
/// review's button afterward, or to the status message when the review leaves the filtered list.
/// </summary>
public partial class TeachReviews : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string StatusAll = "all";

    private const string StatusHidden = "hidden";

    private const string StatusNeedsReply = "needs-reply";

    private static readonly string[] StatusFilters = { StatusAll, StatusNeedsReply, StatusHidden };

    /// <summary>The Delete reply button of each review with a reply.</summary>
    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    /// <summary>The radio of each hide reason in the dialog.</summary>
    private readonly Dictionary<string, ElementReference> reasonRadios = new();

    /// <summary>The Reply or Edit reply button of each review shown.</summary>
    private readonly Dictionary<int, ElementReference> replyButtons = new();

    private readonly StatusMessage status = new();

    /// <summary>The Hide or Show again button of each review shown.</summary>
    private readonly Dictionary<int, ElementReference> visibilityButtons = new();

    /// <summary>The course the reviews are limited to, as an ID, or an empty string for every course.</summary>
    private string courseFilter = string.Empty;

    /// <summary>Whether the delete dialog's action ran, so focus goes to the Reply button instead of Delete reply.</summary>
    private bool deleteDone;

    private Modal deleteModal = default!;

    /// <summary>The review whose reply the delete dialog is about.</summary>
    private LearningReview? deleting;

    private string? dialogError;

    /// <summary>The review whose reply box is open, or <see langword="null" />.</summary>
    private int? editingId;

    /// <summary>The review the next focus move is about.</summary>
    private int focusReviewId;

    /// <summary>Whether the hide dialog's action ran, so focus goes to Show again instead of Hide.</summary>
    private bool hideDone;

    private string? hideError;

    private Modal hideModal = default!;

    private string? hideReason;

    /// <summary>The review the hide dialog is about.</summary>
    private LearningReview? hiding;

    private FocusTarget pendingFocus;

    /// <summary>The rating the reviews are limited to (1–5), or 0 for every rating.</summary>
    private int ratingFilter;

    private string? replyError;

    private ElementReference? replyInput;

    private string replyText = string.Empty;

    private string statusFilter = StatusAll;

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
                await LearningDisplay.FocusAsync(replyInput);
                break;

            case FocusTarget.HideReason:
                if (reasonRadios.TryGetValue(LearningReviewHideReasons.All[0], out var radio))
                    await LearningDisplay.FocusAsync(radio);
                break;

            case FocusTarget.Reply:
                await FocusReviewButtonAsync(replyButtons);
                break;

            case FocusTarget.Visibility:
                await FocusReviewButtonAsync(visibilityButtons);
                break;

            case FocusTarget.Status:
                await LearningDisplay.FocusAsync(statusRegion);
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

    private async Task AskDeleteAsync(LearningReview review)
    {
        CloseEditor();
        deleting = review;
        deleteDone = false;
        dialogError = null;
        await deleteModal.ShowAsync();
    }

    private async Task AskHideAsync(LearningReview review)
    {
        CloseEditor();
        hiding = review;
        hideDone = false;
        hideReason = null;
        hideError = null;
        dialogError = null;
        await hideModal.ShowAsync();
    }

    private void CancelEdit(int reviewId)
    {
        CloseEditor();
        focusReviewId = reviewId;
        pendingFocus = FocusTarget.Reply;
    }

    private void CloseEditor()
    {
        editingId = null;
        replyText = string.Empty;
        replyError = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deleting is not { } review)
            return;

        var result = State.DeleteReply(review.Id);

        switch (result)
        {
            case LearningResult.Ok:
                status.Set($"Your reply to {review.Author} is deleted. It no longer shows on the {CourseTitle(review.CourseId)} course page.");
                break;

            case LearningResult.NoChange:
                status.Set($"Your reply to {review.Author} was already deleted, so nothing changed.");
                break;

            default:
                dialogError = ResultMessage(result, "The reply wasn't deleted.");
                return;
        }

        deleteDone = true;
        await deleteModal.HideAsync();
    }

    private async Task ConfirmHideAsync()
    {
        if (hiding is not { } review)
            return;

        dialogError = null;

        if (hideReason is not { } reason || !LearningReviewHideReasons.All.Contains(reason))
        {
            hideError = "Choose why you're hiding the review.";
            pendingFocus = FocusTarget.HideReason;
            return;
        }

        var result = State.HideReview(review.Id, reason);

        switch (result)
        {
            case LearningResult.Ok:
                status.Set($"The review by {review.Author} is hidden ({reason.ToLowerInvariant()}). It's off the {CourseTitle(review.CourseId)} course page and out of its rating.");
                break;

            case LearningResult.NoChange:
                status.Set($"The review by {review.Author} was already hidden for that reason, so nothing changed.");
                break;

            case LearningResult.Invalid:
                hideError = "Choose one of the reasons listed.";
                pendingFocus = FocusTarget.HideReason;
                return;

            default:
                dialogError = ResultMessage(result, "The review wasn't hidden.");
                return;
        }

        hideDone = true;
        await hideModal.HideAsync();
    }

    private string CourseTitle(int courseId) => State.MyCourses.FirstOrDefault(course => course.Id == courseId)?.Title ?? "removed";

    /// <summary>Says when a review was posted and, if it was, edited.</summary>
    private static string DatesText(LearningReview review) => review.EditedOn is { } edited
        ? $"Posted {LearningDisplay.Date(review.PostedOn)} · edited {LearningDisplay.Date(edited)}"
        : $"Posted {LearningDisplay.Date(review.PostedOn)}";

    private string EmptyText(int total) => total == 0
        ? "Learners enrolled in your courses can review them. Their reviews show up here so you can reply."
        : statusFilter switch
        {
            StatusNeedsReply => "Every review in this view has a reply, or is hidden.",
            StatusHidden => "No review in this view is hidden.",
            _ => "Try another course or rating.",
        };

    private async Task FocusReviewButtonAsync(Dictionary<int, ElementReference> buttons)
    {
        // A review that left the filtered list has no button: the status message takes focus.
        if (IsShown(focusReviewId) && buttons.TryGetValue(focusReviewId, out var button))
            await LearningDisplay.FocusAsync(button);
        else
            await LearningDisplay.FocusAsync(statusRegion);
    }

    private bool IsShown(int reviewId) => Scoped(State.InstructorReviews).Any(review => review.Id == reviewId && MatchesStatus(review, statusFilter));

    private static bool MatchesStatus(LearningReview review, string filter) => filter switch
    {
        StatusNeedsReply => review.HiddenReason is null && review.InstructorReply is null,
        StatusHidden => review.HiddenReason is not null,
        _ => true,
    };

    private void OnCourseChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        courseFilter = State.MyCourses.Any(course => course.Id.ToString(CultureInfo.InvariantCulture) == value) ? value : string.Empty;
        CloseEditor();
        status.Clear();
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After a deletion, focus goes to the review's Reply button
    /// (or the status message when the review is no longer listed); otherwise back to Delete reply.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        var review = deleting;
        deleting = null;
        dialogError = null;

        if (review is null)
            return;

        if (deleteDone)
        {
            deleteDone = false;
            focusReviewId = review.Id;
            pendingFocus = FocusTarget.Reply;
            StateHasChanged();
        }
        else if (deleteButtons.TryGetValue(review.Id, out var button))
        {
            await LearningDisplay.FocusAsync(button);
        }
    }

    /// <summary>After hiding, focus goes to the review's Show again button (or the status message); otherwise back to Hide.</summary>
    private async Task OnHideHiddenAsync()
    {
        var review = hiding;
        hiding = null;
        hideReason = null;
        hideError = null;
        dialogError = null;

        if (review is null)
            return;

        if (hideDone)
        {
            hideDone = false;
            focusReviewId = review.Id;
            pendingFocus = FocusTarget.Visibility;
            StateHasChanged();
        }
        else if (visibilityButtons.TryGetValue(review.Id, out var button))
        {
            await LearningDisplay.FocusAsync(button);
        }
    }

    private void OnRatingChanged(ChangeEventArgs args)
    {
        ratingFilter = int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value is >= 1 and <= 5 ? value : 0;
        CloseEditor();
        status.Clear();
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // A review removed elsewhere (or a sign-out) closes its open reply box.
        if (editingId is { } id && State.InstructorReviews.All(review => review.Id != id))
            CloseEditor();

        StateHasChanged();
    });

    private void OnStatusChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? StatusAll;
        statusFilter = StatusFilters.Contains(value) ? value : StatusAll;
        CloseEditor();
        status.Clear();
    }

    private static string Plural(int count, string one, string many) =>
        count == 1 ? $"1 {one}" : $"{count.ToString("#,0", CultureInfo.InvariantCulture)} {many}";

    private static string ResultMessage(LearningResult result, string nothingDone) => result switch
    {
        LearningResult.NotAllowed => $"Sign in with the instructor account to manage reviews. {nothingDone}",
        LearningResult.NotFound => $"This review is no longer on one of your courses. {nothingDone}",
        LearningResult.Invalid => $"Use {MaxReplyText} characters or fewer. If it's already that short, this demo tab is full; reset the demo data. {nothingDone}",
        LearningResult.WrongState => $"The review can't be changed right now. {nothingDone}",
        _ => $"Something went wrong. {nothingDone} Try again.",
    };

    private void SaveReply(LearningReview review)
    {
        replyError = null;

        var text = replyText.Trim();

        if (text.Length == 0)
        {
            replyError = review.InstructorReply is null
                ? "Write a reply before you post it."
                : "Write a reply before you save it. To take your reply down, cancel and choose Delete reply.";
            pendingFocus = FocusTarget.Input;
            return;
        }

        if (text.Length > LearningState.MaxReviewLength)
        {
            replyError = $"Use {MaxReplyText} characters or fewer.";
            pendingFocus = FocusTarget.Input;
            return;
        }

        var hadReply = review.InstructorReply is not null;
        var result = State.SaveReply(review.Id, text);
        var courseTitle = CourseTitle(review.CourseId);

        switch (result)
        {
            case LearningResult.Ok:
                status.Set(hadReply
                    ? $"Your reply to {review.Author} is updated on the {courseTitle} course page."
                    : $"Your reply to {review.Author} is posted on the {courseTitle} course page.");
                break;

            case LearningResult.NoChange:
                status.Set($"Your reply to {review.Author} is unchanged.");
                break;

            default:
                replyError = ResultMessage(result, "The reply wasn't saved.");
                pendingFocus = FocusTarget.Input;
                return;
        }

        CloseEditor();
        focusReviewId = review.Id;
        pendingFocus = FocusTarget.Reply;
    }

    private string ScopeText(int shown, int total) => total == 0
        ? "No reviews yet."
        : shown == total ? $"Showing all {Plural(total, "review", "reviews")}." : $"Showing {shown.ToString("#,0", CultureInfo.InvariantCulture)} of {Plural(total, "review", "reviews")}.";

    /// <summary>The reviews that match the course and rating filters (the status filter's counts are over these).</summary>
    private List<LearningReview> Scoped(IReadOnlyList<LearningReview> reviews) => reviews
        .Where(review => courseFilter.Length == 0 || review.CourseId.ToString(CultureInfo.InvariantCulture) == courseFilter)
        .Where(review => ratingFilter == 0 || review.Rating == ratingFilter)
        .ToList();

    private void SelectHideReason(string reason)
    {
        hideReason = reason;
        hideError = null;
    }

    private void ShowAgain(LearningReview review)
    {
        CloseEditor();

        var result = State.ShowReview(review.Id);

        switch (result)
        {
            case LearningResult.Ok:
                status.Set($"The review by {review.Author} shows on the {CourseTitle(review.CourseId)} course page again and counts in its rating.");
                break;

            case LearningResult.NoChange:
                status.Set($"The review by {review.Author} already shows, so nothing changed.");
                break;

            default:
                status.Set(ResultMessage(result, "The review is still hidden."), isError: true);
                pendingFocus = FocusTarget.Status;
                return;
        }

        focusReviewId = review.Id;
        pendingFocus = FocusTarget.Visibility;
    }

    private static string StarLabel(int stars) => stars == 1 ? "1 star" : string.Create(CultureInfo.InvariantCulture, $"{stars} stars");

    /// <summary>A status option's text with its count over the course and rating filters, such as "Needs a reply (3)".</summary>
    private static string StatusLabel(string filter, List<LearningReview> scoped)
    {
        var name = filter switch
        {
            StatusNeedsReply => "Needs a reply",
            StatusHidden => "Hidden",
            _ => "All",
        };

        return string.Create(CultureInfo.InvariantCulture, $"{name} ({scoped.Count(review => MatchesStatus(review, filter))})");
    }

    private void StartEdit(LearningReview review)
    {
        editingId = review.Id;
        replyText = review.InstructorReply ?? string.Empty;
        replyError = null;
        status.Clear();
        pendingFocus = FocusTarget.Input;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText => string.Create(CultureInfo.InvariantCulture, $"{replyText.Length:#,0} of {MaxReplyText} characters");

    private static string MaxReplyText => LearningState.MaxReviewLength.ToString("#,0", CultureInfo.InvariantCulture);

    [Inject] private LearningState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Input,
        HideReason,
        Reply,
        Visibility,
        Status,
    }

    /// <summary>A status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private sealed class StatusMessage
    {
        #region Fields and Constants

        private int count;

        private string? text;

        #endregion

        #region Methods

        #region Public Methods

        public void Clear()
        {
            count = 0;
            text = null;
            Message = null;
            IsError = false;
        }

        public void Set(string message, bool isError = false)
        {
            count = message == text ? count + 1 : 1;
            text = message;
            IsError = isError;
            Message = count == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({count})");
        }

        #endregion

        #endregion

        #region Properties, Indexers

        public bool IsError { get; private set; }

        public string? Message { get; private set; }

        #endregion
    }

    #endregion
}
