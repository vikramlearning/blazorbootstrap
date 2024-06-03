﻿namespace BlazorBootstrap;

/// <summary>
/// This component represents the title of a <see cref="Card"/>. <br/>
/// If no title is required, it can be omitted from the card implementation.
/// </summary>
public partial class CardTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.CardTitle)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card title size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H5" />.
    /// </remarks>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H5;

    #endregion
}
