---
sidebar_label: Modal
sidebar_position: 6
---

# Modal

Use BlazorBootstrap's modal component to add dialogs to your site for lightboxes, user notifications, or completely custom content.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| BodyCssClass | string | Additional body CSS class. | | |
| BodyTemplate | RenderFragment | Body template. | ✔️ | |
| ChildContent | RenderFragment | Specifies the content to be rendered inside the alert. | | |
| CloseOnEscape | bool | Indicates whether the modal closes when escape key is pressed. | | true |
| DialogCssClass | string | Additional CSS class for the dialog (div.modal-dialog element). | | |
| FooterCssClass | string | Footer css class. | | |
| FooterTemplate | RenderFragment | Footer template. | ✔️ | |
| Fullscreen | `ModalFullscreen` | Fullscreen behavior of the modal. | ✔️ | `ModalFullscreen.Disabled` |
| HeaderTemplate | RenderFragment | Header template. | ✔️ | |
| HeaderCssClass | string | Additional header CSS class. | | |
| IsScrollable | bool | Allows modal body scroll. | | false |
| IsVerticallyCentered | bool | Shows the modal vertically in the center. | | false |
| Size | `ModalSize` | Size of the modal. | ✔️ | `ModalSize.Regular` |
| ShowCloseButton | bool | Indicates whether the modal shows close button in header. | ✔️ | true |
| Title | string | | ✔️ | |
| UseStaticBackdrop | bool | Indicates whether the modal uses a static backdrop. | | false |

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

## How it works

Before getting started with BlazorBootstrap's modal component, be sure to read the following as our menu options have recently changed.

- Modals are built with HTML, CSS, and JavaScript. They're positioned over everything else in the document and remove scroll from the `<body>` so that modal content scrolls instead.
- Clicking on the modal "backdrop" will automatically close the modal.
- BlazorBootstrap only supports one modal window at a time. Nested modals aren't supported as we believe them to be poor user experiences.

## Examples

### Modal

Clicking the **Show Modal** button below, the modal will slide down and fade in from the top of the page.

<img src="https://i.imgur.com/aWbURjD.jpg" alt="Modal" />

```cshtml
<Modal @ref="modal" Title="Modal title">
    <BodyTemplate>
        Modal body text goes here.
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="OnHideModalClick">Close</Button>
        <Button Color="ButtonColor.Primary">Save changes</Button>
    </FooterTemplate>
</Modal>

<Button Color="ButtonColor.Primary" @onclick="OnShowModalClick">Show Modal</Button>
```
```cs {2,6,11}
@code {
    private Modal modal;

    private async Task OnShowModalClick()
    {
        await modal?.ShowAsync();
    }

    private async Task OnHideModalClick()
    {
        await modal?.HideAsync();
    }
}
```

### Static backdrop

When `UseStaticBackdrop` is set to `true`, the modal will not close when clicking outside it. Click the button below to try it.

<img src="https://i.imgur.com/NeSfMIn.jpg" alt=" Modal with static backdrop " />

```cshtml {1}
<Modal @ref="modal" title="Modal title" UseStaticBackdrop="true">
    <BodyTemplate>
        I will not close if you click outside me. Don't even try to press escape key.
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="OnHideModalClick">Close</Button>
        <Button Color="ButtonColor.Primary">Understood</Button>
    </FooterTemplate>
</Modal>

<Button Color="ButtonColor.Primary" @onclick="OnShowModalClick">Launch static backdrop modal</Button>
```
```cs
@code {
    private Modal modal;

    private async Task OnShowModalClick()
    {
        await modal?.ShowAsync();
    }

    private async Task OnHideModalClick()
    {
        await modal?.HideAsync();
    }
}
```

### Scrolling long content

When modals become too long for the user’s viewport or device, they scroll independent of the page itself. Try the demo below to see what we mean.

<img src="https://i.imgur.com/7lrxeON.jpg" alt="Modal - Scrolling long content" />

```cshtml {1}
<Modal @ref="modal" title="Modal title" IsScrollable="true">
    <BodyTemplate>
        <p style="margin-bottom: 100vh;">This is some placeholder content to show the scrolling behavior for modals. Instead of repeating the text the modal, we use an inline style set a minimum height, thereby extending the length of the overall modal and demonstrating the overflow scrolling. When content becomes longer than the height of the viewport, scrolling will move the modal as needed.</p>
        <p>This content should appear at the bottom after you scroll.</p>
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="OnHideModalClick">Close</Button>
        <Button Color="ButtonColor.Primary">Save changes</Button>
    </FooterTemplate>
</Modal>

<Button Color="ButtonColor.Primary" @onclick="OnShowModalClick">Launch demo modal</Button>
```
```cs
@code {
    private Modal modal;

    private async Task OnShowModalClick()
    {
        await modal?.ShowAsync();
    }

    private async Task OnHideModalClick()
    {
        await modal?.HideAsync();
    }
}
```

### Vertically centered

Add `IsVerticallyCentered="true"` to vertically center the modal.

<img src="https://i.imgur.com/tLiaEs6.jpg" alt="Modal - Vertically centered" />

```cshtml {1}
<Modal @ref="modal" title="Modal title" IsVerticallyCentered="true">
    <BodyTemplate>
        This is a vertically centered modal.
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="OnHideModalClick">Close</Button>
        <Button Color="ButtonColor.Primary">Save changes</Button>
    </FooterTemplate>
</Modal>

<Button Color="ButtonColor.Primary" @onclick="OnShowModalClick">Vertically centered modal</Button>
```
```cs
@code {
    private Modal modal;

    private async Task OnShowModalClick()
    {
        await modal?.ShowAsync();
    }

    private async Task OnHideModalClick()
    {
        await modal?.HideAsync();
    }
}
```

