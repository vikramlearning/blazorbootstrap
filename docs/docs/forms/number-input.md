---
title: Blazor Number Input Components
description: Blazor Bootstrap `NumberInput` component is built around HTML input of `type="number"` that prevents the user input based on the parameters set.
image: https://i.imgur.com/iUNBkki.png

sidebar_label: Number Input
sidebar_position: 2
---

# Blazor Number Input

Blazor Bootstrap `NumberInput` component is built around HTML input of `type="number"` that prevents the user input based on the parameters set.

## Parameters

| Name | Type | Default | Required | Description |
|:--|:--|:--|:--|:--|
| AllowNegativeNumbers | bool | false | | Allows negative numbers. By default, negative numbers are not allowed. |
| AutoComplete | bool | false | | Indicates whether the NumberInput can complete the values automatically by the browser. |
| Disabled | bool | false | | Gets or sets the disabled. |
| EnableMinMax | bool | false | | Determines whether to restrict the user input to Min and Max range. If true, restricts the user input between the Min and Max range. Else accepts the user input. |
| Max| TValue | | | Gets or sets the max. Max ignored if EnableMinMax="false". |
| Min| TValue | | | Gets or sets the min. Min ignored if EnableMinMax="false". |
| Placeholder | string? | null | | Gets or sets the placeholder. |
| Step | double? | null | | Gets or sets the step. |
| TextAlignment | `Alignment` | `Alignment.None` | | Gets or sets the text alignment. |
| Value | TValue | | | Gets or sets the value. |

## Methods

| Name | Description |
|:--|:--|
| Disable | Disables number input. |
| Enable | Enables number input. |

## Events

| Name | Description |
|:--|:--|
| ValueChanged | This is event fires on every user keystroke that changes the `NumberInput` value. |

## Examples

### Basic usage

By default, `e + -` are blocked. For all integral numeric types, dot `.` is blocked.

<img src="https://i.imgur.com/XXD0xF4.png" alt="Blazor Bootstrap: Number Input Component - Basic usage" />

```cshtml {3} showLineNumbers
<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput TValue="int" @bind-Value="@amount" Placeholder="Enter amount" />
</div>
<div class="mb-3">Entered Amount: @amount</div>
```

```cs showLineNumbers
@code {
    private int amount;
}
```

### Generic type

`NumberInput` is a generic component. Always specify the exact type. In the below example <b>TValue</b> is set to `int`, `int?`, `float`, `float?`, `double`, `double?`, `decimal`, and `decimal?`.

<img src="https://i.imgur.com/iUNBkki.png" alt="Blazor Bootstrap: Number Input Component - Generic type" />

```cshtml {3,8,13,18,23,28,33,38} showLineNumbers
<div class="mb-3">
    <label class="form-label">Enter int number</label>
    <NumberInput TValue="int" @bind-Value="@amount" />
</div>

<div class="mb-3">
    <label class="form-label">Enter int? number</label>
    <NumberInput TValue="int?" @bind-Value="@amount2" />
</div>

<div class="mb-3">
    <label class="form-label">Enter float number</label>
    <NumberInput TValue="float" @bind-Value="@amount3" />
</div>

<div class="mb-3">
    <label class="form-label">Enter float? number</label>
    <NumberInput TValue="float?" @bind-Value="@amount4" />
</div>

<div class="mb-3">
    <label class="form-label">Enter double number</label>
    <NumberInput TValue="double" @bind-Value="@amount5" />
</div>

<div class="mb-3">
    <label class="form-label">Enter double? number</label>
    <NumberInput TValue="double?" @bind-Value="@amount6" />
</div>

<div class="mb-3">
    <label class="form-label">Enter decimal number</label>
    <NumberInput TValue="decimal" @bind-Value="@amount7" />
</div>

<div class="mb-3">
    <label class="form-label">Enter decimal? number</label>
    <NumberInput TValue="decimal?" @bind-Value="@amount8" />
</div>
```

```cs {} showLineNumbers
@code {
    private int amount;
    private int? amount2;
    private float amount3;
    private float? amount4;
    private double amount5;
    private double? amount6;
    private decimal amount7;
    private decimal? amount8;
}
```

### Enable min and max

