using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

/// <summary>
/// Fixed, fictitious content for the "Quillpath Academy" learning site. Lesson bodies are seeded Markdown built from
/// fixed templates, so every load shows the same courses, lessons, and reviews.
/// </summary>
public static class LearningSeedData
{
    #region Fields and Constants

    /// <summary>The demo learner's name.</summary>
    public const string LearnerName = "Jordan Ashdown";

    /// <summary>The role of a learner (enrolls, learns, takes quizzes, asks questions, reviews).</summary>
    public const string LearnerRole = "learner";

    /// <summary>The fictitious school name.</summary>
    public const string SchoolName = "Quillpath Academy";

    /// <summary>The ID of the demo learner account.</summary>
    public const string SeededLearnerId = "jordan";

    /// <summary>When a fresh tab's clock starts; the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 0, 0);

    /// <summary>The seed's "today" all seeded dates are relative to.</summary>
    public static readonly DateTime Today = Now.Date;

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

    /// <summary>Certificates in a fresh tab: the demo learner's, and a few other learners' (so the verify page has something to find).</summary>
    public static List<LearningCertificate> Certificates() => new()
    {
        new("QP-7K4M-R2XD", 203, LearnerName, Now.AddDays(-20)) { AccountId = SeededLearnerId },
        new("QP-2D8R-H6NW", 201, "Maya Rahman", Now.AddDays(-41)),
        new("QP-9TFC-3LPQ", 205, "Theo Lindqvist", Now.AddDays(-12)),
        new("QP-M4ZB-8EVK", 208, "Priya Sandhu", Now.AddDays(-3)),
    };

