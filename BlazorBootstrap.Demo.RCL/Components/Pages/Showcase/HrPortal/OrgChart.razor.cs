using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// A person on the org chart's mind map, with the people who report to them. Nodes are equal when their <see cref="Id" />s
/// are, so the map keeps what is open and selected when the tree is rebuilt after a change (or a restored session).
/// </summary>
/// <param name="Id">The employee ID.</param>
/// <param name="Name">The name (also what the map's search matches).</param>
/// <param name="Title">The job title.</param>
/// <param name="Initials">The initials for the avatar.</param>
/// <param name="DepartmentId">The department.</param>
/// <param name="Location">Where they work.</param>
/// <param name="Children">Their direct reports, by name.</param>
public sealed record OrgChartNode(int Id, string Name, string Title, string Initials, string DepartmentId, string Location, IReadOnlyList<OrgChartNode> Children)
{
    /// <summary>Gets how many people are in their branch, below them.</summary>
    public int TeamSize { get; } = Children.Sum(child => 1 + child.TeamSize);

    /// <summary>Returns whether another node is the same person.</summary>
    public bool Equals(OrgChartNode? other) => other is not null && Id == other.Id;

    /// <summary>Returns the hash of the ID.</summary>
    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>
/// Org chart (everyone signed in): a Blazor.Bootstrap <see cref="MindMap{TItem}" /> rooted at the chief executive (the person
/// in the directory with no manager and the largest team), each node a person (initials, name, title) whose children are
/// their direct reports. It starts two levels open, its find box finds people by name, and <c>?focus={id}</c> opens the
/// branches down to that person and selects them. Selecting a person shows a details panel (title, department, location,
/// manager, reports, and a link to the profile). Because the map alone is not enough for every reader, the same tree is
/// listed below it as nested lists with a collapsible "Direct reports" list per manager. A department filter marks that
/// department's people in the map and the list, and says how many there are ("12 people in Engineering").
/// </summary>
public partial class OrgChart : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool announceFocus;

    private string? departmentId;

    private MindMapDirection direction = MindMapDirection.Right;

    /// <summary>The open nodes once the page or the person changed them; <see langword="null" /> keeps the map's two open levels.</summary>
    private IReadOnlyCollection<OrgChartNode>? expanded;

    private bool focusMissing;

    private Dictionary<int, OrgChartNode> nodesById = new();

    private Dictionary<int, OrgChartNode> parentsById = new();

    private int? requestedFocus;

    private OrgChartNode? selected;

    private ElementReference? selectedHeading;

    private string? statusText;

