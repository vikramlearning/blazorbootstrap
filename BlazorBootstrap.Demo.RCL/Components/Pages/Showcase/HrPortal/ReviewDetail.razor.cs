using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// One performance review (the employee, their manager when the cycle opened, or the HR admin): the steps of the review in
/// words (self review, manager review, calibration, sign-off), then what the viewer may do now. The employee writes their
/// self review in a validated form (rule WR-002: 50 to 2,000 characters, an optional own rating) while the cycle is open;
/// the manager (or HR for a manager who can't sign in) writes the review with a required rating once the self review is
/// in (<see cref="HrState.SubmitManagerReview" />); after HR releases the cycle, both read the rating HR calibrated, with
/// its note, and sign off: the employee with an optional comment (up to 500 characters), the manager with one button
/// (<see cref="HrState.SignOff" />). The employee doesn't see the manager's review before the release. Every outcome is
/// announced in a status region that then takes focus; an unknown review, or someone else's, shows <see cref="HrNotFound" />.
/// </summary>
public partial class ReviewDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string commentText = string.Empty;

    private string? errorMessage;

    private bool focusStatus;

    /// <summary>The review the forms were loaded for, or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private EditContext managerContext = new(new ManagerForm());

    private ManagerForm managerForm = new();

    private string managerText = string.Empty;

    private EditContext selfContext = new(new SelfForm());

    private SelfForm selfForm = new();

    private string selfText = string.Empty;

    private EditContext signContext = new(new SignForm());

    private SignForm signForm = new();

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!focusStatus)
            return;

        focusStatus = false;
        await HrDisplay.FocusAsync(statusRegion);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = errorMessage = null;
        ResetForms();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    /// <summary>A review text's length against its limits, like "120 characters (at least 50, up to 2,000)".</summary>
    private static string LengthText(string text) =>
        string.Create(CultureInfo.InvariantCulture, $"{text.Trim().Length} characters (at least {HrState.MinReviewTextLength}, up to {HrState.MaxReviewTextLength:N0})");

    private void Announce(string message, bool isError)
    {
        statusMessage = isError ? null : message;
        errorMessage = isError ? message : null;
        focusStatus = !isError;
    }

    private string EmployeeName(HrReview review) => EmployeeName(review.EmployeeId);

    private string EmployeeName(int employeeId) => State.FindEmployee(employeeId)?.Name ?? "A former employee";

    private bool IsOwn(HrReview review) => review.EmployeeId == State.CurrentEmployee?.Id;

    /// <summary>Why there is no manager review yet, in words, for the viewer.</summary>
    private string ManagerMissingText(HrReview review, HrReviewCycle cycle, bool managerActor, string first)
    {
        if (review.IsLeftOut)
            return "There was no manager review when HR moved the cycle to calibration, so this review was left out.";

        if (cycle.Status != HrCycleStatus.Open)
            return "No manager review was written.";

        if (!managerActor)
            return $"{EmployeeName(review.ManagerId)} hasn't written it yet; it's due {HrDisplay.Date(cycle.ManagerReviewDue)}.";

        return review.SelfSubmittedAt is null
            ? $"You can write it once {first} submits their self review, or after its due date, {HrDisplay.Date(cycle.SelfReviewDue)}."
            : "You can't write this review right now.";
    }

    /// <summary>A state changed elsewhere: re-render (the forms keep what's typed).</summary>
    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Starts fresh forms for the review on the route.</summary>
    private void ResetForms()
    {
        selfForm = new SelfForm();
        selfContext = new EditContext(selfForm);
        managerForm = new ManagerForm();
        managerContext = new EditContext(managerForm);
        signForm = new SignForm();
        signContext = new EditContext(signForm);
        selfText = managerText = commentText = string.Empty;
    }

    /// <summary>A result of the state in words, for the status or the error region.</summary>
    private (string Message, bool IsError) ResultText(HrResult result, string done) => result switch
    {
        HrResult.Ok => (done, false),
        HrResult.Invalid => ("Something in the form is out of range, or this demo tab's storage is full. Nothing was saved.", true),
        HrResult.WrongState => ("This step isn't open any more: the cycle moved on, or it's already done. Nothing was saved.", true),
        HrResult.NotAllowed => ("You can't do this step for this review. Nothing was saved.", true),
        HrResult.NotFound => ("This review no longer exists. Nothing was saved.", true),
        _ => ("That didn't work, and nothing was saved. Try again.", true),
    };

    /// <summary>Why there is no self review yet, in words, for the viewer.</summary>
    private string SelfMissingText(HrReview review, HrReviewCycle cycle, bool own, string first)
    {
        if (review.ManagerSubmittedAt is not null)
            return own ? "You didn't write a self review: your manager's review came first." : $"{first} didn't write a self review.";

        if (cycle.Status != HrCycleStatus.Open)
            return own ? "You didn't write a self review in this cycle." : $"{first} didn't write a self review.";

        return own ? "You can't write it right now." : $"{first} hasn't written it yet; it's due {HrDisplay.Date(cycle.SelfReviewDue)}.";
    }

    /// <summary>Signs off as the manager (or for them, as HR).</summary>
    private void SignAsManager()
    {
        if (Review is not { } review)
            return;

        var first = FirstName(EmployeeName(review));
        var (message, isError) = ResultText(State.SignOff(review.Id, asManager: true, string.Empty),
            $"Signed off {first}'s review as the manager." + (review.EmployeeSignedAt is null ? $" {first} hasn't signed yet." : " Both have signed: it's done."));
        Announce(message, isError);
    }

    /// <summary>Signs off as the employee once the comment is valid (a failed submit shows the summary).</summary>
    private Task SignAsEmployeeAsync()
    {
        if (Review is not { } review || !signContext.Validate())
            return Task.CompletedTask;

        var (message, isError) = ResultText(State.SignOff(review.Id, asManager: false, signForm.Comment),
            "You signed off your review." + (review.ManagerSignedAt is null ? " Your manager signs off next." : " Your manager has signed too: it's done."));
        Announce(message, isError);

        return Task.CompletedTask;
    }

    /// <summary>Submits the manager review once the form is valid (a failed submit shows the summary); the employee sees it after the release.</summary>
    private Task SubmitManagerAsync()
    {
        if (Review is not { } review || !managerContext.Validate())
            return Task.CompletedTask;

        var first = FirstName(EmployeeName(review));
        var (message, isError) = ResultText(State.SubmitManagerReview(review.Id, managerForm.Text, managerForm.Rating!.Value),
            $"Submitted your review of {first}, rated {ReviewDisplay.Rating(managerForm.Rating)}. HR calibrates ratings next; {first} sees the outcome when it's released.");
        Announce(message, isError);

        return Task.CompletedTask;
    }

    /// <summary>Submits the self review once the form is valid (a failed submit shows the summary); the manager is told.</summary>
    private Task SubmitSelfAsync()
    {
        if (Review is not { } review || !selfContext.Validate())
            return Task.CompletedTask;

        var (message, isError) = ResultText(State.SubmitSelfReview(review.Id, selfForm.Text, selfForm.Rating),
            $"Submitted your self review. {EmployeeName(review.ManagerId)} writes their review next; you see it, and your rating, when HR releases the cycle.");
        Announce(message, isError);

        return Task.CompletedTask;
    }

    /// <summary>The review's steps in order, each done, current, or waiting, in words.</summary>
    private IReadOnlyList<Step> Steps(HrReview review, HrReviewCycle cycle)
    {
        var stage = State.StageOf(review);
        StepState Of(HrReviewStage step) => stage == HrReviewStage.LeftOut ? StepState.Waiting : stage == step ? StepState.Current : stage > step ? StepState.Done : StepState.Waiting;

        return new[]
        {
            new Step("Self review", review.SelfSubmittedAt is { } self ? $"Submitted {HrDisplay.Date(self)}" : review.ManagerSubmittedAt is not null || cycle.Status != HrCycleStatus.Open ? "Not written" : $"Due {HrDisplay.Date(cycle.SelfReviewDue)}",
                review.SelfSubmittedAt is not null || review.ManagerSubmittedAt is not null ? StepState.Done : Of(HrReviewStage.SelfReview)),
            new Step("Manager review", review.ManagerSubmittedAt is { } manager ? $"Submitted {HrDisplay.Date(manager)}" : review.IsLeftOut ? "Missing: left out" : $"Due {HrDisplay.Date(cycle.ManagerReviewDue)}",
                review.ManagerSubmittedAt is not null ? StepState.Done : Of(HrReviewStage.ManagerReview)),
            new Step("Calibration by HR", cycle.SignOffAt is { } released ? $"Released {HrDisplay.Date(released)}" : cycle.CalibrationAt is { } calibration ? $"Since {HrDisplay.Date(calibration)}" : "After the manager reviews",
                Of(HrReviewStage.Calibration)),
            new Step("Sign-off", review.EmployeeSignedAt is not null && review.ManagerSignedAt is not null ? "Both signed" : cycle.Status == HrCycleStatus.SignOff ? "Employee and manager" : "After the release",
                Of(HrReviewStage.SignOff)),
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the review ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private HrReview? Review => State.FindReview(Id);

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where a step of the review is.</summary>
    private enum StepState
    {
        Waiting,
        Current,
        Done,
    }

    /// <summary>The manager review form, with the limits of <see cref="HrState.SubmitManagerReview" />.</summary>
    private sealed class ManagerForm
    {
        [Required(ErrorMessage = "Choose a rating from 1 to 5.")]
        [Range(1, 5, ErrorMessage = "Choose a rating from 1 to 5.")]
        public int? Rating { get; set; }

        [Required(ErrorMessage = "Write the review: what went well, and what to work on next.")]
        [StringLength(HrState.MaxReviewTextLength, MinimumLength = HrState.MinReviewTextLength, ErrorMessage = "Write 50 to 2,000 characters, so the review says enough to act on.")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>The self review form, with the limits of <see cref="HrState.SubmitSelfReview" />.</summary>
    private sealed class SelfForm
    {
        [Range(1, 5, ErrorMessage = "Choose a rating from 1 to 5, or no rating.")]
        public int? Rating { get; set; }

        [Required(ErrorMessage = "Write your self review: what you achieved, and what you want to work on next.")]
        [StringLength(HrState.MaxReviewTextLength, MinimumLength = HrState.MinReviewTextLength, ErrorMessage = "Write 50 to 2,000 characters, so your manager has enough to go on.")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>The employee's sign-off, with an optional comment.</summary>
    private sealed class SignForm
    {
        [StringLength(HrState.MaxSignOffCommentLength, ErrorMessage = "Shorten the comment to 500 characters or fewer.")]
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>A step of the review in words.</summary>
    private sealed record Step(string Name, string Text, StepState State);

    #endregion
}
