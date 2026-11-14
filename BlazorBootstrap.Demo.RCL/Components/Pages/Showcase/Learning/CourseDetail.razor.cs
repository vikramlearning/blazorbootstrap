using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Course page: overview, outcomes, the curriculum in an Accordion, reviews with a rating form for enrolled learners,
/// and an enroll or continue panel. An unknown ID renders the site's not-found screen.
/// </summary>
public partial class CourseDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool focusPrimaryAction;

    private bool justEnrolled;

    private int? loadedId;

    private Button? primaryAction;

    private EditContext reviewContext = default!;

    private ReviewForm reviewForm = new();

    private bool reviewPosted;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // "Enroll" is replaced by "Start the course"; focus follows it instead of falling back to the page (WCAG 2.4.3).
        if (focusPrimaryAction)
        {
            focusPrimaryAction = false;
            await LearningDisplay.FocusAsync(primaryAction?.Element);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Only a different course starts with an empty form and no messages.
        if (loadedId == Id)
            return;

        loadedId = Id;
        justEnrolled = false;
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

    private void Enroll()
    {
        if (Course is null)
            return;

        State.Enroll(Course.Id);
        justEnrolled = true;
        focusPrimaryAction = true;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SubmitReview()
    {
        if (Course is null)
            return;

        State.AddReview(Course.Id, reviewForm.Rating, reviewForm.Body);

        // Reset the fields on the same model: a new EditContext would rebuild the form and drop focus from "Post review".
        reviewForm.Body = string.Empty;
        reviewForm.Rating = 0;
        reviewContext.MarkAsUnmodified();
        reviewPosted = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private decimal AverageRating => Course is null ? 0 : Math.Round((decimal)State.AverageRating(Course.Id) * 2, MidpointRounding.AwayFromZero) / 2;

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Courses", Href = LearningLayout.Base },
        new() { Text = Course?.Title, IsCurrentPage = true },
    };

    private LearningCategory Category => State.FindCategory(Course?.CategoryId ?? string.Empty);

    private string CertificateUrl => $"{LearningLayout.Base}/certificates/{Id}";

    private LearningCourse? Course => State.FindCourse(Id);

    /// <summary>Gets or sets the course ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private bool IsEnrolled => Course is not null && State.IsEnrolled(Course.Id);

    private IReadOnlyList<LearningReview> Reviews => State.ReviewsFor(Id);

    [Inject] private LearningState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class ReviewForm
    {
        [Required(ErrorMessage = "Write a few words about the course.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Use 10 to 500 characters.")]
        public string Body { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Choose a rating from 1 to 5 stars.")]
        public int Rating { get; set; }
    }

    #endregion
}
