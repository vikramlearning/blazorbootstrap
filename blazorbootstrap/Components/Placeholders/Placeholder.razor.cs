using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Placeholder : BaseComponent
{
    #region Members

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Placeholder());
        builder.Append(BootstrapClassProvider.PlaceholderWidth(Width));
        builder.Append(BootstrapClassProvider.PlaceholderColor(Color), Color != PlaceholderColor.None);
        builder.Append(BootstrapClassProvider.PlaceholderSize(Size), Size != PlaceholderSize.None);

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the placeholder width. Default is <see cref="PlaceholderWidth.Col1"/>.
    /// </summary>
    [Parameter] public PlaceholderWidth Width { get; set; } = PlaceholderWidth.Col1;

    /// <summary>
    /// Gets or sets the placeholder color. Default is <see cref="PlaceholderColor.None"/>.
    /// </summary>
    [Parameter] public PlaceholderColor Color { get; set; } = PlaceholderColor.None;

    /// <summary>
    /// Gets or sets the placeholder size. Default is <see cref="PlaceholderSize.None"/>.
    /// </summary>
    [Parameter] public PlaceholderSize Size { get; set; } = PlaceholderSize.None;

    #endregion
}
