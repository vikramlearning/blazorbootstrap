using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazorBootstrap.Components;

public partial class GridColumn<TItem> : BaseComponent
{
    #region Members

    private RenderFragment headerTemplate;

    private RenderFragment<TItem> cellTemplate;

    private SortDirection currentSortDirection;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        Parent.AddColumn(this);
    }

    private void OnSortClick()
    {
        if (currentSortDirection == SortDirection.Ascending)
            currentSortDirection = SortDirection = SortDirection.Descending;
        else if (currentSortDirection == SortDirection.Descending)
            currentSortDirection = SortDirection = SortDirection.Ascending;
        else if (currentSortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        Parent.HandleSort(SortKeySelector, SortDirection);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public Grid<TItem> Parent { get; set; }

    [Parameter] public string Title { get; set; }

    [Parameter] public Expression<Func<TItem, IComparable>> SortKeySelector { get; set; }

    [Parameter] public SortDirection SortDirection { get; set; } = SortDirection.None;

    [Parameter] public RenderFragment<TItem> ChildContent { get; set; }

    internal RenderFragment HeaderTemplate
    {
        get
        {
            return headerTemplate ??= (builder =>
            {
                // th > span "title" , span > i "icon"
                var seq = 0;
                builder.OpenElement(seq, "th");
                seq++;
                builder.AddAttribute(seq, "role", "button");
                seq++;
                builder.AddAttribute(seq, "onclick", OnSortClick);
                seq++;
                builder.OpenElement(seq, "span");
                seq++;
                builder.AddAttribute(seq, "class", "me-2");
                seq++;
                builder.AddContent(seq, Title);
                seq++;
                builder.CloseElement(); // close: span
                if (SortDirection != SortDirection.None)
                {
                    seq++;
                    builder.OpenElement(seq, "span");
                    seq++;
                    builder.OpenElement(seq, "i");
                    seq++;
                    var sortIcon = (SortDirection == SortDirection.Ascending) ? "bi bi-sort-alpha-down" : "bi bi-sort-alpha-down-alt"; // TODO: Add Parameter for this
                    builder.AddAttribute(seq, "class", sortIcon);
                    builder.CloseElement(); // close: i
                    builder.CloseElement(); // close: span
                }
                builder.CloseElement(); // close: th
            });
        }
    }

    internal RenderFragment<TItem> CellTemplate
    {
        get
        {
            return cellTemplate ??= (rowData => builder =>
            {
                var seq = 0;
                builder.OpenElement(seq, "td");
                seq++;
                builder.AddAttribute(seq, "role", "button");
                seq++;
                builder.AddContent(seq, ChildContent, rowData);
                builder.CloseElement();
            });
        }
    }

    #endregion Properties
}

