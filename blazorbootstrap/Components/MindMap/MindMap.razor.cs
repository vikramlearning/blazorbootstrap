namespace BlazorBootstrap;

/// <summary>
/// Displays one root item and its descendants as a collapsible node-link tree, or mind map.
/// </summary>
/// <typeparam name="TItem">The type of the items in the mind map.</typeparam>
public partial class MindMap<TItem> : BlazorBootstrapComponentBase where TItem : notnull
{
    #region Fields and Constants

    private const int SearchDebounceMilliseconds = 250;
    private const int ZoomDefault = 100;
    private const int ZoomMaximum = 150;
    private const int ZoomMinimum = 50;
    private const int ZoomStep = 10;

    private static readonly MindMapColor[] automaticPalette =
    {
        MindMapColor.Blue, MindMapColor.Purple, MindMapColor.Teal, MindMapColor.Orange, MindMapColor.Pink,
        MindMapColor.Green, MindMapColor.Indigo, MindMapColor.Cyan, MindMapColor.Red, MindMapColor.Yellow
    };

    private readonly IEqualityComparer<TItem> comparer = EqualityComparer<TItem>.Default;
    private HashSet<TItem> expanded = new(EqualityComparer<TItem>.Default);
    private Node? focusedNode;

    // One handler set per tree path, kept across renders and index rebuilds so Blazor does not re-register
    // every node's event handlers on each render (AC-25).
    private readonly Dictionary<string, NodeHandlers> handlersByPath = new();
    private bool hasSeededExpansion;
    private bool isInteractive;
    private bool isJsInitialized;
    private bool? lastAllowPan;
    private MindMapDirection? lastDirection;
    private IReadOnlyCollection<TItem>? lastExpandedItemsParameter;
    private TItem? lastRoot;
    private TItem? lastSelectedItemParameter;
    private int matchCount;
    private Dictionary<TItem, Node> nodesByItem = new(EqualityComparer<TItem>.Default);
    private Dictionary<string, Node> nodesByPath = new();
    private string? pendingFocusId;
    private bool pendingRootScroll;
    private Node? rootNode;
    private CancellationTokenSource? searchDebounce;
    private string searchInput = string.Empty;
    private string? searchQuery;
    private TItem? selectedItem;
    private int zoom = ZoomDefault;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            searchDebounce?.Cancel();
            searchDebounce?.Dispose();
            searchDebounce = null;

