---
title: Blazor AutoComplete Component
description: Blazor Bootstrap `AutoComplete` component is a textbox that offers the users suggestions as they type from the data source. And it supports client-side and server-side filtering.
image: https://i.imgur.com/gRcdvc0.png

sidebar_label: Auto Complete
sidebar_position: 1
---

# Blazor Auto Complete

Blazor Bootstrap autocomplete component is a textbox that offers the users suggestions as they type from the data source. And it supports client-side and server-side filtering.

## Parameters
| Name | Type | Default | Required | Descritpion | Added Version |
|:--|:--|:--|:--|:--|:--|
| DataProvider | delegate | null | ✔️ | DataProvider is for items to render. The provider should always return an instance of `AutoCompleteDataProviderResult`, and null is not allowed. | 0.4.0 |
| Disabled | bool | false | | Is AutoComplete disabled. | 0.4.0 |
| EmptyText | string | `No records found.` | ✔️ | Gets or sets the empty text. | 1.10.2 |
| LoadingText | string | `Loading...` | ✔️ | Gets or sets the empty text. | 1.10.2 |
| Placeholder | string | null | | AutoComplete placeholder. | 0.4.0 |
| PropertyName | string | null | ✔️ | AutoComplete data text property name. | 0.4.0 |
| Size | enum | `AutoCompleteSize.Default` | | Use `AutoCompleteSize.Default` or `AutoCompleteSize.Large` or `AutoCompleteSize.Small` | 0.4.0 |
| StringComparison | enum | `StringComparison.OrdinalIgnoreCase` | | Specifies the culture, case, and sort rules to be used. Use `StringComparison.CurrentCulture` or `StringComparison.CurrentCultureIgnoreCase` or `StringComparison.InvariantCulture` or `StringComparison.InvariantCultureIgnoreCase` or `StringComparison.Ordinal` or `StringComparison.OrdinalIgnoreCase`. | 0.4.1 |
| StringFilterOperator | enum | `StringFilterOperator.Contains` | | Use `StringFilterOperator.Equals` or `StringFilterOperator.Contains` or `StringFilterOperator.StartsWith` or `StringFilterOperator.EndsWith` | 0.4.1 |
| Value | string | null | ✔️ | AutoComplete value. | 0.4.0 |
| ValueExpression | expression | null | | AutoComplete value expression. | 0.4.0 |

## Methods

| Name | Returns | Description | Added Version |
|:--|:--|:--|:--|
| Disable() | void | Disables autocomplete. | 0.4.0 |
| Enable() | void | Enables autocomplete. | 0.4.0 |
| RefreshDataAsync() | Task | Refresh the autocomplete data. | 0.4.0 |
| ResetAsync() | Task | Resets the autocomplete selection. | 0.4.0 |

## Events

| Name | Description | Added Version |
|:--|:--|:--|
| OnChanged | This event fires immediately when the autocomplete selection changes by the user. | 0.4.0 |
| ValueChanged | This event fires on every user keystroke that changes the textbox value. | 0.4.0 |

## Keyboard Navigation

Blazor Bootstrap autocomplete component supports the following keyboard shortcuts to initiate various actions.

| Key | Description | Added Version |
|:--|:--|:--|
| <kbd>Esc</kbd> | Closes the popup list when it is in an open state. | 1.3.1 |
| <kbd>Up arrow</kbd> | Focuses on the previous item in the list. | 1.3.1 |
| <kbd>Down arrow</kbd> | Focuses on the next item in the list. | 1.3.1 |
| <kbd>Home</kbd> | Focuses on the first item in the list. | 1.3.1 |
| <kbd>End</kbd> | Focuses on the last item in the list. | 1.3.1 |
| <kbd>Enter</kbd> | Selects the currently focused item. | 1.3.1 |

## Examples

### Client side data

<img src="https://i.imgur.com/gRcdvc0.png" alt="Blazor Bootstrap AutoComplete Component - Client side data" />

```cshtml {3-8} showLineNumbers
<div class="row">
    <div class="col-md-5 col-sm-12">
        <AutoComplete @bind-Value="customerName"
                      TItem="Customer"
                      DataProvider="CustomersDataProvider"
                      PropertyName="CustomerName"
                      Placeholder="Search a customer..."
                      OnChanged="(Customer customer) => OnAutoCompleteChanged(customer)" />
    </div>
</div>
```

