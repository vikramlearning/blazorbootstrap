using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Billing rates (account managers, AC-7f): the rate card in an <see cref="EditForm" /> (rule WR-002: each role's hourly
/// rate required, a whole number from <see cref="PbState.MinRate" /> to <see cref="PbState.MaxRate" />), with how many
/// people bill at each role. Saving (<see cref="PbState.SaveRates" />) audits each changed rate; new draft invoices use
/// the new rates.
/// </summary>
public partial class Rates : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private RatesForm form = new();

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private string? statusText;

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

    private void Load()
    {
        form = new RatesForm
        {
            Design = State.RateFor(PbRateRole.Design), Engineering = State.RateFor(PbRateRole.Engineering), Quality = State.RateFor(PbRateRole.Quality),
            Content = State.RateFor(PbRateRole.Content), Management = State.RateFor(PbRateRole.Management),
        };
        editContext = new EditContext(form);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private string People(PbRateRole role)
    {
        var count = State.People.Count(person => PbSeedData.RoleFor(person.Title) == role);

        return count == 1 ? "1 person on the team." : string.Create(CultureInfo.InvariantCulture, $"{count} people on the team.");
    }

    private async Task Save()
    {
        if (!await editContext.ValidateAsync())
            return;

        var result = State.SaveRates(new Dictionary<PbRateRole, int>
        {
            [PbRateRole.Design] = form.Design!.Value, [PbRateRole.Engineering] = form.Engineering!.Value, [PbRateRole.Quality] = form.Quality!.Value,
            [PbRateRole.Content] = form.Content!.Value, [PbRateRole.Management] = form.Management!.Value,
        });
        var message = result switch
        {
            PbResult.Ok => "Saved the rates. New draft invoices use them.",
            PbResult.NoChange => "Nothing changed, so there was nothing to save.",
            PbResult.NotAllowed => "Only account managers can change the rates. Nothing was saved.",
            _ => "Those rates couldn't be saved. Nothing was saved.",
        };
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = result is not (PbResult.Ok or PbResult.NoChange);
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The rate card: each role's hourly rate, a whole number from 50 to 400.</summary>
    private sealed class RatesForm
    {
        [Required(ErrorMessage = "Enter the content rate.")]
        [Range(PbState.MinRate, PbState.MaxRate, ErrorMessage = "Use a content rate from 50 to 400.")]
        public int? Content { get; set; }

        [Required(ErrorMessage = "Enter the design rate.")]
        [Range(PbState.MinRate, PbState.MaxRate, ErrorMessage = "Use a design rate from 50 to 400.")]
        public int? Design { get; set; }

        [Required(ErrorMessage = "Enter the engineering rate.")]
        [Range(PbState.MinRate, PbState.MaxRate, ErrorMessage = "Use an engineering rate from 50 to 400.")]
        public int? Engineering { get; set; }

        [Required(ErrorMessage = "Enter the management rate.")]
        [Range(PbState.MinRate, PbState.MaxRate, ErrorMessage = "Use a management rate from 50 to 400.")]
        public int? Management { get; set; }

        [Required(ErrorMessage = "Enter the quality assurance rate.")]
        [Range(PbState.MinRate, PbState.MaxRate, ErrorMessage = "Use a quality assurance rate from 50 to 400.")]
        public int? Quality { get; set; }
    }

    #endregion
}
