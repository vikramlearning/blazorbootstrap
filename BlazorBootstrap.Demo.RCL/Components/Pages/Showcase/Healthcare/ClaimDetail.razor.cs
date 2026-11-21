using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// One insurance claim (billing; doctors read-only): its status, the patient, visit, plan, member ID, billed amount, filing
/// and appeal deadlines, and the patient's bill; the codes sent with their fees; every version with what changed; the
/// plan's answers; the appeal letter; and its history from the audit log. Billing's next step depends on the status, each
/// in a dialog: Submit or Resubmit (<see cref="HcState.SubmitClaim" />, refused after the filing deadline); Edit codes or
/// Correct (<see cref="HcCodingForm" />: codes, member ID, and a note for a denied claim); Record the plan's answer, an
/// <see cref="EditForm" /> (rule WR-002) with the outcome, the allowed, paid, and patient amounts (the paid and patient
/// amounts add up to the allowed, which is at most the billed amount: cross-field rules, <see cref="HcState.ResponseProblem" />),
/// the reason codes, and a remark (<see cref="HcState.RecordClaimResponse" />); Appeal, with a letter of 50 to 2,000
/// characters before the deadline (<see cref="HcState.AppealClaim" />); and Write off or bill the patient, with a reason of
/// 10 to 300 characters (<see cref="HcState.CloseClaim" />). Outcomes are announced in the status region once the dialog
/// is hidden.
/// </summary>
public partial class ClaimDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The roles that can open a claim.</summary>
    public const string ClaimRoles = HcSeedData.BillingRole + "," + HcSeedData.DoctorRole;

    private const string BillPatientValue = "patient";

    private const string WriteOffValue = "writeoff";

    private EditContext appealContext;

    private AppealForm appealForm = new();

    private EditContext closeContext;

    private CloseForm closeForm = new();

    private Dialog dialog;

    private int dialogErrorCount;

    private string? dialogError;

    private ElementReference? dialogHeading;

    private bool focusStatus;

    private Modal modal = default!;

    /// <summary>The outcome of the dialog's step, announced once the dialog is hidden.</summary>
    private (string Message, bool IsError)? pending;

    private EditContext responseContext;

    private ResponseForm responseForm = new();

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with empty dialog forms.</summary>
    public ClaimDetail()
    {
        appealContext = new EditContext(appealForm);
        closeContext = new EditContext(closeForm);
        responseContext = new EditContext(responseForm);
    }

    #endregion

    #region Enums

    /// <summary>The dialog that is open.</summary>
    private enum Dialog
    {
        None,
        Submit,
        Codes,
        Response,
        Appeal,
        Close,
    }

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await HcDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Returns denial or adjustment codes with their descriptions, like "CO-50 Not medically necessary".</summary>
    public static string CodesText(HcState state, IReadOnlyCollection<string> codes) =>
        codes.Count == 0 ? "none" : string.Join("; ", codes.Select(code => $"{code} {state.DenialCodes.FirstOrDefault(item => item.Code == code)?.Description}".Trim()));

    /// <summary>Returns diagnosis codes with their descriptions.</summary>
    public static string DiagnosesText(HcState state, IReadOnlyCollection<string> codes) =>
        string.Join("; ", codes.Select(code => $"{code} {state.DiagnosisCodes.FirstOrDefault(item => item.Code == code)?.Description}".Trim()));

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the Bootstrap color of a claim status (always shown with its words).</summary>
    public static string StatusColor(HcClaimStatus status) => status switch
    {
        HcClaimStatus.Draft or HcClaimStatus.Corrected => "secondary",
        HcClaimStatus.Submitted or HcClaimStatus.Appealed => "info",
        HcClaimStatus.Paid => "success",
        HcClaimStatus.PartlyPaid => "warning",
        HcClaimStatus.Denied => "danger",
        _ => "dark",
    };

    /// <summary>Returns a claim's status in words, like "Denied after appeal" or "Written off".</summary>
    public static string StatusLabel(HcClaim claim) => claim.Status switch
    {
        HcClaimStatus.Draft => "Draft",
        HcClaimStatus.Submitted => "With the plan",
        HcClaimStatus.Paid => "Paid",
        HcClaimStatus.PartlyPaid => "Partly paid",
        HcClaimStatus.Denied when HcState.IsFinalDenial(claim) => "Denied after appeal",
        HcClaimStatus.Denied => "Denied",
        HcClaimStatus.Corrected => "Corrected, to resubmit",
        HcClaimStatus.Appealed => "Appealed",
        _ => claim.BilledToPatient ? "Billed to the patient" : "Written off",
    };

    /// <summary>Returns the staff route of a claim.</summary>
    public static string Url(int claimId) => HcSeedData.ClaimPath(claimId);

    #endregion

    #region Private Methods

    private static int Cents(decimal? dollars) => dollars is { } value ? (int)Math.Round(value * 100m, MidpointRounding.AwayFromZero) : 0;

    /// <summary>Returns a deadline in words, like "8 days left" or "2 days ago".</summary>
    internal static string DaysLeftText(DateTime deadline, DateTime? today = null)
    {
        var days = (deadline.Date - (today ?? HcSeedData.Now).Date).Days;

        return days switch
        {
            0 => "today",
            1 => "1 day left",
            > 1 => string.Create(CultureInfo.InvariantCulture, $"{days} days left"),
            -1 => "1 day ago",
            _ => string.Create(CultureInfo.InvariantCulture, $"{-days} days ago"),
        };
    }

    /// <summary>Lists what changed from one version to the next.</summary>
    private static IEnumerable<string> DiffText(HcClaimVersion before, HcClaimVersion after)
    {
        var removedDx = before.DiagnosisCodes.Except(after.DiagnosisCodes).ToList();
        var addedDx = after.DiagnosisCodes.Except(before.DiagnosisCodes).ToList();
        var removedPx = before.ProcedureCodes.Except(after.ProcedureCodes).ToList();
        var addedPx = after.ProcedureCodes.Except(before.ProcedureCodes).ToList();

        if (addedDx.Count > 0)
            yield return $"Diagnosis added: {string.Join(", ", addedDx)}";

        if (removedDx.Count > 0)
            yield return $"Diagnosis removed: {string.Join(", ", removedDx)}";

        if (addedPx.Count > 0)
            yield return $"Procedure added: {string.Join(", ", addedPx)}";

        if (removedPx.Count > 0)
            yield return $"Procedure removed: {string.Join(", ", removedPx)}";

        if (before.MemberId != after.MemberId)
            yield return $"Member ID {before.MemberId} → {after.MemberId}";

        if (addedDx.Count + removedDx.Count + addedPx.Count + removedPx.Count == 0 && before.MemberId == after.MemberId)
            yield return "No code or member ID changes";
    }

    private static string OutcomeLabel(HcClaimOutcome outcome) => outcome switch
    {
        HcClaimOutcome.Paid => "Paid",
        HcClaimOutcome.PartlyPaid => "Partly paid",
        _ => "Denied",
    };

    private async Task AppealAsync()
    {
        if (State.FindClaim(Id) is not { } claim)
            return;

        appealForm.PastDeadline = claim.AppealDeadline is { } deadline && State.Clock.Today > deadline;

        if (!appealContext.Validate())
            return;

        var result = State.AppealClaim(claim.Id, appealForm.Letter ?? string.Empty);
        await FinishAsync(result, result switch
        {
            HcResult.Ok => $"Appealed {claim.Number} to {State.FindPayer(claim.PayerId)?.Name}. Record its answer when it comes.",
            HcResult.TooLate => "The appeal deadline has passed, so it can't be appealed. Write it off or bill the patient.",
            HcResult.WrongState => "This claim isn't waiting for an appeal any more. Nothing was sent.",
            _ => CommonText(result),
        });
    }

    private void Announce(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private string BillText(HcBill? bill) => bill is null ? "No bill" : bill.Status switch
    {
        HcBillStatus.Pending => $"Waiting for the plan ({HcDisplay.Money(bill.AmountCents)} billed)",
        HcBillStatus.Covered => "Nothing to pay",
        HcBillStatus.Paid => $"Paid {HcDisplay.Money(bill.AmountCents)}",
        _ => $"Owes {HcDisplay.Money(bill.AmountCents)}, due {HcDisplay.Date(bill.DueDate)}",
    };

    private async Task CloseAsync()
    {
        if (State.FindClaim(Id) is not { } claim || !closeContext.Validate())
            return;

        var billPatient = closeForm.Choice == BillPatientValue;
        var result = State.CloseClaim(claim.Id, billPatient, closeForm.Reason ?? string.Empty);
        await FinishAsync(result, result switch
        {
            HcResult.Ok when billPatient => $"Closed {claim.Number}: the patient was billed {HcDisplay.Money(claim.BilledCents)} and told about it.",
            HcResult.Ok => $"Wrote off {claim.Number}: the patient owes nothing for this visit.",
            HcResult.WrongState => "Only a denied claim can be closed this way. Nothing changed.",
            _ => CommonText(result),
        });
    }

    private string CommonText(HcResult result) => result switch
    {
        HcResult.NotAllowed => "Only billing can change claims. Sign in again; nothing changed.",
        HcResult.NotFound => "This claim can't be found any more, so nothing changed.",
        HcResult.Invalid => "That couldn't be saved: a value is out of range, or this tab's demo storage is full. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private string DialogTitle(HcClaim claim) => dialog switch
    {
        Dialog.Submit => $"{(claim.Status == HcClaimStatus.Corrected ? "Resubmit" : "Submit")} {claim.Number}?",
        Dialog.Codes => claim.Status is HcClaimStatus.Denied ? $"Correct {claim.Number}" : $"Codes of {claim.Number}",
        Dialog.Response => $"Record {State.FindPayer(claim.PayerId)?.Name}'s answer to {claim.Number}",
        Dialog.Appeal => $"Appeal {claim.Number}",
        Dialog.Close => $"Close {claim.Number}",
        _ => claim.Number,
    };

    /// <summary>Closes the dialog after a step: a success or a refusal that ends the step is announced once it is hidden; other refusals stay in the dialog.</summary>
    private async Task FinishAsync(HcResult result, string message)
    {
        if (result is HcResult.Ok or HcResult.WrongState or HcResult.NotFound or HcResult.NotAllowed or HcResult.TooLate)
        {
            pending = (message, result != HcResult.Ok);
            await modal.HideAsync();

            return;
        }

        dialogErrorCount = dialogError?.StartsWith(message, StringComparison.Ordinal) == true ? dialogErrorCount + 1 : 1;
        dialogError = dialogErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({dialogErrorCount})");
    }

    /// <summary>The next step of a claim for billing, in words.</summary>
    private string NextStepText(HcClaim claim) => claim.Status switch
    {
        HcClaimStatus.Draft => $"Check the codes and submit it to {State.FindPayer(claim.PayerId)?.Name} by {HcDisplay.Date(State.FilingDeadline(claim))}.",
        HcClaimStatus.Corrected => $"Corrected: resubmit version {claim.Versions.Count} by {HcDisplay.Date(State.FilingDeadline(claim))}.",
        HcClaimStatus.Submitted => $"With {State.FindPayer(claim.PayerId)?.Name} since {HcDisplay.Date(claim.SubmittedAt ?? claim.CreatedAt)}. Record its answer when it comes.",
        HcClaimStatus.Appealed => $"Appealed {HcDisplay.Date(claim.AppealedAt ?? claim.CreatedAt)}. Record the plan's decision when it comes.",
        HcClaimStatus.Denied when HcState.IsFinalDenial(claim) => "The appeal was denied too: write it off, or bill the patient.",
        HcClaimStatus.Denied => $"Denied: correct and resubmit it, appeal by {(claim.AppealDeadline is { } deadline ? $"{HcDisplay.Date(deadline)} ({DaysLeftText(deadline, State.Clock.Today)})" : "the deadline")}, write it off, or bill the patient.",
        _ => "Closed.",
    };

    private async Task OnCodedAsync(string message)
    {
        pending = (message, false);
        await modal.HideAsync();
    }

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: it moves to the announcement.</summary>
    private void OnDialogHidden()
    {
        dialog = Dialog.None;

        if (pending is { } outcome)
        {
            pending = null;
            Announce(outcome.Message, outcome.IsError);
        }

        focusStatus = true;
        StateHasChanged();
    }

    private Task OnShownAsync() => HcDisplay.FocusAsync(dialogHeading);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Opens a step's dialog with a fresh form; the plan's answer starts from its usual amounts.</summary>
    private async Task OpenAsync(Dialog next)
    {
        if (State.FindClaim(Id) is not { } claim)
            return;

        dialog = next;
        dialogError = null;
        dialogErrorCount = 0;
        pending = null;

        switch (next)
        {
            case Dialog.Response:
                var payer = State.FindPayer(claim.PayerId);
                var allowed = Math.Round(claim.BilledCents * (payer?.AllowedPercent ?? 70) / 100m / 100m, 0);
                var share = Math.Min((payer?.CopayCents ?? 2_500) / 100m, allowed);
                responseForm = new ResponseForm { Allowed = allowed, Patient = share, Paid = allowed - share, BilledCents = claim.BilledCents, Codes = { "CO-45", "PR-3" } };
                responseContext = new EditContext(responseForm);
                responseContext.OnFieldChanged += (_, args) =>
                {
                    dialogError = null;

                    // Only an outcome change re-checks the codes. Notifying here for any field would raise this
                    // handler again for Codes, and again, until the stack overflows.
                    if (args.FieldIdentifier.FieldName != nameof(ResponseForm.Outcome))
                        return;

                    if (responseForm.Outcome == HcClaimOutcome.Denied)
                    {
                        (responseForm.Allowed, responseForm.Paid, responseForm.Patient) = (0, 0, 0);
                        responseForm.Codes.RemoveAll(code => code.StartsWith("PR-", StringComparison.Ordinal) || code == "CO-45");
                    }

                    responseContext.NotifyFieldChanged(new FieldIdentifier(responseForm, nameof(ResponseForm.Codes)));
                };
                break;

            case Dialog.Appeal:
                appealForm = new AppealForm();
                appealContext = new EditContext(appealForm);
                break;

            case Dialog.Close:
                closeForm = new CloseForm();
                closeContext = new EditContext(closeForm);
                break;
        }

        StateHasChanged();
        await modal.ShowAsync();
    }

    private async Task RecordResponseAsync()
    {
        if (State.FindClaim(Id) is not { } claim)
            return;

        responseForm.BilledCents = claim.BilledCents;

        if (!responseContext.Validate())
            return;

        var (allowed, paid, patient) = (Cents(responseForm.Allowed), Cents(responseForm.Paid), Cents(responseForm.Patient));
        var result = State.RecordClaimResponse(claim.Id, responseForm.Outcome, allowed, paid, patient, responseForm.Codes, responseForm.Remark ?? string.Empty);
        await FinishAsync(result, result switch
        {
            HcResult.Ok when responseForm.Outcome == HcClaimOutcome.Denied => HcState.IsFinalDenial(claim)
                ? $"Recorded the denied appeal of {claim.Number}. Write it off or bill the patient."
                : $"Recorded the denial of {claim.Number}. Correct and resubmit it, or appeal by {HcDisplay.Date(claim.AppealDeadline ?? State.Clock.Today)}.",
            HcResult.Ok => $"Recorded: {State.FindPayer(claim.PayerId)?.Name} paid {HcDisplay.Money(paid)}; the patient's bill is now {HcDisplay.Money(patient)}{(patient == 0 ? ", nothing to pay" : null)}, and they were told.",
            HcResult.WrongState => "This claim isn't waiting for the plan any more. Nothing was recorded.",
            _ => CommonText(result),
        });
    }

    private async Task SubmitAsync()
    {
        if (State.FindClaim(Id) is not { } claim)
            return;

        var resubmit = claim.Status == HcClaimStatus.Corrected;
        var result = State.SubmitClaim(claim.Id);
        await FinishAsync(result, result switch
        {
            HcResult.Ok => $"{(resubmit ? "Resubmitted" : "Submitted")} {claim.Number} to {State.FindPayer(claim.PayerId)?.Name}. Record its answer when it comes.",
            HcResult.TooLate => $"The filing deadline ({HcDisplay.Date(State.FilingDeadline(claim))}) has passed, so the plan won't take it. Write it off or bill the patient.",
            HcResult.WrongState => "This claim was already sent. Nothing changed.",
            HcResult.Invalid => "Check the codes and the member ID first: one of them isn't valid any more. Nothing was sent.",
            _ => CommonText(result),
        });
    }

    private void ToggleCode(string code, bool on)
    {
        if (on && !responseForm.Codes.Contains(code))
            responseForm.Codes.Add(code);
        else if (!on)
            responseForm.Codes.Remove(code);

        responseContext.NotifyFieldChanged(new FieldIdentifier(responseForm, nameof(ResponseForm.Codes)));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the claim ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>An appeal letter, with the deadline the page knows.</summary>
    private sealed class AppealForm : IValidatableObject
    {
        [Required(ErrorMessage = "Write the appeal letter.")]
        [StringLength(HcState.MaxAppealLength, MinimumLength = HcState.MinAppealLength, ErrorMessage = "Write the letter in 50 to 2,000 characters.")]
        public string? Letter { get; set; }

        /// <summary>Whether the appeal deadline has passed, set by the page.</summary>
        public bool PastDeadline { get; set; }

        /// <summary>Runs once the letter is written: the deadline hasn't passed.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PastDeadline)
                yield return new ValidationResult("The appeal deadline has passed: write the claim off or bill the patient instead.", new[] { nameof(Letter) });
        }
    }

    /// <summary>How to close a denied claim, and why.</summary>
    private sealed class CloseForm
    {
        [Required(ErrorMessage = "Choose whether to write it off or bill the patient.")]
        public string Choice { get; set; } = string.Empty;

        [Required(ErrorMessage = "Say why the claim is closed.")]
        [StringLength(HcState.MaxClaimNoteLength, MinimumLength = HcState.MinClaimNoteLength, ErrorMessage = "Give the reason in 10 to 300 characters.")]
        public string? Reason { get; set; }
    }

    /// <summary>A plan's answer as typed, in dollars, checked by <see cref="HcState.ResponseProblem" /> with the billed amount the page sets.</summary>
    private sealed class ResponseForm : IValidatableObject
    {
        public decimal? Allowed { get; set; }

        /// <summary>The claim's billed amount, set by the page.</summary>
        public int BilledCents { get; set; }

        public List<string> Codes { get; set; } = new();

        public HcClaimOutcome Outcome { get; set; }

        public decimal? Paid { get; set; }

        public decimal? Patient { get; set; }

        [StringLength(HcState.MaxClaimNoteLength, ErrorMessage = "Use 300 characters or fewer for the remark.")]
        public string? Remark { get; set; }

        /// <summary>Runs on every change: whole cents, and the amounts and codes the outcome needs.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var (value, member) in new[] { (Allowed, nameof(Allowed)), (Paid, nameof(Paid)), (Patient, nameof(Patient)) })
            {
                if (value is { } amount && (amount < 0 || decimal.Round(amount, 2) != amount))
                {
                    yield return new ValidationResult("Enter dollars and cents, not below 0.", new[] { member });

                    yield break;
                }
            }

            if (Outcome != HcClaimOutcome.Denied && Allowed is null)
            {
                yield return new ValidationResult("Enter the amount the plan allows.", new[] { nameof(Allowed) });

                yield break;
            }

            var problem = HcState.ResponseProblem(Outcome, BilledCents, Cents(Allowed), Cents(Paid), Cents(Patient), Codes);

            if (problem is null)
                yield break;

            var field = problem.Contains("code", StringComparison.OrdinalIgnoreCase) ? nameof(Codes)
                : problem.Contains("pay more", StringComparison.Ordinal) || problem.StartsWith("A denial", StringComparison.Ordinal) ? nameof(Paid)
                : problem.Contains("add up", StringComparison.Ordinal) ? nameof(Patient)
                : nameof(Allowed);

            yield return new ValidationResult(problem, new[] { field });
        }
    }

    #endregion
}
