using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

/// <summary>
/// State of the "Quillpath Academy" learning site for the current browser tab: the courses (which the instructor edits),
/// enrollments and lesson progress, quiz results, reviews, lesson questions and answers, certificates, and the demo
/// sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase foundation, and its clock moves on from the
/// seed time (D-029); nothing is sent.
/// </summary>
/// <remarks>
/// Learners see a course through a learner view: only published lessons, and only modules that have some. Progress,
/// completion, and certificates count published lessons. Instructor actions need the instructor role and the course's
/// ownership, checked here.
/// </remarks>
public sealed partial class LearningState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The longest course description.</summary>
    public const int MaxCourseDescriptionLength = 1000;

    /// <summary>The longest course subtitle.</summary>
    public const int MaxCourseSubtitleLength = 120;

    /// <summary>The longest course title.</summary>
    public const int MaxCourseTitleLength = 80;

    /// <summary>The most courses the tab holds.</summary>
    public const int MaxCourses = 40;

    /// <summary>The longest quiz explanation (optional).</summary>
    public const int MaxExplanationLength = 500;

    /// <summary>The longest instructor role shown on the course page.</summary>
    public const int MaxInstructorRoleLength = 80;

    /// <summary>The longest lesson body, formatting included.</summary>
    public const int MaxLessonHtmlLength = 20_000;

    /// <summary>The longest lesson, in minutes.</summary>
    public const int MaxLessonMinutes = 240;

    /// <summary>The most lessons in a course.</summary>
    public const int MaxLessonsPerCourse = 60;

    /// <summary>The longest lesson title.</summary>
    public const int MaxLessonTitleLength = 120;

    /// <summary>The longest module title.</summary>
    public const int MaxModuleTitleLength = 80;

    /// <summary>The most modules in a course.</summary>
    public const int MaxModulesPerCourse = 12;

    /// <summary>The most options a quiz question has.</summary>
    public const int MaxOptions = 6;

    /// <summary>The longest quiz option.</summary>
    public const int MaxOptionLength = 200;

    /// <summary>The longest "what you'll learn" outcome.</summary>
    public const int MaxOutcomeLength = 160;

    /// <summary>The most "what you'll learn" outcomes.</summary>
    public const int MaxOutcomes = 8;

    /// <summary>The longest quiz question prompt.</summary>
    public const int MaxPromptLength = 300;

    /// <summary>The longest question or answer in a lesson's Q&amp;A.</summary>
    public const int MaxQuestionLength = 1000;

    /// <summary>The most questions in a quiz.</summary>
    public const int MaxQuestionsPerQuiz = 10;

    /// <summary>The longest review (and instructor reply).</summary>
    public const int MaxReviewLength = 1000;

    /// <summary>The fewest options a quiz question has.</summary>
    public const int MinOptions = 2;

    private const int FirstAuthoredCourseId = 210;

    private const int FirstAuthoredLessonId = 30_001;

    private const int FirstAuthoredModuleId = 3_001;

    private const int FirstAuthoredQuestionId = 300_001;

    private const int MaxAnswersPerThread = 50;

    private const int MaxCertificates = 500;

    private const int MaxEnrollments = 1000;

    private const int MaxReviews = 2000;

    private const int MaxThreads = 1000;

    // Upper bounds of the ID counters; actions stop before them, so a saved tab always restores.
    private const int NextAnswerIdLimit = 1_000_000;

    private const int NextCourseIdLimit = 1_000;

    private const int NextLessonIdLimit = 1_000_000;

    private const int NextModuleIdLimit = 100_000;

    private const int NextQuestionIdLimit = 10_000_000;

    private const int NextReviewIdLimit = 1_000_000;

    private const int NextThreadIdLimit = 1_000_000;

    /// <summary>
    /// The largest the site's snapshot may grow: the store's limit less room for its envelope (the sign-in and accounts).
    /// The store skips a larger snapshot without a word, so actions that add text refuse (Invalid) before that happens.
    /// </summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<LearningQuizAttempt> attempts;

    private readonly List<LearningCertificate> certificates = LearningSeedData.Certificates();

    private readonly List<LearningCourse> courses = LearningSeedData.Courses().ToList();

    private readonly List<LearningEnrollment> enrollments;

    private readonly List<LearningReview> reviews;

    private readonly List<LearningQuestionThread> threads;

    private int nextAnswerId;

    private int nextCourseId = FirstAuthoredCourseId;

    private int nextLessonId = FirstAuthoredLessonId;

    private int nextModuleId = FirstAuthoredModuleId;

    private int nextQuestionId = FirstAuthoredQuestionId;

    private int nextReviewId;

    private int nextThreadId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public LearningState()
    {
        enrollments = LearningSeedData.Enrollments(courses);
        attempts = LearningSeedData.QuizAttempts(courses);
        reviews = LearningSeedData.Reviews(courses);
        threads = LearningSeedData.Questions(courses);
        nextReviewId = reviews.Max(review => review.Id) + 1;
        nextThreadId = threads.Max(thread => thread.Id) + 1;
        nextAnswerId = threads.SelectMany(thread => thread.Answers).Max(answer => answer.Id) + 1;
        Identity = new ShowcaseIdentity(LearningSeedData.Roles(), LearningSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Adds an empty module at the end of one of the instructor's courses.</summary>
    public LearningResult AddModule(int courseId, string title, out LearningModule? module)
    {
        module = null;

        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        title = title?.Trim() ?? string.Empty;

        if (!IsText(title, MaxModuleTitleLength) || course.Modules.Count >= MaxModulesPerCourse || nextModuleId >= NextModuleIdLimit)
            return LearningResult.Invalid;

        module = new LearningModule(nextModuleId++, title, Array.Empty<LearningLesson>());
        ReplaceCourse(course with { Modules = course.Modules.Append(module).ToList() });

        return LearningResult.Ok;
    }

    /// <summary>
    /// Answers a question on one of the instructor's courses (plain text, up to <see cref="MaxQuestionLength" /> characters).
    /// The learner sees it marked "Instructor".
    /// </summary>
    public LearningResult AnswerQuestion(int threadId, string text, out LearningAnswer? answer)
    {
        answer = null;

        if (Identity.Current is not { RoleId: LearningSeedData.InstructorRole } account)
            return LearningResult.NotAllowed;

        if (threads.FirstOrDefault(thread => thread.Id == threadId) is not { } thread || OwnedCourse(thread.CourseId) is null)
            return LearningResult.NotFound;

        text = text?.Trim() ?? string.Empty;

        if (!IsText(text, MaxQuestionLength) || thread.Answers.Count >= MaxAnswersPerThread || nextAnswerId >= NextAnswerIdLimit)
            return LearningResult.Invalid;

        answer = new LearningAnswer(Clip(account.Name, ShowcaseIdentity.MaxNameLength), true, text, Clock.Now) { AccountId = account.Id, Id = nextAnswerId++ };
        thread.Answers.Add(answer);

        if (!FitsSnapshot())
        {
            thread.Answers.Remove(answer);
            answer = null;

            return LearningResult.Invalid;
        }

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Asks a question about a published lesson of a course the learner is enrolled in (plain text, up to <see cref="MaxQuestionLength" /> characters).</summary>
    public LearningResult AskQuestion(int courseId, int lessonId, string text, out LearningQuestionThread? thread)
    {
        thread = null;

        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (FindCourse(courseId) is not { } course || course.Lessons.All(lesson => lesson.Id != lessonId))
            return LearningResult.NotFound;

        if (!IsEnrolled(courseId))
            return LearningResult.NotEnrolled;

        text = text?.Trim() ?? string.Empty;

        if (!IsText(text, MaxQuestionLength) || threads.Count >= MaxThreads || nextThreadId >= NextThreadIdLimit)
            return LearningResult.Invalid;

        thread = new LearningQuestionThread { Id = nextThreadId++, CourseId = courseId, LessonId = lessonId, AccountId = account.Id, Author = account.Name, Text = text, AskedAt = Clock.Now };
        threads.Add(thread);

        if (!FitsSnapshot())
        {
            threads.Remove(thread);
            thread = null;

            return LearningResult.Invalid;
        }

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Returns the average rating of a course from its visible reviews, or 0 without any.</summary>
    public double AverageRating(int courseId)
    {
        var ratings = reviews.Where(review => review.CourseId == courseId && review.HiddenReason is null).Select(review => review.Rating).ToList();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    /// <summary>Returns the snapshot: courses, enrollments, quiz results, reviews, questions, certificates, and the ID counters.</summary>
    public object CaptureData() => new LearningSnapshot
    {
        Attempts = attempts.ToList(),
        Certificates = certificates.ToList(),
        Courses = courses.ToList(),
        Enrollments = enrollments.ToList(),
        NextAnswerId = nextAnswerId,
        NextCourseId = nextCourseId,
        NextLessonId = nextLessonId,
        NextModuleId = nextModuleId,
        NextQuestionId = nextQuestionId,
        NextReviewId = nextReviewId,
        NextThreadId = nextThreadId,
        Reviews = reviews.ToList(),
        Threads = threads.ToList(),
    };

    /// <summary>Returns a learner's view of any course, published or not, for a certificate (public pages), or <see langword="null" />.</summary>
    public LearningCourse? CertificateCourse(int courseId) => StoredCourse(courseId) is { } course ? LearnerView(course) : null;

    /// <summary>Returns the signed-in learner's certificate for a course, if issued.</summary>
    public LearningCertificate? CertificateFor(int courseId) =>
        Identity.Current is { } account ? certificates.FirstOrDefault(certificate => certificate.CourseId == courseId && certificate.AccountId == account.Id) : null;

    /// <summary>Returns how many published lessons of a course the signed-in learner completed.</summary>
    public int CompletedCount(int courseId) =>
        Enrollment(courseId) is { } enrollment && StoredCourse(courseId) is { } course ? CompletedPublished(course, enrollment) : 0;

    /// <summary>
    /// Deletes one of the instructor's courses. A course with learners or certificates can't be deleted (unpublish it
    /// instead); its reviews and questions go with it.
    /// </summary>
    public LearningResult DeleteCourse(int courseId)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (enrollments.Any(enrollment => enrollment.CourseId == courseId) || certificates.Any(certificate => certificate.CourseId == courseId))
            return LearningResult.WrongState;

        courses.Remove(course);
        reviews.RemoveAll(review => review.CourseId == courseId);
        threads.RemoveAll(thread => thread.CourseId == courseId);
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Deletes one of the instructor's own answers.</summary>
    public LearningResult DeleteAnswer(int threadId, int answerId)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.InstructorRole } account)
            return LearningResult.NotAllowed;

        if (threads.FirstOrDefault(thread => thread.Id == threadId) is not { } thread || OwnedCourse(thread.CourseId) is null)
            return LearningResult.NotFound;

        if (thread.Answers.RemoveAll(answer => answer.Id == answerId && answer.AccountId == account.Id) == 0)
            return LearningResult.NotFound;

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>
    /// Deletes a lesson of one of the instructor's courses: learners' completions, quiz results, and questions about it go
    /// with it. A course that is published or has learners keeps at least one published lesson.
    /// </summary>
    public LearningResult DeleteLesson(int courseId, int lessonId)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.Lessons.FirstOrDefault(lesson => lesson.Id == lessonId) is not { } lesson)
            return LearningResult.NotFound;

        if (lesson.IsPublished && IsLastPublished(course, lessonId))
            return LearningResult.WrongState;

        ReplaceCourse(course with { Modules = course.Modules.Select(module => module with { Lessons = module.Lessons.Where(item => item.Id != lessonId).ToList() }).ToList() });

        foreach (var enrollment in enrollments.Where(enrollment => enrollment.CourseId == courseId))
            enrollment.CompletedLessons.Remove(lessonId);

        attempts.RemoveAll(attempt => attempt.LessonId == lessonId);
        threads.RemoveAll(thread => thread.LessonId == lessonId);
        IssueCertificatesFor(courseId);
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Deletes an empty module of one of the instructor's courses.</summary>
    public LearningResult DeleteModule(int courseId, int moduleId)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.Modules.FirstOrDefault(module => module.Id == moduleId) is not { } module)
            return LearningResult.NotFound;

        if (module.Lessons.Count > 0)
            return LearningResult.WrongState;

        ReplaceCourse(course with { Modules = course.Modules.Where(item => item.Id != moduleId).ToList() });

        return LearningResult.Ok;
    }

    /// <summary>Deletes one of the signed-in learner's questions while it has no answers.</summary>
    public LearningResult DeleteQuestion(int threadId)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (threads.FirstOrDefault(thread => thread.Id == threadId && thread.AccountId == account.Id) is not { } thread)
            return LearningResult.NotFound;

        if (thread.Answers.Count > 0)
            return LearningResult.WrongState;

        threads.Remove(thread);
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Removes the instructor's reply from a review of one of their courses.</summary>
    public LearningResult DeleteReply(int reviewId)
    {
        if (OwnedReview(reviewId, out var index) is { } refusal)
            return refusal;

        if (reviews[index].InstructorReply is null)
            return LearningResult.NoChange;

        reviews[index] = reviews[index] with { InstructorReply = null, RepliedOn = null };
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Deletes the signed-in learner's review of a course.</summary>
    public LearningResult DeleteReview(int courseId)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (reviews.RemoveAll(review => review.CourseId == courseId && review.AccountId == account.Id) == 0)
            return LearningResult.NotFound;

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Enrolls the signed-in learner in a published course; enrolling twice changes nothing.</summary>
    public LearningResult Enroll(int courseId)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (StoredCourse(courseId) is not { } course)
            return LearningResult.NotFound;

        if (IsEnrolled(courseId))
            return LearningResult.NoChange;

        if (!course.IsPublished)
            return LearningResult.WrongState;

        if (enrollments.Count >= MaxEnrollments)
            return LearningResult.Invalid;

        enrollments.Add(new LearningEnrollment
        {
            AccountId = account.Id, CourseId = courseId, EnrolledOn = Clock.Now, LastActivity = Clock.Now, LearnerName = Clip(account.Name, ShowcaseIdentity.MaxNameLength),
        });
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Finds a category by its ID; unknown IDs fall back to the first category.</summary>
    public LearningCategory FindCategory(string categoryId) => Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories[0];

    /// <summary>
    /// Finds a course as learners see it (published lessons only), or returns <see langword="null" />. An unpublished course
    /// is found only by its enrolled learners, learners who earned its certificate, and its instructor.
    /// </summary>
    public LearningCourse? FindCourse(int courseId) =>
        StoredCourse(courseId) is { } course && (course.IsPublished || IsEnrolled(courseId) || CertificateFor(courseId) is not null || Owns(course))
            ? LearnerView(course)
            : null;

    /// <summary>Returns one of the signed-in instructor's courses with every lesson, drafts included, or <see langword="null" />.</summary>
    public LearningCourse? FindCourseForEdit(int courseId) => OwnedCourse(courseId);

    /// <summary>Returns how far the learners of one of the instructor's courses got, or <see langword="null" /> for another course.</summary>
    public LearningFunnel? Funnel(int courseId)
    {
        if (OwnedCourse(courseId) is not { } course)
            return null;

        var total = PublishedLessonIds(course).Count;
        var done = enrollments.Where(enrollment => enrollment.CourseId == courseId).Select(enrollment => CompletedPublished(course, enrollment)).ToList();

        return new LearningFunnel(done.Count, done.Count(count => count > 0), done.Count(count => total > 0 && count * 2 >= total), done.Count(count => total > 0 && count == total));
    }

    /// <summary>Hides a review of one of the instructor's courses from the course page, for one of <see cref="LearningReviewHideReasons.All" />.</summary>
    public LearningResult HideReview(int reviewId, string reason)
    {
        if (OwnedReview(reviewId, out var index) is { } refusal)
            return refusal;

        if (!LearningReviewHideReasons.All.Contains(reason))
            return LearningResult.Invalid;

        if (reviews[index].HiddenReason == reason)
            return LearningResult.NoChange;

        reviews[index] = reviews[index] with { HiddenReason = reason };
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Returns whether the signed-in learner finished every published lesson of a course.</summary>
    public bool IsComplete(int courseId) =>
        Enrollment(courseId) is { } enrollment && StoredCourse(courseId) is { } course && IsCompleteFor(course, enrollment);

    /// <summary>Returns whether the signed-in learner is enrolled in a course.</summary>
    public bool IsEnrolled(int courseId) => Enrollment(courseId) is not null;

    /// <summary>Returns whether the signed-in learner completed a lesson.</summary>
    public bool IsLessonComplete(int courseId, int lessonId) => Enrollment(courseId)?.CompletedLessons.Contains(lessonId) ?? false;

    /// <summary>Returns when the signed-in learner was last active in a course, or <see langword="null" /> when not enrolled.</summary>
    public DateTime? LastActivity(int courseId) => Enrollment(courseId)?.LastActivity;

    /// <summary>Returns what deleting a lesson of one of the instructor's courses (or changing its quiz) takes with it; nothing for another lesson.</summary>
    public LearningLessonImpact LessonImpact(int lessonId) =>
        courses.Any(course => Owns(course) && course.Lessons.Any(lesson => lesson.Id == lessonId))
            ? new(
                enrollments.Count(enrollment => enrollment.CompletedLessons.Contains(lessonId)),
                attempts.Count(attempt => attempt.LessonId == lessonId),
                threads.Count(thread => thread.LessonId == lessonId))
            : new(0, 0, 0);

    /// <summary>Moves a lesson up (<paramref name="offset" /> −1) or down (+1) inside its module.</summary>
    public LearningResult MoveLesson(int courseId, int lessonId, int offset)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.Modules.FirstOrDefault(module => module.Lessons.Any(lesson => lesson.Id == lessonId)) is not { } module)
            return LearningResult.NotFound;

        var lessons = module.Lessons.ToList();
        var index = lessons.FindIndex(lesson => lesson.Id == lessonId);

        if (offset is not (-1 or 1))
            return LearningResult.Invalid;

        if (index + offset < 0 || index + offset >= lessons.Count)
            return LearningResult.NoChange;

        (lessons[index], lessons[index + offset]) = (lessons[index + offset], lessons[index]);
        ReplaceCourse(course with { Modules = course.Modules.Select(item => item.Id == module.Id ? item with { Lessons = lessons } : item).ToList() });

        return LearningResult.Ok;
    }

    /// <summary>Moves a module up (<paramref name="offset" /> −1) or down (+1) in its course.</summary>
    public LearningResult MoveModule(int courseId, int moduleId, int offset)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        var modules = course.Modules.ToList();
        var index = modules.FindIndex(module => module.Id == moduleId);

        if (index < 0)
            return LearningResult.NotFound;

        if (offset is not (-1 or 1))
            return LearningResult.Invalid;

        if (index + offset < 0 || index + offset >= modules.Count)
            return LearningResult.NoChange;

        (modules[index], modules[index + offset]) = (modules[index + offset], modules[index]);
        ReplaceCourse(course with { Modules = modules });

        return LearningResult.Ok;
    }

    /// <summary>Returns the first published lesson not yet complete, or the first lesson when everything is done.</summary>
    public LearningLesson NextLesson(LearningCourse course) =>
        course.Lessons.FirstOrDefault(lesson => !IsLessonComplete(course.Id, lesson.Id)) ?? course.Lessons[0];

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Records that the signed-in learner opened a lesson (for "continue where you left off"); the change is saved.</summary>
    public void OpenedLesson(int courseId)
    {
        if (Enrollment(courseId) is not { } enrollment || enrollment.LastActivity == Clock.Now)
            return;

        enrollment.LastActivity = Clock.Now;
        Changed?.Invoke();
    }

    /// <summary>Returns the share of published lessons complete, from 0 to 100.</summary>
    public int ProgressPercent(int courseId) =>
        StoredCourse(courseId) is { } course && PublishedLessonIds(course).Count is var total and > 0 ? CompletedCount(courseId) * 100 / total : 0;

    /// <summary>Returns the questions about a lesson, newest first.</summary>
    public IReadOnlyList<LearningQuestionThread> QuestionsFor(int lessonId) => threads.Where(thread => thread.LessonId == lessonId).OrderByDescending(thread => thread.AskedAt).ToList();

    /// <summary>Returns the signed-in learner's results for a quiz lesson, if they tried it.</summary>
    public LearningQuizAttempt? QuizResult(int lessonId) =>
        Identity.Current is { } account ? attempts.FirstOrDefault(attempt => attempt.LessonId == lessonId && attempt.AccountId == account.Id) : null;

    /// <summary>Returns how many visible reviews of a course gave 1 to 5 stars (index 0 is one star).</summary>
    public int[] RatingCounts(int courseId)
    {
        var counts = new int[5];

        foreach (var review in reviews.Where(review => review.CourseId == courseId && review.HiddenReason is null))
            counts[review.Rating - 1]++;

        return counts;
    }

    /// <summary>Renames a module of one of the instructor's courses.</summary>
    public LearningResult RenameModule(int courseId, int moduleId, string title)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.Modules.FirstOrDefault(module => module.Id == moduleId) is not { } module)
            return LearningResult.NotFound;

        title = title?.Trim() ?? string.Empty;

        if (!IsText(title, MaxModuleTitleLength))
            return LearningResult.Invalid;

        if (module.Title == title)
            return LearningResult.NoChange;

        ReplaceCourse(course with { Modules = course.Modules.Select(item => item.Id == moduleId ? item with { Title = title } : item).ToList() });

        return LearningResult.Ok;
    }

    /// <summary>Returns the signed-in learner's review of a course, if any (hidden or not).</summary>
    public LearningReview? ReviewBy(int courseId) =>
        Identity.Current is { } account ? reviews.FirstOrDefault(review => review.CourseId == courseId && review.AccountId == account.Id) : null;

    /// <summary>Returns the reviews of a course that show on its page, newest first.</summary>
    public IReadOnlyList<LearningReview> ReviewsFor(int courseId) =>
        reviews.Where(review => review.CourseId == courseId && review.HiddenReason is null).OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).ToList();

    /// <summary>
    /// Creates a course (<paramref name="courseId" /> 0; unpublished, with no modules) or edits the details of one of the
    /// instructor's courses.
    /// </summary>
    public LearningResult SaveCourse(int courseId, string title, string subtitle, string categoryId, LearningLevel level, string instructorRole, string description, IReadOnlyList<string> outcomes, out LearningCourse? saved)
    {
        saved = null;

        if (Identity.Current is not { RoleId: LearningSeedData.InstructorRole } account)
            return LearningResult.NotAllowed;

        var existing = courseId == 0 ? null : OwnedCourse(courseId);

        if (courseId != 0 && existing is null)
            return LearningResult.NotFound;

        title = title?.Trim() ?? string.Empty;
        subtitle = subtitle?.Trim() ?? string.Empty;
        instructorRole = instructorRole?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;
        var cleanOutcomes = (outcomes ?? Array.Empty<string>()).Select(outcome => outcome?.Trim() ?? string.Empty).Where(outcome => outcome.Length > 0).ToList();

        if (!IsText(title, MaxCourseTitleLength) || !IsText(subtitle, MaxCourseSubtitleLength) || Categories.All(category => category.Id != categoryId)
            || !Enum.IsDefined(level) || !IsText(instructorRole, MaxInstructorRoleLength) || !IsText(description, MaxCourseDescriptionLength)
            || cleanOutcomes.Count > MaxOutcomes || cleanOutcomes.Any(outcome => outcome.Length > MaxOutcomeLength))
            return LearningResult.Invalid;

        if (existing is null)
        {
            if (courses.Count >= MaxCourses || nextCourseId >= NextCourseIdLimit)
                return LearningResult.Invalid;

            saved = new LearningCourse(nextCourseId++, title, subtitle, categoryId, level, Clip(account.Name, ShowcaseIdentity.MaxNameLength), instructorRole, description, cleanOutcomes,
                Array.Empty<LearningModule>(), IsPopular: false)
            {
                InstructorId = account.Id,
                IsPublished = false,
            };
            courses.Add(saved);

            if (!FitsSnapshot())
            {
                courses.Remove(saved);
                saved = null;

                return LearningResult.Invalid;
            }

            Changed?.Invoke();

            return LearningResult.Ok;
        }

        if (existing.Title == title && existing.Subtitle == subtitle && existing.CategoryId == categoryId && existing.Level == level
            && existing.InstructorRole == instructorRole && existing.Description == description && existing.Outcomes.SequenceEqual(cleanOutcomes))
        {
            saved = existing;

            return LearningResult.NoChange;
        }

        saved = existing with { Title = title, Subtitle = subtitle, CategoryId = categoryId, Level = level, InstructorRole = instructorRole, Description = description, Outcomes = cleanOutcomes };
        ReplaceCourse(saved, notify: false);

        if (!FitsSnapshot())
        {
            ReplaceCourse(existing, notify: false);
            saved = null;

            return LearningResult.Invalid;
        }

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>
    /// Creates a lesson (<paramref name="lessonId" /> 0, at the end of the module) or edits one, in one of the instructor's
    /// courses. <paramref name="html" /> is the editor's rich text (rendered only through the allowlist); a different
    /// <paramref name="moduleId" /> moves the lesson to the end of that module. Turning a quiz into another kind removes
    /// its questions and learners' quiz results. A quiz is published only with questions, and a course that is published
    /// or has learners keeps at least one published lesson.
    /// </summary>
    public LearningResult SaveLesson(int courseId, int lessonId, int moduleId, string title, LearningLessonKind kind, int minutes, string html, bool publish, out LearningLesson? saved)
    {
        saved = null;

        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.Modules.All(module => module.Id != moduleId))
            return LearningResult.NotFound;

        var existing = lessonId == 0 ? null : course.Lessons.FirstOrDefault(lesson => lesson.Id == lessonId);

        if (lessonId != 0 && existing is null)
            return LearningResult.NotFound;

        title = title?.Trim() ?? string.Empty;

        if (!IsText(title, MaxLessonTitleLength) || !Enum.IsDefined(kind) || minutes is < 1 or > MaxLessonMinutes || !IsText(html, MaxLessonHtmlLength))
            return LearningResult.Invalid;

        var questions = kind == LearningLessonKind.Quiz ? existing?.Questions ?? Array.Empty<LearningQuestion>() : Array.Empty<LearningQuestion>();

        // A quiz needs questions before learners see it; and a course in the catalog or with learners keeps a published lesson.
        if ((publish && kind == LearningLessonKind.Quiz && questions.Count == 0) || (!publish && existing is { IsPublished: true } && IsLastPublished(course, existing.Id)))
            return LearningResult.WrongState;

        if (existing is null)
        {
            if (course.Lessons.Count >= MaxLessonsPerCourse || nextLessonId >= NextLessonIdLimit)
                return LearningResult.Invalid;

            var created = new LearningLesson(nextLessonId++, title, kind, minutes, string.Empty) { Html = html, IsPublished = publish };
            saved = created;
            ReplaceCourse(course with { Modules = course.Modules.Select(module => module.Id == moduleId ? module with { Lessons = module.Lessons.Append(created).ToList() } : module).ToList() }, notify: false);

            if (!FitsSnapshot())
            {
                ReplaceCourse(course, notify: false);
                saved = null;

                return LearningResult.Invalid;
            }

            Changed?.Invoke();

            return LearningResult.Ok;
        }

        var currentModule = course.Modules.First(module => module.Lessons.Any(lesson => lesson.Id == lessonId));

        if (existing.Title == title && existing.Kind == kind && existing.Minutes == minutes && (existing.Html ?? LearningSeedData.SeedHtml(existing.Id)) == html
            && existing.IsPublished == publish && currentModule.Id == moduleId)
        {
            saved = existing;

            return LearningResult.NoChange;
        }

        saved = existing with { Title = title, Kind = kind, Minutes = minutes, Body = string.Empty, Html = html, IsPublished = publish, Questions = questions };
        var updated = saved;

        var modules = course.Modules.Select(module =>
        {
            if (module.Id == currentModule.Id && module.Id == moduleId)
                return module with { Lessons = module.Lessons.Select(lesson => lesson.Id == lessonId ? updated : lesson).ToList() };

            if (module.Id == currentModule.Id)
                return module with { Lessons = module.Lessons.Where(lesson => lesson.Id != lessonId).ToList() };

            return module.Id == moduleId ? module with { Lessons = module.Lessons.Append(updated).ToList() } : module;
        }).ToList();

        ReplaceCourse(course with { Modules = modules }, notify: false);

        if (!FitsSnapshot())
        {
            ReplaceCourse(course, notify: false);
            saved = null;

            return LearningResult.Invalid;
        }

        if (existing.Kind == LearningLessonKind.Quiz && kind != LearningLessonKind.Quiz)
            attempts.RemoveAll(attempt => attempt.LessonId == lessonId);

        if (existing.IsPublished && !publish)
            IssueCertificatesFor(courseId);

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>
    /// Replaces the questions of a quiz lesson in one of the instructor's courses (in order; ID 0 adds a question). Learners'
    /// results for the quiz are removed when it changes, because they answered other questions; lessons they completed stay
    /// complete. A published quiz keeps at least one question. Returns how many results were removed.
    /// </summary>
    public LearningResult SaveQuiz(int courseId, int lessonId, IReadOnlyList<LearningQuestion> questions, out int clearedResults)
    {
        clearedResults = 0;

        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.Lessons.FirstOrDefault(lesson => lesson.Id == lessonId) is not { Kind: LearningLessonKind.Quiz } lesson)
            return LearningResult.NotFound;

        if (questions is null || questions.Count > MaxQuestionsPerQuiz)
            return LearningResult.Invalid;

        var cleaned = new List<LearningQuestion>(questions.Count);
        var newCount = questions.Count(question => question?.Id == 0);

        foreach (var question in questions)
        {
            if (question is null || (question.Id != 0 && lesson.Questions.All(existing => existing.Id != question.Id)))
                return LearningResult.Invalid;

            var prompt = question.Prompt?.Trim() ?? string.Empty;
            var options = (question.Options ?? Array.Empty<string>()).Select(option => option?.Trim() ?? string.Empty).ToList();
            var explanation = question.Explanation?.Trim() ?? string.Empty;

            if (!IsText(prompt, MaxPromptLength) || options.Count is < MinOptions or > MaxOptions || options.Any(option => !IsText(option, MaxOptionLength))
                || options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Count || question.CorrectIndex < 0 || question.CorrectIndex >= options.Count
                || explanation.Length > MaxExplanationLength)
                return LearningResult.Invalid;

            cleaned.Add(question with { Prompt = prompt, Options = options, Explanation = explanation });
        }

        if (cleaned.Select(question => question.Id).Where(id => id != 0).Distinct().Count() != cleaned.Count - newCount)
            return LearningResult.Invalid;

        if (lesson.IsPublished && cleaned.Count == 0)
            return LearningResult.WrongState;

        var unchanged = cleaned.Count == lesson.Questions.Count && cleaned.Zip(lesson.Questions).All(pair =>
            pair.First.Id == pair.Second.Id && pair.First.Prompt == pair.Second.Prompt && pair.First.Options.SequenceEqual(pair.Second.Options)
            && pair.First.CorrectIndex == pair.Second.CorrectIndex && pair.First.Explanation == pair.Second.Explanation);

        if (unchanged)
            return LearningResult.NoChange;

        if (nextQuestionId + newCount >= NextQuestionIdLimit)
            return LearningResult.Invalid;

        var numbered = cleaned.Select(question => question.Id == 0 ? question with { Id = nextQuestionId++ } : question).ToList();

        ReplaceCourse(course with
        {
            Modules = course.Modules.Select(module => module with { Lessons = module.Lessons.Select(item => item.Id == lessonId ? item with { Questions = numbered } : item).ToList() }).ToList(),
        }, notify: false);

        if (!FitsSnapshot())
        {
            ReplaceCourse(course, notify: false);

            return LearningResult.Invalid;
        }

        clearedResults = attempts.RemoveAll(attempt => attempt.LessonId == lessonId);
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Writes or edits the instructor's public reply to a review of one of their courses (plain text).</summary>
    public LearningResult SaveReply(int reviewId, string text)
    {
        if (OwnedReview(reviewId, out var index) is { } refusal)
            return refusal;

        text = text?.Trim() ?? string.Empty;

        if (!IsText(text, MaxReviewLength))
            return LearningResult.Invalid;

        if (reviews[index].InstructorReply == text)
            return LearningResult.NoChange;

        var before = reviews[index];
        reviews[index] = before with { InstructorReply = text, RepliedOn = Clock.Now };

        if (!FitsSnapshot())
        {
            reviews[index] = before;

            return LearningResult.Invalid;
        }

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>
    /// Writes or edits the signed-in learner's review of a course they're enrolled in: 1–5 stars and up to
    /// <see cref="MaxReviewLength" /> characters. One review per course; an edit keeps the instructor's reply (and a hide).
    /// </summary>
    public LearningResult SaveReview(int courseId, int rating, string body)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (StoredCourse(courseId) is null)
            return LearningResult.NotFound;

        if (!IsEnrolled(courseId))
            return LearningResult.NotEnrolled;

        body = body?.Trim() ?? string.Empty;

        if (rating is < 1 or > 5 || !IsText(body, MaxReviewLength))
            return LearningResult.Invalid;

        var index = reviews.FindIndex(review => review.CourseId == courseId && review.AccountId == account.Id);

        if (index >= 0)
        {
            if (reviews[index].Rating == rating && reviews[index].Body == body)
                return LearningResult.NoChange;

            var before = reviews[index];
            reviews[index] = before with { Rating = rating, Body = body, Author = ReviewerName(account.Name), EditedOn = Clock.Now };

            if (!FitsSnapshot())
            {
                reviews[index] = before;

                return LearningResult.Invalid;
            }
        }
        else
        {
            if (reviews.Count >= MaxReviews || nextReviewId >= NextReviewIdLimit)
                return LearningResult.Invalid;

            var review = new LearningReview(nextReviewId++, courseId, ReviewerName(account.Name), rating, body, Clock.Now) { AccountId = account.Id };
            reviews.Add(review);

            if (!FitsSnapshot())
            {
                reviews.Remove(review);

                return LearningResult.Invalid;
            }
        }

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Publishes one of the instructor's courses (it needs a published lesson) or takes it out of the catalog.</summary>
    public LearningResult SetCoursePublished(int courseId, bool isPublished)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.IsPublished == isPublished)
            return LearningResult.NoChange;

        if (isPublished && PublishedLessonIds(course).Count == 0)
            return LearningResult.WrongState;

        ReplaceCourse(course with { IsPublished = isPublished });

        return LearningResult.Ok;
    }

    /// <summary>
    /// Marks a published video or reading lesson complete or not (quiz lessons complete by passing their quiz). Finishing
    /// the last lesson issues the course certificate once.
    /// </summary>
    public LearningResult SetLessonComplete(int courseId, int lessonId, bool isComplete)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole })
            return LearningResult.NotAllowed;

        if (FindCourse(courseId)?.Lessons.FirstOrDefault(lesson => lesson.Id == lessonId) is not { } lesson)
            return LearningResult.NotFound;

        if (Enrollment(courseId) is not { } enrollment)
            return LearningResult.NotEnrolled;

        // A quiz lesson's completion follows its quiz: passing completes it, and it isn't undone by hand.
        if (lesson.Kind == LearningLessonKind.Quiz)
            return LearningResult.WrongState;

        var changed = isComplete ? enrollment.CompletedLessons.Add(lessonId) : enrollment.CompletedLessons.Remove(lessonId);

        if (!changed)
            return LearningResult.NoChange;

        enrollment.LastActivity = Clock.Now;
        IssueCertificateIfComplete(enrollment);
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>
    /// Publishes a lesson of one of the instructor's courses (a quiz needs questions) or turns it back into a draft (a
    /// course that is published or has learners keeps at least one published lesson).
    /// </summary>
    public LearningResult SetLessonPublished(int courseId, int lessonId, bool isPublished)
    {
        if (OwnedCourse(courseId) is not { } course)
            return InstructorRefusal(courseId);

        if (course.Lessons.FirstOrDefault(lesson => lesson.Id == lessonId) is not { } lesson)
            return LearningResult.NotFound;

        if (lesson.IsPublished == isPublished)
            return LearningResult.NoChange;

        if ((isPublished && lesson.Kind == LearningLessonKind.Quiz && lesson.Questions.Count == 0) || (!isPublished && IsLastPublished(course, lessonId)))
            return LearningResult.WrongState;

        ReplaceCourse(course with
        {
            Modules = course.Modules.Select(module => module with { Lessons = module.Lessons.Select(item => item.Id == lessonId ? item with { IsPublished = isPublished } : item).ToList() }).ToList(),
        }, notify: false);

        if (!isPublished)
            IssueCertificatesFor(courseId);

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Shows a hidden review of one of the instructor's courses on the course page again.</summary>
    public LearningResult ShowReview(int reviewId)
    {
        if (OwnedReview(reviewId, out var index) is { } refusal)
            return refusal;

        if (reviews[index].HiddenReason is null)
            return LearningResult.NoChange;

        reviews[index] = reviews[index] with { HiddenReason = null };
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>
    /// Scores a quiz attempt: one chosen option per question, in order. Reaching the pass mark completes the lesson (and may
    /// finish the course). Returns the number of right answers through <paramref name="score" />.
    /// </summary>
    public LearningResult SubmitQuiz(int courseId, int lessonId, IReadOnlyList<int> answers, out int score)
    {
        score = 0;

        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (FindCourse(courseId)?.Lessons.FirstOrDefault(lesson => lesson.Id == lessonId) is not { Kind: LearningLessonKind.Quiz } lesson || lesson.Questions.Count == 0)
            return LearningResult.NotFound;

        if (Enrollment(courseId) is not { } enrollment)
            return LearningResult.NotEnrolled;

        if (answers is null || answers.Count != lesson.Questions.Count || answers.Select((answer, index) => answer < 0 || answer >= lesson.Questions[index].Options.Count).Any(bad => bad))
            return LearningResult.Invalid;

        score = lesson.Questions.Where((question, index) => answers[index] == question.CorrectIndex).Count();

        var attempt = QuizResult(lessonId);

        if (attempt is null)
        {
            attempt = new LearningQuizAttempt { AccountId = account.Id, LessonId = lessonId };
            attempts.Add(attempt);
        }

        attempt.Attempts = Math.Min(attempt.Attempts + 1, 10_000);
        attempt.LastScore = score;
        attempt.BestScore = Math.Max(attempt.BestScore, score);
        attempt.LastAnswers = answers.ToList();
        attempt.LastAt = Clock.Now;

        if (score >= lesson.PassMark && enrollment.CompletedLessons.Add(lessonId))
            IssueCertificateIfComplete(enrollment);

        enrollment.LastActivity = Clock.Now;
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Validates a stored snapshot against its own data (never the live lists) and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        LearningSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<LearningSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return false;
        }

        if (snapshot is not
            {
                Attempts: { } storedAttempts,
                Certificates: { Count: <= MaxCertificates } storedCertificates,
                Courses: { Count: > 0 and <= MaxCourses } storedCourses,
                Enrollments: { Count: <= MaxEnrollments } storedEnrollments,
                Reviews: { Count: <= MaxReviews } storedReviews,
                Threads: { Count: <= MaxThreads } storedThreads,
            }
            || snapshot.NextReviewId is < 1 or > NextReviewIdLimit
            || snapshot.NextThreadId is < 1 or > NextThreadIdLimit
            || snapshot.NextAnswerId is < 1 or > NextAnswerIdLimit
            || snapshot.NextCourseId is < FirstAuthoredCourseId or > NextCourseIdLimit
            || snapshot.NextModuleId is < FirstAuthoredModuleId or > NextModuleIdLimit
            || snapshot.NextLessonId is < FirstAuthoredLessonId or > NextLessonIdLimit
            || snapshot.NextQuestionId is < FirstAuthoredQuestionId or > NextQuestionIdLimit)
            return false;

        if (!ValidCourses(storedCourses, snapshot, storedEnrollments))
            return false;

        var courseById = storedCourses.ToDictionary(course => course.Id);
        var lessons = storedCourses.SelectMany(course => course.Lessons.Select(lesson => (course.Id, Lesson: lesson))).ToDictionary(pair => pair.Lesson.Id);

        if (storedEnrollments.Any(enrollment => enrollment is null || !IsAccountId(enrollment.AccountId) || !courseById.TryGetValue(enrollment.CourseId, out var course)
                || !IsPlausible(enrollment.EnrolledOn) || !IsPlausible(enrollment.LastActivity) || enrollment.CompletedLessons is null
                || !IsText(enrollment.LearnerName, ShowcaseIdentity.MaxNameLength)
                || enrollment.CompletedLessons.Any(id => course.Lessons.All(lesson => lesson.Id != id)))
            || !Unique(storedEnrollments.Select(enrollment => (enrollment.AccountId, enrollment.CourseId))))
            return false;

        if (storedAttempts.Count > storedEnrollments.Count * MaxLessonsPerCourse
            || storedAttempts.Any(attempt => attempt is null || !IsAccountId(attempt.AccountId) || !lessons.TryGetValue(attempt.LessonId, out var entry)
                || entry.Lesson.Kind != LearningLessonKind.Quiz || attempt.Attempts is < 1 or > 10_000
                || attempt.BestScore < 0 || attempt.BestScore > entry.Lesson.Questions.Count || attempt.LastScore < 0 || attempt.LastScore > attempt.BestScore
                || attempt.LastAnswers is null || attempt.LastAnswers.Count != entry.Lesson.Questions.Count
                || attempt.LastAnswers.Where((answer, index) => answer < 0 || answer >= entry.Lesson.Questions[index].Options.Count).Any() || !IsPlausible(attempt.LastAt)
                // Results exist only while enrolled (unenrolling removes them), and a passed quiz has completed its lesson.
                || storedEnrollments.FirstOrDefault(enrollment => enrollment.AccountId == attempt.AccountId && enrollment.CourseId == entry.Id) is not { } owner
                || (attempt.BestScore >= entry.Lesson.PassMark && !owner.CompletedLessons.Contains(attempt.LessonId)))
            || !Unique(storedAttempts.Select(attempt => (attempt.AccountId, attempt.LessonId))))
            return false;

        if (storedReviews.Any(review => review is null || review.Id < 1 || review.Id >= snapshot.NextReviewId || !courseById.ContainsKey(review.CourseId)
                || !IsText(review.Author, ShowcaseIdentity.MaxNameLength) || review.Rating is < 1 or > 5 || !IsText(review.Body, MaxReviewLength) || !IsPlausible(review.PostedOn)
                || (review.EditedOn is { } edited && !IsPlausible(edited)) || (review.InstructorReply is not null && !IsText(review.InstructorReply, MaxReviewLength))
                || (review.RepliedOn is { } replied && !IsPlausible(replied)) || (review.HiddenReason is not null && !LearningReviewHideReasons.All.Contains(review.HiddenReason))
                || (review.AccountId is not null && !IsAccountId(review.AccountId)))
            || !Unique(storedReviews.Select(review => review.Id))
            || !Unique(storedReviews.Where(review => review.AccountId is not null).Select(review => (review.AccountId, review.CourseId))))
            return false;

        if (storedThreads.Any(thread => thread is null || thread.Id < 1 || thread.Id >= snapshot.NextThreadId || !lessons.TryGetValue(thread.LessonId, out var entry)
                || entry.Id != thread.CourseId || (thread.AccountId is not null && !IsAccountId(thread.AccountId)) || !IsText(thread.Author, ShowcaseIdentity.MaxNameLength)
                || !IsText(thread.Text, MaxQuestionLength) || !IsPlausible(thread.AskedAt) || thread.Answers is not { Count: <= MaxAnswersPerThread }
                || thread.Answers.Any(answer => answer is null || answer.Id < 1 || answer.Id >= snapshot.NextAnswerId || !IsText(answer.Author, ShowcaseIdentity.MaxNameLength)
                    || !IsText(answer.Text, MaxQuestionLength) || !IsPlausible(answer.At) || (answer.AccountId is not null && !IsAccountId(answer.AccountId))))
            || !Unique(storedThreads.Select(thread => thread.Id))
            || !Unique(storedThreads.SelectMany(thread => thread.Answers).Select(answer => answer.Id)))
            return false;

        if (storedCertificates.Any(certificate => certificate is null || !CodePattern().IsMatch(certificate.Code ?? string.Empty) || !courseById.ContainsKey(certificate.CourseId)
                || !IsText(certificate.LearnerName, ShowcaseIdentity.MaxNameLength) || !IsPlausible(certificate.IssuedOn)
                || (certificate.AccountId is not null && !IsAccountId(certificate.AccountId)))
            || !Unique(storedCertificates.Select(certificate => certificate.Code))
            || !Unique(storedCertificates.Where(certificate => certificate.AccountId is not null).Select(certificate => (certificate.AccountId, certificate.CourseId))))
            return false;

        courses.Clear();
        courses.AddRange(storedCourses);
        enrollments.Clear();
        enrollments.AddRange(storedEnrollments);
        attempts.Clear();
        attempts.AddRange(storedAttempts);
        reviews.Clear();
        reviews.AddRange(storedReviews);
        threads.Clear();
        threads.AddRange(storedThreads);
        certificates.Clear();
        certificates.AddRange(storedCertificates);
        nextAnswerId = snapshot.NextAnswerId;
        nextCourseId = snapshot.NextCourseId;
        nextLessonId = snapshot.NextLessonId;
        nextModuleId = snapshot.NextModuleId;
        nextQuestionId = snapshot.NextQuestionId;
        nextReviewId = snapshot.NextReviewId;
        nextThreadId = snapshot.NextThreadId;

        return true;
    }

    /// <summary>
    /// Unenrolls the signed-in learner from a course: its progress and quiz results are removed. A certificate already
    /// issued, and the learner's review and questions, stay.
    /// </summary>
    public LearningResult Unenroll(int courseId)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (Enrollment(courseId) is not { } enrollment || StoredCourse(courseId) is not { } course)
            return LearningResult.NotEnrolled;

        enrollments.Remove(enrollment);
        attempts.RemoveAll(attempt => attempt.AccountId == account.Id && course.Lessons.Any(lesson => lesson.Id == attempt.LessonId));
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Returns the certificate with a code (any learner's), or <see langword="null" />: the public verify page.</summary>
    public LearningCertificate? VerifyCertificate(string? code)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", string.Empty, StringComparison.Ordinal);

        return normalized.Length is < 6 or > 20 ? null : certificates.FirstOrDefault(certificate => certificate.Code == normalized);
    }

    #endregion

    #region Private Methods

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    [GeneratedRegex(@"^QP-[A-Z0-9]{4}-[A-Z0-9]{4}$")]
    private static partial Regex CodePattern();

    private static int CompletedPublished(LearningCourse course, LearningEnrollment enrollment) =>
        course.Lessons.Count(lesson => lesson.IsPublished && enrollment.CompletedLessons.Contains(lesson.Id));

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (LearningSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || LearningSeedData.IsSeededLearner(accountId)
            || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsCompleteFor(LearningCourse course, LearningEnrollment enrollment) =>
        PublishedLessonIds(course) is { Count: > 0 } published && published.All(enrollment.CompletedLessons.Contains);

    private static bool IsPlausible(DateTime value) => value >= LearningSeedData.Now.AddYears(-3) && value <= LearningSeedData.Now.AddYears(2);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    /// <summary>A learner's view of a course: published lessons only, and only modules that have some.</summary>
    private static LearningCourse LearnerView(LearningCourse course) =>
        course.Modules.All(module => module.Lessons.All(lesson => lesson.IsPublished))
            ? course
            : course with
            {
                Modules = course.Modules
                    .Select(module => module with { Lessons = module.Lessons.Where(lesson => lesson.IsPublished).ToList() })
                    .Where(module => module.Lessons.Count > 0)
                    .ToList(),
            };

    private static List<int> PublishedLessonIds(LearningCourse course) => course.Lessons.Where(lesson => lesson.IsPublished).Select(lesson => lesson.Id).ToList();

    /// <summary>Returns a reviewer's public name: their first name and last initial, like "Jordan A.".</summary>
    private static string ReviewerName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // The initial is a whole character (a Rune), so a name outside the basic plane isn't cut in half.
        return parts.Length < 2 ? name : $"{parts[0]} {Rune.ToUpperInvariant(parts[^1].EnumerateRunes().First())}.";
    }

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    /// <summary>
    /// Checks stored courses: IDs and counters, text limits, catalog references, quiz questions, and lesson bodies. A lesson
    /// may carry Markdown only as seeded for its ID (Markdown renders raw HTML); otherwise its body is editor HTML, which is
    /// only rendered through the allowlist. A course that is published or has learners has a published lesson, and a
    /// published quiz has questions.
    /// </summary>
    private bool ValidCourses(List<LearningCourse> storedCourses, LearningSnapshot snapshot, List<LearningEnrollment> storedEnrollments)
    {
        if (storedCourses.Any(course => course is null || course.Modules is null || course.Outcomes is null)
            || !Unique(storedCourses.Select(course => course.Id)))
            return false;

        foreach (var course in storedCourses)
        {
            var isSeeded = course.Id is >= 201 and < FirstAuthoredCourseId;

            if ((!isSeeded && (course.Id < FirstAuthoredCourseId || course.Id >= snapshot.NextCourseId))
                || !IsText(course.Title, MaxCourseTitleLength) || !IsText(course.Subtitle, MaxCourseSubtitleLength) || Categories.All(category => category.Id != course.CategoryId)
                || !Enum.IsDefined(course.Level) || !IsText(course.Instructor, ShowcaseIdentity.MaxNameLength) || !IsText(course.InstructorRole, MaxInstructorRoleLength)
                || !IsText(course.Description, MaxCourseDescriptionLength) || course.Outcomes.Count > MaxOutcomes || course.Outcomes.Any(outcome => !IsText(outcome, MaxOutcomeLength))
                || (course.InstructorId is not null && course.InstructorId != LearningSeedData.SeededInstructorId)
                || course.Modules.Count > MaxModulesPerCourse || course.Modules.Any(module => module is null || module.Lessons is null || !IsText(module.Title, MaxModuleTitleLength))
                || course.Lessons.Count > MaxLessonsPerCourse)
                return false;

            if (course.Lessons.Any(lesson => lesson is null))
                return false;

            foreach (var lesson in course.Lessons)
            {
                var seededBody = LearningSeedData.SeedBody(lesson.Id);
                var bodyOk = lesson.Body is not null && (lesson.Body.Length == 0 ? lesson.Html is not null : lesson.Body == seededBody);

                if ((seededBody is null && (lesson.Id < FirstAuthoredLessonId || lesson.Id >= snapshot.NextLessonId))
                    || !IsText(lesson.Title, MaxLessonTitleLength) || !Enum.IsDefined(lesson.Kind) || lesson.Minutes is < 1 or > MaxLessonMinutes
                    || !bodyOk || (lesson.Html is not null && !IsText(lesson.Html, MaxLessonHtmlLength))
                    || lesson.Questions is null || (lesson.Kind != LearningLessonKind.Quiz && lesson.Questions.Count > 0) || lesson.Questions.Count > MaxQuestionsPerQuiz
                    || (lesson.IsPublished && lesson.Kind == LearningLessonKind.Quiz && lesson.Questions.Count == 0))
                    return false;

                foreach (var question in lesson.Questions)
                {
                    if (question is null || question.Options is null || question.Id < 1 || (question.Id >= FirstAuthoredQuestionId && question.Id >= snapshot.NextQuestionId)
                        || !IsText(question.Prompt, MaxPromptLength) || question.Options.Count is < MinOptions or > MaxOptions
                        || question.Options.Any(option => !IsText(option, MaxOptionLength) || option != option.Trim())
                        || question.Options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != question.Options.Count
                        || question.CorrectIndex < 0 || question.CorrectIndex >= question.Options.Count
                        || question.Explanation is null || question.Explanation.Length > MaxExplanationLength)
                        return false;
                }
            }

            var needsLesson = course.IsPublished || storedEnrollments.Any(enrollment => enrollment?.CourseId == course.Id);

            if (needsLesson && PublishedLessonIds(course).Count == 0)
                return false;
        }

        var modules = storedCourses.SelectMany(course => course.Modules).ToList();
        var lessons = storedCourses.SelectMany(course => course.Lessons).ToList();

        return Unique(modules.Select(module => module.Id)) && modules.All(module => module.Id is > 0 && (module.Id < FirstAuthoredModuleId || module.Id < snapshot.NextModuleId))
            && Unique(lessons.Select(lesson => lesson.Id))
            && Unique(lessons.SelectMany(lesson => lesson.Questions).Select(question => question.Id));
    }

    private LearningEnrollment? Enrollment(int courseId) =>
        Identity.Current is { RoleId: LearningSeedData.LearnerRole } account ? enrollments.FirstOrDefault(enrollment => enrollment.CourseId == courseId && enrollment.AccountId == account.Id) : null;

    /// <summary>The refusal for an instructor action on a course that isn't theirs: not signed in as an instructor, or not found.</summary>
    private LearningResult InstructorRefusal(int courseId) =>
        Identity.Current is not { RoleId: LearningSeedData.InstructorRole } ? LearningResult.NotAllowed : LearningResult.NotFound;

    /// <summary>Whether a lesson is the last published one of a course that must keep one (published, or with learners).</summary>
    private bool IsLastPublished(LearningCourse course, int lessonId) =>
        (course.IsPublished || enrollments.Any(enrollment => enrollment.CourseId == course.Id))
        && PublishedLessonIds(course) is { Count: 1 } published && published[0] == lessonId;

    /// <summary>
    /// Issues a course certificate once, when every published lesson is complete, under the learner's name (the signed-in
    /// account's current name, or the name they enrolled with) and a new random code.
    /// </summary>
    private void IssueCertificateIfComplete(LearningEnrollment enrollment)
    {
        if (StoredCourse(enrollment.CourseId) is not { } course || !IsCompleteFor(course, enrollment) || certificates.Count >= MaxCertificates
            || certificates.Any(certificate => certificate.CourseId == course.Id && certificate.AccountId == enrollment.AccountId))
            return;

        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        string code;

        do
        {
            var chars = Enumerable.Range(0, 8).Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]).ToArray();
            code = string.Create(CultureInfo.InvariantCulture, $"QP-{new string(chars, 0, 4)}-{new string(chars, 4, 4)}");
        }
        while (certificates.Any(certificate => certificate.Code == code));

        var name = Identity.Current is { } account && account.Id == enrollment.AccountId ? account.Name : enrollment.LearnerName;

        certificates.Add(new LearningCertificate(code, course.Id, Clip(name, ShowcaseIdentity.MaxNameLength), Clock.Now) { AccountId = enrollment.AccountId });
    }

    /// <summary>After a lesson is deleted or unpublished, learners who have now done every published lesson get their certificate.</summary>
    private void IssueCertificatesFor(int courseId)
    {
        foreach (var enrollment in enrollments.Where(enrollment => enrollment.CourseId == courseId).ToList())
            IssueCertificateIfComplete(enrollment);
    }

    /// <summary>Whether the snapshot, as it would be saved now (with the store's escaping), still fits under <see cref="MaxSnapshotLength" />.</summary>
    private bool FitsSnapshot()
    {
        try
        {
            return JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length <= MaxSnapshotLength;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }

    private bool Owns(LearningCourse course) =>
        Identity.Current is { RoleId: LearningSeedData.InstructorRole } account && course.InstructorId == account.Id;

    /// <summary>Returns one of the signed-in instructor's courses (every lesson), or <see langword="null" />.</summary>
    private LearningCourse? OwnedCourse(int courseId) => StoredCourse(courseId) is { } course && Owns(course) ? course : null;

    /// <summary>Finds a review of one of the instructor's courses; returns the refusal, or <see langword="null" /> with its index.</summary>
    private LearningResult? OwnedReview(int reviewId, out int index)
    {
        index = reviews.FindIndex(review => review.Id == reviewId);

        if (Identity.Current is not { RoleId: LearningSeedData.InstructorRole })
            return LearningResult.NotAllowed;

        return index < 0 || OwnedCourse(reviews[index].CourseId) is null ? LearningResult.NotFound : null;
    }

    private void ReplaceCourse(LearningCourse updated, bool notify = true)
    {
        var index = courses.FindIndex(course => course.Id == updated.Id);

        if (index >= 0)
            courses[index] = updated;

        if (notify)
            Changed?.Invoke();
    }

    private LearningCourse? StoredCourse(int courseId) => courses.FirstOrDefault(course => course.Id == courseId);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the course categories.</summary>
    public IReadOnlyList<LearningCategory> Categories { get; } = LearningSeedData.Categories();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(LearningSeedData.Now);

    /// <summary>Gets the catalog: published courses as learners see them (published lessons only).</summary>
    public IReadOnlyList<LearningCourse> Courses => courses.Where(course => course.IsPublished).Select(LearnerView).ToList();

    /// <summary>Gets the courses the signed-in learner is enrolled in, the most recently active first.</summary>
    public IReadOnlyList<LearningCourse> EnrolledCourses =>
        Identity.Current is { RoleId: LearningSeedData.LearnerRole } account
            ? enrollments.Where(enrollment => enrollment.AccountId == account.Id).OrderByDescending(enrollment => enrollment.LastActivity)
                .Select(enrollment => FindCourse(enrollment.CourseId)).OfType<LearningCourse>().ToList()
            : Array.Empty<LearningCourse>();

    /// <summary>Gets the catalog filters.</summary>
    public LearningFilter Filter { get; } = new();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets the questions on the instructor's courses, newest first.</summary>
    public IReadOnlyList<LearningQuestionThread> InstructorQuestions =>
        threads.Where(thread => OwnedCourse(thread.CourseId) is not null).OrderByDescending(thread => thread.AskedAt).ToList();

    /// <summary>Gets the reviews of the instructor's courses, hidden ones included, newest first.</summary>
    public IReadOnlyList<LearningReview> InstructorReviews =>
        reviews.Where(review => OwnedCourse(review.CourseId) is not null).OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).ToList();

    /// <summary>Gets whether the signed-in account is an instructor.</summary>
    public bool IsInstructor => Identity.Current?.RoleId == LearningSeedData.InstructorRole;

    /// <summary>Gets whether the signed-in account is a learner.</summary>
    public bool IsLearner => Identity.Current?.RoleId == LearningSeedData.LearnerRole;

    /// <summary>Gets the signed-in learner's certificates, the newest first.</summary>
    public IReadOnlyList<LearningCertificate> MyCertificates =>
        Identity.Current is { } account ? certificates.Where(certificate => certificate.AccountId == account.Id).OrderByDescending(certificate => certificate.IssuedOn).ToList() : Array.Empty<LearningCertificate>();

    /// <summary>Gets the signed-in instructor's courses (drafts included), published first, then by title.</summary>
    public IReadOnlyList<LearningCourse> MyCourses =>
        courses.Where(Owns).OrderByDescending(course => course.IsPublished).ThenBy(course => course.Title, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Gets the learners on the instructor's courses: every enrollment with its progress over the published lessons, the most recently active first.</summary>
    public IReadOnlyList<LearningRosterRow> Roster =>
        enrollments
            .Select(enrollment => (enrollment, course: OwnedCourse(enrollment.CourseId)))
            .Where(pair => pair.course is not null)
            .Select(pair => new LearningRosterRow(pair.enrollment, pair.course!, CompletedPublished(pair.course!, pair.enrollment), PublishedLessonIds(pair.course!).Count,
                certificates.FirstOrDefault(certificate => certificate.CourseId == pair.course!.Id && certificate.AccountId == pair.enrollment.AccountId)))
            .OrderByDescending(row => row.Enrollment.LastActivity)
            .ToList();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "learning";

    /// <summary>Gets the snapshot format version (2: courses are part of the snapshot).</summary>
    public int SnapshotVersion => 2;

    /// <summary>Gets how many questions on the instructor's courses have no answer from the instructor yet.</summary>
    public int UnansweredCount => InstructorQuestions.Count(thread => thread.Answers.All(answer => !answer.IsInstructor));

    #endregion

    #region Events

    /// <summary>Raised when anything in the state changes, including sign-in.</summary>
    public event Action? Changed;

    #endregion

    #region Nested Types

    /// <summary>The learning site's part of the tab snapshot.</summary>
    private sealed class LearningSnapshot
    {
        #region Properties, Indexers

        public List<LearningQuizAttempt>? Attempts { get; set; }

        public List<LearningCertificate>? Certificates { get; set; }

        public List<LearningCourse>? Courses { get; set; }

        public List<LearningEnrollment>? Enrollments { get; set; }

        public int NextAnswerId { get; set; }

        public int NextCourseId { get; set; }

        public int NextLessonId { get; set; }

        public int NextModuleId { get; set; }

        public int NextQuestionId { get; set; }

        public int NextReviewId { get; set; }

        public int NextThreadId { get; set; }

        public List<LearningReview>? Reviews { get; set; }

        public List<LearningQuestionThread>? Threads { get; set; }

        #endregion
    }

    #endregion
}
