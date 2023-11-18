namespace BlazorBootstrap.Demo.RCL;

public partial class CarbonAds : ComponentBase
{
    #region Fields and Constants

    private string? classNames;

    #endregion

    #region Properties, Indexers

    [Parameter]
    public string? ClassNames
    {
        get => classNames;
        set => classNames = $"mt-5 {value ?? ""}".Trim();
    }

    #endregion
}
