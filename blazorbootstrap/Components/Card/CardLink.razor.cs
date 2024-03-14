﻿namespace BlazorBootstrap;

public partial class CardLink : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool disabled;

    private bool isFirstRenderComplete = false;

    private bool previousDisabled;

    private int? previousTabIndex;

    private Target previousTarget;

    private bool setButtonAttributesAgain = false;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.CardLink);
        this.AddClass(BootstrapClassProvider.Disabled, Disabled);

        base.BuildClasses();
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
        previousTarget = Target;
        previousTabIndex = TabIndex;

        SetAttributes();

        base.OnInitialized();
    }

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
        Attributes ??= new Dictionary<string, object>();

        if (!Attributes.TryGetValue("href", out _))
            Attributes.Add("href", To!);

        if (Target != Target.None)
            if (!Attributes.TryGetValue("target", out _))
                Attributes.Add("target", Target.ToTargetString()!);

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

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="ChildContent" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

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
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter]
    public int? TabIndex { get; set; }

    /// <summary>
    /// The target attribute specifies where to open the linked document for a <see cref="CardLink" />.
    /// </summary>
    [Parameter]
    public Target Target { get; set; } = Target.None;

    /// <summary>
    /// Gets or sets the target route.
    /// </summary>
    [Parameter]
    public string? To { get; set; }

    #endregion
}
