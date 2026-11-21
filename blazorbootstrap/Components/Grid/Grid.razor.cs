namespace BlazorBootstrap;

public partial class Grid<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    public bool allItemsSelected = false;

    private CancellationTokenSource cancellationTokenSource = default!;

    private Dictionary<int, string> checkboxIds = new();

    /// <summary>
    /// Column keys in the user's display order; <see langword="null" /> keeps the declared order.
    /// </summary>
    private List<string>? columnOrder;

    private string columnChooserSearch = string.Empty;

    private string columnChooserStatus = string.Empty;

    private List<GridColumn<TItem>> columns = new();

    private GridDetailView<TItem>? detailView;

    private GridColumn<TItem>? draggedColumn;

    private string exportStatus = string.Empty;

    public GridEmptyDataTemplate<TItem>? emptyDataTemplate;

    /// <summary>
    /// Current grid state (filters, paging, sorting).
    /// </summary>
    internal GridState<TItem> gridCurrentState = new(1, null);

    private string headerCheckboxId = default!;

    private RenderFragment? headerSelectionTemplate;

    /// <summary>
    /// Keys of the columns the user has hidden. Separate from <see cref="GridColumn{TItem}.IsVisible" />, which the app owns.
    /// </summary>
    private HashSet<string> hiddenColumns = new(StringComparer.Ordinal);

    private bool isColumnVisibilityChanged = false;

    private bool isFirstRenderComplete = false;

    private List<TItem>? items = null;

    private object? lastAssignedDataOrDataProvider;

    public GridLoadingTemplate<TItem>? loadingTemplate;

    private int pageSize;

    private bool requestInProgress = false;

    private HashSet<TItem> selectedItems = new();

    private int? totalCount = null;

    private bool pendingPageSizeChanging = false;

    private bool isColumnsLoading = false;

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

        isColumnsLoading = true;

        base.OnInitialized();
    }

    protected override async Task OnParametersSetAsync()
    {
        if ((Data is null && DataProvider is null) || (Data is not null && DataProvider is not null))
            throw new ArgumentException($"Grid requires either {nameof(Data)} or {nameof(DataProvider)}, but not both or neither.");

        if (AllowPaging && PageSize < 0)
            throw new ArgumentException($"{nameof(PageSize)} must be greater than zero.");

        if (pendingPageSizeChanging)
        {
            PageSize = pageSize;
            return;
        }

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
                await ResetPageNumberAsync();
                await SaveGridSettingsAsync();
            }

            //if (!mustRefreshData && selectedItems != SelectedItems)
            //{
            //    mustRefreshData = true;
            //    SelectedItems = selectedItems;
            //}

            // We want to trigger the first data load when we've collected the initial set of columns
            // because they might perform some action, like setting the default sort order. 
            // It would be wasteful to have to re-query immediately.
            if (columns.Count > 0 && mustRefreshData)
                await RefreshDataAsync(false);
        }

        await base.OnParametersSetAsync();
    }

    /// <summary>
    /// Downloads a CSV file of every row that matches the current filters and sort, across all pages, with the visible columns in display order.
    /// With a <c>DataProvider</c>, the provider is called once with <c>PageNumber</c> and <c>PageSize</c> set to 0, which means all rows.
    /// When no row matches, or the data cannot be read, nothing is downloaded and a status message says so.
    /// </summary>
    /// <returns>Task</returns>
    [AddedVersion("4.1.0")]
    [Description("Downloads a CSV file of every row that matches the current filters and sort, across all pages, with the visible columns in display order.")]
    public async Task ExportToCsvAsync()
    {
        exportStatus = string.Empty;

        // Nothing in an export may break the page: a data source failure or cancellation, a property getter that throws
        // while the CSV is built, or a browser error from the download (for example an outdated cached script).
        // The status message reports it instead.
        try
        {
            var request = new GridDataProviderRequest<TItem>
            {
                PageNumber = 0,
                PageSize = 0,
                Sorting = AllowSorting ? gridCurrentState.Sorting ?? GetDefaultSorting()! : null!,
                Filters = AllowFiltering ? GetFilters()! : null!
            };

            var result = DataProvider is not null ? await DataProvider.Invoke(request) : request.ApplyTo(Data!);
            var rows = result?.Data?.ToList() ?? new List<TItem>();

            if (rows.Count == 0)
            {
                exportStatus = ExportNoDataText;
            }
            else
            {
                var exportColumns = GetDisplayColumns()
                                    .Where(column => column.Exportable && !string.IsNullOrWhiteSpace(column.PropertyName))
                                    .Select(column => (GetColumnLabel(column), column.PropertyName!))
                                    .ToList();

                var csv = GridCsvUtility.Write(rows, exportColumns, GetCultureInfo());
                var fileName = ExportFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ? ExportFileName : $"{ExportFileName}.csv";

                await SafeInvokeVoidAsync("window.blazorBootstrap.grid.downloadFile", fileName, "text/csv;charset=utf-8", csv);
            }
        }
        catch (Exception)
        {
            exportStatus = ExportErrorText;
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Get filters.
    /// </summary>
    /// <returns>IEnumerable</returns>
    [AddedVersion("1.0.0")]
    [Description("Get filters.")]
    public IEnumerable<FilterItem>? GetFilters() =>
        !AllowFiltering || columns == null || !columns.Any()
            ? null
            : columns
              .Where(column => column.Filterable && column.GetFilterOperator() != FilterOperator.None && !string.IsNullOrWhiteSpace(column.GetFilterValue()))
              ?.Select(column => new FilterItem(column.PropertyName!, column.GetFilterValue(), column.GetFilterOperator(), column.StringComparison));

    private string GetColumnSummaryValue(GridSummaryColumnType type, string propertyName, string format, string prefix)
    {
        double value = 0;

        if (type == GridSummaryColumnType.Average)
        {
            prefix ??= "Avg: ";
            value = items?.Average(x => Convert.ToDouble(x?.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
        }
        else if (type == GridSummaryColumnType.Count)
        {
            prefix ??= "Count: ";
            value = items?.Where(x => x?.GetType().GetProperty(propertyName)?.GetValue(x) is not null).Count() ?? 0;
        }
        else if (type == GridSummaryColumnType.Max)
        {
            prefix ??= "Max: ";
            value = items?.Max(x => Convert.ToDouble(x?.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
        }
        else if (type == GridSummaryColumnType.Min)
        {
            prefix ??= "Min: ";
            value = items?.Min(x => Convert.ToDouble(x?.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
        }
        else if (type == GridSummaryColumnType.Sum)
        {
            prefix ??= "Total: ";
            value = items?.Sum(x => Convert.ToDouble(x?.GetType().GetProperty(propertyName)?.GetValue(x))) ?? 0;
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
    [AddedVersion("1.0.0")]
    [Description("Refresh the grid data.")]
    public async Task RefreshDataAsync(CancellationToken cancellationToken = default) => await RefreshDataAsync(false, cancellationToken);

    /// <summary>
    /// Restores the declared column order and shows every column the user has hidden, then saves the grid settings.
    /// </summary>
    /// <returns>Task</returns>
    [AddedVersion("4.1.0")]
    [Description("Restores the declared column order and shows every column the user has hidden, then saves the grid settings.")]
    public async Task ResetColumnsAsync()
    {
        columnOrder = null;
        hiddenColumns.Clear();

        await SaveGridSettingsAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Reset the page number to 1 and refresh the grid.
    /// </summary>
    [AddedVersion("1.4.3")]
    [Description("Reset the page number to 1 and refresh the grid.")]
    public async ValueTask ResetPageNumber() => await ResetPageNumberAsync(true);

    /// <summary>
    /// Selects all items in the grid asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task completes when all items have been selected.</returns>
    [AddedVersion("1.8.0")]
    [Description("Selects all items in the grid asynchronously.")]
    public Task SelectAllItemsAsync() => SelectAllItemsInternalAsync(true);

    /// <summary>
    /// Asynchronously clears the selection of all items in the grid.
    /// </summary>
    /// <remarks>This method can be awaited to ensure that all items are unselected before proceeding with
    /// further actions. If no items are currently selected, the operation completes without effect.</remarks>
    /// <returns>A task that represents the asynchronous operation of deselecting all items.</returns>
    [AddedVersion("1.8.0")]
    [Description("Asynchronously clears the selection of all items in the grid.")]
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

    /// <summary>
    /// Counts the columns declared before <paramref name="column" /> whose name (property name, or header text when none) is <paramref name="name" />.
    /// </summary>
    internal int CountEarlierColumnsNamed(GridColumn<TItem> column, string name)
    {
        var count = 0;

        foreach (var candidate in columns)
        {
            if (candidate == column)
                break;

            if (string.Equals(candidate.ColumnName, name, StringComparison.Ordinal))
                count++;
        }

        return count;
    }

    /// <summary>
    /// Moves a column one place up or down among the columns the column chooser lists, then saves the grid settings.
    /// </summary>
    internal async Task MoveColumnAsync(GridColumn<TItem> column, int offset)
    {
        var listed = GetChooserColumns();
        var index = listed.IndexOf(column);
        var target = index + offset;

        if (index < 0 || target < 0 || target >= listed.Count)
            return;

        await MoveColumnBeforeAsync(column, offset < 0 ? listed[target] : (target + 1 < listed.Count ? listed[target + 1] : null));
    }

    /// <summary>
    /// Moves a column so it comes just before <paramref name="target" /> (or last when <paramref name="target" /> is <see langword="null" />),
    /// then saves the grid settings and announces the new position.
    /// </summary>
    internal async Task MoveColumnBeforeAsync(GridColumn<TItem> column, GridColumn<TItem>? target)
    {
        if (column == target)
            return;

        var ordered = GetOrderedColumns();
        ordered.Remove(column);
        ordered.Insert(target is null ? ordered.Count : ordered.IndexOf(target), column);

        var keys = ordered.Select(c => c.ColumnKey).Where(key => key is not null).Select(key => key!).ToList();
        var declared = columns.Select(c => c.ColumnKey).Where(key => key is not null).Select(key => key!).ToList();

        // Back at the declared order means no saved order, so default grids keep saving null.
        columnOrder = keys.SequenceEqual(declared, StringComparer.Ordinal) ? null : keys;

        var listed = GetChooserColumns();
        columnChooserStatus = string.Format(ColumnChooserMovedText, GetColumnLabel(column), listed.IndexOf(column) + 1, listed.Count);

        await SaveGridSettingsAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Hides or shows a column for the user, then saves the grid settings. A column marked <c>AlwaysVisible</c>,
    /// a column without a key, and the last shown column cannot be hidden.
    /// </summary>
    internal async Task SetColumnHiddenAsync(GridColumn<TItem> column, bool hidden)
    {
        if (column.ColumnKey is null || (hidden && IsColumnLocked(column)))
            return;

        if (hidden)
            hiddenColumns.Add(column.ColumnKey);
        else
            hiddenColumns.Remove(column.ColumnKey);

        columnChooserStatus = string.Empty;

        await SaveGridSettingsAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Gets every column in display order, including hidden ones. Columns missing from the saved order
    /// (new columns, or columns without a key) keep their declared position.
    /// </summary>
    internal List<GridColumn<TItem>> GetOrderedColumns()
    {
        if (columnOrder is null || columnOrder.Count == 0)
            return columns.ToList();

        var ordered = new List<GridColumn<TItem>>(columns.Count);

        foreach (var key in columnOrder)
        {
            var column = columns.FirstOrDefault(c => c.ColumnKey == key && !ordered.Contains(c));

            if (column is not null)
                ordered.Add(column);
        }

        for (var index = 0; index < columns.Count; index++)
            if (!ordered.Contains(columns[index]))
                ordered.Insert(Math.Min(index, ordered.Count), columns[index]);

        return ordered;
    }

    /// <summary>
    /// Determines whether the user has hidden the column. A column marked <c>AlwaysVisible</c>, or one without a key, is never hidden by the user.
    /// </summary>
    internal bool IsHiddenByUser(GridColumn<TItem> column) =>
        !column.AlwaysVisible && column.ColumnKey is not null && hiddenColumns.Contains(column.ColumnKey);

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

        if (firstRender)
            isColumnsLoading = false;

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

    private async Task CheckOrUnCheckAll() => await SafeInvokeVoidAsync("window.blazorBootstrap.grid.checkOrUnCheckAll", $".bb-grid-form-check-{headerCheckboxId} > input.form-check-input", allItemsSelected);

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
            builder.AddAttribute(107, "aria-label", RowSelectionLabel?.Invoke(rowData) ?? SelectRowText);

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

    /// <summary>
    /// Gets the columns the column chooser lists, in display order: those the app shows and that have a key.
    /// </summary>
    private List<GridColumn<TItem>> GetChooserColumns() =>
        GetOrderedColumns().Where(c => c.IsVisible && c.ColumnKey is not null).ToList();

    private static string GetColumnLabel(GridColumn<TItem> column) =>
        !string.IsNullOrWhiteSpace(column.HeaderText) ? column.HeaderText! : column.PropertyName ?? string.Empty;

    /// <summary>
    /// Gets the columns to render, in display order: visible to the app and not hidden by the user.
    /// </summary>
    private List<GridColumn<TItem>> GetDisplayColumns()
    {
        var ordered = GetOrderedColumns();
        var shown = ordered.Where(c => c.IsVisible && !IsHiddenByUser(c)).ToList();

        // The user can never hide every column; ignore saved hidden columns that would.
        if (shown.Count == 0 && hiddenColumns.Count > 0)
            shown = ordered.Where(c => c.IsVisible).ToList();

        return shown;
    }

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
    /// Determines whether the user cannot hide the column: it is marked <c>AlwaysVisible</c>, or it is the only column still shown.
    /// </summary>
    private bool IsColumnLocked(GridColumn<TItem> column) =>
        column.AlwaysVisible || (!IsHiddenByUser(column) && GetDisplayColumns().Count <= 1);

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

        // Settings saved before column order and hidden columns existed have nulls here and keep the defaults.
        columnOrder = settings.ColumnOrder?.Where(key => !string.IsNullOrWhiteSpace(key)).Distinct(StringComparer.Ordinal).ToList();

        if (columnOrder is { Count: 0 })
            columnOrder = null;

        hiddenColumns = settings.HiddenColumns is null
                            ? new HashSet<string>(StringComparer.Ordinal)
                            : new HashSet<string>(settings.HiddenColumns.Where(key => !string.IsNullOrWhiteSpace(key)), StringComparer.Ordinal);

        if (settings.Filters is not null && settings.Filters.Any())
            SetFilters(settings.Filters);

        if ((settings.PageNumber > 0) && (settings.PageSize > 0))
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

    private async Task OnColumnDroppedAsync(GridColumn<TItem> target)
    {
        var dragged = draggedColumn;
        draggedColumn = null;

        if (dragged is not null)
            await MoveColumnBeforeAsync(dragged, target);
    }

    private async Task OnHeaderCheckboxChanged(ChangeEventArgs args)
    {
        var headerCheckboxState = bool.TryParse(args?.Value?.ToString(), out var checkboxState) && checkboxState;
        await SelectAllItemsInternalAsync(headerCheckboxState);
    }

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        pendingPageSizeChanging = true;
        gridCurrentState = new GridState<TItem>(newPageNumber, gridCurrentState.Sorting);
        await SaveGridSettingsAsync();
        await RefreshDataAsync(false);
        pendingPageSizeChanging = false;
    }

    private async Task OnPageSizeChangedAsync(int newPageSize)
    {
        pendingPageSizeChanging = true;
        pageSize = PageSize = newPageSize;
        await ResetPageNumberAsync();
        await SaveGridSettingsAsync();
        await RefreshDataAsync(false);
        pendingPageSizeChanging = false;
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
        await SafeInvokeVoidAsync("window.blazorBootstrap.grid.scroll", Id);
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

    private async Task SaveGridSettingsAsync()
    {
        if (!GridSettingsChanged.HasDelegate)
            return;

        var settings = new GridSettings
        {
            ColumnOrder = columnOrder?.ToList(),
            Filters = AllowFiltering ? GetFilters() : null,
            HiddenColumns = hiddenColumns.Count > 0 ? hiddenColumns.ToList() : null,
            PageNumber = AllowPaging ? gridCurrentState.PageIndex : 0,
            PageSize = AllowPaging ? pageSize : 0
        };

        await GridSettingsChanged.InvokeAsync(settings);
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

    private async Task SetCheckboxStateAsync(string id, CheckboxState checkboxState)
    {
        queuedTasks.Enqueue(async () => await SafeInvokeVoidAsync("window.blazorBootstrap.grid.setSelectAllCheckboxState", id, (int)checkboxState));      
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
    /// If <see langword="true" />, a Columns button above the grid lets the user hide, show, search, and reorder columns.
    /// The choice is part of the grid settings (<c>GridSettingsChanged</c> and <c>SettingsProvider</c>).
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, a Columns button above the grid lets the user hide, show, search, and reorder columns.")]
    [Parameter]
    public bool AllowColumnChooser { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the grid detail view is enabled.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the grid detail view is enabled.")]
    [Parameter]
    public bool AllowDetailView { get; set; }

    /// <summary>
    /// If <see langword="true" />, an Export CSV button above the grid downloads every row that matches the current filters and sort, with the visible columns.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, an Export CSV button above the grid downloads every row that matches the current filters and sort, with the visible columns.")]
    [Parameter]
    public bool AllowExport { get; set; }

    /// <summary>
    /// Gets or sets the grid filtering.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the grid filtering.")]
    [Parameter]
    public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets the grid paging.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the grid paging.")]
    [Parameter]
    public bool AllowPaging { get; set; }

    /// <summary>
    /// Gets or sets the allow row click.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.9.5")]
    [DefaultValue(false)]
    [Description("Gets or sets the allow row click.")]
    [Parameter]
    public bool AllowRowClick { get; set; }

    /// <summary>
    /// Gets or sets the grid selection.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the grid selection.")]
    [Parameter]
    public bool AllowSelection { get; set; }

    /// <summary>
    /// Gets or sets the grid sorting.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the grid sorting.")]
    [Parameter]
    public bool AllowSorting { get; set; }

    /// <summary>
    /// Gets or sets the grid summary.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the grid summary.")]
    [Parameter]
    public bool AllowSummary { get; set; }

    /// <summary>
    /// Automatically hides the paging controls when the grid item count is less than or equal to the <see cref="PageSize" />
    /// and this property is set to `true`.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.5")]
    [DefaultValue(false)]
    [Description("Automatically hides the paging controls when the grid item count is less than or equal to the <b>PageSize</b> and this property is set to <b>true</b>.")]
    [Parameter]
    public bool AutoHidePaging { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the column chooser button text.
    /// <para>
    /// Default value is 'Columns'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Columns")]
    [Description("Gets or sets the column chooser button text.")]
    [Parameter]
    public string ColumnChooserButtonText { get; set; } = "Columns";

    /// <summary>
    /// Gets or sets the note shown when a column cannot be hidden.
    /// <para>
    /// Default value is 'At least one column stays visible.'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("At least one column stays visible.")]
    [Description("Gets or sets the note shown when a column cannot be hidden.")]
    [Parameter]
    public string ColumnChooserLastColumnText { get; set; } = "At least one column stays visible.";

    /// <summary>
    /// Gets or sets the announcement after a column moves. {0} is the column name, {1} its new position, and {2} the number of columns.
    /// <para>
    /// Default value is '{0} moved to position {1} of {2}.'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("{0} moved to position {1} of {2}.")]
    [Description("Gets or sets the announcement after a column moves. {0} is the column name, {1} its new position, and {2} the number of columns.")]
    [Parameter]
    public string ColumnChooserMovedText { get; set; } = "{0} moved to position {1} of {2}.";

    /// <summary>
    /// Gets or sets the accessible name of the move-down button. {0} is the column name.
    /// <para>
    /// Default value is 'Move {0} down'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Move {0} down")]
    [Description("Gets or sets the accessible name of the move-down button. {0} is the column name.")]
    [Parameter]
    public string ColumnChooserMoveDownText { get; set; } = "Move {0} down";

    /// <summary>
    /// Gets or sets the accessible name of the move-up button. {0} is the column name.
    /// <para>
    /// Default value is 'Move {0} up'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Move {0} up")]
    [Description("Gets or sets the accessible name of the move-up button. {0} is the column name.")]
    [Parameter]
    public string ColumnChooserMoveUpText { get; set; } = "Move {0} up";

    /// <summary>
    /// Gets or sets the text shown when the column search matches nothing.
    /// <para>
    /// Default value is 'No columns match'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("No columns match")]
    [Description("Gets or sets the text shown when the column search matches nothing.")]
    [Parameter]
    public string ColumnChooserNoMatchText { get; set; } = "No columns match";

    /// <summary>
    /// Gets or sets the column chooser reset button text.
    /// <para>
    /// Default value is 'Reset to default'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Reset to default")]
    [Description("Gets or sets the column chooser reset button text.")]
    [Parameter]
    public string ColumnChooserResetText { get; set; } = "Reset to default";

    /// <summary>
    /// Gets or sets the column search box placeholder and accessible name.
    /// <para>
    /// Default value is 'Search columns'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Search columns")]
    [Description("Gets or sets the column search box placeholder and accessible name.")]
    [Parameter]
    public string ColumnChooserSearchPlaceholderText { get; set; } = "Search columns";

    /// <summary>
    /// Gets or sets the grid data.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.4.3")]
    [DefaultValue(null)]
    [Description("Gets or sets the grid data.")]
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    /// DataProvider is for items to render.
    /// The provider should always return an instance of <see cref="GridDataProviderResult{TItem}"/>, and <see langword="null"/> is not allowed.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("DataProvider is for items to render. The provider should always return an instance of <b>GridDataProviderResult</b>, and <b>null</b> is not allowed.")]
    [Parameter]
    public GridDataProviderDelegate<TItem>? DataProvider { get; set; }

    /// <summary>
    /// Enable or disable the header checkbox selection.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue(null)]
    [Description("Enable or disable the header checkbox selection.")]
    [Parameter]
    public Func<IEnumerable<TItem>, bool>? DisableAllRowsSelection { get; set; }

    /// <summary>
    /// Enable or disable the row level checkbox selection.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue(null)]
    [Description("Enable or disable the row level checkbox selection.")]
    [Parameter]
    public Func<TItem, bool>? DisableRowSelection { get; set; }

    /// <summary>
    /// Gets or sets the empty text.
    /// Shows text on no records.
    /// <para>
    /// Default value is 'No records to display'.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue("No records to display")]
    [Description("Gets or sets the empty text. Shows text on no records.")]
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Gets or sets the enum filter select text.
    /// <para>
    /// Default value is 'Select'.
    /// </para>
    /// </summary>
    [AddedVersion("3.1.0")]
    [DefaultValue("Select")]
    [Description("Gets or sets the enum filter select text.")]
    [Parameter]
    public string EnumFilterSelectText { get; set; } = "Select";

    /// <summary>
    /// Gets or sets the export button text.
    /// <para>
    /// Default value is 'Export CSV'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Export CSV")]
    [Description("Gets or sets the export button text.")]
    [Parameter]
    public string ExportButtonText { get; set; } = "Export CSV";

    /// <summary>
    /// Gets or sets the message shown when the rows cannot be read for export.
    /// <para>
    /// Default value is 'The rows could not be exported. Try again.'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("The rows could not be exported. Try again.")]
    [Description("Gets or sets the message shown when the rows cannot be read for export.")]
    [Parameter]
    public string ExportErrorText { get; set; } = "The rows could not be exported. Try again.";

    /// <summary>
    /// Gets or sets the downloaded file name; .csv is added when missing.
    /// <para>
    /// Default value is 'export'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("export")]
    [Description("Gets or sets the downloaded file name; .csv is added when missing.")]
    [Parameter]
    public string ExportFileName { get; set; } = "export";

    /// <summary>
    /// Gets or sets the message shown when no row matches the current filters.
    /// <para>
    /// Default value is 'There are no rows to export.'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("There are no rows to export.")]
    [Description("Gets or sets the message shown when no row matches the current filters.")]
    [Parameter]
    public string ExportNoDataText { get; set; } = "There are no rows to export.";

    /// <summary>
    /// Gets or sets the accessible name of each filter input in the filters row. <c>{0}</c> is the column's header text
    /// (or its property name when it has none).
    /// <para>
    /// Default value is 'Filter by {0}'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Filter by {0}")]
    [Description("Gets or sets the accessible name of each filter input in the filters row. <b>{0}</b> is the column's header text (or its property name when it has none).")]
    [Parameter]
    public string FilterInputLabelText { get; set; } = "Filter by {0}";

    /// <summary>
    /// Gets or sets the filters row css class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.9.2")]
    [DefaultValue(null)]
    [Description("Gets or sets the filters row css class.")]
    [Parameter]
    public string? FiltersRowCssClass { get; set; }

    /// <summary>
    /// Gets or sets the filters translation provider.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the filters translation provider.")]
    [Parameter]
    public GridFiltersTranslationDelegate? FiltersTranslationProvider { get; set; }

    /// <summary>
    /// Gets or sets the grid fixed header.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.3")]
    [DefaultValue(false)]
    [Description("Gets or sets the grid fixed header.")]
    [Parameter]
    public bool FixedHeader { get; set; }

    /// <summary>
    /// Gets or sets the grid container css class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the grid container css class.")]
    [Parameter]
    public string? GridContainerClass { get; set; }

    private string? GridContainerClassNames =>
        BuildClassNames(
            GridContainerClass,
            (BootstrapClass.TableResponsive, Responsive)
        );

    /// <summary>
    /// Gets or sets the grid container css style.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the grid container css style.")]
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
    [AddedVersion("1.0.0")]
    [Description("This event is fired when the grid state is changed.")]
    [Parameter]
    public EventCallback<GridSettings> GridSettingsChanged { get; set; }

    /// <summary>
    /// Gets or sets the header row css class but not the thead tag class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.9.2")]
    [DefaultValue(null)]
    [Description("Gets or sets the header row css class but not the thead tag class.")]
    [Parameter]
    public string? HeaderRowCssClass { get; set; }

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
                                            builder.AddAttribute(108, "aria-label", SelectAllRowsText);

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
                                        else
                                        {
                                            // A header cell needs a name even when single selection has no "select all" checkbox.
                                            builder.OpenElement(113, "span");
                                            builder.AddAttribute(114, "class", "visually-hidden");
                                            builder.AddContent(115, SelectRowText);
                                            builder.CloseElement(); // close: span
                                        }

                                        builder.CloseElement(); // close: th
                                    };

    /// <summary>
    /// Gets or sets the grid height.
    /// <para>
    /// Default value is 320 <see cref="Unit.Px" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.3")]
    [DefaultValue(320)]
    [Description("Gets or sets the grid height.")]
    [Parameter]
    public float Height { get; set; } = 320;

    /// <summary>
    /// Gets or sets the items per page text.
    /// <para>
    /// Default value is 'Items per page'.
    /// </para>
    /// </summary>
    [AddedVersion("1.9.5")]
    [DefaultValue("Items per page")]
    [Description("Gets or sets the items per page text.")]
    [Parameter]
    public string ItemsPerPageText { get; set; } = "Items per page"!;

    /// <summary>
    /// Gets or sets the locale.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.4.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the locale.")]
    [Parameter]
    public string? Locale { get; set; }

    /// <summary>
    /// This event is triggered when the user clicks on the row.
    /// Set AllowRowClick to true to enable row clicking.
    /// </summary>
    [AddedVersion("1.9.5")]
    [Description("This event is triggered when the user clicks on the row. Set <b>AllowRowClick</b> to <b>true</b> to enable row clicking.")]
    [Parameter]
    public EventCallback<GridRowEventArgs<TItem>> OnRowClick { get; set; }

    /// <summary>
    /// This event is triggered when the user double clicks on the row.
    /// Set AllowRowClick to true to enable row double clicking.
    /// </summary>
    [AddedVersion("1.9.5")]
    [Description("This event is triggered when the user double clicks on the row. Set <b>AllowRowClick</b> to <b>true</b> to enable row double clicking.")]
    [Parameter]
    public EventCallback<GridRowEventArgs<TItem>> OnRowDoubleClick { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// <para>
    /// Default value is 10.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(10)]
    [Description("Gets or sets the page size.")]
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the page size selector items.
    /// <para>
    /// Default value is '{ 10, 20, 50 }'.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue(new int[] { 10, 20, 50 })]
    [Description("Gets or sets the page size selector items.")]
    [Parameter]
    public int[] PageSizeSelectorItems { get; set; } = { 10, 20, 50 };

    /// <summary>
    /// Gets or sets the page size selector visible.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the page size selector visible.")]
    [Parameter]
    public bool PageSizeSelectorVisible { get; set; }

    private string paginationItemsText => GetPaginationItemsText();

    /// <summary>
    /// Gets or sets the pagination items text format.
    /// <para>
    /// Default value is '{0} - {1} of {2} items'.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue("{0} - {1} of {2} items")]
    [Description("Gets or sets the pagination items text format.")]
    [Parameter]
    public string PaginationItemsTextFormat { get; set; } = "{0} - {1} of {2} items"!;

    /// <summary>
    /// Gets or sets a value indicating whether the grid is responsive.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the grid is responsive.")]
    [Parameter]
    public bool Responsive { get; set; }

    /// <summary>
    /// Gets or sets the row class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.6.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the row class.")]
    [Parameter]
    public Func<TItem, string>? RowClass { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of each row's selection checkbox, for example <c>item => $"Select ticket {item.Id}"</c>.
    /// When <see langword="null" />, <see cref="SelectRowText" /> is used.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of each row's selection checkbox. When <b>null</b>, <b>SelectRowText</b> is used.")]
    [Parameter]
    public Func<TItem, string>? RowSelectionLabel { get; set; }

    /// <summary>
    /// Gets or sets the function used to extract a unique key from a row item.
    /// <para>
    /// The key returned by the function is used to uniquely identify each row in the data set.  This
    /// is typically required for operations such as tracking changes or rendering rows efficiently. 
    /// If not set, the item hash code will be used as the key.
    /// Example usage: `RowKeySelector="(employee) => employee.Id"`.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function used to extract a unique key from a row item.")]
    [Parameter]
    public Func<TItem, object>? RowKeySelector { get; set; }

    /// <summary>
    /// Gets or sets the selected items.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the selected items.")]
    [Parameter]
    public HashSet<TItem>? SelectedItems { get; set; }

    /// <summary>
    /// This event is fired when the item selection changes.
    /// </summary>
    [AddedVersion("1.8.0")]
    [Description("This event is fired when the item selection changes.")]
    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the select-all checkbox in the header.
    /// <para>
    /// Default value is 'Select all rows'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Select all rows")]
    [Description("Gets or sets the accessible name of the select-all checkbox in the header.")]
    [Parameter]
    public string SelectAllRowsText { get; set; } = "Select all rows";

    /// <summary>
    /// Gets or sets the accessible name of a row's selection checkbox when <c>RowSelectionLabel</c> is not set, and of the selection column header in single selection mode.
    /// <para>
    /// Default value is 'Select row'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Select row")]
    [Description("Gets or sets the accessible name of a row's selection checkbox when <b>RowSelectionLabel</b> is not set, and of the selection column header in single selection mode.")]
    [Parameter]
    public string SelectRowText { get; set; } = "Select row";

    /// <summary>
    /// Gets or sets the grid selection mode.
    /// <para>
    /// Default value is <see cref="GridSelectionMode.Single" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.8.0")]
    [DefaultValue(GridSelectionMode.Single)]
    [Description("Gets or sets the grid selection mode.")]
    [Parameter]
    public GridSelectionMode SelectionMode { get; set; } = GridSelectionMode.Single;

    /// <summary>
    /// Settings is for grid to render.
    /// The provider should always return an instance of <see cref="GridSettings"/>, and <see langword="null"/> is not allowed.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Settings is for grid to render. The provider should always return an instance of <b>GridSettings</b>, and <b>null</b> is not allowed.")]
    [Parameter]
    public GridSettingsProviderDelegate? SettingsProvider { get; set; }

    /// <summary>
    /// Gets or sets the thead css class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.3")]
    [DefaultValue(null)]
    [Description("Gets or sets the thead css class.")]
    [Parameter]
    public string? THeadCssClass { get; set; }

    private int totalPages => GetTotalPagesCount();

    /// <summary>
    /// Gets or sets the units.
    /// <para>
    /// Default value is <see cref="Unit.Px" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.3")]
    [DefaultValue(null)]
    [Description("Gets or sets the units.")]
    [Parameter]
    public Unit Unit { get; set; } = Unit.Px;

    #endregion
}
