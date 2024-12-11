namespace BlazorBootstrap;

/// <summary>
/// Build vertically collapsing accordions in combination with our <see cref="Collapse"/>> component. <br/>
/// The accordion is based on the <see href="https://getbootstrap.com/docs/5.0/components/accordion/">Bootstrap Accordion</see> component. 
/// </summary>
public partial class Accordion : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private List<AccordionItem>? items = new();

    #endregion

    #region Methods
      
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

        foreach (var accordionItem in items!.ToList())
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
    /// Shows all <see cref="AccordionItem" /> instances if <see cref="AlwaysOpen"/> is <see langword="true" />.
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
        items ??= new List<AccordionItem>();
        items.Add(accordionItem);
    }
    
    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(AlwaysOpen): AlwaysOpen = (bool)parameter.Value; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Flush): Flush = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(OnHidden): OnHidden = (EventCallback<AccordionEventArgs>)parameter.Value; break;
                case nameof(OnHiding): OnHiding = (EventCallback<AccordionEventArgs>)parameter.Value; break;
                case nameof(OnShowing): OnShowing = (EventCallback<AccordionEventArgs>)parameter.Value; break;
                case nameof(OnShown): OnShown = (EventCallback<AccordionEventArgs>)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers 

    /// <summary>
    /// If <see langword="true" />, accordion items stay open when another item is opened.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool AlwaysOpen { get; set; }
    
    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// If <see langword="true" />, removes borders and rounded corners to render accordions edge-to-edge with their parent container.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Flush { get; set; }

    /// <summary>
    /// This event is fired when an accordion item has been hidden from the user (will wait for CSS transitions to complete).
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
    /// This event is fired when an accordion item has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter]
    public EventCallback<AccordionEventArgs> OnShown { get; set; }

    #endregion
}
