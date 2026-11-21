using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>
/// The insurance claims workflow of <see cref="HcState" /> (AC-7h), across the doctor, the billing specialist, and the
/// patient. A completed visit of a patient with a health plan waits for coding; the doctor (on the visit page) or billing
/// codes it — one to four diagnoses and exactly one office visit code that fits the visit type, plus what was done — which
/// creates a draft claim billed at the procedure fees, and sends the patient's bill to the plan first. Billing submits the
/// claim within <see cref="HcSeedData.FilingDays" /> days of the visit, then records the plan's answer: paid or partly paid
/// (the patient's bill becomes their share, or nothing), or denied with reason codes. A denial is corrected (a new version
/// of the codes and member ID) and resubmitted, appealed with a letter within <see cref="HcSeedData.AppealDays" /> days, written
/// off, or billed to the patient; a denied appeal can only be closed. Every step is audited (codes, amounts, and lengths of
/// letters and notes only), and the people who must act hear about it.
/// </summary>
public sealed partial class HcState
{
    #region Fields and Constants

    /// <summary>The longest appeal letter.</summary>
    public const int MaxAppealLength = 2000;

    /// <summary>The most diagnosis codes on a claim.</summary>
    public const int MaxDiagnoses = 4;

    /// <summary>The longest correction note, write-off reason, or payer remark.</summary>
    public const int MaxClaimNoteLength = 300;

    /// <summary>The most procedure codes on a claim.</summary>
    public const int MaxProcedures = 6;

    /// <summary>The most reason codes on a payer's answer.</summary>
    public const int MaxResponseCodes = 4;

    /// <summary>The shortest appeal letter.</summary>
    public const int MinAppealLength = 50;

    /// <summary>The shortest correction note or write-off reason.</summary>
    public const int MinClaimNoteLength = 10;

    private const int MaxClaims = 4000;

    /// <summary>How many days ahead an appeal deadline is when billing is reminded at sign-in.</summary>
    private const int ReminderDays = 10;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Appeals a denied claim (billing) with a letter of <see cref="MinAppealLength" /> to <see cref="MaxAppealLength" />
    /// characters, up to its appeal deadline (TooLate after it); a denied appeal can't be appealed again (WrongState).
    /// </summary>
    public HcResult AppealClaim(int claimId, string letter)
    {
        if (!IsBilling)
            return HcResult.NotAllowed;

        if (claims.FirstOrDefault(item => item.Id == claimId) is not { } claim)
            return HcResult.NotFound;

        if (claim.Status != HcClaimStatus.Denied || IsFinalDenial(claim))
            return HcResult.WrongState;

        if (claim.AppealDeadline is { } deadline && Clock.Today > deadline)
            return HcResult.TooLate;

        letter = letter?.Trim() ?? string.Empty;

        if (letter.Length is < MinAppealLength or > MaxAppealLength)
            return HcResult.Invalid;

        var previous = (claim.Status, claim.AppealLetter, claim.AppealedAt);
        (claim.Status, claim.AppealLetter, claim.AppealedAt) = (HcClaimStatus.Appealed, letter, Clock.Now);

        return Commit(() => (claim.Status, claim.AppealLetter, claim.AppealedAt) = previous,
            () => Record("Claim appealed", ClaimTarget(claim), HcSeedData.ClaimPath(claim.Id), Invariant($"Letter {letter.Length} characters, before the deadline {HcDisplayText.Day(claim.AppealDeadline ?? Clock.Today)}")));
    }

    /// <summary>Submits several drafts or corrected claims at once from the claims list (billing), each like <see cref="SubmitClaim" />.</summary>
    public HcBulkOutcome BulkSubmitClaims(IReadOnlyCollection<int> claimIds) => Bulk(claimIds, id => SubmitClaim(id) == HcResult.Ok, IsBilling);

    /// <summary>Returns the claim of a visit (staff, or the visit's patient), or <see langword="null" />.</summary>
    public HcClaim? ClaimOfVisit(int appointmentId) =>
        claims.FirstOrDefault(item => item.AppointmentId == appointmentId) is { } claim && CanSeePatient(claim.PatientId) ? claim : null;

    /// <summary>
    /// Closes a denied claim (billing) with a reason of <see cref="MinClaimNoteLength" /> to <see cref="MaxClaimNoteLength" />
    /// characters: written off (the patient owes nothing) or billed to the patient (they owe the whole amount). The patient
    /// hears about their bill.
    /// </summary>
    public HcResult CloseClaim(int claimId, bool billPatient, string reason)
    {
        if (!IsBilling)
            return HcResult.NotAllowed;

        if (claims.FirstOrDefault(item => item.Id == claimId) is not { } claim)
            return HcResult.NotFound;

        if (claim.Status != HcClaimStatus.Denied)
            return HcResult.WrongState;

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length is < MinClaimNoteLength or > MaxClaimNoteLength)
            return HcResult.Invalid;

