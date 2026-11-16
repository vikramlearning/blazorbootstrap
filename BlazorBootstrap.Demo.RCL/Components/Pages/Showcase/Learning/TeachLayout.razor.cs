using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Layout of the Quillpath Academy instructor back office (D-031): a top bar with "View school" and the account menu (its
/// notice count is the questions waiting for an answer), a sidebar with the instructor's modules, and the page behind an
/// instructor-only guard. The sidebar shows only to a signed-in instructor.
/// </summary>
public partial class TeachLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the instructor back office.</summary>
    public const string Base = LearningLayout.Base + "/teach";

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The instructor's modules.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "dashboard", Href = Base, IconName = IconName.Speedometer2, Text = "Dashboard", Match = NavLinkMatch.All },
            new() { Id = "courses", Href = $"{Base}/courses", IconName = IconName.JournalBookmark, Text = "Courses" },
            new() { Id = "questions", Href = $"{Base}/questions", IconName = IconName.ChatLeftText, Text = "Q&A inbox" },
            new() { Id = "reviews", Href = $"{Base}/reviews", IconName = IconName.Star, Text = "Reviews" },
            new() { Id = "learners", Href = $"{Base}/learners", IconName = IconName.People, Text = "Learners" },
            new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonGear, Text = "Account" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
