---
sidebar_label: Pagination
sidebar_position: 9
---

# Pagination

Documentation and examples for showing pagination to indicate a series of related content exists across multiple pages.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|


## Callback Events

| Event | Description | 
|--|--|
| PageChanged | This event fires immediately when the page number is changed. |


## Examples

### Pagination

We use a large block of connected links for our pagination, making links hard to miss and easily scalable - all while providing large hit areas. Pagination is built with list HTML elements so screen readers can announce the number of available links.

<img src="https://i.imgur.com/6wDZ4zP.jpg" alt="Pagination - Examples" />

```cshtml
<Pagination TotalPages="8" />
<Pagination TotalPages="10" />
<Pagination TotalPages="13" />
<Pagination TotalPages="25" />
<Pagination TotalPages="100" />
```

### Working with icons

<img src="https://i.imgur.com/nhfGHfy.jpg" alt="Pagination - Working with icons" />

```cshtml
<Pagination ActivePageNumber="1"
            TotalPages="15"
            DisplayPages="5"
            FirstLinkIcon="IconName.ChevronDoubleLeft"
            PreviousLinkIcon="IconName.ChevronLeft"
            NextLinkIcon="IconName.ChevronRight"
            LastLinkIcon="IconName.ChevronDoubleRight" />
```

### Disabled and active states

<img src="https://i.imgur.com/SCbZVd4.jpg" alt="Pagination - Disabled and active states" />

```cshtml
<Pagination ActivePageNumber="1" TotalPages="10" />
<Pagination ActivePageNumber="3" TotalPages="10" />
<Pagination ActivePageNumber="5" TotalPages="10" />
```

### Sizing

Fancy larger or smaller pagination? Add `Size="PaginationSize.Small"` or `Size="PaginationSize.Large"` for additional sizes.

<img src="https://i.imgur.com/2kMVncQ.jpg" alt="Pagination - Sizing" />

```cshtml
<Pagination ActivePageNumber="5" TotalPages="5" Size="PaginationSize.Small" />
<Pagination ActivePageNumber="5" TotalPages="5" />
<Pagination ActivePageNumber="5" TotalPages="5" Size="PaginationSize.Large" />
```

### Alignment

<img src="https://i.imgur.com/RkpUdJu.jpg" alt="Pagination - Alignment" />

```cshtml
<Pagination ActivePageNumber="2" TotalPages="5" />
<Pagination ActivePageNumber="2" TotalPages="5" Alignment="Alignment.Center" />
<Pagination ActivePageNumber="2" TotalPages="5" Alignment="Alignment.End" />
```

### Callback Events

<img src="https://i.imgur.com/VsB3ZYW.jpg" alt="Pagination - Callback Events" />

```cshtml
<Pagination ActivePageNumber="@currentPageNumber"
            TotalPages="10"
            PageChanged="OnPageChangedAsync" />

<text>Current Page Number: @currentPageNumber</text>
```
```cs
@code {
    int currentPageNumber = 2;

    private async Task OnPageChangedAsync(int newPageNumber)
    {
        await Task.Run(() => { currentPageNumber = newPageNumber; });
    }
}
```