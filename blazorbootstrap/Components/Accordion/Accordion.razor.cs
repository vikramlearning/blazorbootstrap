namespace BlazorBootstrap;

public partial class Accordion
{
    #region Fields and Constants

    private List<AccordionItem>? items = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Accordion());
        builder.Append(BootstrapClassProvider.AccordionFlush(), Flush);
        base.BuildClasses(builder);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing) items = null;

        await base.DisposeAsync(disposing);
    }

    /// <summary>
    /// Show the <see cref="AccordionItem" /> by index.
    /// </summary>
    /// <param name="index"></param>
    public async Task ShowAccordionItemByIndexAsync(int index)
    {
        if (index < 0 || index >= items.Count) throw new IndexOutOfRangeException();

        var accordionItem = items[index];

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Show the <see cref="AccordionItem" /> by name.
    /// </summary>
    /// <param name="accordionItemName">AccordionItem Name</param>
    public async Task ShowAccordionItemByNameAsync(string accordionItemName)
    {
        var accordionItem = items.FirstOrDefault(x => x.Name == accordionItemName);

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Show the first <see cref="AccordionItem" />.
    /// </summary>
    public async Task ShowFirstAccordionItemAsync()
    {
        var accordionItem = items.FirstOrDefault();

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Show the last <see cref="AccordionItem" />.
    /// </summary>
    public async Task ShowLastAccordionItemAsync()
    {
        var accordionItem = items.LastOrDefault();

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    internal void Add(AccordionItem accordionItem)
    {
        if (accordionItem != null) items?.Add(accordionItem);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the AlwaysOpen.
    /// It makes accordion items stay open when another item is opened.
    /// </summary>
    [Parameter]
    public bool AlwaysOpen { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Collapse" />.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the flush.
    /// Removes borders and rounded corners to render accordions edge-to-edge with their parent container.
    /// </summary>
    [Parameter]
    public bool Flush { get; set; }

    /// <summary>
    /// This event is fired when a accordion item has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter]
    public EventCallback<AccordionEventArgs> OnHidden { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter]
    public EventCallback<AccordionEventArgs> OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show method is called.
    /// </summary>
    [Parameter]
    public EventCallback<AccordionEventArgs> OnShowing { get; set; }

    /// <summary>
    /// This event is fired when a accordion item has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter]
    public EventCallback<AccordionEventArgs> OnShown { get; set; }

    #endregion
}
