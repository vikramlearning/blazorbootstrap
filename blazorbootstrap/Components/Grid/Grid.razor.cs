namespace BlazorBootstrap;

public partial class Grid<TItem> : BaseComponent
{
    #region Members

    private List<GridColumn<TItem>> columns = new List<GridColumn<TItem>>();

    private List<TItem> items = null;

    private int pageSize;

    private int? totalCount = null;

    private int totalPages => GetTotalPagesCount();

    private bool requestInProgress = false;

    private string responsiveCssClass => this.Responsive ? "table-responsive" : "";

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        this.pageSize = this.PageSize;

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await RefreshDataAsync(firstRender);

        await base.OnAfterRenderAsync(firstRender);
    }

    internal void AddColumn(GridColumn<TItem> column)
    {
        columns.Add(column);
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
        await SaveGridSettingsAsync();
    }

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        GridCurrentState = new GridState<TItem>(newPageNumber, GridCurrentState.Sorting);
        await SaveGridSettingsAsync();
        await RefreshDataAsync();
    }

    internal async Task ResetPageNumberAsync()
    {
        GridCurrentState = new GridState<TItem>(1, GridCurrentState.Sorting);
        await SaveGridSettingsAsync();
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

                GridCurrentState = new GridState<TItem>(GridCurrentState.PageIndex, c.GetSorting());
            }
            else if (c.ElementId == column.ElementId && c.SortDirection != SortDirection.None)
            {
                GridCurrentState = new GridState<TItem>(GridCurrentState.PageIndex, c.GetSorting());
            }
        });

        await SaveGridSettingsAsync();
        await RefreshDataAsync();
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
                GridCurrentState = new GridState<TItem>(settings.PageNumber, GridCurrentState.Sorting);
                this.pageSize = settings.PageSize;
            }
            else
            {
                GridCurrentState = new GridState<TItem>(1, null);
                this.pageSize = 10;
            }
        }
        else
        {
            GridCurrentState = new GridState<TItem>(1, null);
            this.pageSize = 10;
        }

    }

    /// <summary>
    /// Refresh the grid data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync(bool firstRender = false)
    {
        if (requestInProgress)
            return;

        requestInProgress = true;

        if (firstRender)
            await LoadGridSettingsAsync();

        var request = new GridDataProviderRequest<TItem>
        {
            PageNumber = this.AllowPaging ? GridCurrentState.PageIndex : 0,
            PageSize = this.AllowPaging ? this.pageSize : 0,
            Sorting = this.AllowSorting ? (GridCurrentState.Sorting ?? GetDefaultSorting()) : null,
            Filters = this.AllowFiltering ? GetFilters() : null,
            DataSource = this.DataSource
        };

        if (DataProvider != null)
        {
            var result = await DataProvider.Invoke(request);
            if (result != null)
            {
                items = result.Data.ToList();
                totalCount = result.TotalCount ?? result.Data.Count();
            }
            else
            {
                items = new List<TItem> { };
                totalCount = 0;
            }
        }         

        requestInProgress = false;

        await InvokeAsync(StateHasChanged);
    }

    private async Task SaveGridSettingsAsync()
    {
        if (!GridSettingsChanged.HasDelegate)
            return;

        var settings = new GridSettings
        {
            PageNumber = this.AllowPaging ? GridCurrentState.PageIndex : 0,
            PageSize = this.AllowPaging ? this.pageSize : 0,
            Filters = this.AllowFiltering ? GetFilters() : null
        };

        await GridSettingsChanged.InvokeAsync(settings);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the grid filtering.
    /// </summary>
    [Parameter] public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets the grid paging.
    /// </summary>
    [Parameter] public bool AllowPaging { get; set; }

    /// <summary>
    /// Gets or sets the grid sorting.
    /// </summary>
    [Parameter] public bool AllowSorting { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside the grid.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Shows text on no records.
    /// </summary>
    [Parameter] public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Template to render when there are no rows to display.
    /// </summary>
    public RenderFragment EmptyDataTemplate { get; set; } // TODO: support this in the next release

    /// <summary>
    /// DataProvider is for items to render. 
    /// The provider should always return an instance of 'GridDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter, EditorRequired] public GridDataProviderDelegate<TItem> DataProvider { get; set; }

    /// <summary>
    /// DataSource allows you to optionally, directly pass in the data that gets used by the DataProvider.
    /// </summary>
    [Parameter] public IEnumerable<TItem>? DataSource { get; set; } // TODO: rename this to Data

    /// <summary>
    /// Gets or sets the pagination alignment.
    /// </summary>
    [Parameter] public Alignment PaginationAlignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets the page size of the grid.
    /// </summary>
    [Parameter] public int PageSize { get; set; } = 10;

    /// <summary>
    /// Current grid state (filters, paging, sorting).
    /// </summary>
    internal GridState<TItem> GridCurrentState { get; set; } = new GridState<TItem>(1, null);

    /// <summary>
    /// Gets or sets a value indicating whether Grid is responsive.
    /// </summary>
    [Parameter] public bool Responsive { get; set; }

    [Parameter] public EventCallback<GridSettings> GridSettingsChanged { get; set; }

    /// <summary>
    /// Settings is for grid to render. 
    /// The provider should always return an instance of 'GridSettings', and 'null' is not allowed.
    /// </summary>
    [Parameter] public GridSettingsProviderDelegate SettingsProvider { get; set; }

    #endregion Properties
}
