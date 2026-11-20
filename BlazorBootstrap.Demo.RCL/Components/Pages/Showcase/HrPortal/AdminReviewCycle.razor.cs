using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// One review cycle (HR admin): its status, scope, and due dates; progress as counts (self reviews, manager reviews,
/// calibrated ratings, sign-offs) and one bar per stage with its count in words; the rating distribution by department
/// (counts, shares, and the average, from the calibrated rating where there is one) as a plain table, since calibration is
/// where the spread matters; and a <see cref="Grid{TItem}" /> of the people in it (employee, department, manager, stage,
/// manager rating, calibrated rating, and HR's note), filtered by department and stage, with paging, sorting, the column
/// chooser, CSV export, and a saved view that can be reset; a row opens the review. The stage button moves the cycle on
/// after a confirmation that counts and names who is missing: to calibration (leaving out reviews without a manager review
/// if HR confirms), released for sign-off (uncalibrated ratings keep the manager's), or closed (with who hasn't signed).
/// Calibrate opens a validated dialog (rule WR-002): the rating, and a note of 10 to 300 characters when it differs from
/// the manager's (a cross-field rule), with a preview of the change (<see cref="HrState.Calibrate" />). Outcomes are
/// announced in a status region, and focus returns to the button that opened a dialog, or to the status.
/// </summary>
public partial class AdminReviewCycle : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private readonly Dictionary<int, ElementReference> calibrateButtons = new();

    private ElementReference? advanceButton;

    /// <summary>Whether the stage dialog was confirmed; the step runs once it is hidden.</summary>
    private bool advanceConfirmed;

    private Modal advanceModal = default!;

    private EditContext calibrateContext = new(new CalibrateForm());

    private string? calibrateError;

    private int calibrateErrors;

    private CalibrateForm calibrateForm = new();

    private int calibrateId;

    private Modal calibrateModal = default!;

    /// <summary>Whether the calibration was saved; focus then goes to the status, which says how.</summary>
    private bool calibrateSaved;

    private string departmentFilter = string.Empty;

    private Grid<ParticipantRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string noteText = string.Empty;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private InputSelect<int?>? ratingSelect;

    private string stageFilter = string.Empty;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await view.LoadAsync();
        pageSize = (await view.GetSettingsAsync()).PageSize;
        viewLoaded = true;
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "review-participants", DefaultPageSize);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The stage button's words for a status.</summary>
    private static string AdvanceLabel(HrCycleStatus status) => status switch
    {
        HrCycleStatus.Open => "Move to calibration",
        HrCycleStatus.Calibration => "Release for sign-off",
        _ => "Close the cycle",
    };

    /// <summary>The confirm button's words: they say when people are left out or unsigned.</summary>
    private static string ConfirmLabel(HrCycleStatus status, int missing) => status switch
    {
        HrCycleStatus.Open => missing > 0 ? "Leave them out and move to calibration" : "Move to calibration",
        HrCycleStatus.Calibration => "Release for sign-off",
        _ => missing > 0 ? "Close without their sign-off" : "Close the cycle",
    };

    /// <summary>The title of the list of who is missing in the stage dialog.</summary>
    private static string MissingTitle(HrCycleStatus status, int count) => status switch
    {
        HrCycleStatus.Open => $"{HrDisplay.Plural(count, "review has", "reviews have")} no manager review yet:",
        HrCycleStatus.Calibration => $"{HrDisplay.Plural(count, "rating isn't", "ratings aren't")} calibrated yet, and keep the manager's:",
        _ => $"{HrDisplay.Plural(count, "review isn't", "reviews aren't")} signed off by both yet:",
    };

    /// <summary>The counts by stage, in the order of the workflow (left out last, only when there are any).</summary>
    private static IEnumerable<(HrReviewStage Stage, int Count)> StageCounts(HrCycleProgress progress)
    {
        yield return (HrReviewStage.SelfReview, progress.SelfReview);
        yield return (HrReviewStage.ManagerReview, progress.ManagerReview);
        yield return (HrReviewStage.Calibration, progress.Calibration);
        yield return (HrReviewStage.SignOff, progress.SignOff);
        yield return (HrReviewStage.Done, progress.Done);

        if (progress.LeftOut > 0)
            yield return (HrReviewStage.LeftOut, progress.LeftOut);
    }

    /// <summary>What the stage step does, in words, with the counts.</summary>
    private string AdvanceText(HrReviewCycle cycle, HrCycleProgress progress, int missing)
    {
        var included = progress.Total - progress.LeftOut;

        return cycle.Status switch
        {
            HrCycleStatus.Open when progress.ManagerDone == 0 => "No manager review is in yet, so there's nothing to calibrate. Wait for the managers, or record reviews for managers who can't sign in.",
            HrCycleStatus.Open when missing == 0 => FormattableString.Invariant($"Every manager review is in ({progress.ManagerDone} of {progress.Total}). Self reviews and manager reviews close, and you calibrate the ratings across the group."),
            HrCycleStatus.Open => FormattableString.Invariant($"{progress.ManagerDone} of {progress.Total} manager reviews are in. Moving on now leaves the other {missing} out of calibration and sign-off; their reviews stay as they are, with no rating."),
            HrCycleStatus.Calibration => FormattableString.Invariant($"{progress.Calibrated} of {included} ratings are calibrated. Releasing shows each employee and manager the review and the rating, and asks both to sign off; you can't calibrate after that."),
            _ => FormattableString.Invariant($"{progress.Signed} of {included} reviews are signed off by both. Closing keeps the cycle for the record; nobody can sign off after that."),
        };
    }

    /// <summary>Whether the stage step can run now (an open cycle needs at least one manager review).</summary>
    private static bool CanAdvance(HrReviewCycle cycle, HrCycleProgress progress) =>
        cycle.Status != HrCycleStatus.Closed && (cycle.Status != HrCycleStatus.Open || progress.ManagerDone > 0);

    /// <summary>Says what the dialog's rating does, like "Keeps the manager's 4 · Exceeds expectations" or "Changes it from 4 to 3".</summary>
    private string CalibratePreview(HrReview review) =>
        calibrateForm.Rating is not { } rating ? "Choose a rating."
        : rating == review.ManagerRating ? $"Keeps the manager's rating, {ReviewDisplay.Rating(rating)}. A note is optional."
        : $"Changes {FirstName(EmployeeName(review.EmployeeId))}'s rating from {ReviewDisplay.Rating(review.ManagerRating)} to {ReviewDisplay.Rating(rating)}. Say why in the note.";

    /// <summary>Runs the confirmed stage step after the dialog is hidden, and says what happened.</summary>
    private void Advance(HrReviewCycle cycle)
    {
        int count;
        HrResult result;

        switch (cycle.Status)
        {
            case HrCycleStatus.Open:
                result = State.MoveToCalibration(cycle.Id, leaveOutMissing: true, out count);
                SetStatus(result == HrResult.Ok
                    ? $"Moved “{cycle.Name}” to calibration." + (count > 0 ? $" {HrDisplay.Plural(count, "review was", "reviews were")} left out without a manager review." : " Every manager review was in.") + " Calibrate the ratings, then release for sign-off."
                    : Failure(result, "moved to calibration"));
                break;

            case HrCycleStatus.Calibration:
                result = State.ReleaseForSignOff(cycle.Id, out count);
                SetStatus(result == HrResult.Ok
                    ? $"Released “{cycle.Name}” for sign-off." + (count > 0 ? $" {HrDisplay.Plural(count, "rating", "ratings")} kept the manager's." : null) + " Employees and managers are notified."
                    : Failure(result, "released"));
                break;

            default:
                result = State.CloseCycle(cycle.Id, closeUnsigned: true, out count);
                SetStatus(result == HrResult.Ok
                    ? $"Closed “{cycle.Name}”." + (count > 0 ? $" {HrDisplay.Plural(count, "review was", "reviews were")} not signed off by both." : " Everyone signed off.")
                    : Failure(result, "closed"));
                break;
        }
    }

    private async Task ConfirmAdvanceAsync()
    {
        advanceConfirmed = true;
        await advanceModal.HideAsync();
    }

    private string EmployeeName(int employeeId) => State.FindEmployee(employeeId)?.Name ?? "A former employee";

    /// <summary>A refused step in words.</summary>
    private static string Failure(HrResult result, string verb) => result switch
    {
        HrResult.WrongState => $"The cycle wasn't {verb}: it already moved on, or no manager review is in yet.",
        HrResult.NotAllowed => $"Only the HR admin can do this; the cycle wasn't {verb}.",
        HrResult.Invalid => $"This demo tab's storage is full, so the cycle wasn't {verb}.",
        _ => $"That didn't work, and the cycle wasn't {verb}. Try again.",
    };

    private static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    /// <summary>Who is missing for the next step: no manager review (open), not calibrated (calibration), or not signed by both (sign-off).</summary>
    private IReadOnlyList<string> Missing(HrReviewCycle cycle) =>
        State.CycleReviews(cycle.Id).Where(review => !review.IsLeftOut && cycle.Status switch
            {
                HrCycleStatus.Open => review.ManagerSubmittedAt is null,
                HrCycleStatus.Calibration => review.CalibratedAt is null,
                HrCycleStatus.SignOff => review.EmployeeSignedAt is null || review.ManagerSignedAt is null,
                _ => false,
            })
            .Select(review => EmployeeName(review.EmployeeId))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// The rating distribution: one row per department in the cycle and a total, counting the calibrated rating where there
    /// is one, otherwise the manager's; reviews without either are "Not rated".
    /// </summary>
    private IReadOnlyList<DistributionRow> Distribution(IReadOnlyList<HrReview> reviews)
    {
        DistributionRow Row(string name, IEnumerable<HrReview> group, bool isTotal)
        {
            var ratings = group.Select(review => review.CalibratedRating ?? review.ManagerRating).ToList();
            var rated = ratings.OfType<int>().ToList();
            var counts = Enumerable.Range(1, 5).Select(rating => rated.Count(value => value == rating)).ToArray();

            return new DistributionRow(name, counts, ratings.Count - rated.Count, rated.Count, rated.Count == 0 ? 0 : rated.Average(), isTotal);
        }

        var rows = reviews.Where(review => !review.IsLeftOut)
            .GroupBy(review => State.FindEmployee(review.EmployeeId)?.DepartmentId ?? string.Empty)
            .Select(group => Row(State.FindDepartment(group.Key)?.Name ?? "No department", group, false))
            .OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        rows.Add(Row("All departments", reviews.Where(review => !review.IsLeftOut), true));

        return rows;
    }

    /// <summary>The dialog opens from code: after a step focus goes to the status (the step runs now), otherwise back to the button.</summary>
    private async Task OnAdvanceHiddenAsync()
    {
        if (!advanceConfirmed || Cycle is not { } cycle)
        {
            await HrDisplay.FocusAsync(advanceButton);

            return;
        }

        advanceConfirmed = false;
        Advance(cycle);
        StateHasChanged();
        await Task.Yield();
        await HrDisplay.FocusAsync(statusRegion);
    }

    /// <summary>After a save focus goes to the status (the row's button may have gone), otherwise back to the row's Calibrate button.</summary>
    private async Task OnCalibrateHiddenAsync()
    {
        var id = calibrateId;
        var saved = calibrateSaved;
        calibrateSaved = false;

        if (!saved && calibrateButtons.TryGetValue(id, out var button))
            await HrDisplay.FocusAsync(button);
        else
            await HrDisplay.FocusAsync(statusRegion);
    }

    private Task OnCalibrateShownAsync() => HrDisplay.FocusAsync(ratingSelect?.Element);

    private async Task OnDepartmentChangedAsync(ChangeEventArgs args)
    {
        departmentFilter = args.Value?.ToString() ?? string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private async Task OnStageChangedAsync(ChangeEventArgs args)
    {
        stageFilter = args.Value?.ToString() ?? string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>A review or the cycle changed (here or elsewhere): the grid reads the people again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenAdvanceAsync()
    {
        advanceConfirmed = false;
        await advanceModal.ShowAsync();
    }

    /// <summary>Fills the calibrate dialog with the current rating (the calibrated one, or the manager's) and note, and opens it.</summary>
    private async Task OpenCalibrateAsync(int reviewId)
    {
        if (State.FindReview(reviewId) is not { } review)
            return;

        calibrateId = reviewId;
        calibrateSaved = false;
        calibrateForm = new CalibrateForm { Rating = review.CalibratedRating ?? review.ManagerRating, Note = review.CalibrationNote, ManagerRating = review.ManagerRating };
        calibrateContext = new EditContext(calibrateForm);
        noteText = review.CalibrationNote;
        calibrateError = null;
        calibrateErrors = 0;

        await calibrateModal.ShowAsync();
    }

    /// <summary>The page of people the grid asked for, filtered by department and stage, by name unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ParticipantRow>> ProvideParticipantsAsync(GridDataProviderRequest<ParticipantRow> request)
    {
        var rows = new List<ParticipantRow>();

        if (Cycle is { } cycle)
        {
            foreach (var review in State.CycleReviews(cycle.Id))
            {
                var employee = State.FindEmployee(review.EmployeeId);
                var stage = State.StageOf(review);

                if ((departmentFilter.Length > 0 && employee?.DepartmentId != departmentFilter) || (stageFilter.Length > 0 && stage.ToString() != stageFilter))
                    continue;

                rows.Add(new ParticipantRow(review.Id, employee?.Name ?? "A former employee", State.FindDepartment(employee?.DepartmentId)?.Name ?? "No department",
                    EmployeeName(review.ManagerId), State.ManagerCanSignIn(review), ReviewDisplay.StageLabel(stage), stage, ReviewDisplay.Rating(review.ManagerRating),
                    review.ManagerRating ?? 0, review.CalibratedRating is { } calibrated ? ReviewDisplay.Rating(calibrated) : "Not yet", review.CalibratedRating ?? 0,
                    review.CalibratedRating is not null && review.CalibratedRating != review.ManagerRating, review.CalibrationNote, State.CanCalibrate(review)));
            }
        }

        return view.ApplyAsync(request, rows.OrderBy(row => row.Employee, StringComparer.OrdinalIgnoreCase));
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the status.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The view is reset: people by name, 25 a page, and every column shown.");
        StateHasChanged();
        await Task.Yield();
        await HrDisplay.FocusAsync(statusRegion);
    }

    /// <summary>
    /// Saves the calibration once the form is valid (a failed submit shows the summary); the state checks again, and its
    /// answer maps to a message.
    /// </summary>
    private async Task SaveCalibrationAsync()
    {
        if (CalibrateReview is not { } review || !calibrateContext.Validate())
            return;

        var name = EmployeeName(review.EmployeeId);
        var rating = calibrateForm.Rating!.Value;
        var result = State.Calibrate(review.Id, rating, calibrateForm.Note);

        switch (result)
        {
            case HrResult.Ok:
                SetStatus(rating == review.ManagerRating ? $"Calibrated {name}: the manager's rating, {ReviewDisplay.Rating(rating)}, stands." : $"Calibrated {name} to {ReviewDisplay.Rating(rating)}, with your note.");
                break;

            case HrResult.NoChange:
                SetStatus($"Nothing changed for {name}: that's already the calibrated rating and note.");
                break;

            case HrResult.Invalid:
                calibrateErrors++;
                calibrateError = "Write a note of at least 10 characters when you change the manager's rating." + (calibrateErrors > 1 ? string.Create(CultureInfo.InvariantCulture, $" ({calibrateErrors})") : null);

                return;

            default:
                SetStatus($"{name}'s rating can't be calibrated now: the cycle moved on, or it's your own review.");
                break;
        }

        calibrateSaved = true;
        await calibrateModal.HideAsync();
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private HrReview? CalibrateReview => calibrateId == 0 ? null : State.FindReview(calibrateId) is { } review && State.CanCalibrate(review) ? review : null;

    private HrReviewCycle? Cycle => State.IsHrAdmin ? State.FindCycle(Id) : null;

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets or sets the cycle ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The calibrate dialog's fields, with the limits of <see cref="HrState.Calibrate" />: a rating from 1 to 5, and a note
    /// of up to 300 characters, at least 10 when the rating differs from the manager's (a cross-field rule).
    /// </summary>
    private sealed class CalibrateForm : IValidatableObject
    {
        /// <summary>Gets or sets the manager's rating, for the cross-field rule; not a field.</summary>
        public int? ManagerRating { get; set; }

        [StringLength(HrState.MaxCalibrationNoteLength, ErrorMessage = "Shorten the note to 300 characters or fewer.")]
        public string Note { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose the calibrated rating.")]
        [Range(1, 5, ErrorMessage = "Choose a rating from 1 to 5.")]
        public int? Rating { get; set; }

        /// <summary>Runs once the fields are in range: changing the manager's rating needs a reason.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Rating is { } rating && rating != ManagerRating && Note.Trim().Length < HrState.MinCalibrationNoteLength)
                yield return new ValidationResult("Say why you change the manager's rating, in at least 10 characters.", new[] { nameof(Note) });
        }
    }

    /// <summary>One department's ratings: counts for 1 to 5, not rated, how many are rated, and the average.</summary>
    private sealed record DistributionRow(string Name, int[] Counts, int NotRated, int Rated, double Average, bool IsTotal);

    /// <summary>One review in the cycle as a grid row: the words each column shows and exports, and the values it sorts by.</summary>
    private sealed record ParticipantRow(int Id, string Employee, string Department, string Manager, bool ManagerCanSignIn, string Stage, HrReviewStage StageKind,
        string ManagerRating, int ManagerRatingValue, string CalibratedRating, int CalibratedValue, bool Changed, string Note, bool CanCalibrate);

    #endregion
}
