using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// A course card for the catalog: cover, category, level, title, instructor, rating, and length. On the signed-in
/// learner's courses it adds an "Enrolled" or "Completed" badge in words and the lessons complete.
/// </summary>
public partial class LearningCourseCard : ComponentBase
{
    #region Properties, Indexers

    private LearningCategory Category => State.FindCategory(Course.CategoryId);

    /// <summary>Gets or sets the course to show.</summary>
    [Parameter, EditorRequired] public LearningCourse Course { get; set; } = default!;

    private bool IsComplete => State.IsComplete(Course.Id);

    private bool IsEnrolled => State.IsEnrolled(Course.Id);

    private double Rating => State.AverageRating(Course.Id);

    private int ReviewCount => State.ReviewsFor(Course.Id).Count;

    private string ReviewCountText => ReviewCount == 1 ? "1 review" : $"{ReviewCount} reviews";

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
