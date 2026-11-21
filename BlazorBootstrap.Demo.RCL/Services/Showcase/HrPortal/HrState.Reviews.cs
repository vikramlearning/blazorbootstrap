using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

/// <summary>
/// The performance review cycle, Teamhaven's signature workflow (AC-7b), across the three roles:
/// <list type="number">
/// <item>The HR admin opens a cycle for the whole company or some departments (<see cref="OpenCycle" />). Everyone in scope
/// who works here and has a manager gets a review; the manager is the one they have that day. The chief executive, with
/// no manager, isn't reviewed in the portal.</item>
/// <item>Each employee writes a self review (<see cref="SubmitSelfReview" />).</item>
/// <item>The manager writes a review with a rating from 1 to 5 (<see cref="SubmitManagerReview" />), once the self review is
/// in, or without it when the employee can't sign in or the self-review due date has passed. For a manager who can't sign
/// in to Teamhaven, the HR admin records the review (and later the sign-off) for them.</item>
/// <item>The HR admin moves the cycle to calibration (<see cref="MoveToCalibration" />) once every manager review is in, or
/// leaves out the reviews still missing one after confirming who they are; then calibrates ratings across the group
/// (<see cref="Calibrate" />: a note is required when the rating changes).</item>
/// <item>The HR admin releases the cycle for sign-off (<see cref="ReleaseForSignOff" />); ratings nobody calibrated keep the
/// manager's. The employee (with an optional comment) and the manager each sign off (<see cref="SignOff" />).</item>
/// <item>The HR admin closes the cycle (<see cref="CloseCycle" />) once everyone signed, or after confirming who didn't.</item>
/// </list>
/// Every step writes an audit entry, and whoever must act next is notified. Only one cycle that isn't closed may cover a
/// department at a time.
/// </summary>
public sealed partial class HrState
{
    #region Fields and Constants

    /// <summary>The longest calibration note.</summary>
    public const int MaxCalibrationNoteLength = 300;

    /// <summary>The longest review cycle name.</summary>
    public const int MaxCycleNameLength = 80;

    /// <summary>The most review cycles kept.</summary>
    public const int MaxCycles = 30;

    /// <summary>How far ahead the manager-review due date may be, in days.</summary>
    public const int MaxReviewDueDays = 180;

    /// <summary>The longest self review or manager review.</summary>
    public const int MaxReviewTextLength = 2000;

    /// <summary>The longest sign-off comment.</summary>
    public const int MaxSignOffCommentLength = 500;

    /// <summary>The shortest calibration note, when the rating changes.</summary>
    public const int MinCalibrationNoteLength = 10;

    /// <summary>The shortest self review or manager review.</summary>
    public const int MinReviewTextLength = 50;

    private const int MaxReviews = 1500;

    /// <summary>The HR admin's performance reviews page.</summary>
    private const string ReviewCyclesLink = SiteBase + "/admin/reviews";

    /// <summary>The manager's team reviews page.</summary>
    private const string TeamReviewsLink = SiteBase + "/reviews/team";

    private readonly List<HrReviewCycle> cycles = HrReviewSeedData.Cycles();

    private readonly List<HrReview> reviews = HrReviewSeedData.Reviews();

    private int nextCycleId;

    private int nextReviewId;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The name of a rating from 1 to 5, like "Meets expectations".</summary>
    public static string RatingName(int rating) => rating switch
    {
        1 => "Below expectations",
        2 => "Partly meets expectations",
        3 => "Meets expectations",
        4 => "Exceeds expectations",
        5 => "Exceptional",
        _ => "Not rated",
    };

    /// <summary>
    /// Calibrates one review's rating (HR admin, while the cycle is in calibration; not your own review). Changing the
    /// manager's rating needs a note of at least <see cref="MinCalibrationNoteLength" /> characters (Invalid).
    /// </summary>
    public HrResult Calibrate(int reviewId, int rating, string note)
    {
        if (!IsHrAdmin || Me() is not { } me)
            return HrResult.NotAllowed;

        if (reviews.FirstOrDefault(item => item.Id == reviewId) is not { } review || FindCycleRecord(review.CycleId) is not { } cycle)
            return HrResult.NotFound;

        if (!CanCalibrate(review))
            return review.EmployeeId == me.Id ? HrResult.NotAllowed : HrResult.WrongState;

        note = note?.Trim() ?? string.Empty;

        if (rating is < 1 or > 5 || note.Length > MaxCalibrationNoteLength || (rating != review.ManagerRating && note.Length < MinCalibrationNoteLength))
            return HrResult.Invalid;

        if (review.CalibratedRating == rating && review.CalibrationNote == note)
            return HrResult.NoChange;

        var previous = (review.CalibratedRating, review.CalibrationNote, review.CalibratedAt, review.CalibratedById);
        (review.CalibratedRating, review.CalibrationNote, review.CalibratedAt, review.CalibratedById) = (rating, note, Clock.Now, me.Id);

        return Commit(() => (review.CalibratedRating, review.CalibrationNote, review.CalibratedAt, review.CalibratedById) = previous,
            () => Record("Calibrated", ReviewTarget(review, cycle), HrReviewSeedData.ReviewUrl(review.Id), rating == review.ManagerRating
                ? $"Kept {HrReviewSeedData.RatingText(rating)}" + (note.Length > 0 ? $"; {note}" : null)
                : $"Rating {review.ManagerRating} → {HrReviewSeedData.RatingText(rating)}; {note}"));
    }

