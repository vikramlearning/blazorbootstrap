namespace BlazorBootstrap;

/// <summary>
/// Direction the items in a <see cref="Dropdown" /> should be ordered in.
/// </summary>
public enum DropdownDirection
{
    /// <summary>
    /// Displays the dropdown menu below the <see cref="Dropdown" /> element.
    /// </summary>
    Dropdown,

    /// <summary>
    /// Displays the dropdown menu below the <see cref="Dropdown" /> element and centered.
    /// </summary>
    DropdownCentered,

    /// <summary>
    /// Displays the dropdown menu right of the <see cref="Dropdown" /> element.
    /// </summary>
    DropEnd,

    /// <summary>
    /// Displays the dropdown menu above the <see cref="Dropdown" /> element.
    /// </summary>
    DropUp,

    /// <summary>
    /// Displays the dropdown menu above the <see cref="Dropdown" /> element and centered.
    /// </summary>
    DropUpCentered,

    /// <summary>
    /// Displays the dropdown menu start the <see cref="Dropdown" /> element.
    /// </summary>
    DropStart
}
