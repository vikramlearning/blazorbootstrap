using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// The signed-in person's profile: the shared account form (<see cref="ShowcaseAccountProfile" />: name, email, password),
/// then "Your details in the directory" in a validated form (rule WR-002): phone (up to 30 characters), pronouns (up to 20),
/// and bio (up to 300, with a live count), saved with <see cref="HrState.SaveMyProfile" />; and the facts HR keeps (title,
/// department, manager, location, start date, work email), read-only with "Ask HR to change these". A failed submit shows a
/// summary that links to each field; every result is announced in an always-rendered status line, which takes focus.
/// </summary>
public partial class Profile : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The bio as typed, for the live count (the bound value changes only on change).</summary>
    private string bioText = string.Empty;

    private EditContext editContext = new(new DetailsForm());

    private bool focusStatus;

    private DetailsForm form = new();

    /// <summary>The person whose details fill the form (it fills once the tab's session is restored).</summary>
    private int? loadedEmployeeId;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Moves focus to the outcome, after the render that shows it.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!focusStatus)
            return;

        focusStatus = false;
        await HrDisplay.FocusAsync(statusRegion);
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
        editContext = new EditContext(form);
        bioText = me.Bio;
    }

    private void OnBioInput(ChangeEventArgs args) => bioText = args.Value?.ToString() ?? string.Empty;

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadForm(false);
        StateHasChanged();
    });

    /// <summary>Saves the three fields once the form is valid (a failed submit shows the summary), and maps every result.</summary>
    private async Task Save()
    {
        statusMessage = null;

        if (!await editContext.ValidateAsync())
            return;

        switch (State.SaveMyProfile(form.Phone ?? string.Empty, form.Pronouns ?? string.Empty, form.Bio ?? string.Empty))
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
                statusMessage = "Your details couldn't be saved: this tab's demo storage is full, or a value is too long. Nothing changed.";
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

        focusStatus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the bio's length against its limit, like "120 of 300 characters".</summary>
    private string BioCountText => string.Create(CultureInfo.InvariantCulture, $"{bioText.Length} of {HrState.MaxBioLength} characters");

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The directory details as typed, with the limits of <see cref="HrState.SaveMyProfile" />; all three are optional.</summary>
    private sealed class DetailsForm
    {
        [StringLength(HrState.MaxBioLength, ErrorMessage = "Shorten your bio to 300 characters or fewer.")]
        public string? Bio { get; set; }

        [StringLength(HrState.MaxPhoneLength, ErrorMessage = "Use 30 characters or fewer for the phone number.")]
        [RegularExpression(@"^[0-9+()\-.\s]*$", ErrorMessage = "Use digits, spaces, and + ( ) - . only for the phone number.")]
        public string? Phone { get; set; }

        [StringLength(HrState.MaxPronounsLength, ErrorMessage = "Use 20 characters or fewer for your pronouns.")]
        public string? Pronouns { get; set; }
    }

    #endregion
}
