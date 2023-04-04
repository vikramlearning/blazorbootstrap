namespace BlazorBootstrap;

public partial class AccordionItem
{
    #region Events

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when a accordion item has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter] public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event is fired when a accordion item has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnHidden { get; set; }

    #endregion

    #region Members

    private Collapse collapseAccordionItem = default!;

    private bool isCollapsed = true;

    private string buttonCollapsedStateCSSClass => isCollapsed ? "collapsed" : string.Empty;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.AccordionItem());

        base.BuildClasses(builder);
    }

    protected override void OnParametersSet()
    {
        if (Header is not null && !string.IsNullOrWhiteSpace(HeaderText))
        {
            throw new InvalidOperationException($"{nameof(AccordionItem)} requires one of {nameof(Header)} or {nameof(HeaderText)}, but both were specified.");
        }
    }

    private async Task toggleAccordionItemAsync() => await collapseAccordionItem.ToggleAsync();

    private async Task OnCollapseShowingAsync()
    {
        isCollapsed = false;

        if (OnShowing.HasDelegate)
            await OnShowing.InvokeAsync();
    }

    private async Task OnCollapseShownAsync()
    {
        if (OnShown.HasDelegate)
            await OnShown.InvokeAsync();
    }

    private async Task OnCollapseHidingAsync()
    {
        isCollapsed = true;

        if (OnHiding.HasDelegate)
            await OnHiding.InvokeAsync();
    }

    private async Task OnCollapseHiddenAsync()
    {
        if (OnHidden.HasDelegate)
            await OnHidden.InvokeAsync();
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the header content to be rendered inside the <see cref="Header"/>.
    /// </summary>
    [Parameter]
    public RenderFragment Header { get; set; } = default!;

    /// <summary>
    /// Gets or sets the header text.
    /// </summary>
    [Parameter]
    public string HeaderText { get; set; } = default!;

    /// <summary>
    /// Specifies the body content to be rendered inside the <see cref="Body"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public RenderFragment Body { get; set; } = default!;

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter] internal Accordion Parent { get; set; } = default!;

    #endregion
}

