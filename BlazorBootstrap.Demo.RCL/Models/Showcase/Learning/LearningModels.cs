namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;

/// <summary>Difficulty of a course.</summary>
public enum LearningLevel
{
    /// <summary>No prior knowledge needed.</summary>
    Beginner,

    /// <summary>Builds on the basics.</summary>
    Intermediate,

    /// <summary>For experienced learners.</summary>
    Advanced,
}

/// <summary>Kind of lesson.</summary>
public enum LearningLessonKind
{
    /// <summary>A video lesson; the demo shows a poster and the transcript.</summary>
    Video,

    /// <summary>A reading lesson.</summary>
    Reading,

    /// <summary>A short self-check.</summary>
    Quiz,
}

/// <summary>A course category.</summary>
public sealed record LearningCategory(string Id, string Name, IconName IconName, string Accent);

/// <summary>A multiple-choice quiz question with one right answer and an explanation shown after answering.</summary>
public sealed record LearningQuestion(int Id, string Prompt, IReadOnlyList<string> Options, int CorrectIndex, string Explanation);

/// <summary>A lesson inside a module. <see cref="Body" /> is seeded Markdown, never user input. A quiz lesson has <see cref="Questions" />.</summary>
public sealed record LearningLesson(int Id, string Title, LearningLessonKind Kind, int Minutes, string Body)
{
    #region Properties, Indexers

    /// <summary>Gets the quiz questions (quiz lessons only).</summary>
    public IReadOnlyList<LearningQuestion> Questions { get; init; } = Array.Empty<LearningQuestion>();

    /// <summary>Gets how many right answers pass the quiz: two thirds, rounded up.</summary>
    public int PassMark => (int)Math.Ceiling(Questions.Count * 2 / 3.0);

    #endregion
}

/// <summary>A group of lessons in a course.</summary>
public sealed record LearningModule(string Title, IReadOnlyList<LearningLesson> Lessons);

/// <summary>A course in the catalog.</summary>
public sealed record LearningCourse(
    int Id,
    string Title,
    string Subtitle,
    string CategoryId,
    LearningLevel Level,
    string Instructor,
    string InstructorRole,
    string Description,
    IReadOnlyList<string> Outcomes,
    IReadOnlyList<LearningModule> Modules,
    bool IsPopular)
{
    #region Properties, Indexers

    /// <summary>Gets the path of the course's PDF handout (a fictitious document created for the showcase).</summary>
    public string HandoutPath => $"_content/BlazorBootstrap.Demo.RCL/showcase/learning/handout-{Id}.pdf";

    /// <summary>Gets the cover image path; <c>ShowcaseImage</c> shows a placeholder until the file is supplied.</summary>
    public string ImagePath => $"_content/BlazorBootstrap.Demo.RCL/showcase/learning/course-{Id}.webp";

    /// <summary>Gets every lesson in order.</summary>
    public IReadOnlyList<LearningLesson> Lessons => Modules.SelectMany(module => module.Lessons).ToList();

    /// <summary>Gets the total length in minutes.</summary>
    public int Minutes => Modules.Sum(module => module.Lessons.Sum(lesson => lesson.Minutes));

    #endregion
}

/// <summary>Catalog filters, kept in the site state so they survive leaving the catalog and coming back.</summary>
public sealed class LearningFilter
{
    #region Properties, Indexers

    /// <summary>Gets or sets the category ID; empty means every category.</summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>Gets whether any filter is set.</summary>
    public bool IsActive => CategoryId.Length > 0 || Level.Length > 0 || !string.IsNullOrWhiteSpace(Search);

    /// <summary>Gets or sets the level name; empty means every level.</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>Gets or sets the search text, matched against the title, subtitle, and instructor.</summary>
    public string Search { get; set; } = string.Empty;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Clears every filter.</summary>
    public void Clear()
    {
        CategoryId = string.Empty;
        Level = string.Empty;
        Search = string.Empty;
    }

    #endregion

    #endregion
}

/// <summary>
/// A learner's review of a course. Seeded reviews have no account; a signed-in learner has at most one per course, which
/// they can edit or delete. <see cref="InstructorReply" /> is the instructor's public answer.
/// </summary>
public sealed record LearningReview(int Id, int CourseId, string Author, int Rating, string Body, DateTime PostedOn)
{
    #region Properties, Indexers

