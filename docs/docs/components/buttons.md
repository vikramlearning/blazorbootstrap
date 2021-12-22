﻿---
sidebar_label: Buttons
sidebar_position: 3
---

# Buttons

Use BlazorBootstrap's button styles for actions in forms, dialogs, and more with support for multiple sizes, states, and more.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| Active | bool | When set to `true`, places the component in the active state with active styling. | | false |
| Block | bool | Makes the button to span the full width of a parent. | | false |
| ChildContent | RenderFragment | Specifies the content to be rendered inside this `Button`. | | |
| Color | `ButtonColor` | Gets or sets the button color. | ✔️ | |
| Disabled | bool | When set to `true`, disables the component's functionality and places it in a disabled state. | | false |
| Loading | bool | Shows the loading spinner or a `LoadingTemplate`. | | false |
| LoadingTemplate | RenderFragment | Gets or sets the component loading template. | | |
| LoadingText | string | Gets or sets the loadgin text. | | `Loading...` |
| Outline | bool | Makes the button to have the outlines. | | false |
| Size | `Size` | Changes the size of a button. | | |
| TabIndex | int? | If defined, indicates that its element can be focused and can participates in sequential keyboard navigation. | | |
| Target | `Target` | The target attribute specifies where to open the linked document for a `ButtonType.Link`. | | `Target.None` |
| To | string | Denotes the target route of the `ButtonType.Link` button. | | |
| TooltipPlacement | `TooltipPlacement` | Tooltip placement | | `TooltipPlacement.Top` |
| TooltipTitle | string | Displays informative text when users hover, focus, or tap an element. | | |
| Type | `ButtonType` | Defines the button type. | | `ButtonType.Button` |

## Examples

### Buttons

BlazorBootstrap includes several predefined button styles, each serving its own semantic purpose, with a few extras thrown in for more control.

<img src="https://i.imgur.com/Ne7FJ5H.jpg" alt="Blazor Bootstrap: Button Component" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary"> Primary </Button>
    <Button Color="ButtonColor.Secondary"> Secondary </Button>
    <Button Color="ButtonColor.Success"> Success </Button>
    <Button Color="ButtonColor.Danger"> Danger </Button>
    <Button Color="ButtonColor.Warning"> Warning </Button>
    <Button Color="ButtonColor.Info"> Info </Button>
    <Button Color="ButtonColor.Light"> Light </Button>
    <Button Color="ButtonColor.Dark"> Dark </Button>
    <Button Color="ButtonColor.Link"> Link </Button>
</p>
```

### Button tags

<img src="https://i.imgur.com/ZscbcWh.jpg" alt="Blazor Bootstrap: Button Component - Button tags" />

```cshtml
<p>
    <Button Type="ButtonType.Link" Color="ButtonColor.Primary" To="#"> Link </Button>
    <Button Type="ButtonType.Submit" Color="ButtonColor.Primary" To="#"> Button </Button>
</p>
```

### Outline Buttons

<img src="https://i.imgur.com/ta0Mgtk.jpg" alt="Blazor Bootstrap: Button Component - Outline Buttons" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary" Outline="true"> Primary </Button>
    <Button Color="ButtonColor.Secondary" Outline="true"> Secondary </Button>
    <Button Color="ButtonColor.Success" Outline="true"> Success </Button>
    <Button Color="ButtonColor.Danger" Outline="true"> Danger </Button>
    <Button Color="ButtonColor.Warning" Outline="true"> Warning </Button>
    <Button Color="ButtonColor.Info" Outline="true"> Info </Button>
    <Button Color="ButtonColor.Dark" Outline="true"> Dark </Button>
</p>
```

:::info
Some of the button styles use a relatively light foreground color, and should only be used on a dark background in order to have sufficient contrast.
:::

### Sizes

Fancy larger or smaller buttons? Add `Size="Size.Large"` or `Size="Size.Small"` for additional sizes.

<img src="https://i.imgur.com/Vdiyg6q.jpg" alt="Blazor Bootstrap: Button Component - Sizes" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary" Size="Size.Large"> Large button </Button>
    <Button Color="ButtonColor.Secondary" Size="Size.Large"> Large button </Button>
</p>
<p>
    <Button Color="ButtonColor.Primary" Size="Size.Small"> Small button </Button>
    <Button Color="ButtonColor.Secondary" Size="Size.Small"> Small button </Button>
</p>
```

### Disable State

Make buttons look inactive by adding the `Disabled="true"` boolean parameter to any `<Button>` component. Disabled buttons have `pointer-events: none` applied to, preventing hover and active states from triggering.

<img src="https://i.imgur.com/A0MlIha.jpg" alt="Blazor Bootstrap: Button Component - Disable State" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary" Size="Size.Large" Disabled="true"> Large button </Button>
    <Button Color="ButtonColor.Secondary" Size="Size.Large" Disabled="true"> Large button </Button>
</p>
<p>
    <Button Type="ButtonType.Link" Color="ButtonColor.Primary" Size="Size.Large" Disabled="true"> Primary link </Button>
    <Button Type="ButtonType.Link" Color="ButtonColor.Secondary" Size="Size.Large" Disabled="true"> Link </Button>
</p>
```

