namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// One person in the org chart's list view (<see cref="OrgChart" />): a link to their profile with their title,
/// department, and location, then their direct reports as a nested list in an open <c>&lt;details&gt;</c>, so screen
/// readers get the whole reporting tree as plain lists. People of the highlighted department are marked, with the
/// department name in bold as well as the color.
/// </summary>
public partial class OrgChartBranch : ComponentBase
{
    #region Properties, Indexers

    /// <summary>Gets or sets the selector that returns a department's name.</summary>
    [Parameter, EditorRequired] public Func<string, string> DepartmentName { get; set; } = id => id;

    /// <summary>Gets or sets the department to mark, or <see langword="null" /> for none.</summary>
    [Parameter] public string? HighlightDepartmentId { get; set; }

    /// <summary>Gets or sets the person, with their reports.</summary>
    [Parameter, EditorRequired] public OrgChartNode Node { get; set; } = default!;

    #endregion
}
