namespace BlazorBootstrap;

public partial class AccordionItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Collapse collapse = default!;

    private bool isCollapsed = true;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        Id = IdUtility.GetNextId(); // This is required
        Parent.Add(this);
    }

    protected override void OnParametersSet()
    {
        if (TitleTemplate is not null && !string.IsNullOrWhiteSpace(Title)) throw new InvalidOperationException($"{nameof(AccordionItem)} requires one of {nameof(TitleTemplate)} or {nameof(Title)}, but both were specified.");
    }

    internal async Task HideAsync() => await collapse.HideAsync();

    internal async Task ShowAsync() => await collapse.ShowAsync();

    private async Task OnCollapseHiddenAsync()
    {
        if (Parent is not null && Parent.OnHidden.HasDelegate)
            await Parent.OnHidden.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task OnCollapseHidingAsync()
    {
        isCollapsed = true;

        if (Parent is not null && Parent.OnHiding.HasDelegate)
            await Parent.OnHiding.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task OnCollapseShowingAsync()
    {
        isCollapsed = false;

        if (Parent is not null && Parent.OnShowing.HasDelegate)
            await Parent.OnShowing.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task OnCollapseShownAsync()
    {
        if (Parent is not null && Parent.OnShown.HasDelegate)
            await Parent.OnShown.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task ToggleAsync() => await collapse.ToggleAsync();

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames => 
        BuildClassNames(Class, (BootstrapClass.AccordionItem, true));

    /// <summary>
    /// Gets or sets the active state.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the active state.")]
    [Parameter]
    public bool Active { get; set; }

    private string buttonCollapsedStateCSSClass => isCollapsed ? "collapsed" : string.Empty;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the name.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter]
    internal Accordion Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the title.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the title template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title template.")]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? TitleTemplate { get; set; } = default!;

    #endregion
}
