using System.Globalization;

namespace BlazorBootstrap;

public partial class CurrencyInput<TValue> : BaseComponent
{
    #region Events

    /// <summary>
    /// This is event fires on every user keystroke that changes the NumberInput value.
    /// </summary>
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    #endregion

    #region Members

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private string autoComplete => this.AutoComplete ? "true" : "false";

    private bool disabled;

    private string step;

    private string formattedValue;

    private bool isFirstRender = true;

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());
        builder.Append(BootstrapClassProvider.TextAlignment(this.TextAlignment), this.TextAlignment != Alignment.None);

        base.BuildClasses(builder);
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

        this.disabled = this.Disabled;

        this.step = Step.HasValue ? $"{Step.Value}" : "any";

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.currencyInput.initialize", ElementId, isFloatingNumber(), AllowNegativeNumbers);

            var currentValue = Value; // object

            if (currentValue is null || !TryParseValue(currentValue, out TValue value))
                Value = default;
            else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, Value)) // value < min
                Value = Min;
            else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(Value, Max)) // value > max
                Value = Max;
            else
                Value = value;

            await ValueChanged.InvokeAsync(Value);

            await SetFormattedValueAsync();

            await InvokeAsync(StateHasChanged);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnParametersSetAsync()
    {
        await SetFormattedValueAsync();

        await base.OnParametersSetAsync();
    }

    /// <summary>
    /// Disables number input.
    /// </summary>
    public void Disable()
    {
        this.disabled = true;
    }

    /// <summary>
    /// Enables number input.
    /// </summary>
    public void Enable()
    {
        this.disabled = false;
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        // TODO: cleanup cultureInfo related duplicate code
        var cultureName = "fr-FR"; // "fr-FR"; // "en-IN";
        var cultureInfo = new CultureInfo(cultureName);

        var oldValue = Value;
        var newValue = ExtractValue(e.Value, cultureInfo);

        if (newValue is null || !TryParseValue(newValue, out TValue value))
            Value = default;
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
            sbyte l = Convert.ToSByte(left);
            sbyte r = Convert.ToSByte(right);
            return l > r;
        }
        // sbyte?
        else if (typeof(TValue) == typeof(sbyte?))
        {
            sbyte? l = left as sbyte?;
            sbyte? r = right as sbyte?;
            return l.HasValue && r.HasValue && l > r;
        }
        // short / int16
        else if (typeof(TValue) == typeof(short))
        {
            short l = Convert.ToInt16(left);
            short r = Convert.ToInt16(right);
            return l > r;
        }
        // short? / int16?
        else if (typeof(TValue) == typeof(short?))
        {
            short? l = left as short?;
            short? r = right as short?;
            return l.HasValue && r.HasValue && l > r;
        }
        // int
        else if (typeof(TValue) == typeof(int))
        {
            int l = Convert.ToInt32(left);
            int r = Convert.ToInt32(right);
            return l > r;
        }
        // int?
        else if (typeof(TValue) == typeof(int?))
        {
            int? l = left as int?;
            int? r = right as int?;
            return l.HasValue && r.HasValue && l > r;
        }
        // long
        else if (typeof(TValue) == typeof(long))
        {
            long l = Convert.ToInt64(left);
            long r = Convert.ToInt64(right);
            return l > r;
        }
        // long?
        else if (typeof(TValue) == typeof(long?))
        {
            long? l = left as long?;
            long? r = right as long?;
            return l.HasValue && r.HasValue && l > r;
        }
        // float / single
        else if (typeof(TValue) == typeof(float))
        {
            float l = Convert.ToSingle(left);
            float r = Convert.ToSingle(right);
            return l > r;
        }
        // float? / single?
        else if (typeof(TValue) == typeof(float?))
        {
            float? l = left as float?;
            float? r = right as float?;
            return l.HasValue && r.HasValue && l > r;
        }
        // double
        else if (typeof(TValue) == typeof(double))
        {
            double l = Convert.ToDouble(left);
            double r = Convert.ToDouble(right);
            return l > r;
        }
        // double?
        else if (typeof(TValue) == typeof(double?))
        {
            double? l = left as double?;
            double? r = right as double?;
            return l.HasValue && r.HasValue && l > r;
        }
        // decimal
        else if (typeof(TValue) == typeof(decimal))
        {
            decimal l = Convert.ToDecimal(left);
            decimal r = Convert.ToDecimal(right);
            return l > r;
        }
        // decimal?
        else if (typeof(TValue) == typeof(decimal?))
        {
            decimal? l = left as decimal?;
            decimal? r = right as decimal?;
            return l.HasValue && r.HasValue && l > r;
        }

        return false;
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
            else if (typeof(TValue) == typeof(short?) || typeof(TValue) == typeof(short))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(short));
                return true;
            }
            // int? / int
            else if (typeof(TValue) == typeof(int?) || typeof(TValue) == typeof(int))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(int));
                return true;
            }
            // long? / long
            else if (typeof(TValue) == typeof(long?) || typeof(TValue) == typeof(long))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(long));
                return true;
            }
            // float? / float
            else if (typeof(TValue) == typeof(float?) || typeof(TValue) == typeof(float))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(float));
                return true;
            }
            // double? / double
            else if (typeof(TValue) == typeof(double?) || typeof(TValue) == typeof(double))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(double));
                return true;
            }
            // decimal? / decimal
            else if (typeof(TValue) == typeof(decimal?) || typeof(TValue) == typeof(decimal))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(decimal));
                return true;
            }

            newValue = default;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"exception: {ex.Message}");
            newValue = default;
            return false;
        }
    }

    private bool isFloatingNumber()
    {
        return typeof(TValue) == typeof(float)
            || typeof(TValue) == typeof(float?)
            || typeof(TValue) == typeof(double)
            || typeof(TValue) == typeof(double?)
            || typeof(TValue) == typeof(decimal)
            || typeof(TValue) == typeof(decimal?);
    }

    private string ExtractValue(object value, CultureInfo cultureInfo)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            return string.Empty;

        var validChars = "0123456789";
        if (isFloatingNumber())
            validChars = string.Concat(validChars, ".");

        if (AllowNegativeNumbers)
            validChars = string.Concat(validChars, "-");

        return string.Concat(value?.ToString()?.Replace(",", ".")?.Where(c => validChars.Contains(c)));
    }

    private async Task SetFormattedValueAsync()
    {
        var cultureName = "fr-FR"; // "fr-FR"; // "en-IN";
        var cultureInfo = new CultureInfo(cultureName);
        this.formattedValue = await JS.InvokeAsync<string>("window.blazorBootstrap.currencyInput.getFormattedValue", (Value is null ? "" : Value), cultureName, (new RegionInfo(cultureInfo.Name)).ISOCurrencySymbol);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Allows negative numbers. By default, negative numbers are not allowed.
    /// </summary>
    [Parameter] public bool AllowNegativeNumbers { get; set; }

    /// <summary>
    /// Indicates whether the NumberInput can complete the values automatically by the browser.
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
    /// Max ignored if EnableMinMax="false".
    /// </summary>
    [Parameter] public TValue Max { get; set; }

    /// <summary>
    /// Gets or sets the min.
    /// Min ignored if EnableMinMax="false".
    /// </summary>
    [Parameter] public TValue Min { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the step.
    /// </summary>
    [Parameter] public double? Step { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    [Parameter] public Alignment TextAlignment { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter] public TValue Value { get; set; }

    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; }

    #endregion
}
