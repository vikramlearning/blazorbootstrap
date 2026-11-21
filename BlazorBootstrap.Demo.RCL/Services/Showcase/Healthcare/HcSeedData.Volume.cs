using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>
/// The generated part of a fresh tab (rule WR-001, realistic volume): a year of clinic work around the hand-written records —
/// 320 patients, about 1,900 appointments from twelve months back to six weeks ahead across the six doctors and three
/// clinics, the labs, prescriptions, refill requests, and bills of those visits, and the last three weeks of the audit log
/// and the demo accounts' notifications. Everything is built from fixed tables and a fixed hash, so every tab starts the
/// same, and the tab's snapshot stores only what differs from it.
/// </summary>
public static partial class HcSeedData
{
    #region Fields and Constants

    /// <summary>How many patients a fresh tab has, the hand-written ones included.</summary>
    public const int PatientCount = 320;

    /// <summary>How far back the seeded audit log goes, in days.</summary>
    private const int AuditDays = 21;

    private static readonly string[] allergyPool = { "Penicillin (rash)", "Sulfa drugs", "Latex", "Peanuts", "Shellfish", "Pollen", "Ibuprofen (stomach upset)", "Cat dander" };

    private static readonly string[] cancelReasons =
    {
        "Feeling better, no longer needed.", "Work came up; will book another time.", "Booked with another clinic closer to home.", "Car trouble.", "Child is sick at home.",
        "Called the front desk to cancel.",
    };

    /// <summary>The test cards bills were paid with (brand and last four), the ones the demo's card form accepts.</summary>
    private static readonly (string Brand, string Last4)[] cards = { ("Visa", "4242"), ("Mastercard", "4444"), ("Amex", "0005"), ("Visa", "1881") };

    private static readonly string[] conditionPool =
    {
        "High blood pressure", "Type 2 diabetes", "Asthma", "High cholesterol", "Hypothyroidism", "Migraine", "Eczema", "Osteoarthritis", "Acid reflux", "Anxiety",
    };

    private static readonly string[] femaleNames =
    {
        "Amelia", "Ava", "Beatriz", "Chloe", "Daria", "Elif", "Fatima", "Hana", "Isabel", "Jasmine", "Kaya", "Leila", "Mei", "Nadia", "Olivia", "Rosa", "Sara", "Tamar", "Uma",
        "Valentina", "Wren", "Yara", "Zoe", "Ingrid", "Noor", "Lucia", "Esther", "Aiko", "Clara", "Marisol",
    };

    /// <summary>The front desk staff who check patients in at each clinic (only Lena Fischer has a demo sign-in).</summary>
    private static readonly Dictionary<string, string> frontDeskByClinic = new() { ["downtown"] = "Lena Fischer", ["lakeview"] = "Tom Reyes", ["northfield"] = "Aiko Sato" };

    private static readonly string[] lastNames =
    {
        "Anderson", "Baker", "Castillo", "Dubois", "Eriksen", "Fernandes", "Garcia", "Hughes", "Ibarra", "Jensen", "Kim", "Lambert", "Morales", "Nakamura", "Okafor", "Petrov", "Quinn",
        "Rahman", "Schmidt", "Tanaka", "Usman", "Varga", "Walsh", "Xu", "Yilmaz", "Zielinski", "Adeyemi", "Bianchi", "Chowdhury", "Delgado", "Engel", "Gallagher", "Haddad", "Kowalski",
        "Lindqvist", "Mendes", "Novak", "Pereira", "Silva", "Torres", "Vogel", "Wright", "Young", "Abbott", "Moreau", "Sandoval", "Ito", "Mensah",
    };

