using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// A lab order's entered values as a table with a caption: test, result with its unit, reference range, and the flag in
/// words ("High", "Low", "Normal"), followed by the doctor's comment for the patient. Shown on the staff lab results page
/// and the patient chart; staff see entered values before they are released.
/// </summary>
public partial class LabsResultTable : ComponentBase
{
    #region Properties, Indexers

    /// <summary>Gets or sets the table caption, like "Lipid panel results for Maya Thompson".</summary>
    [Parameter, EditorRequired] public string Caption { get; set; } = string.Empty;

    /// <summary>Gets or sets the lab order.</summary>
    [Parameter, EditorRequired] public HcLabOrder Order { get; set; } = default!;

    /// <summary>Gets or sets the order's panel.</summary>
    [Parameter, EditorRequired] public HcLabPanel Panel { get; set; } = default!;

    #endregion
}
