---
title: Blazor Dropdown Component
description: Dropdowns are toggleable, contextual overlays for displaying lists of links and more. They are toggled by clicking, not by hovering; this is an intentional design decision'by bootstrap.
image: https://i.imgur.com/B5Hf85u.png

sidebar_label: Dropdown
sidebar_position: 9
---

# Blazor Dropdown

Dropdowns are toggleable, contextual overlays for displaying lists of links and more. 
They are toggled by clicking, not by hovering; this is an intentional design decision'by bootstrap.

<img src="https://i.imgur.com/B5Hf85u.png" alt="Blazor Bootstrap: Confirm Dialog component" />

## Parameters

| Name | Type | Descritpion | Required | Default | Added Version |
|:--|:--|:--|:--|:--|:--|
| Active | bool | When set to `true`, places the component in the active state with active styling. | | false | 1.0.0 |
| Block | bool | Makes the button to span the full width of a parent. | | false | 1.0.0 |
| ChildContent | RenderFragment | Specifies the content to be rendered inside this `Button`. | | | 1.0.0 |
| Color | `ButtonColor` | Gets or sets the button color. | ✔️ | | 1.0.0 |
| Disabled | bool | When set to `true`, disables the component's functionality and places it in a disabled state. | | false | 1.0.0 |
| Loading | bool | Shows the loading spinner or a `LoadingTemplate`. | | false | 1.0.0 |
| LoadingTemplate | RenderFragment | Gets or sets the component loading template. | | | 1.0.0 |
| LoadingText | string | Gets or sets the loadgin text. | | `Loading...` | 1.0.0 |
| Outline | bool | Makes the button to have the outlines. | | false | 1.0.0 |
| Position | `Position` | Gets or sets the position. | | | 1.7.0 |
| Size | `Size` | Changes the size of a button. | | | 1.0.0 |
| TabIndex | int? | If defined, indicates that its element can be focused and can participates in sequential keyboard navigation. | | | 1.0.0 |
| Target | `Target` | The target attribute specifies where to open the linked document for a `ButtonType.Link`. | | `Target.None` | 1.0.0 |
| To | string | Denotes the target route of the `ButtonType.Link` button. | | | 1.0.0 |
| TooltipPlacement | `TooltipPlacement` | Tooltip placement | | `TooltipPlacement.Top` | 1.0.0 |
| TooltipTitle | string | Displays informative text when users hover, focus, or tap an element. | | | 1.0.0 |
| TooltipColor | `TooltipColor` | Gets or sets the tooltip color. | | `TooltipColor.None` | 1.10.0 |
| Type | `ButtonType` | Defines the button type. | | `ButtonType.Button` | 1.0.0 |

## Methods

| Name | Return Type | Description | Added Version |
|:--|:--|:--|:--|
| ShowAsync(string title, string message1, ConfirmDialogOptions confirmDialogOptions = null) | Task<bool\> | Shows confirm dialog. | 1.1.0 |
| ShowAsync(string title, string message1, string message2, ConfirmDialogOptions confirmDialogOptions = null) | Task<bool\> | Shows confirm dialog. | 1.1.0 |
| ShowAsync<T\>(string title, Dictionary<string, object> parametres = null, ConfirmDialogOptions confirmDialogOptions = null) | Task<bool\> | Shows confirm dialog. T is component. | 1.1.0 |

## Examples

### Single button

<img src="https://i.imgur.com/B5Hf85u.png" alt="Blazor Bootstrap: Dropdown Component - Single button" />

```cshtml {1} showLineNumbers
<Dropdown>
    <DropdownToggleButton Color="ButtonColor.Secondary">Dropdown button</DropdownToggleButton>
    <DropdownMenu>
        <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
        <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
        <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
    </DropdownMenu>
</Dropdown>
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#examples)

### Color

<img src="https://i.imgur.com/yNv7Wvw.png" alt="Blazor Bootstrap: Dropdown Component - Color" />

```cshtml {1} showLineNumbers
<div class="d-flex gap-2 mb-4">
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Primary">Primary</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Secondary">Secondary</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Success">Success</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Info">Info</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Warning">Warning</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Danger">Danger</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
</div>
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#dynamic-component-as-confirm-dialog)

### Split button

Use `ConfirmDialogOptions` to change the text and color of the button.

