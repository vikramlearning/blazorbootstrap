namespace BlazorBootstrap;

public partial class Dropdown
{
    #region Events

    #endregion

    #region Members

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.ButtonGroup());
        builder.Append(BootstrapClassProvider.DropdownDirection(Direction));

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="ChildContent"/>.
    /// </summary>
    [Parameter]     public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown direction.
    /// </summary>
    [Parameter] public DropdownDirection Direction { get; set; } = DropdownDirection.Dropdown;

    /// <summary>
    /// Gets or sets the toggle button split behavior.
    /// </summary>
    [Parameter] public bool Split { get; set; }

    #endregion
}
