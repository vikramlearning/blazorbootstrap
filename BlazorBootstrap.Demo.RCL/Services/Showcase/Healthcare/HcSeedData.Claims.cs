using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>
/// The insurance claims part of a fresh tab (the Healthcare signature workflow, AC-7h): the code lists the clinics bill with
/// (illustrative ICD-10-style diagnoses, CPT-style procedures with fees, and plan reason codes), the vital signs of past
/// visits, and a claim for every coded visit of a patient with a health plan — paid, partly paid, denied and waiting for a
/// correction or an appeal, corrected, appealed (won or lost), written off, or billed to the patient — with the patients'
/// bills reduced by what the plans paid. A few visits of the last days still wait to be coded.
/// </summary>
public static partial class HcSeedData
{
    #region Fields and Constants

    /// <summary>How many days after a denial an appeal can be filed.</summary>
    public const int AppealDays = 60;

    /// <summary>How many days after a visit its claim must be submitted (the plans' timely-filing limit).</summary>
    public const int FilingDays = 90;

    /// <summary>The name the seeded audit log shows for the billing specialist.</summary>
    private const string BillingName = "Rafael Moreno";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the claim number of a claim ID in a year, like "CLM-2026-00412".</summary>
    public static string ClaimNumber(int year, int claimId) => Invariant($"CLM-{year}-{claimId:00000}");

    /// <summary>Returns the staff route of a claim.</summary>
    public static string ClaimPath(int claimId) => Invariant($"{StaffBase}/claims/{claimId}");

    /// <summary>The reason and adjustment codes plans answer with (illustrative). "Needs doctor" codes are about the coding or the medical need.</summary>
    public static IReadOnlyList<HcDenialCode> DenialCodes() => new List<HcDenialCode>
    {
        new("CO-11", "The diagnosis doesn't support the procedure", NeedsDoctor: true),
        new("CO-16", "Information is missing or wrong on the claim"),
        new("CO-18", "Duplicate of a claim already sent"),
        new("CO-29", "Sent after the filing deadline"),
        new("CO-31", "Patient can't be identified as the plan's member"),
        new("CO-45", "Charge is above the plan's allowed amount"),
        new("CO-50", "Not medically necessary, in the plan's view", NeedsDoctor: true),
        new("CO-97", "Included in another service on the claim", NeedsDoctor: true),
        new("PR-1", "Deductible: the patient pays"),
        new("PR-2", "Coinsurance: the patient pays"),
        new("PR-3", "Copay: the patient pays"),
    };

    /// <summary>The diagnosis codes the clinics bill with (ICD-10 style; illustrative).</summary>
    public static IReadOnlyList<HcDiagnosisCode> DiagnosisCodes() => new List<HcDiagnosisCode>
    {
        new("B07.9", "Viral wart"),
        new("D22.9", "Mole (melanocytic nevus)"),
        new("E03.9", "Hypothyroidism"),
        new("E11.9", "Type 2 diabetes without complications"),
        new("E78.5", "High cholesterol (hyperlipidemia)"),
        new("F41.9", "Anxiety"),
        new("G47.00", "Insomnia"),
        new("H66.90", "Middle ear infection (otitis media)"),
        new("I10", "High blood pressure (essential hypertension)"),
        new("I48.91", "Atrial fibrillation"),
        new("J06.9", "Upper respiratory infection"),
        new("J30.9", "Allergic rhinitis"),
        new("J45.909", "Asthma, uncomplicated"),
        new("K21.9", "Acid reflux (GERD)"),
        new("L30.9", "Dermatitis"),
        new("L65.9", "Hair loss"),
        new("L70.0", "Acne"),
        new("M17.11", "Osteoarthritis, right knee"),
        new("M25.511", "Pain in the right shoulder"),
        new("M25.531", "Pain in the right wrist"),
        new("M25.551", "Pain in the right hip"),
        new("M54.50", "Low back pain"),
        new("R00.2", "Palpitations"),
        new("R05.9", "Cough"),
        new("R07.89", "Chest pain, other"),
        new("R10.9", "Abdominal pain"),
        new("R51.9", "Headache"),
        new("R53.83", "Tiredness (fatigue)"),
        new("S93.401A", "Ankle sprain, first visit"),
        new("Z00.00", "Adult check-up"),
        new("Z00.129", "Child check-up"),
        new("Z23", "Vaccination"),
    };

