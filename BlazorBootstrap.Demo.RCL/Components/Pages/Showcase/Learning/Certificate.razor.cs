using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Certificate of completion for a finished course, with a print button. An unknown course renders the not-found
/// screen; a course not yet finished explains how to earn the certificate.
/// </summary>
public partial class Certificate : ComponentBase
{
    #region Methods

    #region Private Methods

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

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "My learning", Href = $"{LearningLayout.Base}/progress" },
        new() { Text = Course?.Title, Href = Course is null ? null : LearningDisplay.CourseUrl(Course.Id) },
        new() { Text = "Certificate", IsCurrentPage = true },
    };

    private LearningCourse? Course => State.FindCourse(Id);

    /// <summary>Gets or sets the course ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
