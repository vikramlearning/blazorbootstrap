namespace BlazorBootstrap;

public partial class GridColumnFilter : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FilterOperator filterOperator;

    private IEnumerable<FilterOperatorInfo>? filterOperators;

    private string? filterValue;

    private string? selectedFilterSymbol;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        filterOperators = await GetFilterOperatorsAsync(PropertyTypeName!);
        filterOperator = FilterOperator;
        filterValue = FilterValue;

        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        SetDefaultFilter();
        SetSelectedFilterSymbol();
    }

    internal void SetDefaultFilter()
    {
        if (PropertyTypeName is StringConstants.PropertyTypeNameInt16
                                or StringConstants.PropertyTypeNameInt32
                                or StringConstants.PropertyTypeNameInt64
                                or StringConstants.PropertyTypeNameSingle // float
                                or StringConstants.PropertyTypeNameDecimal
                                or StringConstants.PropertyTypeNameDouble)
        {
            if (filterOperator is FilterOperator.None or FilterOperator.Clear)
                filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameString
                                     or StringConstants.PropertyTypeNameChar)
        {
            if (filterOperator is FilterOperator.None or FilterOperator.Clear)
                filterOperator = FilterOperator.Contains;
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameDateOnly
                                     or StringConstants.PropertyTypeNameDateTime)
        {
            if (filterOperator is FilterOperator.None or FilterOperator.Clear)
                filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            if (filterOperator is FilterOperator.None or FilterOperator.Clear)
                filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameEnum)
        {
            if (filterOperator is FilterOperator.None or FilterOperator.Clear)
                filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameGuid)
        {
            if (filterOperator is FilterOperator.None or FilterOperator.Clear)
                filterOperator = FilterOperator.Equals;
        }
    }

    private async Task<IEnumerable<FilterOperatorInfo>> GetFilterOperatorsAsync(string propertyTypeName)
    {
        if (FiltersTranslationProvider is null)
            return FilterOperatorUtility.GetFilterOperators(PropertyTypeName!);

        var filters = await FiltersTranslationProvider.Invoke();

        if (!(filters?.Any() ?? false))
            return FilterOperatorUtility.GetFilterOperators(PropertyTypeName!);

        return FilterOperatorUtility.GetFilterOperators(PropertyTypeName!, filters!);
    }

    private async Task OnEnumFilterValueChangedAsync(object enumValue)
    {
        filterValue = enumValue?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue!, filterOperator));
    }

    private async Task OnFilterOperatorChangedAsync(FilterOperatorInfo filterOperatorInfo)
    {
        if (filterOperatorInfo.FilterOperator == FilterOperator.Clear)
        {
            SetDefaultFilter();

            if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
                filterValue = null;
            else
                filterValue = string.Empty;
        }
        else
        {
            filterOperator = filterOperatorInfo.FilterOperator;
        }

        SetSelectedFilterSymbol();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue!, filterOperator));
    }

    private async Task OnFilterValueChangedAsync(ChangeEventArgs args)
    {
        filterValue = args?.Value?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue!, filterOperator));
    }

    private void SetSelectedFilterSymbol()
    {
        if (PropertyTypeName is StringConstants.PropertyTypeNameInt16
                                or StringConstants.PropertyTypeNameInt32
                                or StringConstants.PropertyTypeNameInt64
                                or StringConstants.PropertyTypeNameSingle // float
                                or StringConstants.PropertyTypeNameDecimal
                                or StringConstants.PropertyTypeNameDouble)
            selectedFilterSymbol = filterOperators?.FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol;
        else if (PropertyTypeName is StringConstants.PropertyTypeNameString
                                     or StringConstants.PropertyTypeNameChar)
            selectedFilterSymbol = filterOperators?.FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol;
        else if (PropertyTypeName is StringConstants.PropertyTypeNameDateOnly
                                     or StringConstants.PropertyTypeNameDateTime)
            selectedFilterSymbol = filterOperators?.FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol;
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean) selectedFilterSymbol = filterOperators?.FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol;
        else if (PropertyTypeName == StringConstants.PropertyTypeNameEnum) selectedFilterSymbol = filterOperators?.FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol;
        else if (PropertyTypeName == StringConstants.PropertyTypeNameGuid) selectedFilterSymbol = filterOperators?.FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol;
    }

    #endregion

    #region Properties, Indexers

    [Parameter]
    public string? EnumFilterSelectText { get; set; }

    /// <summary>
    /// Gets or sets the filter button color.
    /// </summary>
    /// <para>
    /// Default value is <see cref="ButtonColor.Light" />.
    /// </para>
    [Parameter]
    public ButtonColor FilterButtonColor { get; set; } = ButtonColor.Light;

    /// <summary>
    /// Gets or sets the filter button CSS class.
    /// </summary>
    /// <para>
    /// Default value is null.
    /// </para>
    [Parameter]
    public string? FilterButtonCSSClass { get; set; }

    private string filterButtonColorString => FilterButtonColor.ToButtonColorClass();

    /// <summary>
    /// Gets or sets filter operator.
    /// </summary>
    [Parameter]
    public FilterOperator FilterOperator { get; set; }

    /// <summary>
    /// Filters transalation is for grid filters to render.
    /// The provider should always return a 'FilterOperatorInfo' collection, and 'null' is not allowed.
    /// </summary>
    [Parameter]
    public GridFiltersTranslationDelegate FiltersTranslationProvider { get; set; } = default!;

    private string filterStyle => FilterWidth > 0 ? $"width:{FilterWidth.ToString(CultureInfo.InvariantCulture)}{Unit.ToCssString()};" : "";

    /// <summary>
    /// Gets or sets filter value.
    /// </summary>
    /// <para>
    /// Default value is null.
    /// </para>
    [Parameter]
    public string? FilterValue { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width.
    /// </summary>
    [Parameter]
    public int FilterWidth { get; set; }

    /// <summary>
    /// Gets or sets the grid fixed header.
    /// </summary>
    [Parameter]
    public bool FixedHeader { get; set; }

    [Parameter] public EventCallback<FilterEventArgs> GridColumnFilterChanged { get; set; }

    /// <summary>
    /// Gets or sets the filter property name.
    /// </summary>
    [Parameter]
    public Type? PropertyType { get; set; }

    /// <summary>
    /// Gets or sets the filter property name.
    /// </summary>
    /// <para>
    /// Default value is null.
    /// </para>
    [Parameter]
    public string? PropertyTypeName { get; set; }

    /// <summary>
    /// Gets or sets the units.
    /// </summary>
    [Parameter]
    public Unit Unit { get; set; }

    #endregion
}
