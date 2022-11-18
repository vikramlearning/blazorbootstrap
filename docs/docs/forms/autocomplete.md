---
title: Blazor AutoComplete Component
description: Blazor Bootstrap autocomplete component is a textbox that offers the users suggestions as they type from the data source. And it supports client-side and server-side filtering.
image: https://i.imgur.com/gRcdvc0.png

sidebar_label: Auto Complete
sidebar_position: 1
---

# Blazor Auto Complete

Blazor Bootstrap autocomplete component is a textbox that offers the users suggestions as they type from the data source. And it supports client-side and server-side filtering.

## Parameters
| Name | Type | Default | Required | Descritpion |
|--|--|--|--|--|
| DataProvider | delegate | null | ✔️ | DataProvider is for items to render. The provider should always return an instance of `AutoCompleteDataProviderResult`, and null is not allowed. |
| Disabled | bool | false | | Is AutoComplete disabled. |
| Placeholder | string | null | | AutoComplete placeholder. |
| PropertyName | string | null | ✔️ | AutoComplete data text property name. |
| Size | enum | `AutoCompleteSize.Default` | | Use `AutoCompleteSize.Default` or `AutoCompleteSize.Large` or `AutoCompleteSize.Small` |
| StringComparison | enum | `StringComparison.OrdinalIgnoreCase` | | Specifies the culture, case, and sort rules to be used. Use `StringComparison.CurrentCulture` or `StringComparison.CurrentCultureIgnoreCase` or `StringComparison.InvariantCulture` or `StringComparison.InvariantCultureIgnoreCase` or `StringComparison.Ordinal` or `StringComparison.OrdinalIgnoreCase`. | 
| StringFilterOperator | enum | `StringFilterOperator.Contains` | | Use `StringFilterOperator.Equals` or `StringFilterOperator.Contains` or `StringFilterOperator.StartsWith` or `StringFilterOperator.EndsWith` |
| Value | string | null | ✔️ | AutoComplete value. |
| ValueExpression | expression | null | | AutoComplete value expression. |

## Methods

| Name | Returns | Arguments | Description |
|--|--|--|
| Disable() | void | N/A | Disables autocomplete. |
| Enable() | void | N/A | Enables autocomplete. |
| RefreshDataAsync() | Task | N/A | Refresh the autocomplete data. |
| ResetAsync() | Task | N/A | Resets the autocomplete selection. |

## Callback Events

| Name | Description |
|--|--|
| OnChanged | This event fires immediately when the autocomplete selection changes by the user. |
| ValueChanged | This event fires on every user keystroke that changes the textbox value. |

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
```cs {34,36,39,41} showLineNumbers
@code {
    private string customerName;

    public IEnumerable<Customer> customers;

    protected override void OnInitialized()
    {
        customers = new List<Customer> {
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

    private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
    {
        return await Task.FromResult(request.ApplyTo(customers.OrderBy(customer => customer.CustomerName)));
    }

    private void OnAutoCompleteChanged(Customer customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
        Console.WriteLine($"Data null: {customer is null}.");
    }
}
```

```cs showLineNumbers
public record Customer(int CustomerId, string CustomerName);
```

[See demo here](https://demos.getblazorbootstrap.com/grid#client-side-data)

### Client side data with StringComparision

In the below example, `StringComparision.Ordinal` is used to make the filter case-sensitive.

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
```cs {34,36,39,41} showLineNumbers
@code {
    private string customerName;

    public IEnumerable<Customer> customers;

    protected override void OnInitialized()
    {
        customers = new List<Customer> {
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

    private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
    {
        return await Task.FromResult(request.ApplyTo(customers.OrderBy(customer => customer.CustomerName)));
    }

    private void OnAutoCompleteChanged(Customer customer)
    {
        // TODO: handle your own logic

        // NOTE: do null check
        Console.WriteLine($"'{customer?.CustomerName}' selected.");
        Console.WriteLine($"Data null: {customer is null}.");
    }
}
```

```cs showLineNumbers
public record Customer(int CustomerId, string CustomerName);
```
:::info
By default, `StringComparison.OrdinalIgnoreCase` is used to compare culture-agnostic and case-insensitive string matching.
:::

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
```cs {6,8-9,12,14} showLineNumbers
@code {
    private string customerName;

    [Inject] ICustomerService _customerService { get; set; }

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
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#server-side-data)

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

```cs {18,20-21,24,26} showLineNumbers
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

[See demo here](https://demos.getblazorbootstrap.com/grid#validations)