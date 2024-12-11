namespace BlazorBootstrap;

public abstract class BlazorBootstrapLayoutComponentBase : LayoutComponentBase
{

    #region Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        IsRenderComplete = true;

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Id ??= IdUtility.GetNextId();

        base.OnInitialized();
    }
    
    #endregion

    #region Properties, Indexers

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = default!;

    [Parameter] public string? Class { get; set; }

    public ElementReference Element { get; set; }

    [Parameter] public string? Id { get; set; }

    protected bool IsRenderComplete { get; private set; }

    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public string? Style { get; set; }

    #endregion
}
