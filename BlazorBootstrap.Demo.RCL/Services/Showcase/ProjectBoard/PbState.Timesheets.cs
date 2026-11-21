using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

/// <summary>A person's approved billable hours on a project, as an invoice line before the draft exists (the preview).</summary>
/// <param name="ProjectId">The project.</param>
/// <param name="PersonId">The person.</param>
/// <param name="Role">The rate card role of the person's job title.</param>
/// <param name="Hours">The hours.</param>
/// <param name="Rate">The role's hourly rate now.</param>
/// <param name="Amount">Hours times rate, rounded to cents.</param>
public sealed record PbInvoicePreviewLine(int ProjectId, string PersonId, PbRateRole Role, decimal Hours, int Rate, decimal Amount);

/// <summary>A client's approved billable hours that aren't on an invoice yet.</summary>
/// <param name="Client">The client.</param>
/// <param name="Hours">The hours.</param>
/// <param name="Amount">What they come to at today's rates, in the client's currency.</param>
/// <param name="From">The first day of those hours.</param>
/// <param name="To">The last day of those hours.</param>
public sealed record PbReadyToInvoice(PbClient Client, decimal Hours, decimal Amount, DateTime From, DateTime To);

/// <summary>One project's hours in a person's week, as the approvals list shows them.</summary>
/// <param name="Timesheet">The week.</param>
/// <param name="Project">The project.</param>
/// <param name="Hours">The project's hours in the week.</param>
/// <param name="BillableHours">The billable part.</param>
/// <param name="IsApproved">Whether this project's hours are approved.</param>
public sealed record PbApprovalRow(PbTimesheet Timesheet, PbProject Project, decimal Hours, decimal BillableHours, bool IsApproved);

/// <summary>
/// Timesheets to client invoice, the board's signature workflow (AC-7f), as a part of <see cref="PbState" />:
/// <list type="number">
/// <item>Team members log hours against the projects they're on (quarter hours, 0.25 to 12 per entry, at most 24 a day, not
/// in the future, not before the project starts; billable by default on a client project, never on an internal one) and
/// submit the week (Monday to Sunday) once it has an entry.</item>
/// <item>Each project's manager approves that project's hours in the week; a week with hours on two projects needs both.
/// A project whose manager can't sign in can be approved by any project manager who can, so no week waits forever in the
/// demo. Any of them can return the whole week with a reason: it becomes editable again, every approval is cleared (any
/// line may change), and the person submits it again.</item>
/// <item>Once every project is approved, the week's billable hours are ready to invoice. The client's account manager
/// creates a draft invoice from the client's approved, uninvoiced billable hours up to a date: a line per project and person
/// at the person's role's rate (the rate card, editable by account managers). Removing a draft line, or deleting the draft,
/// puts its hours back; issuing gives the invoice its number (INV-year-sequence), the issue date, and a due date by the
/// payment terms, and locks it. An issued invoice past its due date is overdue until it is marked paid.</item>
/// </list>
/// Every step is audited (entry changes once the week has been submitted and returned), and the next person to act is told.
/// </summary>
public sealed partial class PbState
{
    #region Fields and Constants

    /// <summary>The longest note on a time entry.</summary>
    public const int MaxEntryNoteLength = 200;

    /// <summary>The most hours one entry can have.</summary>
    public const decimal MaxEntryHours = 12m;

    /// <summary>The most hours a person can log on one day.</summary>
    public const decimal MaxHoursPerDay = 24m;

    /// <summary>The highest hourly rate on the rate card.</summary>
    public const int MaxRate = 400;

    /// <summary>The longest reason for returning a week.</summary>
    public const int MaxReturnReasonLength = 300;

    /// <summary>The fewest hours one entry can have.</summary>
    public const decimal MinEntryHours = 0.25m;

    /// <summary>The lowest hourly rate on the rate card.</summary>
    public const int MinRate = 50;

    /// <summary>The shortest reason for returning a week.</summary>
    public const int MinReturnReasonLength = 10;

    private const int MaxInvoiceLines = 200;

    private const int MaxInvoices = 1000;

    private const int MaxTimeEntries = 6000;

    private const int MaxTimesheets = 3000;

    private readonly List<PbInvoice> invoices;

    private readonly List<PbRate> rates;

    private readonly List<PbTimeEntry> timeEntries;

    private readonly List<PbTimesheet> timesheets;

    private int nextInvoiceId;

    private int nextInvoiceLineId;

    private int nextTimeEntryId;

