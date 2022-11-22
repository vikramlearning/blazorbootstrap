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

You can customize the color with `Color` parameter. You can use any of the `SpinnerColor` values.

```cshml showLineNumbers
<Preload Color="SpinnerColor.Light"></Preload>
```

```cs {6,13,22} showLineNumbers
@code {
    [Inject] protected PreloadService PageLoadingService { get; set; }

    protected override void OnInitialized()
    {
        Task.Run(async () => await LoadSpinnerAsync());
    }

    private async Task LoadSpinnerAsync()
    {
        try
        {
            PageLoadingService.Show();
            await Task.Delay(5000);
        }
        catch
        {
            // catch exception
        }
        finally
        {
            PageLoadingService.Hide();
        }
    }
}
```

[See Preload demo here.](https://demos.getblazorbootstrap.com/preload#examples)