namespace BlazorBootstrap;

public partial class Accordion : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private List<AccordionItem>? items = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing) items = null;

        await base.DisposeAsyncCore(disposing);
    }

    /// <summary>
    /// Hides the <see cref="AccordionItem" /> by index.
    /// </summary>
    /// <param name="index">The index of the AccordionItem to hide.</param>
    [AddedVersion("1.10.5")]
    [Description("Hides the <b>AccordionItem</b> by index.")]
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
    [AddedVersion("1.10.5")]
    [Description("Hides the <b>AccordionItem</b> by name.")]
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
    [AddedVersion("1.10.5")]
    [Description("Hides all <b>AccordionItem</b> instances.")]
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
    [AddedVersion("1.10.5")]
    [Description("Hides the first <b>AccordionItem</b>.")]
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
    [AddedVersion("1.10.5")]
    [Description("Hides the last <b>AccordionItem</b>.")]
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
    [AddedVersion("1.7.0")]
    [Description("Shows the <b>AccordionItem</b> by index.")]
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
    [AddedVersion("1.7.0")]
    [Description("Shows the <b>AccordionItem</b> by name.")]
    public async Task ShowAccordionItemByNameAsync(string accordionItemName)
    {
        if (!items?.Any() ?? false) return;

        var accordionItem = items!.FirstOrDefault(x => x.Name == accordionItemName);

        if (accordionItem is not null)
            await accordionItem.ShowAsync();
    }

    /// <summary>
    /// Shows all <see cref="AccordionItem" /> instances if <see cref="AlwaysOpen"/> is <see langword="true"/>.
    /// </summary>
    [AddedVersion("1.10.5")]
    [Description("Shows all <b>AccordionItem</b> instances if <b>AlwaysOpen</b> is <b>true</b>.")]
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
    [AddedVersion("1.7.0")]
    [Description("Shows the first <b>AccordionItem</b>.")]
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
    [AddedVersion("1.7.0")]
    [Description("Shows the last <b>AccordionItem</b>.")]
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

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Accordion, true),
            (BootstrapClass.AccordionFlush, Flush));

    /// <summary>
    /// If <see langword="true" />, accordion items stay open when another item is opened.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, accordion items stay open when another item is opened.")]
    [Parameter]
    public bool AlwaysOpen { get; set; }

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
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// If <see langword="true" />, removes borders and rounded corners to render accordions edge-to-edge with their parent container.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, removes borders and rounded corners to render accordions edge-to-edge with their parent container.")]
    [Parameter]
    public bool Flush { get; set; }

    /// <summary>
    /// This event is fired when a accordion item has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("This event is fired when a accordion item has been hidden from the user (will wait for CSS transitions to complete).")]
    [Parameter]
    public EventCallback<AccordionEventArgs> OnHidden { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("This event is fired immediately when the hide method has been called.")]
    [Parameter]
    public EventCallback<AccordionEventArgs> OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show method is called.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("This event fires immediately when the show method is called.")]
    [Parameter]
    public EventCallback<AccordionEventArgs> OnShowing { get; set; }

    /// <summary>
    /// This event is fired when a accordion item has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("This event is fired when a accordion item has been made visible to the user (will wait for CSS transitions to complete).")]
    [Parameter]
    public EventCallback<AccordionEventArgs> OnShown { get; set; }

    #endregion
}
