using System.Net;
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

    /// <summary>The demo instructor's name.</summary>
    public const string InstructorName = "Avery Calloway";

    /// <summary>The role of an instructor (builds and publishes courses, answers questions, replies to reviews).</summary>
    public const string InstructorRole = "instructor";

    /// <summary>The demo learner's name.</summary>
    public const string LearnerName = "Jordan Ashdown";

    /// <summary>The role of a learner (enrolls, learns, takes quizzes, asks questions, reviews).</summary>
    public const string LearnerRole = "learner";

    /// <summary>The fictitious school name.</summary>
    public const string SchoolName = "Quillpath Academy";

    /// <summary>The ID of the demo instructor account.</summary>
    public const string SeededInstructorId = "avery";

    /// <summary>The ID of the demo learner account.</summary>
    public const string SeededLearnerId = "jordan";

    /// <summary>When a fresh tab's clock starts; the clock moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 9, 0, 0);

    /// <summary>The seed's "today" all seeded dates are relative to.</summary>
    public static readonly DateTime Today = Now.Date;

    /// <summary>The catalog's rows: course 209 is the demo instructor's unpublished draft.</summary>
    private static readonly (string Title, string Subtitle, string Category, LearningLevel Level, string Instructor, string Role, string[] Modules)[] courseRows =
    {
        ("Blazor from First Principles", "Build interactive web apps in C#", "development", LearningLevel.Beginner, InstructorName, "Senior .NET engineer",
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
        ("Testing .NET Applications", "Ship changes with confidence", "development", LearningLevel.Advanced, InstructorName, "Senior .NET engineer",
            new[] { "Unit tests that last", "Integration tests", "Testing the UI" }),
        ("Storytelling with Charts", "Turn numbers into decisions", "data", LearningLevel.Intermediate, "Ines Merriweather", "Data visualization designer",
            new[] { "Choosing the right chart", "Color and emphasis", "Presenting findings" }),
        ("Accessible Blazor Components", "Build components everyone can use", "development", LearningLevel.Intermediate, InstructorName, "Senior .NET engineer",
            new[] { "Semantic markup in components", "Focus and announcements" }),
    };

    /// <summary>The demo instructor's courses: two published with learners, and a draft.</summary>
    private static readonly HashSet<int> instructorCourses = new() { 201, 207, 209 };

    /// <summary>Other learners on the demo instructor's roster (no sign-in): ID, name, and lessons done per course.</summary>
    private static readonly (string Id, string Name, int Course, int Done, double EnrolledDaysAgo, double ActiveDaysAgo)[] seededLearners =
    {
        ("s-maya", "Maya Rahman", 201, 9, 60, 41),
        ("s-theo", "Theo Lindqvist", 201, 7, 34, 1),
        ("s-priya", "Priya Sandhu", 201, 3, 21, 4),
        ("s-jonah", "Jonah Wells", 201, 0, 6, 6),
        ("s-elena", "Elena Vasquez", 201, 9, 40, 9),
        ("s-sam", "Sam Kowalski", 201, 1, 12, 10),
        ("s-ines", "Ines Moreau", 201, 5, 18, 2),
        ("s-callum", "Callum Brooks", 207, 9, 50, 15),
        ("s-noor", "Noor Haddad", 207, 4, 16, 3),
        ("s-lucas", "Lucas Ferreira", 207, 0, 2, 2),
        ("s-theo", "Theo Lindqvist", 207, 2, 9, 5),
        ("s-maya", "Maya Rahman", 207, 6, 25, 0.5),
    };

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
        new("QP-2D8R-H6NW", 201, "Maya Rahman", Now.AddDays(-41)) { AccountId = "s-maya" },
        new("QP-H7RW-4KTN", 201, "Elena Vasquez", Now.AddDays(-9)) { AccountId = "s-elena" },
        new("QP-X3MQ-9JDV", 207, "Callum Brooks", Now.AddDays(-15)) { AccountId = "s-callum" },
        new("QP-9TFC-3LPQ", 205, "Theo Lindqvist", Now.AddDays(-12)),
        new("QP-M4ZB-8EVK", 208, "Priya Sandhu", Now.AddDays(-3)),
    };

    /// <summary>The demo accounts: the learner with courses of their own, and the instructor of three courses.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededLearnerId, LearnerRole, LearnerName, "jordan.ashdown@example.com", "learn-demo-26", "JA"),
        new(SeededInstructorId, InstructorRole, InstructorName, "avery.calloway@example.com", "teach-demo-26", "AC"),
    };

    /// <summary>
    /// Enrollments in a fresh tab: the demo learner's (one course finished, two in progress), and other learners' in the
    /// demo instructor's courses, so the roster and dashboard have people on them.
    /// </summary>
    public static List<LearningEnrollment> Enrollments(IEnumerable<LearningCourse> courses)
    {
        var byId = courses.ToDictionary(course => course.Id);
        var enrollments = CompletedLessons(byId.Values).Select((pair, index) => new LearningEnrollment
        {
            AccountId = SeededLearnerId,
            CourseId = pair.Key,
            CompletedLessons = pair.Value,
            EnrolledOn = Now.AddDays(-(45 - (index * 12))),
            LastActivity = Now.AddDays(-(index + 1)),
            LearnerName = LearnerName,
        }).ToList();

        enrollments.AddRange(seededLearners.Select(learner => new LearningEnrollment
        {
            AccountId = learner.Id,
            CourseId = learner.Course,
            CompletedLessons = byId[learner.Course].Lessons.Take(learner.Done).Select(lesson => lesson.Id).ToHashSet(),
            EnrolledOn = Now.AddDays(-learner.EnrolledDaysAgo),
            LastActivity = Now.AddDays(-learner.ActiveDaysAgo),
            LearnerName = learner.Name,
        }));

        return enrollments;
    }

    /// <summary>Returns whether an account ID belongs to one of the seeded learners on the instructor's roster.</summary>
    public static bool IsSeededLearner(string accountId) => seededLearners.Any(learner => learner.Id == accountId);

    /// <summary>The demo learner's quiz results: every completed quiz lesson passed, some at the first try.</summary>
    public static List<LearningQuizAttempt> QuizAttempts(IEnumerable<LearningCourse> courses) =>
        Enrollments(courses)
            .Where(enrollment => enrollment.AccountId == SeededLearnerId)
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
        var answerId = 1;

        void Add(int courseId, int lessonIndex, string author, string? accountId, string text, double daysAgo, params (bool Instructor, string Text, double DaysAgo)[] answers)
        {
            var course = byId[courseId];
            var thread = new LearningQuestionThread
            {
                Id = id++, CourseId = courseId, LessonId = course.Lessons[lessonIndex].Id, AccountId = accountId, Author = author, Text = text, AskedAt = Now.AddDays(-daysAgo),
            };

            // The demo instructor's own answers carry her account, so she can delete them.
            foreach (var answer in answers)
                thread.Answers.Add(new LearningAnswer(answer.Instructor ? course.Instructor : "Sam K.", answer.Instructor, answer.Text, Now.AddDays(-answer.DaysAgo))
                {
                    Id = answerId++,
                    AccountId = answer.Instructor ? course.InstructorId : null,
                });

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
        Add(201, 7, "Ines M.", null, "Is it fine to keep app state in a static class for a small app?", 1);
        Add(207, 1, "Noor H.", null, "How do I test code that reads the current time?", 3);
        Add(207, 3, "Callum B.", null, "Should integration tests share one database or get a fresh one each?", 16,
            (true, "A fresh one per test class is a good default: tests stay independent, and it's still fast enough with a container.", 15));

        return threads;
    }

    /// <summary>The roles of the site.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(LearnerRole, "Learner", "/showcase/learning/progress", "/showcase/learning/profile", "Enrolls in courses, takes quizzes, asks questions, earns certificates, and writes reviews."),
        new(InstructorRole, "Instructor", "/showcase/learning/teach", "/showcase/learning/teach/account", "Builds and publishes courses and quizzes, answers learners' questions, and replies to reviews."),
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

    /// <summary>The catalog: eight published courses and the demo instructor's draft (209), which has draft lessons.</summary>
    public static IReadOnlyList<LearningCourse> Courses()
    {
        var courses = new List<LearningCourse>(courseRows.Length);

        for (var index = 0; index < courseRows.Length; index++)
        {
            var row = courseRows[index];
            var courseId = 201 + index;
            var isDraft = courseId == 209;
            var modules = new List<LearningModule>();

            for (var m = 0; m < row.Modules.Length; m++)
            {
                var lessons = new List<LearningLesson>();
                var topic = row.Modules[m];

                for (var l = 0; l < 3; l++)
                {
                    var number = (m * 3) + l + 1;
                    var kind = KindAt(l);
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
                        // The draft course's second module is still being written.
                        IsPublished = !isDraft || m == 0,
                    });
                }

                modules.Add(new LearningModule((courseId * 10) + m + 1, topic, lessons));
            }

            courses.Add(new LearningCourse(
                courseId,
                row.Title,
                row.Subtitle,
                row.Category,
                row.Level,
                row.Instructor,
                row.Role,
                $"{row.Subtitle}. This course walks through {string.Join(", ", row.Modules.SkipLast(1)).ToLowerInvariant()}, and {row.Modules[^1].ToLowerInvariant()}, with short videos, readings, and self-checks you can finish in an evening each.",
                new[] { $"Explain the core ideas of {row.Modules[0].ToLowerInvariant()}", $"Apply {row.Modules[1].ToLowerInvariant()} to a realistic project", $"Plan the next steps for {row.Modules[^1].ToLowerInvariant()}" },
                modules,
                IsPopular: index % 3 == 0)
            {
                InstructorId = instructorCourses.Contains(courseId) ? SeededInstructorId : null,
                IsPublished = !isDraft,
            });
        }

        return courses;
    }

    /// <summary>
    /// Returns a seeded lesson's Markdown body, or <see langword="null" /> for an ID that isn't a seeded lesson. A stored
    /// lesson may only carry the body its ID was seeded with, because Markdown renders raw HTML (restore checks this).
    /// </summary>
    public static string? SeedBody(int lessonId) => SeedLesson(lessonId) is { } seed ? Body(seed.Course, seed.Topic, seed.Kind) : null;

    /// <summary>
    /// Returns a seeded lesson's body as editor HTML (the same content as its Markdown), which the lesson editor starts
    /// from; <see langword="null" /> for an ID that isn't a seeded lesson. Text is entity-encoded, as the editor writes it.
    /// </summary>
    public static string? SeedHtml(int lessonId) => SeedLesson(lessonId) is { } seed ? BodyHtml(seed.Course, seed.Topic, seed.Kind) : null;

    /// <summary>Three to five reviews per course, with ratings from a fixed pattern; some have an instructor reply.</summary>
    public static List<LearningReview> Reviews(IEnumerable<LearningCourse> courses)
    {
        var reviews = new List<LearningReview>();
        var id = 1;

        // The draft course isn't published, so it has no reviews yet.
        foreach (var course in courses.Where(course => course.IsPublished))
        {
            var count = 3 + (course.Id % 3);

            for (var i = 0; i < count; i++)
            {
                var rating = ratingPattern[(course.Id + (i * 3)) % ratingPattern.Length];
                reviews.Add(new LearningReview(id++, course.Id, reviewers[(course.Id + i) % reviewers.Length], rating, reviewBodies[rating - 1][(course.Id + i) % 2],
                    Today.AddDays(-(5 + (i * 11) + (course.Id % 7))).AddHours(10))
                {
                    InstructorReply = rating <= 3 && i == 0 ? "Thanks for the honest feedback. I've added more worked examples to the first module since your review." : null,
                    RepliedOn = rating <= 3 && i == 0 ? Today.AddDays(-(3 + (i * 11) + (course.Id % 7))).AddHours(15) : null,
                });
            }
        }

        // One review on the demo instructor's course is advertising, already hidden from the course page.
        reviews.Add(new LearningReview(id, 207, "Deals Daily", 5, "Great course! Find cheap hosting deals every day at deals-daily.example.", Today.AddDays(-4).AddHours(8))
        {
            HiddenReason = LearningReviewHideReasons.Spam,
        });

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

    /// <summary>The kind of the lesson at a position in a seeded module: video, reading, then quiz.</summary>
    private static LearningLessonKind KindAt(int position) => position switch { 0 => LearningLessonKind.Video, 1 => LearningLessonKind.Reading, _ => LearningLessonKind.Quiz };

    /// <summary>Finds a seeded lesson's course title, module topic, and kind from its ID (course × 100 + number).</summary>
    private static (string Course, string Topic, LearningLessonKind Kind)? SeedLesson(int lessonId)
    {
        var index = (lessonId / 100) - 201;
        var number = lessonId % 100;

        if (index < 0 || index >= courseRows.Length || number < 1 || number > courseRows[index].Modules.Length * 3)
            return null;

        var row = courseRows[index];

        return (row.Title, row.Modules[(number - 1) / 3], KindAt((number - 1) % 3));
    }

    /// <summary>The seeded body as editor HTML, matching <see cref="Body" />.</summary>
    private static string BodyHtml(string course, string topic, LearningLessonKind kind)
    {
        var c = WebUtility.HtmlEncode(course);
        var t = WebUtility.HtmlEncode(topic);
        var lower = WebUtility.HtmlEncode(topic.ToLowerInvariant());

        return kind switch
        {
            LearningLessonKind.Video =>
                $"<h2>Transcript</h2><p>Welcome back to <strong>{c}</strong>. In this video we look at <em>{lower}</em>: what it is, why it matters, and where people usually go wrong.</p>"
                + "<p>We start with a small example, then build it up step by step until it looks like something you would ship.</p>"
                + "<blockquote>Tip: pause the video after each step and try it yourself before moving on.</blockquote>",
            LearningLessonKind.Reading =>
                $"<h2>{t} in practice</h2><p>The ideas from the video only stick once you use them. Work through the steps below on your own project, or on the sample from the handout.</p>"
                + "<h3>Steps</h3><ol><li>Pick one small, real task.</li><li>Apply the pattern from the video, one change at a time.</li><li>Check the result against the checklist in the handout.</li></ol>"
                + "<h3>Common mistakes</h3><ul><li>Doing everything at once instead of in small steps.</li><li>Skipping the check at the end.</li><li>Copying the example without adapting it to your case.</li></ul>"
                + "<blockquote>Takeaway: small, checked steps beat one big change.</blockquote>",
            _ =>
                $"<h2>Check your understanding</h2><p>Three questions on <em>{lower}</em>. Get at least two right to pass and complete the lesson; you can try again as often as you like, and you'll see why each answer is right.</p>"
                + "<h3>Before you start, reflect on these</h3><p>Answer these on your own, then compare with the notes below.</p>"
                + $"<ol><li>In one sentence, what problem does <em>{lower}</em> solve?</li><li>Name one mistake from the reading and how to avoid it.</li><li>Where in your own work could you use this next week?</li></ol>"
                + "<h3>Notes</h3><p>There are no single right answers here. Good answers name a <strong>specific</strong> situation and a <strong>specific</strong> next step.</p>",
        };
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
