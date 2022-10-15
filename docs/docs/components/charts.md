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

### Bar Chart Parameters

| Name | Type | Default | Required | Descritpion |
|--|--|--|--|--|
| ChartData | ChartData | null | ✔️ | Gets or sets the ChartData. |
| ChartOptions | ChartOptions | null | ✔️ | Gets or sets the ChartOptions. |

#### ChartData Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|
| Labels | `List<string>` | null | ✔️ | Gets or sets the Labels. |
| Datasets | `List<IChartDataset>` | null | ✔️ | Gets or sets the Datasets. |

#### BarChartDataset Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|
| BackgroundColor | | | | Get or sets the BackgroundColor. |
| BarPercentage | double | 0.8 | | Percent (0-1) of the available width each bar should be within the category width. 1.0 will take the whole category width and put the bars right next to each other. |
| BorderColor | | | | Get or sets the BorderColor. |
| BorderRadius | int | 0 | | Border radius. |
| BorderWidth | | | | Get or sets the BorderWidth. |
| CategoryPercentage | double | 0.8 | | Percent (0-1) of the available width each category should be within the sample width. |
| Clip | string | | | How to clip relative to chartArea. Positive value allows overflow, negative value clips that many pixels inside chartArea. 0 = clip at chartArea. Clipping can also be configured per side: clip: {left: 5, top: false, right: -2, bottom: 0} |
| Data | `List<double>` | | | Get or sets the Data. |
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
|--|--|--|--|--|
| IndexAxis | string | x | | The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts. |
| Interaction | Interaction | | | Gets or sets the Interaction. |
| Layout | ChartLayout | | | Gets or sets the ChartLayout. |
| Plugins | Plugins | | | Gets or sets the Plugins. |
| Responsive | bool | false | | Gets or sets the Responsive. |
| Scales | Scales | | | Gets or sets the Scales. |

### Doughnut Chart Parameters

#### DoughnutChartDataset Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|


#### DoughnutChartOptions Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|
| Responsive | bool | false | | Gets or sets the Responsive. |

### Line Chart Parameters

#### LineChartDataset Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|


#### LineChartOptions Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|
| IndexAxis | string | x | | The base axis of the chart. 'x' for vertical charts and 'y' for horizontal charts. |
| Interaction | Interaction | | | Gets or sets the Interaction. |
| Layout | ChartLayout | | | Gets or sets the ChartLayout. |
| Plugins | Plugins | | | Gets or sets the Plugins. |
| Responsive | bool | false | | Gets or sets the Responsive. |
| Scales | Scales | | | Gets or sets the Scales. |

### Pie Chart Parameters

#### PieChartDataset Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|


#### PieChartOptions Members

| Property Name | Type | Default | Required | Description |
|--|--|--|--|--|
| Responsive | bool | false | | Gets or sets the Responsive. |