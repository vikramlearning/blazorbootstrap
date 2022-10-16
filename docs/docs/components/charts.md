---
title: Blazor Chart Components
description: BlazorBootstrap's charts are well-designed chart components on top of Chart.js to visualize data. It contains a rich UI gallery of charts that cater to all charting scenarios. Its high performance helps render large amounts of data quickly.
image: https://i.imgur.com/ATtFiUZ.png

sidebar_label: Charts
sidebar_position: 6
---

# Blazor Charts

BlazorBootstrap's charts are well-designed chart components on top of Chart.js to visualize data. It contains a rich UI gallery of charts that cater to all charting scenarios. Its high performance helps render large amounts of data quickly.

## Bar Chart

<img src="https://i.imgur.com/ATtFiUZ.png" alt="Blazor Chart Component - Blazor Bar Chart" />
<br />
<a href="https://demos.getblazorbootstrap.com/charts#bar-chart">See blazor bar chart demo here.</a>

## Doughnut Chart

<img src="https://i.imgur.com/HV3pxA3.png" alt="Blazor Chart Component - Blazor Doughnut Chart" />
<br />
<a href="https://demos.getblazorbootstrap.com/charts#doughnut-chart">See blazor doughnut chart demo here.</a>

## Line Chart

<img src="https://i.imgur.com/NjrT5D7.png" alt="Blazor Chart Component - Blazor Line Chart" />
<br />
<a href="https://demos.getblazorbootstrap.com/charts#line-chart">See blazor line chart demo here.</a>

## Pie Chart

<img src="https://i.imgur.com/n5TiPtH.png" alt="Blazor Chart Component - Blazor Pie Chart" />
<br />
<a href="https://demos.getblazorbootstrap.com/charts#pie-chart">See blazor pie chart demo here.</a>

## API

### Bar Chart Parameters

| Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Height | int | | | Gets or sets chart height. |
| Width | int | | | Get or sets chart width. |

### Bar Chart Methods

| Name | Descritpion |
|:--|:--|
| InitializeAsync(**ChartData** chartData, **IChartOptions** chartOptions) | Initialize Bar Chart. |
| ResizeAsync(**int** width, **int** height) | Resize the chart. |
| UpdateAsync(**ChartData** chartData, **IChartOptions** chartOptions) | Update Bar Chart. |

#### ChartData Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Labels | `List<string>` | null | ✔️ | Gets or sets the Labels. |
| Datasets | `List<IChartDataset>` | null | ✔️ | Gets or sets the Datasets. |

#### BarChartDataset Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| BackgroundColor | `List<string>` | null | | Get or sets the BackgroundColor. |
| BarPercentage | double | 0.8 | | Percent (0-1) of the available width each bar should be within the category width. 1.0 will take the whole category width and put the bars right next to each other. |
| BorderColor | `List<string>` | null | | Get or sets the BorderColor. |
| BorderRadius | int | 0 | | Border radius. |
| BorderWidth | `List<double>` | null | | Get or sets the BorderWidth. |
| CategoryPercentage | double | 0.8 | | Percent (0-1) of the available width each category should be within the sample width. |
| Clip | string | null | | How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside chartArea. 0 = clip at chartArea. Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0} |
| Data | `List<double>` | null | | Get or sets the Data. |
| Hidden | bool | false | | Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart. |
| HoverBackgroundColor | `List<string>` | null | ✔️ | Get or sets the HoverBackgroundColor. |
| HoverBorderColor | `List<string>` | null | ✔️ | Get or sets the HoverBorderColor. |
| HoverBorderWidth | `List<double>` | null | ✔️ | Get or sets the HoverBorderWidth. |
| Label | string | null | | The label for the dataset which appears in the legend and tooltips. |
| Type | string | null | ✔️ | Get or sets the chart type. |
| XAxisID | string | null | | The ID of the x axis to plot this dataset on. |
| YAxisID | string | null | | The ID of the y axis to plot this dataset on. |

#### BarChartOptions Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| IndexAxis | string | x | | The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts. |
| Interaction | Interaction | | | Gets or sets the Interaction. |
| Layout | ChartLayout | | | Gets or sets the ChartLayout. |
| Plugins | Plugins | | | Gets or sets the Plugins. |
| Responsive | bool | false | | Gets or sets the Responsive. |
| Scales | Scales | | | Gets or sets the Scales. |

### Doughnut Chart Parameters

| Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Height | int | | | Gets or sets chart height. |
| Width | int | | | Get or sets chart width. |

#### DoughnutChartDataset Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| BackgroundColor | `List<string>` | null | | Get or sets the BackgroundColor. |
| BorderColor | `List<string>` | null | | Get or sets the BorderColor. |
| BorderWidth | `List<double>` | null | | Get or sets the BorderWidth. |
| Clip | string | null | | How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside chartArea. 0 = clip at chartArea. Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0} |
| Data | `List<double>` | null | | Get or sets the Data. |
| Hidden | bool | false | | Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart. |
| HoverBackgroundColor | `List<string>` | null | ✔️ | Get or sets the HoverBackgroundColor. |
| HoverBorderColor | `List<string>` | null | ✔️ | Get or sets the HoverBorderColor. |
| HoverBorderWidth | `List<double>` | null | ✔️ | Get or sets the HoverBorderWidth. |
| Type | string | null | ✔️ | Get or sets the chart type. |