    /// <summary>Whether the signed-in HR admin may calibrate a review now: the cycle is in calibration and it has a manager review.</summary>
    public bool CanCalibrate(HrReview review) =>
        IsHrAdmin && Me() is { } me && review.EmployeeId != me.Id && !review.IsLeftOut && review.ManagerRating is not null
        && FindCycleRecord(review.CycleId)?.Status == HrCycleStatus.Calibration;

    /// <summary>Whether the signed-in person may sign off a review as its employee now.</summary>
    public bool CanSignAsEmployee(HrReview review) =>
        Me() is { } me && review.EmployeeId == me.Id && !review.IsLeftOut && review.EmployeeSignedAt is null && FindCycleRecord(review.CycleId)?.Status == HrCycleStatus.SignOff;

    /// <summary>Whether the signed-in person may sign off a review for its manager now (the manager, or HR for a manager who can't sign in).</summary>
    public bool CanSignAsManager(HrReview review) =>
        IsManagerActor(review) && !review.IsLeftOut && review.ManagerSignedAt is null && FindCycleRecord(review.CycleId)?.Status == HrCycleStatus.SignOff;

    /// <summary>
    /// Whether the signed-in person may write a review's manager review now: they act for the manager, the cycle is open,
    /// and the self review is in, or the employee can't sign in, or the self-review due date has passed.
    /// </summary>
    public bool CanWriteManagerReview(HrReview review) =>
        IsManagerActor(review) && !review.IsLeftOut && review.ManagerSubmittedAt is null && FindCycleRecord(review.CycleId) is { Status: HrCycleStatus.Open } cycle
        && (review.SelfSubmittedAt is not null || !CanSignIn(review.EmployeeId) || Clock.Today > cycle.SelfReviewDue.Date);

    /// <summary>Whether the signed-in person may write their own self review now: the cycle is open, and no manager review is in yet.</summary>
    public bool CanWriteSelfReview(HrReview review) =>
        Me() is { } me && review.EmployeeId == me.Id && !review.IsLeftOut && review.SelfSubmittedAt is null && review.ManagerSubmittedAt is null
        && FindCycleRecord(review.CycleId)?.Status == HrCycleStatus.Open;

    /// <summary>
    /// Closes a cycle in sign-off (HR admin). With reviews not signed by both the employee and the manager, it needs
    /// <paramref name="closeUnsigned" /> (WrongState otherwise); <paramref name="unsigned" /> says how many.
    /// </summary>
    public HrResult CloseCycle(int cycleId, bool closeUnsigned, out int unsigned)
    {
        unsigned = 0;

        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        if (FindCycleRecord(cycleId) is not { } cycle)
            return HrResult.NotFound;

        if (cycle.Status != HrCycleStatus.SignOff)
            return HrResult.WrongState;

        var open = Included(cycle.Id).Where(review => review.EmployeeSignedAt is null || review.ManagerSignedAt is null).ToList();
        unsigned = open.Count;

        if (open.Count > 0 && !closeUnsigned)
            return HrResult.WrongState;

        cycle.Status = HrCycleStatus.Closed;
        cycle.ClosedAt = Clock.Now;
        var count = open.Count;

        return Commit(() =>
        {
            cycle.Status = HrCycleStatus.SignOff;
            cycle.ClosedAt = null;
        }, () => Record("Closed", HrReviewSeedData.CycleTarget(cycle), HrReviewSeedData.CycleUrl(cycle.Id), count == 0
            ? "Everyone signed off"
            : string.Create(CultureInfo.InvariantCulture, $"{count} not signed off: ") + Names(open.Select(review => review.EmployeeId))));
    }

    /// <summary>Returns a cycle's reviews the signed-in person may see: all of them for the HR admin, otherwise their own and their reports'.</summary>
    public IReadOnlyList<HrReview> CycleReviews(int cycleId) =>
        Me() is not { } me ? Array.Empty<HrReview>()
            : reviews.Where(review => review.CycleId == cycleId && (IsHrAdmin || review.EmployeeId == me.Id || review.ManagerId == me.Id)).ToList();

    /// <summary>
    /// Returns who would take part in a cycle for these departments (all of them when empty) opened today: people who work
    /// here, started by today, and have a manager; by name.
    /// </summary>
    public IReadOnlyList<HrEmployee> CycleParticipants(IReadOnlyCollection<string> departmentIds) =>
        employees.Where(item => item.Status == HrEmployeeStatus.Active && item.StartDate.Date <= Clock.Today && item.ManagerId is not null
                                && (departmentIds.Count == 0 || departmentIds.Contains(item.DepartmentId)))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>Returns the people in scope who are left out of a cycle opened today: joining, or without a manager.</summary>
    public IReadOnlyList<HrEmployee> CycleExclusions(IReadOnlyCollection<string> departmentIds) =>
        employees.Where(item => item.Status != HrEmployeeStatus.Left && (departmentIds.Count == 0 || departmentIds.Contains(item.DepartmentId))
                                && (item.ManagerId is null || item.Status == HrEmployeeStatus.Joining || item.StartDate.Date > Clock.Today))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>Returns the cycle that isn't closed and already covers one of these departments (or the whole company), or <see langword="null" />.</summary>
    public HrReviewCycle? CycleOverlap(IReadOnlyCollection<string> departmentIds) =>
        cycles.FirstOrDefault(cycle => cycle.Status != HrCycleStatus.Closed && Overlaps(cycle.DepartmentIds, departmentIds));

