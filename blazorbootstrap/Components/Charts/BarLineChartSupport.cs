namespace BlazorBootstrap;

internal static class BarLineChartSupport
{
    internal static void AppendDataPoint(IChartDataset dataset, IChartDatasetData chartDatasetData)
    {
        if (chartDatasetData is not ChartDatasetData chartDataPoint || !TryGetDataValue(chartDataPoint, out var value))
            return;

        switch (dataset)
        {
            case BarChartDataset barChartDataset when barChartDataset.Label == chartDataPoint.DatasetLabel:
                barChartDataset.Data ??= new List<double?>();
                barChartDataset.Data.Add(value);
                break;
            case LineChartDataset lineChartDataset when lineChartDataset.Label == chartDataPoint.DatasetLabel:
                lineChartDataset.Data ??= new List<double?>();
                lineChartDataset.Data.Add(value);
                break;
        }
    }

    internal static IEnumerable<object> GetSupportedDatasets(ChartData chartData)
    {
        if (chartData?.Datasets is null)
            yield break;

        foreach (var dataset in chartData.Datasets)
            switch (dataset)
            {
                case BarChartDataset barChartDataset:
                    yield return barChartDataset;
                    break;
                case LineChartDataset lineChartDataset:
                    yield return lineChartDataset;
                    break;
            }
    }

    internal static bool IsSupportedDataset(IChartDataset chartDataset) => chartDataset is BarChartDataset or LineChartDataset;

    private static bool TryGetDataValue(ChartDatasetData chartDatasetData, out double? value)
    {
        switch (chartDatasetData.Data)
        {
            case double number:
                value = number;
                return true;
            case int number:
                value = number;
                return true;
            case float number:
                value = number;
                return true;
            case decimal number:
                value = (double)number;
                return true;
            case null:
                value = null;
                return true;
            default:
                value = null;
                return false;
        }
    }
}