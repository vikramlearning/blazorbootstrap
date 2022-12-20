namespace BlazorBootstrap;

public partial class Sidebar : BaseComponent
{
    #region Events


    #endregion Events

    #region Members

    private DotNetObjectReference<Sidebar> objRef;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        Attributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();

        //ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.autocomplete.initialize", ElementRef, objRef); });
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
