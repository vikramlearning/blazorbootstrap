namespace BlazorBootstrap;

public enum DropdownAutoCloseBehavior
{
    /// <summary>
    /// <see cref="Dropdown" /> will be closed (only) by clicking inside the dropdown menu.
    /// </summary>
    Inside,

    /// <summary>
    /// <see cref="Dropdown" /> will be closed (only) by clicking outside the dropdown menu.
    /// </summary>
    Outside,

    /// <summary>
    /// <see cref="Dropdown" /> will be closed by clicking outside or inside the dropdown menu.
    /// </summary>
    Both
}