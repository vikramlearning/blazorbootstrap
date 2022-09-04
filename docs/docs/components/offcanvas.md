---
sidebar_label: Offcanvas
sidebar_position: 9
---

# Offcanvas

Build hidden sidebars into your project for navigation, shopping carts, and more with BlazorBootstrap's offcanvas component.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| BodyCssClass | string | Additional body CSS class. | | |
| BodyTemplate | RenderFragment | Body content. | ✔️ | |
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| CloseOnEscape | bool | Indicates whether the offcanvas closes when escape key is pressed. | | true |
| FooterCssClass | string | Additional footer CSS class. | | |
| FooterTemplate | RenderFragment | Footer content. | | |
| HeaderCssClass | string | Additional header CSS class. | | |
| HeaderTemplate | RenderFragment | Content for the header. | | |
| IsScrollable | bool | Indicates whether body (page) scrolling is allowed while offcanvas is open. | | false |
| Placement | `Placement` | Gets or sets the offcanvas placement. By default, offcanvas is placed on the right of the viewport. | | `Placement.End` |
| ShowCloseButton | bool | Indicates whether the modal shows close button in header. | | true |
| Size | `OffcanvasSize` | Size of the offcanvas. | | `OffcanvasSize.Regular` |
| Title | string | Text for the title in header. | | |
| UseBackdrop | bool | Indicates whether to apply a backdrop on body while offcanvas is open. | | true |

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

## How it works

Offcanvas is a sidebar component that can be toggled to appear from the left, right, or bottom edge of the viewport.

- Conceptually, they are quite similar to the Modal component, but they are separate components.
- When shown, offcanvas includes a default backdrop that can be clicked to hide the offcanvas.
- Similar to modals, only one offcanvas can be shown at a time.

## Examples

### Offcanvas

Below is an offcanvas example that is shown by default.

<img src="https://i.imgur.com/1vNz5Ci.jpg" alt="Offcanvas - Example" />

```cshtml showLineNumbers
<Button Color="ButtonColor.Primary" @onclick="(async () => { await ShowOffcanvasAsync(); })">Show Offcanvas</Button>

<Offcanvas @ref="offcanvas">
  ... design your header and body
</Offcanvas>
```

```cs {2,6,11} showLineNumbers
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

[See demo here.](https://demos.getblazorbootstrap.com/offcanvas#examples)

:::info 

Default placement for the offcanvas component is right.

:::

### Placement

Try the top, bottom, and left examples out below.

<img src="https://i.imgur.com/dJpJYpX.jpg" alt="Offcanvas - Top Placement" />

```cshtml showLineNumbers
<Offcanvas @ref="offcanvas" Title="Offcanvas top" Placement="Placement.Top">
    <BodyTemplate>...</BodyTemplate>
</Offcanvas>

