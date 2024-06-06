namespace BlazorBootstrap;

public partial class PaginationLink : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(LinkAriaLabel))
            AdditionalAttributes?.Add("aria-label", LinkAriaLabel); // TODO: this is not working revisit again

        base.OnParametersSet();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.PaginationLink)
            .Build();

    /// <summary>
    /// Gets or sets the link aria-label attribute.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? LinkAriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the link icon.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName LinkIcon { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the link text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? LinkText { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
