using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap.Components;

public partial class GridColumn<TItem> : BaseComponent
{
    #region Members

    private RenderFragment headerTemplate;

    private RenderFragment<TItem> cellTemplate;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        Parent.AddColumn(this);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public Grid<TItem> Parent { get; set; }

    [Parameter] public string Title { get; set; }

    [Parameter] public SortDirection SortDirection { get; set; } = SortDirection.None;

    [Parameter] public RenderFragment<TItem> ChildContent { get; set; }

    internal RenderFragment HeaderTemplate
    {
        get
        {
            return headerTemplate ??= (builder =>
            {
                // th > span "title" , span > i "icon"
                builder.OpenElement(0, "th");
                builder.OpenElement(1, "span");
                builder.AddAttribute(2, "class", "me-2");
                builder.AddContent(3, Title);
                builder.CloseElement(); // close: span
                builder.OpenElement(4, "span");
                builder.OpenElement(5, "i");
                builder.AddAttribute(6, "class", "bi bi-sort-alpha-down");
                builder.CloseElement(); // close: i
                builder.CloseElement(); // close: span
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
                builder.OpenElement(0, "td");
                builder.AddContent(1, ChildContent, rowData);
                builder.CloseElement();
            });
        }
    }

    #endregion Properties
}

