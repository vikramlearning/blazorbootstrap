namespace BlazorBootstrap;

/// <summary>
/// Groups an input with text, icon, checkbox, radio, button, or dropdown add-ons placed before, after, or on both sides of it.
/// <para>
/// Follows the Bootstrap 5.3 input group: <see href="https://getbootstrap.com/docs/5.3/forms/input-group/" />.
/// </para>
/// </summary>
[AddedVersion("4.1.0")]
public partial class InputGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content of the input group: inputs and add-ons in display order.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the input group: inputs and add-ons in display order.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.InputGroup, true),
            (Size.ToInputGroupSizeClass(), Size != InputGroupSize.Default),
            (BootstrapClass.FlexNoWrap, NoWrap));

    /// <summary>
    /// Gets or sets whether the input group stays on one line instead of wrapping its children.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the input group stays on one line instead of wrapping its children.")]
    [Parameter]
    public bool NoWrap { get; set; }

    /// <summary>
    /// Gets or sets the size of the input group. The size applies to every input and add-on in the group.
    /// <para>
    /// Default value is <see cref="InputGroupSize.Default" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(InputGroupSize.Default)]
    [Description("Gets or sets the size of the input group. The size applies to every input and add-on in the group.")]
    [Parameter]
    public InputGroupSize Size { get; set; } = InputGroupSize.Default;

    #endregion
}
