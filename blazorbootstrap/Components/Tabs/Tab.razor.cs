using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Tab : BaseComponent
{
    #region Members

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        ElementId = IdGenerator.Generate;
        Console.WriteLine($"{Title} tab log");
        Parent.AddTab(this);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public Tabs Parent { get; set; }

    /// <summary>
    /// Gets or sets the tab title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the tab title template.
    /// </summary>
    public RenderFragment TitleTemplate { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    [Parameter] public RenderFragment Content { get; set; }


    #endregion
}
