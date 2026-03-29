namespace BlazorBootstrap;

public partial class AutoComplete<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CancellationTokenSource cancellationTokenSource = default!;
    private Button closeButton = default!;

    private FieldIdentifier fieldIdentifier;

    private bool inputHasValue;
    private bool isDropdownShown;
    private IEnumerable<TItem>? items = null;
    private ElementReference list; // ul element reference

    private DotNetObjectReference<AutoComplete<TItem>> objRef = default!;
    private bool searchInProgress;
    private int selectedIndex = -1;
    private TItem? selectedItem;
    private int totalCount;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            cancellationTokenSource?.Dispose();

            try
            {
                if (IsRenderComplete)
                    await SafeInvokeVoidAsync("window.blazorBootstrap.autocomplete.dispose", Element); // NOTE: Always pass ElementRef
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await SafeInvokeVoidAsync("window.blazorBootstrap.autocomplete.initialize", Element, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        // check the default value is assigned.
        if (Value is not null && Value.Length > 0)
            SetInputHasValue();

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public void bsHiddenAutocomplete()
    {
        if (isDropdownShown)
        {
            isDropdownShown = false;

            if (AdditionalAttributes is not null && AdditionalAttributes.TryGetValue(BootstrapAttributes.DataBootstrapToggle, out _))
                AdditionalAttributes.Remove(BootstrapAttributes.DataBootstrapToggle);

            StateHasChanged();
        }
    }

    [JSInvokable]
    public void bsHideAutocomplete() { }

    [JSInvokable]
    public void bsShowAutocomplete() { }

    [JSInvokable]
    public void bsShownAutocomplete() { }

    /// <summary>
    /// Disables autocomplete.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Disables autocomplete.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables autocomplete.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Enables autocomplete.")]
    public void Enable() => Disabled = false;

    /// <summary>
    /// Refresh the autocomplete data.
    /// </summary>
    /// <returns>Task</returns>
    [AddedVersion("1.0.0")]
    [Description("Refresh the autocomplete data.")]
    public async Task RefreshDataAsync() => await FilterDataAsync();

    /// <summary>
    /// Resets the autocomplete selection.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Resets the autocomplete selection.")]
    public async Task ResetAsync() => await ClearInputTextAsync();

    /// <summary>
    /// Clears the input test value.
    /// </summary>
    private async Task ClearInputTextAsync()
    {
        selectedItem = default;
        selectedIndex = -1;
        items = Enumerable.Empty<TItem>();
        Value = string.Empty;
        await ValueChanged.InvokeAsync(Value);

        await HideAsync();

        SetInputHasValue();

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(default);

        await Element.FocusAsync();
    }

    private async Task FilterDataAsync(CancellationToken cancellationToken = default)
    {
        var searchKey = Value;

        if (string.IsNullOrWhiteSpace(searchKey))
            return;

        var request = new AutoCompleteDataProviderRequest<TItem> { Filter = new FilterItem(PropertyName, searchKey, GetFilterOperator(), StringComparison), CancellationToken = cancellationToken };

        if (DataProvider != null)
        {
            var result = await DataProvider.Invoke(request);

            if (result is not null)
            {
                items = result.Data;
                totalCount = result.TotalCount ?? result.Data!.Count();
            }
            else
            {
                items = Enumerable.Empty<TItem>();
                totalCount = 0;
            }
        }
    }

    /// <summary>
    /// Get equivalent filter operator.
    /// </summary>
    /// <returns>FilterOperator</returns>
    private FilterOperator GetFilterOperator() =>
        StringFilterOperator switch
        {
            StringFilterOperator.Equals => FilterOperator.Equals,
            StringFilterOperator.Contains => FilterOperator.Contains,
            StringFilterOperator.DoesNotContain => FilterOperator.DoesNotContain,
            StringFilterOperator.StartsWith => FilterOperator.StartsWith,
            StringFilterOperator.EndsWith => FilterOperator.EndsWith,
            _ => FilterOperator.Contains
        };

    private string? GetPropertyValue(TItem item)
    {
        if (string.IsNullOrWhiteSpace(PropertyName))
            return string.Empty;

        var propertyInfo = typeof(TItem).GetProperty(PropertyName);

        return propertyInfo?.GetValue(item)?.ToString();
    }

    /// <summary>
    /// Hides autocomplete dropdown.
    /// </summary>
    private async Task HideAsync()
    {
        isDropdownShown = false;

        if (AdditionalAttributes is not null && AdditionalAttributes.TryGetValue(BootstrapAttributes.DataBootstrapToggle, out _))
            AdditionalAttributes.Remove(BootstrapAttributes.DataBootstrapToggle);

        await SafeInvokeVoidAsync("window.blazorBootstrap.autocomplete.hide", Element);
    }

    private async Task OnInputChangedAsync(ChangeEventArgs args)
    {
        searchInProgress = true;

        selectedIndex = -1;
        Value = args?.Value?.ToString()!;

        SetInputHasValue();

        if (inputHasValue)
            await ShowAsync();
        else
            await HideAsync();

        closeButton?.ShowLoading();

        if (cancellationTokenSource is not null
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

        searchInProgress = false;
    }

    private async Task OnItemSelectedAsync(TItem item)
    {
        selectedItem = item;
        selectedIndex = -1;
        items = Enumerable.Empty<TItem>();
        Value = GetPropertyValue(item)!;
        await ValueChanged.InvokeAsync(Value);

        await HideAsync();

        SetInputHasValue();

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        if (OnChanged.HasDelegate)
            await OnChanged.InvokeAsync(item);
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        var key = args.Code is not null ? args.Code : args.Key;

        if (key is "ArrowDown" or "ArrowUp" or "Home" or "End")
            selectedIndex = await JSRuntime.InvokeAsync<int>("window.blazorBootstrap.autocomplete.focusListItem", list, key, selectedIndex);
        else if (key == "Enter")
            if (selectedIndex >= 0 && selectedIndex <= items!.Count() - 1)
                await OnItemSelectedAsync(items!.ElementAt(selectedIndex));
        // TODO: check anything needs to be handled here
    }

    /// <summary>
    /// Checks whether the input has value.
    /// </summary>
    private void SetInputHasValue() => inputHasValue = Value is not null && Value.Length > 0;

    /// <summary>
    /// Shows autocomplete dropdown.
    /// </summary>
    private async Task ShowAsync()
    {
        isDropdownShown = true;

        if (AdditionalAttributes is not null && !AdditionalAttributes.TryGetValue(BootstrapAttributes.DataBootstrapToggle, out _))
            AdditionalAttributes.Add(BootstrapAttributes.DataBootstrapToggle, "dropdown");

        await SafeInvokeVoidAsync("window.blazorBootstrap.autocomplete.show", Element);
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.FormControl, true),
            (Size.ToAutoCompleteSizeClass(), true));

    /// <summary>
    /// Gets or sets the data provider.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the data provider.")]
    [EditorRequired]
    [Parameter]
    public AutoCompleteDataProviderDelegate<TItem>? DataProvider { get; set; }

    /// <summary>
    /// Gets all Style attributes for the autocomplete delete button.
    /// </summary>
    private string DeleteButtonStyle =>
        Size switch
        {
            AutoCompleteSize.Small => "z-index: 100; top: -2px;",
            AutoCompleteSize.Default => "z-index: 100; top: 2px;",
            AutoCompleteSize.Large => "z-index: 100; top: 7px;",
            _ => "z-index: 100;"
        };

    /// <summary>
    /// Gets or sets the disabled state.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the disabled state.")]
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] 
    private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets the empty text.
    /// <para>
    /// Default value is 'No records found.'.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue("No records found.")]
    [Description("Gets or sets the empty text.")]
    [Parameter]
    public string EmptyText { get; set; } = "No records found.";

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the loading text.
    /// <para>
    /// Default value is 'Loading...'.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue("Loading...")]
    [Description("Gets or sets the loading text.")]
    [Parameter]
    public string LoadingText { get; set; } = "Loading...";

    /// <summary>
    /// This event fires immediately when the autocomplete selection changes by the user.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires immediately when the autocomplete selection changes by the user.")]
    [Parameter]
    public EventCallback<TItem> OnChanged { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the placeholder.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the property name.")]
    [EditorRequired]
    [Parameter]
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets selected item.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets selected item.")]
    public TItem? SelectedItem => selectedItem;

    /// <summary>
    /// Gets or sets the autocomplete size.
    /// <para>
    /// Default value is <see cref="AutoCompleteSize.Default" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(AutoCompleteSize.Default)]
    [Description("Gets or sets the autocomplete size.")]
    [Parameter]
    public AutoCompleteSize Size { get; set; } = AutoCompleteSize.Default;

    /// <summary>
    /// Gets or sets the StringComparison.
    /// <para>
    /// Default value is <see cref="StringComparison.OrdinalIgnoreCase" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(StringComparison.OrdinalIgnoreCase)]
    [Description("Gets or sets the StringComparison.")]
    [Parameter]
    public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets or sets the string filter operator.
    /// <para>
    /// Default value is <see cref="StringFilterOperator.Contains" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(StringFilterOperator.Contains)]
    [Description("Gets or sets the string filter operator.")]
    [Parameter]
    public StringFilterOperator StringFilterOperator { get; set; } = StringFilterOperator.Contains;

    /// <summary>
    /// Gets or sets the value.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the value.")]
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// This is event fires on every user keystroke that changes the textbox value.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This is event fires on every user keystroke that changes the textbox value.")]
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the value expression.
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the value expression.")]
    [Parameter] 
    public Expression<Func<string?>>? ValueExpression { get; set; }

    #endregion
}
