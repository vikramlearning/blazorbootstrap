namespace BlazorBootstrap;

/// <summary>
/// Defines how items in a <see cref="ListGroup{TItem}" /> are selected.
/// </summary>
public enum ListGroupSelectionMode
{
    /// <summary>
    /// Items are not selectable. Items with a link or click handler are actionable.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Items are not selectable. Items with a link or click handler are actionable.")]
    None,

    /// <summary>
    /// One item can be selected; the list is a listbox.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("One item can be selected; the list is a listbox.")]
    Single,

    /// <summary>
    /// Any number of items can be selected; the list is a multi-select listbox.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Any number of items can be selected; the list is a multi-select listbox.")]
    Multiple,

    /// <summary>
    /// Each item has a checkbox; any number can be checked.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Each item has a checkbox; any number can be checked.")]
    Checkbox,

    /// <summary>
    /// Each item has a radio button; one can be checked.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Each item has a radio button; one can be checked.")]
    Radio
}
