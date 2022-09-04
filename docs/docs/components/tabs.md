---
title: Blazor Tabs Component
description: Documentation and examples for how to use BlazorBootstrap's Tabs components.
image: https://getblazorbootstrap.com/img/logo.svg

sidebar_label: Tabs
sidebar_position: 13
---

# Tabs

Documentation and examples for how to use BlazorBootstrap's <code>Tabs</code> components.

## Tabs Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside this. | | |
| EnableFadeEffect | bool | Gets or sets the tabs fade effect. | | false |
| NavStyle | `NavStyle` | Get or sets the nav style. | | `NavStyle.Tabs` |

## Tab Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| Title | string | Gets or sets the tab title. | | |
| TitleTemplate | RenderFragment | Gets or sets the tab title template. | | |
| Content | RenderFragment | Specifies the content to be rendered inside the tab. | ✔️ | |
| IsActive | bool | Gets or sets the active tab. | | false |
| Disabled | bool | Gets or sets the disabled. | | false |

:::info Note
**Title** or **TitleTemplate** is required.
:::

## Examples

### Tabs

<img src="https://i.imgur.com/ranwriJ.png" alt="Tabs - Examples" />

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

[See demo here.](https://demos.getblazorbootstrap.com/tabs#examples)

### Fade effect

To make tabs fade in, add `EnableFadeEffect="true"`. The first tab pane must also have `IsActive="true"` to make the initial content visible.

<img src="https://i.imgur.com/ranwriJ.png" alt="Tabs - Fade effect" />

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

[See demo here.](https://demos.getblazorbootstrap.com/tabs#fade-effect)

### Title with icon

<img src="https://i.imgur.com/KelXx6Z.png" alt="Tabs - Title with icon" />

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

[See demo here.](https://demos.getblazorbootstrap.com/tabs#title-with-icon)

### Disable Tab

<img src="https://i.imgur.com/TCG6gCz.png" alt="Tabs - Disable Tab" />

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

[See demo here.](https://demos.getblazorbootstrap.com/tabs#disable-tab)

### Pills

<img src="https://i.imgur.com/IyRJ0PS.png" alt="Tabs - Pills" />

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

[See demo here.](https://demos.getblazorbootstrap.com/tabs#pills)

### Callback Events

When showing a new tab, the events fire in the following order:

1. `OnHiding` (on the current active tab)
1. `OnShowing` (on the to-be-shown tab)
1. `OnHidden` (on the previous active tab, the same one as for the `OnHiding` event)
1. `OnShown` (on the newly-active just-shown tab, the same one as for the `OnShowing` event)

:::info Note
If no tab was already active, then the `OnHiding` and `OnHidden` events will not be fired.
:::

[See demo here.](https://demos.getblazorbootstrap.com/tabs#events)
