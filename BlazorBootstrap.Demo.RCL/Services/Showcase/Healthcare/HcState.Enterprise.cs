using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>
/// The enterprise baseline of <see cref="HcState" /> (rule WR-001): the audit log (every change, and every look at a
/// patient's chart, with health information kept out: lengths and field names only), the header's notifications (raised
/// where someone must act or hear back: the doctor about a booking, a patient checked in, a refill request, results ready to
/// review; the front desk about bookings at their clinic; the patient about a confirmed, moved, or cancelled appointment, a
/// refill answer, released results, and new bills), the header's global search for each role, and the lists' bulk actions
/// (check in, no-show, refill answers, releasing results). Audit entries and notifications link only to pages on this site.
/// </summary>
public sealed partial class HcState
{
    #region Fields and Constants

    /// <summary>The longest notification text.</summary>
    private const int MaxNoticeTextLength = 200;

    /// <summary>The longest notification title.</summary>
    private const int MaxNoticeTitleLength = 80;

    /// <summary>The most reminders one sign-in raises about results waiting to be released.</summary>
    private const int MaxRemindersPerSignIn = 5;

    /// <summary>The date forms the search understands for a date of birth.</summary>
    private static readonly string[] birthDateFormats = { "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy", "MMM d, yyyy", "MMMM d, yyyy", "d MMM yyyy", "d MMMM yyyy" };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Checks in several of today's booked patients at once from the check-in list (front desk), each like
    /// <see cref="CheckIn" />: each doctor hears about their patient. Appointments already checked in, closed, or not today are
    /// left as they are.
    /// </summary>
    public HcBulkOutcome BulkCheckIn(IReadOnlyCollection<int> appointmentIds) =>
        Bulk(appointmentIds, id => CheckIn(id) == HcResult.Ok, IsFrontDesk);

    /// <summary>
    /// Answers several refill requests at once from the refill list (doctors), each like <see cref="DecideRefill" />: approve
    /// with one optional note, or deny with one reason the patients read (<see cref="MinDenyReasonLength" /> to
    /// <see cref="MaxReasonLength" /> characters). Requests answered meanwhile are left as they are.
    /// </summary>
    public HcBulkOutcome BulkDecideRefills(IReadOnlyCollection<int> prescriptionIds, bool approve, string? note)
    {
        var text = note?.Trim() ?? string.Empty;

        if (text.Length > MaxReasonLength || (!approve && text.Length < MinDenyReasonLength))
            return new HcBulkOutcome(Doctor() is null ? HcResult.NotAllowed : HcResult.Invalid, 0, 0);

        return Bulk(prescriptionIds, id => DecideRefill(id, approve, text) == HcResult.Ok, Doctor() is not null);
    }

    /// <summary>
    /// Marks several booked patients who didn't come as no-shows at once from the check-in list (front desk), each like
    /// <see cref="MarkNoShow" />. Appointments that haven't started yet, or aren't booked any more, are left as they are.
    /// </summary>
    public HcBulkOutcome BulkMarkNoShow(IReadOnlyCollection<int> appointmentIds) =>
        Bulk(appointmentIds, id => MarkNoShow(id) == HcResult.Ok, IsFrontDesk);

    /// <summary>
    /// Releases several entered results at once from the lab list (the ordering doctor), each like <see cref="ReleaseLab" />:
    /// each test is billed, and each patient hears about both. Orders without results, or released already, are left as they are.
    /// </summary>
    public HcBulkOutcome BulkReleaseLabs(IReadOnlyCollection<int> orderIds) =>
        Bulk(orderIds, id => ReleaseLab(id) == HcResult.Ok, Doctor() is not null);

    /// <summary>Returns the signed-in account's notifications for the header's menu, newest first; empty when nobody is signed in.</summary>
    public IReadOnlyList<ShowcaseNotification> GetNotificationsForCurrentAccount() =>
        Identity.Current is not { } account
            ? Array.Empty<ShowcaseNotification>()
            : notifications.Where(notice => notice.AccountId == account.Id).OrderByDescending(notice => notice.At).ThenByDescending(notice => notice.Id).ToList();

    /// <summary>Marks every notification of the signed-in account as read (the header's "Mark all as read").</summary>
    public void MarkAllNotificationsRead()
    {
        if (Identity.Current is not { } account || notifications.All(notice => notice.AccountId != account.Id || notice.IsRead))
            return;

        foreach (var notice in notifications.Where(notice => notice.AccountId == account.Id))
            notice.IsRead = true;

        RaiseChanged();
    }

