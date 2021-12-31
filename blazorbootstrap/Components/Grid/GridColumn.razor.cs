﻿using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazorBootstrap;

public partial class GridColumn<TItem> : BaseComponent
{
    #region Members

    private RenderFragment headerTemplate;

    private RenderFragment<TItem> cellTemplate;

    internal SortDirection currentSortDirection;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        ElementId = IdGenerator.Generate;
        currentSortDirection = SortDirection;
        Parent.AddColumn(this);
    }

    internal bool CanSort() => Parent.Sortable && SortKeySelector != null;

    internal IEnumerable<SortingItem<TItem>> GetSorting()
    {
        if(SortKeySelector == null)
            yield break;

        yield return new SortingItem<TItem>(this.SortKeySelector, this.currentSortDirection);
    }

    private void OnSortClick()
    {
        if (currentSortDirection == SortDirection.Ascending)
            currentSortDirection = SortDirection = SortDirection.Descending;
        else if (currentSortDirection == SortDirection.Descending)
            currentSortDirection = SortDirection = SortDirection.None;
        else if (currentSortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        Parent.SortingChanged(this);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public Grid<TItem> Parent { get; set; }

    [Parameter] public string HeaderText { get; set; }

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
                builder.AddContent(seq, HeaderText);
                seq++;
                builder.CloseElement(); // close: span

                if (Parent.Sortable && SortDirection != SortDirection.None)
                {
                    seq++;
                    builder.OpenElement(seq, "span");
                    seq++;
                    builder.OpenElement(seq, "i");
                    seq++;

                    var sortIcon = ""; // TODO: Add Parameter for this
                    if (currentSortDirection == SortDirection.Ascending)
                        sortIcon = "bi bi-sort-alpha-down";
                    else if (currentSortDirection == SortDirection.Descending)
                        sortIcon = "bi bi-sort-alpha-down-alt";

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
