namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Components/DocxTable/DocxPropertyRow.razor.cs" />
/// </summary>
public partial class DocxPropertyRow<TItem> : BlazorBootstrapComponentBase
{
    private string DefaultValue => PropertyInfo.GetPropertyDefaultValue();

    private string ParameterTypeName => PropertyInfo.GetParameterTypeName();

    private bool IsRequired => PropertyInfo.IsPropertyRequired();

    private string AddedVersion => PropertyInfo.GetPropertyAddedVersion(VersionContextType ?? typeof(TItem), AddedVersionOverrides);

    private string Description => PropertyInfo.GetPropertyDescription();

    [Parameter]
    public PropertyInfo PropertyInfo { get; set; } = default!;

    [Parameter]
    public IReadOnlyDictionary<string, string>? AddedVersionOverrides { get; set; }

    [Parameter]
    public Type? VersionContextType { get; set; }
}