    /// <summary>Marks one notification of the signed-in account as read; ignores an unknown ID or another account's notification.</summary>
    public void MarkNotificationRead(int id)
    {
        if (Identity.Current is { } account && notifications.FirstOrDefault(notice => notice.Id == id && notice.AccountId == account.Id) is { IsRead: false } notice)
        {
            notice.IsRead = true;
            RaiseChanged();
        }
    }

    /// <summary>
    /// The header's global search, best matches first (exactly the term, then starting with it, then containing it), and
    /// among equal matches by kind and then newest first. Staff find patients by name, record number (like "CL-100001"), or
    /// date of birth (like "1988-04-12" or "Apr 12, 1988"), each opening the chart for a doctor and the registration details
    /// for the front desk, and appointments by number (like "1204" or "#1204"), each opening the visit for a doctor and that
    /// day's calendar for the front desk; a doctor also finds their lab orders by number or test. A patient finds their own
    /// appointments (by number, doctor, or reason), visit summaries, lab reports, and bills, and the clinics' doctors (by
    /// name or specialty). Returns nothing for a blank term or when nobody is signed in.
    /// </summary>
    public IReadOnlyList<ShowcaseSearchResult> Search(string term, int maxResults)
    {
        term = term?.Trim() ?? string.Empty;

        if (term.Length == 0 || maxResults <= 0 || Identity.Current is null)
            return Array.Empty<ShowcaseSearchResult>();

        var number = term.TrimStart('#').Trim();
        var hasDigit = number.Any(char.IsAsciiDigit);
        var matches = new List<(int Rank, int Kind, long Order, ShowcaseSearchResult Result)>();
        var doctorNames = Doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);
        var patientsById = patients.ToDictionary(patient => patient.Id);

        // 0: exactly the term, 1: starts with it, 2: contains it; null: no match.
        static int? Rank(string text, params string?[] values) => values
            .Select(value => value is null ? (int?)null
                : string.Equals(value, text, StringComparison.OrdinalIgnoreCase) ? 0
                : value.StartsWith(text, StringComparison.OrdinalIgnoreCase) ? 1
                : value.Contains(text, StringComparison.OrdinalIgnoreCase) ? 2
                : null)
            .Where(rank => rank is not null)
            .DefaultIfEmpty(null)
            .Min();