    /// <summary>Gets the reviewer's demo account, or <see langword="null" /> for a seeded review.</summary>
    public string? AccountId { get; init; }

    /// <summary>Gets when the review was last edited, if it was.</summary>
    public DateTime? EditedOn { get; init; }

    /// <summary>Gets the instructor's public reply, if any.</summary>
    public string? InstructorReply { get; init; }

    #endregion
}

/// <summary>A learner's enrollment in a course: when, and which lessons are complete.</summary>
public sealed class LearningEnrollment
{
    #region Properties, Indexers

    /// <summary>Gets the learner's demo account.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets the completed lesson IDs.</summary>
    public HashSet<int> CompletedLessons { get; init; } = new();

    /// <summary>Gets the course.</summary>
    public int CourseId { get; init; }

    /// <summary>Gets when the learner enrolled.</summary>
    public DateTime EnrolledOn { get; init; }

    /// <summary>Gets or sets when the learner last opened a lesson of the course.</summary>
    public DateTime LastActivity { get; set; }

    #endregion
}

/// <summary>A learner's quiz results for one quiz lesson.</summary>
public sealed class LearningQuizAttempt
{
    #region Properties, Indexers

    /// <summary>Gets the learner's demo account.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets or sets how many times the learner submitted the quiz.</summary>
    public int Attempts { get; set; }

    /// <summary>Gets or sets the best score (right answers).</summary>
    public int BestScore { get; set; }

    /// <summary>Gets or sets the answer chosen for each question in the last attempt (option index, by question order).</summary>
    public List<int> LastAnswers { get; set; } = new();

    /// <summary>Gets or sets when the last attempt was submitted.</summary>
    public DateTime LastAt { get; set; }

    /// <summary>Gets or sets the last score.</summary>
    public int LastScore { get; set; }

    /// <summary>Gets the quiz lesson.</summary>
    public int LessonId { get; init; }

    #endregion
}

/// <summary>An answer to a lesson question, from the instructor or another learner.</summary>
public sealed record LearningAnswer(string Author, bool IsInstructor, string Text, DateTime At);

/// <summary>A learner's question about a lesson, with its answers (lesson Q&amp;A). Text is plain.</summary>
public sealed class LearningQuestionThread
{
    #region Properties, Indexers

    /// <summary>Gets the asker's demo account, or <see langword="null" /> for a seeded question.</summary>
    public string? AccountId { get; init; }

    /// <summary>Gets the answers, oldest first.</summary>
    public List<LearningAnswer> Answers { get; init; } = new();

    /// <summary>Gets when it was asked.</summary>
    public DateTime AskedAt { get; init; }

    /// <summary>Gets the asker's name.</summary>
    public string Author { get; init; } = string.Empty;

    /// <summary>Gets the course.</summary>
    public int CourseId { get; init; }

    /// <summary>Gets the question number.</summary>
    public int Id { get; init; }

    /// <summary>Gets the lesson.</summary>
    public int LessonId { get; init; }

    /// <summary>Gets the question.</summary>
    public string Text { get; init; } = string.Empty;

    #endregion
}

/// <summary>A certificate of completion, verifiable by its code on the public verify page.</summary>
public sealed record LearningCertificate(string Code, int CourseId, string LearnerName, DateTime IssuedOn)
{
    #region Properties, Indexers

    /// <summary>Gets the learner's demo account, or <see langword="null" /> for another learner's seeded certificate.</summary>
    public string? AccountId { get; init; }

    #endregion
}

/// <summary>Outcome of a learner action.</summary>
public enum LearningResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>Sign in as a learner first, or it isn't the learner's.</summary>
    NotAllowed,

    /// <summary>The course, lesson, review, or question doesn't exist.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range.</summary>
    Invalid,

    /// <summary>Enroll in the course first.</summary>
    NotEnrolled,

    /// <summary>The item's state doesn't allow it (for example, deleting an answered question).</summary>
    WrongState,

    /// <summary>Nothing changed.</summary>
    NoChange,
}
