using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

/// <summary>
/// State of the "Quillpath Academy" learning site for the current browser tab: enrollments and lesson progress, quiz
/// results, reviews, lesson questions and answers, certificates, and the demo sign-in. It is kept in the tab's
/// <c>sessionStorage</c> through the showcase foundation, and its clock moves on from the seed time (D-029); nothing is sent.
/// </summary>
public sealed partial class LearningState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>The longest question or answer in a lesson's Q&amp;A.</summary>
    public const int MaxQuestionLength = 1000;

    /// <summary>The longest review.</summary>
    public const int MaxReviewLength = 1000;

    private const int MaxAnswersPerThread = 50;

    private const int MaxCertificates = 500;

    private const int MaxEnrollments = 1000;

    private const int MaxReviews = 2000;

    private const int MaxThreads = 1000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<LearningQuizAttempt> attempts;

    private readonly List<LearningCertificate> certificates = LearningSeedData.Certificates();

    private readonly List<LearningEnrollment> enrollments;

    private readonly List<LearningReview> reviews;

    private readonly List<LearningQuestionThread> threads;

    private int nextReviewId;

    private int nextThreadId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public LearningState()
    {
        enrollments = LearningSeedData.Enrollments(Courses);
        attempts = LearningSeedData.QuizAttempts(Courses);
        reviews = LearningSeedData.Reviews(Courses);
        threads = LearningSeedData.Questions(Courses);
        nextReviewId = reviews.Max(review => review.Id) + 1;
        nextThreadId = threads.Max(thread => thread.Id) + 1;
        Identity = new ShowcaseIdentity(LearningSeedData.Roles(), LearningSeedData.DemoAccounts());
        Identity.Changed += () => Changed?.Invoke();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Asks a question about a lesson of a course the learner is enrolled in (plain text, up to <see cref="MaxQuestionLength" /> characters).</summary>
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

        if (!IsText(text, MaxQuestionLength) || threads.Count >= MaxThreads)
            return LearningResult.Invalid;

        thread = new LearningQuestionThread { Id = nextThreadId++, CourseId = courseId, LessonId = lessonId, AccountId = account.Id, Author = account.Name, Text = text, AskedAt = Clock.Now };
        threads.Add(thread);
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Returns the average rating of a course, or 0 without reviews.</summary>
    public double AverageRating(int courseId)
    {
        var ratings = reviews.Where(review => review.CourseId == courseId).Select(review => review.Rating).ToList();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    /// <summary>Returns the snapshot: enrollments, quiz results, reviews, questions, certificates, and the ID counters.</summary>
    public object CaptureData() => new LearningSnapshot
    {
        Attempts = attempts.ToList(),
        Certificates = certificates.ToList(),
        Enrollments = enrollments.ToList(),
        NextReviewId = nextReviewId,
        NextThreadId = nextThreadId,
        Reviews = reviews.ToList(),
        Threads = threads.ToList(),
    };

    /// <summary>Returns the signed-in learner's certificate for a course, if issued.</summary>
    public LearningCertificate? CertificateFor(int courseId) =>
        Identity.Current is { } account ? certificates.FirstOrDefault(certificate => certificate.CourseId == courseId && certificate.AccountId == account.Id) : null;

    /// <summary>Returns how many lessons of a course the signed-in learner completed.</summary>
    public int CompletedCount(int courseId) => Enrollment(courseId)?.CompletedLessons.Count ?? 0;

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

    /// <summary>Enrolls the signed-in learner in a course; enrolling twice changes nothing.</summary>
    public LearningResult Enroll(int courseId)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (FindCourse(courseId) is null)
            return LearningResult.NotFound;

        if (IsEnrolled(courseId))
            return LearningResult.NoChange;

        if (enrollments.Count >= MaxEnrollments)
            return LearningResult.Invalid;

        enrollments.Add(new LearningEnrollment { AccountId = account.Id, CourseId = courseId, EnrolledOn = Clock.Now, LastActivity = Clock.Now });
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Finds a category by its ID; unknown IDs fall back to the first category.</summary>
    public LearningCategory FindCategory(string categoryId) => Categories.FirstOrDefault(category => category.Id == categoryId) ?? Categories[0];

    /// <summary>Finds a course by its ID, or returns <see langword="null" />.</summary>
    public LearningCourse? FindCourse(int courseId) => Courses.FirstOrDefault(course => course.Id == courseId);

    /// <summary>Returns whether the signed-in learner finished every lesson of a course.</summary>
    public bool IsComplete(int courseId) => FindCourse(courseId) is { } course && CompletedCount(courseId) == course.Lessons.Count;

    /// <summary>Returns whether the signed-in learner is enrolled in a course.</summary>
    public bool IsEnrolled(int courseId) => Enrollment(courseId) is not null;

    /// <summary>Returns whether the signed-in learner completed a lesson.</summary>
    public bool IsLessonComplete(int courseId, int lessonId) => Enrollment(courseId)?.CompletedLessons.Contains(lessonId) ?? false;

    /// <summary>Returns when the signed-in learner was last active in a course, or <see langword="null" /> when not enrolled.</summary>
    public DateTime? LastActivity(int courseId) => Enrollment(courseId)?.LastActivity;

    /// <summary>Returns the first lesson not yet complete, or the first lesson when everything is done.</summary>
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

    /// <summary>Returns the share of lessons complete, from 0 to 100.</summary>
    public int ProgressPercent(int courseId) =>
        FindCourse(courseId) is { } course && course.Lessons.Count > 0 ? CompletedCount(courseId) * 100 / course.Lessons.Count : 0;

    /// <summary>Returns the questions about a lesson, newest first.</summary>
    public IReadOnlyList<LearningQuestionThread> QuestionsFor(int lessonId) => threads.Where(thread => thread.LessonId == lessonId).OrderByDescending(thread => thread.AskedAt).ToList();

    /// <summary>Returns the signed-in learner's results for a quiz lesson, if they tried it.</summary>
    public LearningQuizAttempt? QuizResult(int lessonId) =>
        Identity.Current is { } account ? attempts.FirstOrDefault(attempt => attempt.LessonId == lessonId && attempt.AccountId == account.Id) : null;

    /// <summary>Returns the signed-in learner's review of a course, if any.</summary>
    public LearningReview? ReviewBy(int courseId) =>
        Identity.Current is { } account ? reviews.FirstOrDefault(review => review.CourseId == courseId && review.AccountId == account.Id) : null;

    /// <summary>Returns the reviews of a course, newest first.</summary>
    public IReadOnlyList<LearningReview> ReviewsFor(int courseId) =>
        reviews.Where(review => review.CourseId == courseId).OrderByDescending(review => review.PostedOn).ThenByDescending(review => review.Id).ToList();

    /// <summary>
    /// Writes or edits the signed-in learner's review of a course they're enrolled in: 1–5 stars and up to
    /// <see cref="MaxReviewLength" /> characters. One review per course; an edit keeps the instructor's reply.
    /// </summary>
    public LearningResult SaveReview(int courseId, int rating, string body)
    {
        if (Identity.Current is not { RoleId: LearningSeedData.LearnerRole } account)
            return LearningResult.NotAllowed;

        if (FindCourse(courseId) is null)
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

            reviews[index] = reviews[index] with { Rating = rating, Body = body, Author = ReviewerName(account.Name), EditedOn = Clock.Now };
        }
        else
        {
            if (reviews.Count >= MaxReviews)
                return LearningResult.Invalid;

            reviews.Add(new LearningReview(nextReviewId++, courseId, ReviewerName(account.Name), rating, body, Clock.Now) { AccountId = account.Id });
        }

        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>
    /// Marks a video or reading lesson complete or not (quiz lessons complete by passing their quiz). Finishing the last
    /// lesson issues the course certificate once.
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
        IssueCertificateIfComplete(courseId);
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

        if (FindCourse(courseId)?.Lessons.FirstOrDefault(lesson => lesson.Id == lessonId) is not { Kind: LearningLessonKind.Quiz } lesson)
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
            IssueCertificateIfComplete(courseId);

        enrollment.LastActivity = Clock.Now;
        Changed?.Invoke();

        return LearningResult.Ok;
    }

    /// <summary>Validates a stored snapshot and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        LearningSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<LearningSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return false;
        }

        if (snapshot is not
            {
                Attempts: { } storedAttempts,
                Certificates: { Count: <= MaxCertificates } storedCertificates,
                Enrollments: { Count: <= MaxEnrollments } storedEnrollments,
                Reviews: { Count: <= MaxReviews } storedReviews,
                Threads: { Count: <= MaxThreads } storedThreads,
            }
            || snapshot.NextReviewId is < 1 or > 1_000_000 - MaxReviews
            || snapshot.NextThreadId is < 1 or > 1_000_000 - MaxThreads)
            return false;

        var lessons = Courses.SelectMany(course => course.Lessons.Select(lesson => (course.Id, Lesson: lesson))).ToDictionary(pair => pair.Lesson.Id);

        if (storedEnrollments.Any(enrollment => enrollment is null || !IsAccountId(enrollment.AccountId) || FindCourse(enrollment.CourseId) is not { } course
                || !IsPlausible(enrollment.EnrolledOn) || !IsPlausible(enrollment.LastActivity) || enrollment.CompletedLessons is null
                || enrollment.CompletedLessons.Any(id => course.Lessons.All(lesson => lesson.Id != id)))
            || !Unique(storedEnrollments.Select(enrollment => (enrollment.AccountId, enrollment.CourseId))))
            return false;

        if (storedAttempts.Count > storedEnrollments.Count * 12
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

        if (storedReviews.Any(review => review is null || review.Id < 1 || review.Id >= snapshot.NextReviewId || FindCourse(review.CourseId) is null
                || !IsText(review.Author, ShowcaseIdentity.MaxNameLength) || review.Rating is < 1 or > 5 || !IsText(review.Body, MaxReviewLength) || !IsPlausible(review.PostedOn)
                || (review.EditedOn is { } edited && !IsPlausible(edited)) || (review.InstructorReply is not null && !IsText(review.InstructorReply, MaxReviewLength))
                || (review.AccountId is not null && !IsAccountId(review.AccountId)))
            || !Unique(storedReviews.Select(review => review.Id))
            || !Unique(storedReviews.Where(review => review.AccountId is not null).Select(review => (review.AccountId, review.CourseId))))
            return false;

        if (storedThreads.Any(thread => thread is null || thread.Id < 1 || thread.Id >= snapshot.NextThreadId || !lessons.TryGetValue(thread.LessonId, out var entry)
                || entry.Id != thread.CourseId || (thread.AccountId is not null && !IsAccountId(thread.AccountId)) || !IsText(thread.Author, ShowcaseIdentity.MaxNameLength)
                || !IsText(thread.Text, MaxQuestionLength) || !IsPlausible(thread.AskedAt) || thread.Answers is not { Count: <= MaxAnswersPerThread }
                || thread.Answers.Any(answer => answer is null || !IsText(answer.Author, ShowcaseIdentity.MaxNameLength) || !IsText(answer.Text, MaxQuestionLength) || !IsPlausible(answer.At)))
            || !Unique(storedThreads.Select(thread => thread.Id)))
            return false;

        if (storedCertificates.Any(certificate => certificate is null || !CodePattern().IsMatch(certificate.Code ?? string.Empty) || FindCourse(certificate.CourseId) is null
                || !IsText(certificate.LearnerName, ShowcaseIdentity.MaxNameLength) || !IsPlausible(certificate.IssuedOn)
                || (certificate.AccountId is not null && !IsAccountId(certificate.AccountId)))
            || !Unique(storedCertificates.Select(certificate => certificate.Code))
            || !Unique(storedCertificates.Where(certificate => certificate.AccountId is not null).Select(certificate => (certificate.AccountId, certificate.CourseId))))
            return false;

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

        if (Enrollment(courseId) is not { } enrollment || FindCourse(courseId) is not { } course)
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

    [GeneratedRegex(@"^QP-[A-Z0-9]{4}-[A-Z0-9]{4}$")]
    private static partial Regex CodePattern();

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (LearningSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsPlausible(DateTime value) => value >= LearningSeedData.Now.AddYears(-3) && value <= LearningSeedData.Now.AddYears(2);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

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

    private LearningEnrollment? Enrollment(int courseId) =>
        Identity.Current is { RoleId: LearningSeedData.LearnerRole } account ? enrollments.FirstOrDefault(enrollment => enrollment.CourseId == courseId && enrollment.AccountId == account.Id) : null;

    /// <summary>Issues the course certificate once, when every lesson is complete, under the learner's current name and a new random code.</summary>
    private void IssueCertificateIfComplete(int courseId)
    {
        if (Identity.Current is not { } account || !IsComplete(courseId) || CertificateFor(courseId) is not null || certificates.Count >= MaxCertificates)
            return;

        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        string code;

        do
        {
            var chars = Enumerable.Range(0, 8).Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]).ToArray();
            code = string.Create(CultureInfo.InvariantCulture, $"QP-{new string(chars, 0, 4)}-{new string(chars, 4, 4)}");
        }
        while (certificates.Any(certificate => certificate.Code == code));

        certificates.Add(new LearningCertificate(code, courseId, account.Name, Clock.Now) { AccountId = account.Id });
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the course categories.</summary>
    public IReadOnlyList<LearningCategory> Categories { get; } = LearningSeedData.Categories();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(LearningSeedData.Now);

    /// <summary>Gets the catalog.</summary>
    public IReadOnlyList<LearningCourse> Courses { get; } = LearningSeedData.Courses();

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

    /// <summary>Gets whether the signed-in account is a learner.</summary>
    public bool IsLearner => Identity.Current?.RoleId == LearningSeedData.LearnerRole;

    /// <summary>Gets the signed-in learner's certificates, the newest first.</summary>
    public IReadOnlyList<LearningCertificate> MyCertificates =>
        Identity.Current is { } account ? certificates.Where(certificate => certificate.AccountId == account.Id).OrderByDescending(certificate => certificate.IssuedOn).ToList() : Array.Empty<LearningCertificate>();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "learning";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

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

        public List<LearningEnrollment>? Enrollments { get; set; }

        public int NextReviewId { get; set; }

        public int NextThreadId { get; set; }

        public List<LearningReview>? Reviews { get; set; }

        public List<LearningQuestionThread>? Threads { get; set; }

        #endregion
    }

    #endregion
}
