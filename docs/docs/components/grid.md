---
sidebar_label: Grid
sidebar_position: 7
---

# Grid

Use BlazorBootstrap's grid component to display tabular data from the data source. And it supports client-side and server-side paging & sorting.

<img src="https://i.imgur.com/36RsWZ3.png" alt="Blazor Bootstrap: Grid Component" />

## Grid Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| AllowFiltering | bool | Gets or sets the grid filtering. | | false |
| AllowPaging | bool | Gets or sets the grid paging. | | false |
| AllowSorting | bool | Gets or sets the grid sorting. | | false |
| ChildContent | RenderFragment | Specifies the content to be rendered inside the grid. | ✔️ | |
| EmptyText | string | Shows text on no records. | | No records to display |
| DataProvider | `GridDataProviderDelegate<TItem>` | DataProvider is for items to render. The provider should always return an instance of `GridDataProviderResult`, and `null` is not allowed. | | |
| PageSize | int | Gets or sets the page size of the grid. | | 10 |
| PaginationAlignment | `Alignment` | Gets or sets the pagination alignment. | | `Alignment.Start` |
| Responsive | bool | Gets or sets a value indicating whether Grid is responsive. | | false |
| Sortable | bool | Gets or sets whether end-users can sort data by the column's values. | | true |

## GridColumn Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside the grid column. | ✔️ | |
| Filterable | bool | Enable or disable the filter on a specific column. The filter is enabled or disabled based on the grid `AllowFiltering` parameter. | | true |
| FilterOperator | `FilterOperator` | Gets or sets the filter operator. | | Assigend based on the property type. |
| FilterTextboxWidth | int | Gets or sets the filter textbox width in pixels. | | |
| FilterValue | string | Gets or sets the filter value. | | |
| HeaderText | string | Gets or sets the table column header. | ✔️ | |
| HeaderTextAlignment | `Alignment` | Gets or sets the header text alignment. | | `Alignment.Start` |
| IsDefaultSortColumn | bool | Gets or sets the default sort column. | | false |
| PropertyName | string | Gets or sets the property name. This is required when `AllowFiltering` is true. | | |
| Sortable | bool | Enable or disable the sorting on a specific column. The sorting is enabled or disabled based on the grid `AllowSorting` parameter. | | false |
| SortDirection | `SortDirection` | Gets or sets the default sort direction of a column. | | `SortDirection.None` |
| SortKeySelector | `Expression<Func<TItem, IComparable>>` | Expression used for sorting. | | |
| SortString | string | Gets or sets the column sort string. This string is passed to the backend/API for sorting. And it is ignored for client-side sorting. | | |
| TextAlignment | `Alignment` | Gets or sets the text alignment. | | `Alignment.Start` |
| TextNoWrap | bool | Gets or sets text nowrap. | | false |

## Examples

### Client side filtering

For filtering, `AllowFiltering` and `PropertyName` parameters are required.
Add `AllowFiltering="true"` parameter to Grid and `PropertyName` parameter to all the GridColumns.

<img src="https://i.imgur.com/Clr8W11.png" alt="Blazor Bootstrap: Grid Component - Client side filtering" />

