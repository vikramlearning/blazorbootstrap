---
title: Blazor Tabs Component
description: Documentation and examples for how to use Blazor Bootstrap tabs components.
image: https://i.imgur.com/KelXx6Z.png

sidebar_label: Tabs
sidebar_position: 17
---

# Blazor Tabs

Documentation and examples for how to use Blazor Bootstrap tabs components.

## Tabs Parameters

| Name | Type | Descritpion | Required | Default |
|:--|:--|:--|:--|:--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| EnableFadeEffect | bool | Gets or sets the tabs fade effect. | | false |
| NavStyle | `NavStyle` | Get or sets the nav style. | | `NavStyle.Tabs` |

## Tabs Methods

| Name | Description |
|:--|:--|
| ShowFirstTabAsync | Selects the first tab and show its associated pane. |
| ShowLastTabAsync | Selects the last tab and show its associated pane. |
| ShowTabByNameAsync(string) | Selects the tab by name and show its associated pane. |
| ShowTabByIndexAsync(int) | Selects the tab by index and show its associated pane. |

## Tab Parameters

| Name | Type | Descritpion | Required | Default |
|:--|:--|:--|:--|:--|
| Title | string | Gets or sets the tab title. | | |
| TitleTemplate | RenderFragment | Gets or sets the tab title template. | | |
| Content | RenderFragment | Specifies the content to be rendered inside the tab. | ✔️ | |
| IsActive | bool | Gets or sets the active tab. | | false |
| Disabled | bool | Gets or sets the disabled. | | false |

:::info Note
Either **Title** or **TitleTemplate** is required.
:::

## Examples

### Tabs

<img src="https://i.imgur.com/ranwriJ.png" alt="Blazor Tabs Component - Examples" />

```cshtml showLineNumbers
<Tabs>
    <Tab Title="Home" IsActive="true">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Home tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Profile">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Profile tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Contact">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Contact tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
</Tabs>
```

