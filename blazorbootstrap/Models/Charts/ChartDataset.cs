namespace BlazorBootstrap;

public class ChartDataset
{
    public List<string> BackgroundColor { get; set; }
    public List<string> BorderColor { get; set; }
    public double BorderWidth { get; set; }
    public string Clip { get; set; } // TODO: chane data type
    public List<int> Data { get; set; }
    public List<string> HoverBackgroundColor { get; set; }
    public List<string> HoverBorderColor { get; set; }
    public List<string> HoverBorderWidth { get; set; }
}
