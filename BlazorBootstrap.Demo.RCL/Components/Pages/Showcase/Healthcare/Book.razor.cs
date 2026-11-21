using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Booking a visit with a doctor, for a signed-in patient, in an <see cref="EditForm" /> (rule WR-002): the kind of visit
/// (a new patient visit of 60 minutes for a first visit with the doctor, a standard visit of 30 minutes after that — a
/// cross-field rule against the patient's history), the time (required: from the link, <c>?time=yyyy-MM-ddTHH:mm</c>, or a
/// <see cref="SlotPicker" /> that offers only times with the whole visit free; it must still be free for the chosen visit's
/// length — a cross-field rule — and is checked again, with a visible "Checking…", when the form is sent), the reason
/// (required, 3 to 200 characters), and the health plan and member ID from the patient's record (a member ID of the plan's
/// letters and nine digits, required for a plan: a cross-field rule; a change is saved to the record). Messages show after
/// a field is touched or on submit, and a failed submit shows a summary that links to each field. Every
/// <see cref="HcState.Book" /> result is mapped to a field or the alert; success opens the appointment page with
/// <c>?booked=1</c>, which announces it. The doctor and clinic sit beside the form.
/// </summary>
public partial class Book : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How long the free-time check takes, so its pending state can be seen (a real portal asks the clinic's calendar).</summary>
    private static readonly TimeSpan CheckDelay = TimeSpan.FromMilliseconds(300);

    private string? bookError;

    private int bookErrors;

    private bool booking;

    /// <summary>Whether the time is being checked again before booking.</summary>
    private bool checkingTime;

    /// <summary>The patient's other appointment at the chosen time, after a Conflict.</summary>
    private int? conflictId;

    private EditContext editContext;

    private FocusTarget focusTarget;

    private BookForm form = new();

    /// <summary>The doctor and query the form was set up for.</summary>
    private string? loadedKey;

    private bool pickerOpen;

    /// <summary>The state's answers that belong to a field (a time just taken), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private ElementReference timeHeading;

    /// <summary>The announcement after a time is picked.</summary>
    private string? timeMessage;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty form.</summary>
    public Book() => editContext = new EditContext(form);

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var target = focusTarget;
        focusTarget = FocusTarget.None;

        if (target == FocusTarget.Time)
            await HcDisplay.FocusAsync(timeHeading);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // A different doctor or link starts a fresh form.
        var key = $"{DoctorId}|{TimeQuery}|{State.CurrentPatient?.Id}";

        if (loadedKey == key)
            return;

        loadedKey = key;
        var patient = State.CurrentPatient;

        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = new BookForm
        {
            Start = DateTime.TryParseExact(TimeQuery, HcDisplay.QuerySlot, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time) ? time : null,
            VisitType = HasSeenDoctor ? HcVisitType.Standard : HcVisitType.NewPatient,
            PayerId = patient?.InsurancePayerId ?? string.Empty,
            MemberId = patient?.InsuranceMemberId ?? string.Empty,
        };
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
        UpdateRules();
        pickerOpen = false;
        booking = false;
        bookError = null;
        timeMessage = null;
        conflictId = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// A new visit type changes the length the time must have free; choosing self-pay clears the member ID; the member ID is
    /// upper-cased as typed. Any change clears the last alert and the state's message on that field.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        bookError = null;
        conflictId = null;
        UpdateRules();

        switch (args.FieldIdentifier.FieldName)
        {
            case nameof(BookForm.VisitType) when form.Start is not null:
                serverErrors?.Clear(FieldIdentifier.Create(() => form.Start));
                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.Start));
                break;

            case nameof(BookForm.PayerId):
                if (form.PayerId.Length == 0)
                    form.MemberId = string.Empty;

                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.MemberId));
                break;

            case nameof(BookForm.MemberId) when form.MemberId.ToUpperInvariant() != form.MemberId:
                form.MemberId = form.MemberId.ToUpperInvariant();
                break;
        }
    }

    /// <summary>Returns the patient's live appointment overlapping a visit, or <see langword="null" />.</summary>
    private HcAppointment? OverlapAt(DateTime start, int minutes) =>
        State.CurrentPatient is { } patient
            ? State.PatientAppointments(patient.Id).FirstOrDefault(item => item.Status is not (HcAppointmentStatus.Cancelled or HcAppointmentStatus.NoShow)
                && item.Start < start.AddMinutes(minutes) && start < item.Start.AddMinutes(item.Minutes))
            : null;

    /// <summary>A picked time becomes the choice; the picker closes and focus returns to "Your time".</summary>
    private void OnPick(HcSlot slot)
    {
        form.Start = slot.Start;
        editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.Start));
        pickerOpen = false;
        bookError = null;
        conflictId = null;
        timeMessage = $"You chose {SlotPicker.SlotName(slot.Start)}.";
        focusTarget = FocusTarget.Time;
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        UpdateRules();
        StateHasChanged();
    });

    private void ServerError(System.Linq.Expressions.Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void ShowError(string problem)
    {
        bookErrors = bookError?.StartsWith(problem, StringComparison.Ordinal) == true ? bookErrors + 1 : 1;
        bookError = bookErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bookErrors})");
    }

    /// <summary>
    /// Checks the form, checks again that the time is still free for the whole visit (the pending state shows meanwhile), books,
    /// and maps every result to a field or the alert (rule WR-002).
    /// </summary>
    private async Task SubmitAsync()
    {
        // Guards a double submit: the first successful one navigates away.
        if (booking || checkingTime || Doctor is not { } doctor)
            return;

        bookError = null;
        conflictId = null;
        serverErrors?.Clear();
        UpdateRules();

        if (!editContext.Validate())
            return;

        checkingTime = true;
        StateHasChanged();
        await Task.Delay(CheckDelay);
        checkingTime = false;
        UpdateRules();

        var minutes = HcVisitTypes.MinutesOf(form.VisitType);
        var start = form.Start!.Value;

        if (State.FindSlot(doctor.Id, start, minutes) is null)
        {
            ServerError(() => form.Start, $"{SlotPicker.SlotName(start)} was just taken, or it isn't open for a {minutes}-minute visit any more. Choose another time; nothing was booked.");
            pickerOpen = true;

            return;
        }

        var result = State.Book(doctor.Id, start, form.VisitType, form.Reason ?? string.Empty, form.PayerId, form.MemberId, out var booked);

        switch (result)
        {
            case HcResult.Ok when booked is not null:
                booking = true;
                Navigation.NavigateTo($"{HcDisplay.AppointmentUrl(booked.Id)}?booked=1");
                break;

            case HcResult.Unavailable:
                ServerError(() => form.Start, $"Sorry, {SlotPicker.SlotName(start)} was just taken, or it isn't open any more. Nothing was booked: choose another time.");
                pickerOpen = true;
                break;

            case HcResult.Conflict:
                var other = OverlapAt(start, minutes);
                conflictId = other?.Id;
                ServerError(() => form.Start, other is null
                    ? $"You already have a visit at {HcDisplay.When(start)}. Choose another time; nothing was booked."
                    : $"You already have a visit at {HcDisplay.When(other.Start)} with {State.FindDoctor(other.DoctorId)?.Name}. Choose another time; nothing was booked.");
                pickerOpen = true;
                break;

            case HcResult.WrongState:
                ShowError($"{doctor.Name} isn't taking new patients right now, so nothing was booked. Find another doctor in the same specialty.");
                break;

            case HcResult.Invalid when HcState.MemberIdProblem(form.PayerId, form.MemberId) is { } problem:
                ServerError(() => form.MemberId, problem);
                break;

            case HcResult.Invalid:
                ShowError(UpcomingCount >= HcState.MaxUpcoming
                    ? $"You already have {HcState.MaxUpcoming} upcoming appointments, the most you can hold. Cancel one, or book after your next visit. Nothing was booked."
                    : "The appointment couldn't be saved: this tab's demo storage is full, or a value is out of range. Nothing was booked.");
                break;

            case HcResult.NotAllowed:
                ShowError("Only a signed-in patient account can book. Sign in as a patient and try again; nothing was booked.");
                break;

            case HcResult.NotFound:
                ShowError("This doctor can't be found any more, so nothing was booked.");
                break;

            default:
                ShowError("The appointment couldn't be booked. Try again.");
                break;
        }
    }

    private void TogglePicker()
    {
        pickerOpen = !pickerOpen;
        timeMessage = null;
    }

    /// <summary>Gives the form what its cross-field rules check: the doctor, whether the patient has seen them, and whether the chosen time fits the visit.</summary>
    private void UpdateRules()
    {
        form.DoctorName = Doctor?.Name ?? "the doctor";
        form.HasSeenDoctor = HasSeenDoctor;
        form.TimeFits = form.Start is { } start && Doctor is { } doctor && State.FindSlot(doctor.Id, start, HcVisitTypes.MinutesOf(form.VisitType)) is not null;
        form.TimeFitsStandard = form.Start is { } time && Doctor is { } known && State.FindSlot(known.Id, time) is not null;
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

    /// <summary>The chosen time when it is still open for the chosen visit's length, with its clinic; otherwise <see langword="null" />.</summary>
    private HcSlot? ChosenSlot => form.Start is { } start && Doctor is { } doctor ? State.FindSlot(doctor.Id, start, HcVisitTypes.MinutesOf(form.VisitType)) : null;

    private HcDoctor? Doctor => State.FindDoctor(DoctorId);

    /// <summary>Gets or sets the doctor ID from the route.</summary>
    [Parameter] public string DoctorId { get; set; } = string.Empty;

    /// <summary>Whether the signed-in patient has had a visit with this doctor (who may then book while closed to new patients).</summary>
    private bool HasSeenDoctor => State.CurrentPatient is { } patient && State.HasSeen(patient.Id, DoctorId);

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
        Time,
    }

    /// <summary>
    /// The booking's fields, with the limits of <see cref="HcState.Book" />, and what the page knows for the cross-field
    /// rules: whether the patient has seen the doctor, and whether the time fits the visit's length.
    /// </summary>
    private sealed class BookForm : IValidatableObject
    {
        /// <summary>The doctor's name, for the messages.</summary>
        public string DoctorName { get; set; } = "the doctor";

        /// <summary>Whether the patient has had a visit with the doctor.</summary>
        public bool HasSeenDoctor { get; set; }

        public string MemberId { get; set; } = string.Empty;

        public string PayerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tell the doctor what the visit is for.")]
        [StringLength(HcState.MaxReasonLength, MinimumLength = HcState.MinVisitReasonLength, ErrorMessage = "Describe the reason in 3 to 200 characters.")]
        public string? Reason { get; set; }

        [Required(ErrorMessage = "Choose a time for the visit.")]
        public DateTime? Start { get; set; }

        /// <summary>Whether the chosen time is open for the chosen visit's whole length.</summary>
        public bool TimeFits { get; set; }

        /// <summary>Whether the chosen time is open for a standard visit (so a too-long visit can be named as the reason).</summary>
        public bool TimeFitsStandard { get; set; }

        public HcVisitType VisitType { get; set; }

        /// <summary>Runs once the fields are filled in: the visit type against the patient's history, the time against the visit's length, and the health plan.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VisitType == HcVisitType.NewPatient && HasSeenDoctor)
                yield return new ValidationResult($"You've seen {DoctorName} before, so this is a standard visit of {HcVisitTypes.StandardMinutes} minutes. Choose that kind of visit.", new[] { nameof(VisitType) });
            else if (VisitType == HcVisitType.Standard && !HasSeenDoctor)
                yield return new ValidationResult($"This is your first visit with {DoctorName}: choose a new patient visit of {HcVisitTypes.NewPatientMinutes} minutes, so there's time to take your history.", new[] { nameof(VisitType) });

            if (Start is { } start && !TimeFits)
            {
                yield return new ValidationResult(TimeFitsStandard && VisitType == HcVisitType.NewPatient
                    ? $"{HcDisplay.Time(start)} has only {HcVisitTypes.StandardMinutes} minutes free, and a new patient visit takes {HcVisitTypes.NewPatientMinutes}. Choose another time below."
                    : $"{HcDisplay.When(start)} isn't open any more. Choose another time below.", new[] { nameof(Start) });
            }

            if (HcState.MemberIdProblem(PayerId, MemberId) is { } problem)
                yield return new ValidationResult(problem, new[] { nameof(MemberId) });
        }
    }

    #endregion
}