        if (IsStaff)
        {
            var doctor = Doctor();
            var birthDate = DateTime.TryParseExact(term, birthDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed) ? parsed.Date : (DateTime?)null;

            // Kind: 0 a patient, 1 an appointment, 2 a lab order, 3 a claim.
            foreach (var patient in patients)
            {
                var found = birthDate is { } dob && patient.DateOfBirth?.Date == dob ? 0 : Rank(term, term.Length >= 2 ? patient.Name : null, patient.Mrn, hasDigit ? patient.Mrn[3..] : null);

                if (found is { } rank)
                {
                    matches.Add((rank, 0, 0, new ShowcaseSearchResult("Patient", $"{patient.Name} · {patient.Mrn}",
                        $"{(patient.DateOfBirth is { } born ? $"Born {HcDisplayText.Date(born)}" : "Date of birth not recorded")} · {FindClinic(patient.ClinicId)?.Name ?? "No home clinic"}",
                        doctor is not null ? PatientChartPath(patient.Id) : PatientDetailsPath(patient.Id))));
                }
            }

            if (hasDigit && birthDate is null)
            {
                foreach (var appointment in appointments)
                {
                    if (Rank(number, appointment.Id.ToString(CultureInfo.InvariantCulture)) is { } found)
                    {
                        var name = patientsById.GetValueOrDefault(appointment.PatientId)?.Name ?? "A former patient";
                        matches.Add((found, 1, -appointment.Start.Ticks, new ShowcaseSearchResult("Appointment", Invariant($"Appointment {appointment.Id}"),
                            $"{name} · {doctorNames.GetValueOrDefault(appointment.DoctorId)} · {HcDisplayText.When(appointment.Start)} · {HcDisplayText.StatusWord(appointment.Status)}",
                            doctor is not null ? StaffAppointmentPath(appointment.Id)
                            : IsBilling ? claims.FirstOrDefault(item => item.AppointmentId == appointment.Id) is { } visitClaim ? HcSeedData.ClaimPath(visitClaim.Id) : PatientDetailsPath(appointment.PatientId)
                            : HcSeedData.StaffCalendarPath(appointment.Start.Date, appointment.DoctorId))));
                    }
                }
            }

            if ((IsBilling || doctor is not null) && hasDigit && birthDate is null)
            {
                foreach (var claim in claims)
                {
                    if (Rank(number, claim.Number, claim.Number[^5..], claim.Id.ToString(CultureInfo.InvariantCulture)) is { } found)
                    {
                        matches.Add((found, 3, -claim.CreatedAt.Ticks, new ShowcaseSearchResult("Claim", claim.Number,
                            $"{patientsById.GetValueOrDefault(claim.PatientId)?.Name} · {FindPayer(claim.PayerId)?.Name} · {HcDisplayText.ClaimWord(claim.Status)} · {HcDisplayText.Money(claim.BilledCents)}",
                            HcSeedData.ClaimPath(claim.Id))));
                    }
                }
            }

            if (doctor is not null)
            {
                foreach (var order in labOrders.Where(order => order.DoctorId == doctor.Id))
                {
                    var panel = LabPanel(order.PanelId)?.Name ?? order.PanelId;
                    var found = birthDate is null && hasDigit ? Rank(number, order.Id.ToString(CultureInfo.InvariantCulture)) : null;
                    found = Min(found, term.Length >= 3 ? Rank(term, panel) : null);

                    if (found is { } rank)
                    {
                        matches.Add((rank, 2, -order.OrderedAt.Ticks, new ShowcaseSearchResult("Lab order", Invariant($"Lab order {order.Id} · {panel}"),
                            $"{patientsById.GetValueOrDefault(order.PatientId)?.Name ?? "A former patient"} · ordered {HcDisplayText.Day(order.OrderedAt)} · {HcDisplayText.LabWord(order.Status)}",
                            HcSeedData.LabOrderPath(order.Id))));
                    }
                }
            }
        }
        else if (Patient() is { } patient)
        {
            // Kind: 0 an appointment, 1 a visit summary, 2 a lab report, 3 a bill, 4 a doctor.
            foreach (var appointment in appointments.Where(item => item.PatientId == patient.Id))
            {
                var doctorName = doctorNames.GetValueOrDefault(appointment.DoctorId);
                var found = hasDigit ? Rank(number, appointment.Id.ToString(CultureInfo.InvariantCulture)) : null;
                found = Min(found, term.Length >= 2 ? Rank(term, doctorName, appointment.Reason) : null);

                if (found is not { } rank)
                    continue;

                var completed = appointment.Status == HcAppointmentStatus.Completed;
                matches.Add((rank, completed ? 1 : 0, -appointment.Start.Ticks, new ShowcaseSearchResult(completed ? "Visit summary" : "Your appointment", $"{HcDisplayText.When(appointment.Start)} · {appointment.Reason}",
                    Invariant($"Appointment {appointment.Id} · {doctorName} · {HcDisplayText.StatusWord(appointment.Status)}"), HcSeedData.AppointmentPath(appointment.Id))));
            }

            foreach (var order in labOrders.Where(item => item.PatientId == patient.Id && item.Status == HcLabStatus.Released))
            {
                var panel = LabPanel(order.PanelId)?.Name ?? order.PanelId;

                if ((term.Length >= 2 ? Rank(term, panel) : null) is { } found)
                {
                    matches.Add((found, 2, -(order.ReleasedAt ?? order.OrderedAt).Ticks, new ShowcaseSearchResult("Lab report", panel,
                        $"Released {HcDisplayText.Date(order.ReleasedAt ?? order.OrderedAt)} · ordered by {doctorNames.GetValueOrDefault(order.DoctorId)}", HcSeedData.LabReportPath(order.Id))));
                }
            }

            foreach (var bill in bills.Where(item => item.PatientId == patient.Id))
            {
                var found = hasDigit ? Rank(number, bill.Id.ToString(CultureInfo.InvariantCulture)) : null;
                found = Min(found, term.Length >= 3 ? Rank(term, bill.Description) : null);

                if (found is { } rank)
                {
                    matches.Add((rank, 3, -bill.CreatedAt.Ticks, new ShowcaseSearchResult("Bill", Invariant($"Bill {bill.Id} · {HcDisplayText.Money(bill.AmountCents)}"),
                        $"{bill.Description} · {bill.Status switch { HcBillStatus.Paid => "paid", HcBillStatus.Pending => "with the health plan", HcBillStatus.Covered => "nothing to pay", _ => $"due {HcDisplayText.Date(bill.DueDate)}" }}", BillsPath(bill.Id))));
                }
            }

            foreach (var doctor in Doctors)
            {
                if ((term.Length >= 2 ? Rank(term, doctor.Name, doctor.Name.Replace("Dr. ", string.Empty, StringComparison.Ordinal), doctor.Specialty) : null) is { } found)
                {
                    matches.Add((found, 4, 0, new ShowcaseSearchResult("Doctor", doctor.Name,
                        $"{doctor.Specialty} · {FindClinic(doctor.ClinicId)?.Name}{(doctor.AcceptingNewPatients ? null : " · not taking new patients")}",
                        $"{SiteBase}/doctors/{Uri.EscapeDataString(doctor.Id)}")));
                }
            }
        }

