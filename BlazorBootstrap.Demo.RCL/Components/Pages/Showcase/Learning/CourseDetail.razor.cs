using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Course page: overview with the enroll status, outcomes, the curriculum in an Accordion (with each lesson's completion
/// in words), and reviews. A visitor is offered "Sign in to enroll" (returning here); a learner enrolls, then sees their
/// progress, the next lesson, the certificate once complete, and "Unenroll" (which asks first and names what is removed
/// and what stays). An enrolled learner writes one review (rating radios and text with a counter) and can edit or delete
/// it (deleting asks first). Every <see cref="LearningResult" /> maps to a message in a status region. A link ending in
/// <c>#your-review</c> moves focus to the "Your review" panel. An unknown ID renders the site's not-found screen.
/// </summary>
public partial class CourseDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> ratingRadios = new();

    private ElementReference bodyInput;

    private string? bodyError;

    private ElementReference deleteButton;

    /// <summary>Set when the delete dialog's action ran, so focus moves to the review status once it is hidden.</summary>
    private bool deleteDone;

    private string? deleteError;

    private Modal deleteModal = default!;

    private ElementReference editButton;

    private readonly StatusMessage enrollStatus = new();

    private ElementReference enrollStatusRegion;

    /// <summary>The element to focus after the next render, read then (it may only exist after that render).</summary>
    private Func<ElementReference?>? focusAfterRender;

    /// <summary>Whether a <c>#your-review</c> link was handled for the course shown.</summary>
    private bool fragmentHandled;

    private bool isEditing;

    private int? loadedId;

    private ElementReference? primaryAction;

    private string? ratingError;

    private string reviewBody = string.Empty;

    private int reviewRating;

    private readonly StatusMessage reviewStatus = new();

    private ElementReference reviewStatusRegion;

    private ElementReference unenrollButton;

    /// <summary>Set when the unenroll dialog's action ran, so focus moves to the enroll status once it is hidden.</summary>
    private bool unenrollDone;

    private string? unenrollError;

    private Modal unenrollModal = default!;

    private ElementReference yourReviewHeading;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusAfterRender is { } target)
        {
            focusAfterRender = null;
            await LearningDisplay.FocusAsync(target());

            return;
        }

        // "Review this course" on My learning links to #your-review; focus follows once the learner's panel is known (WCAG 2.4.3).
        if (!fragmentHandled && Course is not null && Session.IsRestored)
        {
            fragmentHandled = true;

            if (new Uri(Navigation.Uri).Fragment.Equals("#your-review", StringComparison.OrdinalIgnoreCase))
                await LearningDisplay.FocusAsync(yourReviewHeading);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Only a different course starts with an empty form and no messages.
        if (loadedId == Id)
            return;

        loadedId = Id;
        fragmentHandled = false;
        isEditing = false;
        enrollStatus.Clear();
        reviewStatus.Clear();
        ClearReviewForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string AverageText(int count, double average) => count switch
    {
        0 => "No reviews yet",
        1 => $"Rated {LearningDisplay.Rating(average)} out of 5 from 1 review",
        _ => string.Create(CultureInfo.InvariantCulture, $"Rated {LearningDisplay.Rating(average)} out of 5 on average from {count} reviews"),
    };

    private static string CertificateUrl(string code) => $"{LearningLayout.Base}/certificates/{Uri.EscapeDataString(code)}";

    /// <summary>Says when a review was posted and, if it was, edited.</summary>
    private static string DatesText(LearningReview review) => review.EditedOn is { } edited
        ? $"Posted {LearningDisplay.Date(review.PostedOn)} · edited {LearningDisplay.Date(edited)}"
        : $"Posted {LearningDisplay.Date(review.PostedOn)}";

    private static string RatedText(int rating) => string.Create(CultureInfo.InvariantCulture, $"Rated {rating} out of 5");

    private static string RatingLabel(int stars) => stars == 1 ? "1 star" : string.Create(CultureInfo.InvariantCulture, $"{stars} stars");

    private async Task AskDeleteReviewAsync()
    {
        deleteError = null;
        deleteDone = false;

        await deleteModal.ShowAsync();
    }

    private async Task AskUnenrollAsync()
    {
        unenrollError = null;
        unenrollDone = false;

        await unenrollModal.ShowAsync();
    }

    private void CancelEdit()
    {
        isEditing = false;
        ClearReviewForm();
        focusAfterRender = () => editButton;
    }

    private void ClearReviewForm()
    {
        reviewRating = 0;
        reviewBody = string.Empty;
        ratingError = null;
        bodyError = null;
    }

    private async Task ConfirmDeleteReviewAsync()
    {
        if (Course is not { } course)
            return;

        var hadReply = OwnReview?.InstructorReply is not null;
        var result = State.DeleteReview(course.Id);

        switch (result)
        {
            case LearningResult.Ok:
                isEditing = false;
                ClearReviewForm();
                reviewStatus.Set(hadReply
                    ? "Your review and the instructor's reply are deleted. You can write a new review while you're enrolled."
                    : "Your review is deleted. You can write a new one while you're enrolled.");
                break;

            case LearningResult.NotFound:
                reviewStatus.Set("Your review was already deleted, so nothing changed.");
                break;

            default:
                deleteError = ResultText(result, "Nothing was deleted.");
                return;
        }

        deleteDone = true;
        await deleteModal.HideAsync();
    }

    /// <summary>Closes the dialog; the unenroll runs once it is hidden, so the page never changes under an open dialog.</summary>
    private async Task ConfirmUnenrollAsync()
    {
        unenrollDone = true;
        await unenrollModal.HideAsync();
    }

    private void Enroll()
    {
        if (Course is not { } course)
            return;

        var result = State.Enroll(course.Id);

        switch (result)
        {
            case LearningResult.Ok:
                enrollStatus.Set($"You're enrolled in {course.Title}. Your progress is saved as you go; start with the first lesson.");
                focusAfterRender = () => primaryAction;
                break;

            case LearningResult.NoChange:
                enrollStatus.Set($"You were already enrolled in {course.Title}.");
                focusAfterRender = () => primaryAction;
                break;

            case LearningResult.Invalid:
                enrollStatus.Set("This demo tab has reached its limit of enrollments, so you weren't enrolled. Use \"Reset demo data\" in the account menu to start over.", isError: true);
                break;

            case LearningResult.WrongState:
                enrollStatus.Set($"{course.Title} isn't open for new enrollments right now, so you weren't enrolled.", isError: true);
                break;

            default:
                enrollStatus.Set(ResultText(result, "You weren't enrolled."), isError: true);
                break;
        }
    }

    private bool IsMine(LearningReview review) => review.AccountId is not null && review.AccountId == State.Identity.Current?.Id;

    /// <summary>After a confirmed delete, focus goes to the status message (the opener is gone); otherwise back to "Delete".</summary>
    private async Task OnDeleteHiddenAsync()
    {
        deleteError = null;

        if (deleteDone)
        {
            deleteDone = false;
            StateHasChanged();
            await LearningDisplay.FocusAsync(reviewStatusRegion);
        }
        else
        {
            await LearningDisplay.FocusAsync(deleteButton);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Runs a confirmed unenroll once the dialog is hidden, then moves focus to the status message (the opener is gone);
    /// after "Stay enrolled", back to "Unenroll". A course no longer in the catalog disappears for the learner after leaving
    /// it (unless they hold its certificate), so they go to My learning with a notice.
    /// </summary>
    private async Task OnUnenrollHiddenAsync()
    {
        unenrollError = null;

        if (!unenrollDone)
        {
            await LearningDisplay.FocusAsync(unenrollButton);

            return;
        }

        unenrollDone = false;

        if (Course is not { } course)
            return;

        var hasCertificate = State.CertificateFor(course.Id) is not null;
        var result = State.Unenroll(course.Id);

        switch (result)
        {
            case LearningResult.Ok when State.FindCourse(course.Id) is null:
                Toasts.Notify(new ToastMessage(ToastType.Info, IconName.BoxArrowRight, "Unenrolled", $"You left {course.Title}. It's no longer in the catalog, so it's gone from your courses."));
                Navigation.NavigateTo($"{LearningLayout.Base}/progress");

                return;

            case LearningResult.Ok:
                isEditing = false;
                ClearReviewForm();
                enrollStatus.Set($"You're no longer enrolled in {course.Title}. Your progress and self-check results are removed; "
                    + $"{(hasCertificate ? "your certificate, " : string.Empty)}any review and questions you wrote stay. "
                    + (course.IsPublished ? "You can enroll again at any time." : "The course isn't in the catalog any more, so you can't enroll again."));
                break;

            case LearningResult.NotEnrolled:
                enrollStatus.Set($"You weren't enrolled in {course.Title}, so nothing changed.");
                break;

            default:
                enrollStatus.Set(ResultText(result, "You're still enrolled."), isError: true);
                break;
        }

        StateHasChanged();
        await LearningDisplay.FocusAsync(enrollStatusRegion);
    }

    /// <summary>Maps a result that isn't the expected outcome to a message ending with what happened.</summary>
    private string ResultText(LearningResult result, string nothingDone) => result switch
    {
        LearningResult.NotAllowed => $"Sign in with a learner account to do that. {nothingDone}",
        LearningResult.NotFound => $"This course isn't in the catalog any more. {nothingDone}",
        LearningResult.NotEnrolled => $"Enroll in {Course?.Title} first. {nothingDone}",
        LearningResult.Invalid => $"Something in the request wasn't valid. {nothingDone}",
        LearningResult.WrongState => $"That can't be done right now. {nothingDone}",
        LearningResult.NoChange => "Nothing changed.",
        _ => $"That didn't work. {nothingDone} Try again.",
    };

    private void SaveReview()
    {
        if (Course is not { } course)
            return;

        var body = reviewBody.Trim();

        ratingError = reviewRating is < 1 or > 5 ? "Choose a rating from 1 to 5 stars." : null;
        bodyError = body.Length == 0
            ? "Write a few words about the course."
            : body.Length > LearningState.MaxReviewLength ? $"Use up to {MaxReviewText} characters." : null;

        if (ratingError is not null || bodyError is not null)
        {
            reviewStatus.Clear();

            // Focus after the render that adds the error and aria-invalid, so they're read with the field.
            focusAfterRender = ratingError is not null
                ? () => ratingRadios.TryGetValue(1, out var first) ? first : null
                : () => bodyInput;

            return;
        }

        var isNew = OwnReview is null;
        var result = State.SaveReview(course.Id, reviewRating, body);

        switch (result)
        {
            case LearningResult.Ok:
                reviewStatus.Set(isNew
                    ? "Thanks! Your review is posted at the top of the list."
                    : "Your review is updated and shows as edited today. Any reply from the instructor stays.");
                break;

            case LearningResult.NoChange:
                reviewStatus.Set("Nothing changed: your review already says that.");
                break;

            case LearningResult.Invalid:
                reviewStatus.Set($"Choose 1 to 5 stars and write 1 to {MaxReviewText} characters. Nothing was saved.", isError: true);
                return;

            default:
                reviewStatus.Set(ResultText(result, "Nothing was saved."), isError: true);
                return;
        }

        // The form is replaced by the saved review, so focus goes to the message about it.
        isEditing = false;
        ClearReviewForm();
        focusAfterRender = () => reviewStatusRegion;
    }

    private void SelectRating(int stars)
    {
        reviewRating = stars;
        ratingError = null;
    }

    /// <summary>Returns the sign-in (or register) route that comes back to this course.</summary>
    private string SignInUrl(bool register) =>
        $"{LearningLayout.SignInPath}?{(register ? "mode=register&" : string.Empty)}returnUrl={Uri.EscapeDataString(LearningDisplay.CourseUrl(Id))}";

    private void StartEdit()
    {
        if (OwnReview is not { } own)
            return;

        isEditing = true;
        reviewRating = own.Rating;
        reviewBody = own.Body;
        ratingError = null;
        bodyError = null;
        reviewStatus.Clear();
        focusAfterRender = () => ratingRadios.TryGetValue(reviewRating, out var radio) ? radio : null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Courses", Href = LearningLayout.Base },
        new() { Text = Course?.Title, IsCurrentPage = true },
    };

    private LearningCategory Category => State.FindCategory(Course?.CategoryId ?? string.Empty);

    private LearningCourse? Course => State.FindCourse(Id);

    /// <summary>Gets or sets the course ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private bool IsComplete => Course is not null && State.IsComplete(Course.Id);

    private bool IsEnrolled => Course is not null && State.IsEnrolled(Course.Id);

    /// <summary>Says what unenrolling keeps.</summary>
    private string KeptText
    {
        get
        {
            var kept = new List<string>(3);

            if (State.CertificateFor(Id) is not null)
                kept.Add("your certificate");

            kept.Add(OwnReview is null ? "any review you write" : "your review");
            kept.Add("your questions");

            return kept.Count == 3 ? $"{kept[0]}, {kept[1]}, and {kept[2]}" : $"{kept[0]} and {kept[1]}";
        }
    }

    private static string MaxReviewText => LearningState.MaxReviewLength.ToString("#,0", CultureInfo.InvariantCulture);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private LearningReview? OwnReview => State.ReviewBy(Id);

    /// <summary>Counts the self-checks and says what passes one.</summary>
    private string QuizCountText
    {
        get
        {
            var quizzes = Course?.Lessons.Where(lesson => lesson.Kind == LearningLessonKind.Quiz).ToList() ?? new List<LearningLesson>();

            return quizzes.Count switch
            {
                0 => "No self-checks in this course",
                1 => $"1 self-check: pass with {quizzes[0].PassMark} of {quizzes[0].Questions.Count} right",
                // Quizzes can have different lengths, so the shared rule is said instead of one quiz's numbers.
                _ => string.Create(CultureInfo.InvariantCulture, $"{quizzes.Count} self-checks: pass each with two thirds right"),
            };
        }
    }

    private IReadOnlyList<LearningReview> Reviews => State.ReviewsFor(Id);

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    [Inject] private LearningState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion

    #region Nested Types

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
