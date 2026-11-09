namespace BlazorBootstrap;

/// <summary>
/// A group of toggle buttons from which the user selects one value. Each choice is a <see cref="ButtonGroupItem{TValue}" />. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/button-group/#checkbox-and-radio-button-groups" />
/// </summary>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
[AddedVersion("4.1.0")]
public partial class RadioButtonGroup<TValue> : BlazorBootstrapComponentBase, IButtonGroupItemHost<TValue>
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (ValueExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(ValueExpression);
    }

    bool IButtonGroupItemHost<TValue>.IsChecked(TValue value) => EqualityComparer<TValue>.Default.Equals(value, Value);

    async Task IButtonGroupItemHost<TValue>.OnItemCheckedChangedAsync(TValue value, bool isChecked)
    {
        if (Disabled || !isChecked || EqualityComparer<TValue>.Default.Equals(value, Value))
            return;

        Value = value;

        await ValueChanged.InvokeAsync(Value);

        if (fieldIdentifier.FieldName is not null)
            EditContext?.NotifyFieldChanged(fieldIdentifier);

        StateHasChanged();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible name of the group, read by screen readers.
    /// To name the group by visible text instead, set <c>aria-labelledby</c>; it takes precedence.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of the group, read by screen readers. To name the group by visible text instead, set <b>aria-labelledby</b>; it takes precedence.")]
    [EditorRequired]
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the choices of the group: <see cref="ButtonGroupItem{TValue}" /> components.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the choices of the group: <b>ButtonGroupItem</b> components.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ButtonGroup, !Vertical),
            (BootstrapClass.ButtonGroupVertical, Vertical),
            (Size.ToButtonGroupSizeClass(), Size != ButtonGroupSize.None),
            (fieldCssClasses, true));

    /// <summary>
    /// Gets or sets the default color of the buttons. An item's <c>Color</c> overrides it.
    /// <para>
    /// Default value is <see cref="ButtonColor.Primary" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(ButtonColor.Primary)]
    [Description("Gets or sets the default color of the buttons. An item's <b>Color</b> overrides it.")]
    [Parameter]
    public ButtonColor Color { get; set; } = ButtonColor.Primary;

    /// <summary>
    /// Gets or sets whether every button in the group is disabled.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether every button in the group is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses =>
        EditContext is not null && fieldIdentifier.FieldName is not null
            ? EditContext.FieldCssClass(fieldIdentifier)
            : "";

    string IButtonGroupItemHost<TValue>.InputType => "radio";

    bool IButtonGroupItemHost<TValue>.IsInvalid =>
        EditContext is not null
        && fieldIdentifier.FieldName is not null
        && EditContext.GetValidationMessages(fieldIdentifier).Any();

    string? IButtonGroupItemHost<TValue>.Name => Id;

    /// <summary>
    /// Gets or sets whether the buttons use the outline style by default. An item's <c>Outline</c> overrides it.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the buttons use the outline style by default. An item's <b>Outline</b> overrides it.")]
    [Parameter]
    public bool Outline { get; set; } = true;

    /// <summary>
    /// Gets or sets the size of every button in the group.
    /// <para>
    /// Default value is <see cref="ButtonGroupSize.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(ButtonGroupSize.None)]
    [Description("Gets or sets the size of every button in the group.")]
    [Parameter]
    public ButtonGroupSize Size { get; set; } = ButtonGroupSize.None;

    /// <summary>
    /// Gets or sets the selected value.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the selected value.")]
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// This event fires when the user selects a different value.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user selects a different value.")]
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

    /// <summary>
    /// Gets or sets whether the buttons are stacked vertically instead of horizontally.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the buttons are stacked vertically instead of horizontally.")]
    [Parameter]
    public bool Vertical { get; set; }

    #endregion
}