[See demo here.](https://demos.blazorbootstrap.com/tabs#examples)

### Fade effect

To make tabs fade in, add `EnableFadeEffect="true"`. The first tab pane must also have `IsActive="true"` to make the initial content visible.

<img src="https://i.imgur.com/ranwriJ.png" alt="Blazor Tabs Component - Fade effect" />

```cshtml {1, 2} showLineNumbers
<Tabs EnableFadeEffect="true">
    <Tab Title="Home" IsActive="true">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Home tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Profile">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Profile tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Contact">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Contact tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
</Tabs>
```

[See demo here.](https://demos.blazorbootstrap.com/tabs#fade-effect)

### Title with icon

To customize the tab title, use `TitleTemplate`, as shown in the below example.

<img src="https://i.imgur.com/KelXx6Z.png" alt="Blazor Tabs Component - Title with icon" />

```cshtml {3-5,13-15,23-25} showLineNumbers
<Tabs EnableFadeEffect="true">
    <Tab IsActive="true">
        <TitleTemplate>
            <Icon Name="IconName.HouseFill" /> Home
        </TitleTemplate>
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Home tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab>
        <TitleTemplate>
            <Icon Name="IconName.PersonFill" /> Profile
        </TitleTemplate>
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Profile tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab>
        <TitleTemplate>
            <Icon Name="IconName.PhoneFill" /> Contact
        </TitleTemplate>
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Contact tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
</Tabs>
```

[See demo here.](https://demos.blazorbootstrap.com/tabs#title-with-icon)

### Disable Tab

Disable specific tabs by adding `Disabled="true"` parameter.

<img src="https://i.imgur.com/TCG6gCz.png" alt="Blazor Tabs Component - Disable Tab" />

```cshtml {16} showLineNumbers
<Tabs EnableFadeEffect="true">
    <Tab Title="Home" IsActive="true">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Home tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Profile">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Profile tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Projects" Disabled="true">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Projects tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Contact">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Contact tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
</Tabs>
```

[See demo here.](https://demos.blazorbootstrap.com/tabs#disable-tab)

### Pills

Use `NavStyle="NavStyle.Pills"` parameter to change the tabs to pills.

<img src="https://i.imgur.com/IyRJ0PS.png" alt="Blazor Tabs Component - Pills" />

```cshtml {1} showLineNumbers
<Tabs EnableFadeEffect="true" NavStyle="NavStyle.Pills">
    <Tab Title="Home" IsActive="true">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Home tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Profile">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Profile tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
    <Tab Title="Contact">
        <Content>
            <p class="mt-2">
                <b>This is some placeholder content the Contact tab's associated content.</b> Clicking another tab will toggle the visibility of this one for the next.
            </p>
        </Content>
    </Tab>
</Tabs>
```

[See demo here.](https://demos.blazorbootstrap.com/tabs#pills)

### Activate individual tabs

You can activate individual tabs in several ways. Use predefined methods `ShowFirstTabAsync`, `ShowLastTabAsync`, `ShowTabByIndexAsync`, and `ShowTabByNameAsync`, as shown below.

<img src="https://i.imgur.com/NdE5oqH.png" alt="Blazor Tabs Component - Activate individual tabs" />

```cshtml showLineNumbers
<Tabs @ref="tabs" EnableFadeEffect="true">
    <Tab Title="Home" IsActive="true">
        <Content>
            <p class="mt-3">This is the placeholder content for the <b>Home</b> tab.</p>
        </Content>
    </Tab>
    <Tab Title="Profile">
        <Content>
            <p class="mt-3">This is the placeholder content for the <b>Profile</b> tab.</p>
        </Content>
    </Tab>
    <Tab Title="Contact">
        <Content>
            <p class="mt-3">This is the placeholder content for the <b>Contact</b> tab.</p>
        </Content>
    </Tab>
    <Tab Title="Products" Name="Products">
        <Content>
            <p class="mt-3">This is the placeholder content for the <b>Products</b> tab.</p>
        </Content>
    </Tab>
    <Tab Title="FAQs" Name="FAQ">
        <Content>
            <p class="mt-3">This is the placeholder content for the <b>FAQs</b> tab.</p>
        </Content>
    </Tab>
    <Tab Title="About">
        <Content>
            <p class="mt-3">This is the placeholder content for the <b>About</b> tab.</p>
        </Content>
    </Tab>
</Tabs>

<Button Color="ButtonColor.Primary" @onclick="ShowFirstTabAsync">First Tab</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowSecondTabAsync">Second Tab</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowThirdTabAsync">Third Tab</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowProductsTabAsync">Products Tab</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowFaqsAsync">FAQs Tab</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowLastTabAsync">Last Tab</Button>
```

```cs showLineNumbers
@code{
    Tabs tabs;

    private async Task ShowFirstTabAsync() => await tabs.ShowFirstTabAsync();
    private async Task ShowSecondTabAsync() => await tabs.ShowTabByIndexAsync(1);
    private async Task ShowThirdTabAsync() => await tabs.ShowTabByIndexAsync(2);
    private async Task ShowProductsTabAsync() => await tabs.ShowTabByNameAsync("Products");
    private async Task ShowFaqsAsync() => await tabs.ShowTabByNameAsync("FAQ");
    private async Task ShowLastTabAsync() => await tabs.ShowLastTabAsync();
}
```

### Callback Events

When showing a new tab, the events fire in the following order:

1. `OnHiding` (on the current active tab)
1. `OnShowing` (on the to-be-shown tab)
1. `OnHidden` (on the previous active tab, the same one as for the `OnHiding` event)
1. `OnShown` (on the newly-active just-shown tab, the same one as for the `OnShowing` event)

:::info Note
If no tab was already active, then the `OnHiding` and `OnHidden` events will not be fired.
:::

[See demo here.](https://demos.blazorbootstrap.com/tabs#events)
