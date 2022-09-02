namespace BlazorBootstrap;

public partial class AutoComplete : BaseComponent
{
    #region Members

    private Button closeButton;
    private string inputValue;
    private bool inputHasValue;
    private bool showPanel;
    private string panelCSS => showPanel ? "show" : "";
    private IEnumerable<Customer> customers;

    #endregion Members

    #region Methods

    private async Task OnInputChanged(ChangeEventArgs args)
    {
        this.inputValue = args.Value.ToString();
        SetInputHasValue();
        if (inputHasValue)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
        closeButton?.ShowLoading();
        BindData(this.inputValue);

        await Task.Delay(1000); // API call

        closeButton?.HideLoading();
    }

    private void OnItemSelected(string selectedValue)
    {
        this.inputValue = selectedValue;
        HidePanel();
        SetInputHasValue();
    }

    /// <summary>
    /// Clears the input test value.
    /// </summary>
    private void ClearInputText()
    {
        this.inputValue = string.Empty;
        HidePanel();
        SetInputHasValue();
    }

    /// <summary>
    /// Checks whether the input has value.
    /// </summary>
    private void SetInputHasValue() => this.inputHasValue = inputValue.Length > 0;

    private void ShowPanel() => showPanel = true;

    private void HidePanel() => showPanel = false;

    private void BindData(string searchKey)
    {
        var c = new List<Customer>
        {
            new(){ CustomerId = 1, CustomerName = "ABC LLC"},
            new(){ CustomerId = 1, CustomerName = "Aruna LLC"},
            new(){ CustomerId = 1, CustomerName = "Badri LLC"},
            new(){ CustomerId = 1, CustomerName = "Get Blazor Bootstrap India Private Limited, Badri LLC, Badri LLC, Badri LLC"},
            new(){ CustomerId = 1, CustomerName = "Nithay LLC"},
            new(){ CustomerId = 1, CustomerName = "Vikram LLC"}
        };

        customers = c.Where(x => x.CustomerName.Contains(searchKey));
    }

    #endregion Methods

    #region Properties

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    #endregion Properties
}

public class Customer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
}
