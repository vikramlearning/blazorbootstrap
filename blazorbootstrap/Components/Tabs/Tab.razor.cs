﻿namespace BlazorBootstrap;

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
            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.dispose", ElementId); });

        await base.DisposeAsync(disposing);
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside the tab.
    /// </summary>
    [Parameter, EditorRequired] public RenderFragment Content { get; set; } = default!;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the active tab.
    /// </summary>
    [Parameter] public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the tab name.
    /// </summary>
    [Parameter] public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter] internal Tabs Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tab title.
    /// </summary>
    [Parameter] public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tab title template.
    /// </summary>
    [Parameter] public RenderFragment TitleTemplate { get; set; } = default!;

    #endregion Properties
}