    /// <summary>Counts a cycle's reviews by stage (HR admin, or anyone who takes part).</summary>
    public HrCycleProgress CycleProgress(int cycleId)
    {
        var mine = IsHrAdmin ? reviews.Where(review => review.CycleId == cycleId).ToList() : CycleReviews(cycleId).ToList();
        var stages = mine.Select(StageOf).ToList();

        return new HrCycleProgress(mine.Count, stages.Count(stage => stage == HrReviewStage.SelfReview), stages.Count(stage => stage == HrReviewStage.ManagerReview),
            stages.Count(stage => stage == HrReviewStage.Calibration), stages.Count(stage => stage == HrReviewStage.SignOff), stages.Count(stage => stage == HrReviewStage.Done),
            stages.Count(stage => stage == HrReviewStage.LeftOut), mine.Count(review => review.SelfSubmittedAt is not null), mine.Count(review => review.ManagerSubmittedAt is not null),
            mine.Count(review => review.CalibratedAt is not null), mine.Count(review => review.EmployeeSignedAt is not null && review.ManagerSignedAt is not null));
    }

    /// <summary>Finds a cycle the signed-in person may see (HR admin, or anyone who takes part), or returns <see langword="null" />.</summary>
    public HrReviewCycle? FindCycle(int cycleId) =>
        FindCycleRecord(cycleId) is { } cycle && (IsHrAdmin || CycleReviews(cycleId).Count > 0) ? cycle : null;

    /// <summary>Finds a review the signed-in person may see: their own, a report's (as the manager when the cycle opened), or any for HR.</summary>
    public HrReview? FindReview(int reviewId) =>
        Me() is { } me && reviews.FirstOrDefault(item => item.Id == reviewId) is { } review && (IsHrAdmin || review.EmployeeId == me.Id || review.ManagerId == me.Id) ? review : null;

    /// <summary>Whether the signed-in person acts for a review's manager: they are the manager (as a manager or HR admin), or HR for a manager who can't sign in.</summary>
    public bool IsManagerActor(HrReview review) =>
        Me() is { } me && review.EmployeeId != me.Id
        && ((review.ManagerId == me.Id && (IsManager || IsHrAdmin)) || (IsHrAdmin && !ManagerCanSignIn(review)));

    /// <summary>Whether a review's manager works here and can sign in as a manager or HR admin (otherwise HR acts for them).</summary>
    public bool ManagerCanSignIn(HrReview review) =>
        AccountOf(review.ManagerId) is { } accountId && Identity.FindAccount(accountId)?.RoleId is HrSeedData.ManagerRole or HrSeedData.HrAdminRole;

    /// <summary>
    /// Moves an open cycle to calibration (HR admin). With reviews still missing a manager review, it needs
    /// <paramref name="leaveOutMissing" /> (WrongState otherwise): those are left out of calibration and sign-off.
    /// <paramref name="leftOut" /> says how many. A cycle with no manager review at all can't move (WrongState).
    /// </summary>
    public HrResult MoveToCalibration(int cycleId, bool leaveOutMissing, out int leftOut)
    {
        leftOut = 0;

        if (!IsHrAdmin)
            return HrResult.NotAllowed;

        if (FindCycleRecord(cycleId) is not { } cycle)
            return HrResult.NotFound;

        var included = Included(cycle.Id).ToList();
        var missing = included.Where(review => review.ManagerSubmittedAt is null).ToList();
        leftOut = missing.Count;

        if (cycle.Status != HrCycleStatus.Open || missing.Count == included.Count || (missing.Count > 0 && !leaveOutMissing))
            return HrResult.WrongState;

        cycle.Status = HrCycleStatus.Calibration;
        cycle.CalibrationAt = Clock.Now;

        foreach (var review in missing)
            review.IsLeftOut = true;

        return Commit(() =>
        {
            cycle.Status = HrCycleStatus.Open;
            cycle.CalibrationAt = null;

            foreach (var review in missing)
                review.IsLeftOut = false;
        }, () => Record("Moved to calibration", HrReviewSeedData.CycleTarget(cycle), HrReviewSeedData.CycleUrl(cycle.Id), missing.Count == 0
            ? string.Create(CultureInfo.InvariantCulture, $"All {included.Count} manager reviews in")
            : string.Create(CultureInfo.InvariantCulture, $"{missing.Count} left out without a manager review: ") + Names(missing.Select(review => review.EmployeeId))));
    }

    /// <summary>Returns the signed-in person's own reviews, the newest cycle first.</summary>
    public IReadOnlyList<HrReview> MyReviews() =>
        Me() is not { } me ? Array.Empty<HrReview>()
            : reviews.Where(review => review.EmployeeId == me.Id).OrderByDescending(review => FindCycleRecord(review.CycleId)?.CreatedAt).ToList();

