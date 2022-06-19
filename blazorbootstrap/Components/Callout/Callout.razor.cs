using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Callout : BaseComponent
{
    #region Members

    private CalloutColor color = CalloutColor.None;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Callout());
        builder.Append(BootstrapClassProvider.ToCalloutColor(Color));

        base.BuildClasses(builder);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the callout color.
    /// </summary>
    [Parameter]
    public CalloutColor Color
    {
        get => color; set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    #endregion Properties
}
