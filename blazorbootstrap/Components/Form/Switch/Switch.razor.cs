namespace BlazorBootstrap;

[AddedVersion("1.3.0")]
public partial class Switch : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private bool oldValue;

    #endregion

    #region Methods

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
    [AddedVersion("1.3.0")]
    [Description("Disables switch.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables switch.
    /// </summary>
    [AddedVersion("1.3.0")]
    [Description("Enables switch.")]
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

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.FormCheck, true),
            (BootstrapClass.FormSwitch, true),
            (BootstrapClass.FormCheckReverse, Reverse));

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.3.0")]
    [Description("Gets or sets the disabled state.")]
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("1.3.0")]
    [Description("Gets or sets the label.")]
    [Parameter]
    public string Label { get; set; } = default!;

    private string reverse => Reverse ? BootstrapClass.FormCheckReverse : "";

    /// <summary>
    /// Determines whether to put the switch on the opposite side.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.3.0")]
    [Description("Determines whether to put the switch on the opposite side.")]
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("1.3.0")]
    [Description("Gets or sets the value.")]
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// This event is fired when the switch selection changes.
    /// </summary>
    [AddedVersion("1.3.0")]
    [Description("This event is fired when the switch selection changes.")]
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; } = default!;

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("1.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets an expression that identifies the bound value.")]
    [Parameter]
    public Expression<Func<bool>> ValueExpression { get; set; } = default!;

    #endregion
}
