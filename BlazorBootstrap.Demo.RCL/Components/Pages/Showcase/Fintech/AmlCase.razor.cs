using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One AML case (operations): the customer, priority, status, due date (with Overdue), and analyst; the timeline of events,
/// notes, and named evidence; the linked alerts with their triggering transfers; and the actions for where it is, each an
/// <see cref="EditForm" /> (rule WR-002). While open or investigating: start investigating, add a note (10–2,000 characters),
/// add evidence (a name unique on the case, checked as you type, and what it shows), escalate to a senior reviewer other than
/// yourself with a reason and an optional proposal (required to report: Reported needs a SAR filing reference like
/// SAR-2026-00500 and a closing summary), or close with a disposition and a closing summary (not Reported: that always needs
/// a senior reviewer). While escalated: a senior reviewer who didn't escalate it approves and closes it (the proposal is
/// filled in) or returns it to the analyst with a reason; anyone else is told why they can't (four-eyes rule). The state's
/// refusals are announced in the status message. An unknown number shows a not-found screen.
/// </summary>
public partial class AmlCase : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext closeContext = default!;

    private CloseForm closeForm = new();

    private EditContext escalateContext = default!;

    private EscalateForm escalateForm = new();

    private ShowcaseUniqueCheck? evidenceCheck;

    private EditContext evidenceContext = default!;

    private EvidenceForm evidenceForm = new();

    private bool focusStatus;

    private int? loadedId;

    private EditContext noteContext = default!;

    private NoteForm noteForm = new();

    private int noteLength;

    private EditContext returnContext = default!;

    private ReturnForm returnForm = new();

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ElementReference? timelineTitle;

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

    private static string KindText(FintechAmlEventKind kind) => kind switch
    {
        FintechAmlEventKind.Opened => "Opened",
        FintechAmlEventKind.Investigating => "Investigation started",
        FintechAmlEventKind.Note => "Note",
        FintechAmlEventKind.Evidence => "Evidence",
        FintechAmlEventKind.Assigned => "Assigned",
        FintechAmlEventKind.Escalated => "Escalated",
        FintechAmlEventKind.Returned => "Returned to the analyst",
        _ => "Closed",
    };

    private async Task AddEvidenceAsync()
    {
        var free = evidenceCheck is null || await evidenceCheck.CheckNowAsync();

        if (Case is not { } amlCase || !await evidenceContext.ValidateAsync() || !free)
            return;

        var result = State.AddCaseEvidence(amlCase.Id, evidenceForm.Name, evidenceForm.Description);
        Report(result == FintechResult.Ok ? $"Evidence \"{evidenceForm.Name.Trim()}\" is on the timeline." : AmlDisplay.ResultText(result), result);
    }

    private async Task AddNote()
    {
        if (Case is not { } amlCase || !await noteContext.ValidateAsync())
            return;

        var result = State.AddCaseNote(amlCase.Id, noteForm.Text);
        Report(result == FintechResult.Ok ? "The note is on the timeline." : AmlDisplay.ResultText(result), result);
    }

    private async Task CloseAsync()
    {
        if (Case is not { } amlCase)
            return;

        closeForm.Approving = amlCase.Status == FintechAmlCaseStatus.Escalated;

        if (!await closeContext.ValidateAsync())
            return;

        var disposition = closeForm.Disposition!.Value;
        var result = State.CloseCase(amlCase.Id, disposition, disposition == FintechAmlDisposition.Reported ? closeForm.Filing : null, closeForm.Summary);
        Report(result == FintechResult.Ok
            ? $"Case {amlCase.Id.ToString(CultureInfo.InvariantCulture)} is closed: {FintechSeedData.DispositionName(disposition)}. The analyst is told; the customer never is."
            : AmlDisplay.ResultText(result), result);
    }

    private async Task Escalate()
    {
        if (Case is not { } amlCase || !await escalateContext.ValidateAsync())
            return;

        var proposal = escalateForm.Proposal;
        var result = State.EscalateCase(amlCase.Id, escalateForm.ReviewerId, escalateForm.Reason, proposal,
            proposal == FintechAmlDisposition.Reported ? escalateForm.Filing : null, proposal is null ? null : escalateForm.Summary);
        Report(result == FintechResult.Ok
            ? $"Case {amlCase.Id.ToString(CultureInfo.InvariantCulture)} is escalated to {AmlDisplay.StaffText(escalateForm.ReviewerId)}, who is told."
            : AmlDisplay.ResultText(result), result);
    }

    private string EvidenceTakenMessage(string name) => $"This case already has evidence called \"{name.Trim()}\". Use a different name.";

    private Task<bool> IsEvidenceTakenAsync(string name) => Task.FromResult(State.IsEvidenceNameTaken(Id, name));

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Announces an action's outcome (a repeat gets a counter) and starts the forms over after a success.</summary>
    private void Report(string message, FintechResult result)
    {
        statusMessage = message == statusMessage ? $"{message} (2)" : message;
        statusIsError = result != FintechResult.Ok;
        focusStatus = true;

        if (result == FintechResult.Ok)
            ResetForms();
    }

    /// <summary>Starts every form over; an escalated case's approval form starts from the analyst's proposal.</summary>
    private void ResetForms()
    {
        var amlCase = Case;
        closeForm = new CloseForm
        {
            Disposition = amlCase?.ProposedDisposition,
            Filing = amlCase?.ProposedDisposition == FintechAmlDisposition.Reported ? amlCase.FilingReference ?? string.Empty : string.Empty,
            Summary = amlCase?.ProposedSummary ?? string.Empty,
        };
        closeContext = new EditContext(closeForm);
        escalateForm = new EscalateForm { Me = State.CurrentStaffId ?? string.Empty, ReviewerId = FintechSeedData.Staff().FirstOrDefault(member => member.IsSeniorReviewer && member.Id != State.CurrentStaffId)?.Id ?? string.Empty };
        escalateContext = new EditContext(escalateForm);
        evidenceForm = new EvidenceForm();
        evidenceContext = new EditContext(evidenceForm);
        noteForm = new NoteForm();
        noteContext = new EditContext(noteForm);
        noteLength = 0;
        returnForm = new ReturnForm();
        returnContext = new EditContext(returnForm);
    }

    private async Task ReturnCase()
    {
        if (Case is not { } amlCase || !await returnContext.ValidateAsync())
            return;

        var result = State.ReturnCase(amlCase.Id, returnForm.Reason);
        Report(result == FintechResult.Ok
            ? $"Case {amlCase.Id.ToString(CultureInfo.InvariantCulture)} is back with {AmlDisplay.StaffText(amlCase.AssigneeId)}, who is told."
            : AmlDisplay.ResultText(result), result);
    }

    private void StartInvestigation()
    {
        if (Case is not { } amlCase)
            return;

        var result = State.StartInvestigation(amlCase.Id);
        Report(result == FintechResult.Ok ? "The investigation is started." : AmlDisplay.ResultText(result), result);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private FintechAmlCase? Case => State.FindAmlCase(Id);

    /// <summary>Gets or sets the case number from the route.</summary>
    [Parameter] public int Id { get; set; }

    /// <summary>Gets the case's alerts, oldest first.</summary>
    private IReadOnlyList<FintechAmlAlert> LinkedAlerts => Case is not { } amlCase
        ? Array.Empty<FintechAmlAlert>()
        : amlCase.AlertIds.Select(State.FindAmlAlert).OfType<FintechAmlAlert>().OrderBy(alert => alert.CreatedAt).ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The close form: a disposition, a SAR filing reference for Reported (SAR-yyyy-nnnnn), and a closing summary of 20 to
    /// 2,000 characters. Cross-field: Reported is only allowed when a senior reviewer approves an escalated case.
    /// </summary>
    private sealed class CloseForm : IValidatableObject
    {
        /// <summary>Whether a senior reviewer is deciding an escalated case; set by the page.</summary>
        public bool Approving { get; set; }

        [Required(ErrorMessage = "Choose how the case ends.")]
        public FintechAmlDisposition? Disposition { get; set; }

        public string Filing { get; set; } = string.Empty;

        [Required(ErrorMessage = "Write the closing summary.")]
        [StringLength(FintechState.MaxAmlSummary, MinimumLength = FintechState.MinAmlSummary, ErrorMessage = "Write a closing summary of 20 to 2,000 characters.")]
        public string Summary { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Disposition != FintechAmlDisposition.Reported)
                yield break;

            if (!Approving)
                yield return new ValidationResult("Reporting always needs a senior reviewer's approval. Escalate the case instead, proposing Reported.", new[] { nameof(Disposition) });
            else if (!FintechState.IsFilingReference(Filing))
                yield return new ValidationResult("Enter the SAR filing reference like SAR-2026-00500.", new[] { nameof(Filing) });
        }
    }

    /// <summary>
    /// The escalate form: a senior reviewer other than yourself, a reason of 10 to 500 characters, and an optional proposal
    /// (cross-field: a proposal needs a closing summary, and Reported a filing reference).
    /// </summary>
    private sealed class EscalateForm : IValidatableObject
    {
        public string Filing { get; set; } = string.Empty;

        /// <summary>The signed-in staff member; set by the page.</summary>
        public string Me { get; set; } = string.Empty;

        public FintechAmlDisposition? Proposal { get; set; }

        [Required(ErrorMessage = "Write why you're escalating the case.")]
        [StringLength(FintechState.MaxAmlReason, MinimumLength = FintechState.MinAmlNote, ErrorMessage = "Write a reason of 10 to 500 characters.")]
        public string Reason { get; set; } = string.Empty;

        public string ReviewerId { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FintechSeedData.Staff().FirstOrDefault(member => member.Id == ReviewerId) is not { IsSeniorReviewer: true })
                yield return new ValidationResult("Choose a senior reviewer.", new[] { nameof(ReviewerId) });
            else if (ReviewerId == Me)
                yield return new ValidationResult("Four-eyes rule: choose a senior reviewer other than yourself.", new[] { nameof(ReviewerId) });

            if (Proposal is null)
                yield break;

            if (Summary.Trim().Length is < FintechState.MinAmlSummary or > FintechState.MaxAmlSummary)
                yield return new ValidationResult("Write the proposed closing summary, 20 to 2,000 characters.", new[] { nameof(Summary) });

            if (Proposal == FintechAmlDisposition.Reported && !FintechState.IsFilingReference(Filing))
                yield return new ValidationResult("Enter the SAR filing reference like SAR-2026-00500.", new[] { nameof(Filing) });
        }
    }

    /// <summary>The evidence form: a name of 3 to 80 characters (unique on the case) and what it shows, 10 to 500 characters.</summary>
    private sealed class EvidenceForm
    {
        [Required(ErrorMessage = "Enter the evidence's name, like \"Bank statements, 12 months\".")]
        [StringLength(FintechState.MaxEvidenceName, MinimumLength = 3, ErrorMessage = "Use 3 to 80 characters for the name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Say what the evidence shows.")]
        [StringLength(FintechState.MaxAmlReason, MinimumLength = FintechState.MinAmlNote, ErrorMessage = "Describe it in 10 to 500 characters.")]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>The note form: 10 to 2,000 characters.</summary>
    private sealed class NoteForm
    {
        [Required(ErrorMessage = "Write the note first.")]
        [StringLength(FintechState.MaxAmlNote, MinimumLength = FintechState.MinAmlNote, ErrorMessage = "Write a note of 10 to 2,000 characters.")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>The return form: what the analyst should do next, 10 to 500 characters.</summary>
    private sealed class ReturnForm
    {
        [Required(ErrorMessage = "Write what the analyst should do next.")]
        [StringLength(FintechState.MaxAmlReason, MinimumLength = FintechState.MinAmlNote, ErrorMessage = "Write a reason of 10 to 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }

    #endregion
}
