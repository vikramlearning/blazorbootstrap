namespace BlazorBootstrap;

public partial class Spinner : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private SpinnerColor color = SpinnerColor.None;

    private SpinnerType type = SpinnerType.Border;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.SpinnerType(Type));
        this.AddClass(BootstrapClassProvider.SpinnerColor(Color));
        this.AddClass(BootstrapClassProvider.SpinnerTypeSize(Type, Size), Type is (SpinnerType.Border or SpinnerType.Grow));

        base.BuildClasses();
    }

    protected override void OnInitialized()
    {
        Attributes ??= new Dictionary<string, object>();

        if (Type != SpinnerType.Dots)
        {
            if (string.IsNullOrWhiteSpace(Title))
                Attributes.Remove("title");
            else if (!Attributes.TryGetValue("title", out _))
                Attributes.Add("title", Title);
            else if (Attributes.TryGetValue("title", out _))
                Attributes["title"] = Title;
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

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the color of the spinner.
    /// </summary>
    [Parameter]
    public SpinnerColor Color
    {
        get => color;
        set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the size of the spinner.
    /// </summary>
    [Parameter]
    public SpinnerSize Size { get; set; } = SpinnerSize.Medium;

    /// <summary>
    /// Gets the width, height, and circles information for the spinner SVG.
    /// </summary>
    private (int Width, int Height, List<SpinnerCircle> Circles) SpinnerSvg => GetSpinnerSvgInfo();

    /// <summary>
    /// Gets or sets the title text used as an accessibility attribute.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of the spinner.
    /// </summary>
    [Parameter]
    public SpinnerType Type
    {
        get => type;
        set
        {
            type = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets whether the spinner is visible or not.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// </summary>
    [Parameter]
    public string? VisuallyHiddenText { get; set; } = "Loading...";

    #endregion
}