#### DoughnutChartOptions Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Responsive | bool | false | | Gets or sets the Responsive. |

### Line Chart Parameters

| Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Height | int | | | Gets or sets chart height. |
| Width | int | | | Get or sets chart width. |

#### LineChartDataset Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| BackgroundColor | `List<string>` | null | | Get or sets the BackgroundColor. |
| BorderColor | `List<string>` | null | | Get or sets the BorderColor. |
| BorderDash | `List<int>` | null | | Line dash. |
| BorderDashOffset | double | 0.0 | | Line dash offset. |
| BorderWidth | `List<double>` | null | | Get or sets the BorderWidth. |
| Clip | string | null | | How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside chartArea. 0 = clip at chartArea. Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0} |
| Data | `List<double>` | null | | Get or sets the Data. |
| Fill | bool | false | | Both line and radar charts support a fill option on the dataset object which can be used to create area between two datasets or a dataset and a boundary, i.e. the scale origin, start or end. |
| Hidden | bool | false | | Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart. |
| HoverBackgroundColor | `List<string>` | null | ✔️ | Get or sets the HoverBackgroundColor. |
| HoverBorderColor | `List<string>` | null | ✔️ | Get or sets the HoverBorderColor. |
| HoverBorderDash | `List<int>` | null | | Hover line dash. |
| HoverBorderWidth | `List<double>` | null | ✔️ | Get or sets the HoverBorderWidth. |
| Label | string | null | | The label for the dataset which appears in the legend and tooltips. |
| PointBackgroundColor | `List<string>` | `new List<string> { "rgba(0, 0, 0, 0.1)" }` | | The fill color for points. |
| PointBorderColor | `List<string>` | `new List<string> { "rgba(0, 0, 0, 0.1)" }` | | The border color for points. |
| PointBorderWidth | `List<double>` | `new List<double> { 1 }` | | The width of the point border in pixels. | 
| PointHitRadius | `List<double>` | `new List<double> { 1 }` | | The pixel size of the non-displayed point that reacts to mouse events. |
| PointHoverBackgroundColor | `List<string>` | null | | Point background color when hovered. |
| PointHoverBorderColor | `List<string>` | null | | Point border color when hovered. |
| PointHoverBorderWidth | `List<double>` | `new List<double> { 1 }` | | Border width of point when hovered. |
| PointHoverRadius | `new List<int>` | `new List<int> { 1 }` | | The radius of the point when hovered. |
| PointRadius | `List<int>` | `new List<int> { 1 }` | | The radius of the point shape. If set to 0, the point is not rendered. |
| PointRotation | `List<int>` | `new List<int> { 0 }` | | The rotation of the point in degrees. |
| PointStyle | `List<string>` | `new List<string> { "circle" }` | | Style of the point. Use 'circle', 'cross', 'crossRot', 'dash', 'line', 'rect', 'rectRounded', 'rectRot', 'star', and 'triangle' to style the point. |
| ShowLine | bool | true | | If false, the lines between points are not drawn. |
| SpanGaps | bool | false | | If true, lines will be drawn between points with no or null data. If false, points with null data will create a break in the line. Can also be a number specifying the maximum gap length to span. The unit of the value depends on the scale used. |
| Stepped | bool | false | | true to show the line as a stepped line (tension will be ignored). |
| Tension | double | 0.2 | | Bezier curve tension of the line. Set to 0 to draw straightlines. This option is ignored if monotone cubic interpolation is used. |
| Type | string | null | ✔️ | Get or sets the chart type. |
| XAxisID | string | null | | The ID of the x axis to plot this dataset on. |
| YAxisID | string | null | | The ID of the y axis to plot this dataset on. |

#### LineChartOptions Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| IndexAxis | string | x | | The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts. |
| Interaction | Interaction | | | Gets or sets the Interaction. |
| Layout | ChartLayout | | | Gets or sets the ChartLayout. |
| Plugins | Plugins | | | Gets or sets the Plugins. |
| Responsive | bool | false | | Gets or sets the Responsive. |
| Scales | Scales | | | Gets or sets the Scales. |

### Pie Chart Parameters

| Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Height | int | | | Gets or sets chart height. |
| Width | int | | | Get or sets chart width. |

#### PieChartDataset Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| BackgroundColor | `List<string>` | null | | Get or sets the BackgroundColor. |
| BorderColor | `List<string>` | null | | Get or sets the BorderColor. |
| BorderWidth | `List<double>` | null | | Get or sets the BorderWidth. |
| Clip | string | null | | How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside chartArea. 0 = clip at chartArea. Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0} |
| Data | `List<double>` | null | | Get or sets the Data. |
| Hidden | bool | false | | Configures the visibility state of the dataset. Set it to true, to hide the dataset from the chart. |
| HoverBackgroundColor | `List<string>` | null | ✔️ | Get or sets the HoverBackgroundColor. |
| HoverBorderColor | `List<string>` | null | ✔️ | Get or sets the HoverBorderColor. |
| HoverBorderWidth | `List<double>` | null | ✔️ | Get or sets the HoverBorderWidth. |
| Type | string | null | ✔️ | Get or sets the chart type. |

#### PieChartOptions Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Responsive | bool | false | | Gets or sets the Responsive. |