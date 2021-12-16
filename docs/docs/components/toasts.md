---
sidebar_label: Toasts
sidebar_position: 7
---

# Toasts

Documentation and examples for BlazorBootstrap Toasts.

Things to know when using the toasts component:

- Toasts will automatically hide if you do not specify `AutoHide = false`.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| AutoHide | bool | Auto hide the toast. | | `true` |
| Delay | int | Delay hiding the toast (milli seconds). | | 5000 |
| Messages | `List<ToastMessage>` | List of all the toasts. | ✔️ | |
| Placement | `ToastsPlacement` | Specifies the toasts placement. | | `ToastsPlacement.TopRight` |
| StackLength | int | Specifies the toast container maximum capacity. | | 5 |

### ToastMessage Properties

| Name | Type | Description | Required | Default |
|--|--|--|--|--|
| Type | `ToastType` | Gets or sets the type of the toast. | ✔️ | |
| ImageSource | string | Gets or sets the source of the image. | | |
| IconName | `IconName` | Gets or sets the bootstarp icon name. | | |
| Id | Guid | Gets the toast id. | | |
| CustomIconName | string | Gets or sets the custom icon name. | | |
| Title | string | Gets or sets the toast''s message title. | | |
| HelpText | string | Gets or sets the help text. | | |
| Message | string | Gets or sets the toast message. | ✔️ | |

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

### Simple Toast

<div>
<img src="https://i.imgur.com/VRglJqU.jpg" alt="BlazorBootstrap: Toasts Component Example" />
</div>

<div>
<img src="https://i.imgur.com/SUB90wN.jpg" alt="BlazorBootstrap: Toasts Component Example" />
</div>

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
            Type = ToastType.Success,
            IconName = IconName.Alarm,
            Message = $"Hello, world! This is a simple toast message. DateTime: {DateTime.Now}",
        });
}
```

## Articles

- [Blazor Bootstrap: Toasts Component Examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-toasts-component-examples/88/155)