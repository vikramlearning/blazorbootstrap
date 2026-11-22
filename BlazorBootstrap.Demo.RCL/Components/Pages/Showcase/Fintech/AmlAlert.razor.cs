using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One AML alert (operations): the rule, risk and score, status, and assignee; what the rule saw and how the rule works;
/// the triggering transfers (linked to their operations pages); the customer with their other open alerts; and, while it's
/// open, the triage: take it (it moves to In triage under your name), open a case from it for an analyst, or dismiss it as
/// a false positive with a reason. Both forms are <see cref="EditForm" />s (rule WR-002). A dismissed alert shows why, and a
/// linked one its case. An unknown number shows a not-found screen.
/// </summary>
public partial class AmlAlert : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext caseContext = default!;

    private CaseForm caseForm = new();

    private EditContext dismissContext = default!;

    private DismissForm dismissForm = new();

    private bool focusStatus;

    private int? loadedId;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ElementReference? triageTitle;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await FintechDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = null;
        ResetForms();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Explains how a rule works, for the analyst deciding whether it's a false positive.</summary>
    private static string RuleHelp(FintechAmlRule rule) => rule switch
    {
        FintechAmlRule.Structuring => $"Structuring: three or more payments to payees of {FintechDisplay.Money(FintechState.StructuringFloor)} to just under {FintechDisplay.Money(FintechState.HoldLargeAmount)} within 7 days, as if to stay under the check threshold.",
        FintechAmlRule.RapidMovement => "Rapid movement: a large amount comes in and most of it goes out again within 24 hours, a common sign of a money mule account.",
        FintechAmlRule.NewPayeeLargePayment => $"New payee: a first payment of {FintechDisplay.Money(FintechState.HoldLargeAmount)} or more to a payee added in the last {FintechState.NewPayeeDays} days.",
        FintechAmlRule.HighRiskJurisdiction => "High-risk jurisdiction: a payment to or from a bank on the bank's high-risk jurisdiction list.",
        _ => "Velocity spike: money out in a week far above the customer's 90-day weekly average.",
    };

    private async Task Dismiss()
    {
        if (Alert is not { } alert || !await dismissContext.ValidateAsync())
            return;

        var outcome = State.DismissAlerts(new[] { alert.Id }, dismissForm.Reason.Trim());
        Report(outcome.Result == FintechResult.Ok ? $"Alert {alert.Id.ToString(CultureInfo.InvariantCulture)} is dismissed as a false positive." : AmlDisplay.ResultText(outcome.Result),
            outcome.Result != FintechResult.Ok);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenCaseAsync()
    {
        if (Alert is not { } alert || !await caseContext.ValidateAsync())
            return;

        var (result, amlCase) = State.OpenCase(new[] { alert.Id }, caseForm.AssigneeId);

        if (result == FintechResult.Ok && amlCase is not null)
            Navigation.NavigateTo(AmlDisplay.CasePath(amlCase.Id));
        else
            Report(AmlDisplay.ResultText(result), true);
    }

    /// <summary>Says how many other open alerts the customer has, so the analyst can open one case from them together.</summary>
    private string OtherAlertsText(FintechAmlAlert alert)
    {
        var others = State.AmlAlerts.Count(other => other.CustomerId == alert.CustomerId && other.Id != alert.Id && other.Status is FintechAmlAlertStatus.New or FintechAmlAlertStatus.InTriage);
        var cases = State.AmlCases.Count(amlCase => amlCase.CustomerId == alert.CustomerId && amlCase.Status != FintechAmlCaseStatus.Closed);

        return $"{(others == 0 ? "No other open alerts" : others == 1 ? "1 other open alert" : $"{others.ToString(CultureInfo.InvariantCulture)} other open alerts")}, "
            + $"{(cases == 0 ? "no open cases" : cases == 1 ? "1 open case" : $"{cases.ToString(CultureInfo.InvariantCulture)} open cases")}.";
    }

    private void Report(string message, bool isError)
    {
        statusMessage = message == statusMessage ? $"{message} (2)" : message;
        statusIsError = isError;
        focusStatus = true;
        ResetForms();
    }

    private void ResetForms()
    {
        caseForm = new CaseForm { AssigneeId = State.CurrentStaffId ?? string.Empty };
        caseContext = new EditContext(caseForm);
        dismissForm = new DismissForm();
        dismissContext = new EditContext(dismissForm);
    }

    private void StartTriage()
    {
        if (Alert is not { } alert)
            return;

        var result = State.StartAlertTriage(alert.Id);
        Report(result == FintechResult.Ok ? $"Alert {alert.Id.ToString(CultureInfo.InvariantCulture)} is yours, in triage." : AmlDisplay.ResultText(result), result != FintechResult.Ok);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private FintechAmlAlert? Alert => State.FindAmlAlert(Id);

    /// <summary>Gets or sets the alert number from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private FintechState State { get; set; } = default!;

    /// <summary>Gets the alert's triggering transfers, newest first.</summary>
    private IReadOnlyList<FintechTransfer> Triggers => Alert is not { } alert
        ? Array.Empty<FintechTransfer>()
        : alert.TransferReferences.Select(State.FindAnyTransfer).OfType<FintechTransfer>().OrderByDescending(transfer => transfer.CreatedAt).ToList();

    #endregion

    #region Nested Types

    /// <summary>The open-case form's field: the analyst.</summary>
    private sealed class CaseForm : IValidatableObject
    {
        public string AssigneeId { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FintechSeedData.StaffName(AssigneeId) is null)
                yield return new ValidationResult("Choose the analyst who works the case.", new[] { nameof(AssigneeId) });
        }
    }

    /// <summary>The dismiss form's field: why it's a false positive, 10 to 500 characters.</summary>
    private sealed class DismissForm
    {
        [Required(ErrorMessage = "Write why this alert is a false positive.")]
        [StringLength(FintechState.MaxAmlReason, MinimumLength = FintechState.MinAmlNote, ErrorMessage = "Write a reason of 10 to 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }

    #endregion
}
