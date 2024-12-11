namespace BlazorBootstrap;

/// <summary>
/// Blazor Bootstrap autocomplete component is a textbox that offers the users suggestions as they type from the data source. <br/>
/// It supports client-side and server-side filtering.
/// </summary>
/// <typeparam name="TItem">The type that contains the value of the input component</typeparam>
public partial class AutoComplete<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CancellationTokenSource cancellationTokenSource = default!;
    private Button closeButton = default!;

    private FieldIdentifier fieldIdentifier;

    private bool inputHasValue;
    private bool isDropdownShown;
    private IReadOnlyCollection<TItem>? items = null;
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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.autocomplete.dispose", Element); // NOTE: Always pass ElementRef
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.autocomplete.initialize", Element, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        // check the default value is assigned.
        if (Value is not null && Value.Length > 0)
            SetInputHasValue();

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Invoked by JavaScript when the autocomplete dropdown is hidden.
    /// </summary>

    [JSInvokable("bsHiddenAutocomplete")]
    public void BsHiddenAutocomplete()
    {
        if (isDropdownShown)
        {
            isDropdownShown = false;

            AdditionalAttributes.Remove(BootstrapAttributes.DataBootstrapToggle, out _);

            StateHasChanged();
        }
    }

    [JSInvokable("bsHideAutocomplete")]
    public void BsHideAutocomplete() { }

    [JSInvokable("bsShowAutocomplete")]
    public void BsShowAutocomplete() { }

    [JSInvokable("bsShownAutocomplete")]
    public void BsShownAutocomplete() { }

    /// <summary>
    /// Disables autocomplete.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables autocomplete.
    /// </summary>
    public void Enable() => Disabled = false;

    /// <summary>
    /// Refresh the autocomplete data.
    /// </summary>
    /// <returns>Task</returns>
    public Task RefreshDataAsync() => FilterDataAsync();

    /// <summary>
    /// Resets the autocomplete selection.
    /// </summary>
    public Task ResetAsync() => ClearInputTextAsync();

    /// <summary>
    /// Clears the input test value.
    /// </summary>
    private async Task ClearInputTextAsync()
    {
        selectedItem = default;
        selectedIndex = -1;
        items = Array.Empty<TItem>();
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
                totalCount = result.TotalCount ?? result.Data?.Count ?? 0;
            }
            else
            {
                items = Array.Empty<TItem>();
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

        AdditionalAttributes.Remove(BootstrapAttributes.DataBootstrapToggle);

        await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.autocomplete.hide", Element);
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
        items = Array.Empty<TItem>();
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
            selectedIndex = await JsRuntime.InvokeAsync<int>("window.blazorBootstrap.autocomplete.focusListItem", list, key, selectedIndex);
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

         AdditionalAttributes[BootstrapAttributes.DataBootstrapToggle] =  "dropdown";

        await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.autocomplete.show", Element);
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
                case var _ when String.Equals(parameter.Name, nameof(DataProvider), StringComparison.OrdinalIgnoreCase): DataProvider = (AutoCompleteDataProviderDelegate<TItem>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Disabled), StringComparison.OrdinalIgnoreCase): Disabled = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(EditContext), StringComparison.OrdinalIgnoreCase): EditContext = (EditContext)parameter.Value; break;  
                case var _ when String.Equals(parameter.Name, nameof(EmptyText), StringComparison.OrdinalIgnoreCase): EmptyText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LoadingText), StringComparison.OrdinalIgnoreCase): LoadingText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnChanged), StringComparison.OrdinalIgnoreCase): OnChanged = (EventCallback<TItem>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Placeholder), StringComparison.OrdinalIgnoreCase): Placeholder = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(PropertyName), StringComparison.OrdinalIgnoreCase): PropertyName = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (AutoCompleteSize)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(StringComparison), StringComparison.OrdinalIgnoreCase): StringComparison = (StringComparison)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(StringFilterOperator), StringComparison.OrdinalIgnoreCase): StringFilterOperator = (StringFilterOperator)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(Value), StringComparison.OrdinalIgnoreCase): Value = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ValueChanged), StringComparison.OrdinalIgnoreCase): ValueChanged = (EventCallback<string>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ValueExpression), StringComparison.OrdinalIgnoreCase): ValueExpression = (Expression<Func<string?>>)parameter.Value; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
      
    /// <summary>
    /// Gets or sets the data provider.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
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
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets the empty text.
    /// </summary>
    /// <remarks>
    /// Default value is 'No records found.'.
    /// </remarks>
    [Parameter]
    public string EmptyText { get; set; } = "No records found.";

    private string FieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the loading text.
    /// </summary>
    /// <remarks>
    /// Default value is 'Loading...'.
    /// </remarks>
    [Parameter]
    public string LoadingText { get; set; } = "Loading...";

    /// <summary>
    /// This event fires immediately when the autocomplete selection changes by the user.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> OnChanged { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public string PropertyName { get; set; } = null!;

    /// <summary>
    /// Gets selected item.
    /// </summary>
    public TItem SelectedItem => selectedItem = default!;

    /// <summary>
    /// Gets or sets the autocomplete size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="AutoCompleteSize.Default" />.
    /// </remarks>
    [Parameter]
    public AutoCompleteSize Size { get; set; } = AutoCompleteSize.Default;

    /// <summary>
    /// Gets or sets the StringComparison.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="StringComparison.OrdinalIgnoreCase" />.
    /// </remarks>
    [Parameter]
    public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets or sets the string filter operator.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="StringFilterOperator.Contains" />.
    /// </remarks>
    [Parameter]
    public StringFilterOperator StringFilterOperator { get; set; } = StringFilterOperator.Contains;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string Value { get; set; } = default!;

    /// <summary>
    /// This is event fires on every user keystroke that changes the textbox value.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// An expression that identifies the bound value.
    /// </summary>
    [Parameter] 
    public Expression<Func<string?>> ValueExpression { get; set; } = default!;

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
