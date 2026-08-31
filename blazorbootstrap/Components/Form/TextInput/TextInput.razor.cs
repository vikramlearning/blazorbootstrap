namespace BlazorBootstrap;

[AddedVersion("3.3.0")]
public partial class TextInput : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier = default!;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        base.OnInitialized();
    }

    /// <summary>
    /// Disables number input.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Disables number input.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables number input.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Enables number input.")]
    public void Enable() => Disabled = false;

    private async Task OnChange(ChangeEventArgs e)
    {
        var oldValue = Value;
        var newValue = e.Value?.ToString() ?? string.Empty; // object

        await ValueChanged.InvokeAsync(newValue);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.FormControl, true),
            (TextAlignment.ToTextAlignmentClass(), TextAlignment != Alignment.None)
        );

    private string autoComplete => AutoComplete ? "true" : "false";

    /// <summary>
    /// If <see langword="true" />, TextInput can complete the values automatically by the browser.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [Description("If , TextInput can complete the values automatically by the browser.")]
    [Parameter]
    public bool AutoComplete { get; set; }

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

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the maximum number of characters that can be entered.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Gets or sets the maximum number of characters that can be entered.")]
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [Description("Gets or sets the placeholder text.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [Description("Gets or sets the text alignment.")]
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Gets or sets the value.")]
    [Parameter]
    public string Value { get; set; } = default!;

    /// <summary>
    /// This event fires when the <see cref="TextInput" /> value changes.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("This event fires when the value changes.")]
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

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
    public Expression<Func<string>> ValueExpression { get; set; } = default!;

    #endregion
}
