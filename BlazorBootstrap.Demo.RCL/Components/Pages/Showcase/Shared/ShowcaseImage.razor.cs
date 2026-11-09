namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// Renders a committed showcase image lazily, or a sized placeholder until the asset is supplied.
/// </summary>
public partial class ShowcaseImage : ComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the placeholder accent color. Defaults to the site's primary color.
    /// </summary>
    [Parameter] public string? Accent { get; set; }

    /// <summary>
    /// Gets or sets the alternative text. Leave empty for decorative images.
    /// </summary>
    [Parameter] public string? Alt { get; set; }

    /// <summary>
    /// Gets or sets a short caption shown inside the placeholder.
    /// </summary>
    [Parameter] public string? Caption { get; set; }

    /// <summary>
    /// Gets or sets extra CSS classes.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether the image loads eagerly (above the fold) instead of lazily.
    /// </summary>
    [Parameter] public bool Eager { get; set; }

    /// <summary>
    /// Gets or sets the intrinsic height used for the aspect ratio.
    /// </summary>
    [Parameter] public int Height { get; set; } = 600;

    /// <summary>
    /// Gets or sets the icon shown in the placeholder.
    /// </summary>
    [Parameter] public IconName PlaceholderIcon { get; set; } = IconName.Image;

    /// <summary>
    /// Gets or sets the image path relative to the app base.
    /// </summary>
    [Parameter] public string? Src { get; set; }

    /// <summary>
    /// Gets or sets the intrinsic width used for the aspect ratio.
    /// </summary>
    [Parameter] public int Width { get; set; } = 800;

    #endregion
}
