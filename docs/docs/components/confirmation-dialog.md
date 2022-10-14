---
title: Blazor Confirm Dialog Component
description: Use BlazorBootstrap's `Confirm Dialog` component if you want the user to verify or accept something.
image: https://getblazorbootstrap.com/img/logo.svg

sidebar_label: Confirm Dialog
sidebar_position: 7
---

# Confirm Dialog

Use BlazorBootstrap's `Confirm Dialog` component if you want the user to verify or accept something.

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

| Name | Description |
|--|--|
| Show | Shows confirm dialog. |
| Hide | Hides confirm dialog. |

## Callback Events

| Name | Description |
|--|--|
| OnYes | This event is fired when the user confirms 'Yes'. |
| OnNo | This event is fired when the user confirms 'No'. |

## Examples

### Confirm Dialog

<img src="https://i.imgur.com/chdLk3D.jpg" alt="Blazor Bootstrap: Modal Component" />

```cshtml showLineNumbers
<ConfirmDialog @ref="dialog"
               Title="Are you sure you want to save this?"
               Message1="This will save the details enterd in the form."
               Message2="Do you want to proceed?"
               OnYes="OnYesAsync"
               OnNo="OnNoAsync"></ConfirmDialog>

<Button Color="ButtonColor.Primary" @onclick="OnShowConfirmationModalClick">Show confirmation</Button>

<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" />
```

```cs {2,8,11,20} showLineNumbers
@code {
    private ConfirmDialog dialog;

    List<ToastMessage> messages = new List<ToastMessage>();

    private void OnShowConfirmationModalClick()
    {
        dialog.Show();
    }

    private async Task OnYesAsync()
    {
        messages?.Add(new ToastMessage
            {
                Type = ToastType.Success,
                Message = $"Details saved successfully. DateTime: {DateTime.Now}",
            });
    }

    private async Task OnNoAsync()
    {
        messages?.Add(new ToastMessage
            {
                Type = ToastType.Secondary,
                Message = $"Action canceled. DateTime: {DateTime.Now}",
            });
    }
}
```
[See Confirm Dialog demo here.](https://demos.getblazorbootstrap.com/confirm-dialog#examples)