using System.Reflection.Metadata.Ecma335;
using System;

namespace BlazorBootstrap;

public partial class Spinner : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private SpinnerColor color = SpinnerColor.None;
    private SpinnerType type = SpinnerType.Border;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.SpinnerBorder(), Type == SpinnerType.Border);
        builder.Append(BootstrapClassProvider.SpinnerGrow(), Type == SpinnerType.Grow);
        builder.Append(BootstrapClassProvider.SpinnerDots(), Type == SpinnerType.Dots);
        builder.Append(BootstrapClassProvider.Spinner(Color));

        base.BuildClasses(builder);
    }

    protected override void OnInitialized()
    {
        Attributes ??= new Dictionary<string, object>();

        if (Type != SpinnerType.Dots)
        {
            if (string.IsNullOrWhiteSpace(Title))
                Attributes.Remove("title"); 
            else if(!Attributes.TryGetValue("title", out _))
                Attributes.Add("title", Title);
            else if (Attributes.TryGetValue("title", out _))
                Attributes["title"] = Title;
        }

        base.OnInitialized();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

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

    [Parameter] public int Height { get; set; } = 16;

    [Parameter] public string Title { get; set; } = "Loading...";

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

    [Parameter] public int Width { get; set; } = 32;

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// </summary>
    [Parameter]
    public string VisuallyHiddenText { get; set; } = "Loading...";

    #endregion
}
