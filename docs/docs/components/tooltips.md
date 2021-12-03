---
sidebar_label: Tooltips
sidebar_position: 5
---

# Tooltips

Documentation and examples for BlazorBootstrap Tooltips.

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