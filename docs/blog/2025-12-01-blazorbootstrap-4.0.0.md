---
title: Blazor Bootstrap v4.0.0
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
tags: [v4.0.0, blazor, bootstrap, blazorbootstrap, richtexteditor, fileinput, splitview, otpinput, charts, pdfviewer, grid, toasts]
---

We are thrilled to release **Blazor Bootstrap v4.0.0** — a major milestone that introduces powerful new components, significant enhancements, and important breaking changes. This release targets **.NET 8**, **.NET 9**, and **.NET 10**.

<!--truncate-->

## ⚠️ Breaking Changes

### .NET 6.0 support dropped
.NET 6 has reached end-of-life. Blazor Bootstrap 4.0.0 supports **.NET 8**, **.NET 9**, and **.NET 10** only. Please upgrade your application before updating to this version.

### Charts moved to BlazorExpress.ChartJS
All chart components (`BarChart`, `LineChart`, `DoughnutChart`, `PieChart`, `PolarAreaChart`, `RadarChart`, `ScatterChart`) have been extracted from BlazorBootstrap and are now provided by the dedicated [BlazorExpress.ChartJS](https://chartjs.blazorexpress.com/) package.

**Migration steps:**
1. Install the `BlazorExpress.ChartJS` NuGet package.
2. Replace `@using BlazorBootstrap` chart references with the BlazorExpress.ChartJS namespace.
3. Load the BlazorExpress.ChartJS browser scripts.
4. Migrate any chart API calls to the BlazorExpress.ChartJS API.

See the [migration guide](https://docs.blazorbootstrap.com/components/charts) and the [BlazorExpress.ChartJS GitHub repo](https://github.com/BlazorExpress/BlazorExpress.ChartJS) for full details.

---

## ✨ New Components

### RichTextEditor
A full-featured rich text editor component built on Quill.js, supporting text formatting, bullet/numbered lists, links, and image uploads.

- PR [#1302](https://github.com/vikramlearning/blazorbootstrap/pull/1302), [#1304](https://github.com/vikramlearning/blazorbootstrap/pull/1304), [#1305](https://github.com/vikramlearning/blazorbootstrap/pull/1305), [#1308](https://github.com/vikramlearning/blazorbootstrap/pull/1308)
- Docs: [Rich Text Editor](https://docs.blazorbootstrap.com/forms/rich-text-editor)

### FileInput & DragAndDropFileInput
Two new file selection components for modern file upload experiences:
- `FileInput` — standard file picker with multi-select support.
- `DragAndDropFileInput` — drop-zone style file picker with drag-and-drop support.

- PR [#1293](https://github.com/vikramlearning/blazorbootstrap/pull/1293)
- Docs: [File Input](https://docs.blazorbootstrap.com/forms/file-input) | [Drag & Drop File Input](https://docs.blazorbootstrap.com/forms/drag-and-drop-file-input)

### SplitView
A resizable split-panel layout component that lets users drag a divider to resize two side-by-side or top-and-bottom panes.

- PR [#1283](https://github.com/vikramlearning/blazorbootstrap/pull/1283)
- Docs: [SplitView](https://docs.blazorbootstrap.com/components/split-view)

### OTP Input
A one-time password (OTP) input component providing individual digit boxes and auto-advance behavior — ideal for verification code flows.

- PR [#1238](https://github.com/vikramlearning/blazorbootstrap/pull/1238)
- Docs: [OTP Input](https://docs.blazorbootstrap.com/forms/otp-input)

---

## 🚀 Enhancements

### Charts — Data Point Click Event
Chart components (via BlazorExpress.ChartJS) now support a data point click event, enabling interactive drill-down scenarios.

- PR [#1276](https://github.com/vikramlearning/blazorbootstrap/pull/1276)

### PdfViewer
- **Download support**: Users can now download the currently viewed PDF directly from the viewer (PR [#1288](https://github.com/vikramlearning/blazorbootstrap/pull/1288), [#1277](https://github.com/vikramlearning/blazorbootstrap/pull/1277)).
- **Manual Zoom**: Manual zoom control added (PR [#956](https://github.com/vikramlearning/blazorbootstrap/pull/956)).
- **Password prompt improvements**: Improved handling of cancellation in the password prompt (PR [#1255](https://github.com/vikramlearning/blazorbootstrap/pull/1255)).

### ToastService
- Async overloads added for `ShowAsync` to simplify fire-and-forget toast usage (PR [#1231](https://github.com/vikramlearning/blazorbootstrap/pull/1231)).
- New constructor `ToastMessage(ToastType)` for quick toast creation (PR [#993](https://github.com/vikramlearning/blazorbootstrap/pull/993)).

### ConfirmDialog
- Auto-focus on the **Yes** button now handled natively in Blazor for improved accessibility (PR [#1259](https://github.com/vikramlearning/blazorbootstrap/pull/1259), [#1260](https://github.com/vikramlearning/blazorbootstrap/pull/1260), [#1261](https://github.com/vikramlearning/blazorbootstrap/pull/1261)).

### Dropdown
- Dropdown now closes when clicking outside the component (PR [#1251](https://github.com/vikramlearning/blazorbootstrap/pull/1251)).

### EnumInput
- Enum values can now display localized or custom display names using resource-backed attributes, reducing boilerplate in forms.

### Layout — Sticky Header
- Added a CSS custom property (`--bb-sticky-header-z-index`) to allow applications to control the sticky header z-index without overriding internal styles (PR [#1311](https://github.com/vikramlearning/blazorbootstrap/pull/1311)).

### Google Map
- Enhanced demo, script loading, and marker customization options (PR [#1292](https://github.com/vikramlearning/blazorbootstrap/pull/1292)).

### CurrencyInput
- Copy/Paste `KeyDown` handling improved (PR [#1205](https://github.com/vikramlearning/blazorbootstrap/pull/1205)).

### Grid
- Fixed rendering issues, page size reset bug (PR [#1203](https://github.com/vikramlearning/blazorbootstrap/pull/1203)).

### Sidebar / Sidebar2
- Improved behavior on mobile devices (PR [#1203](https://github.com/vikramlearning/blazorbootstrap/pull/1203)).

### SortableList
- Refined grab cursor for sortable list items and handles (PR [#1253](https://github.com/vikramlearning/blazorbootstrap/pull/1253)).

### Tooltip
- Fixed typo in `TooltipColor.Secondary` CSS class name (PR [#1257](https://github.com/vikramlearning/blazorbootstrap/pull/1257)).

---

## 🐛 Bug Fixes

- Grid: Rendering issues and page size reset bug fixed (#1203).
- SortableList: Grab cursor now correctly shown on items and handles (#1253).
- Dropdown: Closes when clicking outside (#1251).
- PdfViewer: Password prompt cancellation handled gracefully (#1255).
- Tooltip: `TooltipColor.Secondary` CSS class name typo fixed (#1257).

---

## 📚 Documentation & API Metadata

Extensive API documentation metadata attributes (`[Parameter]`, `[EditorRequired]`, summary XML docs) have been added to all major components in this release, improving IntelliSense support and auto-generated API reference pages:

- Accordion, Alert, Badge, Breadcrumb, Button, Callout, Card
- Carousel, Collapse, ConfirmDialog
- Dropdown
- GoogleMap, Markdown, Modal
- Offcanvas, Pagination, PdfViewer
- Ribbon, Sidebar, Sidebar2
- SortableList, Spinner, Tabs, TextInput, ToastService, Toasts, Tooltips

---

## 🔧 Upgrade Notes

See the [Upgrade to v4.0.0 guide](https://docs.blazorbootstrap.com/blog/2025/12/01/upgrade-to-v4.0.0) for detailed migration instructions.

---

## Links
- [Docs Website - Blazor Bootstrap](https://docs.blazorbootstrap.com/)
- [Demos Website - Blazor Bootstrap](https://demos.blazorbootstrap.com/)
- [NuGet Package - Blazor.Bootstrap 4.0.0](https://www.nuget.org/packages/Blazor.Bootstrap/4.0.0)
- [GitHub Release](https://github.com/vikramlearning/blazorbootstrap/releases/tag/v4.0.0)
