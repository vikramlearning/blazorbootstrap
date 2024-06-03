namespace BlazorBootstrap;

/// <summary>
/// Use Blazor Bootstrap button styles for actions in forms, dialogs, and more with support for multiple sizes, states, etc. <br/>
/// This component is based on the <see href="https://getbootstrap.com/docs/5.0/components/buttons/">Bootstrap Button</see> component.
/// </summary>
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
                await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

        await base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            isFirstRenderComplete = true;

            if (!string.IsNullOrWhiteSpace(TooltipTitle))
                await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", Element);
        }

        await base.OnAfterRenderAsync(firstRender);
    }
    
    /// <inheritdoc />
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

    /// <inheritdoc />
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
                await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
            }
            else if (previousTooltipTitle != TooltipTitle || previousTooltipColor != TooltipColor)
            {
                await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
                await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.update", Element);
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
#if NET6_0
        StateHasChanged();
        #endif
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
#if NET6_0
        StateHasChanged();
#endif
    }

    protected virtual RenderFragment ProvideDefaultLoadingTemplate() => builder => { builder.AddMarkupContent(0, $"<span class=\"spinner-border spinner-border-{(Size == ButtonSize.None ? ButtonSize.Medium : Size).ToButtonSpinnerSizeClass()}\" role=\"status\" aria-hidden=\"true\"></span> {LoadingText}"); };

    private void SetAttributes()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (Active && !AdditionalAttributes.TryGetValue("aria-pressed", out _))
            AdditionalAttributes.Add("aria-pressed", "true");
        else if (!Active)
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
                AdditionalAttributes["aria-disabled"] = "true";
                AdditionalAttributes["tabindex"] = -1;
            }
            else
            {
                AdditionalAttributes.Remove("aria-disabled");

                if (TabIndex is not null && !AdditionalAttributes.TryGetValue("tabindex", out _))
                    AdditionalAttributes.Add("tabindex", TabIndex);
                else if (TabIndex is null)
                    AdditionalAttributes.Remove("tabindex");
            }
        }
        else // button, submit
        {
            AdditionalAttributes.Remove("role");
            AdditionalAttributes.Remove("href");
            AdditionalAttributes.Remove("target");
            AdditionalAttributes.Remove("aria-disabled");

            // NOTE: This is handled in .razor page - #182
            //if (this.Disabled && !Attributes.TryGetValue("disabled", out _))
            //    Attributes.Add("disabled", "disabled");
            //else if (!this.Disabled && Attributes.TryGetValue("disabled", out _))
            //    Attributes.Remove("disabled");

            if (TabIndex is not null && !AdditionalAttributes.TryGetValue("tabindex", out _))
                AdditionalAttributes.Add("tabindex", TabIndex);
            else if (TabIndex is null)
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

            AdditionalAttributes["title"] = TooltipTitle;

            AdditionalAttributes["data-bs-custom-class"] = TooltipColor.ToTooltipColorClass()!;
        }
        // button disabled (or) tooltip text empty
        else
        {
            AdditionalAttributes.Remove("data-bs-toggle");
            AdditionalAttributes.Remove("data-bs-placement");
                            AdditionalAttributes.Remove("title");
            AdditionalAttributes.Remove("data-bs-custom-class", out _);
        }
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Button)
            .AddClass(Color.ToButtonColorClass(), Color != ButtonColor.None && !Outline)
            .AddClass(Color.ToButtonOutlineColorClass(), Color != ButtonColor.None && Outline)
            .AddClass(Size.ToButtonSizeClass(), Size != ButtonSize.None)
            .AddClass(BootstrapClass.ButtonDisabled, Disabled && Type == ButtonType.Link)
            .AddClass(BootstrapClass.ButtonActive, Active)
            .AddClass(BootstrapClass.ButtonBlock, Block)
            .AddClass(BootstrapClass.ButtonLoading!, Loading && LoadingTemplate is not null)
            .AddClass(Position.ToPositionClass(), Position != Position.None)
            .Build();

    /// <summary>
    /// Gets or sets the button active state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the block level button.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Block { get; set; }

    private string ButtonTypeString => Type.ToButtonTypeString()!;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// If <see langword="true" />, shows the loading spinner or a <see cref="LoadingTemplate" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets the button loading template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; } = default!;

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
    /// Default value is <see langword="false" />.
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
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="null" />.
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
    /// Default value is <see langword="null" />.
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
