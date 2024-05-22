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
        Id = IdGenerator.GetNextId(); // This is required
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
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.AccordionItem)
            .Build();

    /// <summary>
    /// Gets or sets the active state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Active { get; set; }

    private string buttonCollapsedStateCSSClass => isCollapsed ? "collapsed" : string.Empty;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public RenderFragment Content { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter]
    internal Accordion Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the title template.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment TitleTemplate { get; set; } = default!;

    #endregion
}
