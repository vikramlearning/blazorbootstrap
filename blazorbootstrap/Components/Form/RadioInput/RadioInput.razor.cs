namespace BlazorBootstrap;

[AddedVersion("3.3.0")]
public partial class RadioInput : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private DotNetObjectReference<RadioInput> objRef = default!;

    #endregion

    #region Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SafeInvokeVoidAsync("window.blazorBootstrap.radioInput.initialize", Id, Name, objRef);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        objRef ??= DotNetObjectReference.Create(this);

        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        base.OnInitialized();
    }

    /// <summary>
    /// Updates the radio input value from the JavaScript change handler.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Updates the radio input value from the JavaScript change handler.")]
    [JSInvokable]
    public async Task OnChangeJS(bool newValue)
    {
        Value = newValue;

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    /// <summary>
    /// Disables the radio input so users cannot select it.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Disables the radio input so users cannot select it.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the radio input so users can select it.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Enables the radio input so users can select it.")]
    public void Enable() => Disabled = false;

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.FormCheckInput, true),
            (EditContext?.FieldCssClass(fieldIdentifier) ?? string.Empty, true)
        );

    /// <summary>
    /// Gets or sets whether the radio input is disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the radio input is disabled. When true, users cannot select it.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets the associated <see cref="Microsoft.AspNetCore.Components.Forms.EditContext" />.
    /// </summary>
    [CascadingParameter]
    private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the label displayed next to the radio input.")]
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the HTML name used to group related radio inputs.")]
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the current value represented by this radio input.")]
    [Parameter]
    public bool Value { get; set; } = default!;

    /// <summary>
    /// This event fired on every user keystroke that changes the NumberInput value.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Fires whenever user interaction selects this radio input.")]
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound value for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<bool>> ValueExpression { get; set; } = default!;

    #endregion
}
