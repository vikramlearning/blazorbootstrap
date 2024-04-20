namespace BlazorBootstrap.Demo.RCL;

public partial class Skippy : ComponentBase
{
    [Parameter]
    public string Url { get; set; } = default!;

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;
}
