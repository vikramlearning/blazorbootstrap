namespace BlazorBootstrap;

/// <summary>
/// A list of items, links, or buttons, with optional single, multiple, checkbox, or radio selection, or tabs that switch content panes. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/list-group/" />
/// </summary>
/// <typeparam name="TItem">The type of the item values. Static lists without selection still set it, for example <c>TItem="string"</c>.</typeparam>
[AddedVersion("4.1.0")]
public partial class ListGroup<TItem> : BlazorBootstrapComponentBase, IListGroupTabHost
{
    #region Fields and Constants

    private FieldIdentifier fieldIdentifier;

    private bool hasActionableItems;

    private string? initialActiveTabName;

    /// <summary>
    /// What the interop was last initialized for: the tab mode with its tab ids, or the listbox. <see langword="null" /> when not initialized.
    /// </summary>
    private string? initializedInteropKey;

    private bool isInitialActiveTabFrozen;

    private readonly List<ListGroupItem<TItem>> items = new();

    private DotNetObjectReference<ListGroup<TItem>>? objRef;

    private readonly List<ListGroupTabPane> panes = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            if (initializedInteropKey is not null)
                await SafeInvokeVoidAsync("window.blazorBootstrap.listGroup.dispose", Id);

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // A pane rendered conditionally registers last, so when the panes change, take their order from the page (markup order).
        if (TabMode && panes.Count > 1 && GetTabsInteropKey() != initializedInteropKey && await ReorderPanesFromPageAsync())
        {
            StateHasChanged(); // the interop re-wires after this render, with the tabs in order
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        // The key changes when the tab list or the listbox container is replaced or its tabs change, so the interop re-wires (CS-004).
        var interopKey = TabMode ? GetTabsInteropKey() : isListbox ? "listbox" : null;

        if (interopKey != initializedInteropKey)
        {
            initializedInteropKey = interopKey;

            if (interopKey is null)
            {
                await SafeInvokeVoidAsync("window.blazorBootstrap.listGroup.dispose", Id);
            }
            else if (TabMode)
            {
                isInitialActiveTabFrozen = true;
                objRef ??= DotNetObjectReference.Create(this);
                await SafeInvokeVoidAsync("window.blazorBootstrap.listGroup.initializeTabs", Id, objRef);
            }
            else
            {
                await SafeInvokeVoidAsync("window.blazorBootstrap.listGroup.initializeListbox", Id);
            }
        }
        else if (interopKey is not null && isListbox)
        {
            // Options render tabindex="-1"; the interop keeps exactly one of them as the tab stop.
            await SafeInvokeVoidAsync("window.blazorBootstrap.listGroup.syncListbox", Id);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (SelectedItemExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(SelectedItemExpression);
        else if (SelectedItemsExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(SelectedItemsExpression);
    }

    /// <summary>
    /// Shows the tab pane with the given name. Applies in tab mode.
    /// </summary>
    /// <param name="name">The <see cref="ListGroupTabPane.Name" /> of the pane to show.</param>
    [AddedVersion("4.1.0")]
    [Description("Shows the tab pane with the given name. Applies in tab mode.")]
    public async Task ShowTabAsync(string name)
    {
        var pane = panes.FirstOrDefault(p => p.Name == name);

        if (pane is not null && !pane.Disabled)
            await SafeInvokeVoidAsync("window.blazorBootstrap.listGroup.showTab", pane.TabId);
    }

    [JSInvokable]
    public async Task bsHiddenTab(string? activeTabName, string? previousTabName) => await OnHidden.InvokeAsync(new ListGroupTabEventArgs(activeTabName, previousTabName));

    [JSInvokable]
    public async Task bsHideTab(string? activeTabName, string? previousTabName) => await OnHiding.InvokeAsync(new ListGroupTabEventArgs(activeTabName, previousTabName));

    [JSInvokable]
    public async Task bsShowTab(string? activeTabName, string? previousTabName) => await OnShowing.InvokeAsync(new ListGroupTabEventArgs(activeTabName, previousTabName));

    [JSInvokable]
    public async Task bsShownTab(string? activeTabName, string? previousTabName) => await OnShown.InvokeAsync(new ListGroupTabEventArgs(activeTabName, previousTabName));

    internal void AddItem(ListGroupItem<TItem> item)
    {
        items.Add(item);

        if (!hasActionableItems && item.HasOwnAction)
        {
            hasActionableItems = true;
            StateHasChanged();
        }
    }

    internal bool IsSelected(TItem value) =>
        SelectionMode switch
        {
            ListGroupSelectionMode.Single or ListGroupSelectionMode.Radio => EqualityComparer<TItem>.Default.Equals(SelectedItem, value),
            ListGroupSelectionMode.Multiple or ListGroupSelectionMode.Checkbox => SelectedItems?.Contains(value) ?? false,
            _ => false
        };

    internal async Task OnItemCheckedChangedAsync(TItem value, bool isChecked)
    {
        if (Disabled)
            return;

        if (SelectionMode == ListGroupSelectionMode.Radio)
        {
            if (isChecked)
                await SetSelectedItemAsync(value);
        }
        else
        {
            await ToggleSelectedItemsAsync(value, isChecked);
        }
    }

    internal async Task OnItemClickedAsync(TItem value)
    {
        if (Disabled)
            return;

        if (SelectionMode == ListGroupSelectionMode.Single)
            await SetSelectedItemAsync(value);
        else if (SelectionMode == ListGroupSelectionMode.Multiple)
            await ToggleSelectedItemsAsync(value, !IsSelected(value));

        await OnItemClick.InvokeAsync(value);
    }

    internal void RemoveItem(ListGroupItem<TItem> item) => items.Remove(item);

    void IListGroupTabHost.AddPane(ListGroupTabPane pane)
    {
        panes.Add(pane);
        StateHasChanged();
    }

    bool IListGroupTabHost.IsInitiallyActive(ListGroupTabPane pane)
    {
        if (!isInitialActiveTabFrozen)
            initialActiveTabName = (panes.FirstOrDefault(p => p.Active && !p.Disabled) ?? panes.FirstOrDefault(p => !p.Disabled))?.Name;

        return pane.Name == initialActiveTabName;
    }

    void IListGroupTabHost.RemovePane(ListGroupTabPane pane)
    {
        if (panes.Remove(pane))
            StateHasChanged();
    }

    private string? GetTabsInteropKey() => panes.Count > 0 ? $"tabs|{string.Join(",", panes.Select(p => p.TabId))}" : null;

    private string GetTabClassNames(ListGroupTabPane pane) =>
        BuildClassNames(
            (BootstrapClass.ListGroupItem, true),
            (BootstrapClass.ListGroupItemAction, true),
            (BootstrapClass.Active, ((IListGroupTabHost)this).IsInitiallyActive(pane)));

    private async Task NotifyFieldChangedAsync()
    {
        if (fieldIdentifier.FieldName is not null)
            EditContext?.NotifyFieldChanged(fieldIdentifier);

        StateHasChanged();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Orders <see cref="panes" /> by the page order of their pane elements. Returns whether the order changed.
    /// </summary>
    private async Task<bool> ReorderPanesFromPageAsync()
    {
        string[]? paneIds;
        try
        {
            paneIds = await JSRuntime.InvokeAsync<string[]?>("window.blazorBootstrap.listGroup.getPaneOrder", Id);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException)
        {
            return false;
        }

        if (paneIds is null || paneIds.Length != panes.Count)
            return false;

        var ordered = paneIds.Select(id => panes.FirstOrDefault(p => p.Id == id)).ToList();
        if (ordered.Any(p => p is null) || ordered.SequenceEqual(panes))
            return false;

        panes.Clear();
        panes.AddRange(ordered!);

        return true;
    }

    private async Task SetSelectedItemAsync(TItem value)
    {
        if (EqualityComparer<TItem>.Default.Equals(SelectedItem, value))
            return;

        SelectedItem = value;
        await SelectedItemChanged.InvokeAsync(value);
        await NotifyFieldChangedAsync();
    }

    private async Task ToggleSelectedItemsAsync(TItem value, bool isSelected)
    {
        var selected = SelectedItems?.ToList() ?? new List<TItem>();

        if (isSelected == selected.Contains(value))
            return;

        if (isSelected)
            selected.Add(value);
        else
            selected.Remove(value);

        SelectedItems = selected;
        await SelectedItemsChanged.InvokeAsync(selected);
        await NotifyFieldChangedAsync();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible name of the list, read by screen readers. A listbox (<see cref="ListGroupSelectionMode.Single" />,
    /// <see cref="ListGroupSelectionMode.Multiple" />) and a tab list need it, or <c>aria-labelledby</c>.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the accessible name of the list, read by screen readers. A listbox (<b>Single</b>, <b>Multiple</b>) and a tab list need it, or <b>aria-labelledby</b>.")]
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the content: <see cref="ListGroupItem{TItem}" /> components in static mode, or <see cref="ListGroupTabPane" /> components in tab mode.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content: <b>ListGroupItem</b> components in static mode, or <b>ListGroupTabPane</b> components in tab mode.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ListGroup, true),
            (BootstrapClass.ListGroupFlush, Flush && !Horizontal),
            (BootstrapClass.ListGroupNumbered, Numbered),
            (HorizontalBreakpoint.ToListGroupHorizontalClass(), Horizontal),
            (fieldCssClasses, !TabMode));

    /// <summary>
    /// A listbox, or a list of links and buttons, is a <c>div</c> (Bootstrap's markup for actionable items);
    /// other lists are <c>ul</c>, or <c>ol</c> when numbered.
    /// </summary>
    private string containerTag =>
        isListbox || (SelectionMode == ListGroupSelectionMode.None && (hasActionableItems || OnItemClick.HasDelegate))
            ? "div"
            : Numbered ? "ol" : "ul";

    /// <summary>
    /// Whether plain items render as <c>div</c> because the list is a <c>div</c>.
    /// </summary>
    internal bool UsesDivItems => containerTag == "div";

    /// <summary>
    /// Gets or sets the items to render in data-bound mode. When set, <see cref="ChildContent" /> is ignored.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the items to render in data-bound mode. When set, <b>ChildContent</b> is ignored.")]
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    /// Gets or sets whether every item is disabled.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether every item is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets a function that returns whether a data-bound item is disabled.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns whether a data-bound item is disabled.")]
    [Parameter]
    public Func<TItem, bool>? DisabledSelector { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether tab panes fade in and out. Applies in tab mode.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether tab panes fade in and out. Applies in tab mode.")]
    [Parameter]
    public bool Fade { get; set; } = true;

    private string fieldCssClasses =>
        EditContext is not null && fieldIdentifier.FieldName is not null
            ? EditContext.FieldCssClass(fieldIdentifier)
            : "";

    /// <summary>
    /// Gets or sets whether the outer borders and rounded corners are removed, so the list sits edge to edge in its parent.
    /// Ignored when <see cref="Horizontal" /> is set.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the outer borders and rounded corners are removed, so the list sits edge to edge in its parent. Ignored when <b>Horizontal</b> is set.")]
    [Parameter]
    public bool Flush { get; set; }

    /// <summary>
    /// Gets or sets whether the items are laid out in a row instead of a column, from <see cref="HorizontalBreakpoint" /> up.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the items are laid out in a row instead of a column, from <b>HorizontalBreakpoint</b> up.")]
    [Parameter]
    public bool Horizontal { get; set; }

    /// <summary>
    /// Gets or sets the breakpoint from which a horizontal list lays its items out in a row.
    /// <para>
    /// Default value is <see cref="ListGroupHorizontalBreakpoint.Always" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(ListGroupHorizontalBreakpoint.Always)]
    [Description("Gets or sets the breakpoint from which a horizontal list lays its items out in a row.")]
    [Parameter]
    public ListGroupHorizontalBreakpoint HorizontalBreakpoint { get; set; } = ListGroupHorizontalBreakpoint.Always;

    private bool isListbox => !TabMode && SelectionMode is ListGroupSelectionMode.Single or ListGroupSelectionMode.Multiple;

    internal bool IsInvalid =>
        EditContext is not null
        && fieldIdentifier.FieldName is not null
        && EditContext.GetValidationMessages(fieldIdentifier).Any();

    /// <summary>
    /// Gets or sets the template of a data-bound item. Takes precedence over <see cref="TextField" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the template of a data-bound item. Takes precedence over <b>TextField</b>.")]
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether the items are numbered.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the items are numbered.")]
    [Parameter]
    public bool Numbered { get; set; }

    /// <summary>
    /// This event fires when the user clicks an item. Items become buttons when it is set.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user clicks an item. Items become buttons when it is set.")]
    [Parameter]
    public EventCallback<TItem> OnItemClick { get; set; }

    /// <summary>
    /// This event fires after a tab pane is hidden. Applies in tab mode.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires after a tab pane is hidden. Applies in tab mode.")]
    [Parameter]
    public EventCallback<ListGroupTabEventArgs> OnHidden { get; set; }

    /// <summary>
    /// This event fires when a tab pane starts to hide. Applies in tab mode.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when a tab pane starts to hide. Applies in tab mode.")]
    [Parameter]
    public EventCallback<ListGroupTabEventArgs> OnHiding { get; set; }

    /// <summary>
    /// This event fires when a tab pane starts to show. Applies in tab mode.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when a tab pane starts to show. Applies in tab mode.")]
    [Parameter]
    public EventCallback<ListGroupTabEventArgs> OnShowing { get; set; }

    /// <summary>
    /// This event fires after a tab pane is shown. Applies in tab mode.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires after a tab pane is shown. Applies in tab mode.")]
    [Parameter]
    public EventCallback<ListGroupTabEventArgs> OnShown { get; set; }

    /// <summary>
    /// Gets or sets the selected item. Applies when <see cref="SelectionMode" /> is <see cref="ListGroupSelectionMode.Single" /> or <see cref="ListGroupSelectionMode.Radio" />.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the selected item. Applies when <b>SelectionMode</b> is <b>Single</b> or <b>Radio</b>.")]
    [Parameter]
    public TItem SelectedItem { get; set; } = default!;

    /// <summary>
    /// This event fires when the user selects a different item.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user selects a different item.")]
    [Parameter]
    public EventCallback<TItem> SelectedItemChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound selected item.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound selected item for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<TItem>>? SelectedItemExpression { get; set; }

    /// <summary>
    /// Gets or sets the selected items. Applies when <see cref="SelectionMode" /> is <see cref="ListGroupSelectionMode.Multiple" /> or <see cref="ListGroupSelectionMode.Checkbox" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the selected items. Applies when <b>SelectionMode</b> is <b>Multiple</b> or <b>Checkbox</b>.")]
    [Parameter]
    public IEnumerable<TItem>? SelectedItems { get; set; }

    /// <summary>
    /// This event fires when the user selects or deselects an item. The new set keeps the existing order, with a newly selected item added at the end.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user selects or deselects an item. The new set keeps the existing order, with a newly selected item added at the end.")]
    [Parameter]
    public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound selected items.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the expression that identifies the bound selected items for validation and EditContext notifications.")]
    [Parameter]
    public Expression<Func<IEnumerable<TItem>?>>? SelectedItemsExpression { get; set; }

    /// <summary>
    /// Gets or sets how items are selected.
    /// <para>
    /// Default value is <see cref="ListGroupSelectionMode.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(ListGroupSelectionMode.None)]
    [Description("Gets or sets how items are selected.")]
    [Parameter]
    public ListGroupSelectionMode SelectionMode { get; set; } = ListGroupSelectionMode.None;

    /// <summary>
    /// Gets or sets whether the whole row of a checkbox or radio item toggles it, not only the input and its label.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the whole row of a checkbox or radio item toggles it, not only the input and its label.")]
    [Parameter]
    public bool StretchedLink { get; set; }

    /// <summary>
    /// Gets or sets the CSS class of the column that holds the tab list. Applies in tab mode.
    /// <para>
    /// Default value is "col-md-4".
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("col-md-4")]
    [Description("Gets or sets the CSS class of the column that holds the tab list. Applies in tab mode.")]
    [Parameter]
    public string TabListClass { get; set; } = "col-md-4";

    /// <summary>
    /// Gets or sets the CSS class of the column that holds the tab panes. Applies in tab mode.
    /// <para>
    /// Default value is "col-md-8".
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("col-md-8")]
    [Description("Gets or sets the CSS class of the column that holds the tab panes. Applies in tab mode.")]
    [Parameter]
    public string TabContentClass { get; set; } = "col-md-8";

    /// <summary>
    /// Gets or sets whether the items act as tabs that switch the <see cref="ListGroupTabPane" /> children.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the items act as tabs that switch the <b>ListGroupTabPane</b> children.")]
    [Parameter]
    public bool TabMode { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the text of a data-bound item, when there is no <see cref="ItemTemplate" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a function that returns the text of a data-bound item, when there is no <b>ItemTemplate</b>.")]
    [Parameter]
    public Func<TItem, string>? TextField { get; set; }

    #endregion
}
