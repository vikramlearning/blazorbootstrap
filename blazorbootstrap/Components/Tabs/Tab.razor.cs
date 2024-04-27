namespace BlazorBootstrap;

public partial class Tab : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class).Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing && IsRenderComplete)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tabs.dispose", Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        Id = IdGenerator.GetNextId(); // This is required

        Parent.AddTab(this);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Specifies the content to be rendered inside the tab.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment Content { get; set; } = default!;

    /// <summary>
    /// Gets or sets the disabled.
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
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter]
    internal Tabs Parent { get; set; } = default!;

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
