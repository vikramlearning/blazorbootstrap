namespace BlazorBootstrap;

public partial class Grid<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    public bool allItemsSelected = false;

    private CancellationTokenSource cancellationTokenSource = default!;

    private Dictionary<int, string> checkboxIds = new();

    private List<GridColumn<TItem>> columns = new();

    /// <summary>
    /// Current grid state (filters, paging, sorting).
    /// </summary>
    internal GridState<TItem> gridCurrentState = new(1, null);

    private string headerCheckboxId = default!;

    private RenderFragment? headerSelectionTemplate;

    private bool isFirstRenderComplete = false;

    private List<TItem>? items = null;

    private object? lastAssignedDataOrDataProvider;

    private int pageSize;

    private bool requestInProgress = false;

    private HashSet<TItem> selectedItems = new();

    public int SelectedItemsCount = 0;

    private int? totalCount = null;

    #endregion

    #region Methods

   protected override void BuildClasses()
    {
        this.AddClass("bb-table");
        this.AddClass(BootstrapClassProvider.TableSticky, FixedHeader);

        base.BuildClasses();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RefreshDataAsync(firstRender);
            isFirstRenderComplete = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        headerCheckboxId = IdGenerator.GetNextId();

        pageSize = PageSize;

        base.OnInitialized();
    }

    protected override Task OnParametersSetAsync()
    {
        if ((Data is null && DataProvider is null) || (Data is not null && DataProvider is not null)) throw new ArgumentException($"Grid requires either {nameof(Data)} or {nameof(DataProvider)}, but not both or neither.");

        if (AllowPaging && PageSize < 0)
            throw new ArgumentException($"{nameof(PageSize)} must be greater than zero.");

        if (isFirstRenderComplete)
        {
            // Perform a re-query only if the data source or something else has changed
            var newDataOrDataProvider = Data; //?? (object?)DataProvider;
            var dataSourceHasChanged = newDataOrDataProvider != lastAssignedDataOrDataProvider;
            if (dataSourceHasChanged) lastAssignedDataOrDataProvider = newDataOrDataProvider;

            var mustRefreshData = dataSourceHasChanged && !GridSettingsChanged.HasDelegate;

            // page size changed
            if (!mustRefreshData && pageSize != PageSize)
            {
                mustRefreshData = true;
                pageSize = PageSize;
                _ = ResetPageNumberAsync();
                SaveGridSettingsAsync();
            }

            // We want to trigger the first data load when we've collected the initial set of columns
            // because they might perform some action, like setting the default sort order. 
            // It would be wasteful to have to re-query immediately.
            return columns.Count > 0 && mustRefreshData ? RefreshDataAsync(false) : Task.CompletedTask;
        }

        return base.OnParametersSetAsync();
    }

    /// <summary>
    /// Get filters.
    /// </summary>
    /// <returns>IEnumerable</returns>
    public IEnumerable<FilterItem>? GetFilters() =>
        !AllowFiltering || columns == null || !columns.Any()
            ? null
            : columns
              .Where(column => column.Filterable && column.GetFilterOperator() != FilterOperator.None && !string.IsNullOrWhiteSpace(column.GetFilterValue()))
              ?.Select(column => new FilterItem(column.PropertyName, column.GetFilterValue(), column.GetFilterOperator(), column.StringComparison));

    private string GetGridParentStyle()
    {
        var styleAttributes = new HashSet<string>();

        if (FixedHeader)
        {
            styleAttributes.Add($"height:{Height.ToString(CultureInfo.InvariantCulture)}{Unit.ToCssString()}");
        }

        return string.Join(";", styleAttributes);
    }

    /// <summary>
    /// Refresh the grid data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync(CancellationToken cancellationToken = default) => await RefreshDataAsync(false, cancellationToken);

    /// <summary>
    /// Reset the page number to 1 and refresh the grid.
    /// </summary>
    public async ValueTask ResetPageNumber() => await ResetPageNumberAsync(true);

    internal void AddColumn(GridColumn<TItem> column) => columns.Add(column);

    internal async Task FilterChangedAsync()
    {
        if (cancellationTokenSource is not null
            && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        cancellationTokenSource = new CancellationTokenSource();

        var token = cancellationTokenSource.Token;
        await Task.Delay(300, token); // 300ms timeout for the debouncing

        await SaveGridSettingsAsync();
        await RefreshDataAsync(false, token);
    }

    internal async Task RefreshDataAsync(bool firstRender = false, CancellationToken cancellationToken = default)
    {
        if (requestInProgress)
            return;

        requestInProgress = true;

        if (firstRender)
            await LoadGridSettingsAsync();

        var request = new GridDataProviderRequest<TItem>
                      {
                          PageNumber = AllowPaging ? gridCurrentState.PageIndex : 0,
                          PageSize = AllowPaging ? pageSize : 0,
                          Sorting = AllowSorting ? gridCurrentState.Sorting ?? GetDefaultSorting()! : null!,
                          Filters = AllowFiltering ? GetFilters()! : null!,
                          CancellationToken = cancellationToken
                      };

        GridDataProviderResult<TItem> result = default!;

        if (DataProvider is not null)
            result = await DataProvider.Invoke(request);
        else if (Data is not null)
            result = request.ApplyTo(Data);

        if (result is not null)
        {
            items = result.Data!.ToList();
            totalCount = result.TotalCount ?? result.Data!.Count();
        }
        else
        {
            items = new List<TItem>();
            totalCount = 0;
        }

        if (AllowSelection)
        {
            PrepareCheckboxIds();

            if (!firstRender)
                await RefreshSelectionAsync();
        }

        requestInProgress = false;

        await InvokeAsync(StateHasChanged);
    }

    internal async ValueTask ResetPageNumberAsync(bool refreshGrid = false)
    {
        gridCurrentState = new GridState<TItem>(1, gridCurrentState.Sorting);

        if (refreshGrid)
            await RefreshDataAsync(false);
    }

    internal async Task SortingChangedAsync(GridColumn<TItem> column)
    {
        if (columns == null || !columns.Any())
            return;

        // check sorting enabled on any of the columns
        var sortedColumn = columns.FirstOrDefault(c => c.currentSortDirection != SortDirection.None);

        // reset other columns sorting
        columns.ForEach(
            c =>
            {
                if (c.ElementId != column.ElementId)
                    c.currentSortDirection = SortDirection.None;

                // set default sorting
                if (sortedColumn == null && c.IsDefaultSortColumn)
                {
                    if (c.ElementId == column.ElementId
                        && c.currentSortDirection == SortDirection.None
                        && c.defaultSortDirection == SortDirection.Descending)
                        c.currentSortDirection = SortDirection.Ascending; // Default Sorting: DESC                
                    else
                        c.currentSortDirection = c.defaultSortDirection != SortDirection.None ? c.defaultSortDirection : SortDirection.Ascending;

                    gridCurrentState = new GridState<TItem>(gridCurrentState.PageIndex, c.GetSorting());
                }
                else if (c.ElementId == column.ElementId && c.SortDirection != SortDirection.None) // TODO: this condition is not required. 1 -> ASC, 2 -> DESC, 3 -> None. Here 3 scenario is not working
                {
                    gridCurrentState = new GridState<TItem>(gridCurrentState.PageIndex, c.GetSorting());
                }
            }
        );

        await SaveGridSettingsAsync();
        await RefreshDataAsync(false);
    }

    private async Task CheckOrUnCheckAll() => await JS.InvokeVoidAsync("window.blazorBootstrap.grid.checkOrUnCheckAll", $".bb-grid-form-check-{headerCheckboxId} > input.form-check-input", allItemsSelected);

    /// <summary>
    /// Child selection template.
    /// </summary>
    private RenderFragment ChildSelectionTemplate(int rowIndex, TItem rowData) =>
        builder =>
        {
            // td > div "class" > input
            builder.OpenElement(101, "td");

            builder.OpenElement(102, "div");
            builder.AddAttribute(103, "class", $"form-check bb-grid-form-check-{headerCheckboxId}");

            builder.OpenElement(104, "input");
            builder.AddAttribute(105, "class", "form-check-input");
            builder.AddAttribute(106, "type", "checkbox");
            builder.AddAttribute(107, "role", "button");

            if (IsItemSelected(rowData)) builder.AddAttribute(108, "checked", "checked");

            // disable the checkbox
            // remove the onchange event binding
            // add disabled attribute
            if (DisableRowSelection?.Invoke(rowData) ?? false)
            {
                builder.AddAttribute(109, "disabled", "disabled");
            }
            else
            {
                var id = checkboxIds[rowIndex];
                builder.AddAttribute(110, "id", id);
                builder.AddAttribute(111, "onchange", async (ChangeEventArgs args) => await OnRowCheckboxChanged(id, rowData, args));
                builder.AddEventStopPropagationAttribute(111, "onclick", true);
            }

            builder.CloseElement(); // close: input
            builder.CloseElement(); // close: div
            builder.CloseElement(); // close: th
        };

    private IEnumerable<SortingItem<TItem>>? GetDefaultSorting() =>
        !AllowSorting || columns == null || !columns.Any()
            ? null
            : columns?
              .Where(column => column.CanSort() && column.IsDefaultSortColumn)?
              .SelectMany(item => item.GetSorting());

    private string GetPaginationItemsText()
    {
        var startRecord = (gridCurrentState.PageIndex - 1) * pageSize + 1;
        var endRecord = gridCurrentState.PageIndex * pageSize;

        if (endRecord > totalCount)
            endRecord = totalCount ?? 0;

        return string.Format(PaginationItemsTextFormat, startRecord, endRecord, totalCount);
    }

    private int GetTotalPagesCount()
    {
        if (totalCount.HasValue && totalCount.Value > 0)
        {
            var q = totalCount.Value / pageSize;
            var r = totalCount.Value % pageSize;

            return q < 1 ? 1 : q + (r > 0 ? 1 : 0);
        }

        return 1;
    }

    private async Task<IEnumerable<FilterOperatorInfo>?> GridFiltersTranslationProviderAsync()
    {
        if (FiltersTranslationProvider is null)
            return null;

        var filters = await FiltersTranslationProvider.Invoke();

        if (filters is null || !filters.Any())
            return null;

        return filters;
    }

    /// <summary>
    /// Determines whether the item is already selected.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>bool</returns>
    private bool IsItemSelected(TItem item) => selectedItems.Contains(item);

    private async Task LoadGridSettingsAsync()
    {
        if (SettingsProvider is null)
            return;

        var settings = await SettingsProvider.Invoke();

        if (settings is null)
            return;

        if (settings.Filters is not null && settings.Filters.Any())
            SetFilters(settings.Filters);

        if (settings.PageNumber > 0)
        {
            if (settings.PageSize > 0 && settings.PageNumber < settings.PageSize)
            {
                gridCurrentState = new GridState<TItem>(settings.PageNumber, gridCurrentState.Sorting);
                pageSize = settings.PageSize;
            }
            else
            {
                gridCurrentState = new GridState<TItem>(1, null);
                pageSize = 10;
            }
        }
        else
        {
            gridCurrentState = new GridState<TItem>(1, null);
            pageSize = 10;
        }
    }

    private async Task OnHeaderCheckboxChanged(ChangeEventArgs args)
    {
        allItemsSelected = bool.TryParse(args?.Value?.ToString(), out var checkboxState) && checkboxState;
        selectedItems = allItemsSelected ? new HashSet<TItem>(items!) : new HashSet<TItem>();
        SelectedItemsCount = selectedItems.Count;
        await CheckOrUnCheckAll();

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        gridCurrentState = new GridState<TItem>(newPageNumber, gridCurrentState.Sorting);
        await SaveGridSettingsAsync();
        await RefreshDataAsync(false);
    }

    private async Task OnPageSizeChangedAsync(int newPageSize)
    {
        pageSize = PageSize = newPageSize;
        await ResetPageNumberAsync();
        await SaveGridSettingsAsync();
        await RefreshDataAsync(false);
    }

    private async Task OnRowCheckboxChanged(string id, TItem item, ChangeEventArgs args)
    {
        bool.TryParse(args?.Value?.ToString(), out var isChecked);

        if (SelectionMode == GridSelectionMode.Multiple)
        {
            _ = isChecked ? selectedItems.Add(item) : selectedItems.Remove(item);
            SelectedItemsCount = selectedItems.Count;
            allItemsSelected = SelectedItemsCount == (items?.Count ?? 0);

            if (allItemsSelected)
                await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Checked);
            else if (SelectedItemsCount == 0)
                await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Unchecked);
            else
                await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Indeterminate);
        }
        else
        {
            selectedItems = isChecked ? new HashSet<TItem> { item } : new HashSet<TItem>();
            SelectedItemsCount = selectedItems.Count;
            allItemsSelected = false;
            await CheckOrUnCheckAll();
            await SetCheckboxStateAsync(id, isChecked ? CheckboxState.Checked : CheckboxState.Unchecked);
        }

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private async Task OnScroll(EventArgs e)
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.grid.scroll", ElementId);
    }

    private void PrepareCheckboxIds()
    {
        checkboxIds ??= new Dictionary<int, string>();
        var currentLength = checkboxIds.Count;
        var itemsCount = (items?.Count ?? 0);

        if (currentLength < itemsCount)
            for (var i = currentLength; i < itemsCount; i++)
                checkboxIds[i] = IdGenerator.GetNextId();
    }

    /// <summary>
    /// Refresh selection
    /// </summary>
    private async Task RefreshSelectionAsync()
    {
        selectedItems = (items?.Count ?? 0) == 0
                            ? new HashSet<TItem>()
                            : selectedItems?.Intersect(items!).ToHashSet() ?? new HashSet<TItem>();

        SelectedItemsCount = selectedItems.Count;
        allItemsSelected = SelectedItemsCount > 0 && items!.Count == SelectedItemsCount;

        if (allItemsSelected)
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Checked);
        else if (SelectedItemsCount > 0)
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Indeterminate);
        else
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Unchecked);

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private async Task RowClick(TItem item, EventArgs args)
    {
        if (AllowRowClick && OnRowClick.HasDelegate)
            await OnRowClick.InvokeAsync(new GridRowEventArgs<TItem>(item));
    }

    private async Task RowDoubleClick(TItem item, EventArgs args)
    {
        if (AllowRowClick && OnRowDoubleClick.HasDelegate)
            await OnRowDoubleClick.InvokeAsync(new GridRowEventArgs<TItem>(item));
    }

    private Task SaveGridSettingsAsync()
    {
        if (!GridSettingsChanged.HasDelegate)
            return Task.CompletedTask;

        var settings = new GridSettings { PageNumber = AllowPaging ? gridCurrentState.PageIndex : 0, PageSize = AllowPaging ? pageSize : 0, Filters = AllowFiltering ? GetFilters() : null };

        return GridSettingsChanged.InvokeAsync(settings);
    }

    private async Task SetCheckboxStateAsync(string id, CheckboxState checkboxState) => await JS.InvokeVoidAsync("window.blazorBootstrap.grid.setSelectAllCheckboxState", id, (int)checkboxState);

    /// <summary>
    /// Set filters.
    /// </summary>
    /// <param name="filterItems"></param>
    private void SetFilters(IEnumerable<FilterItem> filterItems)
    {
        if (filterItems is null || !filterItems.Any())
            return;

        foreach (var item in filterItems)
        {
            var column = columns.Where(x => x.PropertyName == item.PropertyName).FirstOrDefault();

            if (column != null)
            {
                var allowedFilterOperators = FilterOperatorHelper.GetFilterOperators(column.GetPropertyTypeName());

                if (allowedFilterOperators != null && allowedFilterOperators.Any(x => x.FilterOperator == item.Operator))
                {
                    column.SetFilterOperator(item.Operator);
                    column.SetFilterValue(item.Value);
                }
            }
        }
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the grid delete.
    /// </summary>
    //[Parameter] public int AllowDelete { get; set; }

    /// <summary>
    /// Gets or sets the grid edit.
    /// </summary>
    //[Parameter] public int AllowEdit { get; set; }

    /// <summary>
    /// Gets or sets the grid filtering.
    /// </summary>
    [Parameter]
    public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets the grid paging.
    /// </summary>
    [Parameter]
    public bool AllowPaging { get; set; }

    /// <summary>
    /// Gets or sets the allow row click.
    /// </summary>
    [Parameter]
    public bool AllowRowClick { get; set; }

    /// <summary>
    /// Gets or sets the grid selection.
    /// </summary>
    [Parameter]
    public bool AllowSelection { get; set; }

    /// <summary>
    /// Gets or sets the grid sorting.
    /// </summary>
    [Parameter]
    public bool AllowSorting { get; set; }

    /// <summary>
    /// Automatically hides the paging controls when the grid item count is less than or equal to the <see cref="PageSize" /> and this property is set to `true`.
    /// </summary>
    [Parameter]
    public bool AutoHidePaging { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside the grid.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the grid data.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> Data { get; set; } = default!;

    /// <summary>
    /// DataProvider is for items to render.
    /// The provider should always return an instance of 'GridDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter]
    public GridDataProviderDelegate<TItem> DataProvider { get; set; } = default!;

    /// <summary>
    /// Enable or disable the header checkbox selection.
    /// </summary>
    [Parameter]
    public Func<IEnumerable<TItem>, bool>? DisableAllRowsSelection { get; set; }

    /// <summary>
    /// Enable or disable the row level checkbox selection.
    /// </summary>
    [Parameter]
    public Func<TItem, bool>? DisableRowSelection { get; set; }

    /// <summary>
    /// Template to render when there are no rows to display.
    /// </summary>
    public RenderFragment EmptyDataTemplate { get; set; } = default!;

    /// <summary>
    /// Shows text on no records.
    /// </summary>
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Gets or sets the filters row css class.
    /// </summary>
    [Parameter]
    public string FiltersRowCssClass { get; set; } = default!;

    /// <summary>
    /// Filters transalation is for grid filters to render.
    /// The provider should always return a 'FilterOperatorInfo' collection, and 'null' is not allowed.
    /// </summary>
    [Parameter]
    public GridFiltersTranslationDelegate FiltersTranslationProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the grid fixed header.
    /// </summary>
    [Parameter]
    public bool FixedHeader { get; set; }

    private string gridParentStyle => GetGridParentStyle();

    /// <summary>
    /// This event is fired when the grid state is changed.
    /// </summary>
    [Parameter]
    public EventCallback<GridSettings> GridSettingsChanged { get; set; }

    /// <summary>
    /// Gets or sets the header row css class but not the thead tag class.
    /// </summary>
    [Parameter]
    public string HeaderRowCssClass { get; set; } = default!;

    /// <summary>
    /// Header selection template.
    /// </summary>
    private RenderFragment HeaderSelectionTemplate =>
        headerSelectionTemplate ??= builder =>
                                    {
                                        // th "style" > div "class" > input
                                        builder.OpenElement(101, "th");
                                        builder.AddAttribute(102, "style", "width: 2rem;");

                                        if (SelectionMode == GridSelectionMode.Multiple)
                                        {
                                            builder.OpenElement(103, "div");
                                            builder.AddAttribute(104, "class", "form-check");

                                            builder.OpenElement(105, "input");
                                            builder.AddAttribute(106, "class", "form-check-input");
                                            builder.AddAttribute(107, "type", "checkbox");
                                            builder.AddAttribute(108, "role", "button");

                                            // disable the checkbox
                                            // remove the onchange event binding
                                            // add disabled attribute
                                            if (DisableAllRowsSelection?.Invoke(items!) ?? false)
                                            {
                                                builder.AddAttribute(109, "disabled", "disabled");
                                            }
                                            else
                                            {
                                                builder.AddAttribute(110, "id", headerCheckboxId);
                                                builder.AddAttribute(111, "onchange", async (ChangeEventArgs args) => await OnHeaderCheckboxChanged(args));
                                                builder.AddEventStopPropagationAttribute(112, "onclick", true);
                                            }

                                            builder.CloseElement(); // close: input
                                            builder.CloseElement(); // close: div
                                        }

                                        builder.CloseElement(); // close: th
                                    };

   /// <summary>
    /// Gets or sets the grid height.
    /// </summary>
    [Parameter]
    public float Height { get; set; } = 320;

    [Parameter] 
    //[EditorRequired] 
    public string ItemsPerPageText { get; set; } = "Items per page"!;

    /// <summary>
    /// This event is triggered when the user clicks on the row.
    /// Set AllowRowClick to true to enable row clicking.
    /// </summary>
    [Parameter]
    public EventCallback<GridRowEventArgs<TItem>> OnRowClick { get; set; }

    /// <summary>
    /// This event is triggered when the user double clicks on the row.
    /// Set AllowRowClick to true to enable row double clicking.
    /// </summary>
    [Parameter]
    public EventCallback<GridRowEventArgs<TItem>> OnRowDoubleClick { get; set; }

    /// <summary>
    /// Gets or sets the page size of the grid.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the page size selector items.
    /// </summary>
    [Parameter]
    //[EditorRequired]
    public int[] PageSizeSelectorItems { get; set; } = { 10, 20, 50 };

    /// <summary>
    /// Gets or sets the page size selector visible.
    /// </summary>
    [Parameter]
    public bool PageSizeSelectorVisible { get; set; }

    [Obsolete("PaginationAlignment parameter is not supported from 1.8.0 version onwards")]
    /// <summary>
    /// Gets or sets the pagination alignment.
    /// </summary>
    [Parameter]
    public Alignment PaginationAlignment { get; set; } = Alignment.Start;

    private string paginationItemsText => GetPaginationItemsText();

    /// <summary>
    /// Gets or sets the pagination items text format.
    /// </summary>
    [Parameter]
    //[EditorRequired]
    public string PaginationItemsTextFormat { get; set; } = "{0} - {1} of {2} items"!;

    /// <summary>
    /// Gets or sets a value indicating whether Grid is responsive.
    /// </summary>
    [Parameter]
    public bool Responsive { get; set; }

    private string responsiveCssClass => Responsive ? "table-responsive" : "";

    /// <summary>
    /// Gets or sets the row class.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? RowClass { get; set; }

    /// <summary>
    /// This event is fired when the item selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the grid selection mode.
    /// </summary>
    [Parameter]
    public GridSelectionMode SelectionMode { get; set; } = GridSelectionMode.Single;

    /// <summary>
    /// Settings is for grid to render.
    /// The provider should always return an instance of 'GridSettings', and 'null' is not allowed.
    /// </summary>
    [Parameter]
    public GridSettingsProviderDelegate SettingsProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the thead css class.
    /// </summary>
    [Parameter]
    public string? THeadCssClass { get; set; }

    /// <summary>
    /// Gets or sets the units.
    /// </summary>
    [Parameter]
    public Unit Unit { get; set; } = Unit.Px;

    private int totalPages => GetTotalPagesCount();

    #endregion
}
