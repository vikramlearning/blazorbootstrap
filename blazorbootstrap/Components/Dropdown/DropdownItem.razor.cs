namespace BlazorBootstrap;

public partial class DropdownItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isFirstRenderComplete = false;

    private bool previousActive;

    private bool previousDisabled;

    private int? previousTabIndex;

    private Target previousTarget;

    private DropdownItemType previousType;

    private bool setButtonAttributesAgain = false;

    #endregion

    #region Methods

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            isFirstRenderComplete = true;

        base.OnAfterRender(firstRender);
    }

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        previousActive = Active;
        previousDisabled = Disabled;
        previousTabIndex = TabIndex;
        previousTarget = Target;
        previousType = Type;

        SetAttributes();

        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (isFirstRenderComplete)
        {
            if (previousActive != Active)
            {
                previousActive = Active;
                setButtonAttributesAgain = true;
            }

            if (previousDisabled != Disabled)
            {
                previousDisabled = Disabled;
                setButtonAttributesAgain = true;
            }

            if (previousTabIndex != TabIndex)
            {
                previousTabIndex = TabIndex;
                setButtonAttributesAgain = true;
            }

            if (previousTarget != Target)
            {
                previousTarget = Target;
                setButtonAttributesAgain = true;
            }

            if (previousType != Type)
            {
                previousType = Type;
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

        if (Active && !AdditionalAttributes.TryGetValue("aria-current", out _))
            AdditionalAttributes.Add("aria-current", "true");
        else if (!Active && AdditionalAttributes.TryGetValue("aria-current", out _))
            AdditionalAttributes.Remove("aria-current");

        // 'a' tag
        if (Type == DropdownItemType.Link)
        {
            if (!AdditionalAttributes.TryGetValue("role", out _))
                AdditionalAttributes.Add("role", "button");

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
        else // button
        {
            if (AdditionalAttributes.TryGetValue("role", out _))
                AdditionalAttributes.Remove("role");

            if (AdditionalAttributes.TryGetValue("href", out _))
                AdditionalAttributes.Remove("href");

            if (AdditionalAttributes.TryGetValue("target", out _))
                AdditionalAttributes.Remove("target");

            if (AdditionalAttributes.TryGetValue("aria-disabled", out _))
                AdditionalAttributes.Remove("aria-disabled");

            // NOTE: This is handled in .razor page - #182
            //if (this.Disabled && !Attributes.TryGetValue("disabled", out _))
            //    Attributes.Add("disabled", "disabled");
            //else if (!this.Disabled && Attributes.TryGetValue("disabled", out _))
            //    Attributes.Remove("disabled");

            if (TabIndex is not null && !AdditionalAttributes.TryGetValue("tabindex", out _))
                AdditionalAttributes.Add("tabindex", TabIndex);
            else if (TabIndex is null && AdditionalAttributes.TryGetValue("tabindex", out _))
                AdditionalAttributes.Remove("tabindex");
        }
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.DropdownItem, true),
            (BootstrapClass.Active, Active),
            (BootstrapClass.Disabled, Disabled));

    /// <summary>
    /// Gets or sets the dropdown item active state.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the dropdown item active state.")]
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// If <see langword="true" />, dropdown item will be disabled.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, dropdown item will be disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the dropdown item tab index.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the dropdown item tab index.")]
    [Parameter]
    public int? TabIndex { get; set; }

    /// <summary>
    /// Gets or sets the target of dropdown item (if the type is link).
    /// <para>
    /// Default value is <see cref="Target.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(Target.None)]
    [Description("Gets or sets the target of dropdown item (if the type is link).")]
    [Parameter]
    public Target Target { get; set; } = Target.None;

    /// <summary>
    /// Get or sets the link href attribute (if the type is link).
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Get or sets the link href attribute (if the type is link).")]
    [Parameter]
    public string? To { get; set; }

    /// <summary>
    /// Gets or sets the dropdown item type.
    /// <para>
    /// Default value is <see cref="DropdownItemType.Button" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(DropdownItemType.Button)]
    [Description("Gets or sets the dropdown item type.")]
    [Parameter]
    public DropdownItemType Type { get; set; } = DropdownItemType.Button;

    #endregion
}
