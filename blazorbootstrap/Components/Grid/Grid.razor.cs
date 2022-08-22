using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Grid<TItem> : BaseComponent
{
    #region Members

    private List<GridColumn<TItem>> columns = new List<GridColumn<TItem>>();

    private List<TItem> items = null;

    private int? totalCount = null;

    private int totalPages => GetTotalPagesCount();

    private bool requestInProgress = false;

    #endregion Members

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            RefreshDataAsync(); // for now sync call only
            StateHasChanged(); // This is mandatory
        }
    }

    internal void AddColumn(GridColumn<TItem> column)
    {
        columns.Add(column);
        // TODO: call state changed here
    }

    internal void OnFilterChanged(ChangeEventArgs args, GridColumn<TItem> column)
    {
        #region TEMP Section

        var propertyInfo = typeof(TItem).GetProperty(column.PropertyName);
        if (propertyInfo.PropertyType.Name == "Int32")
        {
            column.FilterOperator = FilterOperator.Equals; // TODO: provide an option to select in the UI
        }
        else if (propertyInfo.PropertyType.Name == "String")
        {
            column.FilterOperator = FilterOperator.Contains; // TODO: provide an option to select in the UI
        }

        #endregion TEMP Section

        if (column != null)
            column.FilterValue = args.Value?.ToString();

        if (columns == null || !columns.Any() || !AllowFiltering || !column.Filterable)
            return;

        RefreshDataAsync(); // for now sync call only
        StateHasChanged(); // This is mandatory
    }

    private FilterItem[] GetFilters()
    {
        if (!AllowFiltering || columns == null || !columns.Any())
            return null;

        return columns
                ?.Where(column => column.Filterable && !string.IsNullOrWhiteSpace(column.FilterValue))
                ?.Select(column => new FilterItem(column.PropertyName, column.FilterValue, column.FilterOperator))
                ?.ToArray();
    }

    internal void SortingChanged(GridColumn<TItem> column)
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

                GridCurrentState = new GridState<TItem>(GridCurrentState.PageIndex, c.GetSorting().ToList().AsReadOnly());
            }
            else if (c.ElementId == column.ElementId && c.SortDirection != SortDirection.None)
            {
                GridCurrentState = new GridState<TItem>(GridCurrentState.PageIndex, c.GetSorting().ToList().AsReadOnly());
            }
        });

        RefreshDataAsync(); // for now sync call only
        StateHasChanged(); // This is mandatory
    }

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        GridCurrentState = new GridState<TItem>(newPageNumber, GridCurrentState.Sorting);

        await RefreshDataAsync();
    }

    private SortingItem<TItem>[] GetDefaultSorting()
    {
        if (!AllowSorting || columns == null || !columns.Any())
            return null;

        return columns?
                .Where(column => column.CanSort() && column.IsDefaultSortColumn)?
                .SelectMany(item => item.GetSorting())?
                .ToArray();
    }

    private int GetTotalPagesCount()
    {
        if (totalCount.HasValue && totalCount.Value > 0)
        {
            var q = totalCount.Value / PageSize;
            var r = totalCount.Value % PageSize;

            if (q < 1)
                return 1;

            return q + (r > 0 ? 1 : 0);
        }

        return 1;
    }

    public async Task RefreshDataAsync()
    {
        requestInProgress = true;

        var request = new GridDataProviderRequest<TItem>
        {
            PageNumber = GridCurrentState.PageIndex,
            PageSize = this.PageSize,
            Sorting = GridCurrentState.Sorting ?? GetDefaultSorting(),
            Filters = GetFilters()
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

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets whether end-users can sort data by the column's values.
    /// </summary>
    [Parameter] public bool AllowSorting { get; set; } = true;

    /// <summary>
    /// Specifies the content to be rendered inside the grid.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Shows text on no records.
    /// </summary>
    [Parameter] public string EmptyText { get; set; } = "No records to display.";

    /// <summary>
    /// Template to render when there are no rows to display.
    /// </summary>
    [Parameter] public RenderFragment EmptyDataTemplate { get; set; }

    /// <summary>
    /// DataProvider is for items to render. The provider should always return an instance of 'GridDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter] public GridDataProviderDelegate<TItem> DataProvider { get; set; }

    /// <summary>
    /// Gets or sets the pagination alignment.
    /// </summary>
    [Parameter] public Alignment PaginationAlignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets the page size of the grid.
    /// </summary>
    [Parameter] public int PageSize { get; set; } = 10;

    /// <summary>
    /// Current grid state (page, sorting).
    /// </summary>
    internal GridState<TItem> GridCurrentState { get; set; } = new GridState<TItem>(1, null);

    #endregion Properties
}
