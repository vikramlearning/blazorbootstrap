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

/// <summary>The doctor's answer to a prescription's last refill request.</summary>
public enum HcRefillOutcome
{
    /// <summary>No request was answered yet.</summary>
    None,

    /// <summary>Approved: one refill was added.</summary>
    Approved,

    /// <summary>Denied, with a note for the patient.</summary>
    Denied,
}

/// <summary>The kind of a visit, which sets its length.</summary>
public enum HcVisitType
{
    /// <summary>A visit with a doctor the patient has seen before, or a new problem: <see cref="HcVisitTypes.StandardMinutes" /> minutes.</summary>
    Standard,

    /// <summary>A first visit with a doctor, with the history taken: <see cref="HcVisitTypes.NewPatientMinutes" /> minutes.</summary>
    NewPatient,
}

/// <summary>The lengths of the visit types.</summary>
public static class HcVisitTypes
{
    /// <summary>The length of a standard visit, in minutes (one slot).</summary>
    public const int StandardMinutes = 30;

    /// <summary>The length of a new patient visit, in minutes (two slots).</summary>
    public const int NewPatientMinutes = 60;

    /// <summary>Returns the length of a visit type, in minutes.</summary>
    public static int MinutesOf(HcVisitType type) => type == HcVisitType.NewPatient ? NewPatientMinutes : StandardMinutes;

    /// <summary>Returns a visit type in words, like "New patient visit".</summary>
    public static string Label(HcVisitType type) => type == HcVisitType.NewPatient ? "New patient visit" : "Visit";
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

    /// <summary>A key value (like a medical record number) already belongs to another record.</summary>
    Duplicate,
}

/// <summary>A clinic location.</summary>
public sealed record HcClinic(string Id, string Name, string Address, string Phone, string Hours, double Latitude, double Longitude)
{
    /// <summary>Gets when the clinic closes on its open days, in minutes after midnight (the end of <see cref="Hours" />).</summary>
    public int CloseMinute { get; init; } = 17 * 60;

    /// <summary>Gets when the clinic opens on its open days, in minutes after midnight (the start of <see cref="Hours" />).</summary>
    public int OpenMinute { get; init; } = 8 * 60;

    /// <summary>Gets the path of the clinic's photo (licensed asset; see <c>wwwroot/showcase/CREDITS.md</c>).</summary>
    public string PhotoPath => $"_content/BlazorBootstrap.Demo.RCL/showcase/healthcare/clinics/{Id}.webp";

    /// <summary>Returns whether the clinic is open on a day of the week (Monday to Friday).</summary>
    public static bool IsOpenDay(DayOfWeek day) => day is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
}

/// <summary>A health plan patients can have on file (fictitious), with the letters its member IDs start with.</summary>
public sealed record HcPayer(string Id, string Name, string MemberIdPrefix);

/// <summary>A pharmacy patients can pick up refills at (fictitious).</summary>
public sealed record HcPharmacy(string Id, string Name, string Address);

/// <summary>The work details of a staff account: a phone number, and for the front desk the clinic they work at.</summary>
public sealed class HcStaffProfile
{
    /// <summary>Gets or sets the staff account.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets or sets the clinic the front desk works at (and gets new bookings for); a doctor's follows their record.</summary>
    public string ClinicId { get; set; } = string.Empty;

    /// <summary>Gets or sets the direct work phone, or empty.</summary>
    public string WorkPhone { get; set; } = string.Empty;
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

    /// <summary>
    /// Gets or sets the diagnosis codes of the visit, for billing a health plan later (empty for now: visit coding and
    /// claims are not part of the portal yet).
    /// </summary>
    public List<string> DiagnosisCodes { get; set; } = new();

    /// <summary>Gets or sets the doctor.</summary>
    public string DoctorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID (shown as the appointment number).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the length in minutes (<see cref="HcVisitTypes.MinutesOf" /> of <see cref="VisitType" />).</summary>
    public int Minutes { get; set; } = 30;