    private static readonly string[] maleNames =
    {
        "Aaron", "Benjamin", "Carlos", "Dmitri", "Elias", "Felix", "Gabriel", "Hugo", "Ibrahim", "Jonah", "Kenji", "Leo", "Mateo", "Nikhil", "Oscar", "Pablo", "Rafael", "Samir",
        "Theo", "Victor", "Wesley", "Xavier", "Yusuf", "Zane", "Arjun", "Tomas", "Marcus", "Emil", "Kwame", "Anton",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the patient portal route of an appointment.</summary>
    public static string AppointmentPath(int appointmentId) => Invariant($"{SiteBase}/appointments/{appointmentId}");

    /// <summary>
    /// Builds a fresh tab's data: the hand-written records and the generated year around them, the seeded audit log, and the
    /// demo accounts' notifications. New objects every call, so a tab can change them without touching another copy.
    /// </summary>
    public static HcSeedSet Build()
    {
        var doctors = Doctors();
        var availability = Availability();
        var timeOff = TimeOff();
        var patients = Patients();
        var appointments = Appointments();
        var labs = LabOrders();
        var prescriptions = Prescriptions();
        var bills = Bills();

        var firstGeneratedVisit = appointments.Max(item => item.Id) + 1;

        GeneratePatients(patients);
        GenerateAppointments(appointments, patients, doctors, availability, timeOff);
        GenerateClinicalWork(appointments, firstGeneratedVisit, doctors, labs, prescriptions, bills);
        AddVitals(appointments);

        var claims = GenerateClaims(appointments, patients, doctors, labs, bills, out var claimAudit, out var claimNotices);
        var audit = Audit(appointments, patients, doctors, labs, prescriptions, bills, claimAudit);
        var notifications = Notifications(appointments, patients, doctors, labs, prescriptions, bills, claimNotices);

        return new HcSeedSet(appointments, audit, availability, bills, labs, notifications, patients, prescriptions, StaffProfiles(), timeOff, doctors, claims);
    }

    /// <summary>Returns the staff route of a lab order.</summary>
    public static string LabOrderPath(int orderId) => Invariant($"{StaffBase}/labs/{orderId}");

    /// <summary>Returns the patient portal route of a released lab report.</summary>
    public static string LabReportPath(int orderId) => Invariant($"{SiteBase}/lab-reports/{orderId}");

    /// <summary>Returns the first letter in lower case, for a fragment in the middle of a sentence ("Annual physical" → "annual physical").</summary>
    public static string LowerFirst(string text) => string.IsNullOrEmpty(text) ? text : string.Concat(text[..1].ToLowerInvariant(), text.AsSpan(1));

    /// <summary>Returns the staff route of a patient's chart (doctors).</summary>
    public static string PatientChartPath(int patientId) => Invariant($"{StaffBase}/patients/{patientId}");

    /// <summary>Returns the staff route of a patient's registration details.</summary>
    public static string PatientDetailsPath(int patientId) => Invariant($"{StaffBase}/patients/{patientId}/edit");

    /// <summary>Returns the staff route of a visit (doctors).</summary>
    public static string StaffAppointmentPath(int appointmentId) => Invariant($"{StaffBase}/appointments/{appointmentId}");

    /// <summary>Returns the front desk's calendar on a day, for one doctor.</summary>
    public static string StaffCalendarPath(DateTime date, string doctorId) =>
        $"{StaffBase}/calendar?date={date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&doctor={Uri.EscapeDataString(doctorId)}";

    #endregion

    #region Private Methods

    /// <summary>The seeded audit log: the last <see cref="AuditDays" /> days of bookings, check-ins, visits, chart views, labs, prescriptions, refills, payments, and registrations.</summary>
    private static List<ShowcaseAuditEntry> Audit(List<HcAppointment> appointments, List<HcPatient> patients, IReadOnlyList<HcDoctor> doctors, List<HcLabOrder> labs,
        List<HcPrescription> prescriptions, List<HcBill> bills, List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)> claimAudit)
    {
        var since = Now.AddDays(-AuditDays);
        var patientsById = patients.ToDictionary(patient => patient.Id);
        var doctorsById = doctors.ToDictionary(doctor => doctor.Id);
        var panels = LabPanels().ToDictionary(panel => panel.Id);
        var entries = new List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)>();

        bool InWindow(DateTime? at) => at is { } value && value >= since && value <= Now;
        string PatientTarget(HcPatient patient) => $"Patient {patient.Name} ({patient.Mrn})";
        string AppointmentTarget(HcAppointment appointment) => Invariant($"Appointment {appointment.Id} · {patientsById[appointment.PatientId].Name}");
        string Desk(string clinicId) => frontDeskByClinic.GetValueOrDefault(clinicId, "Lena Fischer");

        foreach (var patient in patients.Where(patient => InWindow(patient.RegisteredAt) && patient.AccountId is null))
            entries.Add((patient.RegisteredAt, Desk(patient.ClinicId), "Patient registered", PatientTarget(patient), PatientDetailsPath(patient.Id), "Registered at the front desk"));

        foreach (var appointment in appointments)
        {
            var patient = patientsById[appointment.PatientId];
            var doctor = doctorsById[appointment.DoctorId];
            var target = AppointmentTarget(appointment);
            var url = StaffAppointmentPath(appointment.Id);
            var byPatient = patient.AccountId is not null;

            if (InWindow(appointment.CreatedAt))
            {
                entries.Add((appointment.CreatedAt, byPatient ? patient.Name : Desk(appointment.ClinicId), "Appointment booked", target, url,
                    $"{HcVisitTypes.Label(appointment.VisitType)} with {doctor.Name}, {appointment.Start.ToString("ddd, MMM d 'at' HH:mm", CultureInfo.InvariantCulture)}{(byPatient ? ", booked online" : ", booked by phone")}"));
            }

            if (InWindow(appointment.CheckedInAt))
                entries.Add((appointment.CheckedInAt!.Value, Desk(appointment.ClinicId), "Checked in", target, url, $"Arrived for {appointment.Start.ToString("HH:mm", CultureInfo.InvariantCulture)} with {doctor.Name}"));

            if (InWindow(appointment.CompletedAt))
            {
                var viewedAt = appointment.Start.AddMinutes(-3);
                entries.Add((viewedAt, doctor.Name, "Viewed chart", PatientTarget(patient), PatientChartPath(patient.Id), "Doctor, before the visit"));
                entries.Add((appointment.CompletedAt!.Value, doctor.Name, "Visit completed", target, url,
                    Invariant($"Notes {appointment.Notes.Length} characters, summary {appointment.Summary.Length} characters")));
            }

            if (appointment.Status == HcAppointmentStatus.Cancelled && InWindow(appointment.CancelledAt))
                entries.Add((appointment.CancelledAt!.Value, byPatient ? patient.Name : Desk(appointment.ClinicId), "Appointment cancelled", target, url, byPatient ? "Cancelled online" : "Cancelled by the front desk"));

            if (appointment.Status == HcAppointmentStatus.NoShow && InWindow(appointment.Start.AddMinutes(20)))
                entries.Add((appointment.Start.AddMinutes(20), Desk(appointment.ClinicId), "Marked no-show", target, url, null));
        }

        foreach (var order in labs)
        {
            var patient = patientsById[order.PatientId];
            var doctor = doctorsById[order.DoctorId];
            var target = Invariant($"Lab order {order.Id} · {panels[order.PanelId].Name}");

            if (InWindow(order.OrderedAt))
                entries.Add((order.OrderedAt, doctor.Name, "Lab ordered", target, LabOrderPath(order.Id), $"For {patient.Name} ({patient.Mrn})"));

            if (InWindow(order.ResultedAt))
                entries.Add((order.ResultedAt!.Value, doctor.Name, "Lab results entered", target, LabOrderPath(order.Id), Invariant($"{order.Values.Count} values, comment {order.Comment.Length} characters")));

            if (InWindow(order.ReleasedAt))
                entries.Add((order.ReleasedAt!.Value, doctor.Name, "Lab results released", target, LabOrderPath(order.Id), $"Released to {patient.Name}; billed {Money(LabFeeCents)}"));
        }

