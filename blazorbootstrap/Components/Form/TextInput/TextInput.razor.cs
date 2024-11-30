using System.Globalization;

namespace BlazorBootstrap;

public partial class TextInput : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier = default!;

    #endregion

    #region Methods

    /// <summary>
    /// Disables number input.
    /// </summary>
    public void Disable() => Disabled = true;

    /// <summary>
    /// Enables number input.
    /// </summary>
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

    protected override void OnInitialized()
    {
        fieldIdentifier = FieldIdentifier.Create(ValueExpression);
    }

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.FormControl, true),
            (TextAlignment.ToTextAlignmentClass(), TextAlignment != Alignment.None)
        );

    private string autoComplete => AutoComplete ? "true" : "false";

    /// <summary>
    /// If <see langword="true" />, NumberInput can complete the values automatically by the browser.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool AutoComplete { get; set; }

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the maximum length of the input.
    /// </summary>
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = default!;

    /// <summary>
    /// This event fired on every user keystroke that changes the NumberInput value.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter] public Expression<Func<string>> ValueExpression { get; set; } = default!;

    #endregion
}