    /// <summary>The procedure codes the clinics bill with (CPT style; descriptions and fees are illustrative).</summary>
    public static IReadOnlyList<HcProcedureCode> ProcedureCodes() => new List<HcProcedureCode>
    {
        new("99202", "Office visit, new patient, short", 11_000, IsVisit: true, ForNewPatient: true),
        new("99203", "Office visit, new patient, moderate", 16_000, IsVisit: true, ForNewPatient: true),
        new("99204", "Office visit, new patient, extended", 24_000, IsVisit: true, ForNewPatient: true),
        new("99212", "Office visit, established patient, short", 7_500, IsVisit: true, ForNewPatient: false),
        new("99213", "Office visit, established patient, moderate", 11_500, IsVisit: true, ForNewPatient: false),
        new("99214", "Office visit, established patient, extended", 16_500, IsVisit: true, ForNewPatient: false),
        new("99395", "Preventive check-up, adult 18 to 39", 19_500, IsVisit: true),
        new("99396", "Preventive check-up, adult 40 to 64", 21_000, IsVisit: true),
        new("11102", "Skin biopsy", 14_500),
        new("17110", "Wart removal", 12_000),
        new("20610", "Joint injection, large joint", 13_500),
        new("36415", "Blood draw", 1_500),
        new("90686", "Flu vaccine", 3_500),
        new("93000", "Heart tracing (ECG) with reading", 6_000),
        new("94640", "Inhaler treatment in the clinic", 4_500),
    };

    #endregion

    #region Private Methods

    /// <summary>Adds vital signs to past visits: plausible values for the patient's age, a few with high blood pressure.</summary>
    private static void AddVitals(List<HcAppointment> appointments)
    {
        foreach (var visit in appointments.Where(item => item.Status == HcAppointmentStatus.Completed))
        {
            var seed = visit.Id * 13;
            var high = Pick(seed, 1, 100) < 18;
            visit.Vitals = new HcVitals
            {
                Systolic = (high ? 138 : 108) + Pick(seed, 2, 18),
                Diastolic = (high ? 86 : 66) + Pick(seed, 3, 12),
                Pulse = 58 + Pick(seed, 4, 36),
                TemperatureC = Math.Round(36.3 + (Pick(seed, 5, 12) / 10.0), 1),
                WeightKg = Math.Round(52 + Pick(seed, 6, 50) + (Pick(seed, 7, 10) / 10.0), 1),
            };
        }

        // Maya's annual physical: the blood pressure in the doctor's notes.
        if (appointments.FirstOrDefault(item => item.Id == 1)?.Vitals is { } physical)
            (physical.Systolic, physical.Diastolic, physical.WeightKg) = (128, 82, 74.5);
    }

