---
sidebar_label: Alerts
sidebar_position: 1
---

# Alerts

Provide contextual feedback messages for typical user actions with the handful of available and flexible BlazorBootstrap alert messages.

## Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside the alert. | | |
| Color | `AlertColor` | Gets or sets the alert color. | | `AlertColor.None` |
| Dismissable | bool | Enables the alert to be closed by placing the padding for close button. | | false |

## Methods

| Name | Description |
|--|--|
| CloseAsync | Closes an alert by removing it from the DOM. |

## Callback Events

| Name | Description |
|--|--|
| OnClose | Fires immediately when the `close` instance method is called. |
| OnClosed | Fired when the alert has been closed and CSS transitions have completed. |

## Examples

### Alerts

Alerts are available for any length of text, as well as an optional close button. For proper styling, use one of the eight colors.

<img src="https://i.imgur.com/FGgEMp6.jpg" alt="Blazor Bootstrap: Alert Component" />

```cshtml showLineNumbers
<div>
    <Alert Color="AlertColor.Primary"> A simple primary alert - check it out! </Alert>
    <Alert Color="AlertColor.Secondary"> A simple secondary alert - check it out! </Alert>
    <Alert Color="AlertColor.Success"> A simple success alert - check it out! </Alert>
    <Alert Color="AlertColor.Danger"> A simple danger alert - check it out! </Alert>
    <Alert Color="AlertColor.Warning"> A simple warning alert - check it out! </Alert>
    <Alert Color="AlertColor.Info"> A simple info alert - check it out! </Alert>
    <Alert Color="AlertColor.Light"> A simple light alert - check it out! </Alert>
    <Alert Color="AlertColor.Dark"> A simple dark alert - check it out! </Alert>
</div>
```
[See alerts demo here.](https://demos.getblazorbootstrap.com/alerts#examples)

### Additional Content

Alerts can also contain additional HTML elements like headings, paragraphs and dividers.

<img src="https://i.imgur.com/OaQ7Ydj.jpg" alt="Blazor Bootstrap: Alert Component - Alerts with additional content" />

```cshtml showLineNumbers
<div>
    <Alert Color="AlertColor.Success">
        <h4 class="alert-heading">Well done!</h4>
        <p>Aww yeah, you successfully read this important alert message. This example text is going to run a bit longer so that you can see how spacing within an alert works with this kind of content.</p>
        <hr>
        <p class="mb-0">Whenever you need to, be sure to use margin utilities to keep things nice and tidy.</p>
    </Alert>
</div>
```
[See alerts with additional content demo.](https://demos.getblazorbootstrap.com/alerts#additional-content)

### Icons

Similarly, you can use Bootstrap Icons to create alerts with icons.

<img src="https://i.imgur.com/wjCs7bZ.jpg" alt="Blazor Bootstrap: Alert Component - Alerts with an icon" />

```cshtml showLineNumbers
<div>
    <Alert Color="AlertColor.Primary"> <Icon Name="IconName.InfoCircleFill" class="me-2"></Icon>An example alert with an icon </Alert>
    <Alert Color="AlertColor.Success"> <Icon Name="IconName.CheckCircleFill" class="me-2"></Icon>A simple success alert with an icon </Alert>
    <Alert Color="AlertColor.Danger"> <Icon Name="IconName.ExclamationTriangleFill" class="me-2"></Icon>A simple danger alert with an icon </Alert>
    <Alert Color="AlertColor.Warning"> <Icon Name="IconName.ExclamationTriangleFill" class="me-2"></Icon>A simple warning alert with an icon </Alert>
</div>
```
[See alerts with an icon demo.](https://demos.getblazorbootstrap.com/alerts#icons)

### Dismissing

1. Using the `Dismissable="true"`, it's possible to dismiss any alert inline.

It's possible to dismiss any alert inline. Here's how:

<img src="https://i.imgur.com/D9tJpSl.jpg" alt="Blazor Bootstrap: Alert Component - Dismissing" />

```cshtml showLineNumbers
<div>
    <Alert Color="AlertColor.Warning" Dismissable="true"> <strong>Holy guacamole!</strong> You should check in on some of those fields below. </Alert>
</div>
```
2. Manually we can close an alert with button click.

<img src="https://i.imgur.com/dq48mYF.jpg" alt="Blazor Bootstrap: Alert Component - Dismissing manually" />

```cshtml showLineNumbers
<div>
    <Alert @ref="warningAlert" Color="AlertColor.Warning">
        <strong>Holy guacamole!</strong> You should check in on some of those fields below. <Button Color="ButtonColor.Primary" @onclick="CloseAlert">Close</Button>
    </Alert>
</div>
```

```cs {4} showLineNumbers
@code {
    Alert warningAlert;

    private async Task CloseAlert() => await warningAlert?.CloseAsync();
}
```
[See dismissable alerts demo.](https://demos.getblazorbootstrap.com/alerts#dismissing)

:::caution NOTE
When an alert is dismissed, the element is completely removed from the page structure. If a keyboard user dismisses the alert using the close button, their focus will suddenly be lost and, depending on the browser, reset to the start of the page/document. For this reason, we recommend subscribing to the `OnClosed` callback event and programmatically sets focus to the most appropriate location on the page.
:::