<Button Color="ButtonColor.Primary" @onclick="OnShowOffcanvasClick">Show top offcanvas</Button>
```

```cs showLineNumbers
@code {
    private Offcanvas offcanvas;

    private async Task OnShowOffcanvasClick()
    {
        await offcanvas?.ShowAsync();
    }
}
```

[See demo here.](https://demos.getblazorbootstrap.com/offcanvas#placement)

### Backdrop

By default backdrop is enabled, you can disable it using the `UseBackground="false"` parameter.

<img src="https://i.imgur.com/ev2Q8ON.jpg" alt="Offcanvas - Backdrop" />

```cshtml showLineNumbers
<Offcanvas @ref="offcanvas" Title="Offcanvas title" UseBackdrop="false">
    <BodyTemplate>
        <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec tincidunt blandit mauris. Aliquam sit amet lorem laoreet, laoreet elit ut, placerat tellus. In mollis ultricies elit, volutpat maximus ipsum sodales interdum. Suspendisse eget tellus mollis, rutrum mauris ac, vulputate enim. Cras porta neque vitae lacinia elementum. Nunc sit amet pulvinar nibh. Curabitur interdum eget odio in tempor. Nulla dictum orci quis ligula auctor fermentum. Pellentesque finibus tellus ac massa convallis malesuada. Nam id pharetra velit, sed eleifend mi. Sed sed justo lorem. Quisque et nulla ut dolor feugiat vestibulum. Nunc at porttitor orci, at dignissim metus. Donec vitae metus vitae felis semper placerat.</p>
        <p>Proin quis congue enim, ut ultricies erat. Nulla facilisi. Fusce pretium, metus eget tempor vehicula, nisl lorem tincidunt metus, consectetur molestie lorem leo vel lectus. Vivamus pellentesque pharetra mattis. Aenean dignissim quam non velit ultrices rutrum. Aliquam lacinia faucibus sapien vel pretium. Nullam libero massa, ultricies id lacinia nec, scelerisque ut felis. Vivamus ac egestas urna, sit amet condimentum odio. Suspendisse ultrices, libero sed interdum pulvinar, lectus felis pellentesque enim, eu finibus magna massa id augue. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis eget tempor libero. Cras ut interdum purus. Donec eu pulvinar urna, ut porttitor purus. Suspendisse sed sodales nunc. Quisque posuere augue sed luctus placerat.</p>
        <p>Morbi ullamcorper risus turpis, et ullamcorper nulla semper vitae. Proin pharetra dolor dui, non condimentum ex fermentum in. Vestibulum pharetra, risus et pulvinar eleifend, nulla tortor blandit risus, ac imperdiet elit massa quis leo. Vivamus urna lacus, luctus eget felis id, eleifend tristique nisl. Sed dignissim mollis ligula vitae laoreet. Vestibulum eget magna nisi. Aenean auctor elit et turpis blandit, eget porttitor felis suscipit. Duis placerat, sapien a sodales tempus, odio orci malesuada neque, ac molestie ipsum nisi vel eros. Integer sem lectus, luctus vitae sapien ut, efficitur aliquam sem. Praesent placerat est eros, vulputate rutrum nunc imperdiet vitae. Fusce sed felis eget purus aliquet convallis eu eget lacus. Sed finibus nec magna et accumsan. Donec vitae tellus eros. Nullam et ex vitae est sagittis malesuada. Vivamus molestie malesuada libero, a consequat magna dapibus pellentesque. Cras molestie tortor vitae congue pretium.</p>
        <p>Pellentesque nec iaculis justo, sed pretium sem. Mauris finibus lacus at mollis fringilla. Etiam auctor in justo ac bibendum. Vestibulum at lorem accumsan, maximus erat suscipit, suscipit ex. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris dignissim id quam sit amet varius. Etiam pretium ultrices dignissim. Cras at tortor hendrerit metus ultrices lobortis at ac est. Suspendisse consectetur pellentesque nunc sit amet scelerisque. Maecenas feugiat nunc laoreet, auctor erat eget, ultricies ex. Aliquam nisi nulla, cursus et ante ut, interdum volutpat leo. Phasellus laoreet aliquam maximus. Vestibulum eu neque porta, consectetur ipsum non, euismod enim. Vestibulum euismod purus elit, ultrices imperdiet nisl porttitor eget. Vivamus eros turpis, tincidunt a vulputate vel, malesuada tristique nulla.</p>
        <p>Vestibulum sed aliquam urna. Ut ullamcorper erat vitae velit mattis commodo. Phasellus dignissim rhoncus dapibus. Quisque congue egestas tellus id finibus. Suspendisse nibh felis, mattis et finibus vel, tempor in lectus. Nullam eget eros dui. Mauris eget vestibulum nibh. Nullam mattis malesuada lorem vel condimentum. Mauris id odio ac est feugiat condimentum.</p>
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="() => offcanvas?.HideAsync()">Close</Button>
    </FooterTemplate>
</Offcanvas>

