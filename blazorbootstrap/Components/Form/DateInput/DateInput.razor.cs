namespace BlazorBootstrap;

public partial class DateInput<TValue> : BaseComponent
{
    #region Events

    /// <summary>
    /// This event fired on every user keystroke that changes the CurrencyInput value.
    /// </summary>
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    #endregion

    #region Members

    /// <summary>
    /// Date format: yyyy-MM-dd.
    /// </summary>
    private string defaultFormat = "yyyy-MM-dd";

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private string autoComplete => this.AutoComplete ? "true" : "false";

    private bool disabled;

    private string formattedMax;

    private string formattedMin;

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
        if (EnableMinMax
            && Min is not null
            && Max is not null
            && IsLeftGreaterThanRight(Min, Max))
            throw new InvalidOperationException("The Min parameter value is greater than the Max parameter value.");

        if (!(typeof(TValue) == typeof(DateOnly)
            || typeof(TValue) == typeof(DateOnly?)
            || typeof(TValue) == typeof(DateTime)
            || typeof(TValue) == typeof(DateTime?)
            ))
            throw new InvalidOperationException($"{typeof(TValue)} is not supported.");

        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        this.disabled = this.Disabled;

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var currentValue = Value;

            if (currentValue is null || !TryParseValue(currentValue, out TValue value))
            {
                if (EnableMinMax
                    && Min is not null
                    && (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime)))
                {
                    Value = Min;
                }
                else // DateOnly? / DateTime?
                {
                    Value = default!;
                }
            }
            else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, Value)) //  value < min
            {
                Value = EnableMinMax && Min is not null ? Min : default!;
            }
            else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(Value, Max)) // value > max
            {
                Value = Max;
            }
            else
            {
                Value = value;
            }

            this.formattedMax = EnableMinMax && Max is not null ? GetFormattedValue(Max) : string.Empty;
            this.formattedMin = EnableMinMax && Min is not null ? GetFormattedValue(Min) : string.Empty;
            this.formattedValue = GetFormattedValue(Value);

            await ValueChanged.InvokeAsync(Value);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        if (newValue is null || !TryParseValue(newValue, out TValue value))
        {
            if (EnableMinMax
                && Min is not null
                && (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime)))
            {
                Value = Min;
            }
            else // DateOnly? / DateTime?
            {
                Value = default!;
            }
        }
        else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, value)) //  value < min
        {
            Value = Min;
        }
        else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(value, Max)) // value > max
        {
            Value = Max;
        }
        else
        {
            Value = value;
        }

        this.formattedMax = EnableMinMax && Max is not null ? GetFormattedValue(Max) : string.Empty;
        this.formattedMin = EnableMinMax && Min is not null ? GetFormattedValue(Min) : string.Empty;
        this.formattedValue = GetFormattedValue(Value);

        if (oldValue.Equals(Value))
            await JS.InvokeVoidAsync("window.blazorBootstrap.dateInput.setValue", ElementId, this.formattedValue);

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
                if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                {
                    newValue = (TValue)(object)DateOnly.FromDateTime(dt);
                    return true;
                }

                newValue = default!;
                return false;
            }
            // DateTime / DateTime?
            else if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(DateTime), CultureInfo.CurrentCulture);
                return true;
            }

            newValue = default!;
            return false;
        }
        catch (Exception ex)
        {
            newValue = default!;
            return false;
        }
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
            if (DateTime.TryParse(left.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime ldt)
                && DateTime.TryParse(right.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime rdt))
            {
                DateOnly l = DateOnly.FromDateTime(ldt);
                DateOnly r = DateOnly.FromDateTime(rdt);

                return l > r;
            }
        }
        // DateTime / DateTime?
        else if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
        {
            DateTime l = Convert.ToDateTime(left, CultureInfo.CurrentCulture);
            DateTime r = Convert.ToDateTime(right, CultureInfo.CurrentCulture);
            return l > r;
        }

        return false;
    }

    private string GetFormattedValue(object value)
    {
        string formattedDate = "";

        try
        {
            if (value is null)
                return formattedDate;

            // DateOnly / DateOnly?
            if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateOnly?))
            {
                if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                {
                    formattedDate = dt.ToString(defaultFormat);
                }
            }
            // DateTime / DateTime?
            else if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
            {
                var d = Convert.ToDateTime(value, CultureInfo.CurrentCulture); // TODO: update this with .NET 8 upgrade
                formattedDate = d.ToString(defaultFormat);
            }
        }
        catch (FormatException ex)
        {
            return formattedDate;
        }

        return formattedDate;
    }

    /// <summary>
    /// Disables currency input.
    /// </summary>
    public void Disable()
    {
        this.disabled = true;
    }

    /// <summary>
    /// Enables currency input.
    /// </summary>
    public void Enable()
    {
        this.disabled = false;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

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
    /// Allowed format is yyyy-mm-dd.
    /// </summary>
    [Parameter] public TValue Max { get; set; }

    /// <summary>
    /// Gets or sets the min.
    /// Allowed format is yyyy-mm-dd.
    /// </summary>
    [Parameter] public TValue Min { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    [Parameter] public Alignment TextAlignment { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter] public TValue Value { get; set; } = default!;

    [Parameter] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
