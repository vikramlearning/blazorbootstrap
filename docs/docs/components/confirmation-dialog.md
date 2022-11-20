---
title: Blazor Confirm Dialog Component
description: Use Blazor Bootstrap confirm dialog component if you want the user to verify or accept something.
image: https://i.imgur.com/FisZXwK.png

sidebar_label: Confirm Dialog
sidebar_position: 7
---

# Blazor Confirm Dialog

Use Blazor Bootstrap confirm dialog component if you want the user to verify or accept something.

<img src="https://i.imgur.com/FisZXwK.png" alt="Blazor Bootstrap: Confirm Dialog component" />

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| Dismissable | bool | Adds a dismissable close button to the confirm dialog. | | true |
| IsScrollable | bool | Allows confirm dialog body to be scrollable. | | false |
| IsVerticallyCentered | bool | Shows the confirm dialog vertically in the center of the page. | | false |
| Message1 | string | Gets or sets the Message1 of the confirmation dialog. | ✔️ | |
| Message2 | string | Gets or sets the Message2 of the confirmation dialog. This is optional. | | |
| NoButtonText | string | Gets or sets the `No` button text. | | No |
| NoButtonColor | `ButtonColor` | Gets or sets the 'No' button color. | | `ButtonColor.Secondary` |
| Title | string | Gets or sets the title of the confirm dialog. | | |
| TitleBackgroundColor | `BackgroundColor` | Gets or sets the background color of the confirm dialog title. | | `BackgroundColor.None` |
| YesButtonText | string | Gets or sets the `Yes` button text. | | Yes |
| YesButtonColor | `ButtonColor` | Gets or sets the 'Yes' button color. | | `ButtonColor.Primary` |

## Methods

| Name | Description | Version |
|--|--|--|
| Show() | Shows confirm dialog. | |
| Show(string title, string message1, string message2) | Shows confirm dialog. | >= 1.0.1 |
| Hide() | Hides confirm dialog. | |

## Callback Events

| Name | Description |
|--|--|
| OnYes | This event is fired when the user confirms 'Yes'. |
| OnNo | This event is fired when the user confirms 'No'. |

## Examples

### Confirm Dialog

<img src="https://i.imgur.com/FisZXwK.png" alt="Blazor Bootstrap: Confirm Dialog Component - Examples" />

```cshtml showLineNumbers
<ConfirmDialog @ref="dialog"
               Title="Are you sure you want to save this?"
               Message1="This will save the details enterd in the form."
               Message2="Do you want to proceed?"
               OnYes="OnYes"
               OnNo="OnNo" />

<Button Color="ButtonColor.Primary" @onclick="ShowSaveConfirmation"> Save Employee </Button>

<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" AutoHide="true" ShowCloseButton="true" />
```

```cs {2,8,11,20} showLineNumbers
@code {
    private ConfirmDialog dialog;

    List<ToastMessage> messages = new List<ToastMessage>();

    private void ShowSaveConfirmation()
    {
        dialog.Show();
    }

    private void OnYes()
    {
        messages.Add(new ToastMessage
            {
                Type = ToastType.Success,
                Message = $"'Yes' confirmation was received from the user. DateTime: {DateTime.Now}",
            });
    }

    private void OnNo()
    {
        messages.Add(new ToastMessage
            {
                Type = ToastType.Secondary,
                Message = $"'No' confirmation was received from the user. DateTime: {DateTime.Now}",
            });
    }
}
```
[See Confirm Dialog demo here.](https://demos.getblazorbootstrap.com/confirm-dialog#examples)

### Dynamic Title and Message

Dynamically change the confirmation dialog title (Title) and message (Message1 and Message2) by calling show(title, message1, message2). 
In the example below, Save Employee and Delete Employee both will display different titles and messages.

<img src="https://i.imgur.com/FlM6ZVv.png" alt="Blazor Bootstrap: Confirm Dialog Component - Dynamic Title and Message" />

```cshtml showLineNumbers
<ConfirmDialog @ref="dialog" OnYes="OnYes" OnNo="OnNo" />

<Button Color="ButtonColor.Primary" @onclick="ShowSaveConfirmation"> Save Employee </Button>
<Button Color="ButtonColor.Danger" @onclick="ShowDeleteConfirmation"> Delete Employee </Button>

<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" AutoHide="true" ShowCloseButton="true" />
```

```cs {2,8-11,16-19,22,32} showLineNumbers
@code {
    private ConfirmDialog dialog;

    List<ToastMessage> messages = new List<ToastMessage>();

    private void ShowSaveConfirmation()
    {
        dialog.Show(
            title: "Are you sure you want to save this?",
            message1: "This will save the details enterd in the form.",
            message2: "Do you want to proceed?");
    }

    private void ShowDeleteConfirmation()
    {
        dialog.Show(
            title: "Are you sure you want to delete this?",
            message1: "This will delete the record. Once deleted can not be rolled back.",
            message2: "Do you want to proceed?");
    }

    private void OnYes()
    {
        // TODO: call Service/API
        messages.Add(new ToastMessage
            {
                Type = ToastType.Success,
                Message = $"'Yes' confirmation was received from the user. DateTime: {DateTime.Now}",
            });
    }

    private void OnNo()
    {
        messages.Add(new ToastMessage
            {
                Type = ToastType.Secondary,
                Message = $"'No' confirmation was received from the user. DateTime: {DateTime.Now}",
            });
    }
}
```