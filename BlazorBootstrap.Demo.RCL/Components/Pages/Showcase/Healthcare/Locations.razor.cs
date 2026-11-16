using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The clinics (public): a map (or its placeholder without a Google Maps key) with one numbered pin per clinic, and a card
/// per clinic with the same number, its address, phone, hours, how to get directions (in words, no external link), and the
/// doctors who see patients there, linked to their profiles.
/// </summary>
public partial class Locations : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the doctors based at a clinic, then those with weekly hours there.</summary>
    private IReadOnlyList<HcDoctor> DoctorsAt(string clinicId) =>
        State.Doctors
            .Where(doctor => doctor.ClinicId == clinicId || State.AvailabilityOf(doctor.Id).Any(block => block.ClinicId == clinicId))
            .OrderBy(doctor => doctor.ClinicId == clinicId ? 0 : 1)
            .ToList();

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the clinics as map locations, in the cards' order.</summary>
    private IReadOnlyList<ShowcaseLocation> MapLocations =>
        State.Clinics.Select(clinic => new ShowcaseLocation(clinic.Name, clinic.Address, clinic.Latitude, clinic.Longitude)).ToList();

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
