namespace BlazorBootstrap;

/// <summary>
/// This component represents a link within a <see cref="Card"/>.
/// </summary>
public partial class CardLink : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isFirstRenderComplete = false;

    private bool previousDisabled;

    private int? previousTabIndex;

    private Target previousTarget;

    private bool setButtonAttributesAgain = false;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            isFirstRenderComplete = true;

        base.OnAfterRender(firstRender);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        previousDisabled = Disabled;
        previousTarget = Target;
        previousTabIndex = TabIndex;

        SetAttributes();

        base.OnInitialized();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (isFirstRenderComplete)
        {
            if (previousDisabled != Disabled)
            {
                previousDisabled = Disabled;
                setButtonAttributesAgain = true;
            }

            if (previousTarget != Target)
            {
                previousTarget = Target;
                setButtonAttributesAgain = true;
            }

            if (previousTabIndex != TabIndex)
            {
                previousTabIndex = TabIndex;
                setButtonAttributesAgain = true;
            }

            if (setButtonAttributesAgain)
            {
                setButtonAttributesAgain = false;
                SetAttributes();
            }
        }
    }

    private void SetAttributes()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (!AdditionalAttributes.TryGetValue("href", out _))
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
