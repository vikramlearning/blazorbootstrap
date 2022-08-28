using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

partial class GridColumnFilter : BaseComponent
{
    #region Members

    private FilterOperator filterOperator;

    private string filterValue;

    private IEnumerable<FilterOperatorInfo> filterOperators => GetFilterOperators();

    private string selectedFilterSymbol;

    private FilterOperator defaultFilterOperator;

    private string filterStyle => this.FilterWidth > 0 ? $"width:{this.FilterWidth}px;" : "";

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        this.filterOperator = this.FilterOperator;
        this.filterValue = this.FilterValue;

        SetDefaultFilter();

        SetSelectedFilterSymbol();

        base.OnInitialized();
    }

    private void OnFilterOperatorChanged(EventArgs args, FilterOperatorInfo filterOperatorInfo)
    {
        this.filterOperator = filterOperatorInfo.FilterOperator;
        if (filterOperatorInfo.Symbol == "x")
        {
            this.filterValue = string.Empty;
        }
        SetSelectedFilterSymbol();

        if (GridColumnFilterChanged.HasDelegate)
            GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(this.filterValue, this.filterOperator));
    }

    private void OnFilterChanged(ChangeEventArgs args)
    {
        this.filterValue = args?.Value?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(this.filterValue, this.filterOperator));
    }

    private IEnumerable<FilterOperatorInfo> GetFilterOperators()
    {
        if (PropertyTypeName == StringConstants.PropertyTypeNameInt16
            || PropertyTypeName == StringConstants.PropertyTypeNameInt32
            || PropertyTypeName == StringConstants.PropertyTypeNameInt64
            || PropertyTypeName == StringConstants.PropertyTypeNameSingle // float
            || PropertyTypeName == StringConstants.PropertyTypeNameDecimal
            || PropertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            return GetNumberFilterOperators();
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameString
            || PropertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            return GetStringFilterOperators();
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || PropertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            return GetDateFilterOperators();
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            return GetBooleanFilterOperators();
        }

        return null;
    }

    private IEnumerable<FilterOperatorInfo> GetNumberFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("=", "Equals", FilterOperator.Equals));
        result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        result.Add(new("<", "Less than", FilterOperator.LessThan));
        result.Add(new("<=", "Less than or equals", FilterOperator.LessThanOrEquals));
        result.Add(new(">", "Greater than", FilterOperator.GreaterThan));
        result.Add(new(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals));
        result.Add(new("x", "Clear", FilterOperator.Equals));

        return result;
    }

    private IEnumerable<FilterOperatorInfo> GetStringFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("*a*", "Contains", FilterOperator.Contains));
        //result.Add(new("!*a*", "Does not contain", FilterOperator.DoesNotContain));
        result.Add(new("a**", "Starts with", FilterOperator.StartsWith));
        result.Add(new("**a", "Ends with", FilterOperator.EndsWith));
        //result.Add(new("=''", "Is empty", FilterOperator.IsEmpty));
        //result.Add(new("!=''", "Is not empty", FilterOperator.IsNotEmpty));
        result.Add(new("=", "Equals", FilterOperator.Equals));
        result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        //result.Add(new("null", "Is null", FilterOperator.IsNull));
        //result.Add(new("!null", "Is not null", FilterOperator.IsNotNull));
        result.Add(new("x", "Clear", FilterOperator.Contains));

        return result;
    }

    private IEnumerable<FilterOperatorInfo> GetDateFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("=", "Equals", FilterOperator.Equals));
        result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        result.Add(new("<", "Less than", FilterOperator.LessThan));
        result.Add(new("<=", "Less than or equals", FilterOperator.LessThanOrEquals));
        result.Add(new(">", "Greater than", FilterOperator.GreaterThan));
        result.Add(new(">=", "Greater than or equals", FilterOperator.GreaterThanOrEquals));
        result.Add(new("x", "Clear", FilterOperator.Equals));

        return result;
    }

    private IEnumerable<FilterOperatorInfo> GetBooleanFilterOperators()
    {
        List<FilterOperatorInfo> result = new();

        result.Add(new("=", "Equals", FilterOperator.Equals));
        result.Add(new("!=", "Not equals", FilterOperator.NotEquals));
        result.Add(new("x", "Clear", FilterOperator.Equals));

        return result;
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
                this.filterOperator = this.defaultFilterOperator = FilterOperator.Equals;
            else
                this.defaultFilterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameString
            || PropertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            if (this.filterOperator == FilterOperator.None)
                this.filterOperator = this.defaultFilterOperator = FilterOperator.Contains;
            else
                this.defaultFilterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || PropertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            if (this.filterOperator == FilterOperator.None)
                this.filterOperator = this.defaultFilterOperator = FilterOperator.Equals;
            else
                this.defaultFilterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            if (this.filterOperator == FilterOperator.None)
                this.filterOperator = this.defaultFilterOperator = FilterOperator.Equals;
            else
                this.defaultFilterOperator = FilterOperator.Equals;
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
            selectedFilterSymbol = GetNumberFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "=";
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameString
            || PropertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            selectedFilterSymbol = GetStringFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "*a*";
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || PropertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            selectedFilterSymbol = GetDateFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "=";
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            selectedFilterSymbol = GetBooleanFilterOperators().FirstOrDefault(x => x.FilterOperator == this.filterOperator)?.Symbol ?? "=";
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
