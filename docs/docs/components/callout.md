---
sidebar_label: Callout
sidebar_position: 4
---

# Callout

BlazorBootstrap callout component provides presentation of content in a visually distinct manner. Includes a heading, icon and typically text-based content.


## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | ✔️ | |
| Color | `CalloutColor` | Gets or sets the callout color. | | `CalloutColor.None` |

## Examples

### Callout - Default

<img src="https://i.imgur.com/MT3utK8.jpg" alt="Blazor Bootstrap: Callout Component - Default" />

```cshtml
<Callout>
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout default demo here.](https://demos.getblazorbootstrap.com/callout#default)

### Callout - Danger

<img src="https://i.imgur.com/0EAmQcp.jpg" alt="Blazor Bootstrap: Callout Component - Danger" />

```cshtml
<Callout Color="CalloutColor.Danger">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout danger demo here.](https://demos.getblazorbootstrap.com/callout#danger)

### Callout - Warning

<img src="https://i.imgur.com/e9wy7fg.jpg" alt="Blazor Bootstrap: Callout Component - Warning" />

```cshtml
<Callout Color="CalloutColor.Warning">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout warning demo here.](https://demos.getblazorbootstrap.com/callout#warning)

### Callout - Info

<img src="https://i.imgur.com/b4hecTm.jpg" alt="Blazor Bootstrap: Callout Component - Info" />

```cshtml
<Callout Color="CalloutColor.Info">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout info demo here.](https://demos.getblazorbootstrap.com/callout#info)