using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// One visit for its doctor. The header names the patient with age, record number, health plan, a link to the chart, and
/// the allergies prominently, then the reason, status, and doctor. While the visit is in progress or completed the doctor
/// works in <see cref="EditForm" />s (rule WR-002) with a linked error summary each: notes (staff only, up to 4,000
/// characters) and the summary for the patient (up to 2,000) (<see cref="HcState.SaveVisitNotes" />); vital signs, each
/// optional with its unit — blood pressure both or neither, systolic 60 to 250 above diastolic 30 to 150 mmHg (a
/// cross-field rule), pulse 30 to 220, temperature 34 to 42 °C, weight 1 to 400 kg (<see cref="HcState.SaveVitals" />); a lab
/// test (required, not already ordered in this visit; <see cref="HcState.OrderLab" />); and a prescription — medication,
/// dose, and directions required with lengths, quantity 1 to 365, days per fill 1 to 90, refills 0 to 11, and all fills
/// together at most a year (a cross-field rule; <see cref="HcState.Prescribe" />). A checked-in visit offers Start visit; a
/// booked one says the patient isn't checked in yet. Complete visit asks first, saves unsaved notes, and completes once the
/// dialog is hidden (<see cref="HcState.CompleteVisit" />). A completed visit is coded here (<see cref="HcCodingForm" />),
/// which creates the insurance claim of a patient with a health plan; its claim's status and a denial's reasons show above
/// the codes, and a denied claim's codes are corrected with a note. Another doctor's visit is read-only.
/// </summary>
public partial class Visit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Prescriptions written on this page, listed with the visit even when written on a later day.</summary>
    private readonly HashSet<int> prescribedHere = new();

    /// <summary>The visit this page was set up for.</summary>
    private int builtFor;

    private ElementReference? completeButton;

    private Modal completeModal = default!;

    /// <summary>The visit whose stored notes, summary, and vitals are loaded into the forms, or <see langword="null" />.</summary>
    private int? draftsFor;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private EditContext labContext;

    private LabForm labForm = new();

    private ValidationMessageStore? labServerErrors;

    private EditContext notesContext;

    private NotesForm notesForm = new();

    /// <summary>Whether Complete visit was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingComplete;

    private EditContext rxContext;

    private RxForm rxForm = new();

    private ElementReference? startButton;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private EditContext vitalsContext;

    private VitalsForm vitalsForm = new();

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with empty forms.</summary>
    public Visit()
    {
        notesContext = new EditContext(notesForm);
        vitalsContext = new EditContext(vitalsForm);
        labContext = new EditContext(labForm);
        rxContext = new EditContext(rxForm);
    }

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HcDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (builtFor != Id)
        {
            builtFor = Id;
            draftsFor = null;
            statusMessage = null;
            prescribedHere.Clear();
            SetNotesForm(new NotesForm());
            SetVitalsForm(new VitalsForm());
            SetLabForm();
            SetRxForm();
        }

        SyncDrafts();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string BillText(HcBill bill) => bill.Status switch
    {
        HcBillStatus.Pending => $"The bill ({HcDisplay.Money(bill.AmountCents)}) waits for the health plan's answer to the claim.",
        HcBillStatus.Covered => "Nothing for the patient to pay after insurance.",
        HcBillStatus.Paid => $"Billed {HcDisplay.Money(bill.AmountCents)}: paid{(bill.PaidAt is { } paidAt ? $" {HcDisplay.Date(paidAt)}" : null)}.",
        _ => $"Billed {HcDisplay.Money(bill.AmountCents)}, due {HcDisplay.Date(bill.DueDate)}: not paid yet.",
    };

    private static string CompletedText(HcAppointment appointment, string name) =>
        (appointment.CompletedAt is { } at ? $"Completed {HcDisplay.When(at)}." : "Completed.") + $" {name} sees your summary in their records.";

    /// <summary>Returns the visit fee the way <see cref="HcState.CompleteVisit" /> bills it.</summary>
    private static int FeeCents(HcDoctor? doctor) =>
        doctor?.Specialty is "Family medicine" or "Pediatrics" ? HcSeedData.PrimaryCareFeeCents : HcSeedData.SpecialistFeeCents;

    private static string FeeLabel(HcDoctor? doctor) => FeeCents(doctor) == HcSeedData.PrimaryCareFeeCents ? "primary care" : "specialist";

    private static string ScheduleUrl(DateTime start) => $"{HcStaffLayout.Base}/schedule?date={start.ToString(HcDisplay.QueryDate, CultureInfo.InvariantCulture)}";

    private static string? StatusDetail(HcAppointment appointment) => appointment.Status switch
    {
        HcAppointmentStatus.Booked => "Waiting for check-in",
        HcAppointmentStatus.CheckedIn => appointment.CheckedInAt is { } at ? $"Checked in at {HcDisplay.Time(at)}, waiting" : "Checked in, waiting",
        HcAppointmentStatus.InVisit => "Visit in progress",
        HcAppointmentStatus.Completed => appointment.CompletedAt is { } done ? $"Completed {HcDisplay.When(done)}" : null,
        HcAppointmentStatus.Cancelled => appointment.CancelledAt is { } cancelled ? $"Cancelled {HcDisplay.When(cancelled)}" : null,
        _ => "The patient didn't come",
    };

    /// <summary>Returns vital signs in words, like "Blood pressure 128/82 mmHg · pulse 72 · 36.8 °C · 74.5 kg".</summary>
    internal static string VitalsText(HcVitals vitals) => string.Join(" · ", new[]
    {
        vitals.Systolic is { } systolic && vitals.Diastolic is { } diastolic ? string.Create(CultureInfo.InvariantCulture, $"Blood pressure {systolic}/{diastolic} mmHg") : null,
        vitals.Pulse is { } pulse ? string.Create(CultureInfo.InvariantCulture, $"pulse {pulse} a minute") : null,
        vitals.TemperatureC is { } temperature ? string.Create(CultureInfo.InvariantCulture, $"{temperature:0.0} °C") : null,
        vitals.WeightKg is { } weight ? string.Create(CultureInfo.InvariantCulture, $"{weight:0.0} kg") : null,
    }.Where(item => item is not null));

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private async Task ConfirmCompleteAsync()
    {
        pendingComplete = true;

        await completeModal.HideAsync();
    }

    private string NotesFailureText(HcResult result) => result switch
    {
        HcResult.Invalid => "The storage for this demo tab is full. Try again after removing something.",
        HcResult.WrongState => "The visit isn't in progress or completed, so notes can't be written.",
        HcResult.NotFound => "This visit isn't on your schedule any more.",
        HcResult.NotAllowed => "Only the visit's doctor can write notes. Sign in again.",
        _ => "Try again.",
    };

    /// <summary>Saves unsaved notes, then completes a confirmed visit now that the dialog is hidden.</summary>
    private Task OnCompleteHiddenAsync()
    {
        var accepted = pendingComplete;
        pendingComplete = false;

        if (!accepted)
        {
            focusTarget = () => completeButton;
            StateHasChanged();

            return Task.CompletedTask;
        }

        focusTarget = () => statusRegion;

        if (Own() is not { } appointment)
        {
            Announce(false, "This visit isn't on your schedule any more, so it wasn't completed.");
            StateHasChanged();

            return Task.CompletedTask;
        }

        var name = State.FindPatient(appointment.PatientId)?.Name ?? "The patient";
        var notes = (notesForm.Notes ?? string.Empty).Trim();
        var summary = (notesForm.Summary ?? string.Empty).Trim();

        if (notes != appointment.Notes || summary != appointment.Summary)
        {
            if (!notesContext.Validate())
            {
                Announce(false, "The visit wasn't completed: fix the notes, save them, then complete it.");
                StateHasChanged();

                return Task.CompletedTask;
            }

            var saved = State.SaveVisitNotes(appointment.Id, notes, summary);

            if (saved is not (HcResult.Ok or HcResult.NoChange))
            {
                Announce(false, $"The visit wasn't completed because your notes couldn't be saved. {NotesFailureText(saved)}");
                StateHasChanged();

                return Task.CompletedTask;
            }
        }

        var result = State.CompleteVisit(appointment.Id, out var bill);

        Announce(result == HcResult.Ok, result switch
        {
            HcResult.Ok when bill is { Status: HcBillStatus.Pending } => $"The visit with {name} is completed. They see your summary in their records; code the visit below so billing can send the claim.",
            HcResult.Ok when bill is not null => $"The visit with {name} is completed. They see your summary in their records and were billed {HcDisplay.Money(bill.AmountCents)}, due {HcDisplay.Date(bill.DueDate)}.",
            HcResult.Ok => $"The visit with {name} is completed.",
            HcResult.WrongState when appointment.Status == HcAppointmentStatus.Completed => "This visit was already completed.",
            HcResult.WrongState => "This visit isn't in progress, so it can't be completed.",
            HcResult.Invalid => "The storage for this demo tab is full, so the visit wasn't completed and nothing was billed. Try again after removing something.",
            HcResult.NotFound => "This visit isn't on your schedule any more, so it wasn't completed.",
            HcResult.NotAllowed => "Only the visit's doctor can complete it. Sign in again; nothing changed.",
            _ => "That didn't work, and the visit wasn't completed. Try again.",
        });
        StateHasChanged();

        return Task.CompletedTask;
    }

    /// <summary>The coding form saved: its message is announced here.</summary>
    private void OnCoded(string message)
    {
        Announce(true, message);
        focusTarget = () => statusRegion;
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        SyncDrafts();
        StateHasChanged();
    });

    private async Task OpenCompleteAsync()
    {
        pendingComplete = false;

        await completeModal.ShowAsync();
    }

    /// <summary>Orders the chosen lab test in this visit once the form is valid.</summary>
    private void OrderLab()
    {
        if (Own() is not { } appointment)
            return;

        labServerErrors?.Clear();
        labForm.AlreadyOrdered = State.LabOrdersOf(appointment.PatientId).Where(item => item.AppointmentId == appointment.Id).Select(item => item.PanelId).ToList();

        if (!labContext.Validate() || State.LabPanel(labForm.PanelId) is not { } panel)
            return;

        var result = State.OrderLab(appointment.Id, panel.Id, out _);

        switch (result)
        {
            case HcResult.Ok:
                Announce(true, $"Ordered {panel.Name}. Enter the results under Lab results when they come back.");
                SetLabForm();
                focusTarget = () => statusRegion;

                return;

            case HcResult.Conflict:
                labServerErrors?.Add(FieldIdentifier.Create(() => labForm.PanelId), $"{panel.Name} is already ordered in this visit. Choose another test.");
                labContext.NotifyValidationStateChanged();

                return;

            default:
                Announce(false, result switch
                {
                    HcResult.WrongState => "Start the visit before ordering lab tests. Nothing was ordered.",
                    HcResult.Invalid => "The storage for this demo tab is full, so the test wasn't ordered. Try again after removing something.",
                    HcResult.NotFound => "That test or this visit no longer exists, so nothing was ordered.",
                    HcResult.NotAllowed => "Only the visit's doctor can order tests. Sign in again; nothing was ordered.",
                    _ => "That didn't work, and nothing was ordered. Try again.",
                });
                focusTarget = () => statusRegion;

                return;
        }
    }

    /// <summary>The signed-in doctor's own visit, or <see langword="null" />.</summary>
    private HcAppointment? Own() =>
        State.FindAppointment(Id) is { } appointment && State.CurrentDoctor is { } doctor && appointment.DoctorId == doctor.Id ? appointment : null;

    /// <summary>Prescribes in this visit once the form is valid.</summary>
    private void Prescribe()
    {
        if (Own() is not { } appointment || !rxContext.Validate())
            return;

        var name = State.FindPatient(appointment.PatientId)?.Name ?? "The patient";
        var (medication, dose, directions) = (rxForm.Medication.Trim(), rxForm.Dose.Trim(), (rxForm.Directions ?? string.Empty).Trim());
        var (quantity, refills, days) = (rxForm.Quantity!.Value, rxForm.Refills!.Value, rxForm.DaysSupply!.Value);
        var result = State.Prescribe(appointment.Id, medication, dose, directions, quantity, refills, days, out var prescription);

        Announce(result == HcResult.Ok, result switch
        {
            HcResult.Ok => $"Prescribed {medication} {dose}: quantity {quantity}, {days} days per fill, {PatientChart.RefillsText(refills)}. {name} sees it under Prescriptions.",
            HcResult.Invalid => "The storage for this demo tab is full, so nothing was prescribed. Try again after removing something.",
            HcResult.WrongState => "Start the visit before prescribing. Nothing was prescribed.",
            HcResult.NotFound => "This visit isn't on your schedule any more, so nothing was prescribed.",
            HcResult.NotAllowed => "Only the visit's doctor can prescribe. Sign in again; nothing was prescribed.",
            _ => "That didn't work, and nothing was prescribed. Try again.",
        });

        if (result == HcResult.Ok && prescription is not null)
        {
            prescribedHere.Add(prescription.Id);
            SetRxForm();
        }

        focusTarget = () => statusRegion;
    }

    /// <summary>Saves the notes and the summary for the patient once the form is valid.</summary>
    private void SaveNotes()
    {
        if (Own() is not { } appointment || !notesContext.Validate())
            return;

        var result = State.SaveVisitNotes(appointment.Id, notesForm.Notes ?? string.Empty, notesForm.Summary ?? string.Empty);

        if (result == HcResult.Ok)
        {
            SetNotesForm(new NotesForm { Notes = appointment.Notes, Summary = appointment.Summary });
            Announce(true, appointment.Status == HcAppointmentStatus.Completed
                ? "Saved. The patient sees the updated summary in their records."
                : "Saved the notes and the summary. The patient sees the summary once you complete the visit.");
        }
        else
        {
            Announce(result == HcResult.NoChange, result == HcResult.NoChange ? "Nothing changed since the last save." : $"The notes weren't saved. {NotesFailureText(result)}");
        }

        focusTarget = () => statusRegion;
    }

    /// <summary>Saves the vital signs once the form is valid.</summary>
    private void SaveVitals()
    {
        if (Own() is not { } appointment || !vitalsContext.Validate())
            return;

        var vitals = new HcVitals { Systolic = vitalsForm.Systolic, Diastolic = vitalsForm.Diastolic, Pulse = vitalsForm.Pulse, TemperatureC = vitalsForm.TemperatureC, WeightKg = vitalsForm.WeightKg };
        var result = State.SaveVitals(appointment.Id, vitals);

        Announce(result is HcResult.Ok or HcResult.NoChange, result switch
        {
            HcResult.Ok => $"Saved the vital signs: {VitalsText(appointment.Vitals ?? vitals)}.",
            HcResult.NoChange => "Nothing changed: these vital signs are already saved.",
            HcResult.Invalid => "The vital signs couldn't be saved: a value is out of range, or this tab's demo storage is full.",
            HcResult.WrongState => "Start the visit before taking vital signs. Nothing was saved.",
            HcResult.NotAllowed => "Only the visit's doctor can record vital signs. Sign in again; nothing was saved.",
            _ => "That didn't work, and nothing was saved. Try again.",
        });
        focusTarget = () => statusRegion;
    }

    private void SetLabForm()
    {
        labForm = new LabForm();
        labContext = new EditContext(labForm);
        labServerErrors = new ValidationMessageStore(labContext);
        labContext.OnFieldChanged += (_, args) => labServerErrors?.Clear(args.FieldIdentifier);
    }

    private void SetNotesForm(NotesForm next)
    {
        notesForm = next;
        notesContext = new EditContext(notesForm);
    }

    private void SetRxForm()
    {
        rxForm = new RxForm();
        rxContext = new EditContext(rxForm);
    }

    private void SetVitalsForm(VitalsForm next)
    {
        vitalsForm = next;
        vitalsContext = new EditContext(vitalsForm);
    }

    /// <summary>Starts the visit with the checked-in patient.</summary>
    private void StartVisit()
    {
        var appointment = State.FindAppointment(Id);
        var result = State.StartVisit(Id);

        if (result == HcResult.Ok)
        {
            draftsFor = null;
            SyncDrafts();
            Announce(true, "The visit has started. Take the vital signs, and write your notes and the summary for the patient below.");
            focusTarget = () => statusRegion;

            return;
        }

        Announce(false, result switch
        {
            HcResult.WrongState when appointment?.Status is HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed => "The visit has already started.",
            HcResult.WrongState => "The patient isn't checked in any more, so the visit can't start. Ask the front desk.",
            HcResult.NotFound => "This visit isn't on your schedule any more.",
            HcResult.NotAllowed => "Only the visit's doctor can start it. Sign in again.",
            _ => "That didn't work, and the visit didn't start. Try again.",
        });
        focusTarget = () => appointment?.Status == HcAppointmentStatus.CheckedIn ? startButton : statusRegion;
    }

    /// <summary>Loads the stored notes, summary, and vitals into the forms once the doctor's own visit is available.</summary>
    private void SyncDrafts()
    {
        if (draftsFor == Id || Own() is not { } appointment)
            return;

        draftsFor = Id;
        SetNotesForm(new NotesForm { Notes = appointment.Notes, Summary = appointment.Summary });
        SetVitalsForm(appointment.Vitals is { } vitals
            ? new VitalsForm { Systolic = vitals.Systolic, Diastolic = vitals.Diastolic, Pulse = vitals.Pulse, TemperatureC = vitals.TemperatureC, WeightKg = vitals.WeightKg }
            : new VitalsForm());
    }

    /// <summary>
    /// Returns the prescriptions of this visit: written on this page, or by the visit's doctor on the visit's day after
    /// check-in (prescriptions don't record their visit).
    /// </summary>
    private IReadOnlyList<HcPrescription> VisitPrescriptions(HcAppointment appointment)
    {
        var from = appointment.CheckedInAt ?? appointment.Start;

        return State.PrescriptionsOf(appointment.PatientId)
            .Where(item => prescribedHere.Contains(item.Id) || (item.DoctorId == appointment.DoctorId && item.PrescribedAt >= from && item.PrescribedAt.Date == appointment.Start.Date))
            .OrderBy(item => item.PrescribedAt)
            .ToList();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the appointment ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A lab test to order: required, and not one already ordered in this visit.</summary>
    private sealed class LabForm : IValidatableObject
    {
        /// <summary>The tests already ordered in this visit, set by the page before it validates.</summary>
        public IReadOnlyList<string> AlreadyOrdered { get; set; } = Array.Empty<string>();

        [Required(ErrorMessage = "Choose a lab test to order.")]
        public string PanelId { get; set; } = string.Empty;

        /// <summary>Runs once a test is chosen: it isn't ordered in this visit yet.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AlreadyOrdered.Contains(PanelId))
                yield return new ValidationResult("This test is already ordered in this visit. Choose another test.", new[] { nameof(PanelId) });
        }
    }

    /// <summary>The visit notes and the summary for the patient, with the lengths of <see cref="HcState.SaveVisitNotes" />.</summary>
    private sealed class NotesForm
    {
        [StringLength(HcState.MaxNotesLength, ErrorMessage = "Shorten the notes to 4,000 characters or fewer.")]
        public string? Notes { get; set; } = string.Empty;

        [StringLength(HcState.MaxSummaryLength, ErrorMessage = "Shorten the summary to 2,000 characters or fewer.")]
        public string? Summary { get; set; } = string.Empty;
    }

    /// <summary>A prescription, with the limits of <see cref="HcState.Prescribe" /> and its cross-field rule (at most a year of fills).</summary>
    private sealed class RxForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter how many days one fill lasts.")]
        [Range(1, HcState.MaxDaysSupply, ErrorMessage = "Use 1 to 90 days per fill.")]
        public int? DaysSupply { get; set; } = 30;

        [Required(ErrorMessage = "Enter how to take it.")]
        [StringLength(HcState.MaxDirectionsLength, ErrorMessage = "Shorten the directions to 200 characters or fewer.")]
        public string? Directions { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the dose, like 10 mg tablet.")]
        [StringLength(HcState.MaxDoseLength, ErrorMessage = "Shorten the dose to 40 characters or fewer.")]
        public string Dose { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the medication.")]
        [StringLength(HcState.MaxMedicationLength, ErrorMessage = "Shorten the medication to 60 characters or fewer.")]
        public string Medication { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the quantity per fill.")]
        [Range(1, HcState.MaxQuantity, ErrorMessage = "Use a quantity from 1 to 365.")]
        public int? Quantity { get; set; } = 30;

        [Required(ErrorMessage = "Enter the number of refills (0 for none).")]
        [Range(0, HcState.MaxRefills, ErrorMessage = "Use 0 to 11 refills.")]
        public int? Refills { get; set; } = 0;

        /// <summary>Runs once the fields are filled in: the first fill and the refills together last at most a year.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DaysSupply is { } days and > 0 && Refills is { } refills and >= 0 && (refills + 1) * days > 366)
            {
                yield return new ValidationResult(string.Create(CultureInfo.InvariantCulture, $"{refills + 1} fills of {days} days last more than a year. Use at most {Math.Max(0, (366 / days) - 1)} refills."),
                    new[] { nameof(Refills) });
            }
        }
    }

    /// <summary>The vital signs as typed, with the ranges of <see cref="HcState.VitalsProblem" />.</summary>
    private sealed class VitalsForm : IValidatableObject
    {
        [Range(30, 150, ErrorMessage = "Diastolic pressure is 30 to 150 mmHg.")]
        public int? Diastolic { get; set; }

        [Range(30, 220, ErrorMessage = "Pulse is 30 to 220 beats a minute.")]
        public int? Pulse { get; set; }

        [Range(60, 250, ErrorMessage = "Systolic pressure is 60 to 250 mmHg.")]
        public int? Systolic { get; set; }

        [Range(34.0, 42.0, ErrorMessage = "Temperature is 34 to 42 °C.")]
        public double? TemperatureC { get; set; }

        [Range(1.0, 400.0, ErrorMessage = "Weight is 1 to 400 kg.")]
        public double? WeightKg { get; set; }

        /// <summary>Runs once the fields are filled in: something measured, and blood pressure as a pair with systolic above diastolic.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Systolic is null && Diastolic is null && Pulse is null && TemperatureC is null && WeightKg is null)
                yield return new ValidationResult("Enter at least one measurement.", new[] { nameof(Systolic) });
            else if (Systolic is not null && Diastolic is null)
                yield return new ValidationResult("Enter the diastolic pressure too, or clear the systolic.", new[] { nameof(Diastolic) });
            else if (Systolic is null && Diastolic is not null)
                yield return new ValidationResult("Enter the systolic pressure too, or clear the diastolic.", new[] { nameof(Systolic) });
            else if (Systolic is { } systolic && Diastolic is { } diastolic && systolic <= diastolic)
                yield return new ValidationResult(string.Create(CultureInfo.InvariantCulture, $"Systolic pressure must be above diastolic ({diastolic})."), new[] { nameof(Systolic) });
        }
    }

    #endregion
}
