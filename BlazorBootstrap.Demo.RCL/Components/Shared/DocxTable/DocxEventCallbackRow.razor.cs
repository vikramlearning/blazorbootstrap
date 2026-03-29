namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Components/DocxTable/DocxEventCallbackRow.razor.cs" />
/// </summary>
/// <typeparam name="TItem"></typeparam>
public partial class DocxEventCallbackRow<TItem> : BlazorBootstrapComponentBase
{
    private string AddedVersion => PropertyInfo.GetPropertyAddedVersion();

    private string Description => PropertyInfo.GetPropertyDescription();

    private string ParameterTypeName => PropertyInfo.GetParameterTypeName();

    private string ReturnType => ParameterTypeName ?? PropertyInfo.GetEventCallbackReturnType();

    [Parameter]
    public PropertyInfo PropertyInfo { get; set; } = default!;
}
