﻿namespace BlazorBootstrap;

public partial class GridColumn<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private RenderFragment<TItem>? cellTemplate;

    internal SortDirection currentSortDirection;

    internal SortDirection defaultSortDirection;

    private FilterOperator filterOperator;

    private string filterValue = default!;

    private bool isVisible = true;

    private RenderFragment? headerTemplate;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required

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

    internal Type GetPropertyType() => typeof(TItem).GetPropertyType(PropertyName)!;

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
        else if (propertyTypeName == StringConstants.PropertyTypeNameEnum)
        {
            if (filterOperator == FilterOperator.None)
                FilterOperator = filterOperator = FilterOperator.Equals;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameGuid)
        {
            if (filterOperator == FilterOperator.None)
                FilterOperator = filterOperator = FilterOperator.Equals;
        }
        if (isVisible != IsVisible)
        {
            isVisible = IsVisible;
            Parent?.ColumnVisibilityUpdated();
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

    /// <summary>
    /// Cell template.
    /// </summary>
    internal RenderFragment<TItem> CellTemplate =>
        cellTemplate ??= rowData => builder =>
                                    {
                                        builder.OpenElement(100, "td");

                                        var classList = new List<string>();

                                        // default class names
                                        if (!string.IsNullOrWhiteSpace(ClassNames))
                                            classList.Add(ClassNames);

                                        // text alignment
                                        if (TextAlignment != Alignment.None)
                                            classList.Add(TextAlignment.ToTextAlignmentClass());

                                        // text nowrap
                                        if (TextNoWrap)
                                            classList.Add(BootstrapClass.TextNoWrap);

                                        // custom column class
                                        var columnClass = ColumnClass?.Invoke(rowData) ?? "";

                                        if (!string.IsNullOrWhiteSpace(columnClass))
                                            classList.Add(columnClass);

                                        if (Freeze)
                                        {
                                            classList.Add("freeze-column");

                                            var styleList = new List<string>();

                                            if (FreezeDirection == FreezeDirection.Left)
                                            {
                                                styleList.Add($"left:{FreezeLeftPosition.ToString(CultureInfo.InvariantCulture)}{Parent.Unit.ToCssString()}");
                                            }
                                            else
                                            {
                                                styleList.Add($"right:{FreezeRightPosition.ToString(CultureInfo.InvariantCulture)}{Parent.Unit.ToCssString()}");

                                                classList.Add("freeze-column-right");
                                            }

                                            builder.AddAttribute(101, "style", string.Join(";", styleList));
                                        }

                                        if (classList.Any())
                                            builder.AddAttribute(102, "class", string.Join(" ", classList));

                                        builder.AddContent(103, ChildContent, rowData);
                                        builder.CloseElement();
                                    };

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment<TItem> ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the column class.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? ColumnClass { get; set; }

    /// <summary>
    /// If <see langword="true" />, filter is enabled.
    /// The filter is enabled or disabled based on the grid `AllowFiltering` parameter.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    [Parameter]
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Gets or sets the filter button color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ButtonColor.Light" />.
    /// </remarks>
    [Parameter]
    public ButtonColor FilterButtonColor { get; set; } = ButtonColor.Light;

    /// <summary>
    /// Gets or sets the filter button CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? FilterButtonCSSClass { get; set; }

    /// <summary>
    /// Gets or sets the filter operator.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="FilterOperator.None" />.
    /// </remarks>
    [Parameter]
    public FilterOperator FilterOperator { get; set; } = FilterOperator.None;

    /// <summary>
    /// Gets or sets the filter textbox width in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [Parameter]
    public int FilterTextboxWidth { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width units.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Px" />.
    /// </remarks>
    [Parameter]
    public Unit FilterTextboxWidthUnit { get; set; } = Unit.Px;

    /// <summary>
    /// Gets or sets the filter value.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string FilterValue { get; set; } = default!;

    /// <summary>
    /// Indicates whether the column is frozen.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Freeze { get; set; }

    /// <summary>
    /// Gets or sets the freeze direction of the column.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="FreezeDirection.Left" />.
    /// </remarks>
    [Parameter]
    public FreezeDirection FreezeDirection { get; set; } = FreezeDirection.Left;

    /// <summary>
    /// Gets or sets the horizontal position of the column from left. It has no effect on non-positioned columns.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [Parameter]
    public double FreezeLeftPosition { get; set; }

    /// <summary>
    /// Gets or sets the horizontal position of the column from right. It has no effect on non-positioned columns.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [Parameter]
    public double FreezeRightPosition { get; set; }

    /// <summary>
    /// Gets or sets the header content.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment HeaderContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the header template.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    internal RenderFragment HeaderTemplate =>
        headerTemplate ??= builder =>
                           {
                               // th > span "title", span > i "icon"
                               builder.OpenElement(101, "th");

                               var classList = new List<string>();

                               // default class names
                               if (!string.IsNullOrWhiteSpace(ClassNames))
                                   classList.Add(ClassNames);

                               if (HeaderContent is null && HeaderTextAlignment != Alignment.None)
                                   classList.Add(HeaderTextAlignment.ToTextAlignmentClass());

                               if (Freeze)
                               {
                                   classList.Add("freeze-column");

                                   var styleList = new List<string>();

                                   if (FreezeDirection == FreezeDirection.Left)
                                   {
                                       styleList.Add($"left:{FreezeLeftPosition.ToString(CultureInfo.InvariantCulture)}{Parent.Unit.ToCssString()}");
                                   }
                                   else
                                   {
                                       styleList.Add($"right:{FreezeRightPosition.ToString(CultureInfo.InvariantCulture)}{Parent.Unit.ToCssString()}");

                                       classList.Add("freeze-column-right");
                                   }

                                   builder.AddAttribute(102, "style", string.Join(";", styleList));
                               }

                               builder.AddAttribute(103, "class", string.Join(" ", classList));

                               if (HeaderContent is null)
                               {
                                   if (CanSort())
                                   {
                                       builder.AddAttribute(104, "role", "button");
                                       builder.AddAttribute(105, "onclick", async () => await OnSortClickAsync());
                                   }

                                   builder.OpenElement(106, "span"); // open: span
                                   builder.AddAttribute(107, "class", "me-2");
                                   builder.AddContent(108, HeaderText);
                                   builder.CloseElement(); // close: span

                                   if (CanSort())
                                   {
                                       builder.OpenElement(109, "span"); // open: span
                                       builder.OpenElement(110, "i"); // open: i

                                       var sortIcon = "bi bi-arrow-down-up"; // default icon

                                       if (currentSortDirection is not SortDirection.None and SortDirection.Ascending)
                                           sortIcon = "bi bi-sort-alpha-down";
                                       else if (currentSortDirection is not SortDirection.None and SortDirection.Descending)
                                           sortIcon = "bi bi-sort-alpha-down-alt";

                                       builder.AddAttribute(111, "class", sortIcon);
                                       builder.CloseElement(); // close: i
                                       builder.CloseElement(); // close: span
                                   }
                               }
                               else
                               {
                                   // If headercontent is used, filters and sorting wont be added.
                                   builder.AddContent(112, HeaderContent);
                               }

                               builder.CloseElement(); // close: th
                           };

    /// <summary>
    /// Gets or sets the table column header text.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string HeaderText { get; set; } = default!;

    /// <summary>
    /// Gets or sets the header text alignment.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.Start" />.
    /// </remarks>
    [Parameter]
    public Alignment HeaderTextAlignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets the default sort column.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool IsDefaultSortColumn { get; set; } = false;

    /// <summary>
    /// Gets or sets visibility of the Grid column.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    [Parameter]
    public bool IsVisible { get; set; } = true;

    [CascadingParameter(Name = "Parent")]
    public Grid<TItem> Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the property name.
    /// This is required when `AllowFiltering` is true.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string PropertyName { get; set; } = default!;

    /// <summary>
    /// Enable or disable the sorting on a specific column.
    /// The sorting is enabled or disabled based on the `AllowSorting` parameter on the grid.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the default sort direction of a column.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="SortDirection.None" />.
    /// </remarks>
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
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string SortString { get; set; } = default!;

    /// <summary>
    /// Gets or sets the StringComparison.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="StringComparison.OrdinalIgnoreCase" />.
    /// </remarks>
    [Parameter]
    public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets or sets the summary column type.
    /// <para>
    /// Default value is <see cref="GridSummaryColumnType.None"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public GridSummaryColumnType SummaryType { get; set; } = GridSummaryColumnType.None;

    /// <summary>
    /// Gets or sets the summary value display format.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public string? SummaryValueDisplayFormat { get; set; }

    /// <summary>
    /// Gets or sets the summary value prefix. If set, it will be displayed before the summary value.
    /// Otherwise, based on the <see cref="SummaryType"/>, default prefix will be displayed.
    /// To remove the default prefix, set this property to an empty string.
    /// <para>
    /// Example: "Total: ", "Average: ", etc.
    /// </para>
    /// </summary>
    [Parameter]
    public string? SummaryValuePrefix { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.Start" />.
    /// </remarks>
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets text nowrap.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool TextNoWrap { get; set; }

    #endregion
}
