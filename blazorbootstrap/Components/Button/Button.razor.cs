namespace BlazorBootstrap;

public partial class Button : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isFirstRenderComplete = false;

    private bool previousActive;

    private bool previousDisabled;

    private int? previousTabIndex;

    private Target previousTarget;

    private string? previousTo = default!;

    private TooltipColor previousTooltipColor = default!;

    private string previousTooltipTitle = default!;

    private ButtonType previousType;

    private bool setButtonAttributesAgain = false;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing && !string.IsNullOrWhiteSpace(TooltipTitle) && IsRenderComplete)
            try
            {
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            isFirstRenderComplete = true;

            if (!string.IsNullOrWhiteSpace(TooltipTitle))
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", Element);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        previousDisabled = Disabled;
        previousActive = Active;
        previousType = Type;
        previousTarget = Target;
        previousTabIndex = TabIndex;
        previousTo = To;
        previousTooltipTitle = TooltipTitle;
        previousTooltipColor = TooltipColor;

        LoadingTemplate ??= ProvideDefaultLoadingTemplate();

        SetAttributes();

        base.OnInitialized();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (isFirstRenderComplete)
        {
            if (previousDisabled != Disabled)
            {
                previousDisabled = Disabled;
                setButtonAttributesAgain = true;
            }

            if (previousActive != Active)
            {
                previousActive = Active;
                setButtonAttributesAgain = true;
            }

            if (previousType != Type)
            {
                previousType = Type;
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

            if (previousTo != To)
            {
                previousTo = To;
                setButtonAttributesAgain = true;
            }

            if (previousTooltipTitle != TooltipTitle || previousTooltipColor != TooltipColor) setButtonAttributesAgain = true;

            if (setButtonAttributesAgain)
            {
                setButtonAttributesAgain = false;
                SetAttributes();
            }

            // additional scenario
            // NOTE: do not change the below sequence
            if (Disabled)
            {
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
            }
            else if (previousTooltipTitle != TooltipTitle || previousTooltipColor != TooltipColor)
            {
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.update", Element);
            }

            previousTooltipTitle = TooltipTitle;
            previousTooltipColor = TooltipColor;
        }
    }

    /// <summary>
    /// Hides the loading state and enables the button.
    /// </summary>
    public void HideLoading()
    {
        Loading = false;
        Disabled = false;
        StateHasChanged();
    }

    /// <summary>
    /// Shows the loading state and disables the button.
    /// </summary>
    /// <param name="text"></param>
    public void ShowLoading(string text = "")
    {
        LoadingText = text;
        Loading = true;
        Disabled = true;
        StateHasChanged();
    }

    protected virtual RenderFragment ProvideDefaultLoadingTemplate() => builder => { builder.AddMarkupContent(0, $"<span class=\"spinner-border spinner-border-{(Size == ButtonSize.None ? ButtonSize.Medium : Size).ToButtonSpinnerSizeClass()}\" role=\"status\" aria-hidden=\"true\"></span> {LoadingText}"); };

    private void SetAttributes()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (Active && !AdditionalAttributes.TryGetValue("aria-pressed", out _))
            AdditionalAttributes.Add("aria-pressed", "true");
        else if (!Active && AdditionalAttributes.TryGetValue("aria-pressed", out _))
            AdditionalAttributes.Remove("aria-pressed");

        // 'a' tag
        if (Type == ButtonType.Link)
        {
            if (!AdditionalAttributes.TryGetValue("role", out _))
                AdditionalAttributes.Add("role", "button");

            // To can be changed when the Button is used within a Virtualize component
            if (!AdditionalAttributes.TryGetValue("href", out _))
                AdditionalAttributes.Add("href", To!);
            else
                AdditionalAttributes["href"] = To!;

            if (Target != Target.None)
                if (!AdditionalAttributes.TryGetValue("target", out _))
                    AdditionalAttributes.Add("target", Target.ToTargetString()!);
                else
                    AdditionalAttributes["target"] = Target.ToTargetString()!;

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

        // button enabled (and) tooltip text not empty
        if (!Disabled && !string.IsNullOrWhiteSpace(TooltipTitle))
        {
            // Ref: https://getbootstrap.com/docs/5.2/components/buttons/#toggle-states
            // The below code creates an issue when the `button` or `a` element has a tooltip.
            //if (!Attributes.TryGetValue("data-bs-toggle", out _))
            //    Attributes.Add("data-bs-toggle", "button");

            if (!AdditionalAttributes.TryGetValue("data-bs-placement", out _))
                AdditionalAttributes.Add("data-bs-placement", TooltipPlacement.ToTooltipPlacementName());

            if (AdditionalAttributes.TryGetValue("title", out _))
                AdditionalAttributes["title"] = TooltipTitle;
            else
                AdditionalAttributes.Add("title", TooltipTitle);

            if (AdditionalAttributes.TryGetValue("data-bs-custom-class", out _))
                AdditionalAttributes["data-bs-custom-class"] = TooltipColor.ToTooltipColorClass()!;
            else
                AdditionalAttributes.Add("data-bs-custom-class", TooltipColor.ToTooltipColorClass()!);
        }
        // button disabled (or) tooltip text empty
        else
        {
            if (AdditionalAttributes.TryGetValue("data-bs-toggle", out _))
                AdditionalAttributes.Remove("data-bs-toggle");

            if (AdditionalAttributes.TryGetValue("data-bs-placement", out _))
                AdditionalAttributes.Remove("data-bs-placement");

            if (AdditionalAttributes.TryGetValue("title", out _))
                AdditionalAttributes.Remove("title");

            if (AdditionalAttributes.TryGetValue("data-bs-custom-class", out _))
                AdditionalAttributes.Remove("data-bs-custom-class");
        }
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Button, true),
            (Color.ToButtonColorClass(), Color != ButtonColor.None && !Outline),
            (Color.ToButtonOutlineColorClass(), Color != ButtonColor.None && Outline),
            (Size.ToButtonSizeClass(), Size != ButtonSize.None),
            (BootstrapClass.ButtonDisabled, Disabled && Type == ButtonType.Link),
            (BootstrapClass.ButtonActive, Active),
            (BootstrapClass.ButtonBlock, Block),
            (BootstrapClass.ButtonLoading!, Loading && LoadingTemplate is not null),
            (Position.ToPositionClass(), Position != Position.None));

    /// <summary>
    /// Gets or sets the button active state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the block level button.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Block { get; set; }

    private string buttonTypeString => Type.ToButtonTypeString()!;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the button color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ButtonColor.None" />.
    /// </remarks>
    [Parameter]
    public ButtonColor Color { get; set; } = ButtonColor.None;

    /// <summary>
    /// Gets or sets the button disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// If <see langword="true" />, shows the loading spinner or a <see cref="LoadingTemplate" />.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets the button loading template.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the loading text.
    /// <see cref="LoadingTemplate" /> takes precedence.
    /// </summary>
    /// <remarks>
    /// Default value is 'Loading...'.
    /// </remarks>
    [Parameter]
    public string LoadingText { get; set; } = "Loading...";

    /// <summary>
    /// Gets or sets the button outline.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool Outline { get; set; }

    /// <summary>
    /// Gets or sets the position.
    /// Use <see cref="Position" /> to modify a <see cref="Badge" /> and position it in the corner of a link or button.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Position.None" />.
    /// </remarks>
    [Parameter]
    public Position Position { get; set; } = Position.None;

    /// <summary>
    /// Gets or sets the button size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ButtonSize.None" />.
    /// </remarks>
    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.None;

    /// <summary>
    /// Gets or sets the button tab index.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public int? TabIndex { get; set; }

    /// <summary>
    /// Gets or sets the link button target.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Target.None" />
    /// </remarks>
    [Parameter]
    public Target Target { get; set; } = Target.None;

    /// <summary>
    /// Gets or sets the link button href attribute.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? To { get; set; }

    /// <summary>
    /// Gets or sets the button tooltip color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="TooltipColor.None" />.
    /// </remarks>
    [Parameter]
    public TooltipColor TooltipColor { get; set; } = TooltipColor.None;

    /// <summary>
    /// Gets or sets the button tooltip placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="TooltipPlacement.Top" />.
    /// </remarks>
    [Parameter]
    public TooltipPlacement TooltipPlacement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Gets or sets the button tooltip title.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string TooltipTitle { get; set; } = default!;

    /// <summary>
    /// Gets or sets the button type.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ButtonType.Button" />.
    /// </remarks>
    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    #endregion

    // TODO: Review
    // - Disable text wrapping: https://getbootstrap.com/docs/5.1/components/buttons/#disable-text-wrapping
}