:::info
Disabled buttons using the `Type="ButtonType.Link"` parameter behave a bit different.
:::

### Block Buttons

Create responsive stacks of full-width, "block buttons" like those in Bootstrap 4 with a mix of our display and gap utilities. By using utilities instead of button specific classes, we have much greater control over spacing, alignment, and responsive behaviors.

<img src="https://i.imgur.com/jB7joKv.jpg" alt="Blazor Bootstrap: Button Component - Block Buttons" />

```cshtml
<div class="d-grid gap-2">
    <Button Color="ButtonColor.Primary"> Button </Button>
    <Button Color="ButtonColor.Primary"> Button </Button>
</div>
```

<img src="https://i.imgur.com/iSsEMgi.jpg" alt="Blazor Bootstrap: Button Component" />

```cshtml
<div class="d-grid gap-2 d-md-block mt-2">
    <Button Color="ButtonColor.Primary"> Button </Button>
    <Button Color="ButtonColor.Primary"> Button </Button>
</div>
```

<img src="https://i.imgur.com/20LuzVC.jpg" alt="Blazor Bootstrap: Button Component" />

```cshtml
<div class="d-grid gap-2 col-6 mx-auto mt-2">
    <Button Color="ButtonColor.Primary"> Button </Button>
    <Button Color="ButtonColor.Primary"> Button </Button>
</div>
```

<img src="https://i.imgur.com/bJXgFkF.jpg" alt="Blazor Bootstrap: Button Component" />

```cshtml
<div class="d-grid gap-2 d-md-flex justify-content-md-end mt-2">
    <Button Color="ButtonColor.Primary"> Button </Button>
    <Button Color="ButtonColor.Primary"> Button </Button>
</div>
```

### Button Toogle States

If you''re pre-toggling a button, you must manually add the `Active="true"` parameter.

<img src="https://i.imgur.com/JH9SZxQ.jpg" alt="Blazor Bootstrap: Button Component - Button Toogle States" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary"> Toggle button </Button>
    <Button Color="ButtonColor.Primary" Active="true"> Active toggle button </Button>
    <Button Color="ButtonColor.Primary" Disabled="true"> Disabled toggle button </Button>
</p>
```

### Button with Loading

Use spinners within buttons to indicate an action is currently processing or taking place. You may also swap the text out of the spinner element and utilize button text as needed.

<img src="https://i.imgur.com/ENKhcXR.jpg" alt="Blazor Bootstrap: Button Component - Button with Loading" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary" Loading="true" />
    <Button Color="ButtonColor.Primary" Loading="true" LoadingText="Saving..." />
    <Button Color="ButtonColor.Primary" Loading="true">
        <LoadingTemplate>
            <span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span>
            Loading...
        </LoadingTemplate>
    </Button>
</p>
```

### Buttons with Tooltip

Hover over the buttons below to see the four tooltips directions: top, right, bottom, and left.

<img src="https://i.imgur.com/zp3G6pZ.jpg" alt="Blazor Bootstrap: Button Component - Buttons with Tooltip" />

```cshtml
<p>
    <Button Color="ButtonColor.Primary" TooltipTitle="Tooltip text" TooltipPlacement="TooltipPlacement.Top"> Tooltip Top </Button>

    <Button Color="ButtonColor.Primary" TooltipTitle="Tooltip text" TooltipPlacement="TooltipPlacement.Right"> Tooltip Right </Button>

    <Button Color="ButtonColor.Primary" TooltipTitle="Tooltip text" TooltipPlacement="TooltipPlacement.Bottom"> Tooltip Bottom </Button>

    <Button Color="ButtonColor.Primary" TooltipTitle="Tooltip text" TooltipPlacement="TooltipPlacement.Left"> Tooltip Left </Button>
</p>
```

:::caution NOTE

HTML tooltips not supported at this moment.

:::

### Events

#### Click events

```cshtml
<p>
    <Button Color="ButtonColor.Primary" @onclick="OnClick"> Click Event </Button>
</p>
```

```cs
@code{

    protected void OnClick(EventArgs args)
    {
        Console.WriteLine("click event");
    }

}
```

#### Double click event

```cshtml
<p>
    <Button Color="ButtonColor.Primary" @ondblclick="OnDoubleClick"> Double Click Event </Button>
</p>
```

```cs
@code{

    protected void OnDoubleClick(EventArgs args)
    {
        Console.WriteLine("double click event");
    }

}
```

#### Click event with arguments

```cshtml
<p>
    <Button Color="ButtonColor.Primary" @onclick="((args) => OnClickWithArgs(args, message))"> Click   Args </Button>
</p>
```

```cs
@code{

    public string message = "Test message";

    protected void OnClickWithArgs(EventArgs args, string message)
    {
        Console.WriteLine($"message: {message}");
    }

}
```