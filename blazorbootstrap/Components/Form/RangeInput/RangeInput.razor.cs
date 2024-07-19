namespace BlazorBootstrap;

/// <summary>
/// Represents a Blazor component that provides a range input for numeric values. <br/>
/// See <see href="https://getbootstrap.com/docs/5.0/forms/range/">Bootstrap Range Input</see> for more information.
/// </summary>
/// <typeparam name="TValue">The type of the numeric value.</typeparam>
/// <remarks>
/// Supported types for <typeparamref name="TValue"/> are: <see langword="sbyte"/>, <see langword="sbyte?"/>, <see langword="short"/>, <see langword="short?"/>,
/// <see langword="int"/>, <see langword="int?"/>, <see langword="long"/>, <see langword="long?"/>, <see langword="float"/>, <see langword="float?"/>,
/// <see langword="double"/>, <see langword="double?"/>, <see langword="decimal"/> or <see langword="decimal?"/>
/// </remarks>
/// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TValue"/> isn't a number or is an unsigned number, which is illegal.</exception>
/// <exception cref="InvalidOperationException">Thrown if <see cref="Min"/> is larger than <see cref="Max"/></exception>
public partial class RangeInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private const sbyte DefaultMax = 100;

    private const sbyte DefaultMin = 0;

    private DotNetObjectReference<RangeInput<TValue>> objRef = default!;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.rangeInput.initialize", Id, objRef);

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

    /// <inheritdoc />
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
        
        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        SetDefaultValues();

        await base.OnInitializedAsync();
    }

    [JSInvokable("bsOnInput")]
    public async Task BsOnInput(object? newValue)
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

    private static string GetInvariantNumber(TValue? value)
    {
        return value switch
        {
            null => string.Empty,
            float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
            double doubleValue => doubleValue.ToString(CultureInfo.InvariantCulture),
            decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
            _ => value?.ToString() ?? string.Empty
        };
    }

    private async Task HandleChangeAsync()
    {
        await ValueChanged.InvokeAsync(Value);
        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    /// <summary>
    /// Determines where the <paramref name="left"/>> input is greater than the <paramref name="right"/>> input.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is larger than <paramref name="right"/></returns>
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
        
        throw new InvalidOperationException($"{typeof(TValue)} is not supported.");
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
            Min ??= ParseValue(DefaultMin);
            Max ??= ParseValue(DefaultMax);
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

    private static TValue ParseValue(object? value)
    {
               if (value is null || !TryParseValue(value, out var newValue))
                              return default!;
               
               return newValue;
                         }

    private static bool TryParseValue(object value, out TValue newValue)
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
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value; break;
                case nameof(EditContext): EditContext = (EditContext)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Max): Max = (TValue)parameter.Value; break;
                case nameof(Min): Min = (TValue)parameter.Value; break;
                case nameof(Step): Step = (double)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value!; break;
                case nameof(TickMarks): TickMarks = (IReadOnlyCollection<TickMark>)parameter.Value; break;
                case nameof(Value): Value = (TValue)parameter.Value; break;
                case nameof(ValueChanged): ValueChanged = (EventCallback<TValue>)parameter.Value; break;
                case nameof(ValueExpression): ValueExpression = (Expression<Func<TValue>>)parameter.Value; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.FormRange, true));

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string FieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

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

    private bool ShowTickMarks => TickMarks?.Any() ?? false;

    /// <summary>
    /// Gets or sets the step value of the range input.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [Parameter]
    public double Step { get; set; } = 1;

    /// <summary>
    /// Gets or sets the tick marks.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public IReadOnlyCollection<TickMark> TickMarks { get; set; } = default!;

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

    /// <summary>
    /// An expression that identifies the bound value.
    /// </summary>
    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
