using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// The signed-in person's profile: the shared account form (<see cref="ShowcaseAccountProfile" />: name, email, password),
/// then "Your details in the directory": phone, pronouns, and bio (with a character count), saved with
/// <see cref="HrState.SaveMyProfile" />; and the facts HR keeps (title, department, manager, location, start date, work
/// email), read-only with "Ask HR to change these". Field errors are tied to their inputs and the first takes focus after
/// the render that shows it; every result is announced in an always-rendered status line.
/// </summary>
public partial class Profile : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? bioError;

    private ElementReference bioInput;

    private ElementReference? focusTarget;

    private DetailsForm form = new();

    /// <summary>The person whose details fill the form (it fills once the tab's session is restored).</summary>
    private int? loadedEmployeeId;

    private string? phoneError;

    private ElementReference phoneInput;

    private string? pronounsError;

    private ElementReference pronounsInput;

    private bool statusIsError;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Moves focus to the first invalid field, after the render that shows its error.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is not { } target)
            return;

        focusTarget = null;
        await HrDisplay.FocusAsync(target);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => LoadForm(false);

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Fills the form from the signed-in person's record when it's a different person (or always, when forced).</summary>
    private void LoadForm(bool force)
    {
        if (State.CurrentEmployee is not { } me)
        {
            loadedEmployeeId = null;

            return;
        }

        if (!force && loadedEmployeeId == me.Id)
            return;

        loadedEmployeeId = me.Id;
        form = new DetailsForm { Phone = me.Phone, Pronouns = me.Pronouns, Bio = me.Bio };
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadForm(false);
        StateHasChanged();
    });

    /// <summary>Checks the three fields, saves them, and maps every result.</summary>
    private void Save()
    {
        statusMessage = null;
        phoneError = pronounsError = bioError = null;

        var phone = form.Phone?.Trim() ?? string.Empty;
        var pronouns = form.Pronouns?.Trim() ?? string.Empty;
        var bio = form.Bio?.Trim() ?? string.Empty;

        if (phone.Length > HrState.MaxPhoneLength)
            phoneError = $"Use {HrState.MaxPhoneLength} characters or fewer for the phone number.";

        if (pronouns.Length > HrState.MaxPronounsLength)
            pronounsError = $"Use {HrState.MaxPronounsLength} characters or fewer for your pronouns.";

        if (bio.Length > HrState.MaxBioLength)
            bioError = string.Create(CultureInfo.InvariantCulture, $"Shorten your bio to {HrState.MaxBioLength} characters or fewer; it has {bio.Length}.");

        if (phoneError is not null || pronounsError is not null || bioError is not null)
        {
            focusTarget = phoneError is not null ? phoneInput : pronounsError is not null ? pronounsInput : bioInput;
            statusIsError = true;
            statusMessage = "Nothing was saved. Fix the highlighted details.";

            return;
        }

        switch (State.SaveMyProfile(phone, pronouns, bio))
        {
            case HrResult.Ok:
                LoadForm(true);
                statusIsError = false;
                statusMessage = "Your details are saved. Colleagues see them in the directory now.";
                break;

            case HrResult.NoChange:
                statusIsError = false;
                statusMessage = "Nothing to save: these are already your details.";
                break;

            case HrResult.Invalid:
                statusIsError = true;
                statusMessage = "Your details couldn't be saved: this tab's demo storage is full. Nothing changed.";
                break;

            case HrResult.NotAllowed:
                statusIsError = true;
                statusMessage = "Your account isn't linked to an employee record, so there are no directory details to save.";
                break;

            case HrResult.NotFound:
                statusIsError = true;
                statusMessage = "Your employee record wasn't found. Nothing was saved.";
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

    /// <summary>Gets the bio's length against its limit, like "120 of 300 characters".</summary>
    private string BioCountText => string.Create(CultureInfo.InvariantCulture, $"{form.Bio?.Length ?? 0} of {HrState.MaxBioLength} characters");

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The directory details as typed.</summary>
    private sealed class DetailsForm
    {
        public string? Bio { get; set; }

        public string? Phone { get; set; }

        public string? Pronouns { get; set; }
    }

    #endregion
}
