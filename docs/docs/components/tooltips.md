---
sidebar_label: Tooltips
sidebar_position: 15
---

# Tooltips

Use BlazorBootstrap's tooltip component to add custom tooltips to your web pages.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| Placement | Specifies the tooltip placement. Default is top right. | | `TooltipPlacement.Top` |
| Title | string | Displays informative text when users hover, focus, or tap an element. | ✔️ | |

## Examples

### Tooltips

<div>
    <img src="https://i.imgur.com/uqvqb2i.jpg" alt="Blazor Bootstrap: Tooltip Component" />
</div>

<div>
    <img src="https://i.imgur.com/ZHLTCvX.jpg" alt="Blazor Bootstrap: Tooltip Component" />
</div>

<div>
    <img src="https://i.imgur.com/jwJUhkV.jpg" alt="Blazor Bootstrap: Tooltip Component" />
</div>

<div>
    <img src="https://i.imgur.com/T2YMw9p.jpg" alt="Blazor Bootstrap: Tooltip Component" />
</div>

```cshtml
<div>
    <Tooltip Title="Tooltip Left" Placement="TooltipPlacement.Left">Tooltip Left</Tooltip>
</div>
<div>
    <Tooltip Title="Tooltip Top">Tooltip Top</Tooltip>
</div>
<div>
    <Tooltip Title="Tooltip Right" Placement="TooltipPlacement.Right">Tooltip Right</Tooltip>
</div>
<div>
    <Tooltip Title="Tooltip Bottom" Placement="TooltipPlacement.Bottom">Tooltip Bottom</Tooltip>
</div>
```

[See tooltips demo here.](https://demos.getblazorbootstrap.com/tooltips#examples)

### Disabled button with tooltip

<img src="https://i.imgur.com/PGlmZS3.jpg" alt="Blazor Bootstrap: Tooltip Component" />

```cshtml
<Tooltip Class="d-inline-block" Title="Disabled button"role="button">
    <button class="btn btn-primary" type="button" disabled>Disabled button</button>
</Tooltip>
```

[See disabled button with tooltip demo here.](https://demos.getblazorbootstrap.com/tooltips#disabled-button-with-tootip)

### Tooltip icon with click event

<img src="https://i.imgur.com/D3FrZba.jpg" alt="Blazor Bootstrap: Tooltip Component" />

```cshtml
<Tooltip Title="Click here" @onclick="OnClick" role="button">
    <i class="bi bi-arrow-repeat text-danger"></i>
</Tooltip>
```

```cs
@code {
    private void OnClick()
    {
        Console.WriteLine($"clicked");
    }
}
```

[See icon with tooltip demo here.](https://demos.getblazorbootstrap.com/tooltips#icon-with-click-event)