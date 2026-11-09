namespace BlazorBootstrap;

/// <summary>
/// Groups a series of buttons together on a single line or stacked vertically. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/button-group/" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class ButtonGroup : BlazorBootstrapComponentBase
{
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
    /// Gets or sets the content of the group: buttons, links, nested groups, or dropdowns.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the group: buttons, links, nested groups, or dropdowns.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ButtonGroup, !Vertical),
            (BootstrapClass.ButtonGroupVertical, Vertical),
            (Size.ToButtonGroupSizeClass(), Size != ButtonGroupSize.None));

    /// <summary>
    /// Gets or sets the size of every button in the group, nested groups included.
    /// <para>
    /// Default value is <see cref="ButtonGroupSize.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(ButtonGroupSize.None)]
    [Description("Gets or sets the size of every button in the group, nested groups included.")]
    [Parameter]
    public ButtonGroupSize Size { get; set; } = ButtonGroupSize.None;

    /// <summary>
    /// Gets or sets whether the buttons are stacked vertically instead of horizontally.
    /// Split button dropdowns are not supported in a vertical group.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the buttons are stacked vertically instead of horizontally. Split button dropdowns are not supported in a vertical group.")]
    [Parameter]
    public bool Vertical { get; set; }

    #endregion
}
