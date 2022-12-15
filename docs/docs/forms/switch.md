---
title: Blazor Switch Component
description: Use the Blazor Bootstrap `Switch` component to show the consistent cross-browser and cross-device custom checkboxes.
image: https://i.imgur.com/ALKzreq.png

sidebar_label: Switch
sidebar_position: 4
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

[See demo here](https://demos.getblazorbootstrap.com/form/switch#basic-usage)

### Disable

Use the `Disabled` parameter to disable the `Switch`. Also, use <b>Enable()</b> and <b>Disable()</b> methods to enable and disable the `Switch`.

<img src="https://i.imgur.com/85KPLgp.png" alt="Blazor Bootstrap: Switch Component - Disable" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <Switch @ref="_switch" @bind-Value="agree" Disabled="true" Label="Disabled switch checkbox input" />
</div>

<Button Color="ButtonColor.Primary" Size="Size.Small" @onclick="enableSwitch"> Enable </Button>
<Button Color="ButtonColor.Secondary" Size="Size.Small" @onclick="disableSwitch"> Disable </Button>
```

```cs {} showLineNumbers
@code {
    private Switch _switch;
    private bool agree = true;

    private void enableSwitch() => _switch.Enable();

    private void disableSwitch() => _switch.Disable();
}
```

[See demo here](https://demos.getblazorbootstrap.com/form/switch#disable)

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

[See demo here](https://demos.getblazorbootstrap.com/form/switch#reverse)

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

[See demo here](https://demos.getblazorbootstrap.com/form/switch#events-value-changed)