using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap.Components;

public partial class Grid<TItem> : BaseComponent
{
    #region Members

    private List<GridColumn<TItem>> columns = new List<GridColumn<TItem>>();

    #endregion Members

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            var request = new GridDataProviderRequest<TItem>
            {
                Sorting = GridCurrentState.Sorting ?? GetDefaultSorting()
            };

            if (Items == null || !Items.Any())
            {
                RefreshDataAsync(request);
            }
            else if(request.Sorting != null && request.Sorting.Any())
            {
                Items = request.ApplyTo(Items).Data.ToList();
            }

            StateHasChanged();
        }
    }

    internal void AddColumn(GridColumn<TItem> column)
    {
        columns.Add(column);
    }

    internal void SortingChanged(GridColumn<TItem> column)
    {
        // TODO: refactor this method
        if (Items == null)
            return;

        IOrderedEnumerable<TItem> orderedItems =
            (column.currentSortDirection == SortDirection.Ascending)
            ? Items.OrderBy(column.SortKeySelector.Compile())
            : Items.OrderByDescending(column.SortKeySelector.Compile());

        Items = orderedItems.ToList();

        // Reset other columns sorting
        columns.ForEach(c =>
        {
            if (c.ElementId != column.ElementId)
                c.currentSortDirection = SortDirection.None;
        });

        StateHasChanged();
    }

    private SortingItem<TItem>[] GetDefaultSorting()
    {
        if (columns == null || !columns.Any())
            return null;

        return columns?
                .Where(item => item.currentSortDirection != SortDirection.None)?
                .SelectMany(item => item.GetSorting())?
                .ToArray();
    }

    private async Task RefreshDataAsync(GridDataProviderRequest<TItem> request)
    {
        if (DataProvider != null)
        {
            var result = await DataProvider.Invoke(request);
            Items = request.ApplyTo(result.Data).Data.ToList();
        }
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets whether end-users can sort data by the column's values.
    /// </summary>
    [Parameter] public bool Sortable { get; set; } = true;

    [Parameter] public List<TItem> Items { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }

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
