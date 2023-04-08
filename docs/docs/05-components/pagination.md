---
title: Blazor Pagination Component
description: Use Blazor Bootstrap pagination component to indicate a series of related content exists across multiple pages.
image: https://i.imgur.com/SCbZVd4.jpg

sidebar_label: Pagination
sidebar_position: 12
---

# Blazor Pagination

Use Blazor Bootstrap pagination component to indicate a series of related content exists across multiple pages.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ActivePageNumber | int | Active page number. Starts with 1. | | 1 |
| Alignment | `Alignment` | Gets or sets the pagination alignment. | | `Alignment.None` |
| DisplayPages | int | Gets or sets the maximum page links to be displayed. | | 5 |
| FirstLinkIcon | `IconName` | Gets or sets first link icon. | | |
| FirstLinkText | string | Gets or sets first link text. `FirstLinkText` is ignored if `FirstLinkIcon` is specified. | | First |
| LastLinkIcon | `IconName` | Gets or sets last link icon. | | |
| LastLinkText | string | Gets or sets last link text. `LastLinkText` is ignored if `LastLinkIcon` is specified. | | Last |
| NextLinkIcon | `IconName` | Gets or sets next link icon. | | |
| NextLinkText | string | Gets or sets next link text. `NextLinkText` is ignored if `NextLinkIcon` is specified. | | Next |
| PreviousLinkIcon | `IconName` | Gets or sets previous link icon. | | |
| PreviousLinkText | string | Gets or sets previous link text. `PreviousLinkText` is ignored if `PreviousLinkIcon` is specified. | | Previous |
| Size | `PaginationSize` | Gets or sets the pagination size. | | |
| TotalPages | int | Total pages of data items. | | |

## Callback Events

| Event | Description | 
|--|--|
| PageChanged | This event fires immediately when the page number is changed. |


## Examples

### Pagination

We use a large block of connected links for our pagination, making links hard to miss and easily scalable - all while providing large hit areas. Pagination is built with list HTML elements so screen readers can announce the number of available links.

<img src="https://i.imgur.com/6wDZ4zP.jpg" alt="Pagination - Examples" />

```cshtml showLineNumbers
<Pagination TotalPages="8" />
<Pagination TotalPages="10" />
<Pagination TotalPages="13" />
<Pagination TotalPages="25" />
<Pagination TotalPages="100" />
```

[See demo here.](https://demos.blazorbootstrap.com/pagination#examples)

### Working with icons

<img src="https://i.imgur.com/nhfGHfy.jpg" alt="Pagination - Working with icons" />

```cshtml showLineNumbers
<Pagination ActivePageNumber="1"
            TotalPages="15"
            DisplayPages="5"
            FirstLinkIcon="IconName.ChevronDoubleLeft"
            PreviousLinkIcon="IconName.ChevronLeft"
            NextLinkIcon="IconName.ChevronRight"
            LastLinkIcon="IconName.ChevronDoubleRight" />
```

[See demo here.](https://demos.blazorbootstrap.com/pagination#working-with-icons)

### Disabled and active states

<img src="https://i.imgur.com/SCbZVd4.jpg" alt="Pagination - Disabled and active states" />

```cshtml showLineNumbers
<Pagination ActivePageNumber="1" TotalPages="10" />
<Pagination ActivePageNumber="3" TotalPages="10" />
<Pagination ActivePageNumber="5" TotalPages="10" />
```

[See demo here.](https://demos.blazorbootstrap.com/pagination#disabled-and-active-states)

### Sizing

Fancy larger or smaller pagination? Add `Size="PaginationSize.Small"` or `Size="PaginationSize.Large"` for additional sizes.

<img src="https://i.imgur.com/2kMVncQ.jpg" alt="Pagination - Sizing" />

```cshtml showLineNumbers
<Pagination ActivePageNumber="5" TotalPages="5" Size="PaginationSize.Small" />
<Pagination ActivePageNumber="5" TotalPages="5" />
<Pagination ActivePageNumber="5" TotalPages="5" Size="PaginationSize.Large" />
```

[See demo here.](https://demos.blazorbootstrap.com/pagination#sizing)

### Alignment

<img src="https://i.imgur.com/RkpUdJu.jpg" alt="Pagination - Alignment" />

```cshtml showLineNumbers
<Pagination ActivePageNumber="2" TotalPages="5" />
<Pagination ActivePageNumber="2" TotalPages="5" Alignment="Alignment.Center" />
<Pagination ActivePageNumber="2" TotalPages="5" Alignment="Alignment.End" />
```

[See demo here.](https://demos.blazorbootstrap.com/pagination#alignment)

### Callback Events

<img src="https://i.imgur.com/VsB3ZYW.jpg" alt="Pagination - Callback Events" />

```cshtml showLineNumbers
<Pagination ActivePageNumber="@currentPageNumber"
            TotalPages="10"
            PageChanged="OnPageChangedAsync" />

<text>Current Page Number: @currentPageNumber</text>
```

```cs showLineNumbers
@code {
    int currentPageNumber = 2;

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        await Task.Run(() => { currentPageNumber = newPageNumber; });
    }
}
```

[See demo here.](https://demos.blazorbootstrap.com/pagination#events)