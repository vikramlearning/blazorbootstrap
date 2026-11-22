using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Routes, labels, formatting, and focus helpers shared by the Tandem screens. Formatting uses the invariant culture, so the
/// Server and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class PbDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Formats how long ago a time was, like "just now", "5 min ago", "3 h ago", "2 days ago", or "Oct 20".</summary>
    public static string Ago(DateTime time, DateTime now)
    {
        var age = now - time;

        return age.TotalMinutes < 1 ? "just now"
            : age.TotalMinutes < 60 ? FormattableString.Invariant($"{(int)age.TotalMinutes} min ago")
            : age.TotalHours < 24 ? FormattableString.Invariant($"{(int)age.TotalHours} h ago")
            : age.TotalDays < 7 ? ((int)age.TotalDays == 1 ? "yesterday" : FormattableString.Invariant($"{(int)age.TotalDays} days ago"))
            : time.ToString("MMM d", CultureInfo.InvariantCulture);
    }

    /// <summary>Returns the route of a client.</summary>
    public static string ClientUrl(int clientId) => FormattableString.Invariant($"{PbLayout.Base}/clients/{clientId}");

    /// <summary>Formats a date like "Nov 24, 2026".</summary>
    public static string Date(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Describes a due date against today, like "Overdue by 2 days", "Due today", "Due tomorrow", or "Due Dec 3".</summary>
    public static string Due(DateTime? due, DateTime today)
    {
        if (due is not { } date)
            return "No due date";

        var days = (date.Date - today.Date).Days;

        return days switch
        {
            < -1 => FormattableString.Invariant($"Overdue by {-days} days"),
            -1 => "Overdue by 1 day",
            0 => "Due today",
            1 => "Due tomorrow",
            _ => "Due " + date.ToString("MMM d", CultureInfo.InvariantCulture),
        };
    }

    /// <summary>Returns whether an open task's due date has passed.</summary>
    public static bool IsOverdue(PbTask task, DateTime today) => task.Status != PbTaskStatus.Done && task.DueDate is { } due && due.Date < today.Date;

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

    /// <summary>Formats dollars like "$85,000".</summary>
    public static string Money(long dollars) => dollars.ToString("$#,0", CultureInfo.InvariantCulture);

    /// <summary>Formats dollars compactly like "$85k" or "$1.2M".</summary>
    public static string MoneyShort(long dollars) => dollars >= 1_000_000
        ? "$" + (dollars / 1_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + "M"
        : dollars >= 1_000 ? "$" + (dollars / 1_000d).ToString("0.#", CultureInfo.InvariantCulture) + "k" : Money(dollars);

    /// <summary>Returns the Bootstrap color of a priority (always shown with its name).</summary>
    public static string PriorityColor(PbTaskPriority priority) => priority switch
    {
        PbTaskPriority.Urgent => "danger",
        PbTaskPriority.High => "warning",
        PbTaskPriority.Medium => "info",
        _ => "secondary",
    };

    /// <summary>Returns the route of a project's board.</summary>
    public static string ProjectUrl(int projectId) => FormattableString.Invariant($"{PbLayout.Base}/projects/{projectId}");

    /// <summary>Returns the Bootstrap color of a project status (always shown with its name).</summary>
    public static string ProjectStatusColor(PbProjectStatus status) => status switch
    {
        PbProjectStatus.Active => "success",
        PbProjectStatus.Planned => "info",
        PbProjectStatus.OnHold => "warning",
        _ => "secondary",
    };

    /// <summary>Returns a project status in words.</summary>
    public static string ProjectStatusLabel(PbProjectStatus status) => status switch
    {
        PbProjectStatus.OnHold => "On hold",
        _ => status.ToString(),
    };

    /// <summary>Returns a deal stage in words.</summary>
    public static string StageLabel(PbDealStage stage) => stage.ToString();

    /// <summary>Returns a task column in words.</summary>
    public static string StatusLabel(PbTaskStatus status) => status switch
    {
        PbTaskStatus.Todo => "To do",
        PbTaskStatus.InProgress => "In progress",
        PbTaskStatus.Review => "In review",
        _ => "Done",
    };

    /// <summary>Returns the route of a task's detail page.</summary>
    public static string TaskUrl(int taskId) => FormattableString.Invariant($"{PbLayout.Base}/tasks/{taskId}");

    #endregion

    /// <summary>Formats a count with its noun, like "1 ticket" or "1,204 tickets"; <paramref name="plural" /> defaults to the noun plus "s".</summary>
    public static string Plural(int count, string noun, string? plural = null) =>
        count == 1 ? $"1 {noun}" : $"{count.ToString("#,0", CultureInfo.InvariantCulture)} {plural ?? noun + "s"}";

    #endregion
}
