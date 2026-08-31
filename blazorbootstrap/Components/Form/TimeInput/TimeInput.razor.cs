namespace BlazorBootstrap;

[AddedVersion("1.6.0")]
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

        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        await base.OnInitializedAsync();
    }

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
    /// Disables the time input so users cannot change its value.
    /// </summary>
    [AddedVersion("1.6.0")]
    [Description("Disables the time input so users cannot change its value.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the time input so users can change its value.
    /// </summary>
    [AddedVersion("1.6.0")]
    [Description("Enables the time input so users can change its value.")]
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
    /// Determines where the left input is greater than the right input.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>bool</returns>
    private bool IsLeftGreaterThanRight(object left, object right)
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
            await SafeInvokeVoidAsync("window.blazorBootstrap.timeInput.setValue", Id, formattedValue);

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private bool TryParseValue(object value, out TValue newValue)
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

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.FormControl, true));

    private string autoComplete => AutoComplete ? "true" : "false";

    /// <summary>
    /// Gets or sets whether browser autocomplete is enabled for the time input.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.6.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether browser autocomplete is enabled. When true, the browser may offer saved times for the input.")]
    [Parameter]
    public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets whether the time input is disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.6.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the time input is disabled. When true, users cannot change its value.")]
    [Parameter]
    public bool Disabled { get; set; }

    [AddedVersion("1.6.0")]
    [CascadingParameter] private EditContext EditContext { get; set; } = default!;
    [DefaultValue(false)]

    /// <summary>
    /// Gets or sets whether input is restricted to the configured minimum and maximum times.
    /// 
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Description("Gets or sets whether input is restricted to the configured minimum and maximum times. When true, times outside the range are rejected.")]
    [Parameter]
    public bool EnableMinMax { get; set; }

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the maximum permitted time.
    /// 
    /// </summary>
    [AddedVersion("1.6.0")]
    [Description("Gets or sets the maximum permitted time. It is enforced only when EnableMinMax is true.")]
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the minimum permitted time.
    /// 
    /// </summary>
    [AddedVersion("1.6.0")]
    [Description("Gets or sets the minimum permitted time. It is enforced only when EnableMinMax is true.")]
    [Parameter]
    public TValue Min { get; set; } = default!;

    /// <summary>
    /// Gets or sets placeholder text displayed when the input has no value.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("1.6.0")]
    [DefaultValue(null)]
    [Description("Gets or sets placeholder text displayed when the input has no value.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the current time value bound to the input.
    /// </summary>
    [AddedVersion("1.6.0")]
    [Description("Gets or sets the current time value bound to the input.")]
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Occurs whenever user input changes the time value, including changes produced by typing.
    /// </summary>
    [AddedVersion("1.6.0")]
    [Description("Fires whenever user input changes the time value, including changes produced by typing.")]
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value for validation and EditContext notifications.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.6.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound value for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
