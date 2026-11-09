namespace BlazorBootstrap;

/// <summary>
/// A toggle button inside a <see cref="RadioButtonGroup{TValue}" /> or <see cref="CheckboxButtonGroup{TValue}" />.
/// It renders a Bootstrap <c>.btn-check</c> input with its button-styled label. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/button-group/#checkbox-and-radio-button-groups" />
/// </summary>
/// <typeparam name="TValue">The type of the item value.</typeparam>
/// <remarks>
/// <see cref="BlazorBootstrapComponentBase.Class" /> and <see cref="BlazorBootstrapComponentBase.Style" /> apply to the visible button (the label).
/// <see cref="BlazorBootstrapComponentBase.Id" /> and additional attributes, such as <c>aria-label</c>, apply to the input.
/// </remarks>
[AddedVersion("4.1.0")]
public partial class ButtonGroupItem<TValue> : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void OnParametersSet()
    {
        if (Host is null)
            throw new InvalidOperationException($"{nameof(ButtonGroupItem<TValue>)} must be placed inside a {nameof(RadioButtonGroup<TValue>)} or a {nameof(CheckboxButtonGroup<TValue>)}.");

        base.OnParametersSet();
    }

    private async Task OnChangeAsync(ChangeEventArgs e)
    {
        if (isDisabled)
            return;

        // A radio raises change only when it becomes checked; a checkbox reports its new state.
        var isCheckedNow = e.Value is bool checkedValue ? checkedValue : true;

        await Host!.OnItemCheckedChangedAsync(Value, isCheckedNow);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content of the button. Takes precedence over <see cref="Text" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the button. Takes precedence over <b>Text</b>.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Button, true),
            ("bb-button-group-item", true), // hook for the dark-mode outline colors in blazor.bootstrap.css
            (resolvedColor.ToButtonColorClass(), resolvedColor != ButtonColor.None && !resolvedOutline),
            (resolvedColor.ToButtonOutlineColorClass(), resolvedColor != ButtonColor.None && resolvedOutline));

    /// <summary>
    /// Gets or sets the color of this button. When <see langword="null" />, the group's color is used.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the color of this button. When <b>null</b>, the group's color is used.")]
    [Parameter]
    public ButtonColor? Color { get; set; }

    /// <summary>
    /// Gets or sets whether this button is disabled. The button is also disabled when its group is disabled.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether this button is disabled. The button is also disabled when its group is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private IButtonGroupItemHost<TValue>? Host { get; set; }

    private bool isChecked => Host!.IsChecked(Value);

    private bool isDisabled => Disabled || Host!.Disabled;

    /// <summary>
    /// Gets or sets whether this button uses the outline style. When <see langword="null" />, the group's setting is used.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets whether this button uses the outline style. When <b>null</b>, the group's setting is used.")]
    [Parameter]
    public bool? Outline { get; set; }

    private ButtonColor resolvedColor => Color ?? Host!.Color;

    private bool resolvedOutline => Outline ?? Host!.Outline;

    /// <summary>
    /// Gets or sets the text of the button. <see cref="ChildContent" /> takes precedence.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text of the button. <b>ChildContent</b> takes precedence.")]
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the value this button selects.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the value this button selects.")]
    [EditorRequired]
    [Parameter]
    public TValue Value { get; set; } = default!;

    #endregion
}
