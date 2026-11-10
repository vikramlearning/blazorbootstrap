namespace BlazorBootstrap;

/// <summary>
/// Shows events in order along one connector line, top to bottom or left to right, as an ordered list of <see cref="TimelineItem" /> children
/// (or <see cref="TimelineItems{TItem}" /> for data-bound items).
/// </summary>
/// <remarks>
/// Bootstrap 5.3 has no timeline component; the timeline is built from Bootstrap utilities and a small set of <c>--bb-timeline-*</c> CSS properties.
/// </remarks>
[AddedVersion("4.1.0")]
public partial class Timeline : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible name of the timeline list, read by screen readers.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of the timeline list, read by screen readers.")]
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the items: <see cref="TimelineItem" /> components, <see cref="TimelineItems{TItem}" />, or both.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the items: <b>TimelineItem</b> components, <b>TimelineItems</b>, or both.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-timeline list-unstyled", true),
            (Orientation.ToTimelineOrientationClass(), true),
            (Placement.ToTimelinePlacementClass(), Placement != TimelinePlacement.Start && Orientation == TimelineOrientation.Vertical),
            (CollapseBreakpoint.ToTimelineCollapseClass(), CollapseBreakpoint != TimelineCollapseBreakpoint.None));

    /// <summary>
    /// Gets or sets the width below which a horizontal or alternate timeline shows as a single vertical column.
    /// <para>
    /// Default value is <see cref="TimelineCollapseBreakpoint.Medium" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TimelineCollapseBreakpoint.Medium)]
    [Description("Gets or sets the width below which a horizontal or alternate timeline shows as a single vertical column.")]
    [Parameter]
    public TimelineCollapseBreakpoint CollapseBreakpoint { get; set; } = TimelineCollapseBreakpoint.Medium;

    /// <summary>
    /// Gets or sets whether the items are laid out top to bottom or left to right.
    /// A horizontal timeline whose items do not fit scrolls inside its own container.
    /// <para>
    /// Default value is <see cref="TimelineOrientation.Vertical" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TimelineOrientation.Vertical)]
    [Description("Gets or sets whether the items are laid out top to bottom or left to right. A horizontal timeline whose items do not fit scrolls inside its own container.")]
    [Parameter]
    public TimelineOrientation Orientation { get; set; } = TimelineOrientation.Vertical;

    /// <summary>
    /// Gets or sets where the line and markers of a vertical timeline sit: at the start edge, at the end edge,
    /// or centered with the items on alternating sides. Has no effect on a horizontal timeline.
    /// Below <see cref="CollapseBreakpoint" />, the alternate placements show as <see cref="TimelinePlacement.Start" />.
    /// <para>
    /// Default value is <see cref="TimelinePlacement.Start" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TimelinePlacement.Start)]
    [Description("Gets or sets where the line and markers of a vertical timeline sit: at the start edge, at the end edge, or centered with the items on alternating sides. Has no effect on a horizontal timeline. Below <b>CollapseBreakpoint</b>, the alternate placements show as <b>Start</b>.")]
    [Parameter]
    public TimelinePlacement Placement { get; set; } = TimelinePlacement.Start;

    #endregion
}
