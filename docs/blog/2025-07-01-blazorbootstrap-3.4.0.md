---
title: Blazor Bootstrap v3.4.0
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
tags: [v3.4.0, blazor, bootstrap, blazorbootstrap, icons, grid, tabs]
---

We're excited to release version 3.4.0. This update includes Bootstrap and icon upgrades, plus enhancements to the grid, tabs, sidebar, and sidebar2.

![image](https://i.sstatic.net/C0JBvYrk.png "Blazor Bootstrap Radio Input Component - Basic Usage")

<!--truncate-->

This release includes numerous enhancements, new features, bug fixes, and upgrades to both Bootstrap and Bootstrap Icons. Below is a summary of the key changes included in this milestone.

### ✨ New Features & Enhancements

- Grid: Added support for showing/hiding or adding/removing GridColumn at runtime (#1027, #1060)
- Grid: Conditional rendering of GridColumn improvements (#310)
- Grid: Footer support and ability to add grid footers (#472, #921)
- Grid: Column width example added (#1099)
- Grid: Grid summary and culture updates (#1071, #1072, #1075)
- Grid: Enum filters support (#1073)
- Grid: FilterTextboxWidth can now use any unit, not just px (#879, #1058)
- Grid: Improved pagination, item selection, and items-per-page handling on small screens (#1019, #1123)
- Tabs: Setting Tab Class or Style is now functional (#1035, #1028)
- Tabs: Removing a Tab now makes the nearest one show (#1033, #1029)
- Sidebar: Image display support on sidebar/sidebar2 (#812, #1128)
- Icons: Updates and fixes in BootstrapIconUtility (#1086)
- Demos: Home page improvements (#1096)
- General: Ability to modify column visibility using IsVisible parameter (#1060)

### 🐞 Bug Fixes

- Grid: Responsive="true" was not responsive when Enum Filters enabled (#1028, #1124)
- Grid: Removing last item from last page did not navigate to new last page (#689, #1059)
- Grid: Conditional rendering of GridColumn issues (#310)
- Grid: Issue with grid summary updates (#1072)
- Grid: Items selection, pagination, and display issues on small screens (#1019, #1123)
- Grid: How to set Grid page number when current page is empty (#443)
- Grid: FilterTextboxWidth now consistently applies units (#879, #1058)
- Tabs: Setting Tab Class/Style issues fixed (#1035, #1028)
- Tabs: Removing a Tab now correctly shows the nearest tab (#1033, #1029)

### ⬆️ Upgrades

- Upgraded to Bootstrap v5.3.7 (#1118, #1120)
- Upgraded to Bootstrap Icons v1.13.1 (#1119, #1121)

### 🧩 Other Notable Improvements

- Improved documentation and demo coverage.
- Sidebar image support (area-sidebar, area-sidebar-2).
- Numerous merged PRs and community contributions for feature completion and bug fixes.

Special thanks to all contributors for their efforts and dedication to making BlazorBootstrap more robust and feature-rich!

## Links
- [Docs Website - Blazor Bootstrap](https://docs.blazorbootstrap.com/)
- [Demos Website - Blazor Bootstrap](https://demos.blazorbootstrap.com/)
