using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBootstrap;

public partial class Button : BaseComponent
{
    #region Members

    private ButtonColor color = ButtonColor.None;

    private Size size = Size.None;

    private bool outline;

    private bool disabled;

    private bool active;

    private bool block;

    private bool loading;

    private string loadingText;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Button());
        builder.Append(BootstrapClassProvider.ButtonColor(Color), Color != ButtonColor.None && !Outline);
        builder.Append(BootstrapClassProvider.ButtonOutline(Color), Color != ButtonColor.None && Outline);
        builder.Append(BootstrapClassProvider.ButtonSize(Size), Size != Size.None);
        builder.Append(BootstrapClassProvider.ButtonDisabled(), disabled);
        builder.Append(BootstrapClassProvider.ButtonActive(), active);
        builder.Append(BootstrapClassProvider.ButtonBlock(), Block);
        builder.Append(BootstrapClassProvider.ButtonLoading(), Loading && LoadingTemplate != null);

        base.BuildClasses(builder);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder
            .OpenElement(Type.ToButtonTagName())
            .Id(ElementId)
            .Type(Type.ToButtonTypeString())
            .Class(ClassNames)
            .Style(StyleNames)
            .Disabled(Disabled)
            .AriaPressed(Active)
            .TabIndex(TabIndex);

        if (Type == ButtonType.Link)
        {
            builder.Role("button")
                .Href(To)
                .Target(Target);

            if (Disabled)
            {
                builder
                    .TabIndex(-1)
                    .AriaDisabled("true");
            }
        }


        Attributes ??= new Dictionary<string, object>();

        // tooltip
        if (string.IsNullOrWhiteSpace(TooltipTitle))
        {
            if (Attributes.TryGetValue("title", out object title))
                Attributes.Remove("title");
        }
        else if (!Disabled)
        {
            builder.DataBootstrap("toggle", "toggle");
            builder.DataBootstrap("placement", TooltipPlacement.ToTooltipPlacementName());

            if (!Attributes.TryGetValue("title", out object title))
                Attributes.Add("title", TooltipTitle);
            else
                Attributes["title"] = TooltipTitle;

            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", ElementId); });
        }

        builder.Attributes(Attributes);

        if (Loading && LoadingTemplate != null)
            builder.Content(LoadingTemplate);
        else
            builder.Content(ChildContent);

        builder.CloseElement();

        base.BuildRenderTree(builder);
    }

    protected override void OnInitialized()
    {
        this.loadingText = LoadingText;

        this.LoadingTemplate ??= ProvideDefaultLoadingTemplate();

        base.OnInitialized();
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
    [Parameter] public RenderFragment LoadingTemplate { get; set; }

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
    [Parameter] public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Displays informative text when users hover, focus, or tap an element.
    /// </summary>
    [Parameter] public string TooltipTitle { get; set; }

    /// <summary>
    /// Tooltip placement
    /// </summary>
    [Parameter] public BlazorBootstrap.TooltipPlacement TooltipPlacement { get; set; } = TooltipPlacement.Top;

    #endregion

    // TODO: Review
    // - Disable text wrapping: https://getbootstrap.com/docs/5.1/components/buttons/#disable-text-wrapping
    // - Toogle states: https://getbootstrap.com/docs/5.1/components/buttons/#toggle-states
    // - IDispose
}
