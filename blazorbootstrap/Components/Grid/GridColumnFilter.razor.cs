namespace BlazorBootstrap;

public partial class GridColumnFilter<TItem> : BaseComponent
{
    #region Members

    private FilterOperator filterOperator;

    private string? filterValue;

    private IEnumerable<FilterOperatorInfo> filterOperators => FilterOperatorHelper.GetFilterOperators(PropertyTypeName);

    private string? selectedFilterSymbol;

    private string filterStyle => FilterWidth > 0 ? $"width:{FilterWidth}px;" : "";

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        filterOperator = FilterOperator;
        filterValue = FilterValue;

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        SetDefaultFilter();
        SetSelectedFilterSymbol();
    }

    private async Task OnFilterOperatorChangedAsync(FilterOperatorInfo filterOperatorInfo)
    {
        filterOperator = filterOperatorInfo.FilterOperator;

        if (filterOperatorInfo.Symbol == "x")
        {
            if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
                filterValue = null; // TODO: fix reset symbol
            else
                filterValue = string.Empty;
        }

        SetSelectedFilterSymbol();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue, filterOperator));
    }

    private async Task OnFilterValueChangedAsync(ChangeEventArgs args)
    {
        filterValue = args?.Value?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue, filterOperator));
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
            if (filterOperator == FilterOperator.None)
                filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameString
            or StringConstants.PropertyTypeNameChar)
        {
            if (filterOperator == FilterOperator.None)
                filterOperator = FilterOperator.Contains;
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameDateOnly
            or StringConstants.PropertyTypeNameDateTime)
        {
            if (filterOperator == FilterOperator.None)
                filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            if (filterOperator == FilterOperator.None)
                filterOperator = FilterOperator.Equals;
        }
    }

    private void SetSelectedFilterSymbol()
    {
        if (PropertyTypeName is StringConstants.PropertyTypeNameInt16
            or StringConstants.PropertyTypeNameInt32
            or StringConstants.PropertyTypeNameInt64
            or StringConstants.PropertyTypeNameSingle // float
            or StringConstants.PropertyTypeNameDecimal
            or StringConstants.PropertyTypeNameDouble)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetNumberFilterOperators().FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol ?? "=";
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameString
            or StringConstants.PropertyTypeNameChar)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetStringFilterOperators().FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol ?? "*a*";
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameDateOnly
            or StringConstants.PropertyTypeNameDateTime)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetDateFilterOperators().FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol ?? "=";
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetBooleanFilterOperators().FirstOrDefault(x => x.FilterOperator == filterOperator)?.Symbol ?? "=";
        }
    }

    #endregion Methods

    #region Properties

    [CascadingParameter] public Grid<TItem> Grid { get; set; } = default!;

    [Parameter] public EventCallback<FilterEventArgs> GridColumnFilterChanged { get; set; }

    /// <summary>
    /// Gets or sets filter operator.
    /// </summary>
    [Parameter] public FilterOperator FilterOperator { get; set; }

    /// <summary>
    /// Gets or sets filter value.
    /// </summary>
    [Parameter] public string? FilterValue { get; set; }

    /// <summary>
    /// Gets or sets the filter property name.
    /// </summary>
    [Parameter] public string? PropertyTypeName { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width.
    /// </summary>
    [Parameter] public int FilterWidth { get; set; }

    #endregion Properties
}
