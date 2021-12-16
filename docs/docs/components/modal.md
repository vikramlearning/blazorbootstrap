---
sidebar_label: Modal
sidebar_position: 4
---

# Modal

Documentation and examples for BlazorBootstrap Modal.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside the alert. | | |
| IsScrollable | bool | Allows modal body scroll. | | false |
| IsVerticallyCentered | bool | Shows the modal vertically in the center. | | false |

## Methods

| Name | Description |
|--|--|
| ShowAsync | Opens a modal. |
| HideAsync | Hides a modal. |

:::danger Asynchronous methods and transitions

All API methods are **asynchronous** and start a **transition**. They return to the caller as soon as the transition is started but **before it ends**. In addition, a method call on a **transitioning component will be ignored**.

:::

## Callback Events

| Event | Description | 
|--|--|
| OnShowing | This event fires immediately when the show instance method is called. |
| OnShown | This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete). |
| OnHiding | This event is fired immediately when the hide method has been called. |
| OnHidden | This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete). |
| OnHidePrevented | This event is fired when the modal is shown, its backdrop is static and a click outside the modal or an escape key press is performed with the keyboard option or data-bs-keyboard set to false. |

## Examples

### Modal

<img src="https://i.imgur.com/kVDJBMx.jpg" alt="Blazor Bootstrap: Modal Component" />

```cshtml
<Button Color="ButtonColor.Primary" @onclick="(async () => { await ShowModalAsync(); })">Show Modal</Button>

<Modal @ref="modal">
  ... design your header and body
</Modal>
```

### Methods

```cs {7,12}
@code {

    private Modal modal;

    private async Task ShowModalAsync()
    {
        await modal?.ShowAsync();
    }

    private async Task HideModalAsync()
    {
        await modal?.HideAsync();
    }
}
```

### Callback Events

```cshtml
<Modal @ref="modal"
       Showing="OnModalShowingAsync"
       Shown="OnModalShownAsync"
       Hiding="OnModalHidingAsync"
       Hidden="OnModalHiddenAsync"
       HidePrevented="OnModalHidePreventedAsync">
  ... design your header and body
</Modal>
```

```cs
@code {

    private Modal modal;

    private async Task OnModalShowingAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Showing"); });
    }

    private async Task OnModalShownAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Show"); });
    }

    private async Task OnModalHidingAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Hiding"); });
    }

    private async Task OnModalHiddenAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Hide"); });
    }

    private async Task OnModalHidePreventedAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Hide Prevented"); });
    }
}
```

## Articles

- [Blazor Bootstrap: Modal Component Examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-modal-component-examples/88/154)