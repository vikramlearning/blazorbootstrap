using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Catalog: a "continue learning" strip for the signed-in learner, an invitation to sign in or register for a visitor,
/// search, category and level filters, and course cards (with "Enrolled" or "Completed" on the learner's courses). The
/// filters live in the site state, so they are still set after opening a course and coming back.
/// </summary>
public partial class Catalog : ComponentBase, IDisposable
{
    #region Fields and Constants

    private ElementReference resultsCount;

    private string searchBoxValue = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        searchBoxValue = State.Filter.Search;
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The button disables itself, so focus moves to the result count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        State.Filter.Clear();
        searchBoxValue = string.Empty;

        await LearningDisplay.FocusAsync(resultsCount);
    }

    private void OnCategoryChanged(string categoryId) => State.Filter.CategoryId = categoryId;

    private void OnLevelChanged(string level) => State.Filter.Level = level;

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;
        State.Filter.Search = value;
    }

    private void OnSearchInput(ChangeEventArgs args) => State.Filter.Search = args.Value?.ToString() ?? string.Empty;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns the sign-in (or register) route that comes back to the catalog.</summary>
    private static string SignInUrl(bool register) =>
        $"{LearningLayout.SignInPath}?{(register ? "mode=register&" : string.Empty)}returnUrl={Uri.EscapeDataString(LearningLayout.Base)}";

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Greets the signed-in account by first name; a visitor gets the school's promise.</summary>
    private string Greeting => State.Identity.Current is { } account
        ? $"Welcome back, {account.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? account.Name}"
        : "Free courses, at your own pace";

    private IReadOnlyList<LearningCourse> InProgress => State.EnrolledCourses.Where(course => !State.IsComplete(course.Id)).ToList();

    private IReadOnlyList<LearningCourse> Results
    {
        get
        {
            var filter = State.Filter;
            var search = filter.Search.Trim();

            return State.Courses
                .Where(course => filter.CategoryId.Length == 0 || course.CategoryId == filter.CategoryId)
                .Where(course => filter.Level.Length == 0 || course.Level.ToString() == filter.Level)
                .Where(course => search.Length == 0
                    || course.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || course.Subtitle.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || course.Instructor.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private string ResultsText => Results.Count switch
    {
        0 => "No courses found",
        1 => "1 course",
        var count => $"{count} courses",
    };

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
