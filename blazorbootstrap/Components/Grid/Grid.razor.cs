namespace BlazorBootstrap;

public partial class Grid<TItem> : BaseComponent
{
    #region Members

    private RenderFragment? headerSelectionTemplate;

    private RenderFragment<TItem>? rowSelectionTemplate;

    /// <summary>
    /// Current grid state (filters, paging, sorting).
    /// </summary>
    internal GridState<TItem> gridCurrentState = new GridState<TItem>(1, null);

    private List<GridColumn<TItem>> columns = new List<GridColumn<TItem>>();

    private List<TItem> items = null;

    private HashSet<TItem> selectedItems = new HashSet<TItem>();

    public int SelectedItemsCount = 0;

    private int pageSize;

    private int? totalCount = null;

    private int totalPages => GetTotalPagesCount();

    private bool requestInProgress = false;

    private string responsiveCssClass => this.Responsive ? "table-responsive" : "";

    private object? lastAssignedDataOrDataProvider;

    private CancellationTokenSource cancellationTokenSource = default!;

    private CheckboxState headerCheckboxState = CheckboxState.Unchecked;

    private string headerCheckboxId = default!;

    private ElementReference headerCheckboxRef;

    private bool isFirstRenderComplete = false;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        headerCheckboxId = IdGenerator.Generate;

        this.pageSize = this.PageSize;

