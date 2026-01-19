namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Components/DocxTable/DocxMethodRow.razor.cs" />
/// </summary>
public partial class DocxMethodRow<TItem> : BlazorBootstrapComponentBase
{
    private string AddedVersion => MethodInfo.GetMethodAddedVersion();

    private string Description => MethodInfo.GetMethodDescription();

    public string ReturnType => MethodInfo.GetMethodReturnType();

    public string MethodName => MethodInfo.GetMethodName();

    [Parameter]
    public MethodInfo MethodInfo { get; set; } = default!;
}
