using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// One trace (<see cref="MonitoringState.FindTrace" />): a summary (root service, time, total duration, span count, and
/// whether a span failed, in words), a waterfall with each span's service, operation, start offset, and duration in text,
/// indented by its depth, with a decorative bar placed by its start and length within the trace, a table of the same spans,
/// and links to each service and to the trace's log lines. An unknown or expired trace shows a not-found panel. Once found,
/// a trace stays on the page even if its log line later leaves the window; a trace not found yet is looked up again when the
/// state changes (for example after the tab's snapshot is restored).
/// </summary>
public partial class TraceView : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? loadedId;

    private MonitoringTrace? trace;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == TraceId)
            return;

        loadedId = TraceId;
        trace = State.FindTrace(TraceId);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Places a span's bar by its start and length as shares of the whole trace (invariant numbers in the style).</summary>
    private static string BarStyle(MonitoringTrace current, MonitoringSpan span)
    {
        var total = Math.Max(current.DurationMs, 0.001);
        var left = Math.Clamp(span.StartMs / total * 100, 0, 100);
        var width = Math.Clamp(span.DurationMs / total * 100, 0, 100 - left);

        return FormattableString.Invariant($"left: {left:0.##}%; width: max({width:0.##}%, 2px);");
    }

    private static string IndentStyle(int depth) => FormattableString.Invariant($"--tv-depth: {Math.Min(depth, 6)};");

    /// <summary>Returns the spans depth-first from the root (children in start order), each with its depth from <see cref="MonitoringSpan.ParentId" />.</summary>
    private static List<(MonitoringSpan Span, int Depth)> Ordered(MonitoringTrace current)
    {
        var ids = current.Spans.Select(span => span.Id).ToHashSet();
        var result = new List<(MonitoringSpan, int)>();
        var visited = new HashSet<string>();

        void Visit(MonitoringSpan span, int depth)
        {
            if (!visited.Add(span.Id))
                return;

            result.Add((span, depth));

            foreach (var child in current.Spans.Where(item => item.ParentId == span.Id).OrderBy(item => item.StartMs))
                Visit(child, depth + 1);
        }

        // Roots: no parent, or a parent that isn't in the trace.
        foreach (var root in current.Spans.Where(span => span.ParentId is null || !ids.Contains(span.ParentId)).OrderBy(span => span.StartMs))
            Visit(root, 0);

        return result;
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        trace ??= State.FindTrace(TraceId);
        StateHasChanged();
    });

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    /// <summary>Returns the services a trace passes through, in the order they first appear.</summary>
    private List<MonitoringService> Services(MonitoringTrace current) =>
        current.Spans.Select(span => span.ServiceId).Distinct().Select(State.FindService).OfType<MonitoringService>().ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    private static string LogsUrl => $"{ConsoleLayout.Base}/logs";

    [Inject] private MonitoringState State { get; set; } = default!;

    /// <summary>Gets or sets the trace ID from the route.</summary>
    [Parameter] public string TraceId { get; set; } = string.Empty;

    private string TraceLogsUrl => $"{LogsUrl}?search={Uri.EscapeDataString(TraceId)}";

    #endregion
}
