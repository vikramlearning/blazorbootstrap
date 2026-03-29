namespace BlazorBootstrap.Demo.RCL;

public partial class DocxEnumRow<TItem> : BlazorBootstrapComponentBase
{
    private string Name => EnumValue.ToString();

    private string Value => Convert.ToInt64(EnumValue).ToString();

    private string Description => EnumFieldInfo?
        .GetCustomAttribute<DescriptionAttribute>(inherit: false)?
        .Description ?? Name;

    private string AddedVersion => EnumFieldInfo?
        .GetCustomAttribute<AddedVersionAttribute>(inherit: false)?
        .Version ?? string.Empty;

    private FieldInfo? EnumFieldInfo => EnumValue.GetType().GetField(Name);

    [Parameter]
    public Enum EnumValue { get; set; } = default!;
}
