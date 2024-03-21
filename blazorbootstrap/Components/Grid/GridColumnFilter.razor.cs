using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBootstrap;

public partial class GridColumnFilter : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FilterOperator filterOperator;

    private IEnumerable<FilterOperatorInfo>? filterOperators;
    
    private Button closeButton = default!;

    private string? filterValue;
    private string? betweenLeftValue;
    private string? betweenRightValue;
    private string? enumFilterValue;
    private HashSet<string> filterValues = new HashSet<string>();

    private string? selectedFilterSymbol;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        filterOperators = await GetFilterOperatorsAsync();
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
                filterOperator = FilterOperator.In;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameGuid)
        {
            if (filterOperator is FilterOperator.None or FilterOperator.Clear)
                filterOperator = FilterOperator.Contains;
        }
    }

    private async Task<IEnumerable<FilterOperatorInfo>> GetFilterOperatorsAsync()
    {
        if (FiltersTranslationProvider is null)
            return FilterOperatorHelper.GetFilterOperators(PropertyTypeName!);

        var filters = await FiltersTranslationProvider.Invoke();

        if (!(filters?.Any() ?? false))
            return FilterOperatorHelper.GetFilterOperators(PropertyTypeName!);

        return FilterOperatorHelper.GetFilterOperators(PropertyTypeName!, filters!);
    }

    private async Task OnFilterOperatorChangedAsync(FilterOperatorInfo filterOperatorInfo)
    {
        if (filterOperatorInfo.FilterOperator == FilterOperator.Clear) {
            filterOperator = FilterOperator.Clear;
            SetDefaultFilter();
            filterValues.Clear();
            betweenLeftValue = string.Empty;
            betweenRightValue = string.Empty;

            if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
                filterValue = null;
            else
                filterValue = string.Empty;
        }
        else
        {
            filterOperator = filterOperatorInfo.FilterOperator;
            if (filterOperator == FilterOperator.Between)
            {
                filterValue = $"{betweenLeftValue};{betweenRightValue}";
            }
            else if (filterOperator == FilterOperator.In)
            {
                filterValue = string.Join(',', filterValues);
            }
            else
            {
                filterValue = filterValue?.Split(Config.FilterBetweenSeparator).FirstOrDefault();
            }
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

    private async Task OnBetweenLeftValueChangedAsync(ChangeEventArgs args)
    {
        betweenLeftValue = args?.Value?.ToString();

        var rightValue = betweenRightValue ?? betweenLeftValue;
        
        filterValue = $"{betweenLeftValue}{Config.FilterBetweenSeparator}{rightValue}";

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue!, filterOperator));
    }

    private async Task OnBetweenRightValueChangedAsync(ChangeEventArgs args)
    {
        betweenRightValue = args?.Value?.ToString();

        var leftValue = betweenLeftValue ?? betweenRightValue;
        
        filterValue = $"{leftValue}{Config.FilterBetweenSeparator}{betweenRightValue}";

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue!, filterOperator));
    }

    private async Task OnFilterEnumValueChangedAsync(object? e)
    {
        var value = e?.ToString();
        if (value is null) return;
        
        var values = filterValue?.Split(',').ToHashSet() ?? new HashSet<string>();

        if (!values.Add(value)) 
        {
            values.Remove(value);
        }
        values.Remove(string.Empty);
        
        filterValue = string.Join(',', values);
        filterValues = values;

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(filterValue, filterOperator));
    }

    private void OnEnumFilterListValueChangedAsync(ChangeEventArgs? args)
    {
        enumFilterValue = args?.Value?.ToString();

        StateHasChanged();
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

    private string filterStyle => FilterWidth > 0 ? $"width:{FilterWidth.ToString(CultureInfo.InvariantCulture)}{Unit};" : "";

    /// <summary>
    /// Gets or sets filter value.
    /// </summary>
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
    public string? PropertyTypeName { get; set; }

    [Parameter]
    public PropertyInfo? PropertyInfo { get; set; }

    /// <summary>
    /// Gets or sets the units.
    /// </summary>
    [Parameter]
    public Unit Unit { get; set; }

    #endregion

    private void ClearInputText() {
        enumFilterValue = string.Empty;
        StateHasChanged();
    }
}