<img src="https://i.imgur.com/uH19DpG.png" alt="Blazor Bootstrap: Dropdown Component - Split button" />

```cshtml showLineNumbers
<div class="d-flex gap-2 mb-4">
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Primary">Primary</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Primary" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Secondary">Secondary</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Secondary" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Success">Success</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Success" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Info">Info</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Info" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Warning">Warning</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Warning" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Danger">Danger</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Danger" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
</div>
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#change-buttons-text-and-color)

### Sizing

<img src="https://i.imgur.com/PMz3lbn.png" alt="Blazor Bootstrap: Dropdown Component - Sizing" />

```cshtml showLineNumbers
<div class="d-flex gap-2 mb-4">
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Secondary" Size="Size.Large">Large button</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem Size="Size.Large" To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Secondary" Size="Size.Large">Large split button</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Secondary" Size="Size.Large" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
</div>
```

```cshtml showLineNumbers
<div class="d-flex gap-2 mb-4">
    <Dropdown>
        <DropdownToggleButton Color="ButtonColor.Secondary" Size="Size.Small">Small button</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Split="true">
        <DropdownActionButton Color="ButtonColor.Secondary" Size="Size.Small">Small split button</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Secondary" Size="Size.Small" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
</div>
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#optional-sizes)

### Directions

### Dropup

To trigger **DropdownMenu** above elements, add the `Direction="DropdownDirection.Dropup"` to the **Dropdown** component.

<img src="https://i.imgur.com/8P6UejF.png" alt="Blazor Bootstrap: Dropdown Component - Dropup" />

```cshtml showLineNumbers
<div class="d-flex gap-2">
    <Dropdown Direction="DropdownDirection.Dropup">
        <DropdownToggleButton Color="ButtonColor.Secondary">Dropup button with text</DropdownToggleButton>
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
    <Dropdown Direction="DropdownDirection.Dropup" Split="true">
        <DropdownActionButton Color="ButtonColor.Secondary">Dropup split button</DropdownActionButton>
        <DropdownToggleButton Color="ButtonColor.Secondary" />
        <DropdownMenu>
            <DropdownItem To="#" Type="ButtonType.Link">Action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Another action</DropdownItem>
            <DropdownItem To="#" Type="ButtonType.Link">Something else here</DropdownItem>
        </DropdownMenu>
    </Dropdown>
</div>
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#scrolling-long-content)

### Dropup centered

To center the DropdownMenu above the toggle, add the Direction="DropdownDirection.DropupCentered" to the Dropdown component.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Dropend

To trigger DropdownMenu at the right of elements, add the Direction="DropdownDirection.Dropend" to the Dropdown component.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Dropstart

To trigger DropdownMenu at the left of elements, you can add the Direction="DropdownDirection.Dropstart" to the Dropdown component.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Active

To style DropdownItem as active, add the Active="true" parameter to the DropdownItem element in the DropdownMenu.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Disabled

To style DropdownItem as disabled, add the Disabled="true" parameter to the DropdownItem element in the DropdownMenu component.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Menu position

By default, a DropdownMenu is automatically positioned at 100% from the top and along the left side of its parent. You can change this with the Position parameter.

To right-align a DropdownMenu, add the Position="DropdownMenuPosition.End" parameter to the DropdownMenu component. Directions are mirrored when using Bootstrap in RTL.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Menu content

### Header

Add a header to label sections of actions in any dropdown menu.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Dividers

Separate groups of related menu items with a divider.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Text

Place any freeform text within a dropdown menu with text and use spacing utilities. Note that youll likely need additional sizing styles to constrain the menu width.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Forms

Put a form within a dropdown menu, or make it into a dropdown menu, and use margin or padding utilities to give it the negative space you require.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Auto close behavior

By default, the DropdownMenu is closed when clicking either inside or outside the DropdownMenu. You can use the AutoClose and AutoCloseBehavior parameters to change this behavior of the Dropdown.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Methods

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)

### Events

All dropdown events are fired at the toggling element and then bubbled up.

<img src="https://i.imgur.com/MjueRsB.png" alt="Blazor Bootstrap: Dropdown Component - " />

```cshtml showLineNumbers
```

```cs showLineNumbers
```

[See the demo here.](https://demos.blazorbootstrap.com/confirm-dialog#vertically-centered)