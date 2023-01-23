---
title: Blazor Icons Component
description: Use Blazor Bootstrap tooltip component to add custom tooltips to your web pages.
image: https://i.imgur.com/8HcjpiK.png

sidebar_label: Icons
sidebar_position: 1
---

# Blazor Icons

Blazor Bootstrap icon component will display an icon from any icon font.

## Prerequisites

- Install Bootstrap Icons or other.
  - Refer: [Bootstrap Icons](https://icons.getbootstrap.com/)

- Include the icon fonts stylesheet in your website `<head>` or `@import` in CSS from CDN.
```
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.8.0/font/bootstrap-icons.css">
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
[See icons demo here.](https://demos.blazorbootstrap.com/icons#examples)

### Sizes

<img src="https://i.imgur.com/ko7c6k3.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml
<p>
    <Icon Name="IconName.Alarm" Size="IconSize.x2"></Icon>
    <Icon Name="IconName.Alarm" Size="IconSize.x3"></Icon>
    <Icon Name="IconName.Alarm" Size="IconSize.x4"></Icon>
    <Icon Name="IconName.Alarm" Size="IconSize.x5"></Icon>
</p>
```
[See icons with different size demo here.](https://demos.blazorbootstrap.com/icons#sizes)

### Inline text with icon

<img src="https://i.imgur.com/eNKFAKg.jpg" alt="Blazor Bootstrap: Icon Component - Inline text with icon" />

```cshtml
<p>
    Inline text <Icon Name="IconName.Alarm" />
</p>
```
[See inline text with icon demo here.](https://demos.blazorbootstrap.com/icons#inline-text-with-icon)

### Link with icon

<img src="https://i.imgur.com/pDpv29z.jpg" alt="Blazor Bootstrap: Icon Component - Link with icon" />

```cshtml
<p>
    <a href="#" class="text-decoration-none">
        Example link text <Icon Name="IconName.Alarm" />
    </a>
</p>
```
[See link with icon demo here.](https://demos.blazorbootstrap.com/icons#link-with-icon)

### Link with custom icon

<img src="https://i.imgur.com/KNFvgiS.jpg" alt="Blazor Bootstrap: Icon Component - Link with custom icon" />

```cshtml
<p>
    <a href="#" class="text-decoration-none">
        Example link text <Icon CustomIconName="bi bi-bootstrap" />
    </a>
</p>
```
[See link with custom icon demo here.](https://demos.blazorbootstrap.com/icons#link-with-custom-icon)

### Button with icon and text

<img src="https://i.imgur.com/Pkzbm1Q.jpg" alt="Blazor Bootstrap: Icon Component - Button with icon and text" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary"><Icon Name="IconName.Alarm" /> Button </Button>
    <Button Color="ButtonColor.Success"><Icon Name="IconName.Alarm" /> Button </Button>
    <Button Color="ButtonColor.Danger" Outline="true"><Icon Name="IconName.AlarmFill" /> Button </Button>
</p>
```
[See button with icon and text demo here.](https://demos.blazorbootstrap.com/icons#button-with-icon-and-text)

### Button with icon only

<img src="https://i.imgur.com/3WClQmS.jpg" alt="Blazor Bootstrap: Icon Component - Button with icon only" />

```cshtml
<p>
    <Button Color="ButtonColor.Secondary"><Icon Name="IconName.Alarm" /></Button>
</p>
```
[See button with icon only demo here.](https://demos.blazorbootstrap.com/icons#button-with-icon-only)

### Bootstrap Icons

<img src="https://i.imgur.com/8HcjpiK.png" alt="Blazor Bootstrap: Icon Component - Bootstrap Icons" />

[See all bootstrap icons demo here.](https://demos.blazorbootstrap.com/icons#bootstrap-icons)