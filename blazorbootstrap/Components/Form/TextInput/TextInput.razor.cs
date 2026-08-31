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
    /// Disables the text input so users cannot change its value.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Disables the text input so users cannot change its value.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the text input so users can change its value.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Enables the text input so users can change its value.")]
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
    /// Gets or sets whether browser autocomplete is enabled for the text input.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether browser autocomplete is enabled. When true, the browser may offer saved values for the input.")]
    [Parameter]
    public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets whether the input is disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the input is disabled. When true, users cannot change its value.")]
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the maximum number of characters users can enter.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum number of characters users can enter. Additional input is prevented when the limit is reached.")]
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets placeholder text displayed when the input has no value.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets placeholder text displayed when the input has no value.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of the input value.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(Alignment.None)]
    [Description("Gets or sets the horizontal alignment of the input value.")]
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

    /// <summary>
    /// Gets or sets the current text value bound to the input.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the current text value bound to the input.")]
    [Parameter]
    public string Value { get; set; } = default!;

    /// <summary>
    /// This event fires when the <see cref="TextInput" /> value changes.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Fires whenever user input changes the text value.")]
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value for validation and EditContext notifications.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound value for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<string>> ValueExpression { get; set; } = default!;

    #endregion
}
