using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

/// <summary>
/// The timesheet and invoicing part of the seed (AC-7f): the rate card; seven weeks of hours for the demo projects' team
/// (Jordan, Kai, Amara, Ben, and Lucia on WEB, APP, and BRD), approved and partly invoiced in the older weeks, approved and
/// ready to invoice in two weeks, submitted and waiting for Nadia last week (Ben's returned), and Jordan's current week in
/// draft; three invoices from those hours (one paid, one overdue, one issued); and one paid or issued invoice for each
/// completed project of the other clients, from before time tracking. Its audit entries and notifications join the rest.
/// </summary>
public static partial class PbSeedData
{
    #region Fields and Constants

    /// <summary>The payment terms an invoice can have, in days.</summary>
    public static readonly IReadOnlyList<int> PaymentTerms = new[] { 14, 30, 45, 60 };

    private static readonly string[] entryNotes = { "Build and review", "Fixes from review", "Client call", "Pairing", "Design QA", string.Empty, "Planning", string.Empty };

    /// <summary>The people whose hours are seeded, in order.</summary>
    private static readonly string[] timesheetPeople = { SeededMemberId, "kai", "amara", "ben", "lucia" };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns an invoice number like "INV-2026-0042".</summary>
    public static string InvoiceNumber(int year, int sequence) => Invariant($"INV-{year}-{sequence:0000}");

    /// <summary>The rate card in a fresh tab: an hourly rate per role, the same number in every billing currency.</summary>
    public static List<PbRate> Rates() => new()
    {
        new() { Role = PbRateRole.Design, HourlyRate = 140 },
        new() { Role = PbRateRole.Engineering, HourlyRate = 150 },
        new() { Role = PbRateRole.Quality, HourlyRate = 120 },
        new() { Role = PbRateRole.Content, HourlyRate = 110 },
        new() { Role = PbRateRole.Management, HourlyRate = 160 },
    };

    /// <summary>The rate card role of a job title: designers and researchers, QA, content and copy, managers, and everyone else in engineering.</summary>
    public static PbRateRole RoleFor(string? title)
    {
        var value = title ?? string.Empty;

        return value.Contains("design", StringComparison.OrdinalIgnoreCase) || value.Contains("research", StringComparison.OrdinalIgnoreCase) ? PbRateRole.Design
            : value.Contains("QA", StringComparison.Ordinal) || value.Contains("quality", StringComparison.OrdinalIgnoreCase) ? PbRateRole.Quality
            : value.Contains("content", StringComparison.OrdinalIgnoreCase) || value.Contains("copywriter", StringComparison.OrdinalIgnoreCase) ? PbRateRole.Content
            : value.Contains("manager", StringComparison.OrdinalIgnoreCase) ? PbRateRole.Management
            : PbRateRole.Engineering;
    }

    #endregion

    #region Private Methods

    /// <summary>Returns the Monday of the week a date is in.</summary>
    private static DateTime WeekStart(DateTime date) => PbState.WeekStart(date);

    /// <summary>A week's label, like "Nov 16".</summary>
    private static string WeekLabel(DateTime weekStart) => weekStart.ToString("MMM d", CultureInfo.InvariantCulture);

    /// <summary>Builds the invoice lines of a set of entries: one per project and person, at the person's role's rate.</summary>
    private static List<PbInvoiceLine> LinesFor(IEnumerable<PbTimeEntry> entries, IReadOnlyDictionary<string, PbPerson> people, IReadOnlyList<PbRate> rates, ref int nextLineId)
    {
        var lines = new List<PbInvoiceLine>();

        foreach (var group in entries.GroupBy(entry => (entry.ProjectId, entry.PersonId)).OrderBy(group => group.Key.ProjectId).ThenBy(group => people[group.Key.PersonId].Name, StringComparer.Ordinal))
        {
            var role = RoleFor(people[group.Key.PersonId].Title);
            var rate = rates.First(item => item.Role == role).HourlyRate;
            var hours = group.Sum(entry => entry.Hours);
            lines.Add(new PbInvoiceLine { Id = nextLineId++, ProjectId = group.Key.ProjectId, PersonId = group.Key.PersonId, Role = role, Rate = rate, Hours = hours, Amount = Math.Round(hours * rate, 2) });
        }

        return lines;
    }