    /// <summary>
    /// Opens a review cycle (HR admin) for some departments, or the whole company when <paramref name="departmentIds" /> is
    /// empty. Invalid: a name of 1 to 80 characters, an unknown department, self reviews not due after today, manager reviews
    /// not due after the self reviews or more than <see cref="MaxReviewDueDays" /> days ahead, or the most cycles the demo
    /// keeps; Duplicate: the name is taken; Overlap: a cycle that isn't closed covers one of the departments; WrongState:
    /// nobody in scope can take part. Everyone in it who can sign in is told their self review is waiting.
    /// </summary>
    public HrResult OpenCycle(string name, IReadOnlyCollection<string> departmentIds, DateTime selfReviewDue, DateTime managerReviewDue, out HrReviewCycle? opened)
    {
        opened = null;

        if (!IsHrAdmin || Me() is not { } me)
            return HrResult.NotAllowed;

        name = name?.Trim() ?? string.Empty;
        var scope = departmentIds?.Distinct().ToList() ?? new List<string>();
        var today = Clock.Today;

        if (!IsText(name, MaxCycleNameLength) || scope.Any(id => FindDepartment(id) is null) || selfReviewDue.Date <= today || managerReviewDue.Date <= selfReviewDue.Date
            || managerReviewDue.Date > today.AddDays(MaxReviewDueDays) || cycles.Count >= MaxCycles || nextCycleId >= NextIdLimit)
            return HrResult.Invalid;

        if (cycles.Any(cycle => string.Equals(cycle.Name, name, StringComparison.OrdinalIgnoreCase)))
            return HrResult.Duplicate;

        if (CycleOverlap(scope) is not null)
            return HrResult.Overlap;

        var people = CycleParticipants(scope);

        if (people.Count == 0)
            return HrResult.WrongState;

        if (reviews.Count + people.Count > MaxReviews || nextReviewId + people.Count >= NextIdLimit)
            return HrResult.Invalid;

        var cycle = new HrReviewCycle
        {
            Id = nextCycleId++, Name = name, DepartmentIds = scope, SelfReviewDue = selfReviewDue.Date, ManagerReviewDue = managerReviewDue.Date, Status = HrCycleStatus.Open,
            CreatedAt = Clock.Now, CreatedById = me.Id,
        };
        var added = people.Select(person => new HrReview { Id = nextReviewId++, CycleId = cycle.Id, EmployeeId = person.Id, ManagerId = person.ManagerId!.Value }).ToList();
        cycles.Add(cycle);
        reviews.AddRange(added);
        var result = Commit(() =>
        {
            cycles.Remove(cycle);
            reviews.RemoveAll(added.Contains);
            nextCycleId--;
            nextReviewId -= added.Count;
        }, () =>
        {
            Record("Opened a review cycle", HrReviewSeedData.CycleTarget(cycle), HrReviewSeedData.CycleUrl(cycle.Id),
                string.Create(CultureInfo.InvariantCulture, $"{added.Count} people in {ScopeText(cycle)}; self reviews due {Day(cycle.SelfReviewDue)}, manager reviews {Day(cycle.ManagerReviewDue)}"));

            foreach (var review in added)
                Notify(AccountOf(review.EmployeeId), "Your self review is waiting", $"{cycle.Name}: due {Day(cycle.SelfReviewDue)}", HrReviewSeedData.ReviewUrl(review.Id));
        });

        if (result == HrResult.Ok)
            opened = cycle;

        return result;
    }

    /// <summary>
    /// Releases a cycle in calibration for sign-off (HR admin). Reviews nobody calibrated keep the manager's rating
    /// (<paramref name="kept" /> says how many). Each employee and manager who can sign in is told their sign-off is
    /// waiting; HR hears of the sign-offs it records for managers who can't sign in.
    /// </summary>
    public HrResult ReleaseForSignOff(int cycleId, out int kept)
    {
        kept = 0;

        if (!IsHrAdmin || Me() is not { } me)
            return HrResult.NotAllowed;

        if (FindCycleRecord(cycleId) is not { } cycle)
            return HrResult.NotFound;

        if (cycle.Status != HrCycleStatus.Calibration)
            return HrResult.WrongState;

        var included = Included(cycle.Id).ToList();
        var uncalibrated = included.Where(review => review.CalibratedAt is null).ToList();
        kept = uncalibrated.Count;
        var now = Clock.Now;

        foreach (var review in uncalibrated)
            (review.CalibratedRating, review.CalibratedAt, review.CalibratedById, review.CalibrationNote) = (review.ManagerRating, now, me.Id, string.Empty);

        cycle.Status = HrCycleStatus.SignOff;
        cycle.SignOffAt = now;
        var keptCount = kept;

        return Commit(() =>
        {
            cycle.Status = HrCycleStatus.Calibration;
            cycle.SignOffAt = null;

            foreach (var review in uncalibrated)
                (review.CalibratedRating, review.CalibratedAt, review.CalibratedById) = (null, null, null);
        }, () =>
        {
            Record("Released for sign-off", HrReviewSeedData.CycleTarget(cycle), HrReviewSeedData.CycleUrl(cycle.Id),
                string.Create(CultureInfo.InvariantCulture, $"{included.Count} reviews; {keptCount} kept the manager's rating"));

            foreach (var review in included)
                Notify(AccountOf(review.EmployeeId), "Your review is ready to sign off", $"{cycle.Name}: read your manager's review and your rating", HrReviewSeedData.ReviewUrl(review.Id));

            foreach (var group in included.Where(ManagerCanSignIn).GroupBy(review => review.ManagerId))
                Notify(AccountOf(group.Key), group.Count() == 1 ? $"{NameOf(group.First().EmployeeId)}'s review is ready for your sign-off" : $"{group.Count()} reviews are ready for your sign-off",
                    cycle.Name, group.Count() == 1 ? HrReviewSeedData.ReviewUrl(group.First().Id) : TeamReviewsLink);

            var forHr = included.Count(review => !ManagerCanSignIn(review));

            if (forHr > 0)
                foreach (var accountId in HrAdminAccounts(me.Id))
                    Notify(accountId, string.Create(CultureInfo.InvariantCulture, $"{forHr} manager sign-offs to record"), $"{cycle.Name}: for managers who can't sign in", HrReviewSeedData.CycleUrl(cycle.Id));
        });
    }

