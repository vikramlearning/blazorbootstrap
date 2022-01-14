---
sidebar_label: Grid
sidebar_position: 6
---

# Grid

Use BlazorBootstrap's Grid component to display tabular data from the data source. And it supports client-side and server-side paging & sorting.

## Grid Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside the grid. | ✔️ | |
| EmptyDataTemplate | RenderFragment | Template to render when there are no rows to display. | | `No Data.` |
| DataProvider | `GridDataProviderDelegate<TItem>` | DataProvider is for items to render. The provider should always return an instance of `GridDataProviderResult`, and `null` is not allowed. | | |
| PageSize | int | Gets or sets the page size of the grid. | | 10 |
| Sortable | bool | Gets or sets whether end-users can sort data by the column's values. | | true |

## GridColumn Parameters

| Name | Type | Descritpion | Required | Default |
|--|--|--|--|--|
| ChildContent | RenderFragment | Specifies the content to be rendered inside the grid column. | ✔️ | |
| HeaderText | string | Gets or sets the table column header. | ✔️ | |
| IsDefaultSortColumn | bool | Gets or sets the default sort column. | | false |
| SortDirection | `SortDirection` | Gets or sets the default sort direction of a column. | | `SortDirection.None` |
| SortKeySelector | `Expression<Func<TItem, IComparable>>` | Expression used for sorting. | | |
| SortString | string | | | |

## Examples

### Client side sorting and paging

<img src="https://i.imgur.com/0ea5o5X.jpg" alt="Grid - Client side sorting and paging" />

```cshtml
<Grid TItem="Employee" class="table table-hover" DataProvider="EmployeesDataProvider">
    <GridColumn TItem="Employee" Context="employee" HeaderText="Id" SortKeySelector="@(item => item.Id)" IsDefaultSortColumn="true">
        @employee.Id
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="FirstName" SortKeySelector="@(item => item.FirstName)">
        @employee.FirstName
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="LastName" SortKeySelector="@(item => item.LastName)">
        @employee.LastName
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="Designation" SortKeySelector="@(item => item.Designation)">
        @employee.Designation
    </GridColumn>
</Grid>
```
```cs
@code {
    private IEnumerable<Employee> employees;

    protected override void OnInitialized()
    {
        employees = new List<Employee>
        {
            new Employee { Id = 101, FirstName = "Alice", LastName = "Reddy", Designation = "AI Engineer" },
            new Employee { Id = 103, FirstName = "Bob", LastName = "Roy", Designation = "Senior DevOps Engineer" },
            new Employee { Id = 104, FirstName = "John", LastName = "Papa", Designation = "Data Engineer" },
            new Employee { Id = 105, FirstName = "Pop", LastName = "Two", Designation = "Associate Architect" },
            new Employee { Id = 104, FirstName = "Ronald", LastName = "Dire", Designation = "Senior Data Engineer" },
            new Employee { Id = 106, FirstName = "Line", LastName = "K", Designation = "Architect" }
        };
    }

    private async Task<GridDataProviderResult<Employee>> EmployeesDataProvider(GridDataProviderRequest<Employee> request)
    {
        return await Task.FromResult(request.ApplyTo(employees));
    }
}
```

[See demo here.](https://demos.getblazorbootstrap.com/grid#client-side-sorting-and-paging)

### Server side sorting and paging

<img src="https://i.imgur.com/wrHYKGd.jpg" alt="Grid - Server side sorting and paging" />

```cshtml
<Grid TItem="Employee" class="table" DataProvider="EmployeesDataProvider">
    <GridColumn TItem="Employee" Context="employee" HeaderText="Id" SortString="Id" SortKeySelector="@(item => item.Id)">
        @employee.Id
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="FirstName" SortString="FirstName" SortKeySelector="@(item => item.FirstName)" IsDefaultSortColumn="true" SortDirection="SortDirection.Descending">
        @employee.FirstName
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="LastName" SortString="LastName" SortKeySelector="@(item => item.LastName)">
        @employee.LastName
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="Designation" SortString="Designation" SortKeySelector="@(item => item.Designation)">
        @employee.Designation
    </GridColumn>
</Grid>
```
```cs
@code {
    [Inject] public IEmployeeService _employeeService { get; set; }

    private async Task<GridDataProviderResult<Employee>> EmployeesDataProvider(GridDataProviderRequest<Employee> request)
    {
        var result = _employeeService.GetEmployees(request.PageNumber, request.PageSize, request.Sorting[0].SortString, request.Sorting[0].SortDirection);

        return await Task.FromResult(new GridDataProviderResult<Employee> { Data = result.Item1, TotalCount = result.Item2 });
    }
}
```

[See demo here.](https://demos.getblazorbootstrap.com/grid#server-side-sorting-and-paging)

### Empty data

<img src="https://i.imgur.com/rCBsMK2.jpg" alt="Grid - Empty data" />

```cshtml
<Grid TItem="Employee" class="table"  DataProvider="EmployeesDataProvider">
    <GridColumn TItem="Employee" Context="employee" HeaderText="Id" SortKeySelector="@(item => item.Id)" IsDefaultSortColumn="true">
        @employee.Id
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="FirstName" SortKeySelector="@(item => item.FirstName)">
        @employee.FirstName
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="LastName" SortKeySelector="@(item => item.LastName)">
        @employee.LastName
    </GridColumn>
    <GridColumn TItem="Employee" Context="employee" HeaderText="Designation" SortKeySelector="@(item => item.Designation)">
        @employee.Designation
    </GridColumn>
</Grid>
```
```cs
@code {
    private async Task<GridDataProviderResult<Employee>> EmployeesDataProvider(GridDataProviderRequest<Employee> request)
    {
        await Task.Delay(3000);

        return (new GridDataProviderResult<Employee> { Data = new List<Employee>(), TotalCount = 0 });
    }
}
```

[See demo here.](https://demos.getblazorbootstrap.com/grid#empty-data)