    /// <summary>Returns the codes a completed visit is billed with: one diagnosis from its reason, its office visit, and what was done in it.</summary>
    private static (List<string> Diagnoses, List<string> Procedures) CodesFor(HcAppointment visit, string specialty, int age, bool hadLab)
    {
        var reason = visit.Reason.ToLowerInvariant();
        string Diagnosis() => reason switch
        {
            _ when reason.Contains("diabetes", StringComparison.Ordinal) || reason.Contains("hba1c", StringComparison.Ordinal) => "E11.9",
            _ when reason.Contains("blood pressure", StringComparison.Ordinal) => "I10",
            _ when reason.Contains("cholesterol", StringComparison.Ordinal) => "E78.5",
            _ when reason.Contains("cough", StringComparison.Ordinal) => "R05.9",
            _ when reason.Contains("throat", StringComparison.Ordinal) || reason.Contains("flu", StringComparison.Ordinal) || reason.Contains("fever", StringComparison.Ordinal) => "J06.9",
            _ when reason.Contains("ear", StringComparison.Ordinal) => "H66.90",
            _ when reason.Contains("headache", StringComparison.Ordinal) => "R51.9",
            _ when reason.Contains("chest", StringComparison.Ordinal) || reason.Contains("breath", StringComparison.Ordinal) => "R07.89",
            _ when reason.Contains("palpitation", StringComparison.Ordinal) || reason.Contains("rhythm", StringComparison.Ordinal) => "R00.2",
            _ when reason.Contains("mole", StringComparison.Ordinal) || reason.Contains("cancer", StringComparison.Ordinal) => "D22.9",
            _ when reason.Contains("acne", StringComparison.Ordinal) => "L70.0",
            _ when reason.Contains("wart", StringComparison.Ordinal) => "B07.9",
            _ when reason.Contains("hair", StringComparison.Ordinal) => "L65.9",
            _ when reason.Contains("rash", StringComparison.Ordinal) || reason.Contains("eczema", StringComparison.Ordinal) => "L30.9",
            _ when reason.Contains("knee", StringComparison.Ordinal) => "M17.11",
            _ when reason.Contains("shoulder", StringComparison.Ordinal) => "M25.511",
            _ when reason.Contains("ankle", StringComparison.Ordinal) => "S93.401A",
            _ when reason.Contains("wrist", StringComparison.Ordinal) => "M25.531",
            _ when reason.Contains("hip", StringComparison.Ordinal) => "M25.551",
            _ when reason.Contains("back pain", StringComparison.Ordinal) => "M54.50",
            _ when reason.Contains("sleep", StringComparison.Ordinal) => "G47.00",
            _ when reason.Contains("tired", StringComparison.Ordinal) => "R53.83",
            _ when reason.Contains("stomach", StringComparison.Ordinal) => "R10.9",
            _ when reason.Contains("vaccination", StringComparison.Ordinal) => "Z23",
            _ when reason.Contains("allergy", StringComparison.Ordinal) => "J30.9",
            _ => age < 18 ? "Z00.129" : "Z00.00",
        };

        var diagnosis = Diagnosis();
        var preventive = age >= 18 && age < 65 && (reason.Contains("physical", StringComparison.Ordinal) || diagnosis == "Z00.00");
        var visitCode = preventive ? (age < 40 ? "99395" : "99396")
            : visit.VisitType == HcVisitType.NewPatient ? (specialty == "Cardiology" ? "99204" : "99203")
            : specialty is "Cardiology" or "Orthopedics" && Pick(visit.Id, 60, 100) < 40 ? "99214" : "99213";
        var procedures = new List<string> { visitCode };

        if (hadLab)
            procedures.Add("36415");

        if (specialty == "Cardiology" && Pick(visit.Id, 61, 100) < 55)
            procedures.Add("93000");

        if (diagnosis == "D22.9" && Pick(visit.Id, 62, 100) < 35)
            procedures.Add("11102");

        if (diagnosis == "B07.9")
            procedures.Add("17110");

        if (diagnosis is "M17.11" or "M25.511" && Pick(visit.Id, 63, 100) < 35)
            procedures.Add("20610");

        if (diagnosis == "Z23")
            procedures.Add("90686");

        return (new List<string> { diagnosis }, procedures);
    }

