using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// The printable certificate of completion found by its code: the school, the learner's name as issued, the course with
/// its lessons and length, the instructor, the issue date, and the ID with its verify link. Public, so a learner can share
/// it; an unknown code shows a not-found screen that points to the verify page.
/// </summary>
public partial class Certificate : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the route of the certificate with a code.</summary>
    public static string PathFor(string code) => $"{LearningLayout.Base}/certificates/{Uri.EscapeDataString(code)}";

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task PrintAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("print");
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            // Printing is a convenience; the certificate stays on screen.
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The owner sees their certificates in the trail; anyone else came from a shared link or the verify page.</summary>
    private List<BreadcrumbItem> BreadcrumbItems => IsOwn
        ? new()
        {
            new() { Text = "Certificates", Href = $"{LearningLayout.Base}/certificates" },
            new() { Text = Course?.Title, IsCurrentPage = true },
        }
        : new()
        {
            new() { Text = "Verify a certificate", Href = Verify.PathFor(Found?.Code) },
            new() { Text = "Certificate", IsCurrentPage = true },
        };

    /// <summary>Gets or sets the certificate code from the route (case and spaces are ignored).</summary>
    [Parameter] public string? Code { get; set; }

    private LearningCourse? Course => Found is null ? null : State.FindCourse(Found.CourseId);

    private LearningCertificate? Found => State.VerifyCertificate(Code);

    private bool IsOwn => State.IsLearner && Found?.AccountId is { } owner && owner == State.Identity.Current?.Id;

    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    [Inject] private LearningState State { get; set; } = default!;

    private string VerifyUrl => Navigation.ToAbsoluteUri(Verify.PathFor(Found?.Code)).ToString();

    #endregion
}