### Vertically centered and scrollable

<img src="https://i.imgur.com/n0m4Fhq.jpg" alt="Modal - Vertically centered and scrollable" />

```cshtml {1}
<Modal @ref="modal" title="Modal title" IsVerticallyCentered="true" IsScrollable="true">
    <BodyTemplate>
        <p style="margin-bottom: 100vh;">This is some placeholder content to show the scrolling behavior for modals. Instead of repeating the text the modal, we use an inline style set a minimum height, thereby extending the length of the overall modal and demonstrating the overflow scrolling. When content becomes longer than the height of the viewport, scrolling will move the modal as needed.</p>
        <p>This content should appear at the bottom after you scroll.</p>
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="OnHideModalClick">Close</Button>
        <Button Color="ButtonColor.Primary">Save changes</Button>
    </FooterTemplate>
</Modal>

<Button Color="ButtonColor.Primary" @onclick="OnShowModalClick">Vertically centered scrollable modal</Button>
```
```cs
@code {
    private Modal modal;

    private async Task OnShowModalClick()
    {
        await modal?.ShowAsync();
    }

    private async Task OnHideModalClick()
    {
        await modal?.HideAsync();
    }
}
```

### Optional sizes

Modals have three optional sizes. These sizes kick in at certain breakpoints to avoid horizontal scrollbars on narrower viewports.

<img src="https://i.imgur.com/5vKfJQC.jpg" alt="Modal - Optional sizes" />

```cshtml {1,4,7}
<Modal @ref="xlModal" title="Extra large modal" Size="ModalSize.ExtraLarge">
    <BodyTemplate>...</BodyTemplate>
</Modal>
<Modal @ref="lgModal" title="Large modal" Size="ModalSize.Large">
    <BodyTemplate>...</BodyTemplate>
</Modal>
<Modal @ref="smModal" title="Small modal" Size="ModalSize.Small">
    <BodyTemplate>...</BodyTemplate>
</Modal>

<Button Color="ButtonColor.Primary" @onclick="() => xlModal?.ShowAsync()">Extra large modal</Button>
<Button Color="ButtonColor.Primary" @onclick="() => lgModal?.ShowAsync()">Large modal</Button>
<Button Color="ButtonColor.Primary" @onclick="() => smModal?.ShowAsync()">Small modal</Button>
```
```cs
@code {
    private Modal xlModal;
    private Modal lgModal;
    private Modal smModal;
}
```

### Fullscreen Modal

<img src="https://i.imgur.com/3dFUzMz.jpg" alt="Modal - Fullscreen Modal" />

```cshtml {1,4,7,10,13,16}
<Modal @ref="modal" title="Full screen" Fullscreen="ModalFullscreen.Always">
    <BodyTemplate>...</BodyTemplate>
</Modal>
<Modal @ref="smModal" title="Full screen below sm" Fullscreen="ModalFullscreen.SmallDown">
    <BodyTemplate>...</BodyTemplate>
</Modal>
<Modal @ref="mdModal" title="Full screen below md" Fullscreen="ModalFullscreen.MediumDown">
    <BodyTemplate>...</BodyTemplate>
</Modal>
<Modal @ref="lgModal" title="Full screen below lg" Fullscreen="ModalFullscreen.LargeDown">
    <BodyTemplate>...</BodyTemplate>
</Modal>
<Modal @ref="xlModal" title="Full screen below xl" Fullscreen="ModalFullscreen.ExtraLargeDown">
    <BodyTemplate>...</BodyTemplate>
</Modal>
<Modal @ref="xxlModal" title="Full screen below xxl" Fullscreen="ModalFullscreen.ExtraExtraLargeDown">
    <BodyTemplate>...</BodyTemplate>
</Modal>

<Button Color="ButtonColor.Primary" @onclick="() => modal?.ShowAsync()">Full screen</Button>
<Button Color="ButtonColor.Primary" @onclick="() => smModal?.ShowAsync()">Full screen below sm</Button>
<Button Color="ButtonColor.Primary" @onclick="() => mdModal?.ShowAsync()">Full screen below md</Button>
<Button Color="ButtonColor.Primary" @onclick="() => lgModal?.ShowAsync()">Full screen below lg</Button>
<Button Color="ButtonColor.Primary" @onclick="() => xlModal?.ShowAsync()">Full screen below xl</Button>
<Button Color="ButtonColor.Primary" @onclick="() => xxlModal?.ShowAsync()">Full screen below xxl</Button>
```
```cs
@code {
    private Modal modal;
    private Modal smModal;
    private Modal mdModal;
    private Modal lgModal;
    private Modal xlModal;
    private Modal xxlModal;
}
```

### Events

BlazorBootstrap's modal class exposes a few events for hooking into modal functionality.

```cshtml {3-7}
<Modal @ref="modal"
       title="Modal title"
       OnShowing="OnModalShowingAsync"
       OnShown="OnModalShownAsync"
       OnHiding="OnModalHidingAsync"
       OnHidden="OnModalHiddenAsync"
       OnHidePrevented="OnModalHidePreventedAsync">

    <BodyTemplate>
        Modal body text goes here.
    </BodyTemplate>

    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="OnHideModalClick">Close</Button>
        <Button Color="ButtonColor.Primary">Save changes</Button>
    </FooterTemplate>

</Modal>

<Button Color="ButtonColor.Primary" @onclick="OnShowModalClick">Show Modal</Button>
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