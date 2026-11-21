using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>
/// The tab snapshot of <see cref="HcState" /> (devspec finding F-4): only what differs from the deterministic seed is
/// stored — records changed or added, the IDs of seeded records removed, and the ID counters — and the restore rebuilds the
/// rest from the seed and validates every part before applying it, so a fresh tab's snapshot stays a few hundred
/// characters although it holds thousands of records.
/// </summary>
public sealed partial class HcState
{
    #region Methods

    #region Public Methods

    /// <summary>Returns the snapshot: the ID counters and, of every list, only what differs from the fresh tab's seed.</summary>
    public object CaptureData()
    {
        var data = seed.Value;
        var (changedAppointments, removedAppointments) = Diff(appointments, data.Appointments, item => item.Id, SameAppointment);
        var (changedAudit, removedAudit) = Diff(audit, data.Audit, entry => entry.Id, SameAudit);
        var (changedAvailability, removedAvailability) = Diff(availability, data.Availability, item => item.Id, SameJson);
        var (changedBills, removedBills) = Diff(bills, data.Bills, item => item.Id, SameBill);
        var (changedLabs, removedLabs) = Diff(labOrders, data.LabOrders, item => item.Id, SameLab);
        var (changedNotifications, removedNotifications) = Diff(notifications, data.Notifications, notice => notice.Id, SameNotification);
        var (changedPatients, removedPatients) = Diff(patients, data.Patients, item => item.Id, SamePatient);
        var (changedPrescriptions, removedPrescriptions) = Diff(prescriptions, data.Prescriptions, item => item.Id, SamePrescription);
        var (changedProfiles, removedProfiles) = Diff(staffProfiles, data.StaffProfiles, item => item.AccountId, SameJson);
        var (changedTimeOff, removedTimeOff) = Diff(timeOff, data.TimeOff, item => item.Id, SameJson);
        var doctorDetails = Doctors.Where(doctor => data.Doctors.First(seeded => seeded.Id == doctor.Id) is var seeded && (seeded.AcceptingNewPatients != doctor.AcceptingNewPatients || seeded.Bio != doctor.Bio))
            .Select(doctor => new HcDoctorDetails { Id = doctor.Id, AcceptingNewPatients = doctor.AcceptingNewPatients, Bio = doctor.Bio })
            .ToList();

        return new HcSnapshot
        {
            Appointments = changedAppointments,
            RemovedAppointmentIds = removedAppointments,
            Audit = changedAudit,
            RemovedAuditIds = removedAudit,
            Availability = changedAvailability,
            RemovedAvailabilityIds = removedAvailability,
            Bills = changedBills,
            RemovedBillIds = removedBills,
            DoctorDetails = doctorDetails.Count == 0 ? null : doctorDetails,
            LabOrders = changedLabs,
            RemovedLabOrderIds = removedLabs,
            Notifications = changedNotifications,
            RemovedNotificationIds = removedNotifications,
            Patients = changedPatients,
            RemovedPatientIds = removedPatients,
            Prescriptions = changedPrescriptions,
            RemovedPrescriptionIds = removedPrescriptions,
            StaffProfiles = changedProfiles,
            RemovedStaffProfileIds = removedProfiles,
            TimeOff = changedTimeOff,
            RemovedTimeOffIds = removedTimeOff,
            NextAppointmentId = nextAppointmentId,
            NextAuditId = nextAuditId,
            NextAvailabilityId = nextAvailabilityId,
            NextBillId = nextBillId,
            NextLabOrderId = nextLabOrderId,
            NextNotificationId = nextNotificationId,
            NextPatientId = nextPatientId,
            NextPrescriptionId = nextPrescriptionId,
            NextTimeOffId = nextTimeOffId,
        };
    }

    /// <summary>
    /// Rebuilds the portal from the seed and a stored snapshot's changes, validates every part against itself, and applies it
    /// only when everything is valid: known IDs and references, defined enum values, numbers and dates in range, bounded text,
    /// unique record numbers, appointments that never double-book a doctor or a patient, and links that stay on this site.
    /// </summary>
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

