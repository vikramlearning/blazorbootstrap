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

    private string previousTooltipTitle = default!;

    private ButtonType previousType;

    private bool setButtonAttributesAgain = false;

    private TooltipColor previousTooltipColor = default!;

    #endregion

    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.Button)
        .AddClass(Color.ToButtonColorClass(), Color != ButtonColor.None && !Outline)
        .AddClass(Color.ToButtonOutlineColorClass(), Color != ButtonColor.None && Outline)
        .AddClass(Size.ToButtonSizeClass(), Size != Size.None)
        .AddClass(BootstrapClass.ButtonDisabled, Disabled && Type == ButtonType.Link)
        .AddClass(BootstrapClass.ButtonActive, Active)
        .AddClass(BootstrapClass.ButtonBlock, Block)
        .AddClass(BootstrapClass.ButtonLoading!, Loading && LoadingTemplate is not null)
        .AddClass(Position.ToPositionClass(), Position != Position.None)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

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

    protected virtual RenderFragment ProvideDefaultLoadingTemplate() => builder => { builder.AddMarkupContent(0, $"<span class=\"spinner-border spinner-border-{(Size == Size.None ? Size.Medium : Size).ToSizeClass()}\" role=\"status\" aria-hidden=\"true\"></span> {LoadingText}"); };

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

    /// <summary>
    /// When set to 'true', places the component in the active state with active styling.
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Makes the button to span the full width of a parent.
    /// </summary>
    [Parameter]
    public bool Block { get; set; }

    private string buttonTypeString => Type.ToButtonTypeString()!;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the button color.
    /// </summary>
    [Parameter]
    public ButtonColor Color { get; set; } = ButtonColor.None;

    /// <summary>
    /// When set to 'true', disables the component's functionality and places it in a disabled state.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Shows the loading spinner or a <see cref="LoadingTemplate" />.
    /// </summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets the component loading template.
    /// </summary>
    [Parameter]
    public RenderFragment LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the loadgin text.
    /// <see cref="LoadingTemplate" /> takes precedence.
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Loading...";

    /// <summary>
    /// Makes the button to have the outlines.
    /// </summary>
    [Parameter]
    public bool Outline { get; set; }

    /// <summary>
    /// Gets or sets the position.
    /// Use <see cref="Position" /> to modify a <see cref="Badge" /> and position it in the corner of a link or button.
    /// </summary>
    [Parameter]
    public Position Position { get; set; }

    /// <summary>
    /// Changes the size of a button.
    /// </summary>
    [Parameter]
    public Size Size { get; set; } = Size.None;

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
    /// Gets or sets the tooltip color.
    /// </summary>
    [Parameter]
    public TooltipColor TooltipColor { get; set; }

    /// <summary>
    /// Tooltip placement
    /// </summary>
    [Parameter]
    public TooltipPlacement TooltipPlacement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Displays informative text when users hover, focus, or tap an element.
    /// </summary>
    [Parameter]
    public string TooltipTitle { get; set; } = default!;

    /// <summary>
    /// Defines the button type.
    /// </summary>
    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    #endregion

    // TODO: Review
    // - Disable text wrapping: https://getbootstrap.com/docs/5.1/components/buttons/#disable-text-wrapping
    // - Toggle states: https://getbootstrap.com/docs/5.1/components/buttons/#toggle-states
    // - IDispose
}
