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

    private string autoComplete => this.AutoComplete ? "true" : "false";

    private bool disabled;

    private string step;

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

        this.step = Step.HasValue ? $"{Step.Value}" : "any";

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender || Value is null || !(Min.HasValue && Max.HasValue))
            return;

        var currentValue = Value; // object

        if (string.IsNullOrWhiteSpace(currentValue?.ToString())
             || !int.TryParse(currentValue.ToString(), out int value))
            Value = null;
        else if (Min.HasValue && value < Min.Value)
            Value = Min.Value;
        else if (Max.HasValue && value > Max.Value)
            Value = Max.Value;

        await ValueChanged.InvokeAsync(Value);

        Console.WriteLine($"OnAfterRenderAsync - Value: {Value}"); // TODO: remove this console log
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

    private async Task OnInput(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        if (string.IsNullOrWhiteSpace(newValue?.ToString())
             || !int.TryParse(newValue.ToString(), out int value))
            Value = null;
        //else if (Min.HasValue && value < Min.Value)
        //    Value = Min.Value;
        //else if (Max.HasValue && value > Max.Value)
        //    Value = Max.Value;
        else
            Value = value;

        if (oldValue == Value)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.numberInput.setValue", ElementId, Value);
        }

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        Console.WriteLine($"OnInput - Input: {e.Value?.ToString()}, Value: {Value}"); // TODO: remove this console log
    }

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value; // object

        if (string.IsNullOrWhiteSpace(newValue?.ToString())
             || !int.TryParse(newValue.ToString(), out int value))
            Value = null;
        else if (Min.HasValue && value < Min.Value)
            Value = Min.Value;
        else if (Max.HasValue && value > Max.Value)
            Value = Max.Value;
        else
            Value = value;

        if (oldValue == Value)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.numberInput.setValue", ElementId, Value);
        }

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        Console.WriteLine($"OnChange - Input: {e.Value?.ToString()}, Value: {Value}"); // TODO: remove this console log
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public bool AutoComplete { get; set; } = false;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; }

    [Parameter] public int? Max { get; set; }

    [Parameter] public int? Min { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the step.
    /// </summary>
    [Parameter] public int? Step { get; set; }

    [Parameter] public int? Value { get; set; }

    [Parameter] public Expression<Func<int?>> ValueExpression { get; set; }

    #endregion
}
