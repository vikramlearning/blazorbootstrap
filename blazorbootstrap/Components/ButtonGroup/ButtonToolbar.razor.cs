namespace BlazorBootstrap;

/// <summary>
/// Combines several button groups, and optionally input groups, into a toolbar. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/button-group/#button-toolbar" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class ButtonToolbar : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible name of the toolbar, read by screen readers.
    /// To name the toolbar by visible text instead, set <c>aria-labelledby</c>; it takes precedence.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of the toolbar, read by screen readers. To name the toolbar by visible text instead, set <b>aria-labelledby</b>; it takes precedence.")]
    [EditorRequired]
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the content of the toolbar: button groups and input groups.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the toolbar: button groups and input groups.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ButtonToolbar, true));

    #endregion
}