            if (isJsInitialized)
                await SafeInvokeVoidAsync("window.blazorBootstrap.mindMap.dispose", Id);
        }

        await base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            isInteractive = true;
            pendingRootScroll = true;
            await SafeInvokeVoidAsync("window.blazorBootstrap.mindMap.initialize", Id, AllowPan);
            isJsInitialized = true;
            lastAllowPan = AllowPan;
            StateHasChanged();
        }
        else
        {
            if (lastAllowPan != AllowPan)
            {
                lastAllowPan = AllowPan;
                await SafeInvokeVoidAsync("window.blazorBootstrap.mindMap.update", Id, AllowPan);
            }

            if (pendingRootScroll)
            {
                pendingRootScroll = false;
                await SafeInvokeVoidAsync("window.blazorBootstrap.mindMap.scrollRootIntoView", Id, Direction.ToString());
            }

            if (pendingFocusId is not null)
            {
                var id = pendingFocusId;
                pendingFocusId = null;
                await SafeInvokeVoidAsync("window.blazorBootstrap.focusInputElement", id);
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (lastDirection != Direction)
        {
            if (lastDirection is not null)
                pendingRootScroll = true;

            lastDirection = Direction;
        }

        var rootChanged = !hasSeededExpansion || !EqualsItem(lastRoot, Root);
        lastRoot = Root;

        BuildIndex();

        if (ExpandedItems is not null)
        {
            if (!ReferenceEquals(lastExpandedItemsParameter, ExpandedItems))
                expanded = new HashSet<TItem>(ExpandedItems, comparer);

            lastExpandedItemsParameter = ExpandedItems;
            hasSeededExpansion = true;
        }
        else if (rootChanged)
        {
            SeedExpansion();
            hasSeededExpansion = true;
        }

        // Sync from the parameter only when it changes, so an unbound selection survives parent re-renders.
        if (!EqualsItem(lastSelectedItemParameter, SelectedItem))
        {
            selectedItem = SelectedItem;
            lastSelectedItemParameter = SelectedItem;
        }

        // Items removed from the data leave the expanded set and the selection (AC-11).
        var removed = expanded.Where(item => !nodesByItem.ContainsKey(item)).ToList();
        if (removed.Count > 0)
        {
            expanded.ExceptWith(removed);
            await NotifyExpandedChangedAsync(Array.Empty<TItem>(), removed);
        }

        if (selectedItem is not null && !nodesByItem.ContainsKey(selectedItem))
        {
            selectedItem = default;
            lastSelectedItemParameter = default;
            await SelectedItemChanged.InvokeAsync(selectedItem);
        }

        if (focusedNode is not null && nodesByItem.TryGetValue(focusedNode.Item, out var refreshedFocus))
            focusedNode = refreshedFocus;
        else
            focusedNode = rootNode;

        if (!string.IsNullOrEmpty(searchQuery))
            ApplyMatches(searchQuery);

        await base.OnParametersSetAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Closes every node except the root.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Closes every node except the root.")]
    public async Task CollapseAllAsync()
    {
        var toClose = expanded.Where(item => rootNode is null || !comparer.Equals(item, rootNode.Item)).ToList();
        var toOpen = rootNode is not null && rootNode.HasChildren && !expanded.Contains(rootNode.Item)
                         ? new List<TItem> { rootNode.Item }
                         : new List<TItem>();

        await ChangeExpandedAsync(toOpen, toClose);
        StateHasChanged();
    }

    /// <summary>
    /// Opens every node that has children.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Opens every node that has children.")]
    public async Task ExpandAllAsync()
    {
        var toOpen = nodesByItem.Values.Where(node => node.HasChildren && !expanded.Contains(node.Item)).Select(node => node.Item).ToList();

        await ChangeExpandedAsync(toOpen, Array.Empty<TItem>());
        StateHasChanged();
    }

    #endregion

    #region Private Methods

    private void ApplyMatches(string query)
    {
        matchCount = 0;

        foreach (var node in nodesByItem.Values)
        {
            node.IsMatch = node.Label.Contains(query, StringComparison.OrdinalIgnoreCase);
            if (node.IsMatch)
                matchCount++;
        }
    }

    private async Task ApplySearchAsync(string text)
    {
        searchQuery = string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        if (searchQuery is null)
        {
            matchCount = 0;
            foreach (var node in nodesByItem.Values)
                node.IsMatch = false;

            return;
        }

        ApplyMatches(searchQuery);

        var toOpen = new HashSet<TItem>(comparer);
        foreach (var match in nodesByItem.Values.Where(node => node.IsMatch))
            for (var ancestor = match.Parent; ancestor is not null; ancestor = ancestor.Parent)
                if (!expanded.Contains(ancestor.Item))
                    toOpen.Add(ancestor.Item);

        // Runs inside the find box's input handler, which renders once it completes.
        await ChangeExpandedAsync(toOpen, Array.Empty<TItem>());
    }

    private void BuildIndex()
    {
        nodesByItem = new Dictionary<TItem, Node>(comparer);
        nodesByPath = new Dictionary<string, Node>();
        rootNode = Root is null ? null : CreateNode(Root, null, 0, "0", null, 0, new HashSet<TItem>(comparer));

        if (rootNode is not null)
            SplitRootSides(rootNode);
    }

    private async Task ChangeExpandedAsync(IEnumerable<TItem> toOpen, IEnumerable<TItem> toClose)
    {
        var opened = toOpen.Where(item => expanded.Add(item)).ToList();
        var closed = toClose.Where(item => expanded.Remove(item)).ToList();

        if (opened.Count == 0 && closed.Count == 0)
            return;

        await NotifyExpandedChangedAsync(opened, closed);
    }

    private Node CreateNode(TItem item, Node? parent, int depth, string path, MindMapColor? inheritedPinnedColor, int siblingIndex, HashSet<TItem> ancestors)
    {
        var label = GetLabel(item);

        if (!ancestors.Add(item))
            throw new InvalidOperationException($"{nameof(MindMap<TItem>)}: the item '{label}' appears among its own ancestors. A mind map needs tree-shaped data without cycles.");

        var pinnedColor = BranchColor?.Invoke(item) ?? inheritedPinnedColor;
        var node = new Node(item, parent, depth, path, label)
                   {
                       Color = pinnedColor ?? (depth == 0
                                                   ? MindMapColor.None
                                                   : depth == 1
                                                       ? AutoBranchColors ? automaticPalette[siblingIndex % automaticPalette.Length] : MindMapColor.None
                                                       : parent!.Color)
                   };

        nodesByItem.TryAdd(item, node);
        nodesByPath[path] = node;

        var children = Children?.Invoke(item);
        if (children is not null)
        {
            var siblingKeys = new HashSet<TItem>(comparer);
            var index = 0;

            foreach (var child in children)
            {
                var childNode = CreateNode(child, node, depth + 1, $"{path}-{index}", pinnedColor, index, ancestors);
                childNode.Key = siblingKeys.Add(child) ? child : childNode.Path;
                node.Children.Add(childNode);
                index++;
            }
        }

        ancestors.Remove(item);

        return node;
    }

    private bool EqualsItem(TItem? left, TItem? right) =>
        left is null ? right is null : right is not null && comparer.Equals(left, right);

    private string GetCardClass(Node node) =>
        BuildClassNames(
            ("bb-mind-map-card", true),
            ("bb-mind-map-card-root", node.Depth == 0),
            ("bb-mind-map-card-first", node.Depth == 1),
            ("bb-mind-map-card-leaf", node.Depth >= 3 && !node.HasChildren),
            ("bb-mind-map-card-selected", isInteractive && IsSelected(node)),
            ("bb-mind-map-card-match", node.IsMatch && searchQuery is not null));

    private string GetCardId(Node node) => $"{GetNodeId(node)}-c";

    private static string GetGroupClass(bool mirror, bool stack) =>
        BuildClassNames(
            ("bb-mind-map-children", true),
            ("bb-mind-map-mirror", mirror),
            ("bb-mind-map-stack", stack));

    private NodeHandlers GetHandlers(Node node)
    {
        if (handlersByPath.TryGetValue(node.Path, out var handlers))
            return handlers;

        // Handlers resolve the node by path when they run, so they stay valid after the index is rebuilt.
        var path = node.Path;
        handlers = new NodeHandlers(
            () => nodesByPath.TryGetValue(path, out var current) ? OnCardClickAsync(current) : Task.CompletedTask,
            args => nodesByPath.TryGetValue(path, out var current) ? OnNodeKeyDownAsync(args, current) : Task.CompletedTask,
            () => nodesByPath.TryGetValue(path, out var current) ? ToggleAsync(current) : Task.CompletedTask);
        handlersByPath[path] = handlers;

        return handlers;
    }

    private string GetLabel(TItem item) => Label?.Invoke(item) ?? item.ToString() ?? string.Empty;

    private string GetNodeClass(Node node) =>
        BuildClassNames(
            ("bb-mind-map-node", true),
            ($"bb-mind-map-color-{node.Color.ToString().ToLowerInvariant()}", node.Depth > 0),
            ("bb-mind-map-both-root", node.Depth == 0 && Direction == MindMapDirection.BothSides && node.Children.Count > 1));

    private string GetNodeId(Node node) => $"{Id}-n-{node.Path}";

    private Dictionary<string, object>? GetRootAttributes()
    {
        if (AdditionalAttributes is null)
            return null;

        return AdditionalAttributes.Where(attribute => attribute.Key is not ("aria-label" or "aria-labelledby"))
                                   .ToDictionary(attribute => attribute.Key, attribute => attribute.Value);
    }

    private List<Node> GetSelectedPath()
    {
        var path = new List<Node>();

        if (selectedItem is null || !nodesByItem.TryGetValue(selectedItem, out var node))
            return path;

        for (var current = node; current is not null; current = current.Parent)
            path.Insert(0, current);

        return path;
    }

    private string? GetTabIndex(Node node, Node? tabStop) =>
        isInteractive ? IsTabStop(node, tabStop) ? "0" : "-1" : null;

    private object? GetTreeAttribute(string name) =>
        AdditionalAttributes is not null && AdditionalAttributes.TryGetValue(name, out var value) ? value : null;

    private List<Node> GetVisibleNodes()
    {
        var visible = new List<Node>();

        if (rootNode is not null)
            AddVisible(rootNode, visible);

        return visible;

        void AddVisible(Node node, List<Node> list)
        {
            list.Add(node);

            // BothSides renders the right group first in the DOM, so document order is root, right side, left side.
            if (node.HasChildren && IsExpanded(node))
                foreach (var child in node.Children)
                    AddVisible(child, list);
        }
    }

    private bool IsExpanded(Node node) => expanded.Contains(node.Item);

    private bool IsSelected(Node node) => selectedItem is not null && comparer.Equals(selectedItem, node.Item);

    private bool IsStacked(Node node) =>
        StackLeaves
        && Direction is MindMapDirection.Down or MindMapDirection.Up
        && node.Children.Count > 1
        && node.Children.TrueForAll(child => !child.HasChildren);

    private bool IsTabStop(Node node, Node? tabStop) => tabStop is not null && ReferenceEquals(node, tabStop);

    private bool IsVisible(Node node)
    {
        for (var ancestor = node.Parent; ancestor is not null; ancestor = ancestor.Parent)
            if (!IsExpanded(ancestor))
                return false;

        return true;
    }

    private void MoveFocus(Node node)
    {
        focusedNode = node;
        pendingFocusId = GetNodeId(node);
    }

    private async Task NotifyExpandedChangedAsync(IReadOnlyList<TItem> opened, IReadOnlyList<TItem> closed)
    {
        await ExpandedItemsChanged.InvokeAsync(expanded.ToList());
        await OnExpandedChanged.InvokeAsync(new MindMapExpandedChangedEventArgs<TItem>(opened, closed));
    }

    private async Task OnCardClickAsync(Node node)
    {
        focusedNode = node;
        await SelectAsync(node);
    }

    private async Task OnNodeKeyDownAsync(KeyboardEventArgs args, Node node)
    {
        var visible = GetVisibleNodes();
        var index = visible.IndexOf(node);

        switch (args.Key)
        {
            case "ArrowDown":
                if (index >= 0 && index < visible.Count - 1)
                    MoveFocus(visible[index + 1]);
                break;
            case "ArrowUp":
                if (index > 0)
                    MoveFocus(visible[index - 1]);
                break;
            case "Home":
                if (visible.Count > 0)
                    MoveFocus(visible[0]);
                break;
            case "End":
                if (visible.Count > 0)
                    MoveFocus(visible[^1]);
                break;
            case "ArrowRight":
                if (node.HasChildren && !IsExpanded(node))
                    await ToggleAsync(node);
                else if (node.HasChildren)
                    MoveFocus(node.Children[0]);
                break;
            case "ArrowLeft":
                if (node.HasChildren && IsExpanded(node))
                    await ToggleAsync(node);
                else if (node.Parent is not null)
                    MoveFocus(node.Parent);
                break;
            case "Enter":
            case " ":
                focusedNode = node;
                await SelectAsync(node);
                break;
        }
    }

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        searchInput = args.Value?.ToString() ?? string.Empty;

        searchDebounce?.Cancel();
        searchDebounce?.Dispose();
        searchDebounce = new CancellationTokenSource();
        var token = searchDebounce.Token;

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        await ApplySearchAsync(searchInput);
    }

    private void OnZoomIn() => zoom = Math.Min(ZoomMaximum, zoom + ZoomStep);

    private void OnZoomOut() => zoom = Math.Max(ZoomMinimum, zoom - ZoomStep);

    private void OnZoomReset() => zoom = ZoomDefault;

    private void SeedExpansion()
    {
        expanded = new HashSet<TItem>(nodesByItem.Values.Where(node => node.HasChildren && node.Depth < InitialExpandDepth).Select(node => node.Item), comparer);
    }

    private async Task SelectAsync(Node node)
    {
        if (IsSelected(node))
            return;

        selectedItem = node.Item;
        await SelectedItemChanged.InvokeAsync(selectedItem);
    }

    private static void SplitRootSides(Node root)
    {
        // Balance the two sides of a BothSides map by node count; the first part goes right.
        var total = root.Children.Sum(child => child.Size);
        var best = root.Children.Count;
        var bestDifference = int.MaxValue;
        var accumulated = 0;

        for (var split = 1; split < root.Children.Count; split++)
        {
            accumulated += root.Children[split - 1].Size;
            var difference = Math.Abs(total - 2 * accumulated);

            if (difference < bestDifference)
            {
                bestDifference = difference;
                best = split;
            }
        }

        root.RightCount = best;
    }

    private async Task ToggleAsync(Node node)
    {
        if (!node.HasChildren)
            return;

        focusedNode = node;

        if (IsExpanded(node))
            await ChangeExpandedAsync(Array.Empty<TItem>(), new[] { node.Item });
        else
            await ChangeExpandedAsync(new[] { node.Item }, Array.Empty<TItem>());
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets whether the map's background can be dragged with a mouse or pen to scroll the map.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the map's background can be dragged with a mouse or pen to scroll the map.")]
    [Parameter]
    public bool AllowPan { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the built-in find box is shown.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the built-in find box is shown.")]
    [Parameter]
    public bool AllowSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the zoom controls are shown.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the zoom controls are shown.")]
    [Parameter]
    public bool AllowZoom { get; set; } = true;

    /// <summary>
    /// Gets or sets whether each first-level branch takes the next Bootstrap palette color automatically.
    /// When <see langword="false" />, branches without a <see cref="BranchColor" /> use neutral lines.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether each first-level branch takes the next Bootstrap palette color automatically.")]
    [Parameter]
    public bool AutoBranchColors { get; set; } = true;

    /// <summary>
    /// Gets or sets a selector that pins a branch color for a node and its descendants.
    /// Return <see langword="null" /> to keep the inherited or automatic color.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a selector that pins a branch color for a node and its descendants. Return <b>null</b> to keep the inherited or automatic color.")]
    [ParameterTypeName("Func<TItem, MindMapColor?>?")]
    [Parameter]
    public Func<TItem, MindMapColor?>? BranchColor { get; set; }

    /// <summary>
    /// Gets or sets the selector that returns a node's children. A <see langword="null" /> or empty result makes the node a leaf.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the selector that returns a node's children. A <b>null</b> or empty result makes the node a leaf.")]
    [ParameterTypeName("Func<TItem, IEnumerable<TItem>?>?")]
    [Parameter]
    public Func<TItem, IEnumerable<TItem>?>? Children { get; set; }

    /// <inheritdoc />
    protected override string? ClassNames => BuildClassNames(Class, ("bb-mind-map", true));

    /// <summary>
    /// Gets or sets the direction in which branches grow from the root.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="MindMapDirection.Right" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(MindMapDirection.Right)]
    [Description("Gets or sets the direction in which branches grow from the root.")]
    [Parameter]
    public MindMapDirection Direction { get; set; } = MindMapDirection.Right;

    /// <summary>
    /// Gets or sets the open nodes. Use with <c>@bind-ExpandedItems</c> to read, set, or restore them.
    /// When <see langword="null" />, <see cref="InitialExpandDepth" /> decides which nodes start open.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the open nodes. Use with <b>@bind-ExpandedItems</b> to read, set, or restore them. When <b>null</b>, <b>InitialExpandDepth</b> decides which nodes start open.")]
    [ParameterTypeName("IReadOnlyCollection<TItem>?")]
    [Parameter]
    public IReadOnlyCollection<TItem>? ExpandedItems { get; set; }

    /// <summary>
    /// This event fires when the set of open nodes changes.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the set of open nodes changes.")]
    [Parameter]
    public EventCallback<IReadOnlyCollection<TItem>> ExpandedItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets how deep nodes start open when <see cref="ExpandedItems" /> is not set. The root is depth 0, so 1 opens only the root.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(1)]
    [Description("Gets or sets how deep nodes start open when <b>ExpandedItems</b> is not set. The root is depth 0, so 1 opens only the root.")]
    [Parameter]
    public int InitialExpandDepth { get; set; } = 1;

    /// <summary>
    /// Gets or sets the selector that returns a node's label. When <see langword="null" />, <see cref="object.ToString" /> is used.
    /// Search matches this text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the selector that returns a node's label. When <b>null</b>, <b>ToString()</b> is used. Search matches this text.")]
    [ParameterTypeName("Func<TItem, string>?")]
    [Parameter]
    public Func<TItem, string>? Label { get; set; }

    /// <summary>
    /// Gets or sets the template that fills each node card. Without it, the card shows the label.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the template that fills each node card. Without it, the card shows the label.")]
    [ParameterTypeName("RenderFragment<MindMapNodeContext<TItem>>?")]
    [Parameter]
    public RenderFragment<MindMapNodeContext<TItem>>? NodeTemplate { get; set; }

    /// <summary>
    /// This event fires when nodes open or close, listing every node that opened and closed in the change.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when nodes open or close, listing every node that opened and closed in the change.")]
    [Parameter]
    public EventCallback<MindMapExpandedChangedEventArgs<TItem>> OnExpandedChanged { get; set; }

    /// <summary>
    /// Gets or sets the accessible label of the path bar.
    /// </summary>
    /// <remarks>
    /// Default value is "Selected node path".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("Selected node path")]
    [Description("Gets or sets the accessible label of the path bar.")]
    [Parameter]
    public string PathLabel { get; set; } = "Selected node path";

    /// <summary>
    /// Gets or sets the root item of the map.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the root item of the map.")]
    [ParameterTypeName("TItem?")]
    [Parameter]
    public TItem? Root { get; set; }

    /// <summary>
    /// Gets or sets the accessible label of the find box.
    /// </summary>
    /// <remarks>
    /// Default value is "Find a node".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("Find a node")]
    [Description("Gets or sets the accessible label of the find box.")]
    [Parameter]
    public string SearchLabel { get; set; } = "Find a node";

    /// <summary>
    /// Gets or sets the match count text format. <c>{0}</c> is replaced by the number of matches.
    /// </summary>
    /// <remarks>
    /// Default value is "{0} matches".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("{0} matches")]
    [Description("Gets or sets the match count text format. <b>{0}</b> is replaced by the number of matches.")]
    [Parameter]
    public string SearchMatchesTextFormat { get; set; } = "{0} matches";

    /// <summary>
    /// Gets or sets the text shown when a search has no matches.
    /// </summary>
    /// <remarks>
    /// Default value is "No matches".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("No matches")]
    [Description("Gets or sets the text shown when a search has no matches.")]
    [Parameter]
    public string SearchNoMatchesText { get; set; } = "No matches";

    /// <summary>
    /// Gets or sets the text shown when a search has exactly one match.
    /// </summary>
    /// <remarks>
    /// Default value is "1 match".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("1 match")]
    [Description("Gets or sets the text shown when a search has exactly one match.")]
    [Parameter]
    public string SearchOneMatchText { get; set; } = "1 match";

    /// <summary>
    /// Gets or sets the placeholder of the find box.
    /// </summary>
    /// <remarks>
    /// Default value is "Find a node…".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("Find a node…")]
    [Description("Gets or sets the placeholder of the find box.")]
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Find a node…";

    private string SearchStatusText =>
        searchQuery is null
            ? string.Empty
            : matchCount switch
              {
                  0 => SearchNoMatchesText,
                  1 => SearchOneMatchText,
                  _ => string.Format(CultureInfo.CurrentCulture, SearchMatchesTextFormat, matchCount)
              };

    /// <summary>
    /// Gets or sets the selected node's item. Use with <c>@bind-SelectedItem</c>.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the selected node's item. Use with <b>@bind-SelectedItem</b>.")]
    [ParameterTypeName("TItem?")]
    [Parameter]
    public TItem? SelectedItem { get; set; }

    /// <summary>
    /// This event fires when the selected node changes.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the selected node changes.")]
    [Parameter]
    public EventCallback<TItem?> SelectedItemChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the path bar shows the labels from the root to the selected node.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the path bar shows the labels from the root to the selected node.")]
    [Parameter]
    public bool ShowPath { get; set; } = true;

    /// <summary>
    /// Gets or sets whether, in the <see cref="MindMapDirection.Down" /> and <see cref="MindMapDirection.Up" /> directions,
    /// a node whose two or more children are all leaves lists them in a column instead of side by side.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether, in the <b>Down</b> and <b>Up</b> directions, a node whose two or more children are all leaves lists them in a column instead of side by side.")]
    [Parameter]
    public bool StackLeaves { get; set; }

    private string TreeClassNames =>
        BuildClassNames(
            ("bb-mind-map-tree", true),
            ("bb-mind-map-right", Direction == MindMapDirection.Right),
            ("bb-mind-map-left", Direction == MindMapDirection.Left),
            ("bb-mind-map-both", Direction == MindMapDirection.BothSides),
            ("bb-mind-map-down", Direction == MindMapDirection.Down),
            ("bb-mind-map-up", Direction == MindMapDirection.Up),
            ("bb-mind-map-h", Direction is MindMapDirection.Right or MindMapDirection.Left or MindMapDirection.BothSides),
            ("bb-mind-map-v", Direction is MindMapDirection.Down or MindMapDirection.Up));

    private string? TreeStyle => zoom == ZoomDefault ? null : $"zoom: {(zoom / 100d).ToString(CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Gets or sets the accessible label of the zoom-in button.
    /// </summary>
    /// <remarks>
    /// Default value is "Zoom in".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("Zoom in")]
    [Description("Gets or sets the accessible label of the zoom-in button.")]
    [Parameter]
    public string ZoomInText { get; set; } = "Zoom in";

    /// <summary>
    /// Gets or sets the accessible label of the zoom-out button.
    /// </summary>
    /// <remarks>
    /// Default value is "Zoom out".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("Zoom out")]
    [Description("Gets or sets the accessible label of the zoom-out button.")]
    [Parameter]
    public string ZoomOutText { get; set; } = "Zoom out";

    /// <summary>
    /// Gets or sets the text of the reset-zoom button.
    /// </summary>
    /// <remarks>
    /// Default value is "Reset zoom".
    /// </remarks>
    [AddedVersion("4.1.0")]
    [DefaultValue("Reset zoom")]
    [Description("Gets or sets the text of the reset-zoom button.")]
    [Parameter]
    public string ZoomResetText { get; set; } = "Reset zoom";

    #endregion

    #region Nested Types

    private sealed class Node
    {
        #region Constructors and Finalizers

        public Node(TItem item, Node? parent, int depth, string path, string label)
        {
            Item = item;
            Parent = parent;
            Depth = depth;
            Path = path;
            Label = label;
            Key = item;
        }

        #endregion

        #region Properties, Indexers

        public List<Node> Children { get; } = new();

        public MindMapColor Color { get; set; }

        public int Depth { get; }

        public bool HasChildren => Children.Count > 0;

        public bool IsMatch { get; set; }

        public TItem Item { get; }

        public object Key { get; set; }

        public string Label { get; }

        public Node? Parent { get; }

        public string Path { get; }

        public int RightCount { get; set; }

        public int Size => HasChildren ? Children.Sum(child => child.Size) : 1;

        #endregion
    }

    private sealed class NodeHandlers
    {
        #region Constructors and Finalizers

        public NodeHandlers(Func<Task> click, Func<KeyboardEventArgs, Task> keyDown, Func<Task> toggle)
        {
            Click = click;
            KeyDown = keyDown;
            Toggle = toggle;
        }

        #endregion

        #region Properties, Indexers

        public Func<Task> Click { get; }

        public Func<KeyboardEventArgs, Task> KeyDown { get; }

        public Func<Task> Toggle { get; }

        #endregion
    }

    #endregion
}
