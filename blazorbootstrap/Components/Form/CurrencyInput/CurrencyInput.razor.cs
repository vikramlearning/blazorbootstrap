namespace BlazorBootstrap;

public partial class CurrencyInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CultureInfo cultureInfo = default!;

    private FieldIdentifier fieldIdentifier;

    private string formattedValue = default!;

    private bool isFirstRenderComplete = false;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.FormControl);
        this.AddClass(BootstrapClassProvider.TextAlignment(TextAlignment), TextAlignment != Alignment.None);

        base.BuildClasses();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.currencyInput.initialize", ElementId, isFloatingNumber(), AllowNegativeNumbers, cultureInfo.NumberFormat.CurrencyDecimalSeparator);

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

            await SetFormattedValueAsync();

            await InvokeAsync(StateHasChanged);

            isFirstRenderComplete = true;
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

        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

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

    protected override async Task OnParametersSetAsync()
    {
        if (isFirstRenderComplete) await SetFormattedValueAsync();

        await base.OnParametersSetAsync();
    }

    /// <summary>
    /// Disables currency input.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables currency input.
    /// </summary>
    public void Enable() => Disabled = false;

    private string ExtractValue(object value, CultureInfo cultureInfo)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            return string.Empty;

        var validChars = "0123456789";

        if (isFloatingNumber())
            validChars = string.Concat(validChars, ".");

        if (AllowNegativeNumbers)
            validChars = string.Concat(validChars, "-");

        return string.Concat(value?.ToString()?.Replace(",", ".")?.Where(c => validChars.Contains(c))!);
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
        var newValue = ExtractValue(e.Value!, cultureInfo);

        if (newValue is null || !TryParseValue(newValue, out var value))
            Value = default!;
        else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, value)) // value < min
            Value = Min;
        else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(value, Max)) // value > max
            Value = Max;
        else
            Value = value;

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        await SetFormattedValueAsync();
    }

    private async Task SetFormattedValueAsync()
    {
        var options = new CurrencyFormatOptions();

        if (!HideCurrencySymbol)
        {
            options.Style = "currency";
            options.Currency = new RegionInfo(cultureInfo.Name).ISOCurrencySymbol;
        }
        else
        {
            options.Style = "decimal";
            options.Currency = null;
        }

        options.CurrencySign = CurrencySign == CurrencySign.Accounting ? "accounting" : "standard";

        options.MinimumIntegerDigits = MinimumIntegerDigits;

        if (isFloatingNumber())
        {
            if (MinimumFractionDigits.HasValue)
                options.MinimumFractionDigits = MinimumFractionDigits.Value;

            if (MaximumFractionDigits.HasValue)
                options.MaximumFractionDigits = MaximumFractionDigits.Value;
        }
        else
        {
            options.MinimumFractionDigits = 0;
            options.MaximumFractionDigits = 0;
        }

        formattedValue = await JS.InvokeAsync<string>("window.blazorBootstrap.currencyInput.getFormattedValue", Value is null ? "" : Value, Locale, options);
    }

    private bool TryParseValue(object value, out TValue newValue)
    {
        try
        {
            // sbyte? / sbyte
            if (typeof(TValue) == typeof(sbyte?) || typeof(TValue) == typeof(sbyte))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(sbyte), CultureInfo.InvariantCulture);

                return true;
            }
            // short? / short

            if (typeof(TValue) == typeof(short?) || typeof(TValue) == typeof(short))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(short), CultureInfo.InvariantCulture);

                return true;
            }
            // int? / int

            if (typeof(TValue) == typeof(int?) || typeof(TValue) == typeof(int))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(int), CultureInfo.InvariantCulture);

                return true;
            }
            // long? / long

            if (typeof(TValue) == typeof(long?) || typeof(TValue) == typeof(long))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(long), CultureInfo.InvariantCulture);

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

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Allows negative numbers. By default, negative numbers are not allowed.
    /// </summary>
    [Parameter]
    public bool AllowNegativeNumbers { get; set; }

    private string autoComplete => AutoComplete ? "true" : "false";

    /// <summary>
    /// Indicates whether the NumberInput can complete the values automatically by the browser.
    /// </summary>
    [Parameter]
    public bool AutoComplete { get; set; }

    [Parameter] public CurrencySign CurrencySign { get; set; }

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
    /// Determines whether to hide the currency symbol are not.
    /// </summary>
    [Parameter]
    public bool HideCurrencySymbol { get; set; }

    /// <summary>
    /// Gets or sets the locale. Default locale is 'en-US'.
    /// </summary>
    [Parameter]
    //[EditorRequired]
    public string Locale { get; set; } = "en-US";

    /// <summary>
    /// Gets or sets the max.
    /// Max ignored if EnableMinMax="false".
    /// </summary>
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// The maximum number of fraction digits to use.
    /// </summary>
    [Parameter]
    public byte? MaximumFractionDigits { get; set; }

    /// <summary>
    /// Gets or sets the min.
    /// Min ignored if EnableMinMax="false".
    /// </summary>
    [Parameter]
    public TValue Min { get; set; } = default!;

    /// <summary>
    /// The minimum number of fraction digits to use.
    /// </summary>
    [Parameter]
    public byte? MinimumFractionDigits { get; set; }

    /// <summary>
    /// The minimum number of integer digits to use. A value with a smaller number of integer digits than this number will be
    /// left-padded with zeros (to the specified length) when formatted.
    /// The default is 1.
    /// </summary>
    [Parameter]
    public byte MinimumIntegerDigits { get; set; } = 1;

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    [Parameter]
    public Alignment TextAlignment { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// This event fired on every user keystroke that changes the CurrencyInput value.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
