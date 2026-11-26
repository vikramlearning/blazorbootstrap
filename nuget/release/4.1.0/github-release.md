Blazor Bootstrap 4.1.0 adds eleven new components, a column chooser and CSV export for the Grid, eleven sample websites in the new Showcase, and a build for .NET 11. It supports .NET 8, 9, 10 and 11, with no breaking change to the public API.

```shell
dotnet add package Blazor.Bootstrap --version 4.1.0
```

A few components render slightly different markup for accessibility. If your CSS, scripts or UI tests target Grid sort headers, MindMap nodes or the PdfViewer menu, read [Upgrade to v4.1.0](https://docs.blazorbootstrap.com/blog/upgrade-to-v4.1.0) first.

## New components

- **Calendar**: Month, Week, Work week, Day and Agenda views, drag to move and resize, cultures and RTL (#1361)
- **Timeline**: vertical, horizontal or alternating events with dot, icon or custom markers (#1360)
- **Mind Map**: a collapsible node-link tree that grows in any direction, with keyboard navigation (#1342)
- **Table**: a read-only Bootstrap 5.3 table from a list, with captions, row headers, colors and footers (#1415)
- **Avatar and AvatarGroup**: images, initials or icons with status, stacked with a "+N" overflow (#1344)
- **Rating Input**: icon ratings with half values, keyboard support and `EditForm` validation (#1340)
- **Navbar**: a responsive header that collapses behind a toggler or into an offcanvas (#1350)
- **List Group**: items, links or buttons with selection, or tabs that switch panes (#1352)
- **Popover**: rich content next to a trigger, with placements, triggers and colors (#1353)
- **Input Group** and **Button Group**: add-ons around inputs, button groups, toolbars and toggle groups (#1348, #1349)

## Enhancements

- **Grid**: column chooser, CSV export, accessible sort buttons with `aria-sort`, and named filter inputs and selection checkboxes (#1403, #1404)
- **.NET 11**: a `net11.0` build next to .NET 8, 9 and 10 (#1407)
- **Services**: new Breadcrumb Service and Preload Service demos (#1395)
- **Showcase**: [11 complete sample websites](https://demos.blazorbootstrap.com/showcase) built only with Blazor Bootstrap components (#1343)

## Bug fixes

- AutoComplete and Grid filters keep fast typing on slow Blazor Server connections (#407, #580, #1247)
- RangeInput and Switch work with `Value` and `ValueChanged` (#932, #1151)
- EnumInput with `Value` and `ValueChanged` inside an `EditForm` (#1300)
- Carousel autoplay with indicators (#1074)
- Tooltip with an empty title (#1100)
- DropdownToggleButton with extra attributes (#1149)
- SortableList placeholders can't be dragged (#1244)
- Grid filter menu shows the selected operator (#1114)
- Grid summary row and detail row alignment (#1199, #1320)
- Grid detail toggle no longer raises `OnRowClick` (#1388)
- Grid fixed header above positioned cell content (#1345)
- Markdown line after a table (#1285)
- Tabs disposed during first render (#1268)
- ConfirmDialog double click (#1264)

Full release notes: https://docs.blazorbootstrap.com/blog/blazorbootstrap-4.1.0

**Full changelog**: https://github.com/vikramlearning/blazorbootstrap/compare/v4.0.0...v4.1.0
