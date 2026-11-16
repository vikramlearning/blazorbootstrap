using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Booking a visit with a doctor, for a signed-in patient: the time from the link (<c>?time=yyyy-MM-ddTHH:mm</c>), or a
/// <see cref="SlotPicker" /> to choose or change it (with a message in words when the linked time is no longer open), the
/// clinic, a required reason for the visit, and "Book appointment". Every <see cref="HcState.Book" /> result gets its own
/// message; success opens the appointment page with <c>?booked=1</c>, which announces it. The doctor and clinic sit beside
/// the form.
/// </summary>
public partial class Book : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? bookError;

    private bool booking;

    /// <summary>The chosen start, from the link or the picker; it may no longer be open.</summary>
    private DateTime? chosen;

    /// <summary>The patient's other appointment at the chosen time, after a Conflict.</summary>
    private int? conflictId;

    private FocusTarget focusTarget;

    private BookForm form = new();

    /// <summary>The doctor and query the form was set up for.</summary>
    private string? loadedKey;

    private bool pickerOpen;

    private string? reasonError;

    private ElementReference reasonInput;

    private ElementReference timeHeading;

    /// <summary>The announcement after a time is picked.</summary>
    private string? timeMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var target = focusTarget;
        focusTarget = FocusTarget.None;

        if (target == FocusTarget.Reason)
            await HcDisplay.FocusAsync(reasonInput);
        else if (target == FocusTarget.Time)
            await HcDisplay.FocusAsync(timeHeading);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // A different doctor or link starts a fresh form.
        var key = $"{DoctorId}|{TimeQuery}";

        if (loadedKey == key)
            return;

        loadedKey = key;
        chosen = DateTime.TryParseExact(TimeQuery, HcDisplay.QuerySlot, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time) ? time : null;
        form = new BookForm();
        pickerOpen = false;
        booking = false;
        bookError = null;
        reasonError = null;
        timeMessage = null;
        conflictId = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the patient's live appointment overlapping a start, or <see langword="null" />.</summary>
    private HcAppointment? OverlapAt(DateTime start) =>
        State.CurrentPatient is { } patient
            ? State.PatientAppointments(patient.Id).FirstOrDefault(item => item.Status is not (HcAppointmentStatus.Cancelled or HcAppointmentStatus.NoShow)
                && item.Start < start.AddMinutes(HcState.SlotMinutes) && start < item.Start.AddMinutes(item.Minutes))
            : null;

    /// <summary>A picked time becomes the choice; the picker closes and focus returns to "Your time".</summary>
    private void OnPick(HcSlot slot)
    {
        chosen = slot.Start;
        pickerOpen = false;
        bookError = null;
        conflictId = null;
        timeMessage = $"You chose {SlotPicker.SlotName(slot.Start)}.";
        focusTarget = FocusTarget.Time;
    }

    /// <summary>A shown reason error is re-checked as the patient types, so it goes away once fixed.</summary>
    private void OnReasonInput()
    {
        if (reasonError is not null)
            reasonError = ReasonProblem();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns why the reason can't be used, or <see langword="null" />.</summary>
    private string? ReasonProblem()
    {
        var length = form.Reason?.Trim().Length ?? 0;

        if (length == 0)
            return "Write the reason for the visit, so the doctor can prepare.";

        return length > HcState.MaxReasonLength ? $"Use {HcState.MaxReasonLength} characters or fewer." : null;
    }

    /// <summary>Checks the time and the reason, books, and maps every result to its own message.</summary>
    private void Submit()
    {
        // Guards a double submit: the first successful one navigates away.
        if (booking || Doctor is not { } doctor)
            return;

        bookError = null;
        conflictId = null;
        reasonError = ReasonProblem();
        var slot = ChosenSlot;

        if (slot is null)
        {
            bookError = chosen is { } missing
                ? $"{HcDisplay.When(missing)} isn't open any more. Choose another time above; nothing was booked."
                : "Choose a time above first; nothing was booked.";
            focusTarget = FocusTarget.Time;

            return;
        }

        if (reasonError is not null)
        {
            focusTarget = FocusTarget.Reason;

            return;
        }

        var result = State.Book(doctor.Id, slot.Start, form.Reason ?? string.Empty, out var booked);

        switch (result)
        {
            case HcResult.Ok when booked is not null:
                booking = true;
                Navigation.NavigateTo($"{HcDisplay.AppointmentUrl(booked.Id)}?booked=1");
                break;

            case HcResult.Unavailable:
                bookError = $"Sorry, {SlotPicker.SlotName(slot.Start)} was just taken, or it isn't open any more. Nothing was booked: choose another time above.";
                pickerOpen = true;
                focusTarget = FocusTarget.Time;
                break;

            case HcResult.Conflict:
                var other = OverlapAt(slot.Start);
                conflictId = other?.Id;
                bookError = other is null
                    ? $"You already have a visit at {HcDisplay.When(slot.Start)}. Nothing was booked: choose another time above."
                    : $"You already have a visit at {HcDisplay.When(other.Start)} with {State.FindDoctor(other.DoctorId)?.Name}. Nothing was booked: choose another time above.";
                pickerOpen = true;
                break;

            case HcResult.WrongState:
                bookError = $"{doctor.Name} isn't taking new patients right now, so nothing was booked. Find another doctor in the same specialty.";
                break;

            case HcResult.Invalid:
                if (ReasonProblem() is { } problem)
                {
                    reasonError = problem;
                    focusTarget = FocusTarget.Reason;
                }
                else if (UpcomingCount >= HcState.MaxUpcoming)
                {
                    bookError = $"You already have {HcState.MaxUpcoming} upcoming appointments, the most you can hold. Cancel one, or book after your next visit. Nothing was booked.";
                }
                else
                {
                    bookError = "The appointment couldn't be saved: this tab's demo storage is full. Nothing was booked.";
                }

                break;

            case HcResult.NotAllowed:
                bookError = "Only a signed-in patient account can book. Sign in as a patient and try again; nothing was booked.";
                break;

            case HcResult.NotFound:
                bookError = "This doctor can't be found any more, so nothing was booked.";
                break;

            default:
                bookError = "The appointment couldn't be booked. Try again.";
                break;
        }
    }

    private void TogglePicker()
    {
        pickerOpen = !pickerOpen;
        timeMessage = null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Find a doctor", Href = $"{HcLayout.Base}/doctors" },
        new() { Text = Doctor?.Name, Href = Doctor is null ? null : HcDisplay.DoctorUrl(Doctor.Id) },
        new() { Text = "Book", IsCurrentPage = true },
    };

    /// <summary>The chosen time when it is still open, with its clinic; otherwise <see langword="null" />.</summary>
    private HcSlot? ChosenSlot => chosen is { } start && Doctor is { } doctor ? State.Slots(doctor.Id, start.Date).FirstOrDefault(slot => slot.Start == start) : null;

    private HcDoctor? Doctor => State.FindDoctor(DoctorId);

    /// <summary>Gets or sets the doctor ID from the route.</summary>
    [Parameter] public string DoctorId { get; set; } = string.Empty;

    /// <summary>Whether the signed-in patient has had an appointment with this doctor (who may then book while closed to new patients).</summary>
    private bool HasSeenDoctor => State.CurrentPatient is { } patient && State.PatientAppointments(patient.Id).Any(item => item.DoctorId == DoctorId);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HcState State { get; set; } = default!;

    /// <summary>Gets or sets the chosen time from the query (yyyy-MM-ddTHH:mm).</summary>
    [SupplyParameterFromQuery(Name = "time")] public string? TimeQuery { get; set; }

    private int UpcomingCount => State.CurrentPatient is { } patient ? State.UpcomingOf(patient.Id).Count : 0;

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Reason,
        Time,
    }

    /// <summary>The reason for the visit.</summary>
    private sealed class BookForm
    {
        public string? Reason { get; set; }
    }

    #endregion
}
