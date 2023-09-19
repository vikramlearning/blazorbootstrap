---
title: Blazor Preload Component
description: Indicate the loading state of a page with Blazor Bootstrap preload component.
image: https://i.imgur.com/UV6zPTQ.png

sidebar_label: Preload
sidebar_position: 17
---

# Blazor Preload

Indicate the loading state of a page with Blazor Bootstrap preload component.

**Things to know when using the `Preload` component:**

- Add the `Preload` component to your **current page** or your **layout page**.
- Inject `PreloadService`
- Call `PreloadService.Show()` before you make any call to the API.
- Call `PreloadService.Hide()` after you get the response from the API.

<img src="https://i.imgur.com/UV6zPTQ.png" alt="Blazor Bootstrap: Blazor Preload Component - Default" />

## Parameters

| Name | Type | Default | Descritpion | Required |  Version Added |
|--|--|--|--|--|--|
| ChildContent | RenderFragment | null | Specifies the content to be rendered inside this. | | 1.1.0 |

## Preload Service

### Methods

| Name | Return Type | Description | Added Version |
|--|--|--|--|
| Show(SpinnerColor spinnerColor = SpinnerColor.Light)| void | Shows the preload. | 1.1.0 |
| Hide() | void | Hides the preload. | 1.1.0 |

## Global preload service for the application

1. Add the `Preload` component in **MainLayout.razor** page as shown below.

```cshtml {} showLineNumbers
@using BlazorBootstrap
.
.

... MainLayout.razor code goes here ...

.
.
<Preload />
```

2. Inject `PreloadService`, then call the `Show()` and `Hide()` methods before and after the Service/API, respectively, as shown below.

```cs {} showLineNumbers
@code {

    [Inject] protected PreloadService PreloadService { get; set; }

    private void GetEmployees()
    {
        try
        {
            PreloadService.Show();

            // call the service/api to get the employees
        }
        catch
        {
            // handle exception
        }
        finally
        {
            PreloadService.Hide();
        }
    }
}
```

[See Preload demo here.](https://demos.blazorbootstrap.com/preload#global-preload-service-for-the-application)

## Change spinner color

Change the default spinner color by passing the `SpinnerColor` enum to the `Show(...)` method. In the below example, we are using a [global preload service](/components/preload#global-preload-service-for-the-application), as shown in the above section.

<img src="https://i.imgur.com/5PVt5bX.png" alt="Blazor Bootstrap: Blazor Preload Component - Change spinner color" />

```cshml {} showLineNumbers
<Button Color="ButtonColor.Primary" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Primary)">Primary Spinner</Button>
<Button Color="ButtonColor.Secondary" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Secondary)">Secondary Spinner</Button>
<Button Color="ButtonColor.Success" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Success)">Success Spinner</Button>
<Button Color="ButtonColor.Danger" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Danger)">Danger Spinner</Button>
<Button Color="ButtonColor.Warning" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Warning)">Warning Spinner</Button>
<Button Color="ButtonColor.Info" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Info)">Info Spinner</Button>
<Button Color="ButtonColor.Light" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Light)">Light Spinner</Button>
<Button Color="ButtonColor.Dark" @onclick="async () => await ShowSpinnerAsync(SpinnerColor.Dark)">Dark Spinner</Button>
```

```cs {} showLineNumbers
@code {
    [Inject] protected PreloadService PreloadService { get; set; }

    private async Task ShowSpinnerAsync(SpinnerColor spinnerColor)
    {
        PreloadService.Show(spinnerColor);

        await Task.Delay(3000); // call the service/api

        PreloadService.Hide();
    }
}
```

[See Preload demo here.](https://demos.blazorbootstrap.com/preload#change-spinner-color)