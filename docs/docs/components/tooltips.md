---
sidebar_label: Tooltips
sidebar_position: 6
---

# Tooltips

Documentation and examples for BlazorBootstrap Tooltips.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| Placement | Specifies the tooltip placement. Default is top right. | | `TooltipPlacement.Top` |
| Title | string | Displays informative text when users hover, focus, or tap an element. | ✔️ | |

## Examples

### Tooltips

```cshtml
<div>
    <Tooltip TooltipTitle="Tooltip Left" TooltipPlacement="TooltipPlacement.Left">Tooltip Left</Tooltip>
</div>
<div>
    <Tooltip TooltipTitle="Tooltip Top">Tooltip Top</Tooltip>
</div>
<div>
    <Tooltip TooltipTitle="Tooltip Right" TooltipPlacement="TooltipPlacement.Right">Tooltip Right</Tooltip>
</div>
<div>
    <Tooltip TooltipTitle="Tooltip Bottom" TooltipPlacement="TooltipPlacement.Bottom">Tooltip Bottom</Tooltip>
</div>
```

### Disabled button with tooltip

```cshtml
<Tooltip Class="d-inline-block" TooltipTitle="Disabled button"role="button">
    <button class="btn btn-primary" type="button" disabled>Disabled button</button>
</Tooltip>
```

### Tooltip icon with click event

```cshtml
<Tooltip TooltipTitle="Click here" @onclick="OnClick" role="button">
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

## Articles

- [Blazor Bootstrap: Tooltip Component Examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-tooltip-component-examples/88/158)