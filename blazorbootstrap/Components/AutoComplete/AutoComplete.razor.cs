using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace BlazorBootstrap;

public partial class AutoComplete<TItem> : BaseComponent
{
    #region Events

    /// <summary>
    /// This event fires immediately when the autocomplete selection changes by the user.
    /// </summary>
    [Parameter] public EventCallback<TItem> OnChanged { get; set; }

    /// <summary>
    /// This is event fires on every user keystroke that changes the textbox value.
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    #endregion Events

    #region Members

    private FieldIdentifier fieldIdentifier;
    private string fieldCssClasses => CascadedEditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private bool inputHasValue;
    private bool showPanel;
    private string panelCSS => showPanel ? "show" : "";
    private IEnumerable<TItem> items = null;
    private int totalCount;
    private TItem? selectedItem;
    private bool disabled;
    private Button closeButton;

    /// <summary>
    /// Gets selected item.
    /// </summary>
    public TItem SelectedItem => selectedItem;

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        this.disabled = this.Disabled;

        base.OnInitialized();
    }

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());
        builder.Append(BootstrapClassProvider.ToAutoCompleteSize(this.Size));

        base.BuildClasses(builder);
    }

    private async Task OnInputChangedAsync(ChangeEventArgs args)
    {
        this.Value = args.Value.ToString();

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
        this.selectedItem = item;
        this.Value = this.GetPropertyValue(item);
        await ValueChanged.InvokeAsync(this.Value);

        HidePanel();

        SetInputHasValue();

        CascadedEditContext?.NotifyFieldChanged(fieldIdentifier);

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(item);
    }

    /// <summary>
    /// Clears the input test value.
    /// </summary>
    private async Task ClearInputTextAsync()
    {
        this.selectedItem = default(TItem);
        this.Value = string.Empty;
        await ValueChanged.InvokeAsync(this.Value);

        HidePanel();

        SetInputHasValue();

        CascadedEditContext?.NotifyFieldChanged(fieldIdentifier);

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(default(TItem));
    }

    /// <summary>
    /// Checks whether the input has value.
    /// </summary>
    private void SetInputHasValue() => this.inputHasValue = Value.Length > 0;

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
        string searchKey = this.Value;
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
    /// Disables autocomplete.
    /// </summary>
    public void Disable()
    {
        this.disabled = true;
    }

    /// <summary>
    /// Enables autocomplete.
    /// </summary>
    public void Enable()
    {
        this.disabled = false;
    }

    /// <summary>
    /// Refresh the autocomplete data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync() => await this.FilterDataAsync();

    /// <summary>
    /// Resets the autocomplete selection.
    /// </summary>
    public async Task ResetAsync() => await ClearInputTextAsync();

    #endregion Methods

    #region Properties

    [CascadingParameter] private EditContext CascadedEditContext { get; set; }

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

    [Parameter] public string Value { get; set; }

    [Parameter] public Expression<Func<string?>> ValueExpression { get; set; }

    #endregion Properties
}