    private int nextTimesheetId;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Approves one project's hours in a submitted week (the project's approver); the week is approved once every project is.</summary>
    public PbResult ApproveHours(int timesheetId, int projectId)
    {
        var result = ApproveOne(timesheetId, projectId);

        if (result == PbResult.Ok)
            RaiseChanged();

        return result;
    }

    /// <summary>Returns the approvals rows of the projects the signed-in project manager approves: every submitted, approved, or returned week with hours on them.</summary>
    public IReadOnlyList<PbApprovalRow> ApprovalRows()
    {
        if (ProjectManager() is null)
            return Array.Empty<PbApprovalRow>();

        var mine = projects.Where(CanApprove).ToDictionary(project => project.Id);
        var byWeek = timeEntries.Where(entry => mine.ContainsKey(entry.ProjectId)).ToLookup(entry => (entry.PersonId, WeekStart(entry.Date)));
        var rows = new List<PbApprovalRow>();

        foreach (var sheet in timesheets.Where(sheet => sheet.Status != PbTimesheetStatus.Draft))
        {
            foreach (var group in byWeek[(sheet.PersonId, sheet.WeekStart)].GroupBy(entry => entry.ProjectId))
            {
                var approved = sheet.Status == PbTimesheetStatus.Approved || sheet.Approvals.Any(item => item.ProjectId == group.Key && item.ApprovedById is not null);
                rows.Add(new PbApprovalRow(sheet, mine[group.Key], group.Sum(entry => entry.Hours), group.Where(entry => entry.IsBillable).Sum(entry => entry.Hours), approved));
            }
        }

        return rows;
    }

    /// <summary>Returns the amount billed for a project: its lines on issued and paid invoices, in the client's currency.</summary>
    public decimal BilledAmount(int projectId) =>
        invoices.Where(invoice => invoice.Status != PbInvoiceStatus.Draft).SelectMany(invoice => invoice.Lines).Where(line => line.ProjectId == projectId).Sum(line => line.Amount);

    /// <summary>Approves several projects' hours at once from the approvals list, each like <see cref="ApproveHours" />.</summary>
    public PbBulkOutcome BulkApproveHours(IReadOnlyCollection<(int TimesheetId, int ProjectId)> items)
    {
        if (ProjectManager() is null)
            return new PbBulkOutcome(PbResult.NotAllowed, 0, 0);

        var list = (items ?? Array.Empty<(int, int)>()).Distinct().ToList();

        if (list.Count == 0)
            return new PbBulkOutcome(PbResult.Invalid, 0, 0);

        var changed = list.Count(item => ApproveOne(item.TimesheetId, item.ProjectId) == PbResult.Ok);

        if (changed > 0)
            RaiseChanged();

        return new PbBulkOutcome(changed == 0 ? PbResult.NoChange : PbResult.Ok, changed, list.Count - changed);
    }

    /// <summary>Returns several submitted weeks at once with one reason, each like <see cref="ReturnWeek" />.</summary>
    public PbBulkOutcome BulkReturnWeeks(IReadOnlyCollection<int> timesheetIds, string reason)
    {
        if (ProjectManager() is null)
            return new PbBulkOutcome(PbResult.NotAllowed, 0, 0);

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length is < MinReturnReasonLength or > MaxReturnReasonLength)
            return new PbBulkOutcome(PbResult.Invalid, 0, 0);

        return Bulk(timesheetIds, id => ReturnOne(id, reason) == PbResult.Ok);
    }

    /// <summary>Returns whether the signed-in project manager approves a project's hours: its own manager, or any project manager when its manager can't sign in.</summary>
    public bool CanApprove(PbProject project) =>
        ProjectManager() is { } me && (project.OwnerId == me.Id || FindPerson(project.OwnerId)?.AccountId is null || Identity.FindAccount(FindPerson(project.OwnerId)?.AccountId) is null);

    /// <summary>Returns whether a person's week can still change: not submitted yet, or returned.</summary>
    public bool CanEditWeek(string personId, DateTime weekStart) => TimesheetOf(personId, weekStart)?.Status is null or PbTimesheetStatus.Draft or PbTimesheetStatus.Returned;

    /// <summary>
    /// Makes a draft invoice for a client from its approved, uninvoiced billable hours up to a day (account managers): a line per
    /// project and person at today's rates. NoChange when there are no such hours.
    /// </summary>
    public PbResult CreateDraftInvoice(int clientId, DateTime upTo, out PbInvoice? created)
    {
        created = null;

        if (AccountManager() is not { } person)
            return PbResult.NotAllowed;

        if (FindClient(clientId) is not { } client)
            return PbResult.NotFound;

        if (upTo.Date > Clock.Today || !IsPlausible(upTo) || invoices.Count >= MaxInvoices || nextInvoiceId >= NextIdLimit)
            return PbResult.Invalid;

        var covered = Invoiceable(clientId, upTo.Date).ToList();

        if (covered.Count == 0)
            return PbResult.NoChange;

        var preview = Preview(covered);

        if (preview.Count > MaxInvoiceLines || nextInvoiceLineId + preview.Count >= NextIdLimit)
            return PbResult.Invalid;

        var invoice = new PbInvoice
        {
            Id = nextInvoiceId++, ClientId = clientId, Currency = client.BillingCurrency, Status = PbInvoiceStatus.Draft, CreatedAt = Clock.Now, CreatedById = person.Id, HoursUpTo = upTo.Date,
            Lines = preview.Select(line => new PbInvoiceLine
            {
                Id = nextInvoiceLineId++, ProjectId = line.ProjectId, PersonId = line.PersonId, Role = line.Role, Hours = line.Hours, Rate = line.Rate, Amount = line.Amount,
            }).ToList(),
        };

        invoices.Add(invoice);

        foreach (var entry in covered)
            entry.InvoiceId = invoice.Id;

        var result = Commit(() =>
        {
            foreach (var entry in covered)
                entry.InvoiceId = null;

            invoices.Remove(invoice);
            nextInvoiceId--;
            nextInvoiceLineId -= invoice.Lines.Count;
        }, () => Record("Draft invoice created", InvoiceTarget(invoice), InvoicePath(invoice.Id),
            Invariant($"{invoice.Lines.Sum(line => line.Hours):0.##} h, {Total(invoice)}, hours up to {Day(invoice.HoursUpTo)}")));

        if (result == PbResult.Ok)
            created = invoice;

        return result;
    }

    /// <summary>Deletes a draft invoice (account managers); its hours go back to ready to invoice.</summary>
    public PbResult DeleteDraftInvoice(int invoiceId)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (FindInvoice(invoiceId) is not { } invoice)
            return PbResult.NotFound;

        if (invoice.Status != PbInvoiceStatus.Draft)
            return PbResult.WrongState;

        foreach (var entry in timeEntries.Where(entry => entry.InvoiceId == invoiceId))
            entry.InvoiceId = null;

        invoices.Remove(invoice);
        Record("Draft invoice deleted", InvoiceTarget(invoice), null, Invariant($"{invoice.Lines.Sum(line => line.Hours):0.##} h back to ready to invoice"));
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Deletes one of the signed-in person's time entries, while its week can change.</summary>
    public PbResult DeleteTimeEntry(int entryId)
    {
        if (Staff() is not { } person)
            return PbResult.NotAllowed;

        if (FindTimeEntry(entryId) is not { } entry || entry.PersonId != person.Id)
            return PbResult.NotFound;

        if (!CanEditWeek(person.Id, WeekStart(entry.Date)) || entry.InvoiceId is not null)
            return PbResult.WrongState;

        timeEntries.Remove(entry);
        RecordEntryChange("Hours deleted", entry);
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Returns a person's entries in a week, by day and then when they were logged.</summary>
    public IReadOnlyList<PbTimeEntry> EntriesOf(string personId, DateTime weekStart)
    {
        var start = WeekStart(weekStart);

        return timeEntries.Where(entry => entry.PersonId == personId && entry.Date >= start && entry.Date < start.AddDays(7)).OrderBy(entry => entry.Date).ThenBy(entry => entry.Id).ToList();
    }

    /// <summary>Returns the projects a person can log hours on today: the ones they're on (or run) that have started and aren't planned.</summary>
    public IReadOnlyList<PbProject> EntryProjects(string personId) =>
        Projects.Where(project => (project.MemberIds.Contains(personId) || project.OwnerId == personId) && project.Status != PbProjectStatus.Planned && project.StartDate <= Clock.Today).ToList();

    /// <summary>Finds an invoice, or returns <see langword="null" />.</summary>
    public PbInvoice? FindInvoice(int invoiceId) => invoices.FirstOrDefault(invoice => invoice.Id == invoiceId);

    /// <summary>Finds a time entry, or returns <see langword="null" />.</summary>
    public PbTimeEntry? FindTimeEntry(int entryId) => timeEntries.FirstOrDefault(entry => entry.Id == entryId);

    /// <summary>Finds a timesheet, or returns <see langword="null" />.</summary>
    public PbTimesheet? FindTimesheet(int timesheetId) => timesheets.FirstOrDefault(sheet => sheet.Id == timesheetId);

    /// <summary>Returns the hours logged on a project by everyone.</summary>
    public decimal HoursLogged(int projectId) => timeEntries.Where(entry => entry.ProjectId == projectId).Sum(entry => entry.Hours);

    /// <summary>Returns whether an invoice is overdue: issued, and past its due date.</summary>
    public bool IsOverdue(PbInvoice invoice) => invoice.Status == PbInvoiceStatus.Issued && invoice.DueOn is { } due && due.Date < Clock.Today;

    /// <summary>Returns whether hours are valid for one entry: 0.25 to 12, in quarter hours.</summary>
    public static bool IsValidHours(decimal hours) => hours is >= MinEntryHours and <= MaxEntryHours && decimal.Remainder(hours * 4m, 1m) == 0m;

    /// <summary>Marks an issued invoice paid on a day (account managers): on or after its issue date, and not in the future.</summary>
    public PbResult MarkInvoicePaid(int invoiceId, DateTime paidOn)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (FindInvoice(invoiceId) is not { } invoice)
            return PbResult.NotFound;

        if (invoice.Status != PbInvoiceStatus.Issued)
            return PbResult.WrongState;

        if (paidOn.Date < invoice.IssuedOn!.Value.Date || paidOn.Date > Clock.Today)
            return PbResult.Invalid;

        var wasOverdue = IsOverdue(invoice);
        invoice.Status = PbInvoiceStatus.Paid;
        invoice.PaidOn = paidOn.Date;
        Record("Invoice paid", InvoiceTarget(invoice), InvoicePath(invoice.Id), $"{Total(invoice)} received on {Day(paidOn)}{(wasOverdue ? ", after its due date" : null)}");
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Gives an invoice its number and dates (account managers): a draft with lines, due after the payment terms (14, 30, 45, or 60 days).</summary>
    public PbResult IssueInvoice(int invoiceId, int termsDays)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (FindInvoice(invoiceId) is not { } invoice)
            return PbResult.NotFound;

        if (invoice.Status != PbInvoiceStatus.Draft)
            return PbResult.WrongState;

        if (invoice.Lines.Count == 0 || !PbSeedData.PaymentTerms.Contains(termsDays))
            return PbResult.Invalid;

        var year = Clock.Today.Year;
        var prefix = Invariant($"INV-{year}-");
        var sequence = invoices.Where(item => item.Number?.StartsWith(prefix, StringComparison.Ordinal) == true)
            .Select(item => int.TryParse(item.Number![prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0).DefaultIfEmpty(0).Max() + 1;

        if (sequence > 9999)
            return PbResult.Invalid;

        invoice.Status = PbInvoiceStatus.Issued;
        invoice.Number = PbSeedData.InvoiceNumber(year, sequence);
        invoice.IssuedOn = Clock.Today;
        invoice.TermsDays = termsDays;
        invoice.DueOn = Clock.Today.AddDays(termsDays);
        Record("Invoice issued", InvoiceTarget(invoice), InvoicePath(invoice.Id), Invariant($"{Total(invoice)}, Net {termsDays}, due {Day(invoice.DueOn.Value)}"));

        foreach (var group in invoice.Lines.GroupBy(line => line.ProjectId))
        {
            if (FindProject(group.Key) is { } project)
            {
                NotifyPerson(project.OwnerId, $"Hours on {project.Key} were invoiced",
                    Invariant($"{invoice.Number} to {FindClient(invoice.ClientId)?.Name}: {group.Sum(line => line.Hours):0.##} h, {invoice.Currency} {group.Sum(line => line.Amount):#,0.00}."), ProjectPath(project.Id));
            }
        }

        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Returns the lines a draft invoice for a client would have, with hours up to a day, at today's rates (for the dialog's preview).</summary>
    public IReadOnlyList<PbInvoicePreviewLine> PreviewInvoice(int clientId, DateTime upTo) => Preview(Invoiceable(clientId, upTo.Date).ToList());

    /// <summary>Returns the rate of a role on the rate card.</summary>
    public int RateFor(PbRateRole role) => rates.FirstOrDefault(rate => rate.Role == role)?.HourlyRate ?? 0;

    /// <summary>Returns, per client, the approved billable hours not on an invoice yet, with what they come to at today's rates; the largest first.</summary>
    public IReadOnlyList<PbReadyToInvoice> ReadyToInvoice() =>
        clients.Select(client =>
            {
                var covered = Invoiceable(client.Id, Clock.Today).ToList();

                return covered.Count == 0 ? null : new PbReadyToInvoice(client, covered.Sum(entry => entry.Hours), Preview(covered).Sum(line => line.Amount), covered.Min(entry => entry.Date), covered.Max(entry => entry.Date));
            })
            .OfType<PbReadyToInvoice>()
            .OrderByDescending(item => item.Amount)
            .ToList();

    /// <summary>Removes a line from a draft invoice (account managers); its hours go back to ready to invoice.</summary>
    public PbResult RemoveInvoiceLine(int invoiceId, int lineId)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (FindInvoice(invoiceId) is not { } invoice || invoice.Lines.FirstOrDefault(line => line.Id == lineId) is not { } line)
            return PbResult.NotFound;

        if (invoice.Status != PbInvoiceStatus.Draft)
            return PbResult.WrongState;

        foreach (var entry in timeEntries.Where(entry => entry.InvoiceId == invoiceId && entry.ProjectId == line.ProjectId && entry.PersonId == line.PersonId))
            entry.InvoiceId = null;

        invoice.Lines.Remove(line);
        Record("Invoice line removed", InvoiceTarget(invoice), InvoicePath(invoice.Id),
            Invariant($"{FindProject(line.ProjectId)?.Key}, {FindPerson(line.PersonId)?.Name}: {line.Hours:0.##} h back to ready to invoice"));
        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Returns a submitted week with a reason (an approver of one of its projects): it can change again, and every approval is cleared.</summary>
    public PbResult ReturnWeek(int timesheetId, string reason)
    {
        if (ProjectManager() is null)
            return PbResult.NotAllowed;

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length is < MinReturnReasonLength or > MaxReturnReasonLength)
            return PbResult.Invalid;

        var result = ReturnOne(timesheetId, reason);

        if (result == PbResult.Ok)
            RaiseChanged();

        return result;
    }

    /// <summary>Saves the rate card (account managers): each role from <see cref="MinRate" /> to <see cref="MaxRate" />. New drafts use it; existing invoices keep their rates.</summary>
    public PbResult SaveRates(IReadOnlyDictionary<PbRateRole, int> values)
    {
        if (AccountManager() is null)
            return PbResult.NotAllowed;

        if (values is null || Enum.GetValues<PbRateRole>().Any(role => !values.TryGetValue(role, out var value) || value is < MinRate or > MaxRate))
            return PbResult.Invalid;

        var changed = rates.Where(rate => rate.HourlyRate != values[rate.Role]).ToList();

        if (changed.Count == 0)
            return PbResult.NoChange;

        foreach (var rate in changed)
        {
            Record("Rate changed", $"Rate card: {RoleName(rate.Role)}", SiteBase + "/rates", Invariant($"{rate.HourlyRate} → {values[rate.Role]} an hour, for new drafts"));
            rate.HourlyRate = values[rate.Role];
        }

        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>
    /// Logs (<paramref name="entryId" /> 0) or changes one of the signed-in person's time entries, while its week can change:
    /// a project they're on that has started (not planned; a completed one up to its due date), a task of that project, a
    /// day of the week that isn't in the future or before the project's start, 0.25 to 12 hours in quarter hours with at most
    /// 24 that day, billable only on a client project, and a note up to <see cref="MaxEntryNoteLength" /> characters. A new
    /// week starts as a draft.
    /// </summary>
    public PbResult SaveTimeEntry(int entryId, PbTimeEntryInput input, out PbTimeEntry? saved)
    {
        saved = null;

        if (Staff() is not { } person)
            return PbResult.NotAllowed;

        var existing = entryId == 0 ? null : FindTimeEntry(entryId);

        if (entryId != 0 && (existing is null || existing.PersonId != person.Id))
            return PbResult.NotFound;

        if (input is null || FindProject(input.ProjectId) is not { } project)
            return PbResult.NotFound;

        var date = input.Date.Date;
        var note = input.Note?.Trim() ?? string.Empty;

        if (!CanEditWeek(person.Id, WeekStart(date)) || (existing is not null && (!CanEditWeek(person.Id, WeekStart(existing.Date)) || existing.InvoiceId is not null)))
            return PbResult.WrongState;

        if (!IsValidHours(input.Hours) || note.Length > MaxEntryNoteLength || date > Clock.Today || !IsPlausible(date) || !CanLogOn(project, person.Id, date)
            || (input.TaskId is { } taskId && FindTask(taskId)?.ProjectId != project.Id) || (input.IsBillable && project.ClientId is null)
            || HoursOn(person.Id, date, entryId) + input.Hours > MaxHoursPerDay || (existing is null && (timeEntries.Count >= MaxTimeEntries || nextTimeEntryId >= NextIdLimit)))
            return PbResult.Invalid;

        if (existing is not null && existing.ProjectId == project.Id && existing.TaskId == input.TaskId && existing.Date == date && existing.Hours == input.Hours
            && existing.IsBillable == input.IsBillable && existing.Note == note)
            return PbResult.NoChange;

        var sheet = TimesheetOf(person.Id, WeekStart(date));
        var newSheet = sheet is null;

        if (newSheet)
        {
            if (timesheets.Count >= MaxTimesheets || nextTimesheetId >= NextIdLimit)
                return PbResult.Invalid;

            sheet = new PbTimesheet { Id = nextTimesheetId++, PersonId = person.Id, WeekStart = WeekStart(date), Status = PbTimesheetStatus.Draft };
            timesheets.Add(sheet);
        }

        var entry = existing ?? new PbTimeEntry { Id = nextTimeEntryId, PersonId = person.Id, CreatedAt = Clock.Now };
        var previous = (entry.ProjectId, entry.TaskId, entry.Date, entry.Hours, entry.IsBillable, entry.Note);
        (entry.ProjectId, entry.TaskId, entry.Date, entry.Hours, entry.IsBillable, entry.Note) = (project.Id, input.TaskId, date, input.Hours, input.IsBillable, note);

        if (existing is null)
        {
            timeEntries.Add(entry);
            nextTimeEntryId++;
        }

        var result = Commit(() =>
        {
            if (existing is null)
            {
                timeEntries.Remove(entry);
                nextTimeEntryId--;
            }
            else
            {
                (entry.ProjectId, entry.TaskId, entry.Date, entry.Hours, entry.IsBillable, entry.Note) = previous;
            }

            if (newSheet)
            {
                timesheets.Remove(sheet!);
                nextTimesheetId--;
            }
        }, () => RecordEntryChange(existing is null ? "Hours logged" : "Hours changed", entry));

        if (result == PbResult.Ok)
            saved = entry;

        return result;
    }

    /// <summary>
    /// Submits the signed-in person's week (Draft or Returned, with at least one entry, not a future week): each project with
    /// hours in it waits for its approver, who is told.
    /// </summary>
    public PbResult SubmitWeek(DateTime weekStart)
    {
        if (Staff() is not { } person)
            return PbResult.NotAllowed;

        var start = WeekStart(weekStart);

        if (TimesheetOf(person.Id, start) is not { } sheet || EntriesOf(person.Id, start) is not { Count: > 0 } entries)
            return PbResult.NotFound;

        if (sheet.Status is not (PbTimesheetStatus.Draft or PbTimesheetStatus.Returned))
            return PbResult.WrongState;

        if (start > WeekStart(Clock.Today))
            return PbResult.Invalid;

        var projectIds = entries.Select(entry => entry.ProjectId).Distinct().Order().ToList();
        var wasReturned = sheet.Status == PbTimesheetStatus.Returned;
        sheet.Status = PbTimesheetStatus.Submitted;
        sheet.SubmittedAt = Clock.Now;
        sheet.DecidedAt = null;
        sheet.ReturnReason = null;
        sheet.ReturnedById = null;
        sheet.Approvals = projectIds.Select(id => new PbProjectApproval { ProjectId = id }).ToList();

        var hours = entries.Sum(entry => entry.Hours);
        var keys = string.Join(", ", projectIds.Select(id => FindProject(id)?.Key));
        Record(wasReturned ? "Week resubmitted" : "Week submitted", TimesheetTarget(sheet), ApprovalPath(sheet.Id), Invariant($"{hours:0.##} h on {keys}"));

        foreach (var approverId in projectIds.Select(id => FindProject(id)).OfType<PbProject>().SelectMany(ApproverIds).Distinct().ToList())
        {
            Notify(approverId, $"{person.Name} {(wasReturned ? "resubmitted" : "submitted")} the week of {WeekLabel(start)}",
                Invariant($"{hours:0.##} h on {keys}. Approve the hours or return the week."), ApprovalPath(sheet.Id));
        }

        RaiseChanged();

        return PbResult.Ok;
    }

    /// <summary>Returns a person's week, or <see langword="null" /> before they log hours in it.</summary>
    public PbTimesheet? TimesheetOf(string personId, DateTime weekStart)
    {
        var start = WeekStart(weekStart);

        return timesheets.FirstOrDefault(sheet => sheet.PersonId == personId && sheet.WeekStart == start);
    }

    /// <summary>Returns an invoice's total: the sum of its lines.</summary>
    public static decimal TotalOf(PbInvoice invoice) => invoice.Lines.Sum(line => line.Amount);

    /// <summary>Returns a rate card role in words.</summary>
    public static string RoleName(PbRateRole role) => role switch
    {
        PbRateRole.Quality => "Quality assurance",
        _ => role.ToString(),
    };

    /// <summary>Returns a week's label, like "Nov 16".</summary>
    public static string WeekLabel(DateTime weekStart) => weekStart.ToString("MMM d", CultureInfo.InvariantCulture);

    #endregion

    #region Private Methods

    [GeneratedRegex(@"^INV-\d{4}-\d{4}$")]
    private static partial Regex InvoiceNumberPattern();

    private static string ApprovalPath(int timesheetId) => Invariant($"{SiteBase}/approvals/{timesheetId}");

    private static string InvoicePath(int invoiceId) => Invariant($"{SiteBase}/invoices/{invoiceId}");

    private static string TimesheetPath(DateTime weekStart) => Invariant($"{SiteBase}/timesheet?week={weekStart:yyyy-MM-dd}");

    private static string Total(PbInvoice invoice) => Invariant($"{invoice.Currency} {TotalOf(invoice):#,0.00}");

    /// <summary>Approves one project's hours without raising <see cref="Changed" />; tells the person and the account managers once the whole week is approved.</summary>
    private PbResult ApproveOne(int timesheetId, int projectId)
    {
        if (ProjectManager() is not { } me)
            return PbResult.NotAllowed;

        if (FindTimesheet(timesheetId) is not { } sheet || FindProject(projectId) is not { } project)
            return PbResult.NotFound;

        if (!CanApprove(project))
            return PbResult.NotAllowed;

        if (sheet.Status != PbTimesheetStatus.Submitted || sheet.Approvals.FirstOrDefault(item => item.ProjectId == projectId) is not { ApprovedById: null } approval)
            return PbResult.WrongState;

        approval.ApprovedById = me.Id;
        approval.ApprovedAt = Clock.Now;
        var hours = EntriesOf(sheet.PersonId, sheet.WeekStart).Where(entry => entry.ProjectId == projectId).Sum(entry => entry.Hours);
        Record("Hours approved", TimesheetTarget(sheet), ApprovalPath(sheet.Id), Invariant($"{project.Key}: {hours:0.##} h"));

        if (sheet.Approvals.All(item => item.ApprovedById is not null))
        {
            sheet.Status = PbTimesheetStatus.Approved;
            sheet.DecidedAt = Clock.Now;
            Record("Week approved", TimesheetTarget(sheet), ApprovalPath(sheet.Id), "Every project's hours are approved");
            var entries = EntriesOf(sheet.PersonId, sheet.WeekStart);
            NotifyPerson(sheet.PersonId, $"Your week of {WeekLabel(sheet.WeekStart)} is approved", Invariant($"{entries.Sum(entry => entry.Hours):0.##} h approved."), TimesheetPath(sheet.WeekStart));

            foreach (var group in entries.Where(entry => entry.IsBillable).GroupBy(entry => FindProject(entry.ProjectId)?.ClientId).Where(group => group.Key is not null))
            {
                if (FindClient(group.Key) is { } client)
                {
                    NotifyPerson(client.OwnerId, $"Hours are ready to invoice for {client.Name}",
                        Invariant($"{FindPerson(sheet.PersonId)?.Name}'s week of {WeekLabel(sheet.WeekStart)}: {group.Sum(entry => entry.Hours):0.##} billable hours approved. Create a draft invoice."),
                        SiteBase + "/invoices");
                }
            }
        }
        else
        {
            var waiting = string.Join(", ", sheet.Approvals.Where(item => item.ApprovedById is null).Select(item => FindProject(item.ProjectId)?.Key));
            NotifyPerson(sheet.PersonId, $"{project.Key} hours approved for the week of {WeekLabel(sheet.WeekStart)}", $"Still waiting for {waiting}.", TimesheetPath(sheet.WeekStart));
        }

        return PbResult.Ok;
    }

    /// <summary>The people who approve a project's hours: its manager when they can sign in, otherwise every project manager who can.</summary>
    private IEnumerable<string> ApproverIds(PbProject project)
    {
        if (FindPerson(project.OwnerId)?.AccountId is { } accountId && Identity.FindAccount(accountId) is not null)
            return new[] { accountId };

        return Identity.Accounts.Where(account => account.RoleId == PbSeedData.ProjectManagerRole).Select(account => account.Id).ToList();
    }

    /// <summary>Whether a person can log hours on a project on a day: they're on it (or run it), it isn't planned, the day isn't before its start, and a completed project only up to its due date.</summary>
    private bool CanLogOn(PbProject project, string personId, DateTime date) =>
        (project.MemberIds.Contains(personId) || project.OwnerId == personId) && project.Status != PbProjectStatus.Planned && date >= project.StartDate.Date
        && (project.Status != PbProjectStatus.Completed || date <= project.DueDate.Date);

    /// <summary>The hours a person logged on a day, leaving one entry out.</summary>
    private decimal HoursOn(string personId, DateTime date, int exceptEntryId = 0) =>
        timeEntries.Where(entry => entry.PersonId == personId && entry.Date == date.Date && entry.Id != exceptEntryId).Sum(entry => entry.Hours);

    /// <summary>A client's approved billable hours not on an invoice, up to a day.</summary>
    private IEnumerable<PbTimeEntry> Invoiceable(int clientId, DateTime upTo)
    {
        var projectIds = projects.Where(project => project.ClientId == clientId).Select(project => project.Id).ToHashSet();
        var approved = timesheets.Where(sheet => sheet.Status == PbTimesheetStatus.Approved).Select(sheet => (sheet.PersonId, sheet.WeekStart)).ToHashSet();

        return timeEntries.Where(entry => entry.IsBillable && entry.InvoiceId is null && entry.Date <= upTo && projectIds.Contains(entry.ProjectId) && approved.Contains((entry.PersonId, WeekStart(entry.Date))));
    }

    /// <summary>An invoice as the audit log names it: its number (or "Draft") and the client.</summary>
    private string InvoiceTarget(PbInvoice invoice) => $"Invoice {invoice.Number ?? "draft"} · {FindClient(invoice.ClientId)?.Name ?? "a deleted client"}";

    /// <summary>Groups entries into invoice lines: one per project and person, at the person's role's rate now.</summary>
    private List<PbInvoicePreviewLine> Preview(IReadOnlyCollection<PbTimeEntry> entries) =>
        entries.GroupBy(entry => (entry.ProjectId, entry.PersonId))
            .Select(group =>
            {
                var role = PbSeedData.RoleFor(FindPerson(group.Key.PersonId)?.Title);
                var rate = RateFor(role);
                var hours = group.Sum(entry => entry.Hours);

                return new PbInvoicePreviewLine(group.Key.ProjectId, group.Key.PersonId, role, hours, rate, Math.Round(hours * rate, 2));
            })
            .OrderBy(line => FindProject(line.ProjectId)?.Key, StringComparer.Ordinal)
            .ThenBy(line => FindPerson(line.PersonId)?.Name, StringComparer.Ordinal)
            .ToList();

    /// <summary>Audits an entry change once its week has been submitted and returned; a draft week's entries are the person's own work in progress.</summary>
    private void RecordEntryChange(string action, PbTimeEntry entry)
    {
        if (TimesheetOf(entry.PersonId, WeekStart(entry.Date)) is { Status: PbTimesheetStatus.Returned } sheet)
            Record(action, TimesheetTarget(sheet), ApprovalPath(sheet.Id), Invariant($"{FindProject(entry.ProjectId)?.Key}, {Day(entry.Date)}: {entry.Hours:0.##} h{(entry.IsBillable ? null : ", not billable")}"));
    }

    /// <summary>Reminds the signed-in account manager of issued invoices to their clients past their due date (once per invoice).</summary>
    private void RemindOverdueInvoices()
    {
        if (AccountManager() is not { } person || person.AccountId is not { } accountId)
            return;

        foreach (var invoice in invoices.Where(invoice => IsOverdue(invoice) && FindClient(invoice.ClientId)?.OwnerId == person.Id).OrderBy(invoice => invoice.DueOn).Take(MaxRemindersPerSignIn))
        {
            var title = $"{invoice.Number} is overdue";
            var url = InvoicePath(invoice.Id);

            if (notifications.Any(notice => notice.AccountId == accountId && notice.Url == url && notice.Title == title))
                continue;

            Notify(accountId, title, $"{FindClient(invoice.ClientId)?.Name} was due to pay {Total(invoice)} on {Day(invoice.DueOn!.Value)}.", url, fromSystem: true);
        }
    }

    /// <summary>Returns one week without raising <see cref="Changed" />, and tells the person why.</summary>
    private PbResult ReturnOne(int timesheetId, string reason)
    {
        if (ProjectManager() is not { } me)
            return PbResult.NotAllowed;

        if (FindTimesheet(timesheetId) is not { } sheet)
            return PbResult.NotFound;

        if (sheet.Status != PbTimesheetStatus.Submitted)
            return PbResult.WrongState;

        if (!sheet.Approvals.Select(item => FindProject(item.ProjectId)).OfType<PbProject>().Any(CanApprove))
            return PbResult.NotAllowed;

        sheet.Status = PbTimesheetStatus.Returned;
        sheet.DecidedAt = Clock.Now;
        sheet.ReturnedById = me.Id;
        sheet.ReturnReason = Clip(reason, MaxReturnReasonLength);
        sheet.Approvals = new List<PbProjectApproval>();
        Record("Week returned", TimesheetTarget(sheet), ApprovalPath(sheet.Id), sheet.ReturnReason);
        NotifyPerson(sheet.PersonId, $"Your week of {WeekLabel(sheet.WeekStart)} was returned", $"{me.Name}: {sheet.ReturnReason}", TimesheetPath(sheet.WeekStart));

        return PbResult.Ok;
    }

    /// <summary>A timesheet as the audit log names it: the person and the week.</summary>
    private string TimesheetTarget(PbTimesheet sheet) => $"Timesheet {FindPerson(sheet.PersonId)?.Name ?? "a former teammate"}, week of {WeekLabel(sheet.WeekStart)}";

    /// <summary>
    /// Validates the timesheet part of a rebuilt snapshot against itself and the rest of the board: entries, weeks, invoices
    /// with their lines and the hours behind them, and the rate card.
    /// </summary>
    private static bool ValidTimesheetData(List<PbTimeEntry> storedEntries, List<PbTimesheet> storedSheets, List<PbInvoice> storedInvoices, List<PbRate> storedRates, PbSnapshot snapshot,
        Dictionary<int, PbProject> projectsById, HashSet<string> personIds, Dictionary<int, PbClient> clientsById, HashSet<int> taskIds, Dictionary<int, int> taskProjects)
    {
        if (storedRates.Count != Enum.GetValues<PbRateRole>().Length || storedRates.Any(rate => rate is null || !Enum.IsDefined(rate.Role) || rate.HourlyRate is < MinRate or > MaxRate)
            || !Unique(storedRates.Select(rate => rate.Role)))
            return false;

        var invoiceIds = storedInvoices.Where(invoice => invoice is not null).Select(invoice => invoice.Id).ToHashSet();

        if (storedEntries.Any(entry => entry is null || entry.Id < 1 || entry.Id >= snapshot.NextTimeEntryId || !personIds.Contains(entry.PersonId)
                || !projectsById.TryGetValue(entry.ProjectId, out var project) || (entry.TaskId is { } taskId && (!taskIds.Contains(taskId) || taskProjects[taskId] != entry.ProjectId))
                || entry.Date != entry.Date.Date || !IsPlausible(entry.Date) || !IsValidHours(entry.Hours) || (entry.IsBillable && project.ClientId is null)
                || entry.Note is not { Length: <= MaxEntryNoteLength } || !IsPlausible(entry.CreatedAt) || (entry.InvoiceId is { } invoiceId && !invoiceIds.Contains(invoiceId)))
            || !Unique(storedEntries.Select(entry => entry.Id))
            || storedEntries.GroupBy(entry => (entry.PersonId, entry.Date)).Any(group => group.Sum(entry => entry.Hours) > MaxHoursPerDay))
            return false;

        if (storedSheets.Any(sheet => sheet is null || sheet.Id < 1 || sheet.Id >= snapshot.NextTimesheetId || !personIds.Contains(sheet.PersonId) || sheet.WeekStart != WeekStart(sheet.WeekStart)
                || !IsPlausible(sheet.WeekStart) || !Enum.IsDefined(sheet.Status) || sheet.Approvals is null || sheet.Approvals.Any(item => item is null || !projectsById.ContainsKey(item.ProjectId)
                    || (item.ApprovedById is null) != (item.ApprovedAt is null) || (item.ApprovedById is not null && !personIds.Contains(item.ApprovedById)) || (item.ApprovedAt is { } at && !IsPlausible(at)))
                || !Unique(sheet.Approvals.Select(item => item.ProjectId)) || (sheet.SubmittedAt is { } submitted && !IsPlausible(submitted)) || (sheet.DecidedAt is { } decided && !IsPlausible(decided))
                || sheet.Status switch
                {
                    PbTimesheetStatus.Draft => sheet.Approvals.Count > 0 || sheet.ReturnReason is not null,
                    PbTimesheetStatus.Submitted => sheet.SubmittedAt is null || sheet.Approvals.Count == 0 || sheet.Approvals.All(item => item.ApprovedById is not null) || sheet.ReturnReason is not null,
                    PbTimesheetStatus.Returned => sheet.SubmittedAt is null || sheet.DecidedAt is null || sheet.Approvals.Count > 0 || sheet.ReturnedById is null || !personIds.Contains(sheet.ReturnedById)
                        || sheet.ReturnReason is not { Length: >= MinReturnReasonLength and <= MaxReturnReasonLength },
                    _ => sheet.SubmittedAt is null || sheet.DecidedAt is null || sheet.Approvals.Count == 0 || sheet.Approvals.Any(item => item.ApprovedById is null) || sheet.ReturnReason is not null,
                })
            || !Unique(storedSheets.Select(sheet => sheet.Id)) || !Unique(storedSheets.Select(sheet => (sheet.PersonId, sheet.WeekStart))))
            return false;

        var sheets = storedSheets.ToDictionary(sheet => (sheet.PersonId, sheet.WeekStart));

        // Every entry belongs to a week; a submitted or approved week's projects are the projects it has hours on; invoiced hours are billable and approved.
        foreach (var group in storedEntries.GroupBy(entry => (entry.PersonId, WeekStart(entry.Date))))
        {
            if (!sheets.TryGetValue(group.Key, out var sheet))
                return false;

            if (sheet.Status is PbTimesheetStatus.Submitted or PbTimesheetStatus.Approved && !group.Select(entry => entry.ProjectId).Distinct().Order().SequenceEqual(sheet.Approvals.Select(item => item.ProjectId).Order()))
                return false;

            if (group.Any(entry => entry.InvoiceId is not null && (!entry.IsBillable || sheet.Status != PbTimesheetStatus.Approved)))
                return false;
        }

        if (storedInvoices.Any(invoice => invoice is null || invoice.Id < 1 || invoice.Id >= snapshot.NextInvoiceId || !clientsById.ContainsKey(invoice.ClientId) || invoice.Currency is null
                || !PbSeedData.Currencies.Contains(invoice.Currency) || !Enum.IsDefined(invoice.Status) || !personIds.Contains(invoice.CreatedById) || !IsPlausible(invoice.CreatedAt)
                || !IsPlausible(invoice.HoursUpTo) || invoice.Lines is not { Count: <= MaxInvoiceLines }
                || invoice.Lines.Any(line => line is null || line.Id < 1 || line.Id >= snapshot.NextInvoiceLineId || !projectsById.TryGetValue(line.ProjectId, out var project) || project.ClientId != invoice.ClientId
                    || (invoice.IsImported ? line.PersonId is not null : line.PersonId is null || !personIds.Contains(line.PersonId)) || !Enum.IsDefined(line.Role) || line.Hours <= 0 || line.Hours > 100_000
                    || decimal.Remainder(line.Hours * 4m, 1m) != 0m || line.Rate is < 1 or > 1000 || line.Amount != Math.Round(line.Hours * line.Rate, 2))
                || invoice.Status switch
                {
                    PbInvoiceStatus.Draft => invoice.Number is not null || invoice.IssuedOn is not null || invoice.DueOn is not null || invoice.PaidOn is not null || invoice.IsImported,
                    _ => invoice.Number is null || !InvoiceNumberPattern().IsMatch(invoice.Number) || invoice.IssuedOn is not { } issued || !IsPlausible(issued) || invoice.Lines.Count == 0
                        || !PbSeedData.PaymentTerms.Contains(invoice.TermsDays) || invoice.DueOn != issued.Date.AddDays(invoice.TermsDays)
                        || (invoice.Status == PbInvoiceStatus.Paid ? invoice.PaidOn is not { } paid || paid.Date < issued.Date || !IsPlausible(paid) : invoice.PaidOn is not null),
                })
            || !Unique(storedInvoices.Select(invoice => invoice.Id)) || !Unique(storedInvoices.SelectMany(invoice => invoice.Lines).Select(line => line.Id))
            || !Unique(storedInvoices.Where(invoice => invoice.Number is not null).Select(invoice => invoice.Number)))
            return false;

        // The hours behind each line are exactly the entries on that invoice for its project and person (none for an invoice from before time tracking).
        var invoiced = storedEntries.Where(entry => entry.InvoiceId is not null).ToLookup(entry => (entry.InvoiceId!.Value, entry.ProjectId, entry.PersonId));

        foreach (var invoice in storedInvoices)
        {
            if (invoice.IsImported ? storedEntries.Any(entry => entry.InvoiceId == invoice.Id)
                : !Unique(invoice.Lines.Select(line => (line.ProjectId, line.PersonId))) || invoice.Lines.Any(line => invoiced[(invoice.Id, line.ProjectId, line.PersonId!)].Sum(entry => entry.Hours) != line.Hours)
                    || storedEntries.Any(entry => entry.InvoiceId == invoice.Id && invoice.Lines.All(line => line.ProjectId != entry.ProjectId || line.PersonId != entry.PersonId)))
                return false;
        }

        return true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the invoices, newest first (drafts by when they were made, the others by their issue date).</summary>
    public IReadOnlyList<PbInvoice> Invoices => invoices.OrderByDescending(invoice => invoice.IssuedOn ?? invoice.CreatedAt).ThenByDescending(invoice => invoice.Id).ToList();

    /// <summary>Gets the signed-in person's weeks with hours or a status, newest first.</summary>
    public IReadOnlyList<PbTimesheet> MyTimesheets => Staff() is { } person
        ? timesheets.Where(sheet => sheet.PersonId == person.Id).OrderByDescending(sheet => sheet.WeekStart).ToList()
        : Array.Empty<PbTimesheet>();

    /// <summary>Gets the rate card, by role.</summary>
    public IReadOnlyList<PbRate> Rates => rates.OrderBy(rate => rate.Role).ToList();

    #endregion
}