        if (snapshot is null
            || new[]
                {
                    snapshot.NextAppointmentId, snapshot.NextAuditId, snapshot.NextAvailabilityId, snapshot.NextBillId, snapshot.NextLabOrderId, snapshot.NextNotificationId,
                    snapshot.NextPatientId, snapshot.NextPrescriptionId, snapshot.NextTimeOffId,
                }
                .Any(id => id is < 1 or > NextIdLimit))
            return false;

        // A fresh copy of the seed: the stored changes replace, remove, and add records on it; an inconsistent change list discards the snapshot.
        var fresh = HcSeedData.Build();

        if (Overlay(fresh.Appointments, snapshot.Appointments, snapshot.RemovedAppointmentIds, item => item.Id) is not { Count: <= MaxAppointments } storedAppointments
            || Overlay(fresh.Audit, snapshot.Audit, snapshot.RemovedAuditIds, entry => entry.Id) is not { Count: <= MaxAudit } storedAudit
            || Overlay(fresh.Availability, snapshot.Availability, snapshot.RemovedAvailabilityIds, item => item.Id) is not { Count: <= MaxAvailability } storedAvailability
            || Overlay(fresh.Bills, snapshot.Bills, snapshot.RemovedBillIds, item => item.Id) is not { Count: <= MaxBills } storedBills
            || Overlay(fresh.LabOrders, snapshot.LabOrders, snapshot.RemovedLabOrderIds, item => item.Id) is not { Count: <= MaxLabOrders } storedLabs
            || Overlay(fresh.Notifications, snapshot.Notifications, snapshot.RemovedNotificationIds, notice => notice.Id) is not { Count: <= MaxNotifications } storedNotifications
            || Overlay(fresh.Patients, snapshot.Patients, snapshot.RemovedPatientIds, item => item.Id) is not { Count: > 0 and <= MaxPatients } storedPatients
            || Overlay(fresh.Prescriptions, snapshot.Prescriptions, snapshot.RemovedPrescriptionIds, item => item.Id) is not { Count: <= MaxPrescriptions } storedPrescriptions
            || Overlay(fresh.StaffProfiles, snapshot.StaffProfiles, snapshot.RemovedStaffProfileIds, item => item.AccountId) is not { Count: <= ShowcaseIdentity.MaxAccounts } storedProfiles
            || Overlay(fresh.TimeOff, snapshot.TimeOff, snapshot.RemovedTimeOffIds, item => item.Id) is not { Count: <= MaxTimeOff } storedTimeOff
            || snapshot.DoctorDetails is { } details && (details.Count > Doctors.Count || details.Any(item => item is null || FindDoctor(item.Id) is null || item.Bio is not { Length: >= MinBioLength and <= MaxBioLength })
                || !Unique(details.Select(item => item.Id))))
            return false;

        if (!ValidPatients(storedPatients, snapshot, fresh.Patients))
            return false;

        var patientIds = storedPatients.Select(patient => patient.Id).ToHashSet();

        if (storedAppointments.Any(appointment => !ValidAppointment(appointment, snapshot, patientIds)) || !Unique(storedAppointments.Select(appointment => appointment.Id)))
            return false;

        // Two live appointments never hold the same half hour of a doctor, and a patient is never in two visits at once.
        static IEnumerable<DateTime> HalfHours(HcAppointment appointment) => Enumerable.Range(0, appointment.Minutes / SlotMinutes).Select(index => appointment.Start.AddMinutes(index * SlotMinutes));

        if (!Unique(storedAppointments.Where(appointment => appointment.Status != HcAppointmentStatus.Cancelled).SelectMany(appointment => HalfHours(appointment).Select(start => (appointment.DoctorId, start))))
            || !Unique(storedAppointments.Where(appointment => appointment.Status is not (HcAppointmentStatus.Cancelled or HcAppointmentStatus.NoShow))
                .SelectMany(appointment => HalfHours(appointment).Select(start => (appointment.PatientId, start)))))
            return false;

