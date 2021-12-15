---
sidebar_label: Preload
sidebar_position: 5
---

# Preload

Documentation and examples for BlazorBootstrap Preload.

Things to know when using the preload component:

- Add `Preload` component
- Inject `PreloadService`
- Call `preloadService.Show()` before you make any call to the API.
- Call `preloadService.Hide()` after you get the response from the API.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| Color | SpinnerColor | Gets or sets the spinner color. | | `SpinnerColor.None` |
| IsVerticallyCentered | bool | Shows the preload vertically in the center of the page. |  | `true` |

## Preload Service

### Methods

| Name | Description |
|--|--|
| Show | Shows the preload. |
| Hide | Hides the preload. |

## Examples

```cshml
<Preload Color="SpinnerColor.Light"></Preload>
```

```cs {6,13,22}
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
## Articles

- [Blazor Bootstrap: Preload Component Examples](https://vikramlearning.com/dotnet/article/blazor-bootstrap-preload-component-examples/88/159)