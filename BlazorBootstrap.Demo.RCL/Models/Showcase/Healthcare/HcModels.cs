namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;

/// <summary>Where an appointment is in the visit flow.</summary>
public enum HcAppointmentStatus
{
    /// <summary>Booked; the patient hasn't arrived.</summary>
    Booked,

    /// <summary>The front desk checked the patient in.</summary>
    CheckedIn,

    /// <summary>The doctor started the visit.</summary>
    InVisit,

    /// <summary>The visit is finished.</summary>
    Completed,

    /// <summary>Cancelled by the patient or the front desk.</summary>
    Cancelled,

    /// <summary>The patient didn't come.</summary>
    NoShow,
}

/// <summary>Whether a bill is paid.</summary>
public enum HcBillStatus
{
    /// <summary>Waiting for payment.</summary>
    Due,

    /// <summary>Paid with a test card.</summary>
    Paid,
}

/// <summary>Where a lab order is.</summary>
public enum HcLabStatus
{
    /// <summary>Ordered; no results yet.</summary>
    Ordered,

    /// <summary>Results entered by the doctor, not released to the patient yet.</summary>
    Resulted,

    /// <summary>Released: the patient sees the report.</summary>
    Released,
}

/// <summary>Whether a prescription is current.</summary>
public enum HcPrescriptionStatus
{
    /// <summary>Being taken.</summary>
    Active,

    /// <summary>Stopped by the doctor.</summary>
    Stopped,
}

/// <summary>The outcome of an action on the portal's state.</summary>
public enum HcResult
{
    /// <summary>Done.</summary>
    Ok,

    /// <summary>The signed-in account's role (or ownership) doesn't allow this.</summary>
    NotAllowed,

    /// <summary>The item doesn't exist.</summary>
    NotFound,

    /// <summary>A value is missing, too long, or out of range.</summary>
    Invalid,

    /// <summary>The item isn't in a state that allows this (for example, too late to cancel).</summary>
    WrongState,

    /// <summary>Nothing changed.</summary>
    NoChange,

    /// <summary>The time is no longer free (booked by someone else, or outside the doctor's hours).</summary>
    Unavailable,

    /// <summary>It overlaps something that already exists.</summary>
    Conflict,

    /// <summary>The test card was declined; nothing was paid.</summary>
    CardDeclined,
}

/// <summary>A clinic location.</summary>
public sealed record HcClinic(string Id, string Name, string Address, string Phone, string Hours, double Latitude, double Longitude)
{
    /// <summary>Gets the path of the clinic's photo (licensed asset; see <c>wwwroot/showcase/CREDITS.md</c>).</summary>
    public string PhotoPath => $"_content/BlazorBootstrap.Demo.RCL/showcase/healthcare/clinics/{Id}.webp";
}

/// <summary>One measurement of a lab panel, with its reference range.</summary>
public sealed record HcAnalyte(string Name, string Unit, double Low, double High);

/// <summary>A lab test the doctors can order.</summary>
public sealed record HcLabPanel(string Id, string Name, string Description, IReadOnlyList<HcAnalyte> Analytes);

/// <summary>A vaccination on record.</summary>
public sealed record HcImmunization(string Name, DateTime Date);

/// <summary>A visit at a clinic with a doctor.</summary>
public sealed class HcAppointment
{
    /// <summary>Gets or sets the reason the patient gave for cancelling, or the front desk's note.</summary>
    public string CancelReason { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was cancelled.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Gets or sets when the patient was checked in.</summary>
    public DateTime? CheckedInAt { get; set; }

    /// <summary>Gets or sets the clinic.</summary>
    public string ClinicId { get; set; } = string.Empty;

    /// <summary>Gets or sets when the visit was completed.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets when it was booked.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets the doctor.</summary>
    public string DoctorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the length in minutes.</summary>
    public int Minutes { get; set; } = 30;

    /// <summary>Gets or sets the doctor's visit notes (plain text, staff only).</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Gets or sets the patient.</summary>
    public int PatientId { get; init; }

    /// <summary>Gets or sets the reason for the visit, in the patient's words.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets when it starts.</summary>
    public DateTime Start { get; set; }

    /// <summary>Gets or sets where it is in the visit flow.</summary>
    public HcAppointmentStatus Status { get; set; }

    /// <summary>Gets or sets the visit summary the patient sees once the visit is completed (plain text).</summary>
    public string Summary { get; set; } = string.Empty;
}

/// <summary>A weekly block of time a doctor sees patients at a clinic.</summary>
public sealed class HcAvailability
{
    /// <summary>Gets or sets the clinic.</summary>
    public string ClinicId { get; set; } = string.Empty;

    /// <summary>Gets or sets the day of the week.</summary>
    public DayOfWeek Day { get; set; }

    /// <summary>Gets or sets the doctor.</summary>
    public string DoctorId { get; init; } = string.Empty;

    /// <summary>Gets or sets the end, in minutes after midnight.</summary>
    public int EndMinute { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the start, in minutes after midnight.</summary>
    public int StartMinute { get; set; }
}

/// <summary>A bill for a visit or a lab test.</summary>
public sealed class HcBill
{
    /// <summary>Gets or sets the amount in cents.</summary>
    public int AmountCents { get; init; }

    /// <summary>Gets or sets the appointment it is for, if any.</summary>
    public int? AppointmentId { get; init; }

    /// <summary>Gets or sets the brand of the test card that paid it.</summary>
    public string CardBrand { get; set; } = string.Empty;

