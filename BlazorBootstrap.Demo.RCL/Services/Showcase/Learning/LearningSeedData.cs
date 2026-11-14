using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

/// <summary>
/// Fixed, fictitious content for the "Quillpath Academy" learning site. Lesson bodies are seeded Markdown built from
/// fixed templates, so every load shows the same courses, lessons, and reviews.
/// </summary>
public static class LearningSeedData
{
    #region Fields and Constants

    /// <summary>The fictitious learner shown as signed in.</summary>
    public const string LearnerName = "Jordan Ashdown";

    /// <summary>The fictitious school name.</summary>
    public const string SchoolName = "Quillpath Academy";

    /// <summary>The fixed "today" all dates on the site are relative to.</summary>
    public static readonly DateTime Today = new(2026, 11, 24);

    private static readonly string[] reviewers = { "Maya R.", "Theo L.", "Priya S.", "Jonah W.", "Elena V.", "Sam K.", "Ines M.", "Callum B." };

    private static readonly int[] ratingPattern = { 5, 4, 5, 5, 3, 4, 5, 4 };

    /// <summary>Two review texts per star rating, from 1 to 5 stars.</summary>
    private static readonly string[][] reviewBodies =
    {
        new[] { "Hard going for me; I needed more examples before the exercises.", "Not for me. I expected more depth for the time it takes." },
        new[] { "Some good parts, but the self-checks felt too vague to be useful.", "Okay overview. I had to look elsewhere for the details." },
        new[] { "Useful, but a few lessons felt rushed. The handout helped.", "Decent course. The readings were better than the videos." },
        new[] { "Solid course. Clear explanations and realistic exercises.", "Really well paced. I used the ideas at work the same week." },
        new[] { "One of the best courses I've taken. The instructor explains the why, not just the how.", "Excellent. Short lessons, practical examples, and a handout I keep coming back to." },
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The course categories.</summary>
    public static IReadOnlyList<LearningCategory> Categories() => new List<LearningCategory>
    {
        new("development", "Development", IconName.CodeSlash, "#1d4ed8"),
        new("design", "Design", IconName.Palette, "#be185d"),
        new("data", "Data", IconName.BarChart, "#047857"),
        new("business", "Business", IconName.Briefcase, "#a16207"),
        new("growth", "Personal growth", IconName.Flower1, "#7c3aed"),
    };

    /// <summary>The lesson IDs completed in a fresh session: one course finished, two in progress.</summary>
    public static Dictionary<int, HashSet<int>> CompletedLessons(IEnumerable<LearningCourse> courses)
    {
        var byId = courses.ToDictionary(course => course.Id);

        return new Dictionary<int, HashSet<int>>
        {
            [203] = byId[203].Lessons.Select(lesson => lesson.Id).ToHashSet(),
            [201] = byId[201].Lessons.Take(4).Select(lesson => lesson.Id).ToHashSet(),
            [206] = byId[206].Lessons.Take(1).Select(lesson => lesson.Id).ToHashSet(),
        };
    }

    /// <summary>The catalog.</summary>
    public static IReadOnlyList<LearningCourse> Courses()
    {
        var rows = new (string Title, string Subtitle, string Category, LearningLevel Level, string Instructor, string Role, string[] Modules)[]
        {
            ("Blazor from First Principles", "Build interactive web apps in C#", "development", LearningLevel.Beginner, "Avery Calloway", "Senior .NET engineer",
                new[] { "Components and rendering", "Forms and validation", "State and routing" }),
            ("Accessible Interfaces", "Design and build for every user", "design", LearningLevel.Intermediate, "Rowan Pemberton", "Accessibility lead",
                new[] { "Semantics and landmarks", "Keyboard and focus", "Color and contrast" }),
            ("Practical SQL for Analysts", "Answer real questions with data", "data", LearningLevel.Beginner, "Priya Northcott", "Analytics manager",
                new[] { "Selecting and filtering", "Joins that make sense", "Grouping and reporting" }),
            ("Product Strategy Essentials", "Decide what to build next", "business", LearningLevel.Intermediate, "Theo Whitlock", "Head of product",
                new[] { "Understanding customers", "Prioritizing bets", "Measuring outcomes" }),
            ("Design Systems in Practice", "Scale consistent interfaces", "design", LearningLevel.Advanced, "Elena Larkspur", "Design systems lead",
                new[] { "Tokens and themes", "Component APIs", "Governance and adoption" }),
            ("Clear Writing at Work", "Write so people act", "growth", LearningLevel.Beginner, "Sam Fairbank", "Editor and coach",
                new[] { "Know your reader", "Structure and brevity", "Editing your own work" }),
            ("Testing .NET Applications", "Ship changes with confidence", "development", LearningLevel.Advanced, "Jonah Ravenscroft", "Principal engineer",
                new[] { "Unit tests that last", "Integration tests", "Testing the UI" }),
            ("Storytelling with Charts", "Turn numbers into decisions", "data", LearningLevel.Intermediate, "Ines Merriweather", "Data visualization designer",
                new[] { "Choosing the right chart", "Color and emphasis", "Presenting findings" }),
        };

        var courses = new List<LearningCourse>(rows.Length);

        for (var index = 0; index < rows.Length; index++)
        {
            var row = rows[index];
            var courseId = 201 + index;
            var modules = new List<LearningModule>();

            for (var m = 0; m < row.Modules.Length; m++)
            {
                var lessons = new List<LearningLesson>();
                var topic = row.Modules[m];

                for (var l = 0; l < 3; l++)
                {
                    var number = (m * 3) + l + 1;
                    var kind = l switch { 0 => LearningLessonKind.Video, 1 => LearningLessonKind.Reading, _ => LearningLessonKind.Quiz };
                    var title = l switch
                    {
                        0 => $"{topic}: an overview",
                        1 => $"{topic} in practice",
                        _ => $"Check your understanding: {topic.ToLowerInvariant()}",
                    };

                    // Each course has its own pace, so course lengths differ.
                    var minutes = kind switch
                    {
                        LearningLessonKind.Video => 5 + (index * 2) + (number % 4),
                        LearningLessonKind.Reading => 8 + ((index * 5) % 11) + (number % 3),
                        _ => 3 + (index % 3),
                    };

                    lessons.Add(new LearningLesson((courseId * 100) + number, title, kind, minutes, Body(row.Title, topic, kind)));
                }

                modules.Add(new LearningModule(topic, lessons));
            }

            courses.Add(new LearningCourse(
                courseId,
                row.Title,
                row.Subtitle,
                row.Category,
                row.Level,
                row.Instructor,
                row.Role,
                $"{row.Subtitle}. This course walks through {string.Join(", ", row.Modules.Take(2)).ToLowerInvariant()}, and {row.Modules[^1].ToLowerInvariant()}, with short videos, readings, and self-checks you can finish in an evening each.",
                new[] { $"Explain the core ideas of {row.Modules[0].ToLowerInvariant()}", $"Apply {row.Modules[1].ToLowerInvariant()} to a realistic project", $"Plan the next steps for {row.Modules[2].ToLowerInvariant()}" },
                modules,
                IsPopular: index % 3 == 0));
        }

        return courses;
    }

    /// <summary>Three to five reviews per course, with ratings from a fixed pattern.</summary>
    public static List<LearningReview> Reviews(IEnumerable<LearningCourse> courses)
    {
        var reviews = new List<LearningReview>();

        foreach (var course in courses)
        {
            var count = 3 + (course.Id % 3);

            for (var i = 0; i < count; i++)
            {
                var rating = ratingPattern[(course.Id + (i * 3)) % ratingPattern.Length];
                reviews.Add(new LearningReview(course.Id, reviewers[(course.Id + i) % reviewers.Length], rating, reviewBodies[rating - 1][(course.Id + i) % 2], Today.AddDays(-(5 + (i * 11) + (course.Id % 7))), IsFromThisSession: false));
            }
        }

        return reviews;
    }

    #endregion

    #region Private Methods

    private static string Body(string course, string topic, LearningLessonKind kind) => kind switch
    {
        LearningLessonKind.Video => $"""
            ## Transcript

            Welcome back to **{course}**. In this video we look at *{topic.ToLowerInvariant()}*: what it is, why it matters, and where people usually go wrong.

            We start with a small example, then build it up step by step until it looks like something you would ship.

            > Tip: pause the video after each step and try it yourself before moving on.
            """,
        LearningLessonKind.Reading => $"""
            ## {topic} in practice

            The ideas from the video only stick once you use them. Work through the steps below on your own project, or on the sample from the handout.

            ### Steps

            1. Pick one small, real task.
            2. Apply the pattern from the video, one change at a time.
            3. Check the result against the checklist in the handout.

            ### Common mistakes

            - Doing everything at once instead of in small steps.
            - Skipping the check at the end.
            - Copying the example without adapting it to your case.

            > Takeaway: small, checked steps beat one big change.
            """,
        _ => $"""
            ## Check your understanding

            Answer these on your own, then compare with the notes below.

            1. In one sentence, what problem does *{topic.ToLowerInvariant()}* solve?
            2. Name one mistake from the reading and how to avoid it.
            3. Where in your own work could you use this next week?

            ### Notes

            There are no single right answers here. Good answers name a **specific** situation and a **specific** next step.
            """,
    };

    #endregion

    #endregion
}
