---
title: Blazor Bootstrap v3.0.0
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
tags: [v3.0.0, blazor, bootstrap, blazorbootstrap, charts, grid, barchart, doughnutchart, linechart, modal, pdfviewer, piechart, polarareachart, radarchart, scatterchart, sidebar, sidebar2, pdfviewer]
---

We are excited to release version 3.0.0, which includes new Carousel, Google Map, Image, Polar Area Chart, Radar Chart, Scatter Chart components and other improvements!!!

![image](https://i.imgur.com/AbyDP51.png "Blazor Bootstrap: Radar Chart Component")

<!--truncate-->

## What's new

- `Carousel` component
- `Google Map` component
- `Image Chart` component
- `Polar Area Chart` component
- `Radar Chart` component
- `Scatter Chart` component

## What's changed

-  `Chart` components
  - Tick configuration support added
  - Grid configuration support added
  - Updated Dataset and ChartOption properties
  - Updated documentation and demos

- `Color Utility`
  - Added color utility demos

- `Grid` component
  - Grid filters - Supports DoesNotContain filter.
  - Grid filters - Enum support added.
  - Grid filters - Guid support added.
  - Details View support added.
  - Nested Grid support added.
  - New GridLoadingTemplate and GridEmptyDataTemplate params added.
  - New GridContainerClass and GridContainerStyle parameters added.
  - New FilterButtonColor parameter added to change the filter button color to filter button.
  - New FilterButtonCSSClass parameter added to apply custom CSS classes to filter button.
  - Highlight row when selected and customize the row color and background color.

- `Modal` component
  - Bootstrtap 5.3.3 fixes: Modal close button alignment issue fixed.

- `PdfViewer` component
  - Fixed rendering issue for MAUI Blazor Hybrid Apps

- `Sidebar` and `Sidebar2` components
  - Href, Width and WidthUnit parameters added.
  - Fixed icon alignment issue for non-US cultures

- `Tabs` component
  - `GetActiveTab()` method added

- Demos & Docs updated.

## Breaking changes

- `AccordionItem`
  - **IsActive** parameter renamed to **Active**.

- `Button`
  - **Size**'s parameter data type changed from **Size** to **ButtonSize**.

- `Callout`
  - **Type** parameter changed to **Color**.

- `Dropdown`
  - Set the dropdown color on `Dropdown` component only instead of setting on the `DropdownActionButton` and `DropdownToggleButton`.
  - **Size**'s parameter data type changed from **Size** to **DropdownSize**.

- `DropdownActionButton`
  - **Color** parameter removed.

- `DropdownToggleButton`
  - **Color** parameter removed.

- `DropdownItem`
  - **Type**'s parameter data type changed from **ButtonType** to **DropdownItemType**.

- `RibbonTab`
  - **IsActive** parameter renamed to **Active**.

- `Tab`
  - **IsActive** parameter renamed to **Active**.

## Links
- [Demo Website - Blazor Server](https://demos.blazorbootstrap.com/)
- [Demo Website - Blazor WebAssembly](https://demos.getblazorbootstrap.com/)
