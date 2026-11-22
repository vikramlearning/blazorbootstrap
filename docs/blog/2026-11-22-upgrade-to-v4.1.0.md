---
title: Upgrade to v4.1.0
draft: true
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
---

<!-- Draft for the 4.1.0 release: set the file date to the release date and remove draft: true when publishing. -->

## Recommendation

Version **4.1.0** adds features and accessibility fixes without breaking the public API: every new parameter is optional and off by default, and existing grids render as before. A few components now render slightly different markup, so check the list below if your own CSS, scripts, or UI tests target these elements.

---

## Markup changes you may target

### Grid

- **Sortable column headers** stay `<th>` column headers and now contain a `<button class="bb-grid-sort-button">` that sorts with a click, Enter, or Space. The `<th>` no longer has `role="button"` or its own click handler, and it carries `aria-sort="ascending"` or `aria-sort="descending"` while the column is sorted. A selector such as `th[role=button]` no longer matches; the clickable area is the header's text and icon rather than the whole cell.
- **Filter row inputs** have an `aria-label` from the new `FilterInputLabelText` parameter (`Filter by {0}`).
- **Selection checkboxes** no longer have `role="button"`; they are named by `SelectAllRowsText`, `SelectRowText`, or `RowSelectionLabel`.
- **Column keys** in saved settings (`GridSettings.ColumnOrder` and `GridSettings.HiddenColumns`) are each column's `PropertyName`, or its `HeaderText` when it has none; a later column with the same name is keyed `Name#2`, `Name#3`, and so on.

### MindMap

- Each node's **card** is now the tree item: `role="treeitem"`, `tabindex`, keyboard handling, `aria-level`, `aria-setsize`, `aria-posinset`, `aria-expanded`, and `aria-selected` moved from `.bb-mind-map-node` to `.bb-mind-map-card`. The node wrapper and the child lists have `role="none"`.
- The focused tree item's id ends in `-c` (for example `mymap-n-0-c`), and the focus ring is drawn on `.bb-mind-map-card:focus-visible`. Styles written for `.bb-mind-map-node:focus` no longer apply.

### PdfViewer

- The menu toggle in the toolbar is a `<button class="... pdf-viewer-dropdown-toggle">` holding the three-dots icon, instead of an `<i>` element. Its name comes from the new `MoreActionsText` parameter (`More actions`).

---

## New localizable text

| Component | Parameter | Default |
|:--|:--|:--|
| Grid | `ExportButtonText` | `Export CSV` |
| Grid | `ExportErrorText` | `The rows could not be exported. Try again.` |
| Grid | `ExportNoDataText` | `There are no rows to export.` |
| Grid | `ColumnChooserButtonText` | `Columns` |
| Grid | `ColumnChooserSearchPlaceholderText` | `Search columns` |
| Grid | `ColumnChooserResetText` | `Reset to default` |
| Grid | `ColumnChooserMoveUpText`, `ColumnChooserMoveDownText` | `Move {0} up`, `Move {0} down` |
| Grid | `ColumnChooserMovedText` | `{0} moved to position {1} of {2}.` |
| Grid | `ColumnChooserNoMatchText` | `No columns match` |
| Grid | `ColumnChooserLastColumnText` | `At least one column stays visible.` |
| Grid | `FilterInputLabelText` | `Filter by {0}` |
| Grid | `SelectAllRowsText`, `SelectRowText` | `Select all rows`, `Select row` |
| Grid | `UnnamedSortColumnText` | `Sort column {0}` |
| PdfViewer | `MoreActionsText` | `More actions` |

If a translation of one of these texts has placeholders that don't match, it is shown as written instead of failing.

See the [Grid](/docs/components/grid) page for the new column chooser and CSV export.
