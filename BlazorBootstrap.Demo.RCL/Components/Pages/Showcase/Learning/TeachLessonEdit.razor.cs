using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Instructor lesson editor: writes a new lesson in one of the instructor's courses (the <c>module</c> query picks its
/// module) or edits one: title, module, kind, minutes, and a rich-text body with a character counter (formatting
/// included) and a preview rendered exactly as the lesson player renders it (<see cref="ShowcaseHtml.FromEditorHtml" />,
/// A05). A new or draft lesson is saved as a draft or published; a published one updated or unpublished. Turning a quiz
/// into another kind asks first, naming the questions and quiz results that go; deleting names what goes with the lesson
/// (<see cref="LearningState.LessonImpact" />). A quiz lesson links to the quiz builder. Every <see cref="LearningResult" />
/// maps to a message. An unknown or not-own course or lesson shows a not-found panel.
/// </summary>
public partial class TeachLessonEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The editor's starting content for an empty body: one empty paragraph.</summary>
    private const string EmptyBody = "<p><br></p>";

    /// <summary>The minutes a new lesson starts with.</summary>
    private const int NewLessonMinutes = 10;

    /// <summary>Long-form formatting: section headings (the player keeps h2 and h3), lists, quotes, code, and links.</summary>
    private static readonly RichTextEditorToolbarItem[] toolbarItems =
    {
        RichTextEditorToolbarItem.Undo,
        RichTextEditorToolbarItem.Redo,
        RichTextEditorToolbarItem.Bold,
        RichTextEditorToolbarItem.Italic,
        RichTextEditorToolbarItem.Underline,
        RichTextEditorToolbarItem.Strikethrough,
        RichTextEditorToolbarItem.Heading2,
        RichTextEditorToolbarItem.Heading3,
        RichTextEditorToolbarItem.UnorderedList,
        RichTextEditorToolbarItem.OrderedList,
        RichTextEditorToolbarItem.Blockquote,
        RichTextEditorToolbarItem.CodeBlock,
        RichTextEditorToolbarItem.Link,
        RichTextEditorToolbarItem.ClearFormatting,
    };

    private string? bodyError;

    /// <summary>The body as the editor reports it (or as stored, until it is edited); saved as written, shown through the allowlist.</summary>
    private string bodyHtml = string.Empty;

    private string bodyText = string.Empty;

    /// <summary>Set by "Delete lesson"; the delete runs once the dialog is hidden, so the page never leaves under an open dialog.</summary>
    private bool confirmDelete;

    /// <summary>Set by "Change and save"; the save runs once the dialog is hidden.</summary>
    private bool confirmKind;

    /// <summary>The load key of a lesson just created here: its edit route keeps the form as it is instead of reloading it.</summary>
    private string? createdKey;

    private Button? deleteButton;

    private Modal deleteModal = default!;

    /// <summary>The content the editor is built with when a lesson is loaded.</summary>
    private string editorStart = EmptyBody;

    /// <summary>Changes on every load, so the editor is rebuilt with <see cref="editorStart" />.</summary>
    private int editorVersion;

    /// <summary>The element to focus after the next render (the render that shows an error or replaces the clicked button).</summary>
    private Func<ElementReference?>? focusAfterRender;

    private int formErrors;

    private string? formError;

    private Button? keepLessonButton;

    private Button? keepQuizButton;

    private LearningLessonKind kind = LearningLessonKind.Reading;

    private Modal kindModal = default!;

    private ElementReference kindSelect;

    /// <summary>Set after a delete, while the course editor loads.</summary>
    private bool leaving;

    /// <summary>The course and lesson the form was filled from; <see langword="null" /> before the first load.</summary>
    private string? loadedKey;

    /// <summary>The minutes as typed, so a half-typed value isn't replaced while editing.</summary>
    private string minutes = NewLessonMinutes.ToString(CultureInfo.InvariantCulture);

    private string? minutesError;

    private ElementReference minutesInput;

    private string? moduleError;

    private int moduleId;

    private ElementReference moduleSelect;

    /// <summary>Whether the save waiting for the kind confirmation publishes.</summary>
    private bool pendingPublish;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusAfterRender is { } target)
        {
            focusAfterRender = null;
            await LearningDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => EnsureLoaded();

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>
    /// Clears <see cref="LessonId" /> and <see cref="Module" /> first: the new-lesson route has no lesson ID, and the router
    /// reuses this page when moving from a lesson to "New lesson", so the previous lesson's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        LessonId = null;
        Module = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    private static string Count(int count, string singular, string plural) =>
        string.Create(CultureInfo.InvariantCulture, $"{count:#,0} {(count == 1 ? singular : plural)}");

    private static string CounterText(int length, int max) =>
        length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length:#,0} of {max:#,0} characters: {length - max:#,0} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length:#,0} of {max:#,0} characters");

    private static string JoinList(IReadOnlyList<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        2 => $"{items[0]} and {items[1]}",
        _ => $"{string.Join(", ", items.Take(items.Count - 1))}, and {items[^1]}",
    };

    private static string LoadKey(int courseId, int lessonId) => string.Create(CultureInfo.InvariantCulture, $"{courseId}/{lessonId}");

    /// <summary>How many right answers pass a quiz of <paramref name="count" /> questions: two thirds, rounded up (as <see cref="LearningLesson.PassMark" />).</summary>
    private static int PassMarkFor(int count) => (int)Math.Ceiling(count * 2 / 3.0);

    /// <summary>The text of editor HTML, until the editor reports its own.</summary>
    private static string PlainText(string html) =>
        WebUtility.HtmlDecode(TagPattern().Replace(html, " ")).Replace(' ', ' ').Trim();

    private static string QuestionCountText(int count) => Count(count, "question", "questions");

    /// <summary>Says what goes when a quiz becomes another kind: its questions and learners' quiz results (completions stay).</summary>
    private static string RemovedQuizText(int questions, int results, bool future = false)
    {
        var parts = new List<string>();

        if (questions > 0)
            parts.Add($"its {QuestionCountText(questions)}");

        if (results > 0)
            parts.Add($"{Count(results, "learner's", "learners'")} quiz results");

        if (parts.Count == 0)
            return future ? "removes nothing else: it has no questions or results yet." : string.Empty;

        return future
            ? $"removes {JoinList(parts)}. Lessons learners completed stay complete."
            : $"Its quiz is gone: {JoinList(parts)} {(questions + results == 1 ? "was" : "were")} removed. Lessons learners completed stay complete.";
    }

    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();

    private static string? TitleProblem(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Give the lesson a title."
        : value.Trim().Length > LearningState.MaxLessonTitleLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the title to {LearningState.MaxLessonTitleLength} characters or fewer (now {value.Trim().Length}).")
        : null;

    private async Task AskDeleteAsync()
    {
        confirmDelete = false;
        formError = null;

        await deleteModal.ShowAsync();
    }

    private string? BodyProblem() =>
        kind != LearningLessonKind.Quiz && string.IsNullOrWhiteSpace(bodyText)
            ? kind == LearningLessonKind.Video ? "Write the body: learners read it as the video's transcript." : "Write the lesson body."
        : bodyHtml.Length > LearningState.MaxLessonHtmlLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the body: a lesson holds up to {LearningState.MaxLessonHtmlLength:#,0} characters, formatting included (now {bodyHtml.Length:#,0}).")
        : null;

    /// <summary>Saves the form; the kind confirmation (when it was needed) has been given.</summary>
    private void Commit(bool publish)
    {
        if (Course is not { } course)
        {
            ShowFormError("The lesson couldn't be saved: the course is no longer one of yours. Copy your text, then go back to your courses.");
            focusAfterRender = () => statusRegion;

            return;
        }

        var before = Lesson;
        var removedQuestions = QuizWouldBeRemoved ? before!.Questions.Count : 0;
        var removedResults = QuizWouldBeRemoved ? State.LessonImpact(before!.Id).QuizResults : 0;
        var movedTo = before is not null && moduleId != StoredModuleId ? course.Modules.FirstOrDefault(module => module.Id == moduleId) : null;
        var html = string.IsNullOrWhiteSpace(bodyHtml) ? EmptyBody : bodyHtml;
        var result = State.SaveLesson(course.Id, before?.Id ?? 0, moduleId, title, kind, MinutesValue ?? 0, html, publish, out var saved);

        if (result != LearningResult.Ok || saved is null)
        {
            ShowSaveProblem(result, course, before, publish);

            return;
        }

        title = saved.Title;
        formErrors = 0;

        var message = before is null
            ? publish ? $"The lesson \"{saved.Title}\" is published. Learners see it in {course.Title}." : $"The draft \"{saved.Title}\" is saved. Only you see it."
            : publish && before.IsPublished ? $"The lesson \"{saved.Title}\" is updated for learners."
            : publish ? $"The lesson \"{saved.Title}\" is published. Learners see it in {course.Title}."
            : before.IsPublished ? $"The lesson \"{saved.Title}\" is unpublished and saved as a draft. Learners no longer see it."
            : $"The draft \"{saved.Title}\" is saved.";

        if (movedTo is not null)
            message += $" It's now the last lesson of {movedTo.Title}.";

        if (removedQuestions + removedResults > 0)
            message += $" {RemovedQuizText(removedQuestions, removedResults)}";

        if (saved.Kind == LearningLessonKind.Quiz && saved.Questions.Count == 0)
            message += " Add its questions next with Edit questions.";

        SetStatus(message);

        if (before is null)
        {
            // The edit route renders the same page, so the form, editor, and status stay.
            createdKey = LoadKey(course.Id, saved.Id);
            Navigation.NavigateTo(EditUrl(saved.Id), replace: true);

            // The first save swaps the buttons (and adds Delete), so the one pressed is gone: focus goes to the message.
            focusAfterRender = () => statusRegion;

            return;
        }

        if (before.IsPublished != saved.IsPublished)
            focusAfterRender = () => statusRegion;
    }

    private async Task ConfirmDeleteAsync()
    {
        confirmDelete = true;

        await deleteModal.HideAsync();
    }

    private async Task ConfirmKindAsync()
    {
        confirmKind = true;

        await kindModal.HideAsync();
    }

    /// <summary>Says what a delete takes with it: the lesson for learners, completions, quiz results, and questions.</summary>
    private string DeleteConsequence(LearningLesson? lesson)
    {
        if (lesson is null || Course is not { } course)
            return string.Empty;

        var impact = State.LessonImpact(lesson.Id);
        var place = lesson.IsPublished ? $"Learners no longer see it in {course.Title}." : $"The draft is removed from {course.Title}.";
        var parts = new List<string>();

        if (impact.Completions > 0)
            parts.Add($"{Count(impact.Completions, "learner's completion", "learners' completions")} of it");

        if (impact.QuizResults > 0)
            parts.Add($"{Count(impact.QuizResults, "learner's", "learners'")} quiz results");

        if (impact.Questions > 0)
            parts.Add($"{Count(impact.Questions, "question", "questions")} learners asked about it, with the answers");

        var taken = parts.Count == 0
            ? "No learner has completed it or asked about it, so nothing else goes with it."
            : $"These go with it: {JoinList(parts)}.";

        return $"{place} {taken} This can't be undone.";
    }

    private string EditUrl(int lessonId) => string.Create(CultureInfo.InvariantCulture, $"{CourseEditUrl}/lessons/{lessonId}");

    /// <summary>Fills the form for the route once the course (and lesson) can be found; a later state change can complete it.</summary>
    private void EnsureLoaded()
    {
        var key = LoadKey(CourseId, LessonId ?? 0);

        if (loadedKey == key || Course is null || (LessonId is not null && Lesson is null))
            return;

        loadedKey = key;

        // The first save of a new lesson moves to its edit route: the form, editor, and status message stay as they are.
        if (createdKey == key)
        {
            createdKey = null;

            return;
        }

        Load(Lesson);
    }

    /// <summary>Says what turning the stored quiz into another kind removes.</summary>
    private string KindChangeConsequence()
    {
        if (Lesson is not { Kind: LearningLessonKind.Quiz } lesson)
            return string.Empty;

        var label = LearningDisplay.KindLabel(kind).ToLowerInvariant();

        return $"Changing this quiz to a {label} lesson {RemovedQuizText(lesson.Questions.Count, State.LessonImpact(lesson.Id).QuizResults, future: true)}";
    }

    /// <summary>Why the last published lesson stays published.</summary>
    private string LastPublishedText(LearningCourse course, LearningLesson? lesson, string action)
    {
        var why = course.IsPublished ? "is in the catalog" : "has learners";
        var way = course.IsPublished ? "Publish another lesson first, or unpublish the course." : "Publish another lesson first.";

        return $"You can't {action} \"{lesson?.Title}\": it's the only published lesson of {course.Title}, which {why}, so it keeps at least one. {way} Nothing was changed.";
    }

    /// <summary>Fills the form from a lesson, or empties it for a new one (in the module the query names, or the first).</summary>
    private void Load(LearningLesson? lesson)
    {
        var course = Course!;
        var html = lesson is null ? string.Empty : lesson.Html ?? LearningSeedData.SeedHtml(lesson.Id) ?? EmptyBody;

        title = lesson?.Title ?? string.Empty;
        kind = lesson?.Kind ?? LearningLessonKind.Reading;
        minutes = (lesson?.Minutes ?? NewLessonMinutes).ToString(CultureInfo.InvariantCulture);
        moduleId = lesson is not null ? StoredModuleId
            : course.Modules.FirstOrDefault(module => module.Id == Module)?.Id ?? course.Modules.FirstOrDefault()?.Id ?? 0;
        // The editor renders its value as markup, so the stored or seeded body goes through the allowlist first (A05).
        var safeStart = ShowcaseHtml.FromEditorHtml(html, keepHeadings: true).Value;
        editorStart = string.IsNullOrWhiteSpace(safeStart) ? EmptyBody : safeStart;
        bodyHtml = html;
        bodyText = PlainText(html);
        titleError = null;
        moduleError = null;
        minutesError = null;
        bodyError = null;
        formError = null;
        formErrors = 0;
        statusMessage = null;
        statusText = null;
        statusCount = 0;
        editorVersion++;
    }

    private string? MinutesProblem() =>
        MinutesValue is null
            ? string.Create(CultureInfo.InvariantCulture, $"Enter the lesson's length as a whole number of minutes, from 1 to {LearningState.MaxLessonMinutes}.")
            : null;

    private void OnBodyChanged(RichTextEditorChange change)
    {
        bodyHtml = change.Html;
        bodyText = change.Text;

        if (bodyError is not null && BodyProblem() is null)
            bodyError = null;
    }

    /// <summary>
    /// Runs the confirmed delete once the dialog is hidden, then goes back to the course editor; a cancelled or failed
    /// delete returns focus to the Delete button.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        if (!confirmDelete)
        {
            await LearningDisplay.FocusAsync(deleteButton?.Element);

            return;
        }

        confirmDelete = false;

        if (Course is not { } course || Lesson is not { } lesson)
        {
            ShowFormError("The lesson couldn't be deleted: it's no longer in the course.");
            focusAfterRender = () => statusRegion;

            return;
        }

        var result = State.DeleteLesson(course.Id, lesson.Id);

        if (result == LearningResult.Ok)
        {
            leaving = true;
            Toasts.Notify(new ToastMessage(ToastType.Success, IconName.Trash3, "Lesson deleted", $"\"{lesson.Title}\" is removed from {course.Title}."));
            Navigation.NavigateTo(CourseEditUrl);

            return;
        }

        ShowFormError(result switch
        {
            LearningResult.NotFound => "The lesson couldn't be deleted: it's no longer in the course.",
            LearningResult.NotAllowed => "Only the course's instructor can delete its lessons. Sign in with the instructor account.",
            LearningResult.WrongState => LastPublishedText(course, lesson, "delete"),
            _ => "The lesson couldn't be deleted. Try again.",
        });
        focusAfterRender = () => deleteButton?.Element;
    }

    private void OnKindChanged(ChangeEventArgs args)
    {
        if (Enum.TryParse<LearningLessonKind>(args.Value?.ToString(), out var value) && Enum.IsDefined(value))
            kind = value;

        if (bodyError is not null && BodyProblem() is null)
            bodyError = null;
    }

    /// <summary>Runs the save once the kind dialog is hidden; "Keep it a quiz" returns focus to the kind field.</summary>
    private Task OnKindHiddenAsync()
    {
        if (!confirmKind)
            return LearningDisplay.FocusAsync(kindSelect);

        confirmKind = false;
        Commit(pendingPublish);

        // The dialog took focus from the clicked button: the outcome is the next stop, unless an error field needs it.
        focusAfterRender ??= () => statusRegion;

        return Task.CompletedTask;
    }

    private void OnMinutesInput(ChangeEventArgs args)
    {
        minutes = args.Value?.ToString() ?? string.Empty;

        if (minutesError is not null && MinutesProblem() is null)
            minutesError = null;
    }

    private void OnModuleChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            moduleId = value;

        moduleError = null;
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        EnsureLoaded();
        StateHasChanged();
    });

    private void OnTitleInput(ChangeEventArgs args)
    {
        title = args.Value?.ToString() ?? string.Empty;

        if (titleError is not null && TitleProblem(title) is null)
            titleError = null;
    }

    private string QuizUrl(int lessonId) => $"{EditUrl(lessonId)}/quiz";

    /// <summary>
    /// Saves the lesson, as a draft or published. Turning a quiz with questions or results into another kind asks first.
    /// A first save moves to the lesson's edit route; when the buttons change (published ↔ draft), focus goes to the status message.
    /// </summary>
    private async Task SaveAsync(bool publish)
    {
        formError = null;
        titleError = TitleProblem(title);
        moduleError = Course?.Modules.Any(module => module.Id == moduleId) == true ? null : "Choose a module.";
        minutesError = MinutesProblem();
        bodyError = BodyProblem();

        if (titleError is not null)
        {
            focusAfterRender = () => titleInput;

            return;
        }

        if (moduleError is not null)
        {
            focusAfterRender = () => moduleSelect;

            return;
        }

        if (minutesError is not null)
        {
            focusAfterRender = () => minutesInput;

            return;
        }

        if (bodyError is not null)
            return;

        if (QuizWouldBeRemoved)
        {
            pendingPublish = publish;
            confirmKind = false;
            await kindModal.ShowAsync();

            return;
        }

        Commit(publish);
    }

    /// <summary>Enter in a field saves without changing whether the lesson is published.</summary>
    private Task SaveKeepingStatusAsync() => SaveAsync(Lesson?.IsPublished ?? false);

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows a form-level error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowFormError(string message)
    {
        formErrors++;
        formError = formErrors == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrors})");
    }

    /// <summary>Maps a refused save to the field it concerns, or to a form-level message.</summary>
    private void ShowSaveProblem(LearningResult result, LearningCourse course, LearningLesson? before, bool publish)
    {
        switch (result)
        {
            case LearningResult.NotFound when State.FindCourseForEdit(course.Id) is null:
                ShowFormError("The lesson couldn't be saved: the course is no longer one of yours. Copy your text, then go back to your courses.");
                break;

            case LearningResult.NotFound when course.Modules.All(module => module.Id != moduleId):
                moduleError = "That module was deleted. Choose another one.";
                focusAfterRender = () => moduleSelect;
                break;

            case LearningResult.NotFound:
                ShowFormError($"The lesson \"{before?.Title}\" couldn't be saved: it was deleted. Copy your text, then start a new lesson.");
                break;

            case LearningResult.NotAllowed:
                ShowFormError("Only the course's instructor can write its lessons. Sign in with the instructor account.");
                break;

            case LearningResult.WrongState when publish && kind == LearningLessonKind.Quiz:
                ShowFormError(before is { Kind: LearningLessonKind.Quiz }
                    ? "A quiz is published only with questions, and this one has none yet. Choose Edit questions to add them, then publish it. Nothing was saved."
                    : "A quiz is published only with questions. Save it as a draft first, add its questions with Edit questions, then publish it. Nothing was saved.");
                break;

            case LearningResult.WrongState:
                ShowFormError(LastPublishedText(course, before, "unpublish"));
                break;

            case LearningResult.Invalid when before is null && course.Lessons.Count >= LearningState.MaxLessonsPerCourse:
                ShowFormError(FormattableString.Invariant($"The lesson couldn't be added: a course holds up to {LearningState.MaxLessonsPerCourse} lessons. Delete one you no longer need first."));
                break;

            case LearningResult.Invalid:
                ShowFormError("The lesson couldn't be saved: check the title, minutes, and body for missing or too-long text. If they look right, this demo tab is full; reset the demo data.");
                break;

            case LearningResult.NoChange:
                SetStatus("Nothing changed, so there was nothing to save.");
                break;

            default:
                ShowFormError("The lesson couldn't be saved. Try again.");
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string BodyHelp => kind switch
    {
        LearningLessonKind.Video => "Learners read it as the video's transcript.",
        LearningLessonKind.Quiz => "Optional for a quiz: an introduction shown above the questions.",
        _ => "Learners read it as the lesson.",
    };

    private string BodyLabel => kind == LearningLessonKind.Quiz ? "Body (optional)" : "Body";

    private bool BodyOver => bodyHtml.Length > LearningState.MaxLessonHtmlLength;

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Courses", Href = CoursesUrl },
        new() { Text = Course?.Title ?? "Course", Href = CourseEditUrl },
        new() { Text = Lesson?.Title ?? "New lesson", IsCurrentPage = true },
    };

    /// <summary>The course (every lesson, drafts included), or <see langword="null" /> when it isn't one of the instructor's.</summary>
    private LearningCourse? Course => State.FindCourseForEdit(CourseId);

    private string CourseEditUrl => string.Create(CultureInfo.InvariantCulture, $"{CoursesUrl}/{CourseId}");

    /// <summary>Gets or sets the course ID from the route.</summary>
    [Parameter] public int CourseId { get; set; }

    private static string CoursesUrl => $"{TeachLayout.Base}/courses";

    /// <summary>The lesson being edited (drafts included), or <see langword="null" /> for a new or unknown one.</summary>
    private LearningLesson? Lesson => LessonId is int id ? Course?.Lessons.FirstOrDefault(lesson => lesson.Id == id) : null;

    /// <summary>Gets or sets the lesson ID from the route; <see langword="null" /> on the new-lesson route.</summary>
    [Parameter] public int? LessonId { get; set; }

    /// <summary>The minutes typed, when they are a whole number from 1 to <see cref="LearningState.MaxLessonMinutes" />.</summary>
    private int? MinutesValue =>
        int.TryParse(minutes, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value is >= 1 and <= LearningState.MaxLessonMinutes ? value : null;

    /// <summary>Gets or sets the module a new lesson starts in (the <c>module</c> query); the first module when absent or unknown.</summary>
    [SupplyParameterFromQuery(Name = "module")] public int? Module { get; set; }

    /// <summary>The 1-based position of the chosen module, for the preview.</summary>
    private int ModuleNumber => Course is { } course ? course.Modules.ToList().FindIndex(module => module.Id == moduleId) + 1 : 0;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>How many questions the stored quiz has (0 for another kind or a new lesson).</summary>
    private int QuestionCount => Lesson is { Kind: LearningLessonKind.Quiz } lesson ? lesson.Questions.Count : 0;

    private static string QuestionsUrl => $"{TeachLayout.Base}/questions";

    /// <summary>Whether saving would turn the stored quiz, with questions or learners' results, into another kind.</summary>
    private bool QuizWouldBeRemoved =>
        Lesson is { Kind: LearningLessonKind.Quiz } lesson && kind != LearningLessonKind.Quiz
        && (lesson.Questions.Count > 0 || State.LessonImpact(lesson.Id).QuizResults > 0);

    [Inject] private LearningState State { get; set; } = default!;

    /// <summary>The module the stored lesson is in (0 for a new one).</summary>
    private int StoredModuleId => LessonId is int id ? Course?.Modules.FirstOrDefault(module => module.Lessons.Any(lesson => lesson.Id == id))?.Id ?? 0 : 0;

    private bool TitleOver => title.Trim().Length > LearningState.MaxLessonTitleLength;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
