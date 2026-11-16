using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Routes, labels, formatting, and focus helpers shared by the Cedarline Health screens. Formatting uses the invariant
/// culture, so the Server and WebAssembly hosts show the same text whatever the browser locale.
/// </summary>
public static class HcDisplay
{
    #region Fields and Constants

    /// <summary>The query-string date format (<c>yyyy-MM-dd</c>).</summary>
    public const string QueryDate = "yyyy-MM-dd";

    /// <summary>The query-string date and time format of a slot (<c>yyyy-MM-ddTHH:mm</c>).</summary>
    public const string QuerySlot = "yyyy-MM-dd'T'HH:mm";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns a patient's age in whole years on a day, or <see langword="null" /> when the date of birth is unknown.</summary>
    public static int? Age(DateTime? dateOfBirth, DateTime today)
    {
        if (dateOfBirth is not { } dob)
            return null;

        var age = today.Year - dob.Year;

        return dob.Date > today.AddYears(-age) ? age - 1 : age;
    }

    /// <summary>Returns the route of a patient's appointment page.</summary>
    public static string AppointmentUrl(int appointmentId) => FormattableString.Invariant($"{HcLayout.Base}/appointments/{appointmentId}");

    /// <summary>Returns the booking route for a doctor at an open time.</summary>
    public static string BookUrl(string doctorId, DateTime start) =>
        $"{HcLayout.Base}/book/{Uri.EscapeDataString(doctorId)}?time={start.ToString(QuerySlot, CultureInfo.InvariantCulture)}";

    /// <summary>Formats a date like "Tue, Nov 24".</summary>
    public static string Day(DateTime date) => date.ToString("ddd, MMM d", CultureInfo.InvariantCulture);

    /// <summary>Formats a date like "Tuesday, November 24, 2026".</summary>
    public static string DayLong(DateTime date) => date.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns the route of a doctor's profile.</summary>
    public static string DoctorUrl(string doctorId) => $"{HcLayout.Base}/doctors/{Uri.EscapeDataString(doctorId)}";

    /// <summary>Formats a date like "Nov 24, 2026".</summary>
    public static string Date(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

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

    /// <summary>Formats minutes after midnight like "08:30".</summary>
    public static string Minutes(int minuteOfDay) => FormattableString.Invariant($"{minuteOfDay / 60:00}:{minuteOfDay % 60:00}");

    /// <summary>Formats cents like "$40.00".</summary>
    public static string Money(int cents) => (cents / 100m).ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Formats a lab value without trailing zeros, like "6.2" or "248".</summary>
    public static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Returns the route of a released lab report.</summary>
    public static string LabUrl(int orderId) => FormattableString.Invariant($"{HcLayout.Base}/lab-reports/{orderId}");

    /// <summary>Returns the staff route of a patient's chart.</summary>
    public static string PatientChartUrl(int patientId) => FormattableString.Invariant($"{HcStaffLayout.Base}/patients/{patientId}");

    /// <summary>Returns the staff route of a visit.</summary>
    public static string StaffAppointmentUrl(int appointmentId) => FormattableString.Invariant($"{HcStaffLayout.Base}/appointments/{appointmentId}");

    /// <summary>Returns the Bootstrap color of an appointment status (always shown with its name).</summary>
    public static string StatusColor(HcAppointmentStatus status) => status switch
    {
        HcAppointmentStatus.Booked => "primary",
        HcAppointmentStatus.CheckedIn => "info",
        HcAppointmentStatus.InVisit => "warning",
        HcAppointmentStatus.Completed => "success",
        _ => "secondary",
    };

    /// <summary>Returns an appointment status in words.</summary>
    public static string StatusLabel(HcAppointmentStatus status) => status switch
    {
        HcAppointmentStatus.CheckedIn => "Checked in",
        HcAppointmentStatus.InVisit => "With the doctor",
        HcAppointmentStatus.NoShow => "No-show",
        _ => status.ToString(),
    };

    /// <summary>Formats a time like "09:30".</summary>
    public static string Time(DateTime time) => time.ToString("HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Formats a date and time like "Tue, Nov 24 at 09:30".</summary>
    public static string When(DateTime time) => time.ToString("ddd, MMM d 'at' HH:mm", CultureInfo.InvariantCulture);

    #endregion

    #endregion
}
