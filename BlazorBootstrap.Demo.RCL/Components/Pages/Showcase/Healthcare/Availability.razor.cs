using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Doctors' hours (front desk): pick a doctor (kept in <c>?doctor=</c>), then keep their weekly hours — a table by day with
/// Edit and Delete, and an <see cref="EditForm" /> (rule WR-002) with the day, the clinic (required), and the start and end
/// as half-hour selects: the end after the start, on a weekday within the clinic's opening hours, and not overlapping the
/// doctor's other hours that day (cross-field rules) — and their time off — a table with Edit and Delete, and an
/// <see cref="EditForm" /> with the first and last day away (required; the last on or after the first, ending today or
/// later, at most <see cref="HcState.MaxTimeOffDays" /> days, not overlapping the doctor's other time off) and an optional
/// staff-only reason. Messages show after a field is touched or on submit, a failed submit shows a summary that links to
/// each field, and the state's answers map to a field or the status message. Deletes are confirmed in a
/// <see cref="Modal" /> and run once it is hidden. Above both, the doctor's booked appointments now outside their hours or
/// in their time off are listed with the patient's phone and "Call to rebook", and every change says how many there are:
/// patients see no open times on those days or hours.
/// </summary>
public partial class Availability : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Set while the hours form re-checks start and end against each other, so the re-check doesn't recurse.</summary>
    private bool rechecking;

    /// <summary>The <c>?doctor=</c> value applied last, so a re-render with the same query keeps the selection.</summary>
    private string? appliedDoctorQuery;

    private Modal deleteModal = default!;

    /// <summary>The block of hours the delete dialog is about.</summary>
    private HcAvailability? deleteBlock;

    /// <summary>The time off the delete dialog is about.</summary>
    private HcTimeOff? deleteOff;

    /// <summary>The key of the Delete button that opened the dialog, so Keep it returns focus to it.</summary>
    private string? deleteOpenerKey;

    private ElementReference? dialogDismiss;

    private string doctorId = HcSeedData.SeededDoctorId;

    /// <summary>The block being edited; 0 while adding.</summary>
    private int editingBlockId;

    private string editingBlockLabel = string.Empty;

    /// <summary>The time off being edited; 0 while adding.</summary>
    private int editingOffId;

    private string editingOffLabel = string.Empty;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private EditContext hoursContext;

    private HoursForm hoursForm = new();

    private ElementReference? hoursFormHeading;

    /// <summary>The state's answers that belong to an hours field, shown like the form's own messages.</summary>
    private ValidationMessageStore? hoursServerErrors;

    private EditContext offContext;

    private TimeOffForm offForm = new();

    private ElementReference? offFormHeading;

    /// <summary>The state's answers that belong to a time-off field, shown like the form's own messages.</summary>
    private ValidationMessageStore? offServerErrors;

    /// <summary>The Edit and Delete buttons, by key.</summary>
    private readonly Dictionary<string, ElementReference> openers = new();

    /// <summary>The confirmed delete, run once the dialog is hidden.</summary>
    private Action? pendingDelete;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with empty forms.</summary>
    public Availability()
    {
        hoursContext = new EditContext(hoursForm);
        offContext = new EditContext(offForm);
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

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (DoctorQuery == appliedDoctorQuery && hoursForm.ClinicId.Length > 0)
            return;

        appliedDoctorQuery = DoctorQuery;
        SelectDoctor(State.FindDoctor(DoctorQuery)?.Id ?? HcSeedData.SeededDoctorId);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        hoursContext.OnFieldChanged -= OnHoursFieldChanged;
        offContext.OnFieldChanged -= OnOffFieldChanged;
    }

    #endregion

    #region Private Methods

    private static string QueryValue(DateTime date) => date.ToString(HcDisplay.QueryDate, CultureInfo.InvariantCulture);

    private async Task AskDeleteBlockAsync(HcAvailability block, string openerKey)
    {
        deleteBlock = block;
        deleteOff = null;
        await OpenDeleteAsync(openerKey);
    }

    private async Task AskDeleteTimeOffAsync(HcTimeOff entry, string openerKey)
    {
        deleteOff = entry;
        deleteBlock = null;
        await OpenDeleteAsync(openerKey);
    }

    /// <summary>Names a block, like "Tuesday 08:30–12:00 at Cedarline Downtown".</summary>
    private string BlockLabel(HcAvailability block) => $"{block.Day} {DoctorProfile.BlockText(block)} at {State.FindClinic(block.ClinicId)?.Name}";

    private static string CommonMessage(HcResult result) => result switch
    {
        HcResult.NotAllowed => "Only the front desk can change doctors' hours. Sign in again as the front desk; nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private Task ConfirmDeleteAsync()
    {
        if (pendingDelete is not null)
            return Task.CompletedTask;

        if (deleteBlock is { } block)
            pendingDelete = () => DeleteBlock(block);
        else if (deleteOff is { } entry)
            pendingDelete = () => DeleteTimeOff(entry);
        else
            return Task.CompletedTask;

        return deleteModal.HideAsync();
    }

    private void DeleteBlock(HcAvailability block)
    {
        var label = BlockLabel(block);
        var result = State.DeleteAvailability(block.Id);

        if (result == HcResult.Ok && editingBlockId == block.Id)
            ResetHoursForm();

        SetStatus(result switch
        {
            HcResult.Ok => $"Deleted {label} for {DoctorName}. Patients can no longer book those times. {RebookNote()}",
            HcResult.NotFound => $"{label} was already deleted. Nothing changed.",
            _ => CommonMessage(result),
        }, result != HcResult.Ok);
    }

    private void DeleteTimeOff(HcTimeOff entry)
    {
        var dates = DoctorProfile.AwayText(entry);
        var result = State.DeleteTimeOff(entry.Id);

        if (result == HcResult.Ok && editingOffId == entry.Id)
            ResetTimeOffForm();

        SetStatus(result switch
        {
            HcResult.Ok => $"Deleted {DoctorName}'s time off {dates}. Those days are open for booking again in the usual hours. {RebookNote()}",
            HcResult.NotFound => $"The time off {dates} was already deleted. Nothing changed.",
            _ => CommonMessage(result),
        }, result != HcResult.Ok);
    }

    private string DialogTitle() =>
        deleteBlock is { } block ? $"Delete {BlockLabel(block)}?"
        : deleteOff is { } entry ? $"Delete the time off {DoctorProfile.AwayText(entry)}?"
        : "Delete";

    /// <summary>Fills the hours form with a block, to change it.</summary>
    private void EditBlock(HcAvailability block)
    {
        editingBlockId = block.Id;
        editingBlockLabel = BlockLabel(block);
        SetHoursForm(new HoursForm { Day = block.Day, StartMinute = block.StartMinute, EndMinute = block.EndMinute, ClinicId = block.ClinicId });
        focusTarget = () => hoursFormHeading;
    }

    /// <summary>Fills the time-off form with an entry, to change it.</summary>
    private void EditTimeOff(HcTimeOff entry)
    {
        editingOffId = entry.Id;
        editingOffLabel = DoctorProfile.AwayText(entry);
        SetTimeOffForm(new TimeOffForm { From = entry.From, To = entry.To, Reason = entry.Reason });
        focusTarget = () => offFormHeading;
    }

    private Task OnDialogShownAsync() => HcDisplay.FocusAsync(dialogDismiss);

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. A confirmed delete runs now that the dialog is gone and
    /// focus moves to its outcome; Keep it or Escape returns focus to the Delete button.
    /// </summary>
    private void OnDialogHidden()
    {
        var action = pendingDelete;
        var openerKey = deleteOpenerKey;
        pendingDelete = null;
        deleteBlock = null;
        deleteOff = null;
        deleteOpenerKey = null;

        if (action is not null)
            action();
        else if (openerKey is not null && openers.TryGetValue(openerKey, out var opener))
            focusTarget = () => opener;
    }

    /// <summary>A native select, so the doctor changes only when one is picked; the choice goes into the query.</summary>
    private void OnDoctorChanged(ChangeEventArgs e)
    {
        var id = State.FindDoctor(e.Value?.ToString())?.Id ?? HcSeedData.SeededDoctorId;

        if (id == doctorId)
            return;

        SelectDoctor(id);
        statusMessage = null;
        appliedDoctorQuery = id;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("doctor", id), replace: true);
    }

    /// <summary>The rules compare the hours with the clinic and the doctor's other hours: any change re-checks the times.</summary>
    private void OnHoursFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        hoursServerErrors?.Clear();
        UpdateHoursRules();

        // Start and end re-check each other. The notifications below raise this handler again, so a guard stops
        // the second round; without it the two would notify each other until the stack overflows.
        if (rechecking)
            return;

        rechecking = true;

        try
        {
            if (args.FieldIdentifier.FieldName is nameof(HoursForm.Day) or nameof(HoursForm.ClinicId) or nameof(HoursForm.StartMinute))
                hoursContext.NotifyFieldChanged(FieldIdentifier.Create(() => hoursForm.EndMinute));

            if (args.FieldIdentifier.FieldName is nameof(HoursForm.Day) or nameof(HoursForm.ClinicId) or nameof(HoursForm.EndMinute))
                hoursContext.NotifyFieldChanged(FieldIdentifier.Create(() => hoursForm.StartMinute));
        }
        finally
        {
            rechecking = false;
        }
    }

    /// <summary>A first day without a last day gets the same day; a new first day re-checks the last.</summary>
    private void OnOffFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        offServerErrors?.Clear();
        UpdateTimeOffRules();

        if (args.FieldIdentifier.FieldName == nameof(TimeOffForm.From))
        {
            if (offForm.To is null && offForm.From is { } from)
                offForm.To = from;

            offContext.NotifyFieldChanged(FieldIdentifier.Create(() => offForm.To));
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteAsync(string openerKey)
    {
        deleteOpenerKey = openerKey;
        pendingDelete = null;
        StateHasChanged();

        await deleteModal.ShowAsync();
    }

    /// <summary>Returns the doctor's booked appointments from now on that are outside their hours or in their time off, soonest first.</summary>
    private IReadOnlyList<HcAppointment> OutsideAppointments(string id)
    {
        var now = State.Clock.Now;
        var list = new List<HcAppointment>();

        for (var day = 0; day <= HcState.BookingDays; day++)
        {
            list.AddRange(State.AppointmentsOn(State.Clock.Today.AddDays(day), id)
                .Where(item => item.Status == HcAppointmentStatus.Booked && item.Start > now && State.IsOutsideHours(item)));
        }

        return list;
    }

    /// <summary>Returns staff's view of the patients' names, by ID.</summary>
    private IReadOnlyDictionary<int, string> PatientNames() => State.Patients.ToDictionary(patient => patient.Id, patient => patient.Name);

    /// <summary>Says how many booked appointments need a call after a change.</summary>
    private string RebookNote()
    {
        var count = OutsideAppointments(doctorId).Count;

        return count switch
        {
            0 => "No booked appointments are affected.",
            1 => $"1 booked appointment is now outside {ShortName}'s hours or time off: call the patient to rebook (listed under “Booked appointments to rebook”).",
            _ => FormattableString.Invariant($"{count} booked appointments are now outside {ShortName}'s hours or time off: call those patients to rebook (listed under “Booked appointments to rebook”)."),
        };
    }

    private void ResetHoursForm()
    {
        editingBlockId = 0;
        editingBlockLabel = string.Empty;
        SetHoursForm(new HoursForm { Day = DayOfWeek.Monday, StartMinute = 9 * 60, EndMinute = 12 * 60, ClinicId = State.FindDoctor(doctorId)?.ClinicId ?? State.Clinics[0].Id });
    }

    private void ResetTimeOffForm()
    {
        editingOffId = 0;
        editingOffLabel = string.Empty;
        SetTimeOffForm(new TimeOffForm());
    }

    /// <summary>Checks the hours form and saves the block, saying what changed or what is wrong (rule WR-002).</summary>
    private void SaveHours()
    {
        hoursServerErrors?.Clear();
        UpdateHoursRules();

        if (!hoursContext.Validate())
            return;

        var blockId = editingBlockId;
        var (day, start, end, clinicId) = (hoursForm.Day, hoursForm.StartMinute, hoursForm.EndMinute, hoursForm.ClinicId);
        var result = State.SaveAvailability(blockId, doctorId, day, start, end, clinicId, out var saved);
        var verb = blockId == 0 ? "added" : "saved";

        switch (result)
        {
            case HcResult.Ok when saved is not null:
                ResetHoursForm();
                SetStatus($"{(blockId == 0 ? "Added" : "Saved")} {BlockLabel(saved)} for {DoctorName}. Patients see open times in these hours. {RebookNote()}", false);

                return;

            case HcResult.NoChange:
                SetStatus($"You didn't change {editingBlockLabel}, so nothing was saved.", false);

                return;

            case HcResult.Conflict:
                var other = State.AvailabilityOf(doctorId).FirstOrDefault(block => block.Id != blockId && block.Day == day && block.StartMinute < end && start < block.EndMinute);
                HoursError(() => hoursForm.StartMinute, other is null
                    ? $"Those times overlap other hours {ShortName} has on {day}. Change the times; nothing was {verb}."
                    : $"Those times overlap {BlockLabel(other)}. Change the times, or edit that block instead; nothing was {verb}.");

                return;

            case HcResult.Invalid:
                HoursError(() => hoursForm.StartMinute, $"Use whole or half hours within the clinic's opening hours, ending at least {HcState.SlotMinutes} minutes after the start. If they already are, this tab has no room for more hours; delete some first. Nothing was {verb}.");

                return;

            case HcResult.NotFound when blockId != 0:
                ResetHoursForm();
                SetStatus("Those hours were deleted while you were editing them, so nothing was saved. Add them again if you need them.", true);

                return;

            case HcResult.NotFound:
                SetStatus($"That doctor or clinic no longer exists, so nothing was {verb}.", true);

                return;

            default:
                SetStatus(CommonMessage(result), true);

                return;
        }
    }

    /// <summary>Checks the time-off form and saves it, saying what changed or what is wrong (rule WR-002).</summary>
    private void SaveTimeOff()
    {
        offServerErrors?.Clear();
        UpdateTimeOffRules();

        if (!offContext.Validate())
            return;

        var offId = editingOffId;
        var fromDate = offForm.From!.Value.Date;
        var toDate = offForm.To!.Value.Date;
        var result = State.SaveTimeOff(offId, doctorId, fromDate, toDate, offForm.Reason ?? string.Empty, out var saved);
        var verb = offId == 0 ? "added" : "saved";

        switch (result)
        {
            case HcResult.Ok when saved is not null:
                ResetTimeOffForm();
                SetStatus($"{(offId == 0 ? "Added" : "Saved")} time off for {DoctorName}: away {DoctorProfile.AwayText(saved)}. Patients see no open times those days. {RebookNote()}", false);

                return;

            case HcResult.NoChange:
                SetStatus($"You didn't change the time off {editingOffLabel}, so nothing was saved.", false);

                return;

            case HcResult.Conflict:
                var other = State.TimeOffOf(doctorId).FirstOrDefault(entry => entry.Id != offId && entry.From <= toDate && fromDate <= entry.To);
                TimeOffError(() => offForm.From, other is null
                    ? $"Those days overlap other time off {ShortName} has. Change the dates; nothing was {verb}."
                    : $"Those days overlap {ShortName}'s time off {DoctorProfile.AwayText(other)}. Change the dates, or edit that time off instead; nothing was {verb}.");

                return;

            case HcResult.Invalid:
                TimeOffError(() => offForm.To, $"Check the dates: whole days, ending today or later, at most {HcState.MaxTimeOffDays} days. If they're right, this tab has no room for more time off; delete some first. Nothing was {verb}.");

                return;

            case HcResult.NotFound when offId != 0:
                ResetTimeOffForm();
                SetStatus("That time off was deleted while you were editing it, so nothing was saved. Add it again if you need it.", true);

                return;

            case HcResult.NotFound:
                SetStatus($"That doctor no longer exists, so nothing was {verb}.", true);

                return;

            default:
                SetStatus(CommonMessage(result), true);

                return;
        }
    }

    /// <summary>Switches to a doctor and empties both forms.</summary>
    private void SelectDoctor(string id)
    {
        doctorId = id;
        ResetHoursForm();
        ResetTimeOffForm();
    }

    private void SetHoursForm(HoursForm next)
    {
        if (hoursContext is not null)
            hoursContext.OnFieldChanged -= OnHoursFieldChanged;

        hoursForm = next;
        UpdateHoursRules();
        hoursContext = new EditContext(hoursForm);
        hoursServerErrors = new ValidationMessageStore(hoursContext);
        hoursContext.OnFieldChanged += OnHoursFieldChanged;
    }

    private void SetStatus(string message, bool isError)
    {
        statusMessage = message;
        statusIsError = isError;
        focusTarget = () => statusRegion;
    }

    private void SetTimeOffForm(TimeOffForm next)
    {
        if (offContext is not null)
            offContext.OnFieldChanged -= OnOffFieldChanged;

        offForm = next;
        UpdateTimeOffRules();
        offContext = new EditContext(offForm);
        offServerErrors = new ValidationMessageStore(offContext);
        offContext.OnFieldChanged += OnOffFieldChanged;
    }

    /// <summary>Shows a state answer under an hours field, like the form's own messages; the summary and focus follow.</summary>
    private void HoursError(Expression<Func<object?>> field, string message)
    {
        hoursServerErrors?.Clear();
        hoursServerErrors?.Add(FieldIdentifier.Create(field), message);
        hoursContext.NotifyValidationStateChanged();
    }

    /// <summary>Leaves editing hours without saving, and returns focus to the Edit button (or the form's heading when it is gone).</summary>
    private void StopEditingBlock()
    {
        var key = $"hours-edit-{editingBlockId}";
        ResetHoursForm();
        focusTarget = openers.TryGetValue(key, out var opener) ? () => opener : () => hoursFormHeading;
    }

    /// <summary>Leaves editing time off without saving, and returns focus to the Edit button (or the form's heading when it is gone).</summary>
    private void StopEditingTimeOff()
    {
        var key = $"off-edit-{editingOffId}";
        ResetTimeOffForm();
        focusTarget = openers.TryGetValue(key, out var opener) ? () => opener : () => offFormHeading;
    }

    /// <summary>Shows a state answer under a time-off field, like the form's own messages; the summary and focus follow.</summary>
    private void TimeOffError(Expression<Func<object?>> field, string message)
    {
        offServerErrors?.Clear();
        offServerErrors?.Add(FieldIdentifier.Create(field), message);
        offContext.NotifyValidationStateChanged();
    }

    /// <summary>Says whether a time off is past, under way, or ahead.</summary>
    private string TimeOffPhase(HcTimeOff entry)
    {
        var today = State.Clock.Today;

        if (entry.To < today)
            return "Past";

        var days = (int)(entry.To - entry.From).TotalDays + 1;
        var length = days == 1 ? "1 day" : FormattableString.Invariant($"{days} days");

        return entry.From <= today ? $"Away now, {length}" : $"Upcoming, {length}";
    }

    /// <summary>Gives the hours form what its cross-field rules check: the clinic and the doctor's other hours.</summary>
    private void UpdateHoursRules()
    {
        hoursForm.Clinic = State.FindClinic(hoursForm.ClinicId);
        hoursForm.DoctorName = ShortName;
        hoursForm.OtherBlocks = State.AvailabilityOf(doctorId).Where(block => block.Id != editingBlockId).Select(block => (block.Day, block.StartMinute, block.EndMinute, BlockLabel(block))).ToList();
    }

    /// <summary>Gives the time-off form what its cross-field rules check: today and the doctor's other time off.</summary>
    private void UpdateTimeOffRules()
    {
        offForm.Today = State.Clock.Today;
        offForm.DoctorName = ShortName;
        offForm.Others = State.TimeOffOf(doctorId).Where(entry => entry.Id != editingOffId).Select(entry => (entry.From, entry.To, DoctorProfile.AwayText(entry))).ToList();
    }

    /// <summary>Says why a booked appointment needs rebooking: time off (with its days) or outside the weekly hours.</summary>
    private string WhyText(HcAppointment appointment) =>
        State.TimeOffOf(appointment.DoctorId).FirstOrDefault(entry => appointment.Start.Date >= entry.From && appointment.Start.Date <= entry.To) is { } away
            ? $"During time off ({DoctorProfile.AwayText(away)})"
            : "Outside weekly hours";

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the selected doctor.</summary>
    private HcDoctor Doctor => State.FindDoctor(doctorId) ?? State.Doctors[0];

    private string DoctorName => Doctor.Name;

    /// <summary>Gets or sets the doctor ID from the query.</summary>
    [SupplyParameterFromQuery(Name = "doctor")] public string? DoctorQuery { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string ShortName => DoctorProfile.ShortName(Doctor);

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// A block of weekly hours, with the limits of <see cref="HcState.SaveAvailability" />, and what the page knows for the
    /// cross-field rules: the clinic's opening hours and the doctor's other hours.
    /// </summary>
    private sealed class HoursForm : IValidatableObject
    {
        /// <summary>The chosen clinic, set by the page.</summary>
        public HcClinic? Clinic { get; set; }

        [Required(ErrorMessage = "Choose the clinic of these hours.")]
        public string ClinicId { get; set; } = string.Empty;

        public DayOfWeek Day { get; set; } = DayOfWeek.Monday;

        /// <summary>The doctor's short name, for the messages.</summary>
        public string DoctorName { get; set; } = "the doctor";

        public int EndMinute { get; set; } = 12 * 60;

        /// <summary>The doctor's other blocks (not the one being edited), set by the page.</summary>
        public IReadOnlyList<(DayOfWeek Day, int Start, int End, string Label)> OtherBlocks { get; set; } = Array.Empty<(DayOfWeek, int, int, string)>();

        public int StartMinute { get; set; } = 9 * 60;

        /// <summary>Runs once the fields are filled in: the end after the start, within the clinic's days and hours, and no overlap.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndMinute <= StartMinute)
            {
                yield return new ValidationResult($"End after the start ({HcDisplay.Minutes(StartMinute)}): at least one {HcState.SlotMinutes}-minute appointment.", new[] { nameof(EndMinute) });

                yield break;
            }

            if (Clinic is { } clinic)
            {
                if (!HcClinic.IsOpenDay(Day))
                    yield return new ValidationResult($"{clinic.Name} is closed on {Day}s. Choose a weekday.", new[] { nameof(Day) });

                if (StartMinute < clinic.OpenMinute)
                    yield return new ValidationResult($"{clinic.Name} opens at {HcDisplay.Minutes(clinic.OpenMinute)}. Start then or later.", new[] { nameof(StartMinute) });

                if (EndMinute > clinic.CloseMinute)
                    yield return new ValidationResult($"{clinic.Name} closes at {HcDisplay.Minutes(clinic.CloseMinute)}. End then or earlier.", new[] { nameof(EndMinute) });
            }

            if (OtherBlocks.FirstOrDefault(block => block.Day == Day && block.Start < EndMinute && StartMinute < block.End) is { Label: not null } overlap)
                yield return new ValidationResult($"Those times overlap {overlap.Label}. Change the times, or edit that block instead.", new[] { nameof(StartMinute) });
        }
    }

    /// <summary>
    /// A doctor's time off, with the limits of <see cref="HcState.SaveTimeOff" />, and what the page knows for the cross-field
    /// rules: today and the doctor's other time off.
    /// </summary>
    private sealed class TimeOffForm : IValidatableObject
    {
        /// <summary>The doctor's short name, for the messages.</summary>
        public string DoctorName { get; set; } = "the doctor";

        [Required(ErrorMessage = "Enter the first day away.")]
        public DateTime? From { get; set; }

        /// <summary>The doctor's other time off (not the one being edited), set by the page.</summary>
        public IReadOnlyList<(DateTime From, DateTime To, string Label)> Others { get; set; } = Array.Empty<(DateTime, DateTime, string)>();

        [StringLength(HcState.MaxReasonLength, ErrorMessage = "Use 200 characters or fewer for the reason.")]
        public string? Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the last day away.")]
        public DateTime? To { get; set; }

        /// <summary>The clinics' today, set by the page.</summary>
        public DateTime Today { get; set; } = HcSeedData.Now.Date;

        /// <summary>Runs once the fields are filled in: the days in order, ending today or later, not too long, and no overlap.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From is not { } fromValue || To is not { } toValue)
                yield break;

            var from = fromValue.Date;
            var to = toValue.Date;

            if (to < from)
                yield return new ValidationResult($"The last day must be on or after the first ({HcDisplay.Date(from)}).", new[] { nameof(To) });
            else if (to < Today)
                yield return new ValidationResult($"The time off has to end today ({HcDisplay.Date(Today)}) or later.", new[] { nameof(To) });
            else if ((to - from).TotalDays + 1 > HcState.MaxTimeOffDays)
                yield return new ValidationResult($"Time off can be at most {HcState.MaxTimeOffDays} days: end by {HcDisplay.Date(from.AddDays(HcState.MaxTimeOffDays - 1))}.", new[] { nameof(To) });
            else if (Others.FirstOrDefault(entry => entry.From <= to && from <= entry.To) is { Label: not null } overlap)
                yield return new ValidationResult($"Those days overlap {DoctorName}'s time off {overlap.Label}. Change the dates, or edit that time off instead.", new[] { nameof(From) });
        }
    }

    #endregion
}
