namespace BlazorBootstrap;

public partial class Switch : BaseComponent
{
    #region Events

    /// <summary>
    /// This event fired when the switch selection changed.
    /// </summary>
    [Parameter] public EventCallback<bool> ValueChanged { get; set; } = default!;

    #endregion

    #region Members

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private bool disabled;

    private string reverse => this.Reverse ? BootstrapClassProvider.ChecksReverse() : "";

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        this.disabled = this.Disabled;

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Disables switch.
    /// </summary>
    public void Disable() => this.disabled = true;

    /// <summary>
    /// Enables switch.
    /// </summary>
    public void Enable() => this.disabled = false;

    private async Task OnChange(ChangeEventArgs e)
    {
        bool.TryParse(e.Value?.ToString(), out bool newValue);
        this.Value = newValue;
        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [Parameter]
    public bool Disabled
    {
        get => disabled;
        set => disabled = value;
    }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [Parameter] public string Label { get; set; } = default!;

    /// <summary>
    /// Determines whether to put the switch on the opposite side.
    /// </summary>
    [Parameter] public bool Reverse { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter] public bool Value { get; set; }

    [Parameter] public Expression<Func<bool>> ValueExpression { get; set; } = default!;

    #endregion
}