```cshtml {1,2,5,8,11,14} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowFiltering="true" Responsive="true">
    <GridColumn TItem="Employee1" HeaderText="Id" PropertyName="Id">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name" PropertyName="Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" PropertyName="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" PropertyName="DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" PropertyName="IsActive">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#client-side-filtering)

### Client side paging

For paging, `AllowPaging` and `PageSize` parameters are required.
Add `AllowPaging="true"` and `PageSize="20"` parameters to the Grid. `PageSize` parameter is optional. 

<img src="https://i.imgur.com/WXwUqgX.png" alt="Blazor Bootstrap: Grid Component - Client side paging" />

:::info INFO
The default page size is 10.
:::

```cshtml {1} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowPaging="true" PageSize="5" Responsive="true">
    <GridColumn TItem="Employee1" HeaderText="Id">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 113, Name = "Merlin", Designation = "Senior Consultant", DOJ = new DateOnly(1989, 10, 2), IsActive = true },
            new Employee1 { Id = 117, Name = "Sharna", Designation = "Data Analyst", DOJ = new DateOnly(1994, 5, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
            new Employee1 { Id = 111, Name = "Glenda", Designation = "Data Engineer", DOJ = new DateOnly(1994, 1, 12), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#client-side-paging)

### Client side sorting

For sorting, `AllowSorting` and `SortKeySelector` parameters are required.
Add `AllowSorting="true"` parameter to Grid and `SortKeySelector` to all the GridColumns.

<img src="https://i.imgur.com/wkIWG5S.png" alt="Blazor Bootstrap: Grid Component - Client side sorting" />

```cshtml {1,2,5,8,11,14} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowSorting="true" Responsive="true">
    <GridColumn TItem="Employee1" HeaderText="Id" SortKeySelector="item => item.Id">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name" SortKeySelector="item => item.Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" SortKeySelector="item => item.Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" SortKeySelector="item => item.DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" SortKeySelector="item => item.IsActive">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#client-side-sorting)

### Client side filtering, paging, and sorting

<img src="https://i.imgur.com/wZ0cQiO.png" alt="Blazor Bootstrap: Grid Component - Client side filtering, paging, and sorting" />

```cshtml {1,2,5,8,11,14} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowFiltering="true" AllowPaging="true" PageSize="5" AllowSorting="true" Responsive="true">
    <GridColumn TItem="Employee1" HeaderText="Id" PropertyName="Id" SortKeySelector="item => item.Id">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name" PropertyName="Name" SortKeySelector="item => item.Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" PropertyName="Designation" SortKeySelector="item => item.Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" PropertyName="DOJ" SortKeySelector="item => item.DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" PropertyName="IsActive" SortKeySelector="item => item.IsActive">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 113, Name = "Merlin", Designation = "Senior Consultant", DOJ = new DateOnly(1989, 10, 2), IsActive = true },
            new Employee1 { Id = 117, Name = "Sharna", Designation = "Data Analyst", DOJ = new DateOnly(1994, 5, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
            new Employee1 { Id = 111, Name = "Glenda", Designation = "Data Engineer", DOJ = new DateOnly(1994, 1, 12), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#client-side-filtering-paging-sorting)

### Set default filter

`FilterOperator` and `FilterValue` parameters are required to set the default filter. 

<img src="https://i.imgur.com/jog5EA4.png" alt="Blazor Bootstrap: Grid Component - Set default filter" />

:::tip TIP
You can set the default filter on more than one GridColumn.
:::

```cshtml {2} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowFiltering="true" Responsive="true">
    <GridColumn TItem="Employee1" HeaderText="Id" PropertyName="Id" FilterOperator="FilterOperator.GreaterThanOrEquals" FilterValue="105">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name" PropertyName="Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" PropertyName="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" PropertyName="DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" PropertyName="IsActive">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#set-default-filter)

### Disable specific column filter

`Filterable` parameter is required to disable the filter on a specific column. 
Add `Filterable="false"` parameter to GridColumn.

<img src="https://i.imgur.com/oGeA4VO.png" alt="Blazor Bootstrap: Grid Component - Disable specific column filter" />

:::info INFO
By default, `Filterable="true"` on all the columns if the `AllowFiltering` parameter is set to `true` on the grid.
:::
```cshtml {14} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowFiltering="true" Responsive="true">
    <GridColumn TItem="Employee1" HeaderText="Id" Filterable="false">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name" PropertyName="Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" PropertyName="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" PropertyName="DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" Filterable="false">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#disable-specific-column-filter)

### Increase filter textbox width

Add `FilterTextboxWidth` parameter to the GridColumn to increase or decrease the filter textbox width, `FilterTextboxWidth` parameter is optional.

<img src="https://i.imgur.com/eWXiHr0.png" alt="Blazor Bootstrap: Grid Component - Increase filter textbox width" />

:::note NOTE
Filter textbox width measured in pixels.
:::

```cshtml {2,5,8} showLineNumbers
<Grid TItem="Employee3" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowFiltering="true" Responsive="true">
    <GridColumn TItem="Employee3" HeaderText="Id" PropertyName="Id" FilterTextboxWidth="80">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="First Name" PropertyName="FirstName" FilterTextboxWidth="80">
        @context.FirstName
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Last Name" PropertyName="LastName" FilterTextboxWidth="80">
        @context.LastName
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Email" PropertyName="Email">
        @context.Email
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Company" TextNoWrap="true" PropertyName="Company">
        @context.Company
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Designation" TextNoWrap="true" PropertyName="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="DOJ" PropertyName="DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Salary" HeaderTextAlignment="Alignment.End" TextAlignment="Alignment.End" PropertyName="Salary" FilterTextboxWidth="80">
        @context.Salary.ToString("N2")
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Active" HeaderTextAlignment="Alignment.Center" TextAlignment="Alignment.Center" PropertyName="IsActive">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee3> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee3>
        {
            new Employee3 { Id = 107, FirstName = "Alice", LastName = "Reddy", Email = "alice@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), Salary = 7700, IsActive = true },
            new Employee3 { Id = 103, FirstName = "Bob", LastName = "Roy", Email = "bob@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), Salary = 19000, IsActive = true },
            new Employee3 { Id = 106, FirstName = "John", LastName = "Papa", Email = "john@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), Salary = 12000, IsActive = true },
            new Employee3 { Id = 104, FirstName = "Pop", LastName = "Two", Email = "pop@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), Salary = 19000, IsActive = false },
            new Employee3 { Id = 105, FirstName = "Ronald", LastName = "Dire", Email = "ronald@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), Salary = 16500.50M, IsActive = true },
            new Employee3 { Id = 102, FirstName = "Line", LastName = "K", Email = "line@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), Salary = 24000, IsActive = true },
            new Employee3 { Id = 101, FirstName = "Daniel", LastName = "Potter", Email = "daniel@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), Salary = 21000, IsActive = true },
            new Employee3 { Id = 108, FirstName = "Zayne", LastName = "Simmons", Email = "zayne@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), Salary = 17850, IsActive = true },
            new Employee3 { Id = 109, FirstName = "Isha", LastName = "Davison", Email = "isha@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), Salary = 8000, IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee3>> EmployeesDataProvider(GridDataProviderRequest<Employee3> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#increase-filter-textbox-width)

### Server side filtering, paging and sorting

<img src="https://i.imgur.com/xHVXTew.png" alt="Blazor Bootstrap: Grid Component - Server side filtering, paging and sorting" />

:::note NOTE
For server-side sorting, we need the `SortString` parameter on GridColumn along with the `SortKeySelector` parameter.
:::

```cshtml showLineNumbers
<Grid TItem="Employee" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowFiltering="true" AllowPaging="true" PageSize="5" AllowSorting="true" Responsive="true">
    <GridColumn TItem="Employee" HeaderText="Id" PropertyName="Id" SortString="Id" SortKeySelector="item => item.Id">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="First Name" PropertyName="FirstName" SortString="FirstName" SortKeySelector="item => item.FirstName">
        @context.FirstName
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="Last Name" PropertyName="LastName" SortString="LastName" SortKeySelector="item => item.LastName">
        @context.LastName
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="Designation" PropertyName="Designation" SortString="Designation" SortKeySelector="item => item.Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="Salary" PropertyName="Salary" SortString="Salary" SortKeySelector="item => item.Salary">
        @context.Salary
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="DOJ" PropertyName="DOJ" SortString="DOJ" SortKeySelector="item => item.DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="Active" PropertyName="IsActive" SortString="IsActive" SortKeySelector="item => item.IsActive">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs {11} showLineNumbers
@code {
    [Inject] public IEmployeeService _employeeService { get; set; }

    private async Task<GridDataProviderResult<Employee>> EmployeesDataProvider(GridDataProviderRequest<Employee> request)
    {
        string sortString = "";
        SortDirection sortDirection = SortDirection.None;

        if (request.Sorting is not null && request.Sorting.Any())
        {
            // Note: Multi column sorting is not supported at this moment
            sortString = request.Sorting[0].SortString;
            sortDirection = request.Sorting[0].SortDirection;
        }
        var result = _employeeService.GetEmployees(request.Filters, request.PageNumber, request.PageSize, sortString, sortDirection);
        return await Task.FromResult(new GridDataProviderResult<Employee> { Data = result.Item1, TotalCount = result.Item2 });
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#server-side-filtering-paging-sorting)

### Set default sorting

`IsDefaultSortColumn` parameter is required to set the default sorting. Add `IsDefaultSortColumn="true"` parameter to the GridColumn.
The default sort direction will be **ascending**. To change the default sorting of a column, add `SortDirection="SortDirection.Descending"` to the GridColumn.

<img src="https://i.imgur.com/WxULPS6.png" alt="Blazor Bootstrap: Grid Component - Set default sorting" />

:::info INFO
If more than one GridColumn has the `IsDefaultSortColumn` paramter, it will pick the first column as the default sorting column.
:::

```cshtml {5} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowSorting="true">
    <GridColumn TItem="Employee1" HeaderText="Id" SortKeySelector="@(item => item.Id)">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name" SortKeySelector="@(item => item.Name)" IsDefaultSortColumn="true" SortDirection="SortDirection.Descending">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" SortKeySelector="@(item => item.Designation)">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" SortKeySelector="@(item => item.DOJ)">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" SortKeySelector="@(item => item.IsActive)">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#set-default-sorting)

### Disable specific column sorting

Add `Sortable="false"` parameter the GridColumn to disable the sorting. 

:::info INFO
If sorting is disabled, then the `SortKeySelector` parameter is not required.
:::

```cshtml {8} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" AllowSorting="true">
    <GridColumn TItem="Employee1" HeaderText="Id" SortKeySelector="@(item => item.Id)">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name" SortKeySelector="@(item => item.Name)">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" Sortable="false">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" SortKeySelector="@(item => item.DOJ)">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" SortKeySelector="@(item => item.IsActive)">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#disable-specific-column-sorting)

### Header text alignment

Use the `HeaderTextAlignment` parameter to change the header column alignment. 
By default, `HeaderTextAlignment` is set to Alignment.Start. Other options you can use are `Alignment.Center` and `Alignment.End`.

<img src="https://i.imgur.com/gWgIESD.png" alt="Blazor Bootstrap: Grid Component - Header text alignment" />

```cshtml {8,11,14} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider">
    <GridColumn TItem="Employee1" HeaderText="Id" HeaderTextAlignment="Alignment.Center">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation" HeaderTextAlignment="Alignment.Center">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ" HeaderTextAlignment="Alignment.Center">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active" HeaderTextAlignment="Alignment.End">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#header-text-alignment)

### Cell alignment

Use the `TextAlignment` parameter to change the cell data alignment. 
By default, `TextAlignment` is set to `Alignment.Start`. Other options you can use are `Alignment.Center` and `Alignment.End`.

<img src="https://i.imgur.com/0OKp4yd.png" alt="Blazor Bootstrap: Grid Component - Cell alignment" />

```cshtml {11,14} showLineNumbers
<Grid TItem="Employee2" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider">
    <GridColumn TItem="Employee2" HeaderText="Id" HeaderTextAlignment="Alignment.Center" TextAlignment="Alignment.Center">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Employee Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Salary" HeaderTextAlignment="Alignment.End" TextAlignment="Alignment.End">
        @context.Salary
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Active" HeaderTextAlignment="Alignment.Center" TextAlignment="Alignment.Center">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee2> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee2>
        {
            new Employee2 { Id = 107, Name = "Alice", Designation = "AI Engineer", Salary = 7700, IsActive = true },
            new Employee2 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", Salary = 19000, IsActive = true },
            new Employee2 { Id = 106, Name = "John", Designation = "Data Engineer", Salary = 12000, IsActive = true },
            new Employee2 { Id = 104, Name = "Pop", Designation = "Associate Architect", Salary = 19000, IsActive = false },
            new Employee2 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", Salary = 16500.50M, IsActive = true },
            new Employee2 { Id = 102, Name = "Line", Designation = "Architect", Salary = 24000, IsActive = true },
            new Employee2 { Id = 101, Name = "Daniel", Designation = "Architect", Salary = 21000, IsActive = true },
            new Employee2 { Id = 108, Name = "Zayne", Designation = "Data Analyst", Salary = 17850, IsActive = true },
            new Employee2 { Id = 109, Name = "Isha", Designation = "App Maker", Salary = 8000, IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee2>> EmployeesDataProvider(GridDataProviderRequest<Employee2> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#cell-alignment)

### Cell formating
To format the cell data, use `ToString` method and format strings. Refer: [How to format numbers, dates, enums, and other types in .NET](https://docs.microsoft.com/en-us/dotnet/standard/base-types/formatting-types)

<img src="https://i.imgur.com/MitfasL.png" alt="Blazor Bootstrap: Grid Component - Cell formating" />

:::tip EXAMPLE
@context.Salary.ToString("N2")
:::

```cshtml {12} showLineNumbers
<Grid TItem="Employee2" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider">
    <GridColumn TItem="Employee2" HeaderText="Id" HeaderTextAlignment="Alignment.Center" TextAlignment="Alignment.Center">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Employee Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Salary" HeaderTextAlignment="Alignment.End" TextAlignment="Alignment.End">
        @context.Salary.ToString("N2")
    </GridColumn>
    <GridColumn TItem="Employee2" HeaderText="Active" HeaderTextAlignment="Alignment.Center" TextAlignment="Alignment.Center">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee2> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee2>
        {
            new Employee2 { Id = 107, Name = "Alice", Designation = "AI Engineer", Salary = 7700, IsActive = true },
            new Employee2 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", Salary = 19000, IsActive = true },
            new Employee2 { Id = 106, Name = "John", Designation = "Data Engineer", Salary = 12000, IsActive = true },
            new Employee2 { Id = 104, Name = "Pop", Designation = "Associate Architect", Salary = 19000, IsActive = false },
            new Employee2 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", Salary = 16500.50M, IsActive = true },
            new Employee2 { Id = 102, Name = "Line", Designation = "Architect", Salary = 24000, IsActive = true },
            new Employee2 { Id = 101, Name = "Daniel", Designation = "Architect", Salary = 21000, IsActive = true },
            new Employee2 { Id = 108, Name = "Zayne", Designation = "Data Analyst", Salary = 17850, IsActive = true },
            new Employee2 { Id = 109, Name = "Isha", Designation = "App Maker", Salary = 8000, IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee2>> EmployeesDataProvider(GridDataProviderRequest<Employee2> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#cell-formating)

### Cell nowrap

To prevent text from wrapping, add `TextNoWrap="true"` parameter to the GridColumn.

<img src="https://i.imgur.com/Wzc3AUF.png" alt="Blazor Bootstrap: Grid Component - Cell nowrap" />

:::tip TIP
Add `Responsive="true"` parameter to the grid to enable horizontal scrolling.
:::

```cshtml {14,17} showLineNumbers
<Grid TItem="Employee3" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" Responsive="true">
    <GridColumn TItem="Employee3" HeaderText="Id" HeaderTextAlignment="Alignment.Center" TextAlignment="Alignment.Center">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="First Name">
        @context.FirstName
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="First Name">
        @context.LastName
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Email">
        @context.Email
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Company" TextNoWrap="true">
        @context.Company
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Designation" TextNoWrap="true">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Salary" HeaderTextAlignment="Alignment.End" TextAlignment="Alignment.End">
        @context.Salary.ToString("N2")
    </GridColumn>
    <GridColumn TItem="Employee3" HeaderText="Active" HeaderTextAlignment="Alignment.Center" TextAlignment="Alignment.Center">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee3> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee3>
        {
            new Employee3 { Id = 107, FirstName = "Alice", LastName = "Reddy", Email = "alice@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), Salary = 7700, IsActive = true },
            new Employee3 { Id = 103, FirstName = "Bob", LastName = "Roy", Email = "bob@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), Salary = 19000, IsActive = true },
            new Employee3 { Id = 106, FirstName = "John", LastName = "Papa", Email = "john@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), Salary = 12000, IsActive = true },
            new Employee3 { Id = 104, FirstName = "Pop", LastName = "Two", Email = "pop@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), Salary = 19000, IsActive = false },
            new Employee3 { Id = 105, FirstName = "Ronald", LastName = "Dire", Email = "ronald@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), Salary = 16500.50M, IsActive = true },
            new Employee3 { Id = 102, FirstName = "Line", LastName = "K", Email = "line@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), Salary = 24000, IsActive = true },
            new Employee3 { Id = 101, FirstName = "Daniel", LastName = "Potter", Email = "daniel@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), Salary = 21000, IsActive = true },
            new Employee3 { Id = 108, FirstName = "Zayne", LastName = "Simmons", Email = "zayne@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), Salary = 17850, IsActive = true },
            new Employee3 { Id = 109, FirstName = "Isha", LastName = "Davison", Email = "isha@blazorbootstrap.com", Company = "BlazorBootstrap Company", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), Salary = 8000, IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee3>> EmployeesDataProvider(GridDataProviderRequest<Employee3> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#cell-nowrap)

### Pagination alignment

Change the alignment of pagination by adding the `PaginationAlignment` parameter to the Grid. 
By default, `PaginationAlignment` is set to `Alignment.Start`. Other options you can use are `Alignment.Center` and `Alignment.End`.

<img src="https://i.imgur.com/CtSqfJb.png" alt="Blazor Bootstrap: Grid Component - Pagination alignment" />

```cshtml {1} showLineNumbers
<Grid TItem="Employee1" class="table table-hover table-bordered table-striped table-striped" DataProvider="EmployeesDataProvider" AllowPaging="true" PageSize="5" PaginationAlignment="Alignment.End" Responsive="true">
    <GridColumn TItem="Employee1" HeaderText="Id">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Employee Name">
        @context.Name
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Designation">
        @context.Designation
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="DOJ">
        @context.DOJ
    </GridColumn>
    <GridColumn TItem="Employee1" HeaderText="Active">
        @context.IsActive
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private IEnumerable<Employee1> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee1>
        {
            new Employee1 { Id = 107, Name = "Alice", Designation = "AI Engineer", DOJ = new DateOnly(1998, 11, 17), IsActive = true },
            new Employee1 { Id = 103, Name = "Bob", Designation = "Senior DevOps Engineer", DOJ = new DateOnly(1985, 1, 5), IsActive = true },
            new Employee1 { Id = 106, Name = "John", Designation = "Data Engineer", DOJ = new DateOnly(1995, 4, 17), IsActive = true },
            new Employee1 { Id = 104, Name = "Pop", Designation = "Associate Architect", DOJ = new DateOnly(1985, 6, 8), IsActive = false },
            new Employee1 { Id = 105, Name = "Ronald", Designation = "Senior Data Engineer", DOJ = new DateOnly(1991, 8, 23), IsActive = true },
            new Employee1 { Id = 102, Name = "Line", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 101, Name = "Daniel", Designation = "Architect", DOJ = new DateOnly(1977, 1, 12), IsActive = true },
            new Employee1 { Id = 113, Name = "Merlin", Designation = "Senior Consultant", DOJ = new DateOnly(1989, 10, 2), IsActive = true },
            new Employee1 { Id = 117, Name = "Sharna", Designation = "Data Analyst", DOJ = new DateOnly(1994, 5, 12), IsActive = true },
            new Employee1 { Id = 108, Name = "Zayne", Designation = "Data Analyst", DOJ = new DateOnly(1991, 1, 1), IsActive = true },
            new Employee1 { Id = 109, Name = "Isha", Designation = "App Maker", DOJ = new DateOnly(1996, 7, 1), IsActive = true },
            new Employee1 { Id = 111, Name = "Glenda", Designation = "Data Engineer", DOJ = new DateOnly(1994, 1, 12), IsActive = true },
        };
    }

    private async Task<GridDataProviderResult<Employee1>> EmployeesDataProvider(GridDataProviderRequest<Employee1> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#pagination-alignment)

### Empty data

If there are no records to display in the Grid, by default, it will display the **No records to display** message. 
You can change this message by adding the `EmptyText` parameter to the Grid.

<img src="https://i.imgur.com/cLuvfmD.png" alt="Blazor Bootstrap: Grid Component - Empty data" />

```cshtml {1} showLineNumbers
<Grid TItem="Employee" class="table table-hover table-bordered table-striped" DataProvider="EmployeesDataProvider" EmptyText="No records to display">
    <GridColumn TItem="Employee" HeaderText="Id">
        @context.Id
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="First Name">
        @context.FirstName
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="Last Name">
        @context.LastName
    </GridColumn>
    <GridColumn TItem="Employee" HeaderText="Designation">
        @context.Designation
    </GridColumn>
</Grid>
```

```cs showLineNumbers
@code {
    private async Task<GridDataProviderResult<Employee>> EmployeesDataProvider(GridDataProviderRequest<Employee> request)
    {
        await Task.Delay(3000);

        return (new GridDataProviderResult<Employee> { Data = new List<Employee>(), TotalCount = 0 });
    }
}
```

[See demo here](https://demos.getblazorbootstrap.com/grid#empty-data)

:::tip TIP
Add `Responsive="true"` parameter to the grid to enable horizontal scrolling.
:::