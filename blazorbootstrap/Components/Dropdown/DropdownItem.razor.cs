namespace BlazorBootstrap;

public partial class DropdownItem
{
    #region Events

    #endregion

    #region Members

    private bool active;

    private bool disabled;

    private bool isFirstRenderComplete = false;

    private bool previousActive;

    private bool previousDisabled;

    private int? previousTabIndex;

    private ButtonType previousType;

    private Target previousTarget;

    private bool setButtonAttributesAgain = false;

    private string buttonTypeString => Type.ToButtonTypeString();

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.DropdownItem());
        builder.Append(BootstrapClassProvider.Active(), Active);
        builder.Append(BootstrapClassProvider.Disabled(), Disabled);

        base.BuildClasses(builder);
    }

    protected override void OnInitialized()
    {
        Attributes ??= new Dictionary<string, object>();

        previousActive = Active;
        previousDisabled = Disabled;
        previousTabIndex = TabIndex;
        previousTarget = Target;
        previousType = Type;

        SetAttributes();

        base.OnInitialized();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            isFirstRenderComplete = true;

        base.OnAfterRender(firstRender);
    }

    protected override async Task OnParametersSetAsync()
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
        Attributes ??= new Dictionary<string, object>();

        if (Active && !Attributes.TryGetValue("aria-current", out _))
            Attributes.Add("aria-current", "true");
        else if (!Active && Attributes.TryGetValue("aria-current", out _))
            Attributes.Remove("aria-current");

        // 'a' tag
        if (Type == ButtonType.Link)
        {
            if (!Attributes.TryGetValue("role", out _))
                Attributes.Add("role", "button");

            if (!Attributes.TryGetValue("href", out _))
                Attributes.Add("href", To);

            if (Target != Target.None)
            {
                if (!Attributes.TryGetValue("target", out _))
                    Attributes.Add("target", Target.ToTargetString());
            }

            if (Disabled)
            {
                if (Attributes.TryGetValue("aria-disabled", out _))
                    Attributes["aria-disabled"] = "true";
                else
                    Attributes.Add("aria-disabled", "true");

                if (Attributes.TryGetValue("tabindex", out _))
                    Attributes["tabindex"] = -1;
                else
                    Attributes.Add("tabindex", -1);
            }
            else
            {
                if (Attributes.TryGetValue("aria-disabled", out _))
                    Attributes.Remove("aria-disabled");

                if (TabIndex is not null && !Attributes.TryGetValue("tabindex", out _))
                    Attributes.Add("tabindex", TabIndex);
                else if (TabIndex is null && Attributes.TryGetValue("tabindex", out _))
                    Attributes.Remove("tabindex");
            }
        }
        else // button, submit
        {
            if (Attributes.TryGetValue("role", out _))
                Attributes.Remove("role");

            if (Attributes.TryGetValue("href", out _))
                Attributes.Remove("href");

            if (Attributes.TryGetValue("target", out _))
                Attributes.Remove("target");

            if (Attributes.TryGetValue("aria-disabled", out _))
                Attributes.Remove("aria-disabled");

            // NOTE: This is handled in .razor page - #182
            //if (this.Disabled && !Attributes.TryGetValue("disabled", out _))
            //    Attributes.Add("disabled", "disabled");
            //else if (!this.Disabled && Attributes.TryGetValue("disabled", out _))
            //    Attributes.Remove("disabled");

            if (TabIndex is not null && !Attributes.TryGetValue("tabindex", out _))
                Attributes.Add("tabindex", TabIndex);
            else if (TabIndex is null && Attributes.TryGetValue("tabindex", out _))
                Attributes.Remove("tabindex");
        }
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// When set to 'true', places the component in the active state with active styling.
    /// </summary>
    [Parameter]
    public bool Active
    {
        get => active;
        set
        {
            active = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// When set to 'true', disables the component's functionality and places it in a disabled state.
    /// </summary>
    [Parameter]
    public bool Disabled
    {
        get => disabled;
        set
        {
            disabled = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="ChildContent"/>.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter] public int? TabIndex { get; set; }

    /// <summary>
    /// The target attribute specifies where to open the linked document for a <see cref="ButtonType.Link"/>.
    /// </summary>
    [Parameter] public Target Target { get; set; } = Target.None;

    /// <summary>
    /// Denotes the target route of the <see cref="ButtonType.Link"/> button.
    /// </summary>
    [Parameter] public string? To { get; set; }

    /// <summary>
    /// Defines the button type.
    /// </summary>
    [Parameter] public ButtonType Type { get; set; } = ButtonType.Button;

    #endregion
}
