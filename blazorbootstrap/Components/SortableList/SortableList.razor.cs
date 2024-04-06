namespace BlazorBootstrap;

/// <summary>
/// Represents a sortable list component.
/// </summary>
/// <typeparam name="TItem">The type of items in the list.</typeparam>
public partial class SortableList<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// A cancellation token source for managing asynchronous operations.
    /// </summary>
    private CancellationTokenSource cancellationTokenSource = default!;

    /// <summary>
    /// A DotNetObjectReference that allows JavaScript interop with this component.
    /// </summary>
    private DotNetObjectReference<SortableList<TItem>>? objRef;

    /// <summary>
    /// A CSS selector used to filter disabled items.
    /// </summary>
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
    public async Task OnAddJS(int oldIndex, int newIndex)
    {
        if (OnAdd.HasDelegate)
            await OnAdd.InvokeAsync(new(oldIndex, newIndex));
    }

    [JSInvokable]
    public async Task OnRemoveJS(int oldIndex, int newIndex, string fromListName, string toListName)
    {
        if (OnRemove.HasDelegate)
            await OnRemove.InvokeAsync(new(oldIndex, newIndex, fromListName, toListName));
    }

    [JSInvokable]
    public async Task OnUpdateJS(int oldIndex, int newIndex)
    {
        if (OnUpdate.HasDelegate)
            await OnUpdate.InvokeAsync(new(oldIndex, newIndex));
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets a value indicating whether sorting is allowed for the list.
    /// </summary>
    [Parameter]
    public bool AllowSorting { get; set; } = true;

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
    /// Gets or sets a delegate that determines whether an item should be disabled.
    /// </summary>
    [Parameter]
    public Func<TItem, bool> DisableItem { get; set; } = default!;

    /// <summary>
    /// Gets or sets the CSS class applied to disabled items.
    /// </summary>
    [Parameter]
    public string? DisabledItemCssClass { get; set; } = default!;

    /// <summary>
    /// Specifies the template to render when there are no items to display in the list.
    /// </summary>
    [Parameter]
    public RenderFragment EmptyDataTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the text to display when there are no records in the list.
    /// </summary>
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Gets or sets the group name associated with the list.
    /// </summary>
    [Parameter]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the CSS selector for the drag handle element.
    /// </summary>
    [Parameter]
    public string? Handle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the list is currently loading.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Specifies the template used to render individual items in the list.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Specifies the template to render while the list data is loading.
    /// </summary>
    [Parameter]
    public RenderFragment LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the <see cref="SortableList{TItem}" /> component.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an event callback that fires when an item is added to the list.
    /// </summary>
    [Parameter]
    public EventCallback<SortableListEventArgs> OnAdd { get; set; }

    /// <summary>
    /// Gets or sets an event callback that fires when an item is removed from the list.
    /// </summary>
    [Parameter]
    public EventCallback<SortableListEventArgs> OnRemove { get; set; }

    /// <summary>
    /// Gets or sets an event callback that fires when an item is updated in the list.
    /// </summary>
    [Parameter]
    public EventCallback<SortableListEventArgs> OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets the pull mode for the sortable list.
    /// </summary>
    [Parameter]
    public SortableListPullMode Pull { get; set; }

    /// <summary>
    /// Gets or sets the put mode for the sortable list.
    /// </summary>
    [Parameter]
    public SortableListPutMode Put { get; set; }

    /// <summary>
    /// Provides JavaScript interop functionality for the Sortable List.
    /// </summary>
    [Inject] private SortableListJsInterop SortableListJsInterop { get; set; } = default!;

    #endregion
}
