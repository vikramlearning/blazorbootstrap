using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// One of the signed-in patient's appointments: when, the doctor, the clinic, the reason, and the status in words (never the
/// doctor's notes, which are for staff). A booked visit more than 2 hours away can be rescheduled (a <see cref="SlotPicker" />
/// for the same doctor, then a confirmation of the new time) or cancelled (a <see cref="Modal" /> with an optional reason; the
/// cancellation runs once the dialog is hidden); closer than that, the page says to call the clinic, with its phone. A
/// completed visit shows the doctor's summary, the lab tests ordered in it (linking to released reports), the prescriptions
/// from it, and its bill. Arriving from the booking page (<c>?booked=1</c>) announces the booking. Every result is announced
/// in an always-rendered status region.
/// </summary>
public partial class AppointmentDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Whether the booking just made is announced (arrived with <c>?booked=1</c>).</summary>
    private bool announceBooked;

    private ElementReference? cancelButton;

    private Modal cancelModal = default!;

    private string? cancelReason;

    private ElementReference? confirmBox;

    private FocusTarget focusTarget;

    private int? loadedId;

    /// <summary>The new time picked for a reschedule, waiting for confirmation.</summary>
    private DateTime? newStart;

    /// <summary>Whether "Cancel appointment" was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingCancel;

    private ElementReference? rescheduleButton;

    private string? rescheduleError;

    private ElementReference? rescheduleHeading;

    private bool rescheduleOpen;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The booking announcement waits until the guard shows the page (the status region exists), then the address loses
        // ?booked=1 so a reload doesn't announce it again.
        if (announceBooked && focusTarget == FocusTarget.None && statusRegion is not null && Appointment is not null && BookedQuery is not null)
        {
            await HcDisplay.FocusAsync(statusRegion);
            Navigation.NavigateTo(HcDisplay.AppointmentUrl(Id), replace: true);

            return;
        }

        var target = focusTarget;
        focusTarget = FocusTarget.None;

        var element = target switch
        {
            FocusTarget.Status => statusRegion,
            FocusTarget.CancelButton => cancelButton,
            FocusTarget.RescheduleButton => rescheduleButton,
            FocusTarget.RescheduleHeading => rescheduleHeading,
            FocusTarget.Confirm => confirmBox,
            _ => null,
        };

        if (element is not null)
            await HcDisplay.FocusAsync(element);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Only another appointment starts over; dropping ?booked=1 from the address keeps the announcement.
        if (loadedId == Id)
            return;

        loadedId = Id;
        announceBooked = BookedQuery == 1;
        statusMessage = null;
        statusIsError = false;
        rescheduleOpen = false;
        rescheduleError = null;
        newStart = null;
        pendingCancel = false;
        cancelReason = null;
        focusTarget = FocusTarget.None;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        announceBooked = false;
        statusIsError = !ok;
        statusMessage = message;
        focusTarget = FocusTarget.Status;
    }

    /// <summary>Returns "call Cedarline Downtown at (555) 010-2200", or a general line without a clinic.</summary>
    private static string CallText(HcClinic? clinic) => clinic is null ? "call the clinic" : $"call {clinic.Name} at {clinic.Phone}";

    /// <summary>Returns whether a booked appointment can still be changed online (more than 2 hours away).</summary>
    private bool CanChange(HcAppointment appointment) => appointment.Status == HcAppointmentStatus.Booked && appointment.Start > State.Clock.Now.AddHours(2);

    /// <summary>Returns what the patient can do now with an appointment that can't be changed online.</summary>
    private string ChangeSentence(HcAppointment appointment, HcClinic? clinic) => appointment.Status switch
    {
        HcAppointmentStatus.Booked when appointment.Start <= State.Clock.Now =>
            $"This visit's time has passed without a check-in. If you're at the clinic, go to the front desk; otherwise {CallText(clinic)}.",
        HcAppointmentStatus.Booked =>
            $"It's less than 2 hours before your visit, so it can't be changed or cancelled online. To change or cancel it, {CallText(clinic)}.",
        HcAppointmentStatus.CheckedIn => "You're checked in. The doctor will call you in soon.",
        HcAppointmentStatus.InVisit => "You're with the doctor now. The summary appears here once the visit is completed.",
        HcAppointmentStatus.Completed => "This visit is completed. To see the doctor again, book a follow-up.",
        HcAppointmentStatus.Cancelled => "This appointment was cancelled, so its time is open for others.",
        HcAppointmentStatus.NoShow => "This visit was missed. To see the doctor, book a new time.",
        _ => "This appointment can't be changed online.",
    };

    private static string ChangeTitle(HcAppointment appointment) => appointment.Status switch
    {
        HcAppointmentStatus.Booked => "Change or cancel",
        HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit => "Today's visit",
        _ => "What's next",
    };

    /// <summary>Hides the dialog; the cancellation runs once it is hidden.</summary>
    private async Task ConfirmCancelAsync()
    {
        pendingCancel = true;
        await cancelModal.HideAsync();
    }

    /// <summary>Moves the appointment to the picked time and maps every result.</summary>
    private void ConfirmReschedule()
    {
        if (newStart is not { } target || Appointment is not { } appointment)
            return;

        rescheduleError = null;
        var clinic = State.FindClinic(appointment.ClinicId);
        var result = State.Reschedule(appointment.Id, target);

        switch (result)
        {
            case HcResult.Ok:
                rescheduleOpen = false;
                newStart = null;
                Announce(true, $"Your appointment moved to {SlotPicker.SlotName(appointment.Start)}, at {State.FindClinic(appointment.ClinicId)?.Name}.");
                break;

            case HcResult.NoChange:
                rescheduleError = "That's already your appointment time. Choose a different time, or keep the current one.";
                newStart = null;
                break;

            case HcResult.Unavailable:
                rescheduleError = $"Sorry, {SlotPicker.SlotName(target)} was just taken, or it isn't open any more. Your appointment didn't move: choose another time.";
                newStart = null;
                focusTarget = FocusTarget.RescheduleHeading;
                break;

            case HcResult.Conflict:
                rescheduleError = $"You already have another visit at {HcDisplay.When(target)}. Your appointment didn't move: choose another time.";
                newStart = null;
                focusTarget = FocusTarget.RescheduleHeading;
                break;

            case HcResult.WrongState:
                rescheduleOpen = false;
                newStart = null;
                Announce(false, $"This appointment can't be moved online any more: it's less than 2 hours away, or it's no longer booked. To change it, {CallText(clinic)}.");
                break;

            case HcResult.NotAllowed:
                Announce(false, "Only the patient's own account can move this appointment. Nothing changed.");
                break;

            case HcResult.NotFound:
                Announce(false, "This appointment can't be found any more, so nothing changed.");
                break;

            default:
                rescheduleError = "The appointment couldn't be moved. Try again.";
                break;
        }
    }

    private void KeepCurrentTime()
    {
        newStart = null;
        rescheduleError = null;
        rescheduleOpen = false;
        focusTarget = FocusTarget.RescheduleButton;
    }

    /// <summary>Runs a confirmed cancellation, or returns focus to "Cancel appointment" (the dialog doesn't).</summary>
    private void OnCancelHidden()
    {
        if (!pendingCancel)
        {
            focusTarget = FocusTarget.CancelButton;

            return;
        }

        pendingCancel = false;

        if (Appointment is not { } appointment)
        {
            Announce(false, "This appointment can't be found any more, so nothing changed.");

            return;
        }

        var when = SlotPicker.SlotName(appointment.Start);
        var clinic = State.FindClinic(appointment.ClinicId);
        var result = State.CancelAppointment(appointment.Id, cancelReason ?? string.Empty);

        switch (result)
        {
            case HcResult.Ok:
                rescheduleOpen = false;
                newStart = null;
                cancelReason = null;
                Announce(true, $"Your appointment on {when} is cancelled. The time is open for other patients now.");
                break;

            case HcResult.WrongState:
                Announce(false, $"This appointment can't be cancelled online any more: it's less than 2 hours away, or it's no longer booked. To cancel it, {CallText(clinic)}.");
                break;

            case HcResult.Invalid:
                Announce(false, $"The reason is too long: use {HcState.MaxReasonLength} characters or fewer. Nothing was cancelled.");
                break;

            case HcResult.NotAllowed:
                Announce(false, "Only the patient's own account can cancel this appointment. Nothing was cancelled.");
                break;

            case HcResult.NotFound:
                Announce(false, "This appointment can't be found any more, so nothing was cancelled.");
                break;

            default:
                Announce(false, "The appointment couldn't be cancelled. Try again.");
                break;
        }
    }

    /// <summary>A picked time waits for confirmation, which takes focus so it is read out.</summary>
    private void OnPickNew(HcSlot slot)
    {
        newStart = slot.Start;
        rescheduleError = null;
        focusTarget = FocusTarget.Confirm;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenCancelAsync()
    {
        pendingCancel = false;
        await cancelModal.ShowAsync();
    }

    /// <summary>Returns the status in a sentence, like "Booked" or "Cancelled on Nov 20, 2026".</summary>
    private static string StatusSentence(HcAppointment appointment) => appointment.Status switch
    {
        HcAppointmentStatus.Cancelled when appointment.CancelledAt is { } cancelled => $"Cancelled on {HcDisplay.Date(cancelled)}",
        HcAppointmentStatus.CheckedIn when appointment.CheckedInAt is { } checkedIn => $"Checked in at {HcDisplay.Time(checkedIn)}",
        HcAppointmentStatus.Completed when appointment.CompletedAt is { } completed => $"Completed on {HcDisplay.Date(completed)}",
        HcAppointmentStatus.NoShow => "Missed (no-show)",
        _ => HcDisplay.StatusLabel(appointment.Status),
    };

    private void ToggleReschedule()
    {
        rescheduleOpen = !rescheduleOpen;
        newStart = null;
        rescheduleError = null;
        focusTarget = rescheduleOpen ? FocusTarget.RescheduleHeading : FocusTarget.None;
    }

    /// <summary>Returns the visit's bill, if it has one.</summary>
    private HcBill? VisitBill(HcAppointment appointment) => State.BillsOf(appointment.PatientId).FirstOrDefault(bill => bill.AppointmentId == appointment.Id);

    /// <summary>Returns the lab tests ordered in a visit.</summary>
    private IReadOnlyList<HcLabOrder> VisitLabs(HcAppointment appointment) =>
        State.LabOrdersOf(appointment.PatientId).Where(order => order.AppointmentId == appointment.Id).ToList();

    /// <summary>
    /// Returns the prescriptions from a visit: the same doctor, prescribed on the visit's day. A prescription doesn't record
    /// its visit, so this matches by doctor and day.
    /// </summary>
    private IReadOnlyList<HcPrescription> VisitPrescriptions(HcAppointment appointment) =>
        State.PrescriptionsOf(appointment.PatientId).Where(item => item.DoctorId == appointment.DoctorId && item.PrescribedAt.Date == appointment.Start.Date).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The appointment, when it is the signed-in patient's.</summary>
    private HcAppointment? Appointment => State.CurrentPatient is null ? null : State.FindAppointment(Id);

    /// <summary>Gets or sets <c>1</c> when the patient arrives from booking it.</summary>
    [SupplyParameterFromQuery(Name = "booked")] public int? BookedQuery { get; set; }

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Appointments", Href = $"{HcLayout.Base}/appointments" },
        new() { Text = Appointment is { } appointment ? HcDisplay.When(appointment.Start) : "Appointment", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the appointment ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Status,
        CancelButton,
        RescheduleButton,
        RescheduleHeading,
        Confirm,
    }

    #endregion
}
