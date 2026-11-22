using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The coding form of a completed visit, or the correction form of a denied claim, in an <see cref="EditForm" /> (rule
/// WR-002): diagnosis codes (one to <see cref="HcState.MaxDiagnoses" />) and procedure codes (up to
/// <see cref="HcState.MaxProcedures" />, with exactly one office visit code that fits the visit type: a cross-field rule,
/// <see cref="HcState.CodingProblem" />) as checkbox lists with the billed amount they make; for billing's correction the
/// member ID (the plan's letters and nine digits); and for a correction of a denied claim a note of 10 to 300 characters.
/// Saving codes the visit (<see cref="HcState.SaveVisitCoding" />, which creates the draft claim of an insured patient) or
/// corrects the claim (<see cref="HcState.CorrectClaim" />); the outcome goes to <see cref="OnSaved" />, a refusal to the
/// form's alert.
/// </summary>
public partial class HcCodingForm : ComponentBase
{
    #region Fields and Constants

    private EditContext editContext;

    private int errorCount;

    private string? errorMessage;

    private CodingForm form = new();

    /// <summary>The visit and claim version the form was filled from.</summary>
    private string? loadedKey;

    private ValidationMessageStore? serverErrors;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty form.</summary>
    public HcCodingForm() => editContext = new EditContext(form);

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        var key = $"{Appointment.Id}|{Claim?.Id}|{Claim?.Versions.Count}|{Claim?.Status}";

        if (loadedKey == key)
            return;

        loadedKey = key;
        form = new CodingForm
        {
            Diagnoses = (Claim?.DiagnosisCodes ?? Appointment.DiagnosisCodes).ToList(),
            Procedures = (Claim?.ProcedureCodes ?? Appointment.ProcedureCodes).ToList(),
            MemberId = Claim?.MemberId ?? string.Empty,
            VisitType = Appointment.VisitType,
            PayerId = ShowMemberId ? Claim?.PayerId ?? string.Empty : string.Empty,
            NoteRequired = Claim?.Status == HcClaimStatus.Denied,
        };
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) =>
        {
            serverErrors?.Clear(args.FieldIdentifier);
            errorMessage = null;

            if (args.FieldIdentifier.FieldName == nameof(CodingForm.MemberId) && form.MemberId.ToUpperInvariant() != form.MemberId)
                form.MemberId = form.MemberId.ToUpperInvariant();
        };
        errorMessage = null;
    }

    #endregion

    #region Private Methods

    /// <summary>Saves the codes once the form is valid, and maps every result to a field, the alert, or <see cref="OnSaved" />.</summary>
    private async Task SaveAsync()
    {
        serverErrors?.Clear();

        if (!await editContext.ValidateAsync())
            return;

        HcResult result;
        HcClaim? claim = Claim;

        if (ShowMemberId && Claim is not null)
            result = State.CorrectClaim(Claim.Id, form.Diagnoses, form.Procedures, form.MemberId, form.Note ?? string.Empty);
        else
            result = State.SaveVisitCoding(Appointment.Id, form.Diagnoses, form.Procedures, form.Note, out claim);

        claim ??= State.ClaimOfVisit(Appointment.Id);

        switch (result)
        {
            case HcResult.Ok:
                loadedKey = null;
                await OnSaved.InvokeAsync(claim switch
                {
                    null => $"Coded the visit: {string.Join(", ", form.Diagnoses)}; {string.Join(", ", form.Procedures)}. The patient pays their bill themselves (self-pay), so there's no claim.",
                    { Status: HcClaimStatus.Corrected } => $"Corrected {claim.Number}: version {claim.Versions.Count} is ready to resubmit to {State.FindPayer(claim.PayerId)?.Name}.",
                    _ when Claim is null => $"Coded the visit. Claim {claim.Number} ({HcDisplay.Money(claim.BilledCents)}) is a draft for billing to submit to {State.FindPayer(claim.PayerId)?.Name}.",
                    _ => $"Saved the codes of {claim.Number}: billed {HcDisplay.Money(claim.BilledCents)}.",
                });

                return;

            case HcResult.NoChange:
                await OnSaved.InvokeAsync("Nothing changed: these codes are already saved.");

                return;

            case HcResult.WrongState:
                ShowError("The claim is with the plan or closed, so its codes can't change now. Nothing was saved.");

                return;

            case HcResult.Invalid when HcState.CodingProblem(form.Diagnoses, form.Procedures, Appointment.VisitType) is var (diagnoses, procedures) && (diagnoses ?? procedures) is not null:
                if (diagnoses is not null)
                    serverErrors?.Add(FieldIdentifier.Create(() => form.Diagnoses), diagnoses);

                if (procedures is not null)
                    serverErrors?.Add(FieldIdentifier.Create(() => form.Procedures), procedures);

                editContext.NotifyValidationStateChanged();

                return;

            case HcResult.Invalid:
                ShowError("The codes couldn't be saved: check the member ID and the note, or this tab's demo storage is full. Nothing was saved.");

                return;

            case HcResult.NotFound:
                ShowError("This visit or claim can't be found any more, so nothing was saved.");

                return;

            case HcResult.NotAllowed:
                ShowError("Only the visit's doctor or billing can code a visit. Sign in again; nothing was saved.");

                return;

            default:
                ShowError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    private void ShowError(string message)
    {
        errorCount = errorMessage?.StartsWith(message, StringComparison.Ordinal) == true ? errorCount + 1 : 1;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    /// <summary>Ticks or unticks a code; the list is checked again at once.</summary>
    private void Toggle(List<string> list, string code, bool on, string fieldName)
    {
        if (on && !list.Contains(code))
            list.Add(code);
        else if (!on)
            list.Remove(code);

        editContext.NotifyFieldChanged(new FieldIdentifier(form, fieldName));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the visit to code.</summary>
    [Parameter, EditorRequired] public HcAppointment Appointment { get; set; } = default!;

    /// <summary>Gets or sets the visit's claim, or <see langword="null" /> before it has one.</summary>
    [Parameter] public HcClaim? Claim { get; set; }

    /// <summary>Gets or sets the prefix of the form's element IDs.</summary>
    [Parameter] public string IdPrefix { get; set; } = "hc-coding";

    /// <summary>Gets or sets what Cancel does; without it there is no Cancel button.</summary>
    [Parameter] public EventCallback OnCancel { get; set; }

    /// <summary>Gets or sets what happens after a save, with its message.</summary>
    [Parameter] public EventCallback<string> OnSaved { get; set; }

    /// <summary>Gets or sets whether the form corrects the claim's member ID too (billing's correction).</summary>
    [Parameter] public bool ShowMemberId { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The codes as ticked, with the visit type and plan for the cross-field rules.</summary>
    private sealed class CodingForm : IValidatableObject
    {
        public List<string> Diagnoses { get; set; } = new();

        public string MemberId { get; set; } = string.Empty;

        [StringLength(HcState.MaxClaimNoteLength, ErrorMessage = "Use 300 characters or fewer for the note.")]
        public string? Note { get; set; }

        /// <summary>Whether a note is required (correcting a denied claim).</summary>
        public bool NoteRequired { get; set; }

        /// <summary>The plan whose member ID form is checked, or empty when the member ID isn't on the form.</summary>
        public string PayerId { get; set; } = string.Empty;

        public List<string> Procedures { get; set; } = new();

        public HcVisitType VisitType { get; set; }

        /// <summary>Runs on every change: the code rules, the member ID, and the note.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var (diagnoses, procedures) = HcState.CodingProblem(Diagnoses, Procedures, VisitType);

            if (diagnoses is not null)
                yield return new ValidationResult(diagnoses, new[] { nameof(Diagnoses) });

            if (procedures is not null)
                yield return new ValidationResult(procedures, new[] { nameof(Procedures) });

            if (PayerId.Length > 0 && HcState.MemberIdProblem(PayerId, MemberId) is { } member)
                yield return new ValidationResult(member, new[] { nameof(MemberId) });

            if (NoteRequired && (Note?.Trim().Length ?? 0) < HcState.MinClaimNoteLength)
                yield return new ValidationResult("Say what you corrected, in 10 to 300 characters: the plan's denial is answered with it.", new[] { nameof(Note) });
        }
    }

    #endregion
}
