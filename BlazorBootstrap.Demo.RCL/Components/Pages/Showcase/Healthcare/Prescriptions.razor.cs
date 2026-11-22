using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's prescriptions in a <see cref="Grid{TItem}" /> (medication, dose, status, refills left, the
/// refill request or the doctor's last answer in words, the last fill, and who prescribed it and when), active first, with
/// paging, sorting, the column chooser, and CSV export; the grid's view is saved for the visit and can be reset. A
/// prescription has no page of its own, so the row opens a dialog: its directions and fills, and for an active one with no
/// refills left and no request waiting, the refill request <see cref="EditForm" /> (rule WR-002): the medication (any
/// eligible one), a pharmacy (required), the quantity (1 to one fill), and the pick-up day (required; not before the
/// earliest refill day of the chosen medication — when three quarters of the last fill are used up — and at most
/// <see cref="HcState.MaxRefillPickupDays" /> days ahead: cross-field rules). "Request a refill" opens it for the first
/// eligible prescription. <see cref="HcState.RequestRefill" /> results map to a field or the dialog's alert; success is
/// announced in the status region once the dialog is hidden.
/// </summary>
public partial class Prescriptions : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    /// <summary>The rows' buttons, by prescription ID, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private ElementReference? dialogClose;

    private int dialogErrorCount;

    private string? dialogError;

    private Func<ElementReference?>? focusTarget;

    private Grid<RxRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private Modal modal = default!;

    /// <summary>The prescription the dialog opened for.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The request sent from the dialog, announced once the dialog is hidden.</summary>
    private string? pendingAnnouncement;

    private EditContext requestContext;

    private RequestForm requestForm = new();

    /// <summary>The state's answers that belong to a field, shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty request form.</summary>
    public Prescriptions() => requestContext = new EditContext(requestForm);

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HcDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "my-prescriptions", DefaultPageSize, null, SortDirection.None);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        requestContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    private static string DateValue(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private IReadOnlyList<HcPrescription> All() => State.CurrentPatient is { } patient ? State.PrescriptionsOf(patient.Id) : Array.Empty<HcPrescription>();

    /// <summary>Whether a refill can be asked for: active, no refills left, and no request waiting.</summary>
    private static bool CanRequest(HcPrescription prescription) =>
        prescription.Status == HcPrescriptionStatus.Active && prescription.Refills == 0 && prescription.RefillRequestedAt is null;

    /// <summary>The first day the patient can pick up a refill: the earliest refill day, or today when that has passed.</summary>
    private DateTime Earliest(HcPrescription prescription) => HcState.EarliestRefillOn(prescription) is var earliest && earliest > State.Clock.Today ? earliest : State.Clock.Today;

    /// <summary>The patient's prescriptions a refill can be asked for.</summary>
    private IReadOnlyList<HcPrescription> Eligible() => All().Where(CanRequest).ToList();

    private HcPrescription? FindMine(int prescriptionId) => All().FirstOrDefault(item => item.Id == prescriptionId);

    /// <summary>Starts the request form for a prescription: one fill, picked up on the first day it can be.</summary>
    private void LoadForm(HcPrescription prescription)
    {
        if (requestContext is not null)
            requestContext.OnFieldChanged -= OnFieldChanged;

        requestForm = new RequestForm { PrescriptionId = prescription.Id, Quantity = prescription.Quantity, PickupOn = Earliest(prescription) };
        SetLimits(prescription);
        requestContext = new EditContext(requestForm);
        serverErrors = new ValidationMessageStore(requestContext);
        requestContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>A new medication brings its own quantity and earliest day, and checks those fields again.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        dialogError = null;

        if (args.FieldIdentifier.FieldName != nameof(RequestForm.PrescriptionId) || FindMine(requestForm.PrescriptionId) is not { } chosen)
            return;

        SetLimits(chosen);
        requestForm.Quantity = chosen.Quantity;

        if (requestForm.PickupOn is null || requestForm.PickupOn < Earliest(chosen))
            requestForm.PickupOn = Earliest(chosen);

        requestContext.NotifyFieldChanged(FieldIdentifier.Create(() => requestForm.Quantity));
        requestContext.NotifyFieldChanged(FieldIdentifier.Create(() => requestForm.PickupOn));
    }

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after a request focus moves to the announcement, otherwise back to the row.</summary>
    private Task OnHiddenAsync()
    {
        var id = openedId;

        if (pendingAnnouncement is { } message)
        {
            pendingAnnouncement = null;
            SetStatus(message, false);
            focusTarget = () => statusRegion;
        }
        else
        {
            focusTarget = () => openers.TryGetValue(id, out var opener) ? opener : statusRegion;
        }

        return Task.CompletedTask;
    }

    private Task OnShownAsync() => HcDisplay.FocusAsync(dialogClose);

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>Opens the dialog of a prescription, with a fresh request form when a refill can be asked for.</summary>
    private async Task OpenAsync(int prescriptionId)
    {
        openedId = prescriptionId;
        dialogError = null;
        dialogErrorCount = 0;
        pendingAnnouncement = null;

        if (FindMine(prescriptionId) is { } prescription && CanRequest(prescription))
            LoadForm(prescription);

        await modal.ShowAsync();
    }

    /// <summary>The page of prescriptions the grid asked for: active first, then newest, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<RxRow>> ProvidePrescriptionsAsync(GridDataProviderRequest<RxRow> request)
    {
        var rows = All().Select(prescription =>
        {
            var doctor = State.FindDoctor(prescription.DoctorId)?.Name ?? "Your doctor";

            return new RxRow(
                prescription.Id,
                prescription.Medication,
                prescription.Dose,
                prescription.Status == HcPrescriptionStatus.Active ? "Active" : "Stopped",
                prescription.Status,
                prescription.Refills,
                prescription.Status == HcPrescriptionStatus.Stopped ? "Stopped by your doctor"
                    : prescription.RefillRequestedAt is not null ? "Requested, waiting for your doctor"
                    : prescription.Refills > 0 ? "Ask your pharmacy"
                    : prescription.RefillOutcome == HcRefillOutcome.Denied ? "Not approved; you can ask again"
                    : "None left: you can request one",
                prescription.LastFilledOn,
                HcDisplay.Date(prescription.LastFilledOn),
                doctor,
                prescription.PrescribedAt,
                HcDisplay.Date(prescription.PrescribedAt));
        });

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>The refill situation of a prescription that can't be requested now, in words.</summary>
    private string RefillText(HcPrescription prescription, string doctorName)
    {
        if (prescription.Status == HcPrescriptionStatus.Stopped)
        {
            return "Stopped by your doctor. Don't start taking it again unless a doctor tells you to."
                + (prescription.RefillNote.Length > 0 ? $" Note from {doctorName}: {prescription.RefillNote}" : null);
        }

        if (prescription.RefillRequestedAt is { } requested)
        {
            return $"Refill requested on {HcDisplay.Date(requested)}: {prescription.RefillQuantity.ToString(CultureInfo.InvariantCulture)} to pick up {HcDisplay.Day(prescription.RefillPickupOn ?? requested)} at "
                + $"{State.FindPharmacy(prescription.RefillPharmacyId)?.Name ?? "your pharmacy"}, waiting for {doctorName}.";
        }

        var answer = prescription.RefillOutcome switch
        {
            HcRefillOutcome.Approved => $"Your last request was approved by {doctorName}: {prescription.RefillNote} ",
            HcRefillOutcome.Denied => $"Your last request wasn't approved by {doctorName}: {prescription.RefillNote} ",
            _ => string.Empty,
        };

        return answer + (prescription.Refills == 1 ? "1 refill left: ask your pharmacy when you need more." : $"{prescription.Refills.ToString(CultureInfo.InvariantCulture)} refills left: ask your pharmacy when you need more.");
    }

    /// <summary>
    /// Sends the request once the form is valid (rule WR-002); the state checks again, and its answer maps to a field or the
    /// dialog's alert. Success closes the dialog and is announced in the status region.
    /// </summary>
    private async Task RequestAsync()
    {
        serverErrors?.Clear();
        requestContext.NotifyValidationStateChanged();

        if (FindMine(requestForm.PrescriptionId) is { } current)
            SetLimits(current);

        if (!await requestContext.ValidateAsync())
            return;

        if (FindMine(requestForm.PrescriptionId) is not { } prescription)
        {
            ShowDialogError("This prescription isn't on your record any more. Nothing was requested.");

            return;
        }

        var name = $"{prescription.Medication} {prescription.Dose}";
        var doctor = State.FindDoctor(prescription.DoctorId)?.Name ?? "your doctor";
        var pickup = requestForm.PickupOn!.Value;
        var result = State.RequestRefill(prescription.Id, requestForm.PharmacyId, requestForm.Quantity!.Value, pickup);

        switch (result)
        {
            case HcResult.Ok:
                pendingAnnouncement = $"Refill requested: {requestForm.Quantity.Value.ToString(CultureInfo.InvariantCulture)} of {name}, to pick up {HcDisplay.Day(pickup)} at "
                    + $"{State.FindPharmacy(requestForm.PharmacyId)?.Name}. {doctor} will answer here.";
                await modal.HideAsync();

                return;

            case HcResult.WrongState:
                ShowDialogError($"A refill of {name} can't be requested right now: it has refills again, it was stopped, or a request is already waiting. Nothing was requested.");

                return;

            case HcResult.Invalid when pickup < HcState.EarliestRefillOn(prescription):
                serverErrors?.Add(FieldIdentifier.Create(() => requestForm.PickupOn), $"{name} can be refilled from {HcDisplay.Date(HcState.EarliestRefillOn(prescription))}. Choose that day or later.");
                requestContext.NotifyValidationStateChanged();

                return;

            case HcResult.Invalid:
                ShowDialogError("Check the pharmacy, the quantity, and the day, then send the request again. Nothing was requested.");

                return;

            case HcResult.NotFound:
                ShowDialogError("This prescription can't be found any more, so no refill was requested.");

                return;

            case HcResult.NotAllowed:
                ShowDialogError("Only the patient's own account can request refills. Nothing was requested.");

                return;

            default:
                ShowDialogError($"The refill of {name} couldn't be requested. Try again.");

                return;
        }
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the announcement.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The view is reset: active first, 10 prescriptions a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Gives the form the chosen medication's limits, which its cross-field rules check.</summary>
    private void SetLimits(HcPrescription prescription)
    {
        requestForm.MaxQuantity = prescription.Quantity;
        requestForm.EarliestOn = Earliest(prescription);
        requestForm.LatestOn = State.Clock.Today.AddDays(HcState.MaxRefillPickupDays);
        requestForm.MedicationName = $"{prescription.Medication} {prescription.Dose}";
        requestForm.IsEligible = Eligible().Any(item => item.Id == prescription.Id);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows a refusal in the dialog's alert; the same one again gets a counter, so it is announced again.</summary>
    private void ShowDialogError(string problem)
    {
        dialogErrorCount = dialogError?.StartsWith(problem, StringComparison.Ordinal) == true ? dialogErrorCount + 1 : 1;
        dialogError = dialogErrorCount == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({dialogErrorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private HcPrescription? Opened => FindMine(openedId);

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// A refill request: the medication, a pharmacy, a quantity, and a pick-up day, with the chosen medication's limits (set
    /// by the page) for the cross-field rules.
    /// </summary>
    private sealed class RequestForm : IValidatableObject
    {
        /// <summary>The first day the chosen medication can be picked up.</summary>
        public DateTime EarliestOn { get; set; }

        /// <summary>Whether the chosen medication can still be refilled (active, no refills left, nothing waiting).</summary>
        public bool IsEligible { get; set; } = true;

        /// <summary>The last day a pick-up can be asked for.</summary>
        public DateTime LatestOn { get; set; }

        /// <summary>The most one fill of the chosen medication holds.</summary>
        public int MaxQuantity { get; set; } = HcState.MaxQuantity;

        /// <summary>The chosen medication in words, for the messages.</summary>
        public string MedicationName { get; set; } = "this medication";

        [Required(ErrorMessage = "Choose the pharmacy you'll pick it up at.")]
        public string PharmacyId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose the day you'll pick it up.")]
        public DateTime? PickupOn { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Choose the medication to refill.")]
        public int PrescriptionId { get; set; }

        [Required(ErrorMessage = "Enter how many you need.")]
        public int? Quantity { get; set; }

        /// <summary>Runs once the fields are filled in: the medication can be refilled, the quantity fits one fill, and the day fits the medication.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsEligible)
                yield return new ValidationResult($"{MedicationName} has refills left, was stopped, or has a request waiting. Choose another medication.", new[] { nameof(PrescriptionId) });

            if (Quantity is { } quantity && (quantity < 1 || quantity > MaxQuantity))
                yield return new ValidationResult(string.Create(CultureInfo.InvariantCulture, $"Ask for 1 to {MaxQuantity}: one fill of {MedicationName} at most."), new[] { nameof(Quantity) });

            if (PickupOn is { } day && day.Date < EarliestOn)
                yield return new ValidationResult($"{MedicationName} can be refilled from {HcDisplay.Date(EarliestOn)}, when most of your last fill is used up. Choose that day or later.", new[] { nameof(PickupOn) });
            else if (PickupOn is { } later && later.Date > LatestOn)
                yield return new ValidationResult($"Choose a day up to {HcDisplay.Date(LatestOn)}: refills can be asked for up to {HcState.MaxRefillPickupDays} days ahead.", new[] { nameof(PickupOn) });
        }
    }

    /// <summary>One prescription as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record RxRow(
        int Id,
        string Medication,
        string Dose,
        string Status,
        HcPrescriptionStatus StatusValue,
        int Refills,
        string Refill,
        DateTime LastFilled,
        string LastFilledText,
        string Doctor,
        DateTime Prescribed,
        string PrescribedText);

    #endregion
}
