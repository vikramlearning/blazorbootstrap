---
title: Blazor Line Chart
description: A Blazor Bootstrap line chart component is a graphical representation of data that uses a series of connected points to show how the data changes over time. It is a type of x-y chart, where the x-axis represents the independent variable, such as time, and the y-axis represents the dependent variable, such as the value.
image: https://i.imgur.com/MMWdiyi.png

sidebar_label: Line Chart
sidebar_position: 3
---

A Blazor Bootstrap line chart component is a graphical representation of data that uses a series of connected points to show how the data changes over time. It is a type of x-y chart, where the x-axis represents the independent variable, such as time, and the y-axis represents the dependent variable, such as the value.

<img src="https://i.imgur.com/MMWdiyi.png" alt="Blazor Chart Component - Blazor Line Chart" />

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

## LineChartDataset Members

:::info
**LineChartDataset** implements **IChartDataset**.
:::

| Property Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| BackgroundColor | `List<string>` | null | | Get or sets the BackgroundColor. | 1.0.0 |
| BorderColor | `List<string>` | null | | Get or sets the BorderColor. | 1.0.0 |
| BorderDash | `List<int>` | null | | Line dash. | 1.0.0 |
| BorderDashOffset | double | 0.0 | | Line dash offset. | 1.0.0 |
| BorderWidth | `List<double>` | null | | Get or sets the BorderWidth. | 1.0.0 |
| Clip | string | null | | How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside chartArea. 0 = clip at chartArea. Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0} | 1.0.0 |
| Data | `List<double>` | null | | Get or sets the Data. | 1.0.0 |
| Fill | bool | false | | Both line and radar charts support a fill option on the dataset object which can be used to create area between two datasets or a dataset and a boundary, i.e. the scale origin, start or end. | 1.0.0 |
| Hidden | bool | false | | Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart. | 1.0.0 |
| HoverBackgroundColor | `List<string>` | null | ✔️ | Get or sets the HoverBackgroundColor. | 1.0.0 |
| HoverBorderColor | `List<string>` | null | ✔️ | Get or sets the HoverBorderColor. | 1.0.0 |
| HoverBorderDash | `List<int>` | null | | Hover line dash. | 1.0.0 |
| HoverBorderWidth | `List<double>` | null | ✔️ | Get or sets the HoverBorderWidth. | 1.0.0 |
| Label | string | null | | The label for the dataset which appears in the legend and tooltips. | 1.0.0 |
| PointBackgroundColor | `List<string>` | `new List<string> { "rgba(0, 0, 0, 0.1)" }` | | The fill color for points. | 1.0.0 |
| PointBorderColor | `List<string>` | `new List<string> { "rgba(0, 0, 0, 0.1)" }` | | The border color for points. | 1.0.0 |
| PointBorderWidth | `List<double>` | `new List<double> { 1 }` | | The width of the point border in pixels. |  1.0.0 |
| PointHitRadius | `List<double>` | `new List<double> { 1 }` | | The pixel size of the non-displayed point that reacts to mouse events. | 1.0.0 |
| PointHoverBackgroundColor | `List<string>` | null | | Point background color when hovered. | 1.0.0 |
| PointHoverBorderColor | `List<string>` | null | | Point border color when hovered. | 1.0.0 |
| PointHoverBorderWidth | `List<double>` | `new List<double> { 1 }` | | Border width of point when hovered. | 1.0.0 |
| PointHoverRadius | `new List<int>` | `new List<int> { 1 }` | | The radius of the point when hovered. | 1.0.0 |
| PointRadius | `List<int>` | `new List<int> { 1 }` | | The radius of the point shape. If set to 0, the point is not rendered. | 1.0.0 |
| PointRotation | `List<int>` | `new List<int> { 0 }` | | The rotation of the point in degrees. | 1.0.0 |
| PointStyle | `List<string>` | `new List<string> { "circle" }` | | Style of the point. Use 'circle', 'cross', 'crossRot', 'dash', 'line', 'rect', 'rectRounded', 'rectRot', 'star', and 'triangle' to style the point. | 1.0.0 |
| ShowLine | bool | true | | If false, the lines between points are not drawn. | 1.0.0 |
| SpanGaps | bool | false | | If true, lines will be drawn between points with no or null data. If false, points with null data will create a break in the line. Can also be a number specifying the maximum gap length to span. The unit of the value depends on the scale used. | 1.0.0 |
| Stepped | bool | false | | true to show the line as a stepped line (tension will be ignored). | 1.0.0 |
| Tension | double | 0.2 | | Bezier curve tension of the line. Set to 0 to draw straightlines. This option is ignored if monotone cubic interpolation is used. | 1.0.0 |
| Type | string | null | ✔️ | Get or sets the chart type. | 1.0.0 |
| XAxisID | string | null | | The ID of the x axis to plot this dataset on. | 1.0.0 |
| YAxisID | string | null | | The ID of the y axis to plot this dataset on. | 1.0.0 |

## LineChartOptions Members

| Property Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| IndexAxis | string | x | | The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts. | 1.0.0 |
| Interaction | Interaction | | | Gets or sets the Interaction. | 1.0.0 |
| Layout | ChartLayout | | | Gets or sets the ChartLayout. | 1.0.0 |
| Plugins | Plugins | | | Gets or sets the Plugins. | 1.0.0 |
| Responsive | bool | false | | Gets or sets the Responsive. | 1.0.0 |
| Scales | Scales | | | Gets or sets the Scales. | 1.0.0 |