    /// <summary>
    /// What's waiting for the signed-in person in the review cycles, most urgent first: their self review or sign-off, the
    /// manager reviews and sign-offs they owe, and, for the HR admin, cycles ready to move on and reviews to record for
    /// managers who can't sign in.
    /// </summary>
    public IReadOnlyList<HrReviewTodo> ReviewTodos()
    {
        if (Me() is not { } me)
            return Array.Empty<HrReviewTodo>();

        var todos = new List<HrReviewTodo>();

        foreach (var review in reviews.Where(review => review.EmployeeId == me.Id))
        {
            var cycle = FindCycleRecord(review.CycleId)!;

            if (CanWriteSelfReview(review))
                todos.Add(new HrReviewTodo($"Write your self review, due {Day(cycle.SelfReviewDue)}", cycle.Name, HrReviewSeedData.ReviewUrl(review.Id), cycle.SelfReviewDue));
            else if (CanSignAsEmployee(review))
                todos.Add(new HrReviewTodo("Read your review and sign off", cycle.Name, HrReviewSeedData.ReviewUrl(review.Id), cycle.SignOffAt ?? Clock.Now));
        }

        foreach (var review in reviews.Where(review => review.ManagerId == me.Id && review.EmployeeId != me.Id && (IsManager || IsHrAdmin)))
        {
            var cycle = FindCycleRecord(review.CycleId)!;

            if (CanWriteManagerReview(review))
                todos.Add(new HrReviewTodo($"Review {NameOf(review.EmployeeId)}, due {Day(cycle.ManagerReviewDue)}", cycle.Name, HrReviewSeedData.ReviewUrl(review.Id), cycle.ManagerReviewDue));
            else if (CanSignAsManager(review))
                todos.Add(new HrReviewTodo($"Sign off {NameOf(review.EmployeeId)}'s review", cycle.Name, HrReviewSeedData.ReviewUrl(review.Id), cycle.SignOffAt ?? Clock.Now));
        }

        if (IsHrAdmin)
        {
            foreach (var cycle in cycles.Where(cycle => cycle.Status != HrCycleStatus.Closed))
            {
                var included = Included(cycle.Id).ToList();
                var url = HrReviewSeedData.CycleUrl(cycle.Id);

                switch (cycle.Status)
                {
                    case HrCycleStatus.Open when included.Count > 0 && included.All(review => review.ManagerSubmittedAt is not null):
                        todos.Add(new HrReviewTodo("Every manager review is in: move it to calibration", cycle.Name, url, cycle.ManagerReviewDue));
                        break;

                    case HrCycleStatus.Open:
                        var toRecord = included.Count(review => review.EmployeeId != me.Id && review.ManagerSubmittedAt is null && !ManagerCanSignIn(review));

                        if (toRecord > 0)
                            todos.Add(new HrReviewTodo(string.Create(CultureInfo.InvariantCulture, $"Record {toRecord} manager {(toRecord == 1 ? "review" : "reviews")} for managers who can't sign in"),
                                cycle.Name, url, cycle.ManagerReviewDue));

                        break;

                    case HrCycleStatus.Calibration:
                        todos.Add(new HrReviewTodo(string.Create(CultureInfo.InvariantCulture,
                            $"Calibrate the ratings ({included.Count(review => review.CalibratedAt is not null)} of {included.Count} done) and release for sign-off"), cycle.Name, url, cycle.CalibrationAt ?? Clock.Now));
                        break;

                    case HrCycleStatus.SignOff when included.All(review => review.EmployeeSignedAt is not null && review.ManagerSignedAt is not null):
                        todos.Add(new HrReviewTodo("Everyone signed off: close the cycle", cycle.Name, url, cycle.SignOffAt ?? Clock.Now));
                        break;

                    case HrCycleStatus.SignOff:
                        var managerSigns = included.Count(review => review.EmployeeId != me.Id && review.ManagerSignedAt is null && !ManagerCanSignIn(review));

                        if (managerSigns > 0)
                            todos.Add(new HrReviewTodo(string.Create(CultureInfo.InvariantCulture, $"Record {managerSigns} manager {(managerSigns == 1 ? "sign-off" : "sign-offs")} for managers who can't sign in"),
                                cycle.Name, url, cycle.SignOffAt ?? Clock.Now));

                        break;
                }
            }
        }

        return todos.OrderBy(todo => todo.Due).ToList();
    }

    /// <summary>Gets the review cycles the signed-in person may see, the newest first: every cycle for the HR admin, otherwise the ones they take part in.</summary>
    public IReadOnlyList<HrReviewCycle> ReviewCycles() =>
        Me() is not { } me ? Array.Empty<HrReviewCycle>()
            : cycles.Where(cycle => IsHrAdmin || reviews.Any(review => review.CycleId == cycle.Id && (review.EmployeeId == me.Id || review.ManagerId == me.Id)))
                .OrderByDescending(cycle => cycle.CreatedAt).ToList();

    /// <summary>A cycle's scope in words, like "the whole company" or "Engineering and Product".</summary>
    public string ScopeText(HrReviewCycle cycle) => ScopeText(cycle.DepartmentIds);