        if (storedAvailability.Any(block => block is null || block.Id < 1 || block.Id >= snapshot.NextAvailabilityId || FindDoctor(block.DoctorId) is null || FindClinic(block.ClinicId) is not { } clinic
                || !ValidBlock(block.Day, block.StartMinute, block.EndMinute, clinic))
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

        if (storedPrescriptions.Any(prescription => !ValidPrescription(prescription, snapshot, patientIds)) || !Unique(storedPrescriptions.Select(prescription => prescription.Id)))
            return false;

        var labIds = storedLabs.Select(order => order.Id).ToHashSet();

        if (storedBills.Any(bill => bill is null || bill.Id < 1 || bill.Id >= snapshot.NextBillId || !patientIds.Contains(bill.PatientId) || bill.AmountCents is < 1 or > 1_000_000
                || !IsText(bill.Description, MaxBillDescriptionLength) || !IsPlausible(bill.CreatedAt) || !IsPlausible(bill.DueDate) || !Enum.IsDefined(bill.Status)
                || (bill.AppointmentId is { } appointmentId && (!appointmentsById.TryGetValue(appointmentId, out var visit) || visit.PatientId != bill.PatientId))
                || (bill.LabOrderId is { } labId && !labIds.Contains(labId)) || (bill.PaidAt is not null) != (bill.Status == HcBillStatus.Paid)
                || bill.CardBrand is not { Length: <= 20 } || bill.CardLast4 is not { Length: <= 4 } || (bill.CardLast4.Length > 0 && !bill.CardLast4.All(char.IsAsciiDigit)))
            || !Unique(storedBills.Select(bill => bill.Id)))
            return false;

        if (storedAudit.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextAuditId || !IsPlausible(entry.At) || !IsText(entry.Actor, ShowcaseIdentity.MaxNameLength)
                || !IsText(entry.Action, MaxAuditTextLength) || !IsText(entry.Target, MaxAuditTextLength) || (entry.Details is not null && !IsText(entry.Details, MaxAuditTextLength))
                || !IsSiteLink(entry.TargetUrl))
            || !Unique(storedAudit.Select(entry => entry.Id)))
            return false;

        if (storedNotifications.Any(notice => notice is null || notice.Id < 1 || notice.Id >= snapshot.NextNotificationId || !IsAccountId(notice.AccountId) || !IsPlausible(notice.At)
                || !IsText(notice.Title, MaxNoticeTitleLength) || (notice.Text is not null && !IsText(notice.Text, MaxNoticeTextLength)) || !IsSiteLink(notice.Url))
            || !Unique(storedNotifications.Select(notice => notice.Id)))
            return false;

        if (storedProfiles.Any(profile => profile is null || !IsAccountId(profile.AccountId) || FindClinic(profile.ClinicId) is null || profile.WorkPhone is not { } phone || (phone.Length > 0 && !IsPhone(phone))))
            return false;

        Replace(appointments, storedAppointments);
        Replace(audit, storedAudit);
        Replace(availability, storedAvailability);
        Replace(bills, storedBills);
        Replace(labOrders, storedLabs);
        Replace(notifications, storedNotifications);
        Replace(patients, storedPatients);
        Replace(prescriptions, storedPrescriptions);
        Replace(staffProfiles, storedProfiles);
        Replace(timeOff, storedTimeOff);

        foreach (var doctor in Doctors)
        {
            var seeded = fresh.Doctors.First(item => item.Id == doctor.Id);
            var stored = snapshot.DoctorDetails?.FirstOrDefault(item => item.Id == doctor.Id);
            (doctor.AcceptingNewPatients, doctor.Bio) = stored is null ? (seeded.AcceptingNewPatients, seeded.Bio) : (stored.AcceptingNewPatients, stored.Bio);
        }

