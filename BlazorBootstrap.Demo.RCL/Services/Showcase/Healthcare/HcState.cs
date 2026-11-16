using System.Text.Json;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>An open appointment time with a doctor, at the clinic of that block of hours.</summary>
public sealed record HcSlot(DateTime Start, string ClinicId);

/// <summary>
/// State of "Cedarline Health", a fictitious clinic network's portal, for the current browser tab: patients and their
/// appointments, records, lab orders, prescriptions, and bills; doctors' weekly hours and time off; and the demo sign-in.
/// It is kept in the tab's <c>sessionStorage</c> through the showcase foundation; nothing is sent anywhere, and nothing is
/// medical advice.
/// </summary>
/// <remarks>
/// Rights come from the identity role. A patient books, reschedules, and cancels their own appointments (open times come
/// from the doctor's weekly hours, less time off and booked times), requests refills, and pays bills with a test card. A
/// doctor runs their visits (start, notes, labs, prescriptions, complete — which bills the visit), enters and releases lab
/// results (which bills the test), edits charts, and answers refill requests. The front desk checks patients in, marks
/// no-shows, cancels, and keeps doctors' hours and time off.
/// </remarks>
public sealed partial class HcState : IShowcaseSiteState
{
    #region Fields and Constants

    /// <summary>How many days ahead patients can book.</summary>
    public const int BookingDays = 60;

    /// <summary>The longest list item (an allergy or a condition).</summary>
    public const int MaxListItemLength = 80;

    /// <summary>The most allergies or conditions on a chart.</summary>
    public const int MaxListItems = 20;

    /// <summary>The longest lab comment.</summary>
    public const int MaxCommentLength = 1000;

    /// <summary>The longest prescription directions.</summary>
    public const int MaxDirectionsLength = 200;

    /// <summary>The longest dose.</summary>
    public const int MaxDoseLength = 40;

    /// <summary>The longest medication name.</summary>
    public const int MaxMedicationLength = 60;

    /// <summary>The longest visit note.</summary>
    public const int MaxNotesLength = 4000;

    /// <summary>The longest phone number.</summary>
    public const int MaxPhoneLength = 30;

    /// <summary>The highest prescription quantity.</summary>
    public const int MaxQuantity = 365;

    /// <summary>The longest visit reason, cancel reason, time-off reason, or refill answer.</summary>
    public const int MaxReasonLength = 200;

    /// <summary>The most refills on a prescription.</summary>
    public const int MaxRefills = 11;

    /// <summary>The longest visit summary for the patient.</summary>
    public const int MaxSummaryLength = 2000;

    /// <summary>The longest time off, in days.</summary>
    public const int MaxTimeOffDays = 60;

    /// <summary>The most upcoming appointments a patient can hold.</summary>
    public const int MaxUpcoming = 5;

    /// <summary>The length of an appointment, in minutes.</summary>
    public const int SlotMinutes = 30;

    /// <summary>The earliest a block of hours may start (7:00), in minutes after midnight.</summary>
    public const int DayStartMinute = 7 * 60;

    /// <summary>The latest a block of hours may end (20:00), in minutes after midnight.</summary>
    public const int DayEndMinute = 20 * 60;

    private const int MaxAppointments = 600;

    /// <summary>The longest bill description (a visit's is clipped to it).</summary>
    private const int MaxBillDescriptionLength = 200;

    private const int MaxAvailability = 150;

    private const int MaxBills = 600;

    private const int MaxLabOrders = 300;

    private const int MaxPatients = 10 + ShowcaseIdentity.MaxAccounts;

    private const int MaxPrescriptions = 300;

    /// <summary>The largest the site's snapshot may grow: the store's limit less room for its envelope.</summary>
    private const int MaxSnapshotLength = ShowcaseSessionStore.MaxSnapshotLength - (48 * 1024);

    private const int MaxTimeOff = 80;

    private const int NextIdLimit = 1_000_000;

    private static readonly JsonSerializerOptions snapshotOptions = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly List<HcAppointment> appointments = HcSeedData.Appointments();

    private readonly List<HcAvailability> availability = HcSeedData.Availability();

    private readonly List<HcBill> bills = HcSeedData.Bills();

    private readonly List<HcLabOrder> labOrders = HcSeedData.LabOrders();

    private readonly List<HcPatient> patients = HcSeedData.Patients();

    private readonly List<HcPrescription> prescriptions = HcSeedData.Prescriptions();

    private readonly List<HcTimeOff> timeOff = HcSeedData.TimeOff();

    private int nextAppointmentId;

    private int nextAvailabilityId;

    private int nextBillId;

    private int nextLabOrderId;

    private int nextPatientId;

    private int nextPrescriptionId;