    /// <summary>A scope in words, like "the whole company" or "Engineering and Product".</summary>
    public string ScopeText(IReadOnlyCollection<string> departmentIds)
    {
        var names = departmentIds.Select(id => FindDepartment(id)?.Name ?? id).ToList();

        return names.Count switch
        {
            0 => "the whole company",
            1 => names[0],
            2 => $"{names[0]} and {names[1]}",
            _ => $"{string.Join(", ", names.Take(names.Count - 1))}, and {names[^1]}",
        };
    }

    /// <summary>
    /// Signs off a review in sign-off: as its employee (with an optional comment of up to
    /// <see cref="MaxSignOffCommentLength" /> characters) or, with <paramref name="asManager" />, for its manager.
    /// WrongState: not in sign-off, or already signed. When everyone in the cycle has signed, the HR admins are told.
    /// </summary>
    public HrResult SignOff(int reviewId, bool asManager, string comment)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        if (FindReview(reviewId) is not { } review || FindCycleRecord(review.CycleId) is not { } cycle)
            return HrResult.NotFound;

        comment = asManager ? string.Empty : comment?.Trim() ?? string.Empty;

        if (comment.Length > MaxSignOffCommentLength)
            return HrResult.Invalid;

        if (asManager ? !IsManagerActor(review) : review.EmployeeId != me.Id)
            return HrResult.NotAllowed;

        if (!(asManager ? CanSignAsManager(review) : CanSignAsEmployee(review)))
            return HrResult.WrongState;

        var now = Clock.Now;
        var previous = (review.EmployeeSignedAt, review.EmployeeComment, review.ManagerSignedAt, review.ManagerSignedById);

        if (asManager)
            (review.ManagerSignedAt, review.ManagerSignedById) = (now, me.Id);
        else
            (review.EmployeeSignedAt, review.EmployeeComment) = (now, comment);

