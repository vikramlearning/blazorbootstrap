using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's profile: the shared account form (<see cref="ShowcaseAccountProfile" />: name, email, password),
/// then "Your details": the medical record number, and an <see cref="EditForm" /> (rule WR-002) for the date of birth
/// (required; not in the future and at most 120 years ago), the phone (required, of the right form), an emergency contact
/// (both a name and a phone or neither, and a phone other than the patient's: cross-field rules), and the health plan with
/// its member ID (required for a plan, of the plan's letters and nine digits: a cross-field rule), saved with
/// <see cref="HcState.SaveMyDetails" />. Messages show after a field is touched or on submit, and a failed submit shows a
/// summary that links to each field; every result is announced in an always-rendered status line.
/// </summary>
public partial class Profile : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext;

    private DetailsForm form = new();

    /// <summary>The patient whose details fill the form (it fills once the tab's session is restored).</summary>
    private int? loadedPatientId;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private string? statusText;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty form.</summary>
    public Profile() => editContext = new EditContext(form);

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => LoadForm(false);

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

    private static string DateValue(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private void Announce(bool ok, string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = !ok;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Fills the form from the signed-in patient's record when it's a different patient (or always, when forced).</summary>
    private void LoadForm(bool force)
    {
        if (State.CurrentPatient is not { } patient)
        {
            loadedPatientId = null;

            return;
        }

        if (!force && loadedPatientId == patient.Id)
            return;

        loadedPatientId = patient.Id;

        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = new DetailsForm
        {
            DateOfBirth = patient.DateOfBirth,
            Phone = patient.Phone,
            EmergencyName = patient.EmergencyContactName,
            EmergencyPhone = patient.EmergencyContactPhone,
            PayerId = patient.InsurancePayerId,
            MemberId = patient.InsuranceMemberId,
            Today = State.Clock.Today,
        };
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>Choosing self-pay clears the member ID; the member ID is upper-cased as typed; a new phone checks the emergency phone again.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        switch (args.FieldIdentifier.FieldName)
        {
            case nameof(DetailsForm.PayerId):
                if (form.PayerId.Length == 0)
                    form.MemberId = string.Empty;

                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.MemberId));
                break;

            case nameof(DetailsForm.MemberId) when form.MemberId.ToUpperInvariant() != form.MemberId:
                form.MemberId = form.MemberId.ToUpperInvariant();
                break;

            case nameof(DetailsForm.Phone) when form.EmergencyPhone.Length > 0:
                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.EmergencyPhone));
                break;
        }
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadForm(false);
        StateHasChanged();
    });

    /// <summary>Saves the details once the form is valid, and maps every result.</summary>
    private async Task Save()
    {
        form.Today = State.Clock.Today;

        if (!await editContext.ValidateAsync())
            return;

        switch (State.SaveMyDetails(form.DateOfBirth, form.Phone, form.EmergencyName, form.EmergencyPhone, form.PayerId, form.MemberId))
        {
            case HcResult.Ok:
                LoadForm(true);
                Announce(true, "Your details are saved.");
                break;

            case HcResult.NoChange:
                Announce(true, "Nothing to save: these are already your details.");
                break;

            case HcResult.Invalid:
                Announce(false, "Your details couldn't be saved: this tab's demo storage is full, or a value is out of range. Nothing changed.");
                break;

            case HcResult.NotAllowed:
                Announce(false, "Only a patient account has these details. Nothing was saved.");
                break;

            default:
                Announce(false, "Your details couldn't be saved. Try again.");
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The details as entered, with the limits of <see cref="HcState.SaveMyDetails" /> and its cross-field rules.</summary>
    private sealed class DetailsForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter your date of birth.")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(ShowcaseIdentity.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the emergency contact's name.")]
        public string EmergencyName { get; set; } = string.Empty;

        public string EmergencyPhone { get; set; } = string.Empty;

        public string MemberId { get; set; } = string.Empty;

        public string PayerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the phone number the clinic can call you on.")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>The clinics' today, set by the page before it validates.</summary>
        public DateTime Today { get; set; } = HcSeedData.Now.Date;

        /// <summary>Runs once the fields are filled in: the date of birth against today, the phone's form, the emergency contact, and the health plan.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth is { } dob && dob.Date > Today)
                yield return new ValidationResult($"Your date of birth can't be in the future (today is {HcDisplay.Date(Today)}).", new[] { nameof(DateOfBirth) });
            else if (DateOfBirth is { } born && born.Date < Today.AddYears(-HcState.MaxAgeYears))
                yield return new ValidationResult($"Check the year: that would make you older than {HcState.MaxAgeYears}.", new[] { nameof(DateOfBirth) });

            if (Phone.Trim().Length > 0 && !HcState.IsPhone(Phone.Trim()))
                yield return new ValidationResult("Enter the phone with digits, spaces, and + - ( ), at least 7 digits.", new[] { nameof(Phone) });

            var contactName = EmergencyName.Trim();
            var contactPhone = EmergencyPhone.Trim();

            if (contactName.Length > 0 && contactPhone.Length == 0)
                yield return new ValidationResult($"Add a phone number for {contactName}, or clear the emergency contact's name.", new[] { nameof(EmergencyPhone) });
            else if (contactName.Length == 0 && contactPhone.Length > 0)
                yield return new ValidationResult("Add the emergency contact's name, or clear their phone.", new[] { nameof(EmergencyName) });
            else if (contactPhone.Length > 0 && !HcState.IsPhone(contactPhone))
                yield return new ValidationResult("Enter the emergency contact's phone with digits, spaces, and + - ( ), at least 7 digits.", new[] { nameof(EmergencyPhone) });
            else if (contactPhone.Length > 0 && !HcState.ValidEmergencyContact(contactName, contactPhone, Phone))
                yield return new ValidationResult("Your emergency contact needs a phone other than your own.", new[] { nameof(EmergencyPhone) });

            if (HcState.MemberIdProblem(PayerId, MemberId) is { } problem)
                yield return new ValidationResult(problem, new[] { nameof(MemberId) });
        }
    }

    #endregion
}
