namespace BlazorBootstrap;

/// <summary>
/// Represents an event rendered by a <see cref="Timeline"/>.
/// </summary>
public class TimelineItem
{
    /// <summary>Gets or sets the actions displayed below the item content.</summary>
    public IEnumerable<TimelineAction>? Actions { get; set; }

    /// <summary>Gets or sets an optional badge or label.</summary>
    public string? Badge { get; set; }

    /// <summary>Gets or sets the Bootstrap color used by the badge.</summary>
    public BadgeColor BadgeColor { get; set; } = BadgeColor.Secondary;

    /// <summary>Gets or sets additional content rendered in the item body.</summary>
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets additional CSS classes applied to the item.</summary>
    public string? CssClass { get; set; }

    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets Bootstrap Icons, Font Awesome, or custom icon CSS classes.</summary>
    public string? IconCssClass { get; set; }

    /// <summary>Gets or sets the icon color.</summary>
    public IconColor IconColor { get; set; } = IconColor.Primary;

    /// <summary>Gets or sets an accessible description of the image.</summary>
    public string? ImageAlt { get; set; }

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the timeline marker icon is hidden.</summary>
    public bool ShowIcon { get; set; } = true;

    /// <summary>Gets or sets the optional timestamp.</summary>
    public string? Timestamp { get; set; }

    /// <summary>Gets or sets the item title.</summary>
    public string? Title { get; set; }
}
