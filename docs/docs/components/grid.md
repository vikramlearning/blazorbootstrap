---
sidebar_label: Grid
sidebar_position: 6
---

# Grid

Use BlazorBootstrap's Grid component to display tabular data from the data source. And it supports client-side and server-side paging & sorting.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside the grid. | | |
| EmptyDataTemplate | RenderFragment | Template to render when there are no rows to display. | | `No Data.` |
| DataProvider | `GridDataProviderDelegate<TItem\>` | DataProvider is for items to render. The provider should always return an instance of `GridDataProviderResult`, and `null` is not allowed. | | |
| PageSize | int | Gets or sets the page size of the grid. | | 10 |
| Sortable | bool | Gets or sets whether end-users can sort data by the column's values. | | true |

## Examples

### Grid