<Button Color="ButtonColor.Primary" @onclick="OnShowOffcanvasClick">Show offcanvas</Button>
```

```cs showLineNumbers
@code {
    private Offcanvas offcanvas;

    private async Task OnShowOffcanvasClick()
    {
        await offcanvas?.ShowAsync();
    }
}
```

[See demo here.](https://demos.getblazorbootstrap.com/offcanvas#backdrop)

### Sizes

Set the size of the `Offcanvas` with the Size parameter. The default value is `OffcanvasSize.Regular`.

#### Small Offcanvas

<img src="https://i.imgur.com/DJ09ngz.jpg" alt="Offcanvas - Small Size" />

```cshtml showLineNumbers
<Offcanvas @ref="offcanvas" Title="Offcanvas title" Size="OffcanvasSize.Small">
    <BodyTemplate>
        <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec tincidunt blandit mauris. Aliquam sit amet lorem laoreet, laoreet elit ut, placerat tellus. In mollis ultricies elit, volutpat maximus ipsum sodales interdum. Suspendisse eget tellus mollis, rutrum mauris ac, vulputate enim. Cras porta neque vitae lacinia elementum. Nunc sit amet pulvinar nibh. Curabitur interdum eget odio in tempor. Nulla dictum orci quis ligula auctor fermentum. Pellentesque finibus tellus ac massa convallis malesuada. Nam id pharetra velit, sed eleifend mi. Sed sed justo lorem. Quisque et nulla ut dolor feugiat vestibulum. Nunc at porttitor orci, at dignissim metus. Donec vitae metus vitae felis semper placerat.</p>
        <p>Proin quis congue enim, ut ultricies erat. Nulla facilisi. Fusce pretium, metus eget tempor vehicula, nisl lorem tincidunt metus, consectetur molestie lorem leo vel lectus. Vivamus pellentesque pharetra mattis. Aenean dignissim quam non velit ultrices rutrum. Aliquam lacinia faucibus sapien vel pretium. Nullam libero massa, ultricies id lacinia nec, scelerisque ut felis. Vivamus ac egestas urna, sit amet condimentum odio. Suspendisse ultrices, libero sed interdum pulvinar, lectus felis pellentesque enim, eu finibus magna massa id augue. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis eget tempor libero. Cras ut interdum purus. Donec eu pulvinar urna, ut porttitor purus. Suspendisse sed sodales nunc. Quisque posuere augue sed luctus placerat.</p>
        <p>Morbi ullamcorper risus turpis, et ullamcorper nulla semper vitae. Proin pharetra dolor dui, non condimentum ex fermentum in. Vestibulum pharetra, risus et pulvinar eleifend, nulla tortor blandit risus, ac imperdiet elit massa quis leo. Vivamus urna lacus, luctus eget felis id, eleifend tristique nisl. Sed dignissim mollis ligula vitae laoreet. Vestibulum eget magna nisi. Aenean auctor elit et turpis blandit, eget porttitor felis suscipit. Duis placerat, sapien a sodales tempus, odio orci malesuada neque, ac molestie ipsum nisi vel eros. Integer sem lectus, luctus vitae sapien ut, efficitur aliquam sem. Praesent placerat est eros, vulputate rutrum nunc imperdiet vitae. Fusce sed felis eget purus aliquet convallis eu eget lacus. Sed finibus nec magna et accumsan. Donec vitae tellus eros. Nullam et ex vitae est sagittis malesuada. Vivamus molestie malesuada libero, a consequat magna dapibus pellentesque. Cras molestie tortor vitae congue pretium.</p>
        <p>Pellentesque nec iaculis justo, sed pretium sem. Mauris finibus lacus at mollis fringilla. Etiam auctor in justo ac bibendum. Vestibulum at lorem accumsan, maximus erat suscipit, suscipit ex. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris dignissim id quam sit amet varius. Etiam pretium ultrices dignissim. Cras at tortor hendrerit metus ultrices lobortis at ac est. Suspendisse consectetur pellentesque nunc sit amet scelerisque. Maecenas feugiat nunc laoreet, auctor erat eget, ultricies ex. Aliquam nisi nulla, cursus et ante ut, interdum volutpat leo. Phasellus laoreet aliquam maximus. Vestibulum eu neque porta, consectetur ipsum non, euismod enim. Vestibulum euismod purus elit, ultrices imperdiet nisl porttitor eget. Vivamus eros turpis, tincidunt a vulputate vel, malesuada tristique nulla.</p>
        <p>Vestibulum sed aliquam urna. Ut ullamcorper erat vitae velit mattis commodo. Phasellus dignissim rhoncus dapibus. Quisque congue egestas tellus id finibus. Suspendisse nibh felis, mattis et finibus vel, tempor in lectus. Nullam eget eros dui. Mauris eget vestibulum nibh. Nullam mattis malesuada lorem vel condimentum. Mauris id odio ac est feugiat condimentum.</p>
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="() => offcanvas?.HideAsync()">Close</Button>
    </FooterTemplate>
