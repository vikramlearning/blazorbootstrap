---
title: Blazor Bootstrap v4.1.0
slug: blazorbootstrap-4.1.0
description: "Release notes for Blazor Bootstrap 4.1.0: Calendar, Timeline, Mind Map, Table and seven more new components, Grid column chooser and CSV export, the Showcase of 11 sample websites, and .NET 11 support."
draft: true
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
tags: [v4.1.0, blazor, bootstrap, blazorbootstrap, calendar, timeline, mindmap, table, navbar, listgroup, popover, avatar, ratinginput, grid, showcase, dotnet11]
---

<!-- Draft for the 4.1.0 release: set the file date to the release date and remove draft: true when publishing. The slug keeps the URL (/blog/blazorbootstrap-4.1.0) independent of that date, because the release kit's social posts link to it. -->

**Blazor Bootstrap v4.1.0** adds eleven new components, a column chooser and CSV export for the Grid, eleven complete sample websites in the new Showcase, and a build for **.NET 11**. It targets **.NET 8**, **.NET 9**, **.NET 10** and **.NET 11**, and makes no breaking change to the public API: every new parameter is optional and off by default.

<!--truncate-->

## 🔧 Upgrade notes

There is no breaking API change, but a few components render slightly different markup for accessibility (Grid sortable headers, MindMap tree items, the PdfViewer menu button). If your CSS, scripts or UI tests target those elements, read [Upgrade to v4.1.0](https://docs.blazorbootstrap.com/blog/upgrade-to-v4.1.0) first.

```shell
dotnet add package Blazor.Bootstrap --version 4.1.0
```

---

## .NET 11

The package now includes a `net11.0` build alongside its .NET 8, .NET 9 and .NET 10 builds, so an app on any of these versions installs the same package. The getting-started guides cover .NET 11 and .NET 10.

- PR [#1407](https://github.com/vikramlearning/blazorbootstrap/pull/1407)
- Docs: [Getting started](https://docs.blazorbootstrap.com/getting-started/blazor-webassembly-net-11)

---

## ✨ New components

### Calendar
An event calendar with Month, Week, Work week, Day and Agenda views, drag to move and resize events, cultures and right-to-left layout.

- PR [#1361](https://github.com/vikramlearning/blazorbootstrap/pull/1361)
- Docs: [Calendar](https://docs.blazorbootstrap.com/components/calendar) | Demo: [Calendar](https://demos.blazorbootstrap.com/demos/calendar)

### Timeline
Shows events in order: vertical, horizontal or alternating, with dot, icon or custom markers, declared in markup or bound to data.

- PR [#1360](https://github.com/vikramlearning/blazorbootstrap/pull/1360)
- Docs: [Timeline](https://docs.blazorbootstrap.com/components/timeline) | Demo: [Timeline](https://demos.blazorbootstrap.com/demos/timeline)

### Mind Map
Shows hierarchical data as a collapsible node-link tree that grows right, left, to both sides, down or up, with full keyboard navigation.

- PR [#1342](https://github.com/vikramlearning/blazorbootstrap/pull/1342)
- Docs: [Mind Map](https://docs.blazorbootstrap.com/components/mind-map) | Demo: [Mind Map](https://demos.blazorbootstrap.com/demos/mind-map)

### Table
Renders a read-only Bootstrap 5.3 table from a list, with declared columns, captions, row headers, row and cell colors, footers and responsive scrolling. Use it when you need a styled table without the Grid's paging, sorting and filtering.

- PR [#1415](https://github.com/vikramlearning/blazorbootstrap/pull/1415)
- Docs: [Table](https://docs.blazorbootstrap.com/components/table) | Demo: [Table](https://demos.blazorbootstrap.com/demos/table)

### Avatar and AvatarGroup
`Avatar` shows a person as an image, initials or icon with an optional status. `AvatarGroup` stacks avatars with a "+N" overflow.

- PR [#1344](https://github.com/vikramlearning/blazorbootstrap/pull/1344)
- Docs: [Avatar](https://docs.blazorbootstrap.com/components/avatar) | Demo: [Avatar](https://demos.blazorbootstrap.com/demos/avatar)

### Rating Input
Lets users select or display a score with icons, with half values, keyboard support and `EditForm` validation.

- PR [#1340](https://github.com/vikramlearning/blazorbootstrap/pull/1340)
- Docs: [Rating Input](https://docs.blazorbootstrap.com/forms/rating-input) | Demo: [Rating Input](https://demos.blazorbootstrap.com/demos/form/rating-input)

### Navbar
A responsive header with brand, links, dropdowns and forms that collapses behind a toggler or into an offcanvas.

- PR [#1350](https://github.com/vikramlearning/blazorbootstrap/pull/1350)
- Docs: [Navbar](https://docs.blazorbootstrap.com/components/navbar) | Demo: [Navbar](https://demos.blazorbootstrap.com/demos/navbar)

### List Group
Shows items, links or buttons with single, multiple, checkbox or radio selection, or as tabs that switch content panes.

- PR [#1352](https://github.com/vikramlearning/blazorbootstrap/pull/1352)
- Docs: [List Group](https://docs.blazorbootstrap.com/components/list-group) | Demo: [List Group](https://demos.blazorbootstrap.com/demos/list-group)

### Popover
Shows a title and rich content next to a trigger, with placements, triggers, colors, and content that updates while the popover is open.

- PR [#1353](https://github.com/vikramlearning/blazorbootstrap/pull/1353)
- Docs: [Popover](https://docs.blazorbootstrap.com/components/popover) | Demo: [Popover](https://demos.blazorbootstrap.com/demos/popover)

### Input Group
Adds text, icon, checkbox, radio, button and dropdown add-ons on either side of form inputs, as in Bootstrap 5.3.

- PR [#1348](https://github.com/vikramlearning/blazorbootstrap/pull/1348), [#1349](https://github.com/vikramlearning/blazorbootstrap/pull/1349)
- Docs: [Input Group](https://docs.blazorbootstrap.com/components/input-group) | Demo: [Input Group](https://demos.blazorbootstrap.com/demos/form/input-group)

### Button Group
Joins buttons into groups and toolbars, and turns toggle buttons into radio and checkbox inputs.

- PR [#1349](https://github.com/vikramlearning/blazorbootstrap/pull/1349)
- Docs: [Button Group](https://docs.blazorbootstrap.com/components/button-group) | Demo: [Button Group](https://demos.blazorbootstrap.com/demos/button-group)

---

## 🚀 Enhancements

### Grid
- **Column chooser**: users show, hide and reorder columns from a searchable menu, with a reset to the default layout. The choice is part of the saved `GridSettings` (PR [#1403](https://github.com/vikramlearning/blazorbootstrap/pull/1403)).
- **CSV export**: export the grid's rows to a CSV file (PR [#1403](https://github.com/vikramlearning/blazorbootstrap/pull/1403)).
- **Accessible sorting**: sortable headers hold a real button that sorts with a click, Enter or Space, and announce the sort direction with `aria-sort`. Filter inputs and selection checkboxes now have accessible names (PR [#1404](https://github.com/vikramlearning/blazorbootstrap/pull/1404)).
- **New localizable text** for every new Grid label and message; see the table in [Upgrade to v4.1.0](https://docs.blazorbootstrap.com/blog/upgrade-to-v4.1.0#new-localizable-text).
- Demos: [Column chooser](https://demos.blazorbootstrap.com/demos/grid/column-chooser) | [Export](https://demos.blazorbootstrap.com/demos/grid/export)

### Services
- New demo pages for **Breadcrumb Service** and **Preload Service**, next to the Modal and Toast service pages (PR [#1395](https://github.com/vikramlearning/blazorbootstrap/pull/1395)).

### Accessibility
- Pagination controls have clearer names and semantics (PR [#1330](https://github.com/vikramlearning/blazorbootstrap/pull/1330)).
- MindMap cards are the keyboard tree items, and the PdfViewer menu toggle is a named button (see the upgrade guide).
- The demo site meets WCAG 2.2 AA in more places: named navbar icon links, higher-contrast code snippets, one solid focus outline, and keyboard-operable copy buttons (PR [#1396](https://github.com/vikramlearning/blazorbootstrap/pull/1396), [#1397](https://github.com/vikramlearning/blazorbootstrap/pull/1397)).

---

## 🖥️ Showcase: 11 sample websites

The new [Showcase](https://demos.blazorbootstrap.com/showcase) has eleven complete, interactive websites built only with Blazor Bootstrap components, each with demo accounts for its roles, fictitious data, and light and dark themes: Help Desk & Ticketing, HR & Employee Portal, IoT & DevOps Monitoring, SaaS Product, Commerce Back Office, Banking Operations, Online Learning, Healthcare Clinic Portal, Travel & Stay Booking, Project & Client Board, and AI Assistant Workspace.

- Issue [#1343](https://github.com/vikramlearning/blazorbootstrap/issues/1343) and its follow-up PRs
- Docs: [Showcase](https://docs.blazorbootstrap.com/showcase)

---

## 🐛 Bug fixes

- **AutoComplete and Grid filters** no longer lose typed text when you type fast on a slow Blazor Server connection ([#407](https://github.com/vikramlearning/blazorbootstrap/issues/407), [#580](https://github.com/vikramlearning/blazorbootstrap/issues/580), [#1247](https://github.com/vikramlearning/blazorbootstrap/issues/1247)).
- **RangeInput and Switch** no longer throw when used with `Value` and `ValueChanged` instead of `@bind-Value`, and Switch can show read-only properties ([#932](https://github.com/vikramlearning/blazorbootstrap/issues/932), [#1151](https://github.com/vikramlearning/blazorbootstrap/issues/1151)).
- **EnumInput** with `Value` and `ValueChanged` inside an `EditForm` no longer crashes the circuit ([#1300](https://github.com/vikramlearning/blazorbootstrap/issues/1300)).
- **Carousel** with indicators and autoplay keeps cycling, and only the active indicator has `aria-current` ([#1074](https://github.com/vikramlearning/blazorbootstrap/issues/1074)).
- **Tooltip** shows nothing when its title becomes empty, instead of the old text ([#1100](https://github.com/vikramlearning/blazorbootstrap/issues/1100)).
- **Dropdown**: a `DropdownToggleButton` with extra attributes such as `Color` keeps opening after the parent re-renders ([#1149](https://github.com/vikramlearning/blazorbootstrap/issues/1149)).
- **SortableList** loading and empty placeholders can no longer be dragged ([#1244](https://github.com/vikramlearning/blazorbootstrap/issues/1244)).
- **Grid** filter menu highlights the selected operator ([#1114](https://github.com/vikramlearning/blazorbootstrap/issues/1114)).
- **Grid** summary row lines up with its columns when detail view is on ([#1199](https://github.com/vikramlearning/blazorbootstrap/issues/1199)), and the expanded detail row spans exactly the table's columns when selection is also on (PR [#1320](https://github.com/vikramlearning/blazorbootstrap/pull/1320)).
- **Grid** detail view toggle no longer raises `OnRowClick` ([#1388](https://github.com/vikramlearning/blazorbootstrap/issues/1388)).
- **Grid** fixed header stays above positioned content in body cells (PR [#1345](https://github.com/vikramlearning/blazorbootstrap/pull/1345)).
- **Markdown** renders the line right after a table ([#1285](https://github.com/vikramlearning/blazorbootstrap/issues/1285)).
- **Tabs** disposed during first render no longer throw `NullReferenceException` ([#1268](https://github.com/vikramlearning/blazorbootstrap/issues/1268)).
- **ConfirmDialog** resolves once when Yes, No or Close is double-clicked, with no exception ([#1264](https://github.com/vikramlearning/blazorbootstrap/issues/1264)).

---

## 📚 Docs and demos

- The docs site moved to Docusaurus 3.
- Refreshed artwork, screenshots and README.
- A new demo home page with a searchable list of all 76 components and setup guides for each project type.

---

## Links
- [Docs Website - Blazor Bootstrap](https://docs.blazorbootstrap.com/)
- [Demos Website - Blazor Bootstrap](https://demos.blazorbootstrap.com/)
- [NuGet Package - Blazor.Bootstrap 4.1.0](https://www.nuget.org/packages/Blazor.Bootstrap/4.1.0)
- [GitHub Release](https://github.com/vikramlearning/blazorbootstrap/releases/tag/v4.1.0)
