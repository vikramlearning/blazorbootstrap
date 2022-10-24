namespace BlazorBootstrap;

partial class GridColumnFilter : BaseComponent
{
    #region Members

    private FilterOperator filterOperator;

    private string filterValue;

    private IEnumerable<FilterOperatorInfo> filterOperators => FilterOperatorHelper.GetFilterOperators(this.PropertyTypeName);

    private string selectedFilterSymbol;

    private string filterStyle => this.FilterWidth > 0 ? $"width:{this.FilterWidth}px;" : "";

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        this.filterOperator = this.FilterOperator;
        this.filterValue = this.FilterValue;

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        SetDefaultFilter();
        SetSelectedFilterSymbol();

        base.OnParametersSet();
    }

    private async Task OnFilterOperatorChangedAsync(FilterOperatorInfo filterOperatorInfo)
    {
        this.filterOperator = filterOperatorInfo.FilterOperator;

        if (filterOperatorInfo.Symbol == "x")
        {
            if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
                this.filterValue = null; // TODO: fix reset symbol
            else
                this.filterValue = string.Empty;
        }

        SetSelectedFilterSymbol();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(this.filterValue, this.filterOperator));
    }

    private async Task OnFilterValueChangedAsync(ChangeEventArgs args)
    {
        this.filterValue = args?.Value?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(this.filterValue, this.filterOperator));
    }

    internal void SetDefaultFilter()
    {
        if (PropertyTypeName == StringConstants.PropertyTypeNameInt16
            || PropertyTypeName == StringConstants.PropertyTypeNameInt32
            || PropertyTypeName == StringConstants.PropertyTypeNameInt64
            || PropertyTypeName == StringConstants.PropertyTypeNameSingle // float
            || PropertyTypeName == StringConstants.PropertyTypeNameDecimal
            || PropertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            if (this.filterOperator == FilterOperator.None)
                this.filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameString
            || PropertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            if (this.filterOperator == FilterOperator.None)
                this.filterOperator = FilterOperator.Contains;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || PropertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            if (this.filterOperator == FilterOperator.None)
                this.filterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            if (this.filterOperator == FilterOperator.None)
                this.filterOperator = FilterOperator.Equals;
        }
    }

    private void SetSelectedFilterSymbol()
    {
        if (PropertyTypeName == StringConstants.PropertyTypeNameInt16
            || PropertyTypeName == StringConstants.PropertyTypeNameInt32
            || PropertyTypeName == StringConstants.PropertyTypeNameInt64
            || PropertyTypeName == StringConstants.PropertyTypeNameSingle // float
            || PropertyTypeName == StringConstants.PropertyTypeNameDecimal
            || PropertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetNumberFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "=";
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameString
            || PropertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetStringFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "*a*";
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || PropertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetDateFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "=";
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            selectedFilterSymbol = FilterOperatorHelper.GetBooleanFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "=";
        }
    }

    #endregion Methods

    #region Properties

    [Parameter] public EventCallback<FilterEventArgs> GridColumnFilterChanged { get; set; }

    /// <summary>
    /// Gets or sets filter operator.
    /// </summary>
    [Parameter] public FilterOperator FilterOperator { get; set; }

    /// <summary>
    /// Gets or sets filter value.
    /// </summary>
    [Parameter] public string FilterValue { get; set; }

    /// <summary>
    /// Gets or sets the filter property name.
    /// </summary>
    [Parameter] public string PropertyTypeName { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width.
    /// </summary>
    [Parameter] public int FilterWidth { get; set; }

    #endregion Properties
}
