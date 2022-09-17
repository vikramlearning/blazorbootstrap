namespace BlazorBootstrap;

public class ChartData<TChartDataset> where TChartDataset : ChartDataset
{
    public List<string> Labels { get; set; }
    public List<TChartDataset> Datasets { get; set; }
}