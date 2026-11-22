using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Creates a course or edits one of the signed-in instructor's courses. <b>Details</b>: title, subtitle, category, level,
/// the instructor's role on the course page, a description, and up to <see cref="LearningState.MaxOutcomes" /> "What you'll
/// learn" outcomes, with counters and field errors (focus moves to the first invalid field after the render that shows it).
/// <b>Status</b>: Published or Draft in words, Publish (a course needs a published lesson, explained otherwise) or
/// Unpublish (asks first and names the effect), and "View in school". <b>Curriculum</b> (a saved course only): modules in
/// order with Rename (inline), Move up/down, and Delete (only when empty, explained otherwise); their lessons in order with
/// kind, length, Published or Draft, and for a self-check its questions; each lesson has Edit, Move up/down, Publish or
/// Unpublish, and Delete (asks first and names what goes with it, from <see cref="LearningState.LessonImpact" />). A
/// reorder keeps focus on the moved item's button and announces its new position. Every <see cref="LearningResult" /> maps
/// to a message. An unknown ID, or a course that isn't the instructor's, shows a not-found panel.
/// </summary>
public partial class TeachCourseEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly StatusMessage curriculumStatus = new();

    private readonly StatusMessage detailsStatus = new();

    private readonly Dictionary<int, ElementReference> lessonDeleteButtons = new();

    private readonly Dictionary<int, ElementReference> lessonDownButtons = new();

    private readonly Dictionary<int, ElementReference> lessonUpButtons = new();

    private readonly Dictionary<int, ElementReference> moduleDownButtons = new();

    private readonly Dictionary<int, ElementReference> moduleUpButtons = new();

    private readonly Dictionary<int, ElementReference> outcomeInputs = new();

    private readonly List<OutcomeDraft> outcomes = new();

    private readonly StatusMessage publishStatus = new();

    private readonly Dictionary<int, ElementReference> renameButtons = new();

    private ElementReference addOutcomeButton;

    private string? categoryError;

    private string categoryId = string.Empty;

    private ElementReference categorySelect;

    /// <summary>Set by "Delete lesson"; the delete runs once the dialog is hidden, so nothing changes under an open dialog.</summary>
    private bool confirmLessonDelete;

    /// <summary>Set by "Unpublish"; the change runs once the dialog is hidden.</summary>
    private bool confirmUnpublish;

    /// <summary>The ID a new course got on its first save: its edit route keeps the form and message instead of reloading.</summary>
    private int? createdId;

    private ElementReference curriculumStatusRegion;

    private Modal deleteLessonModal = default!;

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference descriptionInput;

    private ElementReference detailsStatusRegion;

    /// <summary>The element to focus after the next render, read then (it may only exist after that render).</summary>
    private Func<ElementReference?>? focusAfterRender;

    private int formErrors;

    private string? formError;

    private string instructorRole = string.Empty;

    private LearningLevel level = LearningLevel.Beginner;

    /// <summary>The route ID the form was filled from (0 for a new course); <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private string? newModuleError;

    private ElementReference newModuleInput;

    private string newModuleTitle = string.Empty;

    /// <summary>The next key for an outcome row, so each input keeps its element (and focus) when rows are removed.</summary>
    private int nextOutcomeKey;

    /// <summary>The lesson the delete dialog is about.</summary>
    private LearningLesson? pendingLesson;

    /// <summary>Whether the lesson in the delete dialog is the last published one of a course that must keep one.</summary>
    private bool pendingLessonBlocked;

    private ElementReference publishButton;

    private ElementReference publishStatusRegion;

    private string? renameError;

    private ElementReference renameInput;

    /// <summary>The module whose inline rename form is open, if any.</summary>
    private int? renamingModuleId;

    private string renameText = string.Empty;

    private string? roleError;

    private ElementReference roleInput;

    private string subtitle = string.Empty;

    private string? subtitleError;

    private ElementReference subtitleInput;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference titleInput;

    private Modal unpublishModal = default!;

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

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;

        // The first save of a new course moves to its edit route: the form and its status message stay as they are.
        if (createdId == routeId)
        {
            createdId = null;

            return;
        }

        Load(Item);
    }

    #endregion

    #region Public Methods

    /// <summary>Returns the route of a course's editor.</summary>
    public static string EditUrl(int courseId) => FormattableString.Invariant($"{TeachLayout.Base}/courses/{courseId}");

    /// <summary>Returns the route of a lesson's editor.</summary>
    public static string LessonEditUrl(int courseId, int lessonId) => FormattableString.Invariant($"{EditUrl(courseId)}/lessons/{lessonId}");

    /// <summary>Returns the route of a new lesson at the end of a module.</summary>
    public static string NewLessonUrl(int courseId, int moduleId) => FormattableString.Invariant($"{EditUrl(courseId)}/lessons/new?module={moduleId}");

    /// <summary>Returns the route of a self-check's question builder.</summary>
    public static string QuizUrl(int courseId, int lessonId) => $"{LessonEditUrl(courseId, lessonId)}/quiz";

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-course route has no ID, and the router reuses this page when moving from a
    /// course to "New course", so the previous course's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    private static string CounterText(string value, int max)
    {
        var length = value.Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    private static string CurriculumText(LearningCourse course)
    {
        var lessons = course.Lessons;
        var published = lessons.Count(lesson => lesson.IsPublished);
        var modules = course.Modules.Count == 1 ? "1 module" : string.Create(CultureInfo.InvariantCulture, $"{course.Modules.Count} modules");
        var lessonText = lessons.Count == 1 ? "1 lesson" : string.Create(CultureInfo.InvariantCulture, $"{lessons.Count} lessons");

        return lessons.Count == 0
            ? $"{modules}, no lessons yet."
            : string.Create(CultureInfo.InvariantCulture, $"{modules}, {lessonText} ({published} published), {LearningDisplay.Duration(course.Minutes)} in all.");
    }

    /// <summary>Says what deleting a lesson takes with it.</summary>
    private static string ImpactText(LearningLessonImpact impact)
    {
        var parts = new List<string>(3);

        if (impact.Completions > 0)
            parts.Add(impact.Completions == 1 ? "1 learner's completion" : string.Create(CultureInfo.InvariantCulture, $"{impact.Completions} learners' completions"));

        if (impact.QuizResults > 0)
            parts.Add(impact.QuizResults == 1 ? "1 learner's quiz results" : string.Create(CultureInfo.InvariantCulture, $"{impact.QuizResults} learners' quiz results"));

        if (impact.Questions > 0)
            parts.Add(impact.Questions == 1 ? "1 learner question with its answers" : string.Create(CultureInfo.InvariantCulture, $"{impact.Questions} learner questions with their answers"));

        return parts.Count switch
        {
            0 => "No learner has completed it, taken it, or asked about it, so nothing else is removed.",
            1 => $"Also removed: {parts[0]}.",
            _ => $"Also removed: {string.Join(", ", parts.SkipLast(1))} and {parts[^1]}.",
        };
    }

    private static bool IsOver(string value, int max) => value.Trim().Length > max;

    /// <summary>Says who keeps access when a course is unpublished.</summary>
    private static string KeepAccessText(int learners) => learners switch
    {
        0 => "No learners are enrolled, so nobody loses access.",
        1 => "Its 1 learner keeps access to the course, their progress, and any certificate.",
        _ => string.Create(CultureInfo.InvariantCulture, $"Its {learners} learners keep access to the course, their progress, and their certificates."),
    };

    private static string ModuleText(LearningModule module)
    {
        if (module.Lessons.Count == 0)
            return "No lessons yet";

        var published = module.Lessons.Count(lesson => lesson.IsPublished);
        var lessons = module.Lessons.Count == 1 ? "1 lesson" : string.Create(CultureInfo.InvariantCulture, $"{module.Lessons.Count} lessons");

        return string.Create(CultureInfo.InvariantCulture, $"{lessons}, {published} published · {LearningDisplay.Duration(module.Lessons.Sum(lesson => lesson.Minutes))}");
    }

    private static string QuestionsText(LearningLesson lesson) => lesson.Questions.Count == 1
        ? "1 question, pass with 1"
        : string.Create(CultureInfo.InvariantCulture, $"{lesson.Questions.Count} questions, pass with {lesson.PassMark}");

    /// <summary>The message for a refusal that isn't about the item's state.</summary>
    private static string RefusalText(LearningResult result, string what) => result switch
    {
        LearningResult.NotFound => $"{what} is no longer in your course, so nothing changed. It may have been deleted.",
        LearningResult.NotAllowed => "Sign in as an instructor to do that. Nothing changed.",
        LearningResult.Invalid => "That isn't possible here, and nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private static string? TextProblem(string value, int max, string emptyMessage, string field) =>
        string.IsNullOrWhiteSpace(value) ? emptyMessage
        : value.Trim().Length > max ? string.Create(CultureInfo.InvariantCulture, $"Shorten the {field} to {max} characters or fewer (now {value.Trim().Length}).")
        : null;

    private void AddModule(LearningCourse shown)
    {
        newModuleError = TextProblem(newModuleTitle, LearningState.MaxModuleTitleLength, "Give the module a title.", "module title");

        if (newModuleError is not null)
        {
            focusAfterRender = () => newModuleInput;

            return;
        }

        var result = State.AddModule(shown.Id, newModuleTitle, out var module);

        switch (result)
        {
            case LearningResult.Ok when module is not null:
                newModuleTitle = string.Empty;
                curriculumStatus.Set($"Added the module \"{module.Title}\" at the end of the curriculum. Use \"Add lesson\" to write its first lesson.");
                break;

            case LearningResult.Invalid when Item is { } course && course.Modules.Count >= LearningState.MaxModulesPerCourse:
                newModuleError = FormattableString.Invariant($"A course holds up to {LearningState.MaxModulesPerCourse} modules. Delete an empty one first.");
                focusAfterRender = () => newModuleInput;
                break;

            case LearningResult.Invalid:
                newModuleError = "The module couldn't be added: check its title.";
                focusAfterRender = () => newModuleInput;
                break;

            default:
                curriculumStatus.Set(RefusalText(result, "The course"), isError: true);
                break;
        }
    }

    private void AddOutcome()
    {
        if (outcomes.Count >= LearningState.MaxOutcomes)
            return;

        var row = new OutcomeDraft(nextOutcomeKey++, string.Empty);
        outcomes.Add(row);
        focusAfterRender = () => outcomeInputs.TryGetValue(row.Key, out var input) ? input : null;
    }

    private async Task AskDeleteLessonAsync(LearningCourse shown, LearningLesson lesson)
    {
        pendingLesson = lesson;
        pendingLessonBlocked = IsLastPublished(shown, lesson);
        confirmLessonDelete = false;

        await deleteLessonModal.ShowAsync();
    }

    private void CancelRename()
    {
        var moduleId = renamingModuleId;
        renamingModuleId = null;
        renameError = null;
        focusAfterRender = () => moduleId is int id && renameButtons.TryGetValue(id, out var button) ? button : null;
    }

    private async Task ConfirmDeleteLessonAsync()
    {
        confirmLessonDelete = true;

        await deleteLessonModal.HideAsync();
    }

    private async Task ConfirmUnpublishAsync()
    {
        confirmUnpublish = true;

        await unpublishModal.HideAsync();
    }

    /// <summary>Deletes an empty module at once; a module with lessons can't be deleted, which is explained instead.</summary>
    private void DeleteModule(LearningCourse shown, LearningModule module)
    {
        if (module.Lessons.Count > 0)
        {
            curriculumStatus.Set(ModuleNotEmptyText(module), isError: true);

            return;
        }

        var result = State.DeleteModule(shown.Id, module.Id);

        switch (result)
        {
            case LearningResult.Ok:
                if (renamingModuleId == module.Id)
                    renamingModuleId = null;

                renameButtons.Remove(module.Id);
                moduleUpButtons.Remove(module.Id);
                moduleDownButtons.Remove(module.Id);
                curriculumStatus.Set($"Deleted the empty module \"{module.Title}\".");
                // Its buttons are gone; the status message is next to where it was.
                focusAfterRender = () => curriculumStatusRegion;
                break;

            case LearningResult.WrongState:
                curriculumStatus.Set(ModuleNotEmptyText(Item?.Modules.FirstOrDefault(item => item.Id == module.Id) ?? module), isError: true);
                break;

            default:
                curriculumStatus.Set(RefusalText(result, $"The module \"{module.Title}\""), isError: true);
                break;
        }
    }

    /// <summary>Whether a lesson is the only published one of a course that must keep one (published, or with learners).</summary>
    private bool IsLastPublished(LearningCourse course, LearningLesson lesson) =>
        lesson.IsPublished && (course.IsPublished || LearnerCount(course.Id) > 0) && course.Lessons.Count(item => item.IsPublished) == 1;

    /// <summary>Explains the rule that keeps the last published lesson of a published course, or one with learners.</summary>
    private string LastLessonText(LearningCourse course, LearningLesson lesson, string action)
    {
        var learners = LearnerCount(course.Id);
        var reason = course.IsPublished ? "a published course" : "a course with learners";
        var instead = course.IsPublished && learners == 0 ? " Publish another lesson first, or unpublish the course." : " Publish another lesson first.";

        return $"\"{lesson.Title}\" can't be {action}: it's the only published lesson, and {reason} keeps at least one, so learners always have something to open.{instead}";
    }

    private int LearnerCount(int courseId) => State.Roster.Count(row => row.Course.Id == courseId);

    /// <summary>Fills the form from a course, or empties it for a new one, and clears every message.</summary>
    private void Load(LearningCourse? course)
    {
        title = course?.Title ?? string.Empty;
        subtitle = course?.Subtitle ?? string.Empty;
        categoryId = course?.CategoryId ?? State.Categories.FirstOrDefault()?.Id ?? string.Empty;
        level = course?.Level ?? LearningLevel.Beginner;
        instructorRole = course?.InstructorRole ?? string.Empty;
        description = course?.Description ?? string.Empty;
        SetOutcomes(course?.Outcomes);

        // A new course starts with one empty outcome to fill in.
        if (course is null)
            outcomes.Add(new OutcomeDraft(nextOutcomeKey++, string.Empty));

        titleError = subtitleError = categoryError = roleError = descriptionError = null;
        formError = null;
        formErrors = 0;
        renamingModuleId = null;
        renameError = null;
        newModuleTitle = string.Empty;
        newModuleError = null;
        pendingLesson = null;
        detailsStatus.Clear();
        publishStatus.Clear();
        curriculumStatus.Clear();
    }

    private string ModuleNotEmptyText(LearningModule module) =>
        (module.Lessons.Count == 1 ? $"\"{module.Title}\" still has 1 lesson" : string.Create(CultureInfo.InvariantCulture, $"\"{module.Title}\" still has {module.Lessons.Count} lessons"))
        + ", and only an empty module can be deleted. Delete its lessons, or move them to another module in each lesson's editor, first.";

    /// <summary>Moves a lesson inside its module; focus stays on the same button, or the other one at an end, and the move is announced.</summary>
    private void MoveLesson(LearningCourse shown, LearningModule module, LearningLesson lesson, int step)
    {
        var result = State.MoveLesson(shown.Id, lesson.Id, step);

        if (result != LearningResult.Ok)
        {
            curriculumStatus.Set(result == LearningResult.NoChange
                ? $"\"{lesson.Title}\" is already {(step < 0 ? "first" : "last")} in its module."
                : RefusalText(result, $"The lesson \"{lesson.Title}\""), isError: result != LearningResult.NoChange);

            return;
        }

        var lessons = Item?.Modules.FirstOrDefault(item => item.Id == module.Id)?.Lessons ?? module.Lessons;
        var index = lessons.ToList().FindIndex(item => item.Id == lesson.Id);
        var atEnd = step < 0 ? index == 0 : index == lessons.Count - 1;
        var buttons = (step < 0) != atEnd ? lessonUpButtons : lessonDownButtons;

        curriculumStatus.Set(string.Create(CultureInfo.InvariantCulture, $"Moved {lesson.Title} to position {index + 1} of {lessons.Count}."));
        focusAfterRender = () => buttons.TryGetValue(lesson.Id, out var button) ? button : null;
    }

    /// <summary>Moves a module; focus stays on the same button, or the other one at an end, and the move is announced.</summary>
    private void MoveModule(LearningCourse shown, LearningModule module, int step)
    {
        var result = State.MoveModule(shown.Id, module.Id, step);

        if (result != LearningResult.Ok)
        {
            curriculumStatus.Set(result == LearningResult.NoChange
                ? $"\"{module.Title}\" is already the {(step < 0 ? "first" : "last")} module."
                : RefusalText(result, $"The module \"{module.Title}\""), isError: result != LearningResult.NoChange);

            return;
        }

        var modules = Item?.Modules ?? shown.Modules;
        var index = modules.ToList().FindIndex(item => item.Id == module.Id);
        var atEnd = step < 0 ? index == 0 : index == modules.Count - 1;
        var buttons = (step < 0) != atEnd ? moduleUpButtons : moduleDownButtons;

        curriculumStatus.Set(string.Create(CultureInfo.InvariantCulture, $"Moved {module.Title} to position {index + 1} of {modules.Count}."));
        focusAfterRender = () => buttons.TryGetValue(module.Id, out var button) ? button : null;
    }

    private void OnCategoryChanged(ChangeEventArgs args)
    {
        categoryId = args.Value?.ToString() ?? string.Empty;
        categoryError = null;
    }

    /// <summary>
    /// Runs a confirmed delete once the dialog is hidden; focus goes to the curriculum status (the lesson is gone), or back
    /// to the Delete button after Cancel or a refusal.
    /// </summary>
    private async Task OnDeleteLessonHiddenAsync()
    {
        var lesson = pendingLesson;
        pendingLesson = null;
        var opener = lesson is not null && lessonDeleteButtons.TryGetValue(lesson.Id, out var button) ? button : (ElementReference?)null;

        if (!confirmLessonDelete || lesson is null || Item is not { } course)
        {
            confirmLessonDelete = false;
            await LearningDisplay.FocusAsync(opener);

            return;
        }

        confirmLessonDelete = false;
        var impact = State.LessonImpact(lesson.Id);
        var result = State.DeleteLesson(course.Id, lesson.Id);

        switch (result)
        {
            case LearningResult.Ok:
                lessonDeleteButtons.Remove(lesson.Id);
                lessonUpButtons.Remove(lesson.Id);
                lessonDownButtons.Remove(lesson.Id);
                curriculumStatus.Set(impact == new LearningLessonImpact(0, 0, 0)
                    ? $"Deleted the lesson \"{lesson.Title}\"."
                    : $"Deleted the lesson \"{lesson.Title}\". {ImpactText(impact)}");
                focusAfterRender = () => curriculumStatusRegion;
                break;

            case LearningResult.WrongState:
                curriculumStatus.Set(LastLessonText(course, lesson, "deleted"), isError: true);
                focusAfterRender = () => opener;
                break;

            default:
                curriculumStatus.Set(RefusalText(result, $"The lesson \"{lesson.Title}\""), isError: true);
                focusAfterRender = () => curriculumStatusRegion;
                break;
        }
    }

    private void OnDescriptionInput(ChangeEventArgs args)
    {
        description = args.Value?.ToString() ?? string.Empty;

        if (descriptionError is not null)
            descriptionError = TextProblem(description, LearningState.MaxCourseDescriptionLength, DescriptionMissing, "description");
    }

    private void OnLevelChanged(ChangeEventArgs args)
    {
        if (Enum.TryParse<LearningLevel>(args.Value?.ToString(), out var value) && Enum.IsDefined(value))
            level = value;
    }

    private void OnNewModuleInput(ChangeEventArgs args)
    {
        newModuleTitle = args.Value?.ToString() ?? string.Empty;

        if (newModuleError is not null)
            newModuleError = TextProblem(newModuleTitle, LearningState.MaxModuleTitleLength, "Give the module a title.", "module title");
    }

    private void OnOutcomeInput(OutcomeDraft row, ChangeEventArgs args)
    {
        row.Text = args.Value?.ToString() ?? string.Empty;

        if (row.Error is not null && !IsOver(row.Text, LearningState.MaxOutcomeLength))
            row.Error = null;
    }

    private void OnRenameInput(ChangeEventArgs args)
    {
        renameText = args.Value?.ToString() ?? string.Empty;

        if (renameError is not null)
            renameError = TextProblem(renameText, LearningState.MaxModuleTitleLength, "Give the module a title.", "module title");
    }

    private void OnRoleInput(ChangeEventArgs args)
    {
        instructorRole = args.Value?.ToString() ?? string.Empty;

        if (roleError is not null)
            roleError = TextProblem(instructorRole, LearningState.MaxInstructorRoleLength, RoleMissing, "role");
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnSubtitleInput(ChangeEventArgs args)
    {
        subtitle = args.Value?.ToString() ?? string.Empty;

        if (subtitleError is not null)
            subtitleError = TextProblem(subtitle, LearningState.MaxCourseSubtitleLength, SubtitleMissing, "subtitle");
    }

    private void OnTitleInput(ChangeEventArgs args)
    {
        title = args.Value?.ToString() ?? string.Empty;

        if (titleError is not null)
            titleError = TextProblem(title, LearningState.MaxCourseTitleLength, TitleMissing, "title");
    }

    /// <summary>Runs a confirmed unpublish once the dialog is hidden, then focuses its status message; Cancel returns focus to the button.</summary>
    private async Task OnUnpublishHiddenAsync()
    {
        if (!confirmUnpublish || Item is not { } course)
        {
            confirmUnpublish = false;
            await LearningDisplay.FocusAsync(publishButton);

            return;
        }

        confirmUnpublish = false;
        var result = State.SetCoursePublished(course.Id, false);

        publishStatus.Set(result switch
        {
            LearningResult.Ok => $"\"{course.Title}\" is unpublished: it's out of the catalog and takes no new enrollments. {KeepAccessText(LearnerCount(course.Id))}",
            LearningResult.NoChange => $"\"{course.Title}\" was already unpublished.",
            _ => RefusalText(result, "The course"),
        }, isError: result is not (LearningResult.Ok or LearningResult.NoChange));
        focusAfterRender = () => publishStatusRegion;
    }

    private void RemoveOutcome(int index)
    {
        if (index < 0 || index >= outcomes.Count)
            return;

        outcomeInputs.Remove(outcomes[index].Key);
        outcomes.RemoveAt(index);

        // Focus goes to the outcome now in its place (or the last one), or to "Add outcome" when none is left.
        if (outcomes.Count == 0)
        {
            focusAfterRender = () => addOutcomeButton;

            return;
        }

        var next = outcomes[Math.Min(index, outcomes.Count - 1)];
        focusAfterRender = () => outcomeInputs.TryGetValue(next.Key, out var input) ? input : null;
    }

    /// <summary>
    /// Checks every field, then saves. The first field with a problem gets focus after the render that shows the errors;
    /// the form-level alert names the fields. A first save moves to the new course's edit route, keeping the message.
    /// </summary>
    private void Save()
    {
        formError = null;
        titleError = TextProblem(title, LearningState.MaxCourseTitleLength, TitleMissing, "title");
        subtitleError = TextProblem(subtitle, LearningState.MaxCourseSubtitleLength, SubtitleMissing, "subtitle");
        categoryError = State.Categories.Any(category => category.Id == categoryId) ? null : "Choose a category.";
        roleError = TextProblem(instructorRole, LearningState.MaxInstructorRoleLength, RoleMissing, "role");
        descriptionError = TextProblem(description, LearningState.MaxCourseDescriptionLength, DescriptionMissing, "description");

        var problems = new List<(string Field, Func<ElementReference?> Target)>();

        if (titleError is not null)
            problems.Add(("title", () => titleInput));

        if (subtitleError is not null)
            problems.Add(("subtitle", () => subtitleInput));

        if (categoryError is not null)
            problems.Add(("category", () => categorySelect));

        if (roleError is not null)
            problems.Add(("your role", () => roleInput));

        if (descriptionError is not null)
            problems.Add(("description", () => descriptionInput));

        for (var i = 0; i < outcomes.Count; i++)
        {
            var row = outcomes[i];
            row.Error = IsOver(row.Text, LearningState.MaxOutcomeLength)
                ? string.Create(CultureInfo.InvariantCulture, $"Shorten outcome {i + 1} to {LearningState.MaxOutcomeLength} characters or fewer (now {row.Text.Trim().Length}).")
                : null;

            if (row.Error is not null)
                problems.Add((string.Create(CultureInfo.InvariantCulture, $"outcome {i + 1}"), () => outcomeInputs.TryGetValue(row.Key, out var input) ? input : null));
        }

        if (problems.Count > 0)
        {
            ShowFormError(problems.Count == 1
                ? $"Check the {problems[0].Field}, then save again."
                : string.Create(CultureInfo.InvariantCulture, $"Check these {problems.Count} fields, then save again: {string.Join(", ", problems.Select(problem => problem.Field))}."));
            focusAfterRender = problems[0].Target;

            return;
        }

        var before = Item;
        var result = State.SaveCourse(before?.Id ?? 0, title, subtitle, categoryId, level, instructorRole, description, outcomes.Select(row => row.Text).ToList(), out var saved);

        switch (result)
        {
            case LearningResult.Ok when saved is not null:
                formErrors = 0;
                title = saved.Title;
                subtitle = saved.Subtitle;
                instructorRole = saved.InstructorRole;
                description = saved.Description;
                SetOutcomes(saved.Outcomes);

                if (before is null)
                {
                    detailsStatus.Set($"\"{saved.Title}\" is saved as a draft. Next, add a module and its lessons below, then publish the course.");
                    // The edit route renders this same page, so the form and the message stay.
                    createdId = saved.Id;
                    Navigation.NavigateTo(EditUrl(saved.Id), replace: true);

                    return;
                }

                detailsStatus.Set(saved.IsPublished
                    ? $"The details of \"{saved.Title}\" are saved. The course page shows them now."
                    : $"The details of \"{saved.Title}\" are saved. It's a draft, so only you see the course page.");
                break;

            case LearningResult.NoChange:
                detailsStatus.Set("Nothing changed, so there was nothing to save.");
                break;

            case LearningResult.Invalid when State.Categories.All(category => category.Id != categoryId):
                categoryError = "The chosen category no longer exists. Choose another one.";
                ShowFormError("Check the category, then save again.");
                focusAfterRender = () => categorySelect;
                break;

            case LearningResult.Invalid:
                ShowFormError(before is null
                    ? FormattableString.Invariant($"The course couldn't be created: a value is missing or too long, or the school already holds its limit of {LearningState.MaxCourses} courses. Check every field and try again.")
                    : "The course couldn't be saved: a value is missing or too long. Check every field and try again.");
                break;

            case LearningResult.NotFound:
                ShowFormError("The course couldn't be saved: it's no longer one of yours. It may have been deleted. Copy your text, then start a new course.");
                break;

            case LearningResult.NotAllowed:
                ShowFormError("Sign in as an instructor to save courses. Nothing was saved.");
                break;

            default:
                ShowFormError("The course couldn't be saved. Try again.");
                break;
        }
    }

    /// <summary>Saves the inline rename; focus returns to the module's Rename button, or stays in the field on an error.</summary>
    private void SaveRename(LearningCourse shown, LearningModule module)
    {
        renameError = TextProblem(renameText, LearningState.MaxModuleTitleLength, "Give the module a title.", "module title");

        if (renameError is not null)
        {
            focusAfterRender = () => renameInput;

            return;
        }

        var result = State.RenameModule(shown.Id, module.Id, renameText);

        switch (result)
        {
            case LearningResult.Ok:
                curriculumStatus.Set($"Renamed the module \"{module.Title}\" to \"{renameText.Trim()}\".");
                break;

            case LearningResult.NoChange:
                curriculumStatus.Set($"The module title is still \"{module.Title}\": nothing changed.");
                break;

            case LearningResult.Invalid:
                renameError = "The title couldn't be saved: check that it isn't empty or too long.";
                focusAfterRender = () => renameInput;

                return;

            default:
                curriculumStatus.Set(RefusalText(result, $"The module \"{module.Title}\""), isError: true);
                break;
        }

        renamingModuleId = null;
        focusAfterRender = () => renameButtons.TryGetValue(module.Id, out var button) ? button : null;
    }

    /// <summary>Replaces the outcome rows with saved outcomes.</summary>
    private void SetOutcomes(IReadOnlyList<string>? saved)
    {
        outcomes.Clear();
        outcomeInputs.Clear();

        foreach (var outcome in saved ?? Array.Empty<string>())
            outcomes.Add(new OutcomeDraft(nextOutcomeKey++, outcome));
    }

    /// <summary>Shows a form-level error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowFormError(string message)
    {
        formErrors++;
        formError = formErrors == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrors})");
    }

    private void StartRename(LearningModule module)
    {
        renamingModuleId = module.Id;
        renameText = module.Title;
        renameError = null;
        focusAfterRender = () => renameInput;
    }

    /// <summary>Unpublishing asks first; publishing happens at once (the course needs a published lesson) and is reported.</summary>
    private async Task ToggleCoursePublishedAsync(LearningCourse shown)
    {
        if (shown.IsPublished)
        {
            confirmUnpublish = false;

            await unpublishModal.ShowAsync();

            return;
        }

        var result = State.SetCoursePublished(shown.Id, true);

        publishStatus.Set(result switch
        {
            LearningResult.Ok => $"\"{shown.Title}\" is published: it's in the catalog, and learners can enroll.",
            LearningResult.NoChange => $"\"{shown.Title}\" was already published.",
            LearningResult.WrongState => $"\"{shown.Title}\" can't be published yet: it has no published lesson. Publish at least one lesson in the curriculum, then publish the course.",
            _ => RefusalText(result, "The course"),
        }, isError: result is not (LearningResult.Ok or LearningResult.NoChange));
    }

    /// <summary>Publishes a lesson or turns it back into a draft; the button stays, so focus does too, and the outcome is announced.</summary>
    private void ToggleLessonPublished(LearningCourse shown, LearningLesson lesson)
    {
        var publish = !lesson.IsPublished;
        var result = State.SetLessonPublished(shown.Id, lesson.Id, publish);
        var learnersSee = shown.IsPublished || LearnerCount(shown.Id) > 0;

        curriculumStatus.Set(result switch
        {
            LearningResult.Ok when publish => learnersSee
                ? $"\"{lesson.Title}\" is published: learners see it in the course now."
                : $"\"{lesson.Title}\" is published. Learners see it once you publish the course.",
            LearningResult.Ok => learnersSee ? $"\"{lesson.Title}\" is a draft again: learners no longer see it." : $"\"{lesson.Title}\" is a draft again.",
            LearningResult.NoChange => $"\"{lesson.Title}\" was already {(publish ? "published" : "a draft")}.",
            LearningResult.WrongState when publish => $"\"{lesson.Title}\" can't be published yet: a self-check needs questions. Add them with \"Add questions before publishing\", then publish it.",
            LearningResult.WrongState => LastLessonText(shown, lesson, "unpublished"),
            _ => RefusalText(result, $"The lesson \"{lesson.Title}\""),
        }, isError: result is not (LearningResult.Ok or LearningResult.NoChange));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the route of a new course.</summary>
    public static string NewUrl => $"{TeachLayout.Base}/courses/new";

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Courses", Href = TeachCourses.ListUrl },
        new() { Text = Item?.Title ?? "New course", IsCurrentPage = true },
    };

    private static string DescriptionMissing => "Write a description: what the course covers and who it's for.";

    /// <summary>Gets or sets the course ID from the route; <see langword="null" /> on the new-course route.</summary>
    [Parameter] public int? Id { get; set; }

    /// <summary>The course being edited (every lesson, drafts included), or <see langword="null" /> for a new, unknown, or not-own one.</summary>
    private LearningCourse? Item => Id is int id ? State.FindCourseForEdit(id) : null;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string OutcomeCountText => outcomes.Count >= LearningState.MaxOutcomes
        ? FormattableString.Invariant($"{outcomes.Count} of {LearningState.MaxOutcomes}: the most a course can list.")
        : FormattableString.Invariant($"{outcomes.Count} of {LearningState.MaxOutcomes} outcomes");

    private static string RoleMissing => "Say what you do, as learners see it after your name (for example, \"Senior .NET engineer\").";

    [Inject] private LearningState State { get; set; } = default!;

    private static string SubtitleMissing => "Write a subtitle: one line on what learners get from the course.";

    private static string TitleMissing => "Give the course a title.";

    #endregion

    #region Nested Types

    /// <summary>An outcome being edited, with a stable key so its input keeps its element when rows are removed.</summary>
    private sealed class OutcomeDraft
    {
        #region Constructors

        public OutcomeDraft(int key, string text)
        {
            Key = key;
            Text = text;
        }

        #endregion

        #region Properties, Indexers

        public string? Error { get; set; }

        public int Key { get; }

        public string Text { get; set; }

        #endregion
    }

    /// <summary>A status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private sealed class StatusMessage
    {
        #region Fields and Constants

        private int count;

        private string? text;

        #endregion

        #region Methods

        #region Public Methods

        public void Clear()
        {
            count = 0;
            text = null;
            Message = null;
            IsError = false;
        }

        public void Set(string message, bool isError = false)
        {
            count = message == text ? count + 1 : 1;
            text = message;
            IsError = isError;
            Message = count == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({count})");
        }

        #endregion

        #endregion

        #region Properties, Indexers

        public bool IsError { get; private set; }

        public string? Message { get; private set; }

        #endregion
    }

    #endregion
}
