---
title: Blazor Time Input Component
description: The Blazor Bootstrap `TimeInput` component is constructed using an HTML input of `type="time"` which limits user input based on pre-defined parameters. This component enables users to input a time using a text box with validation or a special time picker interface.
image: https://i.imgur.com/TlvjRlP.png

sidebar_label: Time Input
sidebar_position: 3
---

# Blazor Time Input

The Blazor Bootstrap `TimeInput` component is constructed using an HTML input of `type="time"` which limits user input based on pre-defined parameters. This component enables users to input a time using a text box with validation or a special time picker interface.

<img src="https://i.imgur.com/TlvjRlP.png" alt="Blazor Bootstrap: Time Input Component" />

## Parameters

| Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| AutoComplete | bool | false | | Indicates whether the TimeInput can complete the values automatically by the browser. | 1.6.0 |
| Disabled | bool | false | | Gets or sets the disabled. | 1.6.0 |
| EnableMinMax | bool | false | | Determines whether to restrict the user input to Min and Max range. If true, restricts the user input between the Min and Max range. Else accepts the user input. | 1.6.0 |
| Max| TValue | | | Gets or sets the max. Max ignored if EnableMinMax="false". | 1.6.0 |
| Min| TValue | | | Gets or sets the min. Min ignored if EnableMinMax="false". | 1.6.0 |
| Placeholder | string? | null | | Gets or sets the placeholder. | 1.6.0 |
| Value | TValue | | | Gets or sets the value. | 1.6.0 |
| ValueExpression | `Expression<Func<TValue>>` | | | Gets or sets the expression | 1.6.0 |

## Methods

| Name | Description | Added Version |
|:--|:--|:--|
| Disable | Disables time input. | 1.6.0 |
| Enable | Enables time input. | 1.6.0 |

## Events

| Name | Description |
|:--|:--|
| ValueChanged | This event fired on every user keystroke that changes the `TimeInput` value. |

## Examples

### Basic usage

:::caution NOTE
The input UI generally varies from browser to browser. In unsupported browsers, the control degrades gracefully to `type="text"`.
:::

<img src="https://i.imgur.com/TlvjRlP.png" alt="Blazor Bootstrap: Time Input Component - Basic usage" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <TimeInput TValue="TimeOnly" @bind-Value="@date1" />
</div>
<div class="mb-3">Entered date: @date1</div>

@code {
    private TimeOnly date1 = new TimeOnly(13, 14);
}
```

[See demo here](https://demos.blazorbootstrap.com/form/time-input#basic-usage)

### Generic type

The Blazor Bootstrap TimeInput component supports `TimeOnly` and `TimeOnly?`. In the below example, `TValue` is set to `TimeOnly` and `TimeOnly?`.

<img src="https://i.imgur.com/7tf0Ut2.png" alt="Blazor Bootstrap: Time Input Component - Generic type" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <strong>TimeOnly</strong>:
</div>
<div class="mb-3">
    <TimeInput TValue="TimeOnly" @bind-Value="@time1" />
</div>
<div class="mb-3">Entered time: @time1</div>

<div class="mb-3">
    <strong>TimeOnly?</strong>:
</div>
<div class="mb-3">
    <TimeInput TValue="TimeOnly?" @bind-Value="@time2" />
</div>
<div class="mb-3">Entered time: @time2</div>
```

```cs {} showLineNumbers
@code {
    private TimeOnly time1 = new TimeOnly(6, 40);
    private TimeOnly? time2;
}
```

