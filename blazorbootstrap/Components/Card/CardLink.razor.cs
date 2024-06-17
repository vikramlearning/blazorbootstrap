namespace BlazorBootstrap;

/// <summary>
/// This component represents a link within a <see cref="Card"/>.
/// </summary>
public partial class CardLink : BlazorBootstrapComponentBase
{
    #region Methods
    
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
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(TabIndex): TabIndex = (int?)parameter.Value; break;
                case nameof(Target): Target = (Target)parameter.Value; break;
                case nameof(To): To = (string)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        SetAttributes();

        return base.SetParametersAsync(ParameterView.Empty);
    }

    private void SetAttributes()
    {
        if (!AdditionalAttributes!.TryGetValue("href", out _))
            AdditionalAttributes.Add("href", To!);

        if (Target != Target.None)
            if (!AdditionalAttributes.TryGetValue("target", out _))
                AdditionalAttributes.Add("target", Target.ToTargetString()!);

        if (Disabled)
        {
            if (AdditionalAttributes.TryGetValue("aria-disabled", out _))
                AdditionalAttributes["aria-disabled"] = "true";
            else
                AdditionalAttributes.Add("aria-disabled", "true");

            if (AdditionalAttributes.TryGetValue("tabindex", out _))
                AdditionalAttributes["tabindex"] = -1;
            else
                AdditionalAttributes.Add("tabindex", -1);
        }
        else
        {
            if (AdditionalAttributes.TryGetValue("aria-disabled", out _))
                AdditionalAttributes.Remove("aria-disabled");

            if (TabIndex is not null && !AdditionalAttributes.TryGetValue("tabindex", out _))
                AdditionalAttributes.Add("tabindex", TabIndex);
            else if (TabIndex is null && AdditionalAttributes.TryGetValue("tabindex", out _))
                AdditionalAttributes.Remove("tabindex");
        }
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.CardLink, true),
            (BootstrapClass.Disabled, Disabled));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// If <see langword="true" />, disables the card link.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the card link tab index.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public int? TabIndex { get; set; }

    /// <summary>
    /// Gets or sets the card link target.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Target.None" />.
    /// </remarks>
    [Parameter]
    public Target Target { get; set; } = Target.None;

    /// <summary>
    /// Gets or sets the link href attribute.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? To { get; set; }

    #endregion
    
}
