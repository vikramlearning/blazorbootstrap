namespace BlazorBootstrap;

public partial class FileInput : FileInputBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.FormControl, true),
            (Size.ToFileInputSizeClass(), Size != FileInputSize.Default));

    #endregion
}