    /// <summary>
    /// Codes the completed visits and claims those of patients with a health plan, from the oldest: the claim is created when
    /// the visit is coded, submitted a day or two later, and answered one to three weeks after that; a denial is then
    /// corrected and resubmitted, appealed, written off, or billed to the patient, or still waits. Each step that is still
    /// in the future isn't taken yet, so the newest claims are drafts or waiting for the plan, and the newest visits wait to
    /// be coded. Four claims of Dr. Okoye's patients are set up for the demo: a denial waiting for a correction, an appeal
    /// close to its deadline, an appeal waiting for the plan, and a correction ready to resubmit. The patients' visit bills
    /// follow: waiting for the plan, then what the patient owes.
    /// </summary>
    private static List<HcClaim> GenerateClaims(List<HcAppointment> appointments, List<HcPatient> patients, IReadOnlyList<HcDoctor> doctors, List<HcLabOrder> labs, List<HcBill> bills,
        out List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)> audit, out List<(string AccountId, DateTime At, string Title, string Text, string Url)> notices)
    {
        audit = new();
        notices = new();
        var auditList = audit;
        var noticeList = notices;
        var claims = new List<HcClaim>();
        var patientsById = patients.ToDictionary(patient => patient.Id);
        var doctorsById = doctors.ToDictionary(doctor => doctor.Id);
        var payers = Payers().ToDictionary(payer => payer.Id);
        var fees = ProcedureCodes().ToDictionary(code => code.Code, code => code.FeeCents);
        var denials = DenialCodes().ToDictionary(code => code.Code);
        var labVisits = labs.Where(order => order.AppointmentId is not null).Select(order => order.AppointmentId!.Value).ToHashSet();
        var billsByVisit = bills.Where(bill => bill.AppointmentId is not null).GroupBy(bill => bill.AppointmentId!.Value).ToDictionary(group => group.Key, group => group.First());
        var nextId = 1;

        // The demo's four set-up claims: Dr. Okoye's insured patients' visits, with the day of the denial.
        var okoyeVisits = appointments.Where(item => item.DoctorId == SeededDoctorId && item.Status == HcAppointmentStatus.Completed && item.PatientId != SeededPatientId
                && patientsById[item.PatientId].InsurancePayerId.Length > 0)
            .OrderBy(item => item.Start).ToList();
        var forced = new Dictionary<int, (string Plan, DateTime DeniedAt, string Code)>();

        void Force(int daysBeforeDenial, string plan, double deniedDaysAgo, string code)
        {
            if (okoyeVisits.LastOrDefault(item => item.Start < Now.AddDays(-deniedDaysAgo - daysBeforeDenial) && !forced.ContainsKey(item.Id)) is { } visit)
                forced[visit.Id] = (plan, Now.AddDays(-deniedDaysAgo).Date.AddHours(10).AddMinutes(15), code);
        }

        Force(16, "open", 2.4, "CO-16");
        Force(16, "open", 52.3, "CO-50");
        Force(16, "appeal-open", 31.2, "CO-50");
        Force(16, "corrected-open", 9.3, "CO-11");
        Force(16, "appeal-final", 45.2, "CO-50");

        void Note(string accountId, DateTime at, string title, string text, string url) => noticeList.Add((accountId, at, title, text, url));

        foreach (var visit in appointments.Where(item => item.Status == HcAppointmentStatus.Completed && item.CompletedAt is not null).OrderBy(item => item.CompletedAt))
        {
            var patient = patientsById[visit.PatientId];
            var doctor = doctorsById[visit.DoctorId];
            var age = Age(patient);
            var completed = visit.CompletedAt!.Value;
            var coded = completed.AddMinutes(20 + Pick(visit.Id, 70, 2_400));

            // The newest visits of insured patients wait to be coded (their bills wait with them).
            if (coded > Now || (completed > Now.AddDays(-2) && Pick(visit.Id, 71, 100) < 60 && visit.PatientId != SeededPatientId))
            {
                if (patient.InsurancePayerId.Length > 0 && billsByVisit.TryGetValue(visit.Id, out var waiting))
                    (waiting.Status, waiting.PaidAt, waiting.CardBrand, waiting.CardLast4) = (HcBillStatus.Pending, null, string.Empty, string.Empty);

                if (patient.InsurancePayerId.Length > 0 && completed > Now.AddDays(-3))
                    Note(SeededBillingId, completed.AddMinutes(2), $"Visit to code: {patient.Name}", $"{doctor.Name}, {visit.Start.ToString("MMM d", CultureInfo.InvariantCulture)}: {visit.Reason}. Code it to claim {payers[patient.InsurancePayerId].Name}.", StaffBase + "/claims");

                continue;
            }

            var (diagnoses, procedures) = CodesFor(visit, doctor.Specialty, age, labVisits.Contains(visit.Id));
            visit.DiagnosisCodes = diagnoses;
            visit.ProcedureCodes = procedures;

            if (coded >= Now.AddDays(-AuditDays))
                auditList.Add((coded, doctor.Name, "Visit coded", Invariant($"Appointment {visit.Id} · {patient.Name}"), StaffAppointmentPath(visit.Id), $"Diagnoses {string.Join(", ", diagnoses)}; procedures {string.Join(", ", procedures)}"));

            if (patient.InsurancePayerId.Length == 0 || !payers.TryGetValue(patient.InsurancePayerId, out var payer))
                continue;

            var id = nextId++;
            var billed = procedures.Sum(code => fees[code]);
            var number = ClaimNumber(coded.Year, id);
            var claim = new HcClaim
            {
                Id = id, Number = number, PatientId = patient.Id, AppointmentId = visit.Id, PayerId = payer.Id, MemberId = patient.InsuranceMemberId, CreatedAt = coded,
                DiagnosisCodes = diagnoses.ToList(), ProcedureCodes = procedures.ToList(), BilledCents = billed, Status = HcClaimStatus.Draft,
            };
            var target = $"Claim {number} · {patient.Name}";
            var url = ClaimPath(id);
            claims.Add(claim);

            void Log(DateTime at, string actor, string action, string? details)
            {
                if (at >= Now.AddDays(-AuditDays) && at <= Now)
                    auditList.Add((at, actor, action, target, url, details));
            }

            var plan = forced.TryGetValue(visit.Id, out var set) ? set.Plan : string.Empty;
            var submitted = plan.Length > 0 ? set.DeniedAt.AddDays(-(10 + Pick(id, 72, 4))) : coded.AddHours(4 + Pick(id, 73, 44));
            var answered = plan.Length > 0 ? set.DeniedAt : submitted.AddDays(7 + Pick(id, 74, 14)).Date.AddHours(9 + Pick(id, 75, 8));

            // A typo in the member ID is what a "patient can't be identified" denial corrects.
            var roll = Pick(id, 76, 100);
            var denialCode = plan.Length > 0 ? set.Code : new[] { "CO-16", "CO-50", "CO-11", "CO-31", "CO-97", "CO-16" }[Pick(id, 77, 6)];
            var outcome = plan.Length > 0 ? HcClaimOutcome.Denied : roll < 77 || visit.PatientId == SeededPatientId ? HcClaimOutcome.Paid : roll < 88 ? HcClaimOutcome.PartlyPaid : HcClaimOutcome.Denied;

            // Maya's diabetes follow-up last week is still with her plan; her rash visit was answered.
            if (visit.Id == 3)
                answered = Now.AddDays(5);
            else if (visit.Id == 2)
                answered = completed.AddDays(9).Date.AddHours(11);
            var firstMember = outcome == HcClaimOutcome.Denied && denialCode == "CO-31" ? string.Concat(claim.MemberId.AsSpan(0, claim.MemberId.Length - 2), claim.MemberId.AsSpan(claim.MemberId.Length - 1), claim.MemberId.AsSpan(claim.MemberId.Length - 2, 1)) : claim.MemberId;

            if (firstMember == claim.MemberId && outcome == HcClaimOutcome.Denied && denialCode == "CO-31")
                denialCode = "CO-16";

            claim.MemberId = firstMember;
            claim.Versions.Add(new HcClaimVersion { Number = 1, At = coded, By = doctor.Name, DiagnosisCodes = diagnoses.ToList(), ProcedureCodes = procedures.ToList(), MemberId = firstMember });
            billsByVisit.TryGetValue(visit.Id, out var bill);

            if (bill is not null)
            {
                (bill.Status, bill.PaidAt, bill.CardBrand, bill.CardLast4) = (HcBillStatus.Pending, null, string.Empty, string.Empty);
                bill.AmountCents = billed;
            }

            Log(coded, doctor.Name, "Claim created", Invariant($"{payer.Name}, billed {Money(billed)}"));

            if (submitted > Now)
            {
                if (coded > Now.AddDays(-3))
                    Note(SeededBillingId, coded.AddMinutes(1), $"Claim ready to submit: {number}", $"{doctor.Name} coded {patient.Name}'s visit; {Money(billed)} to {payer.Name}.", url);

                continue;
            }

            (claim.Status, claim.SubmittedAt) = (HcClaimStatus.Submitted, submitted);
            Log(submitted, BillingName, "Claim submitted", $"Version 1 to {payer.Name}");

            if (answered > Now)
                continue;

            // The plan's first answer.
            var allowed = RoundCents(billed * payer.AllowedPercent / 100);
            var share = Math.Min(payer.CopayCents, allowed);

            if (outcome == HcClaimOutcome.Denied)
            {
                Respond(claim, answered, HcClaimOutcome.Denied, 0, 0, new[] { denialCode }, DenialRemark(denialCode), false);
                (claim.Status, claim.AppealDeadline) = (HcClaimStatus.Denied, answered.Date.AddDays(AppealDays));
                Log(answered, BillingName, "Payer response recorded", $"Denied ({denialCode})");
                Note(SeededBillingId, answered.AddMinutes(1), $"Claim denied: {number}", $"{payer.Name}: {denialCode} {denials[denialCode].Description}. Appeal by {claim.AppealDeadline.Value.ToString("MMM d", CultureInfo.InvariantCulture)}.", url);

                if (denials[denialCode].NeedsDoctor && doctor.AccountId is { } doctorAccount)
                    Note(doctorAccount, answered.AddMinutes(2), $"Claim denied: check the codes of {patient.Name}'s visit", $"{payer.Name}: {denialCode} {denials[denialCode].Description}.", StaffAppointmentPath(visit.Id));

                var next = plan.Length > 0 ? plan : (roll % 10) switch { 0 or 1 or 2 or 3 => "corrected", 4 or 5 => "appeal", 6 => "writeoff", 7 => "patient", _ => "open" };
                var step = answered.AddDays(2 + Pick(id, 78, 6));

                // A denial past its appeal deadline was settled long ago.
                if (next == "open" && claim.AppealDeadline < Now.Date)
                    next = "writeoff";

                switch (next)
                {
                    case "corrected" or "corrected-open" when step <= Now || next == "corrected-open":
                    {
                        var correctedAt = next == "corrected-open" ? Now.AddDays(-0.8) : step;
                        var fixedDiagnoses = denialCode is "CO-11" or "CO-50" ? new List<string> { diagnoses[0], age < 18 ? "J06.9" : "I10" }.Distinct().ToList() : diagnoses.ToList();
                        var fixedProcedures = denialCode == "CO-97" && procedures.Count > 1 ? procedures.Take(procedures.Count - 1).ToList() : procedures.ToList();
                        claim.Versions.Add(new HcClaimVersion
                        {
                            Number = 2, At = correctedAt, By = denialCode is "CO-11" or "CO-50" or "CO-97" ? doctor.Name : BillingName, DiagnosisCodes = fixedDiagnoses, ProcedureCodes = fixedProcedures,
                            MemberId = patient.InsuranceMemberId, Note = CorrectionNote(denialCode),
                        });
                        (claim.DiagnosisCodes, claim.ProcedureCodes, claim.MemberId, claim.Status) = (fixedDiagnoses.ToList(), fixedProcedures.ToList(), patient.InsuranceMemberId, HcClaimStatus.Corrected);
                        claim.BilledCents = fixedProcedures.Sum(code => fees[code]);
                        claim.AppealDeadline = null;
                        Log(correctedAt, claim.Versions[^1].By, "Claim corrected", $"Version 2: {CorrectionNote(denialCode)}");

                        if (bill is not null)
                            bill.AmountCents = claim.BilledCents;

                        if (next == "corrected-open")
                        {
                            Note(SeededBillingId, correctedAt.AddMinutes(1), $"Claim corrected: {number}", $"{claim.Versions[^1].By} changed the codes. Resubmit it to {payer.Name}.", url);

                            break;
                        }

                        var resubmitted = correctedAt.AddHours(3);
                        var paidAt = resubmitted.AddDays(8 + Pick(id, 79, 10));

                        if (resubmitted > Now)
                            break;

                        (claim.Status, claim.SubmittedAt) = (HcClaimStatus.Submitted, resubmitted);
                        Log(resubmitted, BillingName, "Claim resubmitted", $"Version 2 to {payer.Name}");

                        if (paidAt <= Now)
                        {
                            var allowed2 = RoundCents(claim.BilledCents * payer.AllowedPercent / 100);
                            var share2 = Math.Min(payer.CopayCents, allowed2);
                            Respond(claim, paidAt, HcClaimOutcome.Paid, allowed2, allowed2 - share2, new[] { "PR-3" }, "Processed after correction.", false);
                            Settle(claim, bill, paidAt, share2, HcClaimStatus.Paid);
                            Log(paidAt, BillingName, "Payer response recorded", $"Paid {Money(allowed2 - share2)}; patient owes {Money(share2)}");
                        }

                        break;
                    }

                    case "appeal" or "appeal-open" or "appeal-final" when step <= Now || next is "appeal-open" or "appeal-final":
                    {
                        var appealedAt = next is "appeal-open" or "appeal-final" ? answered.AddDays(10).Date.AddHours(14) : step;
                        claim.AppealLetter = AppealLetter(patient.Name, payer.Name, denialCode);
                        (claim.Status, claim.AppealedAt) = (HcClaimStatus.Appealed, appealedAt);
                        Log(appealedAt, BillingName, "Claim appealed", Invariant($"Letter {claim.AppealLetter.Length} characters"));
                        // The set-up final denial was decided yesterday and waits for billing to close it.
                        var decided = next == "appeal-final" ? Now.AddDays(-1).Date.AddHours(11) : appealedAt.AddDays(21 + Pick(id, 80, 20));

                        if (next == "appeal-open" || decided > Now)
                            break;

                        if (next != "appeal-final" && Pick(id, 81, 100) < 60)
                        {
                            Respond(claim, decided, HcClaimOutcome.Paid, allowed, allowed - share, new[] { "PR-3" }, "Appeal upheld: paid as billed, less the copay.", true);
                            Settle(claim, bill, decided, share, HcClaimStatus.Paid);
                            Log(decided, BillingName, "Payer response recorded", $"Appeal won: paid {Money(allowed - share)}");
                        }
                        else
                        {
                            Respond(claim, decided, HcClaimOutcome.Denied, 0, 0, new[] { denialCode }, "Appeal reviewed: the denial stands.", true);
                            (claim.Status, claim.AppealDeadline) = (HcClaimStatus.Denied, null);
                            Log(decided, BillingName, "Payer response recorded", $"Appeal denied ({denialCode})");
                            var closed = decided.AddDays(2);

                            if (next == "appeal-final")
                                Note(SeededBillingId, decided.AddMinutes(1), $"Appeal denied: {number}", $"{payer.Name} kept its denial ({denialCode}). Write the claim off or bill the patient.", url);
                            else if (closed <= Now)
                            {
                                (claim.Status, claim.ClosedAt, claim.WriteOffReason) = (HcClaimStatus.WrittenOff, closed, "Appeal denied; written off as a courtesy to the patient.");
                                Settle(claim, bill, closed, 0, HcClaimStatus.WrittenOff);
                                Log(closed, BillingName, "Claim written off", "Reason given (55 characters)");
                            }
                        }

                        break;
                    }

                    case "writeoff" when step <= Now:
                        claim.AppealDeadline = null;
                        (claim.Status, claim.ClosedAt, claim.WriteOffReason) = (HcClaimStatus.WrittenOff, step, "Small balance; not worth an appeal. Written off.");
                        Settle(claim, bill, step, 0, HcClaimStatus.WrittenOff);
                        Log(step, BillingName, "Claim written off", "Reason given (48 characters)");
                        break;

                    case "patient" when step <= Now:
                        claim.AppealDeadline = null;
                        (claim.Status, claim.ClosedAt, claim.WriteOffReason, claim.BilledToPatient) = (HcClaimStatus.WrittenOff, step, "Not covered by the plan; the patient agreed to pay.", true);
                        Settle(claim, bill, step, billed, HcClaimStatus.WrittenOff);
                        Log(step, BillingName, "Billed to the patient", Invariant($"{Money(billed)}; reason given (51 characters)"));
                        break;
                }

                if (next == "open" && plan.Length > 0 && claim.AppealDeadline is { } deadline && deadline <= Now.Date.AddDays(10))
                    Note(SeededBillingId, Now.Date.AddHours(7), $"Appeal deadline {DaysAway((deadline - Now.Date).Days)}: {number}", $"{payer.Name} denied it ({denialCode}). Appeal or close it by {deadline.ToString("MMM d", CultureInfo.InvariantCulture)}.", url);

                continue;
            }

            if (outcome == HcClaimOutcome.PartlyPaid)
            {
                // One line bundled into the visit, or a deductible the patient pays.
                var bundled = procedures.Count > 1;
                var partAllowed = bundled ? RoundCents((billed - fees[procedures[^1]]) * payer.AllowedPercent / 100) : allowed;
                var partShare = bundled ? Math.Min(payer.CopayCents, partAllowed) : Math.Min(partAllowed, payer.CopayCents + 5_000);
                Respond(claim, answered, HcClaimOutcome.PartlyPaid, partAllowed, partAllowed - partShare, bundled ? new[] { "CO-97", "PR-3" } : new[] { "CO-45", "PR-1" },
                    bundled ? "One service is included in another and isn't paid separately." : "Part of the deductible is still open.", false);
                Settle(claim, bill, answered, partShare, HcClaimStatus.PartlyPaid);
                Log(answered, BillingName, "Payer response recorded", $"Partly paid {Money(partAllowed - partShare)}; patient owes {Money(partShare)}");

                continue;
            }

            Respond(claim, answered, HcClaimOutcome.Paid, allowed, allowed - share, new[] { "CO-45", "PR-3" }, "Processed as billed.", false);
            Settle(claim, bill, answered, share, HcClaimStatus.Paid);
            Log(answered, BillingName, "Payer response recorded", $"Paid {Money(allowed - share)}; patient owes {Money(share)}");
        }

        // Maya hears what her plan paid on her recent bills.
        foreach (var claim in claims.Where(item => item.PatientId == SeededPatientId && item.Responses.Count > 0 && item.Responses[^1].At >= Now.AddDays(-60)))
        {
            var response = claim.Responses[^1];
            Note(SeededPatientAccountId, response.At.AddMinutes(5), $"Bill updated after insurance: you owe {Money(response.PatientCents)}",
                $"{payers[claim.PayerId].Name} paid {Money(response.PaidCents)} for your visit.", SiteBase + "/bills");
        }

        return claims;

        void Settle(HcClaim claim, HcBill? bill, DateTime at, int patientCents, HcClaimStatus status)
        {
            claim.Status = status;
            claim.ClosedAt = at;
            claim.AppealDeadline = null;

            if (bill is null)
                return;

            bill.AmountCents = patientCents;

            if (patientCents == 0)
            {
                bill.Status = HcBillStatus.Covered;

                return;
            }

            // The share is due 30 days from the plan's answer. Most patients pay it within a few weeks; the newest are still due.
            bill.DueDate = at.Date.AddDays(30);
            var paidAt = at.AddDays(2 + Pick(claim.Id, 82, 14)).Date.AddHours(19);

            if (paidAt <= Now.AddHours(-1) && (claim.PatientId == SeededPatientId ? at < Now.AddDays(-30) : Pick(claim.Id, 83, 100) < 85))
                (bill.Status, bill.PaidAt, bill.CardBrand, bill.CardLast4) = (HcBillStatus.Paid, paidAt, cards[Pick(claim.Id, 84, cards.Length)].Brand, cards[Pick(claim.Id, 84, cards.Length)].Last4);
            else
                bill.Status = HcBillStatus.Due;
        }
    }

    /// <summary>Returns "today", "tomorrow", or "in N days".</summary>
    internal static string DaysAway(int days) => days switch { <= 0 => "today", 1 => "tomorrow", _ => Invariant($"in {days} days") };

    private static string AppealLetter(string patient, string payer, string code) =>
        $"We ask {payer} to review the denial ({code}) of this claim for {patient}. The visit notes show the symptoms that made the visit and its services necessary, "
        + "and the diagnosis supports each procedure billed. Please reprocess the claim and pay it under the patient's plan.";

    private static string CorrectionNote(string code) => code switch
    {
        "CO-31" => "Member ID corrected from the patient's card.",
        "CO-16" => "Missing information added and member ID checked.",
        "CO-97" => "Removed the service the plan includes in the visit.",
        _ => "Added the diagnosis that supports the procedure.",
    };

    private static string DenialRemark(string code) => code switch
    {
        "CO-31" => "Member not found under this ID.",
        "CO-16" => "Claim lacks information needed to process it.",
        "CO-97" => "A service is included in another one billed.",
        "CO-50" => "Medical necessity not shown for the service.",
        _ => "Diagnosis inconsistent with the procedure.",
    };

    /// <summary>Records a plan's answer on a seeded claim.</summary>
    private static void Respond(HcClaim claim, DateTime at, HcClaimOutcome outcome, int allowed, int paid, IEnumerable<string> codes, string remark, bool afterAppeal) =>
        claim.Responses.Add(new HcClaimResponse
        {
            Version = claim.Versions.Count, At = at, RecordedBy = BillingName, Outcome = outcome, AllowedCents = allowed, PaidCents = paid, PatientCents = allowed - paid,
            DenialCodes = codes.ToList(), Remark = remark, AfterAppeal = afterAppeal,
        });

    private static int RoundCents(int cents) => (int)(Math.Round(cents / 100.0, MidpointRounding.AwayFromZero) * 100);

    #endregion

    #endregion
}
