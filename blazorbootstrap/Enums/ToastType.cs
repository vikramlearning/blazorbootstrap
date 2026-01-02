namespace BlazorBootstrap;

public enum ToastType
{
    /// <summary>
    /// Sets the primary background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the primary background color to the toast.")]
    Primary = 1,

    /// <summary>
    /// Sets the secondary background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the secondary background color to the toast.")]
    Secondary,

    /// <summary>
    /// Sets the success background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the success background color to the toast.")]
    Success,

    /// <summary>
    /// Sets the danger background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the danger background color to the toast.")]
    Danger,

    /// <summary>
    /// Sets the warning background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the warning background color to the toast.")]
    Warning,

    /// <summary>
    /// Sets the info background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the info background color to the toast.")]
    Info,

    /// <summary>
    /// Sets the light background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the light background color to the toast.")]
    Light,

    /// <summary>
    /// Sets the dark background color to the toast.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Sets the dark background color to the toast.")]
    Dark

    // TODO: Review
    // https://getbootstrap.com/docs/5.1/utilities/background/#background-gradient
    // https://getbootstrap.com/docs/5.1/utilities/background/#opacity
}
