namespace BlazorBootstrap;

public partial class SortableList<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CancellationTokenSource cancellationTokenSource = default!;

    private DotNetObjectReference<SortableList<TItem>>? objRef;

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

        QueueAfterRenderAction(async () => await SortableListJsInterop.InitializeAsync(objRef!, ElementId!, Handle!, Group!), new RenderPriority());
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing) Data = null!;

        await base.DisposeAsync(disposing);
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
    /// Gets or sets the items.
    /// </summary>
    [Parameter]
    public List<TItem> Data { get; set; } = default!;

    /// <summary>
    /// Template to render when there are no items to display.
    /// </summary>
    public RenderFragment EmptyDataTemplate { get; set; } = default!;

    /// <summary>
    /// Shows text on no records.
    /// </summary>
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

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
    public RenderFragment LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// Provides JavaScript interop functionality for the Sortable List.
    /// </summary>
    [Inject] private SortableListJsInterop SortableListJsInterop { get; set; } = default!;

    #endregion
}
