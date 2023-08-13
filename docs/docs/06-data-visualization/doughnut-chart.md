---
title: Blazor Doughnut Chart
description: A Blazor Bootstrap donut chart component is a circular chart that shows the proportional values of different categories. It is similar to a pie chart, but the center of the donut chart is hollow. This makes it easier to see the individual values of each category.
image: https://i.imgur.com/Q1bBWPQ.png

sidebar_label: Doughnut Chart
sidebar_position: 2
---

A Blazor Bootstrap donut chart component is a circular chart that shows the proportional values of different categories. 
It is similar to a pie chart, but the center of the donut chart is hollow. This makes it easier to see the individual values of each category.

<img src="https://i.imgur.com/Q1bBWPQ.png" alt="Blazor Chart Component - Blazor Doughnut Chart" />

## Parameters

| Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| Height | int | | | Gets or sets chart height. | 1.0.0 |
| Width | int | | | Get or sets chart width. | 1.0.0 |

## Methods

| Name | Return type | Descritpion | Added Version |
|:--|:--|:--|:--|
| AddDataAsync | `ChartData` | Adds data to bar chart. | 1.10.0 |
| AddDatasetAsync | `ChartData` | Adds dataset to bar chart. | 1.10.0 |
| InitializeAsync | Task | Initialize Bar Chart. | 1.0.0 |
| UpdateAsync | Task | Update Bar Chart. | 1.0.0 |

## ChartData Members

| Property Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| Labels | `List<string>` | null | ✔️ | Gets or sets the Labels. | 1.0.0 |
| Datasets | `List<IChartDataset>` | null | ✔️ | Gets or sets the Datasets. | 1.0.0 |

## DoughnutChartDataset Members

:::info
**DoughnutChartDataset** implements **IChartDataset**.
:::

| Property Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| BackgroundColor | `List<string>` | null | | Get or sets the BackgroundColor. | 1.0.0 |
| BorderColor | `List<string>` | null | | Get or sets the BorderColor. | 1.0.0 |
| BorderWidth | `List<double>` | null | | Get or sets the BorderWidth. | 1.0.0 |
| Clip | string | null | | How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside chartArea. 0 = clip at chartArea. Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0} | 1.0.0 |
| Data | `List<double>` | null | | Get or sets the Data. | 1.0.0 |
| Hidden | bool | false | | Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart. | 1.0.0 |
| HoverBackgroundColor | `List<string>` | null | ✔️ | Get or sets the HoverBackgroundColor. | 1.0.0 |
| HoverBorderColor | `List<string>` | null | ✔️ | Get or sets the HoverBorderColor. | 1.0.0 |
| HoverBorderWidth | `List<double>` | null | ✔️ | Get or sets the HoverBorderWidth. | 1.0.0 |
| Type | string | null | ✔️ | Get or sets the chart type. | 1.0.0 |

## DoughnutChartOptions Members

| Property Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| Responsive | bool | false | | Gets or sets the Responsive. | 1.0.0 |

## Examples

### How it works

In the following example, a categorical 12-color palette is used.

:::tip
For data visualization, you can use the predefined palettes `ColorBuilder.CategoricalTwelveColors` for a 12-color palette and `ColorBuilder.CategoricalSixColors` for a 6-color palette. 
These palettes offer a range of distinct and visually appealing colors that can be applied to represent different categories or data elements in your visualizations.
:::

<img src="https://i.imgur.com/Q1bBWPQ.png" alt="Blazor Bootstrap: Doughnut Chart Component - How it works" />

```cshtml {} showLineNumbers
<DoughnutChart @ref="doughnutChart" Width="500" Class="mb-5" />

<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await RandomizeAsync()"> Randomize </Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await AddDatasetAsync()"> Add Dataset </Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await AddDataAsync()">Add Data</Button>
```