    /// <summary>Gets or sets the last four digits of the test card that paid it.</summary>
    public string CardLast4 { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was issued.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets what it is for.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets or sets when it is due.</summary>
    public DateTime DueDate { get; init; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the lab order it is for, if any.</summary>
    public int? LabOrderId { get; init; }

    /// <summary>Gets or sets when it was paid.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Gets or sets the patient.</summary>
    public int PatientId { get; init; }

    /// <summary>Gets or sets whether it is paid.</summary>
    public HcBillStatus Status { get; set; }
}

/// <summary>A doctor at one of the clinics.</summary>
public sealed class HcDoctor
{
    /// <summary>Gets or sets whether they accept new patients.</summary>
    public bool AcceptingNewPatients { get; init; } = true;

    /// <summary>Gets or sets the demo account, or <see langword="null" /> for a doctor who can't sign in.</summary>
    public string? AccountId { get; init; }

    /// <summary>Gets or sets a short biography.</summary>
    public string Bio { get; init; } = string.Empty;

    /// <summary>Gets or sets the main clinic.</summary>
    public string ClinicId { get; init; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets or sets the initials for the avatar.</summary>
    public string Initials { get; init; } = string.Empty;

    /// <summary>Gets or sets the languages spoken.</summary>
    public IReadOnlyList<string> Languages { get; init; } = Array.Empty<string>();

    /// <summary>Gets or sets the name, like "Dr. Samuel Okoye".</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the path of the doctor's portrait (licensed asset; see <c>wwwroot/showcase/CREDITS.md</c>).</summary>
    public string PhotoPath => $"_content/BlazorBootstrap.Demo.RCL/showcase/healthcare/doctors/{Id}.webp";

    /// <summary>Gets or sets the specialty.</summary>
    public string Specialty { get; init; } = string.Empty;
}

/// <summary>A lab test ordered for a patient, with its results once entered.</summary>
public sealed class HcLabOrder
{
    /// <summary>Gets or sets the visit it was ordered in, if any.</summary>
    public int? AppointmentId { get; init; }

    /// <summary>Gets or sets the doctor's comment for the patient (plain text).</summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>Gets or sets the ordering doctor.</summary>
    public string DoctorId { get; init; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets when it was ordered.</summary>
    public DateTime OrderedAt { get; init; }

    /// <summary>Gets or sets the panel.</summary>
    public string PanelId { get; init; } = string.Empty;

    /// <summary>Gets or sets the patient.</summary>
    public int PatientId { get; init; }

    /// <summary>Gets or sets when it was released to the patient.</summary>
    public DateTime? ReleasedAt { get; set; }

    /// <summary>Gets or sets when results were entered.</summary>
    public DateTime? ResultedAt { get; set; }

    /// <summary>Gets or sets the values, in the panel's analyte order (empty until results are entered).</summary>
    public List<double> Values { get; set; } = new();

    /// <summary>Gets or sets where it is.</summary>
    public HcLabStatus Status { get; set; }
}

/// <summary>A patient of the clinics.</summary>
public sealed class HcPatient
{
    /// <summary>Gets or sets the demo account, or <see langword="null" /> for a patient who can't sign in.</summary>
    public string? AccountId { get; set; }

    /// <summary>Gets or sets known allergies (plain text, one per item).</summary>
    public List<string> Allergies { get; set; } = new();

    /// <summary>Gets or sets ongoing conditions (plain text, one per item).</summary>
    public List<string> Conditions { get; set; } = new();

    /// <summary>Gets or sets the date of birth (unknown for a new account until the patient adds it).</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID (shown as the medical record number).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets vaccinations on record.</summary>
    public List<HcImmunization> Immunizations { get; set; } = new();

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone number.</summary>
    public string Phone { get; set; } = string.Empty;
}

/// <summary>A medication prescribed to a patient.</summary>
public sealed class HcPrescription
{
    /// <summary>Gets or sets how to take it.</summary>
    public string Directions { get; init; } = string.Empty;

    /// <summary>Gets or sets the prescribing doctor.</summary>
    public string DoctorId { get; init; } = string.Empty;

    /// <summary>Gets or sets the dose, like "10 mg".</summary>
    public string Dose { get; init; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the medication.</summary>
    public string Medication { get; init; } = string.Empty;

    /// <summary>Gets or sets the patient.</summary>
    public int PatientId { get; init; }

    /// <summary>Gets or sets when it was prescribed.</summary>
    public DateTime PrescribedAt { get; init; }

    /// <summary>Gets or sets how many units per fill.</summary>
    public int Quantity { get; init; }

    /// <summary>Gets or sets the doctor's answer to the last refill request (plain text).</summary>
    public string RefillNote { get; set; } = string.Empty;

    /// <summary>Gets or sets when the patient asked for a refill that is still waiting for the doctor.</summary>
    public DateTime? RefillRequestedAt { get; set; }

    /// <summary>Gets or sets the refills left.</summary>
    public int Refills { get; set; }

    /// <summary>Gets or sets whether it is current.</summary>
    public HcPrescriptionStatus Status { get; set; }
}

/// <summary>A period a doctor is away (no appointments can be booked).</summary>
public sealed class HcTimeOff
{
    /// <summary>Gets or sets the doctor.</summary>
    public string DoctorId { get; init; } = string.Empty;

    /// <summary>Gets or sets the first day away.</summary>
    public DateTime From { get; set; }

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the reason (plain text, staff only).</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the last day away.</summary>
    public DateTime To { get; set; }
}
