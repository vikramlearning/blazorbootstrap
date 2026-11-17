using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Routes, labels, form checks, and the onboarding outcome shared by Teamhaven's HR admin pages (employees, the employee
/// editor, leave policies, onboarding templates, and headcount analytics). Formatting uses the invariant culture.
/// </summary>
internal static partial class AdminHelpers
{
    #region Fields and Constants

    /// <summary>The route of headcount analytics.</summary>
    internal const string AnalyticsUrl = HrLayout.Base + "/admin/analytics";

    /// <summary>The route of the employees list.</summary>
    internal const string EmployeesUrl = HrLayout.Base + "/admin/employees";

    /// <summary>The route of the new-employee editor.</summary>
    internal const string NewEmployeeUrl = EmployeesUrl + "/new";

    /// <summary>The route of the managers' and HR's new-hires page, where onboarding progress is followed.</summary>
    internal const string NewHiresUrl = HrLayout.Base + "/team/onboarding";

    /// <summary>The route of the leave policies.</summary>
    internal const string PoliciesUrl = HrLayout.Base + "/admin/leave-policies";

    /// <summary>The route of the onboarding templates.</summary>
    internal const string TemplatesUrl = HrLayout.Base + "/admin/onboarding-templates";

    #endregion

    #region Methods

    #region Internal Methods

