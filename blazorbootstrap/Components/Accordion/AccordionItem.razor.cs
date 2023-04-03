namespace BlazorBootstrap;

public partial class AccordionItem
{
    #region Events

    #endregion

    #region Members

    private Collapse collapseAccordionItem = default!;

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

    #endregion
}

