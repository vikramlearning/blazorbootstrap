namespace BlazorBootstrap;

public partial class GridColumnFilter : BlazorBootstrapComponentBase
{
    #region Fields and Constants
    
    private IReadOnlyCollection<FilterOperatorInfo> filterOperators = default!;
    
    private string? selectedFilterSymbol;

    #endregion

    #region Methods
 
    internal void SetDefaultFilter()
    {
        if (PropertyTypeName is StringConstants.PropertyTypeNameInt16
                                or StringConstants.PropertyTypeNameInt32
                                or StringConstants.PropertyTypeNameInt64
                                or StringConstants.PropertyTypeNameSingle // float
                                or StringConstants.PropertyTypeNameDecimal
                                or StringConstants.PropertyTypeNameDouble)
        {
            if (FilterOperator is FilterOperator.None or FilterOperator.Clear)
                FilterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameString
                                     or StringConstants.PropertyTypeNameChar)
        {
            if (FilterOperator is FilterOperator.None or FilterOperator.Clear)
                FilterOperator = FilterOperator.Contains;
        }
        else if (PropertyTypeName is StringConstants.PropertyTypeNameDateOnly
                                     or StringConstants.PropertyTypeNameDateTime)
        {
            if (FilterOperator is FilterOperator.None or FilterOperator.Clear)
                FilterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            if (FilterOperator is FilterOperator.None or FilterOperator.Clear)
                FilterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameEnum)
        {
            if (FilterOperator is FilterOperator.None or FilterOperator.Clear)
                FilterOperator = FilterOperator.Equals;
        }
        else if (PropertyTypeName == StringConstants.PropertyTypeNameGuid)
        {
            if (FilterOperator is FilterOperator.None or FilterOperator.Clear)
                FilterOperator = FilterOperator.Equals;
        }
    }

    private IReadOnlyCollection<FilterOperatorInfo> GetFilterOperatorsAsync()
    {
        if (FiltersTranslationProvider is null)
            return FilterOperatorUtility.GetFilterOperators(PropertyTypeName);

        var filters = FiltersTranslationProvider.Invoke();

        if (!(filters?.Any() ?? false))
            return FilterOperatorUtility.GetFilterOperators(PropertyTypeName);

        return FilterOperatorUtility.GetFilterOperators(PropertyTypeName, filters);
    }

    private async Task OnEnumFilterValueChangedAsync(object enumValue)
    {
        FilterValue = enumValue?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(FilterValue!, FilterOperator));
    }

    private async Task OnFilterOperatorChangedAsync(FilterOperatorInfo filterOperatorInfo)
    {
        if (filterOperatorInfo.FilterOperator == FilterOperator.Clear)
        {
            SetDefaultFilter();

            FilterValue = PropertyTypeName == StringConstants.PropertyTypeNameBoolean ? null : String.Empty;
        }
        else
        {
            FilterOperator = filterOperatorInfo.FilterOperator;
        }

        SetSelectedFilterSymbol();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(FilterValue!, FilterOperator));
    }

    private async Task OnFilterValueChangedAsync(ChangeEventArgs args)
    {
        FilterValue = args?.Value?.ToString();

        if (GridColumnFilterChanged.HasDelegate)
            await GridColumnFilterChanged.InvokeAsync(new FilterEventArgs(FilterValue!, FilterOperator));
    }

    private void SetSelectedFilterSymbol()
    {
        selectedFilterSymbol = PropertyTypeName switch
        {
            StringConstants.PropertyTypeNameInt16 or StringConstants.PropertyTypeNameInt32
                or StringConstants.PropertyTypeNameInt64 or StringConstants.PropertyTypeNameSingle // float
                or StringConstants.PropertyTypeNameDecimal
                or StringConstants.PropertyTypeNameDouble => filterOperators
                    ?.FirstOrDefault(x => x.FilterOperator == FilterOperator)
                    ?.Symbol,
            StringConstants.PropertyTypeNameString or StringConstants.PropertyTypeNameChar => filterOperators
                ?.FirstOrDefault(x => x.FilterOperator == FilterOperator)
                ?.Symbol,
            StringConstants.PropertyTypeNameDateOnly or StringConstants.PropertyTypeNameDateTime => filterOperators
                ?.FirstOrDefault(x => x.FilterOperator == FilterOperator)
                ?.Symbol,
            StringConstants.PropertyTypeNameBoolean => filterOperators
                ?.FirstOrDefault(x => x.FilterOperator == FilterOperator)
                ?.Symbol,
            StringConstants.PropertyTypeNameEnum => filterOperators
                ?.FirstOrDefault(x => x.FilterOperator == FilterOperator)
                ?.Symbol,
            StringConstants.PropertyTypeNameGuid => filterOperators
                ?.FirstOrDefault(x => x.FilterOperator == FilterOperator)
                ?.Symbol,
            _ => selectedFilterSymbol
        };
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
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(GridColumnFilterChanged), StringComparison.OrdinalIgnoreCase): GridColumnFilterChanged = (EventCallback<FilterEventArgs>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterButtonColor), StringComparison.OrdinalIgnoreCase): FilterButtonColor = (ButtonColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterButtonCssClass), StringComparison.OrdinalIgnoreCase): FilterButtonCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterOperator), StringComparison.OrdinalIgnoreCase): FilterOperator = (FilterOperator)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FiltersTranslationProvider), StringComparison.OrdinalIgnoreCase): FiltersTranslationProvider = (GridFiltersTranslationDelegate)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterValue), StringComparison.OrdinalIgnoreCase): FilterValue = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FilterWidth), StringComparison.OrdinalIgnoreCase): 
                    FilterWidth = (CssPropertyValue)parameter.Value;
                    FilterStyle = FilterWidth.Value > 0f ? $"width:{FilterWidth.ToString()};" : "";
                    break;
                case var _ when String.Equals(parameter.Name, nameof(FixedHeader), StringComparison.OrdinalIgnoreCase): FixedHeader = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(PropertyType), StringComparison.OrdinalIgnoreCase): PropertyType = (Type)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(PropertyTypeName), StringComparison.OrdinalIgnoreCase): PropertyTypeName = (string)parameter.Value; break;
                
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        
        filterOperators = GetFilterOperatorsAsync();
        SetDefaultFilter();
        SetSelectedFilterSymbol();
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers

    [Parameter]
    public string? EnumFilterSelectText { get; set; }

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
    /// Gets or sets filter operator.
    /// </summary>
    [Parameter]
    public FilterOperator FilterOperator { get; set; }

    /// <summary>
    /// Filters translation is for grid filters to render.
    /// The provider should always return a 'FilterOperatorInfo' collection, and 'null' is not allowed.
    /// </summary>
    [Parameter]
    public GridFiltersTranslationDelegate? FiltersTranslationProvider { get; set; } = default!;

    private string FilterStyle { get; set; } = "";

    /// <summary>
    /// Gets or sets filter value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? FilterValue { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width.
    /// </summary>
    [Parameter]
    public CssPropertyValue FilterWidth { get; set; } = 0;

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
    /// Gets or sets the type the property is based on. (<see langword="int"/>, <see langword="string"/>, etc.). Must be filled in.
    /// </summary> 
    [Parameter]
    [EditorRequired]
    public string PropertyTypeName { get; set; } = default!;

    #endregion
}