Set `EnableMinMax="true"` and set the `Min` and `Max` parameters to restrict the user input between the Min and Max range.

<img src="https://i.imgur.com/v4SWtWs.png" alt="Blazor Bootstrap: Number Input Component - Enable min and max" />

:::caution NOTE
If the user tries to enter a number in the <b>NumberInput</b> field which is out of range, then it will override with Min or Max value based on the context. 
If the user input is less than the Min value, then it will override with the Min value. 
If the user input exceeds the Max value, it will override with the Max value.
:::

```cshtml {3} showLineNumbers
<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput TValue="decimal?" @bind-Value="@amount" EnableMinMax="true" Min="10" Max="500" Placeholder="Enter amount" />
    <span class="form-text">Tip: The amount must be between 10 and 500.</span>
</div>
<div class="mb-3">Entered Amount: @amount</div>
```

```cs {} showLineNumbers
@code {
    private decimal? amount;
}
```

### Step

The `Step` sets the stepping interval when clicking the up and down spinner buttons. If not explicitly included, `Step` defaults to <b>1</b>.

<img src="https://i.imgur.com/Lq10NP1.png" alt="Blazor Bootstrap: Number Input Component - Step" />

```cshtml {3,10} showLineNumbers
<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput TValue="int?" @bind-Value="@amount" Step="10" Placeholder="Enter amount" />
    <span class="form-text">Info: Here <code>Step</code> parameter is set to <b>10</b>.</span>
</div>
<div class="mb-3">Entered Amount: @amount</div>

<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput TValue="decimal?" @bind-Value="@amount2" Step="2.5" Placeholder="Enter amount" />
    <span class="form-text">Info: Here <code>Step</code> parameter is set to <b>2.5</b>.</span>
</div>
<div class="mb-3">Entered Amount: @amount2</div>
```

```cs {} showLineNumbers
@code {
    private int? amount;
    private decimal? amount2;
}
```

### Text alignment

You can change the text alignment according to your need. Use the `TextAlignment` parameter to set the alignment. In the below example, alignment is set to center and end.

<img src="https://i.imgur.com/hbHHfKr.png" alt="Blazor Bootstrap: Number Input Component - Text alignment" />

```cshtml {3,9} showLineNumbers
<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput TValue="int" @bind-Value="@amount" TextAlignment="Alignment.Center" Placeholder="Enter amount" />
</div>
<div class="mb-3">Entered Amount: @amount</div>

<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput TValue="decimal" @bind-Value="@amount2" TextAlignment="Alignment.End" Placeholder="Enter amount" />
</div>
<div class="mb-3">Entered Amount: @amount2</div>
```

```cs {} showLineNumbers
@code {
    private int amount;
    private decimal amount2 = 2.34M;
}
```

### Allow negative numbers

By default, negative numbers are not allowed. Set the `AllowNegativeNumbers` parameter to true to allow the negative numbers.

<img src="https://i.imgur.com/jmtUxzB.png" alt="Blazor Bootstrap: Number Input Component - Allow negative numbers" />

```cshtml {3} showLineNumbers
<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput TValue="int" @bind-Value="@amount" AllowNegativeNumbers="true" Placeholder="Enter amount" />
    <span class="form-text">Tip: Negative numbers are also allowed.</span>
</div>
<div class="mb-3">Entered Amount: @amount</div>
```

```cs {} showLineNumbers
@code {
    private int amount;
}
```

### Disable

Use the `Disable` parameter to disable the `NumberInput`. Also, use <b>Enable()</b> and <b>Disable()</b> methods to enable and disable the `NumberInput`.

<img src="https://i.imgur.com/ezc3VBH.png" alt="Blazor Bootstrap: Number Input Component - Disable" />

```cshtml {3} showLineNumbers
<div class="mb-3">
    <label class="form-label">Amount</label>
    <NumberInput @ref="numberInput" TValue="int?" @bind-Value="@amount" Disabled="true" Placeholder="Enter amount" />
</div>

<Button Color="ButtonColor.Primary" @onclick="enableNumberInput"> Enable </Button>
<Button Color="ButtonColor.Secondary" @onclick="disableNumberInput"> Disable </Button>
```

