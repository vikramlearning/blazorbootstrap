﻿---
sidebar_label: Icons
sidebar_position: 1
---

# Icons

Documentation and examples for BlazorBootstrap Icons.

## Prerequisites

- Install Bootstrap Icons.
  - Refer: [Bootstrap Icons](https://icons.getbootstrap.com/)

- Include the icon fonts stylesheet in your website `<head>` or `@import` in CSS from CDN.
```
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.7.1/font/bootstrap-icons.css">
```

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| CustomIconName | string | Specify custom icons of your own, like `fontawesome`. Example: `fas fa-alarm-clock` | ✔️ | |
| Name | `IconName` | Gets or sets the icon name. | ✔️ | |
| Size | `IconSize` | Gets or sets the icon size. | | `IconSize.None` |

:::caution NOTE

Either `Name` or `CustomIconName` parameter is mandatory.

:::

## Examples

### Icons

<img src="https://i.imgur.com/WClg4kQ.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    <Icon Name="IconName.Alarm"></Icon>
    <Icon Name="IconName.AlarmFill"></Icon>
</p>
```

### Icons with different Sizes

<img src="https://i.imgur.com/ko7c6k3.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    <Icon Name="IconName.Alarm" Size="IconSize.x2"></Icon>
    <Icon Name="IconName.Alarm" Size="IconSize.x3"></Icon>
    <Icon Name="IconName.Alarm" Size="IconSize.x4"></Icon>
    <Icon Name="IconName.Alarm" Size="IconSize.x5"></Icon>
</p>
```

### Inline text with icon

<img src="https://i.imgur.com/eNKFAKg.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    Inline text <Icon Name="IconName.Alarm" />
</p>
```

### Link with icon

<img src="https://i.imgur.com/pDpv29z.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    <a href="#" class="text-decoration-none">
        Example link text <Icon Name="IconName.Alarm" />
    </a>
</p>
```

### Link with custom icon

<img src="https://i.imgur.com/KNFvgiS.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    <a href="#" class="text-decoration-none">
        Example link text <Icon CustomIconName="bi bi-bootstrap" />
    </a>
</p>
```

### Button with icon and text

<img src="https://i.imgur.com/Pkzbm1Q.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary"><Icon Name="IconName.Alarm" /> Button </Button>
    <Button Color="ButtonColor.Success"><Icon Name="IconName.Alarm" /> Button </Button>
    <Button Color="ButtonColor.Danger" Outline="true"><Icon Name="IconName.AlarmFill" /> Button </Button>
</p>
```

### Button with icon only

<img src="https://i.imgur.com/3WClQmS.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    <Button Color="ButtonColor.Secondary"><Icon Name="IconName.Alarm" /></Button>
</p>
```

## Articles

- [BlazorBootstrap: Icon Component Examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-icon-component-examples/88/156)