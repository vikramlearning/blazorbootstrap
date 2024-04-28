namespace BlazorBootstrap;

public partial class DropdownItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isFirstRenderComplete = false;

    private bool previousActive;

    private bool previousDisabled;

    private int? previousTabIndex;

    private Target previousTarget;

    private ButtonType previousType;

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
        if (Type == ButtonType.Link)
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
        else // button, submit
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
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.DropdownItem)
            .AddClass(BootstrapClass.Active, Active)
            .AddClass(BootstrapClass.Disabled, Disabled)
            .Build();

    /// <summary>
    /// When set to 'true', places the component in the active state with active styling.
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    private string buttonTypeString => Type.ToButtonTypeString()!;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// When set to 'true', disables the component's functionality and places it in a disabled state.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter]
    public int? TabIndex { get; set; }

    /// <summary>
    /// The target attribute specifies where to open the linked document for a <see cref="ButtonType.Link" />.
    /// </summary>
    [Parameter]
    public Target Target { get; set; } = Target.None;

    /// <summary>
    /// Denotes the target route of the <see cref="ButtonType.Link" /> button.
    /// </summary>
    [Parameter]
    public string? To { get; set; }

    /// <summary>
    /// Defines the button type.
    /// </summary>
    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    #endregion
}
