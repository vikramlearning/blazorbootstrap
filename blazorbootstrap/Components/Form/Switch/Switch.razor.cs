namespace BlazorBootstrap;

/// <summary>
/// Use the Blazor Bootstrap <see cref="Switch"/>> component to show the consistent cross-browser and cross-device custom checkboxes.
/// </summary>
public partial class Switch : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private bool oldValue;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        oldValue = Value;
        
        fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        await base.OnInitializedAsync();
    }

    /// <inheritdoc />
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
        if (!bool.TryParse(args.Value?.ToString(), out var newValue))
            return;
        
        Value = newValue;

        await ValueChanged.InvokeAsync(Value);

        EditContext?.NotifyFieldChanged(fieldIdentifier);

        oldValue = Value;
    }


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value; break;
                case nameof(EditContext): EditContext = (EditContext)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Label): Label = (string)parameter.Value; break;
                case nameof(Reverse): Reverse = (bool)parameter.Value; break;
                
                case nameof(Value): Value = (bool)parameter.Value; break;
                case nameof(ValueChanged): ValueChanged = (EventCallback<bool>)parameter.Value; break;
                case nameof(ValueExpression): ValueExpression = (Expression<Func<bool>>)parameter.Value; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Disabled { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    private string FieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string Label { get; set; } = default!;

    /// <summary>
    /// Determines whether to put the switch on the opposite side.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Reverse { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Value { get; set; }

    /// <summary>
    /// This event is fired when the switch selection changes.
    /// </summary>
    [Parameter] public EventCallback<bool> ValueChanged { get; set; } 

    /// <summary>
    /// An expression that identifies the bound value.
    /// </summary>
    [Parameter] public Expression<Func<bool>> ValueExpression { get; set; } = default!;

    #endregion
}
