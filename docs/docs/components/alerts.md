---
sidebar_label: Alerts
sidebar_position: 1
---

# Alerts

Documentation and examples for BlazorBootstrap Alerts.

## Parameters

| Name | Type | Descritpion | Default Value |
|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside the alert. | |
| Color | Enum | Gets or sets the alert color. | |
| Dismisable | bool | Enables the alert to be closed by placing the padding for close button. | |

## Methods

| Name | Description |
|--|--|--|
| CloseAsync | Closes an alert by removing it from the DOM. |

## Callback Events

| Name | Description |
|--|--|--|
| OnClose | Fires immediately when the `close` instance method is called. |
| OnClosed | Fired when the alert has been closed and CSS transitions have completed. |

## Examples

### Alerts

```cshtml
<div>
    <Alert Color="AlertColor.Primary"> A simple primary alert. </Alert>
    <Alert Color="AlertColor.Secondary"> A simple secondary alert. </Alert>
    <Alert Color="AlertColor.Success"> A simple success alert. </Alert>
    <Alert Color="AlertColor.Danger"> A simple danger alert. </Alert>
    <Alert Color="AlertColor.Warning"> A simple warning alert. </Alert>
    <Alert Color="AlertColor.Info"> A simple info alert. </Alert>
</div>
```

### Dismissable Alerts

```cshtml
<div>
    <Alert Color="AlertColor.Light" Dismisable="true" Visible="true"> A simple light alert. </Alert>
</div>
```

### Toogle Alerts

```cshtml
<div>
    <Alert @ref="darkAlert" Color="AlertColor.Dark" Dismisable="true"> A simple light alert. </Alert>
    <Button Color="ButtonColor.Dark" @onclick="CloseAlert"> Close Alert </Button>
</div>
```

```cs {6}
@code{
    Alert darkAlert;

    void CloseAlert()
    {
        darkAlert?.CloseAsync();
    }
}
```

### Alerts with additional content

```cshtml
<div>
    <Alert Color="AlertColor.Success" Dismisable="true" Visible="true">
        <h4 class="alert-heading">Well done!</h4>
        <p>Aww yeah, you successfully read this important alert message. This example text is going to run a bit longer so that you can see how spacing within an alert works with this kind of content.</p>
        <hr>
        <p class="mb-0">Whenever you need to, be sure to use margin utilities to keep things nice and tidy.</p>
    </Alert>
</div>
```

```cshtml
<div>
    <Alert Color="AlertColor.Success" Dismisable="true" Visible="true">
        <h4 class="alert-heading">Well done!</h4>
        <p>Aww yeah, you successfully read this important alert message. This example text is going to run a bit longer so that you can see how spacing within an alert works with this kind of content.</p>
        <hr>
        <p class="mb-0">Whenever you need to, be sure to use margin utilities to keep things nice and tidy.</p>
    </Alert>
</div>
```