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
    /// Hides the <see cref="AccordionItem" /> by index.
    /// </summary>
    /// <param name="index">The index of the AccordionItem to hide.</param>
    public async Task HideAccordionItemByIndexAsync(int index)
    {
        if (!items?.Any() ?? false) return;

        if (index < 0 || index >= items!.Count) throw new IndexOutOfRangeException();

        var accordionItem = items[index];

        if (accordionItem is not null)
            await accordionItem.HideAsync();
    }

    /// <summary>
    /// Hides the <see cref="AccordionItem" /> by name.
    /// </summary>
    /// <param name="accordionItemName">The name of the AccordionItem to hide.</param>
    public async Task HideAccordionItemByNameAsync(string accordionItemName)
    {
        if (!items?.Any() ?? false) return;

        var accordionItem = items!.FirstOrDefault(x => x.Name == accordionItemName);

        if (accordionItem is not null)
            await accordionItem.HideAsync();
    }

    /// <summary>
    /// Hides all <see cref="AccordionItem" /> instances.
    /// </summary>
    public async Task HideAllAccordionItemsAsync()
    {
        if (!items?.Any() ?? false) return;

        foreach (var accordionItem in items!)
            if (accordionItem is not null)
                await accordionItem.HideAsync();
    }

    /// <summary>
    /// Hides the first <see cref="AccordionItem" />.
    /// </summary>
    public async Task HideFirstAccordionItemAsync()
    {
        if (!items?.Any() ?? false) return;

        var accordionItem = items!.FirstOrDefault();

        if (accordionItem is not null)
            await accordionItem.HideAsync();
    }

    /// <summary>
    /// Hides the last <see cref="AccordionItem" />.
    /// </summary>
    public async Task HideLastAccordionItemAsync()
    {
        if (!items?.Any() ?? false) return;

        var accordionItem = items!.LastOrDefault();

        if (accordionItem is not null)
            await accordionItem.HideAsync();
    }

    /// <summary>
    /// Shows the <see cref="AccordionItem" /> by index.
    /// </summary>
    /// <param name="index">The index of the AccordionItem to show.</param>
    public async Task ShowAccordionItemByIndexAsync(int index)
    {
        if (!items?.Any() ?? false) return;

        if (index < 0 || index >= items!.Count) throw new IndexOutOfRangeException();

        var accordionItem = items[index];

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Shows the <see cref="AccordionItem" /> by name.
    /// </summary>
    /// <param name="accordionItemName">The name of the AccordionItem to show.</param>
    public async Task ShowAccordionItemByNameAsync(string accordionItemName)
    {
        if (!items?.Any() ?? false) return;

        var accordionItem = items!.FirstOrDefault(x => x.Name == accordionItemName);

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Shows all <see cref="AccordionItem" /> instances if AlwaysOpen is true.
    /// </summary>
    public async Task ShowAllAccordionItemsAsync()
    {
        if (!items?.Any() ?? false) return;

        if (AlwaysOpen)
            foreach (var accordionItem in items!)
                if (accordionItem is not null)
                    await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Shows the first <see cref="AccordionItem" />.
    /// </summary>
    public async Task ShowFirstAccordionItemAsync()
    {
        if (!items?.Any() ?? false) return;

        var accordionItem = items!.FirstOrDefault();

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Shows the last <see cref="AccordionItem" />.
    /// </summary>
    public async Task ShowLastAccordionItemAsync()
    {
        if (!items?.Any() ?? false) return;

        var accordionItem = items!.LastOrDefault();

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Adds an <see cref="AccordionItem" /> to the collection.
    /// </summary>
    /// <param name="accordionItem">The AccordionItem to add.</param>
    internal void Add(AccordionItem accordionItem)
    {
        if (items is null) 
            items = new List<AccordionItem>();

        if (accordionItem is not null)
            items.Add(accordionItem);
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
