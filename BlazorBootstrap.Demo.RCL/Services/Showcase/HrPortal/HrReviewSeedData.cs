using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

/// <summary>
/// The performance review cycles a fresh tab starts with (the signature workflow, AC-7b): the mid-year cycle, closed in
/// June with the whole company's ratings and sign-offs; the year-end cycle for Engineering and Product, open since
/// November 2, where Theo's self review is waiting, Rafael has two of his reports' reviews to write, and other teams are
/// part way through; and the year-end cycle for Sales, in calibration with every manager review in. Ratings, texts, and
/// dates come from small word lists through <see cref="Mix(int)" />, so every tab gets the same history.
/// </summary>
public static class HrReviewSeedData
{
    #region Fields and Constants

    /// <summary>The closed mid-year cycle.</summary>
    public const int MidYearCycleId = 1;

    /// <summary>The open year-end cycle for Engineering and Product.</summary>
    public const int YearEndCycleId = 2;

    /// <summary>The year-end cycle for Sales, in calibration.</summary>
    public const int SalesCycleId = 3;

    private static readonly string[] growth =
    {
        "lead a project from start to finish", "get better at estimating my work", "mentor someone who joins the team", "present our work to customers",
        "learn more about the hardware side", "write down how we make decisions", "take on more of the planning",
    };