    /// <summary>Gets or sets the procedure codes of the visit, for billing a health plan later (empty for now, like <see cref="DiagnosisCodes" />).</summary>
    public List<string> ProcedureCodes { get; set; } = new();

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

    /// <summary>Gets or sets the kind of visit, which sets <see cref="Minutes" />.</summary>
    public HcVisitType VisitType { get; set; }
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
    /// <summary>Gets or sets whether they accept new patients (the doctor changes it on their account page).</summary>
    public bool AcceptingNewPatients { get; set; } = true;

    /// <summary>Gets or sets the demo account, or <see langword="null" /> for a doctor who can't sign in.</summary>
    public string? AccountId { get; init; }

    /// <summary>Gets or sets a short biography (the doctor changes it on their account page).</summary>
    public string Bio { get; set; } = string.Empty;

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

    /// <summary>Gets or sets the home clinic, where the front desk looks after the patient's bookings.</summary>
    public string ClinicId { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the person to call in an emergency, or empty.</summary>
    public string EmergencyContactName { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone number of the emergency contact, or empty.</summary>
    public string EmergencyContactPhone { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID (internal; people know the patient by <see cref="Mrn" />).</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets vaccinations on record.</summary>
    public List<HcImmunization> Immunizations { get; set; } = new();

    /// <summary>Gets or sets the member ID on the health plan card, or empty for self-pay.</summary>
    public string InsuranceMemberId { get; set; } = string.Empty;

    /// <summary>Gets or sets the health plan (<see cref="HcPayer.Id" />), or empty for self-pay; claims to it are not part of the portal yet.</summary>
    public string InsurancePayerId { get; set; } = string.Empty;

    /// <summary>Gets or sets the medical record number, like "CL-100001": unique, and what staff and the patient quote.</summary>
    public string Mrn { get; set; } = string.Empty;

    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone number.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets when the patient was registered.</summary>
    public DateTime RegisteredAt { get; init; }
}

/// <summary>A medication prescribed to a patient.</summary>
public sealed class HcPrescription
{
    /// <summary>Gets or sets how many days one fill lasts.</summary>
    public int DaysSupply { get; init; } = 30;

    /// <summary>Gets or sets how to take it.</summary>
    public string Directions { get; init; } = string.Empty;

    /// <summary>Gets or sets the prescribing doctor.</summary>
    public string DoctorId { get; init; } = string.Empty;

    /// <summary>Gets or sets the dose, like "10 mg".</summary>
    public string Dose { get; init; } = string.Empty;

    /// <summary>Gets or sets the ID.</summary>
    public int Id { get; init; }

    /// <summary>Gets or sets the day the pharmacy last filled it (the prescribing day for the first fill).</summary>
    public DateTime LastFilledOn { get; set; }

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

    /// <summary>Gets or sets the doctor's answer to the last refill request.</summary>
    public HcRefillOutcome RefillOutcome { get; set; }

    /// <summary>Gets or sets the day the waiting refill request asks to pick it up, or <see langword="null" />.</summary>
    public DateTime? RefillPickupOn { get; set; }

    /// <summary>Gets or sets the pharmacy (<see cref="HcPharmacy.Id" />) of the waiting refill request, or empty.</summary>
    public string RefillPharmacyId { get; set; } = string.Empty;

    /// <summary>Gets or sets the quantity the waiting refill request asks for, or 0.</summary>
    public int RefillQuantity { get; set; }

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

/// <summary>The outcome of a bulk change from a list: the result, and how many records changed and how many were left as they were.</summary>
public sealed record HcBulkOutcome(HcResult Result, int Changed, int Unchanged);

/// <summary>
/// A patient's registration details as the staff form enters them (<c>HcState.SavePatient</c>): who they are, how to reach
/// them and their emergency contact, their home clinic, and their health plan.
/// </summary>
public sealed record HcPatientInput(
    string Mrn,
    string Name,
    DateTime DateOfBirth,
    string Phone,
    string Email,
    string ClinicId,
    string EmergencyContactName,
    string EmergencyContactPhone,
    string InsurancePayerId,
    string InsuranceMemberId);