```cs {} showLineNumbers
@code {
    private DoughnutChart doughnutChart = default!;
    private DoughnutChartOptions doughnutChartOptions = default!;
    private ChartData chartData = default!;
    private string[]? backgroundColors;

    private int datasetsCount = 0;
    private int dataLabelsCount = 0;

    private Random random = new();

    protected override void OnInitialized()
    {
        backgroundColors = ColorBuilder.CategoricalTwelveColors;
        chartData = new ChartData { Labels = GetDefaultDataLabels(4), Datasets = GetDefaultDataSets(1) };

        doughnutChartOptions = new();
        doughnutChartOptions.Responsive = true;
        doughnutChartOptions.Plugins.Title.Text = "2022 - Sales";
        doughnutChartOptions.Plugins.Title.Display = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await doughnutChart.InitializeAsync(chartData, doughnutChartOptions);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task RandomizeAsync()
    {
        if (chartData is null || chartData.Datasets is null || !chartData.Datasets.Any()) return;

        var newDatasets = new List<IChartDataset>();

        foreach (var dataset in chartData.Datasets)
        {
            if (dataset is DoughnutChartDataset doughnutChartDataset
                && doughnutChartDataset is not null
                && doughnutChartDataset.Data is not null)
            {
                var count = doughnutChartDataset.Data.Count;

                var newData = new List<double>();
                for (var i = 0; i < count; i++)
                {
                    newData.Add(random.Next(0, 100));
                }

                doughnutChartDataset.Data = newData;
                newDatasets.Add(doughnutChartDataset);
            }
        }

        chartData.Datasets = newDatasets;

        await doughnutChart.UpdateAsync(chartData, doughnutChartOptions);
    }

    private async Task AddDatasetAsync()
    {
        if (chartData is null || chartData.Datasets is null) return;

        var chartDataset = GetRandomDoughnutChartDataset();
        chartData = await doughnutChart.AddDatasetAsync(chartData, chartDataset, doughnutChartOptions);
    }

    private async Task AddDataAsync()
    {
        if (dataLabelsCount >= 12)
            return;

        if (chartData is null || chartData.Datasets is null)
            return;

        var data = new List<IChartDatasetData>();
        foreach (var dataset in chartData.Datasets)
        {
            if (dataset is DoughnutChartDataset doughnutChartDataset)
            {
                data.Add(new DoughnutChartDatasetData(doughnutChartDataset.Label, random.Next(0, 100), backgroundColors![dataLabelsCount]));
            }
        }

        chartData = await doughnutChart.AddDataAsync(chartData, GetNextDataLabel(), data);

        dataLabelsCount += 1;
    }

    #region Data Preparation

    private List<IChartDataset> GetDefaultDataSets(int numberOfDatasets)
    {
        var datasets = new List<IChartDataset>();

        for (var index = 0; index < numberOfDatasets; index++)
        {
            datasets.Add(GetRandomDoughnutChartDataset());
        }

        return datasets;
    }

    private DoughnutChartDataset GetRandomDoughnutChartDataset()
    {
        datasetsCount += 1;
        return new() { Label = $"Team {datasetsCount}", Data = GetRandomData(), BackgroundColor = GetRandomBackgroundColors() };
    }

    private List<double> GetRandomData()
    {
        var data = new List<double>();
        for (var index = 0; index < dataLabelsCount; index++)
        {
            data.Add(random.Next(0, 100));
        }

        return data;
    }

    private List<string> GetRandomBackgroundColors()
    {
        var colors = new List<string>();
        for (var index = 0; index < dataLabelsCount; index++)
        {
            colors.Add(backgroundColors![index]);
        }

        return colors;
    }

    private List<string> GetDefaultDataLabels(int numberOfLabels)
    {
        var labels = new List<string>();
        for (var index = 0; index < numberOfLabels; index++)
        {
            labels.Add(GetNextDataLabel());
            dataLabelsCount += 1;
        }

        return labels;
    }

    private string GetNextDataLabel() => $"Product {dataLabelsCount + 1}";

    private string GetNextDataBackgrounfColor() => backgroundColors![dataLabelsCount];

    #endregion  Data Preparation
}
```

