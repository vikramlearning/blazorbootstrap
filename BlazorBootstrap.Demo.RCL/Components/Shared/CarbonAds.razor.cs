namespace BlazorBootstrap.Demo.RCL;

public partial class CarbonAds : ComponentBase
{
    #region Properties, Indexers

    [Parameter]
    public string? Class { get; set; }

    private bool isLocalUrl;

    [Inject]
    private NavigationManager _navigationManager { get; set; } = default!;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        isLocalUrl = _navigationManager.Uri.Contains("localhost");
    }

    #endregion
}
