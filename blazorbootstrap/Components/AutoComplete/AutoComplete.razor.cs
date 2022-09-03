using System.Reflection;

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
    private TItem? selectedItem;    

    public TItem SelectedItem => selectedItem;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        if (this.Items is not null && this.Items.Any())
            this.items = this.Items;

        base.OnInitialized();
    }

    private async Task OnInputChanged(ChangeEventArgs args)
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
        FilterData(this.inputValue);

        closeButton?.HideLoading();
    }

    private void OnItemSelected(TItem item)
    {
        this.inputValue = GetPropertyValue(item);
        this.selectedItem = item;
        HidePanel();
        SetInputHasValue();

        if(OnChanged.HasDelegate)
            OnChanged.InvokeAsync(item);
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
    }

    /// <summary>
    /// Checks whether the input has value.
    /// </summary>
    private void SetInputHasValue() => this.inputHasValue = inputValue.Length > 0;

    private void ShowPanel() => showPanel = true;

    private void HidePanel() => showPanel = false;

    private string? GetPropertyValue(TItem item)
    {
        if(string.IsNullOrWhiteSpace(this.PropertyName))
            return string.Empty;

        var propertyInfo = typeof(TItem).GetProperty(this.PropertyName);
        return propertyInfo?.GetValue(item)?.ToString();
    }

    private void FilterData(string searchKey)
    {
    }

    #endregion Methods

    #region Properties

    [Parameter] public IEnumerable<TItem> Items { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    [Parameter, EditorRequired] public string PropertyName { get; set; } = null!;

    [Parameter] public EventCallback<TItem> OnChanged { get; set; }

    #endregion Properties
}