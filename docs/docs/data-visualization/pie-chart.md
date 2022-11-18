---
title: Blazor Pie Chart Components
description: Blazor Bootstrap charts are well-designed chart components on top of Chart.js to visualize data. It contains a rich UI gallery of charts that cater to all charting scenarios. Its high performance helps render large amounts of data quickly.
image: https://i.imgur.com/ATtFiUZ.png

sidebar_label: Pie Chart
sidebar_position: 4
---

# Blazor Charts

Blazor Bootstrap charts are well-designed chart components on top of Chart.js to visualize data. It contains a rich UI gallery of charts that cater to all charting scenarios. Its high performance helps render large amounts of data quickly.

## Blazor Pie Chart

<img src="https://i.imgur.com/n5TiPtH.png" alt="Blazor Chart Component - Blazor Pie Chart" />
<br />
<a href="https://demos.getblazorbootstrap.com/charts#pie-chart">See blazor pie chart demo here.</a>

## Parameters

| Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Height | int | | | Gets or sets chart height. |
| Width | int | | | Get or sets chart width. |

## Methods

| Name | Descritpion |
|:--|:--|
| InitializeAsync(**ChartData** chartData, **IChartOptions** chartOptions) | Initialize Bar Chart. |
| ResizeAsync(**int** width, **int** height) | Resize the chart. |
| UpdateAsync(**ChartData** chartData, **IChartOptions** chartOptions) | Update Bar Chart. |

## ChartData Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Labels | `List<string>` | null | ✔️ | Gets or sets the Labels. |
| Datasets | `List<IChartDataset>` | null | ✔️ | Gets or sets the Datasets. |

## PieChartDataset Members

:::info
**PieChartDataset** implements **IChartDataset**.
:::

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

## PieChartOptions Members

| Property Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| Responsive | bool | false | | Gets or sets the Responsive. |