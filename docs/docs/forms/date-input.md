---
title: Blazor Date Input Component
description: Blazor Bootstrap `DateInput` component is constructed using an HTML input of `type="date"` which limits user input based on pre-defined parameters. This component enables users to input a date using a text box with validation or a special date picker interface.
image: https://i.imgur.com/1mVjqQv.png

sidebar_label: Date Input
sidebar_position: 3
---

# Blazor Date Input

Blazor Bootstrap `DateInput` component is constructed using an HTML input of `type="date"` which limits user input based on pre-defined parameters. This component enables users to input a date using a text box with validation or a special date picker interface.

<img src="https://i.imgur.com/1mVjqQv.png" alt="Blazor Bootstrap: Date Input Component" />

## Parameters

| Name | Type | Default | Required | Description | Added Version |
|:--|:--|:--|:--|:--|:--|
| AutoComplete | bool | false | | Indicates whether the DateInput can complete the values automatically by the browser. | 1.5.0 |
| Disabled | bool | false | | Gets or sets the disabled. | 1.5.0 |
| EnableMinMax | bool | false | | Determines whether to restrict the user input to Min and Max range. If true, restricts the user input between the Min and Max range. Else accepts the user input. | 1.5.0 |
| Max| TValue | | | Gets or sets the max. Max ignored if EnableMinMax="false". | 1.5.0 |
| Min| TValue | | | Gets or sets the min. Min ignored if EnableMinMax="false". | 1.5.0 |
| Placeholder | string? | null | | Gets or sets the placeholder. | 1.5.0 |
| Value | TValue | | | Gets or sets the value. | 1.5.0 |

## Methods

| Name | Description | Added Version |
|:--|:--|:--|
| Disable | Disables date input. | 1.5.0 |
| Enable | Enables date input. | 1.5.0 |

## Events

| Name | Description |
|:--|:--|
| ValueChanged | This event fired on every user keystroke that changes the `DateInput` value. |

## Examples

### Basic usage

:::caution NOTE
The input UI generally varies from browser to browser. In unsupported browsers, the control degrades gracefully to `type="text"`.
:::

<img src="https://i.imgur.com/1mVjqQv.png" alt="Blazor Bootstrap: Date Input Component - Basic usage" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <DateInput TValue="DateOnly" @bind-Value="@date1" Placeholder="Enter Date" />
</div>
<div class="mb-3">Entered date: @date1</div>
```

```cs showLineNumbers
@code {
    private DateOnly date1 = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
}
```

[See demo here](https://demos.blazorbootstrap.com/form/date-input#basic-usage)

### Generic type

The Blazor Bootstrap DateInput component supports several data types: `DateOnly`, `DateOnly?`, `DateTime`, and `DateTime?`. This allows flexible component usage to accommodate various data types in Blazor applications.

In the below example, TValue is set to `DateOnly`, `DateOnly?`, `DateTime`, and `DateTime?`.

<img src="https://i.imgur.com/KyyqyNf.png" alt="Blazor Bootstrap: Date Input Component - Generic type" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <strong>DateOnly</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateOnly" @bind-Value="@date1" Placeholder="Enter Date" />
</div>
<div class="mb-3">Entered date: @date1</div>

<div class="mb-3">
    <strong>DateOnly?</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateOnly?" @bind-Value="@date2" Placeholder="Enter Date" />
</div>
<div class="mb-3">Entered date: @date2</div>

<div class="mb-3">
    <strong>DateTime</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateTime" @bind-Value="@date3" Placeholder="Enter Date" />
</div>
<div class="mb-3">Entered date: @date3</div>

<div class="mb-3">
    <strong>DateTime?</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateTime?" @bind-Value="@date4" Placeholder="Enter Date" />
</div>
<div class="mb-3">Entered date: @date4</div>
```

```cs {} showLineNumbers
@code {
    private DateOnly date1 = DateOnly.FromDateTime(DateTime.Now.AddMonths(3));
    private DateOnly? date2;
    private DateTime date3 = DateTime.Now.AddMonths(3);
    private DateTime? date4;
}
```

