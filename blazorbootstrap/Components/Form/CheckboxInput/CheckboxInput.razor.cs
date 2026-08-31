namespace BlazorBootstrap;

[AddedVersion("3.3.0")]
public partial class CheckboxInput : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        base.OnInitialized();
    }

    /// <summary>
    /// Disables checkbox input.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Disables checkbox input.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables checkbox input.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Enables checkbox input.")]
    public void Enable() => Disabled = false;

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value is not null && (bool)e.Value;

        await ValueChanged.InvokeAsync(newValue);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.FormCheckInput, true),
            (EditContext?.FieldCssClass(fieldIdentifier) ?? string.Empty, true)
        );

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [Description("Gets or sets the disabled state.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets the associated <see cref="Microsoft.AspNetCore.Components.Forms.EditContext" />.
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Gets or sets the label.")]
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Gets or sets the value.")]
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// This event fires when the <see cref="CheckboxInput" /> value changes.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("This event fires when the value changes.")]
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
    [Description("Gets or sets an expression that identifies the bound value.")]
    [Parameter]
    public Expression<Func<bool>> ValueExpression { get; set; } = default!;

    #endregion
}
