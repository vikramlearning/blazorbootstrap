namespace BlazorBootstrap.Demo.RCL;

public class GoogleMapDemoComponentBase : ComponentBase
{
    public string ApiKey = default!;

    [Inject] private IConfiguration Configuration { get; set; } = default!;

    protected override void OnInitialized()
    {
        ApiKey = Configuration["GoogleMap:ApiKey"].ToString();
        base.OnInitialized();
    }
}
