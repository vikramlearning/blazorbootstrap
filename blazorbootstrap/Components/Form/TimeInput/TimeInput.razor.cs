namespace BlazorBootstrap;


/// <summary>
/// Blazor Bootstrap <see cref="TimeInput{T}"/> component is constructed using an HTML input of type="time" which limits user input based on pre-defined parameters.
/// This component enables users to input a time using a text box with validation or a special time picker interface.
/// </summary>
/// <typeparam name="TValue">Time datatype to insert: <see cref="TimeOnly"/> and its <see cref="Nullable{T}"/>> variant are supported.</typeparam>
/// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TValue"/> isn't of type <see cref="TimeOnly"/> or its <see cref="Nullable{T}"/>> alternative.</exception>
/// <exception cref="InvalidOperationException">Thrown if <see cref="Min"/> is larger than <see cref="Max"/> and <see cref="EnableMinMax"/> is <see langword="true" /></exception>

public partial class TimeInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// Time format: HH:mm. 24-hours format.
    /// </summary>
    private readonly string defaultFormat = "HH:mm";

    private FieldIdentifier fieldIdentifier;

    private string formattedMax = default!;

    private string formattedMin = default!;

    private string formattedValue = default!;

    private TValue max = default!;

    private TValue min = default!;

    private TValue? oldValue;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var currentValue = Value;

            if (currentValue is null || !TryParseValue(currentValue, out var value))
            {
                if (EnableMinMax
                    && min is not null
                    && typeof(TValue) == typeof(TimeOnly))
                    Value = min;
                else // TimeOnly?
                    Value = default!;
            }
            else if (EnableMinMax && min is not null && IsLeftGreaterThanRight(min, Value!)) //  value < min
            {
                Value = EnableMinMax && min is not null ? min : default!;
            }
            else if (EnableMinMax && max is not null && IsLeftGreaterThanRight(Value!, max)) // value > max
            {
                Value = max;
            }
            else
            {
                Value = value;
            }

            formattedMax = EnableMinMax && max is not null ? GetFormattedValue(max) : string.Empty;
            formattedMin = EnableMinMax && min is not null ? GetFormattedValue(min) : string.Empty;
            formattedValue = GetFormattedValue(Value!);

            await ValueChanged.InvokeAsync(Value);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        max = Max;
        min = Min;

        if (EnableMinMax
            && min is not null
            && max is not null
            && IsLeftGreaterThanRight(min, max))
            throw new InvalidOperationException("The Min parameter value is greater than the Max parameter value.");

        if (!(typeof(TValue) == typeof(TimeOnly)
              || typeof(TValue) == typeof(TimeOnly?)
             ))
            throw new InvalidOperationException($"{typeof(TValue)} is not supported.");
        
        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        await base.OnInitializedAsync();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (EnableMinMax && !min!.Equals(Min))
        {
            min = Min;
            formattedMin = EnableMinMax && min is not null ? GetFormattedValue(min) : string.Empty;
        }

        if (EnableMinMax && !max!.Equals(Max))
        {
            max = Max;
            formattedMax = EnableMinMax && max is not null ? GetFormattedValue(max) : string.Empty;
        }

        if ((oldValue is null && Value is not null)
            || (oldValue is not null && Value is null)
            || !oldValue!.Equals(Value))
        {
            await SetValueAsync(oldValue!, Value!);
            oldValue = Value;
        }
    }

    /// <summary>
    /// Disables currency input.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables currency input.
    /// </summary>
    public void Enable() => Disabled = false;

    private string GetFormattedValue(object value)
    {
        var formattedTime = "";

        try
        {
            if (value is null)
                return formattedTime;

            // TimeOnly / TimeOnly?
            if (typeof(TValue) == typeof(TimeOnly) || typeof(TValue) == typeof(TimeOnly?))
                if (TimeOnly.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var t))
                    formattedTime = t.ToString(defaultFormat);
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"exception: {ex.Message}");

            return formattedTime;
        }

        return formattedTime;
    }

    /// <summary>
    /// Determines where the <paramref name="left"/>> input is greater than the <paramref name="right"/>> input.
    /// </summary>
    /// <param name="left">Left hand object</param>
    /// <param name="right">Right hand object</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/></returns>
    private static bool IsLeftGreaterThanRight(object? left, object? right)
    {
        if (left is null || right is null)
            return false;

        // TimeOnly / TimeOnly?
        if (typeof(TValue) == typeof(TimeOnly) || typeof(TValue) == typeof(TimeOnly?))
            if (TimeOnly.TryParse(left.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var lt)
                && TimeOnly.TryParse(right.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var rt))
                return lt > rt;

        return false;
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        await SetValueAsync(oldValue, newValue);

        this.oldValue = Value;
    }

    private async Task SetValueAsync(TValue oldValue, object? newValue)
    {
        if (newValue is null || !TryParseValue(newValue, out var value))
        {
            if (EnableMinMax
                && min is not null
                && typeof(TValue) == typeof(TimeOnly))
                Value = min;
            else // TimeOnly?
                Value = default!;
        }
        else if (EnableMinMax && min is not null && IsLeftGreaterThanRight(min, value!)) //  value < min
        {
            Value = min;
        }
        else if (EnableMinMax && max is not null && IsLeftGreaterThanRight(value!, max)) // value > max
        {
            Value = max;
        }
        else
        {
            Value = value;
        }

        formattedValue = GetFormattedValue(Value!);

        if (oldValue!.Equals(Value))
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.timeInput.setValue", Id, formattedValue);

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private static bool TryParseValue(object value, out TValue newValue)
    {
        try
        {
            // TimeOnly / TimeOnly?
            if (typeof(TValue) == typeof(TimeOnly) || typeof(TValue) == typeof(TimeOnly?))
            {
                if (TimeOnly.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var time))
                {
                    newValue = (TValue)(object)time;

                    return true;
                }

                newValue = default!;

                return false;
            }

            newValue = default!;

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"exception: {ex.Message}");
            newValue = default!;

            return false;
        }
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
                case var _ when String.Equals(parameter.Name, nameof(AutoComplete), StringComparison.OrdinalIgnoreCase): AutoComplete = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Disabled), StringComparison.OrdinalIgnoreCase): Disabled = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(EditContext), StringComparison.OrdinalIgnoreCase): EditContext = (EditContext)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(EnableMinMax), StringComparison.OrdinalIgnoreCase): EnableMinMax = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Max), StringComparison.OrdinalIgnoreCase): Max = (TValue)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Min), StringComparison.OrdinalIgnoreCase): Min = (TValue)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Placeholder), StringComparison.OrdinalIgnoreCase): Placeholder = (string)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(Value), StringComparison.OrdinalIgnoreCase): Value = (TValue)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ValueChanged), StringComparison.OrdinalIgnoreCase): ValueChanged = (EventCallback<TValue>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ValueExpression), StringComparison.OrdinalIgnoreCase): ValueExpression = (Expression<Func<TValue>>)parameter.Value; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// If <see langword="true" />, DateInput can complete the values automatically by the browser.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AutoComplete { get; set; }

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
    /// Determines whether to restrict the user input to Min and Max range.
    /// If <see langword="true" />, restricts the user input between the Min and Max range. Else accepts the user input.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool EnableMinMax { get; set; }

    private string FieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the max.
    /// Allowed format is hh:mm.
    /// </summary>
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the min.
    /// Allowed format is hh:mm.
    /// </summary>
    [Parameter]
    public TValue Min { get; set; } = default!;

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// This event fired on every user keystroke that changes the TimeInput value.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    ///  An expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; } = default!;


    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
