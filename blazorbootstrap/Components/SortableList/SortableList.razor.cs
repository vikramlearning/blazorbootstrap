namespace BlazorBootstrap;

/// <summary>
/// Represents a sortable list component.
/// </summary>
/// <typeparam name="TItem">The type of items in the list.</typeparam>
public partial class SortableList<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// A CSS selector used to filter disabled items.
    /// </summary>
    private readonly string filter = ".bb-sortable-list-item-disabled";

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

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await SortableListJsInterop.InitializeAsync(Id!, Name!, Handle!, Group!, AllowSorting, Pull.ToSortableListPullMode(), Put.ToSortableListPutMode(), filter, objRef!);

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    [JSInvokable("onAddJs")]
    public async Task OnAddJS(int oldIndex, int newIndex)
    {
        if (OnAdd.HasDelegate)
            await OnAdd.InvokeAsync(new SortableListEventArgs(oldIndex, newIndex));
    }

    [JSInvokable("onRemoveJS")]
    public async Task OnRemoveJS(int oldIndex, int newIndex, string fromListName, string toListName)
    {
        if (OnRemove.HasDelegate)
            await OnRemove.InvokeAsync(new SortableListEventArgs(oldIndex, newIndex, fromListName, toListName));
    }

    [JSInvokable("onUpdateJS")]
    public async Task OnUpdateJS(int oldIndex, int newIndex)
    {
        if (OnUpdate.HasDelegate)
            await OnUpdate.InvokeAsync(new SortableListEventArgs(oldIndex, newIndex));
    }


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case var _ when String.Equals(parameter.Name, nameof(AllowSorting), StringComparison.OrdinalIgnoreCase): AllowSorting = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Data), StringComparison.OrdinalIgnoreCase): Data = (IReadOnlyCollection<TItem>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(DisabledItemCssClass), StringComparison.OrdinalIgnoreCase): DisabledItemCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(DisableItem), StringComparison.OrdinalIgnoreCase): DisableItem = (Func<TItem, bool>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(EmptyDataTemplate), StringComparison.OrdinalIgnoreCase): EmptyDataTemplate = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(EmptyText), StringComparison.OrdinalIgnoreCase): EmptyText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Group), StringComparison.OrdinalIgnoreCase): Group = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Handle), StringComparison.OrdinalIgnoreCase): Handle = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsLoading), StringComparison.OrdinalIgnoreCase): IsLoading = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ItemTemplate), StringComparison.OrdinalIgnoreCase): ItemTemplate = (RenderFragment<TItem>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LoadingTemplate), StringComparison.OrdinalIgnoreCase): LoadingTemplate = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Name), StringComparison.OrdinalIgnoreCase): Name = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnAdd), StringComparison.OrdinalIgnoreCase): OnAdd = (EventCallback<SortableListEventArgs>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnRemove), StringComparison.OrdinalIgnoreCase): OnRemove = (EventCallback<SortableListEventArgs>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnUpdate), StringComparison.OrdinalIgnoreCase): OnUpdate = (EventCallback<SortableListEventArgs>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Pull), StringComparison.OrdinalIgnoreCase): Pull = (SortableListPullMode)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Put), StringComparison.OrdinalIgnoreCase): Put = (SortableListPutMode)parameter.Value; break;
                
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets a value indicating whether sorting is allowed for the list.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter]
    public bool AllowSorting { get; set; } = true;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public IReadOnlyCollection<TItem> Data { get; set; } = default!;

    /// <summary>
    /// Gets or sets the CSS class applied to disabled items.
    /// </summary>
    /// <remarks>
    /// Default value is  <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? DisabledItemCssClass { get; set; } 

    /// <summary>
    /// Gets or sets a delegate that determines whether an item should be disabled.
    /// </summary>
    [Parameter]
    public Func<TItem, bool> DisableItem { get; set; } = default!;

    /// <summary>
    /// Gets or sets the empty data template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? EmptyDataTemplate { get; set; }  

    /// <summary>
    /// Gets or sets the text to display when there are no records in the list.
    /// </summary>
    /// <remarks>
    /// Default value is `No records to display`.
    /// </remarks>
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Gets or sets the group name associated with the list.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the CSS selector for the drag handle element.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Handle { get; set; }

    /// <summary>
    /// Gets or sets the loading state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets the template used to render individual items in the list.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the loading template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }  

    /// <summary>
    /// Gets or sets the name of the <see cref="SortableList{TItem}" /> component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
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
    /// <remarks>
    /// Default value is <see cref="SortableListPullMode.True" />.
    /// </remarks>
    [Parameter]
    public SortableListPullMode Pull { get; set; } = SortableListPullMode.True;

    /// <summary>
    /// Gets or sets the put mode for the sortable list.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="SortableListPutMode.True" />.
    /// </remarks>
    [Parameter]
    public SortableListPutMode Put { get; set; } = SortableListPutMode.True;

    /// <summary>
    /// Provides JavaScript interop functionality for the Sortable List.
    /// </summary>
    [Inject]
    private SortableListJsInterop SortableListJsInterop { get; set; } = default!;

    #endregion
}
