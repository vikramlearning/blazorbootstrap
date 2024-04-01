namespace BlazorBootstrap;

public partial class SortableList<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CancellationTokenSource cancellationTokenSource = default!;

    private DotNetObjectReference<SortableList<TItem>>? objRef;

    private string filter = ".bb-sortable-list-item-disabled";

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

        QueueAfterRenderAction(async () => await SortableListJsInterop.InitializeAsync(ElementId!, Name!, Handle!, Group!, AllowSorting, Pull.ToSortableListPullMode(), Put.ToSortableListPutMode(), filter, objRef!), new RenderPriority());
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing) Data = null!;

        await base.DisposeAsync(disposing);
    }

    [JSInvokable]
    public async Task OnUpdateJS(int oldIndex, int newIndex)
    {
        if (OnUpdate.HasDelegate)
            await OnUpdate.InvokeAsync(new(oldIndex, newIndex));
    }

    [JSInvokable]
    public async Task OnRemoveJS(int oldIndex, int newIndex)
    {
        if (OnRemove.HasDelegate)
            await OnRemove.InvokeAsync(new(oldIndex, newIndex));
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside the <see cref="SortableList" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    [Parameter]
    public List<TItem> Data { get; set; } = default!;

    /// <summary>
    /// Template to render when there are no items to display.
    /// </summary>
    [Parameter]
    public RenderFragment EmptyDataTemplate { get; set; } = default!;

    /// <summary>
    /// Shows text on no records.
    /// </summary>
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    [Parameter]
    public Func<TItem, bool> DisableItem { get; set; } = default!;

    [Parameter]
    public string? DisabledItemCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the group.
    /// </summary>
    [Parameter]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the drag handle selector.
    /// </summary>
    [Parameter]
    public string? Handle { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Template to render when data load in-progress.
    /// </summary>
    [Parameter]
    public RenderFragment LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the <see cref="SortableList<TItem>" />.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public EventCallback<SortableListEventArgs> OnUpdate { get; set; }

    [Parameter]
    public EventCallback<SortableListEventArgs> OnRemove { get; set; }

    [Parameter]
    public SortableListPullMode Pull { get; set; }

    [Parameter]
    public SortableListPutMode Put { get; set; }

    [Parameter]
    public bool AllowSorting { get; set; } = true;

    /// <summary>
    /// Provides JavaScript interop functionality for the Sortable List.
    /// </summary>
    [Inject] private SortableListJsInterop SortableListJsInterop { get; set; } = default!;

    #endregion
}
