---
sidebar_label: Offcanvas
sidebar_position: 4
---

# Offcanvas

Documentation and examples for BlazorBootstrap Offcanvas.

## Examples

### Offcanvas

```cshtml
<Button Color="ButtonColor.Primary" @onclick="(async () => { await ShowOffcanvasAsync(); })">Show Offcanvas</Button>

<Offcanvas @ref="offcanvas">
  ... design your header and body
</Offcanvas>
```

### Methods

| Method | Description |
|--|--|
| Show | Shows an offcanvas element. Returns to the caller before the offcanvas element has actually been shown (i.e. before the <code>Shown</code> event occurs). |
| Hide | Hides an offcanvas element. Returns to the caller before the offcanvas element has actually been hidden (i.e. before the <code>Hidden</code> event occurs). |

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

| Event | Description |
|--|--|
| Showing | This event fires immediately when the show instance method is called. |
| Shown | This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete). |
| Hiding | This event is fired immediately when the hide method has been called. |
| Hidden | This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete). |

```cshtml
<Offcanvas @ref="offcanvas"
           Showing="OnOffcanvasShowingAsync"
           Shown="OnOffcanvasShownAsync"
           Hiding="OnOffcanvasHidingAsync"
           Hidden="OnOffcanvasHiddenAsync">

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

### Full Example

```cshtml
@using BlazorBootstrap
@page "/offcanvas"

<h4 class="my-4">Offcanvas Examples</h4>
<Button Color="ButtonColor.Primary" @onclick="(async () => { await ShowOffcanvasAsync(); })">Show Offcanvas</Button>

<Offcanvas @ref="offcanvas"
           Showing="OnOffcanvasShowingAsync"
           Shown="OnOffcanvasShownAsync"
           Hiding="OnOffcanvasHidingAsync"
           Hidden="OnOffcanvasHiddenAsync">

    <div class="offcanvas-header">
        <h5 class="offcanvas-title" id="offcanvasExampleLabel">Offcanvas</h5>
        <button type="button" class="btn-close text-reset" data-bs-dismiss="offcanvas" aria-label="Close"></button>
    </div>
    <div class="offcanvas-body">
        <div>
            Some text as placeholder. In real life you can have the elements you have chosen. Like, text, images, lists, etc.
        </div>
        <div class="dropdown mt-3">
            <button class="btn btn-secondary dropdown-toggle" type="button" id="dropdownMenuButton" data-bs-toggle="dropdown">
                Dropdown button
            </button>
            <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton">
                <li><a class="dropdown-item" href="#">Action</a></li>
                <li><a class="dropdown-item" href="#">Another action</a></li>
                <li><a class="dropdown-item" href="#">Something else here</a></li>
            </ul>
        </div>
        <div class="mt-3">
            <Button Color="ButtonColor.Primary" @onclick="(async () => { await HideOffcanvasAsync(); })">Hide Offcanvas</Button>
        </div>
    </div>

</Offcanvas>
```

```cs
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

### Screenshot

<img src="https://i.imgur.com/J1mcowH.jpg" alt="BlazorBootstrap: Offcanvas Component Example" />