```cs {2,5,7} showLineNumbers
@code {
    private NumberInput<int?> numberInput;
    private int? amount;

    private void enableNumberInput() => numberInput.Enable();

    private void disableNumberInput() => numberInput.Disable();
}
```

### Validations

Like any other blazor input components, `NumberInput` supports validations. Add the DataAnnotations on the `NumberInput` component to validate the user input before submitting the form. In the below example, we used <b>Required</b> and <b>Range</b> attributes.

<img src="https://i.imgur.com/K7bepKc.png" alt="Blazor Bootstrap: Number Input Component - Validations" />

```cshtml {17,18,23,24,31,32,39,40} showLineNumbers
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
        <label class="col-md-2 col-form-label">Item Price: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <NumberInput TValue="decimal?" Value="invoice.Price" ValueExpression="() => invoice.Price" ValueChanged="(value) => PriceChanged(value)" Placeholder="Enter price" />
            <ValidationMessage For="@(() => invoice.Price)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Item Discount:</label>
        <div class="col-md-10">
            <NumberInput TValue="decimal?" Value="invoice.Discount" ValueExpression="() => invoice.Discount" ValueChanged="(value) => DiscountChanged(value)" Placeholder="Enter discount" />
            <ValidationMessage For="@(() => invoice.Discount)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label class="col-md-2 col-form-label">Total Amount: <span class="text-danger">*</span></label>
        <div class="col-md-10">
            <NumberInput TValue="decimal?" @bind-Value="invoice.Total" Disabled="true" Placeholder="Enter total" />
            <ValidationMessage For="@(() => invoice.Total)" />
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

```cs {2-3,7-8,36,46,49-61} showLineNumbers
@code {
    private Invoice invoice = new();
    private EditContext editContext;

    protected override void OnInitialized()
    {
        editContext = new EditContext(invoice);
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        CalculateToatl();
        base.OnParametersSet();
    }

    private void PriceChanged(decimal? value)
    {
        invoice.Price = value;
        CalculateToatl();
    }

    private void DiscountChanged(decimal? value)
    {
        invoice.Discount = value;
        CalculateToatl();
    }

    private void CalculateToatl()
    {
        var price = invoice.Price.HasValue ? invoice.Price.Value : 0;
        var discount = invoice.Discount.HasValue ? invoice.Discount.Value : 0;
        invoice.Total = price - discount;
    }

    public void HandleOnValidSubmit()
    {
        Console.WriteLine($"Price: {invoice.Price}");
        Console.WriteLine($"Discount: {invoice.Discount}");
        Console.WriteLine($"Total: {invoice.Total}");
    }

    private void ResetForm()
    {
        invoice = new();
        editContext = new EditContext(invoice);
    }

    public class Invoice
    {
        [Required(ErrorMessage = "Price required.")]
        [Range(60, 500, ErrorMessage = "Price should be between 60 and 500.")]
        public decimal? Price { get; set; } = 232M;

        [Range(0, 50, ErrorMessage = "Discount should be between 0 and 50.")]
        public decimal? Discount { get; set; }

        [Required(ErrorMessage = "Amount required.")]
        [Range(10, 500, ErrorMessage = "Total should be between 60 and 500.")]
        public decimal? Total { get; set; }
    }
}
```

### Events: ValueChanged

This event fires on every user keystroke that changes the `NumberInput` value.

<img src="https://i.imgur.com/1DGW8dG.png" alt="Blazor Bootstrap: Number Input Component - Events: ValueChanged" />

```cshtml {4} showLineNumbers
<div class="row mb-3">
    <label class="col-md-2 col-form-label">Item Price: <span class="text-danger">*</span></label>
    <div class="col-md-10">
        <NumberInput TValue="decimal?" Value="price" ValueExpression="() => price" ValueChanged="(value) => PriceChanged(value)" Placeholder="Enter price" />
    </div>
</div>
<div>
    @displayPrice
</div>
```

```cs {5-9} showLineNumbers
@code {
    private decimal? price = 10M;
    private string displayPrice;

    private void PriceChanged(decimal? value)
    {
        price = value; // this is mandatory
        displayPrice = $"Price: {value}, changed at {DateTime.Now.ToLocalTime()}.";
    }
}
```