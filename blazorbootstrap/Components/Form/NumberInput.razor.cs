using Microsoft.AspNetCore.Components.Forms;
using System.Runtime.Serialization;

namespace BlazorBootstrap;

public partial class NumberInput<TValue> : BaseComponent
{
    #region Events

    /// <summary>
    /// This is event fires on every user keystroke that changes the textbox value.
    /// </summary>
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    #endregion

    #region Members

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private string autoComplete => this.AutoComplete ? "true" : "false";

    private bool disabled;

    private string step;

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());

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
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender || Value is null || !(Min is not null && Max is not null))
            return;

        var currentValue = Value; // object

        if (currentValue is null || !TryParseValue(currentValue, out TValue value))
            Value = default;
        else if (IsLeftGreaterThanRight(Min, Value)) // value < min
            Value = Min;
        else if (IsLeftGreaterThanRight(Value, Max)) // value > max
            Value = Max;
        else
            Value = value;

        await ValueChanged.InvokeAsync(Value);

        Console.WriteLine($"OnAfterRenderAsync - Value: {Value}"); // TODO: remove this console log
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

    private async Task OnInput(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        if (newValue is null || !TryParseValue(newValue, out TValue value))
            Value = default;
        else
            Value = value;

        if (oldValue.Equals(Value))
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.numberInput.setValue", ElementId, Value);
        }

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        Console.WriteLine($"OnInput - Input: {e.Value?.ToString()}, Value: {Value}"); // TODO: remove this console log
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        if (newValue is null || !TryParseValue(newValue, out TValue value))
            Value = default;
        else if (IsLeftGreaterThanRight(Min, Value)) // value < min
            Value = Min;
        else if (IsLeftGreaterThanRight(Value, Max)) // value > max
            Value = Max;
        else
            Value = value;

        if (oldValue.Equals(Value))
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.numberInput.setValue", ElementId, Value);
        }

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        Console.WriteLine($"OnChange - Input: {e.Value?.ToString()}, Value: {Value}"); // TODO: remove this console log
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

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public bool AutoComplete { get; set; } = false;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; }

    [Parameter] public TValue Max { get; set; }

    [Parameter] public TValue Min { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the step. Default value is 1.
    /// </summary>
    [Parameter] public double? Step { get; set; } = 1;

    [Parameter] public TValue Value { get; set; }

    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; }

    #endregion
}
