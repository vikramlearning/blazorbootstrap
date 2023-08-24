namespace BlazorBootstrap;

/// <summary>
/// Defines the position of an element.
/// </summary>
public enum Position
{
    /// <summary>
    /// No position will be applied to an element.
    /// </summary>
    None,

    /// <summary>
    /// The element is positioned according to the normal flow of the document.
    /// </summary>
    Static,

    /// <summary>
    /// The element is positioned according to the normal flow of the document,
    /// and then offset relative to itself based on the values of top, right, bottom, and left.
    /// </summary>
    Relative,

    /// <summary>
    /// The element is removed from the normal document flow, and no space is created for the element in the page layout.
    /// It is positioned relative to its closest positioned ancestor, if any; otherwise, it is placed relative to the initial
    /// containing block.
    /// Its final position is determined by the values of top, right, bottom, and left.
    /// </summary>
    Absolute,

    /// <summary>
    /// The element is removed from the normal document flow, and no space is created for the element in the page layout.
    /// It is positioned relative to the initial containing block established by the viewport
    /// </summary>
    Fixed,

    /// <summary>
    /// The element is positioned according to the normal flow of the document, and then offset relative to its nearest
    /// scrolling ancestor and containing block (nearest block-level ancestor), including table-related elements,
    /// based on the values of top, right, bottom, and left.
    /// </summary>
    Sticky
}
