namespace BlazorBootstrap;

/// <summary>
/// Blazor Bootstrap <see cref="DateInput{T}"/> component is constructed using an HTML input of type="date" which limits user input based on pre-defined parameters.
/// This component enables users to input a date using a text box with validation or a special date picker interface.
/// </summary>
/// <typeparam name="TValue">Date type to insert: <see cref="DateTime"/>, <see cref="DateOnly"/> and their <see cref="Nullable{T}"/>> variants are supported.</typeparam>
/// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TValue"/> isn't of type <see cref="DateTime"/>. <see cref="DateOnly"/> or their <see cref="Nullable{T}"/>> alternatives.</exception>
/// <exception cref="InvalidOperationException">Thrown if <see cref="Min"/> is larger than <see cref="Max"/> and <see cref="EnableMinMax"/> is <see langword="true" /></exception>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
        
        fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        
        await base.OnInitializedAsync();
    }

    /// <inheritdoc />
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
    /// Disables currency input.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables currency input.
    /// </summary>
    public void Enable() => Disabled = false;

    private string GetFormattedValue(object? value)
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
    /// Determines where the <paramref name="left"/> input is greater than the <paramref name="right"/> input.
    /// </summary>
    /// <param name="left">Left hand object</param>
    /// <param name="right">Right hand object</param>
    /// <returns><see langword="true" /> if <paramref name="left"/> is greater than <paramref name="right"/>.</returns>
    private static bool IsLeftGreaterThanRight(object? left, object? right)
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
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.dateInput.setValue", Id, formattedValue);

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private static bool TryParseValue(object value, out TValue newValue)
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
                 case nameof(AutoComplete): AutoComplete = (bool)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value; break;
                case nameof(EditContext): EditContext = (EditContext)parameter.Value!; break;
                case nameof(EnableMinMax): EnableMinMax = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Max): Max = (TValue)parameter.Value; break;
                 case nameof(Min): Min = (TValue)parameter.Value; break; 
                case nameof(Placeholder): Placeholder = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value!; break;
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
        BuildClassNames(Class, (BootstrapClass.FormControl, true));

    /// <summary>
    /// If <see langword="true" />, DateInput can complete the values automatically by the browser.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Determines whether to restrict the user input to Min and Max range.
    /// If <see langword="true" />, restricts the user input between the Min and Max range. Else accepts the user input.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool EnableMinMax { get; set; }

    private string FieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the max.
    /// Allowed format is yyyy-mm-dd.
    /// </summary>
    [Parameter]
    public TValue Max { get; set; } = default!;

    /// <summary>
    /// Gets or sets the min.
    /// Allowed format is yyyy-mm-dd.
    /// </summary>
    [Parameter]
    public TValue Min { get; set; } = default!;

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// This event fired on every user keystroke that changes the DateInput value.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// An expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