    private static readonly string[] projects =
    {
        "fleet dashboard", "pick-path optimizer", "spring customer launch", "warehouse pilot in Porto", "new hiring plan", "pricing review", "firmware update rollout",
        "onboarding revamp", "support handbook", "quarterly forecast",
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The cycles in a fresh tab, oldest first.</summary>
    public static List<HrReviewCycle> Cycles() => new()
    {
        new()
        {
            Id = MidYearCycleId, Name = "2026 mid-year reviews", CreatedAt = new DateTime(2026, 5, 4, 9, 30, 0), CreatedById = HrSeedData.SeededHrAdminId,
            SelfReviewDue = new DateTime(2026, 5, 22), ManagerReviewDue = new DateTime(2026, 6, 5), Status = HrCycleStatus.Closed,
            CalibrationAt = new DateTime(2026, 6, 8, 10, 0, 0), SignOffAt = new DateTime(2026, 6, 15, 10, 0, 0), ClosedAt = new DateTime(2026, 6, 30, 16, 0, 0),
        },
        new()
        {
            Id = YearEndCycleId, Name = "2026 year-end reviews: Engineering and Product", DepartmentIds = new() { "engineering", "product" }, CreatedAt = new DateTime(2026, 11, 2, 9, 0, 0),
            CreatedById = HrSeedData.SeededHrAdminId, SelfReviewDue = new DateTime(2026, 11, 27), ManagerReviewDue = new DateTime(2026, 12, 11), Status = HrCycleStatus.Open,
        },
        new()
        {
            Id = SalesCycleId, Name = "2026 year-end reviews: Sales", DepartmentIds = new() { "sales" }, CreatedAt = new DateTime(2026, 10, 12, 9, 0, 0),
            CreatedById = HrSeedData.SeededHrAdminId, SelfReviewDue = new DateTime(2026, 10, 30), ManagerReviewDue = new DateTime(2026, 11, 13), Status = HrCycleStatus.Calibration,
            CalibrationAt = new DateTime(2026, 11, 16, 10, 0, 0),
        },
    };

    /// <summary>Whether someone takes part in a cycle opened on a day: they work here (started by then, not left by its close) and have a manager.</summary>
    public static bool IsParticipant(HrEmployee employee, HrReviewCycle cycle)
    {
        var lastDay = cycle.ClosedAt?.Date ?? HrSeedData.Now.Date;

        return employee.ManagerId is not null && employee.StartDate.Date <= cycle.CreatedAt.Date && (employee.EndDate is not { } end || end.Date >= lastDay)
               && (cycle.DepartmentIds.Count == 0 || cycle.DepartmentIds.Contains(employee.DepartmentId));
    }

    /// <summary>
    /// The reviews in a fresh tab, numbered from 1: everyone who took part in each cycle, at the stage the cycle's story needs.
    /// </summary>
    public static List<HrReview> Reviews()
    {
        var employees = HrSeedData.Employees();
        var list = new List<HrReview>();
        var id = 1;

        foreach (var cycle in Cycles())
        {
            foreach (var person in employees.Where(item => IsParticipant(item, cycle)).OrderBy(item => item.Id))
            {
                var review = new HrReview { Id = id++, CycleId = cycle.Id, EmployeeId = person.Id, ManagerId = person.ManagerId!.Value };
                var seed = (person.Id * 31) + (cycle.Id * 1009);
                var first = FirstName(person.Name);

                switch (cycle.Id)
                {
                    case MidYearCycleId:
                        FillMidYear(review, cycle, person, seed, first);
                        break;

                    case SalesCycleId:
                        SelfReview(review, seed, cycle.CreatedAt.AddDays(3 + (int)(Mix(seed) % 14)));
                        ManagerReview(review, seed, first, cycle.SelfReviewDue.AddDays(1 + (int)(Mix(seed * 3) % 12)).AddHours(10));

                        // HR has looked at about a third of the group: most ratings confirmed, two moved.
                        if (Mix(seed * 5) % 3 == 0)
                            Calibrate(review, seed, cycle.CalibrationAt!.Value.AddDays(1 + (int)(Mix(seed * 7) % 6)).AddHours(1));

                        break;

                    default:
                        FillYearEnd(review, cycle, person, seed, first);
                        break;
                }

                list.Add(review);
            }
        }

        // Seeded manager reviews were written by the managers themselves.
        foreach (var review in list.Where(item => item.ManagerSubmittedAt is not null))
            review.ManagerSubmittedById = review.ManagerId;

        foreach (var review in list.Where(item => item.ManagerSignedAt is not null))
            review.ManagerSignedById = review.ManagerId;

        return list;
    }

    /// <summary>
    /// The audit log's lines about the seeded cycles: opening them, the self and manager reviews, the move to calibration,
    /// calibrations, the release, sign-offs, and the close.
    /// </summary>
    public static IEnumerable<(DateTime At, string Actor, string Action, string Target, string? Url, string? Details)> AuditLines(IReadOnlyList<HrEmployee> employees,
        IReadOnlyList<HrReviewCycle> cycles, IReadOnlyList<HrReview> reviews)
    {
        var byId = employees.ToDictionary(item => item.Id);
        string NameOf(int? employeeId) => employeeId is { } key && byId.TryGetValue(key, out var person) ? person.Name : HrSeedData.ProductName;
        var hr = NameOf(HrSeedData.SeededHrAdminId);

        foreach (var cycle in cycles)
        {
            var target = CycleTarget(cycle);
            var url = CycleUrl(cycle.Id);
            var mine = reviews.Where(item => item.CycleId == cycle.Id).ToList();

            yield return (cycle.CreatedAt, hr, "Opened a review cycle", target, url, string.Create(CultureInfo.InvariantCulture, $"{mine.Count} people; self reviews due {Day(cycle.SelfReviewDue)}"));

            if (cycle.CalibrationAt is { } calibration)
                yield return (calibration, hr, "Moved to calibration", target, url, string.Create(CultureInfo.InvariantCulture, $"All {mine.Count} manager reviews in"));

            if (cycle.SignOffAt is { } signOff)
                yield return (signOff, hr, "Released for sign-off", target, url, string.Create(CultureInfo.InvariantCulture, $"{mine.Count} reviews"));

            if (cycle.ClosedAt is { } closed)
                yield return (closed, hr, "Closed", target, url, string.Create(CultureInfo.InvariantCulture, $"{mine.Count(item => item.EmployeeSignedAt is null)} not signed off by the employee"));

            // The closed cycle's single steps are long past; the log keeps the recent ones.
            if (cycle.Status == HrCycleStatus.Closed)
                continue;

            foreach (var review in mine)
            {
                var reviewTarget = ReviewTarget(NameOf(review.EmployeeId), cycle);
                var reviewUrl = ReviewUrl(review.Id);

                if (review.SelfSubmittedAt is { } self)
                    yield return (self, NameOf(review.EmployeeId), "Submitted a self review", reviewTarget, reviewUrl, review.SelfRating is { } own ? $"Own rating {RatingText(own)}" : null);

                if (review.ManagerSubmittedAt is { } manager)
                    yield return (manager, NameOf(review.ManagerSubmittedById), "Submitted a manager review", reviewTarget, reviewUrl, $"Rating {RatingText(review.ManagerRating!.Value)}");

                if (review.CalibratedAt is { } calibrated)
                    yield return (calibrated, hr, "Calibrated", reviewTarget, reviewUrl, review.CalibratedRating == review.ManagerRating
                        ? $"Kept {RatingText(review.CalibratedRating!.Value)}"
                        : $"Rating {review.ManagerRating} → {RatingText(review.CalibratedRating!.Value)}; {review.CalibrationNote}");
            }
        }
    }

    /// <summary>
    /// The seeded notifications about reviews, per account: the self reviews waiting, the self reviews in for the manager,
    /// and, for the HR admin, the cycle whose manager reviews are all in.
    /// </summary>
    public static IEnumerable<(string AccountId, DateTime At, string Title, string? Text, string Url)> NotificationLines(IReadOnlyList<HrEmployee> employees,
        IReadOnlyList<HrReviewCycle> cycles, IReadOnlyList<HrReview> reviews)
    {
        var byId = employees.ToDictionary(item => item.Id);

        foreach (var cycle in cycles.Where(item => item.Status is HrCycleStatus.Open or HrCycleStatus.Calibration))
        {
            foreach (var review in reviews.Where(item => item.CycleId == cycle.Id))
            {
                var employee = byId[review.EmployeeId];
                var manager = byId[review.ManagerId];

                if (employee.AccountId is { } employeeAccount)
                    yield return (employeeAccount, cycle.CreatedAt, "Your self review is waiting", $"{cycle.Name}: due {Day(cycle.SelfReviewDue)}", ReviewUrl(review.Id));

                if (review.SelfSubmittedAt is { } self && manager.AccountId is { } managerAccount)
                    yield return (managerAccount, self, $"{employee.Name}'s self review is in", $"{cycle.Name}: write your review by {Day(cycle.ManagerReviewDue)}", ReviewUrl(review.Id));
            }

            var last = reviews.Where(item => item.CycleId == cycle.Id).Select(item => item.ManagerSubmittedAt).ToList();

            if (last.Count > 0 && last.All(at => at is not null) && byId[HrSeedData.SeededHrAdminId].AccountId is { } hrAccount)
                yield return (hrAccount, last.Max()!.Value, $"All manager reviews are in for {cycle.Name}", "Move it to calibration when you're ready.", CycleUrl(cycle.Id));
        }
    }

    /// <summary>The name of a rating, like "3 Meets expectations".</summary>
    public static string RatingText(int rating) => string.Create(CultureInfo.InvariantCulture, $"{rating} {HrState.RatingName(rating)}");

    /// <summary>The HR admin's page of a cycle.</summary>
    public static string CycleUrl(int cycleId) => FormattableString.Invariant($"{DemoRouteConstants.Showcase_URL_HrPortal}/admin/reviews/{cycleId}");

    /// <summary>The page of one review.</summary>
    public static string ReviewUrl(int reviewId) => FormattableString.Invariant($"{DemoRouteConstants.Showcase_URL_HrPortal}/reviews/{reviewId}");

    /// <summary>The audit log's name for a cycle, like "Review cycle: 2026 year-end reviews".</summary>
    public static string CycleTarget(HrReviewCycle cycle) => $"Review cycle: {cycle.Name}";

    /// <summary>The audit log's name for one review, like "Review: Leila Haddad (2026 year-end reviews)".</summary>
    public static string ReviewTarget(string employeeName, HrReviewCycle cycle) => $"Review: {employeeName} ({cycle.Name})";

    #endregion

    #region Private Methods

    private static string Day(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Sets the calibrated rating: mostly the manager's; one in five moves a step toward "Meets expectations", with a note.</summary>
    private static void Calibrate(HrReview review, int seed, DateTime at)
    {
        var rating = review.ManagerRating!.Value;
        var moves = Mix(seed * 11) % 5 == 0 && rating != 3;

        review.CalibratedRating = moves ? rating > 3 ? rating - 1 : rating + 1 : rating;
        review.CalibrationNote = !moves ? string.Empty
            : rating > 3 ? "Lowered to match the group: the goals were met in full, but not beyond what the plan asked for."
            : "Raised after comparing the group: the impact on the team was larger than the review shows.";
        review.CalibratedAt = at;
        review.CalibratedById = HrSeedData.SeededHrAdminId;
    }

    /// <summary>The closed mid-year cycle: every review written, calibrated, released, and signed off by the manager; a few employees never signed.</summary>
    private static void FillMidYear(HrReview review, HrReviewCycle cycle, HrEmployee person, int seed, string first)
    {
        SelfReview(review, seed, cycle.CreatedAt.AddDays(2 + (int)(Mix(seed) % 16)));
        ManagerReview(review, seed, first, cycle.SelfReviewDue.AddDays(1 + (int)(Mix(seed * 3) % 13)).AddHours(11));
        Calibrate(review, seed, cycle.CalibrationAt!.Value.AddDays(1 + (int)(Mix(seed * 7) % 5)).AddHours(2));
        review.ManagerSignedAt = cycle.SignOffAt!.Value.AddDays(1 + (int)(Mix(seed * 13) % 5)).AddHours(3);

        if (Mix(seed * 17) % 25 != 0)
        {
            review.EmployeeSignedAt = cycle.SignOffAt.Value.AddDays(1 + (int)(Mix(seed * 19) % 10)).AddHours(5);
            review.EmployeeComment = Mix(seed * 23) % 6 == 0 ? "Thanks for the honest feedback; I agree with the goals for next half." : string.Empty;
        }
    }

    /// <summary>
    /// The open year-end cycle: Theo hasn't written his self review; Rafael, Leila, and Mei have, and wait for a manager
    /// review; Jonas is reviewed. The rest of the group is part way: about seven in ten self reviews are in, and about half
    /// of those have their manager's review.
    /// </summary>
    private static void FillYearEnd(HrReview review, HrReviewCycle cycle, HrEmployee person, int seed, string first)
    {
        DateTime? self = person.Id switch
        {
            HrSeedData.SeededEmployeeId => null,
            HrSeedData.SeededManagerId => new DateTime(2026, 11, 18, 14, 10, 0),
            5 => new DateTime(2026, 11, 16, 11, 20, 0),
            6 => new DateTime(2026, 11, 12, 16, 45, 0),
            7 => new DateTime(2026, 11, 19, 10, 5, 0),
            _ => Mix(seed) % 10 < 7 ? cycle.CreatedAt.AddDays(3 + (int)(Mix(seed * 3) % 18)).AddHours((int)(Mix(seed * 29) % 7)) : null,
        };

        if (self is not { } selfAt)
            return;

        SelfReview(review, seed, selfAt);

        DateTime? manager = person.Id switch
        {
            6 => new DateTime(2026, 11, 20, 15, 30, 0),
            HrSeedData.SeededManagerId or 5 or 7 => null,
            _ => person.ManagerId != HrSeedData.SeededManagerId && Mix(seed * 37) % 10 < 5 ? selfAt.AddDays(1 + (int)(Mix(seed * 41) % 4)).AddHours(2) : null,
        };

        if (manager is { } managerAt && managerAt <= HrSeedData.Now.AddHours(-1))
            ManagerReview(review, seed, first, managerAt);
    }

    private static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    /// <summary>The manager's review and rating: mostly "Meets" or "Exceeds expectations", a few at either end.</summary>
    private static void ManagerReview(HrReview review, int seed, string first, DateTime at)
    {
        var pick = (int)(Mix(seed * 43) % 100);
        var rating = pick switch { < 3 => 1, < 15 => 2, < 62 => 3, < 90 => 4, _ => 5 };
        var project = projects[Mix(seed * 47) % (ulong)projects.Length];

        review.ManagerRating = rating;
        review.ManagerText = rating switch
        {
            1 => $"{first} missed several of the goals we agreed, including the {project}. We set up a support plan with weekly check-ins.",
            2 => $"{first} met some goals, but the {project} slipped. Let's focus on planning and on asking for help earlier.",
            3 => $"{first} delivered what we agreed, including the {project}. Solid, reliable work; next, let's grow the scope.",
            4 => $"{first} went beyond the plan: the {project} landed early, and teammates rely on {first}'s reviews and advice.",
            _ => $"{first} had an exceptional half: the {project} changed how the whole team works and raised the bar for everyone.",
        };
        review.ManagerSubmittedAt = at;
    }

    /// <summary>A deterministic number from 0 to 999 for a seed value.</summary>
    private static ulong Mix(int value)
    {
        var x = (ulong)(value + 0x9E37) * 0x9E3779B97F4A7C15UL;
        x ^= x >> 29;

        return x % 1000;
    }

    /// <summary>A short self review, with an own rating about two times in three.</summary>
    private static void SelfReview(HrReview review, int seed, DateTime at)
    {
        var project = projects[Mix(seed * 53) % (ulong)projects.Length];
        var next = growth[Mix(seed * 59) % (ulong)growth.Length];

        review.SelfText = (Mix(seed * 61) % 3) switch
        {
            0 => $"I delivered my part of the {project} on time and wrote the runbook the team uses now. Next I want to {next}.",
            1 => $"I took over the {project} after a reorg and got it back on schedule with the team. Next I want to {next}.",
            _ => $"Most of my time went into the {project}; it shipped, with a few late fixes I learned from. Next I want to {next}.",
        };
        review.SelfRating = Mix(seed * 67) % 3 == 0 ? null : (int)(3 + (Mix(seed * 71) % 2));
        review.SelfSubmittedAt = at;
    }

    #endregion

    #endregion
}