[See demo here](https://demos.blazorbootstrap.com/form/time-input#generic-type)

### Enable min and max

Set `EnableMinMax="true"` and set the `Min` and `Max` parameters to restrict the user input between the Min and Max range.

<img src="https://i.imgur.com/FWKLqKj.png" alt="Blazor Bootstrap: Date Input Component - Enable min and max" />

:::caution NOTE
If the user tries to enter a number in the <b>TimeInput</b> field which is out of range, then it will override with Min or Max value based on the context. 
If the user input is less than the Min value, then it will override with the Min value. 
If the user input exceeds the Max value, it will override with the Max value.
:::

```cshtml {} showLineNumbers
<div class="mb-3">
    <strong>TimeOnly</strong>:
</div>
<div class="mb-3">
    <TimeInput TValue="TimeOnly" @bind-Value="@time1" EnableMinMax="true" Min="@min1" Max="@max1" />
</div>
<div class="mb-3">Min time: @min1</div>
<div class="mb-3">Max time: @max1</div>
<div class="mb-3">Entered time: @time1</div>

<div class="mb-3">
    <strong>TimeOnly?</strong>:
</div>
<div class="mb-3">
    <TimeInput TValue="TimeOnly?" @bind-Value="@time2" EnableMinMax="true" Min="@min2" Max="@max2" />
</div>
<div class="mb-3">Min time: @min2</div>
<div class="mb-3">Max time: @max2</div>
<div class="mb-3">Entered time: @time2</div>
```

```cs {} showLineNumbers
@code {
    private TimeOnly time1, min1, max1;
    private TimeOnly? time2, min2, max2;

    protected override void OnInitialized()
    {
        time1 = new TimeOnly(10, 0); // 10:00 AM
        min1 = new TimeOnly(8, 0); // 08:00 AM
        max1 = new TimeOnly(18, 0); // 06:00 PM

        time2 = null;
        min2 = new TimeOnly(8, 0); // 08:00 AM
        max2 = new TimeOnly(18, 0); // 06:00 PM
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/form/time-input#enable-max-min)

### Disable

Use the `Disabled` parameter to disable the `TimeInput`. Also, use <b>Enable()</b> and <b>Disable()</b> methods to enable and disable the `TimeInput`.

<img src="https://i.imgur.com/8NcaVEq.png" alt="Blazor Bootstrap: Time Input Component - Disable" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <TimeInput @ref="timeInput" TValue="TimeOnly" @bind-Value="@time1" Disabled="true" />
</div>
<div class="mb-3">Entered time: @time1</div>

<Button Color="ButtonColor.Primary" @onclick="enableTimeInput"> Enable </Button>
<Button Color="ButtonColor.Secondary" @onclick="disableTimeInput"> Disable </Button>
```

```cs {} showLineNumbers
@code {
    private TimeInput<TimeOnly> timeInput = default!;

    private TimeOnly time1 = new TimeOnly(10, 50); // 10:50 AM

    private void enableTimeInput() => timeInput.Enable();

    private void disableTimeInput() => timeInput.Disable();
}
```

[See demo here](https://demos.blazorbootstrap.com/form/time-input#disable)

### Validations

Like any other blazor input component, `TimeInput` component supports validations. 
Use the Use the **DataAnnotations** to validate the user input before submitting the form. In the below example, we used the `Required` attributes.

<img src="https://i.imgur.com/qLmWjXC.png" alt="Blazor Bootstrap: Time Input Component - Validations" />

```cshtml {} showLineNumbers
@using System.ComponentModel.DataAnnotations

<style>
    .valid.modified:not([type=checkbox]) {
        outline: 1px solid #26b050;
    }

    .invalid {
        outline: 1px solid red;
    }

    .validation-message {
        color: red;
    }
</style>

<EditForm EditContext="@editContext" OnValidSubmit="HandleValidSubmit" novalidate>
    <DataAnnotationsValidator />

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Flight Number: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <InputText class="form-control" @bind-Value="flightForm.FlightNumber" Placeholder="Enter Flight Number" />
            <ValidationMessage For="@(() => flightForm.FlightNumber)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Departure Date: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <DateInput TValue="DateOnly?" class="form-control" @bind-Value="flightForm.DepartureDate" />
            <ValidationMessage For="@(() => flightForm.DepartureDate)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Departure Time: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <TimeInput TValue="TimeOnly?" @bind-Value="flightForm.DepartureTime" />
            <ValidationMessage For="@(() => flightForm.DepartureTime)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Arrival Date: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <DateInput TValue="DateOnly?" class="form-control" @bind-Value="flightForm.ArrivalDate" />
            <ValidationMessage For="@(() => flightForm.ArrivalDate)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Arrival Time: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <TimeInput TValue="TimeOnly?" @bind-Value="flightForm.ArrivalTime" />
            <ValidationMessage For="@(() => flightForm.ArrivalTime)" />
        </div>
    </div>

    <div class="row">
        <div class="col-md-12 text-right">
            <Button Type="ButtonType.Button" Color="ButtonColor.Secondary" Class="float-end" @onclick="ResetForm">Reset</Button>
            <Button Type="ButtonType.Submit" Color="ButtonColor.Success" Class="float-end me-2">Submit</Button>
        </div>
    </div>
</EditForm>
```

```cs {} showLineNumbers
@code {
    private FlightForm flightForm = new();
    private EditContext editContext;

    [Inject] ToastService _toastService { get; set; }

    protected override void OnInitialized()
    {
        editContext = new EditContext(flightForm);
        base.OnInitialized();
    }

    public void HandleValidSubmit()
    {
        var toastMessage = new ToastMessage
        (
            type: ToastType.Success,
            iconName: IconName.Check2All,
            title: "Success!",
            helpText: $"{DateTime.Now.ToLocalTime()}",
            message: "Flight schedule created."
        );
        _toastService.Notify(toastMessage);
    }

    private void ResetForm()
    {
        flightForm = new();
        editContext = new EditContext(flightForm);
    }

    public class FlightForm
    {
        [Required(ErrorMessage = "Flight Number required.")]
        public string FlightNumber { get; set; }

        [Required(ErrorMessage = "Departure Date required.")]
        public DateOnly? DepartureDate { get; set; }

        [Required(ErrorMessage = "Departure Time required.")]
        public TimeOnly? DepartureTime { get; set; }

        [Required(ErrorMessage = "Arrival Date required.")]
        public DateOnly? ArrivalDate { get; set; }

        [Required(ErrorMessage = "Arrival Time required.")]
        public TimeOnly? ArrivalTime { get; set; }
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/form/time-input#validations)

### Events: ValueChanged

This event fires on every user keystroke/selection that changes the `TimeInput` value.

<img src="https://i.imgur.com/G1QQGsB.png" alt="Blazor Bootstrap: Time Input Component - Events: ValueChanged" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <TimeInput TValue="TimeOnly" Value="time1" ValueExpression="() => time1" ValueChanged="(value) => TimeChanged(value)" />
</div>
<div class="mb-3">Changed time: @time1</div>

@code {
    private TimeOnly time1 = new TimeOnly(10, 0); // 10:00 AM

    private void TimeChanged(TimeOnly timeOnly)
    {
        time1 = timeOnly;
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/form/time-input#event-value-changed)

### Restrict the date field based on the entry in another date field

One common scenario is that the time fields are restricted based on the entry in another time field. 
In the example below, we restrict the arrival time based on the selection of departure.

<img src="https://i.imgur.com/lauXJ7H.png" alt="Blazor Bootstrap:- Time Input Component - Restrict the date field based on the entry in another date field" />

```cshtml {} showLineNumbers
@using System.ComponentModel.DataAnnotations
```

```css {} showLineNumbers
<style>
    .valid.modified:not([type=checkbox]) {
        outline: 1px solid #26b050;
    }

    .invalid {
        outline: 1px solid red;
    }

    .validation-message {
        color: red;
    }
</style>
```

```cshtml {} showLineNumbers
<EditForm EditContext="@editContext" OnValidSubmit="HandleValidSubmit" novalidate>
    <DataAnnotationsValidator />

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Departure Time: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <TimeInput TValue="TimeOnly?"
                       Value="flightForm.DepartureTime"
                       ValueExpression="() => flightForm.DepartureTime"
                       ValueChanged="(value) => DepartureTimeChanged(value)" />
            <ValidationMessage For="@(() => flightForm.DepartureTime)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Arrival Time: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <TimeInput @ref="arrivalTimeInput" TValue="TimeOnly?"
                       @bind-Value="flightForm.ArrivalTime"
                       EnableMinMax="true"
                       Min="arrivalMinTime"
                       Max="arrivalMaxTime"
                       Disabled="true" />
            <ValidationMessage For="@(() => flightForm.ArrivalTime)" />
        </div>
    </div>

    <div class="row">
        <div class="col-md-12 text-right">
            <Button Type="ButtonType.Button" Color="ButtonColor.Secondary" Class="float-end" @onclick="ResetForm">Reset</Button>
            <Button Type="ButtonType.Submit" Color="ButtonColor.Success" Class="float-end me-2">Submit</Button>
        </div>
    </div>
</EditForm>
```

```cshtml {} showLineNumbers
@code {
    TimeInput<TimeOnly?> arrivalTimeInput = default!;

    private FlightForm flightForm = new();
    private EditContext editContext;

    private TimeOnly? arrivalMinTime;
    private TimeOnly? arrivalMaxTime;

    [Inject] ToastService _toastService { get; set; }

    protected override void OnInitialized()
    {
        editContext = new EditContext(flightForm);
        base.OnInitialized();
    }

    private void DepartureTimeChanged(TimeOnly? departureTime)
    {
        if (departureTime is null || !departureTime.HasValue)
        {
            flightForm.DepartureTime = null;
            flightForm.ArrivalTime = null;
            arrivalMinTime = null;
            arrivalMaxTime = null;
            arrivalTimeInput.Disable();

            return;
        }

        flightForm.DepartureTime = departureTime;
        flightForm.ArrivalTime = null;
        arrivalMinTime = departureTime.Value.AddHours(1);
        arrivalMaxTime = departureTime.Value.AddHours(12);
        arrivalTimeInput.Enable();
    }

    public void HandleValidSubmit()
    {
        var toastMessage = new ToastMessage
        (
            type: ToastType.Success,
            iconName: IconName.Check2All,
            title: "Success!",
            helpText: $"{DateTime.Now.ToLocalTime()}",
            message: "Flight schedule created."
        );
        _toastService.Notify(toastMessage);
    }

    private void ResetForm()
    {
        flightForm = new();
        editContext = new EditContext(flightForm);
    }

    public class FlightForm
    {
        [Required(ErrorMessage = "Departure Time required.")]
        public TimeOnly? DepartureTime { get; set; }

        [Required(ErrorMessage = "Arrival Time required.")]
        public TimeOnly? ArrivalTime { get; set; }
    }
}
```
