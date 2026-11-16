using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Submit a request, as the signed-in customer: category, how urgent it is (customers choose Low, Normal, or High; only
/// agents set Urgent), subject, description, and an optional company. The name and email come from the account. As the
/// subject is typed, matching articles from the chosen category are suggested. A submitted request opens on its own page
/// with a "received" status (<see cref="HelpDeskState.SubmitRequest" />). A visitor is asked to sign in or register first,
/// with this page as the return address; an agent is pointed to the agent desk's New ticket.
/// </summary>
public partial class RequestNew : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest company name <see cref="HelpDeskState.SubmitRequest" /> accepts.</summary>
    private const int MaxCompanyLength = 80;

    private const int MaxSuggestions = 3;

    /// <summary>Words too common to say anything about an article.</summary>
    private static readonly HashSet<string> stopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "are", "but", "can", "cant", "can't", "doesn't", "does", "for", "from", "get", "has", "have", "how", "into", "isn't",
        "not", "our", "the", "this", "that", "was", "what", "when", "why", "with", "won't", "you", "your",
    };

    /// <summary>The choices a customer has; Urgent is set by agents.</summary>
    private static readonly Urgency[] urgencies =
    {
        new(HelpDeskPriority.Low, "A question, or something small that can wait."),
        new(HelpDeskPriority.Normal, "Something isn't working as it should."),
        new(HelpDeskPriority.High, "You can't get your work done."),
    };

    private readonly List<BreadcrumbItem> breadcrumbItems = new()
    {
        new() { Text = "Help center", Href = HelpDeskLayout.Base },
        new() { Text = "Submit a request", IsCurrentPage = true },
    };

    private EditContext editContext = default!;

    private RequestForm form = new();

    private bool prefilled;

    private string? submitError;

    /// <summary>The subject as it is typed, for the suggestions; the form's value follows on change.</summary>
    private string? subjectDraft;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        editContext = new EditContext(form);
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet() => Prefill();

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static IEnumerable<string> Words(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim('.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '“', '”'))
            .Where(word => word.Length >= 3 && !stopWords.Contains(word))
            .Select(Stem)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    /// <summary>Drops a common English ending, so "charged" finds "charge" and "invoices" finds "invoice".</summary>
    private static string Stem(string word)
    {
        foreach (var ending in new[] { "ing", "ed", "es", "s" })
        {
            if (word.Length - ending.Length >= 4 && word.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
                return word[..^ending.Length];
        }

        return word;
    }

    private static string SignInUrl(bool register) =>
        $"{HelpDeskLayout.SignInPath}?{(register ? "mode=register&" : string.Empty)}returnUrl={Uri.EscapeDataString(HelpDeskLayout.Base + "/requests/new")}";

    private string TargetText(HelpDeskPriority priority)
    {
        var hours = State.TargetHours(priority);

        return hours % 24 == 0 && hours >= 48
            ? $"We aim to solve it within {hours / 24} days."
            : $"We aim to solve it within {hours} hours.";
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        Prefill();
        StateHasChanged();
    });

    private void OnSubjectInput(ChangeEventArgs args) => subjectDraft = args.Value?.ToString() ?? string.Empty;

    /// <summary>Fills the company from the customer's latest request once, after the session is restored.</summary>
    private void Prefill()
    {
        if (prefilled || !State.IsCustomer)
            return;

        prefilled = true;

        if (string.IsNullOrEmpty(form.Company))
            form.Company = State.MyRequests().Select(ticket => ticket.Company).FirstOrDefault(company => !string.IsNullOrWhiteSpace(company));
    }

    private void Submit()
    {
        submitError = null;

        // The radios only offer these, but the value is checked again: customers don't set Urgent.
        if (urgencies.All(option => option.Priority != form.Priority))
        {
            submitError = "Choose how urgent it is: Low, Normal, or High.";
            return;
        }

        var ticket = State.SubmitRequest(form.Company, form.CategoryId, form.Priority, form.Subject, form.Description);

        if (ticket is null)
        {
            submitError = State.IsCustomer
                ? "The request couldn't be submitted. Check each field and try again."
                : "You're no longer signed in as a customer. Sign in again to submit the request.";
            return;
        }

        Navigation.NavigateTo($"{HelpDeskDisplay.RequestUrl(ticket.Id)}?received=1");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    [Inject] private HelpDeskState State { get; set; } = default!;

    /// <summary>Gets up to three articles whose title or summary shares words with the subject, from the chosen category (or any, before one is chosen).</summary>
    private IReadOnlyList<HelpDeskArticle> Suggestions
    {
        get
        {
            var words = Words(subjectDraft ?? form.Subject ?? string.Empty).ToList();

            if (words.Count == 0)
                return Array.Empty<HelpDeskArticle>();

            return State.Articles
                .Where(article => string.IsNullOrEmpty(form.CategoryId) || article.CategoryId == form.CategoryId)
                .Select(article => (Article: article, Score: words.Count(word => article.Title.Contains(word, StringComparison.OrdinalIgnoreCase)
                    || article.Summary.Contains(word, StringComparison.OrdinalIgnoreCase))))
                .Where(match => match.Score > 0)
                .OrderByDescending(match => match.Score)
                .ThenByDescending(match => match.Article.Views)
                .Take(MaxSuggestions)
                .Select(match => match.Article)
                .ToList();
        }
    }

    private string SuggestionsText => Suggestions.Count switch
    {
        0 => string.Empty,
        1 => "1 article might answer this before you submit.",
        var count => $"{count} articles might answer this before you submit.",
    };

    #endregion

    #region Nested Types

    /// <summary>One "How urgent is it?" choice.</summary>
    private sealed record Urgency(HelpDeskPriority Priority, string Help);

    private sealed class RequestForm
    {
        [Required(ErrorMessage = "Choose a category.")]
        public string CategoryId { get; set; } = string.Empty;

        [StringLength(MaxCompanyLength, ErrorMessage = "Use 80 characters or fewer.")]
        public string? Company { get; set; }

        [Required(ErrorMessage = "Describe what's happening.")]
        [StringLength(HelpDeskState.MaxCustomerMessageLength, MinimumLength = 10, ErrorMessage = "Use 10 to 4,000 characters.")]
        public string Description { get; set; } = string.Empty;

        public HelpDeskPriority Priority { get; set; } = HelpDeskPriority.Normal;

        [Required(ErrorMessage = "Add a short subject.")]
        [StringLength(HelpDeskState.MaxSubjectLength, MinimumLength = 4, ErrorMessage = "Use 4 to 120 characters.")]
        public string Subject { get; set; } = string.Empty;
    }

    #endregion
}
