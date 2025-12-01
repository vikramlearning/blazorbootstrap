namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Components/DocxTable/DocxMethodRow.razor.cs" />
/// </summary>
public partial class DocxMethodRow<TItem> : BlazorBootstrapComponentBase
{
    private string AddedVersion => MethodInfo.GetMethodAddedVersion();

    private string Description => MethodInfo.GetMethodDescription();

    public string ReturnType => MethodInfo.GetMethodReturnTypeName() ?? MethodInfo.GetMethodReturnType();

    public string MethodNameWithParameters => $"{MethodInfo.Name}({MethodParameters})";

    public string MethodParameters => MethodInfo.GetMethodParameters();

    public string ReturnTypeShortName => ReturnType.Contains(".")
        ? ReturnType.Split('.').Last()
        : ReturnType;

    [Parameter]
    public MethodInfo MethodInfo { get; set; } = default!;
}
