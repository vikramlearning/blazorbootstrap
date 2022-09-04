---
title: Blazor Placeholder Component
description: Use BlazorBootstrap's loading placeholders for your components or pages to indicate something may still be loading. 
image: https://getblazorbootstrap.com/img/logo.svg

sidebar_label: Placeholders
sidebar_position: 11
---

# Placeholders

Use BlazorBootstrap's loading placeholders for your components or pages to indicate something may still be loading.

## PlaceholderContainer Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| Animation | `PlaceholderAnimation` |  Gets or sets the placeholder animation. | | `PlaceholderAnimation.Glow` |
| ChildContent | `RenderFragment` | Specifies the content to be rendered inside this. | | |

## Placeholder Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| Width | `PlaceholderWidth` | Gets or sets the placeholder width. | | PlaceholderWidth.Col1 |
| Color | `PlaceholderColor` | Gets or sets the placeholder color. | | PlaceholderColor.None |
| Size | `PlaceholderSize` | Gets or sets the placeholder size. | | PlaceholderSize.None |

## Examples

### Placeholders

<img src="https://i.imgur.com/Mw13qfX.png" alt="Placeholders - Example" />

```cshtml showLineNumbers
<PlaceholderContainer Animation="PlaceholderAnimation.Glow">
    <Placeholder Width="PlaceholderWidth.Col7" />
    <Placeholder Width="PlaceholderWidth.Col9" />
    <Placeholder Width="PlaceholderWidth.Col6" />
    <Placeholder Width="PlaceholderWidth.Col7" />
</PlaceholderContainer>
```

[See demo here.](https://demos.getblazorbootstrap.com/placeholders#examples)

### Width

You can change the width through `PlaceholderWidth`, width utilities, or inline styles.

<img src="https://i.imgur.com/JcLisSd.png" alt="Placeholders - Width Example" />

```cshtml showLineNumbers
<Placeholder Width="PlaceholderWidth.Col6" />
<Placeholder Class="w-75" />
<Placeholder Style="width: 25%;" />
```

[See demo here.](https://demos.getblazorbootstrap.com/placeholders#width)

### Color

By default, the placeholder uses currentColor. This can be overridden with the `Color` property of type enum.

<img src="https://i.imgur.com/g8m05MQ.png" alt="Placeholders - Color Example" />

```cshtml showLineNumbers
<Placeholder Width="PlaceholderWidth.Col12" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Primary" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Secondary" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Success" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Danger" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Warning" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Info" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Light" />
<Placeholder Width="PlaceholderWidth.Col12" Color="PlaceholderColor.Dark" />
```

[See demo here.](https://demos.getblazorbootstrap.com/placeholders#color)

### Sizing

The size of placeholders are based on the typographic style of the parent element. Customize them with `Size` property of type enum.

<img src="https://i.imgur.com/udkkloE.png" alt="Placeholders - Sizing Example" />

```cshtml showLineNumbers
<Placeholder Width="PlaceholderWidth.Col12" />
<Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.Large" />
<Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.Small" />
<Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.ExtraSmall" />
```

[See demo here.](https://demos.getblazorbootstrap.com/placeholders#sizing)

### Animation

Animate placeholders with `PlaceholderAnimation.Glow` or `PlaceholderAnimation.Wave` to better convey the perception of something being actively loaded.

<img src="https://i.imgur.com/loMuJzD.png" alt="Placeholders - Animation Example" />

```cshtml showLineNumbers
<PlaceholderContainer Animation="PlaceholderAnimation.Glow">
    <Placeholder Width="PlaceholderWidth.Col12" />
    <Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.Large" />
    <Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.Small" />
    <Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.ExtraSmall" />
</PlaceholderContainer>

<br />
<br />

<PlaceholderContainer Animation="PlaceholderAnimation.Wave">
    <Placeholder Width="PlaceholderWidth.Col12" />
    <Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.Large" />
    <Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.Small" />
    <Placeholder Width="PlaceholderWidth.Col12" Size="PlaceholderSize.ExtraSmall" />
</PlaceholderContainer>
```

[See demo here.](https://demos.getblazorbootstrap.com/placeholders#animation)