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

            if (currentValue is null || !TryParseValue(currentValue, out TValue value))
            {
                if (EnableMinMax
                    && Min is not null
                    && (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime)))
                {
                    Value = Min;
                }
                else
                    Value = default!;
            }
            else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, Value)) //  value < min
                Value = EnableMinMax && Min is not null ? Min : default!;
            else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(Value, Max)) // value > max
                Value = Max;
            else
                Value = value;

            this.formattedMax = GetFormattedValue(Max);
            this.formattedMin = GetFormattedValue(Min);
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
            else
                Value = default!;

            Console.WriteLine($"OnChange 1: {Value}");
        }
        else if (EnableMinMax && Min is not null && IsLeftGreaterThanRight(Min, value)) //  value < min
        {
            Value = Min;
            Console.WriteLine($"OnChange 2: {Value}, oldValue: {oldValue}");
        }
        else if (EnableMinMax && Max is not null && IsLeftGreaterThanRight(value, Max)) // value > max
        {
            Value = Max;
            Console.WriteLine($"OnChange 3: {Value}, oldValue: {oldValue}");
        }
        else
        {
            Value = value;
            Console.WriteLine($"OnChange 4: {Value}");
        }

        this.formattedMax = GetFormattedValue(Max);
        this.formattedMin = GetFormattedValue(Min);
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
                if (DateTime.TryParse(value.ToString(), CultureInfo.GetCultureInfo(defaultLocale), DateTimeStyles.None, out DateTime dt))
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

        // DateOnly? / DateOnly
        if (typeof(TValue) == typeof(DateOnly?) || typeof(TValue) == typeof(DateOnly))
        {
            Console.WriteLine($"Before: left: {left}, right: {right}");
            if (DateTime.TryParse(left.ToString(), CultureInfo.GetCultureInfo(defaultLocale), DateTimeStyles.None, out DateTime ldt)
                && DateTime.TryParse(right.ToString(), CultureInfo.GetCultureInfo(defaultLocale), DateTimeStyles.None, out DateTime rdt))
            {
                DateOnly l = DateOnly.FromDateTime(ldt);
                DateOnly r = DateOnly.FromDateTime(rdt);

                Console.WriteLine($"After: left: {l}, right: {r}");
                return l > r;
            }
        }

        //// DateOnly
        //if (typeof(TValue) == typeof(DateOnly))
        //{
        //    DateOnly l = DateOnly.FromDateTime(Convert.ToDateTime(left.ToString())); // TODO: update this with .NET 8 upgrade
        //    DateOnly r = DateOnly.FromDateTime(Convert.ToDateTime(right.ToString())); // TODO: update this with .NET 8 upgrade

        //    Console.WriteLine($"left: {l}, right: {r}");

        //    return l > r;
        //}
        //// DateOnly?
        //else if (typeof(TValue) == typeof(DateOnly?))
        //{
        //    DateOnly? l = left as DateOnly?;
        //    DateOnly? r = right as DateOnly?;
        //    return l.HasValue && r.HasValue && l > r;
        //}
        //// DateTime
        //else if (typeof(TValue) == typeof(DateTime))
        //{
        //    DateTime l = Convert.ToDateTime(left);
        //    DateTime r = Convert.ToDateTime(right);
        //    return l > r;
        //}
        //// DateTime?
        //else if (typeof(TValue) == typeof(DateTime?))
        //{
        //    DateTime? l = left as DateTime?;
        //    DateTime? r = right as DateTime?;
        //    return l.HasValue && r.HasValue && l > r;
        //}

        return false;
    }

    private string GetFormattedValue(TValue value)
    {
        string date = "";

        try
        {
            if (value is null)
                return date;

            // DateOnly
            if (typeof(TValue) == typeof(DateOnly) || typeof(TValue) == typeof(DateTime))
            {
                if (value is not null)
                {
                    var d = Convert.ToDateTime(value.ToString()); // TODO: update this with .NET 8 upgrade
                    date = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }
            // DateOnly?
            else if (typeof(TValue) == typeof(DateOnly?) || typeof(TValue) == typeof(DateTime?))
            {
                var d = value as DateTime?;
                if (d is not null && d.HasValue)
                    date = d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            // DateTime
            else if (typeof(TValue) == typeof(DateOnly?) || typeof(TValue) == typeof(DateTime?))
            {
                var d = Convert.ToDateTime(value.ToString()); // TODO: update this with .NET 8 upgrade
                date = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            // DateTime?
            else if (typeof(TValue) == typeof(DateOnly?) || typeof(TValue) == typeof(DateTime?))
            {
                var d = value as DateTime?;
                if (d is not null && d.HasValue)
                    date = d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }
        catch (FormatException)
        {
            return date;
        }

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
