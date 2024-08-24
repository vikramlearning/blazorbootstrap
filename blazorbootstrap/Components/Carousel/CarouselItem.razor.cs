namespace BlazorBootstrap;

public partial class CarouselItem : BlazorBootstrapComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required
        Parent.AddItem(this);
        await base.OnInitializedAsync();
    }

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.CarouselItem, true),
            (BootstrapClass.Active, Active)
        );

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null"/>.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the active state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    [Parameter]
    public bool Active { get; set; }

    [CascadingParameter(Name= "Carousel")]
    public Carousel Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the aria-label.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// The amount of time to delay between automatically cycling an item.
    /// </summary>
    /// <remarks>
    /// Default value is 5000.
    /// </remarks>
    [Parameter]
    public int? Interval { get; set; } = 5000;

    #endregion
}
