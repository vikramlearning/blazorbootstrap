namespace BlazorBootstrap;

public partial class GridColumn<TItem>
{
    #region Fields and Constants

    private RenderFragment<TItem>? cellTemplate;

    internal SortDirection currentSortDirection;

    internal SortDirection defaultSortDirection;

    private FilterOperator filterOperator;

    private string filterValue = default!;

    private RenderFragment? headerTemplate;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        ElementId = IdGenerator.GetNextId(); // Required

        filterOperator = FilterOperator;
        filterValue = FilterValue;

        currentSortDirection = SortDirection;
        defaultSortDirection = SortDirection;

        if (IsDefaultSortColumn && SortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        Parent.AddColumn(this);

        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet() => SetDefaultFilter();

    internal bool CanSort() => Parent is not null && Parent.AllowSorting && Sortable && SortKeySelector is not null;

    internal FilterOperator GetFilterOperator() => filterOperator;

    internal string GetFilterValue() => filterValue;

    internal string GetPropertyTypeName() => typeof(TItem).GetPropertyTypeName(PropertyName);

    internal IEnumerable<SortingItem<TItem>> GetSorting()
    {
        if (SortKeySelector == null && string.IsNullOrWhiteSpace(SortString))
            yield break;

        yield return new SortingItem<TItem>(SortString, SortKeySelector!, currentSortDirection);
    }

    internal async Task OnFilterChangedAsync(FilterEventArgs args, GridColumn<TItem> column)
    {
        if (filterValue != args.Text || filterOperator != args.FilterOperator)
            await Parent.ResetPageNumberAsync();

        filterValue = args.Text;
        filterOperator = args.FilterOperator;
        await Parent.FilterChangedAsync();
    }

    internal void SetDefaultFilter()
    {
        var propertyTypeName = GetPropertyTypeName();

        if (propertyTypeName is StringConstants.PropertyTypeNameInt16
                                or StringConstants.PropertyTypeNameInt32
                                or StringConstants.PropertyTypeNameInt64
                                or StringConstants.PropertyTypeNameSingle // float
                                or StringConstants.PropertyTypeNameDecimal
                                or StringConstants.PropertyTypeNameDouble)
        {
            if (filterOperator == FilterOperator.None)
                FilterOperator = filterOperator = FilterOperator.Equals;
        }
        else if (propertyTypeName is StringConstants.PropertyTypeNameString
                                     or StringConstants.PropertyTypeNameChar)
        {
            if (filterOperator == FilterOperator.None)
                FilterOperator = filterOperator = FilterOperator.Contains;
        }
        else if (propertyTypeName is StringConstants.PropertyTypeNameDateOnly
                                     or StringConstants.PropertyTypeNameDateTime)
        {
            if (filterOperator == FilterOperator.None)
                FilterOperator = filterOperator = FilterOperator.Equals;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            if (filterOperator == FilterOperator.None)
                FilterOperator = filterOperator = FilterOperator.Equals;
        }
    }

    internal void SetFilterOperator(FilterOperator filterOperator) => FilterOperator = this.filterOperator = filterOperator;

    internal void SetFilterValue(string filterValue) => FilterValue = this.filterValue = filterValue;

    private async Task OnSortClickAsync()
    {
        // toggle the direction
        if (currentSortDirection == SortDirection.Ascending)
            currentSortDirection = SortDirection = SortDirection.Descending;
        else if (currentSortDirection == SortDirection.Descending)
            currentSortDirection = SortDirection = SortDirection.None;
        else if (currentSortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        await Parent.SortingChangedAsync(this);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Cell template.
    /// </summary>
    internal RenderFragment<TItem> CellTemplate =>
        cellTemplate ??= rowData => builder =>
                                    {
                                        builder.OpenElement(100, "td");

                                        var classList = new List<string>();

                                        // text alignment
                                        if (TextAlignment != Alignment.None)
                                            classList.Add(BootstrapClassProvider.TextAlignment(TextAlignment));

                                        // text nowrap
                                        if (TextNoWrap)
                                            classList.Add(BootstrapClassProvider.TextNoWrap());

                                        // custom column class
                                        var columnClass = ColumnClass?.Invoke(rowData) ?? "";

                                        if (!string.IsNullOrWhiteSpace(columnClass))
                                            classList.Add(columnClass);

                                        if (classList.Any())
                                            builder.AddAttribute(101, "class", string.Join(" ", classList));

                                        if (Freeze)
                                        {
                                            var styleList = new List<string>();
                                            styleList.Add($"position:sticky");
                                            if (FreezeDirection == FreezeDirection.Left)
                                            {
                                                styleList.Add($"left:{FreezeLeft.ToString(CultureInfo.InvariantCulture)}{Parent.Unit}");
                                            }
                                            else
                                            {
                                                styleList.Add($"right:{FreezeRight.ToString(CultureInfo.InvariantCulture)}{Parent.Unit}");
                                            }
                                            styleList.Add($"background-color:var(--bb-table-freeze-column-background-color)");
                                            builder.AddAttribute(102, "style", string.Join(";", styleList));
                                        }

                                        builder.AddContent(103, ChildContent, rowData);
                                        builder.CloseElement();
                                    };

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem> ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the column class.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? ColumnClass { get; set; }

    /// <summary>
    /// Enable or disable the filter on a specific column.
    /// The filter is enabled or disabled based on the grid `AllowFiltering` parameter.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Gets or sets the filter operator.
    /// </summary>
    [Parameter]
    public FilterOperator FilterOperator { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width in pixels.
    /// </summary>
    [Parameter]
    public int FilterTextboxWidth { get; set; }

    /// <summary>
    /// Gets or sets the filter value.
    /// </summary>
    [Parameter]
    public string FilterValue { get; set; } = default!;

    [Parameter]
    public bool Freeze { get; set; }

    [Parameter]
    public FreezeDirection FreezeDirection { get; set; }

    /// <summary>
    /// Gets or sets the horizontal position of a column. It has no effect on non-positioned columns.
    /// </summary>
    [Parameter]
    public double FreezeLeft { get; set; }

    /// <summary>
    /// Gets or sets the horizontal position of a column. It has no effect on non-positioned columns.
    /// </summary>
    [Parameter]
    public double FreezeRight { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside the grid column header.
    /// </summary>
    [Parameter]
    public RenderFragment HeaderContent { get; set; } = default!;

    /// <summary>
    /// Header template.
    /// </summary>
    internal RenderFragment HeaderTemplate =>
        headerTemplate ??= builder =>
                           {
                               // th > span "title", span > i "icon"
                               builder.OpenElement(101, "th");

                               var classList = new List<string>();

                               if (HeaderContent is null && HeaderTextAlignment != Alignment.None)
                                   classList.Add(BootstrapClassProvider.TextAlignment(HeaderTextAlignment));

                               if (Freeze)
                               {
                                   var styleList = new List<string>();
                                   styleList.Add($"position:sticky");
                                   if (FreezeDirection == FreezeDirection.Left)
                                   {
                                       styleList.Add($"left:{FreezeLeft.ToString(CultureInfo.InvariantCulture)}{Parent.Unit}");
                                   }
                                   else
                                   {
                                       styleList.Add($"right:{FreezeRight.ToString(CultureInfo.InvariantCulture)}{Parent.Unit}");
                                   }
                                   styleList.Add($"background-color:var(--bb-table-freeze-column-background-color)");
                                   builder.AddAttribute(112, "style", string.Join(";", styleList));

                                   classList.Add("freeze-column");
                                   //classList.Add("table-active");
                               }

                               builder.AddAttribute(113, "class", string.Join(" ", classList));

                               if (HeaderContent is null)
                               {
                                   if (CanSort())
                                   {
                                       builder.AddAttribute(102, "role", "button");
                                       builder.AddAttribute(103, "onclick", async () => await OnSortClickAsync());
                                   }

                                   builder.OpenElement(105, "span"); // open: span
                                   builder.AddAttribute(106, "class", "me-2");
                                   builder.AddContent(107, HeaderText);
                                   builder.CloseElement(); // close: span

                                   if (CanSort())
                                   {
                                       builder.OpenElement(108, "span"); // open: span
                                       builder.OpenElement(109, "i"); // open: i

                                       var sortIcon = "bi bi-arrow-down-up"; // default icon

                                       if (currentSortDirection is not SortDirection.None and SortDirection.Ascending)
                                           sortIcon = "bi bi-sort-alpha-down";
                                       else if (currentSortDirection is not SortDirection.None and SortDirection.Descending)
                                           sortIcon = "bi bi-sort-alpha-down-alt";

                                       builder.AddAttribute(110, "class", sortIcon);
                                       builder.CloseElement(); // close: i
                                       builder.CloseElement(); // close: span
                                   }
                               }
                               else
                               {
                                   // If headercontent is used, filters and sorting wont be added.
                                   builder.AddContent(111, HeaderContent);
                               }

                               builder.CloseElement(); // close: th
                           };

    /// <summary>
    /// Gets or sets the table column header.
    /// </summary>
    [Parameter]
    public string HeaderText { get; set; } = default!;

    /// <summary>
    /// Gets or sets the header text alignment.
    /// </summary>
    [Parameter]
    public Alignment HeaderTextAlignment { get; set; }

    /// <summary>
    /// Gets or sets the default sort column.
    /// </summary>
    [Parameter]
    public bool IsDefaultSortColumn { get; set; } = false;

    [CascadingParameter]
    public Grid<TItem> Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the property name.
    /// This is required when `AllowFiltering` is true.
    /// </summary>
    [Parameter]
    public string PropertyName { get; set; } = default!;

    /// <summary>
    /// Enable or disable the sorting on a specific column.
    /// The sorting is enabled or disabled based on the `AllowSorting` parameter on the grid.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the default sort direction of a column.
    /// </summary>
    [Parameter]
    public SortDirection SortDirection { get; set; } = SortDirection.None;

    /// <summary>
    /// Expression used for sorting.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, IComparable>> SortKeySelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the column sort string.
    /// This value will be passed to the backend/API for sorting.
    /// And this property is ignored for the client-side sorting.
    /// </summary>
    [Parameter]
    public string SortString { get; set; } = default!;

    /// <summary>
    /// Gets or sets the StringComparison.
    /// </summary>
    [Parameter]
    public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    [Parameter]
    public Alignment TextAlignment { get; set; }

    /// <summary>
    /// Gets or sets text nowrap.
    /// </summary>
    [Parameter]
    public bool TextNoWrap { get; set; }

    #endregion
}
