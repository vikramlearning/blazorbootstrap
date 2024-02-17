﻿namespace BlazorBootstrap;

public partial class CardHeader : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.CardHeader);
        this.AddClass(BootstrapClassProvider.ToCardColor(Color));

        base.BuildClasses();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="ChildContent" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card header color.
    /// </summary>
    [Parameter]
    public CardColor Color { get; set; }

    #endregion
}
