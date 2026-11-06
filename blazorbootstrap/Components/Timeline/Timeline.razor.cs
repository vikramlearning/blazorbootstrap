namespace BlazorBootstrap;

/// <summary>
/// Displays a responsive chronological sequence of events.
/// </summary>
public partial class Timeline : BlazorBootstrapComponentBase
{
    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-timeline", true),
            ("bb-timeline-vertical", Orientation == TimelineOrientation.Vertical),
            ("bb-timeline-horizontal", Orientation == TimelineOrientation.Horizontal),
            ("bb-timeline-right", Orientation == TimelineOrientation.Vertical && Alignment == TimelineAlignment.Right),
            ("bb-timeline-alternate", Orientation == TimelineOrientation.Vertical && AlternateItems));

    private static string GetActionClass(TimelineAction action) =>
        BuildClassNames(action.CssClass, ("btn btn-sm", true), (action.Color.ToButtonColorClass(), action.Color != ButtonColor.None));

    private static string GetBadgeClass(TimelineItem item) =>
        BuildClassNames(("badge", true), (item.BadgeColor.ToBadgeColorClass(), item.BadgeColor != BadgeColor.None));

    private static string GetIconClass(TimelineItem item) =>
        BuildClassNames((item.IconCssClass, true), (item.IconColor.ToIconColorClass(), item.IconColor != IconColor.None));

    private static string GetItemClass(TimelineItem item) => BuildClassNames(item.CssClass, ("bb-timeline-item", true));

    private static Task InvokeActionAsync(TimelineAction action, TimelineItem item) =>
        action.Disabled || !action.OnClick.HasDelegate ? Task.CompletedTask : action.OnClick.InvokeAsync(item);

    /// <summary>Gets or sets the accessible name of the timeline.</summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Timeline";

    /// <summary>Gets or sets the alignment of a vertical timeline.</summary>
    [Parameter]
    public TimelineAlignment Alignment { get; set; } = TimelineAlignment.Left;

    /// <summary>Gets or sets whether vertical items alternate around a centered rail on larger screens.</summary>
    [Parameter]
    public bool AlternateItems { get; set; }

    /// <summary>Gets or sets the events to display.</summary>
    [Parameter]
    public IEnumerable<TimelineItem>? Items { get; set; }

    /// <summary>Gets or sets the timeline orientation.</summary>
    [Parameter]
    public TimelineOrientation Orientation { get; set; } = TimelineOrientation.Vertical;
}
