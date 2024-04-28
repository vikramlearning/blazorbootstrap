﻿namespace BlazorBootstrap;

public partial class Card : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Card)
            .AddClass(TextAlignment.ToTextAlignmentClass())
            .AddClass(Color.ToCardColorClass())
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card color.
    /// </summary>
    [Parameter]
    public CardColor Color { get; set; }

    /// <summary>
    /// Gets or sets the text alignment of the card.
    /// </summary>
    [Parameter]
    public Alignment TextAlignment { get; set; }

    #endregion
}