    private OrgChartNode? tree;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>After the render that shows a ?focus= person, announces them and moves focus to their details.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!announceFocus)
            return;

        announceFocus = false;

        if (selected is not null)
        {
            statusText = $"{selected.Name} is selected in the org chart. Their details are shown in the Details section.";
            StateHasChanged();
            await HrDisplay.FocusAsync(selectedHeading);
        }
        else if (focusMissing)
        {
            statusText = "That person isn't in the org chart.";
            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        BuildTree();
        State.Changed += OnStateChanged;
    }

    /// <summary>Applies a new <c>?focus=</c>: opens the branches down to that person and selects them.</summary>
    protected override void OnParametersSet()
    {
        if (FocusQuery == requestedFocus)
            return;

        requestedFocus = FocusQuery;

        if (FocusQuery is not { } id)
        {
            focusMissing = false;

            return;
        }

        // Only someone on the map itself (people not placed under the chief executive are listed below it).
        if (nodesById.TryGetValue(id, out var node) && tree is not null && (node == tree || AncestorsOf(node).Contains(tree)))
        {
            focusMissing = false;
            selected = node;
            Expand(AncestorsOf(node));
        }
        else
        {
            focusMissing = true;
        }

        announceFocus = true;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Each node's line takes its department's color (the department is also in the details and the list).</summary>
    private static MindMapColor? BranchColor(OrgChartNode node) => node.DepartmentId switch
    {
        "leadership" => MindMapColor.Indigo,
        "engineering" => MindMapColor.Blue,
        "product" => MindMapColor.Teal,
        "design" => MindMapColor.Pink,
        "sales" => MindMapColor.Orange,
        "people" => MindMapColor.Purple,
        "finance" => MindMapColor.Green,
        _ => null,
    };

    /// <summary>Returns a person's managers, from their own up to the top.</summary>
    private IEnumerable<OrgChartNode> AncestorsOf(OrgChartNode node)
    {
        var seen = new HashSet<int>();

        for (var current = node; parentsById.TryGetValue(current.Id, out var parent) && seen.Add(parent.Id); current = parent)
            yield return parent;
    }

    /// <summary>
    /// Builds the tree from the directory: the root is the person with no manager and the largest team (the chief executive);
    /// people whose reporting line doesn't reach them are listed as not placed.
    /// </summary>
    private void BuildTree()
    {
        var people = State.Directory;
        var byManager = people.Where(person => person.ManagerId is not null).ToLookup(person => person.ManagerId!.Value);
        var built = new Dictionary<int, OrgChartNode>();
        var parents = new Dictionary<int, OrgChartNode>();

        OrgChartNode Build(HrEmployee person, HashSet<int> path)
        {
            path.Add(person.Id);
            var children = byManager[person.Id].Where(child => !path.Contains(child.Id)).OrderBy(child => child.Name, StringComparer.OrdinalIgnoreCase).Select(child => Build(child, path)).ToList();
            path.Remove(person.Id);

            var node = new OrgChartNode(person.Id, person.Name, person.Title, person.Initials, person.DepartmentId, person.Location, children);
            built[person.Id] = node;

            foreach (var child in children)
                parents[child.Id] = node;

            return node;
        }

        var tops = people.Where(person => person.ManagerId is null || people.All(other => other.Id != person.ManagerId)).ToList();
        var roots = tops.Select(person => Build(person, new HashSet<int>())).ToList();

        tree = roots.Where(node => people.First(person => person.Id == node.Id).ManagerId is null)
            .OrderByDescending(node => node.TeamSize)
            .ThenBy(node => node.Id)
            .FirstOrDefault();
        Unplaced = roots.Where(node => node != tree).ToList();
        nodesById = built;
        parentsById = parents;
    }

    private string DepartmentCountText(string departmentName) =>
        $"{HrDisplay.Plural(State.Directory.Count(person => person.DepartmentId == departmentId), "person", "people")} in {departmentName}";

    private string DepartmentNameOf(string id) => State.FindDepartment(id)?.Name ?? id;

    /// <summary>Opens these nodes as well as the ones already open.</summary>
    private void Expand(IEnumerable<OrgChartNode> nodes)
    {
        var set = new HashSet<OrgChartNode>(expanded ?? InitiallyOpen());
        set.UnionWith(nodes);
        expanded = set.ToList();
    }

    /// <summary>The nodes the map opens by itself (InitialExpandDepth 2): the root and the people one level below it, with reports.</summary>
    private IEnumerable<OrgChartNode> InitiallyOpen() =>
        tree is null ? Array.Empty<OrgChartNode>() : new[] { tree }.Concat(tree.Children).Where(node => node.Children.Count > 0);

    /// <summary>Highlights a department; opens the branches down to its people so they show in the map.</summary>
    private void OnDepartmentChanged(ChangeEventArgs e)
    {
        departmentId = State.FindDepartment(e.Value?.ToString())?.Id;

        if (departmentId is not null)
            Expand(nodesById.Values.Where(node => node.DepartmentId == departmentId).SelectMany(AncestorsOf).ToList());
    }

    private void OnDirectionChanged(ChangeEventArgs e) =>
        direction = Enum.TryParse<MindMapDirection>(e.Value?.ToString(), out var value) && value is MindMapDirection.Right or MindMapDirection.Down ? value : MindMapDirection.Right;

    private void OnExpandedChanged(IReadOnlyCollection<OrgChartNode> items) => expanded = items;

    private void OnSelectedChanged(OrgChartNode? node)
    {
        selected = node;
        statusText = node is null ? null : $"Details of {node.Name} are shown in the Details section.";
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        BuildTree();
        StateHasChanged();
    });

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets <c>?focus={id}</c>: the person to open the chart at and select.</summary>
    [SupplyParameterFromQuery(Name = "focus")] public int? FocusQuery { get; set; }

    /// <summary>Gets the selected person's current record, if they are still in the directory.</summary>
    private HrEmployee? SelectedPerson => selected is not null && nodesById.ContainsKey(selected.Id) ? State.FindEmployee(selected.Id) : null;

    [Inject] private HrState State { get; set; } = default!;

    /// <summary>Gets the people whose reporting line doesn't reach the top of the chart (no manager set), with their teams.</summary>
    private IReadOnlyList<OrgChartNode> Unplaced { get; set; } = Array.Empty<OrgChartNode>();

    #endregion
}
