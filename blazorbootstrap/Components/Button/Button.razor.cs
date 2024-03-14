namespace BlazorBootstrap;

public partial class Button : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool active;

    private bool block;

    private ButtonColor color = ButtonColor.None;

    private bool disabled;

    private bool isFirstRenderComplete = false;

    private bool loading;

    private string loadingText = default!;

    private bool outline;

    private Position position;

    private bool previousActive;

    private bool previousDisabled;

    private int? previousTabIndex;

    private Target previousTarget;

    private string? previousTo = default!;

    private string previousTooltipTitle = default!;

    private ButtonType previousType;

    private bool setButtonAttributesAgain = false;

    private Size size = Size.None;

    private TooltipColor tooltipColor = default!;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Button);
        this.AddClass(BootstrapClassProvider.ButtonColor(Color), Color != ButtonColor.None && !Outline);
        this.AddClass(BootstrapClassProvider.ButtonOutline(Color), Color != ButtonColor.None && Outline);
        this.AddClass(BootstrapClassProvider.ButtonSize(Size), Size != Size.None);
        this.AddClass(BootstrapClassProvider.ButtonDisabled, Disabled && Type == ButtonType.Link);
        this.AddClass(BootstrapClassProvider.ButtonActive, Active);
        this.AddClass(BootstrapClassProvider.ButtonBlock, Block);
        this.AddClass(BootstrapClassProvider.ButtonLoading!, Loading && LoadingTemplate is not null);
        this.AddClass(BootstrapClassProvider.ToPosition(Position), Position != Position.None);

        base.BuildClasses();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && !string.IsNullOrWhiteSpace(TooltipTitle) && Rendered)
            try
            {
                await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", ElementRef);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

        await base.DisposeAsync(disposing);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            isFirstRenderComplete = true;

        base.OnAfterRender(firstRender);
    }

    protected override void OnInitialized()
    {
        previousDisabled = Disabled;
        previousActive = Active;
        loadingText = LoadingText;
        LoadingTemplate ??= ProvideDefaultLoadingTemplate();
        previousType = Type;
        previousTarget = Target;
        previousTabIndex = TabIndex;
        previousTo = To;
        previousTooltipTitle = TooltipTitle;
        tooltipColor = TooltipColor;

        SetAttributes();

        base.OnInitialized();

        if (!string.IsNullOrWhiteSpace(TooltipTitle)) QueueAfterRenderAction(async () => await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", ElementRef), new RenderPriority());
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

            if (previousTooltipTitle != TooltipTitle || tooltipColor != TooltipColor) setButtonAttributesAgain = true;

            if (setButtonAttributesAgain)
            {
                setButtonAttributesAgain = false;
                SetAttributes();
            }

            // additional scenario
            // NOTE: do not change the below sequence
            if (Disabled)
            {
                await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", ElementRef);
            }
            else if (previousTooltipTitle != TooltipTitle || tooltipColor != TooltipColor)
            {
                await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", ElementRef);
                await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.update", ElementRef);
            }

            previousTooltipTitle = TooltipTitle;
            tooltipColor = TooltipColor;
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
        loadingText = text;
        Loading = true;
        Disabled = true;
        StateHasChanged();
    }

    protected virtual RenderFragment ProvideDefaultLoadingTemplate() => builder => { builder.AddMarkupContent(0, $"<span class=\"spinner-border spinner-border-{BootstrapClassProvider.ToSize(Size == Size.None ? Size.Medium : Size)}\" role=\"status\" aria-hidden=\"true\"></span> {loadingText}"); };

    private void SetAttributes()
    {
        Attributes ??= new Dictionary<string, object>();

        if (Active && !Attributes.TryGetValue("aria-pressed", out _))
            Attributes.Add("aria-pressed", "true");
        else if (!Active && Attributes.TryGetValue("aria-pressed", out _))
            Attributes.Remove("aria-pressed");

        // 'a' tag
        if (Type == ButtonType.Link)
        {
            if (!Attributes.TryGetValue("role", out _))
                Attributes.Add("role", "button");

            // To can be changed when the Button is used within a Virtualize component
            if (!Attributes.TryGetValue("href", out _))
                Attributes.Add("href", To!);
            else
                Attributes["href"] = To!;

            if (Target != Target.None)
                if (!Attributes.TryGetValue("target", out _))
                    Attributes.Add("target", Target.ToTargetString()!);
                else
                    Attributes["target"] = Target.ToTargetString()!;

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

        // button enabled (and) tooltip text not empty
        if (!Disabled && !string.IsNullOrWhiteSpace(TooltipTitle))
        {
            // Ref: https://getbootstrap.com/docs/5.2/components/buttons/#toggle-states
            // The below code creates an issue when the `button` or `a` element has a tooltip.
            //if (!Attributes.TryGetValue("data-bs-toggle", out _))
            //    Attributes.Add("data-bs-toggle", "button");

            if (!Attributes.TryGetValue("data-bs-placement", out _))
                Attributes.Add("data-bs-placement", TooltipPlacement.ToTooltipPlacementName());

            if (Attributes.TryGetValue("title", out _))
                Attributes["title"] = TooltipTitle;
            else
                Attributes.Add("title", TooltipTitle);

            if (Attributes.TryGetValue("data-bs-custom-class", out _))
                Attributes["data-bs-custom-class"] = BootstrapClassProvider.TooltipColor(TooltipColor)!;
            else
                Attributes.Add("data-bs-custom-class", BootstrapClassProvider.TooltipColor(TooltipColor)!);
        }
        // button disabled (or) tooltip text empty
        else
        {
            if (Attributes.TryGetValue("data-bs-toggle", out _))
                Attributes.Remove("data-bs-toggle");

            if (Attributes.TryGetValue("data-bs-placement", out _))
                Attributes.Remove("data-bs-placement");

            if (Attributes.TryGetValue("title", out _))
                Attributes.Remove("title");

            if (Attributes.TryGetValue("data-bs-custom-class", out _))
                Attributes.Remove("data-bs-custom-class");
        }
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
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
    /// Makes the button to span the full width of a parent.
    /// </summary>
    [Parameter]
    public bool Block
    {
        get => block;
        set
        {
            block = value;
            DirtyClasses();
        }
    }

    private string buttonTypeString => Type.ToButtonTypeString()!;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Button" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the button color.
    /// </summary>
    [Parameter]
    public ButtonColor Color
    {
        get => color;
        set
        {
            color = value;
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
    /// Shows the loading spinner or a <see cref="LoadingTemplate" />.
    /// </summary>
    [Parameter]
    public bool Loading
    {
        get => loading;
        set
        {
            loading = value;
            DirtyClasses();
        }
    }

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
    public bool Outline
    {
        get => outline;
        set
        {
            outline = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the position.
    /// Use <see cref="Position" /> to modify a <see cref="Badge" /> and position it in the corner of a link or button.
    /// </summary>
    [Parameter]
    public Position Position
    {
        get => position;
        set
        {
            position = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Changes the size of a button.
    /// </summary>
    [Parameter]
    public Size Size
    {
        get => size;
        set
        {
            size = value;
            DirtyClasses();
        }
    }

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
