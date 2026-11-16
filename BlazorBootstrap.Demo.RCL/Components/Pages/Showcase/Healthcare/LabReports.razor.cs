using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's lab tests: released reports (newest first, with the release date, the test, the ordering doctor,
/// and how many results are out of range, in words) linking to the report, and tests still waiting for results (ordered on
/// a date; no values are ever shown until the doctor releases them).
/// </summary>
public partial class LabReports : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns how many of a released order's results are below or above their reference range.</summary>
    internal static int OutOfRange(HcLabOrder order, HcLabPanel? panel) =>
        panel is null || order.Status != HcLabStatus.Released ? 0 : panel.Analytes.Zip(order.Values).Count(pair => HcState.Flag(pair.First, pair.Second) != "Normal");

    /// <summary>Returns the out-of-range count in words, like "All results in range" or "2 results out of range".</summary>
    internal static string RangeText(int outOfRange) => outOfRange switch
    {
        0 => "All results in range",
        1 => "1 result out of range",
        _ => string.Create(CultureInfo.InvariantCulture, $"{outOfRange} results out of range"),
    };

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns the patient's lab orders, newest first.</summary>
    private IReadOnlyList<HcLabOrder> Orders() => State.CurrentPatient is { } patient ? State.LabOrdersOf(patient.Id) : Array.Empty<HcLabOrder>();

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
