namespace BlazorBootstrap;

/// <summary>
/// Defines the supported button behaviors when rendering a BlazorBootstrap button.
/// </summary>
public enum ButtonType
{
    /// <summary>
    /// Indicates a standard clickable button behavior.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates a standard clickable button behavior.")]
    Button,

    /// <summary>
    /// Indicates a submit action for the nearest associated form.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates a submit action for the nearest associated form.")]
    Submit,

    /// <summary>
    /// Indicates a reset action for the nearest associated form.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates a reset action for the nearest associated form.")]
    Reset,

    /// <summary>
    /// Indicates a navigation-style behavior typically rendered as a link.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Indicates a navigation-style behavior typically rendered as a link.")]
    Link
}
