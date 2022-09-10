using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
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

    private DotNetObjectReference<AutoComplete<TItem>> objRef;

    private FieldIdentifier fieldIdentifier;
    private string fieldCssClasses => CascadedEditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private bool inputHasValue;
    private bool isDropdownShown;
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

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());
        builder.Append(BootstrapClassProvider.ToAutoCompleteSize(this.Size));

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        this.disabled = this.Disabled;

        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.autocomplete.initialize", ElementRef, objRef); });
    }

    /// <summary>
    /// Shows autocomplete dropdown.
    /// </summary>
    private async Task ShowAsync()
    {
        isDropdownShown = true;

        if (Attributes is not null && !Attributes.TryGetValue(StringConstants.DataBootstrapToggle, out object? toggle))
            Attributes.Add(StringConstants.DataBootstrapToggle, "dropdown");

        await JS.InvokeVoidAsync("window.blazorBootstrap.autocomplete.show", ElementRef);
    }

    /// <summary>
    /// Hides autocomplete dropdown.
    /// </summary>
    private async Task HideAsync()
    {
        isDropdownShown = false;

        if (Attributes is not null && Attributes.TryGetValue(StringConstants.DataBootstrapToggle, out object? toggle))
            Attributes.Remove(StringConstants.DataBootstrapToggle);

        await JS.InvokeVoidAsync("window.blazorBootstrap.autocomplete.hide", ElementRef);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.autocomplete.dispose", ElementId);
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    [JSInvokable] public async Task bsShowAutocomplete() { }
    [JSInvokable] public async Task bsShownAutocomplete() { }
    [JSInvokable] public async Task bsHideAutocomplete() { }
    [JSInvokable]
    public async Task bsHiddenAutocomplete()
    {
        if (isDropdownShown)
        {
            isDropdownShown = false;

            if (Attributes is not null && Attributes.TryGetValue(StringConstants.DataBootstrapToggle, out object? toggle))
                Attributes.Remove(StringConstants.DataBootstrapToggle);

            StateHasChanged();
        }
    }

    private async Task OnInputChangedAsync(ChangeEventArgs args)
    {
        this.Value = args.Value.ToString();

        SetInputHasValue();

        if (inputHasValue)
        {
            await ShowAsync();
        }
        else
        {
            await HideAsync();
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

        await HideAsync();

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

        await HideAsync();

        SetInputHasValue();

        CascadedEditContext?.NotifyFieldChanged(fieldIdentifier);

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(default(TItem));
    }

    /// <summary>
    /// Checks whether the input has value.
    /// </summary>
    private void SetInputHasValue() => this.inputHasValue = Value.Length > 0;

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

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

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