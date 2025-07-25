﻿namespace BlazorBootstrap;

public partial class Grid<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    public bool allItemsSelected = false;

    private CancellationTokenSource cancellationTokenSource = default!;

    private Dictionary<int, string> checkboxIds = new();

    private List<GridColumn<TItem>> columns = new();

    private GridDetailView<TItem>? detailView;

    public GridEmptyDataTemplate<TItem>? emptyDataTemplate;

    /// <summary>
    /// Current grid state (filters, paging, sorting).
    /// </summary>
    internal GridState<TItem> gridCurrentState = new(1, null);

    private string headerCheckboxId = default!;

    private RenderFragment? headerSelectionTemplate;

    private bool isColumnVisibilityChanged = false;

    private bool isFirstRenderComplete = false;

    private List<TItem>? items = null;

    private object? lastAssignedDataOrDataProvider;

    public GridLoadingTemplate<TItem>? loadingTemplate;

    private int pageSize;

    private bool requestInProgress = false;

    private HashSet<TItem> selectedItems = new();

    private int? totalCount = null;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RefreshDataAsync(firstRender);
            isFirstRenderComplete = true;
        }

        // As Rendering now complete we can reset the column visibility change to false
        isColumnVisibilityChanged = false;

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        headerCheckboxId = IdUtility.GetNextId();

        pageSize = PageSize;
        selectedItems = SelectedItems!;

        base.OnInitialized();
    }

    protected override Task OnParametersSetAsync()
    {
        if ((Data is null && DataProvider is null) || (Data is not null && DataProvider is not null))
            throw new ArgumentException($"Grid requires either {nameof(Data)} or {nameof(DataProvider)}, but not both or neither.");

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

            //if (!mustRefreshData && selectedItems != SelectedItems)
            //{
            //    mustRefreshData = true;
            //    SelectedItems = selectedItems;
            //}

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

    private string GetColumnSummaryValue(GridSummaryColumnType type, string propertyName, string format, string prefix)
    {
        double value = 0;

        if (type == GridSummaryColumnType.Average)
        {
            prefix ??= "Avg: ";
            value = items?.Average(x => Convert.ToDouble(x.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
        }
        else if (type == GridSummaryColumnType.Count)
        {
            prefix ??= "Count: ";
            value = items?.Where(x => x.GetType().GetProperty(propertyName)?.GetValue(x) is not null).Count() ?? 0;
        }
        else if (type == GridSummaryColumnType.Max)
        {
            prefix ??= "Max: ";
            value = items?.Max(x => Convert.ToDouble(x.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
        }
        else if (type == GridSummaryColumnType.Min)
        {
            prefix ??= "Min: ";
            value = items?.Min(x => Convert.ToDouble(x.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
        }
        else if (type == GridSummaryColumnType.Sum)
        {
            prefix ??= "Total: ";
            value = items?.Sum(x => Convert.ToDouble(x.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
        }

        if (string.IsNullOrWhiteSpace(format))
            return $"{prefix}{value}";
        else
            return $"{prefix}{value.ToString(format, GetCultureInfo())}";
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

    public Task SelectAllItemsAsync() => SelectAllItemsInternalAsync(true);

    public Task UnSelectAllItemsAsync() => SelectAllItemsInternalAsync(false);

    internal void AddColumn(GridColumn<TItem> column) => columns.Add(column);

    internal void ColumnVisibilityUpdated()
    {
        if (!isColumnVisibilityChanged)
        {
            isColumnVisibilityChanged = true;
            StateHasChanged();
        }
    }

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

        // TODO: validate the below two lines - `Save and Load Grid Settings` functionality impacted
        //await InvokeAsync(StateHasChanged); // trigger the state changed to show the loading
        //await Task.Delay(300);

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
            if (result.PageNumber.HasValue)
            {
                gridCurrentState = new GridState<TItem>(result.PageNumber.Value, gridCurrentState.Sorting);
            }
        }
        else
        {
            items = new List<TItem>();
            totalCount = 0;
        }

        if (AllowSelection)
        {
            PrepareCheckboxIds();
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

    internal void SetGridDetailView(GridDetailView<TItem> detailView) => this.detailView = detailView;

    internal void SetGridEmptyDataTemplate(GridEmptyDataTemplate<TItem> emptyDataTemplate) => this.emptyDataTemplate = emptyDataTemplate;

    internal void SetGridLoadingTemplate(GridLoadingTemplate<TItem> loadingTemplate) => this.loadingTemplate = loadingTemplate;

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
                if (c.Id != column.Id)
                    c.currentSortDirection = SortDirection.None;

                // set default sorting
                if (sortedColumn == null && c.IsDefaultSortColumn)
                {
                    if (c.Id == column.Id
                        && c.currentSortDirection == SortDirection.None
                        && c.defaultSortDirection == SortDirection.Descending)
                        c.currentSortDirection = SortDirection.Ascending; // Default Sorting: DESC                
                    else
                        c.currentSortDirection = c.defaultSortDirection != SortDirection.None ? c.defaultSortDirection : SortDirection.Ascending;

                    gridCurrentState = new GridState<TItem>(gridCurrentState.PageIndex, c.GetSorting());
                }
                else if (c.Id == column.Id && c.SortDirection != SortDirection.None) // TODO: this condition is not required. 1 -> ASC, 2 -> DESC, 3 -> None. Here 3 scenario is not working
                {
                    gridCurrentState = new GridState<TItem>(gridCurrentState.PageIndex, c.GetSorting());
                }
            }
        );

        await SaveGridSettingsAsync();
        await RefreshDataAsync(false);
    }

    private async Task CheckOrUnCheckAll() => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.grid.checkOrUnCheckAll", $".bb-grid-form-check-{headerCheckboxId} > input.form-check-input", allItemsSelected);

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
            builder.AddAttribute(105, "class", "form-check-input bb-grid-form-check-input");
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

    private CultureInfo GetCultureInfo()
    {
        if (string.IsNullOrWhiteSpace(Locale))
            return CultureInfo.CurrentCulture;

        try
        {
            return CultureInfo.GetCultureInfo(Locale);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    private IEnumerable<SortingItem<TItem>>? GetDefaultSorting() =>
        !AllowSorting || columns == null || !columns.Any()
            ? null
            : columns?
              .Where(column => column.CanSort() && column.IsDefaultSortColumn)
              ?
              .SelectMany(item => item.GetSorting());

    private string GetGridContainerStyle()
    {
        var styleAttributes = new HashSet<string>();

        if (FixedHeader) styleAttributes.Add($"height:{Height.ToString(CultureInfo.InvariantCulture)}{Unit.ToCssString()}");

        return string.Join(";", styleAttributes);
    }

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
    private bool IsItemSelected(TItem item) => selectedItems?.Contains(item) ?? false;

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
        var headerCheckboxState = bool.TryParse(args?.Value?.ToString(), out var checkboxState) && checkboxState;
        await SelectAllItemsInternalAsync(headerCheckboxState);
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
            allItemsSelected = (selectedItems.Count == (items?.Count ?? 0));

            if (allItemsSelected)
                await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Checked);
            else if (selectedItems.Count == 0)
                await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Unchecked);
            else
                await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Indeterminate);
        }
        else
        {
            selectedItems = isChecked ? new HashSet<TItem> { item } : new HashSet<TItem>();
            allItemsSelected = false;
            await CheckOrUnCheckAll();
            await SetCheckboxStateAsync(id, isChecked ? CheckboxState.Checked : CheckboxState.Unchecked);
        }

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
        else
            SelectedItems = selectedItems;
    }

    private async Task OnScroll(EventArgs e)
    {
        await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.grid.scroll", Id);
    }

    private void PrepareCheckboxIds()
    {
        checkboxIds ??= new Dictionary<int, string>();
        var currentLength = checkboxIds.Count;
        var itemsCount = items?.Count ?? 0;

        if (currentLength < itemsCount)
            for (var i = currentLength; i < itemsCount; i++)
                checkboxIds[i] = IdUtility.GetNextId();
    }

    /// <summary>
    /// Refresh selection
    /// </summary>
    private async Task RefreshSelectionAsync()
    {
        selectedItems = (items?.Count ?? 0) == 0
                            ? new HashSet<TItem>()
                            : selectedItems?.Intersect(items!).ToHashSet() ?? new HashSet<TItem>();

        allItemsSelected = selectedItems.Count > 0 && items!.Count == selectedItems.Count;

        if (allItemsSelected)
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Checked);
        else if (selectedItems.Count > 0)
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Indeterminate);
        else
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Unchecked);

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
        else
            SelectedItems = selectedItems;
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

    private async Task SelectAllItemsInternalAsync(bool selectAll)
    {
        if (SelectionMode != GridSelectionMode.Multiple)
            return;

        allItemsSelected = selectAll;
        selectedItems = allItemsSelected ? new HashSet<TItem>(items!) : new HashSet<TItem>();

        if (allItemsSelected)
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Checked);
        else
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Unchecked);

        await CheckOrUnCheckAll();

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
        else
            SelectedItems = selectedItems;
    }

    private Task SetCheckboxStateAsync(string id, CheckboxState checkboxState)
    {
        queuedTasks.Enqueue(async () => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.grid.setSelectAllCheckboxState", id, (int)checkboxState));

        return Task.CompletedTask;
    }

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
                var allowedFilterOperators = FilterOperatorUtility.GetFilterOperators(column.GetPropertyTypeName());

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

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            ("bb-table", true),
            (BootstrapClass.TableSticky, FixedHeader)
        );

    /// <summary>
    /// Gets or sets the grid delete.
    /// </summary>
    //[Parameter] public int AllowDelete { get; set; }

    /// <summary>
    /// Gets or sets the grid edit.
    /// </summary>
    //[Parameter] public int AllowEdit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the grid detail view is enabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AllowDetailView { get; set; }

    /// <summary>
    /// Gets or sets the grid filtering.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets the grid paging.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AllowPaging { get; set; }

    /// <summary>
    /// Gets or sets the allow row click.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AllowRowClick { get; set; }

    /// <summary>
    /// Gets or sets the grid selection.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AllowSelection { get; set; }

    /// <summary>
    /// Gets or sets the grid sorting.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AllowSorting { get; set; }

    /// <summary>
    /// Gets or sets the grid summary.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public bool AllowSummary { get; set; }

    /// <summary>
    /// Automatically hides the paging controls when the grid item count is less than or equal to the <see cref="PageSize" />
    /// and this property is set to `true`.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AutoHidePaging { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the grid data.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public IEnumerable<TItem> Data { get; set; } = default!;

    /// <summary>
    /// DataProvider is for items to render.
    /// The provider should always return an instance of 'GridDataProviderResult', and 'null' is not allowed.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
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
    /// Gets or sets the empty text.
    /// Shows text on no records.
    /// </summary>
    /// <remarks>
    /// Default value is 'No records to display'.
    /// </remarks>
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Gets or sets the enum filter select text.
    /// </summary>
    /// <remarks>
    /// Default value is 'Select'.
    /// </remarks>
    [Parameter]
    public string? EnumFilterSelectText { get; set; } = "Select";

    /// <summary>
    /// Gets or sets the filters row css class.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string FiltersRowCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the filters translation provider.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public GridFiltersTranslationDelegate FiltersTranslationProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the grid fixed header.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool FixedHeader { get; set; }

    /// <summary>
    /// Gets or sets the grid container css class.
    /// </summary>
    [Parameter]
    public string? GridContainerClass { get; set; }

    private string? GridContainerClassNames =>
        BuildClassNames(
            GridContainerClass,
            (BootstrapClass.TableResponsive, Responsive)
        );

    /// <summary>
    /// Gets or sets the grid container css style.
    /// </summary>
    [Parameter]
    public string? GridContainerStyle { get; set; }

    private string? GridContainerStyleNames =>
        BuildStyleNames(
            GridContainerStyle,
            ($"height:{Height.ToString(CultureInfo.InvariantCulture)}{Unit.ToCssString()}", FixedHeader),
            ($"overflow-x:auto;", Responsive)
        );

    /// <summary>
    /// This event is fired when the grid state is changed.
    /// </summary>
    [Parameter]
    public EventCallback<GridSettings> GridSettingsChanged { get; set; }

    /// <summary>
    /// Gets or sets the header row css class but not the thead tag class.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
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
    /// <remarks>
    /// Default value is 320 <see cref="Unit.Px" />.
    /// </remarks>
    [Parameter]
    public float Height { get; set; } = 320;

    /// <summary>
    /// Gets or sets the items per page text.
    /// </summary>
    /// <remarks>
    /// Default value is 'Items per page'.
    /// </remarks>
    [Parameter]
    //[EditorRequired] 
    public string ItemsPerPageText { get; set; } = "Items per page"!;

    /// <summary>
    /// Gets or sets the locale.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public string? Locale { get; set; }

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
    /// Gets or sets the page size.
    /// </summary>
    /// <remarks>
    /// Default value is 10.
    /// </remarks>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the page size selector items.
    /// </summary>
    /// <remarks>
    /// Default value is '{ 10, 20, 50 }'.
    /// </remarks>
    [Parameter]
    //[EditorRequired]
    public int[] PageSizeSelectorItems { get; set; } = { 10, 20, 50 };

    /// <summary>
    /// Gets or sets the page size selector visible.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool PageSizeSelectorVisible { get; set; }

    [Obsolete("PaginationAlignment parameter is not supported from 1.8.0 version onwards")]
    /// <summary>
    /// Gets or sets the pagination alignment.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.Start" />.
    /// </remarks>
    [Parameter]
    public Alignment PaginationAlignment { get; set; } = Alignment.Start;

    private string paginationItemsText => GetPaginationItemsText();

    /// <summary>
    /// Gets or sets the pagination items text format.
    /// </summary>
    /// <remarks>
    /// Default value is '{0} - {1} of {2} items'.
    /// </remarks>
    [Parameter]
    //[EditorRequired]
    public string PaginationItemsTextFormat { get; set; } = "{0} - {1} of {2} items"!;

    /// <summary>
    /// Gets or sets a value indicating whether the grid is responsive.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Responsive { get; set; }

    /// <summary>
    /// Gets or sets the row class.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? RowClass { get; set; }

    /// <summary>
    /// Gets or sets the function used to extract a unique key from a row item.
    /// </summary>
    /// <remarks>
    /// The key returned by the function is used to uniquely identify each row in the data set.  This
    /// is typically required for operations such as tracking changes or rendering rows efficiently. 
    /// If not set, the item hash code will be used as the key.
    /// Example usage: `RowKeySelector="(employee) => employee.Id"`.
    /// </remarks>
    [Parameter]
    public Func<TItem, object>? RowKeySelector { get; set; }
    
    /// <summary>
    /// Gets or sets the selected items.
    /// </summary>
    [Parameter]
    public HashSet<TItem>? SelectedItems { get; set; }

    /// <summary>
    /// This event is fired when the item selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the grid selection mode.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="GridSelectionMode.Single" />.
    /// </remarks>
    [Parameter]
    public GridSelectionMode SelectionMode { get; set; } = GridSelectionMode.Single;

    /// <summary>
    /// Settings is for grid to render.
    /// The provider should always return an instance of 'GridSettings', and 'null' is not allowed.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public GridSettingsProviderDelegate SettingsProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the thead css class.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? THeadCssClass { get; set; }

    private int totalPages => GetTotalPagesCount();

    /// <summary>
    /// Gets or sets the units.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Px" />.
    /// </remarks>
    [Parameter]
    public Unit Unit { get; set; } = Unit.Px;

    #endregion
}
