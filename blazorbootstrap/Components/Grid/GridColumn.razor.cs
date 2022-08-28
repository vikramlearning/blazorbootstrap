using Microsoft.AspNetCore.Components;
using System.Data.Common;
using System.Linq.Expressions;

namespace BlazorBootstrap;

public partial class GridColumn<TItem> : BaseComponent
{
    #region Members

    private RenderFragment headerTemplate;

    private RenderFragment<TItem> cellTemplate;

    private FilterOperator filterOperator;

    private string filterValue;

    internal SortDirection currentSortDirection;

    internal SortDirection defaultSortDirection;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        ElementId = IdGenerator.Generate; // Required
        currentSortDirection = SortDirection;
        defaultSortDirection = SortDirection;
        filterOperator = FilterOperator;
        filterValue = FilterValue;

        if (IsDefaultSortColumn && SortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        Parent.AddColumn(this);

        SetDefaultFilter(this.filterOperator, this.GetPropertyTypeName());
    }

    internal bool CanSort() => Parent.AllowSorting && this.Sortable && this.SortKeySelector != null;

    internal IEnumerable<SortingItem<TItem>> GetSorting()
    {
        if (SortKeySelector == null && string.IsNullOrWhiteSpace(this.SortString))
            yield break;

        yield return new SortingItem<TItem>(this.SortString, this.SortKeySelector, this.currentSortDirection);
    }

    private void OnSortClick()
    {
        // toggle the direction
        if (currentSortDirection == SortDirection.Ascending)
            currentSortDirection = SortDirection = SortDirection.Descending;
        else if (currentSortDirection == SortDirection.Descending)
            currentSortDirection = SortDirection = SortDirection.None;
        else if (currentSortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        Parent.SortingChanged(this);
    }

    private void SetDefaultFilter(FilterOperator columnFilterOperator, string propertyTypeName)
    {
        if (propertyTypeName == StringConstants.PropertyTypeNameInt16
            || propertyTypeName == StringConstants.PropertyTypeNameInt32
            || propertyTypeName == StringConstants.PropertyTypeNameInt64
            || propertyTypeName == StringConstants.PropertyTypeNameSingle // float
            || propertyTypeName == StringConstants.PropertyTypeNameDecimal
            || propertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            this.filterOperator = this.filterOperator == FilterOperator.None ? FilterOperator.Equals : this.filterOperator;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameString
            || propertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            this.filterOperator = this.filterOperator == FilterOperator.None ? FilterOperator.Contains : this.filterOperator;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            this.filterOperator = this.filterOperator == FilterOperator.None ? FilterOperator.Equals : this.filterOperator;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            this.filterOperator = this.filterOperator == FilterOperator.None ? FilterOperator.Equals : this.filterOperator;
        }
    }

    internal void OnFilterChanged(FilterEventArgs args, GridColumn<TItem> column)
    {
        this.filterValue = args.Text;
        this.filterOperator = args.FilterOperator;

        this.Parent.RefreshDataAsync();
        //StateHasChanged();
    }

    internal string GetPropertyTypeName()
    {
        if (string.IsNullOrWhiteSpace(this.PropertyName))
            return string.Empty;

        return typeof(TItem).GetProperty(this.PropertyName)?.PropertyType?.Name;
    }

    internal string GetFilterValue() => this.filterValue;

    internal FilterOperator GetFilterOperator() => this.filterOperator;

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public Grid<TItem> Parent { get; set; }

    /// <summary>
    /// Gets or sets the table column header.
    /// </summary>
    [Parameter] public string HeaderText { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// This is required when `AllowFiltering` is true.
    /// </summary>
    [Parameter] public string PropertyName { get; set; }

    [Parameter] public bool Filterable { get; set; } = true;

    [Parameter] public FilterOperator FilterOperator { get; set; }

    [Parameter] public string FilterValue { get; set; }

    /// <summary>
    /// Enable or disble sorting on specific column.
    /// By default sorting enabled on all columns.
    /// </summary>
    [Parameter] public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the column sort string. 
    /// This value will be passed to the backend/API for sorting. 
    /// And this property is ignored for the client-side sorting.
    /// </summary>
    [Parameter] public string SortString { get; set; }

    /// <summary>
    /// Expression used for sorting.
    /// </summary>
    [Parameter] public Expression<Func<TItem, IComparable>> SortKeySelector { get; set; }

    /// <summary>
    /// Gets or sets the default sort direction of a column.
    /// </summary>
    [Parameter] public SortDirection SortDirection { get; set; } = SortDirection.None;

    /// <summary>
    /// Gets or sets the default sort column.
    /// </summary>
    [Parameter] public bool IsDefaultSortColumn { get; set; } = false;

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    [Parameter] public RenderFragment<TItem> ChildContent { get; set; }

    internal RenderFragment HeaderTemplate
    {
        get
        {
            return headerTemplate ??= (builder =>
            {
                // th > span "title", span > i "icon"
                var seq = 0;
                builder.OpenElement(seq, "th");
                if (this.CanSort())
                {
                    seq++;
                    builder.AddAttribute(seq, "role", "button");
                    seq++;
                    builder.AddAttribute(seq, "onclick", OnSortClick);
                }
                if (this.HeaderTextAlignment != Alignment.None)
                {
                    seq++;
                    builder.AddAttribute(seq, "class", BootstrapClassProvider.TextAlignment(this.TextAlignment));
                }
                seq++;
                builder.OpenElement(seq, "span");
                seq++;
                builder.AddAttribute(seq, "class", "me-2");
                seq++;
                builder.AddContent(seq, HeaderText);
                seq++;
                builder.CloseElement(); // close: span

                if (this.CanSort() && currentSortDirection != SortDirection.None)
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
                if (this.TextAlignment != Alignment.None)
                {
                    seq++;
                    builder.AddAttribute(seq, "class", BootstrapClassProvider.TextAlignment(this.TextAlignment));
                }
                if (this.TextNoWrap)
                {
                    seq++;
                    builder.AddAttribute(seq, "class", BootstrapClassProvider.TextNoWrap());
                }
                seq++;
                builder.AddContent(seq, ChildContent, rowData);
                builder.CloseElement();
            });
        }
    }

    /// <summary>
    /// Gets or sets the header text alignment.
    /// </summary>
    [Parameter] public Alignment HeaderTextAlignment { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    [Parameter] public Alignment TextAlignment { get; set; }

    /// <summary>
    /// Gets or sets text nowrap.
    /// </summary>
    [Parameter] public bool TextNoWrap { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width in pixels.
    /// </summary>
    [Parameter] public int FilterTextboxWidth { get; set; }

    #endregion Properties
}