    /// <summary>Returns a character counter, like "12 of 80 characters" (or how far over the limit it is).</summary>
    internal static string Counter(string value, int max)
    {
        var length = value.Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    /// <summary>Checks a date field: empty, not a date, or outside the range the portal keeps (2015 to three years ahead).</summary>
    internal static string? DateProblem(string text, string what, out DateTime value)
    {
        if (TryDate(text) is not { } date)
        {
            value = default;

            return string.IsNullOrWhiteSpace(text) ? $"Pick the {what}." : $"Enter the {what} as a date, like {HrDisplay.Date(HrSeedData.Now)}.";
        }

        value = date;

        return date < MinDate || date > MaxDate ? $"Pick a {what} from {HrDisplay.Date(MinDate)} to {HrDisplay.Date(MaxDate)}." : null;
    }

    /// <summary>Formats a date for a native date input (<c>yyyy-MM-dd</c>).</summary>
    internal static string DateValue(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// Suggests a template for someone: one named after their department (like "Engineering new hire"), else a "General"
    /// one, else the first with tasks; <see langword="null" /> when no template has tasks.
    /// </summary>
    internal static HrOnboardingTemplate? DefaultTemplate(HrState state, HrEmployee? employee)
    {
        var usable = state.Templates.Where(template => template.Tasks.Count > 0).ToList();
        var department = employee is null ? null : state.FindDepartment(employee.DepartmentId)?.Name;

        return (department is null ? null : usable.FirstOrDefault(template => template.Name.Contains(department, StringComparison.OrdinalIgnoreCase)))
            ?? usable.FirstOrDefault(template => template.Name.Contains("General", StringComparison.OrdinalIgnoreCase))
            ?? usable.FirstOrDefault();
    }

    /// <summary>Returns the route of someone's record in the employee editor.</summary>
    internal static string EditEmployeeUrl(int employeeId) => FormattableString.Invariant($"{EmployeesUrl}/{employeeId}/edit");

    /// <summary>Returns someone's status today: left, joining (starts after today), or active.</summary>
    internal static HrEmployeeStatus EffectiveStatus(HrEmployee employee, DateTime today) =>
        employee.Status == HrEmployeeStatus.Left ? HrEmployeeStatus.Left : employee.StartDate.Date > today ? HrEmployeeStatus.Joining : HrEmployeeStatus.Active;

    /// <summary>Whether a text is a work email the portal accepts (the same pattern as <see cref="HrState.SaveEmployee" />).</summary>
    internal static bool IsEmail(string value) => value.Length <= MaxEmailLength && EmailPattern().IsMatch(value);

    /// <summary>Returns a template task's due day, like "3 days before day one", "Day one", or "Day 14".</summary>
    internal static string OffsetLabel(int offset) => offset switch
    {
        0 => "Day one",
        < 0 => $"{HrDisplay.Days(-offset)} before day one",
        _ => string.Create(CultureInfo.InvariantCulture, $"Day {offset}"),
    };

    /// <summary>Returns a template task's due day as a full phrase, like "3 days before day one" or "14 days after day one".</summary>
    internal static string OffsetPhrase(int offset) => offset switch
    {
        0 => "On day one (the start date)",
        < 0 => $"{HrDisplay.Days(-offset)} before day one",
        _ => $"{HrDisplay.Days(offset)} after day one",
    };

    /// <summary>Says who does a template's tasks, like "4 for the new hire, 3 for the manager, 3 for HR".</summary>
    internal static string OwnerSplit(IEnumerable<HrTaskOwner> owners)
    {
        var list = owners.ToList();
        var parts = new List<string>();

        foreach (var (owner, label) in new[] { (HrTaskOwner.NewHire, "the new hire"), (HrTaskOwner.Manager, "the manager"), (HrTaskOwner.Hr, "HR") })
        {
            var count = list.Count(item => item == owner);

            if (count > 0)
                parts.Add(string.Create(CultureInfo.InvariantCulture, $"{count} for {label}"));
        }

        return string.Join(", ", parts);
    }

    /// <summary>Returns a share as a percentage from 0 to 100 (0 when there is nothing to count).</summary>
    internal static double Percent(int part, int whole) => whole <= 0 ? 0 : Math.Clamp(part * 100.0 / whole, 0, 100);

    /// <summary>Returns plan progress in words, like "3 of 10 tasks done".</summary>
    internal static string ProgressText(int done, int total) =>
        total == 0 ? "No tasks" : string.Create(CultureInfo.InvariantCulture, $"{done} of {HrDisplay.Plural(total, "task")} done");

    /// <summary>
    /// Says what <see cref="HrState.StartOnboarding" /> did, for the status region: whether it worked, and the message.
    /// </summary>
    internal static (bool Ok, string Message) StartOnboardingOutcome(HrState state, HrResult result, HrEmployee employee, HrOnboardingTemplate? template, HrOnboarding? plan)
    {
        var today = state.Clock.Today;

        switch (result)
        {
            case HrResult.Ok when plan is not null:
                var first = plan.Tasks.OrderBy(task => task.Due).FirstOrDefault();
                var overdue = plan.Tasks.Count(task => task.Due < today);

                return (true, $"Started “{plan.TemplateName}” onboarding for {employee.Name}: {HrDisplay.Plural(plan.Tasks.Count, "task")}"
                              + (first is null ? "." : $", the first due {HrDisplay.Date(first.Due)}.")
                              + (overdue == 0 ? null : $" {HrDisplay.Plural(overdue, "task is", "tasks are")} already past due, because they started on {HrDisplay.Date(employee.StartDate)}.")
                              + " Their manager follows it on New hires, and they see the checklist once they sign in.");

            case HrResult.Duplicate:
                return (false, state.OnboardingOf(employee.Id) is { } existing
                    ? $"{employee.Name} already has an onboarding plan (“{existing.TemplateName}”, {ProgressText(HrState.Progress(existing).Done, existing.Tasks.Count)}), so nothing changed."
                    : $"{employee.Name} already has an onboarding plan, so nothing changed.");

            case HrResult.WrongState:
                return (false, employee.Status == HrEmployeeStatus.Left
                    ? $"{employee.Name} has left, so their onboarding can't start."
                    : $"“{template?.Name ?? "That template"}” has no tasks yet. Add tasks to it, or pick another template.");

            case HrResult.NotFound:
                return (false, "That person or template was removed, so onboarding didn't start.");

            case HrResult.NotAllowed:
                return (false, "Only the HR admin can start onboarding.");

            case HrResult.Invalid:
                return (false, "Onboarding couldn't start: this demo tab holds the most onboarding plans it can, or its storage is full.");

            default:
                return (false, "That didn't work, and onboarding didn't start. Try again.");
        }
    }

    /// <summary>Returns someone's status in words, like "Active", "Joining Dec 1", or "Left Aug 28".</summary>
    internal static string StatusText(HrEmployee employee, DateTime today) => EffectiveStatus(employee, today) switch
    {
        HrEmployeeStatus.Left => employee.EndDate is { } end ? "Left " + ShortDate(end, today) : "Left",
        HrEmployeeStatus.Joining => "Joining " + ShortDate(employee.StartDate, today),
        _ => "Active",
    };

    /// <summary>Checks a required text field: missing, or longer than the limit.</summary>
    internal static string? TextProblem(string value, int max, string missing, string what)
    {
        var length = value.Trim().Length;

        return length == 0 ? missing
            : length > max ? string.Create(CultureInfo.InvariantCulture, $"Shorten the {what} to {max} characters or fewer (now {length}).")
            : null;
    }

    /// <summary>Returns the template's tasks in words, like "Engineering new hire (10 tasks)".</summary>
    internal static string TemplateOption(HrOnboardingTemplate template) =>
        template.Tasks.Count == 0 ? $"{template.Name} (no tasks yet)" : $"{template.Name} ({HrDisplay.Plural(template.Tasks.Count, "task")})";

    /// <summary>Returns the route of an onboarding template's page.</summary>
    internal static string TemplateUrl(int templateId) => FormattableString.Invariant($"{TemplatesUrl}/{templateId}");

    /// <summary>Reads a native date input's value (<c>yyyy-MM-dd</c>), or <see langword="null" /> while it's empty or half-typed.</summary>
    internal static DateTime? TryDate(string? text) =>
        DateTime.TryParseExact(text?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

    /// <summary>Reads a whole number typed in a native number input, or <see langword="null" />.</summary>
    internal static int? TryNumber(string? text) =>
        int.TryParse(text?.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) ? value : null;

    #endregion

    #region Private Methods

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

    /// <summary>Formats a date like "Dec 1", adding the year when it isn't this year.</summary>
    private static string ShortDate(DateTime date, DateTime today) =>
        date.Year == today.Year ? date.ToString("MMM d", CultureInfo.InvariantCulture) : HrDisplay.Date(date);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the longest work email the sign-in accepts.</summary>
    internal static int MaxEmailLength => BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation.ShowcaseIdentity.MaxEmailLength;

    /// <summary>Gets the latest date the portal keeps (three years after its start).</summary>
    internal static DateTime MaxDate => HrSeedData.Now.AddYears(3).Date;

    /// <summary>Gets the earliest date the portal keeps.</summary>
    internal static DateTime MinDate => new(2015, 1, 1);

    #endregion
}
