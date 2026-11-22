using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// A staff member's account page: the shared account form (<see cref="ShowcaseAccountProfile" />), then their work details
/// in an <see cref="EditForm" /> (rule WR-002). The front desk chooses the clinic they work at (required; they hear about
/// new, moved, and cancelled bookings there) and a work phone (optional, of the right form)
/// (<see cref="HcState.SaveStaffProfile" />). A doctor keeps a work phone, whether they take new patients, and the biography
/// on their profile (required, 20 to 600 characters) (<see cref="HcState.SaveDoctorProfile" />). Messages show after a field
/// is touched or on submit, a failed submit shows a summary that links to each field, and every result is announced.
/// </summary>
public partial class StaffAccount : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext;

    private WorkForm form = new();

    /// <summary>The account whose details fill the form.</summary>
    private string? loadedFor;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private string? statusText;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty form.</summary>
    public StaffAccount() => editContext = new EditContext(form);

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => LoadForm(false);

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = !ok;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Fills the form from the signed-in staff member's work details when it's a different account (or always, when forced).</summary>
    private void LoadForm(bool force)
    {
        var account = State.Identity.Current?.Id;

        if (!State.IsStaff || account is null || State.CurrentStaffProfile is not { } profile)
        {
            loadedFor = null;

            return;
        }

        if (!force && loadedFor == account)
            return;

        loadedFor = account;
        var doctor = State.CurrentDoctor;
        form = new WorkForm
        {
            IsDoctor = doctor is not null,
            ClinicId = profile.ClinicId,
            WorkPhone = profile.WorkPhone,
            AcceptingNewPatients = doctor?.AcceptingNewPatients ?? true,
            Bio = doctor?.Bio ?? string.Empty,
        };
        editContext = new EditContext(form);
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadForm(false);
        StateHasChanged();
    });

    /// <summary>Saves the work details once the form is valid, and maps every result.</summary>
    private async Task Save()
    {
        if (!await editContext.ValidateAsync())
            return;

        var result = form.IsDoctor ? State.SaveDoctorProfile(form.WorkPhone, form.AcceptingNewPatients, form.Bio ?? string.Empty) : State.SaveStaffProfile(form.ClinicId, form.WorkPhone);

        switch (result)
        {
            case HcResult.Ok:
                LoadForm(true);
                Announce(true, form.IsDoctor ? "Your work details are saved. Patients see the change on your profile." : $"Your work details are saved. You hear about bookings at {State.FindClinic(form.ClinicId)?.Name}.");
                break;

            case HcResult.NoChange:
                Announce(true, "Nothing to save: these are already your work details.");
                break;

            case HcResult.Invalid:
                Announce(false, "Your work details couldn't be saved: this tab's demo storage is full, or a value is out of range. Nothing changed.");
                break;

            case HcResult.NotAllowed:
                Announce(false, "Only staff accounts have work details. Sign in again; nothing was saved.");
                break;

            default:
                Announce(false, "Your work details couldn't be saved. Try again.");
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The work details as entered, with the limits of <see cref="HcState.SaveStaffProfile" /> and <see cref="HcState.SaveDoctorProfile" />.</summary>
    private sealed class WorkForm : IValidatableObject
    {
        public bool AcceptingNewPatients { get; set; } = true;

        public string? Bio { get; set; }

        public string ClinicId { get; set; } = string.Empty;

        /// <summary>Whether the account is a doctor (the biography rules) or the front desk (the clinic rule).</summary>
        public bool IsDoctor { get; set; }

        public string WorkPhone { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the role's required fields and the phone's form.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsDoctor && ClinicId.Length == 0)
                yield return new ValidationResult("Choose the clinic you work at.", new[] { nameof(ClinicId) });

            if (WorkPhone.Trim().Length > 0 && !HcState.IsPhone(WorkPhone.Trim()))
                yield return new ValidationResult("Enter the phone with digits, spaces, and + - ( ), at least 7 digits, or leave it empty.", new[] { nameof(WorkPhone) });

            var bio = (Bio ?? string.Empty).Trim();

            if (IsDoctor && bio.Length == 0)
                yield return new ValidationResult("Write a short biography for your public profile.", new[] { nameof(Bio) });
            else if (IsDoctor && bio.Length is < HcState.MinBioLength or > HcState.MaxBioLength)
                yield return new ValidationResult(string.Create(CultureInfo.InvariantCulture, $"Use {HcState.MinBioLength} to {HcState.MaxBioLength} characters for your biography (now {bio.Length})."), new[] { nameof(Bio) });
        }
    }

    #endregion
}
