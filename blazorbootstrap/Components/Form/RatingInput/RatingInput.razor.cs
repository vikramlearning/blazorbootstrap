namespace BlazorBootstrap;

/// <summary>
/// Represents a Blazor component that lets users select or display a score using icons.
/// </summary>
/// <typeparam name="TValue">The type of the rating value.</typeparam>
/// <remarks>
/// Supported types for TValue: int, int?, decimal, decimal?
/// </remarks>
[AddedVersion("4.1.0")]
public partial class RatingInput<TValue> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private decimal? currentValue;

    private string descriptionId = default!;

    private decimal descriptionValue;

    private decimal displayValue;

    private FieldIdentifier fieldIdentifier;

    private decimal readOnlyCheckedValue;

    private decimal readOnlyTabStopValue;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (ValueExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(ValueExpression);
    }

    protected override void OnParametersSet()
    {
        if (!(typeof(TValue) == typeof(int)
              || typeof(TValue) == typeof(int?)
              || typeof(TValue) == typeof(decimal)
              || typeof(TValue) == typeof(decimal?)
             ))
            throw new InvalidOperationException($"{typeof(TValue)} is not supported.");

        if (AllowHalf && IsIntegerType())
            throw new InvalidOperationException($"The {nameof(AllowHalf)} parameter is supported only for decimal and decimal? values. {typeof(TValue)} cannot hold half values.");

        if (MaxValue < 1)
            throw new InvalidOperationException($"The {nameof(MaxValue)} parameter value must be greater than or equal to 1.");

        descriptionId = $"{Id}-description";
        currentValue = ToDecimal(Value);
        UpdateDisplayState();

        base.OnParametersSet();
    }

    #endregion

    #region Private Methods

    private TValue ConvertToValue(decimal? value)
    {
        if (typeof(TValue) == typeof(int))
            return (TValue)(object)(int)(value ?? 0);

        if (typeof(TValue) == typeof(int?))
            return value.HasValue ? (TValue)(object)(int)value.Value : default!;

        if (typeof(TValue) == typeof(decimal))
            return (TValue)(object)(value ?? 0m);

        return value.HasValue ? (TValue)(object)value.Value : default!;
    }

    private string GetColorClass() =>
        Color switch
        {
            RatingInputColor.Primary => TextColor.Primary.ToTextColorClass(),
            RatingInputColor.Secondary => TextColor.Secondary.ToTextColorClass(),
            RatingInputColor.Success => TextColor.Success.ToTextColorClass(),
            RatingInputColor.Danger => TextColor.Danger.ToTextColorClass(),
            RatingInputColor.Warning => TextColor.Warning.ToTextColorClass(),
            RatingInputColor.Info => TextColor.Info.ToTextColorClass(),
            RatingInputColor.Light => TextColor.Light.ToTextColorClass(),
            RatingInputColor.Dark => TextColor.Dark.ToTextColorClass(),
            RatingInputColor.Body => TextColor.Body.ToTextColorClass(),
            RatingInputColor.Muted => TextColor.Muted.ToTextColorClass(),
            RatingInputColor.White => TextColor.White.ToTextColorClass(),
            _ => ""
        };

    private string GetItemClassNames(decimal itemValue, bool isHalfItem, bool isStartHalf) =>
        BuildClassNames(
            ("bb-rating-input-item", true),
            ("bb-rating-input-item-start", isHalfItem && isStartHalf),
            ("bb-rating-input-item-end", isHalfItem && !isStartHalf),
            (GetItemStateClass(itemValue, isHalfItem), true));

    private string GetItemLabel(decimal value) =>
        string.Format(CultureInfo.CurrentCulture, ItemLabelFormat, NormalizeDecimal(value), MaxValue);

    private string GetItemStateClass(decimal itemValue, bool isHalfItem)
    {
        if (displayValue >= itemValue)
            return "bb-rating-input-item-filled";

        if (!isHalfItem && displayValue == itemValue - 0.5m)
            return "bb-rating-input-item-half";

        return "bb-rating-input-item-empty";
    }

    private IEnumerable<decimal> GetSelectableValues()
    {
        var step = isHalfStepEnabled ? 0.5m : 1m;

        for (var value = step; value <= MaxValue; value += step)
            yield return value;
    }

    private bool IsIntegerType() => typeof(TValue) == typeof(int) || typeof(TValue) == typeof(int?);

    /// <summary>
    /// Removes trailing zeros so labels read "3" and "3.5" instead of "3.0" and "3.50".
    /// </summary>
    private static decimal NormalizeDecimal(decimal value) => value / 1.0000000000000000000000000000m;

    private async Task OnItemChangedAsync(decimal itemValue)
    {
        if (ReadOnly || Disabled)
            return;

        await SetValueAsync(itemValue);
    }

    private async Task OnItemClickedAsync(decimal itemValue)
    {
        if (!AllowClear || ReadOnly || Disabled || itemValue != currentValue)
            return;

        await SetValueAsync(null);
    }

    private async Task SetValueAsync(decimal? newValue)
    {
        var value = ConvertToValue(newValue);

        if (EqualityComparer<TValue>.Default.Equals(value, Value))
            return;

        Value = value;
        currentValue = ToDecimal(Value);
        UpdateDisplayState();

        await ValueChanged.InvokeAsync(Value);

        if (fieldIdentifier.FieldName is not null)
            EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private static decimal? ToDecimal(TValue value) =>
        value switch
        {
            int intValue => intValue,
            decimal decimalValue => decimalValue,
            _ => null
        };

    private void UpdateDisplayState()
    {
        descriptionValue = Math.Clamp(currentValue ?? 0m, 0m, MaxValue);
        displayValue = Math.Round(descriptionValue * 2, MidpointRounding.AwayFromZero) / 2;
        readOnlyCheckedValue = Math.Round(descriptionValue, MidpointRounding.AwayFromZero);
        readOnlyTabStopValue = readOnlyCheckedValue > 0 ? readOnlyCheckedValue : 1;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets whether clicking the currently selected item clears the value.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether clicking the currently selected item clears the value to empty (null for nullable types, 0 otherwise).")]
    [Parameter]
    public bool AllowClear { get; set; }

    /// <summary>
    /// Gets or sets whether half values can be selected. Supported only for decimal and decimal? values.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether half values can be selected. Supported only for decimal and decimal? values.")]
    [Parameter]
    public bool AllowHalf { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-rating-input", true),
            ("bb-rating-input-interactive", !ReadOnly && !Disabled),
            ("bb-rating-input-readonly", ReadOnly),
            ("bb-rating-input-disabled", Disabled),
            ("bb-rating-input-sm", Size == RatingInputSize.Small),
            ("bb-rating-input-lg", Size == RatingInputSize.Large),
            (GetColorClass(), Color != RatingInputColor.None),
            (fieldCssClasses, true));

    /// <summary>
    /// Gets or sets the color of the rating icons.
    /// <para>
    /// Default value is <see cref="RatingInputColor.Warning" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(RatingInputColor.Warning)]
    [Description("Gets or sets the color of the rating icons.")]
    [Parameter]
    public RatingInputColor Color { get; set; } = RatingInputColor.Warning;

    /// <summary>
    /// Gets or sets the disabled state.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the rating input is disabled. When true, users cannot focus or change it.")]
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets the icon used for empty items.
    /// <para>
    /// Default value is <see cref="IconName.Star" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(IconName.Star)]
    [Description("Gets or sets the icon used for empty items.")]
    [Parameter]
    public IconName EmptyIconName { get; set; } = IconName.Star;

    private string fieldCssClasses =>
        EditContext is not null && fieldIdentifier.FieldName is not null
            ? EditContext.FieldCssClass(fieldIdentifier)
            : "";

    /// <summary>
    /// Gets or sets the icon used for filled items.
    /// <para>
    /// Default value is <see cref="IconName.StarFill" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(IconName.StarFill)]
    [Description("Gets or sets the icon used for filled items.")]
    [Parameter]
    public IconName FilledIconName { get; set; } = IconName.StarFill;

    /// <summary>
    /// Gets or sets the icon used for half-filled items when a fractional value is displayed.
    /// <para>
    /// Default value is <see cref="IconName.StarHalf" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(IconName.StarHalf)]
    [Description("Gets or sets the icon used for half-filled items when a fractional value is displayed.")]
    [Parameter]
    public IconName HalfIconName { get; set; } = IconName.StarHalf;

    private bool isHalfStepEnabled => AllowHalf && !IsIntegerType();

    /// <summary>
    /// Gets or sets the format of each item's accessible label and of the value description.
    /// Placeholder {0} is the item value and {1} is <see cref="MaxValue" />.
    /// <para>
    /// Default value is "{0} of {1}".
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("{0} of {1}")]
    [Description("Gets or sets the format of each item's accessible label and of the value description. Placeholder {0} is the item value and {1} is the maximum value.")]
    [Parameter]
    public string ItemLabelFormat { get; set; } = "{0} of {1}";

    /// <summary>
    /// Gets or sets the number of rating items.
    /// <para>
    /// Default value is 5.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(5)]
    [Description("Gets or sets the number of rating items. Must be greater than or equal to 1.")]
    [Parameter]
    public int MaxValue { get; set; } = 5;

    /// <summary>
    /// Gets or sets the read-only state.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the rating input is read-only. When true, the value is displayed, including half values, and cannot be changed.")]
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the size of the rating icons.
    /// <para>
    /// Default value is <see cref="RatingInputSize.Default" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(RatingInputSize.Default)]
    [Description("Gets or sets the size of the rating icons.")]
    [Parameter]
    public RatingInputSize Size { get; set; } = RatingInputSize.Default;

    /// <summary>
    /// Gets or sets the rating value.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the current rating value.")]
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// This event fires when the user changes the rating value.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Fires once whenever user interaction changes the rating value. It does not fire when the component is read-only or disabled.")]
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound value for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; } = default!;

    #endregion
}
