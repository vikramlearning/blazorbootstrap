using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Formatting and focus helpers shared by the Quillpath screens. Formatting uses the invariant culture, so the Server
/// and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class LearningDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Returns the route of a course page.</summary>
    public static string CourseUrl(int courseId) => $"{LearningLayout.Base}/courses/{courseId}";

    /// <summary>Formats a date like "Nov 24, 2026".</summary>
    public static string Date(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Formats a length in minutes like "2 h 05 min" or "45 min".</summary>
    public static string Duration(int minutes) => minutes < 60
        ? FormattableString.Invariant($"{minutes} min")
        : FormattableString.Invariant($"{minutes / 60} h {minutes % 60:00} min");

    /// <summary>Moves focus to an element, ignoring one that has left the page or a closed circuit (WCAG 2.4.3).</summary>
    public static async Task FocusAsync(ElementReference? element)
    {
        if (element is not ElementReference target)
            return;

        try
        {
            await target.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Nothing left to focus.
        }
    }

    /// <summary>Returns the icon of a lesson kind.</summary>
    public static IconName KindIcon(LearningLessonKind kind) => kind switch
    {
        LearningLessonKind.Video => IconName.PlayCircle,
        LearningLessonKind.Reading => IconName.FileText,
        _ => IconName.PatchQuestion,
    };

    /// <summary>Returns the label of a lesson kind.</summary>
    public static string KindLabel(LearningLessonKind kind) => kind switch
    {
        LearningLessonKind.Video => "Video",
        LearningLessonKind.Reading => "Reading",
        _ => "Self-check",
    };

    /// <summary>Returns the route of a lesson in the player.</summary>
    public static string LessonUrl(int courseId, int lessonId) => $"{CourseUrl(courseId)}/learn/{lessonId}";

    /// <summary>Returns the badge color of a level; the level name is always shown too.</summary>
    public static BadgeColor LevelColor(LearningLevel level) => level switch
    {
        LearningLevel.Beginner => BadgeColor.Success,
        LearningLevel.Intermediate => BadgeColor.Info,
        _ => BadgeColor.Dark,
    };

    /// <summary>Formats an average rating like "4.5".</summary>
    public static string Rating(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>Returns the star icons for an average rating, rounded to the nearest half star.</summary>
    public static IEnumerable<IconName> Stars(double rating)
    {
        var halves = (int)Math.Round(rating * 2, MidpointRounding.AwayFromZero);

        for (var star = 1; star <= 5; star++)
            yield return halves >= star * 2 ? IconName.StarFill : halves == (star * 2) - 1 ? IconName.StarHalf : IconName.Star;
    }

    #endregion

    #endregion
}
