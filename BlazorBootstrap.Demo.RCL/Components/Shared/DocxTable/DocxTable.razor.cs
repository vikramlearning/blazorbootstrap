namespace BlazorBootstrap.Demo.RCL;

public partial class DocxTable<TItem> : BlazorBootstrapComponentBase
{
    [Parameter]
    public DocType DocType { get; set; } = DocType.Parameters;
}
