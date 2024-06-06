
namespace BlazorBootstrap;

/// <summary>
/// For use on properties in <see cref="BootstrapCssSettings"/> when applied to general items, borders, links, texts, etc.
/// </summary>
public enum CssStyleEnum : byte
{
    // Defaults
    /// <summary>
    /// No Css style applied.
    /// </summary>
    NotApplied,
    /// <summary>
    /// Null value.
    /// </summary>
    Null,
    /// <summary>
    /// Inherit from parent
    /// </summary>
    Inherit,
    /// <summary>
    /// Initial value of the property.
    /// </summary>
    Initial,
    /// <summary>
    /// Revert value of the property.
    /// </summary>
    Revert,
    /// <summary>
    /// Revert value of the property for a specific layer.
    /// </summary>
    RevertLayer,
    /// <summary>
    /// Unset value of the property.
    /// </summary>
    Unset,
    /// <summary>
    /// The automatic value of the property.
    /// </summary>
    Auto,

    // Clip
    /// <summary>
    /// Determine the width and height based on the parent container
    /// </summary>
    BorderBox,
    /// <summary>
    /// Determine the width and height based on the parent, but not including padding
    /// </summary>
    ContentBox,
    /// <summary>
    /// 
    /// </summary>
    Text,

    // Display

    /// <summary>
    /// The element generates one or more inline boxes that do not generate line breaks before or after themselves.
    /// In normal flow, the next element will be on the same line if there is space.
    /// </summary>
    Inline,
    /// <summary>
    /// The element generates a block box that will be flowed with surrounding content as if it were a single inline box (behaving much like a replaced element would).
    /// </summary>
    InlineBlock,
    /// <summary>
    /// The element behaves like an inline-level element and lays out its content according to the flexbox model.
    /// </summary>
    InlineFlex,
    /// <summary>
    /// The element behaves like an inline-level element and lays out its content according to the grid model.
    /// </summary>
    InlineGrid,
    /// <summary>
    /// The inline-table value does not have a direct mapping in HTML. It behaves like an HTML table element, but as an inline box, rather than a block-level box. Inside the table box is a block-level context.
    /// </summary>
    InlineTable,
    /// <summary>
    /// The element generates a block box for the content and a separate list-item inline box.
    /// </summary>
    ListItem,
    /// <summary>
    /// The element generates a block box, generating line breaks both before and after the element when in the normal flow.
    /// </summary>
    Block,
    /// <summary>
    /// The element behaves like a block-level element and lays out its content according to the flexbox model.
    /// </summary>
    Flex,
    /// <summary>
    /// The element behaves like a block-level element and lays out its content according to the grid model.
    /// </summary>
    Grid,
    /// <summary>
    /// These elements behave like HTML table elements. It defines a block-level box.
    /// </summary>
    Table,
    /// <summary>
    /// These elements behave like tr HTML elements.
    /// </summary>
    TableRow,
    /// <summary>
    /// The element generates a block box that establishes a new block formatting context, defining where the formatting root lies.
    /// </summary>
    FlowRoot,
    /// <summary>
    /// Do not display the content
    /// </summary>
    None,
    
    // Flex-direction
    Row,
    RowReverse,
    Column,
    ColumnReverse,

    // Ordering of items
    Before,
    After,

    // Text/Links/etc
    Underline,
    Overline,
    LineThrough,
    
    // Borders/outlines/etc.
    Dotted,
    Dashed,
    Solid,
    Double,
    Groove,
    Ridge,
    Inset,
    Outset,

    // position
    /// <summary>
    /// The element is positioned according to the normal flow of the document, and then offset relative to itself based on the values of top, right, bottom, and left.
    /// The offset does not affect the position of any other elements; thus, the space given for the element in the page layout is the same as if position were static.
    /// </summary>
    Relative,
    /// <summary>
    /// The element is removed from the normal document flow, and no space is created for the element in the page layout.
    /// The element is positioned relative to its closest positioned ancestor (if any) or to the initial containing block.
    /// Its final position is determined by the values of top, right, bottom, and left.
    /// </summary>
    Absolute,
    /// <summary>
    /// The element is removed from the normal document flow, and no space is created for the element in the page layout.
    /// The element is positioned relative to its initial containing block, which is the viewport in the case of visual media.
    /// Its final position is determined by the values of top, right, bottom, and left.
    /// </summary>
    Fixed,
    /// <summary>
    /// The element is positioned according to the normal flow of the document, and then offset relative to its nearest scrolling ancestor and containing block (nearest block-level ancestor), including table-related elements,
    /// based on the values of top, right, bottom, and left. The offset does not affect the position of any other elements.
    /// </summary>
    Sticky,
    /// <summary>
    /// The element is positioned according to the Normal Flow of the document. The top, right, bottom, left, and z-index properties have no effect. This is the default value.
    /// </summary>
    Static,
    
    // text-align
    Start,
    End,
    Left,
    Right,
    Center,
    Justify,
    JustifyAll,
    MatchParent,
    
    // word-break
    Normal,
    BreakAll,
    KeepAll,
    BreakWord,
    AutoPhrase
}