</Offcanvas>

<Button Color="ButtonColor.Primary" @onclick="OnShowOffcanvasClick">Show small offcanvas</Button>
```

```cs showLineNumbers
@code {
    private Offcanvas offcanvas;

    private async Task OnShowOffcanvasClick()
    {
        await offcanvas?.ShowAsync();
    }
}
```

[See demo here.](https://demos.getblazorbootstrap.com/offcanvas#sizes)

#### Large Offcanvas

<img src="https://i.imgur.com/MG0NFrU.jpg" alt="Large Offcanvas" />

```cshtml showLineNumbers
<Offcanvas @ref="offcanvas" Title="Offcanvas title" Size="OffcanvasSize.Large">
    <BodyTemplate>
        <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec tincidunt blandit mauris. Aliquam sit amet lorem laoreet, laoreet elit ut, placerat tellus. In mollis ultricies elit, volutpat maximus ipsum sodales interdum. Suspendisse eget tellus mollis, rutrum mauris ac, vulputate enim. Cras porta neque vitae lacinia elementum. Nunc sit amet pulvinar nibh. Curabitur interdum eget odio in tempor. Nulla dictum orci quis ligula auctor fermentum. Pellentesque finibus tellus ac massa convallis malesuada. Nam id pharetra velit, sed eleifend mi. Sed sed justo lorem. Quisque et nulla ut dolor feugiat vestibulum. Nunc at porttitor orci, at dignissim metus. Donec vitae metus vitae felis semper placerat.</p>
        <p>Proin quis congue enim, ut ultricies erat. Nulla facilisi. Fusce pretium, metus eget tempor vehicula, nisl lorem tincidunt metus, consectetur molestie lorem leo vel lectus. Vivamus pellentesque pharetra mattis. Aenean dignissim quam non velit ultrices rutrum. Aliquam lacinia faucibus sapien vel pretium. Nullam libero massa, ultricies id lacinia nec, scelerisque ut felis. Vivamus ac egestas urna, sit amet condimentum odio. Suspendisse ultrices, libero sed interdum pulvinar, lectus felis pellentesque enim, eu finibus magna massa id augue. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis eget tempor libero. Cras ut interdum purus. Donec eu pulvinar urna, ut porttitor purus. Suspendisse sed sodales nunc. Quisque posuere augue sed luctus placerat.</p>
        <p>Morbi ullamcorper risus turpis, et ullamcorper nulla semper vitae. Proin pharetra dolor dui, non condimentum ex fermentum in. Vestibulum pharetra, risus et pulvinar eleifend, nulla tortor blandit risus, ac imperdiet elit massa quis leo. Vivamus urna lacus, luctus eget felis id, eleifend tristique nisl. Sed dignissim mollis ligula vitae laoreet. Vestibulum eget magna nisi. Aenean auctor elit et turpis blandit, eget porttitor felis suscipit. Duis placerat, sapien a sodales tempus, odio orci malesuada neque, ac molestie ipsum nisi vel eros. Integer sem lectus, luctus vitae sapien ut, efficitur aliquam sem. Praesent placerat est eros, vulputate rutrum nunc imperdiet vitae. Fusce sed felis eget purus aliquet convallis eu eget lacus. Sed finibus nec magna et accumsan. Donec vitae tellus eros. Nullam et ex vitae est sagittis malesuada. Vivamus molestie malesuada libero, a consequat magna dapibus pellentesque. Cras molestie tortor vitae congue pretium.</p>
        <p>Pellentesque nec iaculis justo, sed pretium sem. Mauris finibus lacus at mollis fringilla. Etiam auctor in justo ac bibendum. Vestibulum at lorem accumsan, maximus erat suscipit, suscipit ex. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris dignissim id quam sit amet varius. Etiam pretium ultrices dignissim. Cras at tortor hendrerit metus ultrices lobortis at ac est. Suspendisse consectetur pellentesque nunc sit amet scelerisque. Maecenas feugiat nunc laoreet, auctor erat eget, ultricies ex. Aliquam nisi nulla, cursus et ante ut, interdum volutpat leo. Phasellus laoreet aliquam maximus. Vestibulum eu neque porta, consectetur ipsum non, euismod enim. Vestibulum euismod purus elit, ultrices imperdiet nisl porttitor eget. Vivamus eros turpis, tincidunt a vulputate vel, malesuada tristique nulla.</p>
        <p>Vestibulum sed aliquam urna. Ut ullamcorper erat vitae velit mattis commodo. Phasellus dignissim rhoncus dapibus. Quisque congue egestas tellus id finibus. Suspendisse nibh felis, mattis et finibus vel, tempor in lectus. Nullam eget eros dui. Mauris eget vestibulum nibh. Nullam mattis malesuada lorem vel condimentum. Mauris id odio ac est feugiat condimentum.</p>
    </BodyTemplate>
    <FooterTemplate>
        <Button Color="ButtonColor.Secondary" @onclick="() => offcanvas?.HideAsync()">Close</Button>
    </FooterTemplate>
