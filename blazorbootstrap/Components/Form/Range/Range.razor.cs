﻿namespace BlazorBootstrap;

public partial class Range<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;
    // default values
    private sbyte min = 0;
    private sbyte max = 100;
    private sbyte step = 1;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormRange());
        base.BuildClasses(builder);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var currentValue = Value; // object

            if (currentValue is null || !TryParseValue(currentValue, out var value))
                Value = default;
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

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Disables number input.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables number input.
    /// </summary>
    public void Enable() => Disabled = false;

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
            Value = default;
        else if (Min is not null && IsLeftGreaterThanRight(Min, value)) // value < min
            Value = Min;
        else if (Max is not null && IsLeftGreaterThanRight(value, Max)) // value > max
            Value = Max;
        else
            Value = value;

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
                newValue = (TValue)Convert.ChangeType(value, typeof(float));

                return true;
            }
            // double? / double

            if (typeof(TValue) == typeof(double?) || typeof(TValue) == typeof(double))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(double));

                return true;
            }
            // decimal? / decimal

            if (typeof(TValue) == typeof(decimal?) || typeof(TValue) == typeof(decimal))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(decimal));

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
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the max.
    /// </summary>
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the min.
    /// </summary>
    [Parameter]
    public TValue Min { get; set; } = default!;

    /// <summary>
    /// Gets or sets the step.
    /// </summary>
    [Parameter]
    public double Step { get; set; } = 1;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// This event fired on every user keystroke that changes the NumberInput value.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
