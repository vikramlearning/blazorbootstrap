namespace BlazorBootstrap;

/// <summary>
/// Defines the supported sizes for a <see cref="ButtonGroup" />, <see cref="RadioButtonGroup{TValue}" />, and <see cref="CheckboxButtonGroup{TValue}" />.
/// </summary>
public enum ButtonGroupSize
{
    /// <summary>
    /// Indicates the default button group size.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates the default button group size.")]
    None,

    /// <summary>
    /// Indicates a small button group.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates a small button group.")]
    Small,

    /// <summary>
    /// Indicates a large button group.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates a large button group.")]
    Large
}
