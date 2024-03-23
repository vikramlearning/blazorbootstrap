namespace BlazorBootstrap;

public partial class SortableListItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass("list-group-item");

        base.BuildClasses();
    }

    protected override void OnInitialized()
    {
        ElementId = IdGenerator.GetNextId(); // This is required
        Parent.Add(this);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside the <see cref="SortableListItem" />.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the active <see cref="SortableListItem" />.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter]
    internal SortableList Parent { get; set; } = default!;

    #endregion
}
