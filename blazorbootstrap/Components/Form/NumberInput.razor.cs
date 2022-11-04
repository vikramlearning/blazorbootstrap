using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap;

public partial class NumberInput : BaseComponent
{
    #region Events

    /// <summary>
    /// This is event fires on every user keystroke that changes the textbox value.
    /// </summary>
    [Parameter] public EventCallback<int?> ValueChanged { get; set; }

    #endregion

    #region Members

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private bool disabled;

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.FormControl());

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        this.disabled = this.Disabled;

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Disables number input.
    /// </summary>
    public void Disable()
    {
        this.disabled = true;
    }

    /// <summary>
    /// Enables number input.
    /// </summary>
    public void Enable()
    {
        this.disabled = false;
    }

    private async Task OnInputChange(ChangeEventArgs e)
    {
        Console.WriteLine($"Input: {e.Value.ToString()}");

        if (string.IsNullOrEmpty(e.Value.ToString()))
            Value = null;
        else if (!int.TryParse(e.Value.ToString(), out int value))
            Value = null;
        //else if ((Min.HasValue && Max.HasValue && (value < Min.Value || value > Max.Value))
        //   || (Min.HasValue && value < Min.Value)
        //   || (Max.HasValue && value > Max.Value))
        //{
        //    // do nothing
        //    //Value = null;
        //}
        else
            Value = value;

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        Console.WriteLine($"Value: {Value}");
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    [Parameter] public int? Value { get; set; }

    [Parameter] public Expression<Func<int?>> ValueExpression { get; set; }

    #endregion
}
