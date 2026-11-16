using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// One visit for its doctor. The header names the patient with age, record number, a link to the chart, and the allergies
/// prominently, then the reason, status, and doctor. While the visit is in progress or completed the doctor writes notes
/// (staff only) and the summary for the patient (<see cref="HcState.SaveVisitNotes" />), orders lab tests
/// (<see cref="HcState.OrderLab" />, linked to Lab results), and prescribes with checked fields
/// (<see cref="HcState.Prescribe" />). A checked-in visit offers Start visit (<see cref="HcState.StartVisit" />); a booked one
/// says the patient isn't checked in yet. Complete visit asks first, naming the fee and warning about an empty summary, saves
/// unsaved notes, and completes once the dialog is hidden (<see cref="HcState.CompleteVisit" />), then shows the bill.
/// Another doctor's visit is read-only. Every result is announced in an always-rendered status region.
/// </summary>
public partial class Visit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string DirectionsField = "directions";

    private const string DoseField = "dose";

    private const string MedicationField = "medication";

    private const string QuantityField = "quantity";

    private const string RefillsField = "refills";

    /// <summary>Prescriptions written on this page, listed with the visit even when written on a later day.</summary>
    private readonly HashSet<int> prescribedHere = new();

    private readonly Dictionary<string, string> rxErrors = new();

    private readonly Dictionary<string, ElementReference> rxInputs = new();

    /// <summary>The visit this page was set up for.</summary>
    private int builtFor;

    private ElementReference? completeButton;

    private Modal completeModal = default!;

    private string directions = string.Empty;

    private string dose = string.Empty;

    /// <summary>The visit whose stored notes and summary are loaded into the fields, or <see langword="null" />.</summary>
    private int? draftsFor;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private string medication = string.Empty;

    private string notes = string.Empty;

    private string? notesError;

    private ElementReference? notesInput;

    private string? panelError;

    private string panelId = string.Empty;

    private ElementReference? panelSelect;

    /// <summary>Whether Complete visit was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingComplete;

    private string quantity = "30";

    private string refills = "0";

    private ElementReference? startButton;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string summary = string.Empty;

    private string? summaryError;

    private ElementReference? summaryInput;

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
            notes = summary = medication = dose = directions = panelId = string.Empty;
            quantity = "30";
            refills = "0";
            notesError = summaryError = panelError = statusMessage = null;
            rxErrors.Clear();
            prescribedHere.Clear();
        }

        SyncDrafts();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string BillText(HcBill bill) =>
        $"Billed {HcDisplay.Money(bill.AmountCents)}, due {HcDisplay.Date(bill.DueDate)}: "
        + (bill.Status == HcBillStatus.Paid ? bill.PaidAt is { } paidAt ? $"paid {HcDisplay.Date(paidAt)}." : "paid." : "not paid yet.");

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

    private string FieldClass(string field) => rxErrors.ContainsKey(field) ? "form-control is-invalid" : "form-control";

    private string? Invalid(string field) => rxErrors.ContainsKey(field) ? "true" : null;

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

        if (notes.Trim() != appointment.Notes || summary.Trim() != appointment.Summary)
        {
            if (!ValidateNotes())
            {
                Announce(false, "The visit wasn't completed: fix the marked field, save, then complete it.");
                focusTarget = () => notesError is not null ? notesInput : summaryInput;
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

            notes = appointment.Notes;
            summary = appointment.Summary;
        }

        var result = State.CompleteVisit(appointment.Id, out var bill);

        Announce(result == HcResult.Ok, result switch
        {
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

    /// <summary>Orders the chosen lab test in this visit.</summary>
    private void OrderLab()
    {
        if (Own() is not { } appointment)
            return;

        var panel = State.LabPanel(panelId);

        if (panel is null)
        {
            panelError = "Choose a lab test to order.";
            Announce(false, "No lab test was ordered.");
            focusTarget = () => panelSelect;

            return;
        }

        var result = State.OrderLab(appointment.Id, panel.Id, out _);
        panelError = result == HcResult.Conflict ? $"{panel.Name} is already ordered in this visit. Choose another test." : null;

        Announce(result == HcResult.Ok, result switch
        {
            HcResult.Ok => $"Ordered {panel.Name}. Enter the results under Lab results when they come back.",
            HcResult.Conflict => $"{panel.Name} wasn't ordered again: it's already ordered in this visit.",
            HcResult.WrongState => "Start the visit before ordering lab tests. Nothing was ordered.",
            HcResult.Invalid => "The storage for this demo tab is full, so the test wasn't ordered. Try again after removing something.",
            HcResult.NotFound => "That test or this visit no longer exists, so nothing was ordered.",
            HcResult.NotAllowed => "Only the visit's doctor can order tests. Sign in again; nothing was ordered.",
            _ => "That didn't work, and nothing was ordered. Try again.",
        });

        if (result == HcResult.Ok)
            panelId = string.Empty;
        else
            focusTarget = () => result == HcResult.Conflict ? panelSelect : statusRegion;
    }

    /// <summary>The signed-in doctor's own visit, or <see langword="null" />.</summary>
    private HcAppointment? Own() =>
        State.FindAppointment(Id) is { } appointment && State.CurrentDoctor is { } doctor && appointment.DoctorId == doctor.Id ? appointment : null;

    private string NotesFailureText(HcResult result) => result switch
    {
        HcResult.Invalid => "The storage for this demo tab is full. Try again after removing something.",
        HcResult.WrongState => "The visit isn't in progress or completed, so notes can't be written.",
        HcResult.NotFound => "This visit isn't on your schedule any more.",
        HcResult.NotAllowed => "Only the visit's doctor can write notes. Sign in again.",
        _ => "Try again.",
    };

    /// <summary>Checks the prescription fields and prescribes in this visit.</summary>
    private void Prescribe()
    {
        if (Own() is not { } appointment)
            return;

        rxErrors.Clear();
        var medicationText = medication.Trim();
        var doseText = dose.Trim();
        var directionsText = directions.Trim();

        if (medicationText.Length == 0)
            rxErrors[MedicationField] = "Enter the medication.";
        else if (medicationText.Length > HcState.MaxMedicationLength)
            rxErrors[MedicationField] = $"Shorten it to {HcState.MaxMedicationLength} characters or fewer.";

        if (doseText.Length == 0)
            rxErrors[DoseField] = "Enter the dose, like 10 mg tablet.";
        else if (doseText.Length > HcState.MaxDoseLength)
            rxErrors[DoseField] = $"Shorten it to {HcState.MaxDoseLength} characters or fewer.";

        if (directionsText.Length == 0)
            rxErrors[DirectionsField] = "Enter how to take it.";
        else if (directionsText.Length > HcState.MaxDirectionsLength)
            rxErrors[DirectionsField] = $"Shorten the directions to {HcState.MaxDirectionsLength} characters or fewer.";

        if (!int.TryParse(quantity, NumberStyles.None, CultureInfo.InvariantCulture, out var quantityValue) || quantityValue is < 1 or > HcState.MaxQuantity)
            rxErrors[QuantityField] = $"Enter a whole number from 1 to {HcState.MaxQuantity}.";

        if (!int.TryParse(refills, NumberStyles.None, CultureInfo.InvariantCulture, out var refillsValue) || refillsValue is < 0 or > HcState.MaxRefills)
            rxErrors[RefillsField] = $"Enter a whole number from 0 to {HcState.MaxRefills}.";

        if (new[] { MedicationField, DoseField, DirectionsField, QuantityField, RefillsField }.FirstOrDefault(rxErrors.ContainsKey) is { } first)
        {
            Announce(false, "Nothing was prescribed. Fix the marked fields, then prescribe again.");
            focusTarget = () => rxInputs.TryGetValue(first, out var input) ? input : statusRegion;

            return;
        }

        var name = State.FindPatient(appointment.PatientId)?.Name ?? "The patient";
        var result = State.Prescribe(appointment.Id, medicationText, doseText, directionsText, quantityValue, refillsValue, out var prescription);

        Announce(result == HcResult.Ok, result switch
        {
            HcResult.Ok => $"Prescribed {medicationText} {doseText}: quantity {quantityValue}, {PatientChart.RefillsText(refillsValue)}. {name} sees it under Prescriptions.",
            HcResult.Invalid => "The storage for this demo tab is full, so nothing was prescribed. Try again after removing something.",
            HcResult.WrongState => "Start the visit before prescribing. Nothing was prescribed.",
            HcResult.NotFound => "This visit isn't on your schedule any more, so nothing was prescribed.",
            HcResult.NotAllowed => "Only the visit's doctor can prescribe. Sign in again; nothing was prescribed.",
            _ => "That didn't work, and nothing was prescribed. Try again.",
        });

        if (result == HcResult.Ok && prescription is not null)
        {
            prescribedHere.Add(prescription.Id);
            medication = dose = directions = string.Empty;
            quantity = "30";
            refills = "0";
            focusTarget = () => rxInputs.TryGetValue(MedicationField, out var input) ? input : statusRegion;
        }
        else
        {
            focusTarget = () => statusRegion;
        }
    }

    /// <summary>Saves the notes and the summary for the patient.</summary>
    private void SaveNotes()
    {
        if (Own() is not { } appointment)
            return;

        if (!ValidateNotes())
        {
            Announce(false, "The notes weren't saved. Fix the marked field, then save again.");
            focusTarget = () => notesError is not null ? notesInput : summaryInput;

            return;
        }

        var result = State.SaveVisitNotes(appointment.Id, notes, summary);

        if (result == HcResult.Ok)
        {
            notes = appointment.Notes;
            summary = appointment.Summary;
            Announce(true, appointment.Status == HcAppointmentStatus.Completed
                ? "Saved. The patient sees the updated summary in their records."
                : "Saved the notes and the summary. The patient sees the summary once you complete the visit.");

            return;
        }

        if (result == HcResult.NoChange)
        {
            Announce(true, "Nothing changed since the last save.");

            return;
        }

        Announce(false, $"The notes weren't saved. {NotesFailureText(result)}");
        focusTarget = () => statusRegion;
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
            Announce(true, "The visit has started. Write your notes and the summary for the patient below.");
            focusTarget = () => notesInput;

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

    /// <summary>Loads the stored notes and summary into the fields once the doctor's own visit is available.</summary>
    private void SyncDrafts()
    {
        if (draftsFor == Id || Own() is not { } appointment)
            return;

        draftsFor = Id;
        notes = appointment.Notes;
        summary = appointment.Summary;
    }

    private bool ValidateNotes()
    {
        var notesLength = notes.Trim().Length;
        var summaryLength = summary.Trim().Length;

        notesError = notesLength > HcState.MaxNotesLength ? $"Shorten the notes to {HcState.MaxNotesLength} characters or fewer (now {notesLength})." : null;
        summaryError = summaryLength > HcState.MaxSummaryLength ? $"Shorten the summary to {HcState.MaxSummaryLength} characters or fewer (now {summaryLength})." : null;

        return notesError is null && summaryError is null;
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
}
