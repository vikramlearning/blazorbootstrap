using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// My learning: every enrolled course with its progress bar, the next lesson, and a certificate link once complete.
/// </summary>
public partial class MyProgress : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private LearningState State { get; set; } = default!;

    private string Summary
    {
        get
        {
            var enrolled = State.EnrolledCourses;
            var completed = enrolled.Count(course => State.IsComplete(course.Id));
            var lessons = enrolled.Sum(course => State.CompletedCount(course.Id));

            return $"{enrolled.Count} {(enrolled.Count == 1 ? "course" : "courses")} enrolled, {completed} completed, {lessons} {(lessons == 1 ? "lesson" : "lessons")} done.";
        }
    }

    #endregion
}
