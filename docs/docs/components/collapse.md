---
title: Blazor Collapse Component
description: Toggle the visibility of content across your project with the Blazor Bootstrap Collapse component.
image: https://i.imgur.com/8A0emQe.png

sidebar_label: Collapse
sidebar_position: 7
---

# Blazor Collapse

Toggle the visibility of content across your project with the Blazor Bootstrap `Collapse` component.

<img src="https://i.imgur.com/8A0emQe.png" alt="Blazor Bootstrap: Collapse Component" />

## Parameters

| Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| ChildContent | RenderFragment | | ✔️ | Specifies the content to be rendered inside the collapse. | 1.7.0 |
| CollapseHorizontal | bool | false |  | Gets or sets the collapse horizontal. | 1.7.0 |
| Parent | string | null | | Gets or sets the parent. | 1.7.0 |
| Toggle | bool | false | | Toggles the collapsible element on invocation. | 1.7.0 |

## How it works

The `Collapse` component is used to show and hide content. Use `ShowAsync`, `HideAsync`, and `ToggleAsync` methods to toggle the content. 
Collapsing an element will animate the height from its current value to `0`.

:::info
The animation effect of this component is dependent on the prefers-reduced-motion media query.
See the [reduced motion section of our accessibility documentation](https://getbootstrap.com/docs/5.3/getting-started/accessibility/#reduced-motion).
:::

## Examples

Click the buttons below to show and hide the content.

<img src="https://i.imgur.com/8A0emQe.png" alt="Blazor Bootstrap: Collapse Component - Examples" />

```cshtml {1-3,5} showLineNumbers
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" @onclick="ShowContentAsync">Show content</Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" @onclick="HideContentAsync">Hide content</Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" @onclick="ToggleContentAsync">Toggle content</Button>

<Collapse @ref="collapse1">
    <Card>
        <CardBody>
            Some placeholder content for the collapse component. This panel is hidden by default but revealed when the user activates the relevant trigger.
        </CardBody>
    </Card>
</Collapse>
```
```cshtml {2,4-6} showLineNumbers
@code {
    Collapse collapse1 = default!;

    private async Task ShowContentAsync() => await collapse1.ShowAsync();
    private async Task HideContentAsync() => await collapse1.HideAsync();
    private async Task ToggleContentAsync() => await collapse1.ToggleAsync();
}
```

[See demo here](https://demos.blazorbootstrap.com/collapse#examples)

## Horizontal

The `Collapse` component supports horizontal collapsing. 
Set the `CollapseHorizontal` parameter to `true` to enable horizontal collapsing.

<img src="https://i.imgur.com/kgSAEVL.png" alt="Blazor Bootstrap: Collapse Component - Horizontal" />

```cshtml {1-3,5} showLineNumbers
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" @onclick="ShowContentAsync">Show content</Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" @onclick="HideContentAsync">Hide content</Button>
<Button Type="ButtonType.Button" Color="ButtonColor.Primary" @onclick="ToggleContentAsync">Toggle content</Button>

<Collapse @ref="collapse1" CollapseHorizontal="true">
    <Card Style="width:300px;">
        <CardBody>
            This is some placeholder content for a horizontal collapse. It's hidden by default and shown when triggered.
        </CardBody>
    </Card>
</Collapse>
```
```cshtml {2,4-6} showLineNumbers
@code {
    Collapse collapse1 = default!;

    private async Task ShowContentAsync() => await collapse1.ShowAsync();
    private async Task HideContentAsync() => await collapse1.HideAsync();
    private async Task ToggleContentAsync() => await collapse1.ToggleAsync();
}
```

[See demo here](https://demos.blazorbootstrap.com/collapse#horizontal)