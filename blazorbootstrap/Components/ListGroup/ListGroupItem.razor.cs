namespace BlazorBootstrap;

/// <summary>
/// An item of a <see cref="ListGroup{TItem}" />. It renders as a list item, a link (<see cref="Href" />), a button (<see cref="OnClick" />),
/// a selectable option, or a checkbox or radio row, depending on its parameters and the group's <see cref="ListGroup{TItem}.SelectionMode" />. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/list-group/" />
/// </summary>
/// <typeparam name="TItem">The type of the item value; it comes from the parent <see cref="ListGroup{TItem}" />.</typeparam>
[AddedVersion("4.1.0")]
public partial class ListGroupItem<TItem> : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
            Parent?.RemoveItem(this);

        return base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Parent is null)
            throw new InvalidOperationException($"{nameof(ListGroupItem<TItem>)} must be placed inside a {nameof(ListGroup<TItem>)}.");

        Parent.AddItem(this);
    }

    private async Task OnCheckedChangeAsync(ChangeEventArgs e)
    {
        if (IsDisabled)
            return;

        // A radio raises change only when it becomes checked; a checkbox reports its new state.
        var isChecked = e.Value is bool checkedValue ? checkedValue : true;

        await Parent!.OnItemCheckedChangedAsync(Value, isChecked);
    }

    private async Task OnClickAsync(MouseEventArgs e)
    {
        // A disabled link has no href, so it does not navigate; its click is ignored here too.
        if (IsDisabled)
            return;

        await OnClick.InvokeAsync(e);
        await Parent!.OnItemClickedAsync(Value);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets whether the item is active, marked with <c>aria-current="true"</c>. Applies when the group is not selectable;
    /// a selectable group marks its selected items instead.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the item is active, marked with <b>aria-current=\"true\"</b>. Applies when the group is not selectable; a selectable group marks its selected items instead.")]
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the item. In checkbox and radio modes it names the input; set it when the item has no visible text.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of the item. In checkbox and radio modes it names the input; set it when the item has no visible text.")]
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the content of the item: text or any markup.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the item: text or any markup.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ListGroupItem, true),
            (BootstrapClass.ListGroupItemAction, isActionable),
            (BootstrapClass.Active, isOption ? isSelected : Active && !isCheckMode),
            (BootstrapClass.Disabled, IsDisabled && !isOption && !isButton && !isCheckMode),
            ("d-flex justify-content-between align-items-center", isOption),
            (Color.ToListGroupColorClass(), Color != ListGroupColor.None));

    /// <summary>
    /// Gets or sets the contextual color of the item.
    /// <para>
    /// Default value is <see cref="ListGroupColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(ListGroupColor.None)]
    [Description("Gets or sets the contextual color of the item.")]
    [Parameter]
    public ListGroupColor Color { get; set; } = ListGroupColor.None;

    /// <summary>
    /// Gets or sets whether the item is disabled. The item is also disabled when its group is disabled.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the item is disabled. The item is also disabled when its group is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether the item renders a link or button on its own, before the group knows about it.
    /// </summary>
    internal bool HasOwnAction => Href is not null || OnClick.HasDelegate;

    /// <summary>
    /// Gets or sets the URL of the item, which makes it a link. A disabled link has no <c>href</c> and is marked <c>aria-disabled="true"</c>.
    /// Applies when the group is not selectable.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the URL of the item, which makes it a link. A disabled link has no <b>href</b> and is marked <b>aria-disabled=\"true\"</b>. Applies when the group is not selectable.")]
    [Parameter]
    public string? Href { get; set; }

    private bool isActionable => isOption || Href is not null || isButton;

    private bool isButton => !isOption && !isCheckMode && Href is null && (OnClick.HasDelegate || Parent!.OnItemClick.HasDelegate);

    private bool isCheckMode => Parent!.SelectionMode is ListGroupSelectionMode.Checkbox or ListGroupSelectionMode.Radio;

    internal bool IsDisabled => Disabled || Parent!.Disabled;

    private bool isOption => Parent!.SelectionMode is ListGroupSelectionMode.Single or ListGroupSelectionMode.Multiple;

    private string? firstOptionTabIndex;

    /// <summary>
    /// The option's <c>tabindex</c>, fixed at its first render so Blazor never rewrites it: "0" for the tab stop before the interop runs,
    /// otherwise "-1". From then on the interop moves the tab stop.
    /// </summary>
    private string optionTabIndex => firstOptionTabIndex ??= Parent!.IsListboxInteropReady || !Parent.IsInitialTabStop(this) ? "-1" : "0";

    private bool isSelected => Parent!.IsSelected(Value);

    private string labelClassNames =>
        BuildClassNames(
            (BootstrapClass.FormCheckLabel, true),
            ("stretched-link", Parent!.StretchedLink));

    /// <summary>
    /// This event fires when the user clicks the item, which makes it a button. Applies when the group is not selectable.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user clicks the item, which makes it a button. Applies when the group is not selectable.")]
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [CascadingParameter] private ListGroup<TItem>? Parent { get; set; }

    /// <summary>
    /// Gets or sets the value of the item. Selection and <see cref="ListGroup{TItem}.OnItemClick" /> report it.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the value of the item. Selection and <b>OnItemClick</b> report it.")]
    [Parameter]
    public TItem Value { get; set; } = default!;

    #endregion
}
