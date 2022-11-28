---
title: Blazor Preload Component
description: Indicate the loading state of a page with Blazor Bootstrap preload component.
image: https://i.imgur.com/3pvzbXY.png

sidebar_label: Preload
sidebar_position: 13
---

# Blazor Preload

Indicate the loading state of a page with Blazor Bootstrap preload component.

**Things to know when using the `Preload` component:**

- Add the `Preload` component to your current page or your layout page.
- Inject `PreloadService`
- Call `preloadService.Show()` before you make any call to the API.
- Call `preloadService.Hide()` after you get the response from the API.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| Color | `SpinnerColor` | Gets or sets the spinner color. | | `SpinnerColor.None` |
| IsVerticallyCentered | bool | Shows the preload vertically in the center of the page. |  | `true` |

## Preload Service

### Methods

| Name | Description |
|--|--|
| Show | Shows the preload. |
| Hide | Hides the preload. |

## Examples

YIn the below example, when the page loads `PreloadService.Show()` is called to show the spinner.

```cshml {1} showLineNumbers
<Preload Color="@spinnerColor"></Preload>

<Button Color="ButtonColor.Primary" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Primary)">Primary Spinner</Button>
<Button Color="ButtonColor.Secondary" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Secondary)">Secondary Spinner</Button>
<Button Color="ButtonColor.Success" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Success)">Success Spinner</Button>
<Button Color="ButtonColor.Danger" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Danger)">Danger Spinner</Button>
<Button Color="ButtonColor.Warning" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Warning)">Warning Spinner</Button>
<Button Color="ButtonColor.Info" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Info)">Info Spinner</Button>
<Button Color="ButtonColor.Light" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Light)">Light Spinner</Button>
<Button Color="ButtonColor.Dark" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Dark)">Dark Spinner</Button>
```

```cs {6,13,22} showLineNumbers
@code {
    public SpinnerColor spinnerColor = SpinnerColor.Light;

    [Inject] protected PreloadService PreloadService { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Task.Run(() => ShowSpinnerAsync());
    }

    private async Task ShowSpinnerAsync(SpinnerColor spinnerColor = SpinnerColor.Light)
    {
        this.spinnerColor = spinnerColor;

        PreloadService.Show();

        await Task.Delay(3000); // call the service/api

        PreloadService.Hide();
    }
}
```

[See Preload demo here.](https://demos.getblazorbootstrap.com/preload#examples)