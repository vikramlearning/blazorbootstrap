---
title: Blazor Progress Component
description: Documentation and examples for using the Blazor Progress component featuring support for stacked bars, animated backgrounds, and text labels.
image: https://i.imgur.com/MK142lQ.png

sidebar_label: Progress
sidebar_position: 14
---

# Blazor Progress
Documentation and examples for using the Blazor Progress component featuring support for stacked bars, animated backgrounds, and text labels.

<img src="https://i.imgur.com/MK142lQ.png" alt="Blazor Progress" />

## Progress Parameters

| Name | Type | Default | Required | Descritpion |
|--|--|--|--|--|


## ProgressBar Parameters

| Name | Type | Default | Required | Descritpion |
|--|--|--|--|--|

## Examples

### How it works

<img src="https://i.imgur.com/MK142lQ.png" alt="Blazor Progress - How it works" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Width="20" Label="20%" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Success" Width="40" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Warning" Width="60" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Danger" Width="70" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Dark" Width="80" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Success" Width="20" />
    <ProgressBar Color="ProgressColor.Info" Width="20" />
    <ProgressBar Color="ProgressColor.Warning" Width="20" />
    <ProgressBar Color="ProgressColor.Danger" Width="30" />
</Progress>
```

### Labels

Add labels to your Blazor ProgressBar component using the Label parameter or by calling the `SetLabel(...)` method.

<img src="https://i.imgur.com/De7gSvL.png" alt="Blazor Progress - Labels" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar Width="20" Label="20%" />
</Progress>
```

### Set width programmatically

Use `IncreaseWidth()` or `DecreaseProgressBar()` methods to increase or decrease the Blazor ProgressBar width.

<img src="https://i.imgur.com/ZDfpxN7.png" alt="Blazor Progress - Set width programmatically" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar @ref="progressBar" />
</Progress>

<div class="mb-3">
    <Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="IncreaseProgressBar">Increase by 10%</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="DecreaseProgressBar">Decrease by 10%</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="ResetProgressBar">Reset</Button>
</div>
```

```cs showLineNumbers
@code {
    ProgressBar progressBar;

    private void IncreaseProgressBar()
    {
        progressBar.IncreaseWidth(10);
        progressBar.SetLabel($"{progressBar.GetWidth()}%");
    }

    private void DecreaseProgressBar()
    {
        progressBar.DecreaseProgressBar(10);
        progressBar.SetLabel($"{progressBar.GetWidth()}%");
    }

    private void ResetProgressBar()
    {
        progressBar.SetWidth(0);
        progressBar.SetLabel($"{progressBar.GetWidth()}%");
    }
}
```

### Height

Set the height of the Blazor Progress by using the `Height` parameter. Height is measured in pixels.

<img src="https://i.imgur.com/GzPRWkS.png" alt="Blazor Progress - Height" />

```cshtml showLineNumbers
<Progress Class="mb-3" Height="1">
    <ProgressBar Width="20" />
</Progress>
<Progress Class="mb-3" Height="5">
    <ProgressBar Width="20" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Width="40" />
</Progress>
<Progress Class="mb-3" Height="20">
    <ProgressBar Width="40" />
</Progress>
```

### Backgrounds

Use the `Color` parameter or the `SetColor(ProgressColor color)` method to change the appearance of individual Blazor ProgressBar components.

<img src="https://i.imgur.com/QkG4fyX.png" alt="Blazor Progress - Backgrounds" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Success" Width="10" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Info" Width="20" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Warning" Width="30" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Danger" Width="40" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Primary" Width="60" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Secondary" Width="70" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Dark" Width="80" />
</Progress>
```

### Set background programmatically

You can dynamically set the Blazor ProgressBar color by calling the `SetColor()` method.

<img src="https://i.imgur.com/rXnCzk1.png" alt="Blazor Progress - Set background programmatically" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar @ref="progressBar" Width="30" Label="30%" />
</Progress>

<div>
    <Button Type="ButtonType.Button" Color="ButtonColor.Success" Size="Size.Small" @onclick="() => SetColor(ProgressColor.Success)">Set Color</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Info" Size="Size.Small" @onclick="() => SetColor(ProgressColor.Info)">Set Color</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Warning" Size="Size.Small" @onclick="() => SetColor(ProgressColor.Warning)">Set Color</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Danger" Size="Size.Small" @onclick="() => SetColor(ProgressColor.Danger)">Set Color</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Primary" Size="Size.Small" @onclick="() => SetColor(ProgressColor.Primary)">Set Color</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Secondary" Size="Size.Small" @onclick="() => SetColor(ProgressColor.Secondary)">Set Color</Button>
    <Button Type="ButtonType.Button" Color="ButtonColor.Dark" Size="Size.Small" @onclick="() => SetColor(ProgressColor.Dark)">Set Color</Button>
</div>
```

```cs showLineNumbers
@code {
    ProgressBar progressBar;
    private void SetColor(ProgressColor color) => progressBar.SetColor(color);
}
```
### Multiple bars

Include multiple Blazor ProgressBar components in a Blazor Progress component if needed.

<img src="https://i.imgur.com/PCkUVYq.png" alt="Blazor Progress - Multiple bars" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar Color="ProgressColor.Success" Width="20" />
    <ProgressBar Color="ProgressColor.Info" Width="20" />
    <ProgressBar Color="ProgressColor.Warning" Width="20" />
    <ProgressBar Color="ProgressColor.Danger" Width="10" />
</Progress>
```

### Striped

Add `Type="ProgressType.Striped"` to any Blazor ProgressBar component to apply a stripe.

<img src="https://i.imgur.com/Ggg3N6i.png" alt="Blazor Progress - Striped" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Success" Width="10" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Info" Width="20" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Warning" Width="30" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Danger" Width="40" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Primary" Width="60" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Secondary" Width="80" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.Striped" Color="ProgressColor.Dark" Width="100" />
</Progress>
```

### Animated stripes

The stripes can also be animated. Add `Type="ProgressType.StripedAndAnimated"` to the Blazor ProgressBar component to animate the stripes right to the left.

<img src="https://i.imgur.com/X4MpgJG.png" alt="Blazor Progress - Animated stripes" />

```cshtml showLineNumbers
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Success" Width="10" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Info" Width="20" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Warning" Width="30" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Danger" Width="40" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Primary" Width="60" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Secondary" Width="80" />
</Progress>
<Progress Class="mb-3">
    <ProgressBar Type="ProgressType.StripedAndAnimated" Color="ProgressColor.Dark" Width="100" />
</Progress>
```