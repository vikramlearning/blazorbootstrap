namespace BlazorBootstrap.Models;

public sealed record GridColumnFilterRenderContext<T>(GridColumnFilterContext GridContext, GridColumn<T> Column)
{
}
