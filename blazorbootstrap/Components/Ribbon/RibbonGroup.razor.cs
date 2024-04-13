namespace BlazorBootstrap;

public partial class RibbonGroup : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("bb-ribbon-group");
        this.AddClass(BootstrapClassProvider.Flex);
        this.AddClass(BootstrapClassProvider.FlexRow);
        this.AddClass(BootstrapClassProvider.Border);

        base.BuildClasses();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            // TODO: update
        }

        await base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
