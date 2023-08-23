namespace BlazorBootstrap;

public partial class DropdownMenu
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(ClassProvider.DropdownMenu());
        builder.Append(ClassProvider.DropdownMenuPosition(Position));

        base.BuildClasses(builder);
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
    /// Gets or sets the dropdown menu position.
    /// </summary>
    [Parameter]
    public DropdownMenuPosition Position { get; set; }

    #endregion
}