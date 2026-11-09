namespace BlazorBootstrap;

/// <summary>
/// Shows a title and content in an overlay next to its trigger, the first element in <see cref="ChildContent" />.
/// It wraps Bootstrap's popover plugin; the title and content are rendered by Blazor and keep updating while the popover is open. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/popovers/" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class Popover : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isInitialized;

    private DotNetObjectReference<Popover>? objRef;

    private bool? previousAutoClose;

    private bool? previousDisabled;

    private string? previousOptionsKey;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            if (isInitialized)
                await SafeInvokeVoidAsync("window.blazorBootstrap.popover.dispose", Id);

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var optionsKey = GetOptionsKey();

        if (!isInitialized || optionsKey != previousOptionsKey)
        {
            // First render, or an option Bootstrap reads only at creation changed: (re)create the popover.
            objRef ??= DotNetObjectReference.Create(this);
            await SafeInvokeVoidAsync("window.blazorBootstrap.popover.initialize", Id, GetOptions(), objRef);

            isInitialized = true;
            previousOptionsKey = optionsKey;
            previousDisabled = Disabled;
            previousAutoClose = AutoClose;
        }
        else
        {
            if (previousDisabled != Disabled)
            {
                previousDisabled = Disabled;
                await SafeInvokeVoidAsync("window.blazorBootstrap.popover.setEnabled", Id, !Disabled);
            }

            if (previousAutoClose != AutoClose)
            {
                previousAutoClose = AutoClose;
                await SafeInvokeVoidAsync("window.blazorBootstrap.popover.setAutoClose", Id, AutoClose);
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public async Task bsHidden() => await OnHidden.InvokeAsync();

    [JSInvokable]
    public async Task bsHide() => await OnHiding.InvokeAsync();

    [JSInvokable]
    public async Task bsInserted() => await OnInserted.InvokeAsync();

    [JSInvokable]
    public async Task bsShow() => await OnShowing.InvokeAsync();

    [JSInvokable]
    public async Task bsShown() => await OnShown.InvokeAsync();

    /// <summary>
    /// Hides the popover.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Hides the popover.")]
    public async Task HideAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.popover.hide", Id);

    /// <summary>
    /// Shows the popover. Does nothing when it has no title and no content, or when it is disabled.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the popover. Does nothing when it has no title and no content, or when it is disabled.")]
    public async Task ShowAsync()
    {
        if (hasTitle || hasContent)
            await SafeInvokeVoidAsync("window.blazorBootstrap.popover.show", Id);
    }

    /// <summary>
    /// Shows the popover if it is hidden, or hides it if it is shown.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the popover if it is hidden, or hides it if it is shown.")]
    public async Task ToggleAsync()
    {
        if (hasTitle || hasContent)
            await SafeInvokeVoidAsync("window.blazorBootstrap.popover.toggle", Id);
    }

    /// <summary>
    /// Updates the position of an open popover, for example after its trigger moved. Content size changes reposition it automatically.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Updates the position of an open popover, for example after its trigger moved. Content size changes reposition it automatically.")]
    public async Task UpdateAsync() => await SafeInvokeVoidAsync("window.blazorBootstrap.popover.update", Id);

    private object GetOptions() =>
        new
        {
            hasTitle,
            hasContent,
            placement = Placement.ToString().ToLowerInvariant(),
            fallbackPlacements = FallbackPlacements?.Where(p => p != PopoverPlacement.Auto).Select(p => p.ToString().ToLowerInvariant()).ToArray(),
            trigger = triggerString,
            showDelay = ShowDelay,
            hideDelay = HideDelay,
            offset = Offset,
            container = Container,
            customClass = customClassNames,
            animation = Animation,
            autoClose = AutoClose,
            enabled = !Disabled
        };

    /// <summary>
    /// The options Bootstrap reads only when the popover is created. A change re-creates it; <see cref="Disabled" /> and <see cref="AutoClose" /> update in place.
    /// </summary>
    private string GetOptionsKey() =>
        string.Join("|",
            hasTitle,
            hasContent,
            Placement,
            FallbackPlacements is null ? "" : string.Join(",", FallbackPlacements),
            triggerString,
            ShowDelay,
            HideDelay,
            Offset,
            Container,
            customClassNames,
            Animation);

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets whether the popover fades in and out. The fade also respects the user's reduced-motion setting.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the popover fades in and out. The fade also respects the user's reduced-motion setting.")]
    [Parameter]
    public bool Animation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the popover closes on a click outside it and its trigger, on Escape, and when focus leaves its content.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the popover closes on a click outside it and its trigger, on Escape, and when focus leaves its content.")]
    [Parameter]
    public bool AutoClose { get; set; } = true;

    /// <summary>
    /// Gets or sets the trigger of the popover. Its first element gets the popover; use a focusable, interactive element such as a button.
    /// When that element is disabled, the wrapper around it gets the popover and becomes focusable instead.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the trigger of the popover. Its first element gets the popover; use a focusable, interactive element such as a button. When that element is disabled, the wrapper around it gets the popover and becomes focusable instead.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("d-inline-block", true));

    /// <summary>
    /// Gets or sets the color of the popover header and border.
    /// <para>
    /// Default value is <see cref="PopoverColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(PopoverColor.None)]
    [Description("Gets or sets the color of the popover header and border.")]
    [Parameter]
    public PopoverColor Color { get; set; } = PopoverColor.None;

    /// <summary>
    /// Gets or sets the CSS selector of the element the popover is added to, for example a modal's <c>.modal-body</c> so its content can take focus inside the modal.
    /// When <see langword="null" />, the popover is added to the page body.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS selector of the element the popover is added to, for example a modal's <b>.modal-body</b> so its content can take focus inside the modal. When <b>null</b>, the popover is added to the page body.")]
    [Parameter]
    public string? Container { get; set; }

    /// <summary>
    /// Gets or sets the content text, always shown as text. <see cref="ContentTemplate" /> takes precedence.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content text, always shown as text. <b>ContentTemplate</b> takes precedence.")]
    [Parameter]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the content markup, such as formatted text, a table, or buttons. It updates in place while the popover is open.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content markup, such as formatted text, a table, or buttons. It updates in place while the popover is open.")]
    [Parameter]
    public RenderFragment? ContentTemplate { get; set; }

    /// <summary>
    /// Gets or sets a CSS class added to the popover, for styling it through Bootstrap's popover CSS variables.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a CSS class added to the popover, for styling it through Bootstrap's popover CSS variables.")]
    [Parameter]
    public string? CustomClass { get; set; }

    private string customClassNames =>
        BuildClassNames(
            (Color.ToPopoverColorClass(), Color != PopoverColor.None),
            (CustomClass, !string.IsNullOrWhiteSpace(CustomClass)));

    /// <summary>
    /// Gets or sets whether the popover is disabled. A disabled popover does not open until it is enabled again, and closes if it is open.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the popover is disabled. A disabled popover does not open until it is enabled again, and closes if it is open.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the placements to try, in order, when <see cref="Placement" /> has no room. When <see langword="null" />, Bootstrap tries top, right, bottom, and left.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the placements to try, in order, when <b>Placement</b> has no room. When <b>null</b>, Bootstrap tries top, right, bottom, and left.")]
    [Parameter]
    public IEnumerable<PopoverPlacement>? FallbackPlacements { get; set; }

    private bool hasContent => ContentTemplate is not null || !string.IsNullOrEmpty(Content);

    private bool hasTitle => TitleTemplate is not null || !string.IsNullOrEmpty(Title);

    /// <summary>
    /// Gets or sets the delay before hiding, in milliseconds. Applies to the Hover and Focus triggers.
    /// <para>
    /// Default value is 0.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the delay before hiding, in milliseconds. Applies to the Hover and Focus triggers.")]
    [Parameter]
    public int HideDelay { get; set; }

    private string homeId => $"{Id}-home";

    /// <summary>
    /// Gets or sets the offset of the popover from its trigger, as "skidding,distance" in pixels.
    /// When <see langword="null" />, Bootstrap's "0,8" is used.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the offset of the popover from its trigger, as \"skidding,distance\" in pixels. When <b>null</b>, Bootstrap's \"0,8\" is used.")]
    [Parameter]
    public string? Offset { get; set; }

    /// <summary>
    /// This event fires after the popover is hidden.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires after the popover is hidden.")]
    [Parameter]
    public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event fires when the popover starts to hide.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the popover starts to hide.")]
    [Parameter]
    public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires after the popover is added to the page, before it is shown.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires after the popover is added to the page, before it is shown.")]
    [Parameter]
    public EventCallback OnInserted { get; set; }

    /// <summary>
    /// This event fires when the popover starts to show.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the popover starts to show.")]
    [Parameter]
    public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event fires after the popover is shown.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires after the popover is shown.")]
    [Parameter]
    public EventCallback OnShown { get; set; }

    /// <summary>
    /// Gets or sets where the popover opens relative to its trigger. Right and Left are mirrored in right-to-left layouts.
    /// <para>
    /// Default value is <see cref="PopoverPlacement.Right" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(PopoverPlacement.Right)]
    [Description("Gets or sets where the popover opens relative to its trigger. Right and Left are mirrored in right-to-left layouts.")]
    [Parameter]
    public PopoverPlacement Placement { get; set; } = PopoverPlacement.Right;

    /// <summary>
    /// Gets or sets the delay before showing, in milliseconds. Applies to the Hover and Focus triggers.
    /// <para>
    /// Default value is 0.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the delay before showing, in milliseconds. Applies to the Hover and Focus triggers.")]
    [Parameter]
    public int ShowDelay { get; set; }

    /// <summary>
    /// Gets or sets the title text, always shown as text. <see cref="TitleTemplate" /> takes precedence.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title text, always shown as text. <b>TitleTemplate</b> takes precedence.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the title markup. It updates in place while the popover is open.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title markup. It updates in place while the popover is open.")]
    [Parameter]
    public RenderFragment? TitleTemplate { get; set; }

    /// <summary>
    /// Gets or sets how the popover opens. Click, Hover, and Focus can be combined, for example <c>PopoverTrigger.Hover | PopoverTrigger.Focus</c>;
    /// Manual cannot, and wins when combined.
    /// <para>
    /// Default value is <see cref="PopoverTrigger.Click" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(PopoverTrigger.Click)]
    [Description("Gets or sets how the popover opens. Click, Hover, and Focus can be combined, for example <b>PopoverTrigger.Hover | PopoverTrigger.Focus</b>; Manual cannot, and wins when combined.")]
    [Parameter]
    public PopoverTrigger Trigger { get; set; } = PopoverTrigger.Click;

    private string triggerString
    {
        get
        {
            if (Trigger.HasFlag(PopoverTrigger.Manual))
                return "manual";

            var triggers = new List<string>();

            if (Trigger.HasFlag(PopoverTrigger.Click))
                triggers.Add("click");

            if (Trigger.HasFlag(PopoverTrigger.Hover))
                triggers.Add("hover");

            if (Trigger.HasFlag(PopoverTrigger.Focus))
                triggers.Add("focus");

            return triggers.Count == 0 ? "click" : string.Join(" ", triggers);
        }
    }

    #endregion
}
