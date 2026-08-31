namespace BlazorBootstrap;

/// <summary>
/// Represents an action displayed on a <see cref="TimelineItem"/>.
/// </summary>
public class TimelineAction
{
    /// <summary>Gets or sets the accessible label for the action.</summary>
    public string? AriaLabel { get; set; }

    /// <summary>Gets or sets the Bootstrap button color.</summary>
    public ButtonColor Color { get; set; } = ButtonColor.Secondary;

    /// <summary>Gets or sets additional CSS classes applied to the button.</summary>
    public string? CssClass { get; set; }

    /// <summary>Gets or sets a value indicating whether the action is disabled.</summary>
    public bool Disabled { get; set; }

    /// <summary>Gets or sets Bootstrap Icons, Font Awesome, or custom icon CSS classes.</summary>
    public string? IconCssClass { get; set; }

    /// <summary>Gets or sets the callback invoked with the containing item.</summary>
    public EventCallback<TimelineItem> OnClick { get; set; }

    /// <summary>Gets or sets the button text.</summary>
    public string? Text { get; set; }
}
