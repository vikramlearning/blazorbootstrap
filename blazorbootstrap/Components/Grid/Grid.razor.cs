using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Grid<TItem> : BaseComponent
{
    #region Members

    private List<GridColumn<TItem>> columns = new List<GridColumn<TItem>>();

    private List<TItem> items = null;

    private int? totalCount = null;

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
                c.currentSortDirection = c.defaultSortDirection != SortDirection.None ? c.defaultSortDirection : SortDirection.Ascending;
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

    private SortingItem<TItem>[] GetDefaultSorting()
    {
        if (columns == null || !columns.Any())
            return null;

        return columns?
                .Where(item => item.IsDefaultSortColumn)?
                .SelectMany(item => item.GetSorting())?
                .ToArray();
    }

    private async Task RefreshDataAsync()
    {
        var request = new GridDataProviderRequest<TItem>
        {
            Sorting = GridCurrentState.Sorting ?? GetDefaultSorting()
        };

        if (DataProvider != null)
        {
            var result = await DataProvider.Invoke(request);
            if (result != null)
            {
                items = request.ApplyTo(result.Data).Data.ToList();
                totalCount = result.TotalCount ?? result.Data.Count();
            }
            else
            {
                items = new List<TItem> { };
                totalCount = 0;
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets whether end-users can sort data by the column's values.
    /// </summary>
    [Parameter] public bool Sortable { get; set; } = true;

    [Parameter] public RenderFragment ChildContent { get; set; }

    [Parameter] public RenderFragment EmptyDataTemplate { get; set; }

    /// <summary>
    /// Data provider for items to render as a table.
    /// </summary>
    [Parameter] public GridDataProviderDelegate<TItem> DataProvider { get; set; }

    /// <summary>
    /// Current grid state (page, sorting).
    /// </summary>
    [Parameter] public GridState<TItem> GridCurrentState { get; set; } = new GridState<TItem>(0, null);

    /// <summary>
    /// Event fires when grid state is changed.
    /// </summary>
    //[Parameter] public EventCallback<GridState<TItem>> GridCurrentStateChanged { get; set; }

    #endregion Properties
}