    /// <summary>The timesheet audit entries: weeks submitted, approved, and returned, and invoices created, issued, and paid in the last 90 days.</summary>
    private static void TimesheetAudit(List<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)> entries, IReadOnlyList<PbPerson> people,
        IReadOnlyList<PbClient> clients, IReadOnlyList<PbProject> projects, IReadOnlyList<PbTimeEntry> timeEntries, IReadOnlyList<PbTimesheet> timesheets, IReadOnlyList<PbInvoice> invoices)
    {
        var since = Now.AddDays(-90);
        var names = people.ToDictionary(person => person.Id, person => person.Name);
        var keys = projects.ToDictionary(project => project.Id, project => project.Key);
        var clientNames = clients.ToDictionary(client => client.Id, client => client.Name);

        foreach (var sheet in timesheets.Where(sheet => sheet.SubmittedAt is not null))
        {
            var target = $"Timesheet {names[sheet.PersonId]}, week of {WeekLabel(sheet.WeekStart)}";
            var url = Invariant($"{SiteBase}/approvals/{sheet.Id}");
            var hours = timeEntries.Where(entry => entry.PersonId == sheet.PersonId && entry.Date >= sheet.WeekStart && entry.Date < sheet.WeekStart.AddDays(7)).Sum(entry => entry.Hours);

            entries.Add((sheet.SubmittedAt!.Value, names[sheet.PersonId], "Week submitted", target, url, Invariant($"{hours:0.##} h on {string.Join(", ", sheet.Approvals.Select(item => keys[item.ProjectId]).DefaultIfEmpty("its projects"))}")));

            if (sheet.Status == PbTimesheetStatus.Approved)
            {
                foreach (var approval in sheet.Approvals)
                    entries.Add((approval.ApprovedAt!.Value, names[approval.ApprovedById!], "Hours approved", target, url, $"{keys[approval.ProjectId]}: approved"));

                entries.Add((sheet.DecidedAt!.Value.AddSeconds(1), SystemActorName, "Week approved", target, url, "Every project's hours are approved"));
            }
            else if (sheet.Status == PbTimesheetStatus.Returned)
            {
                entries.Add((sheet.DecidedAt!.Value, names[sheet.ReturnedById!], "Week returned", target, url, sheet.ReturnReason));
            }
        }

        foreach (var invoice in invoices)
        {
            var label = invoice.Number ?? "Draft invoice";
            var target = $"Invoice {label} · {clientNames[invoice.ClientId]}";
            var url = Invariant($"{SiteBase}/invoices/{invoice.Id}");
            var total = Invariant($"{invoice.Currency} {invoice.Lines.Sum(line => line.Amount):#,0.00}");

            if (!invoice.IsImported)
                entries.Add((invoice.CreatedAt, names[invoice.CreatedById], "Draft invoice created", target, url, $"{total}, hours up to {Day(invoice.HoursUpTo)}"));

            if (invoice.IssuedOn is { } issued)
                entries.Add((issued.AddHours(11), names[invoice.CreatedById], "Invoice issued", target, url, $"{total}, Net {invoice.TermsDays}, due {Day(invoice.DueOn!.Value)}"));

            if (invoice.PaidOn is { } paid)
                entries.Add((paid.AddHours(14), names[invoice.CreatedById], "Invoice paid", target, url, $"{total} received on {Day(paid)}"));
        }

        entries.RemoveAll(entry => entry.At < since && (entry.Action.StartsWith("Invoice", StringComparison.Ordinal) || entry.Action.StartsWith("Week", StringComparison.Ordinal) || entry.Action == "Hours approved" || entry.Action == "Draft invoice created"));
    }

    /// <summary>The timesheet notifications: weeks waiting for Nadia, Jordan's approved week, hours ready to invoice and an overdue invoice for Elena, and hours invoiced for Nadia.</summary>
    private static void TimesheetNotifications(List<(string AccountId, DateTime At, string Title, string Text, string Url, bool Action)> items, IReadOnlyList<PbPerson> people,
        IReadOnlyList<PbClient> clients, IReadOnlyList<PbProject> projects, IReadOnlyList<PbTimeEntry> timeEntries, IReadOnlyList<PbTimesheet> timesheets, IReadOnlyList<PbInvoice> invoices)
    {
        var names = people.ToDictionary(person => person.Id, person => person.Name);
        var keys = projects.ToDictionary(project => project.Id, project => project.Key);

        decimal HoursOf(PbTimesheet sheet) => timeEntries.Where(entry => entry.PersonId == sheet.PersonId && entry.Date >= sheet.WeekStart && entry.Date < sheet.WeekStart.AddDays(7)).Sum(entry => entry.Hours);

        foreach (var sheet in timesheets.Where(sheet => sheet.Status == PbTimesheetStatus.Submitted))
        {
            items.Add((SeededProjectManagerId, sheet.SubmittedAt!.Value.AddMinutes(1), $"{names[sheet.PersonId]} submitted the week of {WeekLabel(sheet.WeekStart)}",
                Invariant($"{HoursOf(sheet):0.##} h on {string.Join(", ", sheet.Approvals.Select(item => keys[item.ProjectId]))}. Approve the hours or return the week."), Invariant($"{SiteBase}/approvals/{sheet.Id}"), true));
        }

        if (timesheets.Where(sheet => sheet.PersonId == SeededMemberId && sheet.Status == PbTimesheetStatus.Approved).OrderByDescending(sheet => sheet.WeekStart).FirstOrDefault() is { } approved)
        {
            items.Add((SeededMemberId, approved.DecidedAt!.Value.AddMinutes(1), $"Your week of {WeekLabel(approved.WeekStart)} is approved",
                Invariant($"{HoursOf(approved):0.##} h approved by {names[SeededProjectManagerId]}."), Invariant($"{SiteBase}/timesheet?week={approved.WeekStart:yyyy-MM-dd}"), false));
        }

        // The newest approved week made the uninvoiced hours ready to invoice for each client of the account manager.
        var lastApproved = timesheets.Where(sheet => sheet.Status == PbTimesheetStatus.Approved).Max(sheet => sheet.DecidedAt!.Value);

        foreach (var client in clients.Where(client => client.OwnerId == SeededAccountManagerId))
        {
            var ready = timeEntries.Where(entry => entry.IsBillable && entry.InvoiceId is null && projects.First(project => project.Id == entry.ProjectId).ClientId == client.Id
                && timesheets.Any(sheet => sheet.PersonId == entry.PersonId && sheet.WeekStart == WeekStart(entry.Date) && sheet.Status == PbTimesheetStatus.Approved)).Sum(entry => entry.Hours);

            if (ready > 0)
                items.Add((SeededAccountManagerId, lastApproved.AddMinutes(2), $"Hours are ready to invoice for {client.Name}", Invariant($"{ready:0.##} approved billable hours. Create a draft invoice."), SiteBase + "/invoices", true));
        }

        foreach (var invoice in invoices.Where(invoice => invoice.Status == PbInvoiceStatus.Issued && invoice.DueOn < Now.Date && !invoice.IsImported))
        {
            var client = clients.First(item => item.Id == invoice.ClientId);
            items.Add((SeededAccountManagerId, invoice.DueOn!.Value.AddDays(1).AddHours(7), $"{invoice.Number} is overdue",
                Invariant($"{client.Name} was due to pay {invoice.Currency} {invoice.Lines.Sum(line => line.Amount):#,0.00} on {invoice.DueOn:MMM d}."), Invariant($"{SiteBase}/invoices/{invoice.Id}"), true));
        }

        foreach (var invoice in invoices.Where(invoice => !invoice.IsImported && invoice.IssuedOn is not null))
        {
            var projectKeys = string.Join(", ", invoice.Lines.Select(line => keys[line.ProjectId]).Distinct());
            items.Add((SeededProjectManagerId, invoice.IssuedOn!.Value.AddHours(11).AddMinutes(1), $"Hours on {projectKeys} were invoiced",
                Invariant($"{invoice.Number}: {invoice.Lines.Sum(line => line.Hours):0.##} h, up to {invoice.HoursUpTo:MMM d}."), Invariant($"{SiteBase}/projects/{invoice.Lines[0].ProjectId}"), false));
        }
    }

    /// <summary>
    /// The hours, weeks, and invoices of a fresh tab. Hours are deterministic: each weekday a person splits a 7.5 or 8 hour
    /// day over the demo projects they're on (from each project's start), with a short non-billable team retro on Fridays.
    /// </summary>
    private static (List<PbTimeEntry> Entries, List<PbTimesheet> Timesheets, List<PbInvoice> Invoices) Timesheets(IReadOnlyList<PbPerson> people, IReadOnlyList<PbClient> clients,
        IReadOnlyList<PbProject> projects, IReadOnlyList<PbTask> tasks, IReadOnlyList<PbRate> rates)
    {
        var thisWeek = WeekStart(Now.Date);
        var peopleById = people.ToDictionary(person => person.Id);
        var demoProjects = projects.Where(project => project.Id is 1 or 2 or 3).ToList();
        var entries = new List<PbTimeEntry>();
        var timesheets = new List<PbTimesheet>();
        var nextEntryId = 1;

        void Add(string personId, int projectId, DateTime date, decimal hours, bool billable, string note, int seed)
        {
            var own = tasks.Where(task => task.ProjectId == projectId && task.AssigneeId == personId && task.CreatedAt.Date <= date).ToList();
            entries.Add(new PbTimeEntry
            {
                Id = nextEntryId++, PersonId = personId, ProjectId = projectId, Date = date, Hours = hours, IsBillable = billable, Note = note,
                TaskId = billable && own.Count > 0 && Pick(seed, 41, 3) != 0 ? own[Pick(seed, 42, own.Count)].Id : null, CreatedAt = date.AddHours(17).AddMinutes(Pick(seed, 43, 50)),
            });
        }

        for (var week = -6; week <= -1; week++)
        {
            var start = thisWeek.AddDays(7 * week);

            for (var index = 0; index < timesheetPeople.Length; index++)
            {
                var personId = timesheetPeople[index];

                for (var day = 0; day < 5; day++)
                {
                    var date = start.AddDays(day);
                    var available = demoProjects.Where(project => project.MemberIds.Contains(personId) && project.StartDate <= date).ToList();

                    if (available.Count == 0)
                        continue;

                    var seed = (week + 10) * 100 + index * 10 + day;
                    var total = Pick(seed, 44, 3) == 0 ? 7.5m : 8m;
                    var first = available[(day + index) % available.Count];
                    var note = entryNotes[Pick(seed, 45, entryNotes.Length)];

                    if (available.Count == 1)
                    {
                        Add(personId, first.Id, date, total, true, note, seed);
                    }
                    else
                    {
                        var second = available[(day + index + 1) % available.Count];
                        var split = Math.Round(total * (Pick(seed, 46, 3) + 4) / 8m * 4m, MidpointRounding.AwayFromZero) / 4m;
                        Add(personId, first.Id, date, split, true, note, seed);
                        Add(personId, second.Id, date, total - split, true, entryNotes[Pick(seed, 47, entryNotes.Length)], seed + 5000);
                    }

                    if (day == 4)
                        Add(personId, first.Id, date, 0.5m, false, "Team retro", seed + 9000);
                }
            }
        }

        // Jordan's current week, in draft: Monday so far.
        Add(SeededMemberId, 1, thisWeek, 4.5m, true, "Gallery zoom and size picker", 1);
        Add(SeededMemberId, 2, thisWeek, 3m, true, "Slot list", 2);
        entries[^2].TaskId = 5;
        entries[^1].TaskId = 14;

        var nextSheetId = 1;

        foreach (var group in entries.GroupBy(entry => (entry.PersonId, Week: WeekStart(entry.Date))).OrderBy(group => group.Key.Week).ThenBy(group => Array.IndexOf(timesheetPeople, group.Key.PersonId)))
        {
            var (personId, start) = group.Key;
            var sheet = new PbTimesheet { Id = nextSheetId++, PersonId = personId, WeekStart = start };
            var projectIds = group.Select(entry => entry.ProjectId).Distinct().Order().ToList();
            var seed = sheet.Id;

            if (start == thisWeek)
            {
                sheet.Status = PbTimesheetStatus.Draft;
            }
            else if (start == thisWeek.AddDays(-7) && personId == "ben")
            {
                sheet.Status = PbTimesheetStatus.Returned;
                sheet.SubmittedAt = start.AddDays(4).AddHours(17).AddMinutes(20);
                sheet.DecidedAt = thisWeek.AddHours(10).AddMinutes(15);
                sheet.ReturnedById = SeededProjectManagerId;
                sheet.ReturnReason = "Wednesday's hours on APP look high for the booking tests. Check them and move any WEB work to WEB, then submit again.";
            }
            else if (start == thisWeek.AddDays(-7))
            {
                sheet.Status = PbTimesheetStatus.Submitted;
                sheet.SubmittedAt = start.AddDays(4).AddHours(16).AddMinutes(30 + Pick(seed, 48, 60));
                sheet.Approvals = projectIds.Select(id => new PbProjectApproval { ProjectId = id }).ToList();
            }
            else
            {
                var decided = start.AddDays(7).AddHours(10).AddMinutes(Pick(seed, 49, 40));
                sheet.Status = PbTimesheetStatus.Approved;
                sheet.SubmittedAt = start.AddDays(4).AddHours(16).AddMinutes(30 + Pick(seed, 48, 60));
                sheet.DecidedAt = decided;
                sheet.Approvals = projectIds.Select(id => new PbProjectApproval { ProjectId = id, ApprovedById = SeededProjectManagerId, ApprovedAt = decided }).ToList();
            }

            timesheets.Add(sheet);
        }

        var invoices = new List<PbInvoice>();
        var nextLineId = 1;

        // One invoice for each completed project of the other clients, from before time tracking: paid, or issued in the last month.
        foreach (var project in projects.Where(project => project.Id > 4 && project.Status == PbProjectStatus.Completed && project.ClientId is not null && project.BudgetHours > 0)
                     .OrderBy(project => project.DueDate).ThenBy(project => project.Id))
        {
            var client = clients.First(item => item.Id == project.ClientId);
            var issued = project.DueDate.Date.AddDays(3 + Pick(project.Id, 50, 8));

            if (issued > Now.Date.AddDays(-1))
                continue;

            var hours = Math.Round(project.BudgetHours * (85 + Pick(project.Id, 51, 21)) / 100m);
            var rate = Math.Clamp(project.BudgetAmount / project.BudgetHours, 50, 400);
            var paid = Min(issued.AddDays(10 + Pick(project.Id, 52, 25)), Now.Date.AddDays(-1));
            var isPaid = issued <= Now.Date.AddDays(-30);
            invoices.Add(new PbInvoice
            {
                Id = invoices.Count + 1, ClientId = client.Id, Currency = client.BillingCurrency, CreatedAt = issued.AddDays(-1).AddHours(15), CreatedById = client.OwnerId,
                HoursUpTo = project.DueDate.Date, IsImported = true, IssuedOn = issued, TermsDays = 30, DueOn = issued.AddDays(30),
                Status = isPaid ? PbInvoiceStatus.Paid : PbInvoiceStatus.Issued, PaidOn = isPaid ? paid : null,
                Lines = new List<PbInvoiceLine>
                {
                    new() { Id = nextLineId++, ProjectId = project.Id, PersonId = null, Role = PbRateRole.Engineering, Hours = hours, Rate = rate, Amount = Math.Round(hours * rate, 2) },
                },
            });
        }

        // The demo projects' invoices from the approved hours: Meridian's first weeks (paid), Harbor & Pine's first week (overdue), and Meridian's next week (issued).
        PbInvoice FromHours(int clientId, int projectId, DateTime from, DateTime upTo, DateTime issued, int terms, DateTime? paid)
        {
            var client = clients.First(item => item.Id == clientId);
            var covered = entries.Where(entry => entry.ProjectId == projectId && entry.IsBillable && entry.Date >= from && entry.Date <= upTo).ToList();
            var invoice = new PbInvoice
            {
                Id = invoices.Count + 1, ClientId = clientId, Currency = client.BillingCurrency, CreatedAt = issued.AddDays(-1).AddHours(10), CreatedById = SeededAccountManagerId,
                HoursUpTo = upTo, IssuedOn = issued, TermsDays = terms, DueOn = issued.AddDays(terms), Status = paid is null ? PbInvoiceStatus.Issued : PbInvoiceStatus.Paid, PaidOn = paid,
                Lines = LinesFor(covered, peopleById, rates, ref nextLineId),
            };

            foreach (var entry in covered)
                entry.InvoiceId = invoice.Id;

            invoices.Add(invoice);

            return invoice;
        }

        var weekOf = thisWeek.AddDays(-28);
        FromHours(2, 2, thisWeek.AddDays(-42), weekOf.AddDays(-1), weekOf.AddDays(1), 30, weekOf.AddDays(18));
        FromHours(1, 1, weekOf, weekOf.AddDays(6), weekOf.AddDays(8), 14, null);
        FromHours(2, 2, weekOf, weekOf.AddDays(6), weekOf.AddDays(8), 30, null);

        // Numbers in issue order, a sequence per year.
        var sequence = new Dictionary<int, int>();

        foreach (var invoice in invoices.Where(invoice => invoice.IssuedOn is not null).OrderBy(invoice => invoice.IssuedOn).ThenBy(invoice => invoice.Id))
        {
            var year = invoice.IssuedOn!.Value.Year;
            sequence[year] = sequence.GetValueOrDefault(year) + 1;
            invoice.Number = InvoiceNumber(year, sequence[year]);
        }

        return (entries, timesheets, invoices);
    }

    #endregion

    #endregion
}
