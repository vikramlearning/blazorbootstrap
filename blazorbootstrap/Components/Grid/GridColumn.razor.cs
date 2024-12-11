namespace BlazorBootstrap;

/// <summary>
/// Represents a column in the <see cref="Grid{T}"/>.
/// </summary>
/// <typeparam name="TItem">Data model to apply to the column of the grid</typeparam>
public partial class GridColumn<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private RenderFragment<TItem>? cellTemplate;

    internal SortDirection CurrentSortDirection;

    internal SortDirection DefaultSortDirection;

    private FilterOperator filterOperator;

    private string filterValue = default!;

    private RenderFragment? headerTemplate;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required

        filterOperator = FilterOperator;
        filterValue = FilterValue;

        CurrentSortDirection = SortDirection;
        DefaultSortDirection = SortDirection;

        if (IsDefaultSortColumn && SortDirection == SortDirection.None)
            CurrentSortDirection = SortDirection = SortDirection.Ascending;

        Parent.AddColumn(this);

        await base.OnInitializedAsync();
    }

    /// <inheritdoc />
    protected override void OnParametersSet() => SetDefaultFilter();

    internal bool CanSort() => Parent.AllowSorting && Sortable && SortKeySelector is not null;

    internal FilterOperator GetFilterOperator() => filterOperator;

    internal string GetFilterValue() => filterValue;

    internal Type GetPropertyType() => typeof(TItem).GetPropertyType(PropertyName)!;

    internal string GetPropertyTypeName() => typeof(TItem).GetPropertyTypeName(PropertyName);

    internal IReadOnlyCollection<SortingItem<TItem>> GetSorting()
    {
        if (SortKeySelector == null && string.IsNullOrWhiteSpace(SortString))
        {
            return Array.Empty<SortingItem<TItem>>();
        }

        return new SortingItem<TItem>[] { new(SortString, SortKeySelector!, CurrentSortDirection) };
    }

    internal async Task OnFilterChangedAsync(FilterEventArgs args)
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
    }

    internal void SetFilterOperator(FilterOperator filterOperator) => FilterOperator = this.filterOperator = filterOperator;

    internal void SetFilterValue(string filterValue) => FilterValue = this.filterValue = filterValue;

    private async Task OnSortClickAsync()
    {
        // toggle the direction
        if (CurrentSortDirection == SortDirection.Ascending)
            CurrentSortDirection = SortDirection = SortDirection.Descending;
        else if (CurrentSortDirection == SortDirection.Descending)
            CurrentSortDirection = SortDirection = SortDirection.None;
        else if (CurrentSortDirection == SortDirection.None)
            CurrentSortDirection = SortDirection = SortDirection.Ascending;

        await Parent.SortingChangedAsync(this);
    }


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment<TItem>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ColumnClass), StringComparison.OrdinalIgnoreCase): ColumnClass = (Func<TItem, string>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Filterable), StringComparison.OrdinalIgnoreCase): Filterable = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterButtonColor), StringComparison.OrdinalIgnoreCase): FilterButtonColor = (ButtonColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterButtonCssClass), StringComparison.OrdinalIgnoreCase): FilterButtonCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterOperator), StringComparison.OrdinalIgnoreCase): FilterOperator = (FilterOperator)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterTextboxWidth), StringComparison.OrdinalIgnoreCase): FilterTextboxWidth = (int)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterValue), StringComparison.OrdinalIgnoreCase): FilterValue = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Freeze), StringComparison.OrdinalIgnoreCase): Freeze = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FreezeDirection), StringComparison.OrdinalIgnoreCase): FreezeDirection = (FreezeDirection)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FreezeLeftPosition), StringComparison.OrdinalIgnoreCase): FreezeLeftPosition = (CssPropertyValue)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FreezeRightPosition), StringComparison.OrdinalIgnoreCase): FreezeRightPosition = (CssPropertyValue)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HeaderContent), StringComparison.OrdinalIgnoreCase): HeaderContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HeaderText), StringComparison.OrdinalIgnoreCase): HeaderText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HeaderTextAlignment), StringComparison.OrdinalIgnoreCase): HeaderTextAlignment = (Alignment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsDefaultSortColumn), StringComparison.OrdinalIgnoreCase): IsDefaultSortColumn = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Parent), StringComparison.OrdinalIgnoreCase): Parent = (Grid<TItem>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(PropertyName), StringComparison.OrdinalIgnoreCase): PropertyName = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Sortable), StringComparison.OrdinalIgnoreCase): Sortable = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(SortDirection), StringComparison.OrdinalIgnoreCase): SortDirection = (SortDirection)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(SortKeySelector), StringComparison.OrdinalIgnoreCase): SortKeySelector = (Expression<Func<TItem, IComparable>>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(SortString), StringComparison.OrdinalIgnoreCase): SortString = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(StringComparison), StringComparison.OrdinalIgnoreCase): StringComparison = (StringComparison)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(TextAlignment), StringComparison.OrdinalIgnoreCase): TextAlignment = (Alignment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(TextNoWrap), StringComparison.OrdinalIgnoreCase): TextNoWrap = (bool)parameter.Value; break;

                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
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

                                        var classList = new List<string>
                                        {
                                            // default class names
                                            Class ?? "", 
                                            // text alignment 
                                            EnumExtensions.TextAlignmentClassMap[TextAlignment]
                                        };

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
                                                styleList.Add($"left:{FreezeLeftPosition.ToString()}");
                                            }
                                            else
                                            {
                                                styleList.Add($"right:{FreezeRightPosition.ToString()}");

                                                classList.Add("freeze-column-right");
                                            }

                                            builder.AddAttribute(101, "style", string.Join(";", styleList));
                                        }

                                        if (classList.Count > 0)
                                            builder.AddAttribute(102, "class", string.Join(" ", classList));

                                        builder.AddContent(103, ChildContent, rowData);
                                        builder.CloseElement();
                                    };

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] [EditorRequired]
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
    /// Default value is <see langword="true" />.
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
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? FilterButtonCssClass { get; set; }

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
    /// Default value is <see langword="0"/>.
    /// </remarks>
    [Parameter]
    public int FilterTextboxWidth { get; set; }

    /// <summary>
    /// Gets or sets the filter value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string FilterValue { get; set; } = default!;

    /// <summary>
    /// Indicates whether the column is frozen.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Freeze { get; set; }

    /// <summary>
    /// Gets or sets the freeze direction of the column.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="FreezeDirection.Left" />.
    /// </remarks>
    [Parameter] public FreezeDirection FreezeDirection { get; set; } = FreezeDirection.Left;

    /// <summary>
    /// Gets or sets the horizontal position of the column from left. It has no effect on non-positioned columns.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="0"/>.
    /// </remarks>
    [Parameter] public CssPropertyValue FreezeLeftPosition { get; set; } = 0;

    /// <summary>
    /// Gets or sets the horizontal position of the column from right. It has no effect on non-positioned columns.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="0"/>.
    /// </remarks>
    [Parameter] public CssPropertyValue FreezeRightPosition { get; set; } = 0;

    /// <summary>
    /// Gets or sets the header content.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Gets or sets the header template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    internal RenderFragment HeaderTemplate =>
        headerTemplate ??= builder =>
                           {
                               // th > span "title", span > i "icon"
                               builder.OpenElement(101, "th");

                               var classList = new List<string>();

                               // default class names
                               if (!string.IsNullOrWhiteSpace(Class))
                                   classList.Add(Class);

                               if (HeaderContent is null)
                                   classList.Add(EnumExtensions.TextAlignmentClassMap[HeaderTextAlignment]);

                               if (Freeze)
                               {
                                   classList.Add("freeze-column");

                                   var styleList = new List<string>();

                                   if (FreezeDirection == FreezeDirection.Left)
                                   {
                                       styleList.Add($"left:{FreezeLeftPosition.ToString()}");
                                   }
                                   else
                                   {
                                       styleList.Add($"right:{FreezeRightPosition.ToString()}");

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

                                       if (CurrentSortDirection is not SortDirection.None and SortDirection.Ascending)
                                           sortIcon = "bi bi-sort-alpha-down";
                                       else if (CurrentSortDirection is not SortDirection.None and SortDirection.Descending)
                                           sortIcon = "bi bi-sort-alpha-down-alt";

                                       builder.AddAttribute(111, "class", sortIcon);
                                       builder.CloseElement(); // close: i
                                       builder.CloseElement(); // close: span
                                   }
                               }
                               else
                               {
                                   // If headercontent is used, filters and sorting won't be added.
                                   builder.AddContent(112, HeaderContent);
                               }

                               builder.CloseElement(); // close: th
                           };

    /// <summary>
    /// Gets or sets the table column header text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool IsDefaultSortColumn { get; set; } 

    [CascadingParameter(Name = "Parent")]
    public Grid<TItem> Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the property name.
    /// This is required when <see cref="Filterable"/> is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string PropertyName { get; set; } = default!;

    /// <summary>
    /// Enable or disable the sorting on a specific column.
    /// The sorting is enabled or disabled based on the <see cref="SortString"/> parameter on the grid.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the default sort direction of a column.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="SortDirection.None" />.
    /// </remarks>
    [Parameter] public SortDirection SortDirection { get; set; } = SortDirection.None;

    /// <summary>
    /// Expression used for sorting.
    /// </summary>
    [Parameter] public Expression<Func<TItem, IComparable>>? SortKeySelector { get; set; }
    
    /// <summary>
    /// Gets or sets the column sort string.
    /// This value will be passed to the backend/API for sorting.
    /// And this property is ignored for the client-side sorting.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool TextNoWrap { get; set; }

    #endregion
}
