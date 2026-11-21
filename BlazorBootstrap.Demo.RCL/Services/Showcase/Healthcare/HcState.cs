using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>An open appointment time with a doctor, at the clinic of that block of hours.</summary>
public sealed record HcSlot(DateTime Start, string ClinicId);

/// <summary>One look at a patient's record by a staff member, for the patient's "Who viewed my record" list.</summary>
/// <param name="At">When it happened.</param>
/// <param name="Actor">The staff member's name.</param>
/// <param name="What">What they opened, in words, like "Viewed your chart".</param>
/// <param name="Role">Their role and the context, like "Doctor, before the visit".</param>
public sealed record HcRecordAccess(DateTime At, string Actor, string What, string? Role);

/// <summary>
/// State of "Cedarline Health", a fictitious clinic network's portal, for the current browser tab: patients and their
/// appointments, records, lab orders, prescriptions, and bills; doctors' weekly hours and time off; staff work details; the
/// audit log and notifications; and the demo sign-in. It is kept in the tab's <c>sessionStorage</c> through the showcase
/// foundation (only what differs from the deterministic seed); nothing is sent anywhere, and nothing is medical advice.
/// </summary>
/// <remarks>
/// Rights come from the identity role. A patient books, reschedules, and cancels their own appointments (open times come
/// from the doctor's weekly hours, less time off and booked times), requests refills, keeps their details, and pays bills
/// with a test card. A doctor runs their visits (start, notes, labs, prescriptions, complete — which bills the visit),
/// enters and releases lab results (which bills the test), edits charts, and answers refill requests. The front desk
/// registers patients, checks them in, marks no-shows, cancels, and keeps doctors' hours and time off. Every change is
/// audited, and so is every look at a patient's chart (<see cref="RecordChartView" />), as clinical systems log access.
/// </remarks>
public sealed partial class HcState : IShowcaseEnterpriseState
{
    #region Fields and Constants

    /// <summary>How many days ahead patients can book.</summary>
    public const int BookingDays = 60;

    /// <summary>The latest a block of hours may end (20:00), in minutes after midnight.</summary>
    public const int DayEndMinute = 20 * 60;

    /// <summary>The earliest a block of hours may start (7:00), in minutes after midnight.</summary>
    public const int DayStartMinute = 7 * 60;

    /// <summary>The oldest a patient's date of birth may make them, in years.</summary>
    public const int MaxAgeYears = 120;

    /// <summary>The longest doctor's biography.</summary>
    public const int MaxBioLength = 600;

    /// <summary>The longest lab comment.</summary>
    public const int MaxCommentLength = 1000;

    /// <summary>The longest prescription directions.</summary>
    public const int MaxDirectionsLength = 200;

    /// <summary>The longest dose.</summary>
    public const int MaxDoseLength = 40;

    /// <summary>The longest list item (an allergy or a condition).</summary>
    public const int MaxListItemLength = 80;

    /// <summary>The most allergies or conditions on a chart.</summary>
    public const int MaxListItems = 20;

    /// <summary>The longest medication name.</summary>
    public const int MaxMedicationLength = 60;

    /// <summary>The longest visit note.</summary>
    public const int MaxNotesLength = 4000;

    /// <summary>The most patients the tab keeps (the seed, those the front desk registers, and registered accounts).</summary>
    public const int MaxPatients = HcSeedData.PatientCount + 200 + ShowcaseIdentity.MaxAccounts;

    /// <summary>The longest phone number.</summary>
    public const int MaxPhoneLength = 30;

    /// <summary>The most days one fill of a prescription may last.</summary>
    public const int MaxDaysSupply = 90;

    /// <summary>The highest prescription quantity.</summary>
    public const int MaxQuantity = 365;

    /// <summary>The longest visit reason, cancel reason, time-off reason, or refill answer.</summary>
    public const int MaxReasonLength = 200;

    /// <summary>How many days ahead a refill can be asked for.</summary>
    public const int MaxRefillPickupDays = 30;

    /// <summary>The most refills on a prescription.</summary>
    public const int MaxRefills = 11;

    /// <summary>The longest visit summary for the patient.</summary>
    public const int MaxSummaryLength = 2000;

    /// <summary>The longest time off, in days.</summary>
    public const int MaxTimeOffDays = 60;

    /// <summary>The most upcoming appointments a patient can hold.</summary>
    public const int MaxUpcoming = 5;

    /// <summary>The shortest doctor's biography.</summary>
    public const int MinBioLength = 20;

    /// <summary>The shortest reason for denying a refill.</summary>
    public const int MinDenyReasonLength = 5;

    /// <summary>The shortest reason for a visit.</summary>
    public const int MinVisitReasonLength = 3;

    /// <summary>The length of an appointment slot, in minutes (a new patient visit takes two).</summary>
    public const int SlotMinutes = HcVisitTypes.StandardMinutes;

    private const int MaxAppointments = 6000;

    /// <summary>The most audit entries a tab adds to the seeded log; older added ones give way, so the snapshot stays within its limit.</summary>
    private const int MaxAddedAudit = 800;

    private const int MaxAudit = 6000;

    /// <summary>The longest audit text (action, record, details).</summary>
    private const int MaxAuditTextLength = 300;

    private const int MaxAvailability = 150;

    /// <summary>The longest bill description (a visit's is clipped to it).</summary>
    private const int MaxBillDescriptionLength = 200;

    private const int MaxBills = 8000;

    /// <summary>The most diagnosis or procedure codes a visit keeps.</summary>
    private const int MaxCodes = 12;

    private const int MaxLabOrders = 3000;

    private const int MaxNotifications = 400;

    private const int MaxPrescriptions = 3000;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int MaxTimeOff = 80;

    private const int NextIdLimit = 1_000_000;

    /// <summary>The fresh tab's data, built once and never changed: a snapshot keeps only what differs from it.</summary>
    private static readonly Lazy<HcSeedSet> seed = new(HcSeedData.Build);

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<HcAppointment> appointments;

    private readonly List<ShowcaseAuditEntry> audit;

    private readonly List<HcAvailability> availability;

    private readonly List<HcBill> bills;

    private readonly List<HcClaim> claims;

    private readonly List<HcLabOrder> labOrders;

    private readonly List<ShowcaseNotification> notifications;

    private readonly List<HcPatient> patients;

    private readonly List<HcPrescription> prescriptions;

    private readonly List<HcStaffProfile> staffProfiles;

    private readonly List<HcTimeOff> timeOff;

    /// <summary>How many bulk changes are running; <see cref="Changed" /> is raised once when the outermost one ends.</summary>
    private int batchDepth;

    private bool changedInBatch;

    private int nextAppointmentId;

    private int nextAuditId;

    private int nextAvailabilityId;

    private int nextBillId;

    private int nextClaimId;

    private int nextLabOrderId;

    private int nextNotificationId;

    private int nextPatientId;

    private int nextPrescriptionId;

    private int nextTimeOffId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public HcState()
    {
        var data = HcSeedData.Build();
        appointments = data.Appointments;
        audit = data.Audit;
        availability = data.Availability;
        bills = data.Bills;
        claims = data.Claims;
        labOrders = data.LabOrders;
        notifications = data.Notifications;
        patients = data.Patients;
        prescriptions = data.Prescriptions;
        staffProfiles = data.StaffProfiles;
        timeOff = data.TimeOff;
        Doctors = data.Doctors;
        ResetCounters(data);

        Identity = new ShowcaseIdentity(HcSeedData.Roles(), HcSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Adds an allergy to a patient's chart (doctors).</summary>
    public HcResult AddAllergy(int patientId, string text) => AddToChart(patientId, text, patient => patient.Allergies, "Allergy added", "allergies");

    /// <summary>Adds a condition to a patient's chart (doctors).</summary>
    public HcResult AddCondition(int patientId, string text) => AddToChart(patientId, text, patient => patient.Conditions, "Condition added", "conditions");

    /// <summary>Returns the appointments starting on a day (staff: everyone's, optionally one doctor's), by start time.</summary>
    public IReadOnlyList<HcAppointment> AppointmentsOn(DateTime date, string? doctorId = null) =>
        IsStaff ? appointments.Where(item => item.Start.Date == date.Date && (doctorId is null || item.DoctorId == doctorId)).OrderBy(item => item.Start).ToList() : Array.Empty<HcAppointment>();

    /// <summary>Returns a doctor's weekly hours, by day and start.</summary>
    public IReadOnlyList<HcAvailability> AvailabilityOf(string doctorId) =>
        availability.Where(item => item.DoctorId == doctorId).OrderBy(item => ((int)item.Day + 6) % 7).ThenBy(item => item.StartMinute).ToList();

    /// <summary>Returns a patient's bills, due first, then those waiting for the health plan (staff, or the patient themselves).</summary>
    public IReadOnlyList<HcBill> BillsOf(int patientId) =>
        CanSeePatient(patientId)
            ? bills.Where(item => item.PatientId == patientId).OrderBy(item => item.Status switch { HcBillStatus.Due => 0, HcBillStatus.Pending => 1, _ => 2 }).ThenByDescending(item => item.CreatedAt).ToList()
            : Array.Empty<HcBill>();

    /// <summary>
    /// Books an open time with a doctor for the signed-in patient, as a standard or a new patient visit (which needs two free
    /// slots in a row), and keeps the health plan the patient gave (an empty payer for self-pay). Unavailable: the time isn't
    /// open for that long (taken, outside hours, time off, too soon, or too far ahead); Conflict: the patient has another
    /// appointment then; WrongState: the doctor isn't taking new patients and hasn't seen this one; Invalid: a bad reason, visit
    /// type, or health plan, or already <see cref="MaxUpcoming" /> upcoming.
    /// </summary>
    public HcResult Book(string doctorId, DateTime start, HcVisitType visitType, string reason, string? payerId, string? memberId, out HcAppointment? booked)
    {
        booked = null;

        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        if (FindDoctor(doctorId) is not { } doctor)
            return HcResult.NotFound;

        reason = reason?.Trim() ?? string.Empty;
        payerId = payerId?.Trim() ?? string.Empty;
        memberId = memberId?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!IsText(reason, MaxReasonLength) || reason.Length < MinVisitReasonLength || !Enum.IsDefined(visitType) || MemberIdProblem(payerId, memberId) is not null
            || Upcoming(patient.Id).Count >= MaxUpcoming || appointments.Count >= MaxAppointments || nextAppointmentId >= NextIdLimit)
            return HcResult.Invalid;

        if (!doctor.AcceptingNewPatients && !HasSeen(patient.Id, doctor.Id))
            return HcResult.WrongState;

        // A first visit with the doctor is a new patient visit, and later ones are standard visits (the booking form says so first).
        if ((visitType == HcVisitType.NewPatient) == HasSeen(patient.Id, doctor.Id))
            return HcResult.Invalid;

        var minutes = HcVisitTypes.MinutesOf(visitType);

        if (FindSlot(doctor.Id, start, minutes) is not { } slot)
            return HcResult.Unavailable;

        if (HasAppointmentAt(patient.Id, start, minutes, null))
            return HcResult.Conflict;

        var appointment = new HcAppointment
        {
            Id = nextAppointmentId++, PatientId = patient.Id, DoctorId = doctor.Id, ClinicId = slot.ClinicId, Start = start, Minutes = minutes, VisitType = visitType, Reason = reason,
            Status = HcAppointmentStatus.Booked, CreatedAt = Clock.Now,
        };
        var insurance = (patient.InsurancePayerId, patient.InsuranceMemberId);
        appointments.Add(appointment);
        (patient.InsurancePayerId, patient.InsuranceMemberId) = (payerId, payerId.Length == 0 ? string.Empty : memberId);

        var result = Commit(() =>
        {
            appointments.Remove(appointment);
            nextAppointmentId--;
            (patient.InsurancePayerId, patient.InsuranceMemberId) = insurance;
        }, () =>
        {
            var when = HcDisplayText.When(start);
            Record("Appointment booked", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id),
                $"{HcVisitTypes.Label(visitType)} with {doctor.Name}, {when}, booked online");

            if (insurance != (patient.InsurancePayerId, patient.InsuranceMemberId))
                Record("Health plan changed", PatientTarget(patient), PatientDetailsPath(patient.Id), "Changed when booking: health plan, member ID");

            NotifyDoctor(doctor, $"New booking: {patient.Name}", $"{HcVisitTypes.Label(visitType)} on {when}: {reason}", StaffAppointmentPath(appointment.Id));
            NotifyFrontDesk(appointment.ClinicId, $"New booking at {FindClinic(appointment.ClinicId)?.Name}: {patient.Name}", $"{when} with {doctor.Name}.",
                HcSeedData.StaffCalendarPath(start.Date, doctor.Id));
            Notify(patient.AccountId, "Appointment confirmed", $"{when} with {doctor.Name} at {FindClinic(appointment.ClinicId)?.Name}.", HcSeedData.AppointmentPath(appointment.Id), fromSystem: true);
        });

        if (result == HcResult.Ok)
            booked = appointment;

        return result;
    }

