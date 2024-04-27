namespace BlazorBootstrap;

public partial class Switch : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private bool oldValue;

    #endregion

    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.FormCheck)
        .AddClass(BootstrapClass.FormSwitch)
        .AddClass(BootstrapClass.FormCheckReverse, Reverse)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    protected override async Task OnInitializedAsync()
    {
        oldValue = Value;

        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (oldValue != Value)
        {
            await ValueChanged.InvokeAsync(Value);

            EditContext?.NotifyFieldChanged(fieldIdentifier);

            oldValue = Value;
        }
    }

    /// <summary>
    /// Disables switch.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables switch.
    /// </summary>
    public void Enable() => Disabled = false;

    /// <summary>
    /// This event is triggered only when the user changes the selection from the UI.
    /// </summary>
    /// <param name="args"></param>
    private async Task OnChange(ChangeEventArgs args)
    {
        bool.TryParse(args.Value?.ToString(), out var newValue);
        Value = newValue;

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        oldValue = Value;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [Parameter]
    public string Label { get; set; } = default!;

    private string reverse => Reverse ? BootstrapClass.FormCheckReverse : "";

    /// <summary>
    /// Determines whether to put the switch on the opposite side.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// This event is fired when the switch selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; } = default!;

    [Parameter] public Expression<Func<bool>> ValueExpression { get; set; } = default!;

    #endregion
}
