using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// My learning (learners only): courses in progress, the most recently active first, with their progress and the next
/// lesson; completed courses with the certificate and a link to the course page's "Your review" panel; and an empty
/// state that links to the catalog. A course finished before unenrolling still counts as completed: its certificate stays.
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

    private static string CertificateUrl(string code) => $"{LearningLayout.Base}/certificates/{Uri.EscapeDataString(code)}";

    private static string Count(int count, string one, string many) =>
        count == 1 ? $"1 {one}" : string.Create(CultureInfo.InvariantCulture, $"{count} {many}");

    private static string SummaryText(int inProgress, int completed) =>
        $"{Count(inProgress, "course", "courses")} in progress, {Count(completed, "course", "courses")} completed.";

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Completed courses: enrolled ones with every lesson done, then courses left after finishing (their certificate
    /// stays), newest certificate first.
    /// </summary>
    private IReadOnlyList<LearningCourse> Completed
    {
        get
        {
            var enrolled = State.EnrolledCourses;
            var finished = enrolled.Where(course => State.IsComplete(course.Id)).ToList();
            var left = State.MyCertificates
                .Where(certificate => enrolled.All(course => course.Id != certificate.CourseId))
                .Select(certificate => State.FindCourse(certificate.CourseId))
                .OfType<LearningCourse>();

            return finished.Concat(left).DistinctBy(course => course.Id).ToList();
        }
    }

    /// <summary>Enrolled courses not yet finished, the most recently active first.</summary>
    private IReadOnlyList<LearningCourse> InProgress => State.EnrolledCourses.Where(course => !State.IsComplete(course.Id)).ToList();

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
