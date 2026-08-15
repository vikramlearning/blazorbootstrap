namespace BlazorBootstrap;

public partial class RichTextEditorToolbarGroup : BlazorBootstrapComponentBase
{
    [Parameter, EditorRequired] 
    public RenderFragment? ChildContent { get; set; }

    [Parameter, EditorRequired] 
    public string Label { get; set; } = default!;
}