    /// <summary>The demo accounts: the learner with courses of their own.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededLearnerId, LearnerRole, LearnerName, "jordan.ashdown@example.com", "learn-demo-26", "JA"),
    };

    /// <summary>The demo learner's enrollments: one course finished, two in progress.</summary>
    public static List<LearningEnrollment> Enrollments(IEnumerable<LearningCourse> courses) =>
        CompletedLessons(courses).Select((pair, index) => new LearningEnrollment
        {
            AccountId = SeededLearnerId,
            CourseId = pair.Key,
            CompletedLessons = pair.Value,
            EnrolledOn = Now.AddDays(-(45 - (index * 12))),
            LastActivity = Now.AddDays(-(index + 1)),
        }).ToList();

    /// <summary>The demo learner's quiz results: every completed quiz lesson passed, some at the first try.</summary>
    public static List<LearningQuizAttempt> QuizAttempts(IEnumerable<LearningCourse> courses) =>
        Enrollments(courses)
            .SelectMany(enrollment => courses.First(course => course.Id == enrollment.CourseId).Lessons
                .Where(lesson => lesson.Kind == LearningLessonKind.Quiz && enrollment.CompletedLessons.Contains(lesson.Id))
                .Select(lesson => new LearningQuizAttempt
                {
                    AccountId = SeededLearnerId,
                    LessonId = lesson.Id,
                    Attempts = 1 + (lesson.Id % 2),
                    BestScore = lesson.Id % 3 == 0 ? lesson.Questions.Count : lesson.PassMark,
                    LastScore = lesson.Id % 3 == 0 ? lesson.Questions.Count : lesson.PassMark,
                    LastAnswers = lesson.Questions.Select((question, index) => index < (lesson.Id % 3 == 0 ? lesson.Questions.Count : lesson.PassMark) ? question.CorrectIndex : (question.CorrectIndex + 1) % question.Options.Count).ToList(),
                    LastAt = enrollment.EnrolledOn.AddDays(2),
                }))
            .ToList();

    /// <summary>Lesson questions and answers: a few answered by instructors or learners, and one of the demo learner's still open.</summary>
    public static List<LearningQuestionThread> Questions(IReadOnlyList<LearningCourse> courses)
    {
        var byId = courses.ToDictionary(course => course.Id);
        var threads = new List<LearningQuestionThread>();
        var id = 1;

        void Add(int courseId, int lessonIndex, string author, string? accountId, string text, double daysAgo, params (bool Instructor, string Text, double DaysAgo)[] answers)
        {
            var course = byId[courseId];
            var thread = new LearningQuestionThread
            {
                Id = id++, CourseId = courseId, LessonId = course.Lessons[lessonIndex].Id, AccountId = accountId, Author = author, Text = text, AskedAt = Now.AddDays(-daysAgo),
            };

            foreach (var answer in answers)
                thread.Answers.Add(new LearningAnswer(answer.Instructor ? course.Instructor : "Sam K.", answer.Instructor, answer.Text, Now.AddDays(-answer.DaysAgo)));

            threads.Add(thread);
        }

        Add(201, 0, "Maya R.", null, "Do I need Visual Studio, or is VS Code enough for this course?", 20,
            (true, "VS Code with the C# Dev Kit is enough. Everything in the course works from the command line too.", 19));
        Add(201, 1, "Theo L.", null, "When should a component own its state, and when should the parent own it?", 14,
            (true, "Start with the component owning it. Lift it up as soon as two components need the same value.", 13),
            (false, "That rule of thumb saved me a lot of refactoring, thanks!", 12));
        Add(201, 4, LearnerName, SeededLearnerId, "Is it fine to validate on every keystroke, or only when the form is submitted?", 2);
        Add(203, 1, "Priya S.", null, "Is a LEFT JOIN always slower than an INNER JOIN?", 30,
            (true, "Not always. It depends on the data and indexes; measure with the query plan rather than guessing.", 29));
        Add(206, 0, "Elena V.", null, "How long should a status update be?", 8,
            (true, "Short enough to read on a phone: a headline, three bullets, and the one thing you need from the reader.", 7));

        return threads;
    }

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(LearnerRole, "Learner", "/showcase/learning/progress", "/showcase/learning/profile", "Enrolls in courses, takes quizzes, asks questions, earns certificates, and writes reviews."),
    };

    /// <summary>The lesson IDs completed in a fresh tab: one course finished, two in progress.</summary>
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

                    var lessonId = (courseId * 100) + number;
                    lessons.Add(new LearningLesson(lessonId, title, kind, minutes, Body(row.Title, topic, kind))
                    {
                        Questions = kind == LearningLessonKind.Quiz ? Quiz(lessonId, topic) : Array.Empty<LearningQuestion>(),
                    });
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

    /// <summary>Three to five reviews per course, with ratings from a fixed pattern; some have an instructor reply.</summary>
    public static List<LearningReview> Reviews(IEnumerable<LearningCourse> courses)
    {
        var reviews = new List<LearningReview>();
        var id = 1;

        foreach (var course in courses)
        {
            var count = 3 + (course.Id % 3);

            for (var i = 0; i < count; i++)
            {
                var rating = ratingPattern[(course.Id + (i * 3)) % ratingPattern.Length];
                reviews.Add(new LearningReview(id++, course.Id, reviewers[(course.Id + i) % reviewers.Length], rating, reviewBodies[rating - 1][(course.Id + i) % 2],
                    Today.AddDays(-(5 + (i * 11) + (course.Id % 7))).AddHours(10))
                {
                    InstructorReply = rating <= 3 && i == 0 ? "Thanks for the honest feedback. I've added more worked examples to the first module since your review." : null,
                });
            }
        }

        return reviews;
    }

    #endregion

    #region Private Methods

    /// <summary>Three questions for a quiz lesson; the options are rotated so the right answer isn't always first.</summary>
    private static IReadOnlyList<LearningQuestion> Quiz(int lessonId, string topic)
    {
        var name = topic.ToLowerInvariant();
        var templates = new (string Prompt, string[] Options, string Explanation)[]
        {
            ($"What is the best first step when you start with {name}?",
                new[] { "Pick one small, real task", "Change everything at once", "Skip the checklist to save time", "Copy the example exactly as it is" },
                "Small, real tasks show whether the idea works before you invest more."),
            ("Why check the result against the checklist at the end?",
                new[] { "It catches mistakes before they spread", "It makes the work look more formal", "The checklist replaces testing", "Only beginners need it" },
                "A quick check at the end finds problems while they are still cheap to fix."),
            ($"Which of these is a common mistake with {name}?",
                new[] { "Doing everything at once", "Working in small steps", "Checking each change", "Adapting the example to your case" },
                "Big-bang changes hide which step caused a problem; the other options are good habits."),
        };

        return templates.Select((template, q) =>
        {
            var rotation = (lessonId + q) % template.Options.Length;
            var options = template.Options.Skip(rotation).Concat(template.Options.Take(rotation)).ToList();

            return new LearningQuestion((lessonId * 10) + q + 1, template.Prompt, options, (template.Options.Length - rotation) % template.Options.Length, template.Explanation);
        }).ToList();
    }

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

            Three questions on *{topic.ToLowerInvariant()}*. Get at least two right to pass and complete the lesson; you can try again as often as you like, and you'll see why each answer is right.

            ### Before you start, reflect on these

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