    private int nextTimeOffId;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh tab, signed out.</summary>
    public HcState()
    {
        nextAppointmentId = appointments.Max(item => item.Id) + 1;
        nextAvailabilityId = availability.Max(item => item.Id) + 1;
        nextBillId = bills.Max(item => item.Id) + 1;
        nextLabOrderId = labOrders.Max(item => item.Id) + 1;
        nextPatientId = patients.Max(item => item.Id) + 1;
        nextPrescriptionId = prescriptions.Max(item => item.Id) + 1;
        nextTimeOffId = timeOff.Max(item => item.Id) + 1;
        Identity = new ShowcaseIdentity(HcSeedData.Roles(), HcSeedData.DemoAccounts());
        Identity.Changed += OnIdentityChanged;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Adds an allergy to a patient's chart (doctors).</summary>
    public HcResult AddAllergy(int patientId, string text) => AddToChart(patientId, text, patient => patient.Allergies);

    /// <summary>Adds a condition to a patient's chart (doctors).</summary>
    public HcResult AddCondition(int patientId, string text) => AddToChart(patientId, text, patient => patient.Conditions);

    /// <summary>Returns the appointments starting on a day (staff: everyone's, optionally one doctor's), by start time.</summary>
    public IReadOnlyList<HcAppointment> AppointmentsOn(DateTime date, string? doctorId = null) =>
        IsStaff ? appointments.Where(item => item.Start.Date == date.Date && (doctorId is null || item.DoctorId == doctorId)).OrderBy(item => item.Start).ToList() : Array.Empty<HcAppointment>();

    /// <summary>Returns a doctor's weekly hours, by day and start.</summary>
    public IReadOnlyList<HcAvailability> AvailabilityOf(string doctorId) =>
        availability.Where(item => item.DoctorId == doctorId).OrderBy(item => ((int)item.Day + 6) % 7).ThenBy(item => item.StartMinute).ToList();

    /// <summary>Returns a patient's bills, due first (staff, or the patient themselves).</summary>
    public IReadOnlyList<HcBill> BillsOf(int patientId) =>
        CanSeePatient(patientId) ? bills.Where(item => item.PatientId == patientId).OrderBy(item => item.Status).ThenByDescending(item => item.CreatedAt).ToList() : Array.Empty<HcBill>();

    /// <summary>
    /// Books an open time with a doctor for the signed-in patient. Unavailable: the time isn't open (taken, outside hours,
    /// time off, too soon, or too far ahead); Conflict: the patient has another appointment then; WrongState: the doctor
    /// isn't taking new patients and hasn't seen this one; Invalid: no reason, or already <see cref="MaxUpcoming" /> upcoming.
    /// </summary>
    public HcResult Book(string doctorId, DateTime start, string reason, out HcAppointment? booked)
    {
        booked = null;

        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        if (FindDoctor(doctorId) is not { } doctor)
            return HcResult.NotFound;

        reason = reason?.Trim() ?? string.Empty;

        if (!IsText(reason, MaxReasonLength) || Upcoming(patient.Id).Count >= MaxUpcoming || appointments.Count >= MaxAppointments || nextAppointmentId >= NextIdLimit)
            return HcResult.Invalid;

        if (!doctor.AcceptingNewPatients
            && !appointments.Any(item => item.PatientId == patient.Id && item.DoctorId == doctor.Id && item.Status is HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed))
            return HcResult.WrongState;

        if (Slots(doctor.Id, start.Date).FirstOrDefault(slot => slot.Start == start) is not { } slot)
            return HcResult.Unavailable;

        if (HasAppointmentAt(patient.Id, start, null))
            return HcResult.Conflict;

        var appointment = new HcAppointment
        {
            Id = nextAppointmentId++, PatientId = patient.Id, DoctorId = doctor.Id, ClinicId = slot.ClinicId, Start = start, Minutes = SlotMinutes, Reason = reason,
            Status = HcAppointmentStatus.Booked, CreatedAt = Clock.Now,
        };
        appointments.Add(appointment);
        var result = Commit(() =>
        {
            appointments.Remove(appointment);
            nextAppointmentId--;
        });

        if (result == HcResult.Ok)
            booked = appointment;

        return result;
    }

    /// <summary>
    /// Cancels an appointment: the patient their own booked one up to 2 hours before it starts; the front desk any booked or
    /// checked-in one. The time opens again for others.
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

        return Commit(() => (appointment.Status, appointment.CancelledAt, appointment.CancelReason) = previous);
    }

    /// <summary>Returns the snapshot: every collection and the ID counters.</summary>
    public object CaptureData() => new HcSnapshot
    {
        Appointments = appointments.ToList(),
        Availability = availability.ToList(),
        Bills = bills.ToList(),
        LabOrders = labOrders.ToList(),
        NextAppointmentId = nextAppointmentId,
        NextAvailabilityId = nextAvailabilityId,
        NextBillId = nextBillId,
        NextLabOrderId = nextLabOrderId,
        NextPatientId = nextPatientId,
        NextPrescriptionId = nextPrescriptionId,
        NextTimeOffId = nextTimeOffId,
        Patients = patients.ToList(),
        Prescriptions = prescriptions.ToList(),
        TimeOff = timeOff.ToList(),
    };

    /// <summary>Checks a patient in for today's appointment (front desk).</summary>
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
        Changed?.Invoke();

