namespace BlazorBootstrap;

public partial class CardSubTitle : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.CardSubTitle);

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
    /// Gets or sets the heading size.
    /// </summary>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H6;

    #endregion
}
