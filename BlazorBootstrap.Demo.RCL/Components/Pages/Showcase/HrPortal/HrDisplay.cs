using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Routes, labels, formatting, and focus helpers shared by the Teamhaven screens. Formatting uses the invariant culture, so
/// the Server and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class HrDisplay
{
    #region Methods

    #region Public Methods

    /// <summary>Formats how long ago a time was, like "just now", "3 h ago", "2 days ago", or "Oct 20".</summary>
    public static string Ago(DateTime time, DateTime now)
    {
        var age = now - time;

        return age.TotalMinutes < 1 ? "just now"
            : age.TotalMinutes < 60 ? FormattableString.Invariant($"{(int)age.TotalMinutes} min ago")
            : age.TotalHours < 24 ? FormattableString.Invariant($"{(int)age.TotalHours} h ago")
            : age.TotalDays < 7 ? ((int)age.TotalDays == 1 ? "yesterday" : FormattableString.Invariant($"{(int)age.TotalDays} days ago"))
            : time.ToString("MMM d", CultureInfo.InvariantCulture);
    }

    /// <summary>Formats a date like "Nov 24, 2026".</summary>
    public static string Date(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Formats a day count like "1 day" or "3 days".</summary>
    public static string Days(int days) => days == 1 ? "1 day" : FormattableString.Invariant($"{days} days");

    /// <summary>Returns the route of someone's directory profile.</summary>
    public static string EmployeeUrl(int employeeId) => FormattableString.Invariant($"{HrLayout.Base}/people/{employeeId}");

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

    /// <summary>Returns the Bootstrap color of a leave status (always shown with its name).</summary>
    public static string LeaveColor(HrLeaveStatus status) => status switch
    {
        HrLeaveStatus.Pending => "warning",
        HrLeaveStatus.Approved => "success",
        HrLeaveStatus.Rejected => "danger",
        _ => "secondary",
    };

    /// <summary>Returns a leave status in words.</summary>
    public static string LeaveLabel(HrLeaveStatus status) => status == HrLeaveStatus.Pending ? "Waiting for approval" : status.ToString();

    /// <summary>Formats a leave range like "Dec 21–23, 2026" or "Dec 28, 2026 – Jan 1, 2027".</summary>
    public static string Range(DateTime from, DateTime to) =>
        from.Date == to.Date ? Date(from)
        : from.Year == to.Year && from.Month == to.Month ? FormattableString.Invariant($"{from.ToString("MMM d", CultureInfo.InvariantCulture)}–{to.Day}, {to.Year}")
        : from.Year == to.Year ? FormattableString.Invariant($"{from.ToString("MMM d", CultureInfo.InvariantCulture)} – {to.ToString("MMM d", CultureInfo.InvariantCulture)}, {to.Year}")
        : $"{Date(from)} – {Date(to)}";

    /// <summary>Returns an onboarding task owner in words.</summary>
    public static string OwnerLabel(HrTaskOwner owner) => owner switch
    {
        HrTaskOwner.NewHire => "New hire",
        HrTaskOwner.Manager => "Manager",
        _ => "HR",
    };

    /// <summary>Returns someone's status in words (a joining person who has started counts as active).</summary>
    public static string StatusLabel(HrEmployee employee, DateTime today) => employee.Status switch
    {
        HrEmployeeStatus.Left => "Left",
        HrEmployeeStatus.Joining when employee.StartDate > today => "Joining " + employee.StartDate.ToString("MMM d", CultureInfo.InvariantCulture),
        _ => "Active",
    };

    /// <summary>Formats years and months of tenure, like "3 years, 2 months" or "3 weeks".</summary>
    public static string Tenure(DateTime start, DateTime today)
    {
        if (start > today)
            return "Not started";

        var months = ((today.Year - start.Year) * 12) + today.Month - start.Month - (today.Day < start.Day ? 1 : 0);

        if (months < 1)
        {
            var weeks = (int)((today - start).TotalDays / 7);

            return weeks <= 1 ? "Less than 2 weeks" : FormattableString.Invariant($"{weeks} weeks");
        }

        var years = months / 12;
        var rest = months % 12;

        return years == 0 ? Plural(rest, "month")
            : rest == 0 ? Plural(years, "year")
            : $"{Plural(years, "year")}, {Plural(rest, "month")}";
    }

    /// <summary>Formats a count with a noun, like "1 person" or "3 people" (with an explicit plural).</summary>
    public static string Plural(int count, string noun, string? plural = null) =>
        count == 1 ? $"1 {noun}" : string.Create(CultureInfo.InvariantCulture, $"{count} {plural ?? noun + "s"}");

    #endregion

    #endregion
}
