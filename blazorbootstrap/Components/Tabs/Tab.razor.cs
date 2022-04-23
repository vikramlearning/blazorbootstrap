using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.dispose", ElementId);
        }

        await base.DisposeAsync(disposing);
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
    [Parameter, EditorRequired] public RenderFragment Content { get; set; }

    /// <summary>
    /// Gets or sets the active tab.
    /// </summary>
    [Parameter] public bool IsActive { get; set; }

    #endregion Properties
}