[See the demo here.](https://demos.blazorbootstrap.com/charts/doughnut-chart#how-it-works)

### Locale

By default, the chart is using the default locale of the platform on which it is running. 
In the following example, you will see the chart in the **German** locale (**de_DE**).

<img src="https://i.imgur.com/3qndkPO.png" alt="Blazor Bootstrap: Bar Chart Component - Locale" />

```cshtml {} showLineNumbers
@using BlazorBootstrap.Extensions
@using Color = System.Drawing.Color

<LineChart @ref="lineChart" Width="800" Class="mb-4" />
```

```cs {} showLineNumbers
@code {
    private LineChart lineChart = default!;
    private LineChartOptions lineChartOptions = default!;
    private ChartData chartData = default!;

    protected override void OnInitialized()
    {
        var colors = ColorBuilder.CategoricalTwelveColors;

        var labels = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        var datasets = new List<IChartDataset>();

        var dataset1 = new LineChartDataset()
            {
                Label = "Windows",
                Data = new List<double> { 7265791, 5899643, 6317759, 6315641, 5338211, 8496306, 7568556, 8538933, 8274297, 8657298, 7548388, 7764845 },
                BackgroundColor = new List<string> { colors[0] },
                BorderColor = new List<string> { colors[0] },
                BorderWidth = new List<double> { 2 },
                HoverBorderWidth = new List<double> { 4 },
                PointBackgroundColor = new List<string> { colors[0] },
                PointRadius = new List<int> { 0 }, // hide points
                PointHoverRadius = new List<int> { 4 },
            };
        datasets.Add(dataset1);

        var dataset2 = new LineChartDataset()
            {
                Label = "macOS",
                Data = new List<double> { 1809499, 1816642, 2122410, 1809499, 1850793, 1846743, 1954797, 2391313, 1983430, 2469918, 2633303, 2821149 },
                BackgroundColor = new List<string> { colors[1] },
                BorderColor = new List<string> { colors[1] },
                BorderWidth = new List<double> { 2 },
                HoverBorderWidth = new List<double> { 4 },
                PointBackgroundColor = new List<string> { colors[1] },
                PointRadius = new List<int> { 0 }, // hide points
                PointHoverRadius = new List<int> { 4 },
            };
        datasets.Add(dataset2);

        var dataset3 = new LineChartDataset()
            {
                Label = "Other",
                Data = new List<double> { 1081241, 1100363, 1118136, 1073255, 1120315, 1395736, 1488788, 1489466, 1489947, 1414739, 1735811, 1820171 },
                BackgroundColor = new List<string> { colors[2] },
                BorderColor = new List<string> { colors[2] },
                BorderWidth = new List<double> { 2 },
                HoverBorderWidth = new List<double> { 4 },
                PointBackgroundColor = new List<string> { colors[2] },
                PointRadius = new List<int> { 0 }, // hide points
                PointHoverRadius = new List<int> { 4 },
            };
        datasets.Add(dataset3);

        chartData = new ChartData
            {
                Labels = labels,
                Datasets = datasets
            };

        lineChartOptions = new();
        lineChartOptions.Locale = "de-DE";
        lineChartOptions.Responsive = true;
        lineChartOptions.Interaction = new Interaction { Mode = InteractionMode.Index };

        lineChartOptions.Scales.X.Title.Text = "2019";
        lineChartOptions.Scales.X.Title.Display = true;

        lineChartOptions.Scales.Y.Title.Text = "Visitors";
        lineChartOptions.Scales.Y.Title.Display = true;

        lineChartOptions.Plugins.Title.Text = "Operating system";
        lineChartOptions.Plugins.Title.Display = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await lineChart.InitializeAsync(chartData, lineChartOptions);
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
```

[See the demo here.](https://demos.blazorbootstrap.com/charts/doughnut-chart#locale)