        return HcResult.Ok;
    }

    /// <summary>
    /// Completes the signed-in doctor's visit in progress: the patient sees the summary under their records, and the visit is
    /// billed (primary care or specialist fee, due in 30 days).
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
        var created = new HcBill
        {
            Id = nextBillId++, PatientId = appointment.PatientId, AppointmentId = appointment.Id, AmountCents = fee, CreatedAt = Clock.Now, DueDate = Clock.Today.AddDays(30),
            Description = Clip($"Visit: {appointment.Reason.ToLowerInvariant()} with {doctor.Name}", MaxBillDescriptionLength),
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
        });

        if (result == HcResult.Ok)
            bill = created;

        return result;
    }

    /// <summary>Answers a patient's refill request (doctors): approving adds one refill; declining needs a note for the patient.</summary>
    public HcResult DecideRefill(int prescriptionId, bool approve, string note)
    {
        if (Doctor() is null)
            return HcResult.NotAllowed;

        if (prescriptions.FirstOrDefault(item => item.Id == prescriptionId) is not { } prescription)
            return HcResult.NotFound;

        if (prescription.RefillRequestedAt is null || prescription.Status != HcPrescriptionStatus.Active)
            return HcResult.WrongState;

        note = note?.Trim() ?? string.Empty;

        if (note.Length > MaxReasonLength || (!approve && note.Length == 0) || (approve && prescription.Refills >= MaxRefills))
            return HcResult.Invalid;

        var previous = (prescription.Refills, prescription.RefillNote, prescription.RefillRequestedAt);
        prescription.Refills += approve ? 1 : 0;
        prescription.RefillNote = approve ? (note.Length > 0 ? note : "Approved: one refill added.") : note;
        prescription.RefillRequestedAt = null;

        return Commit(() => (prescription.Refills, prescription.RefillNote, prescription.RefillRequestedAt) = previous);
    }

    /// <summary>Deletes a block of weekly hours (front desk); booked appointments in it stay booked.</summary>
    public HcResult DeleteAvailability(int availabilityId)
    {
        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        if (availability.FirstOrDefault(item => item.Id == availabilityId) is not { } block)
            return HcResult.NotFound;

        availability.Remove(block);
        Changed?.Invoke();

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
        Changed?.Invoke();

        return HcResult.Ok;
    }

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

        if (values is null || values.Count != panel.Analytes.Count || values.Any(value => !double.IsFinite(value) || value is < 0 or > 100_000) || comment.Length > MaxCommentLength)
            return HcResult.Invalid;

        var previous = (order.Values, order.Comment, order.Status, order.ResultedAt);
        order.Values = values.Select(value => Math.Round(value, 2)).ToList();
        order.Comment = comment;
        order.Status = HcLabStatus.Resulted;
        order.ResultedAt = Clock.Now;

        return Commit(() => (order.Values, order.Comment, order.Status, order.ResultedAt) = previous);
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

    /// <summary>Returns whether a value is below, inside, or above an analyte's reference range: "Low", "Normal", or "High".</summary>
    public static string Flag(HcAnalyte analyte, double value) => value < analyte.Low ? "Low" : value > analyte.High ? "High" : "Normal";

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
        Changed?.Invoke();

        return HcResult.Ok;
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
        });

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

        if (bill.Status == HcBillStatus.Paid)
            return HcResult.WrongState;

        if (charge is null || !charge.IsApproved)
            return HcResult.CardDeclined;

        bill.Status = HcBillStatus.Paid;
        bill.PaidAt = Clock.Now;
        bill.CardBrand = Clip(charge.Brand ?? string.Empty, 20);
        bill.CardLast4 = Clip(charge.Last4 ?? string.Empty, 4);
        Changed?.Invoke();

        return HcResult.Ok;
    }

    /// <summary>Prescribes a medication in the signed-in doctor's visit (in progress or completed).</summary>
    public HcResult Prescribe(int appointmentId, string medication, string dose, string directions, int quantity, int refills, out HcPrescription? prescribed)
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
            || prescriptions.Count >= MaxPrescriptions || nextPrescriptionId >= NextIdLimit)
            return HcResult.Invalid;

        var prescription = new HcPrescription
        {
            Id = nextPrescriptionId++, PatientId = appointment.PatientId, DoctorId = doctor.Id, Medication = medication, Dose = dose, Directions = directions, Quantity = quantity, Refills = refills,
            PrescribedAt = Clock.Now, Status = HcPrescriptionStatus.Active,
        };
        prescriptions.Add(prescription);
        var result = Commit(() =>
        {
            prescriptions.Remove(prescription);
            nextPrescriptionId--;
        });

        if (result == HcResult.Ok)
            prescribed = prescription;

        return result;
    }

    /// <summary>Returns a patient's prescriptions, active first (staff, or the patient themselves).</summary>
    public IReadOnlyList<HcPrescription> PrescriptionsOf(int patientId) =>
        CanSeePatient(patientId)
            ? prescriptions.Where(item => item.PatientId == patientId).OrderBy(item => item.Status).ThenByDescending(item => item.PrescribedAt).ToList()
            : Array.Empty<HcPrescription>();

    /// <summary>Releases a lab order's entered results to the patient (the ordering doctor), and bills the test.</summary>
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
            Description = Clip($"Lab: {panel.Name.ToLowerInvariant()}", MaxBillDescriptionLength),
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
        });
    }

    /// <summary>Removes an allergy from a patient's chart (doctors).</summary>
    public HcResult RemoveAllergy(int patientId, string text) => RemoveFromChart(patientId, text, patient => patient.Allergies);

    /// <summary>Removes a condition from a patient's chart (doctors).</summary>
    public HcResult RemoveCondition(int patientId, string text) => RemoveFromChart(patientId, text, patient => patient.Conditions);

    /// <summary>
    /// Asks the doctor for a refill of one of the signed-in patient's active prescriptions with no refills left (WrongState
    /// when refills remain, it's stopped, or a request is already waiting).
    /// </summary>
    public HcResult RequestRefill(int prescriptionId)
    {
        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        if (prescriptions.FirstOrDefault(item => item.Id == prescriptionId && item.PatientId == patient.Id) is not { } prescription)
            return HcResult.NotFound;

        if (prescription.Status != HcPrescriptionStatus.Active || prescription.Refills > 0 || prescription.RefillRequestedAt is not null)
            return HcResult.WrongState;

        prescription.RefillRequestedAt = Clock.Now;
        prescription.RefillNote = string.Empty;
        Changed?.Invoke();

        return HcResult.Ok;
    }

    /// <summary>
    /// Moves one of the signed-in patient's booked appointments to another open time with the same doctor, up to 2 hours
    /// before it starts (WrongState after that).
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

        if (Slots(appointment.DoctorId, newStart.Date).FirstOrDefault(slot => slot.Start == newStart) is not { } slot)
            return HcResult.Unavailable;

        if (HasAppointmentAt(patient.Id, newStart, appointment.Id))
            return HcResult.Conflict;

        appointment.Start = newStart;
        appointment.ClinicId = slot.ClinicId;
        Changed?.Invoke();

        return HcResult.Ok;
    }

    /// <summary>
    /// Creates (<paramref name="availabilityId" /> 0) or edits a block of a doctor's weekly hours (front desk): whole and half
    /// hours between 7:00 and 20:00, at least one appointment long, not overlapping the doctor's other blocks that day (Conflict).
    /// </summary>
    public HcResult SaveAvailability(int availabilityId, string doctorId, DayOfWeek day, int startMinute, int endMinute, string clinicId, out HcAvailability? saved)
    {
        saved = null;

        if (!IsFrontDesk)
            return HcResult.NotAllowed;

        var existing = availabilityId == 0 ? null : availability.FirstOrDefault(item => item.Id == availabilityId);

        if ((availabilityId != 0 && existing is null) || FindDoctor(doctorId) is null || FindClinic(clinicId) is null)
            return HcResult.NotFound;

        if (!Enum.IsDefined(day) || startMinute % SlotMinutes != 0 || endMinute % SlotMinutes != 0 || startMinute < DayStartMinute || endMinute > DayEndMinute
            || endMinute - startMinute < SlotMinutes || (existing is not null && existing.DoctorId != doctorId))
            return HcResult.Invalid;

        if (availability.Any(item => item != existing && item.DoctorId == doctorId && item.Day == day && item.StartMinute < endMinute && startMinute < item.EndMinute))
            return HcResult.Conflict;

        if (existing is not null)
        {
            if (existing.Day == day && existing.StartMinute == startMinute && existing.EndMinute == endMinute && existing.ClinicId == clinicId)
                return HcResult.NoChange;

            (existing.Day, existing.StartMinute, existing.EndMinute, existing.ClinicId) = (day, startMinute, endMinute, clinicId);
            saved = existing;
            Changed?.Invoke();

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
        });

        if (result == HcResult.Ok)
            saved = block;

        return result;
    }

    /// <summary>Saves the signed-in patient's date of birth and phone number.</summary>
    public HcResult SaveMyDetails(DateTime? dateOfBirth, string phone)
    {
        if (Patient() is not { } patient)
            return HcResult.NotAllowed;

        phone = phone?.Trim() ?? string.Empty;

        if (phone.Length > MaxPhoneLength || (dateOfBirth is { } dob && (dob.Date < new DateTime(1900, 1, 1) || dob.Date > Clock.Today)))
            return HcResult.Invalid;

        if (patient.DateOfBirth == dateOfBirth?.Date && patient.Phone == phone)
            return HcResult.NoChange;

        var previous = (patient.DateOfBirth, patient.Phone);
        (patient.DateOfBirth, patient.Phone) = (dateOfBirth?.Date, phone);

        return Commit(() => (patient.DateOfBirth, patient.Phone) = previous);
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

        if ((timeOffId != 0 && existing is null) || FindDoctor(doctorId) is null)
            return HcResult.NotFound;

        from = from.Date;
        to = to.Date;
        reason = reason?.Trim() ?? string.Empty;

        if (to < from || to < Clock.Today || (to - from).TotalDays + 1 > MaxTimeOffDays || !IsPlausible(from) || !IsPlausible(to) || reason.Length > MaxReasonLength
            || (existing is not null && existing.DoctorId != doctorId))
            return HcResult.Invalid;

        if (timeOff.Any(item => item != existing && item.DoctorId == doctorId && item.From <= to && from <= item.To))
            return HcResult.Conflict;

        if (existing is not null)
        {
            if (existing.From == from && existing.To == to && existing.Reason == reason)
                return HcResult.NoChange;

            var previous = (existing.From, existing.To, existing.Reason);
            (existing.From, existing.To, existing.Reason) = (from, to, reason);
            saved = existing;

            return Commit(() => (existing.From, existing.To, existing.Reason) = previous);
        }

        if (timeOff.Count >= MaxTimeOff || nextTimeOffId >= NextIdLimit)
            return HcResult.Invalid;

        var entry = new HcTimeOff { Id = nextTimeOffId++, DoctorId = doctorId, From = from, To = to, Reason = reason };
        timeOff.Add(entry);
        var result = Commit(() =>
        {
            timeOff.Remove(entry);
            nextTimeOffId--;
        });

        if (result == HcResult.Ok)
            saved = entry;

        return result;
    }

    /// <summary>Saves the signed-in doctor's notes and patient summary for a visit in progress or completed (plain text).</summary>
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

        return Commit(() => (appointment.Notes, appointment.Summary) = previous);
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
    /// Returns the open times with a doctor on a day: their weekly hours there, in <see cref="SlotMinutes" /> steps, less time
    /// off and times already booked, from an hour from now up to <see cref="BookingDays" /> days ahead.
    /// </summary>
    public IReadOnlyList<HcSlot> Slots(string doctorId, DateTime date)
    {
        date = date.Date;

        if (date < Clock.Today || date > Clock.Today.AddDays(BookingDays) || FindDoctor(doctorId) is null
            || timeOff.Any(item => item.DoctorId == doctorId && date >= item.From && date <= item.To))
            return Array.Empty<HcSlot>();

        var earliest = Clock.Now.AddHours(1);
        var taken = appointments.Where(item => item.DoctorId == doctorId && item.Start.Date == date && item.Status != HcAppointmentStatus.Cancelled).Select(item => item.Start).ToHashSet();
        var list = new List<HcSlot>();

        foreach (var block in availability.Where(item => item.DoctorId == doctorId && item.Day == date.DayOfWeek).OrderBy(item => item.StartMinute))
        {
            for (var minute = block.StartMinute; minute + SlotMinutes <= block.EndMinute; minute += SlotMinutes)
            {
                var start = date.AddMinutes(minute);

                if (start >= earliest && !taken.Contains(start))
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
        Changed?.Invoke();

        return HcResult.Ok;
    }

    /// <summary>Stops an active prescription (doctors); a waiting refill request is closed with it.</summary>
    public HcResult StopPrescription(int prescriptionId)
    {
        if (Doctor() is null)
            return HcResult.NotAllowed;

        if (prescriptions.FirstOrDefault(item => item.Id == prescriptionId) is not { } prescription)
            return HcResult.NotFound;

        if (prescription.Status != HcPrescriptionStatus.Active)
            return HcResult.NoChange;

        prescription.Status = HcPrescriptionStatus.Stopped;
        prescription.RefillNote = prescription.RefillRequestedAt is not null ? "Stopped by your doctor, so no refill." : string.Empty;
        prescription.RefillRequestedAt = null;

        Changed?.Invoke();

        return HcResult.Ok;
    }

    /// <summary>Returns a doctor's time off, soonest first; the reason is for staff only (others get copies without it).</summary>
    public IReadOnlyList<HcTimeOff> TimeOffOf(string doctorId)
    {
        var list = timeOff.Where(item => item.DoctorId == doctorId).OrderBy(item => item.From);

        return IsStaff ? list.ToList() : list.Select(item => new HcTimeOff { Id = item.Id, DoctorId = item.DoctorId, From = item.From, To = item.To }).ToList();
    }

    /// <summary>Validates a stored snapshot against its own data and applies it only when every part is valid.</summary>
    public bool TryRestoreData(JsonElement data)
    {
        HcSnapshot? snapshot;

        try
        {
            snapshot = data.Deserialize<HcSnapshot>(snapshotOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            return false;
        }

        if (snapshot is not
            {
                Appointments: { Count: <= MaxAppointments } storedAppointments,
                Availability: { Count: <= MaxAvailability } storedAvailability,
                Bills: { Count: <= MaxBills } storedBills,
                LabOrders: { Count: <= MaxLabOrders } storedLabs,
                Patients: { Count: > 0 and <= MaxPatients } storedPatients,
                Prescriptions: { Count: <= MaxPrescriptions } storedPrescriptions,
                TimeOff: { Count: <= MaxTimeOff } storedTimeOff,
            }
            || new[] { snapshot.NextAppointmentId, snapshot.NextAvailabilityId, snapshot.NextBillId, snapshot.NextLabOrderId, snapshot.NextPatientId, snapshot.NextPrescriptionId, snapshot.NextTimeOffId }
                .Any(id => id is < 1 or > NextIdLimit))
            return false;

        // Patients: unique IDs, emails, and accounts; the demo patient keeps her account.
        var seededAccounts = HcSeedData.Patients().ToDictionary(patient => patient.Id, patient => patient.AccountId);

        // A seeded record keeps its account (or none) exactly; any other record belongs to one registered account.
        if (storedPatients.Any(patient => patient is null || patient.Id < 1 || patient.Id >= snapshot.NextPatientId || !IsText(patient.Name, ShowcaseIdentity.MaxNameLength)
                || !IsText(patient.Email, ShowcaseIdentity.MaxEmailLength)
                || (seededAccounts.TryGetValue(patient.Id, out var seededAccount) ? patient.AccountId != seededAccount : patient.AccountId is null || !patient.AccountId.StartsWith("u-", StringComparison.Ordinal))
                || patient.Phone is not { Length: <= MaxPhoneLength } || (patient.DateOfBirth is { } dob && (dob < new DateTime(1900, 1, 1) || dob > HcSeedData.Now.AddYears(2)))
                || (patient.AccountId is not null && !IsAccountId(patient.AccountId)) || !ValidList(patient.Allergies) || !ValidList(patient.Conditions)
                || patient.Immunizations is not { Count: <= MaxListItems } || patient.Immunizations.Any(item => item is null || !IsText(item.Name, MaxListItemLength) || !IsPlausible(item.Date))
                || !Unique(patient.Immunizations.Select(item => (item.Name.ToLowerInvariant(), item.Date))))
            || !Unique(storedPatients.Select(patient => patient.Id)) || !Unique(storedPatients.Where(patient => patient.AccountId is not null).Select(patient => patient.AccountId))
            || storedPatients.FirstOrDefault(patient => patient.Id == HcSeedData.SeededPatientId)?.AccountId is not (null or HcSeedData.SeededPatientAccountId))
            return false;

        var patientIds = storedPatients.Select(patient => patient.Id).ToHashSet();

        if (storedAppointments.Any(appointment => appointment is null || appointment.Id < 1 || appointment.Id >= snapshot.NextAppointmentId || !patientIds.Contains(appointment.PatientId)
                || FindDoctor(appointment.DoctorId) is null || FindClinic(appointment.ClinicId) is null || !IsPlausible(appointment.Start) || appointment.Start.TimeOfDay.Ticks % TimeSpan.FromMinutes(SlotMinutes).Ticks != 0
                || appointment.Minutes != SlotMinutes || !IsText(appointment.Reason, MaxReasonLength) || !Enum.IsDefined(appointment.Status) || !IsPlausible(appointment.CreatedAt)
                || appointment.Notes is not { Length: <= MaxNotesLength } || appointment.Summary is not { Length: <= MaxSummaryLength } || appointment.CancelReason is not { Length: <= MaxReasonLength }
                || (appointment.CancelledAt is not null) != (appointment.Status == HcAppointmentStatus.Cancelled)
                || (appointment.CompletedAt is not null) != (appointment.Status == HcAppointmentStatus.Completed)
                || (appointment.CheckedInAt is null && appointment.Status is HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit))
            || !Unique(storedAppointments.Select(appointment => appointment.Id)))
            return false;

        // Two live appointments never hold the same doctor's time, and a patient is never in two visits at once (all are
        // one half-hour slot, so overlapping means the same start).
        if (!Unique(storedAppointments.Where(appointment => appointment.Status != HcAppointmentStatus.Cancelled).Select(appointment => (appointment.DoctorId, appointment.Start)))
            || !Unique(storedAppointments.Where(appointment => appointment.Status is not (HcAppointmentStatus.Cancelled or HcAppointmentStatus.NoShow)).Select(appointment => (appointment.PatientId, appointment.Start))))
            return false;

        if (storedAvailability.Any(block => block is null || block.Id < 1 || block.Id >= snapshot.NextAvailabilityId || FindDoctor(block.DoctorId) is null || FindClinic(block.ClinicId) is null
                || !Enum.IsDefined(block.Day) || block.StartMinute % SlotMinutes != 0 || block.EndMinute % SlotMinutes != 0 || block.StartMinute < DayStartMinute || block.EndMinute > DayEndMinute
                || block.EndMinute - block.StartMinute < SlotMinutes)
            || !Unique(storedAvailability.Select(block => block.Id))
            || storedAvailability.GroupBy(block => (block.DoctorId, block.Day)).Any(group => group.OrderBy(block => block.StartMinute).Zip(group.OrderBy(block => block.StartMinute).Skip(1)).Any(pair => pair.Second.StartMinute < pair.First.EndMinute)))
            return false;

        if (storedTimeOff.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextTimeOffId || FindDoctor(entry.DoctorId) is null || entry.From != entry.From.Date || entry.To != entry.To.Date
                || entry.To < entry.From || (entry.To - entry.From).TotalDays + 1 > MaxTimeOffDays || !IsPlausible(entry.From) || !IsPlausible(entry.To) || entry.Reason is not { Length: <= MaxReasonLength })
            || !Unique(storedTimeOff.Select(entry => entry.Id)))
            return false;

        var appointmentsById = storedAppointments.ToDictionary(appointment => appointment.Id);

        if (storedLabs.Any(order => order is null || order.Id < 1 || order.Id >= snapshot.NextLabOrderId || !patientIds.Contains(order.PatientId) || FindDoctor(order.DoctorId) is null
                || LabPanel(order.PanelId) is not { } panel || !Enum.IsDefined(order.Status) || !IsPlausible(order.OrderedAt) || order.Comment is not { Length: <= MaxCommentLength }
                || (order.AppointmentId is { } appointmentId && (!appointmentsById.TryGetValue(appointmentId, out var visit) || visit.PatientId != order.PatientId))
                || order.Values is null || (order.Status == HcLabStatus.Ordered ? order.Values.Count != 0 || order.ResultedAt is not null : order.Values.Count != panel.Analytes.Count || order.ResultedAt is null)
                || order.Values.Any(value => !double.IsFinite(value) || value is < 0 or > 100_000) || (order.ReleasedAt is not null) != (order.Status == HcLabStatus.Released))
            || !Unique(storedLabs.Select(order => order.Id)))
            return false;

        if (storedPrescriptions.Any(prescription => prescription is null || prescription.Id < 1 || prescription.Id >= snapshot.NextPrescriptionId || !patientIds.Contains(prescription.PatientId)
                || FindDoctor(prescription.DoctorId) is null || !IsText(prescription.Medication, MaxMedicationLength) || !IsText(prescription.Dose, MaxDoseLength)
                || !IsText(prescription.Directions, MaxDirectionsLength) || prescription.Quantity is < 1 or > MaxQuantity || prescription.Refills is < 0 or > MaxRefills
                || !Enum.IsDefined(prescription.Status) || !IsPlausible(prescription.PrescribedAt) || prescription.RefillNote is not { Length: <= MaxReasonLength }
                || (prescription.RefillRequestedAt is not null && (prescription.Status != HcPrescriptionStatus.Active || !IsPlausible(prescription.RefillRequestedAt.Value))))
            || !Unique(storedPrescriptions.Select(prescription => prescription.Id)))
            return false;

        var labIds = storedLabs.Select(order => order.Id).ToHashSet();

        if (storedBills.Any(bill => bill is null || bill.Id < 1 || bill.Id >= snapshot.NextBillId || !patientIds.Contains(bill.PatientId) || bill.AmountCents is < 1 or > 1_000_000
                || !IsText(bill.Description, MaxBillDescriptionLength) || !IsPlausible(bill.CreatedAt) || !IsPlausible(bill.DueDate) || !Enum.IsDefined(bill.Status)
                || (bill.AppointmentId is { } appointmentId && (!appointmentsById.TryGetValue(appointmentId, out var visit) || visit.PatientId != bill.PatientId))
                || (bill.LabOrderId is { } labId && !labIds.Contains(labId)) || (bill.PaidAt is not null) != (bill.Status == HcBillStatus.Paid)
                || bill.CardBrand is not { Length: <= 20 } || bill.CardLast4 is not { Length: <= 4 } || (bill.CardLast4.Length > 0 && !bill.CardLast4.All(char.IsAsciiDigit)))
            || !Unique(storedBills.Select(bill => bill.Id)))
            return false;

        Replace(appointments, storedAppointments);
        Replace(availability, storedAvailability);
        Replace(bills, storedBills);
        Replace(labOrders, storedLabs);
        Replace(patients, storedPatients);
        Replace(prescriptions, storedPrescriptions);
        Replace(timeOff, storedTimeOff);
        nextAppointmentId = snapshot.NextAppointmentId;
        nextAvailabilityId = snapshot.NextAvailabilityId;
        nextBillId = snapshot.NextBillId;
        nextLabOrderId = snapshot.NextLabOrderId;
        nextPatientId = snapshot.NextPatientId;
        nextPrescriptionId = snapshot.NextPrescriptionId;
        nextTimeOffId = snapshot.NextTimeOffId;

        return true;
    }

    /// <summary>
    /// Returns a patient's upcoming appointments (booked and not more than one slot late, checked in, or with the doctor),
    /// soonest first (staff, or the patient themselves).
    /// </summary>
    public IReadOnlyList<HcAppointment> UpcomingOf(int patientId) => CanSeePatient(patientId) ? Upcoming(patientId).Select(ForViewer).ToList() : Array.Empty<HcAppointment>();

    #endregion

    #region Private Methods

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    private static bool IsAccountId(string? accountId) =>
        accountId is not null && (HcSeedData.DemoAccounts().Any(demo => demo.Id == accountId) || (accountId.StartsWith("u-", StringComparison.Ordinal) && accountId.Length <= 12));

    private static bool IsPlausible(DateTime value) => value >= HcSeedData.Now.AddYears(-10) && value <= HcSeedData.Now.AddYears(3);

    private static bool IsText(string? value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static void Replace<T>(List<T> target, List<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private static bool Unique<T>(IEnumerable<T> values)
    {
        var list = values.ToList();

        return list.Distinct().Count() == list.Count;
    }

    private static bool ValidList(List<string>? items) =>
        items is { Count: <= MaxListItems } && items.All(item => IsText(item, MaxListItemLength)) && Unique(items.Select(item => item.ToLowerInvariant()));

    private HcResult AddToChart(int patientId, string text, Func<HcPatient, List<string>> list)
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

        return Commit(() => items.Remove(text));
    }

    private bool CanSeePatient(int patientId) => IsStaff || Patient()?.Id == patientId;

    /// <summary>
    /// Returns the appointment itself for staff; for a patient, a copy without the staff notes, and without the summary until
    /// the visit is completed.
    /// </summary>
    private HcAppointment ForViewer(HcAppointment appointment) => IsStaff ? appointment : new HcAppointment
    {
        Id = appointment.Id, PatientId = appointment.PatientId, DoctorId = appointment.DoctorId, ClinicId = appointment.ClinicId, Start = appointment.Start, Minutes = appointment.Minutes,
        Reason = appointment.Reason, Status = appointment.Status, CreatedAt = appointment.CreatedAt, CheckedInAt = appointment.CheckedInAt, CompletedAt = appointment.CompletedAt,
        CancelledAt = appointment.CancelledAt, CancelReason = appointment.CancelReason,
        Summary = appointment.Status == HcAppointmentStatus.Completed ? appointment.Summary : string.Empty,
    };

    /// <summary>Returns the order itself for staff; for a patient, a copy without values or comment until it is released.</summary>
    private HcLabOrder ForViewer(HcLabOrder order) => IsStaff || order.Status == HcLabStatus.Released ? order : new HcLabOrder
    {
        Id = order.Id, PatientId = order.PatientId, DoctorId = order.DoctorId, AppointmentId = order.AppointmentId, PanelId = order.PanelId, OrderedAt = order.OrderedAt,
        Status = order.Status,
    };

    private List<HcAppointment> Upcoming(int patientId) =>
        appointments.Where(item => item.PatientId == patientId
                && (item.Status is HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit || (item.Status == HcAppointmentStatus.Booked && item.Start >= Clock.Now.AddMinutes(-SlotMinutes))))
            .OrderBy(item => item.Start).ToList();

    /// <summary>Raises <see cref="Changed" /> when the snapshot still fits; otherwise undoes the change and reports Invalid.</summary>
    private HcResult Commit(Action undo)
    {
        if (!FitsSnapshot())
        {
            undo();

            return HcResult.Invalid;
        }

        Changed?.Invoke();

        return HcResult.Ok;
    }

    /// <summary>The signed-in doctor, or <see langword="null" />.</summary>
    private HcDoctor? Doctor() =>
        Identity.Current is { RoleId: HcSeedData.DoctorRole } account ? Doctors.FirstOrDefault(doctor => doctor.AccountId == account.Id) : null;

    /// <summary>Whether the snapshot, as it would be saved now, still fits under <see cref="MaxSnapshotLength" />.</summary>
    private bool FitsSnapshot()
    {
        try
        {
            return JsonSerializer.Serialize(CaptureData(), snapshotOptions).Length <= MaxSnapshotLength;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return false;
        }
    }

    private bool HasAppointmentAt(int patientId, DateTime start, int? exceptId) =>
        appointments.Any(item => item.PatientId == patientId && item.Id != exceptId && item.Status is not (HcAppointmentStatus.Cancelled or HcAppointmentStatus.NoShow)
            && item.Start < start.AddMinutes(SlotMinutes) && start < item.Start.AddMinutes(item.Minutes));

    /// <summary>A new patient account gets a patient record; a patient's name and email follow their profile.</summary>
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
                patients.Add(new HcPatient { Id = nextPatientId++, AccountId = account.Id, Name = name, Email = Clip(email, ShowcaseIdentity.MaxEmailLength) });
            }
        }

        Changed?.Invoke();
    }

    /// <summary>The signed-in patient's record, or <see langword="null" />.</summary>
    private HcPatient? Patient() =>
        Identity.Current is { RoleId: HcSeedData.PatientRole } account ? patients.FirstOrDefault(patient => patient.AccountId == account.Id) : null;

    private HcResult RemoveFromChart(int patientId, string text, Func<HcPatient, List<string>> list)
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
        Changed?.Invoke();

        return HcResult.Ok;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the clinics.</summary>
    public IReadOnlyList<HcClinic> Clinics { get; } = HcSeedData.Clinics();

    /// <summary>Gets the site's clock, which moves on from the seed time (D-029).</summary>
    public ShowcaseClock Clock { get; } = new(HcSeedData.Now);

    /// <summary>Gets the signed-in doctor, or <see langword="null" />.</summary>
    public HcDoctor? CurrentDoctor => Doctor();

    /// <summary>Gets the signed-in patient's record, or <see langword="null" />.</summary>
    public HcPatient? CurrentPatient => Patient();

    /// <summary>Gets the doctors.</summary>
    public IReadOnlyList<HcDoctor> Doctors { get; } = HcSeedData.Doctors();

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity { get; }

    /// <summary>Gets whether the theme switcher applied dark mode.</summary>
    public bool IsDarkTheme { get; private set; }

    /// <summary>Gets whether the signed-in account is the front desk.</summary>
    public bool IsFrontDesk => Identity.Current?.RoleId == HcSeedData.FrontDeskRole;

    /// <summary>Gets whether the signed-in account is staff (a doctor with a record, or the front desk).</summary>
    public bool IsStaff => IsFrontDesk || Doctor() is not null;

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

    /// <summary>Gets the refill requests waiting for a doctor (doctors only), oldest first.</summary>
    public IReadOnlyList<HcPrescription> RefillRequests =>
        Doctor() is not null ? prescriptions.Where(item => item.RefillRequestedAt is not null).OrderBy(item => item.RefillRequestedAt).ToList() : Array.Empty<HcPrescription>();

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    public string SiteSlug => "healthcare";

    /// <summary>Gets the snapshot format version.</summary>
    public int SnapshotVersion => 1;

    /// <summary>Gets the specialties of the doctors, by name.</summary>
    public IReadOnlyList<string> Specialties => Doctors.Select(doctor => doctor.Specialty).Distinct().Order().ToList();

    #endregion

    #region Events

    /// <summary>Raised after any change, so open pages re-render and the session store saves.</summary>
    public event Action? Changed;

    /// <summary>Raised when the site's theme changes.</summary>
    public event Action? ThemeChanged;

    #endregion

    #region Other

    /// <summary>The stored form of the state.</summary>
    private sealed class HcSnapshot
    {
        public List<HcAppointment>? Appointments { get; set; }

        public List<HcAvailability>? Availability { get; set; }

        public List<HcBill>? Bills { get; set; }

        public List<HcLabOrder>? LabOrders { get; set; }

        public int NextAppointmentId { get; set; }

        public int NextAvailabilityId { get; set; }

        public int NextBillId { get; set; }

        public int NextLabOrderId { get; set; }

        public int NextPatientId { get; set; }

        public int NextPrescriptionId { get; set; }

        public int NextTimeOffId { get; set; }

        public List<HcPatient>? Patients { get; set; }

        public List<HcPrescription>? Prescriptions { get; set; }

        public List<HcTimeOff>? TimeOff { get; set; }
    }

    #endregion
}
