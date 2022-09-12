---
title: Blazor Callout Component
description: BlazorBootstrap's callout component provides content presentation in a visually distinct manner.
image: https://getblazorbootstrap.com/img/logo.svg

sidebar_label: Callout
sidebar_position: 5
---

# Callout

BlazorBootstrap's callout component provides content presentation in a visually distinct manner.

## Parameters

| Name | Type | Default | Required | Descritpion |
|--|--|--|--|--|
| ChildContent | RenderFragment | | ✔️ | Specifies the content to be rendered inside this. |
| Heading | string | | | Gets or sets the callout heading. |
| Type | enum | `CalloutType.Default` | | Use `CalloutType.Default` or `CalloutType.Info` or `CalloutType.Warning` or `CalloutType.Danger` or `CalloutType.Tip` |

## Examples

### Callout - Default

<img src="https://i.imgur.com/y2SYI6u.png" alt="Blazor Bootstrap: Callout Component - Default" />

```cshtml showLineNumbers
<Callout>
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout default demo here.](https://demos.getblazorbootstrap.com/callout#default)

### Callout - Danger

<img src="https://i.imgur.com/r8qs6l6.png" alt="Blazor Bootstrap: Callout Component - Danger" />

```cshtml{1} showLineNumbers
<Callout Type="CalloutType.Danger">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout danger demo here.](https://demos.getblazorbootstrap.com/callout#danger)

### Callout - Warning

<img src="https://i.imgur.com/7UxLMlb.png" alt="Blazor Bootstrap: Callout Component - Warning" />

```cshtml{1} showLineNumbers
<Callout Type="CalloutType.Warning">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout warning demo here.](https://demos.getblazorbootstrap.com/callout#warning)

### Callout - Info

<img src="https://i.imgur.com/7FOMpJl.png" alt="Blazor Bootstrap: Callout Component - Info" />

```cshtml{1} showLineNumbers
<Callout Type="CalloutType.Info">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout info demo here.](https://demos.getblazorbootstrap.com/callout#info)

### Callout - Tip

<img src="https://i.imgur.com/6iYeZyK.png" alt="Blazor Bootstrap: Callout Component - Tip" />

```cshtml{1} showLineNumbers
<Callout Type="CalloutType.Tip">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout info demo here.](https://demos.getblazorbootstrap.com/callout#tip)