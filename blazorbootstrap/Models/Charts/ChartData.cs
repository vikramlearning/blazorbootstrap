namespace BlazorBootstrap;

public class ChartData
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IChartDataset>? Datasets { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Labels { get; set; }
}