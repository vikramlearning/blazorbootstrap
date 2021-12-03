---
sidebar_label: Alerts
sidebar_position: 1
---

# Alerts

Documentation and examples for BlazorBootstrap Alerts.

## Usage

```cshtml
<div>
    <Alert Color="AlertColor.Primary"> A simple primary alert—check it out! </Alert>
    <Alert Color="AlertColor.Secondary"> A simple secondary alert—check it out! </Alert>
    <Alert Color="AlertColor.Success"> A simple success alert—check it out! </Alert>
    <Alert Color="AlertColor.Danger"> A simple danger alert—check it out! </Alert>
    <Alert Color="AlertColor.Warning"> A simple warning alert—check it out! </Alert>
    <Alert Color="AlertColor.Info"> A simple info alert—check it out! </Alert>
</div>
```

## Dismissable Alerts

```cshtml
<div>
    <Alert Color="AlertColor.Light" Dismisable="true" Visible="true"> A simple light alert—check it out! </Alert>
</div>
```

## Toogle Alerts

```cshtml
<div>
    <Alert @ref="darkAlert" Color="AlertColor.Dark" Dismisable="true"> A simple light alert—check it out! </Alert>
    <Button Color="ButtonColor.Dark" @onclick="onClick"> Toggle Alert </Button>
</div>
```

```cs
@code{
    Alert darkAlert;

    void onClick()
    {
        darkAlert.Toogle();
    }
}
```

## Alerts with additional content

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