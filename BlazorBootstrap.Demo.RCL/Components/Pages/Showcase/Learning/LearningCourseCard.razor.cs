using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// A course card for the catalog: cover, category, level, title, instructor, rating, length, and the learner's
/// progress when enrolled.
/// </summary>
public partial class LearningCourseCard : ComponentBase
{
    #region Properties, Indexers

    private LearningCategory Category => State.FindCategory(Course.CategoryId);

    /// <summary>Gets or sets the course to show.</summary>
    [Parameter, EditorRequired] public LearningCourse Course { get; set; } = default!;

    private double Rating => State.AverageRating(Course.Id);

    private int ReviewCount => State.ReviewsFor(Course.Id).Count;

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