        base.OnInitialized();
    }

    protected override Task OnParametersSetAsync()
    {
        if (isFirstRenderComplete)
        {
            if (Data is null && DataProvider is null)
                throw new InvalidOperationException($"Grid requires one of {nameof(Data)} or {nameof(DataProvider)}, but both were not specified.");

            if (Data is not null && DataProvider is not null)
                throw new InvalidOperationException($"Grid requires one of {nameof(Data)} or {nameof(DataProvider)}, but both were specified.");

            // Perform a re-query only if the data source or something else has changed
            var newDataOrDataProvider = Data; //?? (object?)DataProvider;
            var dataSourceHasChanged = newDataOrDataProvider != lastAssignedDataOrDataProvider;
            if (dataSourceHasChanged)
            {
                lastAssignedDataOrDataProvider = newDataOrDataProvider;
            }

            var mustRefreshData = dataSourceHasChanged && !GridSettingsChanged.HasDelegate;

            // We want to trigger the first data load when we've collected the initial set of columns
            // because they might perform some action, like setting the default sort order. 
            // It would be wasteful to have to re-query immediately.
            return (columns.Count > 0 && mustRefreshData) ? RefreshDataAsync(false, default) : Task.CompletedTask;
        }

        return base.OnParametersSetAsync();
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

    internal void AddColumn(GridColumn<TItem> column)
    {
        columns.Add(column);
    }

    private void OnHeaderCheckboxChange(ChangeEventArgs args)
    {
        Console.WriteLine($"OnHeaderCheckboxChange: args={args.Value}");
    }

    private async Task OnRowCheckboxChange(TItem item, ChangeEventArgs args)
    {
        Console.WriteLine($"CheckboxChange: args={args.Value}");
        if (bool.TryParse(args?.Value?.ToString(), out bool checkboxState) && checkboxState)
            _ = selectedItems.Add(item);
        else
            _ = selectedItems.Remove(item);

        SelectedItemsCount = selectedItems.Count;

        Console.WriteLine($"SelectedItemsCount: {SelectedItemsCount}");

        await SetCheckboxStateAsync();

        //await InvokeAsync(StateHasChanged);

        StateHasChanged();
    }

    private async Task SetCheckboxStateAsync()
    {
        await JS.InvokeVoidAsync("window.blazorBootstrap.checkbox.setState", headerCheckboxRef, (int)CheckboxState.Indeterminate);
    }

    /// <summary>
    /// Get filters.
    /// </summary>
    /// <returns>IEnumerable</returns>
    public IEnumerable<FilterItem> GetFilters()
    {
        if (!AllowFiltering || columns == null || !columns.Any())
            return null;

        return columns
                .Where(column => column.Filterable && column.GetFilterOperator() != FilterOperator.None && !string.IsNullOrWhiteSpace(column.GetFilterValue()))
                ?.Select(column => new FilterItem(column.PropertyName, column.GetFilterValue(), column.GetFilterOperator(), column.StringComparison));
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
                var allowedFilterOperators = FilterOperatorHelper.GetFilterOperators(column.GetPropertyTypeName());
                if (allowedFilterOperators != null && allowedFilterOperators.Any(x => x.FilterOperator == item.Operator))
                {
                    column.SetFilterOperator(item.Operator);
                    column.SetFilterValue(item.Value);
                }
            }
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

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        gridCurrentState = new GridState<TItem>(newPageNumber, gridCurrentState.Sorting);
        await SaveGridSettingsAsync();
        await RefreshDataAsync(false, default);
    }

    /// <summary>
    /// Reset the page number to 1 and refresh the grid.
    /// </summary>
    public async ValueTask ResetPageNumber()
    {
        await ResetPageNumberAsync(true);
    }

    internal async ValueTask ResetPageNumberAsync(bool refreshGrid = false)
    {
        gridCurrentState = new GridState<TItem>(1, gridCurrentState.Sorting);

        if (refreshGrid)
            await RefreshDataAsync(false, default);
    }

    internal async Task SortingChangedAsync(GridColumn<TItem> column)
    {
        if (columns == null || !columns.Any())
            return;

        // check sorting enabled on any of the columns
        var sortedColumn = columns.FirstOrDefault(c => c.currentSortDirection != SortDirection.None);

        // reset other columns sorting
        columns.ForEach(c =>
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
                    c.currentSortDirection = (c.defaultSortDirection != SortDirection.None) ? c.defaultSortDirection : SortDirection.Ascending;

                gridCurrentState = new GridState<TItem>(gridCurrentState.PageIndex, c.GetSorting());
            }
            else if (c.ElementId == column.ElementId && c.SortDirection != SortDirection.None)
            {
                gridCurrentState = new GridState<TItem>(gridCurrentState.PageIndex, c.GetSorting());
            }
        });

        await SaveGridSettingsAsync();
        await RefreshDataAsync(false, default);
    }

    private IEnumerable<SortingItem<TItem>> GetDefaultSorting()
    {
        if (!AllowSorting || columns == null || !columns.Any())
            return null;

        return columns?
                .Where(column => column.CanSort() && column.IsDefaultSortColumn)?
                .SelectMany(item => item.GetSorting());
    }

    private int GetTotalPagesCount()
    {
        if (totalCount.HasValue && totalCount.Value > 0)
        {
            var q = totalCount.Value / pageSize;
            var r = totalCount.Value % pageSize;

            if (q < 1)
                return 1;

            return q + (r > 0 ? 1 : 0);
        }

        return 1;
    }

    private async Task LoadGridSettingsAsync()
    {
        if (this.SettingsProvider is null)
            return;

        var settings = await this.SettingsProvider.Invoke();
        if (settings is null)
            return;

        if (settings.Filters is not null && settings.Filters.Any())
            SetFilters(settings.Filters);

        if (settings.PageNumber > 0)
        {
            if (settings.PageSize > 0 && settings.PageNumber < settings.PageSize)
            {
                gridCurrentState = new GridState<TItem>(settings.PageNumber, gridCurrentState.Sorting);
                this.pageSize = settings.PageSize;
            }
            else
            {
                gridCurrentState = new GridState<TItem>(1, null);
                this.pageSize = 10;
            }
        }
        else
        {
            gridCurrentState = new GridState<TItem>(1, null);
            this.pageSize = 10;
        }

    }

    /// <summary>
    /// Refresh the grid data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync(CancellationToken cancellationToken = default)
    {
        await RefreshDataAsync(false, cancellationToken);
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
            PageNumber = this.AllowPaging ? gridCurrentState.PageIndex : 0,
            PageSize = this.AllowPaging ? this.pageSize : 0,
            Sorting = this.AllowSorting ? (gridCurrentState.Sorting ?? GetDefaultSorting()) : null,
            Filters = this.AllowFiltering ? GetFilters() : null,
            CancellationToken = cancellationToken
        };

        GridDataProviderResult<TItem> result = default!;

        if (DataProvider is not null)
            result = await DataProvider.Invoke(request);
        else if (Data is not null)
            result = request.ApplyTo(Data);

        if (result is not null)
        {
            items = result.Data.ToList();
            totalCount = result.TotalCount ?? result.Data.Count();
        }
        else
        {
            items = new List<TItem> { };
            totalCount = 0;
        }

        requestInProgress = false;

        await InvokeAsync(StateHasChanged);
    }

    private Task SaveGridSettingsAsync()
    {
        if (!GridSettingsChanged.HasDelegate)
            return Task.CompletedTask;

        var settings = new GridSettings
        {
            PageNumber = this.AllowPaging ? gridCurrentState.PageIndex : 0,
            PageSize = this.AllowPaging ? this.pageSize : 0,
            Filters = this.AllowFiltering ? GetFilters() : null
        };

        return GridSettingsChanged.InvokeAsync(settings);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public int AllowDelete { get; set; }

    [Parameter] public int AllowEdit { get; set; }

    /// <summary>
    /// Gets or sets the grid filtering.
    /// </summary>
    [Parameter] public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets the grid paging.
    /// </summary>
    [Parameter] public bool AllowPaging { get; set; }

    [Parameter] public bool AllowSelection { get; set; }

    /// <summary>
    /// Gets or sets the grid sorting.
    /// </summary>
    [Parameter] public bool AllowSorting { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside the grid.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the grid data.
    /// </summary>
    [Parameter] public IEnumerable<TItem> Data { get; set; } = default!;

    /// <summary>
    /// DataProvider is for items to render. 
    /// The provider should always return an instance of 'GridDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter] public GridDataProviderDelegate<TItem> DataProvider { get; set; } = default!;

    /// <summary>
    /// Shows text on no records.
    /// </summary>
    [Parameter] public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Template to render when there are no rows to display.
    /// </summary>
    public RenderFragment EmptyDataTemplate { get; set; } = default!;

    /// <summary>
    /// This event is fired when the grid state is changed.
    /// </summary>
    [Parameter] public EventCallback<GridSettings> GridSettingsChanged { get; set; }

    /// <summary>
    /// Gets or sets the pagination alignment.
    /// </summary>
    [Parameter] public Alignment PaginationAlignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets the page size of the grid.
    /// </summary>
    [Parameter] public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether Grid is responsive.
    /// </summary>
    [Parameter] public bool Responsive { get; set; }

    /// <summary>
    /// Gets or sets the row class.
    /// </summary>
    [Parameter] public Func<TItem, string>? RowClass { get; set; }

    /// <summary>
    /// Settings is for grid to render. 
    /// The provider should always return an instance of 'GridSettings', and 'null' is not allowed.
    /// </summary>
    [Parameter] public GridSettingsProviderDelegate SettingsProvider { get; set; } = default!;

    /// <summary>
    /// Header selection template.
    /// </summary>
    internal RenderFragment HeaderSelectionTemplate
    {
        get
        {
            return headerSelectionTemplate ??= (builder =>
            {
                // th "style" > div "class" > input
                var seq = 0;
                builder.OpenElement(seq, "th");
                seq++;
                builder.AddAttribute(seq, "style", "width: 2rem;");

                seq++;
                builder.OpenElement(seq, "div");
                seq++;
                builder.AddAttribute(seq, "class", "form-check");

                seq++;
                builder.OpenElement(seq, "input");
                //seq++;
                //builder.AddAttribute(seq, "ref", "headerCheckboxRef");
                seq++;
                builder.AddAttribute(seq, "class", "form-check-input");
                seq++;
                builder.AddAttribute(seq, "type", "checkbox");
                seq++;
                builder.AddAttribute(seq, "role", "button");
                seq++;
                builder.AddAttribute(seq, "onchange", OnHeaderCheckboxChange);

                builder.CloseElement(); // close: input

                builder.CloseElement(); // close: div

                builder.CloseElement(); // close: th
            });
        }
    }

    [Parameter] public Func<TItem, bool>? DisableHeaderSelection { get; set; }

    /// <summary>
    /// Row selection template.
    /// </summary>
    internal RenderFragment<TItem> RowSelectionTemplate
    {
        get
        {
            return rowSelectionTemplate ??= (rowData => builder =>
            {
                // td > div "class" > input
                var seq = 0;
                builder.OpenElement(seq, "td");

                seq++;
                builder.OpenElement(seq, "div");
                seq++;
                builder.AddAttribute(seq, "class", "form-check");

                seq++;
                builder.OpenElement(seq, "input");
                seq++;
                builder.AddAttribute(seq, "class", "form-check-input");
                seq++;
                builder.AddAttribute(seq, "type", "checkbox");
                seq++;
                builder.AddAttribute(seq, "role", "button");
                seq++;

                // disable the checkbox
                // remove the onchange event binding
                // add disabled attribute
                if (DisableRowSelection?.Invoke(rowData) ?? true)
                    builder.AddAttribute(seq, "disabled", "disabled");
                else
                    builder.AddAttribute(seq, "onchange", async (ChangeEventArgs args) => await OnRowCheckboxChange(rowData, args));

                builder.CloseElement(); // close: input
                builder.CloseElement(); // close: div
                builder.CloseElement(); // close: th
            });
        }
    }

    [Parameter] public Func<TItem, bool>? DisableRowSelection { get; set; }

    #endregion Properties
}
