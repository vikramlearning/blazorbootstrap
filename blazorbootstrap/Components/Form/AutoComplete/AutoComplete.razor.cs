﻿namespace BlazorBootstrap;

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

    private DotNetObjectReference<AutoComplete<TItem>> objRef = default!;

    private FieldIdentifier fieldIdentifier;
    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private bool inputHasValue;
    private bool isDropdownShown;
    private IEnumerable<TItem> items = null;
    private int totalCount;
    private TItem? selectedItem;
    private int selectedIndex = -1;
    private bool disabled;
    private Button closeButton = default!;
    private ElementReference list; // ul element reference

    /// <summary>
    /// Gets selected item.
    /// </summary>
    public TItem SelectedItem => selectedItem = default!;

    private CancellationTokenSource cancellationTokenSource = default!;

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

        // check the default value is assigned.
        if (Value is not null && Value.Length > 0)
            SetInputHasValue();

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
        this.selectedIndex = -1;
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

        if(cancellationTokenSource is not null 
            && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        cancellationTokenSource = new CancellationTokenSource();

        var token = cancellationTokenSource.Token;
        await Task.Delay(300, token); // 300ms timeout for the debouncing
        await FilterDataAsync(token);

        closeButton?.HideLoading();
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        var key = args.Code is not null ? args.Code : args.Key;

        if(key == "ArrowDown" || key == "ArrowUp" || key == "Home" || key == "End")
        {
            selectedIndex = await JS.InvokeAsync<int>("window.blazorBootstrap.autocomplete.focusListItem", list, key, selectedIndex);
        }
        else if(key == "Enter")
        {
            if(selectedIndex >= 0 && selectedIndex <= items.Count() - 1)
            {
                await OnItemSelectedAsync(items.ElementAt(selectedIndex));
            }
        }
        else
        {
            // TODO: check anything needs to be handled here
        }
    }

    private async Task OnItemSelectedAsync(TItem item)
    {
        this.selectedItem = item;
        this.selectedIndex = -1;
        this.items = Enumerable.Empty<TItem>();
        this.Value = this.GetPropertyValue(item);
        await ValueChanged.InvokeAsync(this.Value);

        await HideAsync();

        SetInputHasValue();

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(item);
    }

    /// <summary>
    /// Clears the input test value.
    /// </summary>
    private async Task ClearInputTextAsync()
    {
        this.selectedItem = default(TItem);
        this.selectedIndex = -1;
        this.items = Enumerable.Empty<TItem>();
        this.Value = string.Empty;
        await ValueChanged.InvokeAsync(this.Value);

        await HideAsync();

        SetInputHasValue();

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(default(TItem));

        await ElementRef.FocusAsync();
    }

    /// <summary>
    /// Checks whether the input has value.
    /// </summary>
    private void SetInputHasValue() => this.inputHasValue = Value is not null && Value.Length > 0;

    private string? GetPropertyValue(TItem item)
    {
        if (string.IsNullOrWhiteSpace(this.PropertyName))
            return string.Empty;

        var propertyInfo = typeof(TItem).GetProperty(this.PropertyName);
        return propertyInfo?.GetValue(item)?.ToString();
    }

    /// <summary>
    /// Get equivalent filter operator.
    /// </summary>
    /// <returns>FilterOperator</returns>
    private FilterOperator GetFilterOperator()
    {
        return this.StringFilterOperator switch
        {
            BlazorBootstrap.StringFilterOperator.Equals => FilterOperator.Equals,
            BlazorBootstrap.StringFilterOperator.Contains => FilterOperator.Contains,
            BlazorBootstrap.StringFilterOperator.StartsWith => FilterOperator.StartsWith,
            BlazorBootstrap.StringFilterOperator.EndsWith => FilterOperator.EndsWith,
            _ => FilterOperator.Contains,
        };
    }

    private async Task FilterDataAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        string searchKey = this.Value;
        if (string.IsNullOrWhiteSpace(searchKey))
            return;

        var request = new AutoCompleteDataProviderRequest<TItem>
        {
            Filter = new FilterItem(this.PropertyName, searchKey, GetFilterOperator(), this.StringComparison),
            CancellationToken = cancellationToken
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
                items = Enumerable.Empty<TItem>();
                totalCount = 0;
            }
        }
    }

    /// <summary>
    /// Disables autocomplete.
    /// </summary>
    public void Disable() => this.disabled = true;

    /// <summary>
    /// Enables autocomplete.
    /// </summary>
    public void Enable() => this.disabled = false;

    /// <summary>
    /// Refresh the autocomplete data.
    /// </summary>
    /// <returns>Task</returns>
    public async Task RefreshDataAsync() => await this.FilterDataAsync();

    /// <summary>
    /// Resets the autocomplete selection.
    /// </summary>
    public async Task ResetAsync() => await ClearInputTextAsync();

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            cancellationTokenSource?.Dispose();
            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.autocomplete.dispose", ElementRef); }); // NOTE: Always pass ElementRef
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// DataProvider is for items to render. 
    /// The provider should always return an instance of 'AutoCompleteDataProviderResult', and 'null' is not allowed.
    /// </summary>
    [Parameter, EditorRequired] public AutoCompleteDataProviderDelegate<TItem> DataProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter]
    public bool Disabled
    {
        get => disabled;
        set => disabled = value;
    }

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

    /// <summary>
    /// Gets or sets the StringComparison.
    /// </summary>
    [Parameter] public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets or sets the string filter operator.
    /// </summary>
    [Parameter] public StringFilterOperator StringFilterOperator { get; set; } = StringFilterOperator.Contains;

    [Parameter] public string Value { get; set; } = default!;

    [Parameter] public Expression<Func<string?>> ValueExpression { get; set; } = default!;

    #endregion Properties
}