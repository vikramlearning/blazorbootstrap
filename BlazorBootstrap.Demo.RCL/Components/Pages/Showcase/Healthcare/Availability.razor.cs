using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Doctors' hours (front desk): pick a doctor (kept in <c>?doctor=</c>), then keep their weekly hours — a table by day with
/// Edit and Delete, and a form with the day, start and end as half-hour selects between 07:00 and 20:00, and the clinic —
/// and their time off — a table with Edit and Delete, and a form with native date inputs and an optional staff-only
/// reason. Conflicts name the block or time off they overlap. Deletes are confirmed in a <see cref="Modal" /> and run once
/// it is hidden. Above both, the doctor's booked appointments now outside their hours or in their time off are listed with
/// the patient's phone and "Call to rebook", and every change says how many there are: patients see no open times on
/// those days or hours.
/// </summary>
public partial class Availability : ComponentBase, IDisposable
{
    #region Fields and Constants

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

    private string? endError;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private string? fromError;

    private string hoursClinic = string.Empty;

    private DayOfWeek hoursDay = DayOfWeek.Monday;

    private int hoursEnd = 12 * 60;

    private ElementReference? hoursEndInput;

    private string? hoursError;

    private ElementReference? hoursFormHeading;

    private int hoursStart = 9 * 60;

    private ElementReference? hoursStartInput;

    private string? offError;

    private ElementReference? offFormHeading;

    private string offFrom = string.Empty;

    private ElementReference? offFromInput;

    private string offReason = string.Empty;

    private string offTo = string.Empty;

    private ElementReference? offToInput;

    /// <summary>The Edit and Delete buttons, by key.</summary>
    private readonly Dictionary<string, ElementReference> openers = new();

    /// <summary>The confirmed delete, run once the dialog is hidden.</summary>
    private Action? pendingDelete;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? toError;

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
        if (DoctorQuery == appliedDoctorQuery && hoursClinic.Length > 0)
            return;

        appliedDoctorQuery = DoctorQuery;
        SelectDoctor(State.FindDoctor(DoctorQuery)?.Id ?? HcSeedData.SeededDoctorId);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string QueryValue(DateTime date) => date.ToString(HcDisplay.QueryDate, CultureInfo.InvariantCulture);

    private static DateTime? ParseDate(string? text) =>
        DateTime.TryParseExact(text?.Trim(), HcDisplay.QueryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date.Date : null;

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

    private void ClearHoursErrors() => hoursError = endError = null;

    private void ClearTimeOffErrors() => fromError = toError = offError = null;

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
        ClearHoursErrors();
        editingBlockId = block.Id;
        editingBlockLabel = BlockLabel(block);
        (hoursDay, hoursStart, hoursEnd, hoursClinic) = (block.Day, block.StartMinute, block.EndMinute, block.ClinicId);
        focusTarget = () => hoursFormHeading;
    }

    /// <summary>Fills the time-off form with an entry, to change it.</summary>
    private void EditTimeOff(HcTimeOff entry)
    {
        ClearTimeOffErrors();
        editingOffId = entry.Id;
        editingOffLabel = DoctorProfile.AwayText(entry);
        offFrom = QueryValue(entry.From);
        offTo = QueryValue(entry.To);
        offReason = entry.Reason;
        focusTarget = () => offFormHeading;
    }

