namespace BlazorBootstrap;

public partial class RichTextEditorToolbarButton : BlazorBootstrapComponentBase
{
    private IconName Icon => Item switch
    {
        RichTextEditorToolbarItem.Print => IconName.Printer,
        RichTextEditorToolbarItem.Undo => IconName.ArrowCounterclockwise,
        RichTextEditorToolbarItem.Redo => IconName.ArrowClockwise,
        RichTextEditorToolbarItem.Bold => IconName.TypeBold,
        RichTextEditorToolbarItem.Italic => IconName.TypeItalic,
        RichTextEditorToolbarItem.Underline => IconName.TypeUnderline,
        RichTextEditorToolbarItem.Strikethrough => IconName.TypeStrikethrough,
        RichTextEditorToolbarItem.ClearFormatting => IconName.Eraser,
        RichTextEditorToolbarItem.AlignLeft => IconName.TextLeft,
        RichTextEditorToolbarItem.AlignCenter => IconName.TextCenter,
        RichTextEditorToolbarItem.AlignRight => IconName.TextRight,
        RichTextEditorToolbarItem.OrderedList => IconName.ListOl,
        RichTextEditorToolbarItem.UnorderedList => IconName.ListUl,
        RichTextEditorToolbarItem.Blockquote => IconName.Quote,
        RichTextEditorToolbarItem.CodeBlock => IconName.Code,
        RichTextEditorToolbarItem.Link => IconName.Link45Deg,
        RichTextEditorToolbarItem.Image => IconName.Image,
        RichTextEditorToolbarItem.Table => IconName.Table,
        _ => IconName.Type
    };

    private string Label => Item switch
    {
        RichTextEditorToolbarItem.AlignLeft => "Align left",
        RichTextEditorToolbarItem.AlignCenter => "Align center",
        RichTextEditorToolbarItem.AlignRight => "Align right",
        RichTextEditorToolbarItem.OrderedList => "Ordered list",
        RichTextEditorToolbarItem.UnorderedList => "Unordered list",
        RichTextEditorToolbarItem.ClearFormatting => "Clear formatting",
        RichTextEditorToolbarItem.CodeBlock => "Code block",
        _ => System.Text.RegularExpressions.Regex.Replace(Item.ToString(), "([a-z])([A-Z])", "$1 $2")
    };

    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter, EditorRequired] 
    public RichTextEditorToolbarItem Item { get; set; }
}
