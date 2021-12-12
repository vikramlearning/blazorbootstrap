---
sidebar_label: Offcanvas
sidebar_position: 4
---

# Offcanvas

Documentation and examples for BlazorBootstrap Offcanvas.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| Placement | `Placement` | Gets or sets the offcanvas placement. By default, offcanvas is placed on the right of the viewport. | | `Placement.End` |

## Methods

| Method | Description |
|--|--|
| ShowAsync | Shows an offcanvas element. Returns to the caller before the offcanvas element has actually been shown (i.e. before the `OnShown` event occurs). |
| HideAsync | Hides an offcanvas element. Returns to the caller before the offcanvas element has actually been hidden (i.e. before the `OnHidden` event occurs). |

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

## Examples

### Offcanvas

<img src="https://i.imgur.com/J1mcowH.jpg" alt="BlazorBootstrap: Offcanvas Component" />

```cshtml
<Button Color="ButtonColor.Primary" @onclick="(async () => { await ShowOffcanvasAsync(); })">Show Offcanvas</Button>

<Offcanvas @ref="offcanvas">
  ... design your header and body
</Offcanvas>

### Methods

```cs {6,11}
@code {
    private Offcanvas offcanvas;

    private async Task ShowOffcanvasAsync()
    {
        await offcanvas?.ShowAsync();
    }

    private async Task HideOffcanvasAsync()
    {
        await offcanvas?.HideAsync();
    }
}
```

### Callback Events

```cshtml
<Offcanvas @ref="offcanvas"
           OnShowing="OnOffcanvasShowingAsync"
           OnShown="OnOffcanvasShownAsync"
           OnHiding="OnOffcanvasHidingAsync"
           OnHidden="OnOffcanvasHiddenAsync">

    ... add offcanvas header and body

</Offcanvas>
```

```cs
@code {
    private Offcanvas offcanvas;

    private async Task OnOffcanvasShowingAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Showing"); });
    }

    private async Task OnOffcanvasShownAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Shown"); });
    }

    private async Task OnOffcanvasHidingAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Hiding"); });
    }

    private async Task OnOffcanvasHiddenAsync()
    {
        await Task.Run(() => { Console.WriteLine("Event: Hidden"); });
    }
}
```

## Articles

- [Blazor Bootstrap: Offcanvas examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-offcanvas-examples/88/153)