        return Commit(() => (review.EmployeeSignedAt, review.EmployeeComment, review.ManagerSignedAt, review.ManagerSignedById) = previous, () =>
        {
            var forManager = asManager && review.ManagerId != me.Id ? $" for {NameOf(review.ManagerId)}" : null;
            Record("Signed off", ReviewTarget(review, cycle), HrReviewSeedData.ReviewUrl(review.Id),
                (asManager ? $"As the manager{forManager}" : "As the employee") + (comment.Length > 0 ? $"; comment: {comment}" : null));

            if (Included(cycle.Id).All(item => item.EmployeeSignedAt is not null && item.ManagerSignedAt is not null))
                foreach (var accountId in HrAdminAccounts(-1))
                    Notify(accountId, $"Everyone signed off {cycle.Name}", "Close the cycle when you're ready.", HrReviewSeedData.CycleUrl(cycle.Id));
        });
    }

    /// <summary>Returns where a review is: worked out from what was submitted and its cycle's status.</summary>
    public HrReviewStage StageOf(HrReview review)
    {
        if (review.IsLeftOut)
            return HrReviewStage.LeftOut;

        var status = FindCycleRecord(review.CycleId)?.Status ?? HrCycleStatus.Closed;

        return status switch
        {
            HrCycleStatus.Closed => HrReviewStage.Done,
            HrCycleStatus.SignOff => review.EmployeeSignedAt is not null && review.ManagerSignedAt is not null ? HrReviewStage.Done : HrReviewStage.SignOff,
            HrCycleStatus.Calibration => HrReviewStage.Calibration,
            _ => review.ManagerSubmittedAt is not null ? HrReviewStage.Calibration : review.SelfSubmittedAt is not null ? HrReviewStage.ManagerReview : HrReviewStage.SelfReview,
        };
    }

    /// <summary>
    /// Submits the manager's review with a rating from 1 to 5 (the manager, or HR for a manager who can't sign in; see
    /// <see cref="CanWriteManagerReview" />). Invalid: text of <see cref="MinReviewTextLength" /> to
    /// <see cref="MaxReviewTextLength" /> characters, or the rating out of range. When the cycle's last manager review is
    /// in, the HR admins are told it can move to calibration.
    /// </summary>
    public HrResult SubmitManagerReview(int reviewId, string text, int rating)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        if (FindReview(reviewId) is not { } review || FindCycleRecord(review.CycleId) is not { } cycle)
            return HrResult.NotFound;

        if (!IsManagerActor(review))
            return HrResult.NotAllowed;

        if (!CanWriteManagerReview(review))
            return HrResult.WrongState;

        text = text?.Trim() ?? string.Empty;

        if (text.Length is < MinReviewTextLength or > MaxReviewTextLength || rating is < 1 or > 5)
            return HrResult.Invalid;

        (review.ManagerText, review.ManagerRating, review.ManagerSubmittedAt, review.ManagerSubmittedById) = (text, rating, Clock.Now, me.Id);

        return Commit(() => (review.ManagerText, review.ManagerRating, review.ManagerSubmittedAt, review.ManagerSubmittedById) = (string.Empty, null, null, null), () =>
        {
            var forManager = review.ManagerId != me.Id ? $" for {NameOf(review.ManagerId)}" : null;
            Record("Submitted a manager review", ReviewTarget(review, cycle), HrReviewSeedData.ReviewUrl(review.Id), $"Rating {HrReviewSeedData.RatingText(rating)}{forManager}");

            if (Included(cycle.Id).All(item => item.ManagerSubmittedAt is not null))
                foreach (var accountId in HrAdminAccounts(-1))
                    Notify(accountId, $"All manager reviews are in for {cycle.Name}", "Move it to calibration when you're ready.", HrReviewSeedData.CycleUrl(cycle.Id));
        });
    }

    /// <summary>
    /// Submits the signed-in employee's self review, with an optional rating of their own from 1 to 5 (see
    /// <see cref="CanWriteSelfReview" />). Invalid: text of <see cref="MinReviewTextLength" /> to
    /// <see cref="MaxReviewTextLength" /> characters, or the rating out of range. The manager is told (or the HR admins, for
    /// a manager who can't sign in).
    /// </summary>
    public HrResult SubmitSelfReview(int reviewId, string text, int? rating)
    {
        if (Me() is not { } me)
            return HrResult.NotAllowed;

        if (reviews.FirstOrDefault(item => item.Id == reviewId && item.EmployeeId == me.Id) is not { } review || FindCycleRecord(review.CycleId) is not { } cycle)
            return HrResult.NotFound;

        if (!CanWriteSelfReview(review))
            return HrResult.WrongState;

        text = text?.Trim() ?? string.Empty;

        if (text.Length is < MinReviewTextLength or > MaxReviewTextLength || rating is < 1 or > 5)
            return HrResult.Invalid;

        (review.SelfText, review.SelfRating, review.SelfSubmittedAt) = (text, rating, Clock.Now);

        return Commit(() => (review.SelfText, review.SelfRating, review.SelfSubmittedAt) = (string.Empty, null, null), () =>
        {
            Record("Submitted a self review", ReviewTarget(review, cycle), HrReviewSeedData.ReviewUrl(review.Id), rating is { } own ? $"Own rating {HrReviewSeedData.RatingText(own)}" : null);
            var title = $"{me.Name}'s self review is in";

            if (ManagerCanSignIn(review))
                Notify(AccountOf(review.ManagerId), title, $"{cycle.Name}: write your review by {Day(cycle.ManagerReviewDue)}", HrReviewSeedData.ReviewUrl(review.Id));
            else
                foreach (var accountId in HrAdminAccounts(me.Id))
                    Notify(accountId, title, $"{cycle.Name}: record the review for {NameOf(review.ManagerId)}, who can't sign in", HrReviewSeedData.ReviewUrl(review.Id));
        });
    }

    /// <summary>Returns the reviews the signed-in manager writes and signs (their reports' when each cycle opened), the newest cycle first.</summary>
    public IReadOnlyList<HrReview> TeamReviews() =>
        Me() is not { } me || !(IsManager || IsHrAdmin) ? Array.Empty<HrReview>()
            : reviews.Where(review => review.ManagerId == me.Id && review.EmployeeId != me.Id).OrderByDescending(review => FindCycleRecord(review.CycleId)?.CreatedAt).ToList();

    #endregion

    #region Private Methods

    /// <summary>Whether two scopes share a department; an empty scope is the whole company.</summary>
    private static bool Overlaps(IReadOnlyCollection<string> first, IReadOnlyCollection<string> second) => first.Count == 0 || second.Count == 0 || first.Intersect(second).Any();

    /// <summary>Whether someone who works here can sign in (has an account).</summary>
    private bool CanSignIn(int employeeId) => AccountOf(employeeId) is not null;

    private HrReviewCycle? FindCycleRecord(int cycleId) => cycles.FirstOrDefault(cycle => cycle.Id == cycleId);

    /// <summary>A cycle's reviews that weren't left out.</summary>
    private IEnumerable<HrReview> Included(int cycleId) => reviews.Where(review => review.CycleId == cycleId && !review.IsLeftOut);

    /// <summary>Up to five people's names, then "and N more", for audit details.</summary>
    private string Names(IEnumerable<int> employeeIds)
    {
        var names = employeeIds.Select(NameOf).ToList();

        return string.Join(", ", names.Take(5)) + (names.Count > 5 ? string.Create(CultureInfo.InvariantCulture, $", and {names.Count - 5} more") : null);
    }

    private string NameOf(int employeeId) => FindEmployeeRecord(employeeId)?.Name ?? "A former employee";

    private string ReviewTarget(HrReview review, HrReviewCycle cycle) => HrReviewSeedData.ReviewTarget(NameOf(review.EmployeeId), cycle);

    /// <summary>Checks the stored cycles and reviews against each other and the stored people; <see langword="false" /> discards the snapshot.</summary>
    private static bool ValidReviews(List<HrReviewCycle> storedCycles, List<HrReview> storedReviews, int nextCycle, int nextReview, Dictionary<int, HrEmployee> byId)
    {
        var departments = HrSeedData.Departments().Select(item => item.Id).ToHashSet(StringComparer.Ordinal);

        bool Plausible(DateTime? value) => value is not { } at || IsPlausible(at);

        if (storedCycles.Any(cycle => cycle is null || cycle.Id < 1 || cycle.Id >= nextCycle || !IsText(cycle.Name, MaxCycleNameLength) || cycle.DepartmentIds is null
                || cycle.DepartmentIds.Any(id => id is null || !departments.Contains(id)) || !Unique(cycle.DepartmentIds) || !Enum.IsDefined(cycle.Status)
                || !IsPlausible(cycle.CreatedAt) || !byId.ContainsKey(cycle.CreatedById) || !IsPlausible(cycle.SelfReviewDue) || !IsPlausible(cycle.ManagerReviewDue)
                || cycle.ManagerReviewDue <= cycle.SelfReviewDue || !Plausible(cycle.CalibrationAt) || !Plausible(cycle.SignOffAt) || !Plausible(cycle.ClosedAt)
                || (cycle.CalibrationAt is not null) != (cycle.Status >= HrCycleStatus.Calibration) || (cycle.SignOffAt is not null) != (cycle.Status >= HrCycleStatus.SignOff)
                || (cycle.ClosedAt is not null) != (cycle.Status == HrCycleStatus.Closed))
            || !Unique(storedCycles.Select(cycle => cycle.Id)) || !Unique(storedCycles.Select(cycle => cycle.Name.ToUpperInvariant())))
            return false;

        // Only one cycle that isn't closed covers a department.
        var live = storedCycles.Where(cycle => cycle.Status != HrCycleStatus.Closed).ToList();

        if (live.Any(cycle => live.Any(other => other.Id != cycle.Id && Overlaps(cycle.DepartmentIds, other.DepartmentIds))))
            return false;

        var cyclesById = storedCycles.ToDictionary(cycle => cycle.Id);

        if (storedReviews.Any(review => review is null || review.Id < 1 || review.Id >= nextReview || !cyclesById.ContainsKey(review.CycleId) || !byId.ContainsKey(review.EmployeeId)
                || !byId.ContainsKey(review.ManagerId) || review.ManagerId == review.EmployeeId
                || review.SelfText is not { Length: <= MaxReviewTextLength } || review.ManagerText is not { Length: <= MaxReviewTextLength }
                || review.CalibrationNote is not { Length: <= MaxCalibrationNoteLength } || review.EmployeeComment is not { Length: <= MaxSignOffCommentLength }
                || review.SelfRating is < 1 or > 5 || review.ManagerRating is < 1 or > 5 || review.CalibratedRating is < 1 or > 5
                || (review.SelfSubmittedAt is null) != (review.SelfText.Length == 0) || (review.SelfSubmittedAt is null && review.SelfRating is not null)
                || (review.ManagerSubmittedAt is null) != (review.ManagerText.Length == 0) || (review.ManagerSubmittedAt is null) != (review.ManagerRating is null)
                || (review.ManagerSubmittedAt is null) != (review.ManagerSubmittedById is null) || (review.ManagerSubmittedById is { } submitter && !byId.ContainsKey(submitter))
                || (review.CalibratedAt is null) != (review.CalibratedRating is null) || (review.CalibratedAt is null) != (review.CalibratedById is null)
                || (review.CalibratedById is { } calibrator && !byId.ContainsKey(calibrator)) || (review.CalibratedAt is null && review.CalibrationNote.Length > 0)
                || (review.EmployeeSignedAt is null && review.EmployeeComment.Length > 0) || (review.ManagerSignedAt is null) != (review.ManagerSignedById is null)
                || (review.ManagerSignedById is { } signer && !byId.ContainsKey(signer))
                || !Plausible(review.SelfSubmittedAt) || !Plausible(review.ManagerSubmittedAt) || !Plausible(review.CalibratedAt) || !Plausible(review.EmployeeSignedAt)
                || !Plausible(review.ManagerSignedAt) || !ValidStage(review, cyclesById[review.CycleId].Status))
            || !Unique(storedReviews.Select(review => review.Id)) || !Unique(storedReviews.Select(review => (review.CycleId, review.EmployeeId))))
            return false;

        return true;
    }

    /// <summary>
    /// Whether a review fits its cycle's status: while open, nothing is calibrated, signed, or left out; from calibration on,
    /// every review not left out has a manager review, and a left-out one has nothing after it; from sign-off on, every
    /// review not left out is calibrated; signatures only come with sign-off.
    /// </summary>
    private static bool ValidStage(HrReview review, HrCycleStatus status)
    {
        var signed = review.EmployeeSignedAt is not null || review.ManagerSignedAt is not null;

        if (review.IsLeftOut)
            return status != HrCycleStatus.Open && review.ManagerSubmittedAt is null && review.CalibratedAt is null && !signed;

        return status switch
        {
            HrCycleStatus.Open => review.CalibratedAt is null && !signed,
            HrCycleStatus.Calibration => review.ManagerSubmittedAt is not null && !signed,
            _ => review.ManagerSubmittedAt is not null && review.CalibratedAt is not null,
        };
    }

    #endregion

    #endregion
}

