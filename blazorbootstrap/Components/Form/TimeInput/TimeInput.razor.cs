namespace BlazorBootstrap;

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

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());
        base.BuildClasses(builder);
    }

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

        Attributes ??= new Dictionary<string, object>();

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
            await JS.InvokeVoidAsync("window.blazorBootstrap.timeInput.setValue", ElementId, formattedValue);

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

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    private string autoComplete => AutoComplete ? "true" : "false";

    /// <summary>
    /// Indicates whether the DateInput can complete the values automatically by the browser.
    /// </summary>
    [Parameter]
    public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Determines whether to restrict the user input to Min and Max range.
    /// If true, restricts the user input between the Min and Max range. Else accepts the user input.
    /// </summary>
    [Parameter]
    public bool EnableMinMax { get; set; }

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

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
    /// Gets or sets the expression.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