        return matches
            .OrderBy(match => match.Rank)
            .ThenBy(match => match.Kind)
            .ThenBy(match => match.Order)
            .ThenBy(match => match.Result.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(match => match.Result)
            .ToList();
    }

    #endregion

    #region Private Methods

    /// <summary>Checks a stored audit or notification link: none, or a path on this site without spaces, quotes, or a second slash.</summary>
    private static bool IsSiteLink(string? url) =>
        url is null || (url.Length <= 200 && (url == SiteBase || url.StartsWith(SiteBase + "/", StringComparison.Ordinal)) && !url.Contains("//", StringComparison.Ordinal)
            && url.All(character => !char.IsWhiteSpace(character) && !char.IsControl(character) && character is not ('"' or '\'' or '<' or '>' or '\\')));

    private static int? Min(int? left, int? right) => left is null ? right : right is null ? left : Math.Min(left.Value, right.Value);

    /// <summary>Runs one change on several records, raising <see cref="Changed" /> once; NotAllowed without the role, Invalid for an empty list.</summary>
    private HcBulkOutcome Bulk(IReadOnlyCollection<int> ids, Func<int, bool> change, bool allowed)
    {
        if (!allowed)
            return new HcBulkOutcome(HcResult.NotAllowed, 0, 0);

        var list = (ids ?? Array.Empty<int>()).Distinct().ToList();

        if (list.Count == 0)
            return new HcBulkOutcome(HcResult.Invalid, 0, 0);

        int changed = 0, unchanged = 0;
        batchDepth++;

        try
        {
            foreach (var id in list)
            {
                if (change(id))
                    changed++;
                else
                    unchanged++;
            }
        }
        finally
        {
            batchDepth--;
        }

        if (batchDepth == 0 && changedInBatch)
        {
            changedInBatch = false;
            Changed?.Invoke();
        }

        return new HcBulkOutcome(changed == 0 ? HcResult.NoChange : HcResult.Ok, changed, unchanged);
    }

    /// <summary>
    /// Raises a notification for an account that can sign in (rule WR-001); nothing for a patient or doctor without a
    /// sign-in, and nothing for the person who caused it, unless the portal itself sends it (<paramref name="fromSystem" />,
    /// like a booking confirmation). The site keeps its newest notifications within the limit a restore accepts.
    /// </summary>
    private void Notify(string? accountId, string title, string text, string? link, bool fromSystem = false)
    {
        if (accountId is null || Identity.FindAccount(accountId) is null || (!fromSystem && accountId == Identity.Current?.Id) || nextNotificationId >= NextIdLimit)
            return;

        notifications.Add(new ShowcaseNotification
        {
            Id = nextNotificationId++,
            AccountId = accountId,
            At = Clock.Now,
            Title = Clip(title, MaxNoticeTitleLength),
            Text = string.IsNullOrWhiteSpace(text) ? null : Clip(text, MaxNoticeTextLength),
            Url = IsSiteLink(link) ? link : null,
        });

        while (notifications.Count > MaxNotifications)
            notifications.RemoveAt(0);
    }

    /// <summary>Tells a doctor who can sign in, unless they caused it.</summary>
    private void NotifyDoctor(HcDoctor? doctor, string title, string text, string? link) => Notify(doctor?.AccountId, title, text, link);

    /// <summary>Tells every front desk account that works at a clinic (their work details), except the one who caused it.</summary>
    private void NotifyFrontDesk(string clinicId, string title, string text, string? link)
    {
        foreach (var account in Identity.Accounts.Where(account => account.RoleId == HcSeedData.FrontDeskRole).ToList())
        {
            if (ProfileOf(account.Id, "downtown").ClinicId == clinicId)
                Notify(account.Id, title, text, link);
        }
    }

