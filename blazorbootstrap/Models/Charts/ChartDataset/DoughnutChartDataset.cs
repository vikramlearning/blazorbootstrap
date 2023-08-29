﻿namespace BlazorBootstrap;

public class DoughnutChartDataset : ChartDataset
{
    #region Properties, Indexers

    /// <summary>
    /// The label for the dataset which appears in the legend and tooltips.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    #endregion
}