namespace BlazorBootstrap;

public partial class Switch : BaseComponent
{
    #region Events

    /// <summary>
    /// This event fired when the switch selection changed.
    /// </summary>
    [Parameter] public EventCallback<bool> ValueChanged { get; set; }

    #endregion

    #region Members

    private FieldIdentifier fieldIdentifier;

    private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    private bool disabled;

    private string reverse => this.Reverse ? BootstrapClassProvider.ChecksReverse() : "";

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Checks());
        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        this.disabled = this.Disabled;

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Disables currency input.
    /// </summary>
    public void Disable()
    {
        this.disabled = true;
    }

    /// <summary>
    /// Enables currency input.
    /// </summary>
    public void Enable()
    {
        this.disabled = false;
    }

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
    [Parameter] public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [Parameter] public string Label { get; set; }

    /// <summary>
    /// Determines whether to put the switch on the opposite side.
    /// </summary>
    [Parameter] public bool Reverse { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter] public bool Value { get; set; }

    [Parameter] public Expression<Func<bool>> ValueExpression { get; set; }

    #endregion
}
