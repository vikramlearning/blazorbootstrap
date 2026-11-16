using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's profile: the shared account form (<see cref="ShowcaseAccountProfile" />: name, email, password),
/// then "Your details": the medical record number (the patient ID), and the date of birth (a native date input) and phone,
/// saved with <see cref="HcState.SaveMyDetails" />. Field errors are tied to their inputs and the first takes focus; every
/// result is announced in an always-rendered status line.
/// </summary>
public partial class Profile : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? dobError;

    private ElementReference dobInput;

    private bool focusDob;

    private bool focusPhone;

    private DetailsForm form = new();

    /// <summary>The patient whose details fill the form (it fills once the tab's session is restored).</summary>
    private int? loadedPatientId;

    private string? phoneError;

    private ElementReference phoneInput;

    private bool statusIsError;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusDob)
        {
            focusDob = false;
            focusPhone = false;
            await HcDisplay.FocusAsync(dobInput);
        }
        else if (focusPhone)
        {
            focusPhone = false;
            await HcDisplay.FocusAsync(phoneInput);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => LoadForm(false);

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

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
        form = new DetailsForm
        {
            DateOfBirth = patient.DateOfBirth?.ToString(HcDisplay.QueryDate, CultureInfo.InvariantCulture) ?? string.Empty,
            Phone = patient.Phone,
        };
    }

    /// <summary>A native date input sends <c>yyyy-MM-dd</c>, or an empty value when cleared or incomplete.</summary>
    private void OnDateOfBirthChanged(ChangeEventArgs args) => form.DateOfBirth = args.Value?.ToString();

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadForm(false);
        StateHasChanged();
    });

    /// <summary>Checks both fields, saves them, and maps every result.</summary>
    private void Save()
    {
        statusMessage = null;
        dobError = null;
        phoneError = null;

        DateTime? dateOfBirth = null;
        var dobText = form.DateOfBirth?.Trim() ?? string.Empty;
        var today = State.Clock.Today;

        if (dobText.Length > 0)
        {
            if (!DateTime.TryParseExact(dobText, HcDisplay.QueryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                dobError = "Enter a full date of birth: day, month, and year.";
            else if (parsed < new DateTime(1900, 1, 1) || parsed > today)
                dobError = $"Enter a date of birth between January 1, 1900 and today ({HcDisplay.Date(today)}).";
            else
                dateOfBirth = parsed;
        }

        var phone = form.Phone?.Trim() ?? string.Empty;

        if (phone.Length > HcState.MaxPhoneLength)
            phoneError = $"Use {HcState.MaxPhoneLength} characters or fewer.";

        if (dobError is not null || phoneError is not null)
        {
            focusDob = dobError is not null;
            focusPhone = dobError is null;
            statusIsError = true;
            statusMessage = "Nothing was saved. Fix the highlighted details.";

            return;
        }

        switch (State.SaveMyDetails(dateOfBirth, phone))
        {
            case HcResult.Ok:
                LoadForm(true);
                statusIsError = false;
                statusMessage = "Your details are saved.";
                break;

            case HcResult.NoChange:
                statusIsError = false;
                statusMessage = "Nothing to save: these are already your details.";
                break;

            case HcResult.Invalid:
                statusIsError = true;
                statusMessage = "Your details couldn't be saved: this tab's demo storage is full. Nothing changed.";
                break;

            case HcResult.NotAllowed:
                statusIsError = true;
                statusMessage = "Only a patient account has these details. Nothing was saved.";
                break;

            default:
                statusIsError = true;
                statusMessage = "Your details couldn't be saved. Try again.";
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The details as typed: the date as <c>yyyy-MM-dd</c> (empty when unknown) and the phone.</summary>
    private sealed class DetailsForm
    {
        public string? DateOfBirth { get; set; }

        public string? Phone { get; set; }
    }

    #endregion
}