## Examples

### How it works

In the following example, a categorical 12-color palette is used.

:::tip
For data visualization, you can use the predefined palettes `ColorBuilder.CategoricalTwelveColors` for a 12-color palette and `ColorBuilder.CategoricalSixColors` for a 6-color palette. 
These palettes offer a range of distinct and visually appealing colors that can be applied to represent different categories or data elements in your visualizations.
:::

<img src="https://i.imgur.com/MMWdiyi.png" alt="Blazor Bootstrap: Line Chart Component - How it works" />

```cshtml {} showLineNumbers
@using BlazorBootstrap.Extensions
@using Color = System.Drawing.Color

<LineChart @ref="lineChart" Width="800" Class="mb-4" />

<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await RandomizeAsync()"> Randomize </Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await AddDatasetAsync()"> Add Dataset </Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await AddDataAsync()"> Add Data </Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await ShowHorizontalLineChartAsync()"> Horizontal Line Chart </Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="async () => await ShowVerticalLineChartAsync()"> Vertical Line Chart </Button>
```

```cs {} showLineNumbers
@code {
    private LineChart lineChart = default!;
    private LineChartOptions lineChartOptions = default!;
    private ChartData chartData = default!;

    private int datasetsCount = 0;
    private int labelsCount = 0;

    private Random random = new();

    protected override void OnInitialized()
    {
        chartData = new ChartData { Labels = GetDefaultDataLabels(6), Datasets = GetDefaultDataSets(3) };
        lineChartOptions = new() { Responsive = true, Interaction = new Interaction { Mode = InteractionMode.Index } };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await lineChart.InitializeAsync(chartData, lineChartOptions);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task RandomizeAsync()
    {
        if (chartData is null || chartData.Datasets is null || !chartData.Datasets.Any()) return;

        var newDatasets = new List<IChartDataset>();

        foreach (var dataset in chartData.Datasets)
        {
            if (dataset is LineChartDataset lineChartDataset
                && lineChartDataset is not null
                && lineChartDataset.Data is not null)
            {
                var count = lineChartDataset.Data.Count;

                var newData = new List<double>();
                for (var i = 0; i < count; i++)
                {
                    newData.Add(random.Next(200));
                }

                lineChartDataset.Data = newData;
                newDatasets.Add(lineChartDataset);
            }
        }

        chartData.Datasets = newDatasets;

        await lineChart.UpdateAsync(chartData, lineChartOptions);
    }

    private async Task AddDatasetAsync()
    {
        if (chartData is null || chartData.Datasets is null) return;

        var chartDataset = GetRandomLineChartDataset();
        chartData = await lineChart.AddDatasetAsync(chartData, chartDataset, lineChartOptions);
    }

    private async Task AddDataAsync()
    {
        if (chartData is null || chartData.Datasets is null)
            return;

        var data = new List<IChartDatasetData>();
        foreach (var dataset in chartData.Datasets)
        {
            if (dataset is LineChartDataset lineChartDataset)
                data.Add(new LineChartDatasetData(lineChartDataset.Label, random.Next(200)));
        }

        chartData = await lineChart.AddDataAsync(chartData, GetNextDataLabel(), data);
    }

    private async Task ShowHorizontalLineChartAsync()
    {
        lineChartOptions.IndexAxis = "y";
        await lineChart.UpdateAsync(chartData, lineChartOptions);
    }

    private async Task ShowVerticalLineChartAsync()
    {
        lineChartOptions.IndexAxis = "x";
        await lineChart.UpdateAsync(chartData, lineChartOptions);
    }

    #region Data Preparation

    private List<IChartDataset> GetDefaultDataSets(int numberOfDatasets)
    {
        var datasets = new List<IChartDataset>();

        for (var index = 0; index < numberOfDatasets; index++)
        {
            datasets.Add(GetRandomLineChartDataset());
        }

        return datasets;
    }

    private LineChartDataset GetRandomLineChartDataset()
    {
        var c = ColorBuilder.CategoricalTwelveColors[datasetsCount].ToColor();

        datasetsCount += 1;

        return new LineChartDataset()
            {
                Label = $"Team {datasetsCount}",
                Data = GetRandomData(),
                BackgroundColor = new List<string> { c.ToRgbString() },
                BorderColor = new List<string> { c.ToRgbString() },
                BorderWidth = new List<double> { 2 },
                HoverBorderWidth = new List<double> { 4 },
                PointBackgroundColor = new List<string> { c.ToRgbString() },
                PointRadius = new List<int> { 0 }, // hide points
                PointHoverRadius = new List<int> { 4 },
            };
    }

    private List<double> GetRandomData()
    {
        var data = new List<double>();
        for (var index = 0; index < labelsCount; index++)
        {
            data.Add(random.Next(200));
        }

        return data;
    }

    private List<string> GetDefaultDataLabels(int numberOfLabels)
    {
        var labels = new List<string>();
        for (var index = 0; index < numberOfLabels; index++)
        {
            labels.Add(GetNextDataLabel());
        }

        return labels;
    }

    private string GetNextDataLabel()
    {
        labelsCount += 1;
        return $"Day {labelsCount}";
    }

    #endregion Data Preparation
}
```

### Another example

<img src="https://i.imgur.com/qetH0UX.png" alt="Blazor Bootstrap: Line Chart Component - Another example" />

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

[See the demo here.](https://demos.blazorbootstrap.com/charts/line-chart#how-it-works)
