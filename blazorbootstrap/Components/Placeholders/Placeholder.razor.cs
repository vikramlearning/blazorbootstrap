namespace BlazorBootstrap;

public partial class Placeholder : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private PlaceholderColor color = PlaceholderColor.None;

    private PlaceholderSize size = PlaceholderSize.None;

    private PlaceholderWidth width = PlaceholderWidth.Col1;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Placeholder);
        this.AddClass(BootstrapClassProvider.PlaceholderWidth(Width));
        this.AddClass(BootstrapClassProvider.PlaceholderColor(Color), Color != PlaceholderColor.None);
        this.AddClass(BootstrapClassProvider.PlaceholderSize(Size), Size != PlaceholderSize.None);

        base.BuildClasses();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the placeholder color. Default is <see cref="PlaceholderColor.None" />.
    /// </summary>
    [Parameter]
    public PlaceholderColor Color
    {
        get => color;
        set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the placeholder size. Default is <see cref="PlaceholderSize.None" />.
    /// </summary>
    [Parameter]
    public PlaceholderSize Size
    {
        get => size;
        set
        {
            size = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the placeholder width. Default is <see cref="PlaceholderWidth.Col1" />.
    /// </summary>
    [Parameter]
    public PlaceholderWidth Width
    {
        get => width;
        set
        {
            width = value;
            DirtyClasses();
        }
    }

    #endregion
}
