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
    private ValidationState state = ValidationState.UnModified;
    private bool disabled;

    /// <summary>
    /// Gets selected item.
    /// </summary>
    public TItem SelectedItem => selectedItem;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        this.disabled = this.Disabled;

        base.OnInitialized();
    }

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());
        builder.Append(BootstrapClassProvider.ToAutoCompleteSize(this.Size));
        builder.Append(BootstrapClassProvider.IsInValid(), state == ValidationState.InValid);
        builder.Append(BootstrapClassProvider.IsValid(), state == ValidationState.Valid);

        base.BuildClasses(builder);
    }

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

        await FilterDataAsync();

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

    private async Task FilterDataAsync()
    {
        string searchKey = this.inputValue;
        if (string.IsNullOrWhiteSpace(searchKey))
            return;

        var request = new AutoCompleteDataProviderRequest<TItem>
        {
            Filter = new FilterItem(this.PropertyName, searchKey, FilterOperator.Contains)
        };

        if (DataProvider != null)
        {
            var result = await DataProvider.Invoke(request);
            if (result is not null)
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

    /// <summary>
    /// Disable autocomplete.
    /// </summary>
    public void Disable()
    {
        this.disabled = true;
    }

    /// <summary>
    /// Enable autocomplete.
    /// </summary>
    public void Enable()
    {
        this.disabled = false;
    }

    /// <summary>
    /// Mark autocomplete validation state as in-valid.
    /// </summary>
    public void MarkAsInValid()
    {
        state = ValidationState.InValid;
        DirtyClasses();
    }

    /// <summary>
    /// Mark autocomplete validation state as valid.
    /// </summary>
    public void MarkAsValid()
    {
        state = ValidationState.Valid;
        DirtyClasses();
    }

    /// <summary>
    /// Mark autocomplete validation state as unmodified.
    /// </summary>
    public void MarkAsUnModified()
    {
        state = ValidationState.UnModified;
        DirtyClasses();
    }

    /// <summary>
    /// Refresh the autocomplete data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync() => await this.FilterDataAsync();

    /// <summary>
    /// Resets the autocomplete selection.
    /// </summary>
    public void Reset() => ClearInputText();

    #endregion Methods

    #region Properties

    /// <summary>
    /// DataProvider is for items to render. 
    /// The provider should always return an instance of 'AutoCompleteDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter, EditorRequired] public AutoCompleteDataProviderDelegate<TItem> DataProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    [Parameter, EditorRequired] public string PropertyName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the autocomplete size.
    /// </summary>
    [Parameter] public AutoCompleteSize Size { get; set; }

    [Parameter] public EventCallback<TItem> OnChanged { get; set; }

    #endregion Properties
}