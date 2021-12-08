---
sidebar_label: Toasts
sidebar_position: 5
---

# Toasts

Documentation and examples for BlazorBootstrap Toasts.

## Overview

Things to know when using the toast component:

- Toasts will automatically hide if you do not specify AutoHide = false.

## Examples:

### Toast

<img src="https://i.imgur.com/8tcFedx.jpg" alt="BlazorBootstrap: Toasts Component Example" />

```cshtml
<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" AutoHide="false">
</Toasts>

<div class="mb-3">
    <Button Color="ButtonColor.Primary" @onclick="ShowToast">Show Toast</Button>
</div>
```

```cs {5-12}
List<ToastMessage> messages = new List<ToastMessage>();

private void ShowToast()
{
    messages?.Add(new ToastMessage
        {
            Type = ToastType.Warning,
            IconName = IconName.Alarm,
            Title = "Blazor Bootstrap",
            HelpText = $"{DateTime.Now}",
            Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",
        });
}
```

### Sample Toast

<img src="https://i.imgur.com/VRglJqU.jpg" alt="BlazorBootstrap: Toasts Component Example" />

<img src="https://i.imgur.com/SUB90wN.jpg" alt="BlazorBootstrap: Toasts Component Example" />

```cshtml
<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" AutoHide="false">
</Toasts>

<div class="mb-3">
    <Button Color="ButtonColor.Primary" @onclick="ShowSimpleToast">Show Simple Toast</Button>
</div>
```

```cs {5-10}
List<ToastMessage> messages = new List<ToastMessage>();

private void ShowSimpleToast()
{
    messages?.Add(new ToastMessage
        {
            Type = (ToastType)int.Parse(ToastTypeAsString),
            IconName = IconName.Alarm,
            Message = $"Hello, world! This is a simple toast message. DateTime: {DateTime.Now}",
        });
}
```
