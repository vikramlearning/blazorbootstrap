namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// A patient's allergies, shown prominently on the chart and the visit page: a warning icon and the word "Allergies" with
/// the list, or "none known" with a neutral look.
/// </summary>
public partial class PatientChartAllergies : ComponentBase
{
    #region Properties, Indexers

    /// <summary>Gets or sets the patient's allergies (plain text).</summary>
    [Parameter, EditorRequired] public IReadOnlyList<string> Allergies { get; set; } = Array.Empty<string>();

    #endregion
}
