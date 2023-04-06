namespace BlazorBootstrap;

public partial class AccordionItem
{
    #region Events

    #endregion

    #region Members

    private Collapse collapse = default!;

    private bool isCollapsed = true;

    private string buttonCollapsedStateCSSClass => isCollapsed ? "collapsed" : string.Empty;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        ElementId = IdGenerator.Generate; // This is required
        Parent.Add(this);
    }

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.AccordionItem());

        base.BuildClasses(builder);
    }

    protected override void OnParametersSet()
    {
        if (TitleTemplate is not null && !string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException($"{nameof(AccordionItem)} requires one of {nameof(TitleTemplate)} or {nameof(Title)}, but both were specified.");
        }
    }

    internal async Task ShowAsync() => await collapse.ShowAsync();

    internal async Task HideAsync() => await collapse.HideAsync();

    private async Task ToggleAsync() => await collapse.ToggleAsync();

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

    private async Task OnCollapseHidingAsync()
    {
        isCollapsed = true;

        if (Parent is not null && Parent.OnHiding.HasDelegate)
            await Parent.OnHiding.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task OnCollapseHiddenAsync()
    {
        if (Parent is not null && Parent.OnHidden.HasDelegate)
            await Parent.OnHidden.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside the <see cref="AccordionItem"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public RenderFragment Content { get; set; } = default!;

    /// <summary>
    /// Gets or sets the active <see cref="AccordionItem"/>.
    /// </summary>
    [Parameter] public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [Parameter] public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter] internal Accordion Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the <see cref="AccordionItem"/> title.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the <see cref="AccordionItem"/> title template.
    /// </summary>
    [Parameter]
    public RenderFragment TitleTemplate { get; set; } = default!;

    #endregion
}