[See demo here](https://demos.blazorbootstrap.com/form/date-input#generic-type)

### Enable min and max

Set `EnableMinMax="true"` and set the `Min` and `Max` parameters to restrict the user input between the Min and Max range.

<img src="https://i.imgur.com/rZpYrFL.png" alt="Blazor Bootstrap: Date Input Component - Enable min and max" />

:::caution NOTE
If the user tries to enter a number in the <b>DateInput</b> field which is out of range, then it will override with Min or Max value based on the context. 
If the user input is less than the Min value, then it will override with the Min value. 
If the user input exceeds the Max value, it will override with the Max value.
:::

```cshtml {} showLineNumbers
<div class="mb-3">
    <strong>DateOnly</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateOnly" @bind-Value="@date1" EnableMinMax="true" Min="@min1" Max="@max1" Placeholder="Enter Date" />
</div>
<div class="mb-3">Min date: @min1</div>
<div class="mb-3">Max date: @max1</div>
<div class="mb-3">Entered date: @date1</div>

<div class="mb-3">
    <strong>DateOnly?</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateOnly?" @bind-Value="@date2" EnableMinMax="true" Min="@min2" Max="@max2" Placeholder="Enter Date" />
</div>
<div class="mb-3">Min date: @min2</div>
<div class="mb-3">Max date: @max2</div>
<div class="mb-3">Entered date: @date2</div>

<div class="mb-3">
    <strong>DateTime</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateTime" @bind-Value="@date3" EnableMinMax="true" Min="@min3" Max="@max3" Placeholder="Enter Date" />
</div>
<div class="mb-3">Min date: @min3</div>
<div class="mb-3">Max date: @max3</div>
<div class="mb-3">Entered date: @date3</div>

<div class="mb-3">
    <strong>DateTime?</strong>:
</div>
<div class="mb-3">
    <DateInput TValue="DateTime?" @bind-Value="@date4" EnableMinMax="true" Min="@min4" Max="@max4" Placeholder="Enter Date" />
</div>
<div class="mb-3">Min date: @min4</div>
<div class="mb-3">Max date: @max4</div>
<div class="mb-3">Entered date: @date4</div>
```

```cs {} showLineNumbers
@code {
    private DateTime date = DateTime.Now.AddMonths(3);
    private DateTime min = DateTime.Now.AddMonths(-1);
    private DateTime max = DateTime.Now.AddYears(1);

    private DateOnly date1, min1, max1;
    private DateOnly? date2, min2, max2;
    private DateTime date3, min3, max3;
    private DateTime? date4, min4, max4;

    protected override void OnInitialized()
    {
        date1 = DateOnly.FromDateTime(date);
        min1 = DateOnly.FromDateTime(min);
        max1 = DateOnly.FromDateTime(max);

        date2 = null;
        min2 = DateOnly.FromDateTime(min);
        max2 = DateOnly.FromDateTime(max);

        date3 = DateTime.Now.AddMonths(3);
        min3 = min;
        max3 = max;

        date4 = null;
        min4 = min;
        max4 = max;
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/form/date-input#enable-max-min)

### Disable

Use the `Disabled` parameter to disable the `DateInput`. Also, use <b>Enable()</b> and <b>Disable()</b> methods to enable and disable the `DateInput`.

<img src="https://i.imgur.com/wlEZFWI.png" alt="Blazor Bootstrap: Date Input Component - Disable" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <DateInput @ref="dateInput" TValue="DateOnly" @bind-Value="@date1" Disabled="true" Placeholder="Enter Date" />
</div>
<div class="mb-3">Entered date: @date1</div>

<Button Color="ButtonColor.Primary" @onclick="enableDateInput"> Enable </Button>
<Button Color="ButtonColor.Secondary" @onclick="disableDateInput"> Disable </Button>
```

```cs {} showLineNumbers
@code {
    private DateInput<DateOnly> dateInput;

    private DateOnly date1 = DateOnly.FromDateTime(DateTime.Now);

    private void enableDateInput() => dateInput.Enable();

    private void disableDateInput() => dateInput.Disable();
}
```

[See demo here](https://demos.blazorbootstrap.com/form/date-input#disable)

### Validations

Like any other blazor input component, `DateInput` component supports validations. Use the Use the **DataAnnotations** to validate the user input before submitting the form. In the below example, we used the `Required` attributes.

<img src="https://i.imgur.com/V4SWntV.png" alt="Blazor Bootstrap: Date Input Component - Validations" />

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

<EditForm EditContext="@editContext" OnValidSubmit="HandleOnValidSubmit">
    <DataAnnotationsValidator />

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Invoice Date: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <DateInput TValue="DateOnly?" @bind-Value="invoice.InvoiceDate" />
            <ValidationMessage For="@(() => invoice.InvoiceDate)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Customer Name: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <InputText class="form-control" @bind-Value="invoice.CustomerName" Placeholder="Enter Customer Name" />
            <ValidationMessage For="@(() => invoice.CustomerName)" />
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
    private Invoice invoice = new();
    private EditContext editContext;

    protected override void OnInitialized()
    {
        editContext = new EditContext(invoice);
        base.OnInitialized();
    }

    public void HandleOnValidSubmit()
    {
        Console.WriteLine($"Invoice Date: {invoice.InvoiceDate}");
        Console.WriteLine($"Customer Name: {invoice.CustomerName}");
    }

    private void ResetForm()
    {
        invoice = new();
        editContext = new EditContext(invoice);
    }

    public class Invoice
    {
        [Required(ErrorMessage = "Invoice Date required.")]
        public DateOnly? InvoiceDate { get; set; }

        [Required(ErrorMessage = "Customer Name required.")]
        public string CustomerName { get; set; }
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/form/date-input#validations)

### Events: ValueChanged

This event fires on every user keystroke/selection that changes the `DateInput` value.

<img src="https://i.imgur.com/3OFrZJX.png" alt="Blazor Bootstrap: Date Input Component - Events: ValueChanged" />

```cshtml {} showLineNumbers
<div class="mb-3">
    <DateInput TValue="DateOnly" Value="date1" ValueExpression="() => date1" ValueChanged="(value) => DateChanged(value)" />
</div>
<div class="mb-3">Changed date: @date1</div>
```

```cs {} showLineNumbers
@code {
    private DateOnly date1 = DateOnly.FromDateTime(DateTime.Now);

    private void DateChanged(DateOnly dateOnly)
    {
        date1 = dateOnly;    
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/form/date-input#event-value-changed)