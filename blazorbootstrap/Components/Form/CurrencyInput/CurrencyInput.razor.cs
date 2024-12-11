﻿namespace BlazorBootstrap;

/// <summary>
/// Input form for currency values, such as dollars, euros or pounds.
/// </summary>
/// <typeparam name="TValue">Number type to store the value in. (<see langword="float"/>, <see langword="int"/>, etc.)</typeparam>
public partial class CurrencyInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private CultureInfo cultureInfo = default!;

    private FieldIdentifier fieldIdentifier;

    private string formattedValue = default!;

    private bool isFirstRenderComplete = false;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.currencyInput.initialize", Id, IsFloatingNumber(), AllowNegativeNumbers, cultureInfo.NumberFormat.CurrencyDecimalSeparator);

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

    /// <inheritdoc />
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

    private string ExtractValue(object value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            return string.Empty;

        var validChars = "0123456789";

        if (IsFloatingNumber())
            validChars = string.Concat(validChars, ".");

        if (AllowNegativeNumbers)
            validChars = string.Concat(validChars, "-");

        return string.Concat(value?.ToString()?.Replace(",", ".")?.Where(c => validChars.Contains(c))!);
    }

    /// <summary>
    /// Returns <see langword="true" /> if <typeparamref name="TValue" /> is a type that represents a floating number.
    /// </summary>
    /// <returns><see langword="true" /> if <typeparamref name="TValue" />  is a type that represents a floating number.</returns>
    private static bool IsFloatingNumber() =>
        typeof(TValue) == typeof(float)
        || typeof(TValue) == typeof(float?)
        || typeof(TValue) == typeof(double)
        || typeof(TValue) == typeof(double?)
        || typeof(TValue) == typeof(decimal)
        || typeof(TValue) == typeof(decimal?);



    /// <summary>
    /// Determines whether the <paramref name="left"/>> value is greater than the <paramref name="right"/> value.
    /// </summary>
    /// <param name="left">Left-hand value</param>
    /// <param name="right">Right-hand value</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is greater than <paramref name="right"/></returns>
    private static bool IsLeftGreaterThanRight(TValue left, TValue right)
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

            return l > r;
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

            return l > r;
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

            return l > r;
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

            return l > r;
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

            return l > r;
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

            return l > r;
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

            return l > r;
        }

        return false;
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var newValue = ExtractValue(e.Value!);

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

        if (IsFloatingNumber())
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

        formattedValue = await JsRuntime.InvokeAsync<string>("window.blazorBootstrap.currencyInput.getFormattedValue", Value is null ? "" : Value, Locale, options);
    }

    private static bool TryParseValue(object value, out TValue newValue)
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
                case var _ when String.Equals(parameter.Name, nameof(AllowNegativeNumbers), StringComparison.OrdinalIgnoreCase): AllowNegativeNumbers = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(AutoComplete), StringComparison.OrdinalIgnoreCase): AutoComplete = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(CurrencySign), StringComparison.OrdinalIgnoreCase): CurrencySign = (CurrencySign)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Disabled), StringComparison.OrdinalIgnoreCase): Disabled = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(EditContext), StringComparison.OrdinalIgnoreCase): EditContext = (EditContext)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(EnableMinMax), StringComparison.OrdinalIgnoreCase): EnableMinMax = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HideCurrencySymbol), StringComparison.OrdinalIgnoreCase): HideCurrencySymbol = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Locale), StringComparison.OrdinalIgnoreCase): Locale = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Max), StringComparison.OrdinalIgnoreCase): Max = (TValue)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(MaximumFractionDigits), StringComparison.OrdinalIgnoreCase): MaximumFractionDigits = (byte?)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Min), StringComparison.OrdinalIgnoreCase): Min = (TValue)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(MinimumFractionDigits), StringComparison.OrdinalIgnoreCase): MinimumFractionDigits = (byte?)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(MinimumIntegerDigits), StringComparison.OrdinalIgnoreCase): MinimumIntegerDigits = (byte)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Placeholder), StringComparison.OrdinalIgnoreCase): Placeholder = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(TextAlignment), StringComparison.OrdinalIgnoreCase): TextAlignment = (Alignment)parameter.Value; break;
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
    /// If <see langword="true" />, allows negative numbers.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool AllowNegativeNumbers { get; set; }
 
    /// <summary>
    /// If <see langword="true" />, CurrencyInput can complete the values automatically by the browser.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets the currency sign.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CurrencySign.Standard" />.
    /// </remarks>
    [Parameter] public CurrencySign CurrencySign { get; set; } = CurrencySign.Standard;

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Determines whether to restrict the user input to Min and Max range.
    /// If <see langword="true" />, restricts the user input between the Min and Max range. Else accepts the user input.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool EnableMinMax { get; set; }

    private string FieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Determines whether to hide the currency symbol are not.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool HideCurrencySymbol { get; set; }

    /// <summary>
    /// Gets or sets the locale.
    /// </summary>
    /// <remarks>
    /// Default value is 'en-US'.
    /// </remarks>
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
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
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
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public byte? MinimumFractionDigits { get; set; }

    /// <summary>
    /// The minimum number of integer digits to use. A value with a smaller number of integer digits than this number will be
    /// left-padded with zeros (to the specified length) when formatted.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [Parameter]
    public byte MinimumIntegerDigits { get; set; } = 1;

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

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

    /// <summary>
    /// An expression that identifies the bound value.
    /// </summary>
    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
