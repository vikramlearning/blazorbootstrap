using System.ComponentModel;

namespace BlazorBootstrap;

public enum Position
{
    /// <summary>
    /// No position will be applied to an element.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("No position will be applied to an element.")]
    None,

    /// <summary>
    /// The element is positioned according to the normal flow of the document.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("The element is positioned according to the normal flow of the document.")]
    Static,

    /// <summary>
    /// The element is positioned according to the normal flow of the document,
    /// and then offset relative to itself based on the values of top, right, bottom, and left.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("The element is positioned according to the normal flow of the document, and then offset relative to itself based on the values of top, right, bottom, and left.")]
    Relative,

    /// <summary>
    /// The element is removed from the normal document flow, and no space is created for the element in the page layout.
    /// It is positioned relative to its closest positioned ancestor, if any; otherwise, it is placed relative to the initial
    /// containing block.
    /// Its final position is determined by the values of top, right, bottom, and left.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("The element is removed from the normal document flow, and no space is created for the element in the page layout. It is positioned relative to its closest positioned ancestor, if any; otherwise, it is placed relative to the initial containing block. Its final position is determined by the values of top, right, bottom, and left.")]
    Absolute,

    /// <summary>
    /// The element is removed from the normal document flow, and no space is created for the element in the page layout.
    /// It is positioned relative to the initial containing block established by the viewport
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("The element is removed from the normal document flow, and no space is created for the element in the page layout. It is positioned relative to the initial containing block established by the viewport.")]
    Fixed,

    /// <summary>
    /// The element is positioned according to the normal flow of the document, and then offset relative to its nearest
    /// scrolling ancestor and containing block (nearest block-level ancestor), including table-related elements,
    /// based on the values of top, right, bottom, and left.
    /// </summary>
    [AddedVersion("1.7.0")]
    [Description("The element is positioned according to the normal flow of the document, and then offset relative to its nearest scrolling ancestor and containing block (nearest block-level ancestor), including table-related elements, based on the values of top, right, bottom, and left.")]
    Sticky
}