</Offcanvas>

<Button Color="ButtonColor.Primary" @onclick="OnShowOffcanvasClick">Show large offcanvas</Button>
```

```cs showLineNumbers
@code {
    private Offcanvas offcanvas;

    private async Task OnShowOffcanvasClick()
    {
        await offcanvas?.ShowAsync();
    }
}
```

### Callback Events

BlazorBootstrap's offcanvas component exposes a few events for hooking into offcanvas functionality.

```cshtml showLineNumbers
<Offcanvas @ref="offcanvas" 
           title="Offcanvas title"
           OnShowing="OnOffcanvasShowingAsync"
           OnShown="OnOffcanvasShownAsync"
           OnHiding="OnOffcanvasHidingAsync"
           OnHidden="OnOffcanvasHiddenAsync">

   <BodyTemplate>
      <div>Some text as placeholder. In real life you can have the elements you have chosen. Like, text, images, lists, etc.</div>
   </BodyTemplate>

   <FooterTemplate>
       <Button Color="ButtonColor.Primary" @onclick="OnHideOffcanvasClick">Hide Offcanvas</Button>
   </FooterTemplate>

</Offcanvas>

<Button Color="ButtonColor.Primary" @onclick="OnShowOffcanvasClick">Show offcanvas</Button>
```

```cs showLineNumbers
@code {
    private Offcanvas offcanvas;

    private async Task OnShowOffcanvasClick()
    {
        await offcanvas?.ShowAsync();
    }

    private async Task OnHideOffcanvasClick()
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

[See demo here.](https://demos.getblazorbootstrap.com/offcanvas#events)