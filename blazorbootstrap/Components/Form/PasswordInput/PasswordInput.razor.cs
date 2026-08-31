namespace BlazorBootstrap;

[AddedVersion("3.3.0")]
public partial class PasswordInput : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private string? oldValue;

    private bool showPassword = false;

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
    /// Disables the password input so users cannot change its value.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Disables the password input so users cannot change its value.")]
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables the password input so users can change its value.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Enables the password input so users can change its value.")]
    public void Enable() => Disabled = false;

    /// <summary>
    /// This event is triggered only when the user changes the selection from the UI.
    /// </summary>
    /// <param name="args"></param>
    private async Task OnChange(ChangeEventArgs args)
    {
        oldValue = Value;

        Value = args.Value?.ToString() ?? string.Empty; // object

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private void ShowHidePassword() => showPassword = !showPassword;

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.FormControl, true),
            (EditContext?.FieldCssClass(fieldIdentifier) ?? string.Empty, true)
        );

    /// <summary>
    /// Gets or sets whether the password input is disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the password input is disabled. When true, users cannot change its value.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets the associated <see cref="Microsoft.AspNetCore.Components.Forms.EditContext" />.
    /// </summary>
    [CascadingParameter]
    private EditContext EditContext { get; set; } = default!;

    private string InputTextType => showPassword ? "text" : "password";

    /// <summary>
    /// Gets or sets the show/hide password button CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is `btn btn-primary btn-sm`.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue("btn border-top border-end border-bottom border border-start-0")]
    [Description("Gets or sets CSS classes applied to the button that reveals or hides the password.")]
    [Parameter]
    public string? ShowHidePasswordButtonCssClass { get; set; } = "btn border-top border-end border-bottom border border-start-0"; //""btn btn-light border";

    private string ShowHidePasswordButtonIcon => showPassword ? "bi bi-eye-fill" : "bi bi-eye-slash-fill";

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [AddedVersion("3.3.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the current password value bound to the input.")]
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// This event is fired when the PasswordInput value changes.
    /// </summary>
    [AddedVersion("3.3.0")]
    [Description("Fires whenever user input changes the password value.")]
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

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
    public Expression<Func<string?>> ValueExpression { get; set; } = default!;

    #endregion
}
