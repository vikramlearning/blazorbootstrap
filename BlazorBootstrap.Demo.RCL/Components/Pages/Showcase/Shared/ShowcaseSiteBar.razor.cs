using BlazorBootstrap.Demo.RCL.Models.Showcase;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The slim toolbar above every showcase site: back to the gallery, docs, source, and theme.
/// </summary>
public partial class ShowcaseSiteBar : ComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the site this toolbar belongs to.
    /// </summary>
    [Parameter, EditorRequired] public ShowcaseSite Site { get; set; } = default!;

    #endregion
}