        nextAppointmentId = snapshot.NextAppointmentId;
        nextAuditId = snapshot.NextAuditId;
        nextAvailabilityId = snapshot.NextAvailabilityId;
        nextBillId = snapshot.NextBillId;
        nextLabOrderId = snapshot.NextLabOrderId;
        nextNotificationId = snapshot.NextNotificationId;
        nextPatientId = snapshot.NextPatientId;
        nextPrescriptionId = snapshot.NextPrescriptionId;
        nextTimeOffId = snapshot.NextTimeOffId;

        return true;
    }

    #endregion

    #region Private Methods

    [GeneratedRegex("^[A-Z0-9][A-Z0-9.]{2,7}$")]
    private static partial Regex CodePattern();

    /// <summary>
    /// Returns the records that differ from the seed (changed or added, in list order) and the keys of seeded records that
    /// were removed; <see langword="null" /> for an empty list, so it is left out of the snapshot.
    /// </summary>
    private static (List<T>? Changed, List<TKey>? Removed) Diff<T, TKey>(IReadOnlyList<T> current, IReadOnlyList<T> seeded, Func<T, TKey> key, Func<T, T, bool> equals)
        where TKey : notnull
    {
        var seededByKey = seeded.ToDictionary(key);
        var currentKeys = new HashSet<TKey>();
        var changed = new List<T>();

        foreach (var item in current)
        {
            var itemKey = key(item);
            currentKeys.Add(itemKey);

            if (!seededByKey.TryGetValue(itemKey, out var original) || !equals(original, item))
                changed.Add(item);
        }

        var removed = seeded.Where(item => !currentKeys.Contains(key(item))).Select(key).ToList();

        return (changed.Count == 0 ? null : changed, removed.Count == 0 ? null : removed);
    }

    /// <summary>
    /// Rebuilds a list from the seed and a snapshot's changes: seeded records in their order (replaced by a stored record
    /// with the same key, left out when removed), then the added records in their stored order. Returns
    /// <see langword="null" /> for an inconsistent change list (a missing record, a repeated key, or a removed key that isn't
    /// seeded), which discards the snapshot.
    /// </summary>
    private static List<T>? Overlay<T, TKey>(IReadOnlyList<T> seeded, List<T>? changed, List<TKey>? removed, Func<T, TKey> key)
        where TKey : notnull
    {
        changed ??= new List<T>();
        removed ??= new List<TKey>();

        if (changed.Count > 20_000 || removed.Count > 20_000 || changed.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var replacements = new Dictionary<TKey, T>();

        foreach (var item in changed)
        {
            if (key(item) is not { } itemKey || !replacements.TryAdd(itemKey, item))
                return null;
        }

        var seededKeys = seeded.Select(key).ToHashSet();
        var removedKeys = removed.ToHashSet();

        if (removedKeys.Count != removed.Count || removedKeys.Any(item => !seededKeys.Contains(item) || replacements.ContainsKey(item)))
            return null;

        var result = new List<T>(seeded.Count + changed.Count);

        foreach (var item in seeded)
        {
            var itemKey = key(item);

            if (replacements.TryGetValue(itemKey, out var replacement))
                result.Add(replacement);
            else if (!removedKeys.Contains(itemKey))
                result.Add(item);
        }

        result.AddRange(changed.Where(item => !seededKeys.Contains(key(item))));

        return result;
    }

    private static bool SameAppointment(HcAppointment left, HcAppointment right) =>
        left.Id == right.Id && left.PatientId == right.PatientId && left.DoctorId == right.DoctorId && left.ClinicId == right.ClinicId && left.Start == right.Start && left.Minutes == right.Minutes
        && left.VisitType == right.VisitType && left.Reason == right.Reason && left.Status == right.Status && left.CreatedAt == right.CreatedAt && left.CheckedInAt == right.CheckedInAt
        && left.CompletedAt == right.CompletedAt && left.CancelledAt == right.CancelledAt && left.CancelReason == right.CancelReason && left.Notes == right.Notes && left.Summary == right.Summary
        && left.DiagnosisCodes.SequenceEqual(right.DiagnosisCodes) && left.ProcedureCodes.SequenceEqual(right.ProcedureCodes);

    private static bool SameAudit(ShowcaseAuditEntry left, ShowcaseAuditEntry right) =>
        left.Id == right.Id && left.At == right.At && left.Actor == right.Actor && left.Action == right.Action && left.Target == right.Target
        && left.TargetUrl == right.TargetUrl && left.Details == right.Details;

    private static bool SameBill(HcBill left, HcBill right) =>
        left.Id == right.Id && left.PatientId == right.PatientId && left.AppointmentId == right.AppointmentId && left.LabOrderId == right.LabOrderId && left.AmountCents == right.AmountCents
        && left.Description == right.Description && left.CreatedAt == right.CreatedAt && left.DueDate == right.DueDate && left.Status == right.Status && left.PaidAt == right.PaidAt
        && left.CardBrand == right.CardBrand && left.CardLast4 == right.CardLast4;

    private static bool SameJson<T>(T left, T right) => JsonSerializer.Serialize(left, snapshotOptions) == JsonSerializer.Serialize(right, snapshotOptions);

    private static bool SameLab(HcLabOrder left, HcLabOrder right) =>
        left.Id == right.Id && left.PatientId == right.PatientId && left.DoctorId == right.DoctorId && left.AppointmentId == right.AppointmentId && left.PanelId == right.PanelId
        && left.OrderedAt == right.OrderedAt && left.Status == right.Status && left.ResultedAt == right.ResultedAt && left.ReleasedAt == right.ReleasedAt && left.Comment == right.Comment
        && left.Values.SequenceEqual(right.Values);

    private static bool SameNotification(ShowcaseNotification left, ShowcaseNotification right) =>
        left.Id == right.Id && left.AccountId == right.AccountId && left.At == right.At && left.IsRead == right.IsRead && left.Title == right.Title && left.Text == right.Text
        && left.Url == right.Url;

    private static bool SamePatient(HcPatient left, HcPatient right) =>
        left.Id == right.Id && left.Mrn == right.Mrn && left.AccountId == right.AccountId && left.Name == right.Name && left.DateOfBirth == right.DateOfBirth && left.Email == right.Email
        && left.Phone == right.Phone && left.ClinicId == right.ClinicId && left.RegisteredAt == right.RegisteredAt && left.EmergencyContactName == right.EmergencyContactName
        && left.EmergencyContactPhone == right.EmergencyContactPhone && left.InsurancePayerId == right.InsurancePayerId && left.InsuranceMemberId == right.InsuranceMemberId
        && left.Allergies.SequenceEqual(right.Allergies) && left.Conditions.SequenceEqual(right.Conditions) && left.Immunizations.SequenceEqual(right.Immunizations);

    private static bool SamePrescription(HcPrescription left, HcPrescription right) =>
        left.Id == right.Id && left.PatientId == right.PatientId && left.DoctorId == right.DoctorId && left.Medication == right.Medication && left.Dose == right.Dose
        && left.Directions == right.Directions && left.Quantity == right.Quantity && left.DaysSupply == right.DaysSupply && left.Refills == right.Refills && left.PrescribedAt == right.PrescribedAt
        && left.LastFilledOn == right.LastFilledOn && left.Status == right.Status && left.RefillNote == right.RefillNote && left.RefillOutcome == right.RefillOutcome
        && left.RefillRequestedAt == right.RefillRequestedAt && left.RefillPharmacyId == right.RefillPharmacyId && left.RefillQuantity == right.RefillQuantity && left.RefillPickupOn == right.RefillPickupOn;

    private static bool ValidCodes(List<string>? codes) => codes is { Count: <= MaxCodes } && codes.All(code => code is not null && CodePattern().IsMatch(code)) && Unique(codes);

    private bool ValidAppointment(HcAppointment? appointment, HcSnapshot snapshot, HashSet<int> patientIds) =>
        appointment is not null && appointment.Id >= 1 && appointment.Id < snapshot.NextAppointmentId && patientIds.Contains(appointment.PatientId) && FindDoctor(appointment.DoctorId) is not null
        && FindClinic(appointment.ClinicId) is not null && IsPlausible(appointment.Start) && appointment.Start.TimeOfDay.Ticks % TimeSpan.FromMinutes(SlotMinutes).Ticks == 0
        && Enum.IsDefined(appointment.VisitType) && appointment.Minutes == HcVisitTypes.MinutesOf(appointment.VisitType) && IsText(appointment.Reason, MaxReasonLength)
        && Enum.IsDefined(appointment.Status) && IsPlausible(appointment.CreatedAt) && appointment.Notes is { Length: <= MaxNotesLength } && appointment.Summary is { Length: <= MaxSummaryLength }
        && appointment.CancelReason is { Length: <= MaxReasonLength } && ValidCodes(appointment.DiagnosisCodes) && ValidCodes(appointment.ProcedureCodes)
        && (appointment.CancelledAt is not null) == (appointment.Status == HcAppointmentStatus.Cancelled)
        && (appointment.CompletedAt is not null) == (appointment.Status == HcAppointmentStatus.Completed)
        && !(appointment.CheckedInAt is null && appointment.Status is HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit);

    /// <summary>
    /// Checks the patients: unique IDs, record numbers, and accounts; a seeded record keeps its account (or none) exactly, any
    /// other record belongs to one registered account or to none (registered by the front desk); and every field in range.
    /// </summary>
    private bool ValidPatients(List<HcPatient> stored, HcSnapshot snapshot, List<HcPatient> seeded)
    {
        var seededAccounts = seeded.ToDictionary(patient => patient.Id, patient => patient.AccountId);

        return !stored.Any(patient => patient is null || patient.Id < 1 || patient.Id >= snapshot.NextPatientId || !IsText(patient.Name, ShowcaseIdentity.MaxNameLength)
                || !IsText(patient.Email, ShowcaseIdentity.MaxEmailLength) || !IsMrn(patient.Mrn) || patient.Mrn != patient.Mrn.ToUpperInvariant()
                || (seededAccounts.TryGetValue(patient.Id, out var seededAccount) ? patient.AccountId != seededAccount : patient.AccountId is not null && !patient.AccountId.StartsWith("u-", StringComparison.Ordinal))
                || patient.Phone is not { Length: <= MaxPhoneLength } phone || (phone.Length > 0 && !IsPhone(phone))
                || (patient.DateOfBirth is { } dob && (dob < HcSeedData.Now.AddYears(-MaxAgeYears - 1) || dob > HcSeedData.Now.AddYears(3)))
                || (patient.AccountId is not null && !IsAccountId(patient.AccountId)) || !ValidList(patient.Allergies) || !ValidList(patient.Conditions)
                || patient.ClinicId is null || (patient.ClinicId.Length > 0 && FindClinic(patient.ClinicId) is null) || patient.RegisteredAt < HcSeedData.Now.AddYears(-MaxAgeYears) || patient.RegisteredAt > HcSeedData.Now.AddYears(3)
                || patient.EmergencyContactName is null || patient.EmergencyContactPhone is null || !ValidEmergencyContact(patient.EmergencyContactName, patient.EmergencyContactPhone, patient.Phone)
                || patient.InsurancePayerId is null || patient.InsuranceMemberId is null || MemberIdProblem(patient.InsurancePayerId, patient.InsuranceMemberId) is not null
                || patient.Immunizations is not { Count: <= MaxListItems } || patient.Immunizations.Any(item => item is null || !IsText(item.Name, MaxListItemLength) || !IsPlausible(item.Date))
                || !Unique(patient.Immunizations.Select(item => (item.Name.ToLowerInvariant(), item.Date))))
            && Unique(stored.Select(patient => patient.Id)) && Unique(stored.Select(patient => patient.Mrn))
            && Unique(stored.Where(patient => patient.AccountId is not null).Select(patient => patient.AccountId));
    }

    /// <summary>Checks a prescription: known references, bounded text and numbers, and a waiting refill request that is complete or absent.</summary>
    private bool ValidPrescription(HcPrescription? prescription, HcSnapshot snapshot, HashSet<int> patientIds) =>
        prescription is not null && prescription.Id >= 1 && prescription.Id < snapshot.NextPrescriptionId && patientIds.Contains(prescription.PatientId) && FindDoctor(prescription.DoctorId) is not null
        && IsText(prescription.Medication, MaxMedicationLength) && IsText(prescription.Dose, MaxDoseLength) && IsText(prescription.Directions, MaxDirectionsLength)
        && prescription.Quantity is >= 1 and <= MaxQuantity && prescription.Refills is >= 0 and <= MaxRefills && prescription.DaysSupply is >= 1 and <= 90
        && Enum.IsDefined(prescription.Status) && Enum.IsDefined(prescription.RefillOutcome) && IsPlausible(prescription.PrescribedAt) && IsPlausible(prescription.LastFilledOn)
        && prescription.LastFilledOn.Date >= prescription.PrescribedAt.Date && prescription.RefillNote is { Length: <= MaxReasonLength } && prescription.RefillPharmacyId is not null
        && (prescription.RefillRequestedAt is { } requested
            ? prescription.Status == HcPrescriptionStatus.Active && IsPlausible(requested) && FindPharmacy(prescription.RefillPharmacyId) is not null
              && prescription.RefillQuantity >= 1 && prescription.RefillQuantity <= prescription.Quantity && prescription.RefillPickupOn is { } pickup && IsPlausible(pickup)
            : prescription.RefillPharmacyId.Length == 0 && prescription.RefillQuantity == 0 && prescription.RefillPickupOn is null);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the snapshot format version: 2 stores only what differs from the seed, and adds the audit log, notifications,
    /// staff work details, doctors' profiles, visit types, registration and health plan details, refill requests with a
    /// pharmacy and a day, and room for visit codes.
    /// </summary>
    public int SnapshotVersion => 2;

    #endregion

    #region Other

    /// <summary>A doctor's work details that differ from the seed.</summary>
    private sealed class HcDoctorDetails
    {
        public bool AcceptingNewPatients { get; set; }

        public string Bio { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// The stored form of the state: the ID counters and, of each list, the records that differ from the seed (changed or
    /// added) and the IDs of seeded records that were removed; it is rebuilt over the seed on restore. Empty change lists are
    /// left out of the JSON.
    /// </summary>
    private sealed class HcSnapshot
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcAppointment>? Appointments { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseAuditEntry>? Audit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcAvailability>? Availability { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcBill>? Bills { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcDoctorDetails>? DoctorDetails { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcLabOrder>? LabOrders { get; set; }

        public int NextAppointmentId { get; set; }

        public int NextAuditId { get; set; }

        public int NextAvailabilityId { get; set; }

        public int NextBillId { get; set; }

        public int NextLabOrderId { get; set; }

        public int NextNotificationId { get; set; }

        public int NextPatientId { get; set; }

        public int NextPrescriptionId { get; set; }

        public int NextTimeOffId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ShowcaseNotification>? Notifications { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcPatient>? Patients { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcPrescription>? Prescriptions { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAppointmentIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAuditIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedAvailabilityIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedBillIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedLabOrderIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedNotificationIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedPatientIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedPrescriptionIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RemovedStaffProfileIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<int>? RemovedTimeOffIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcStaffProfile>? StaffProfiles { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<HcTimeOff>? TimeOff { get; set; }
    }

    #endregion
}
