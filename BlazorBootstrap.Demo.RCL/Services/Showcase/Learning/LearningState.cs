using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

/// <summary>
/// In-memory state of the learning site for the current session: enrollments, completed lessons, and reviews written
/// here. Nothing is persisted or sent.
/// </summary>
public sealed class LearningState
{
    #region Fields and Constants

    private readonly Dictionary<int, HashSet<int>> completed;

    private readonly List<LearningReview> reviews;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session.</summary>
    public LearningState()
    {
        completed = LearningSeedData.CompletedLessons(Courses);
        reviews = LearningSeedData.Reviews(Courses);
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Adds a review written in this session.</summary>
    public void AddReview(int courseId, int rating, string body)
    {
        reviews.Add(new LearningReview(courseId, LearningSeedData.LearnerName, Math.Clamp(rating, 1, 5), body.Trim(), LearningSeedData.Today, IsFromThisSession: true));
        Changed?.Invoke();
    }

    /// <summary>Returns the average rating of a course, or 0 without reviews.</summary>
    public double AverageRating(int courseId)
    {
        var ratings = reviews.Where(review => review.CourseId == courseId).Select(review => review.Rating).ToList();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    /// <summary>Returns how many lessons of a course are complete.</summary>
    public int CompletedCount(int courseId) => completed.TryGetValue(courseId, out var lessons) ? lessons.Count : 0;

    /// <summary>Enrolls the learner in a course; enrolling twice does nothing.</summary>
    public void Enroll(int courseId)
    {
        if (completed.ContainsKey(courseId) || FindCourse(courseId) is null)
            return;

        completed[courseId] = new HashSet<int>();
        Changed?.Invoke();
    }

    /// <summary>Finds a course by its ID, or returns <see langword="null" />.</summary>
    public LearningCourse? FindCourse(int courseId) => Courses.FirstOrDefault(course => course.Id == courseId);

    /// <summary>Finds a category by its ID; unknown IDs fall back to the first category.</summary>
    public LearningCategory FindCategory(string categoryId) => Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories[0];

    /// <summary>Returns whether the learner finished every lesson of a course.</summary>
    public bool IsComplete(int courseId) => FindCourse(courseId) is { } course && CompletedCount(courseId) == course.Lessons.Count;

    /// <summary>Returns whether a lesson is complete.</summary>
    public bool IsLessonComplete(int courseId, int lessonId) => completed.TryGetValue(courseId, out var lessons) && lessons.Contains(lessonId);

    /// <summary>Returns whether the learner is enrolled in a course.</summary>
    public bool IsEnrolled(int courseId) => completed.ContainsKey(courseId);

    /// <summary>Returns the first lesson not yet complete, or the first lesson when everything is done.</summary>
    public LearningLesson NextLesson(LearningCourse course) =>
        course.Lessons.FirstOrDefault(lesson => !IsLessonComplete(course.Id, lesson.Id)) ?? course.Lessons[0];

    /// <summary>Returns the share of lessons complete, from 0 to 100.</summary>
    public int ProgressPercent(int courseId) =>
        FindCourse(courseId) is { } course && course.Lessons.Count > 0 ? CompletedCount(courseId) * 100 / course.Lessons.Count : 0;

    /// <summary>Returns the reviews of a course, newest first; this session's reviews come before seeded ones on the same day.</summary>
    public IReadOnlyList<LearningReview> ReviewsFor(int courseId) => reviews
        .Select((review, index) => (review, index))
        .Where(entry => entry.review.CourseId == courseId)
        .OrderByDescending(entry => entry.review.PostedOn)
        .ThenByDescending(entry => entry.index)
        .Select(entry => entry.review)
        .ToList();

    /// <summary>Marks a lesson complete or not; the learner must be enrolled.</summary>
    public void SetLessonComplete(int courseId, int lessonId, bool isComplete)
    {
        if (!completed.TryGetValue(courseId, out var lessons) || FindCourse(courseId)?.Lessons.Any(lesson => lesson.Id == lessonId) != true)
            return;

        var changed = isComplete ? lessons.Add(lessonId) : lessons.Remove(lessonId);

        if (changed)
            Changed?.Invoke();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the course categories.</summary>
    public IReadOnlyList<LearningCategory> Categories { get; } = LearningSeedData.Categories();

    /// <summary>Gets the catalog.</summary>
    public IReadOnlyList<LearningCourse> Courses { get; } = LearningSeedData.Courses();

    /// <summary>Gets the courses the learner is enrolled in, in catalog order.</summary>
    public IReadOnlyList<LearningCourse> EnrolledCourses => Courses.Where(course => IsEnrolled(course.Id)).ToList();

    /// <summary>Gets the catalog filters.</summary>
    public LearningFilter Filter { get; } = new();

    #endregion

    #region Events

    /// <summary>Raised when enrollments, lesson completion, or reviews change.</summary>
    public event Action? Changed;

    #endregion
}
