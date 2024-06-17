namespace BlazorBootstrap;

/// <summary>
/// Visualize the loading state of a component or page using the Blazor Bootstrap Spinner component. <br/>
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/spinners/">Bootstrap Spinner</see> documentation.
/// </summary>
public partial class Spinner : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void OnInitialized()
    {
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
    private (int Width, int Height, IReadOnlyList<SpinnerCircle> Circles) GetSpinnerSvgInfo()
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


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Color): Color = (SpinnerColor)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Size): Size = (SpinnerSize)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(Title): Title = (string)parameter.Value; break;
                case nameof(Type): Type = (SpinnerType)parameter.Value; break;
                case nameof(Visible): Visible = (bool)parameter.Value; break;
                case nameof(VisuallyHiddenText): VisuallyHiddenText = (string)parameter.Value; break;

                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
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
    private (int Width, int Height, IReadOnlyList<SpinnerCircle> Circles) SpinnerSvg => GetSpinnerSvgInfo();

    /// <summary>
    /// Gets or sets the title text used as an accessibility attribute.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="true" />.
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
