using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Find a doctor (public): filters for specialty, clinic, language, and "Accepting new patients", kept in the query string
/// (<c>?specialty=</c>, <c>?clinic=</c>, <c>?language=</c>, <c>?accepting=yes</c>) so a filtered list can be linked, and a
/// name search that stays on the page. Each result card has the doctor's photo (or initials), name, specialty, clinic, languages,
/// whether they take new patients (in words), the next open time as a link to book it, and a link to the profile. The
/// count is announced in a status line; an empty result says how to widen the search.
/// </summary>
public partial class FindDoctor : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool acceptingOnly;

    private string? clinicId;

    private ElementReference countRegion;

    private string? language;

    private string nameSearch = string.Empty;

    private string? specialty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>Reads the filters from the query; unknown values are ignored.</summary>
    protected override void OnParametersSet()
    {
        specialty = SpecialtyQuery is not null && State.Specialties.Contains(SpecialtyQuery) ? SpecialtyQuery : null;
        clinicId = State.FindClinic(ClinicQuery) is { } clinic ? clinic.Id : null;
        language = LanguageQuery is not null && Languages.Contains(LanguageQuery) ? LanguageQuery : null;
        acceptingOnly = string.Equals(AcceptingQuery, "yes", StringComparison.Ordinal);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CountText(int count) => count switch
    {
        0 => "No doctors match.",
        1 => "1 doctor matches.",
        _ => FormattableString.Invariant($"{count} doctors match."),
    };

    /// <summary>Clears every filter; the button that was pressed may disappear, so focus moves to the announced count.</summary>
    private async Task ClearFiltersAsync()
    {
        nameSearch = string.Empty;
        specialty = clinicId = language = null;
        acceptingOnly = false;
        UpdateQuery();

        await HcDisplay.FocusAsync(countRegion);
    }

    private bool Matches(HcDoctor doctor)
    {
        var name = nameSearch.Trim();

        return (specialty is null || doctor.Specialty == specialty)
            && (clinicId is null || doctor.ClinicId == clinicId || State.AvailabilityOf(doctor.Id).Any(block => block.ClinicId == clinicId))
            && (language is null || doctor.Languages.Contains(language))
            && (!acceptingOnly || doctor.AcceptingNewPatients)
            && (name.Length == 0 || doctor.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    private void OnAcceptingChanged(ChangeEventArgs e)
    {
        acceptingOnly = e.Value is true;
        UpdateQuery();
    }

    private void OnClinicChanged(ChangeEventArgs e)
    {
        clinicId = State.FindClinic(e.Value?.ToString()) is { } clinic ? clinic.Id : null;
        UpdateQuery();
    }

    private void OnLanguageChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString();
        language = value is not null && Languages.Contains(value) ? value : null;
        UpdateQuery();
    }

    private void OnSpecialtyChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString();
        specialty = value is not null && State.Specialties.Contains(value) ? value : null;
        UpdateQuery();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Puts the filters (never the name search) into the address, replacing the history entry.</summary>
    private void UpdateQuery() =>
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["specialty"] = specialty,
            ["clinic"] = clinicId,
            ["language"] = language,
            ["accepting"] = acceptingOnly ? "yes" : null,
        }), replace: true);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets <c>?accepting=yes</c>: only doctors taking new patients.</summary>
    [SupplyParameterFromQuery(Name = "accepting")] public string? AcceptingQuery { get; set; }

    /// <summary>Gets or sets the clinic ID from the query.</summary>
    [SupplyParameterFromQuery(Name = "clinic")] public string? ClinicQuery { get; set; }

    private bool HasFilters => specialty is not null || clinicId is not null || language is not null || acceptingOnly || nameSearch.Trim().Length > 0;

    /// <summary>Gets the languages the doctors speak, by name.</summary>
    private IReadOnlyList<string> Languages => State.Doctors.SelectMany(doctor => doctor.Languages).Distinct().Order(StringComparer.Ordinal).ToList();

    /// <summary>Gets or sets the language from the query.</summary>
    [SupplyParameterFromQuery(Name = "language")] public string? LanguageQuery { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets the doctors that match the filters, in the network's order.</summary>
    private IReadOnlyList<HcDoctor> Results => State.Doctors.Where(Matches).ToList();

    /// <summary>Gets or sets the specialty from the query.</summary>
    [SupplyParameterFromQuery(Name = "specialty")] public string? SpecialtyQuery { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
