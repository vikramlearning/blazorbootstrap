namespace BlazorBootstrap;

[AddedVersion("1.0.0")]
public partial class NumberInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CultureInfo cultureInfo = default!;

    private FieldIdentifier fieldIdentifier;

    private string step = default!;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SafeInvokeVoidAsync("window.blazorBootstrap.numberInput.initialize", Id, isFloatingNumber(), AllowNegativeNumbers, cultureInfo.NumberFormat.NumberDecimalSeparator);

            var currentValue = Value; // object

            if (currentValue is null || !TryParseValue(currentValue, out var value))
                Value = default!;
            else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, Value)) // value < min
                Value = Min;
            else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(Value, Max)) // value > max
                Value = Max;
            else
                Value = value;

            await ValueChanged.InvokeAsync(Value);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        if (IsLeftGreaterThanRight(Min, Max))
            throw new InvalidOperationException("The Min parameter value is greater than the Max parameter value.");

        if (!(typeof(TValue) == typeof(sbyte)
              || typeof(TValue) == typeof(sbyte?)
              || typeof(TValue) == typeof(short)
              || typeof(TValue) == typeof(short?)
              || typeof(TValue) == typeof(int)
              || typeof(TValue) == typeof(int?)
              || typeof(TValue) == typeof(long)
              || typeof(TValue) == typeof(long?)
              || typeof(TValue) == typeof(float)
              || typeof(TValue) == typeof(float?)
              || typeof(TValue) == typeof(double)
              || typeof(TValue) == typeof(double?)
              || typeof(TValue) == typeof(decimal)
              || typeof(TValue) == typeof(decimal?)
             ))
            throw new InvalidOperationException($"{typeof(TValue)} is not supported.");

        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        step = Step.HasValue ? $"{Step.Value}" : "any";

        try
        {
            cultureInfo = new CultureInfo(Locale);
        }
        catch (CultureNotFoundException)
        {
            cultureInfo = new CultureInfo("en-US");
        }

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Disables the number input so users cannot change its value.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Disables the number input so users cannot change its value.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the number input so users can change its value.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Enables the number input so users can change its value.")]
    public void Enable() => Disabled = false;

    private string GetInvariantNumber(TValue value)
    {
        if (value is null) return string.Empty;

        if (value is float floatValue) return floatValue.ToString(CultureInfo.InvariantCulture);

        if (value is double doubleValue) return doubleValue.ToString(CultureInfo.InvariantCulture);

        if (value is decimal decimalValue) return decimalValue.ToString(CultureInfo.InvariantCulture);

        // All numbers without decimal places work fine by default
        return value?.ToString() ?? string.Empty;
    }

    private bool isFloatingNumber() =>
        typeof(TValue) == typeof(float)
        || typeof(TValue) == typeof(float?)
        || typeof(TValue) == typeof(double)
        || typeof(TValue) == typeof(double?)
        || typeof(TValue) == typeof(decimal)
        || typeof(TValue) == typeof(decimal?);

    /// <summary>
    /// Determines where the left input is greater than the right input.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>bool</returns>
    private bool IsLeftGreaterThanRight(TValue left, TValue right)
    {
        // sbyte
        if (typeof(TValue) == typeof(sbyte))
        {
            var l = Convert.ToSByte(left);
            var r = Convert.ToSByte(right);

            return l > r;
        }
        // sbyte?

        if (typeof(TValue) == typeof(sbyte?))
        {
            var l = left as sbyte?;
            var r = right as sbyte?;

            return l.HasValue && r.HasValue && l > r;
        }
        // short / int16

        if (typeof(TValue) == typeof(short))
        {
            var l = Convert.ToInt16(left);
            var r = Convert.ToInt16(right);

            return l > r;
        }
        // short? / int16?

        if (typeof(TValue) == typeof(short?))
        {
            var l = left as short?;
            var r = right as short?;

            return l.HasValue && r.HasValue && l > r;
        }
        // int

        if (typeof(TValue) == typeof(int))
        {
            var l = Convert.ToInt32(left);
            var r = Convert.ToInt32(right);

            return l > r;
        }
        // int?

        if (typeof(TValue) == typeof(int?))
        {
            var l = left as int?;
            var r = right as int?;

            return l.HasValue && r.HasValue && l > r;
        }
        // long

        if (typeof(TValue) == typeof(long))
        {
            var l = Convert.ToInt64(left);
            var r = Convert.ToInt64(right);

            return l > r;
        }
        // long?

        if (typeof(TValue) == typeof(long?))
        {
            var l = left as long?;
            var r = right as long?;

            return l.HasValue && r.HasValue && l > r;
        }
        // float / single

        if (typeof(TValue) == typeof(float))
        {
            var l = Convert.ToSingle(left);
            var r = Convert.ToSingle(right);

            return l > r;
        }
        // float? / single?

        if (typeof(TValue) == typeof(float?))
        {
            var l = left as float?;
            var r = right as float?;

            return l.HasValue && r.HasValue && l > r;
        }
        // double

        if (typeof(TValue) == typeof(double))
        {
            var l = Convert.ToDouble(left);
            var r = Convert.ToDouble(right);

            return l > r;
        }
        // double?

        if (typeof(TValue) == typeof(double?))
        {
            var l = left as double?;
            var r = right as double?;

            return l.HasValue && r.HasValue && l > r;
        }
        // decimal

        if (typeof(TValue) == typeof(decimal))
        {
            var l = Convert.ToDecimal(left);
            var r = Convert.ToDecimal(right);

            return l > r;
        }
        // decimal?

        if (typeof(TValue) == typeof(decimal?))
        {
            var l = left as decimal?;
            var r = right as decimal?;

            return l.HasValue && r.HasValue && l > r;
        }

        return false;
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        if (newValue is null || !TryParseValue(newValue, out var value))
            Value = default!;
        else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, value)) // value < min
            Value = Min;
        else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(value, Max)) // value > max
            Value = Max;
        else
            Value = value;

        if (oldValue!.Equals(Value))
            await SafeInvokeVoidAsync("window.blazorBootstrap.numberInput.setValue", Id, Value);

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private bool TryParseValue(object value, out TValue newValue)
    {
        try
        {
            // sbyte? / sbyte
            if (typeof(TValue) == typeof(sbyte?) || typeof(TValue) == typeof(sbyte))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(sbyte));

                return true;
            }
            // short? / short

            if (typeof(TValue) == typeof(short?) || typeof(TValue) == typeof(short))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(short));

                return true;
            }
            // int? / int

            if (typeof(TValue) == typeof(int?) || typeof(TValue) == typeof(int))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(int));

                return true;
            }
            // long? / long

            if (typeof(TValue) == typeof(long?) || typeof(TValue) == typeof(long))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(long));

                return true;
            }
            // float? / float

            if (typeof(TValue) == typeof(float?) || typeof(TValue) == typeof(float))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(float), CultureInfo.InvariantCulture);

                return true;
            }
            // double? / double

            if (typeof(TValue) == typeof(double?) || typeof(TValue) == typeof(double))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(double), CultureInfo.InvariantCulture);

                return true;
            }
            // decimal? / decimal

            if (typeof(TValue) == typeof(decimal?) || typeof(TValue) == typeof(decimal))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(decimal), CultureInfo.InvariantCulture);

                return true;
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
        BuildClassNames(Class,
            (BootstrapClass.FormControl, true),
            (TextAlignment.ToTextAlignmentClass(), TextAlignment != Alignment.None));

    /// <summary>
    /// If <see langword="true" />, allows negative numbers.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether users can enter negative numbers. When false, negative input is rejected during parsing.")]
    [Parameter]
    public bool AllowNegativeNumbers { get; set; }

    private string autoComplete => AutoComplete ? "true" : "false";

    /// <summary>
    /// Gets or sets whether browser autocomplete is enabled for the number input.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether browser autocomplete is enabled. When true, the browser may offer saved values for the input.")]
    [Parameter]
    public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets whether the input is disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the input is disabled. When true, users cannot change its value.")]
    [Parameter]
    public bool Disabled { get; set; }

    [AddedVersion("1.0.0")]
    [CascadingParameter] private EditContext EditContext { get; set; } = default!;
    [DefaultValue(false)]

    /// <summary>
    /// Gets or sets whether input is restricted to the configured minimum and maximum values.
    /// 
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Description("Gets or sets whether input is restricted to the configured minimum and maximum values. When true, values outside the range are rejected.")]
    [Parameter]
    public bool EnableMinMax { get; set; }

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the culture name used to parse and format the number.
    /// </summary>
    /// <remarks>
    /// Default value is 'en-US'.
    /// </remarks>
    [Parameter]
    //[EditorRequired]
    [AddedVersion("1.0.0")]
    public string Locale { get; set; } = "en-US";

    /// <summary>
    /// Gets or sets the maximum permitted value.
    /// 
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets or sets the maximum permitted value. It is enforced only when EnableMinMax is true.")]
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the minimum permitted value.
    /// 
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets or sets the minimum permitted value. It is enforced only when EnableMinMax is true.")]
    [Parameter]
    public TValue Min { get; set; } = default!;

    /// <summary>
    /// Gets or sets placeholder text displayed when the input has no value.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets placeholder text displayed when the input has no value.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the increment used by the browser's number input controls.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the increment used by the browser's number input controls.")]
    [Parameter]
    public double? Step { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of the input value.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(Alignment.None)]
    [Description("Gets or sets the horizontal alignment of the input value.")]
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

    /// <summary>
    /// Gets or sets the current numeric value bound to the input.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets or sets the current numeric value bound to the input.")]
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Occurs whenever user input changes the number value, including changes produced by typing.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Occurs whenever user input changes the number value, including changes produced by typing.")]
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value for validation and EditContext notifications.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound value for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
