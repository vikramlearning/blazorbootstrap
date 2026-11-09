namespace BlazorBootstrap;

/// <summary>
/// A content pane of a <see cref="ListGroup{TItem}" /> in tab mode. The group renders a tab for each pane, labeled with its <see cref="Title" />. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/list-group/#javascript-behavior" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class ListGroupTabPane : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
            Parent?.RemovePane(this);

        return base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Parent is null)
            throw new InvalidOperationException($"{nameof(ListGroupTabPane)} must be placed inside a ListGroup with TabMode set to true.");

        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException($"{nameof(ListGroupTabPane)} requires a {nameof(Name)}.");

        Parent.AddPane(this);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets whether this pane is shown when the group first renders. When no pane sets it, the first enabled pane is shown.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether this pane is shown when the group first renders. When no pane sets it, the first enabled pane is shown.")]
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the content of the pane.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the pane.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("tab-pane", true),
            ("fade", Parent!.Fade),
            (BootstrapClass.Show, Parent.Fade && Parent.IsActive(this)),
            (BootstrapClass.Active, Parent.IsActive(this)));

    /// <summary>
    /// Gets or sets whether the pane's tab is disabled.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the pane's tab is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the unique name of the pane. Tab events report it, and <see cref="ListGroup{TItem}.ShowTabAsync" /> takes it.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the unique name of the pane. Tab events report it, and <b>ShowTabAsync</b> takes it.")]
    [EditorRequired]
    [Parameter]
    public string Name { get; set; } = default!;

    [CascadingParameter] private IListGroupTabHost? Parent { get; set; }

    internal string TabId => $"{Id}-tab";

    /// <summary>
    /// Gets or sets the text of the pane's tab. <see cref="TitleTemplate" /> takes precedence.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text of the pane's tab. <b>TitleTemplate</b> takes precedence.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the content of the pane's tab, such as text with a badge.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the pane's tab, such as text with a badge.")]
    [Parameter]
    public RenderFragment? TitleTemplate { get; set; }

    #endregion
}
