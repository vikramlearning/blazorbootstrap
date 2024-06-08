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
    /// Gets or sets the color of the spinner.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="SpinnerColor.None" />.
    /// </remarks>
    [Parameter]
    public SpinnerColor Color { get; set; } = SpinnerColor.None;

    /// <summary>
    /// Gets or sets the size of the spinner.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="SpinnerSize.Medium" />.
    /// </remarks>
    [Parameter]
    public SpinnerSize Size { get; set; } = SpinnerSize.Medium;

    /// <summary>
    /// Gets the width, height, and circles information for the spinner SVG.
    /// </summary>
    private (int Width, int Height, List<SpinnerCircle> Circles) SpinnerSvg => GetSpinnerSvgInfo();

    /// <summary>
    /// Gets or sets the title text used as an accessibility attribute.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of the spinner.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="SpinnerType.Border" />.
    /// </remarks>
    [Parameter]
    public SpinnerType Type { get; set; } = SpinnerType.Border;

    /// <summary>
    /// Gets or sets whether the spinner is visible or not.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// </summary>
    /// <remarks>
    /// Default value is 'Loading...'.
    /// </remarks>
    [Parameter]
    public string? VisuallyHiddenText { get; set; } = "Loading...";

    #endregion
}
