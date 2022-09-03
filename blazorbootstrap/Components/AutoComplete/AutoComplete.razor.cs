namespace BlazorBootstrap;

public partial class AutoComplete<TItem> : BaseComponent
{
    #region Members

    private Button closeButton;
    private string inputValue;
    private bool inputHasValue;
    private bool showPanel;
    private string panelCSS => showPanel ? "show" : "";
    private IEnumerable<TItem> items = null;
    private int totalCount;
    private TItem? selectedItem;

    public TItem SelectedItem => selectedItem;

    #endregion Members

    #region Methods

    private async Task OnInputChangedAsync(ChangeEventArgs args)
    {
        this.inputValue = args.Value.ToString();

        SetInputHasValue();

        if (inputHasValue)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }

        closeButton?.ShowLoading();

        await FilterDataAsync(this.inputValue);

        closeButton?.HideLoading();
    }

    private async Task OnItemSelectedAsync(TItem item)
    {
        this.inputValue = GetPropertyValue(item);

        this.selectedItem = item;

        HidePanel();

        SetInputHasValue();

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(item);
    }

    /// <summary>
    /// Clears the input test value.
    /// </summary>
    private void ClearInputText()
    {
        this.inputValue = string.Empty;

        this.selectedItem = default(TItem);

        HidePanel();

        SetInputHasValue();

        if (OnChanged.HasDelegate)
            OnChanged.InvokeAsync(default(TItem));
    }

    /// <summary>
    /// Checks whether the input has value.
    /// </summary>
    private void SetInputHasValue() => this.inputHasValue = inputValue.Length > 0;

    private void ShowPanel() => showPanel = true;

    private void HidePanel() => showPanel = false;

    private string? GetPropertyValue(TItem item)
    {
        if (string.IsNullOrWhiteSpace(this.PropertyName))
            return string.Empty;

        var propertyInfo = typeof(TItem).GetProperty(this.PropertyName);
        return propertyInfo?.GetValue(item)?.ToString();
    }

    private async Task FilterDataAsync(string searchKey)
    {
        var request = new AutoCompleteDataProviderRequest<TItem>
        {
            Filter = new FilterItem(this.PropertyName, searchKey, FilterOperator.Contains)
        };

        if (DataProvider != null)
        {
            var result = await DataProvider.Invoke(request);
            if(result is not null)
            {
                items = result.Data;
                totalCount = result.TotalCount ?? result.Data.Count();
            }
            else
            {
                items = new List<TItem> { };
                totalCount = 0;
            }
        }
    }

    #endregion Methods

    #region Properties

    /// <summary>
    /// DataProvider is for items to render. 
    /// The provider should always return an instance of 'AutoCompleteDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter, EditorRequired] public AutoCompleteDataProviderDelegate<TItem> DataProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    [Parameter, EditorRequired] public string PropertyName { get; set; } = null!;

    [Parameter] public EventCallback<TItem> OnChanged { get; set; }

    #endregion Properties
}