using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Registers (<c>/new</c>) or edits a patient (staff) in an <see cref="EditForm" /> (rule WR-002): medical record number
/// (required, CL- and six digits, upper-cased as typed, unique at the clinics, checked as you type and again on submit),
/// full name (required, up to 80 characters), date of birth (required; not in the future and at most 120 years ago, a rule
/// against today), phone and email (required, of the right form), home clinic (required), an emergency contact (both a name
/// and a phone or neither, and a phone other than the patient's: cross-field rules), and a health plan with its member ID
/// (required for a plan, of the plan's three letters and nine digits: a cross-field rule; cleared for self-pay). A patient
/// with their own sign-in keeps the name and email of their account. Messages show after a field is touched or on submit,
/// and a failed submit shows a summary that links to each field; every <see cref="HcState.SavePatient" /> result is mapped
/// to a field, the status message, or the alert. Registering moves to the patient's edit route and keeps the message.
/// Opening a patient's details is audited once the tab's data is restored (<see cref="HcState.RecordChartView" />).
/// </summary>
public partial class PatientEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after registering a patient: the form and its status message stay.</summary>
    private int? createdId;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private bool focusStatus;

    private PatientForm form = new();

    /// <summary>The route ID the form was loaded for (0 for a new patient), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    /// <summary>The patient whose details opening is already in the audit log.</summary>
    private int? loggedFor;

    private ShowcaseUniqueCheck? mrnCheck;

    /// <summary>The state's answers that belong to a field (a taken record number), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The look is logged once the tab's data is restored (a log entry before that would be replaced by the restore).
        if (Session?.IsRestored == true && Id is { } id && loggedFor != id && State.IsStaff && Patient is not null)
        {
            loggedFor = id;
            State.RecordChartView(id, details: true);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await HcDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        // Load once the patient can be found (a link opened before the tab's data is restored loads after it).
        if (loadedId == routeId && (routeId == 0 || form.Name.Length > 0))
            return;

        loadedId = routeId;

        // The first save of a new patient moves to their edit route: the form and the status message stay as they are.
        if (createdId is not null && createdId == routeId)
        {
            createdId = null;

            return;
        }

        statusMessage = null;
        statusIsError = false;
        ClearError();
        Load(Patient);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-patient route has no ID, and the router reuses this page when moving from a
    /// patient to "Register a patient", so the previous patient's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    private static string DateValue(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
        focusStatus = true;
    }

    private void ClearError()
    {
        errorMessage = errorText = null;
        errorCount = 0;
    }

    private Task<bool> IsMrnTakenAsync(string mrn) => Task.FromResult(State.IsMrnTaken(mrn, Patient?.Id ?? 0));

    /// <summary>Fills the form from a patient, or with a new patient's starting values (the next free record number, the front desk's clinic).</summary>
    private void Load(HcPatient? patient)
    {
        var next = new PatientForm
        {
            Mrn = patient?.Mrn ?? State.SuggestMrn(),
            Name = patient?.Name ?? string.Empty,
            DateOfBirth = patient?.DateOfBirth,
            Phone = patient?.Phone ?? string.Empty,
            Email = patient?.Email ?? string.Empty,
            ClinicId = patient?.ClinicId ?? State.CurrentStaffProfile?.ClinicId ?? string.Empty,
            EmergencyName = patient?.EmergencyContactName ?? string.Empty,
            EmergencyPhone = patient?.EmergencyContactPhone ?? string.Empty,
            PayerId = patient?.InsurancePayerId ?? string.Empty,
            MemberId = patient?.InsuranceMemberId ?? string.Empty,
            Today = State.Clock.Today,
        };

        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    private string MemberIdHelp(string payerId) =>
        State.FindPayer(payerId) is { } payer ? $"{payer.MemberIdPrefix} and nine digits, as printed on the {payer.Name} card." : "Not needed for self-pay.";

    private string MrnTakenMessage(string mrn) =>
        State.Patients.FirstOrDefault(item => item.Id != (Patient?.Id ?? 0) && string.Equals(item.Mrn, mrn.Trim(), StringComparison.OrdinalIgnoreCase)) is { } holder
            ? $"{mrn.Trim().ToUpperInvariant()} is {holder.Name}'s record number. Use the suggested number, or check you're not registering them twice."
            : $"{mrn.Trim().ToUpperInvariant()} belongs to another patient. Choose another record number.";

    /// <summary>
    /// The record number and member ID are upper-cased as typed; choosing self-pay clears the member ID; the patient's phone
    /// checks the emergency phone again (it must differ). Any change clears the last error and the state's message on that field.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);

        switch (args.FieldIdentifier.FieldName)
        {
            case nameof(PatientForm.Mrn) when form.Mrn.ToUpperInvariant() != form.Mrn:
                form.Mrn = form.Mrn.ToUpperInvariant();
                break;

            case nameof(PatientForm.MemberId) when form.MemberId.ToUpperInvariant() != form.MemberId:
                form.MemberId = form.MemberId.ToUpperInvariant();
                break;

            case nameof(PatientForm.PayerId):
                if (form.PayerId.Length == 0)
                    form.MemberId = string.Empty;

                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.MemberId));
                break;

            case nameof(PatientForm.Phone) when form.EmergencyPhone.Length > 0:
                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.EmergencyPhone));
                break;
        }

        ClearError();
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // A link opened before the tab's data was restored finds the patient once it is.
        if (Id is not null && form.Name.Length == 0 && Patient is not null)
            Load(Patient);

        StateHasChanged();
    });

    /// <summary>
    /// Saves the patient once the form is valid: the record number check finishes first, then every rule runs, and a failed
    /// submit shows the summary (rule WR-002). The state checks again; its answer maps to a field, the status message, or the alert.
    /// </summary>
    private async Task SaveAsync()
    {
        var existing = Patient;

        if (Id is not null && existing is null)
            return;

        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();
        form.Today = State.Clock.Today;

        var mrnFree = mrnCheck is null || await mrnCheck.CheckNowAsync();

        if (!editContext.Validate() || !mrnFree)
            return;

        var input = new HcPatientInput(form.Mrn, form.Name, form.DateOfBirth!.Value, form.Phone, form.Email, form.ClinicId, form.EmergencyName, form.EmergencyPhone, form.PayerId,
            form.MemberId);
        var result = State.SavePatient(existing?.Id ?? 0, input, out var saved);

        switch (result)
        {
            case HcResult.Ok when saved is not null:
                Load(saved);

                if (existing is null)
                {
                    Announce(true, $"Registered {saved.Name} with record number {saved.Mrn}. They're on the patient list now.");

                    // The edit route renders the same page, so the form and the status message stay.
                    createdId = saved.Id;
                    loggedFor = saved.Id;
                    Navigation.NavigateTo(HcDisplay.PatientDetailsUrl(saved.Id), replace: true);

                    return;
                }

                Announce(true, $"Saved {saved.Name}'s details.");

                return;

            case HcResult.Duplicate:
                ServerError(() => form.Mrn, MrnTakenMessage(form.Mrn));

                return;

            case HcResult.NoChange:
                Announce(true, "Nothing changed: these details are already saved.");

                return;

            case HcResult.NotFound:
                ShowError("This patient's record was removed, so the changes couldn't be saved.");

                return;

            case HcResult.NotAllowed:
                ShowError("Only clinic staff can register patients and change their details. Sign in again; nothing was saved.");

                return;

            case HcResult.Invalid when HcState.MemberIdProblem(form.PayerId, form.MemberId) is { } problem:
                ServerError(() => form.MemberId, problem);

                return;

            case HcResult.Invalid:
                ShowError(existing is null && State.Patients.Count >= HcState.MaxPatients
                    ? $"This tab already holds {HcState.MaxPatients} patients, the most it can. Nothing was saved."
                    : "The details couldn't be saved: the storage for this demo tab is full, or a value is out of range. Nothing was saved.");

                return;

            default:
                ShowError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the patient ID from the route, or <see langword="null" /> on the new-patient route.</summary>
    [Parameter] public int? Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private HcPatient? Patient => Id is { } id ? State.FindPatient(id) : null;

    [CascadingParameter] private ShowcaseSession? Session { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The patient's fields, with the limits of <see cref="HcState.SavePatient" />: required text with lengths and forms, a
    /// date of birth against today, and the cross-field rules of the emergency contact and the health plan.
    /// </summary>
    private sealed class PatientForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose the patient's home clinic.")]
        public string ClinicId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the date of birth.")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Enter the patient's email address.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer for the email address.")]
        [EmailAddress(ErrorMessage = "Enter the email address like name@example.com.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(ShowcaseIdentity.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the emergency contact's name.")]
        public string EmergencyName { get; set; } = string.Empty;

        public string EmergencyPhone { get; set; } = string.Empty;

        public string MemberId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the medical record number.")]
        [RegularExpression("^[Cc][Ll]-[0-9]{6}$", ErrorMessage = "Use CL- and six digits for the record number, like the one suggested.")]
        public string Mrn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the patient's full name.")]
        [StringLength(ShowcaseIdentity.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        public string PayerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the patient's phone number.")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>The clinics' today, set by the page before it validates.</summary>
        public DateTime Today { get; set; } = HcSeedData.Now.Date;

        /// <summary>Runs once the fields are filled in: the date of birth against today, the phone's form, and the emergency contact and health plan rules.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth is { } dob && dob.Date > Today)
                yield return new ValidationResult($"The date of birth can't be in the future (today is {HcDisplay.Date(Today)}).", new[] { nameof(DateOfBirth) });
            else if (DateOfBirth is { } born && born.Date < Today.AddYears(-HcState.MaxAgeYears))
                yield return new ValidationResult($"Check the year: that makes the patient older than {HcState.MaxAgeYears}.", new[] { nameof(DateOfBirth) });

            if (Phone.Trim().Length > 0 && !HcState.IsPhone(Phone.Trim()))
                yield return new ValidationResult("Enter the phone with digits, spaces, and + - ( ), at least 7 digits, like (555) 014-2290.", new[] { nameof(Phone) });

            var contactName = EmergencyName.Trim();
            var contactPhone = EmergencyPhone.Trim();

            if (contactName.Length > 0 && contactPhone.Length == 0)
                yield return new ValidationResult($"Add a phone number for {contactName}, or clear the emergency contact's name.", new[] { nameof(EmergencyPhone) });
            else if (contactName.Length == 0 && contactPhone.Length > 0)
                yield return new ValidationResult("Add the emergency contact's name, or clear their phone.", new[] { nameof(EmergencyName) });
            else if (contactPhone.Length > 0 && !HcState.IsPhone(contactPhone))
                yield return new ValidationResult("Enter the emergency contact's phone with digits, spaces, and + - ( ), at least 7 digits.", new[] { nameof(EmergencyPhone) });
            else if (contactPhone.Length > 0 && !HcState.ValidEmergencyContact(contactName, contactPhone, Phone))
                yield return new ValidationResult("The emergency contact needs a phone other than the patient's own.", new[] { nameof(EmergencyPhone) });

            if (HcState.MemberIdProblem(PayerId, MemberId) is { } problem)
                yield return new ValidationResult(problem, new[] { nameof(MemberId) });
        }
    }

    #endregion
}
