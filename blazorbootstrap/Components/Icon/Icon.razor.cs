namespace BlazorBootstrap;

public partial class Icon : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private IconColor color = IconColor.None;

    private string customName = default!;

    private IconName name;

    private IconSize size = IconSize.None;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapIconProvider.Icon(), string.IsNullOrWhiteSpace(CustomIconName));
        this.AddClass(BootstrapIconProvider.Icon(Name), string.IsNullOrWhiteSpace(CustomIconName));
        this.AddClass(customName, !string.IsNullOrWhiteSpace(CustomIconName));
        this.AddClass(BootstrapIconProvider.IconSize(Size)!, Size != IconSize.None);
        this.AddClass(BootstrapClassProvider.IconColor(color), Color != IconColor.None);

        base.BuildClasses();
    }

    #endregion

    #region Properties, Indexers
    
    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    [Parameter]
    public IconColor Color
    {
        get => color;
        set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Icon name that can be either a string or <see cref="IconName" />.
    /// </summary>
    [Parameter]
    public string CustomIconName
    {
        get => customName;
        set
        {
            customName = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Icon name that can be either a string or <see cref="IconName" />.
    /// </summary>
    [Parameter]
    public IconName Name
    {
        get => name;
        set
        {
            name = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Defines the icon size.
    /// </summary>
    [Parameter]
    public IconSize Size
    {
        get => size;
        set
        {
            size = value;
            DirtyClasses();
        }
    }

    #endregion
}
