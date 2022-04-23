using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Tab : BaseComponent
{
    #region Members

    #endregion Members

    #region Methods

    protected override void OnInitialized()
    {
        ElementId = IdGenerator.Generate; // This is required
        Parent.AddTab(this);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public Tabs Parent { get; set; }

    /// <summary>
    /// Gets or sets the tab title.
    /// </summary>
    [Parameter] public string Title { get; set; }

    /// <summary>
    /// Gets or sets the tab title template.
    /// </summary>
    [Parameter] public RenderFragment TitleTemplate { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    [Parameter] public RenderFragment Content { get; set; }

    /// <summary>
    /// Gets or sets the active tab.
    /// </summary>
    [Parameter] public bool IsActive { get; set; }

    #endregion Properties
}
