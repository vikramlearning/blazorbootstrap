namespace BlazorBootstrap;

public partial class EnumInput<TEnum> : BlazorBootstrapComponentBase where TEnum : Enum
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier = default!;

    private List<EnumItem>? items;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        items = EnumUtility<TEnum>.GetEnumItems();

        AdditionalAttributes ??= new Dictionary<string, object>();

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (TextExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(TextExpression);

        if (ValueExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(ValueExpression!);
    }

    /// <summary>
    /// Disables the <see cref="EnumInput" />.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the <see cref="EnumInput" />.
    /// </summary>
    public void Enable() => Disabled = false;

    private void OnChange(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            Value = default!;
            return;
        }

        var newValue = int.TryParse(e.Value.ToString(), out int _value) ? _value : default!;

        // Value
        if (ValueChanged.HasDelegate)
            ValueChanged.InvokeAsync(newValue);
        else
            Value = newValue;

        // Text
        if (TextChanged.HasDelegate)
            TextChanged.InvokeAsync(items?.FirstOrDefault(i => i.Value == newValue)?.Text ?? string.Empty);
        else
            Text = items?.FirstOrDefault(i => i.Value == newValue)?.Text ?? string.Empty;
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.FormSelect, true),
            (Size.ToEnumIputSizeClass(), (Size != EnumInputSize.None && Size != EnumInputSize.Normal))
        );

    /// <summary>
    /// Gets or sets the disabled state.
    /// <para>
    /// Default value is false.
    /// </para>
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    [CascadingParameter]
    private EditContext EditContext { get; set; } = default!;

    private string FieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the size.
    /// <para>
    /// Default value is <see cref="EnumInputSize.None" />.
    /// </para>
    /// </summary>
    [Parameter]
    public EnumInputSize Size { get; set; } = EnumInputSize.None;

    /// <summary>
    /// Gets or sets the text.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public string Text { get; set; } = default!;

    /// <summary>
    /// This event fires when the <see cref="EnumInput" /> text changes.
    /// </summary>
    [Parameter]
    public EventCallback<string> TextChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression.
    /// </summary>
    [Parameter]
    public Expression<Func<string>> TextExpression { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value.
    /// <para>
    /// Default value is 0.
    /// </para>
    /// </summary>
    [Parameter]
    public int Value { get; set; }

    /// <summary>
    /// This event fires when the <see cref="EnumInput" /> value changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression.
    /// </summary>
    [Parameter]
    public Expression<Func<int>> ValueExpression { get; set; } = default!;

    #endregion
}
