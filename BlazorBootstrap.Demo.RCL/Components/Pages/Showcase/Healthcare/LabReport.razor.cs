using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// One of the signed-in patient's released lab reports (<see cref="HcState.FindLabOrder" /> returns only released ones to a
/// patient): a results table (test, result, unit, reference range, and the flag in words, with High and Low also bold and
/// with an icon), the doctor's comment, and the report as a PDF from <see cref="HcLabPdf" /> in a <see cref="PdfViewer" />
/// with a "Download PDF" link. The PDF is built once per version of the report, not on every render. A report that is
/// unknown or not released shows the not-found screen.
/// </summary>
public partial class LabReport : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? pdfError;

    /// <summary>The report version the PDF was built for (the viewer is keyed by it).</summary>
    private string? pdfKey;

    /// <summary>The PDF as a <c>data:</c> URL, or <see langword="null" /> when there is no report to show.</summary>
    private string? pdfUrl;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => EnsurePdf();

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns a reference range like "4–11", or "Up to 200" when the range starts at zero.</summary>
    private static string RangeLabel(HcAnalyte analyte) =>
        analyte.Low <= 0 ? $"Up to {HcDisplay.Number(analyte.High)}" : $"{HcDisplay.Number(analyte.Low)}–{HcDisplay.Number(analyte.High)}";

    /// <summary>Builds the PDF when the report (or anything printed on it) changed since the last build; otherwise keeps it.</summary>
    private void EnsurePdf()
    {
        if (Order is not { } order || State.LabPanel(order.PanelId) is not { } panel || State.CurrentPatient is not { } patient)
        {
            pdfKey = null;
            pdfUrl = null;

            return;
        }

        var doctor = State.FindDoctor(order.DoctorId);
        var clinic = State.FindClinic(doctor?.ClinicId);
        var version = HashCode.Combine(order.ReleasedAt, string.Join(",", order.Values.Select(value => value.ToString(CultureInfo.InvariantCulture))), order.Comment, patient.Name, patient.DateOfBirth, doctor?.Name, clinic?.Name);
        var key = string.Create(CultureInfo.InvariantCulture, $"{order.Id}-{version}");

        if (key == pdfKey)
            return;

        pdfKey = key;
        pdfError = null;
        pdfUrl = $"data:application/pdf;base64,{HcLabPdf.ToBase64(order, panel, patient, doctor, clinic)}";
    }

    private void OnPdfError(string message) => pdfError = "The PDF couldn't be shown here. Use Download PDF to open it.";

    private void OnStateChanged() => InvokeAsync(() =>
    {
        EnsurePdf();
        StateHasChanged();
    });

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Lab reports", Href = $"{HcLayout.Base}/lab-reports" },
        new() { Text = Order is { } order ? State.LabPanel(order.PanelId)?.Name ?? "Report" : "Report", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the lab order ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    /// <summary>The report, when it is the signed-in patient's and released.</summary>
    private HcLabOrder? Order => State.CurrentPatient is not null && State.FindLabOrder(Id) is { Status: HcLabStatus.Released } order ? order : null;

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