        var bill = VisitBill(claim);
        var previous = (claim.Status, claim.ClosedAt, claim.WriteOffReason, claim.BilledToPatient, claim.AppealDeadline);
        var previousBill = bill is null ? default : (bill.AmountCents, bill.Status, bill.DueDate);
        (claim.Status, claim.ClosedAt, claim.WriteOffReason, claim.BilledToPatient, claim.AppealDeadline) = (HcClaimStatus.WrittenOff, Clock.Now, reason, billPatient, null);

        // The patient gets 30 days from the day the clinic bills them.
        if (bill is not null && bill.Status != HcBillStatus.Paid)
            (bill.AmountCents, bill.Status, bill.DueDate) = billPatient ? (claim.BilledCents, HcBillStatus.Due, Clock.Today.AddDays(30)) : (0, HcBillStatus.Covered, bill.DueDate);

        return Commit(() =>
        {
            (claim.Status, claim.ClosedAt, claim.WriteOffReason, claim.BilledToPatient, claim.AppealDeadline) = previous;

            if (bill is not null)
                (bill.AmountCents, bill.Status, bill.DueDate) = previousBill;
        }, () =>
        {
            Record(billPatient ? "Billed to the patient" : "Claim written off", ClaimTarget(claim), HcSeedData.ClaimPath(claim.Id),
                Invariant($"{HcDisplayText.Money(claim.BilledCents)}; reason given ({reason.Length} characters)"));

            if (bill is not null)
                RecordBill(bill, previousBill.AmountCents);

            Notify(PatientAccount(claim.PatientId), billPatient ? $"New bill: {HcDisplayText.Money(claim.BilledCents)}" : "Nothing to pay for your visit",
                billPatient ? $"{PayerName(claim)} didn't cover your visit of {VisitDay(claim)}, so the clinic billed you." : $"{PayerName(claim)} didn't cover your visit of {VisitDay(claim)}; the clinic wrote it off.",
                SiteBase + "/bills");
        });
    }

    /// <summary>
    /// Returns why a set of codes can't be billed, for the diagnoses and for the procedures, or <see langword="null" /> for
    /// each that's fine: one to <see cref="MaxDiagnoses" /> known diagnosis codes of the right form, and up to
    /// <see cref="MaxProcedures" /> known procedure codes with exactly one office visit code, a new-patient one for a new
    /// patient visit and an established-patient one otherwise (preventive check-ups fit either).
    /// </summary>
    public static (string? Diagnoses, string? Procedures) CodingProblem(IReadOnlyCollection<string>? diagnoses, IReadOnlyCollection<string>? procedures, HcVisitType visitType)
    {
        diagnoses ??= Array.Empty<string>();
        procedures ??= Array.Empty<string>();
        var knownDiagnoses = HcSeedData.DiagnosisCodes().Select(code => code.Code).ToHashSet(StringComparer.Ordinal);
        var knownProcedures = HcSeedData.ProcedureCodes().ToDictionary(code => code.Code);
        string? diagnosisProblem = diagnoses.Count == 0 ? "Choose at least one diagnosis code."
            : diagnoses.Count > MaxDiagnoses ? $"Use up to {MaxDiagnoses} diagnosis codes."
            : diagnoses.FirstOrDefault(code => !DiagnosisPattern().IsMatch(code ?? string.Empty)) is { } badForm ? $"{badForm} isn't a diagnosis code: they look like E11.9 or I10."
            : diagnoses.FirstOrDefault(code => !knownDiagnoses.Contains(code)) is { } unknown ? $"{unknown} isn't on the clinics' list of diagnosis codes."
            : diagnoses.Distinct().Count() != diagnoses.Count ? "Each diagnosis code can be on a claim once."
            : null;

        var visits = procedures.Where(code => knownProcedures.TryGetValue(code, out var known) && known.IsVisit).Select(code => knownProcedures[code]).ToList();
        string? procedureProblem = procedures.Count > MaxProcedures ? $"Use up to {MaxProcedures} procedure codes."
            : procedures.FirstOrDefault(code => !ProcedurePattern().IsMatch(code ?? string.Empty)) is { } badCode ? $"{badCode} isn't a procedure code: they are five digits, like 99213."
            : procedures.FirstOrDefault(code => !knownProcedures.ContainsKey(code)) is { } unknownCode ? $"{unknownCode} isn't on the clinics' list of procedure codes."
            : procedures.Distinct().Count() != procedures.Count ? "Each procedure code can be on a claim once."
            : visits.Count == 0 ? "Choose the office visit code: every claim has exactly one."
            : visits.Count > 1 ? "Choose only one office visit code."
            : visitType == HcVisitType.NewPatient && visits[0].ForNewPatient == false ? $"This was a new patient visit: use a new-patient visit code (99202 to 99204) or a preventive check-up, not {visits[0].Code}."
            : visitType == HcVisitType.Standard && visits[0].ForNewPatient == true ? $"This patient was seen before: use an established-patient visit code (99212 to 99214) or a preventive check-up, not {visits[0].Code}."
            : null;

        return (diagnosisProblem, procedureProblem);
    }

    /// <summary>
    /// Corrects a denied (or already corrected) claim (billing): new codes and the member ID from the patient's card, with a
    /// note of <see cref="MinClaimNoteLength" /> to <see cref="MaxClaimNoteLength" /> characters. A denied claim gets a new
    /// version and is Corrected, ready to resubmit; a corrected one keeps its version and takes the new values. The visit's
    /// codes and, when it changed, the patient's member ID follow. NoChange when nothing differs.
    /// </summary>
    public HcResult CorrectClaim(int claimId, IReadOnlyList<string> diagnoses, IReadOnlyList<string> procedures, string memberId, string note)
    {
        if (!IsBilling)
            return HcResult.NotAllowed;

        if (claims.FirstOrDefault(item => item.Id == claimId) is not { } claim || appointments.FirstOrDefault(item => item.Id == claim.AppointmentId) is not { } visit)
            return HcResult.NotFound;

        return Correct(claim, visit, diagnoses, procedures, memberId, note);
    }

    /// <summary>Finds a claim the signed-in account may see (billing and doctors), or returns <see langword="null" />.</summary>
    public HcClaim? FindClaim(int claimId) => IsBilling || Doctor() is not null ? claims.FirstOrDefault(item => item.Id == claimId) : null;

    /// <summary>Returns whether a claim's denial answered an appeal, so it can only be closed.</summary>
    public static bool IsFinalDenial(HcClaim claim) => claim.Status == HcClaimStatus.Denied && claim.Responses.LastOrDefault()?.AfterAppeal == true;

    /// <summary>
    /// Returns the plain-language state of the claim behind one of the signed-in patient's bills, like "Sent to Evergreen
    /// Mutual on Nov 17; nothing to pay until it answers.", or <see langword="null" /> for a bill without a claim.
    /// </summary>
    public string? PatientClaimText(HcBill bill)
    {
        if (bill.AppointmentId is not { } visitId || claims.FirstOrDefault(item => item.AppointmentId == visitId) is not { } claim || !CanSeePatient(claim.PatientId))
            return null;

        var payer = PayerName(claim);
        var answer = claim.Responses.LastOrDefault();

        return claim.Status switch
        {
            HcClaimStatus.Draft => $"Being prepared for {payer}; nothing to pay until it answers.",
            HcClaimStatus.Submitted => $"Sent to {payer} on {HcDisplayText.Date(claim.SubmittedAt ?? claim.CreatedAt)}; nothing to pay until it answers.",
            HcClaimStatus.Denied or HcClaimStatus.Corrected or HcClaimStatus.Appealed => $"{payer} hasn't paid yet; the clinic is following it up. Nothing to pay for now.",
            HcClaimStatus.WrittenOff when claim.BilledToPatient => $"{payer} didn't cover it, so the clinic billed you the whole amount.",
            HcClaimStatus.WrittenOff => $"{payer} didn't cover it; the clinic wrote it off, so there's nothing to pay.",
            _ when answer is not null => answer.PatientCents == 0
                ? $"{payer} paid {HcDisplayText.Money(answer.PaidCents)} on {HcDisplayText.Date(answer.At)}; nothing left to pay."
                : $"{payer} paid {HcDisplayText.Money(answer.PaidCents)} on {HcDisplayText.Date(answer.At)}; you owe {HcDisplayText.Money(answer.PatientCents)} after insurance.",
            _ => null,
        };
    }

    /// <summary>
    /// Records a plan's answer to a submitted or appealed claim (billing), checked by <see cref="ResponseProblem" />. Paid or
    /// partly paid closes the claim and turns the patient's bill into their share (nothing to pay when it's 0); denied sets
    /// an appeal deadline (none after an appeal: the denial is final). The patient hears about their bill, and the visit's
    /// doctor about a denial that questions the codes or the medical need.
    /// </summary>
    public HcResult RecordClaimResponse(int claimId, HcClaimOutcome outcome, int allowedCents, int paidCents, int patientCents, IReadOnlyList<string> codes, string remark)
    {
        if (!IsBilling)
            return HcResult.NotAllowed;

        if (claims.FirstOrDefault(item => item.Id == claimId) is not { } claim)
            return HcResult.NotFound;

        if (claim.Status is not (HcClaimStatus.Submitted or HcClaimStatus.Appealed))
            return HcResult.WrongState;

        remark = remark?.Trim() ?? string.Empty;
        codes ??= Array.Empty<string>();

        if (!Enum.IsDefined(outcome) || ResponseProblem(outcome, claim.BilledCents, allowedCents, paidCents, patientCents, codes) is not null || remark.Length > MaxClaimNoteLength)
            return HcResult.Invalid;

        var afterAppeal = claim.Status == HcClaimStatus.Appealed;
        var response = new HcClaimResponse
        {
            Version = claim.Versions.Count, At = Clock.Now, RecordedBy = Identity.Current?.Name ?? HcSeedData.SystemActor, Outcome = outcome, AllowedCents = allowedCents, PaidCents = paidCents,
            PatientCents = patientCents, DenialCodes = codes.ToList(), Remark = remark, AfterAppeal = afterAppeal,
        };
        var bill = VisitBill(claim);
        var previous = (claim.Status, claim.ClosedAt, claim.AppealDeadline);
        var previousBill = bill is null ? default : (bill.AmountCents, bill.Status, bill.DueDate);
        claim.Responses.Add(response);

        if (outcome == HcClaimOutcome.Denied)
        {
            (claim.Status, claim.AppealDeadline) = (HcClaimStatus.Denied, afterAppeal ? null : Clock.Today.AddDays(HcSeedData.AppealDays));
        }
        else
        {
            (claim.Status, claim.ClosedAt, claim.AppealDeadline) = (outcome == HcClaimOutcome.Paid ? HcClaimStatus.Paid : HcClaimStatus.PartlyPaid, Clock.Now, null);

            // The patient's share is due 30 days from the plan's answer (previous bill state kept for the undo).
            if (bill is not null && bill.Status != HcBillStatus.Paid)
                (bill.AmountCents, bill.Status, bill.DueDate) = patientCents == 0 ? (0, HcBillStatus.Covered, bill.DueDate) : (patientCents, HcBillStatus.Due, Clock.Today.AddDays(30));
        }

        return Commit(() =>
        {
            claim.Responses.Remove(response);
            (claim.Status, claim.ClosedAt, claim.AppealDeadline) = previous;

            if (bill is not null)
                (bill.AmountCents, bill.Status, bill.DueDate) = previousBill;
        }, () =>
        {
            var codeText = codes.Count == 0 ? null : $" ({string.Join(", ", codes)})";
            Record("Payer response recorded", ClaimTarget(claim), HcSeedData.ClaimPath(claim.Id), outcome switch
            {
                HcClaimOutcome.Denied => $"{(afterAppeal ? "Appeal denied" : "Denied")}{codeText}",
                _ => $"{(outcome == HcClaimOutcome.Paid ? "Paid" : "Partly paid")} {HcDisplayText.Money(paidCents)} of {HcDisplayText.Money(allowedCents)} allowed; patient owes {HcDisplayText.Money(patientCents)}{codeText}",
            });

            if (outcome == HcClaimOutcome.Denied)
            {
                var denials = HcSeedData.DenialCodes().Where(code => codes.Contains(code.Code)).ToList();

                if (!afterAppeal && denials.Any(code => code.NeedsDoctor) && appointments.FirstOrDefault(item => item.Id == claim.AppointmentId) is { } visit)
                {
                    NotifyDoctor(FindDoctor(visit.DoctorId), $"Claim denied: check the codes of {PatientName(claim.PatientId)}'s visit",
                        $"{PayerName(claim)}: {string.Join("; ", denials.Where(code => code.NeedsDoctor).Select(code => $"{code.Code} {code.Description}"))}. Correct the codes on the visit page.",
                        StaffAppointmentPath(visit.Id));
                }

                Notify(PatientAccount(claim.PatientId), "Your plan hasn't paid for a visit yet", $"{PayerName(claim)} asked questions about your visit of {VisitDay(claim)}. The clinic is following it up; nothing to pay for now.", SiteBase + "/bills");

                return;
            }

            if (bill is not null)
                RecordBill(bill, previousBill.AmountCents);

            Notify(PatientAccount(claim.PatientId), patientCents == 0 ? "Your plan covered your visit" : $"Bill updated after insurance: you owe {HcDisplayText.Money(patientCents)}",
                $"{PayerName(claim)} paid {HcDisplayText.Money(paidCents)} for your visit of {VisitDay(claim)}.", SiteBase + "/bills");
        });
    }

    /// <summary>
    /// Returns why a plan's answer can't be recorded, or <see langword="null" />: paid and partly paid need an allowed amount
    /// from 1 up to the billed amount, and the plan's payment plus the patient's share equal to it; partly paid also needs a
    /// code for the part not paid; denied has no amounts and needs a denial code (a CO code other than CO-45). Codes are
    /// known, used once, and at most <see cref="MaxResponseCodes" />.
    /// </summary>
    public static string? ResponseProblem(HcClaimOutcome outcome, int billedCents, int allowedCents, int paidCents, int patientCents, IReadOnlyCollection<string> codes)
    {
        var known = HcSeedData.DenialCodes().Select(code => code.Code).ToHashSet(StringComparer.Ordinal);

        if (codes.Count > MaxResponseCodes)
            return $"Use up to {MaxResponseCodes} reason codes.";

        if (codes.FirstOrDefault(code => !known.Contains(code)) is { } unknown)
            return $"{unknown} isn't on the list of reason codes.";

        if (codes.Distinct().Count() != codes.Count)
            return "Each reason code can be used once.";

        if (allowedCents < 0 || paidCents < 0 || patientCents < 0)
            return "Amounts can't be negative.";

        var denial = codes.Any(code => code.StartsWith("CO-", StringComparison.Ordinal) && code != "CO-45");

        return outcome switch
        {
            HcClaimOutcome.Denied when allowedCents != 0 || paidCents != 0 || patientCents != 0 => "A denial pays nothing: leave the amounts at 0, or record it as partly paid.",
            HcClaimOutcome.Denied when !denial => "Add the plan's denial code (a CO code, like CO-16).",
            HcClaimOutcome.Denied => null,
            _ when allowedCents < 1 => "Enter the amount the plan allows.",
            _ when allowedCents > billedCents => $"The plan can't allow more than was billed ({HcDisplayText.Money(billedCents)}).",
            _ when paidCents > allowedCents => "The plan can't pay more than it allows.",
            _ when paidCents + patientCents != allowedCents => $"The plan's payment and the patient's share must add up to the allowed amount ({HcDisplayText.Money(allowedCents)}).",
            HcClaimOutcome.PartlyPaid when codes.Count == 0 => "Add the code the plan gave for the part it didn't pay.",
            HcClaimOutcome.Paid when denial => "A denial code means part wasn't paid: record it as partly paid.",
            _ => null,
        };
    }

    /// <summary>
    /// Codes a completed visit (its doctor, or billing): checked by <see cref="CodingProblem" />. For a patient with a health
    /// plan the first coding creates a draft claim (billing hears about it) and sends the patient's bill to the plan; a draft
    /// or corrected claim takes the new codes; a denied claim is corrected, which needs a note of
    /// <see cref="MinClaimNoteLength" /> to <see cref="MaxClaimNoteLength" /> characters. A claim that is with the plan or
    /// closed can't change (WrongState).
    /// </summary>
    public HcResult SaveVisitCoding(int appointmentId, IReadOnlyList<string> diagnoses, IReadOnlyList<string> procedures, string? note, out HcClaim? claim)
    {
        claim = null;
        var doctor = Doctor();

        if (doctor is null && !IsBilling)
            return HcResult.NotAllowed;

        if (appointments.FirstOrDefault(item => item.Id == appointmentId && (doctor is null || item.DoctorId == doctor.Id)) is not { } visit)
            return HcResult.NotFound;

        if (visit.Status != HcAppointmentStatus.Completed)
            return HcResult.WrongState;

        diagnoses = (diagnoses ?? Array.Empty<string>()).Select(code => code?.Trim().ToUpperInvariant() ?? string.Empty).ToList();
        procedures = (procedures ?? Array.Empty<string>()).Select(code => code?.Trim() ?? string.Empty).ToList();

        if (CodingProblem(diagnoses, procedures, visit.VisitType) is not (null, null))
            return HcResult.Invalid;

        var existing = claims.FirstOrDefault(item => item.AppointmentId == visit.Id);
        claim = existing;

        if (existing is not null)
        {
            if (existing.Status is not (HcClaimStatus.Draft or HcClaimStatus.Denied or HcClaimStatus.Corrected) || IsFinalDenial(existing))
                return HcResult.WrongState;

            return Correct(existing, visit, diagnoses, procedures, existing.MemberId, note ?? string.Empty);
        }

        if (visit.DiagnosisCodes.SequenceEqual(diagnoses) && visit.ProcedureCodes.SequenceEqual(procedures))
            return HcResult.NoChange;

        var previous = (visit.DiagnosisCodes, visit.ProcedureCodes);
        (visit.DiagnosisCodes, visit.ProcedureCodes) = (diagnoses.ToList(), procedures.ToList());
        var patient = patients.FirstOrDefault(item => item.Id == visit.PatientId);
        HcClaim? created = null;
        var bill = bills.FirstOrDefault(item => item.AppointmentId == visit.Id);
        var previousBill = bill is null ? default : (bill.AmountCents, bill.Status);

        if (patient is not null && FindPayer(patient.InsurancePayerId) is { } payer && claims.Count < MaxClaims && nextClaimId < NextIdLimit)
        {
            var id = nextClaimId++;
            var billed = BilledCents(procedures);
            created = new HcClaim
            {
                Id = id, Number = HcSeedData.ClaimNumber(Clock.Today.Year, id), PatientId = patient.Id, AppointmentId = visit.Id, PayerId = payer.Id, MemberId = patient.InsuranceMemberId,
                CreatedAt = Clock.Now, DiagnosisCodes = diagnoses.ToList(), ProcedureCodes = procedures.ToList(), BilledCents = billed, Status = HcClaimStatus.Draft,
                Versions = { new HcClaimVersion { Number = 1, At = Clock.Now, By = Identity.Current?.Name ?? HcSeedData.SystemActor, DiagnosisCodes = diagnoses.ToList(), ProcedureCodes = procedures.ToList(), MemberId = patient.InsuranceMemberId } },
            };
            claims.Add(created);

            if (bill is not null && bill.Status == HcBillStatus.Due)
                (bill.AmountCents, bill.Status) = (billed, HcBillStatus.Pending);
            else if (bill is not null && bill.Status == HcBillStatus.Pending)
                bill.AmountCents = billed;
        }

        var result = Commit(() =>
        {
            (visit.DiagnosisCodes, visit.ProcedureCodes) = previous;

            if (created is not null)
            {
                claims.Remove(created);
                nextClaimId--;
            }

            if (bill is not null)
                (bill.AmountCents, bill.Status) = previousBill;
        }, () =>
        {
            Record(previous.DiagnosisCodes.Count == 0 ? "Visit coded" : "Codes changed", AppointmentTarget(visit), StaffAppointmentPath(visit.Id),
                $"Diagnoses {string.Join(", ", diagnoses)}; procedures {string.Join(", ", procedures)}");

            if (created is not null)
            {
                Record("Claim created", ClaimTarget(created), HcSeedData.ClaimPath(created.Id), $"{PayerName(created)}, billed {HcDisplayText.Money(created.BilledCents)}");
                NotifyBilling($"Claim ready to submit: {created.Number}", $"{Identity.Current?.Name} coded {patient?.Name}'s visit; {HcDisplayText.Money(created.BilledCents)} to {PayerName(created)}.",
                    HcSeedData.ClaimPath(created.Id));
            }
        });

        if (result == HcResult.Ok)
            claim = created;

        return result;
    }

    /// <summary>
    /// Submits a draft or corrected claim to the plan (billing): its codes must still be valid, the member ID of the plan's
    /// form, and the visit within <see cref="HcSeedData.FilingDays" /> days (TooLate after it: the plans' timely-filing rule).
    /// </summary>
    public HcResult SubmitClaim(int claimId)
    {
        if (!IsBilling)
            return HcResult.NotAllowed;

        if (claims.FirstOrDefault(item => item.Id == claimId) is not { } claim || appointments.FirstOrDefault(item => item.Id == claim.AppointmentId) is not { } visit)
            return HcResult.NotFound;

        if (claim.Status is not (HcClaimStatus.Draft or HcClaimStatus.Corrected))
            return HcResult.WrongState;

        if (Clock.Today > FilingDeadline(claim))
            return HcResult.TooLate;

        if (CodingProblem(claim.DiagnosisCodes, claim.ProcedureCodes, visit.VisitType) is not (null, null) || MemberIdProblem(claim.PayerId, claim.MemberId) is not null)
            return HcResult.Invalid;

        var resubmit = claim.Status == HcClaimStatus.Corrected;
        var previous = (claim.Status, claim.SubmittedAt);
        (claim.Status, claim.SubmittedAt) = (HcClaimStatus.Submitted, Clock.Now);

        return Commit(() => (claim.Status, claim.SubmittedAt) = previous,
            () => Record(resubmit ? "Claim resubmitted" : "Claim submitted", ClaimTarget(claim), HcSeedData.ClaimPath(claim.Id),
                Invariant($"Version {claim.Versions.Count} to {PayerName(claim)}, {HcDisplayText.Money(claim.BilledCents)}")));
    }

    /// <summary>Returns the last day a claim can be submitted: <see cref="HcSeedData.FilingDays" /> days after its visit.</summary>
    public DateTime FilingDeadline(HcClaim claim) =>
        (appointments.FirstOrDefault(item => item.Id == claim.AppointmentId)?.Start.Date ?? claim.CreatedAt.Date).AddDays(HcSeedData.FilingDays);

    #endregion

    #region Private Methods

    private static int BilledCents(IEnumerable<string> procedures)
    {
        var fees = HcSeedData.ProcedureCodes().ToDictionary(code => code.Code, code => code.FeeCents);

        return procedures.Sum(code => fees.GetValueOrDefault(code));
    }

    [GeneratedRegex(@"^[A-Z][0-9][0-9A-Z](\.[0-9A-Z]{1,4})?$")]
    private static partial Regex DiagnosisPattern();

    [GeneratedRegex("^[0-9]{5}$")]
    private static partial Regex ProcedurePattern();

    /// <summary>A claim as the audit log names it: its number and patient.</summary>
    private string ClaimTarget(HcClaim claim) => $"Claim {claim.Number} · {PatientName(claim.PatientId)}";

    /// <summary>Changes a claim's codes and member ID: a new version after a denial, or the current one for a draft or corrected claim.</summary>
    private HcResult Correct(HcClaim claim, HcAppointment visit, IReadOnlyList<string> diagnoses, IReadOnlyList<string> procedures, string memberId, string note)
    {
        if (claim.Status is not (HcClaimStatus.Draft or HcClaimStatus.Denied or HcClaimStatus.Corrected) || IsFinalDenial(claim))
            return HcResult.WrongState;

        diagnoses = diagnoses.Select(code => code?.Trim().ToUpperInvariant() ?? string.Empty).ToList();
        procedures = procedures.Select(code => code?.Trim() ?? string.Empty).ToList();
        memberId = memberId?.Trim().ToUpperInvariant() ?? string.Empty;
        note = note?.Trim() ?? string.Empty;
        var newVersion = claim.Status == HcClaimStatus.Denied;

        if (CodingProblem(diagnoses, procedures, visit.VisitType) is not (null, null) || MemberIdProblem(claim.PayerId, memberId) is not null || note.Length > MaxClaimNoteLength
            || (newVersion && note.Length < MinClaimNoteLength))
            return HcResult.Invalid;

        if (claim.DiagnosisCodes.SequenceEqual(diagnoses) && claim.ProcedureCodes.SequenceEqual(procedures) && claim.MemberId == memberId)
            return HcResult.NoChange;

        var before = $"{string.Join(", ", claim.DiagnosisCodes)} / {string.Join(", ", claim.ProcedureCodes)}{(claim.MemberId == memberId ? null : ", member ID")}";
        var snapshot = (claim.DiagnosisCodes, claim.ProcedureCodes, claim.MemberId, claim.BilledCents, claim.Status, claim.AppealDeadline, claim.Versions.ToList(), visit.DiagnosisCodes, visit.ProcedureCodes);
        var patient = patients.FirstOrDefault(item => item.Id == claim.PatientId);
        var previousMember = patient?.InsuranceMemberId;
        var bill = VisitBill(claim);
        var previousBillAmount = bill?.AmountCents;
        var version = new HcClaimVersion
        {
            Number = newVersion ? claim.Versions.Count + 1 : Math.Max(1, claim.Versions.Count), At = Clock.Now, By = Identity.Current?.Name ?? HcSeedData.SystemActor,
            DiagnosisCodes = diagnoses.ToList(), ProcedureCodes = procedures.ToList(), MemberId = memberId, Note = newVersion || claim.Status == HcClaimStatus.Corrected ? note : string.Empty,
        };

        if (newVersion || claim.Versions.Count == 0)
            claim.Versions.Add(version);
        else
            claim.Versions[^1] = version;

        (claim.DiagnosisCodes, claim.ProcedureCodes, claim.MemberId, claim.BilledCents) = (diagnoses.ToList(), procedures.ToList(), memberId, BilledCents(procedures));
        (visit.DiagnosisCodes, visit.ProcedureCodes) = (diagnoses.ToList(), procedures.ToList());

        if (newVersion)
            (claim.Status, claim.AppealDeadline) = (HcClaimStatus.Corrected, null);

        if (patient is not null && patient.InsurancePayerId == claim.PayerId && patient.InsuranceMemberId != memberId)
            patient.InsuranceMemberId = memberId;

        if (bill is not null && bill.Status == HcBillStatus.Pending)
            bill.AmountCents = claim.BilledCents;

        var fromDoctor = Doctor() is not null;

        return Commit(() =>
        {
            (claim.DiagnosisCodes, claim.ProcedureCodes, claim.MemberId, claim.BilledCents, claim.Status, claim.AppealDeadline, _, visit.DiagnosisCodes, visit.ProcedureCodes) = snapshot;
            claim.Versions = snapshot.Item7;

            if (patient is not null && previousMember is not null)
                patient.InsuranceMemberId = previousMember;

            if (bill is not null && previousBillAmount is { } amount)
                bill.AmountCents = amount;
        }, () =>
        {
            var after = $"{string.Join(", ", diagnoses)} / {string.Join(", ", procedures)}";
            Record(newVersion || claim.Status == HcClaimStatus.Corrected ? "Claim corrected" : "Codes changed", ClaimTarget(claim), HcSeedData.ClaimPath(claim.Id),
                Invariant($"Version {claim.Versions.Count}: {before} → {after}{(note.Length > 0 ? $"; note {note.Length} characters" : null)}"));

            if (patient is not null && previousMember != patient.InsuranceMemberId)
                Record("Health plan changed", PatientTarget(patient), PatientDetailsPath(patient.Id), "Member ID corrected from a claim");

            if (fromDoctor && claim.Status == HcClaimStatus.Corrected)
                NotifyBilling($"Claim corrected: {claim.Number}", $"{Identity.Current?.Name} changed the codes of {PatientName(claim.PatientId)}'s visit. Resubmit it to {PayerName(claim)}.", HcSeedData.ClaimPath(claim.Id));
        });
    }

    /// <summary>Tells every billing account, except the one who caused it.</summary>
    private void NotifyBilling(string title, string text, string? link)
    {
        foreach (var account in Identity.Accounts.Where(account => account.RoleId == HcSeedData.BillingRole).ToList())
            Notify(account.Id, title, text, link);
    }

    private string? PatientAccount(int patientId) => patients.FirstOrDefault(item => item.Id == patientId)?.AccountId;

    private string PatientName(int patientId) => patients.FirstOrDefault(item => item.Id == patientId)?.Name ?? "a former patient";

    private string PayerName(HcClaim claim) => FindPayer(claim.PayerId)?.Name ?? "the health plan";

    /// <summary>Audits a patient's bill that a claim changed.</summary>
    private void RecordBill(HcBill bill, int before) =>
        Record("Bill updated after insurance", BillTarget(bill), PatientDetailsPath(bill.PatientId),
            $"{HcDisplayText.Money(before)} → {HcDisplayText.Money(bill.AmountCents)}, {(bill.Status == HcBillStatus.Covered ? "nothing to pay" : bill.Status == HcBillStatus.Due ? "to pay" : bill.Status.ToString().ToLowerInvariant())}");

    /// <summary>
    /// Reminds the signed-in billing specialist of denied claims whose appeal deadline is within <see cref="ReminderDays" />
    /// days, and of claims not sent within ten days of their filing deadline (a few at a time, once per claim and kind).
    /// </summary>
    private void RemindClaims()
    {
        if (!IsBilling || Identity.Current is not { } account)
            return;

        var reminded = 0;
        var today = Clock.Today;

        foreach (var claim in claims.Where(item => item.Status == HcClaimStatus.Denied && !IsFinalDenial(item) && item.AppealDeadline is { } deadline && deadline >= today && deadline <= today.AddDays(ReminderDays))
                     .OrderBy(item => item.AppealDeadline))
        {
            var title = $"Appeal deadline {HcSeedData.DaysAway((claim.AppealDeadline!.Value - today).Days)}: {claim.Number}";
            var url = HcSeedData.ClaimPath(claim.Id);

            if (notifications.Any(notice => notice.AccountId == account.Id && notice.Url == url && notice.Title.StartsWith("Appeal deadline", StringComparison.Ordinal)))
                continue;

            Notify(account.Id, title, $"{PayerName(claim)} denied it. Appeal or close it by {HcDisplayText.Day(claim.AppealDeadline.Value)}.", url, fromSystem: true);

            if (++reminded >= MaxRemindersPerSignIn)
                return;
        }

        foreach (var claim in claims.Where(item => item.Status is HcClaimStatus.Draft or HcClaimStatus.Corrected && FilingDeadline(item) <= today.AddDays(ReminderDays) && FilingDeadline(item) >= today))
        {
            var url = HcSeedData.ClaimPath(claim.Id);

            if (notifications.Any(notice => notice.AccountId == account.Id && notice.Url == url && notice.Title.StartsWith("Filing deadline", StringComparison.Ordinal)))
                continue;

            Notify(account.Id, $"Filing deadline near: {claim.Number}", $"Submit it to {PayerName(claim)} by {HcDisplayText.Day(FilingDeadline(claim))}, or it can't be paid.", url, fromSystem: true);

            if (++reminded >= MaxRemindersPerSignIn)
                return;
        }
    }

    private string VisitDay(HcClaim claim) => appointments.FirstOrDefault(item => item.Id == claim.AppointmentId) is { } visit ? HcDisplayText.Date(visit.Start) : HcDisplayText.Date(claim.CreatedAt);

    /// <summary>The patient's bill for a claim's visit, or <see langword="null" />.</summary>
    private HcBill? VisitBill(HcClaim claim) => bills.FirstOrDefault(item => item.AppointmentId == claim.AppointmentId && item.PatientId == claim.PatientId);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the claims (billing and doctors), newest first.</summary>
    public IReadOnlyList<HcClaim> Claims => IsBilling || Doctor() is not null ? claims.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id).ToList() : Array.Empty<HcClaim>();

    /// <summary>Gets the reason and adjustment codes plans answer with.</summary>
    public IReadOnlyList<HcDenialCode> DenialCodes { get; } = HcSeedData.DenialCodes();

    /// <summary>Gets the diagnosis codes the clinics bill with.</summary>
    public IReadOnlyList<HcDiagnosisCode> DiagnosisCodes { get; } = HcSeedData.DiagnosisCodes();

    /// <summary>Gets whether the signed-in account is the billing team.</summary>
    public bool IsBilling => Identity.Current?.RoleId == HcSeedData.BillingRole;

    /// <summary>Gets the procedure codes the clinics bill with.</summary>
    public IReadOnlyList<HcProcedureCode> ProcedureCodes { get; } = HcSeedData.ProcedureCodes();

    /// <summary>Gets the completed visits of patients with a health plan that aren't coded yet (staff), newest first.</summary>
    public IReadOnlyList<HcAppointment> VisitsToCode
    {
        get
        {
            if (!IsStaff)
                return Array.Empty<HcAppointment>();

            var claimed = claims.Select(item => item.AppointmentId).ToHashSet();
            var insured = patients.Where(item => item.InsurancePayerId.Length > 0).Select(item => item.Id).ToHashSet();

            return appointments.Where(item => item.Status == HcAppointmentStatus.Completed && item.ProcedureCodes.Count == 0 && insured.Contains(item.PatientId) && !claimed.Contains(item.Id))
                .OrderByDescending(item => item.Start).ToList();
        }
    }

    #endregion
}
