namespace BlazorBootstrap;

public partial class Button : BaseComponent
{
    #region Members

    private string buttonTypeString => this.Type.ToButtonTypeString();

    private ButtonColor color = ButtonColor.None;

    private Size size = Size.None;

    private bool outline;

    private bool disabled, previousDisabled;

    private bool active, previousActive;

    private bool block;

    private bool loading;

    private string loadingText = default!;

    private string previousTooltipTitle = default!;

    private Position position;

    private ButtonType previousType;

    private Target previousTarget;

    private int? previousTabIndex;

    private bool setButtonAttributesAgain = false;

    private bool isFirstRenderComplete = false;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Button());
        builder.Append(BootstrapClassProvider.ButtonColor(Color), Color != ButtonColor.None && !Outline);
        builder.Append(BootstrapClassProvider.ButtonOutline(Color), Color != ButtonColor.None && Outline);
        builder.Append(BootstrapClassProvider.ButtonSize(Size), Size != Size.None);
        builder.Append(BootstrapClassProvider.ButtonDisabled(), Disabled && Type == ButtonType.Link);
        builder.Append(BootstrapClassProvider.ButtonActive(), Active);
        builder.Append(BootstrapClassProvider.ButtonBlock(), Block);
        builder.Append(BootstrapClassProvider.ButtonLoading(), Loading && LoadingTemplate != null);
        builder.Append(BootstrapClassProvider.ToPosition(this.Position), this.Position != Position.None);

        base.BuildClasses(builder);
    }

    protected override void OnInitialized()
    {
        this.previousDisabled = Disabled;
        this.previousActive = Active;
        this.loadingText = LoadingText;
        this.LoadingTemplate ??= ProvideDefaultLoadingTemplate();
        this.previousType = Type;
        this.previousTarget = Target;
        this.previousTabIndex = TabIndex;
        this.previousTooltipTitle = TooltipTitle;

        SetAttributes();

        base.OnInitialized();

        if (!string.IsNullOrWhiteSpace(this.TooltipTitle))
        {
            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", ElementRef); });
        }
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

            if (setButtonAttributesAgain)
            {
                SetAttributes();
                setButtonAttributesAgain = false;
            }

            // additional scenario
            if (!Disabled && previousTooltipTitle != TooltipTitle)
            {
                previousTooltipTitle = TooltipTitle;

                if (Attributes is not null && Attributes.TryGetValue("title", out _))
                    Attributes["title"] = TooltipTitle;

                await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", ElementRef);
                await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.update", ElementRef);
            }
            else if (Disabled)
            {
                if (previousTooltipTitle != TooltipTitle)
                    previousTooltipTitle = TooltipTitle;

                if (Attributes is not null && Attributes.TryGetValue("title", out _))
                {
                    Attributes.Remove("title");
                    await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", ElementRef);
                }
            }
        }
    }

    private void SetAttributes()
    {
        Attributes ??= new Dictionary<string, object>();

        if (this.Active && !Attributes.TryGetValue("aria-pressed", out _))
            Attributes.Add("aria-pressed", "true");
        else if (!this.Active && Attributes.TryGetValue("aria-pressed", out _))
            Attributes.Remove("aria-pressed");

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

                if (this.TabIndex is not null && !Attributes.TryGetValue("tabindex", out _))
                    Attributes.Add("tabindex", TabIndex);
                else if (this.TabIndex is null && Attributes.TryGetValue("tabindex", out _))
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

            if (this.TabIndex is not null && !Attributes.TryGetValue("tabindex", out _))
                Attributes.Add("tabindex", TabIndex);
            else if (this.TabIndex is null && Attributes.TryGetValue("tabindex", out _))
                Attributes.Remove("tabindex");
        }

        // has tooltip and enabled        
        if (!string.IsNullOrWhiteSpace(TooltipTitle) && !Disabled)
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
        }
        // no tooltip and disabled
        else
        {
            if (Attributes.TryGetValue("data-bs-toggle", out _))
                Attributes.Remove("data-bs-toggle");

            if (Attributes.TryGetValue("data-bs-placement", out _))
                Attributes.Remove("data-bs-placement");

            if (Attributes.TryGetValue("title", out _))
                Attributes.Remove("title");
        }
    }

    protected virtual RenderFragment ProvideDefaultLoadingTemplate() => builder =>
    {
        builder.MarkupContent($"<span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\"></span> {this.loadingText}");
    };

    /// <summary>
    /// Shows the loading state and disables the button.
    /// </summary>
    /// <param name="text"></param>
    public void ShowLoading(string text = "")
    {
        this.loadingText = text;
        this.Loading = true;
        this.Disabled = true;
        this.StateHasChanged();
    }

    /// <summary>
    /// Hides the loading state and enables the button.
    /// </summary>
    public void HideLoading()
    {
        this.Loading = false;
        this.Disabled = false;
        this.StateHasChanged();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", ElementRef);
        }

        await base.DisposeAsync(disposing);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Defines the button type.
    /// </summary>
    [Parameter] public ButtonType Type { get; set; } = ButtonType.Button;

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

    /// <summary>
    /// Shows the loading spinner or a <see cref="LoadingTemplate"/>.
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
    /// Gets or sets the loadgin text.
    /// <see cref="LoadingTemplate"/> takes precedence.
    /// </summary>
    [Parameter] public string LoadingText { get; set; } = "Loading...";

    /// <summary>
    /// Gets or sets the component loading template.
    /// </summary>
    [Parameter] public RenderFragment LoadingTemplate { get; set; } = default!;

    /// <summary>
    /// Denotes the target route of the <see cref="ButtonType.Link"/> button.
    /// </summary>
    [Parameter] public string To { get; set; }

    /// <summary>
    /// The target attribute specifies where to open the linked document for a <see cref="ButtonType.Link"/>.
    /// </summary>
    [Parameter] public Target Target { get; set; } = Target.None;

    /// <summary>
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter] public int? TabIndex { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Button"/>.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Displays informative text when users hover, focus, or tap an element.
    /// </summary>
    [Parameter]
    public string TooltipTitle { get; set; } = default!;

    /// <summary>
    /// Tooltip placement
    /// </summary>
    [Parameter] public TooltipPlacement TooltipPlacement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Gets or sets the position.
    /// Use <see cref="Position"/> to modify a <see cref="Badge"/> and position it in the corner of a link or button.
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

    #endregion

    // TODO: Review
    // - Disable text wrapping: https://getbootstrap.com/docs/5.1/components/buttons/#disable-text-wrapping
    // - Toogle states: https://getbootstrap.com/docs/5.1/components/buttons/#toggle-states
    // - IDispose
}
