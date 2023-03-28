namespace BlazorBootstrap;

public partial class Collapse
{
    #region Events

    #endregion

    #region Members

    private bool collapseHorizontal;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Collapse());
        builder.Append(BootstrapClassProvider.CollapseHorizontal(), this.CollapseHorizontal);

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Collapse"/>.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collapse horizontal.
    /// </summary>
    [Parameter]
    public bool CollapseHorizontal
    {
        get => collapseHorizontal;
        set
        {
            collapseHorizontal = value;
            DirtyClasses();
        }
    }

    #endregion
}

