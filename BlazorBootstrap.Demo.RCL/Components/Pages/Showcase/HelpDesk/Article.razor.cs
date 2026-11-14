using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Knowledge base article: the seeded Markdown body, a "was this helpful?" vote, and related articles. An unknown ID
/// renders the site's not-found screen.
/// </summary>
public partial class Article : ComponentBase, IDisposable
{
    #region Fields and Constants

    private int? loadedId;

    private bool voted;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // A different article starts without the previous article's vote message.
        if (loadedId == Id)
            return;

        loadedId = Id;
        voted = false;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void Vote(bool helpful)
    {
        if (Item is null)
            return;

        State.Vote(Item.Id, helpful);
        voted = true;
        Toasts.Notify(new ToastMessage(ToastType.Info, helpful ? IconName.HandThumbsUp : IconName.HandThumbsDown, "Feedback saved", "Thanks: it helps us improve this article."));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Knowledge base", Href = $"{HelpDeskLayout.Base}/kb" },
        new() { Text = Item?.Title, IsCurrentPage = true },
    };

    private HelpDeskCategory Category => State.FindCategory(Item?.CategoryId);

    /// <summary>Gets or sets the article ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private HelpDeskArticle? Item => State.FindArticle(Id);

    private IReadOnlyList<HelpDeskArticle> Related => Item is null ? Array.Empty<HelpDeskArticle>() : State.RelatedArticles(Item.CategoryId, Item.Id);

    [Inject] private HelpDeskState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    private string? VoteText
    {
        get
        {
            if (Item is null || !voted)
                return null;

            var (helpful, notHelpful) = State.Votes(Item.Id);

            return $"Thanks for the feedback. This session: {helpful} found it helpful, {notHelpful} didn't.";
        }
    }

    #endregion
}
