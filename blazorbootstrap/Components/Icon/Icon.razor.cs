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
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapIconProvider.Icon(), string.IsNullOrWhiteSpace(CustomIconName));
        builder.Append(BootstrapIconProvider.Icon(Name), string.IsNullOrWhiteSpace(CustomIconName));
        builder.Append(customName, !string.IsNullOrWhiteSpace(CustomIconName));
        builder.Append(BootstrapIconProvider.IconSize(Size)!, Size != IconSize.None);
        builder.Append(BootstrapClassProvider.IconColor(color), Color != IconColor.None);

        base.BuildClasses(builder);
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
