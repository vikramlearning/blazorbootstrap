namespace BlazorBootstrap;

public partial class SortableList<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CancellationTokenSource cancellationTokenSource = default!;

    private List<TItem>? items = new();

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

        QueueAfterRenderAction(async () => await SortableListJsInterop.InitializeAsync(objRef!, ElementId!), new RenderPriority());
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

    [Parameter]
    public List<TItem> Data { get; set; } = default!;

    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Provides JavaScript interop functionality for the Sortable List.
    /// </summary>
    [Inject] private SortableListJsInterop SortableListJsInterop { get; set; } = default!;

    #endregion
}