    private static string CommonMessage(HcResult result) => result switch
    {
        HcResult.NotAllowed => "Only the front desk can change doctors' hours. Sign in again as the front desk; nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

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

    // The date inputs bind by hand: @bind would treat them as DateTime, and an empty or partial value as an error.
    private void OnFromChanged(ChangeEventArgs e)
    {
        offFrom = e.Value?.ToString() ?? string.Empty;

        // A first day without a last day gets the same day, which the desk can change.
        if (offTo.Length == 0 && ParseDate(offFrom) is { } from)
            offTo = QueryValue(from);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnToChanged(ChangeEventArgs e) => offTo = e.Value?.ToString() ?? string.Empty;

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
        ClearHoursErrors();
        editingBlockId = 0;
        editingBlockLabel = string.Empty;
        (hoursDay, hoursStart, hoursEnd) = (DayOfWeek.Monday, 9 * 60, 12 * 60);
        hoursClinic = State.FindDoctor(doctorId)?.ClinicId ?? State.Clinics[0].Id;
    }

    private void ResetTimeOffForm()
    {
        ClearTimeOffErrors();
        editingOffId = 0;
        editingOffLabel = string.Empty;
        offFrom = offTo = offReason = string.Empty;
    }

    /// <summary>Checks the hours form and saves the block, saying what changed or what is wrong.</summary>
    private void SaveHours()
    {
        ClearHoursErrors();

        if (hoursEnd <= hoursStart)
        {
            endError = $"End after the start ({HcDisplay.Minutes(hoursStart)}).";
            focusTarget = () => hoursEndInput;

            return;
        }

        var blockId = editingBlockId;
        var result = State.SaveAvailability(blockId, doctorId, hoursDay, hoursStart, hoursEnd, hoursClinic, out var saved);
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
                var other = State.AvailabilityOf(doctorId).FirstOrDefault(block => block.Id != blockId && block.Day == hoursDay && block.StartMinute < hoursEnd && hoursStart < block.EndMinute);
                hoursError = other is null
                    ? $"Those times overlap other hours {ShortName} has on {hoursDay}. Change the times; nothing was {verb}."
                    : $"Those times overlap {BlockLabel(other)}. Change the times, or edit that block instead; nothing was {verb}.";
                focusTarget = () => hoursStartInput;

                return;

            case HcResult.Invalid:
                hoursError = $"Use whole or half hours from {HcDisplay.Minutes(HcState.DayStartMinute)} to {HcDisplay.Minutes(HcState.DayEndMinute)}, ending at least {HcState.SlotMinutes} minutes after the start. If they already are, this tab has no room for more hours; delete some first. Nothing was {verb}.";
                focusTarget = () => hoursStartInput;

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

    /// <summary>Checks the time-off form and saves it, saying what changed or what is wrong.</summary>
    private void SaveTimeOff()
    {
        ClearTimeOffErrors();
        var today = State.Clock.Today;
        var from = ParseDate(offFrom);
        var to = ParseDate(offTo);

        fromError = from is null ? "Enter the first day away." : null;
        toError = to is not { } last ? "Enter the last day away."
            : from is { } first && last < first ? $"The last day must be on or after the first ({HcDisplay.Date(first)})."
            : last < today ? $"The time off has to end today ({HcDisplay.Date(today)}) or later."
            : from is { } start && (last - start).TotalDays + 1 > HcState.MaxTimeOffDays ? $"Time off can be at most {HcState.MaxTimeOffDays} days: end by {HcDisplay.Date(start.AddDays(HcState.MaxTimeOffDays - 1))}."
            : null;

        if (fromError is not null || toError is not null || from is not { } fromDate || to is not { } toDate)
        {
            focusTarget = fromError is not null ? () => offFromInput : () => offToInput;

            return;
        }

        var offId = editingOffId;
        var result = State.SaveTimeOff(offId, doctorId, fromDate, toDate, offReason, out var saved);
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
                offError = other is null
                    ? $"Those days overlap other time off {ShortName} has. Change the dates; nothing was {verb}."
                    : $"Those days overlap {ShortName}'s time off {DoctorProfile.AwayText(other)}. Change the dates, or edit that time off instead; nothing was {verb}.";
                focusTarget = () => offFromInput;

                return;

            case HcResult.Invalid:
                offError = $"Check the dates: whole days, ending today or later, at most {HcState.MaxTimeOffDays} days, and a reason of up to {HcState.MaxReasonLength} characters. If they're right, this tab has no room for more time off; delete some first. Nothing was {verb}.";
                focusTarget = () => offFromInput;

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

    private void SetStatus(string message, bool isError)
    {
        statusMessage = message;
        statusIsError = isError;
        focusTarget = () => statusRegion;
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
}
