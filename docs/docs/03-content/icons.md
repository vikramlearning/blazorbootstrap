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

| Name | Type | Descritpion | Required | Default | Added Version |
|:--|:--|:--|:--|:--|:--|
| Color | `IconColor` | Gets or sets the icon color. | | `IconColor.None` | 1.9.0 |
| CustomIconName | string | Specify custom icons of your own, like `fontawesome`. Example: `fas fa-alarm-clock` | ✔️ | | 1.0.0 |
| Name | `IconName` | Gets or sets the icon name. | ✔️ | | 1.0.0 |
| Size | `IconSize` | Gets or sets the icon size. | | `IconSize.None` | 1.0.0 |

:::caution NOTE
Either `Name` or `CustomIconName` parameter is mandatory.
:::

## Examples

### Icons

<img src="https://i.imgur.com/WClg4kQ.jpg" alt="Blazor Bootstrap: Icon Component" />

```cshtml {} showLineNumbers
<Icon Name="IconName.Alarm" />
<Icon Name="IconName.AlarmFill" />
<Icon Name="IconName.Window" />
<Icon Name="IconName.Apple" />
```
[See icons demo here.](https://demos.blazorbootstrap.com/icons#examples)

### Sizes

<img src="https://i.imgur.com/ko7c6k3.jpg" alt="Blazor Bootstrap: Icon Component - Sizes" />

```cshtml {} showLineNumbers
<Icon Name="IconName.Alarm" Size="IconSize.x2" />
<Icon Name="IconName.Alarm" Size="IconSize.x3" />
<Icon Name="IconName.Alarm" Size="IconSize.x4" />
<Icon Name="IconName.Alarm" Size="IconSize.x5" />
```
[See icons with different size demo here.](https://demos.blazorbootstrap.com/icons#sizes)

### Colors

<img src="https://i.imgur.com/pUutAt8.png" alt="Blazor Bootstrap: Icon Component - Colors" />

```cshtml {} showLineNumbers
<Icon Name="IconName.Facebook" Size="IconSize.x2" Color="IconColor.Primary" />
<Icon Name="IconName.CloudLightningRainFill" Size="IconSize.x2" Color="IconColor.Secondary" />
<Icon Name="IconName.CheckAll" Size="IconSize.x2" Color="IconColor.Success" />
<Icon Name="IconName.Bug" Size="IconSize.x2" Color="IconColor.Danger" />
<Icon Name="IconName.ExclamationDiamondFill" Size="IconSize.x2" Color="IconColor.Warning" />
<Icon Name="IconName.InfoCircleFill" Size="IconSize.x2" Color="IconColor.Info" />
<Icon Name="IconName.CreditCard2FrontFill" Size="IconSize.x2" Color="IconColor.Light" />
<Icon Name="IconName.Apple" Size="IconSize.x2" Color="IconColor.Dark" />
<Icon Name="IconName.Asterisk" Size="IconSize.x2" Color="IconColor.Body" />
<Icon Name="IconName.VolumeMuteFill" Size="IconSize.x2" Color="IconColor.Muted" />
<Icon Name="IconName.BrowserSafari" Size="IconSize.x2" Color="IconColor.White" />
```
[See icons with different size demo here.](https://demos.blazorbootstrap.com/icons#colors)

### Inline text with icon

<img src="https://i.imgur.com/eNKFAKg.jpg" alt="Blazor Bootstrap: Icon Component - Inline text with icon" />

```cshtml {} showLineNumbers
Inline text <Icon Name="IconName.Alarm" />
```
[See inline text with icon demo here.](https://demos.blazorbootstrap.com/icons#inline-text-with-icon)

### Link with icon

<img src="https://i.imgur.com/pDpv29z.jpg" alt="Blazor Bootstrap: Icon Component - Link with icon" />

```cshtml {} showLineNumbers
<a href="#" class="text-decoration-none">
    Example link text <Icon Name="IconName.Alarm" />
</a>
```
[See link with icon demo here.](https://demos.blazorbootstrap.com/icons#link-with-icon)

### Link with custom icon

<img src="https://i.imgur.com/KNFvgiS.jpg" alt="Blazor Bootstrap: Icon Component - Link with custom icon" />

```cshtml {} showLineNumbers
<a href="#" class="text-decoration-none">
    Example link text <Icon CustomIconName="bi bi-bootstrap" />
</a>
```
[See link with custom icon demo here.](https://demos.blazorbootstrap.com/icons#link-with-custom-icon)

### Button with icon and text

<img src="https://i.imgur.com/Pkzbm1Q.jpg" alt="Blazor Bootstrap: Icon Component - Button with icon and text" />

```cshtml {} showLineNumbers
<Button Color="ButtonColor.Primary"><Icon Name="IconName.Alarm" /> Button </Button>
<Button Color="ButtonColor.Success"><Icon Name="IconName.Alarm" /> Button </Button>
<Button Color="ButtonColor.Danger" Outline="true"><Icon Name="IconName.AlarmFill" /> Button </Button>
```
[See button with icon and text demo here.](https://demos.blazorbootstrap.com/icons#button-with-icon-and-text)

### Button with icon only

<img src="https://i.imgur.com/3WClQmS.jpg" alt="Blazor Bootstrap: Icon Component - Button with icon only" />

```cshtml {} showLineNumbers
<Button Color="ButtonColor.Secondary"><Icon Name="IconName.Alarm" /></Button>
```
[See button with icon only demo here.](https://demos.blazorbootstrap.com/icons#button-with-icon-only)

### Bootstrap Icons

<img src="https://i.imgur.com/8HcjpiK.png" alt="Blazor Bootstrap: Icon Component - Bootstrap Icons" />

[See all bootstrap icons demo here.](https://demos.blazorbootstrap.com/icons#bootstrap-icons)