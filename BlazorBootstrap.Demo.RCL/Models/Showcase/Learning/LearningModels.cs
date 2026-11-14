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

/// <summary>A lesson inside a module. <see cref="Body" /> is seeded Markdown, never user input.</summary>
public sealed record LearningLesson(int Id, string Title, LearningLessonKind Kind, int Minutes, string Body);

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

/// <summary>A learner's review of a course.</summary>
public sealed record LearningReview(int CourseId, string Author, int Rating, string Body, DateTime PostedOn, bool IsFromThisSession);
