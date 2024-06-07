---
title: Blazor Bootstrap v3.0.0
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
tags: [v3.0.0, blazor, bootstrap, blazorbootstrap, accordion, button, callout, dropdown, grid, modal, ribbon, tab, sidebar, sidebar2]
---

We are excited to release version 3.0.0, which includes a Grid, Modal, Sidebar, Sidebar2 updates, and other improvements!!!

![image](https://i.imgur.com/XG4Wv17.png "Blazor Bootstrap: Grid Component - Column class")

<!--truncate-->

## What's changed

- `Grid` component
  - New parameter **FilterButtonColor** added to change the filter button color.
  - New parameter **FilterButtonCSSClass** added to apply custom CSS classes.
  - Grid filters - Supports `DoesNotContain` filter.
  - Grid filters - **Enum** support added.
  - Grid filters - **Guid** support added.

- `Modal` component
  - Bootstrtap 5.3.3 fixes: Modal close button alignment issue fixed. 

- `Sidebar` component
  - **Href** parameter added.

- `Sidebar2` component
  - **Href** parameter added.

- BlazorBootstrtap component library targets .NET 6 and .NET 8 frameworks.

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
