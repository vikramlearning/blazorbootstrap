﻿using Microsoft.AspNetCore.Components.Rendering;
using System.Text;

namespace BlazorBootstrap;

/// <summary>
/// Use Blazor Bootstrap button styles for actions in forms, dialogs, and more with support for multiple sizes, states, etc. <br/>
/// This component is based on the <see href="https://getbootstrap.com/docs/5.0/components/buttons/">Bootstrap Button</see> component.
/// </summary>
public sealed class Button : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isFirstRenderComplete = false;

    private bool previousActive;

    private bool previousDisabled;

    private int? previousTabIndex;

    private Target previousTarget;

    private string? previousTo;

    private TooltipColor previousTooltipColor;

    private string? previousTooltipTitle;

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

    /// <summary>
    /// Default <see cref="LoadingTemplate"/> for a button in case the user does not provide one.
    /// </summary>
    /// <returns>The default loading template</returns>
    private RenderFragment ProvideDefaultLoadingTemplate() => builder =>
    {
        builder.AddMarkupContent(0, $"<span class=\"spinner-border spinner-border-{EnumExtensions.ButtonSpinnerSizeClassMap[Size]}\" role=\"status\" aria-hidden=\"true\"></span> {LoadingText}");
    };

    private void SetAttributes()
    {
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
                    AdditionalAttributes.Add("target", EnumExtensions.TargetStringMap[Target]);
                else
                    AdditionalAttributes["target"] = EnumExtensions.TargetStringMap[Target];

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

            if (TabIndex is not null && !AdditionalAttributes.TryGetValue("tabindex", out _))
                AdditionalAttributes.Add("tabindex", TabIndex);
            else if (TabIndex is null)
                AdditionalAttributes.Remove("tabindex");
        }

        // button enabled (and) tooltip text not empty
        if (!Disabled && !String.IsNullOrWhiteSpace(TooltipTitle))
        {
            // Ref: https://getbootstrap.com/docs/5.2/components/buttons/#toggle-states
            // The below code creates an issue when the `button` or `a` element has a tooltip.

            if (!AdditionalAttributes.TryGetValue("data-bs-placement", out _))
                AdditionalAttributes.Add("data-bs-placement", EnumExtensions.TooltipPlacementNameMap[TooltipPlacement]);

            AdditionalAttributes["title"] = TooltipTitle;

            if (TooltipColor != TooltipColor.None)
            {
                AdditionalAttributes["data-bs-custom-class"] = EnumExtensions.TooltipColorClassMap[TooltipColor];
            }
        }
        else // button disabled (or) tooltip text empty
        {
            AdditionalAttributes.Remove("data-bs-toggle");
            AdditionalAttributes.Remove("data-bs-placement");
            AdditionalAttributes.Remove("title");
            AdditionalAttributes.Remove("data-bs-custom-class");
        }
    }


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case var _ when String.Equals(parameter.Name, nameof(Active), StringComparison.OrdinalIgnoreCase): Active = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Block), StringComparison.OrdinalIgnoreCase): Block = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Color), StringComparison.OrdinalIgnoreCase): Color = (ButtonColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Disabled), StringComparison.OrdinalIgnoreCase): Disabled = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Loading), StringComparison.OrdinalIgnoreCase): Loading = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LoadingTemplate), StringComparison.OrdinalIgnoreCase): LoadingTemplate = (RenderFragment?)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LoadingText), StringComparison.OrdinalIgnoreCase): LoadingText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Outline), StringComparison.OrdinalIgnoreCase): Outline = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Position), StringComparison.OrdinalIgnoreCase): Position = (Position)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (ButtonSize)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(TabIndex), StringComparison.OrdinalIgnoreCase): TabIndex = (int?)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Target), StringComparison.OrdinalIgnoreCase): Target = (Target)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(To), StringComparison.OrdinalIgnoreCase): To = (string?)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(TooltipColor), StringComparison.OrdinalIgnoreCase): TooltipColor = (TooltipColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(TooltipPlacement), StringComparison.OrdinalIgnoreCase): TooltipPlacement = (TooltipPlacement)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(TooltipTitle), StringComparison.OrdinalIgnoreCase): TooltipTitle = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Type), StringComparison.OrdinalIgnoreCase): 
                    Type = (ButtonType)parameter.Value;
                    ButtonTypeString = EnumExtensions.ButtonTypeStringMap[Type];
                    break;
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

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
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the button active state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the block level button.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Block { get; set; }

    private string ButtonTypeString { get; set; } = "";

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }  

    /// <summary>
    /// Gets or sets the button color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ButtonColor.None" />.
    /// </remarks>
    [Parameter] public ButtonColor Color { get; set; } = ButtonColor.None;

    /// <summary>
    /// Gets or sets the button disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// If <see langword="true" />, shows the loading spinner or a <see cref="LoadingTemplate" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets the button loading template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? LoadingTemplate { get; set; }  

    /// <summary>
    /// Gets or sets the loading text.
    /// <see cref="LoadingTemplate" /> takes precedence.
    /// </summary>
    /// <remarks>
    /// Default value is 'Loading...'.
    /// </remarks>
    [Parameter] public string LoadingText { get; set; } = "Loading...";
    
    /// <summary>
    /// Gets or sets the button outline.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Outline { get; set; }

    /// <summary>
    /// Gets or sets the position.
    /// Use <see cref="Position" /> to modify a <see cref="Badge" /> and position it in the corner of a link or button.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Position.None" />.
    /// </remarks>
    [Parameter] public Position Position { get; set; } = Position.None;

    /// <summary>
    /// Gets or sets the button size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ButtonSize.None" />.
    /// </remarks>
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.None;

    /// <summary>
    /// Gets or sets the button tab index.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public int? TabIndex { get; set; }

    /// <summary>
    /// Gets or sets the link button target.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Target.None" />
    /// </remarks>
    [Parameter] public Target Target { get; set; } = Target.None;

    /// <summary>
    /// Gets or sets the link button href attribute.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? To { get; set; }

    /// <summary>
    /// Gets or sets the button tooltip color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="TooltipColor.None" />.
    /// </remarks>
    [Parameter] public TooltipColor TooltipColor { get; set; } = TooltipColor.None;

    /// <summary>
    /// Gets or sets the button tooltip placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="TooltipPlacement.Top" />.
    /// </remarks>
    [Parameter] public TooltipPlacement TooltipPlacement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Gets or sets the button tooltip title.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? TooltipTitle { get; set; } 

    /// <summary>
    /// Gets or sets the button type.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ButtonType.Button" />.
    /// </remarks>
    [Parameter] public ButtonType Type { get; set; } = ButtonType.Button;

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion


    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var btnClasses = new StringBuilder(BootstrapClass.Button);
        btnClasses.Append(' ').Append(Outline ? EnumExtensions.ButtonOutlineColorClassMap[Color] : EnumExtensions.ButtonColorClassMap[Color]);

        btnClasses.Append(' ').Append(EnumExtensions.ButtonSizeClassMap[Size]);
        if (Disabled && Type == ButtonType.Link)
        {
            btnClasses.Append(' ').Append(BootstrapClass.ButtonDisabled);
        }

        if (Active)
        {
            btnClasses.Append(' ').Append(BootstrapClass.ButtonActive);
        }

        if (Block)
        {
            btnClasses.Append(' ').Append(BootstrapClass.ButtonBlock);
        }
        
        if (Loading && LoadingTemplate is not null)
        {
            btnClasses.Append(' ').Append(BootstrapClass.ButtonLoading);
        }

        btnClasses.Append(' ').Append(EnumExtensions.PositionClassMap[Position]);
        btnClasses.Append(' ').Append(Class);


        builder.OpenElement(0, Type == ButtonType.Link ? "a" : "button");
        builder.AddAttribute(1, "type", ButtonTypeString);
        builder.AddAttribute(2, "id", Id);
        builder.AddAttribute(3, "class", btnClasses.ToString());
        builder.AddAttribute(4, "disabled", Disabled);
        builder.AddMultipleAttributes(5, AdditionalAttributes);

        builder.AddElementReferenceCapture(6, (value) => { Element = value; });
        if (Loading && LoadingTemplate is not null)
        { 
            builder.AddContent(7, LoadingTemplate);
        }
        else if (ChildContent != null)
        {
            builder.AddContent(8, ChildContent); 
        } 
        builder.CloseElement();
    }



    // TODO: Review
    // - Disable text wrapping: https://getbootstrap.com/docs/5.1/components/buttons/#disable-text-wrapping
}
