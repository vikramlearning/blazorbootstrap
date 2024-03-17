namespace BlazorBootstrap;

/// <summary>
/// Represents a Blazor component that provides a range input for numeric values.
/// </summary>
/// <typeparam name="TValue">The type of the numeric value.</typeparam>
/// <remarks>
/// Supported types for TValue: sbyte, sbyte?, short, short?, int, int?, long, long?, float, float?, double, double?,
/// decimal, decimal?
/// </remarks>
public partial class RangeInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private sbyte max = 100;

    private sbyte min = 0;

    private DotNetObjectReference<RangeInput<TValue>> objRef = default!;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.FormRange);

        base.BuildClasses();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.rangeInput.initialize", ElementId, objRef);

            var currentValue = Value; // object

            if (currentValue is null || !TryParseValue(currentValue, out var value))
                Value = default!;
            else if (Min is not null && IsLeftGreaterThanRight(Min, Value)) // value < min
                Value = Min;
            else if (Max is not null && IsLeftGreaterThanRight(Value, Max)) // value > max
                Value = Max;
            else
                Value = value;

            await ValueChanged.InvokeAsync(Value);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

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

        if (IsLeftGreaterThanRight(Min, Max))
            throw new InvalidOperationException("The Min parameter value is greater than the Max parameter value.");

        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        SetDefaultValues();

        await base.OnInitializedAsync();
    }

    [JSInvokable]
    public async Task bsOnInput(object? newValue)
    {
        SetValue(newValue?.ToString());
        await HandleChangeAsync();
    }

    /// <summary>
    /// Disables the range input.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the range input.
    /// </summary>
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

    private async Task HandleChangeAsync()
    {
        await ValueChanged.InvokeAsync(Value);
        EditContext?.NotifyFieldChanged(fieldIdentifier);
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
        SetValue(e.Value);
        await HandleChangeAsync();
    }

    private void SetDefaultValues()
    {
        // sbyte? / sbyte
        if (typeof(TValue) == typeof(sbyte?)
            || typeof(TValue) == typeof(short?)
            || typeof(TValue) == typeof(int?)
            || typeof(TValue) == typeof(long?)
            || typeof(TValue) == typeof(float?)
            || typeof(TValue) == typeof(double?)
            || typeof(TValue) == typeof(decimal?))
        {
            if (Min is null)
                Min = TryParseValue(min, out var _min) ? _min : _min;

            if (Max is null)
                Max = TryParseValue(max, out var _max) ? _max : _max;
        }
    }

    private void SetValue(object? newValue)
    {
        if (newValue is null || !TryParseValue(newValue, out var value))
            Value = default!;
        else if (Min is not null && IsLeftGreaterThanRight(Min, value)) // value < min
            Value = Min;
        else if (Max is not null && IsLeftGreaterThanRight(value, Max)) // value > max
            Value = Max;
        else
            Value = value;
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

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Disables or enables the range input.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the maximum value of the range input.
    /// </summary>
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the minimum value of the range input.
    /// </summary>
    [Parameter]
    public TValue Min { get; set; } = default!;

    private bool showTickMarks => TickMarks?.Any() ?? false;

    /// <summary>
    /// Gets or sets the step value of the range input.
    /// </summary>
    [Parameter]
    public double Step { get; set; } = 1;

    /// <summary>
    /// Gets or sets the tick marks.
    /// </summary>
    [Parameter]
    public IEnumerable<TickMark> TickMarks { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the range input.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// This event fires when the user specifies a numeric value.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