```cs {6-12} showLineNumbers
@code {
    private string customerName;

    public IEnumerable<Customer> customers;

    private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
    {
        if (customers is null) // pull customers only one time for client-side autocomplete
            customers = GetCustomers(); // call a service or an API to pull the customers

        return await Task.FromResult(request.ApplyTo(customers.OrderBy(customer => customer.CustomerName)));
    }

    private IEnumerable<Customer> GetCustomers()
    {
        return new List<Customer> {
            new(1, "Pich S"),
            new(2, "sfh Sobi"),
            new(3, "Jojo chan"),
            new(4, "Jee ja"),
            new(5, "Rose Canon"),
            new(6, "Manju A"),
            new(7, "Bandita PA"),
            new(8, "Sagar Adil"),
            new(9, "Isha Wang"),
            new(10, "Daina JJ"),
            new(11, "Komala Mug"),
            new(12, "Dikshita BD"),
            new(13, "Neha Gosar"),
            new(14, "Preeti S"),
            new(15, "Sagar Seth"),
            new(16, "Vinayak MM"),
            new(17, "Vijaya Lakhsmi"),
            new(18, "Jahan K"),
            new(19, "Joy B"),
            new(20, "Zaraiah C"),
            new(21, "Laura L"),
            new(22, "Punith ES")
        };
    }

    private void OnAutoCompleteChanged(Customer customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
    }
}
```

```cs showLineNumbers
public record Customer(int CustomerId, string CustomerName);
```

