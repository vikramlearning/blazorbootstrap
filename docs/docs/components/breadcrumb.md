---
sidebar_label: Breadcrumb
sidebar_position: 2
---

# Breadcrumb

Indicate the current page's location within a navigational hierarchy that automatically adds separators via CSS.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| Items | `List<BreadcrumbItem>` | List of all the items. | ✔️ | |

## Examples

### Breadcrumb

<img src="https://i.imgur.com/hO90HoC.jpg" alt="Blazor Bootstrap: Breadcrumb Component" />

```cshtml
<div>
    <Breadcrumb Items="NavItems1"></Breadcrumb>
</div>

<div>
    <Breadcrumb Items="NavItems2"></Breadcrumb>
</div>
```

```cs
@code {
    private List<BreadcrumbItem> NavItems1 { get; set; }
    private List<BreadcrumbItem> NavItems2 { get; set; }

    protected override void OnInitialized()
    {
        NavItems1 = new List<BreadcrumbItem>
        {
            new BreadcrumbItem{ Text = "Home", Href ="/" },
            new BreadcrumbItem{ Text = "Breadcrumb", IsCurrentPage = true }
        };

        NavItems2 = new List<BreadcrumbItem>
        {
            new BreadcrumbItem{ Text = "Home", Href ="/" },
            new BreadcrumbItem{ Text = "Docs", Href ="/docs" },
            new BreadcrumbItem{ Text = "Breadcrumb", IsCurrentPage = true }
        };
    }
}
```

### Dividers

Dividers are automatically added in CSS through `::before` and `content`. They can be changed by modifying a local CSS custom property `--bs-breadcrumb-divider.`

<img src="https://i.imgur.com/oUqUOY1.jpg" alt="Blazor Bootstrap: Breadcrumb Component - Dividers" />

```cshtml
<div>
    <Breadcrumb style="--bs-breadcrumb-divider: '>';" Items="NavItems2"></Breadcrumb>
</div>
```

```cs
@code {
    private List<BreadcrumbItem> NavItems2 { get; set; }

    protected override void OnInitialized()
    {
        NavItems2 = new List<BreadcrumbItem>
        {
            new BreadcrumbItem{ Text = "Home", Href ="/" },
            new BreadcrumbItem{ Text = "Docs", Href ="/docs" },
            new BreadcrumbItem{ Text = "Breadcrumb", IsCurrentPage = true }
        };
    }
}
```

### Embedded SVG icon

It's also possible to use an embedded SVG icon. Apply it via our CSS custom property

<img src="https://i.imgur.com/mZaXqgZ.jpg" alt="Blazor Bootstrap: Breadcrumb Component - Embedded SVG icon" />

```cshtml
<div>
    <Breadcrumb style="--bs-breadcrumb-divider: url(&#34;data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='8' height='8'%3E%3Cpath d='M2.5 0L1 1.5 3.5 4 1 6.5 2.5 8l4-4-4-4z' fill='currentColor'/%3E%3C/svg%3E&#34;);" Items="NavItems2"></Breadcrumb>
</div>
```

```cs
@code {
    private List<BreadcrumbItem> NavItems2 { get; set; }

    protected override void OnInitialized()
    {
        NavItems2 = new List<BreadcrumbItem>
        {
            new BreadcrumbItem{ Text = "Home", Href ="/" },
            new BreadcrumbItem{ Text = "Docs", Href ="/docs" },
            new BreadcrumbItem{ Text = "Breadcrumb", IsCurrentPage = true }
        };
    }
}

## Articles

- [Blazor Bootstrap: Breadcrumb Examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-breadcrumb-examples/88/160)