    /// <summary>
    /// Cancels an appointment: the patient their own booked one up to 2 hours before it starts; the front desk any booked or
    /// checked-in one. The time opens again for others. The doctor hears about it, and so does the patient when the front
    /// desk cancelled.
    /// </summary>
    public HcResult CancelAppointment(int appointmentId, string reason)
    {
        var account = Identity.Current;
        var patient = Patient();

        if (patient is null && !IsFrontDesk)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId) is not { } appointment || (patient is not null && appointment.PatientId != patient.Id))
            return HcResult.NotFound;

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length > MaxReasonLength)
            return HcResult.Invalid;

        var allowed = patient is not null
            ? appointment.Status == HcAppointmentStatus.Booked && appointment.Start > Clock.Now.AddHours(2)
            : appointment.Status is HcAppointmentStatus.Booked or HcAppointmentStatus.CheckedIn;

        if (!allowed || account is null)
            return HcResult.WrongState;

        var previous = (appointment.Status, appointment.CancelledAt, appointment.CancelReason);
        appointment.Status = HcAppointmentStatus.Cancelled;
        appointment.CancelledAt = Clock.Now;
        appointment.CancelReason = reason;

        return Commit(() => (appointment.Status, appointment.CancelledAt, appointment.CancelReason) = previous, () =>
        {
            var owner = patients.FirstOrDefault(item => item.Id == appointment.PatientId);
            var doctor = FindDoctor(appointment.DoctorId);
            var when = HcDisplayText.When(appointment.Start);
            Record("Appointment cancelled", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id),
                $"{(patient is not null ? "Cancelled online" : "Cancelled by the front desk")}{(reason.Length > 0 ? Invariant($", reason given ({reason.Length} characters)") : ", no reason given")}");
            NotifyDoctor(doctor, $"Appointment cancelled: {owner?.Name}", $"{when} is free again.", StaffAppointmentPath(appointment.Id));

            if (patient is not null)
            {
                NotifyFrontDesk(appointment.ClinicId, $"Cancelled at {FindClinic(appointment.ClinicId)?.Name}: {owner?.Name}", $"{when} with {doctor?.Name} is free again.",
                    HcSeedData.StaffCalendarPath(appointment.Start.Date, appointment.DoctorId));
                Notify(owner?.AccountId, "Appointment cancelled", $"Your visit on {when} with {doctor?.Name} is cancelled.", HcSeedData.AppointmentPath(appointment.Id), fromSystem: true);
            }
            else
            {
                Notify(owner?.AccountId, "Your appointment was cancelled", $"The clinic cancelled your visit on {when} with {doctor?.Name}. Book another time when it suits you.",
                    HcSeedData.AppointmentPath(appointment.Id));
            }
        });
    }

    /// <summary>Checks a patient in for today's appointment (front desk); the doctor hears about it.</summary>
    public HcResult CheckIn(int appointmentId)
    {
        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status != HcAppointmentStatus.Booked || appointment.Start.Date != Clock.Today)
            return HcResult.WrongState;

        appointment.Status = HcAppointmentStatus.CheckedIn;
        appointment.CheckedInAt = Clock.Now;
        var patient = patients.FirstOrDefault(item => item.Id == appointment.PatientId);
        Record("Checked in", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id), $"Arrived for {HcDisplayText.Time(appointment.Start)} with {FindDoctor(appointment.DoctorId)?.Name}");
        NotifyDoctor(FindDoctor(appointment.DoctorId), $"{patient?.Name} is checked in",
            $"For {HcDisplayText.Time(appointment.Start)} at {FindClinic(appointment.ClinicId)?.Name}: {appointment.Reason}", StaffAppointmentPath(appointment.Id));
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>
    /// Completes the signed-in doctor's visit in progress: the patient sees the summary under their records, and the visit is
    /// billed (primary care or specialist fee, due in 30 days); the patient hears about both. A patient with a health plan
    /// pays nothing yet: the bill waits for the plan's answer to the claim, and billing hears the visit is ready to code.
    /// </summary>
    public HcResult CompleteVisit(int appointmentId, out HcBill? bill)
    {
        bill = null;

        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && item.DoctorId == doctor.Id) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status != HcAppointmentStatus.InVisit)
            return HcResult.WrongState;

        if (bills.Count >= MaxBills || nextBillId >= NextIdLimit)
            return HcResult.Invalid;

        var fee = doctor.Specialty is "Family medicine" or "Pediatrics" ? HcSeedData.PrimaryCareFeeCents : HcSeedData.SpecialistFeeCents;
        var payer = FindPayer(patients.FirstOrDefault(item => item.Id == appointment.PatientId)?.InsurancePayerId);
        var created = new HcBill
        {
            Id = nextBillId++, PatientId = appointment.PatientId, AppointmentId = appointment.Id, AmountCents = fee, CreatedAt = Clock.Now, DueDate = Clock.Today.AddDays(30),
            Description = Clip($"Visit: {HcSeedData.LowerFirst(appointment.Reason)} with {doctor.Name}", MaxBillDescriptionLength),
            Status = payer is null ? HcBillStatus.Due : HcBillStatus.Pending,
        };
        appointment.Status = HcAppointmentStatus.Completed;
        appointment.CompletedAt = Clock.Now;
        bills.Add(created);
        var result = Commit(() =>
        {
            bills.Remove(created);
            nextBillId--;
            appointment.Status = HcAppointmentStatus.InVisit;
            appointment.CompletedAt = null;
        }, () =>
        {
            var owner = patients.FirstOrDefault(item => item.Id == appointment.PatientId);
            Record("Visit completed", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id),
                Invariant($"Notes {appointment.Notes.Length} characters, summary {appointment.Summary.Length} characters; billed {HcDisplayText.Money(fee)}"));
            Notify(owner?.AccountId, "Your visit summary is ready", $"{doctor.Name} completed your visit on {HcDisplayText.Day(appointment.Start)}.", HcSeedData.AppointmentPath(appointment.Id));

            if (payer is null)
            {
                Notify(owner?.AccountId, $"New bill: {HcDisplayText.Money(fee)}", $"{created.Description}, due {HcDisplayText.Day(created.DueDate)}.", BillsPath(created.Id));
            }
            else
            {
                Notify(owner?.AccountId, "Your visit goes to your health plan first", $"Nothing to pay until {payer.Name} answers; your bill shows what you owe after that.", BillsPath(created.Id));
                NotifyBilling($"Visit to code: {owner?.Name}", $"{doctor.Name}, {HcDisplayText.Day(appointment.Start)}: {appointment.Reason}. Code it to claim {payer.Name}.", HcSeedData.StaffBase + "/claims");
            }
        });

        if (result == HcResult.Ok)
            bill = created;

        return result;
    }

    /// <summary>
    /// Answers a patient's refill request (doctors): approving adds one refill; denying needs a reason of at least
    /// <see cref="MinDenyReasonLength" /> characters for the patient, who hears the answer either way.
    /// </summary>
    public HcResult DecideRefill(int prescriptionId, bool approve, string note)
    {
        if (Doctor() is null)
            return HcResult.NotAllowed;

        if (prescriptions.FirstOrDefault(item => item.Id == prescriptionId) is not { } prescription)
            return HcResult.NotFound;

        if (prescription.RefillRequestedAt is null || prescription.Status != HcPrescriptionStatus.Active)
            return HcResult.WrongState;

        note = note?.Trim() ?? string.Empty;

        if (note.Length > MaxReasonLength || (!approve && note.Length < MinDenyReasonLength) || (approve && prescription.Refills >= MaxRefills))
            return HcResult.Invalid;

        var previous = (prescription.Refills, prescription.RefillNote, prescription.RefillRequestedAt, prescription.RefillOutcome, prescription.RefillPharmacyId, prescription.RefillQuantity,
            prescription.RefillPickupOn);
        prescription.Refills += approve ? 1 : 0;
        prescription.RefillNote = approve ? (note.Length > 0 ? note : "Approved: one refill added.") : note;
        prescription.RefillOutcome = approve ? HcRefillOutcome.Approved : HcRefillOutcome.Denied;
        prescription.RefillRequestedAt = null;
        prescription.RefillPharmacyId = string.Empty;
        prescription.RefillQuantity = 0;
        prescription.RefillPickupOn = null;

        return Commit(() => (prescription.Refills, prescription.RefillNote, prescription.RefillRequestedAt, prescription.RefillOutcome, prescription.RefillPharmacyId, prescription.RefillQuantity,
            prescription.RefillPickupOn) = previous, () =>
        {
            var owner = patients.FirstOrDefault(item => item.Id == prescription.PatientId);
            Record(approve ? "Refill approved" : "Refill denied", PrescriptionTarget(prescription), PatientChartPath(prescription.PatientId),
                approve ? "One refill added" + (note.Length > 0 ? Invariant($"; note {note.Length} characters") : null) : Invariant($"Reason given ({note.Length} characters)"));
            Notify(owner?.AccountId, approve ? $"Refill approved: {prescription.Medication}" : $"Refill denied: {prescription.Medication}",
                approve ? $"{Identity.Current?.Name} added a refill of {prescription.Medication} {prescription.Dose}." : $"{Identity.Current?.Name} answered: {note}",
                SiteBase + "/prescriptions");
        });
    }

    /// <summary>Deletes a block of weekly hours (front desk); booked appointments in it stay booked.</summary>
    public HcResult DeleteAvailability(int availabilityId)
    {
        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        if (availability.FirstOrDefault(item => item.Id == availabilityId) is not { } block)
            return HcResult.NotFound;

        availability.Remove(block);
        Record("Hours deleted", HoursTarget(block), AvailabilityPath(block.DoctorId), $"{block.Day} {HcDisplayText.Minutes(block.StartMinute)}–{HcDisplayText.Minutes(block.EndMinute)} at {FindClinic(block.ClinicId)?.Name}");
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>Deletes a time off (front desk): the days open for booking again.</summary>
    public HcResult DeleteTimeOff(int timeOffId)
    {
        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        if (timeOff.FirstOrDefault(item => item.Id == timeOffId) is not { } entry)
            return HcResult.NotFound;

        timeOff.Remove(entry);
        Record("Time off deleted", $"Time off · {FindDoctor(entry.DoctorId)?.Name}", AvailabilityPath(entry.DoctorId), $"{HcDisplayText.Day(entry.From)} to {HcDisplayText.Day(entry.To)}");
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>Returns the first day a prescription can be refilled: when three quarters of its last fill are used up.</summary>
    public static DateTime EarliestRefillOn(HcPrescription prescription) =>
        prescription.LastFilledOn.Date.AddDays(Math.Max(1, (int)Math.Ceiling(Math.Max(1, prescription.DaysSupply) * 0.75)));

    /// <summary>
    /// Enters (or corrects) a lab order's results as the doctor who ordered it: one value per analyte, in the panel's order,
    /// and an optional comment for the patient. The patient sees nothing until <see cref="ReleaseLab" />.
    /// </summary>
    public HcResult EnterLabResults(int orderId, IReadOnlyList<double> values, string comment)
    {
        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (labOrders.FirstOrDefault(item => item.Id == orderId && item.DoctorId == doctor.Id) is not { } order || LabPanel(order.PanelId) is not { } panel)
            return HcResult.NotFound;

        if (order.Status == HcLabStatus.Released)
            return HcResult.WrongState;

        comment = comment?.Trim() ?? string.Empty;

        if (values is null || values.Count != panel.Analytes.Count || values.Select((value, index) => (value, index)).Any(pair => !double.IsFinite(pair.value) || pair.value < 0 || pair.value > panel.Analytes[pair.index].MaxValue)
            || comment.Length > MaxCommentLength)
            return HcResult.Invalid;

        var corrected = order.Status == HcLabStatus.Resulted;
        var previous = (order.Values, order.Comment, order.Status, order.ResultedAt);
        order.Values = values.Select(value => Math.Round(value, 2)).ToList();
        order.Comment = comment;
        order.Status = HcLabStatus.Resulted;
        order.ResultedAt = Clock.Now;

        return Commit(() => (order.Values, order.Comment, order.Status, order.ResultedAt) = previous,
            () => Record(corrected ? "Lab results corrected" : "Lab results entered", LabTarget(order), HcSeedData.LabOrderPath(order.Id),
                Invariant($"{order.Values.Count} values, comment {comment.Length} characters")));
    }

    /// <summary>
    /// Finds an appointment the signed-in account may see (a patient their own; staff any), or returns
    /// <see langword="null" />.
    /// </summary>
    public HcAppointment? FindAppointment(int appointmentId) =>
        appointments.FirstOrDefault(item => item.Id == appointmentId && (IsStaff || item.PatientId == Patient()?.Id)) is { } found ? ForViewer(found) : null;

    /// <summary>Finds a clinic, or returns <see langword="null" />.</summary>
    public HcClinic? FindClinic(string? clinicId) => Clinics.FirstOrDefault(item => item.Id == clinicId);

    /// <summary>Finds a doctor, or returns <see langword="null" />.</summary>
    public HcDoctor? FindDoctor(string? doctorId) => Doctors.FirstOrDefault(item => item.Id == doctorId);

    /// <summary>
    /// Finds a lab order the signed-in account may see (a patient their own <em>released</em> ones; staff any), or returns
    /// <see langword="null" />.
    /// </summary>
    public HcLabOrder? FindLabOrder(int orderId) =>
        labOrders.FirstOrDefault(item => item.Id == orderId && (IsStaff || (item.PatientId == Patient()?.Id && item.Status == HcLabStatus.Released)));

    /// <summary>Finds a patient the signed-in account may see (staff any; a patient only themselves), or returns <see langword="null" />.</summary>
    public HcPatient? FindPatient(int patientId) => CanSeePatient(patientId) ? patients.FirstOrDefault(item => item.Id == patientId) : null;

    /// <summary>Finds a health plan, or returns <see langword="null" />.</summary>
    public HcPayer? FindPayer(string? payerId) => Payers.FirstOrDefault(item => item.Id == payerId);

    /// <summary>Finds a pharmacy, or returns <see langword="null" />.</summary>
    public HcPharmacy? FindPharmacy(string? pharmacyId) => Pharmacies.FirstOrDefault(item => item.Id == pharmacyId);

    /// <summary>
    /// Returns the open time with a doctor at <paramref name="start" /> when the whole visit (<paramref name="minutes" />) fits
    /// in one block of their hours and is free, or <see langword="null" />.
    /// </summary>
    public HcSlot? FindSlot(string doctorId, DateTime start, int minutes = SlotMinutes) => Slots(doctorId, start.Date, minutes).FirstOrDefault(slot => slot.Start == start);

    /// <summary>Returns whether a value is below, inside, or above an analyte's reference range: "Low", "Normal", or "High".</summary>
    public static string Flag(HcAnalyte analyte, double value) => value < analyte.Low ? "Low" : value > analyte.High ? "High" : "Normal";

    /// <summary>Returns whether a patient has had a visit with a doctor (who may then book them while closed to new patients).</summary>
    public bool HasSeen(int patientId, string doctorId) =>
        appointments.Any(item => item.PatientId == patientId && item.DoctorId == doctorId && item.Status is HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed);

    /// <summary>Returns whether another patient has this medical record number, ignoring case and surrounding spaces; the patient being edited doesn't count.</summary>
    public bool IsMrnTaken(string? mrn, int exceptPatientId = 0) =>
        !string.IsNullOrWhiteSpace(mrn) && patients.Any(patient => patient.Id != exceptPatientId && string.Equals(patient.Mrn, mrn.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns whether a medical record number has the clinics' form: CL- and six digits, like CL-100001 (any case; it is saved in capitals).</summary>
    public static bool IsMrn(string? value) => value?.Trim() is { Length: 9 } mrn && MrnPattern().IsMatch(mrn.ToUpperInvariant());

    /// <summary>
    /// Returns whether a booked appointment falls outside its doctor's current hours at its clinic, or inside their time off.
    /// </summary>
    public bool IsOutsideHours(HcAppointment appointment)
    {
        var minute = (appointment.Start.Hour * 60) + appointment.Start.Minute;

        return timeOff.Any(item => item.DoctorId == appointment.DoctorId && appointment.Start.Date >= item.From && appointment.Start.Date <= item.To)
            || !availability.Any(block => block.DoctorId == appointment.DoctorId && block.ClinicId == appointment.ClinicId && block.Day == appointment.Start.DayOfWeek
                && minute >= block.StartMinute && minute + appointment.Minutes <= block.EndMinute);
    }

    /// <summary>Returns whether a phone number has the right form: 7 to 30 characters of digits, spaces, and + - ( ) ., with at least 7 digits.</summary>
    public static bool IsPhone(string? value) => value is { Length: >= 7 and <= MaxPhoneLength } && PhonePattern().IsMatch(value) && value.Count(char.IsAsciiDigit) >= 7;

    /// <summary>Returns a lab panel, or <see langword="null" />.</summary>
    public HcLabPanel? LabPanel(string? panelId) => LabPanels.FirstOrDefault(item => item.Id == panelId);

    /// <summary>
    /// Returns a patient's lab orders, newest first: for the patient, released ones plus ones still waiting (with no values
    /// shown); for staff, all.
    /// </summary>
    public IReadOnlyList<HcLabOrder> LabOrdersOf(int patientId) =>
        CanSeePatient(patientId) ? labOrders.Where(item => item.PatientId == patientId).OrderByDescending(item => item.OrderedAt).Select(ForViewer).ToList() : Array.Empty<HcLabOrder>();

    /// <summary>Marks a booked appointment that has started without the patient as a no-show (front desk).</summary>
    public HcResult MarkNoShow(int appointmentId)
    {
        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status != HcAppointmentStatus.Booked || appointment.Start > Clock.Now)
            return HcResult.WrongState;

        appointment.Status = HcAppointmentStatus.NoShow;
        Record("Marked no-show", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id), $"Didn't arrive for {HcDisplayText.Time(appointment.Start)} with {FindDoctor(appointment.DoctorId)?.Name}");
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>
    /// Returns why a health plan and member ID can't be kept, in words, or <see langword="null" />: no member ID for self-pay
    /// (an empty payer), a known payer, and its member ID of the payer's three letters and nine digits.
    /// </summary>
    public static string? MemberIdProblem(string? payerId, string? memberId)
    {
        var payer = HcSeedData.Payers().FirstOrDefault(item => item.Id == payerId);
        var id = memberId?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(payerId))
            return id.Length == 0 ? null : "Choose the health plan this member ID belongs to, or clear the member ID for self-pay.";

        if (payer is null)
            return "Choose a health plan from the list.";

        if (id.Length == 0)
            return $"Enter the member ID from the {payer.Name} card.";

        if (!MemberIdPattern().IsMatch(id))
            return "Enter the member ID as 3 letters and 9 digits, as printed on the card, without spaces.";

        return id.StartsWith(payer.MemberIdPrefix, StringComparison.Ordinal) ? null : $"{payer.Name} member IDs start with {payer.MemberIdPrefix}. Check the card, or the health plan chosen.";
    }

    /// <summary>Returns the first open time with a doctor in the booking window, or <see langword="null" />.</summary>
    public HcSlot? NextSlot(string doctorId)
    {
        for (var day = 0; day <= BookingDays; day++)
        {
            if (Slots(doctorId, Clock.Today.AddDays(day)).FirstOrDefault() is { } slot)
                return slot;
        }

        return null;
    }

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored.</summary>
    public void NotifyRestored() => Changed?.Invoke();

    /// <summary>Orders a lab test in the signed-in doctor's visit (in progress or completed).</summary>
    public HcResult OrderLab(int appointmentId, string panelId, out HcLabOrder? ordered)
    {
        ordered = null;

        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && item.DoctorId == doctor.Id) is not { } appointment || LabPanel(panelId) is null)
            return HcResult.NotFound;

        if (appointment.Status is not (HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed))
            return HcResult.WrongState;

        if (labOrders.Any(item => item.AppointmentId == appointmentId && item.PanelId == panelId))
            return HcResult.Conflict;

        if (labOrders.Count >= MaxLabOrders || nextLabOrderId >= NextIdLimit)
            return HcResult.Invalid;

        var order = new HcLabOrder { Id = nextLabOrderId++, PatientId = appointment.PatientId, DoctorId = doctor.Id, AppointmentId = appointment.Id, PanelId = panelId, OrderedAt = Clock.Now };
        labOrders.Add(order);
        var result = Commit(() =>
        {
            labOrders.Remove(order);
            nextLabOrderId--;
        }, () => Record("Lab ordered", LabTarget(order), HcSeedData.LabOrderPath(order.Id), $"For {patients.FirstOrDefault(item => item.Id == order.PatientId)?.Name}, in appointment {appointment.Id.ToString(CultureInfo.InvariantCulture)}"));

        if (result == HcResult.Ok)
            ordered = order;

        return result;
    }

    /// <summary>Returns a patient's appointments, newest first (staff, or the patient themselves).</summary>
    public IReadOnlyList<HcAppointment> PatientAppointments(int patientId) =>
        CanSeePatient(patientId) ? appointments.Where(item => item.PatientId == patientId).OrderByDescending(item => item.Start).Select(ForViewer).ToList() : Array.Empty<HcAppointment>();

    /// <summary>
    /// Pays one of the signed-in patient's due bills with a test card already checked by <see cref="ShowcaseCards" />:
    /// CardDeclined (nothing paid) when the charge was declined.
    /// </summary>
    public HcResult PayBill(int billId, ShowcaseCardCharge charge)
    {
        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        if (bills.FirstOrDefault(item => item.Id == billId && item.PatientId == patient.Id) is not { } bill)
            return HcResult.NotFound;

        // Only a bill to pay can be paid: not one already paid, one still with the health plan, or one insurance covered.
        if (bill.Status != HcBillStatus.Due)
            return HcResult.WrongState;

        if (charge is null || !charge.IsApproved)
            return HcResult.CardDeclined;

        bill.Status = HcBillStatus.Paid;
        bill.PaidAt = Clock.Now;
        bill.CardBrand = Clip(charge.Brand ?? string.Empty, 20);
        bill.CardLast4 = Clip(charge.Last4 ?? string.Empty, 4);
        Record("Bill paid", BillTarget(bill), PatientDetailsPath(patient.Id), $"{HcDisplayText.Money(bill.AmountCents)} with {(bill.CardBrand.Length > 0 ? bill.CardBrand : "a test card")} ending {bill.CardLast4}");
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>
    /// Prescribes a medication in the signed-in doctor's visit (in progress or completed); the first fill is today and lasts
    /// <paramref name="daysSupply" /> days (1 to <see cref="MaxDaysSupply" />), and all fills together at most a year.
    /// </summary>
    public HcResult Prescribe(int appointmentId, string medication, string dose, string directions, int quantity, int refills, int daysSupply, out HcPrescription? prescribed)
    {
        prescribed = null;

        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && item.DoctorId == doctor.Id) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status is not (HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed))
            return HcResult.WrongState;

        medication = medication?.Trim() ?? string.Empty;
        dose = dose?.Trim() ?? string.Empty;
        directions = directions?.Trim() ?? string.Empty;

        if (!IsText(medication, MaxMedicationLength) || !IsText(dose, MaxDoseLength) || !IsText(directions, MaxDirectionsLength) || quantity is < 1 or > MaxQuantity || refills is < 0 or > MaxRefills
            || daysSupply is < 1 or > MaxDaysSupply || (refills + 1) * daysSupply > 366 || prescriptions.Count >= MaxPrescriptions || nextPrescriptionId >= NextIdLimit)
            return HcResult.Invalid;

        var prescription = new HcPrescription
        {
            Id = nextPrescriptionId++, PatientId = appointment.PatientId, DoctorId = doctor.Id, Medication = medication, Dose = dose, Directions = directions, Quantity = quantity, Refills = refills,
            PrescribedAt = Clock.Now, LastFilledOn = Clock.Today, DaysSupply = daysSupply, Status = HcPrescriptionStatus.Active,
        };
        prescriptions.Add(prescription);
        var result = Commit(() =>
        {
            prescriptions.Remove(prescription);
            nextPrescriptionId--;
        }, () => Record("Prescribed", PrescriptionTarget(prescription), PatientChartPath(prescription.PatientId), Invariant($"Quantity {quantity}, {refills} refills")));

        if (result == HcResult.Ok)
            prescribed = prescription;

        return result;
    }

    /// <summary>Returns a patient's prescriptions, active first (staff, or the patient themselves).</summary>
    public IReadOnlyList<HcPrescription> PrescriptionsOf(int patientId) =>
        CanSeePatient(patientId)
            ? prescriptions.Where(item => item.PatientId == patientId).OrderBy(item => item.Status).ThenByDescending(item => item.PrescribedAt).ToList()
            : Array.Empty<HcPrescription>();

    /// <summary>
    /// Audits that a staff member opened a patient's chart (<paramref name="details" /> <see langword="false" />) or their
    /// registration details: clinical systems log every look at a record, and the patient sees it under Records. Only the
    /// fact and the role are kept, never what was on screen.
    /// </summary>
    public void RecordChartView(int patientId, bool details = false)
    {
        if (!IsStaff || patients.FirstOrDefault(item => item.Id == patientId) is not { } patient)
            return;

        Record(details ? "Viewed details" : "Viewed chart", PatientTarget(patient), details ? PatientDetailsPath(patient.Id) : PatientChartPath(patient.Id),
            IsFrontDesk ? "Front desk" : IsBilling ? "Billing" : "Doctor");
        RaiseChanged();
    }

    /// <summary>Returns who looked at the signed-in patient's chart or details, newest first (from the audit log); empty for anyone else.</summary>
    public IReadOnlyList<HcRecordAccess> RecordAccessOfMine()
    {
        if (Patient() is not { } patient)
            return Array.Empty<HcRecordAccess>();

        var chart = PatientChartPath(patient.Id);
        var details = PatientDetailsPath(patient.Id);

        return audit.Where(entry => entry.Action is "Viewed chart" or "Viewed details" && (entry.TargetUrl == chart || entry.TargetUrl == details))
            .OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id)
            .Select(entry => new HcRecordAccess(entry.At, entry.Actor, entry.Action == "Viewed chart" ? "Viewed your chart" : "Viewed your registration details", entry.Details))
            .ToList();
    }

    /// <summary>Releases a lab order's entered results to the patient (the ordering doctor), and bills the test; the patient hears about both.</summary>
    public HcResult ReleaseLab(int orderId)
    {
        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (labOrders.FirstOrDefault(item => item.Id == orderId && item.DoctorId == doctor.Id) is not { } order || LabPanel(order.PanelId) is not { } panel)
            return HcResult.NotFound;

        if (order.Status != HcLabStatus.Resulted)
            return HcResult.WrongState;

        if (bills.Count >= MaxBills || nextBillId >= NextIdLimit)
            return HcResult.Invalid;

        var bill = new HcBill
        {
            Id = nextBillId++, PatientId = order.PatientId, LabOrderId = order.Id, AmountCents = HcSeedData.LabFeeCents, CreatedAt = Clock.Now, DueDate = Clock.Today.AddDays(30),
            Description = Clip($"Lab: {HcSeedData.LowerFirst(panel.Name)}", MaxBillDescriptionLength),
        };
        order.Status = HcLabStatus.Released;
        order.ReleasedAt = Clock.Now;
        bills.Add(bill);

        return Commit(() =>
        {
            bills.Remove(bill);
            nextBillId--;
            order.Status = HcLabStatus.Resulted;
            order.ReleasedAt = null;
        }, () =>
        {
            var owner = patients.FirstOrDefault(item => item.Id == order.PatientId);
            Record("Lab results released", LabTarget(order), HcSeedData.LabOrderPath(order.Id), $"Released to {owner?.Name}; billed {HcDisplayText.Money(bill.AmountCents)}");
            Notify(owner?.AccountId, $"Lab results released: {panel.Name}", $"{doctor.Name} released your results.", HcSeedData.LabReportPath(order.Id));
            Notify(owner?.AccountId, $"New bill: {HcDisplayText.Money(bill.AmountCents)}", $"{bill.Description}, due {HcDisplayText.Day(bill.DueDate)}.", BillsPath(bill.Id));
        });
    }

    /// <summary>Removes an allergy from a patient's chart (doctors).</summary>
    public HcResult RemoveAllergy(int patientId, string text) => RemoveFromChart(patientId, text, patient => patient.Allergies, "Allergy removed", "allergies");

    /// <summary>Removes a condition from a patient's chart (doctors).</summary>
    public HcResult RemoveCondition(int patientId, string text) => RemoveFromChart(patientId, text, patient => patient.Conditions, "Condition removed", "conditions");

    /// <summary>
    /// Asks the doctor for a refill of one of the signed-in patient's active prescriptions with no refills left, to pick up at
    /// a pharmacy on a day: quantity 1 to the prescription's, and a day from the earliest refill day
    /// (<see cref="EarliestRefillOn" />) and today, up to <see cref="MaxRefillPickupDays" /> days ahead (Invalid otherwise).
    /// WrongState when refills remain, it's stopped, or a request is already waiting. The prescribing doctor hears about it.
    /// </summary>
    public HcResult RequestRefill(int prescriptionId, string pharmacyId, int quantity, DateTime pickupOn)
    {
        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        if (prescriptions.FirstOrDefault(item => item.Id == prescriptionId && item.PatientId == patient.Id) is not { } prescription)
            return HcResult.NotFound;

        if (prescription.Status != HcPrescriptionStatus.Active || prescription.Refills > 0 || prescription.RefillRequestedAt is not null)
            return HcResult.WrongState;

        pickupOn = pickupOn.Date;

        if (FindPharmacy(pharmacyId) is null || quantity < 1 || quantity > prescription.Quantity || pickupOn < Clock.Today || pickupOn < EarliestRefillOn(prescription)
            || pickupOn > Clock.Today.AddDays(MaxRefillPickupDays))
            return HcResult.Invalid;

        var previous = (prescription.RefillRequestedAt, prescription.RefillNote, prescription.RefillOutcome, prescription.RefillPharmacyId, prescription.RefillQuantity, prescription.RefillPickupOn);
        prescription.RefillRequestedAt = Clock.Now;
        prescription.RefillNote = string.Empty;
        prescription.RefillOutcome = HcRefillOutcome.None;
        prescription.RefillPharmacyId = pharmacyId;
        prescription.RefillQuantity = quantity;
        prescription.RefillPickupOn = pickupOn;

        return Commit(() => (prescription.RefillRequestedAt, prescription.RefillNote, prescription.RefillOutcome, prescription.RefillPharmacyId, prescription.RefillQuantity, prescription.RefillPickupOn) = previous,
            () =>
            {
                Record("Refill requested", PrescriptionTarget(prescription), RefillsPath, Invariant($"Quantity {quantity}, to pick up {HcDisplayText.Day(pickupOn)} at {FindPharmacy(pharmacyId)?.Name}"));
                NotifyDoctor(FindDoctor(prescription.DoctorId), $"Refill requested: {prescription.Medication}",
                    $"{patient.Name} asked for {quantity.ToString(CultureInfo.InvariantCulture)} of {prescription.Medication} {prescription.Dose}, to pick up {HcDisplayText.Day(pickupOn)}.", RefillsPath);
            });
    }

    /// <summary>
    /// Moves one of the signed-in patient's booked appointments to another open time with the same doctor (with room for the
    /// whole visit), up to 2 hours before it starts (WrongState after that). The doctor and the front desk hear about it.
    /// </summary>
    public HcResult Reschedule(int appointmentId, DateTime newStart)
    {
        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && item.PatientId == patient.Id) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status != HcAppointmentStatus.Booked || appointment.Start <= Clock.Now.AddHours(2))
            return HcResult.WrongState;

        if (newStart == appointment.Start)
            return HcResult.NoChange;

        if (Slots(appointment.DoctorId, newStart.Date, appointment.Minutes, appointment.Id).FirstOrDefault(slot => slot.Start == newStart) is not { } slot)
            return HcResult.Unavailable;

        if (HasAppointmentAt(patient.Id, newStart, appointment.Minutes, appointment.Id))
            return HcResult.Conflict;

        var before = appointment.Start;
        appointment.Start = newStart;
        appointment.ClinicId = slot.ClinicId;

        var doctor = FindDoctor(appointment.DoctorId);
        var when = HcDisplayText.When(newStart);
        Record("Appointment rescheduled", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id), $"{HcDisplayText.When(before)} → {when}, online");
        NotifyDoctor(doctor, $"Appointment moved: {patient.Name}", $"From {HcDisplayText.When(before)} to {when}.", StaffAppointmentPath(appointment.Id));
        NotifyFrontDesk(appointment.ClinicId, $"Moved at {FindClinic(appointment.ClinicId)?.Name}: {patient.Name}", $"Now {when} with {doctor?.Name}.",
            HcSeedData.StaffCalendarPath(newStart.Date, appointment.DoctorId));
        Notify(patient.AccountId, "Appointment rescheduled", $"Your visit with {doctor?.Name} is now {when}.", HcSeedData.AppointmentPath(appointment.Id), fromSystem: true);
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>
    /// Creates (<paramref name="availabilityId" /> 0) or edits a block of a doctor's weekly hours (front desk): whole and half
    /// hours on a weekday the clinic is open, within its opening hours, at least one appointment long, not overlapping the
    /// doctor's other blocks that day (Conflict).
    /// </summary>
    public HcResult SaveAvailability(int availabilityId, string doctorId, DayOfWeek day, int startMinute, int endMinute, string clinicId, out HcAvailability? saved)
    {
        saved = null;

        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        var existing = availabilityId == 0 ? null : availability.FirstOrDefault(item => item.Id == availabilityId);

        if ((availabilityId != 0 && existing is null) || FindDoctor(doctorId) is null || FindClinic(clinicId) is not { } clinic)
            return HcResult.NotFound;

        if (!ValidBlock(day, startMinute, endMinute, clinic) || (existing is not null && existing.DoctorId != doctorId))
            return HcResult.Invalid;

        if (availability.Any(item => item != existing && item.DoctorId == doctorId && item.Day == day && item.StartMinute < endMinute && startMinute < item.EndMinute))
            return HcResult.Conflict;

        var text = $"{day} {HcDisplayText.Minutes(startMinute)}–{HcDisplayText.Minutes(endMinute)} at {clinic.Name}";

        if (existing is not null)
        {
            if (existing.Day == day && existing.StartMinute == startMinute && existing.EndMinute == endMinute && existing.ClinicId == clinicId)
                return HcResult.NoChange;

            var before = $"{existing.Day} {HcDisplayText.Minutes(existing.StartMinute)}–{HcDisplayText.Minutes(existing.EndMinute)} at {FindClinic(existing.ClinicId)?.Name}";
            (existing.Day, existing.StartMinute, existing.EndMinute, existing.ClinicId) = (day, startMinute, endMinute, clinicId);
            saved = existing;
            Record("Hours changed", HoursTarget(existing), AvailabilityPath(doctorId), $"{before} → {text}");
            RaiseChanged();

            return HcResult.Ok;
        }

        if (availability.Count >= MaxAvailability || nextAvailabilityId >= NextIdLimit)
            return HcResult.Invalid;

        var block = new HcAvailability { Id = nextAvailabilityId++, DoctorId = doctorId, Day = day, StartMinute = startMinute, EndMinute = endMinute, ClinicId = clinicId };
        availability.Add(block);
        var result = Commit(() =>
        {
            availability.Remove(block);
            nextAvailabilityId--;
        }, () => Record("Hours added", HoursTarget(block), AvailabilityPath(doctorId), text));

        if (result == HcResult.Ok)
            saved = block;

        return result;
    }

    /// <summary>
    /// Saves the signed-in doctor's work details on their account page: a direct work phone (or none), whether they take new
    /// patients (patients see it and can book only if they have seen the doctor before), and the biography on their profile.
    /// </summary>
    public HcResult SaveDoctorProfile(string workPhone, bool acceptingNewPatients, string bio)
    {
        if (Doctor() is not { } doctor || Identity.Current is not { } account)
            return HcResult.NotAllowed;

        workPhone = workPhone?.Trim() ?? string.Empty;
        bio = bio?.Trim() ?? string.Empty;

        if ((workPhone.Length > 0 && !IsPhone(workPhone)) || bio.Length is < MinBioLength or > MaxBioLength)
            return HcResult.Invalid;

        var profile = ProfileOf(account.Id, doctor.ClinicId);

        if (profile.WorkPhone == workPhone && doctor.AcceptingNewPatients == acceptingNewPatients && doctor.Bio == bio)
            return HcResult.NoChange;

        var changed = new List<string>();

        if (profile.WorkPhone != workPhone)
            changed.Add("work phone");

        if (doctor.AcceptingNewPatients != acceptingNewPatients)
            changed.Add(acceptingNewPatients ? "now taking new patients" : "no longer taking new patients");

        if (doctor.Bio != bio)
            changed.Add("biography");

        var previous = (profile.WorkPhone, doctor.AcceptingNewPatients, doctor.Bio);
        (profile.WorkPhone, doctor.AcceptingNewPatients, doctor.Bio) = (workPhone, acceptingNewPatients, bio);

        return Commit(() => (profile.WorkPhone, doctor.AcceptingNewPatients, doctor.Bio) = previous,
            () => Record("Work details changed", $"Doctor {doctor.Name}", HcSeedData.SiteBase + "/doctors/" + Uri.EscapeDataString(doctor.Id), "Changed: " + string.Join(", ", changed)));
    }

    /// <summary>
    /// Saves the signed-in patient's details: date of birth (not in the future, at most <see cref="MaxAgeYears" /> years ago),
    /// phone, emergency contact (both a name and a phone, or neither; a phone other than the patient's), and health plan.
    /// </summary>
    public HcResult SaveMyDetails(DateTime? dateOfBirth, string phone, string emergencyName, string emergencyPhone, string payerId, string memberId)
    {
        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        phone = phone?.Trim() ?? string.Empty;
        emergencyName = emergencyName?.Trim() ?? string.Empty;
        emergencyPhone = emergencyPhone?.Trim() ?? string.Empty;
        payerId = payerId?.Trim() ?? string.Empty;
        memberId = memberId?.Trim().ToUpperInvariant() ?? string.Empty;

        if ((phone.Length > 0 && !IsPhone(phone)) || (dateOfBirth is { } dob && !ValidBirthDate(dob)) || !ValidEmergencyContact(emergencyName, emergencyPhone, phone)
            || MemberIdProblem(payerId, memberId) is not null)
            return HcResult.Invalid;

        var changed = ChangedFields(patient, patient.Mrn, patient.Name, dateOfBirth?.Date, phone, patient.Email, patient.ClinicId, emergencyName, emergencyPhone, payerId, memberId);

        if (changed.Count == 0)
            return HcResult.NoChange;

        var previous = (patient.DateOfBirth, patient.Phone, patient.EmergencyContactName, patient.EmergencyContactPhone, patient.InsurancePayerId, patient.InsuranceMemberId);
        (patient.DateOfBirth, patient.Phone, patient.EmergencyContactName, patient.EmergencyContactPhone, patient.InsurancePayerId, patient.InsuranceMemberId) =
            (dateOfBirth?.Date, phone, emergencyName, emergencyPhone, payerId, payerId.Length == 0 ? string.Empty : memberId);

        return Commit(() => (patient.DateOfBirth, patient.Phone, patient.EmergencyContactName, patient.EmergencyContactPhone, patient.InsurancePayerId, patient.InsuranceMemberId) = previous,
            () => Record("Details changed", PatientTarget(patient), PatientDetailsPath(patient.Id), "Changed by the patient: " + string.Join(", ", changed)));
    }

    /// <summary>
    /// Registers (<paramref name="patientId" /> 0) or edits a patient (staff): a unique medical record number of the clinics'
    /// form (Duplicate when taken), a name, a date of birth (not in the future, at most <see cref="MaxAgeYears" /> years
    /// ago), phone and email of the right form, a home clinic, an emergency contact (both or neither, a different phone),
    /// and a health plan (<see cref="MemberIdProblem" />). A patient with a sign-in keeps their name and email from their
    /// account. Only changed field names are audited, never the values.
    /// </summary>
    public HcResult SavePatient(int patientId, HcPatientInput input, out HcPatient? saved)
    {
        saved = null;

        if (!IsStaff)
            return HcResult.NotAllowed;

        var existing = patientId == 0 ? null : patients.FirstOrDefault(item => item.Id == patientId);

        if (patientId != 0 && existing is null)
            return HcResult.NotFound;

        if (input is null)
            return HcResult.Invalid;

        var mrn = input.Mrn?.Trim().ToUpperInvariant() ?? string.Empty;
        var name = input.Name?.Trim() ?? string.Empty;
        var phone = input.Phone?.Trim() ?? string.Empty;
        var email = input.Email?.Trim() ?? string.Empty;
        var emergencyName = input.EmergencyContactName?.Trim() ?? string.Empty;
        var emergencyPhone = input.EmergencyContactPhone?.Trim() ?? string.Empty;
        var payerId = input.InsurancePayerId?.Trim() ?? string.Empty;
        var memberId = input.InsuranceMemberId?.Trim().ToUpperInvariant() ?? string.Empty;

        // A patient with a sign-in keeps the name and email of their account.
        if (existing?.AccountId is not null)
            (name, email) = (existing.Name, existing.Email);

        if (!IsMrn(mrn) || !IsText(name, ShowcaseIdentity.MaxNameLength) || !ValidBirthDate(input.DateOfBirth) || !IsPhone(phone) || !IsEmail(email) || FindClinic(input.ClinicId) is null
            || !ValidEmergencyContact(emergencyName, emergencyPhone, phone) || MemberIdProblem(payerId, memberId) is not null)
            return HcResult.Invalid;

        if (IsMrnTaken(mrn, existing?.Id ?? 0))
            return HcResult.Duplicate;

        if (existing is not null)
        {
            var changed = ChangedFields(existing, mrn, name, input.DateOfBirth.Date, phone, email, input.ClinicId, emergencyName, emergencyPhone, payerId, memberId);

            if (changed.Count == 0)
                return HcResult.NoChange;

            var previous = (existing.Mrn, existing.Name, existing.DateOfBirth, existing.Phone, existing.Email, existing.ClinicId, existing.EmergencyContactName, existing.EmergencyContactPhone,
                existing.InsurancePayerId, existing.InsuranceMemberId);
            (existing.Mrn, existing.Name, existing.DateOfBirth, existing.Phone, existing.Email, existing.ClinicId, existing.EmergencyContactName, existing.EmergencyContactPhone,
                existing.InsurancePayerId, existing.InsuranceMemberId) = (mrn, name, input.DateOfBirth.Date, phone, email, input.ClinicId, emergencyName, emergencyPhone, payerId,
                payerId.Length == 0 ? string.Empty : memberId);
            saved = existing;

            return Commit(() => (existing.Mrn, existing.Name, existing.DateOfBirth, existing.Phone, existing.Email, existing.ClinicId, existing.EmergencyContactName, existing.EmergencyContactPhone,
                    existing.InsurancePayerId, existing.InsuranceMemberId) = previous,
                () => Record("Details changed", PatientTarget(existing), PatientDetailsPath(existing.Id), $"Changed by {(IsFrontDesk ? "the front desk" : IsBilling ? "billing" : "the doctor")}: {string.Join(", ", changed)}"));
        }

        if (patients.Count >= MaxPatients || nextPatientId >= NextIdLimit)
            return HcResult.Invalid;

        var patient = new HcPatient
        {
            Id = nextPatientId++, Mrn = mrn, Name = name, DateOfBirth = input.DateOfBirth.Date, Phone = phone, Email = email, ClinicId = input.ClinicId, EmergencyContactName = emergencyName,
            EmergencyContactPhone = emergencyPhone, InsurancePayerId = payerId, InsuranceMemberId = payerId.Length == 0 ? string.Empty : memberId, RegisteredAt = Clock.Now,
        };
        patients.Add(patient);
        var result = Commit(() =>
        {
            patients.Remove(patient);
            nextPatientId--;
        }, () => Record("Patient registered", PatientTarget(patient), PatientDetailsPath(patient.Id), $"Home clinic {FindClinic(patient.ClinicId)?.Name}{(payerId.Length > 0 ? ", health plan on file" : ", self-pay")}"));

        if (result == HcResult.Ok)
            saved = patient;

        return result;
    }

    /// <summary>Saves the signed-in front desk's work details: the clinic they work at (and get new bookings for) and a work phone (or none).</summary>
    public HcResult SaveStaffProfile(string clinicId, string workPhone)
    {
        if (!(IsFrontDesk || IsBilling) || Identity.Current is not { } account)
            return HcResult.NotAllowed;

        workPhone = workPhone?.Trim() ?? string.Empty;

        if (FindClinic(clinicId) is not { } clinic || (workPhone.Length > 0 && !IsPhone(workPhone)))
            return HcResult.Invalid;

        var profile = ProfileOf(account.Id, clinic.Id);

        if (profile.ClinicId == clinic.Id && profile.WorkPhone == workPhone && staffProfiles.Contains(profile))
            return HcResult.NoChange;

        var changed = new List<string>();

        if (profile.ClinicId != clinic.Id)
            changed.Add($"clinic {FindClinic(profile.ClinicId)?.Name ?? "none"} → {clinic.Name}");

        if (profile.WorkPhone != workPhone)
            changed.Add("work phone");

        var isNew = !staffProfiles.Contains(profile);
        var previous = (profile.ClinicId, profile.WorkPhone);
        (profile.ClinicId, profile.WorkPhone) = (clinic.Id, workPhone);

        if (isNew)
            staffProfiles.Add(profile);

        return Commit(() =>
        {
            (profile.ClinicId, profile.WorkPhone) = previous;

            if (isNew)
                staffProfiles.Remove(profile);
        }, () => Record("Work details changed", $"{(IsBilling ? "Billing" : "Front desk")} {account.Name}", HcSeedData.StaffBase + "/account", "Changed: " + (changed.Count == 0 ? "work details" : string.Join(", ", changed))));
    }

    /// <summary>
    /// Creates (<paramref name="timeOffId" /> 0) or edits a doctor's time off (front desk): whole days, ending today or later,
    /// at most <see cref="MaxTimeOffDays" /> days, not overlapping the doctor's other time off (Conflict). Booked appointments
    /// in it stay booked: <see cref="IsOutsideHours" /> finds them.
    /// </summary>
    public HcResult SaveTimeOff(int timeOffId, string doctorId, DateTime from, DateTime to, string reason, out HcTimeOff? saved)
    {
        saved = null;

        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        var existing = timeOffId == 0 ? null : timeOff.FirstOrDefault(item => item.Id == timeOffId);

        if ((timeOffId != 0 && existing is null) || FindDoctor(doctorId) is not { } doctor)
            return HcResult.NotFound;

        from = from.Date;
        to = to.Date;
        reason = reason?.Trim() ?? string.Empty;

        if (to < from || to < Clock.Today || (to - from).TotalDays + 1 > MaxTimeOffDays || !IsPlausible(from) || !IsPlausible(to) || reason.Length > MaxReasonLength
            || (existing is not null && existing.DoctorId != doctorId))
            return HcResult.Invalid;

        if (timeOff.Any(item => item != existing && item.DoctorId == doctorId && item.From <= to && from <= item.To))
            return HcResult.Conflict;

        var text = $"{HcDisplayText.Day(from)} to {HcDisplayText.Day(to)}";

        if (existing is not null)
        {
            if (existing.From == from && existing.To == to && existing.Reason == reason)
                return HcResult.NoChange;

            var previous = (existing.From, existing.To, existing.Reason);
            var before = $"{HcDisplayText.Day(existing.From)} to {HcDisplayText.Day(existing.To)}";
            (existing.From, existing.To, existing.Reason) = (from, to, reason);
            saved = existing;

            return Commit(() => (existing.From, existing.To, existing.Reason) = previous,
                () => Record("Time off changed", $"Time off · {doctor.Name}", AvailabilityPath(doctorId), before == text ? "Reason changed" : $"{before} → {text}"));
        }

        if (timeOff.Count >= MaxTimeOff || nextTimeOffId >= NextIdLimit)
            return HcResult.Invalid;

        var entry = new HcTimeOff { Id = nextTimeOffId++, DoctorId = doctorId, From = from, To = to, Reason = reason };
        timeOff.Add(entry);
        var result = Commit(() =>
        {
            timeOff.Remove(entry);
            nextTimeOffId--;
        }, () => Record("Time off added", $"Time off · {doctor.Name}", AvailabilityPath(doctorId), text));

        if (result == HcResult.Ok)
            saved = entry;

        return result;
    }

    /// <summary>
    /// Saves the vital signs of the signed-in doctor's visit in progress or completed, checked by <see cref="VitalsProblem" />;
    /// the audit names the measurements taken, not their values.
    /// </summary>
    public HcResult SaveVitals(int appointmentId, HcVitals vitals)
    {
        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && item.DoctorId == doctor.Id) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status is not (HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed))
            return HcResult.WrongState;

        if (vitals is null || VitalsProblem(vitals) is not null)
            return HcResult.Invalid;

        var saved = new HcVitals
        {
            Systolic = vitals.Systolic, Diastolic = vitals.Diastolic, Pulse = vitals.Pulse,
            TemperatureC = vitals.TemperatureC is { } temperature ? Math.Round(temperature, 1) : null, WeightKg = vitals.WeightKg is { } weight ? Math.Round(weight, 1) : null,
        };

        if (appointment.Vitals is { } current && current.Systolic == saved.Systolic && current.Diastolic == saved.Diastolic && current.Pulse == saved.Pulse
            && current.TemperatureC == saved.TemperatureC && current.WeightKg == saved.WeightKg)
            return HcResult.NoChange;

        var previous = appointment.Vitals;
        appointment.Vitals = saved;
        var taken = new[] { saved.Systolic is null ? null : "blood pressure", saved.Pulse is null ? null : "pulse", saved.TemperatureC is null ? null : "temperature", saved.WeightKg is null ? null : "weight" }
            .Where(item => item is not null);

        return Commit(() => appointment.Vitals = previous, () => Record("Vitals saved", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id), "Taken: " + string.Join(", ", taken)));
    }

    /// <summary>Saves the signed-in doctor's notes and patient summary for a visit in progress or completed (plain text); only their lengths are audited.</summary>
    public HcResult SaveVisitNotes(int appointmentId, string notes, string summary)
    {
        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && item.DoctorId == doctor.Id) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status is not (HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed))
            return HcResult.WrongState;

        notes = notes?.Trim() ?? string.Empty;
        summary = summary?.Trim() ?? string.Empty;

        if (notes.Length > MaxNotesLength || summary.Length > MaxSummaryLength)
            return HcResult.Invalid;

        if (appointment.Notes == notes && appointment.Summary == summary)
            return HcResult.NoChange;

        var previous = (appointment.Notes, appointment.Summary);
        (appointment.Notes, appointment.Summary) = (notes, summary);

        return Commit(() => (appointment.Notes, appointment.Summary) = previous,
            () => Record("Visit notes saved", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id), Invariant($"Notes {notes.Length} characters, summary {summary.Length} characters")));
    }

    /// <summary>Records the color mode the theme switcher applied (kept for parity with sites that draw charts).</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (IsDarkTheme == isDark)
            return;

        IsDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>
    /// Returns the open times with a doctor on a day for a visit of <paramref name="minutes" />: their weekly hours there, in
    /// <see cref="SlotMinutes" /> steps, where the whole visit fits in the block and every half hour of it is free, less time
    /// off, from an hour from now up to <see cref="BookingDays" /> days ahead. <paramref name="exceptAppointmentId" /> leaves
    /// one appointment's own times free (for rescheduling it).
    /// </summary>
    public IReadOnlyList<HcSlot> Slots(string doctorId, DateTime date, int minutes = SlotMinutes, int? exceptAppointmentId = null)
    {
        date = date.Date;
        minutes = minutes == HcVisitTypes.NewPatientMinutes ? minutes : SlotMinutes;

        if (date < Clock.Today || date > Clock.Today.AddDays(BookingDays) || FindDoctor(doctorId) is null
            || timeOff.Any(item => item.DoctorId == doctorId && date >= item.From && date <= item.To))
            return Array.Empty<HcSlot>();

        var earliest = Clock.Now.AddHours(1);
        var taken = new HashSet<DateTime>();

        foreach (var item in appointments.Where(item => item.DoctorId == doctorId && item.Start.Date == date && item.Status != HcAppointmentStatus.Cancelled && item.Id != exceptAppointmentId))
        {
            for (var offset = 0; offset < item.Minutes; offset += SlotMinutes)
                taken.Add(item.Start.AddMinutes(offset));
        }

        var list = new List<HcSlot>();

        foreach (var block in availability.Where(item => item.DoctorId == doctorId && item.Day == date.DayOfWeek).OrderBy(item => item.StartMinute))
        {
            for (var minute = block.StartMinute; minute + minutes <= block.EndMinute; minute += SlotMinutes)
            {
                var start = date.AddMinutes(minute);

                if (start < earliest)
                    continue;

                var free = true;

                for (var offset = 0; offset < minutes && free; offset += SlotMinutes)
                    free = !taken.Contains(start.AddMinutes(offset));

                if (free)
                    list.Add(new HcSlot(start, block.ClinicId));
            }
        }

        return list;
    }

    /// <summary>Starts the signed-in doctor's visit with a checked-in patient (WrongState: not checked in yet).</summary>
    public HcResult StartVisit(int appointmentId)
    {
        if (Doctor() is not { } doctor)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && item.DoctorId == doctor.Id) is not { } appointment)
            return HcResult.NotFound;

        if (appointment.Status != HcAppointmentStatus.CheckedIn)
            return HcResult.WrongState;

        appointment.Status = HcAppointmentStatus.InVisit;
        Record("Visit started", AppointmentTarget(appointment), StaffAppointmentPath(appointment.Id), null);
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>Stops an active prescription (doctors); a waiting refill request is closed with it, and the patient hears about it.</summary>
    public HcResult StopPrescription(int prescriptionId)
    {
        if (Doctor() is null)
            return HcResult.NotAllowed;

        if (prescriptions.FirstOrDefault(item => item.Id == prescriptionId) is not { } prescription)
            return HcResult.NotFound;

        if (prescription.Status != HcPrescriptionStatus.Active)
            return HcResult.NoChange;

        var hadRequest = prescription.RefillRequestedAt is not null;
        prescription.Status = HcPrescriptionStatus.Stopped;
        prescription.RefillNote = hadRequest ? "Stopped by your doctor, so no refill." : string.Empty;
        prescription.RefillOutcome = hadRequest ? HcRefillOutcome.Denied : prescription.RefillOutcome;
        prescription.RefillRequestedAt = null;
        prescription.RefillPharmacyId = string.Empty;
        prescription.RefillQuantity = 0;
        prescription.RefillPickupOn = null;

        Record("Prescription stopped", PrescriptionTarget(prescription), PatientChartPath(prescription.PatientId), hadRequest ? "The waiting refill request was closed" : null);
        Notify(patients.FirstOrDefault(item => item.Id == prescription.PatientId)?.AccountId, $"Prescription stopped: {prescription.Medication}",
            $"{Identity.Current?.Name} stopped {prescription.Medication} {prescription.Dose}. Don't start it again unless a doctor tells you to.", SiteBase + "/prescriptions");
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>Suggests the next free medical record number for a new patient, like CL-100321.</summary>
    public string SuggestMrn()
    {
        for (var id = nextPatientId; ; id++)
        {
            var mrn = HcSeedData.MrnFor(id);

            if (!IsMrnTaken(mrn))
                return mrn;
        }
    }

    /// <summary>Returns a doctor's time off, soonest first; the reason is for staff only (others get copies without it).</summary>
    public IReadOnlyList<HcTimeOff> TimeOffOf(string doctorId)
    {
        var list = timeOff.Where(item => item.DoctorId == doctorId).OrderBy(item => item.From);

        return IsStaff ? list.ToList() : list.Select(item => new HcTimeOff { Id = item.Id, DoctorId = item.DoctorId, From = item.From, To = item.To }).ToList();
    }

    /// <summary>
    /// Returns a patient's upcoming appointments (booked and not more than one slot late, checked in, or with the doctor),
    /// soonest first (staff, or the patient themselves).
    /// </summary>
    public IReadOnlyList<HcAppointment> UpcomingOf(int patientId) => CanSeePatient(patientId) ? Upcoming(patientId).Select(ForViewer).ToList() : Array.Empty<HcAppointment>();

    /// <summary>
    /// Returns why vital signs can't be kept, or <see langword="null" />: at least one measurement; both blood pressure values
    /// or neither, systolic 60 to 250 above diastolic 30 to 150 mmHg; pulse 30 to 220 a minute; temperature 34 to 42 °C;
    /// weight 1 to 400 kg.
    /// </summary>
    public static string? VitalsProblem(HcVitals vitals) =>
        vitals.Systolic is null && vitals.Diastolic is null && vitals.Pulse is null && vitals.TemperatureC is null && vitals.WeightKg is null ? "Enter at least one measurement."
        : (vitals.Systolic is null) != (vitals.Diastolic is null) ? "Enter both blood pressure values, or neither."
        : vitals.Systolic is < 60 or > 250 ? "Systolic pressure is 60 to 250 mmHg."
        : vitals.Diastolic is < 30 or > 150 ? "Diastolic pressure is 30 to 150 mmHg."
        : vitals.Systolic <= vitals.Diastolic ? "Systolic pressure must be above diastolic."
        : vitals.Pulse is < 30 or > 220 ? "Pulse is 30 to 220 beats a minute."
        : vitals.TemperatureC is { } temperature && (!double.IsFinite(temperature) || temperature < 34 || temperature > 42) ? "Temperature is 34 to 42 °C."
        : vitals.WeightKg is { } weight && (!double.IsFinite(weight) || weight < 1 || weight > 400) ? "Weight is 1 to 400 kg."
        : null;

    /// <summary>Returns whether a date of birth can be kept: not in the future, and at most <see cref="MaxAgeYears" /> years ago.</summary>
    public bool ValidBirthDate(DateTime dateOfBirth) => dateOfBirth.Date <= Clock.Today && dateOfBirth.Date >= Clock.Today.AddYears(-MaxAgeYears);

    /// <summary>Returns whether a block of hours fits a clinic: a weekday it's open, in half hours within its opening hours, at least one slot long.</summary>
    public static bool ValidBlock(DayOfWeek day, int startMinute, int endMinute, HcClinic clinic) =>
        Enum.IsDefined(day) && HcClinic.IsOpenDay(day) && startMinute % SlotMinutes == 0 && endMinute % SlotMinutes == 0 && startMinute >= Math.Max(DayStartMinute, clinic.OpenMinute)
        && endMinute <= Math.Min(DayEndMinute, clinic.CloseMinute) && endMinute - startMinute >= SlotMinutes;

    /// <summary>Returns whether an emergency contact can be kept: a name and a phone of the right form (not the patient's own), or neither.</summary>
    public static bool ValidEmergencyContact(string? name, string? phone, string? patientPhone)
    {
        name = name?.Trim() ?? string.Empty;
        phone = phone?.Trim() ?? string.Empty;

        if (name.Length == 0 && phone.Length == 0)
            return true;

        return IsText(name, ShowcaseIdentity.MaxNameLength) && IsPhone(phone) && DigitsOf(phone) != DigitsOf(patientPhone);
    }

    #endregion

    #region Private Methods

    private static string AvailabilityPath(string doctorId) => $"{HcSeedData.StaffBase}/availability?doctor={Uri.EscapeDataString(doctorId)}";

    private static string BillsPath(int billId) => Invariant($"{SiteBase}/bills?bill={billId}");

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength - 1), "…");

    /// <summary>The digits of a phone number, to compare two numbers written differently.</summary>
    private static string DigitsOf(string? phone) => new((phone ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (HcSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsEmail(string? value) => value is not null && value.Length <= ShowcaseIdentity.MaxEmailLength && EmailPattern().IsMatch(value);

    private static bool IsPlausible(DateTime value) => value >= HcSeedData.Now.AddYears(-10) && value <= HcSeedData.Now.AddYears(3);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    [GeneratedRegex("^[A-Z]{3}[0-9]{9}$")]
    private static partial Regex MemberIdPattern();

    [GeneratedRegex("^CL-[0-9]{6}$")]
    private static partial Regex MrnPattern();

    private static string PatientChartPath(int patientId) => HcSeedData.PatientChartPath(patientId);

    private static string PatientDetailsPath(int patientId) => HcSeedData.PatientDetailsPath(patientId);

    [GeneratedRegex(@"^\+?[0-9 ().\-]+$")]
    private static partial Regex PhonePattern();

    private static void Replace<T>(List<T> target, List<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private static string StaffAppointmentPath(int appointmentId) => HcSeedData.StaffAppointmentPath(appointmentId);

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    private static bool ValidList(List<string>? items) =>
        items is { Count: <= MaxListItems } && items.All(item => IsText(item, MaxListItemLength)) && Unique(items.Select(item => item.ToLowerInvariant()));

    private HcResult AddToChart(int patientId, string text, Func<HcPatient, List<string>> list, string action, string noun)
    {
        if (Doctor() is null)
            return HcResult.NotAllowed;

        if (patients.FirstOrDefault(item => item.Id == patientId) is not { } patient)
            return HcResult.NotFound;

        text = text?.Trim() ?? string.Empty;
        var items = list(patient);

        if (!IsText(text, MaxListItemLength) || items.Count >= MaxListItems)
            return HcResult.Invalid;

        if (items.Any(item => item.ToLowerInvariant() == text.ToLowerInvariant()))
            return HcResult.NoChange;

        items.Add(text);

        // The item itself is health information, so the log keeps only how many there are.
        return Commit(() => items.Remove(text), () => Record(action, PatientTarget(patient), PatientChartPath(patient.Id), Invariant($"{noun} {items.Count - 1} → {items.Count}")));
    }

    /// <summary>An appointment as the audit log names it: its number and the patient, like "Appointment 1204 · Maya Thompson".</summary>
    private string AppointmentTarget(HcAppointment appointment) =>
        Invariant($"Appointment {appointment.Id} · {patients.FirstOrDefault(item => item.Id == appointment.PatientId)?.Name ?? "a former patient"}");

    /// <summary>A bill as the audit log names it.</summary>
    private string BillTarget(HcBill bill) => Invariant($"Bill {bill.Id} · {patients.FirstOrDefault(item => item.Id == bill.PatientId)?.Name ?? "a former patient"}");

    private bool CanSeePatient(int patientId) => IsStaff || Patient()?.Id == patientId;

    /// <summary>The names of the registration fields that differ from a patient's record, for the audit log (never the values).</summary>
    private static List<string> ChangedFields(HcPatient patient, string mrn, string name, DateTime? dateOfBirth, string phone, string email, string clinicId, string emergencyName,
        string emergencyPhone, string payerId, string memberId)
    {
        var changed = new List<string>();
        void Check(bool same, string field)
        {
            if (!same)
                changed.Add(field);
        }

        Check(patient.Mrn == mrn, "record number");
        Check(patient.Name == name, "name");
        Check(patient.DateOfBirth == dateOfBirth, "date of birth");
        Check(patient.Phone == phone, "phone");
        Check(patient.Email == email, "email");
        Check(patient.ClinicId == clinicId, "home clinic");
        Check(patient.EmergencyContactName == emergencyName && patient.EmergencyContactPhone == emergencyPhone, "emergency contact");
        Check(patient.InsurancePayerId == payerId, "health plan");
        Check(patient.InsuranceMemberId == (payerId.Length == 0 ? string.Empty : memberId), "member ID");

        return changed;
    }

    /// <summary>
    /// Raises <see cref="Changed" /> when the snapshot still fits, after <paramref name="committed" /> (the audit entries and
    /// notifications of the change); otherwise undoes the change and reports Invalid.
    /// </summary>
    private HcResult Commit(Action undo, Action? committed = null)
    {
        if (!FitsSnapshot())
        {
            undo();

            return HcResult.Invalid;
        }

        committed?.Invoke();
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>The signed-in doctor, or <see langword="null" />.</summary>
    private HcDoctor? Doctor() =>
        Identity.Current is { RoleId: HcSeedData.DoctorRole } account ? Doctors.FirstOrDefault(doctor => doctor.AccountId == account.Id) : null;

    /// <summary>Whether the snapshot, as it would be saved now (plus <paramref name="reserve" /> characters), still fits under <see cref="MaxSnapshotLength" />.</summary>
    private bool FitsSnapshot(int reserve = 2048)
    {
        try
        {
            return JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length + reserve <= MaxSnapshotLength;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the appointment itself for staff; for a patient, a copy without the staff notes and codes, and without the
    /// summary until the visit is completed.
    /// </summary>
    private HcAppointment ForViewer(HcAppointment appointment) => IsStaff ? appointment : new HcAppointment
    {
        Id = appointment.Id, PatientId = appointment.PatientId, DoctorId = appointment.DoctorId, ClinicId = appointment.ClinicId, Start = appointment.Start, Minutes = appointment.Minutes,
        VisitType = appointment.VisitType, Reason = appointment.Reason, Status = appointment.Status, CreatedAt = appointment.CreatedAt, CheckedInAt = appointment.CheckedInAt,
        CompletedAt = appointment.CompletedAt, CancelledAt = appointment.CancelledAt, CancelReason = appointment.CancelReason,
        Summary = appointment.Status == HcAppointmentStatus.Completed ? appointment.Summary : string.Empty,
    };

    /// <summary>Returns the order itself for staff; for a patient, a copy without values or comment until it is released.</summary>
    private HcLabOrder ForViewer(HcLabOrder order) => IsStaff || order.Status == HcLabStatus.Released ? order : new HcLabOrder
    {
        Id = order.Id, PatientId = order.PatientId, DoctorId = order.DoctorId, AppointmentId = order.AppointmentId, PanelId = order.PanelId, OrderedAt = order.OrderedAt,
        Status = order.Status,
    };

    private bool HasAppointmentAt(int patientId, DateTime start, int minutes, int? exceptId) =>
        appointments.Any(item => item.PatientId == patientId && item.Id != exceptId && item.Status is not (HcAppointmentStatus.Cancelled or HcAppointmentStatus.NoShow)
            && item.Start < start.AddMinutes(minutes) && start < item.Start.AddMinutes(item.Minutes));

    /// <summary>A block of hours as the audit log names it.</summary>
    private string HoursTarget(HcAvailability block) => $"Hours · {FindDoctor(block.DoctorId)?.Name}";

    /// <summary>A lab order as the audit log names it: its number and test, like "Lab order 45 · Lipid panel".</summary>
    private string LabTarget(HcLabOrder order) => Invariant($"Lab order {order.Id} · {LabPanel(order.PanelId)?.Name ?? order.PanelId}");

    /// <summary>
    /// A new patient account gets a patient record (with a free record number); a patient's name and email follow their
    /// profile. A doctor signing in is reminded of results waiting to be released.
    /// </summary>
    private void OnIdentityChanged()
    {
        if (Identity.Current is { RoleId: HcSeedData.PatientRole } account)
        {
            var email = account.Email.Trim().ToLowerInvariant();
            var name = Clip(account.Name.Trim(), ShowcaseIdentity.MaxNameLength);

            if (patients.FirstOrDefault(patient => patient.AccountId == account.Id) is { } patient)
            {
                patient.Name = name;
                patient.Email = Clip(email, ShowcaseIdentity.MaxEmailLength);
            }
            else if (patients.Count < MaxPatients && nextPatientId < NextIdLimit)
            {
                var mrn = SuggestMrn();
                var created = new HcPatient { Id = nextPatientId++, Mrn = mrn, AccountId = account.Id, Name = name, Email = Clip(email, ShowcaseIdentity.MaxEmailLength), RegisteredAt = Clock.Now };
                patients.Add(created);
                Record("Patient registered", PatientTarget(created), PatientDetailsPath(created.Id), "Registered online");
            }
        }

        RemindLabResults();
        RemindClaims();
        Changed?.Invoke();
    }

    /// <summary>The signed-in patient's record, or <see langword="null" />.</summary>
    private HcPatient? Patient() =>
        Identity.Current is { RoleId: HcSeedData.PatientRole } account ? patients.FirstOrDefault(patient => patient.AccountId == account.Id) : null;

    /// <summary>A patient as the audit log names them: their name and record number, like "Patient Maya Thompson (CL-100001)".</summary>
    private static string PatientTarget(HcPatient patient) => $"Patient {patient.Name} ({patient.Mrn})";

    /// <summary>A prescription as the audit log names it: the medicine and the patient.</summary>
    private string PrescriptionTarget(HcPrescription prescription) =>
        $"Prescription {prescription.Medication} {prescription.Dose} · {patients.FirstOrDefault(item => item.Id == prescription.PatientId)?.Name ?? "a former patient"}";

    /// <summary>Returns a staff account's work details, or new ones at a clinic (not yet kept).</summary>
    private HcStaffProfile ProfileOf(string accountId, string clinicId) =>
        staffProfiles.FirstOrDefault(item => item.AccountId == accountId) ?? new HcStaffProfile { AccountId = accountId, ClinicId = clinicId };

    /// <summary>Raises <see cref="Changed" />, or once at the end of a bulk change.</summary>
    private void RaiseChanged()
    {
        if (batchDepth > 0)
        {
            changedInBatch = true;

            return;
        }

        Changed?.Invoke();
    }

    private HcResult RemoveFromChart(int patientId, string text, Func<HcPatient, List<string>> list, string action, string noun)
    {
        if (Doctor() is null)
            return HcResult.NotAllowed;

        if (patients.FirstOrDefault(item => item.Id == patientId) is not { } patient)
            return HcResult.NotFound;

        var items = list(patient);
        var index = items.FindIndex(item => string.Equals(item, text?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (index < 0)
            return HcResult.NotFound;

        items.RemoveAt(index);
        Record(action, PatientTarget(patient), PatientChartPath(patient.Id), Invariant($"{noun} {items.Count + 1} → {items.Count}"));
        RaiseChanged();

        return HcResult.Ok;
    }

    /// <summary>Sets the ID counters past the highest IDs of a fresh tab's data.</summary>
    private void ResetCounters(HcSeedSet data)
    {
        nextAppointmentId = data.Appointments.Max(item => item.Id) + 1;
        nextAuditId = data.Audit.Count == 0 ? 1 : data.Audit.Max(item => item.Id) + 1;
        nextAvailabilityId = data.Availability.Max(item => item.Id) + 1;
        nextBillId = data.Bills.Max(item => item.Id) + 1;
        nextClaimId = data.Claims.Count == 0 ? 1 : data.Claims.Max(item => item.Id) + 1;
        nextLabOrderId = data.LabOrders.Max(item => item.Id) + 1;
        nextNotificationId = data.Notifications.Count == 0 ? 1 : data.Notifications.Max(item => item.Id) + 1;
        nextPatientId = data.Patients.Max(item => item.Id) + 1;
        nextPrescriptionId = data.Prescriptions.Max(item => item.Id) + 1;
        nextTimeOffId = data.TimeOff.Count == 0 ? 1 : data.TimeOff.Max(item => item.Id) + 1;
    }

    private List<HcAppointment> Upcoming(int patientId) =>
        appointments.Where(item => item.PatientId == patientId
                && (item.Status is HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit || (item.Status == HcAppointmentStatus.Booked && item.Start >= Clock.Now.AddMinutes(-SlotMinutes))))
            .OrderBy(item => item.Start).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets every appointment (staff only), in the order they were booked.</summary>
    public IReadOnlyList<HcAppointment> AllAppointments => IsStaff ? appointments : Array.Empty<HcAppointment>();

    /// <summary>Gets the clinics.</summary>
    public IReadOnlyList<HcClinic> Clinics { get; } = HcSeedData.Clinics();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(HcSeedData.Now);

    /// <summary>Gets the signed-in doctor, or <see langword="null" />.</summary>
    public HcDoctor? CurrentDoctor => Doctor();

    /// <summary>Gets the signed-in patient's record, or <see langword="null" />.</summary>
    public HcPatient? CurrentPatient => Patient();

    /// <summary>Gets the signed-in staff account's work details (a doctor's clinic is their record's), or <see langword="null" />.</summary>
    public HcStaffProfile? CurrentStaffProfile =>
        IsStaff && Identity.Current is { } account ? ProfileOf(account.Id, Doctor()?.ClinicId ?? Clinics[0].Id) : null;

    /// <summary>Gets the doctors (their work details change on their account page).</summary>
    public IReadOnlyList<HcDoctor> Doctors { get; }

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the theme switcher applied dark mode.</summary>
    public bool IsDarkTheme { get; private set; }

    /// <summary>Gets whether the signed-in account is the front desk.</summary>
    public bool IsFrontDesk => Identity.Current?.RoleId == HcSeedData.FrontDeskRole;

    /// <summary>Gets whether the signed-in account is staff (a doctor with a record, the front desk, or billing).</summary>
    public bool IsStaff => IsFrontDesk || IsBilling || Doctor() is not null;

    /// <summary>Gets the lab tests doctors can order.</summary>
    public IReadOnlyList<HcLabPanel> LabPanels { get; } = HcSeedData.LabPanels();

    /// <summary>
    /// Gets the signed-in doctor's lab orders: waiting for results first, then entered but not released, then released, newest
    /// first within each.
    /// </summary>
    public IReadOnlyList<HcLabOrder> MyLabWork =>
        Doctor() is { } doctor ? labOrders.Where(item => item.DoctorId == doctor.Id).OrderBy(item => item.Status).ThenByDescending(item => item.OrderedAt).ToList() : Array.Empty<HcLabOrder>();

    /// <summary>Gets the patients (staff only), by name.</summary>
    public IReadOnlyList<HcPatient> Patients => IsStaff ? patients.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList() : Array.Empty<HcPatient>();

    /// <summary>Gets the health plans patients can have on file.</summary>
    public IReadOnlyList<HcPayer> Payers { get; } = HcSeedData.Payers();

    /// <summary>Gets the pharmacies patients can pick up refills at.</summary>
    public IReadOnlyList<HcPharmacy> Pharmacies { get; } = HcSeedData.Pharmacies();

    /// <summary>Gets the refill requests waiting for a doctor (doctors only), oldest first.</summary>
    public IReadOnlyList<HcPrescription> RefillRequests =>
        Doctor() is not null ? prescriptions.Where(item => item.RefillRequestedAt is not null).OrderBy(item => item.RefillRequestedAt).ToList() : Array.Empty<HcPrescription>();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "healthcare";

    /// <summary>Gets the specialties of the doctors, by name.</summary>
    public IReadOnlyList<string> Specialties => Doctors.Select(doctor => doctor.Specialty).Distinct().Order().ToList();

    private static string RefillsPath => HcSeedData.StaffBase + "/refills";

    private static string SiteBase => HcSeedData.SiteBase;

    #endregion

    #region Events

    /// <summary>Raised after any change, so open pages re-render and the session store saves.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes.</summary>
    public event Action? ThemeChanged;

    #endregion
}
