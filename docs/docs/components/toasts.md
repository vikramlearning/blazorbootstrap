---
title: Blazor Toasts Component
description: Push notifications to your visitors with a toast, a lightweight and easily customizable BlazorBootstrap toast message.
image: https://getblazorbootstrap.com/img/logo.svg

sidebar_label: Toasts
sidebar_position: 16
---

# Blazor Toasts

Push notifications to your visitors with a toast, a lightweight and easily customizable BlazorBootstrap toast message.

Toasts are lightweight notifications designed to mimic the push notifications that have been popularized by mobile and desktop operating systems. They’re built with flexbox, so they’re easy to align and position.

<img src="https://i.imgur.com/OCQUchu.png" alt="Blazor Bootstrap: Blazor Toasts Component" />

**Things to know when using the toasts component:**

- Toasts will not hide automatically if you do not specify `AutoHide="true"`.

## Toasts Parameters

| Name | Type | Descritpion | Required | Default |
|:--|:--|:--|:--|:--|
| AutoHide | bool | Auto hide the toast. | | `false` |
| Delay | int | Delay hiding the toast (milli seconds). | | 5000 |
| Messages | `List<ToastMessage>` | List of all the toasts. | ✔️ | |
| Placement | `ToastsPlacement` | Specifies the toasts placement. | | `ToastsPlacement.TopRight` |
| ShowCloseButton | bool | Show close button. | | `true` |
| StackLength | int | Specifies the toast container maximum capacity. | | 5 |

## ToastMessage Properties

| Name | Type | Description | Required | Default |
|:--|:--|:--|:--|:--|
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

To encourage extensible and predictable toasts, we recommend a header and body.

Toasts are as flexible as you need and have very little required markup. At a minimum, we require a single element to contain your "toasted" content and strongly encourage a dismiss button.

<img src="https://i.imgur.com/OCQUchu.png" alt="Blazor Bootstrap: Blazor Toasts Component - Example" />

```cshtml {1} showLineNumbers
<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" />

<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>
<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>
<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>
<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>
<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>
<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>
```

```cs {7-13} showLineNumbers
@code {
    List<ToastMessage> messages = new List<ToastMessage>();

    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));

    private ToastMessage CreateToastMessage(ToastType toastType)
    => new ToastMessage
        {
            Type = toastType,
            Title = "Blazor Bootstrap",
            HelpText = $"{DateTime.Now}",
            Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",
        };
}
```

[See toasts demo here.](https://demos.getblazorbootstrap.com/toasts#examples)

### Toast without title

Customize your toasts by removing sub-components, tweaking them with utilities.

Here we've created a simple toast. You can create different toast color schemes with the `Color` parameter.

<div>
<img src="https://i.imgur.com/VRglJqU.jpg" alt="Blazor Bootstrap: Toasts Component - Example" />
</div>

<div>
<img src="https://i.imgur.com/SUB90wN.jpg" alt="Blazor Bootstrap: Toasts Component - Example" />
</div>

```cshtml {1} showLineNumbers
<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" />

<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>
<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>
<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>
<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>
<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>
<Button Color="ButtonColor.Light" @onclick="() => ShowMessage(ToastType.Light)">Light Toast</Button>
<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>
```

```cs {7-11} showLineNumbers
@code {
    List<ToastMessage> messages = new List<ToastMessage>();

    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));

    private ToastMessage CreateToastMessage(ToastType toastType)
    => new ToastMessage
        {
            Type = toastType,
            Message = $"Hello, world! This is a simple toast message. DateTime: {DateTime.Now}",
        };
}
```

[See toasts without title demo here.](https://demos.getblazorbootstrap.com/toasts#toast-without-title)

### Auto hide

Add `AutoHide="true"` parameter to hide the Blazor Toasts after the delay. The default delay is 5000 milliseconds, be sure to update the delay timeout so that users have enough time to read the toast.

<img src="https://i.imgur.com/W1YkmJH.png" alt="Blazor Bootstrap: Blazor Toasts Component - Auto hide" />

```cshtml {1} showLineNumbers
<Toasts class="p-3" Messages="messages" AutoHide="true" Delay="6000" Placement="ToastsPlacement.TopRight" />

<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>
<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>
<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>
<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>
<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>
<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>
```

```cs showLineNumbers
@code {
    List<ToastMessage> messages = new List<ToastMessage>();

    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));

    private ToastMessage CreateToastMessage(ToastType toastType)
    => new ToastMessage
        {
            Type = toastType,
            Title = "Blazor Bootstrap",
            HelpText = $"{DateTime.Now}",
            Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",
        };
}
```

[See auto hide toasts demo here.](https://demos.getblazorbootstrap.com/toasts#auto-hide)

### Placement

Change the Blazor Toasts placement according to your need. The default placement will be top right corner. Use the `ToastsPlacement` parameter to update the Blazor Toasts placement.

```cshtml {1} showLineNumbers
<Toasts class="p-3" Messages="messages" Placement="@toastsPlacement" />

<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.TopLeft)">Top Left</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.TopCenter)">Top Center</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.TopRight)">Top Right</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.MiddleLeft)">Middle Left</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.MiddleCenter)">Middle Center</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.MiddleRight)">Middle Right</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.BottomLeft)">Bottom Left</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.BottomCenter)">Bottom Center</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.BottomRight)">Bottom Right</Button>
```

```cs showLineNumbers
@code {
    ToastsPlacement toastsPlacement = ToastsPlacement.TopRight;
    List<ToastMessage> messages = new();

    private void ChangePlacement(ToastsPlacement placement)
    {
        if (!messages.Any())
        {
            messages.Add(
                new ToastMessage()
                    {
                        Type = ToastType.Success,
                        Title = "Blazor Bootstrap",
                        HelpText = $"{DateTime.Now}",
                        Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",
                    });
        }
        toastsPlacement = placement;
    }
}
```

[See demo here.](https://demos.getblazorbootstrap.com/toasts#placement)

### Stack Length

Blazor Toasts component shows a maximum of 5 toasts by default. If you add a new toast to the existing list, the first toast gets deleted like FIFO (First In First Out). Change the maximum capacity according to your need by using the **StackLength** parameter.

In the below example, StackLength is set to 3. It shows a maximum of 3 toast messages at any time.

```cshtml {1} showLineNumbers
<Toasts class="p-3" Messages="messages" AutoHide="true" StackLength="3" Placement="ToastsPlacement.TopRight" />

<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>
<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>
<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>
<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>
<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>
<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>
<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>
```

```cs showLineNumbers
@code {
    List<ToastMessage> messages = new List<ToastMessage>();

    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));

    private ToastMessage CreateToastMessage(ToastType toastType)
    => new ToastMessage
        {
            Type = toastType,
            Title = "Blazor Bootstrap",
            HelpText = $"{DateTime.Now}",
            Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",
        };
}
```

[See demo here.](https://demos.getblazorbootstrap.com/toasts#stack-length)