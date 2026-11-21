using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The signed-in person's profile: name, email, and password (<see cref="ShowcaseAccountProfile" />), their work details in
/// an <see cref="EditForm" /> (rule WR-002: job title required, up to <see cref="PbState.MaxContactFieldLength" />
/// characters; weekly capacity 0 to <see cref="PbState.MaxCapacityHours" /> hours; <see cref="PbState.UpdateMyWork" />), then
/// their work at the agency: open and overdue task counts, what's due by Sunday against capacity, and links to the running
/// projects they're on.
/// </summary>
public partial class Profile : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private WorkForm form = new();

    /// <summary>The person the form was filled for, so a sign-in as someone else fills it again.</summary>
    private string? loadedFor;

    private int workCount;

    private bool workIsError;

    private string? workStatus;

    private string? workText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        Load();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        workCount = message == workText ? workCount + 1 : 1;
        workText = message;
        workIsError = !ok;
        workStatus = workCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({workCount})");
    }

    private void Load()
    {
        var person = State.CurrentPerson;
        loadedFor = person?.Id;
        form = new WorkForm { Title = person?.Title ?? string.Empty, CapacityHours = person?.CapacityHours };
        editContext = new EditContext(form);
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        if (State.CurrentPerson?.Id != loadedFor)
            Load();

        StateHasChanged();
    });

    /// <summary>Saves the work details once the form is valid; the state's answer is announced next to the button.</summary>
    private void SaveWork()
    {
        if (!editContext.Validate())
            return;

        var result = State.UpdateMyWork(form.Title, form.CapacityHours!.Value);

        Announce(result is PbResult.Ok or PbResult.NoChange, result switch
        {
            PbResult.Ok => "Saved your work details.",
            PbResult.NoChange => "Nothing changed, so there was nothing to save.",
            PbResult.NotAllowed => "Your sign-in ended. Sign in again to change your work details. Nothing was saved.",
            PbResult.Invalid => "Those values couldn't be saved: the storage for this demo tab may be full. Nothing was saved.",
            _ => "That didn't work, and nothing was saved. Try again.",
        });
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The person's work details, with the limits of <see cref="PbState.UpdateMyWork" />.</summary>
    private sealed class WorkForm
    {
        [Required(ErrorMessage = "Enter your weekly capacity in hours, like 32.")]
        [Range(0, PbState.MaxCapacityHours, ErrorMessage = "Use a weekly capacity from 0 to 60 hours.")]
        public int? CapacityHours { get; set; }

        [Required(ErrorMessage = "Enter your job title, like Front-end developer.")]
        [StringLength(PbState.MaxContactFieldLength, ErrorMessage = "Use 80 characters or fewer for your job title.")]
        public string Title { get; set; } = string.Empty;
    }

    #endregion
}
