
namespace BlazorBootstrap;

/// <summary>
/// For use on properties in <see cref="BootstrapCssSettings"/> when applied to general items, borders, links, texts, etc.
/// </summary>
public enum CssStyleEnum : byte
{
    // Defaults
    None,
    Null,
    Inherit,
    Initial,
    Revert,
    RevertLayer,
    Unset,
    
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
    Outset
}