    /// <summary>
    /// Adds an audit entry (rule WR-001): the signed-in person did <paramref name="action" /> (past tense) to
    /// <paramref name="target" />, now, on the site's clock; the portal itself when nobody is signed in. Details name what
    /// changed, never health information (notes, results, and chart items appear as lengths and counts). The log keeps its
    /// newest entries (at most <see cref="MaxAddedAudit" /> added by the tab).
    /// </summary>
    private void Record(string action, string target, string? targetUrl, string? details = null)
    {
        if (nextAuditId >= NextIdLimit)
            return;

        var actor = Identity.Current?.Name;

        audit.Add(new ShowcaseAuditEntry
        {
            Id = nextAuditId++,
            At = Clock.Now,
            Actor = Clip(string.IsNullOrWhiteSpace(actor) ? HcSeedData.SystemActor : actor.Trim(), ShowcaseIdentity.MaxNameLength),
            Action = Clip(action, MaxAuditTextLength),
            Target = Clip(target, MaxAuditTextLength),
            TargetUrl = IsSiteLink(targetUrl) ? targetUrl : null,
            Details = string.IsNullOrWhiteSpace(details) ? null : Clip(details, MaxAuditTextLength),
        });

        // The seeded log stays; what this tab added keeps its newest entries, so the snapshot (which stores only those) stays small.
        var firstAdded = seed.Value.Audit.Count + 1;

        if (audit.Count(entry => entry.Id >= firstAdded) > MaxAddedAudit)
            audit.RemoveAt(audit.FindIndex(entry => entry.Id >= firstAdded));

        while (audit.Count > MaxAudit)
            audit.RemoveAt(0);
    }

    /// <summary>
    /// Reminds the signed-in doctor of their results entered but not released yet (a few at a time, once per order), so the
    /// bell says what needs reviewing when they sign in.
    /// </summary>
    private void RemindLabResults()
    {
        if (Doctor() is not { } doctor || doctor.AccountId is not { } accountId)
            return;

        var reminded = 0;

        foreach (var order in labOrders.Where(order => order.DoctorId == doctor.Id && order.Status == HcLabStatus.Resulted).OrderBy(order => order.ResultedAt))
        {
            var url = HcSeedData.LabOrderPath(order.Id);

            if (notifications.Any(notice => notice.AccountId == accountId && notice.Url == url))
                continue;

            Notify(accountId, "Lab results ready to review",
                $"{LabPanel(order.PanelId)?.Name} for {patients.FirstOrDefault(item => item.Id == order.PatientId)?.Name}. Release them when you've checked them.", url, fromSystem: true);

            if (++reminded >= MaxRemindersPerSignIn)
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the audit log, newest first, for staff; empty for patients (they see who viewed their record under Records instead).</summary>
    public IReadOnlyList<ShowcaseAuditEntry> AuditEntries =>
        IsStaff ? audit.OrderByDescending(entry => entry.At).ThenByDescending(entry => entry.Id).ToList() : Array.Empty<ShowcaseAuditEntry>();

    #endregion
}

/// <summary>
/// The words and formats the state writes into audit entries and notifications, in the invariant culture (the same as the
/// screens' <c>HcDisplay</c>).
/// </summary>
internal static class HcDisplayText
{
    #region Methods

    /// <summary>Formats a date like "Nov 24, 2026".</summary>
    public static string Date(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Formats a date like "Tue, Nov 24".</summary>
    public static string Day(DateTime date) => date.ToString("ddd, MMM d", CultureInfo.InvariantCulture);

    /// <summary>Returns a claim status in words for a sentence, like "partly paid".</summary>
    public static string ClaimWord(HcClaimStatus status) => status switch
    {
        HcClaimStatus.PartlyPaid => "partly paid",
        HcClaimStatus.WrittenOff => "closed",
        _ => status.ToString().ToLowerInvariant(),
    };

    /// <summary>Returns a lab status in words, like "waiting for results".</summary>
    public static string LabWord(HcLabStatus status) => status switch
    {
        HcLabStatus.Ordered => "waiting for results",
        HcLabStatus.Resulted => "entered, not released",
        _ => "released",
    };

    /// <summary>Formats minutes after midnight like "08:30".</summary>
    public static string Minutes(int minuteOfDay) => FormattableString.Invariant($"{minuteOfDay / 60:00}:{minuteOfDay % 60:00}");

    /// <summary>Formats cents like "$40.00".</summary>
    public static string Money(int cents) => (cents / 100m).ToString("$#,0.00", CultureInfo.InvariantCulture);

    /// <summary>Returns an appointment status in words for a sentence, like "checked in".</summary>
    public static string StatusWord(HcAppointmentStatus status) => status switch
    {
        HcAppointmentStatus.CheckedIn => "checked in",
        HcAppointmentStatus.InVisit => "with the doctor",
        HcAppointmentStatus.NoShow => "no-show",
        _ => status.ToString().ToLowerInvariant(),
    };

    /// <summary>Formats a time like "09:30".</summary>
    public static string Time(DateTime time) => time.ToString("HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Formats a date and time like "Tue, Nov 24 at 09:30".</summary>
    public static string When(DateTime time) => time.ToString("ddd, MMM d 'at' HH:mm", CultureInfo.InvariantCulture);

    #endregion
}
