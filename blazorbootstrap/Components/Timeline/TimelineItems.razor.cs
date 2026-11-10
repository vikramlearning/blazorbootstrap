namespace BlazorBootstrap;

/// <summary>
/// Data-bound items of a <see cref="Timeline" />: renders one <see cref="TimelineItem" /> for each element of <see cref="Data" />.
/// Place it inside a <see cref="Timeline" />, alone or next to <see cref="TimelineItem" /> components.
/// </summary>
/// <typeparam name="TItem">The type of the data elements; inferred from <see cref="Data" />.</typeparam>
[AddedVersion("4.1.0")]
public partial class TimelineItems<TItem> : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets a function that returns the marker color of an element.
    /// <para>
    /// Default value is <see langword="null" /> (the neutral marker).
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the marker color of an element.")]
    [Parameter]
    public Func<TItem, TimelineColor>? ColorField { get; set; }

    /// <summary>
    /// Gets or sets a function that returns whether an element is the current item.
    /// <para>
    /// Default value is <see langword="null" /> (no current item).
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns whether an element is the current item.")]
    [Parameter]
    public Func<TItem, bool>? CurrentField { get; set; }

    /// <summary>
    /// Gets or sets the elements to show, in order. A null or empty collection renders no items.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the elements to show, in order. A null or empty collection renders no items.")]
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the marker icon of an element.
    /// <para>
    /// Default value is <see langword="null" /> (a dot).
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the marker icon of an element.")]
    [Parameter]
    public Func<TItem, IconName>? IconNameField { get; set; }

    /// <summary>
    /// Gets or sets the template of an element's content, shown under its title and subtitle.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the template of an element's content, shown under its title and subtitle.")]
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template of an element's marker, such as an <see cref="Avatar" />. Takes precedence over <see cref="IconNameField" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the template of an element's marker, such as an <b>Avatar</b>. Takes precedence over <b>IconNameField</b>.")]
    [Parameter]
    public RenderFragment<TItem>? MarkerTemplate { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the subtitle of an element, such as a timestamp.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the subtitle of an element, such as a timestamp.")]
    [Parameter]
    public Func<TItem, string?>? SubtitleField { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the title of an element.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the title of an element.")]
    [Parameter]
    public Func<TItem, string?>? TitleField { get; set; }

    #endregion
}
