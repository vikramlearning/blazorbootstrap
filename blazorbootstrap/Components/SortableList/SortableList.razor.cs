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
    /// A CSS selector used to filter disabled items.
    /// </summary>
    private string filter = ".bb-sortable-list-item-disabled";

    /// <summary>
    /// A DotNetObjectReference that allows JavaScript interop with this component.
    /// </summary>
    private DotNetObjectReference<SortableList<TItem>>? objRef;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing) Data = null!;

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await SortableListJsInterop.InitializeAsync(Id!, Name!, Handle!, Group!, AllowSorting, Pull.ToSortableListPullMode(), Put.ToSortableListPutMode(), filter, objRef!);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task OnAddJS(int oldIndex, int newIndex)
    {
        if (OnAdd.HasDelegate)
            await OnAdd.InvokeAsync(new SortableListEventArgs(oldIndex, newIndex));
    }

    [JSInvokable]
    public async Task OnRemoveJS(int oldIndex, int newIndex, string fromListName, string toListName)
    {
        if (OnRemove.HasDelegate)
            await OnRemove.InvokeAsync(new SortableListEventArgs(oldIndex, newIndex, fromListName, toListName));
    }

    [JSInvokable]
    public async Task OnUpdateJS(int oldIndex, int newIndex)
    {
        if (OnUpdate.HasDelegate)
            await OnUpdate.InvokeAsync(new SortableListEventArgs(oldIndex, newIndex));
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, ("list-group", true));

    /// <summary>
    /// Gets or sets a value indicating whether sorting is allowed for the list.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether sorting is allowed for the list.")]
    [Parameter]
    public bool AllowSorting { get; set; } = true;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the items.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the items.")]
    [Parameter]
    public List<TItem>? Data { get; set; }

    /// <summary>
    /// Gets or sets the CSS class applied to disabled items.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS class applied to disabled items.")]
    [Parameter]
    public string? DisabledItemCssClass { get; set; }

    /// <summary>
    /// Gets or sets a delegate that determines whether an item should be disabled.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a delegate that determines whether an item should be disabled.")]
    [Parameter]
    public Func<TItem, bool>? DisableItem { get; set; }

    /// <summary>
    /// Gets or sets the empty data template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the empty data template.")]
    [Parameter]
    public RenderFragment? EmptyDataTemplate { get; set; }

    /// <summary>
    /// Gets or sets the text to display when there are no records in the list.
    /// <para>
    /// Default value is `No records to display`.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue("No records to display")]
    [Description("Gets or sets the text to display when there are no records in the list.")]
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Gets or sets the group name associated with the list.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the group name associated with the list.")]
    [Parameter]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the CSS selector for the drag handle element.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS selector for the drag handle element.")]
    [Parameter]
    public string? Handle { get; set; }

    /// <summary>
    /// Gets or sets the loading state.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the loading state.")]
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets the template used to render individual items in the list.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the template used to render individual items in the list.")]
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the loading template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the loading template.")]
    [Parameter]
    public RenderFragment LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the <see cref="SortableList{TItem}" /> component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the name of the <b>SortableList{TItem}</b> component.")]
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an event callback that fires when an item is added to the list.
    /// </summary>
    [AddedVersion("2.2.0")]
    [Description("Gets or sets an event callback that fires when an item is added to the list.")]
    [Parameter]
    public EventCallback<SortableListEventArgs> OnAdd { get; set; }

    /// <summary>
    /// Gets or sets an event callback that fires when an item is removed from the list.
    /// </summary>
    [AddedVersion("2.2.0")]
    [Description("Gets or sets an event callback that fires when an item is removed from the list.")]
    [Parameter]
    public EventCallback<SortableListEventArgs> OnRemove { get; set; }

    /// <summary>
    /// Gets or sets an event callback that fires when an item is updated in the list.
    /// </summary>
    [AddedVersion("2.2.0")]
    [Description("Gets or sets an event callback that fires when an item is updated in the list.")]
    [Parameter]
    public EventCallback<SortableListEventArgs> OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets the pull mode for the sortable list.
    /// <para>
    /// Default value is <see cref="SortableListPullMode.True" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(SortableListPullMode.True)]
    [Description("Gets or sets the pull mode for the sortable list.")]
    [Parameter]
    public SortableListPullMode Pull { get; set; } = SortableListPullMode.True;

    /// <summary>
    /// Gets or sets the put mode for the sortable list.
    /// <para>
    /// Default value is <see cref="SortableListPutMode.True" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(SortableListPutMode.True)]
    [Description("Gets or sets the put mode for the sortable list.")]
    [Parameter]
    public SortableListPutMode Put { get; set; } = SortableListPutMode.True;

    /// <summary>
    /// Provides JavaScript interop functionality for the Sortable List.
    /// </summary>
    [Inject]
    private SortableListJsInterop SortableListJsInterop { get; set; } = default!;

    #endregion
}
