namespace BlazorBootstrap
{
  public class LineChartScales : Scales
  {
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    [JsonPropertyOrder( 2 )]
    [JsonPropertyName( "Y1" )]
    public ChartAxes? SecondaryY { get; set; } = new() { Display = false, Position = "right" };
  }
}