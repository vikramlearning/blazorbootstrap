---
sidebar_label: AutoComplete
sidebar_position: 2
---

# AutoComplete

Use BlazorBootstrap's autocomplete component is a textbox that offers the users suggestions as they type from the data source. And it supports client-side and server-side filtering.

## Parameters
| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| DataProvider | `AutoCompleteDataProviderDelegate<TItem>` | | | |
| Disabled | bool | | | |
| Placeholder | string | | | |
| PropertyName | string | | | |
| Size | string | `AutoCompleteSize` | | |
| Value | string | | | |
| ValueExpression | `Expression<Func<string?>>` | | | |

## Methods
| Name | Description |
|--|--|
| Disable | |
| Enable | |
| RefreshDataAsync | |
| ResetAsync | |

## Callback Events
| Name | Description |
|--|--|
| OnChanged | |


## Examples

### Client side data

```cshtml  showLineNumbers
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
```cs  showLineNumbers
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

### Server side data

```cshtml showLineNumbers
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
```cs showLineNumbers
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

### Validations

```cs showLineNumbers
@using System.ComponentModel.DataAnnotations
```

```css showLineNumbers
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

```cshtml showLineNumbers
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

```cs showLineNumbers
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