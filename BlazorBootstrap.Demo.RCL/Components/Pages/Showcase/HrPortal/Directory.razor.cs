using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Directory (everyone signed in): <see cref="HrState.Directory" /> as cards with the initials, name, pronouns, title,
/// department, location, and "Joining Dec 1" for people who haven't started. A search for a name or job title stays on the
/// page; the department and location filters are kept in the query string (<c>?department=</c>, <c>?location=</c>) so a
/// filtered list can be linked. The count is announced in a status line; an empty result says how to widen the search.
/// </summary>
public partial class Directory : ComponentBase, IDisposable
{
    #region Fields and Constants

    private ElementReference countRegion;

    private string? departmentId;

    private string? location;

    private string search = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>Reads the filters from the query; unknown values are ignored.</summary>
    protected override void OnParametersSet()
    {
        departmentId = State.FindDepartment(DepartmentQuery)?.Id;
        location = LocationQuery is not null && State.Locations.Contains(LocationQuery) ? LocationQuery : null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CountText(int count, int total) =>
        count == total ? $"{HrDisplay.Plural(count, "person", "people")} in the directory."
        : count == 0 ? "Nobody matches."
        : $"{HrDisplay.Plural(count, "person", "people")} of {total} match.";

    /// <summary>Clears the search and the filters; the button that was pressed may disappear, so focus moves to the announced count.</summary>
    private async Task ClearFiltersAsync()
    {
        search = string.Empty;
        departmentId = location = null;
        UpdateQuery();

        await HrDisplay.FocusAsync(countRegion);
    }

    private bool Matches(HrEmployee person)
    {
        var text = search.Trim();

        return (departmentId is null || person.DepartmentId == departmentId)
            && (location is null || person.Location == location)
            && (text.Length == 0 || person.Name.Contains(text, StringComparison.OrdinalIgnoreCase) || person.Title.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private void OnDepartmentChanged(ChangeEventArgs e)
    {
        departmentId = State.FindDepartment(e.Value?.ToString())?.Id;
        UpdateQuery();
    }

    private void OnLocationChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString();
        location = value is not null && State.Locations.Contains(value) ? value : null;
        UpdateQuery();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Puts the filters (never the search) into the address, replacing the history entry.</summary>
    private void UpdateQuery() =>
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["department"] = departmentId,
            ["location"] = location,
        }), replace: true);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the department ID from the query.</summary>
    [SupplyParameterFromQuery(Name = "department")] public string? DepartmentQuery { get; set; }

    private bool HasFilters => departmentId is not null || location is not null || search.Trim().Length > 0;

    /// <summary>Gets or sets the location from the query.</summary>
    [SupplyParameterFromQuery(Name = "location")] public string? LocationQuery { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets the people who match the search and filters, by name.</summary>
    private IReadOnlyList<HrEmployee> Results => State.Directory.Where(Matches).ToList();

    [Inject] private HrState State { get; set; } = default!;

    #endregion
}
