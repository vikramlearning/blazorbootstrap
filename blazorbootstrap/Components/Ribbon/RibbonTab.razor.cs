namespace BlazorBootstrap;

public partial class RibbonTab : BlazorBootstrapComponentBase
{
    #region Methods

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
        Id = IdUtility.GetNextId(); // This is required
        Parent.AddTab(this);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the active state.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the active state.")]
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Gets or sets the disabled state.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the disabled state.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the tab name.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the tab name.")]
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// This event fires when the user clicks the corresponding tab button and the tab is displayed.
    /// </summary>
    [AddedVersion("2.2.0")]
    [Description("This event fires when the user clicks the corresponding tab button and the tab is displayed.")]
    [Parameter]
    public EventCallback<TabEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the parent ribbon.
    /// </summary>
    [CascadingParameter(Name = "Ribbon1")]
    internal Ribbon? Parent { get; set; }

    /// <summary>
    /// Gets or sets the tab title.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the tab title.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the tab title template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the tab title template.")]
    [Parameter]
    public RenderFragment? TitleTemplate { get; set; }

    #endregion
}
