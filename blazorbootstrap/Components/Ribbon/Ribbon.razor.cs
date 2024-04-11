namespace BlazorBootstrap;

public partial class Ribbon : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        //this.AddClass(BootstrapClassProvider.Modal);

        base.BuildClasses();
    }

    protected override void BuildStyles()
    {
        //this.AddStyle("display:block", showBackdrop);

        base.BuildStyles();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
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
