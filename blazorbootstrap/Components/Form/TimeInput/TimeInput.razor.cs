namespace BlazorBootstrap;

public partial class TimeInput<TValue> : BaseComponent
{
    #region Events

    /// <summary>
    /// This event fired on every user keystroke that changes the TimeInput value.
    /// </summary>
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    #endregion

    #region Members

    /// <summary>
    /// Time format: HH:mm. 24-hours format.
    /// </summary>
    private string defaultFormat = "HH:mm";

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private string autoComplete => this.AutoComplete ? "true" : "false";

    private bool disabled;

    private TValue max;

    private TValue min;

    private string formattedMax;

    private string formattedMin;

    private string formattedValue;

    private bool isFirstRender = true;

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());
        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        this.max = Max;
        this.min = Min;

        if (EnableMinMax
            && min is not null
            && max is not null
            && IsLeftGreaterThanRight(min, max))
            throw new InvalidOperationException("The Min parameter value is greater than the Max parameter value.");

        if (!(typeof(TValue) == typeof(TimeOnly)
            || typeof(TValue) == typeof(TimeOnly?)
            ))
            throw new InvalidOperationException($"{typeof(TValue)} is not supported.");

        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        this.disabled = this.Disabled;

        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        Console.WriteLine($"OnParametersSet called...");
        if (EnableMinMax && !min.Equals(Min))
        {
            Console.WriteLine($"OnParametersSet 1 - min: {min}, Min: {Min}");
            min = Min;
            this.formattedMin = EnableMinMax && min is not null ? GetFormattedValue(min) : string.Empty;
        }

        if (EnableMinMax && !max.Equals(Max))
        {
            Console.WriteLine($"OnParametersSet 2 - max: {max}, Max: {Max}");
            max = Max;
            this.formattedMax = EnableMinMax && max is not null ? GetFormattedValue(max) : string.Empty;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Console.WriteLine($"OnAfterRenderAsync called...");
            var currentValue = Value;

            Console.WriteLine($"OnAfterRenderAsync 1 - currentValue: {currentValue}");
            if (currentValue is null || !TryParseValue(currentValue, out TValue value))
            {
                if (EnableMinMax
                    && min is not null
                    && (typeof(TValue) == typeof(TimeOnly)))
                {
                    Value = min;
                    Console.WriteLine($"OnAfterRenderAsync 2 - Value: {Value}");
                }
                else // TimeOnly?
                {
                    Value = default!;
                    Console.WriteLine($"OnAfterRenderAsync 3 - Value: {Value}");
                }
            }
            else if (EnableMinMax && min is not null && IsLeftGreaterThanRight(min, Value)) //  value < min
            {
                Value = EnableMinMax && min is not null ? min : default!;
                Console.WriteLine($"OnAfterRenderAsync 4 - Value: {Value}");
            }
            else if (EnableMinMax && max is not null && IsLeftGreaterThanRight(Value, max)) // value > max
            {
                Value = max;
                Console.WriteLine($"OnAfterRenderAsync 5 - Value: {Value}");
            }
            else
            {
                Value = value;
                Console.WriteLine($"OnAfterRenderAsync 6 - Value: {Value}");
            }

            this.formattedMax = EnableMinMax && max is not null ? GetFormattedValue(max) : string.Empty;
            this.formattedMin = EnableMinMax && min is not null ? GetFormattedValue(min) : string.Empty;
            this.formattedValue = GetFormattedValue(Value);

            Console.WriteLine($"OnAfterRenderAsync 7 - formattedValue: {formattedValue}");

            await ValueChanged.InvokeAsync(Value);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        if (newValue is null || !TryParseValue(newValue, out TValue value))
        {
            if (EnableMinMax
                && min is not null
                && (typeof(TValue) == typeof(TimeOnly)))
            {
                Value = min;
            }
            else // TimeOnly?
            {
                Value = default!;
            }
        }
        else if (EnableMinMax && min is not null && IsLeftGreaterThanRight(min, value)) //  value < min
        {
            Value = min;
        }
        else if (EnableMinMax && max is not null && IsLeftGreaterThanRight(value, max)) // value > max
        {
            Value = max;
        }
        else
        {
            Value = value;
        }

        //this.formattedMax = EnableMinMax && max is not null ? GetFormattedValue(max) : string.Empty;
        //this.formattedMin = EnableMinMax && min is not null ? GetFormattedValue(min) : string.Empty;
        this.formattedValue = GetFormattedValue(Value);

        if (oldValue.Equals(Value))
            await JS.InvokeVoidAsync("window.blazorBootstrap.timeInput.setValue", ElementId, this.formattedValue);

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private bool TryParseValue(object value, out TValue newValue)
    {
        try
        {
            Console.WriteLine($"TryParseValue 1 - value: {value}");
            // TimeOnly / TimeOnly?
            if (typeof(TValue) == typeof(TimeOnly) || typeof(TValue) == typeof(TimeOnly?))
            {
                if (TimeOnly.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out TimeOnly time))
                {
                    Console.WriteLine($"TryParseValue 2 - time: {time}");
                    newValue = (TValue)(object)(time);
                    return true;
                }
                else
                {
                    Console.WriteLine($"TryParseValue 3 - value: {value}");
                }

                newValue = default!;
                return false;
            }
            else
            {
                Console.WriteLine($"TryParseValue 4 - value: {value}");
            }

            newValue = default!;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TryParseValue 5 - error: {ex.Message}");
            newValue = default!;
            return false;
        }
    }

    /// <summary>
    /// Determines where the left input is greater than the right input.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>bool</returns>
    private bool IsLeftGreaterThanRight(object left, object right)
    {
        Console.WriteLine($"IsLeftGreaterThanRight called...");
        Console.WriteLine($"IsLeftGreaterThanRight 1 - left: {left}, right: {right}");

        if (left is null || right is null)
            return false;

        // TimeOnly / TimeOnly?
        if (typeof(TValue) == typeof(TimeOnly) || typeof(TValue) == typeof(TimeOnly?))
        {
            if (TimeOnly.TryParse(left.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out TimeOnly lt)
                && TimeOnly.TryParse(right.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out TimeOnly rt))
            {
                Console.WriteLine($"IsLeftGreaterThanRight 2 - lt: {lt}, rt: {rt}, result: {lt > rt}");
                return lt > rt;
            }
            else
            {
                Console.WriteLine($"IsLeftGreaterThanRight 3 - left: {left}, right: {right}");
            }
        }
        else
        {
            Console.WriteLine($"IsLeftGreaterThanRight 4 - left: {left}, right: {right}");
        }

        return false;
    }

    private string GetFormattedValue(object value)
    {
        string formattedTime = "";

        try
        {
            Console.WriteLine($"GetFormattedValue 1 - value: {value}");
            if (value is null)
                return formattedTime;

            // TimeOnly / TimeOnly?
            if (typeof(TValue) == typeof(TimeOnly) || typeof(TValue) == typeof(TimeOnly?))
            {
                if (TimeOnly.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out TimeOnly t))
                {
                    Console.WriteLine($"GetFormattedValue 2 - t: {t}");
                    formattedTime = t.ToString(defaultFormat);
                    Console.WriteLine($"GetFormattedValue 3 - value: {formattedTime}");
                }
                else
                {
                    Console.WriteLine($"GetFormattedValue 4 - value: {value}");
                }
            }
            else
            {
                Console.WriteLine($"GetFormattedValue 5 - value: {value}");
            }
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"GetFormattedValue 6 - exception: {ex.Message}");
            return formattedTime;
        }

        return formattedTime;
    }

    /// <summary>
    /// Disables currency input.
    /// </summary>
    public void Disable()
    {
        this.disabled = true;
    }

    /// <summary>
    /// Enables currency input.
    /// </summary>
    public void Enable()
    {
        this.disabled = false;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Indicates whether the DateInput can complete the values automatically by the browser.
    /// </summary>
    [Parameter] public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; }

    /// <summary>
    /// Determines whether to restrict the user input to Min and Max range.
    /// If true, restricts the user input between the Min and Max range. Else accepts the user input.
    /// </summary>
    [Parameter] public bool EnableMinMax { get; set; }

    /// <summary>
    /// Gets or sets the max.
    /// Allowed format is hh:mm.
    /// </summary>
    [Parameter] public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the min.
    /// Allowed format is hh:mm.
    /// </summary>
    [Parameter] public TValue Min { get; set; } = default!;

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter] public TValue Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the expression.
    /// </summary>
    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
