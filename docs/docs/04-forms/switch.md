---
title: Blazor Switch Component
description: Use the Blazor Bootstrap `Switch` component to show the consistent cross-browser and cross-device custom checkboxes.
image: https://i.imgur.com/ALKzreq.png

sidebar_label: Switch
sidebar_position: 5
---

# Blazor Switch

Use the Blazor Bootstrap `Switch` component to show the consistent cross-browser and cross-device custom checkboxes.

<img src="https://i.imgur.com/ALKzreq.png" alt="Blazor Bootstrap: Switch Component" />

## Parameters

| Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| Disabled | bool | false | | Gets or sets the disabled. | 1.3.0 |
| Label | string | null | | Gets or sets the label.  | 1.3.0 |
| Reverse | bool | false | | Determines whether to put the switch on the opposite side. | 1.3.0 |
| Value | bool | false | ✔️ | Gets or sets the value. | 1.3.0 |

## Methods

| Name | Return Type |Description | Added Version |
|:--|:--|:--|:--|
| Disable() | void | Disables switch. | 1.3.0 |
| Enable() | void | Enables switch. | 1.3.0 |

## Events

| Name | Description | Added Version |
|:--|:--|:--|
| ValueChanged | This event fired when the switch selection changed. | 1.3.0 |

## Examples

### Basic usage

<img src="https://i.imgur.com/ALKzreq.png" alt="Blazor Bootstrap: Switch Component - Basic usage" />

```cshtml {} showLineNumbers
<Switch @bind-Value="agree1" Label="Default switch checkbox input" />
<Switch @bind-Value="agree2" Label="Checked switch checkbox input" />

<div class="mt-3">Switch 1 Status: <b>@agree1</b></div>
<div>Switch 2 Status: <b>@agree2</b></div>
```

```cs showLineNumbers
@code {
    bool agree1;
    bool agree2 = true;
}
```

[See demo here](https://demos.blazorbootstrap.com/form/switch#basic-usage)

### Disable

### Disable

Use the <b>Disabled</b> parameter to disable the Switch.

```cshtml {2,5-7} showLineNumbers
<div class="mb-3">
    <Switch @bind-Value="agree" Disabled="@disabled" Label="Disabled switch checkbox input" />
</div>

<Button Color="ButtonColor.Primary" @onclick="Enable"> Enable </Button>
<Button Color="ButtonColor.Secondary" @onclick="Disable"> Disable </Button>
<Button Color="ButtonColor.Warning" @onclick="Toggle"> Toggle </Button>
```

```cs {3,5,7,9} showLineNumbers
@code {
    private bool agree = true;
    private bool disabled = true;

    private void Enable() => disabled = false;

    private void Disable() => disabled = true;

    private void Toggle() => disabled = !disabled;
}
```

Also, use **Enable()** and **Disable()** methods to enable and disable the Switch.

:::caution NOTE
Do not use both the **Disabled** parameter and **Enable()** & **Disable()** methods.
:::

```cshtml {2,5-6} showLineNumbers
<div class="mb-3">
    <Switch @ref="switch1" @bind-Value="agree" Label="Disabled switch checkbox input" />
</div>

<Button Color="ButtonColor.Secondary" @onclick="Disable"> Disable </Button>
<Button Color="ButtonColor.Primary" @onclick="Enable"> Enable </Button>
```

```cs {2,5,7} showLineNumbers
@code {
    private Switch switch1 = default!;
    private bool agree = true;

    private void Disable() => switch1.Disable();

    private void Enable() => switch1.Enable();
}
```

[See demo here](https://demos.blazorbootstrap.com/autocomplete#disable)

### Reverse

Put your switches on the opposite side by using the `Reverse` parameter.

<img src="https://i.imgur.com/Eo7kY1f.png" alt="Blazor Bootstrap: Switch Component - Reverse" />

```cshtml {} showLineNumbers
<Switch @bind-Value="agree" Label="Reverse switch checkbox input" Reverse="true" />
```

```cs {} showLineNumbers
@code {
    bool agree;
}
```

[See demo here](https://demos.blazorbootstrap.com/form/switch#reverse)

### Events: ValueChanged

This event fired when the `Switch` selection changed.

<img src="https://i.imgur.com/MZe3u1z.png" alt="Blazor Bootstrap: Switch Component - Events: ValueChanged" />

```cshtml {} showLineNumbers
<Switch Value="agree" Label="Default switch checkbox input" ValueExpression="() => agree" ValueChanged="SwitchChanged" />
<div class="mt-3">@displaySwitchStatus</div>
```

```cs {} showLineNumbers
@code {
    private bool agree;
    private string displaySwitchStatus;

    private void SwitchChanged(bool value)
    {
        agree = value; // this is mandatory
        displaySwitchStatus = $"Switch Status: {value}, changed at {DateTime.Now.ToLocalTime()}.";
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/form/switch#events-value-changed)