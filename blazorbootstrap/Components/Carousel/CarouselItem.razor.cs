namespace BlazorBootstrap;

public partial class CarouselItem : BlazorBootstrapComponentBase
{
    #region Methods

    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required
        Parent.AddItem(this);
        await base.OnInitializedAsync();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.CarouselItem, true),
            (BootstrapClass.Active, Active)
        );

    /// <summary>
    /// Gets or sets the active state.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the active state.")]
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The amount of time to delay between automatically cycling an item.
    /// <para>
    /// Default value is 5000 milliseconds.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(5000)]
    [Description("The amount of time to delay between automatically cycling an item.")]
    [Parameter]
    public int Interval { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the aria-label.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the aria-label.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? Label { get; set; }

    [CascadingParameter(Name = "Carousel")] 
    public Carousel Parent { get; set; } = default!;

    #endregion
}
