namespace BlazorBootstrap;

public partial class DropdownMenu : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
    
    /// <inheritdoc />
    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.DropdownMenu)
            .AddClass(Position.ToDropdownMenuPositionClass())
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown menu position.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownMenuPosition.Start" />.
    /// </remarks>
    [Parameter]
    public DropdownMenuPosition Position { get; set; } = DropdownMenuPosition.Start;

    #endregion
}
