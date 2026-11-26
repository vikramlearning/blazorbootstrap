# Blazor Bootstrap Component Library

### An Enterprise-class Blazor Bootstrap Component library built on the Blazor and Bootstrap CSS frameworks.

**NOTE:** We strongly recommend all BlazorBootstrap users upgrade to version 4.1.0. [Read more...](https://docs.blazorbootstrap.com/blog/upgrade-to-v4.1.0)

## Table of contents

- [Quick start](#quick-start)
- [Online Demos](#online-demos)
- [Charts](#charts)
- [Components](#blazor-bootstrap-components)
- [Documentation](#documentation)
- [Creators](#creators)
- [Copyright and license](#copyright-and-license)

## Quick start

Get started any way you want

- Clone the repo: `git clone https://github.com/vikramlearning/blazorbootstrap.git`
- Install with [NuGet](https://www.nuget.org/): `Install-Package Blazor.Bootstrap -Version 4.1.0`
  - Supported .NET versions: .NET 8, .NET 9, .NET 10, and .NET 11.
- Install [Blazor Bootstrap templates](https://github.com/vikramlearning/blazorbootstrap-project-templates) with **.NET CLI**: `dotnet new install Blazor.Bootstrap.Templates::1.11.0`

  ![image](https://user-images.githubusercontent.com/2337067/233800604-43986ae7-27dd-4f17-9af6-c2f1a6f07097.png)

- Read the [Getting started page](https://docs.blazorbootstrap.com/getting-started/blazor-webassembly-net-11) for information on the framework installation, contents, examples, and more.

## Online Demos

- [Docs Website](https://docs.blazorbootstrap.com/)
- [Demo Website - Blazor Server](https://demos.blazorbootstrap.com/)
- [Demo Website - Blazor WebAssembly](https://demos.getblazorbootstrap.com/)

## Charts

Chart components have moved out of BlazorBootstrap and are now provided by the dedicated [BlazorExpress.ChartJS](https://chartjs.blazorexpress.com/) package.

This is a breaking change for existing chart users: install `BlazorExpress.ChartJS`, import its namespace, load its browser scripts, and migrate from the former BlazorBootstrap chart API. See the [migration and setup guide](https://docs.blazorbootstrap.com/components/charts), the [GitHub repository](https://github.com/BlazorExpress/BlazorExpress.ChartJS), and the [original demos](https://chartjs.blazorexpress.com/demos/bar-chart).

## Blazor Bootstrap Components

| Component | Docs | Demos |
|:--|:--|:--|
| Accordion | [Docs](https://docs.blazorbootstrap.com/components/accordion) | [Demos](https://demos.blazorbootstrap.com/demos/accordion) |
| Alerts | [Docs](https://docs.blazorbootstrap.com/components/alerts) | [Demos](https://demos.blazorbootstrap.com/demos/alerts) |
| Auto Complete | [Docs](https://docs.blazorbootstrap.com/forms/autocomplete) | [Demos](https://demos.blazorbootstrap.com/demos/form/autocomplete) |
| Avatar | [Docs](https://docs.blazorbootstrap.com/components/avatar) | [Demos](https://demos.blazorbootstrap.com/demos/avatar) |
| Badge | [Docs](https://docs.blazorbootstrap.com/components/badge) | [Demos](https://demos.blazorbootstrap.com/demos/badge) |
| Breadcrumb | [Docs](https://docs.blazorbootstrap.com/components/breadcrumb) | [Demos](https://demos.blazorbootstrap.com/demos/breadcrumb) |
| Button Group | [Docs](https://docs.blazorbootstrap.com/components/button-group) | [Demos](https://demos.blazorbootstrap.com/demos/button-group) |
| Buttons | [Docs](https://docs.blazorbootstrap.com/components/buttons) | [Demos](https://demos.blazorbootstrap.com/demos/buttons) |
| Calendar | [Docs](https://docs.blazorbootstrap.com/components/calendar) | [Demos](https://demos.blazorbootstrap.com/demos/calendar) |
| Callout | [Docs](https://docs.blazorbootstrap.com/components/callout) | [Demos](https://demos.blazorbootstrap.com/demos/callout) |
| Card | [Docs](https://docs.blazorbootstrap.com/components/card) | [Demos](https://demos.blazorbootstrap.com/demos/card) |
| Carousel | [Docs](https://docs.blazorbootstrap.com/components/carousel) | [Demos](https://demos.blazorbootstrap.com/demos/carousel) |
| Checkbox Input | [Docs](https://docs.blazorbootstrap.com/forms/checkbox-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/checkbox-input) |
| Collapse | [Docs](https://docs.blazorbootstrap.com/components/collapse) | [Demos](https://demos.blazorbootstrap.com/demos/collapse) |
| Confirm Dialog | [Docs](https://docs.blazorbootstrap.com/components/confirm-dialog) | [Demos](https://demos.blazorbootstrap.com/demos/confirm-dialog) |
| Currency Input | [Docs](https://docs.blazorbootstrap.com/forms/currency-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/currency-input) |
| Date Input | [Docs](https://docs.blazorbootstrap.com/forms/date-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/date-input) |
| Drag and Drop File Input | [Docs](https://docs.blazorbootstrap.com/forms/drag-and-drop-file-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/drag-and-drop-file-input) |
| Dropdown | [Docs](https://docs.blazorbootstrap.com/components/dropdown) | [Demos](https://demos.blazorbootstrap.com/demos/dropdown) |
| Enum Input | [Docs](https://docs.blazorbootstrap.com/forms/enum-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/enum-input) |
| File Input | [Docs](https://docs.blazorbootstrap.com/forms/file-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/file-input) |
| Google Map | [Docs](https://docs.blazorbootstrap.com/components/google-map) | [Demos](https://demos.blazorbootstrap.com/demos/google-map) |
| Grid | [Docs](https://docs.blazorbootstrap.com/components/grid) | [Demos](https://demos.blazorbootstrap.com/demos/grid/overview) |
| Icons | [Docs](https://docs.blazorbootstrap.com/content/icons) | [Demos](https://demos.blazorbootstrap.com/demos/icons) |
| Images | [Docs](https://docs.blazorbootstrap.com/content/images) | [Demos](https://demos.blazorbootstrap.com/demos/images) |
| Input Group | [Docs](https://docs.blazorbootstrap.com/components/input-group) | [Demos](https://demos.blazorbootstrap.com/demos/form/input-group) |
| List Group | [Docs](https://docs.blazorbootstrap.com/components/list-group) | [Demos](https://demos.blazorbootstrap.com/demos/list-group) |
| Markdown | [Docs](https://docs.blazorbootstrap.com/components/markdown) | [Demos](https://demos.blazorbootstrap.com/demos/markdown) |
| Mind Map | [Docs](https://docs.blazorbootstrap.com/components/mind-map) | [Demos](https://demos.blazorbootstrap.com/demos/mind-map) |
| Modals | [Docs](https://docs.blazorbootstrap.com/components/modal) | [Demos](https://demos.blazorbootstrap.com/demos/modals) |
| Navbar | [Docs](https://docs.blazorbootstrap.com/components/navbar) | [Demos](https://demos.blazorbootstrap.com/demos/navbar) |
| Number Input | [Docs](https://docs.blazorbootstrap.com/forms/number-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/number-input) |
| Offcanvas | [Docs](https://docs.blazorbootstrap.com/components/offcanvas) | [Demos](https://demos.blazorbootstrap.com/demos/offcanvas) |
| OTP Input | [Docs](https://docs.blazorbootstrap.com/forms/otp-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/otp-input) |
| Pagination | [Docs](https://docs.blazorbootstrap.com/components/pagination) | [Demos](https://demos.blazorbootstrap.com/demos/pagination) |
| Password Input | [Docs](https://docs.blazorbootstrap.com/forms/password-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/password-input) |
| PDF Viewer | [Docs](https://docs.blazorbootstrap.com/components/pdf-viewer) | [Demos](https://demos.blazorbootstrap.com/demos/pdf-viewer) |
| Placeholders | [Docs](https://docs.blazorbootstrap.com/components/placeholders) | [Demos](https://demos.blazorbootstrap.com/demos/placeholders) |
| Popover | [Docs](https://docs.blazorbootstrap.com/components/popover) | [Demos](https://demos.blazorbootstrap.com/demos/popover) |
| Preload | [Docs](https://docs.blazorbootstrap.com/components/preload) | [Demos](https://demos.blazorbootstrap.com/demos/preload) |
| Progress | [Docs](https://docs.blazorbootstrap.com/components/progress) | [Demos](https://demos.blazorbootstrap.com/demos/progress) |
| Radio Input | [Docs](https://docs.blazorbootstrap.com/forms/radio-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/radio-input) |
| Range Input | [Docs](https://docs.blazorbootstrap.com/forms/range-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/range-input) |
| Rating Input | [Docs](https://docs.blazorbootstrap.com/forms/rating-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/rating-input) |
| Ribbon | [Docs](https://docs.blazorbootstrap.com/components/ribbon) | [Demos](https://demos.blazorbootstrap.com/demos/ribbon) |
| Rich Text Editor | [Docs](https://docs.blazorbootstrap.com/forms/rich-text-editor) | [Demos](https://demos.blazorbootstrap.com/demos/form/rich-text-editor) |
| Script Loader | [Docs](https://docs.blazorbootstrap.com/components/script-loader) | [Demos](https://demos.blazorbootstrap.com/demos/script-loader) |
| Sidebar | [Docs](https://docs.blazorbootstrap.com/components/sidebar) | [Demos](https://demos.blazorbootstrap.com/demos/sidebar) |
| Sidebar 2 | [Docs](https://docs.blazorbootstrap.com/components/sidebar2) | [Demos](https://demos.blazorbootstrap.com/demos/sidebar2) |
| Sortable List | [Docs](https://docs.blazorbootstrap.com/components/sortable-list) | [Demos](https://demos.blazorbootstrap.com/demos/sortable-list) |
| Spinners | [Docs](https://docs.blazorbootstrap.com/components/spinners) | [Demos](https://demos.blazorbootstrap.com/demos/spinners) |
| Split View | [Docs](https://docs.blazorbootstrap.com/components/split-view) | [Demos](https://demos.blazorbootstrap.com/demos/split-view) |
| Switch | [Docs](https://docs.blazorbootstrap.com/forms/switch) | [Demos](https://demos.blazorbootstrap.com/demos/form/switch) |
| Tabs | [Docs](https://docs.blazorbootstrap.com/components/tabs) | [Demos](https://demos.blazorbootstrap.com/demos/tabs) |
| Text Input | [Docs](https://docs.blazorbootstrap.com/forms/text-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/text-input) |
| Text Area Input | [Docs](https://docs.blazorbootstrap.com/forms/textarea-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/text-area-input) |
| Theme Switcher | [Docs](https://docs.blazorbootstrap.com/components/theme-switcher) | [Demos](https://demos.blazorbootstrap.com/demos/theme-switcher) |
| Time Input | [Docs](https://docs.blazorbootstrap.com/forms/time-input) | [Demos](https://demos.blazorbootstrap.com/demos/form/time-input) |
| Timeline | [Docs](https://docs.blazorbootstrap.com/components/timeline) | [Demos](https://demos.blazorbootstrap.com/demos/timeline) |
| Toasts | [Docs](https://docs.blazorbootstrap.com/components/toasts) | [Demos](https://demos.blazorbootstrap.com/demos/toasts) |
| Tooltips | [Docs](https://docs.blazorbootstrap.com/components/tooltips) | [Demos](https://demos.blazorbootstrap.com/demos/tooltips) |

More components coming...

## Documentation

Blazor Bootstrap's documentation lives in the `docs` folder of the [GitHub repository](https://github.com/vikramlearning/blazorbootstrap). It is built with [Docusaurus](https://docusaurus.io/) and publicly hosted on GitHub Pages at <https://docs.blazorbootstrap.com/>.

## Creators

**Vikram Reddy**

## Copyright and license

© 2024–2026 Vikram Reddy and contributors. Licensed under the [Apache License 2.0](https://github.com/vikramlearning/blazorbootstrap/blob/main/LICENSE.txt).
