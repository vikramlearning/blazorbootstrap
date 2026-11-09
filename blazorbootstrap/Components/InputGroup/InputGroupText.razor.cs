namespace BlazorBootstrap;

/// <summary>
/// Renders a text, icon, checkbox, or radio add-on inside an <see cref="InputGroup" />.
/// <para>
/// Reference its <see cref="BlazorBootstrapComponentBase.Id" /> from the input's <c>aria-describedby</c> attribute
/// so assistive technology reads the add-on with the input.
/// </para>
/// </summary>
[AddedVersion("4.1.0")]
public partial class InputGroupText : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content of the add-on, such as text, an icon, or a checkbox or radio input.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the add-on, such as text, an icon, or a checkbox or radio input.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.InputGroupText, true));

    /// <summary>
    /// Gets or sets the id of the input this add-on labels. When set, the add-on renders as a <c>label</c> element
    /// linked to that input; otherwise it renders as a <c>span</c>.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the id of the input this add-on labels. When set, the add-on renders as a label element linked to that input; otherwise it renders as a span.")]
    [Parameter]
    public string? For { get; set; }

    #endregion
}
