namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Components/DocxTable/DocxPropertyRow.razor.cs" />
/// </summary>
public partial class DocxPropertyRow<TItem> : BlazorBootstrapComponentBase
{
    private string DefaultValue => PropertyInfo.GetPropertyDefaultValue();

    private string ParameterTypeName => PropertyInfo.GetParameterTypeName() ?? PropertyInfo.PropertyType.GetCSharpTypeName();

    private string PropertyTypeShortName => ParameterTypeName ?? (PropertyTypeName.Contains(".")
        ? PropertyTypeName.Split('.').Last()
        : PropertyTypeName);

    private bool IsRequired => PropertyInfo.IsPropertyRequired();

    private string AddedVersion => PropertyInfo.GetPropertyAddedVersion();

    private string Description => PropertyInfo.GetPropertyDescription();

    private string PropertyTypeName => PropertyInfo.GetPropertyTypeName();

    [Parameter]
    public PropertyInfo PropertyInfo { get; set; } = default!;
}
