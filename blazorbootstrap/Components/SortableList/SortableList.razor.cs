namespace BlazorBootstrap;

public partial class SortableList : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private List<SortableListItem>? items = new();

    private DotNetObjectReference<SortableList>? objRef;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass("list-group");

        base.BuildClasses();
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        QueueAfterRenderAction(async () => await SortableListJsInterop.InitializeAsync(objRef!, ElementId!), new RenderPriority());
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing) items = null;

        await base.DisposeAsync(disposing);
    }

    /// <summary>
    /// Adds an <see cref="SortableListItem" /> to the collection.
    /// </summary>
    /// <param name="item">The <see cref="SortableListItem" /> to add.</param>
    internal void Add(SortableListItem item)
    {
        if (items is null)
            items = new List<SortableListItem>();

        if (item is not null)
            items.Add(item);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside the <see cref="SortableList" />.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Provides JavaScript interop functionality for the Sortable List.
    /// </summary>
    [Inject] private SortableListJsInterop SortableListJsInterop { get; set; } = default!;

    #endregion
}
