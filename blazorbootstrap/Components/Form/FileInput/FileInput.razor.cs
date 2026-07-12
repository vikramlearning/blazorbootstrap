using System.ComponentModel;

namespace BlazorBootstrap;

public partial class FileInput : FileInputBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the component size.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(FileInputSize.Default)]
    [Description("Gets or sets the component size.")]
    [Parameter]
    public FileInputSize Size { get; set; } = FileInputSize.Default;

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.FormControl, true),
            (Size.ToFileInputSizeClass(), Size != FileInputSize.Default));

    #endregion
}
