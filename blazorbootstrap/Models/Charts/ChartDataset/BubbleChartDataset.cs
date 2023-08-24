namespace BlazorBootstrap;

public class BubbleChartDataset : ChartDataset
{
    #region Properties, Indexers

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public new string? BackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public new string? BorderColor { get; set; }

    public new double BorderWidth { get; set; }

    public new List<BubbleData>? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public new string? HoverBackgroundColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public new string? HoverBorderColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public new string? HoverBorderWidth { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Label { get; set; }

    public int Radius { get; set; } = 3;

    #endregion
}

public class BubbleData
{
    #region Constructors

    public BubbleData(double x, double y, double r)
    {
        X = x;
        Y = y;
        R = r;
    }

    #endregion

    #region Properties, Indexers

    public double R { get; set; }

    public double X { get; set; }
    public double Y { get; set; }

    #endregion
}
