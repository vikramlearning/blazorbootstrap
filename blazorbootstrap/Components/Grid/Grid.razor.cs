using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap.Components;

public partial class Grid<TItem> : BaseComponent
{
    #region Members

    private List<GridColumn<TItem>> columns = new List<GridColumn<TItem>>();

    #endregion Members

    #region Methods

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            StateHasChanged();
    }

    internal void AddColumn(GridColumn<TItem> column)
    {
        columns.Add(column);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public List<TItem> Items { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }

    #endregion Properties
}