/// <summary>Counts of a cycle's reviews by stage, and of the steps done.</summary>
/// <param name="Total">Every review in the cycle.</param>
/// <param name="SelfReview">Waiting for the self review.</param>
/// <param name="ManagerReview">Waiting for the manager review.</param>
/// <param name="Calibration">Manager review in, waiting for HR.</param>
/// <param name="SignOff">Released, waiting for a sign-off.</param>
/// <param name="Done">Signed by both, or closed.</param>
/// <param name="LeftOut">Left out without a manager review.</param>
/// <param name="SelfDone">Self reviews submitted.</param>
/// <param name="ManagerDone">Manager reviews submitted.</param>
/// <param name="Calibrated">Ratings calibrated.</param>
/// <param name="Signed">Reviews signed by both.</param>
public sealed record HrCycleProgress(int Total, int SelfReview, int ManagerReview, int Calibration, int SignOff, int Done, int LeftOut, int SelfDone, int ManagerDone, int Calibrated,
    int Signed);

/// <summary>Something waiting for the signed-in person in a review cycle.</summary>
/// <param name="Text">What to do, like "Write your self review, due Nov 27, 2026".</param>
/// <param name="CycleName">The cycle it belongs to.</param>
/// <param name="Url">Where to do it.</param>
/// <param name="Due">When it's due (or became waiting), for the order.</param>
public sealed record HrReviewTodo(string Text, string CycleName, string Url, DateTime Due);