[See demo here](https://demos.blazorbootstrap.com/autocomplete#client-side-data)

### Client side data with StringComparision

In the below example, `StringComparision.Ordinal` is used to make the filter case-sensitive.

:::info
By default, `StringComparison.OrdinalIgnoreCase` is used to compare culture-agnostic and case-insensitive string matching.
:::

<img src="https://i.imgur.com/8YZzW9f.png" alt="Blazor Bootstrap AutoComplete Component - Client side data with StringComparision" />

```cshtml {8} showLineNumbers
<div class="row">
    <div class="col-md-5 col-sm-12">
        <AutoComplete @bind-Value="customerName"
                      TItem="Customer"
                      DataProvider="CustomersDataProvider"
                      PropertyName="CustomerName"
                      Placeholder="Search a customer..."
                      StringComparison="StringComparison.Ordinal"
                      OnChanged="(Customer customer) => OnAutoCompleteChanged(customer)" />
    </div>
</div>
```

```cs {6-12} showLineNumbers
@code {
    private string customerName;

    public IEnumerable<Customer> customers;

    private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
    {
        if (customers is null) // pull customers only one time for client-side autocomplete
            customers = GetCustomers(); // call a service or an API to pull the customers

        return await Task.FromResult(request.ApplyTo(customers.OrderBy(customer => customer.CustomerName)));
    }

    private IEnumerable<Customer> GetCustomers()
    {
        return new List<Customer> {
            new(1, "Pich S"),
            new(2, "sfh Sobi"),
            new(3, "Jojo chan"),
            new(4, "Jee ja"),
            new(5, "Rose Canon"),
            new(6, "Manju A"),
            new(7, "Bandita PA"),
            new(8, "Sagar Adil"),
            new(9, "Isha Wang"),
            new(10, "Daina JJ"),
            new(11, "Komala Mug"),
            new(12, "Dikshita BD"),
            new(13, "Neha Gosar"),
            new(14, "Preeti S"),
            new(15, "Sagar Seth"),
            new(16, "Vinayak MM"),
            new(17, "Vijaya Lakhsmi"),
            new(18, "Jahan K"),
            new(19, "Joy B"),
            new(20, "Zaraiah C"),
            new(21, "Laura L"),
            new(22, "Punith ES")
        };
    }

    private void OnAutoCompleteChanged(Customer customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
    }
}
```

```cs showLineNumbers
public record Customer(int CustomerId, string CustomerName);
```

[See demo here](https://demos.blazorbootstrap.com/autocomplete#client-side-data-with-string-comparision)

### Server side data

<img src="https://i.imgur.com/D5ox9um.png" alt="Blazor Bootstrap AutoComplete Component - Server side data" />

```cshtml {3-8} showLineNumbers
<div class="row">
    <div class="col-md-5 col-sm-12">
        <AutoComplete @bind-Value="customerName"
                      TItem="Customer"
                      DataProvider="CustomersDataProvider"
                      PropertyName="CustomerName"
                      Placeholder="Search a customer..."
                      OnChanged="(Customer customer) => OnAutoCompleteChanged(customer)" />
    </div>
</div>
```

```cs {6-10} showLineNumbers
@code {
    private string customerName;

    [Inject] ICustomerService _customerService { get; set; }

    private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
    {
        var customers = await _customerService.GetCustomers(request.Filter, request.CancellationToken); // API call
        return await Task.FromResult(new AutoCompleteDataProviderResult<Customer> { Data = customers, TotalCount = customers.Count() });
    }

    private void OnAutoCompleteChanged(Customer customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/autocomplete#server-side-data)

### Set default value

<img src="https://i.imgur.com/AMqyt8h.png" alt="Blazor Bootstrap AutoComplete Component - Set default value" />

```cshtml {} showLineNumbers
<div class="row">
    <div class="col-md-5 col-sm-12">
        <AutoComplete @bind-Value="customerName"
                      TItem="Customer"
                      DataProvider="CustomersDataProvider"
                      PropertyName="CustomerName"
                      Placeholder="Search a customer..."
                      OnChanged="(Customer customer) => OnAutoCompleteChanged(customer)" />
    </div>
</div>
```

```csharp
@code {
    private string customerName;

    public IEnumerable<Customer> customers;

    protected override void OnInitialized()
    {
        customerName = "Pich S";
    }

    private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
    {
        if (customers is null) // pull customers only one time for client-side autocomplete
            customers = GetCustomers(); // call a service or an API to pull the customers

        return await Task.FromResult(request.ApplyTo(customers.OrderBy(customer => customer.CustomerName)));
    }

    private IEnumerable<Customer> GetCustomers()
    {
        return new List<Customer> {
            new(1, "Pich S"),
            new(2, "sfh Sobi"),
            new(3, "Jojo chan"),
            new(4, "Jee ja"),
            new(5, "Rose Canon"),
            new(6, "Manju A"),
            new(7, "Bandita PA"),
            new(8, "Sagar Adil"),
            new(9, "Isha Wang"),
            new(10, "Daina JJ"),
            new(11, "Komala Mug"),
            new(12, "Dikshita BD"),
            new(13, "Neha Gosar"),
            new(14, "Preeti S"),
            new(15, "Sagar Seth"),
            new(16, "Vinayak MM"),
            new(17, "Vijaya Lakhsmi"),
            new(18, "Jahan K"),
            new(19, "Joy B"),
            new(20, "Zaraiah C"),
            new(21, "Laura L"),
            new(22, "Punith ES")
        };
    }

    private void OnAutoCompleteChanged(Customer customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/autocomplete#set-default-value)

### Validations

<img src="https://i.imgur.com/sMQ7Uc6.png" alt="Blazor Bootstrap AutoComplete Component - Validations - Data empty" />

<img src="https://i.imgur.com/4IzNcdp.png" alt="Blazor Bootstrap AutoComplete Component - Validations - Item selected" />

```cs showLineNumbers
@using System.ComponentModel.DataAnnotations
```

```html showLineNumbers
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

```cshtml {7-13} showLineNumbers
<EditForm EditContext="@_editContext" OnValidSubmit="HandleOnValidSubmit">
    <DataAnnotationsValidator />

    <div class="form-group row mb-2">
        <label for="supplier" class="col-md-2 col-form-label">Customer:</label>
        <div class="col-md-10">
            <AutoComplete @bind-Value="customerAddress.CustomerName"
                          TItem="Customer"
                          DataProvider="CustomersDataProvider"
                          PropertyName="CustomerName"
                          Placeholder="Search a customer..."
                          OnChanged="(Customer customer) => OnAutoCompleteChanged(customer)" />
            <ValidationMessage For="@(() => customerAddress.CustomerName)" />
        </div>
    </div>

    <div class="form-group row mb-3">
        <label for="name" class="col-md-2 col-form-label">Address:</label>
        <div class="col-md-10">
            <InputText id="name" class="form-control" @bind-Value="customerAddress.Address" />
            <ValidationMessage For="@(() => customerAddress.Address)" />
        </div>
    </div>

    <div class="row">
        <div class="col-md-12 text-right">
            <button type="submit" class="btn btn-success float-right">Submit</button>
        </div>
    </div>
</EditForm>
```

```cs {} showLineNumbers
@code {
    private CustomerAddress customerAddress = new();
    private EditContext _editContext;

    [Inject] ICustomerService _customerService { get; set; }

    protected override void OnInitialized()
    {
        _editContext = new EditContext(customerAddress);
        base.OnInitialized();
    }

    public void HandleOnValidSubmit()
    {
        Console.WriteLine($"Customer name is {customerAddress.CustomerName} and address is {customerAddress.Address}");
    }

    private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
    {
        var customers = await _customerService.GetCustomers(request.Filter); // API call
        return await Task.FromResult(new AutoCompleteDataProviderResult<Customer> { Data = customers, TotalCount = customers.Count() });
    }

    private void OnAutoCompleteChanged(Customer customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
        Console.WriteLine($"Data null: {customer is null}.");
    }

    public class CustomerAddress
    {
        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string Address { get; set; }
    }
}
```

[See demo here](https://demos.blazorbootstrap.com/autocomplete#validations)

### Disable

Use the <b>Disabled</b> parameter to disable the AutoComplete.

```cshtml {8,13-15} showLineNumbers
<div class="row mb-3">
    <div class="col-md-5 col-sm-12">
        <AutoComplete @bind-Value="customerName"
                      TItem="Customer2"
                      DataProvider="CustomersDataProvider"
                      PropertyName="CustomerName"
                      Placeholder="Search a customer..."
                      Disabled="@disabled"
                      OnChanged="(Customer2 customer) => OnAutoCompleteChanged(customer)" />
    </div>
</div>

<Button Color="ButtonColor.Primary" @onclick="Enable"> Enable </Button>
<Button Color="ButtonColor.Secondary" @onclick="Disable"> Disable </Button>
<Button Color="ButtonColor.Warning" @onclick="Toggle"> Toggle </Button>
```

```cs {3,21,23,25} showLineNumbers
@code {
    private string customerName = default!;
    private bool disabled = true;

    [Inject] ICustomerService _customerService { get; set; } = default!;

    private async Task<AutoCompleteDataProviderResult<Customer2>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer2> request)
    {
        var customers = await _customerService.GetCustomersAsync(request.Filter, request.CancellationToken); // API call
        return await Task.FromResult(new AutoCompleteDataProviderResult<Customer2> { Data = customers, TotalCount = customers.Count() });
    }

    private void OnAutoCompleteChanged(Customer2 customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
    }

    private void Enable() => disabled = false;

    private void Disable() => disabled = true;

    private void Toggle() => disabled = !disabled;
}
```

Also, use **Enable()** and **Disable()** methods to enable and disable the AutoComplete.

:::caution NOTE
Do not use both the **Disabled** parameter and **Enable()** & **Disable()** methods.
:::

```cshtml {3,13-14} showLineNumbers
<div class="row mb-3">
    <div class="col-md-5 col-sm-12">
        <AutoComplete @ref="autoComplete1" 
                      @bind-Value="customerName"
                      TItem="Customer2"
                      DataProvider="CustomersDataProvider"
                      PropertyName="CustomerName"
                      Placeholder="Search a customer..."
                      OnChanged="(Customer2 customer) => OnAutoCompleteChanged(customer)" />
    </div>
</div>

<Button Color="ButtonColor.Secondary" @onclick="Disable"> Disable </Button>
<Button Color="ButtonColor.Primary" @onclick="Enable"> Enable </Button>
```

```cs {2,21,23} showLineNumbers
@code {
    private AutoComplete<Customer2> autoComplete1 = default!;
    private string customerName = default!;

    [Inject] ICustomerService _customerService { get; set; } = default!;

    private async Task<AutoCompleteDataProviderResult<Customer2>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer2> request)
    {
        var customers = await _customerService.GetCustomersAsync(request.Filter, request.CancellationToken); // API call
        return await Task.FromResult(new AutoCompleteDataProviderResult<Customer2> { Data = customers, TotalCount = customers.Count() });
    }

    private void OnAutoCompleteChanged(Customer2 customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
    }

    private void Disable() => autoComplete1.Disable();

    private void Enable() => autoComplete1.Enable();
}
```

[See demo here](https://demos.blazorbootstrap.com/autocomplete#disable)
