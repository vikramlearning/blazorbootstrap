namespace BlazorBootstrap;

public partial class Spinner : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (Type != SpinnerType.Dots)
        {
            if (string.IsNullOrWhiteSpace(Title))
                AdditionalAttributes.Remove("title");
            else if (!AdditionalAttributes.TryGetValue("title", out _))
                AdditionalAttributes.Add("title", Title);
            else if (AdditionalAttributes.TryGetValue("title", out _))
                AdditionalAttributes["title"] = Title;
        }

        base.OnInitialized();
    }

    /// <summary>
    /// Calculates width, height, and circles information for the spinner SVG.
    /// </summary>
    /// <returns>A tuple containing width, height, and a list of spinner circles.</returns>
    private (int Width, int Height, List<SpinnerCircle> Circles) GetSpinnerSvgInfo()
    {
        // Calculate radius based on Size
        var radius = 4; // default: SpinnerSize.Medium

        if (Size == SpinnerSize.Small)
            radius = 2;
        else if (Size == SpinnerSize.Large)
            radius = 6;
        else if (Size == SpinnerSize.ExtraLarge)
            radius = 8;

        var defaultSpace = 4;

        // Calculate other dimensions based on radius
        var diameter = 2 * radius;

        var circle1 = new SpinnerCircle(radius, radius, diameter);
        var circle2 = new SpinnerCircle(radius, circle1.Cx + diameter + defaultSpace, diameter);
        var circle3 = new SpinnerCircle(radius, circle2.Cx + diameter + defaultSpace, diameter);

        var width = defaultSpace + diameter * 3 + defaultSpace;
        var height = defaultSpace + diameter + defaultSpace;

        return (width, height, new List<SpinnerCircle> { circle1, circle2, circle3 });
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (Type.ToSpinnerTypeClass(), true),
            (Color.ToSpinnerColorClass(), true),
            ($"{Type.ToSpinnerTypeClass()}-{Size.ToSpinnerSizeClass()}", Type is (SpinnerType.Border or SpinnerType.Grow)));

    /// <summary>
    /// Gets or sets the color of the <see cref="Spinner" />.
    /// <para>
    /// Default value is <see cref="SpinnerColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.0.0")]
    [DefaultValue(null)]
    [Description("")]
    [Parameter]
    public SpinnerColor Color { get; set; } = SpinnerColor.None;

    /// <summary>
    /// Gets or sets the size of the <see cref="Spinner" />.
    /// <para>
    /// Default value is <see cref="SpinnerSize.Medium" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.0.0")]
    [DefaultValue(SpinnerSize.Medium)]
    [Description("Gets or sets the color of the <b>Spinner</b>.")]
    [Parameter]
    public SpinnerSize Size { get; set; } = SpinnerSize.Medium;

    /// <summary>
    /// Gets the width, height, and circles information for the <see cref="Spinner" /> SVG.
    /// </summary>
    private (int Width, int Height, List<SpinnerCircle> Circles) SpinnerSvg => GetSpinnerSvgInfo();

    /// <summary>
    /// Gets or sets the title text used as an accessibility attribute.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title text used as an accessibility attribute.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of the <see cref="Spinner" />.
    /// <para>
    /// Default value is <see cref="SpinnerType.Border" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.0.0")]
    [DefaultValue(SpinnerType.Border)]
    [Description("Gets or sets the type of the <b>Spinner</b>.")]
    [Parameter]
    public SpinnerType Type { get; set; } = SpinnerType.Border;

    /// <summary>
    /// Gets or sets whether the <see cref="Spinner" /> is visible or not.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.0.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the <b>Spinner</b> is visible or not.")]
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// <para>
    /// Default value is 'Loading...'.
    /// </para>
    /// </summary>
    [AddedVersion("2.0.0")]
    [DefaultValue("Loading...")]
    [Description("Gets or sets the visually hidden text.")]
    [Parameter]
    public string VisuallyHiddenText { get; set; } = "Loading...";

    #endregion
}
