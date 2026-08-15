namespace BlazorBootstrap;

public partial class RichTextEditorToolbar : BlazorBootstrapComponentBase
{
    private static readonly RichTextEditorToolbarItem[] historyItems = { RichTextEditorToolbarItem.Undo, RichTextEditorToolbarItem.Redo };
    private static readonly RichTextEditorToolbarItem[] textItems = { RichTextEditorToolbarItem.Paragraph, RichTextEditorToolbarItem.Heading1, RichTextEditorToolbarItem.Heading2, RichTextEditorToolbarItem.Heading3, RichTextEditorToolbarItem.Bold, RichTextEditorToolbarItem.Italic, RichTextEditorToolbarItem.Underline, RichTextEditorToolbarItem.Strikethrough, RichTextEditorToolbarItem.ClearFormatting };
    private static readonly RichTextEditorToolbarItem[] paragraphItems = { RichTextEditorToolbarItem.AlignLeft, RichTextEditorToolbarItem.AlignCenter, RichTextEditorToolbarItem.AlignRight, RichTextEditorToolbarItem.Justify, RichTextEditorToolbarItem.Indent, RichTextEditorToolbarItem.Outdent, RichTextEditorToolbarItem.OrderedList, RichTextEditorToolbarItem.UnorderedList, RichTextEditorToolbarItem.Blockquote, RichTextEditorToolbarItem.CodeBlock };
    private static readonly RichTextEditorToolbarItem[] insertItems = { RichTextEditorToolbarItem.Link, RichTextEditorToolbarItem.Image, RichTextEditorToolbarItem.Table, RichTextEditorToolbarItem.HorizontalRule };

    private bool HasAny(IEnumerable<RichTextEditorToolbarItem> toolbarItems) => toolbarItems.Any(Items.Contains);

    [Parameter, EditorRequired] 
    public IReadOnlyCollection<RichTextEditorToolbarItem> Items { get; set; } = Array.Empty<RichTextEditorToolbarItem>();

    [Parameter] 
    public bool Disabled { get; set; }
}
