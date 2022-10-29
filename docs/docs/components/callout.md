---
title: Blazor Callout Component
description: BlazorBootstrap's callout component provides content presentation in a visually distinct manner.
image: https://i.imgur.com/vmibzEu.png

sidebar_label: Callout
sidebar_position: 5
---

# Blazor Callout

BlazorBootstrap's callout component provides content presentation in a visually distinct manner.

## Parameters

| Name | Type | Default | Required | Descritpion |
|:--|:--|:--|:--|:--|
| ChildContent | RenderFragment | | ✔️ | Specifies the content to be rendered inside this. |
| Heading | string | | | Gets or sets the callout heading. |
| Type | enum | `CalloutType.Default` | | Use `CalloutType.Default` or `CalloutType.Info` or `CalloutType.Warning` or `CalloutType.Danger` or `CalloutType.Tip` |

## Examples

### Callout

<img src="https://i.imgur.com/vmibzEu.png" alt="Blazor Bootstrap: Callout Component - Examples" />

```cshtml {1,5,9,13,17} showLineNumbers
<Callout>
    <strong>This is an default callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Danger">
    <strong>This is an danger callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Warning">
    <strong>This is an warning callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Info">
    <strong>This is an info callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Tip">
    <strong>This is an tip callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>
```
[See callout default demo here.](https://demos.getblazorbootstrap.com/callout#examples)

### Custom callout heading

<img src="https://i.imgur.com/gaZkJqo.png" alt="Blazor Bootstrap: Callout Component - Custom callout heading" />

```cshtml {1,5,9,13,17} showLineNumbers
<Callout Heading="Important">
    <strong>This is an default callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Danger" Heading="Important">
    <strong>This is an danger callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Warning" Heading="Important">
    <strong>This is an warning callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Info" Heading="Important">
    <strong>This is an info callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>

<Callout Type="CalloutType.Tip" Heading="Important">
    <strong>This is an tip callout</strong>. Example text to show it in action. See <a href="https://getblazorbootstrap.com/docs/components/callout">callout documentation</a>.
</Callout>
```
[See callout danger demo here.](https://demos.getblazorbootstrap.com/callout#custom-callout-heading)

### Large text

<img src="https://i.imgur.com/m4LeerM.png" alt="Blazor Bootstrap: Callout Component - Large text" />

```cshtml showLineNumbers
<Callout>
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>

<Callout Type="CalloutType.Danger">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>

<Callout Type="CalloutType.Warning">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>

<Callout Type="CalloutType.Info">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>

<Callout Type="CalloutType.Tip">
    <h4>Conveying meaning to assistive technologies</h4>
    <p>Using color to add meaning only provides a visual indication, which will not be conveyed to users of assistive technologies – such as screen readers. Ensure that information denoted by the color is either obvious from the content itself (e.g. the visible text), or is included through alternative means, such as additional text hidden with the <code>.visually-hidden</code> class.</p>
</Callout>
```
[See callout warning demo here.](https://demos.getblazorbootstrap.com/callout#large-text)