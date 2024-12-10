using System.Collections.Immutable;

namespace BlazorBootstrap;

/// <summary>
/// Use Blazor Bootstrap grid component to display tabular data from the data source. It supports client-side and server-side paging and sorting.
/// </summary>
/// <typeparam name="TItem">Data model to apply to the grid.</typeparam>
public partial class Grid<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    public bool AllItemsSelected = false;

    private CancellationTokenSource? cancellationTokenSource;

    private Dictionary<int, string> checkboxIds = new();

    private readonly List<GridColumn<TItem>> columns = new();

    /// <summary>
    /// Current grid state (filters, paging, sorting).
    /// </summary>
    internal GridState<TItem> GridCurrentState = new(1, null);

    private string headerCheckboxId = default!;

    private RenderFragment? headerSelectionTemplate;

    private GridDetailView<TItem>? detailView;

    public GridEmptyDataTemplate<TItem>? emptyDataTemplate;

    public GridLoadingTemplate<TItem>? loadingTemplate;

    private bool isFirstRenderComplete = false;

    private IReadOnlyCollection<TItem>? items = null;

    private object? lastAssignedDataOrDataProvider;

    private int pageSize;

    private bool requestInProgress = false;

    private HashSet<TItem> selectedItems = new();

    public int SelectedItemsCount = 0;

    private int? totalCount = null;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RefreshDataAsync(firstRender);
            isFirstRenderComplete = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        headerCheckboxId = IdUtility.GetNextId();

        pageSize = PageSize;

        base.OnInitialized();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
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
                await ResetPageNumberAsync();
                await SaveGridSettingsAsync();
            }

            // We want to trigger the first data load when we've collected the initial set of columns
            // because they might perform some action, like setting the default sort order. 
            // It would be wasteful to have to re-query immediately.
            if (columns.Count > 0 && mustRefreshData)
            {
                await RefreshDataAsync(false);
            }
        }

        await base.OnParametersSetAsync();
    }

    /// <summary>
    /// Get filters.
    /// </summary>
    /// <returns><see cref="IReadOnlyCollection{T}"/></returns>
    public IReadOnlyCollection<FilterItem>? GetFilters() =>
        !AllowFiltering || !columns.Any()
            ? null
            : columns
              .Where(column => column.Filterable && column.GetFilterOperator() != FilterOperator.None && !string.IsNullOrWhiteSpace(column.GetFilterValue()))
              ?.Select(column => new FilterItem(column.PropertyName, column.GetFilterValue(), column.GetFilterOperator(), column.StringComparison)).ToImmutableArray();

    /// <summary>
    /// Refresh the grid data.
    /// </summary>
    /// <returns>Task</returns>
    public Task RefreshDataAsync(CancellationToken cancellationToken = default) => RefreshDataAsync(false, cancellationToken);

    /// <summary>
    /// Reset the page number to 1 and refresh the grid.
    /// </summary>
    public ValueTask ResetPageNumber() => ResetPageNumberAsync(true);

    internal void AddColumn(GridColumn<TItem> column) => columns?.Add(column);

    internal async Task FilterChangedAsync()
    {
        if (cancellationTokenSource is not null && !cancellationTokenSource.IsCancellationRequested)
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
                          PageNumber = AllowPaging ? GridCurrentState.PageIndex : 0,
                          PageSize = AllowPaging ? pageSize : 0,
                          Sorting = AllowSorting ? GridCurrentState.Sorting ?? GetDefaultSorting()! : null!,
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
            items = result.Data;
            totalCount = result.TotalCount ?? result.Data!.Count;
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
        GridCurrentState = new GridState<TItem>(1, GridCurrentState.Sorting);

        if (refreshGrid)
            await RefreshDataAsync(false);
    }

    internal async Task SortingChangedAsync(GridColumn<TItem> column)
    {
        if (!columns.Any())
            return;

        // check sorting enabled on any of the columns
        var sortedColumn = columns.FirstOrDefault(c => c.CurrentSortDirection != SortDirection.None);

        // reset other columns sorting
        columns.ForEach(
            c =>
            {
                if (c.Id != column.Id)
                    c.CurrentSortDirection = SortDirection.None;

                // set default sorting
                if (sortedColumn == null && c.IsDefaultSortColumn)
                {
                    if (c.Id == column.Id
                        && c.CurrentSortDirection == SortDirection.None
                        && c.DefaultSortDirection == SortDirection.Descending)
                        c.CurrentSortDirection = SortDirection.Ascending; // Default Sorting: DESC                
                    else
                        c.CurrentSortDirection = c.DefaultSortDirection != SortDirection.None ? c.DefaultSortDirection : SortDirection.Ascending;

                    GridCurrentState = new GridState<TItem>(GridCurrentState.PageIndex, c.GetSorting());
                }
                else if (c.Id == column.Id && c.SortDirection != SortDirection.None) // TODO: this condition is not required. 1 -> ASC, 2 -> DESC, 3 -> None. Here 3 scenario is not working
                {
                    GridCurrentState = new GridState<TItem>(GridCurrentState.PageIndex, c.GetSorting());
                }
            }
        );

        await SaveGridSettingsAsync();
        await RefreshDataAsync(false);
    }

    private async Task CheckOrUnCheckAll() => await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.grid.checkOrUnCheckAll", $".bb-grid-form-check-{headerCheckboxId} > input.form-check-input", AllItemsSelected);

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

    private IReadOnlyCollection<SortingItem<TItem>>? GetDefaultSorting() =>
        !AllowSorting || !columns.Any()
            ? null
            : columns?.Where(column => column.CanSort() && column.IsDefaultSortColumn).SelectMany(item => item.GetSorting()).ToImmutableArray();

    private string GetGridContainerStyle()
    {
        var styleAttributes = new HashSet<string>();

        if (FixedHeader) styleAttributes.Add($"height:{Height.ToString(CultureInfo.InvariantCulture)}{EnumExtensions.UnitCssStringMap[Unit]}");

        return string.Join(";", styleAttributes);
    }

    private string GetPaginationItemsText()
    {
        var startRecord = (GridCurrentState.PageIndex - 1) * pageSize + 1;
        var endRecord = GridCurrentState.PageIndex * pageSize;

        if (endRecord > totalCount)
            endRecord = totalCount ?? 0;

        return string.Format(PaginationItemsTextFormat, startRecord, endRecord, totalCount);
    }

    private int GetTotalPagesCount()
    {
        if (totalCount is > 0)
        {
            var q = totalCount.Value / pageSize;
            var r = totalCount.Value % pageSize;

            return q < 1 ? 1 : q + (r > 0 ? 1 : 0);
        }

        return 1;
    }

    private IReadOnlyCollection<FilterOperatorInfo>? GridFiltersTranslationProviderAsync()
    {
        if (FiltersTranslationProvider is null)
            return null;

        var filters = FiltersTranslationProvider.Invoke();

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
                GridCurrentState = new GridState<TItem>(settings.PageNumber, GridCurrentState.Sorting);
                pageSize = settings.PageSize;
            }
            else
            {
                GridCurrentState = new GridState<TItem>(1, null);
                pageSize = 10;
            }
        }
        else
        {
            GridCurrentState = new GridState<TItem>(1, null);
            pageSize = 10;
        }
    }

    private async Task OnHeaderCheckboxChanged(ChangeEventArgs args)
    {
        AllItemsSelected = bool.TryParse(args?.Value?.ToString(), out var checkboxState) && checkboxState;
        selectedItems = AllItemsSelected ? new HashSet<TItem>(items!) : new HashSet<TItem>();
        SelectedItemsCount = selectedItems.Count;
        await CheckOrUnCheckAll();

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        GridCurrentState = new GridState<TItem>(newPageNumber, GridCurrentState.Sorting);
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
        if (!bool.TryParse(args?.Value?.ToString(), out var isChecked))
            return;

        if (SelectionMode == GridSelectionMode.Multiple)
        {
            _ = isChecked ? selectedItems.Add(item) : selectedItems.Remove(item);
            SelectedItemsCount = selectedItems.Count;
            AllItemsSelected = SelectedItemsCount == (items?.Count ?? 0);

            if (AllItemsSelected)
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
            AllItemsSelected = false;
            await CheckOrUnCheckAll();
            await SetCheckboxStateAsync(id, isChecked ? CheckboxState.Checked : CheckboxState.Unchecked);
        }

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private async Task OnScroll(EventArgs e)
    {
        await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.grid.scroll", Id);
    }

    private void PrepareCheckboxIds()
    { 
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

        SelectedItemsCount = selectedItems.Count;
        AllItemsSelected = SelectedItemsCount > 0 && items!.Count == SelectedItemsCount;

        if (AllItemsSelected)
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Checked);
        else if (SelectedItemsCount > 0)
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Indeterminate);
        else
            await SetCheckboxStateAsync(headerCheckboxId, CheckboxState.Unchecked);

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private async Task RowClick(TItem item, MouseEventArgs args)
    {
        if (AllowRowClick && OnRowClick.HasDelegate)
            await OnRowClick.InvokeAsync(new GridRowEventArgs<TItem>(item, args));
    }

    private async Task RowDoubleClick(TItem item, MouseEventArgs args)
    {
        if (AllowRowClick && OnRowDoubleClick.HasDelegate)
            await OnRowDoubleClick.InvokeAsync(new GridRowEventArgs<TItem>(item, args));
    }

    private Task SaveGridSettingsAsync()
    {
        if (!GridSettingsChanged.HasDelegate)
            return Task.CompletedTask;

        var settings = new GridSettings { PageNumber = AllowPaging ? GridCurrentState.PageIndex : 0, PageSize = AllowPaging ? pageSize : 0, Filters = AllowFiltering ? GetFilters() : null };

        return GridSettingsChanged.InvokeAsync(settings);
    }

    private ValueTask SetCheckboxStateAsync(string id, CheckboxState checkboxState) => JsRuntime.InvokeVoidAsync("window.blazorBootstrap.grid.setSelectAllCheckboxState", id, (int)checkboxState);

    internal void SetGridDetailView(GridDetailView<TItem> detailView) => this.detailView = detailView;

    internal void SetGridEmptyDataTemplate(GridEmptyDataTemplate<TItem> emptyDataTemplate) => this.emptyDataTemplate = emptyDataTemplate;

    internal void SetGridLoadingTemplate(GridLoadingTemplate<TItem> loadingTemplate) => this.loadingTemplate = loadingTemplate;

    /// <summary>
    /// Set filters.
    /// </summary>
    /// <param name="filterItems"></param>
    private void SetFilters(IReadOnlyCollection<FilterItem>? filterItems)
    {
        if (filterItems is null || !filterItems.Any())
            return;

        foreach (var item in filterItems)
        {
            var column = columns.FirstOrDefault(x => x.PropertyName == item.PropertyName);

            if (column != null)
            {
                var allowedFilterOperators = FilterOperatorUtility.GetFilterOperators(column.GetPropertyTypeName());

                if (allowedFilterOperators.Any(x => x.FilterOperator == item.Operator))
                {
                    column.SetFilterOperator(item.Operator);
                    column.SetFilterValue(item.Value);
                }
            }
        }
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
                case nameof(AllowFiltering): AllowFiltering = (bool)parameter.Value!; break;
                case nameof(AllowPaging): AllowPaging = (bool)parameter.Value!; break;
                case nameof(AllowRowClick): AllowRowClick = (bool)parameter.Value!; break;
                case nameof(AllowSelection): AllowSelection = (bool)parameter.Value!; break;
                case nameof(AllowSorting): AllowSorting = (bool)parameter.Value!; break;
                case nameof(AutoHidePaging): AutoHidePaging = (bool)parameter.Value!; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value!; break;
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Data): Data = (IReadOnlyCollection<TItem>)parameter.Value!; break;
                case nameof(DataProvider): DataProvider = (GridDataProviderDelegate<TItem>)parameter.Value!; break;
                case nameof(DisableAllRowsSelection): DisableAllRowsSelection = (Func<IReadOnlyCollection<TItem>, bool>)parameter.Value!; break;
                case nameof(DisableRowSelection): DisableRowSelection = (Func<TItem, bool>)parameter.Value!; break;
                case nameof(EmptyDataTemplate): EmptyDataTemplate = (RenderFragment)parameter.Value!; break;
                case nameof(EmptyText): EmptyText = (string)parameter.Value!; break;
                case nameof(FiltersRowCssClass): FiltersRowCssClass = (string)parameter.Value!; break;
                case nameof(FiltersTranslationProvider): FiltersTranslationProvider = (GridFiltersTranslationDelegate)parameter.Value!; break;
                case nameof(FixedHeader): FixedHeader = (bool)parameter.Value!; break;
                case nameof(GridSettingsChanged): GridSettingsChanged = (EventCallback<GridSettings>)parameter.Value!; break;
                case nameof(HeaderRowCssClass): HeaderRowCssClass = (string)parameter.Value!; break;
                case nameof(Height): Height = (float)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(IsResponsive): IsResponsive = (bool)parameter.Value!; break;
                case nameof(ItemsPerPageText): ItemsPerPageText = (string)parameter.Value!; break;
                case nameof(LoadingTemplate): LoadingTemplate = (RenderFragment)parameter.Value!; break;
                case nameof(OnRowClick): OnRowClick = (EventCallback<GridRowEventArgs<TItem>>)parameter.Value!; break;
                case nameof(OnRowDoubleClick): OnRowDoubleClick = (EventCallback<GridRowEventArgs<TItem>>)parameter.Value!; break;
                case nameof(PageSize): PageSize = (int)parameter.Value!; break;
                case nameof(PageSizeSelectorItems): PageSizeSelectorItems = (int[])parameter.Value!; break;
                case nameof(PageSizeSelectorVisible): PageSizeSelectorVisible = (bool)parameter.Value!; break;
                case nameof(PaginationItemsTextFormat): PaginationItemsTextFormat = (string)parameter.Value!; break;
                case nameof(RowClass): RowClass = (Func<TItem, string>)parameter.Value!; break;
                case nameof(SelectedItemsChanged): SelectedItemsChanged = (EventCallback<HashSet<TItem>>)parameter.Value!; break;
                case nameof(SelectionMode): SelectionMode = (GridSelectionMode)parameter.Value!; break;
                case nameof(SettingsProvider): SettingsProvider = (GridSettingsProviderDelegate)parameter.Value!; break;
                
                case nameof(TableHeaderCssClass): TableHeaderCssClass = (string)parameter.Value!; break;
                case nameof(Unit): Unit = (Unit)parameter.Value!; break;
                
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets adding a column for detailed view.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AllowDetailView { get; set; }
    
    /// <summary>
    /// Gets or sets the grid filtering.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets the grid paging.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AllowPaging { get; set; }

    /// <summary>
    /// Gets or sets the allow row click.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AllowRowClick { get; set; }

    /// <summary>
    /// Gets or sets the grid selection.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AllowSelection { get; set; }

    /// <summary>
    /// Gets or sets the grid sorting.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AllowSorting { get; set; }

    /// <summary>
    /// Automatically hides the paging controls when the grid item count is less than or equal to the <see cref="PageSize" />
    /// and this property is set to `true`.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AutoHidePaging { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the grid data.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public IReadOnlyCollection<TItem>? Data { get; set; } = default!;

    /// <summary>
    /// DataProvider is for items to render.
    /// The provider should always return an instance of 'GridDataProviderResult', and 'null' is not allowed.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public GridDataProviderDelegate<TItem>? DataProvider { get; set; } = default!;

    /// <summary>
    /// Enable or disable the header checkbox selection.
    /// </summary>
    [Parameter]
    public Func<IReadOnlyCollection<TItem>, bool>? DisableAllRowsSelection { get; set; }

    /// <summary>
    /// Enable or disable the row level checkbox selection.
    /// </summary>
    [Parameter]
    public Func<TItem, bool>? DisableRowSelection { get; set; }

    /// <summary>
    /// Gets or sets the empty data template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    public RenderFragment EmptyDataTemplate { get; set; } = default!;

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
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string FiltersRowCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the filters translation provider.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public GridFiltersTranslationDelegate? FiltersTranslationProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the grid fixed header.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool FixedHeader { get; set; }
 
    /// <summary>
    /// This event is fired when the grid state is changed.
    /// </summary>
    [Parameter]
    public EventCallback<GridSettings> GridSettingsChanged { get; set; }

    /// <summary>
    /// Gets or sets the header row css class but not the thead tag class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
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
    /// Gets or sets the loading template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    public RenderFragment? LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// This event is triggered when the user clicks on the row.
    /// Set AllowRowClick to  <see langword="true" /> to enable row clicking.
    /// </summary>
    [Parameter]
    public EventCallback<GridRowEventArgs<TItem>> OnRowClick { get; set; }

    /// <summary>
    /// This event is triggered when the user double-clicks on the row.
    /// Set AllowRowClick to <see langword="true" /> to enable row double-clicking.
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
    public int[] PageSizeSelectorItems { get; set; } = { 10, 20, 50 };

    /// <summary>
    /// Gets or sets the page size selector visible.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool PageSizeSelectorVisible { get; set; }
    
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
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool IsResponsive { get; set; }

    private string ResponsiveCssClass => IsResponsive ? "table-responsive" : "";

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
    /// <remarks>
    /// Default value is <see cref="GridSelectionMode.Single" />.
    /// </remarks>
    [Parameter]
    public GridSelectionMode SelectionMode { get; set; } = GridSelectionMode.Single;

    /// <summary>
    /// Settings for the grid to render.
    /// The provider should always return an instance of 'GridSettings', and 'null' is not allowed.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public GridSettingsProviderDelegate? SettingsProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the thead css class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? TableHeaderCssClass { get; set; }

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
