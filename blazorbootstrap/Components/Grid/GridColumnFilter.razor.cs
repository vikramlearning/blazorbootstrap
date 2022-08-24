using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class GridColumnFilter : BaseComponent
{
    #region Members

    private IEnumerable<FilterOperatorInfo> filterOperators => GetFilterOperators();
    private string selectedFilterSymbol;
    private FilterOperator defaultFilterOperator;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        if (PropertyTypeName == "Int32")
            this.FilterOperator = this.defaultFilterOperator = FilterOperator.Equals;
        else if (PropertyTypeName == "String")
            this.FilterOperator = this.defaultFilterOperator = FilterOperator.Contains;

        SetSelectedFilterSymbol();

        base.OnInitialized();
    }

    private void FilterOperatorChanged(EventArgs args, FilterOperatorInfo filterOperatorInfo)
    {
        this.FilterOperator = filterOperatorInfo.FilterOperator;
        if (filterOperatorInfo.Symbol == "x")
        {
            this.FilterValue = string.Empty;
        }
        SetSelectedFilterSymbol();

        if (GridColumnFilterChanged.HasDelegate)
            GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(this.FilterValue, this.FilterOperator));
    }

    private void OnFilterChanged(ChangeEventArgs args)
    {
        this.FilterValue = args?.Value?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(this.FilterValue, this.FilterOperator));
    }

    private IEnumerable<FilterOperatorInfo> GetFilterOperators()
    {
        if (PropertyTypeName == "Int32")
            return GetNumberFilterOperators();
        else if (PropertyTypeName == "String")
            return GetStringFilterOperators();

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

    private void SetSelectedFilterSymbol()
    {
        if (PropertyTypeName == "Int32")
        {
            selectedFilterSymbol = GetNumberFilterOperators().FirstOrDefault(x => x.FilterOperator == this.FilterOperator)?.Symbol ?? "=";
        }
        else if (PropertyTypeName == "String")
        {
            selectedFilterSymbol = GetStringFilterOperators().FirstOrDefault(x => x.FilterOperator == this.FilterOperator)?.Symbol ?? "*a*";
        }
    }

    #endregion Methods

    #region Properties

    [Parameter] public EventCallback<FilterEventArgs> GridColumnFilterChanged { get; set; }

    [Parameter] public FilterOperator FilterOperator { get; set; }

    [Parameter] public string FilterValue { get; set; }

    [Parameter] public string PropertyTypeName { get; set; }

    #endregion Properties
}