        foreach (var prescription in prescriptions)
        {
            var patient = patientsById[prescription.PatientId];
            var target = $"Prescription {prescription.Medication} {prescription.Dose} · {patient.Name}";

            if (InWindow(prescription.PrescribedAt))
            {
                entries.Add((prescription.PrescribedAt, doctorsById[prescription.DoctorId].Name, "Prescribed", target, PatientChartPath(patient.Id),
                    Invariant($"Quantity {prescription.Quantity}, {prescription.Refills} refills")));
            }

            if (InWindow(prescription.RefillRequestedAt))
                entries.Add((prescription.RefillRequestedAt!.Value, patient.Name, "Refill requested", target, StaffBase + "/refills", Invariant($"Quantity {prescription.RefillQuantity}")));
        }

        foreach (var bill in bills.Where(bill => InWindow(bill.PaidAt)))
        {
            entries.Add((bill.PaidAt!.Value, patientsById[bill.PatientId].Name, "Bill paid", Invariant($"Bill {bill.Id} · {patientsById[bill.PatientId].Name}"), PatientDetailsPath(bill.PatientId),
                $"{Money(bill.AmountCents)} with {bill.CardBrand} ending {bill.CardLast4}"));
        }

        entries.AddRange(claimAudit.Where(entry => InWindow(entry.At)));

