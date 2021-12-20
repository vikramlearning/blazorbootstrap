---
sidebar_label: Confirm Dialog
sidebar_position: 5
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

```cshtml
<Button Color="ButtonColor.Primary" @onclick="ShowConfirmationModal">Show confirmation</Button>

<ConfirmDialog @ref="dialog"
               Title="Are you sure you want to save this?"
               Message1="This will save the details enterd in the form."
               Message2="Do you want to proceed?"
               OnYes="OnYesAsync"
               OnNo="OnNoAsync"></ConfirmDialog>
```

```cs {2,6,9,14}
@code {
    private ConfirmDialog dialog;

    private void ShowConfirmationModal()
    {
        dialog.Show();
    }

    private async Task OnYesAsync()
    {
        Console.WriteLine("OnYesAsync");
    }

    private async Task OnNoAsync()
    {
        Console.WriteLine("OnNoAsync");
    }
}
```

## Articles

- [Blazor Bootstrap: Modal Component Examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-confirm-dialog-component-examples/88/161)