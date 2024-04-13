namespace BlazorBootstrap;

public partial class RibbonTab : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && Rendered)
        {
            try
            {
                await JS.InvokeVoidAsync("window.blazorBootstrap.tabs.dispose", ElementId);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }
        }

        await base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
        ElementId = IdGenerator.GetNextId(); // This is required
        Parent.AddTab(this);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside the tab.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment Content { get; set; } = default!;

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the active tab.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the tab name.
    /// </summary>
    [Parameter]
    public string Name { get; set; } = default!;

    /// <summary>
    /// This event fires when the user clicks the corresponding tab button and the tab is displayed.
    /// </summary>
    [Parameter]
    public EventCallback<TabEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the parent ribbon.
    /// </summary>
    [CascadingParameter(Name = "Ribbon1")]
    internal Ribbon Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tab title.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tab title template.
    /// </summary>
    [Parameter]
    public RenderFragment TitleTemplate { get; set; } = default!;

    #endregion
}