        return entries
            .Where(entry => entry.At <= Now)
            .OrderBy(entry => entry.At)
            .ThenBy(entry => entry.Target, StringComparer.Ordinal)
            .Select((entry, index) => new ShowcaseAuditEntry
            {
                Id = index + 1, At = entry.At, Actor = entry.Actor, Action = entry.Action, Target = entry.Target, TargetUrl = entry.Url, Details = entry.Details,
            })
            .ToList();
    }

    /// <summary>A fill, as a percentage of a doctor's open slots, by how far a day is from today: thin a year back, busy around now.</summary>
    private static int FillPercent(int day) => day switch
    {
        < -56 => 9,
        < 0 => 34,
        0 => 40,
        <= 14 => 45,
        _ => 14,
    };

    /// <summary>
    /// Adds a year of appointments in the doctors' weekly hours (outside their time off), one patient per slot: past ones
    /// completed, cancelled, or no-shows; this morning's checked in; later ones booked. A patient's first visit with a doctor
    /// is a new patient visit when the next half hour is free too.
    /// </summary>
    private static void GenerateAppointments(List<HcAppointment> appointments, List<HcPatient> patients, IReadOnlyList<HcDoctor> doctors, List<HcAvailability> availability,
        List<HcTimeOff> timeOff)
    {
        var today = Now.Date;
        var nextId = appointments.Max(item => item.Id) + 1;
        var taken = new HashSet<(string Doctor, DateTime Start)>();
        var busy = new HashSet<(int Patient, DateTime Start)>();
        var seen = new HashSet<(int Patient, string Doctor)>();

        void Hold(HcAppointment appointment)
        {
            for (var minute = 0; minute < appointment.Minutes; minute += HcVisitTypes.StandardMinutes)
            {
                taken.Add((appointment.DoctorId, appointment.Start.AddMinutes(minute)));
                busy.Add((appointment.PatientId, appointment.Start.AddMinutes(minute)));
            }
        }

        foreach (var appointment in appointments)
        {
            seen.Add((appointment.PatientId, appointment.DoctorId));

            if (appointment.Status != HcAppointmentStatus.Cancelled)
                Hold(appointment);
        }

        // The demo patient keeps her hand-written story; everyone else can be booked.
        var children = patients.Where(patient => patient.Id != SeededPatientId && Age(patient) < 18).ToList();
        var adults = patients.Where(patient => patient.Id != SeededPatientId && Age(patient) >= 16).ToList();
        var everyone = patients.Where(patient => patient.Id != SeededPatientId).ToList();
        var specialties = doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Specialty);
        var sequence = 0;

        for (var day = -364; day <= 42; day++)
        {
            var date = today.AddDays(day);
            var percent = FillPercent(day);

            foreach (var block in availability.Where(item => item.Day == date.DayOfWeek).OrderBy(item => item.DoctorId, StringComparer.Ordinal).ThenBy(item => item.StartMinute))
            {
                if (timeOff.Any(entry => entry.DoctorId == block.DoctorId && date >= entry.From && date <= entry.To))
                    continue;

                var specialty = specialties[block.DoctorId];

                for (var minute = block.StartMinute; minute + HcVisitTypes.StandardMinutes <= block.EndMinute; minute += HcVisitTypes.StandardMinutes)
                {
                    var start = date.AddMinutes(minute);
                    var index = sequence++;

                    if (taken.Contains((block.DoctorId, start)) || Pick(index, 1, 100) >= percent)
                        continue;

                    // Children see the pediatrician; specialists see teenagers and adults; family doctors see anyone, mostly from their clinic.
                    var pool = specialty == "Pediatrics" ? children : specialty == "Family medicine" ? everyone : adults;
                    var local = pool.Where(patient => patient.ClinicId == block.ClinicId).ToList();
                    var candidates = local.Count > 0 && Pick(index, 2, 10) < 8 ? local : pool;
                    HcPatient? patient = null;

                    for (var attempt = 0; attempt < 4 && patient is null; attempt++)
                    {
                        var candidate = candidates[Pick(index, 3 + attempt, candidates.Count)];

                        if (!busy.Contains((candidate.Id, start)))
                            patient = candidate;
                    }

                    if (patient is null)
                        continue;

                    var isNew = !seen.Contains((patient.Id, block.DoctorId)) && minute + HcVisitTypes.NewPatientMinutes <= block.EndMinute
                        && !taken.Contains((block.DoctorId, start.AddMinutes(30))) && !busy.Contains((patient.Id, start.AddMinutes(30)));
                    var length = isNew ? HcVisitTypes.NewPatientMinutes : HcVisitTypes.StandardMinutes;
                    var roll = Pick(index, 8, 100);
                    var status = start < Now
                        ? day < 0 ? roll < 6 ? HcAppointmentStatus.NoShow : roll < 15 ? HcAppointmentStatus.Cancelled : HcAppointmentStatus.Completed : HcAppointmentStatus.CheckedIn
                        : roll < 5 ? HcAppointmentStatus.Cancelled : HcAppointmentStatus.Booked;
                    var created = start.AddDays(-(1 + Pick(index, 9, 28))).Date.AddHours(8 + Pick(index, 10, 10)).AddMinutes(Pick(index, 11, 60));

                    if (created >= Now)
                        created = Now.AddMinutes(-(30 + Pick(index, 12, 900)));

                    var reasons = ReasonsFor(specialty);
                    var appointment = new HcAppointment
                    {
                        Id = nextId++, PatientId = patient.Id, DoctorId = block.DoctorId, ClinicId = block.ClinicId, Start = start, Minutes = length,
                        VisitType = isNew ? HcVisitType.NewPatient : HcVisitType.Standard, Reason = isNew ? $"New patient: {LowerFirst(reasons[Pick(index, 13, reasons.Length)])}" : reasons[Pick(index, 13, reasons.Length)],
                        Status = status, CreatedAt = created,
                    };

                    switch (status)
                    {
                        case HcAppointmentStatus.Completed:
                            appointment.CheckedInAt = start.AddMinutes(-(3 + Pick(index, 14, 12)));
                            appointment.CompletedAt = start.AddMinutes(length - 5);
                            var (notes, summary) = VisitTextFor(specialty, Pick(index, 15, 4));
                            appointment.Notes = notes;
                            appointment.Summary = summary;
                            break;

                        case HcAppointmentStatus.CheckedIn:
                            appointment.CheckedInAt = Min(start.AddMinutes(-(3 + Pick(index, 14, 12))), Now.AddMinutes(-2));
                            break;

                        case HcAppointmentStatus.Cancelled:
                            appointment.CancelledAt = Min(created.AddTicks((start - created).Ticks / 2), Now.AddMinutes(-5));
                            appointment.CancelReason = cancelReasons[Pick(index, 16, cancelReasons.Length)];
                            break;
                    }

                    appointments.Add(appointment);
                    seen.Add((patient.Id, block.DoctorId));

                    if (status != HcAppointmentStatus.Cancelled)
                        Hold(appointment);
                }
            }
        }
    }

    /// <summary>
    /// Adds what the completed visits left behind: a bill for each one without a bill, some lab orders (released after a few
    /// days, with a bill; the last days' still waiting for results or for release), and some prescriptions (refilled over
    /// time; a few with no refills left now waiting for the doctor's answer).
    /// </summary>
    private static void GenerateClinicalWork(List<HcAppointment> appointments, int firstGeneratedVisit, IReadOnlyList<HcDoctor> doctors, List<HcLabOrder> labs,
        List<HcPrescription> prescriptions, List<HcBill> bills)
    {
        var doctorsById = doctors.ToDictionary(doctor => doctor.Id);
        var panels = LabPanels().ToDictionary(panel => panel.Id);
        var pharmacies = Pharmacies();
        var billed = bills.Where(bill => bill.AppointmentId is not null).Select(bill => bill.AppointmentId!.Value).ToHashSet();
        var nextLab = labs.Max(item => item.Id) + 1;
        var nextPrescription = prescriptions.Max(item => item.Id) + 1;
        var nextBill = bills.Max(item => item.Id) + 1;
        var requests = 0;

        void AddBill(int patientId, int? appointmentId, int? labId, string description, int cents, DateTime created, int seed)
        {
            var age = (Now - created).TotalDays;
            var paid = age > 25 ? Pick(seed, 40, 100) < 93 : age > 3 && Pick(seed, 40, 100) < 35;
            var card = cards[Pick(seed, 41, cards.Length)];
            var paidAt = paid ? Min(created.AddDays(1 + Pick(seed, 42, Math.Max(1, Math.Min(20, (int)age - 1)))).Date.AddHours(18).AddMinutes(Pick(seed, 43, 50)), Now.AddHours(-1)) : (DateTime?)null;

            bills.Add(new HcBill
            {
                Id = nextBill++, PatientId = patientId, AppointmentId = appointmentId, LabOrderId = labId, Description = Clip(description, 200), AmountCents = cents, CreatedAt = created,
                DueDate = created.Date.AddDays(30), Status = paid ? HcBillStatus.Paid : HcBillStatus.Due, PaidAt = paidAt, CardBrand = paid ? card.Brand : string.Empty, CardLast4 = paid ? card.Last4 : string.Empty,
            });
        }

        foreach (var visit in appointments.Where(item => item.Status == HcAppointmentStatus.Completed).OrderBy(item => item.CompletedAt))
        {
            var doctor = doctorsById[visit.DoctorId];
            var primary = doctor.Specialty is "Family medicine" or "Pediatrics";

            if (!billed.Contains(visit.Id))
                AddBill(visit.PatientId, visit.Id, null, $"Visit: {LowerFirst(visit.Reason)} with {doctor.Name}", primary ? PrimaryCareFeeCents : SpecialistFeeCents, visit.CompletedAt!.Value.AddMinutes(5), visit.Id);

            // Labs and prescriptions are written into the hand-written visits by hand.
            if (visit.Id < firstGeneratedVisit)
                continue;

            var labChance = doctor.Specialty switch { "Family medicine" => 40, "Cardiology" => 55, "Pediatrics" => 10, _ => 0 };

            if (Pick(visit.Id, 20, 100) < labChance)
            {
                var choices = doctor.Specialty switch { "Cardiology" => new[] { "lipid", "bmp" }, "Pediatrics" => new[] { "cbc" }, _ => new[] { "a1c", "lipid", "bmp", "cbc", "tsh" } };
                var panel = panels[choices[Pick(visit.Id, 21, choices.Length)]];
                var ordered = visit.Start.AddMinutes(20);
                var days = (Now - ordered).TotalDays;
                var status = days < 1.5 ? HcLabStatus.Ordered : days < 8 && Pick(visit.Id, 22, 100) < 65 ? HcLabStatus.Resulted : days < 2 ? HcLabStatus.Ordered : HcLabStatus.Released;
                var order = new HcLabOrder { Id = nextLab++, PatientId = visit.PatientId, DoctorId = visit.DoctorId, AppointmentId = visit.Id, PanelId = panel.Id, OrderedAt = ordered, Status = status };

                if (status != HcLabStatus.Ordered)
                {
                    order.Values = panel.Analytes.Select((analyte, position) => LabValue(analyte, visit.Id * 7 + position)).ToList();
                    order.ResultedAt = Min(ordered.Date.AddDays(1).AddHours(14).AddMinutes(Pick(visit.Id, 23, 50)), Now.AddMinutes(-30));
                }

                if (status == HcLabStatus.Released)
                {
                    var outside = panel.Analytes.Zip(order.Values).Count(pair => pair.Second < pair.First.Low || pair.Second > pair.First.High);
                    order.ReleasedAt = Min(order.ResultedAt!.Value.Date.AddDays(1).AddHours(9).AddMinutes(Pick(visit.Id, 24, 50)), Now.AddMinutes(-20));
                    order.Comment = outside == 0 ? "All results are in the normal range." : "Some results are outside the normal range. We'll go over them at your next visit.";
                    AddBill(order.PatientId, null, order.Id, $"Lab: {LowerFirst(panel.Name)}", LabFeeCents, order.ReleasedAt.Value, order.Id + 5000);
                }

                labs.Add(order);
            }

            var prescribeChance = doctor.Specialty switch { "Family medicine" => 26, "Cardiology" => 30, "Dermatology" => 35, "Orthopedics" => 20, _ => 12 };

            if (Pick(visit.Id, 30, 100) < prescribeChance)
            {
                var medicines = MedicinesFor(doctor.Specialty);
                var (medication, dose, directions, quantity, supply) = medicines[Pick(visit.Id, 31, medicines.Length)];
                var prescribed = visit.Start.AddMinutes(22);
                var refills = Pick(visit.Id, 32, 6);
                var fills = Math.Min(refills, (int)((Now.Date - prescribed.Date).TotalDays / supply));
                var lastFilled = prescribed.Date.AddDays(fills * supply);
                var stopped = (supply <= 14 && (Now - prescribed).TotalDays > supply + 7) || ((Now - prescribed).TotalDays > 200 && Pick(visit.Id, 33, 100) < 50);
                var prescription = new HcPrescription
                {
                    Id = nextPrescription++, PatientId = visit.PatientId, DoctorId = visit.DoctorId, Medication = medication, Dose = dose, Directions = directions, Quantity = quantity,
                    DaysSupply = supply, Refills = refills - fills, PrescribedAt = prescribed, LastFilledOn = lastFilled, Status = stopped ? HcPrescriptionStatus.Stopped : HcPrescriptionStatus.Active,
                };

                // A few active ones with nothing left and the supply running low wait for the doctor; older ones were answered.
                if (!stopped && prescription.Refills == 0 && HcState.EarliestRefillOn(prescription) <= Now.Date.AddDays(3))
                {
                    if (requests < 16 && Pick(visit.Id, 34, 100) < 45)
                    {
                        requests++;
                        prescription.RefillRequestedAt = Now.AddHours(-(2 + Pick(visit.Id, 35, 60)));
                        prescription.RefillPharmacyId = pharmacies[Pick(visit.Id, 36, pharmacies.Count)].Id;
                        prescription.RefillQuantity = quantity;
                        prescription.RefillPickupOn = Max(HcState.EarliestRefillOn(prescription), Now.Date.AddDays(1));
                    }
                    else if (fills > 0)
                    {
                        prescription.RefillOutcome = HcRefillOutcome.Approved;
                        prescription.RefillNote = "Approved: one refill added.";
                    }
                }

                prescriptions.Add(prescription);
            }
        }
    }

    /// <summary>Adds generated patients until there are <see cref="PatientCount" />: names, ages, contact details, a home clinic, most with a health plan on file.</summary>
    private static void GeneratePatients(List<HcPatient> patients)
    {
        var names = patients.Select(patient => patient.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var payers = Payers();
        var clinics = new[] { "downtown", "downtown", "downtown", "downtown", "lakeview", "lakeview", "lakeview", "northfield", "northfield", "northfield" };

        for (var index = 0; patients.Count < PatientCount; index++)
        {
            var female = Pick(index, 1, 2) == 0;
            var first = female ? femaleNames[(index * 7 + index / 13) % femaleNames.Length] : maleNames[(index * 7 + index / 11) % maleNames.Length];
            var last = lastNames[(index * 11 + index / lastNames.Length) % lastNames.Length];
            var name = $"{first} {last}";

            if (!names.Add(name))
                continue;

            var id = patients.Max(patient => patient.Id) + 1;
            var bucket = Pick(index, 2, 100);
            var age = bucket < 15 ? Pick(index, 3, 18) : bucket < 72 ? 18 + Pick(index, 3, 47) : 65 + Pick(index, 3, 28);
            var dob = Now.Date.AddYears(-age).AddDays(-Pick(index, 4, 360));
            var slug = new string($"{first}.{last}".ToLowerInvariant().Where(character => char.IsAsciiLetterLower(character) || character == '.').ToArray());
            var clinic = clinics[Pick(index, 5, clinics.Length)];
            // A child is registered two weeks after birth at the earliest, and nobody after yesterday.
            var registered = Now.Date.AddDays(-(Pick(index, 7, 2500) + 1)).AddHours(9 + Pick(index, 8, 8));
            registered = Min(Max(registered, dob.AddDays(14).AddHours(10)), Now.Date.AddDays(-1).AddHours(15));
            var patient = new HcPatient
            {
                Id = id, Mrn = MrnFor(id), Name = name, DateOfBirth = dob, Email = Invariant($"{slug}{id}@example.com"), Phone = Invariant($"(555) 0{15 + Pick(index, 6, 5)}-{1000 + (id * 37 % 9000):0000}"),
                ClinicId = clinic, RegisteredAt = registered,
            };

            if (Pick(index, 9, 10) < 8)
            {
                var payer = payers[Pick(index, 10, payers.Count)];
                patient.InsurancePayerId = payer.Id;
                patient.InsuranceMemberId = Invariant($"{payer.MemberIdPrefix}{100_000_000 + ((long)id * 7_919_731 % 899_999_999):000000000}");
            }

            if (Pick(index, 11, 10) < 7)
            {
                var contactFirst = Pick(index, 12, 2) == 0 ? femaleNames[Pick(index, 13, femaleNames.Length)] : maleNames[Pick(index, 13, maleNames.Length)];
                patient.EmergencyContactName = $"{contactFirst} {last}";
                patient.EmergencyContactPhone = Invariant($"(555) 0{20 + Pick(index, 14, 5)}-{1000 + (id * 53 % 9000):0000}");
            }

            if (Pick(index, 15, 100) < 16)
                patient.Allergies.Add(allergyPool[Pick(index, 16, allergyPool.Length)]);

            var conditions = age < 18 ? (Pick(index, 17, 100) < 12 ? 1 : 0) : age < 50 ? (Pick(index, 17, 100) < 35 ? 1 : 0) : 1 + Pick(index, 17, 2);

            for (var count = 0; count < conditions; count++)
            {
                var condition = age < 18 ? (count == 0 ? "Asthma" : "Eczema") : conditionPool[Pick(index, 18 + count, conditionPool.Length)];

                if (!patient.Conditions.Contains(condition))
                    patient.Conditions.Add(condition);
            }

            if (Pick(index, 20, 100) < 45)
                patient.Immunizations.Add(new HcImmunization("Influenza", Now.Date.AddDays(-(3 + Pick(index, 21, 50)))));

            if (age >= 11 && Pick(index, 22, 100) < 40)
                patient.Immunizations.Add(new HcImmunization("Tdap", Now.Date.AddYears(-(1 + Pick(index, 23, 8))).AddDays(-Pick(index, 24, 300))));

            patients.Add(patient);
        }
    }

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    private static int Age(HcPatient patient) => patient.DateOfBirth is { } dob ? Now.Year - dob.Year - (dob.Date > Now.Date.AddYears(-(Now.Year - dob.Year)) ? 1 : 0) : 40;

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    /// <summary>A lab value near the middle of its range, outside it about one time in six.</summary>
    private static double LabValue(HcAnalyte analyte, int seed)
    {
        var low = analyte.Low;
        var high = analyte.High;
        var roll = Pick(seed, 50, 100);
        var span = high - low;
        var value = roll < 9 ? high + (span * (0.05 + (Pick(seed, 51, 30) / 100.0)))
            : roll < 16 && low > 0 ? low - (low * (0.04 + (Pick(seed, 51, 15) / 100.0)))
            : low + (span * (0.25 + (Pick(seed, 51, 50) / 100.0)));

        return Math.Round(value, value >= 100 ? 0 : 1);
    }

    private static DateTime Max(DateTime left, DateTime right) => left > right ? left : right;

    /// <summary>Medicines each specialty prescribes: name, dose, directions, quantity per fill, and days one fill lasts.</summary>
    private static (string Medication, string Dose, string Directions, int Quantity, int Supply)[] MedicinesFor(string specialty) => specialty switch
    {
        "Cardiology" => new[]
        {
            ("Atorvastatin", "20 mg tablet", "Take one tablet at bedtime.", 30, 30), ("Metoprolol", "25 mg tablet", "Take one tablet twice a day.", 60, 30),
            ("Apixaban", "5 mg tablet", "Take one tablet twice a day.", 60, 30),
        },
        "Dermatology" => new[]
        {
            ("Hydrocortisone", "1% cream", "Apply a thin layer twice a day for 7 days.", 1, 7), ("Clindamycin", "1% gel", "Apply to the affected skin once a day.", 1, 30),
            ("Cetirizine", "10 mg tablet", "Take one tablet at bedtime while the skin itches.", 30, 30),
        },
        "Orthopedics" => new[]
        {
            ("Naproxen", "250 mg tablet", "Take one tablet twice a day with food for up to 10 days.", 20, 10), ("Diclofenac", "1% gel", "Rub into the sore joint four times a day.", 1, 14),
        },
        "Pediatrics" => new[]
        {
            ("Amoxicillin", "250 mg/5 mL liquid", "Give 5 mL three times a day for 7 days.", 1, 7), ("Albuterol", "90 mcg inhaler", "Two puffs every 4 to 6 hours when wheezing.", 1, 30),
        },
        _ => new[]
        {
            ("Lisinopril", "10 mg tablet", "Take one tablet every morning.", 30, 30), ("Metformin", "500 mg tablet", "Take one tablet twice a day with meals.", 60, 30),
            ("Levothyroxine", "50 mcg tablet", "Take one tablet each morning before breakfast.", 30, 30), ("Amlodipine", "5 mg tablet", "Take one tablet every day.", 30, 30),
            ("Omeprazole", "20 mg capsule", "Take one capsule before breakfast.", 30, 30), ("Sertraline", "50 mg tablet", "Take one tablet every morning.", 30, 30),
        },
    };

    private static DateTime Min(DateTime left, DateTime right) => left < right ? left : right;

    private static string Money(int cents) => (cents / 100m).ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>
    /// The demo accounts' notifications, where someone must act or hear back: the doctor about new bookings, patients
    /// checked in today, refill requests, and lab results ready to review; the front desk about new bookings at her clinic;
    /// the patient about her confirmed appointment, released lab results, and new bills. The newest two of each are unread.
    /// </summary>
    private static List<ShowcaseNotification> Notifications(List<HcAppointment> appointments, List<HcPatient> patients, IReadOnlyList<HcDoctor> doctors, List<HcLabOrder> labs,
        List<HcPrescription> prescriptions, List<HcBill> bills, List<(string AccountId, DateTime At, string Title, string Text, string Url)> claimNotices)
    {
        var patientsById = patients.ToDictionary(patient => patient.Id);
        var doctorsById = doctors.ToDictionary(doctor => doctor.Id);
        var panels = LabPanels().ToDictionary(panel => panel.Id);
        var items = new List<(string AccountId, DateTime At, string Title, string Text, string Url)>();

        string When(DateTime start) => start.ToString("ddd, MMM d 'at' HH:mm", CultureInfo.InvariantCulture);

        // The doctor: the latest bookings of the last three days, today's check-ins, refill requests for his prescriptions, results to review.
        foreach (var appointment in appointments.Where(item => item.DoctorId == SeededDoctorId && item.Status == HcAppointmentStatus.Booked && item.CreatedAt >= Now.AddDays(-3)).OrderByDescending(item => item.CreatedAt).Take(12))
        {
            items.Add((SeededDoctorId, appointment.CreatedAt.AddMinutes(1), $"New booking: {patientsById[appointment.PatientId].Name}",
                $"{HcVisitTypes.Label(appointment.VisitType)} on {When(appointment.Start)}: {appointment.Reason}", StaffAppointmentPath(appointment.Id)));
        }

        foreach (var appointment in appointments.Where(item => item.DoctorId == SeededDoctorId && item.Status == HcAppointmentStatus.CheckedIn && item.CheckedInAt is not null))
        {
            items.Add((SeededDoctorId, appointment.CheckedInAt!.Value, $"{patientsById[appointment.PatientId].Name} is checked in",
                $"For {appointment.Start.ToString("HH:mm", CultureInfo.InvariantCulture)} at {Clinics().First(clinic => clinic.Id == appointment.ClinicId).Name}: {appointment.Reason}", StaffAppointmentPath(appointment.Id)));
        }

        foreach (var prescription in prescriptions.Where(item => item.DoctorId == SeededDoctorId && item.RefillRequestedAt is not null))
        {
            items.Add((SeededDoctorId, prescription.RefillRequestedAt!.Value, $"Refill requested: {prescription.Medication}",
                $"{patientsById[prescription.PatientId].Name} asked for a refill of {prescription.Medication} {prescription.Dose}.", StaffBase + "/refills"));
        }

        foreach (var order in labs.Where(item => item.DoctorId == SeededDoctorId && item.Status == HcLabStatus.Resulted && item.ResultedAt is not null))
        {
            items.Add((SeededDoctorId, order.ResultedAt!.Value, "Lab results ready to review",
                $"{panels[order.PanelId].Name} for {patientsById[order.PatientId].Name}. Release them when you've checked them.", LabOrderPath(order.Id)));
        }

        // The front desk: the latest bookings at her clinic.
        foreach (var appointment in appointments.Where(item => item.ClinicId == "downtown" && item.Status == HcAppointmentStatus.Booked && item.CreatedAt >= Now.AddDays(-2)).OrderByDescending(item => item.CreatedAt).Take(8))
        {
            items.Add((SeededFrontDeskId, appointment.CreatedAt.AddMinutes(1), $"New booking at Cedarline Downtown: {patientsById[appointment.PatientId].Name}",
                $"{When(appointment.Start)} with {doctorsById[appointment.DoctorId].Name}.", StaffCalendarPath(appointment.Start.Date, appointment.DoctorId)));
        }

        // The patient: her bookings confirmed, results released, and bills issued.
        foreach (var appointment in appointments.Where(item => item.PatientId == SeededPatientId && item.Status == HcAppointmentStatus.Booked))
        {
            items.Add((SeededPatientAccountId, appointment.CreatedAt.AddMinutes(1), "Appointment confirmed",
                $"{When(appointment.Start)} with {doctorsById[appointment.DoctorId].Name} at {Clinics().First(clinic => clinic.Id == appointment.ClinicId).Name}.", AppointmentPath(appointment.Id)));
        }

        foreach (var order in labs.Where(item => item.PatientId == SeededPatientId && item.ReleasedAt is not null))
            items.Add((SeededPatientAccountId, order.ReleasedAt!.Value, $"Lab results released: {panels[order.PanelId].Name}", $"{doctorsById[order.DoctorId].Name} released your results.", LabReportPath(order.Id)));

        foreach (var bill in bills.Where(item => item.PatientId == SeededPatientId && item.CreatedAt >= Now.AddDays(-60) && item.Status is HcBillStatus.Due or HcBillStatus.Paid && item.AppointmentId is null))
            items.Add((SeededPatientAccountId, bill.CreatedAt, $"New bill: {Money(bill.AmountCents)}", $"{bill.Description}, due {bill.DueDate.ToString("MMM d", CultureInfo.InvariantCulture)}.", SiteBase + "/bills"));

        items.AddRange(claimNotices);

        var all = items.Where(item => item.At <= Now).OrderByDescending(item => item.At).ThenBy(item => item.Title, StringComparer.Ordinal).ToList();
        var unread = new Dictionary<string, int>(StringComparer.Ordinal);

        return all.Select((item, index) => new ShowcaseNotification
        {
            Id = all.Count - index, AccountId = item.AccountId, At = item.At, Title = Clip(item.Title, 80), Text = Clip(item.Text, 200), Url = item.Url,
            IsRead = (unread[item.AccountId] = unread.GetValueOrDefault(item.AccountId) + 1) > 2,
        }).OrderBy(notice => notice.Id).ToList();
    }

    /// <summary>A fixed hash of an index and a salt, in 0 to <paramref name="count" /> − 1: the same in every tab and process.</summary>
    private static int Pick(int index, int salt, int count)
    {
        unchecked
        {
            var value = ((uint)index * 2_654_435_761u) ^ ((uint)salt * 40_503u) ^ 0x9E37_79B9u;
            value ^= value >> 16;
            value *= 0x7FEB_352Du;
            value ^= value >> 15;
            value *= 0x846C_A68Bu;
            value ^= value >> 16;

            return (int)(value % (uint)Math.Max(1, count));
        }
    }

    /// <summary>The reasons patients give when booking with each specialty.</summary>
    private static string[] ReasonsFor(string specialty) => specialty switch
    {
        "Pediatrics" => new[] { "Ear pain", "Fever and cough", "Well-child checkup", "Rash on the arms", "Sports physical", "Sore throat", "Vaccinations", "Stomach ache" },
        "Cardiology" => new[] { "Chest tightness", "Heart palpitations", "Blood pressure follow-up", "Cholesterol follow-up", "Shortness of breath on stairs", "Heart rhythm check" },
        "Dermatology" => new[] { "Mole check", "Itchy rash", "Acne", "Skin cancer screening", "Hair loss", "Eczema flare-up", "Wart removal" },
        "Orthopedics" => new[] { "Knee pain", "Shoulder pain", "Ankle sprain follow-up", "Back pain", "Wrist pain after a fall", "Hip pain when walking" },
        _ => new[]
        {
            "Annual physical", "Blood pressure check", "Diabetes check-up", "Persistent cough", "Headaches", "Medication review", "Feeling tired", "Sore throat", "Back pain", "Flu symptoms",
            "Travel vaccinations", "Sleep problems",
        },
    };

    /// <summary>The doctor's notes and the summary for the patient of a completed visit, by specialty (plain text, short).</summary>
    private static (string Notes, string Summary) VisitTextFor(string specialty, int variant) => (specialty, variant % 2) switch
    {
        ("Pediatrics", 0) => ("Well appearing. Exam normal. Growth on track.", "Your child is growing well. No treatment needed today."),
        ("Pediatrics", _) => ("Viral illness likely. Fluids and rest. Return if fever lasts over 3 days.", "This looks like a virus. Give plenty of fluids and rest, and call us if the fever lasts more than 3 days."),
        ("Cardiology", 0) => ("ECG normal sinus rhythm. Continue current medicines. Lipids next visit.", "Your heart tracing was normal. Keep taking your medicines as before."),
        ("Cardiology", _) => ("Palpitations, likely benign. Holter monitor considered; reduce caffeine.", "Your heartbeat changes look harmless. Cut back on caffeine and tell us if they get worse."),
        ("Dermatology", 0) => ("Benign-appearing lesions. No biopsy needed. Sun protection advised.", "Your skin check looked fine. Use sunscreen and check your skin once a month."),
        ("Dermatology", _) => ("Mild eczema. Moisturize twice daily; topical steroid for flares.", "This is mild eczema. Moisturize twice a day and use the cream on flare-ups only."),
        ("Orthopedics", 0) => ("Strain, no instability. Physical therapy referral. Ice and rest.", "This is a strain. Rest it, use ice, and start the exercises from physical therapy."),
        ("Orthopedics", _) => ("Degenerative changes. Anti-inflammatory as needed; review in 6 weeks.", "The joint shows some wear. Take the anti-inflammatory when needed and come back in 6 weeks."),
        (_, 0) => ("Stable. Vitals normal. Continue current plan; recheck in 3 months.", "Everything looks stable. Keep up your current plan, and book a check-up in three months."),
        _ => ("Symptoms improving. Supportive care. Follow up if not better in 2 weeks.", "You're getting better. Rest and fluids should help; book again if you're not better in two weeks."),
    };

    #endregion

    #endregion
}

/// <summary>A fresh tab's data, built by <see cref="HcSeedData.Build" />.</summary>
public sealed record HcSeedSet(
    List<HcAppointment> Appointments,
    List<ShowcaseAuditEntry> Audit,
    List<HcAvailability> Availability,
    List<HcBill> Bills,
    List<HcLabOrder> LabOrders,
    List<ShowcaseNotification> Notifications,
    List<HcPatient> Patients,
    List<HcPrescription> Prescriptions,
    List<HcStaffProfile> StaffProfiles,
    List<HcTimeOff> TimeOff,
    IReadOnlyList<HcDoctor> Doctors,
    List<HcClaim> Claims);
