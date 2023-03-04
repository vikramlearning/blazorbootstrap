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

    private string defaultFormat = "yyyy-MM-dd";
    private string defaultLocale = "en-US";

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private string autoComplete => this.AutoComplete ? "true" : "false";

    private bool disabled;

    private string formattedMax;

    private string formattedMin;

    private string formattedValue;

    private bool isFirstRender = true;

    private CultureInfo cultureInfo;

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

        try
        {
            this.cultureInfo = new CultureInfo(Locale); // TODO: check locale is required or not?
        }
        catch (CultureNotFoundException)
        {
            this.cultureInfo = new CultureInfo("en-US");
        }

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var currentValue = Value;
            Console.WriteLine($"OnAfterRenderAsync 1: currentValue: {currentValue}, Min: {Min}, Max: {Max}");

            if (currentValue is null || !TryParseValue(currentValue, out TValue value))
            {
                if (EnableMinMax
                    && Min is not null
                    && (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime)))
                {
                    Value = Min;
                }
                else // DateOnly? / DateTime?
                    Value = default!;

                Console.WriteLine($"OnAfterRenderAsync 2: currentValue: {currentValue}, Value: {Value}");
            }
            else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, Value)) //  value < min
            {
                Value = EnableMinMax && Min is not null ? Min : default!;
                Console.WriteLine($"OnAfterRenderAsync 3: currentValue: {currentValue}, Value: {Value}");
            }
            else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(Value, Max)) // value > max
            {
                Value = Max;
                Console.WriteLine($"OnAfterRenderAsync 4: currentValue: {currentValue}, Value: {Value}");
            }
            else
            {
                Value = value;
                Console.WriteLine($"OnAfterRenderAsync 5: currentValue: {currentValue}, Value: {Value}");
            }

            this.formattedMax = EnableMinMax ? GetFormattedValue(Max) : string.Empty;
            this.formattedMin = EnableMinMax ? GetFormattedValue(Min) : string.Empty;
            this.formattedValue = GetFormattedValue(Value);

            Console.WriteLine($"OnAfterRenderAsync 6: formattedMax: {formattedMax}, formattedMin: {formattedMin}, formattedValue: {formattedValue}");

            await ValueChanged.InvokeAsync(Value);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        Console.WriteLine($"OnChange called...");

        var oldValue = Value;
        var newValue = e.Value; // object

        Console.WriteLine($"OnChange 1: oldValue: {oldValue}, newValue: {newValue}");

        if (newValue is null || !TryParseValue(newValue, out TValue value))
        {
            if (EnableMinMax
                && Min is not null
                && (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime)))
            {
                Value = Min;
                Console.WriteLine($"OnChange 2: {Value}");
            }
            else // DateOnly? / DateTime?
            {
                Value = default!;
                Console.WriteLine($"OnChange 3: {Value}");
            }

            Console.WriteLine($"OnChange 4: {Value}");
        }
        else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, value)) //  value < min
        {
            Value = Min;
            Console.WriteLine($"OnChange 5: {Value}, oldValue: {oldValue}");
        }
        else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(value, Max)) // value > max
        {
            Value = Max;
            Console.WriteLine($"OnChange 6: {Value}, oldValue: {oldValue}");
        }
        else
        {
            Value = value;
            Console.WriteLine($"OnChange 7: {Value}");
        }

        this.formattedMax = EnableMinMax ? GetFormattedValue(Max) : string.Empty;
        this.formattedMin = EnableMinMax ? GetFormattedValue(Min) : string.Empty;
        this.formattedValue = GetFormattedValue(Value);

        if (oldValue.Equals(Value))
            await JS.InvokeVoidAsync("window.blazorBootstrap.dateInput.setValue", ElementId, this.formattedValue);

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        Console.WriteLine($"OnChange: formattedValue: {this.formattedValue}");
    }

    private bool TryParseValue(object value, out TValue newValue)
    {
        try
        {
            Console.WriteLine($"TryParseValue 1: {value}");

            // DateOnly / DateOnly?
            if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateOnly?))
            {
                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    Console.WriteLine($"TryParseValue 2: {dt}");
                    newValue = (TValue)(object)DateOnly.FromDateTime(dt);
                    Console.WriteLine($"TryParseValue 3: {newValue}");
                    return true;
                }

                newValue = default!;
                Console.WriteLine($"TryParseValue 4: {newValue}");
                return false;
            }
            // DateTime / DateTime?
            else if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
            {
                newValue = (TValue)Convert.ChangeType(value, typeof(DateTime), CultureInfo.InvariantCulture);
                Console.WriteLine($"TryParseValue 5: {newValue}");
                return true;
            }

            newValue = default!;
            Console.WriteLine($"TryParseValue 6: {newValue}");
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
    /// Determines where the left input is greater than the right input.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>bool</returns>
    private bool IsLeftGreaterThanRight(object left, object right)
    {
        Console.WriteLine($"IsLeftGreaterThanRight called...");
        if (left is null || right is null)
            return false;

        // DateOnly / DateOnly?
        if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateOnly?))
        {
            if (DateTime.TryParse(left.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ldt)
                && DateTime.TryParse(right.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime rdt))
            {
                DateOnly l = DateOnly.FromDateTime(ldt);
                DateOnly r = DateOnly.FromDateTime(rdt);

                Console.WriteLine($"IsLeftGreaterThanRight 1: left: {l}, right: {r}, result: {l > r}");
                return l > r;
            }
        }
        // DateTime / DateTime?
        else if (typeof(TValue) == typeof(DateTime) || typeof(TValue) == typeof(DateTime?))
        {
            DateTime l = Convert.ToDateTime(left, CultureInfo.InvariantCulture);
            DateTime r = Convert.ToDateTime(right, CultureInfo.InvariantCulture);
            Console.WriteLine($"IsLeftGreaterThanRight 2: left: {l.ToString("dd-MM-yyyy")}, right: {r.ToString("dd-MM-yyyy")}, result: {l > r}");
            return l > r;
        }

        return false;
    }

    private string GetFormattedValue(TValue value)
    {
        Console.WriteLine($"GetFormattedValue 1: value: {value}");

        string date = "";

        try
        {
            if (value is null)
                return date;

            // DateOnly / DateOnly?
            if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateOnly?))
            {
                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    date = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    Console.WriteLine($"GetFormattedValue 3: value: {date}");
                }
            }
            //// DateOnly?
            //else if (typeof(TValue) == typeof(DateOnly?))
            //{
            //    if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            //    {
            //        date = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            //        Console.WriteLine($"GetFormattedValue 3: value: {date}");
            //    }
            //}
            // DateTime
            else if (typeof(TValue) == typeof(DateTime))
            {
                var d = Convert.ToDateTime(value.ToString()); // TODO: update this with .NET 8 upgrade
                date = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                Console.WriteLine($"GetFormattedValue 4: value: {date}");
            }
            // DateTime?
            else if (typeof(TValue) == typeof(DateTime?))
            {
                var d = value as DateTime?;
                if (d is not null && d.HasValue)
                {
                    date = d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    Console.WriteLine($"GetFormattedValue 5: value: {date}");
                }
            }
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"GetFormattedValue 6: FormatException: {ex.Message}");
            return date;
        }

        Console.WriteLine($"GetFormattedValue 7: value: {date}");
        return date;
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
    /// Gets or sets the locale. Default locale is 'en-US'.
    /// </summary>
    [Parameter, EditorRequired] public string Locale { get; set; } = "en-US";

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
    /// Gets or sets the step.
    /// The default value of step is 1, indicating 1 day.
    /// </summary>
    [Parameter] public int Step { get; set; } = 1;

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
