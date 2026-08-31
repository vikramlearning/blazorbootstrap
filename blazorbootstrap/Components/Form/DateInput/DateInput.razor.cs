namespace BlazorBootstrap;

[AddedVersion("1.5.0")]
public partial class DateInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// Date format: yyyy-MM-dd.
    /// </summary>
    private readonly string defaultFormat = "yyyy-MM-dd";

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
                    && (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime)))
                    Value = min;
                else // DateOnly? / DateTime?
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

        if (!(typeof(TValue) == typeof(DateOnly)
              || typeof(TValue) == typeof(DateOnly?)
              || typeof(TValue) == typeof(DateTime)
              || typeof(TValue) == typeof(DateTime?)
             ))
            throw new InvalidOperationException($"{typeof(TValue)} is not supported.");

        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        Disabled = Disabled;

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
            await SetValueAsync(oldValue!, Value);
            oldValue = Value;
        }
    }

    /// <summary>
    /// Disables the date input so users cannot change its value.
    /// </summary>
    [AddedVersion("1.5.0")]
    [Description("Disables the date input so users cannot change its value.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the date input so users can change its value.
    /// </summary>
    [AddedVersion("1.5.0")]
    [Description("Enables the date input so users can change its value.")]
    public void Enable() => Disabled = false;

    private string GetFormattedValue(object value)
    {
        var formattedDate = "";

        try
        {
            if (value is null)
                return formattedDate;

            // DateOnly / DateOnly?
            if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateOnly?))
            {
                if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt)) formattedDate = dt.ToString(defaultFormat);
            }
            // DateTime / DateTime?
            else if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
            {
                var d = Convert.ToDateTime(value, CultureInfo.CurrentCulture); // TODO: update this with .NET 8 upgrade
                formattedDate = d.ToString(defaultFormat);
            }
        }
        catch (FormatException)
        {
            return formattedDate;
        }

        return formattedDate;
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

        // DateOnly / DateOnly?
        if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateOnly?))
        {
            if (DateTime.TryParse(left.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var ldt)
                && DateTime.TryParse(right.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var rdt))
            {
                var l = DateOnly.FromDateTime(ldt);
                var r = DateOnly.FromDateTime(rdt);

                return l > r;
            }
        }
        // DateTime / DateTime?
        else if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
        {
            var l = Convert.ToDateTime(left, CultureInfo.CurrentCulture);
            var r = Convert.ToDateTime(right, CultureInfo.CurrentCulture);

            return l > r;
        }

        return false;
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        // When pressing 0 first the component falls back to default value
        // We can avoid this by checking for an empty string first
        if (e.Value is string tmp && tmp != string.Empty) await SetValueAsync(oldValue, newValue);

        this.oldValue = Value;
    }

    private async Task SetValueAsync(TValue oldValue, object? newValue)
    {
        if (newValue is null || !TryParseValue(newValue, out var value))
        {
            if (EnableMinMax
                && min is not null
                && (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime)))
                Value = min;
            else // DateOnly? / DateTime?
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

        //this.formattedMax = EnableMinMax && max is not null ? GetFormattedValue(max) : string.Empty;
        //this.formattedMin = EnableMinMax && min is not null ? GetFormattedValue(min) : string.Empty;
        formattedValue = GetFormattedValue(Value!);

        if (oldValue!.Equals(Value))
            await SafeInvokeVoidAsync("window.blazorBootstrap.dateInput.setValue", Id, formattedValue);

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private bool TryParseValue(object value, out TValue newValue)
    {
        try
        {
            // DateOnly / DateOnly?
            if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateOnly?))
            {
                if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))
                {
                    newValue = (TValue)(object)DateOnly.FromDateTime(dt);

                    return true;
                }

                newValue = default!;

                return false;
            }
            // DateTime / DateTime?

            if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(DateTime), CultureInfo.CurrentCulture);

                return true;
            }

            newValue = default!;

            return false;
        }
        catch (Exception)
        {
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
    /// Gets or sets whether browser autocomplete is enabled for the date input.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.5.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether browser autocomplete is enabled. When true, the browser may offer saved dates for the input.")]
    [Parameter]
    public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets whether the date input is disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.5.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the date input is disabled. When true, users cannot change its value.")]
    [Parameter]
    public bool Disabled { get; set; }

    [AddedVersion("1.5.0")]
    [DefaultValue(false)]
    [CascadingParameter]
    private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether input is restricted to the configured minimum and maximum dates.
    /// 
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Description("Gets or sets whether input is restricted to the configured minimum and maximum dates. When true, dates outside the range are rejected.")]
    [Parameter]
    public bool EnableMinMax { get; set; }

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the maximum permitted date.
    /// 
    /// </summary>
    [AddedVersion("1.5.0")]
    [Description("Gets or sets the maximum permitted date. It is enforced only when EnableMinMax is true.")]
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the minimum permitted date.
    /// 
    /// </summary>
    [AddedVersion("1.5.0")]
    [Description("Gets or sets the minimum permitted date. It is enforced only when EnableMinMax is true.")]
    [Parameter]
    public TValue Min { get; set; } = default!;

    /// <summary>
    /// Gets or sets placeholder text displayed when the input has no value.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("1.5.0")]
    [DefaultValue(null)]
    [Description("Gets or sets placeholder text displayed when the input has no value.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the current date value bound to the input.
    /// </summary>
    [AddedVersion("1.5.0")]
    [Description("Gets or sets the current date value bound to the input.")]
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Occurs whenever user input changes the date value, including changes produced by typing.
    /// </summary>
    [AddedVersion("1.5.0")]
    [Description("Fires whenever user input changes the date value, including changes produced by typing.")]
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value for validation and EditContext notifications.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.5.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound value for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
