namespace BlazorBootstrap;

/// <summary>
/// One event of a <see cref="Timeline" />: a marker (a dot, an icon, or custom content) with a title, a subtitle, and content.
/// </summary>
[AddedVersion("4.1.0")]
public partial class TimelineItem : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content of the item, shown under its title and subtitle.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the item, shown under its title and subtitle.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-timeline-item", true),
            ("bb-timeline-item-current", Current));

    /// <summary>
    /// Gets or sets the color of the dot or icon marker. A <see cref="MarkerTemplate" /> brings its own look.
    /// <para>
    /// Default value is <see cref="TimelineColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TimelineColor.None)]
    [Description("Gets or sets the color of the dot or icon marker. A <b>MarkerTemplate</b> brings its own look.")]
    [Parameter]
    public TimelineColor Color { get; set; } = TimelineColor.None;

    /// <summary>
    /// Gets or sets whether this is the current item. It is marked <c>aria-current="step"</c> and shown with a ring around its marker.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether this is the current item. It is marked <b>aria-current=\"step\"</b> and shown with a ring around its marker.")]
    [Parameter]
    public bool Current { get; set; }

    /// <summary>
    /// Gets or sets the icon shown in the marker. When <see cref="IconName.None" />, the marker is a dot.
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the icon shown in the marker. When <b>None</b>, the marker is a dot.")]
    [Parameter]
    public IconName IconName { get; set; } = IconName.None;

    private string? markerClassNames =>
        BuildClassNames(null,
            ("bb-timeline-dot rounded-circle d-inline-flex align-items-center justify-content-center", true),
            ("bb-timeline-dot-icon", IconName != IconName.None),
            (Color.ToTimelineMarkerColorClass(), true));

    /// <summary>
    /// Gets or sets custom marker content, such as an <see cref="Avatar" />. Takes precedence over <see cref="IconName" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets custom marker content, such as an <b>Avatar</b>. Takes precedence over <b>IconName</b>.")]
    [Parameter]
    public RenderFragment? MarkerTemplate { get; set; }

    /// <summary>
    /// Gets or sets the subtitle of the item, such as a timestamp.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the subtitle of the item, such as a timestamp.")]
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets the title of the item.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title of the item.")]
    [Parameter]
    public string? Title { get; set; }

    #endregion
}
